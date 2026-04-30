using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Helpers;
using Suity.NodeQuery;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Resolves document formats for Suity asset files (.sasset) by scanning headers and matching format names.
/// </summary>
public class SAssetDocumentFormatResolver : IDocumentFormatResolver
{
    const string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
    const string SuityHeader = "<SuityAsset version=\"1.0\" format=\"";

    private readonly Dictionary<string, DocumentFormat> _factories = [];
    private readonly UniqueMultiDictionary<string, DocumentFormat> _documentFormatByExt = new(IgnoreCaseStringComparer.Instance);

    /// <inheritdoc/>
    public string Extension => "sasset";

    /// <inheritdoc/>
    public IEnumerable<DocumentFormat> Formats => _factories.Values;

    /// <summary>
    /// Creates a new resolver and scans for available document formats.
    /// </summary>
    public SAssetDocumentFormatResolver()
    {
        Scan();
    }

    /// <inheritdoc/>
    public IDocumentResolveResult ResolveDocumentFormat(Stream stream)
    {
        long? pos = stream.CanSeek ? stream.Position : null;

        string headerStr = null;

        using (var reader = new StreamReader(stream))
        {
            // Read first 120 characters
            char[] buffer = new char[120];
            int bytesRead = reader.Read(buffer, 0, buffer.Length);

            // Convert char array to string
            headerStr = new string(buffer, 0, bytesRead);
        }

        if (string.IsNullOrWhiteSpace(headerStr))
        {
            return null;
        }

        if (!headerStr.StartsWith(XmlHeader))
        {
            return null;
        }

        headerStr = headerStr.RemoveFromFirst(XmlHeader.Length);
        headerStr = headerStr.TrimStart();

        if (!headerStr.StartsWith(SuityHeader))
        {
            return null;
        }

        headerStr = headerStr.RemoveFromFirst(SuityHeader.Length);
        headerStr = headerStr.TrimStart();

        int quoteIndex = headerStr.IndexOf("\""); // Get the index position of double quotes
        if (quoteIndex < 0)
        {
            return null;
        }

        string formatName = headerStr[..quoteIndex]; // Use index position to extract string

        var factory = _factories.GetValueSafe(formatName);
        if (factory != null)
        {
            // stream is actually automatically closed after reader is closed,
            if (stream.CanSeek && pos.Value is { } p)
            {
                stream.Seek(p, SeekOrigin.Begin);

                return new SAssetResolveResult
                {
                    Format = factory,
                    LoaderObject = null,
                    Stream = stream
                };
            }
            else
            {
                return new SAssetResolveResult
                {
                    Format = factory,
                    LoaderObject = null,
                    Stream = null
                };
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public IDocumentResolveResult ResolveDocumentFormat2(Stream stream)
    {
        var reader = XmlNodeReader.FromStream(stream, false);
        if (reader is null || !reader.Exist)
        {
            return null;
        }

        if (reader.NodeName != "SuityAsset")
        {
            return null;
        }

        string version = reader.GetAttribute("version");
        if (version != "1.0")
        {
            return null;
        }

        var formatName = reader.GetAttribute("format");
        if (string.IsNullOrEmpty(formatName))
        {
            return null;
        }

        var factory = _factories.GetValueSafe(formatName);
        if (factory != null)
        {
            return new SAssetResolveResult
            {
                Format = factory,
                LoaderObject = reader,
                Stream = stream
            };
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Scans for all derived SAssetDocument types and registers their formats.
    /// </summary>
    private void Scan()
    {
        foreach (Type docType in typeof(SAssetDocument).GetDerivedTypes())
        {
            DocumentFormatAttribute docTypeAttr = docType.GetAttributeCached<DocumentFormatAttribute>();
            if (docTypeAttr is null)
            {
                continue;
            }
            if (string.IsNullOrEmpty(docTypeAttr.FormatName))
            {
                continue;
            }

            var format = new SAssetDocumentFormat(docType, docTypeAttr);

            RegisterDocumentFormat(format);
        }
    }

    /// <summary>
    /// Registers a document format with its name and associated extensions.
    /// </summary>
    /// <param name="format">The document format to register.</param>
    private void RegisterDocumentFormat(DocumentFormat format)
    {
        if (format is null)
        {
            throw new ArgumentNullException();
        }
        string name = format.FormatName;
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException();
        }
        if (_factories.ContainsKey(name))
        {
            throw new InvalidOperationException("Document factory exist : " + name);
        }

        _factories[name] = format;

        if (format.FormatNames?.Length > 0)
        {
            foreach (var alias in format.FormatNames)
            {
                if (string.IsNullOrEmpty(alias))
                {
                    continue;
                }

                if (_factories.ContainsKey(alias))
                {
                    Logs.LogError($"Register sasset document factory : {alias} already exist");

                    continue;
                }
                _factories[alias] = format;
            }
        }

        string[] exts = format.GetAdditionalExtensions();
        if (exts != null)
        {
            EditorServices.SystemLog.AddLog($"Register sasset document factory : {format.FormatName} to {format.GetType().Name} ext : {string.Join(",", exts)}");

            foreach (string ext in exts)
            {
                if (string.IsNullOrEmpty(ext))
                {
                    throw new ArgumentException();
                }
                _documentFormatByExt.Add(ext, format);
            }
        }
    }

    /// <summary>
    /// Resolution result for SAsset document format detection.
    /// </summary>
    public class SAssetResolveResult : IDocumentResolveResult
    {
        /// <inheritdoc/>
        public DocumentFormat Format { get; set; }

        /// <inheritdoc/>
        public object LoaderObject { get; set; }

        /// <inheritdoc/>
        public Stream Stream { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                Stream?.Close();
                Stream?.Dispose();
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }

            Stream = null;
        }
    }
}

/// <summary>
/// Document format implementation for SAsset document types.
/// </summary>
internal class SAssetDocumentFormat : DocumentFormat
{
    private readonly Type _documentType;
    private readonly DocumentFormatAttribute _attribute;

    /// <summary>
    /// Creates a new SAsset document format.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="attribute">The format attribute.</param>
    public SAssetDocumentFormat(Type documentType, DocumentFormatAttribute attribute)
    {
        _documentType = documentType;
        _attribute = attribute;
    }

    /// <inheritdoc/>
    public override string FormatName => _attribute.FormatName;

    /// <inheritdoc/>
    public override string[] FormatNames => _attribute.FormatNames ?? [];

    /// <inheritdoc/>
    public override string Extension => "sasset";

    /// <inheritdoc/>
    public override string[] GetAdditionalExtensions() => _attribute.Extensions;

    /// <inheritdoc/>
    public override string DisplayText => _attribute.DisplayText;

    /// <inheritdoc/>
    public override Image Icon => EditorUtility.GetIconByAssetKey(_attribute.Icon);

    /// <inheritdoc/>
    public override bool CanCreate => _attribute.CanCreate;

    /// <inheritdoc/>
    public override bool CanShowView => _attribute.CanShowView;

    /// <inheritdoc/>
    public override bool CanShowAsProperty => _attribute.CanShowAsProperty;

    /// <inheritdoc/>
    public override Task<string> OpenCreationUI(string basePath)
    {
        return EditorServices.FileNameService.ShowCreateDocumentDialogAsync(basePath, _attribute.FormatName, Extension);
    }

    /// <inheritdoc/>
    public override Type DocumentType => _documentType;

    /// <inheritdoc/>
    public override string Category => _attribute.Categoty;

    /// <inheritdoc/>
    public override int Order => _attribute.Order;

    /// <inheritdoc/>
    public override bool IsAttached => _attribute.IsAttached;

    /// <inheritdoc/>
    public override LoadingIterations Iteration => _attribute.Iteration;
}
