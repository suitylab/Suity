using Suity.Views;
using System;

namespace Suity.Editor.Documents;

/// <summary>
/// [Obsolete] Interface for sub-document views.
/// </summary>
[Obsolete]
public interface IDocumentSubView : IServiceProvider, INavigable
{
    /// <summary>
    /// Event raised when the title of the sub-view changes.
    /// </summary>
    event EventHandler TitleChanged;

    /// <summary>
    /// Gets the title of the sub-view.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Activates the sub-view.
    /// </summary>
    void ActivateView();

    /// <summary>
    /// Gets the UI object associated with this sub-view.
    /// </summary>
    /// <returns>The UI control object.</returns>
    object GetUIObject();

    /// <summary>
    /// Stops the sub-view.
    /// </summary>
    void StopView();
}