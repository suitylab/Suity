using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Suity.Views;

namespace Suity.Controls;

/// <summary>
/// Provides a flyout-based drop-down editing control using Avalonia's adorner layer.
/// </summary>
public class AvaDropDownAdornerEdit
{
    private readonly Control _control;   // Host custom control
    private readonly MenuFlyout _flyout;
    private readonly Canvas _container;  // Transit container for positioning anchor

    private Action<object>? _currentCallback;
    private bool _editing;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaDropDownAdornerEdit"/> class.
    /// </summary>
    /// <param name="control">The host control.</param>
    public AvaDropDownAdornerEdit(Control control)
    {
        _control = control;
        _flyout = new MenuFlyout();

        // Create a transparent Canvas as positioning transit station
        _container = new Canvas
        {
            Focusable = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Brushes.Transparent,
            IsHitTestVisible = true
        };

        // Click blank area to close menu (Flyout has LightDismiss, this is double insurance)
        _container.PointerPressed += (s, e) => EndMenu();

        // Listen for Flyout closing event to ensure resource cleanup
        _flyout.Closed += (s, e) => EndMenu();
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
        if (_editing) return;
        _editing = true;
        _currentCallback = callBack;

        // 1. Dynamically build menu items
        _flyout.Items.Clear();
        foreach (var item in items)
        {
            bool isSelected = item.Equals(selectedItem);
            var menuItem = new MenuItem
            {
                Header = GetText(item),
                DataContext = item,
                // Simulate selected effect: bold or show Check
                FontWeight = isSelected ? FontWeight.Bold : FontWeight.Normal,
                Icon = isSelected ? CreateCheckIcon() : null
            };

            menuItem.Click += (s, e) =>
            {
                if (menuItem.DataContext != null)
                {
                    _currentCallback?.Invoke(menuItem.DataContext);
                    _currentCallback = null; // Prevent multiple triggers
                }
            };
            _flyout.Items.Add(menuItem);
        }

        // 2. Get adorner layer and mount container
        var layer = AdornerLayer.GetAdornerLayer(_control);
        if (layer == null) return;

        if (!layer.Children.Contains(_container))
        {
            layer.Children.Add(_container);
        }
        AdornerLayer.SetAdornedElement(_container, _control);

        _container.IsVisible = true;

        // 3. Create an invisible anchor control in Canvas for precise positioning
        var anchor = new Control { Width = rect.Width, Height = 0 };
        Canvas.SetLeft(anchor, rect.X);
        Canvas.SetTop(anchor, rect.Y);

        _container.Children.Clear();
        _container.Children.Add(anchor);

        // 4. [Core]: Must display asynchronously in Dispatcher to ensure layout is updated
        Dispatcher.UIThread.Post(() =>
        {
            _flyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
            // At this point anchor already has position relative to _control
            _flyout.ShowAt(anchor);
        }, DispatcherPriority.Input);
    }

    /// <summary>
    /// Closes the drop-down menu and cleans up resources.
    /// </summary>
    private void EndMenu()
    {
        if (!_editing) return;
        _editing = false;

        _container.IsVisible = false;

        if (_container.Parent is AdornerLayer layer)
        {
            layer.Children.Remove(_container);
        }

        _control.Focus();
    }

    private object CreateCheckIcon()
    {
        // Return a simple checkmark graphic
        return new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse("M1,5 L4,8 L10,1"),
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 2,
            Width = 12,
            Height = 10,
            Margin = new Thickness(2, 0)
        };
    }

    private static string GetText(object obj)
    {
        if (obj is ITextDisplay d && d.DisplayText is { } text && !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return obj.ToString() ?? string.Empty;
    }
}