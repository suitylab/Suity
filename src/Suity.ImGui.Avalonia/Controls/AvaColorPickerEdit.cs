using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;

namespace Suity.Controls;

/// <summary>
/// Provides a color picker editing control using Avalonia's Flyout and ColorView.
/// </summary>
public class AvaColorPickerEdit
{
    private readonly Control _targetControl;
    private readonly Flyout _flyout;
    private readonly ColorView _colorView; // Use ColorView instead
    
    private Color _initColor;
    private Action<Color, bool>? _onColorSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaColorPickerEdit"/> class.
    /// </summary>
    /// <param name="control">The target control to anchor the flyout.</param>
    public AvaColorPickerEdit(Control control)
    {
        _targetControl = control;

        // 1. Directly instantiate ColorView
        // ColorView is the core view of the picker, without the outer preview button
        _colorView = new ColorView
        {
            IsAlphaEnabled = true,
            IsColorPreviewVisible = true, // Whether to show the color comparison preview in the top-left corner
            Width = 400,                  // Recommended to set a fixed width to prevent Flyout auto-shrink
            Height = 400
        };

        // 2. Construct Flyout
        _flyout = new Flyout
        {
            Content = _colorView,
            Placement = PlacementMode.TopEdgeAlignedLeft,
            ShowMode = FlyoutShowMode.Standard
        };

        // --- Core modification: Subscribe to color change event ---
        // This event is triggered whenever the user adjusts the color on the panel
        _colorView.ColorChanged += (s, e) =>
        {
            _onColorSelected?.Invoke(e.NewColor, false);
        };

        // 3. Listen for Flyout closing
        _flyout.Closed += (s, e) =>
        {
            if (_colorView.Color != _initColor)
            {
                _onColorSelected?.Invoke(_colorView.Color, true);
            }
            _onColorSelected = null;
        };
    }

    /// <summary>
    /// Shows the color picker at the specified location.
    /// </summary>
    /// <param name="rect">The rectangle defining the picker position.</param>
    /// <param name="initialColor">The initial color to display.</param>
    /// <param name="callBack">The callback invoked when a color is selected.</param>
    public void Show(System.Drawing.Rectangle rect, Color initialColor, Action<Color, bool> callBack)
    {
        _initColor = initialColor;
        _onColorSelected = callBack;
        _colorView.Color = initialColor;

        _flyout.HorizontalOffset = rect.X;
        _flyout.VerticalOffset = rect.Y + _colorView.Bounds.Height;

        if (_targetControl.IsAttachedToVisualTree())
        {
            _flyout.ShowAt(_targetControl);
        }
    }
}