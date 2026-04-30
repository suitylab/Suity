using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Documents;


public enum DocumentLoadingIntent
{
    /// <summary>
    /// Normal loading, used for opening documents.
    /// </summary>
    Normal,

    /// <summary>
    /// Loading for project startup, used for loading documents at project startup. May have different behavior or optimizations compared to normal loading.
    /// </summary>
    ProjectStartup,

    /// <summary>
    /// Loading for import, used for loading documents during import operations. May have different behavior or optimizations compared to normal loading.
    /// </summary>
    Import,

    /// <summary>
    /// Loading for reload, used for reloading documents. May have different behavior or optimizations compared to normal loading.
    /// </summary>
    Reload,
}

/// <summary>
/// Document manager
/// </summary>
public abstract class DocumentManager
{
    /// <summary>
    /// Gets or sets the singleton instance of the document manager.
    /// </summary>
    public static DocumentManager Instance { get; internal set; }


    /// <summary>
    /// Event raised when a new document is created.
    /// </summary>
    public event Action<DocumentEntry> DocumentNew;

    /// <summary>
    /// Event raised when a document is loaded.
    /// </summary>
    public event Action<DocumentEntry> DocumentLoaded;

    /// <summary>
    /// Event raised when a document is saved.
    /// </summary>
    public event Action<DocumentEntry> DocumentSaved;

    /// <summary>
    /// Event raised when a document is closed.
    /// </summary>
    public event Action<DocumentEntry> DocumentClosed;

    /// <summary>
    /// Event raised when a document is changed externally.
    /// </summary>
    public event Action<DocumentEntry> DocumentChangedExternal;

    /// <summary>
    /// Event raised when all documents are saved.
    /// </summary>
    public event Action AllDocumentsSaved;


    /// <summary>
    /// Gets a document format by its name.
    /// </summary>
    /// <param name="formatName">The format name.</param>
    /// <returns>The document format, or null if not found.</returns>
    public abstract DocumentFormat GetDocumentFormat(string formatName);

    /// <summary>
    /// Gets a document format by file extension.
    /// </summary>
    /// <param name="ext">The file extension.</param>
    /// <returns>The document format, or null if not found.</returns>
    public abstract DocumentFormat GetDocumentFormatByExtension(string ext);

    /// <summary>
    /// Gets a document format by file path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The document format, or null if not found.</returns>
    public abstract DocumentFormat GetDocumentFormatByPath(string path);

    /// <summary>
    /// Resolves the document format from a stream based on extension.
    /// </summary>
    /// <param name="ext">The file extension.</param>
    /// <param name="stream">The stream to examine.</param>
    /// <returns>The resolve result.</returns>
    public abstract IDocumentResolveResult ResolveInFileFormat(string ext, Stream stream);

    /// <summary>
    /// Resolves the document format from a file path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The resolve result.</returns>
    public abstract IDocumentResolveResult ResolveInFileFormat(string path);

    /// <summary>
    /// Gets all document formats that support a given extension.
    /// </summary>
    /// <param name="ext">The file extension.</param>
    /// <returns>Collection of document formats.</returns>
    public abstract IEnumerable<DocumentFormat> GetDocumentFormats(string ext);

    /// <summary>
    /// Gets all registered document formats.
    /// </summary>
    /// <returns>Collection of document formats.</returns>
    public abstract IEnumerable<DocumentFormat> GetDocumentFormats();

    /// <summary>
    /// Gets all opened documents.
    /// </summary>
    public abstract IEnumerable<DocumentEntry> AllOpenedDocuments { get; }

    /// <summary>
    /// Opens a document from a storage location.
    /// </summary>
    /// <param name="path">The storage location.</param>
    /// <returns>The document entry.</returns>
    public abstract DocumentEntry OpenDocument(StorageLocation path, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal);

    /// <summary>
    /// Opens a document from a path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The document entry.</returns>
    public abstract DocumentEntry OpenDocument(string path, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal);

    /// <summary>
    /// Reloads a document from a path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The document entry.</returns>
    public abstract DocumentEntry ReloadDocument(string path);

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="format">The document format.</param>
    /// <returns>The document entry.</returns>
    public abstract DocumentEntry NewDocument(string path, DocumentFormat format);

    /// <summary>
    /// Gets a document by storage location.
    /// </summary>
    /// <param name="path">The storage location.</param>
    /// <returns>The document entry, or null if not found.</returns>
    public abstract DocumentEntry GetDocument(StorageLocation path);

    /// <summary>
    /// Gets a document by path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The document entry, or null if not found.</returns>
    public abstract DocumentEntry GetDocument(string path);

    /// <summary>
    /// Clones a document to a new location.
    /// </summary>
    /// <param name="path">Original path.</param>
    /// <param name="pathClone">Clone path.</param>
    /// <returns>The cloned document entry.</returns>
    public abstract DocumentEntry CloneDocument(string path, string pathClone);

    /// <summary>
    /// Closes a document by path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>True if closed successfully.</returns>
    public abstract bool CloseDocument(string path);

    /// <summary>
    /// Closes a document by entry.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    /// <returns>True if closed successfully.</returns>
    public abstract bool CloseDocument(DocumentEntry documentEntry);

    /// <summary>
    /// Closes all open documents.
    /// </summary>
    public abstract void CloseAllDocuments();

    /// <summary>
    /// Shows a document, creating it if necessary.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The document view.</returns>
    public abstract IDocumentView ShowDocument(string path);

    /// <summary>
    /// Shows a document with a specific format.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="format">The document format.</param>
    /// <returns>The document view.</returns>
    public abstract IDocumentView ShowDocument(string path, DocumentFormat format);

    /// <summary>
    /// Shows a document in the property editor.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>True if shown successfully.</returns>
    public abstract bool ShowProperty(string path);

    /// <summary>
    /// Saves all open documents.
    /// </summary>
    public abstract void SaveAllDocuments();

    /// <summary>
    /// Saves all unopened documents.
    /// </summary>
    public abstract void SaveUnopenedDocuments();

    /// <summary>
    /// Cleans up document manager resources.
    /// </summary>
    public abstract void CleanUp();

    /// <summary>
    /// Gets all currently viewing documents.
    /// </summary>
    public abstract DocumentEntry[] ViewingDocuments { get; }



    /// <summary>
    /// Raises the DocumentNew event.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    public void RaiseDocumentNew(DocumentEntry documentEntry)
    {
        DocumentNew?.Invoke(documentEntry);
    }

    /// <summary>
    /// Raises the DocumentLoaded event.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    protected void RaiseDocumentLoaded(DocumentEntry documentEntry)
    {
        DocumentLoaded?.Invoke(documentEntry);
    }

    /// <summary>
    /// Raises the DocumentSaved event.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    public void RaiseDocumentSaved(DocumentEntry documentEntry)
    {
        DocumentSaved?.Invoke(documentEntry);
    }

    /// <summary>
    /// Raises the DocumentClosed event.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    protected void RaiseDocumentClosed(DocumentEntry documentEntry)
    {
        DocumentClosed?.Invoke(documentEntry);
    }

    /// <summary>
    /// Raises the DocumentChangedExternal event.
    /// </summary>
    /// <param name="documentEntry">The document entry.</param>
    public void RaiseDocumentChangeExternal(DocumentEntry documentEntry)
    {
        DocumentChangedExternal?.Invoke(documentEntry);
    }

    /// <summary>
    /// Raises the AllDocumentsSaved event.
    /// </summary>
    protected void RaiseAllDocumentsSaved()
    {
        AllDocumentsSaved?.Invoke();
    }
}