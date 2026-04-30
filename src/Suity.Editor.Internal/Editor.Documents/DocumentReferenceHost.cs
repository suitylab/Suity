using System;
using System.IO;

namespace Suity.Editor.Documents;

/// <summary>
/// A reference host that resolves to a document's content, or falls back to a file asset if the document is unloaded.
/// </summary>
/// <param name="path">The document file path.</param>
public class DocumentReferenceHost(string path) : SyncReferenceHost, IDocumentHost
{
    private string _path = path ?? throw new ArgumentNullException(nameof(path));

    /// <summary>
    /// Gets or sets the document file path.
    /// </summary>
    public string DocumentPath
    {
        get => _path;
        internal set => _path = value;
    }

    /// <inheritdoc/>
    public override object Target
    {
        get
        {
            object obj;

            obj = DocumentManager.Instance.GetDocument(_path)?.Content;
            if (obj != null)
            {
                return obj;
            }

            // When document is unloaded, try to propagate reference update to asset
            obj = FileAssetManager.Current.GetAsset(_path);
            if (obj != null)
            {
                return obj;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public override object GetOrLoadTarget()
    {
        object obj;

        obj = DocumentManager.Instance.GetDocument(_path)?.Content;
        if (obj != null)
        {
            return obj;
        }

        obj = DocumentManager.Instance.OpenDocument(_path)?.Content;
        if (obj != null)
        {
            // Logs.LogDebug($"DocumentReferenceHost open document : {_path}");
            return obj;
        }

        // When document is unloaded, try to propagate reference update to asset
        obj = FileAssetManager.Current.GetAsset(_path);
        if (obj != null)
        {
            return obj;
        }

        return null;
    }

    /// <summary>
    /// Opens the document and returns its content.
    /// </summary>
    /// <returns>The document content, or null if opening failed.</returns>
    public Document OpenDocument()
    {
        return DocumentManager.Instance.OpenDocument(_path)?.Content;
    }

    /// <inheritdoc/>
    public override object GetNavigationTarget()
    {
        return StorageLocation.Create(_path);

        //return Target;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path.GetFileName(_path);
    }
}