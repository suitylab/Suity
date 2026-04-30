using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Suity.Controls;

/// <summary>
/// Provides a context menu-based drop-down editing control for Avalonia.
/// </summary>
public class AvaDropDownContextMenuEdit
{
    private readonly Control _hostControl;
    private readonly ContextMenu _menu;
    private Action<object>? _currentCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaDropDownContextMenuEdit"/> class.
    /// </summary>
    /// <param name="hostControl">The host control.</param>
    public AvaDropDownContextMenuEdit(Control hostControl)
    {
        _hostControl = hostControl;
        _menu = new ContextMenu();

        // ContextMenu supports by default“floating outside”outside window boundaries
    }

    /// <summary>
    /// Shows a drop-down menu with the specified items.
    /// </summary>
    /// <param name="rect">The rectangle defining the drop-down position.</param>
    /// <param name="items">The items to display.</param>
    /// <param name="selectedItem">The currently selected item.</param>
    /// <param name="callBack">The callback invoked when an item is selected.</param>
    public void ShowMenu(
        System.Drawing.Rectangle rect,
        IEnumerable<object> items,
        object? selectedItem,
        Action<object> callBack)
    {
        _currentCallback = callBack;

        // 1. Build menu items
        _menu.Items.Clear();
        foreach (var item in items)
        {
            var isSelected = item.Equals(selectedItem);
            var menuItem = new MenuItem
            {
                Header = GetText(item),
                DataContext = item,
                FontWeight = isSelected ? FontWeight.Bold : FontWeight.Normal,
                Icon = isSelected ? CreateCheckIcon() : null
            };

            menuItem.Click += (s, e) =>
            {
                if (menuItem.DataContext is { } data)
                {
                    _currentCallback?.Invoke(data);
                }
            };
            _menu.Items.Add(menuItem);
        }

        // 2. Position and popup
        // In Avalonia 11, set via Placement property
        _menu.Placement = PlacementMode.TopEdgeAlignedLeft;

        // Key: Although ContextMenu does not have PlacementRect property,
        // we can manually set its offset or popup directly based on hostControl.
        // If you need precise alignment to rect, the simplest method is to calculate logical offset:
        _menu.HorizontalOffset = rect.X;
        _menu.VerticalOffset = rect.Y + _menu.Bounds.Height;

        // 3. Popup menu
        _menu.Open(_hostControl);
    }

    private static Visual CreateCheckIcon() => new Avalonia.Controls.Shapes.Path
    {
        Data = Geometry.Parse("M1,5 L4,8 L10,1"),
        Stroke = Brushes.DodgerBlue,
        StrokeThickness = 2,
        Width = 12,
        Height = 10,
        Margin = new Thickness(2, 0)
    };

    private static string GetText(object obj) => obj?.ToString() ?? string.Empty;
}