using Avalonia.Controls;
using Avalonia.Input;
using Suity.Helpers;
using Suity.Views.Menu;
using AvaloniaImage = Avalonia.Controls.Image; // Avoid confusion with System.Drawing

namespace Suity.Controls;

/// <summary>
/// Avalonia implementation of the menu item view interface.
/// </summary>
public class AvaMenuItemView : IMenuItemView
{
    private Suity.Drawing.ImageDef? _image;
    private readonly MenuItem _menuItem;
    private readonly ItemCollection _items;

    /// <summary>
    /// Initializes a new instance for a root container (e.g., ContextMenu Items).
    /// </summary>
    /// <param name="collection">The item collection.</param>
    public AvaMenuItemView(ItemCollection collection)
    {
        _items = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    /// <summary>
    /// Initializes a new instance for a specific menu item.
    /// </summary>
    /// <param name="menuItem">The menu item.</param>
    public AvaMenuItemView(MenuItem menuItem)
    {
        _menuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
        _items = (ItemCollection)menuItem.Items;

        _menuItem.Click += (s, e) =>
        {
            Click?.Invoke(s, e);
        };
        // Avalonia's MenuItem doesn't have a direct DropDownOpening event,
        // We listen via PointerEntered or template, but here we simply simulate the interface requirement
        _menuItem.SubmenuOpened += (s, e) =>
        {
            DropDownOpening?.Invoke(s, e);
        };
    }

    /// <inheritdoc/>
    public string Text
    {
        get => _menuItem?.Header?.ToString() ?? string.Empty;
        set => _menuItem?.Header = value;
    }

    /// <inheritdoc/>
    public Suity.Drawing.ImageDef? Image
    {
        get => _image; 
        set
        {
            _image = value;
            if (_menuItem != null && value != null)
            {
                // 1. Convert image
                var avaloniaBitmap = AvaConversionHelper.ToAvaloniaBitmapCached(value);

                // 2. In Avalonia, Icon property usually accepts a control
                // We create an Image control and set its Source
                _menuItem.Icon = new AvaloniaImage
                {
                    Source = avaloniaBitmap,
                    Width = 16,  // Recommended to fix menu icon size
                    Height = 16
                };
            }
            else if (_menuItem != null)
            {
                _menuItem.Icon = null;
            }
        }
    }

    /// <inheritdoc/>
    public string HotKey
    {
        get => _menuItem?.HotKey?.ToString() ?? string.Empty;
        set
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var gesture = KeyGesture.Parse(value);
                    _menuItem?.HotKey = gesture;
                    _menuItem?.InputGesture = gesture;
                }
                else
                {
                    _menuItem?.HotKey = null;
                    _menuItem?.InputGesture = null;
                }
            }
            catch (Exception)
            {
            }
        }
    }

    /// <inheritdoc/>
    public bool Visible
    {
        get => _menuItem?.IsVisible ?? false;
        set => _menuItem?.IsVisible = value;
    }

    /// <inheritdoc/>
    public bool Enabled
    {
        get => _menuItem?.IsEnabled ?? false;
        set => _menuItem?.IsEnabled = value;
    }

    /// <inheritdoc/>
    public event EventHandler Click;
    /// <inheritdoc/>
    public event EventHandler DropDownOpening;

    /// <inheritdoc/>
    public IMenuItemView CreateChildItemView()
    {
        var item = new MenuItem();
        _items.Add(item);
        return new AvaMenuItemView(item);
    }

    /// <inheritdoc/>
    public IMenuItemView CreateSeparator()
    {
        var separator = new Separator();
        _items.Add(separator);
        // Separator doesn't need View wrapper operations, return an empty implementation or basic wrapper
        return new AvaloniaSeparatorView(separator);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _items.Clear();
    }
}

/// <summary>
/// Helper class for handling separator menu items.
/// </summary>
internal class AvaloniaSeparatorView : IMenuItemView
{
    private readonly Separator _separator;
    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaSeparatorView"/> class.
    /// </summary>
    /// <param name="separator">The separator control.</param>
    public AvaloniaSeparatorView(Separator separator) => _separator = separator;
    /// <inheritdoc/>
    public string Text { get; set; }
    /// <inheritdoc/>
    public Suity.Drawing.ImageDef Image { get; set; }
    /// <inheritdoc/>
    public string HotKey { get; set; }
    /// <inheritdoc/>
    public bool Visible { get => _separator.IsVisible; set => _separator.IsVisible = value; }
    /// <inheritdoc/>
    public bool Enabled { get; set; }
    /// <inheritdoc/>
    public event EventHandler Click;
    /// <inheritdoc/>
    public event EventHandler DropDownOpening;
    /// <inheritdoc/>
    public IMenuItemView CreateChildItemView() => throw new NotSupportedException();
    /// <inheritdoc/>
    public IMenuItemView CreateSeparator() => throw new NotSupportedException();
    /// <inheritdoc/>
    public void Clear() { }
}