using Suity.Views.Menu;
using System.Drawing;

namespace Suity.Views.Gui;

/// <summary>
/// Defines the contract for a window that displays a view object.
/// Provides metadata such as title, icon, docking hint, and menu commands.
/// </summary>
public interface IViewObjectWindow
{
    /// <summary>
    /// Gets the title displayed in the window's title bar.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the icon displayed in the window's title bar.
    /// </summary>
    Image Icon { get; }

    /// <summary>
    /// Gets the preferred docking position for the window.
    /// </summary>
    DockHint DockHint { get; }

    /// <summary>
    /// Gets the root menu command associated with the window.
    /// </summary>
    RootMenuCommand Menu { get; }

    /// <summary>
    /// Returns the view object displayed by this window.
    /// </summary>
    /// <returns>The view object to display.</returns>
    object GetViewObject();

    /// <summary>
    /// Called when the window is shown.
    /// </summary>
    void NotifyShow();

    /// <summary>
    /// Called when the window is hidden.
    /// </summary>
    void NotifyHide();

    /// <summary>
    /// Called when the window is closed.
    /// </summary>
    void NotifyClose();

    /// <summary>
    /// Activates the view and optionally sets focus to it.
    /// </summary>
    /// <param name="focus">Whether to set input focus to the view.</param>
    void ActivateView(bool focus);
}

/// <summary>
/// A simple implementation of <see cref="IViewObjectWindow"/> that wraps a UI object with metadata.
/// </summary>
internal class SimpleViewObjectWindow : IViewObjectWindow
{
    private object _uiObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleViewObjectWindow"/> class.
    /// </summary>
    /// <param name="uiObject">The UI object to display in the window.</param>
    /// <param name="title">The title of the window.</param>
    /// <param name="icon">The icon of the window.</param>
    /// <param name="menu">The root menu command associated with the window.</param>
    public SimpleViewObjectWindow(object uiObject, string title, Image icon, RootMenuCommand menu)
    {
        _uiObject = uiObject;
        Title = title;
        Icon = icon;
        Menu = menu;
    }

    /// <inheritdoc/>
    public string Title { get; }
    /// <inheritdoc/>
    public Image Icon { get; }
    /// <inheritdoc/>
    public DockHint DockHint { get; }
    /// <inheritdoc/>
    public RootMenuCommand Menu { get; }

    /// <inheritdoc/>
    public object GetViewObject() => _uiObject;

    /// <inheritdoc/>
    public void NotifyShow()
    { }

    /// <inheritdoc/>
    public void NotifyHide()
    { }

    /// <inheritdoc/>
    public void NotifyClose()
    { }

    /// <inheritdoc/>
    public void ActivateView(bool focus)
    { }
}