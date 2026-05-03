using Suity.Drawing;
using Suity.Views.Graphics;
using System.Drawing;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Provides extension methods for configuring axis-style numeric input fields.
/// </summary>
public static class AxisEditorExtensions
{
    /// <summary>
    /// Configures the theme with an axis indicator for numeric input fields.
    /// </summary>
    /// <param name="theme">The theme to configure.</param>
    /// <param name="axis">The axis identifier (e.g., "X", "Y", "Z").</param>
    /// <param name="color">The color of the axis indicator.</param>
    /// <param name="width">The width of the axis indicator in pixels. Default is 3.</param>
    /// <param name="offset">The horizontal offset from the input field. Default is 10.</param>
    /// <returns>The configured theme instance.</returns>
    public static ImGuiTheme SetAxisNumericInput(this ImGuiTheme theme, string axis, Color color, int width = 3, int offset = 10)
    {
        theme.SetStyle(new AxisStyle { Axis = axis, Color = color, Width = width, Offset = offset });
        theme.SetRenderFunctionChain(RenderAxis);

        return theme;
    }

    private static void RenderAxis(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeline);

        var style = node.GetStyle<AxisStyle>();
        if (style is null)
        {
            return;
        }

        var rect = node.InnerRect;
        rect.Width = style.Width;
        rect.X -= style.Offset;

        var brush = new SolidBrushDef(style.Color);
        output.FillRectangle(brush, rect);
    }
}

/// <summary>
/// Defines styling properties for axis indicators in numeric input fields.
/// </summary>
public class AxisStyle
{
    /// <summary>
    /// Gets or sets the axis identifier (e.g., "X", "Y", "Z").
    /// </summary>
    public string? Axis { get; set; }

    /// <summary>
    /// Gets or sets the color of the axis indicator.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets or sets the width of the axis indicator in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the horizontal offset from the input field.
    /// </summary>
    public int Offset { get; set; }
}
