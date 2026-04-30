using System;

namespace Suity.Editor.Documents.External;

/// <summary>
/// Represents a Word document asset that extracts text content from .doc and .docx files.
/// </summary>
public class WordTextAsset : TextAsset
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
            return WordToMarkdownConverter.ConvertDocxToMarkdown(fileName);

            //var sb = new StringBuilder();

            //using (var stroage = fileName.GetStorageItem())
            //using (var stream = stroage.GetInputStream())
            //using (var doc = new XWPFDocument(stream))
            //{
            //    foreach (var paragraph in doc.Paragraphs)
            //    {
            //        sb.AppendLine(paragraph.Text);
            //    }
            //}

            //return sb.ToString();
        }
        catch (Exception err)
        {
            err.LogError($"Load word document failed : {fileName}");

            return string.Empty;
        }
    }
}

/// <summary>
/// Activator for creating Word document text assets.
/// </summary>
public class WordTextAssetActivator : AssetActivator
{
    private static readonly string[] _extensions = ["doc", "docx"];

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        return new WordTextAsset();
    }

    /// <inheritdoc/>
    public override string[] GetExtensions() => _extensions;
}
