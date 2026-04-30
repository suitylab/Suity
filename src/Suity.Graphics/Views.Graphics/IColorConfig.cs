using Suity.Helpers;
using System.Drawing;

namespace Suity.Views.Graphics;

/// <summary>
/// Interface for retrieving colors based on style.
/// </summary>
public interface IColorConfig
{
    /// <summary>
    /// Gets the color for the specified style.
    /// </summary>
    /// <param name="style">The color style.</param>
    /// <returns>The color associated with the style.</returns>
    Color GetColor(ColorStyle style);
}

/// <summary>
/// Default implementation of IColorConfig with predefined color schemes.
/// </summary>
public class DefaultColorConfig : IColorConfig
{
    // Standard Blue: 008EFF
    // Purple icon color: D9BAFF
    // Green icon color: 2FB688
    // Light blue: CBE4FF

    /// <summary>
    /// Gets the singleton default instance.
    /// </summary>
    public static readonly DefaultColorConfig Default = new();

    protected DefaultColorConfig()
    {
    }

    /// <summary>
    /// Gets the MDI background color.
    /// </summary>
    public Color MdiBackground { get; } = ColorHelper.IntToColor(0x1B1B1B);

    /// <summary>
    /// Gets the primary background color.
    /// </summary>
    public Color Background { get; } = ColorHelper.IntToColor(0x535353);

    /// <summary>
    /// Gets the inner background color.
    /// </summary>
    public Color BackgroundInner { get; } = ColorHelper.IntToColor(0x323232);

    /// <summary>
    /// Gets the focus color.
    /// </summary>
    public Color Focus => ColorHelper.IntToColor(0x737373);

    /// <summary>
    /// Gets the inactive focus color.
    /// </summary>
    public Color FocusInactive => ColorHelper.IntToColor(0x606060);

    private readonly Color _styleMdiBackground = ColorHelper.IntToColor(0x1B1B1B);
    private readonly Color _styleBackground = ColorHelper.IntToColor(0x242424);
    private readonly Color _styleBackgroundInner = ColorHelper.IntToColor(0x2d2d2d);

    private readonly Color _styleFocus = ColorHelper.IntToColor(0x737373);
    private readonly Color _styleFocusInactive = ColorHelper.IntToColor(0x606060);

    private readonly Color _styleButton = ColorHelper.IntToColor(0x2693DD);

    private readonly Color _styleScrollBar = ColorHelper.IntToColor(0x1B1B1B, 128);

    private readonly Color _styleProgressBar = ColorHelper.IntToColor(0x858585, 128);

    private readonly Color _statusError = ColorHelper.IntToColor(0xFF0000);
    private readonly Color _statusWarning = ColorHelper.IntToColor(0xFF9B00);
    private readonly Color _statusInfo = ColorHelper.IntToColor(0x008EFF/*0x789AFF*/);
    private readonly Color _statusComment = ColorHelper.IntToColor(0x27BB1E);
    private readonly Color _statusNormal = ColorHelper.IntToColor(0xF0F0F0);
    private readonly Color _statusDisabled = ColorHelper.IntToColor(0xB0B0B0);
    private readonly Color _statusAnonlymouse = ColorHelper.IntToColor(0x008EFF);

    private readonly Color _statusImport = ColorHelper.IntToColor(0x8DD0B2);
    private readonly Color _statusTag = ColorHelper.IntToColor(0x008EFF);
    private readonly Color _statusUserCode = ColorHelper.IntToColor(0x7CB2D2);
    private readonly Color _statusResourceUse = ColorHelper.IntToColor(0x10CFEC);

    private readonly Color _statusRefA = ColorHelper.IntToColor(0x008EFF);
    private readonly Color _statusRefB = ColorHelper.IntToColor(0xA784FF);
    private readonly Color _statusRefC = ColorHelper.IntToColor(0x2FB688);

    /// <inheritdoc/>
    public virtual Color GetColor(ColorStyle style)
    {
        switch (style)
        {
            case ColorStyle.Normal:
                return _statusNormal;

            case ColorStyle.MdiBackground:
                return _styleMdiBackground;

            case ColorStyle.Background:
                return _styleBackground;

            case ColorStyle.BackgroundInner:
                return _styleBackgroundInner;

            case ColorStyle.Focus:
                return _styleFocus;

            case ColorStyle.FocusInactive:
                return _styleFocusInactive;

            case ColorStyle.Button:
            case ColorStyle.CheckBox:
                return _styleButton;

            case ColorStyle.Border:
                return _styleBackgroundInner;

            case ColorStyle.ScrollBar:
                return _styleScrollBar;

            case ColorStyle.ProgressBar:
                return _styleProgressBar;

            default:
                return Color.Black;
        }
    }
}