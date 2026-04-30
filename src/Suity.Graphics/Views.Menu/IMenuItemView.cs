using System;
using System.Drawing;

namespace Suity.Views.Menu;

/// <summary>
/// Interface representing a view for a menu item.
/// </summary>
public interface IMenuItemView
{
    /// <summary>
    /// Gets or sets the text displayed for the menu item.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Gets or sets the icon displayed for the menu item.
    /// </summary>
    Image Image { get; set; }

    /// <summary>
    /// Gets or sets the hot key text for the menu item.
    /// </summary>
    string HotKey { get; set; }

    /// <summary>
    /// Gets or sets the visibility of the menu item.
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Gets or sets whether the menu item is enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Event raised when the menu item is clicked.
    /// </summary>
    event EventHandler Click;

    /// <summary>
    /// Event raised when a drop-down menu is opening.
    /// </summary>
    event EventHandler DropDownOpening;

    /// <summary>
    /// Creates a child menu item view.
    /// </summary>
    /// <returns>A new child menu item view.</returns>
    IMenuItemView CreateChildItemView();

    /// <summary>
    /// Creates a separator view.
    /// </summary>
    /// <returns>A new separator view.</returns>
    IMenuItemView CreateSeparator();

    /// <summary>
    /// Clears all child views.
    /// </summary>
    void Clear();
}

/// <summary>
/// Empty implementation of IMenuItemView that performs no operations.
/// </summary>
public class EmptyMenuItemView : IMenuItemView
{
    /// <summary>
    /// Gets the singleton empty instance.
    /// </summary>
    public static readonly EmptyMenuItemView Empty = new();

    private EmptyMenuItemView()
    {
    }

    /// <inheritdoc/>
    public string Text { get => string.Empty; set { } }

    /// <inheritdoc/>
    public Image Image { get => null; set { } }

    /// <inheritdoc/>
    public string HotKey { get => string.Empty; set { } }

    /// <inheritdoc/>
    public bool Visible { get => false; set { } }

    /// <inheritdoc/>
    public bool Enabled { get => false; set { } }

    /// <inheritdoc/>
    public event EventHandler Click;

    /// <inheritdoc/>
    public event EventHandler DropDownOpening;

    /// <inheritdoc/>
    public IMenuItemView CreateChildItemView() => Empty;

    /// <inheritdoc/>
    public IMenuItemView CreateSeparator() => Empty;

    /// <inheritdoc/>
    public void Clear()
    {
    }

}