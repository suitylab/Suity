using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Text;
using Suity.Helpers;

namespace Suity.Editor.Documents.External;

/// <summary>
/// Represents a PDF document asset that extracts text content from .pdf files.
/// </summary>
public class PdfTextAsset : TextAsset
{
    string _cachedString;

    /// <inheritdoc/>
    public override string GetText()
    {
        if (_cachedString != null)
        {
            return _cachedString;
        }

        var path = FileName;

        if (path != null)
        {
            _cachedString = LoadText(path);
        }

        return _cachedString ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override void OnUpdated(EntryEventArgs args)
    {
        base.OnUpdated(args);

        _cachedString = null;
    }

    /// <inheritdoc/>
    protected override void OnAssetActivate(string assetKey)
    {
        base.OnAssetActivate(assetKey);

        _cachedString = null;
    }

    /// <inheritdoc/>
    protected override void OnAssetDeactivate(string assetKey)
    {
        base.OnAssetDeactivate(assetKey);

        _cachedString = null;
    }

    private static string LoadText(StorageLocation fileName)
    {
        try
        {
            var builder = new StringBuilder();

            using (var stroage = fileName.GetStorageItem())
            using (var document = PdfReader.Open(stroage.GetInputStream()))
            using (var extractor = new PdfSharpExtractor(document))
            {
                foreach (PdfPage page in document.Pages)
                {
                    extractor.ExtractText(page, builder);
                }
            }

            return builder.ToString();
        }
        catch (Exception err)
        {
            err.LogError($"Load pdf failed : {fileName}");

            return string.Empty;
        }
    }
}

/// <summary>
/// Activator for creating PDF document text assets.
/// </summary>
public class PdfTextAssetActivator : AssetActivator
{
    private static readonly string[] _extensions = ["pdf"];

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new PdfTextAsset();
    }

    /// <inheritdoc/>
    public override string[] GetExtensions() => _extensions;
}
