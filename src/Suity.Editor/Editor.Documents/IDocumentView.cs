using Suity.Views;
using System;

namespace Suity.Editor.Documents;

/// <summary>
/// Document view interface for displaying and editing document content.
/// </summary>
public interface IDocumentView : IObjectView, IServiceProvider
{
    /// <summary>
    /// Starts the view with a document and host.
    /// </summary>
    /// <param name="document">The document to display.</param>
    /// <param name="host">The document view host.</param>
    void StartView(Document document, IDocumentViewHost host);

    /// <summary>
    /// Stops the view and releases resources.
    /// </summary>
    void StopView();

    /// <summary>
    /// Activates the view, optionally focusing it.
    /// </summary>
    /// <param name="focus">Whether to focus the view.</param>
    void ActivateView(bool focus);

    /// <summary>
    /// Gets the UI control object for the view.
    /// </summary>
    /// <returns>The UI control object.</returns>
    object GetUIObject();

    /// <summary>
    /// Pushes view content to the document.
    /// </summary>
    void SetDataToDocument();

    /// <summary>
    /// Gets data from the document and updates the view.
    /// </summary>
    void GetDataFromDocument();

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    void RefreshView();
}

/// <summary>
/// Interface for managing sub-document views.
/// </summary>
public interface IHasSubDocumentView
{
    /// <summary>
    /// Opens a sub-view for the specified document.
    /// </summary>
    /// <param name="document">The document for the sub-view.</param>
    /// <returns>The UI object for the sub-view.</returns>
    object OpenSubView(Document document);

    /// <summary>
    /// Gets the current sub-view.
    /// </summary>
    object CurrentSubView { get; }
}