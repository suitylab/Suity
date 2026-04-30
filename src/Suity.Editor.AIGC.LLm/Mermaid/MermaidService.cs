using Ionic.Zlib;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Mermaid;

/// <summary>
/// Provides implementation for generating, caching, and rendering Mermaid diagrams.
/// </summary>
internal class MermaidService : IMermaidService
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="MermaidService"/>.
    /// </summary>
    public static MermaidService Instance { get; } = new();
    readonly Dictionary<int, Bitmap> _cachedImages = [];

    bool _generating;
    DateTime _genTime;

    #region IMermaidService

    /// <inheritdoc/>
    public async Task<IArticle> GenerateMermaid(IArticle article, MermaidGraphType graphType, string prompt)
    {
        string text = article.GetFullText();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        string result = await GenerateMermaid(text, graphType, prompt);
        result = result?.Trim();
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        IArticle graphArticle = null;

        if (article.Parent is { } parent)
        {
            graphArticle = parent.GetOrAddArticle(article.Title + " - " + graphType.ToString());
            graphArticle.Content = result;
            article.Commit();
        }
        else
        {
            graphArticle = article.GetOrAddArticle(graphType.ToString());
            graphArticle.Content = result;
            article.Commit();
        }

        if (article.GetArticlewDocument() is Document doc && DocumentViewManager.Current.GetDocumentView(doc.Entry) is { } view)
        {
            view.RefreshView();

            QueuedAction.Do(() =>
            {
                if (view.GetService<IViewSelectable>() is { } sel && graphArticle is not null)
                {
                    sel.SetSelection(new ViewSelection(graphArticle));
                }
            });
        }

        return graphArticle;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateMermaid(string input, MermaidGraphType graphType, string prompt)
    {
        var assistant = new MermaidAssistant();

        var option = new MermaidOption
        {
            GraphType = graphType,
            Content = input,
            UserMessage = prompt,
        };

        string result = (await LLmService.Instance.InputMainChat(graphType.ToString(), assistant, option))?.ToString() ?? string.Empty;

        return result;
    }

    /// <inheritdoc/>
    public bool IsMermaidCodeBlock(string input)
    {
        return input?.StartsWith("```mermaid", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <inheritdoc/>
    public string GetLiveUrl(string input)
    {
        input = TrimMermaid(input);
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return "https://mermaid.live/edit#pako:" + EncodeMermaid(input);
    }

    /// <inheritdoc/>
    public string GetImageUrl(string input)
    {
        input = TrimMermaid(input);
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return "https://mermaid.ink/img/pako:" + EncodeMermaid(input) + "?type=png";
    }

    /// <inheritdoc/>
    public async Task<Bitmap> GenerateMermaidBitmap(string input)
    {
        if (GetCachedMermaidBitmap(input) is { } cachedImg)
        {
            return cachedImg;
        }

        if (_generating)
        {
            return null;
        }

        input = TrimMermaid(input);
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string urlInk = GetImageUrl(input);
        if (string.IsNullOrWhiteSpace(urlInk))
        {
            return null;
        }

        try
        {
            _generating = true;
            _genTime = DateTime.Now;
            int hashCode = input.GetHashCode();

            var img = await GnerateMermaidBitmapByUrl(urlInk);
            if (img != null)
            {
                lock (_cachedImages)
                {
                    _cachedImages[hashCode] = img;
                }

                try
                {
                    string cachePath = GetImageCacheFileName(input);
                    if (File.Exists(cachePath))
                    {
                        File.Delete(cachePath);
                    }

                    img.Save(cachePath);
                }
                catch (Exception)
                {
                }
            }

            return img;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _generating = false;
        }

        
    }

    /// <inheritdoc/>
    public Bitmap GetCachedMermaidBitmap(string input)
    {
        input = TrimMermaid(input);
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        Bitmap cachedBitmap = null;

        int hashCode = input.GetHashCode();
        lock (_cachedImages)
        {
            if (_cachedImages.TryGetValue(hashCode, out cachedBitmap))
            {
                return cachedBitmap;
            }
        }

        string cachePath = GetImageCacheFileName(input);

        if (File.Exists(cachePath))
        {
            try
            {
                cachedBitmap = new Bitmap(cachePath);
                lock (_cachedImages)
                {
                    _cachedImages[hashCode] = cachedBitmap;
                }

                return cachedBitmap;
            }
            catch (Exception)
            {
                // Ignore loading error
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public bool IsImageGenerating() => _generating;
    #endregion

    /// <summary>
    /// Downloads and creates a <see cref="Bitmap"/> from the specified URL.
    /// </summary>
    /// <param name="url">The URL of the image to download.</param>
    /// <returns>A <see cref="Bitmap"/> object if successful; otherwise, null.</returns>
    public static async Task<Bitmap> GnerateMermaidBitmapByUrl(string url)
    {
        try
        {
            var httpClient = new HttpClient();

            // 1. Send request to get byte array
            byte[] imageBytes = await httpClient.GetByteArrayAsync(url);

            // 2. Convert byte array to memory stream
            using var ms = new MemoryStream(imageBytes);

            // 3. Create Bitmap from stream
            // Note: In .NET Core environment, need to install System.Drawing.Common
            return new Bitmap(ms);
        }
        catch (Exception ex)
        {
            ex.LogError("Failed to get image");
            return null;
        }
    }

    /// <summary>
    /// Encodes Mermaid diagram code into a URL-safe Base64 string using pako-compatible zlib compression.
    /// </summary>
    /// <param name="mermaidCode">The raw Mermaid diagram code to encode.</param>
    /// <returns>A URL-safe Base64 encoded string suitable for use with mermaid.live and mermaid.ink URLs.</returns>
    public static string EncodeMermaid(string mermaidCode)
    {
        // 1. Define Mermaid.live state object
        var state = new
        {
            code = mermaidCode,
            mermaid = new { theme = "default" },
            updateEditor = false,
            autoSync = true,
            updateDiagram = true
        };

        // 2. Serialize to JSON
        string jsonString = JsonSerializer.Serialize(state);
        byte[] inputBytes = Encoding.UTF8.GetBytes(jsonString);

        using var outputStream = new MemoryStream();

        // 3. Compress using Ionic.Zlib
        // Ionic.Zlib.ZlibStream will automatically add the Zlib header (0x78 0x9C) required by pako
        using (var zlibStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.BestCompression))
        {
            zlibStream.Write(inputBytes, 0, inputBytes.Length);
        }

        // 4. Convert to Base64 string
        byte[] compressedBytes = outputStream.ToArray();
        string base64 = Convert.ToBase64String(compressedBytes);

        // 5. URL escape
        base64 = base64.Replace('/', '_');
        base64 = base64.Replace('+', '-');

        return base64;
    }

    /// <summary>
    /// Removes the Mermaid code block fence markers (```) from the input string.
    /// </summary>
    /// <param name="str">The input string that may contain Mermaid code block markers.</param>
    /// <returns>The cleaned Mermaid code without fence markers.</returns>
    public static string TrimMermaid(string str)
    {
        str ??= string.Empty;

        string trimed = str
        .Trim()
        .RemoveFromFirst("```mermaid", StringComparison.OrdinalIgnoreCase)
        .RemoveFromLast("```")
        .Trim();

        return trimed;
    }

    /// <summary>
    /// Gets the file path for caching a Mermaid diagram image based on the input content.
    /// </summary>
    /// <param name="inputTrimmed">The trimmed Mermaid code used to compute the cache file name.</param>
    /// <returns>The full file path for the cached image file.</returns>
    public static string GetImageCacheFileName(string inputTrimmed)
    {
        string cacheDir = Project.Current.UserDirectory.PathAppend("MermaidCache");

        try
        {
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
        }
        catch (Exception)
        {
        }

        string md5 = CheckSumHelper.GetMD5Hash(inputTrimmed);
        string cachePath = Path.Combine(cacheDir, md5 + ".png");

        return cachePath;
    }
}
