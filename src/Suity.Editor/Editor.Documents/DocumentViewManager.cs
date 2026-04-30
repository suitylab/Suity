using System.Collections.Generic;

namespace Suity.Editor.Documents;

/// <summary>
/// Document view manager
/// </summary>
public abstract class DocumentViewManager
{
    private static DocumentViewManager _current;

    /// <summary>
    /// Gets or sets the current document view manager instance.
    /// </summary>
    public static DocumentViewManager Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Device.Current.GetService<DocumentViewManager>();
            return _current;
        }
        internal set
        {
            _current = value;
        }
    }

    /// <summary>
    /// Gets the document view
    /// </summary>
    /// <param name="entry">The document entry.</param>
    /// <returns>The document view, or null if not available.</returns>
    public abstract IDocumentView GetDocumentView(DocumentEntry entry);

    /// <summary>
    /// Shows the document view, creates a new view if the document has none
    /// </summary>
    /// <param name="entry">Document</param>
    public abstract IDocumentView ShowDocumentView(DocumentEntry entry);

    /// <summary>
    /// Closes the document
    /// </summary>
    /// <param name="entry">Document</param>
    /// <returns>True if the document was closed successfully.</returns>
    public abstract bool CloseDocument(DocumentEntry entry);

    /// <summary>
    /// Sets focus to the specified document
    /// </summary>
    /// <param name="entry">Document</param>
    /// <returns>True if focus was set successfully.</returns>
    public abstract bool FocusDocument(DocumentEntry entry);


    /// <summary>
    /// Gets all opened documents
    /// </summary>
    public abstract IEnumerable<DocumentEntry> OpenedDocuments { get; }

    /// <summary>
    /// Gets the focused document
    /// </summary>
    public abstract DocumentEntry ActiveDocument { get; }

    /// <summary>
    /// Refreshes the view for the specified document entry.
    /// </summary>
    /// <param name="entry">The document entry to refresh.</param>
    public void RefreshView(DocumentEntry entry)
    {
        var view = GetDocumentView(entry);

        view?.RefreshView();
    }

    /// <summary>
    /// Refreshes the view for the specified document.
    /// </summary>
    /// <param name="document">The document to refresh.</param>
    public void RefreshDocumentView(Document document)
    {
        if (document?.Entry is not { } entry)
        {
            return;
        }

        var view = GetDocumentView(entry);

        view?.RefreshView();
    }
}