using Suity.Helpers;
using Suity.Views.Graphics;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Style for storing a color value with support for transitions.
/// </summary>
public class GuiColorStyle : IValueTransition<GuiColorStyle>
{
    /// <summary>
    /// The color value.
    /// </summary>
    public Color Color { get; set; }

    /// <inheritdoc/>
    public GuiColorStyle Lerp(GuiColorStyle v2, float t)
    {
        var v = ValuePool.Get<GuiColorStyle>();
        v.Color = ColorHelper.Lerp(Color, v2.Color, t);
        return v;
    }
}

/// <summary>
/// Style for storing border properties with support for transitions.
/// </summary>
public class GuiBorderStyle : IValueTransition<GuiBorderStyle>
{
    /// <summary>
    /// The border width.
    /// </summary>
    public float? Width { get; set; }

    /// <summary>
    /// The border color.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Whether the border width should be scaled.
    /// </summary>
    public bool Scaled { get; set; } = true;

    /// <inheritdoc/>
    public GuiBorderStyle Lerp(GuiBorderStyle v2, float t)
    {
        var v = ValuePool.Get<GuiBorderStyle>();
        if (Width.HasValue && v2.Width.HasValue)
        {
            v.Width = MathHelper.Lerp(Width.Value, v2.Width.Value, t);
        }
        else
        {
            v.Width = Width;
        }
        if (Color.HasValue && v2.Color.HasValue)
        {
            v.Color = ColorHelper.Lerp(Color.Value, v2.Color.Value, t);
        }
        else
        {
            v.Color = Color;
        }
        return v;
    }
}

/// <summary>
/// Style for storing header properties with support for transitions.
/// </summary>
public class GuiHeaderStyle : IValueTransition<GuiHeaderStyle>
{
    /// <summary>
    /// The header width.
    /// </summary>
    public float? Width { get; set; }

    /// <summary>
    /// The header height.
    /// </summary>
    public float? Height { get; set; }

    /// <summary>
    /// The header padding.
    /// </summary>
    public float? Padding { get; set; }

    /// <summary>
    /// The header spacing.
    /// </summary>
    public float? Spacing { get; set; }

    /// <summary>
    /// The header color.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Whether the header is on the right side.
    /// </summary>
    public bool RightSide { get; set; }

    /// <inheritdoc/>
    public GuiHeaderStyle Lerp(GuiHeaderStyle v2, float t)
    {
        var v = ValuePool.Get<GuiHeaderStyle>();
        if (Width.HasValue && v2.Width.HasValue)
        {
            v.Width = MathHelper.Lerp(Width.Value, v2.Width.Value, t);
        }
        else { v.Width = Width; }
        if (Height.HasValue && v2.Height.HasValue)
        {
            v.Height = MathHelper.Lerp(Height.Value, v2.Height.Value, t);
        }
        else { v.Height = Height; }
        if (Padding.HasValue && v2.Padding.HasValue)
        {
            v.Padding = MathHelper.Lerp(Padding.Value, v2.Padding.Value, t);
        }
        else { v.Padding = Padding; }
        if (Spacing.HasValue && v2.Spacing.HasValue)
        {
            v.Spacing = MathHelper.Lerp(Spacing.Value, v2.Spacing.Value, t);
        }
        else { v.Spacing = Spacing; }
        if (Color.HasValue && v2.Color.HasValue)
        {
            v.Color = ColorHelper.Lerp(Color.Value, v2.Color.Value, t);
        }
        else { v.Color = Color; }
        v.RightSide = RightSide;
        return v;
    }
}

/// <summary>
/// Style for storing font properties with support for transitions.
/// </summary>
public class GuiFontStyle : IValueTransition<GuiFontStyle>
{
    /// <summary>
    /// The font.
    /// </summary>
    public Font? Font { get; set; }

    /// <summary>
    /// The font color.
    /// </summary>
    public Color? Color { get; set; }

    /// <inheritdoc/>
    public GuiFontStyle Lerp(GuiFontStyle v2, float t)
    {
        var v = ValuePool.Get<GuiFontStyle>();
        if (Font is { } && v2.Font is { })
        {
            float size = MathHelper.Lerp(Font.Size, v2.Font.Size, t);
            v.Font = new Font(Font.FontFamily, size, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
        }
        else { v.Font = Font; }
        if (Color.HasValue && v2.Color.HasValue)
        {
            v.Color = ColorHelper.Lerp(Color.Value, v2.Color.Value, t);
        }
        else { v.Color = Color; }
        return v;
    }
}

/// <summary>
/// Style for storing corner roundness with support for transitions.
/// </summary>
public class GuiFrameStyle : IValueTransition<GuiFrameStyle>
{
    /// <summary>
    /// The corner roundness radius.
    /// </summary>
    public float? CornerRound { get; set; }

    /// <inheritdoc/>
    public GuiFrameStyle Lerp(GuiFrameStyle v2, float t)
    {
        var v = ValuePool.Get<GuiFrameStyle>();
        if (CornerRound.HasValue && v2.CornerRound.HasValue)
        {
            v.CornerRound = MathHelper.Lerp(CornerRound.Value, v2.CornerRound.Value, t);
        }
        else { v.CornerRound = CornerRound; }
        return v;
    }
}

/// <summary>
/// Style for storing image filter color with support for transitions.
/// </summary>
public class GuiImageFilterStyle : IValueTransition<GuiImageFilterStyle>
{
    /// <summary>
    /// The filter color applied to images.
    /// </summary>
    public Color? Color { get; set; }

    /// <inheritdoc/>
    public GuiImageFilterStyle Lerp(GuiImageFilterStyle v2, float t)
    {
        var v = ValuePool.Get<GuiImageFilterStyle>();
        if (Color.HasValue && v2.Color.HasValue)
        {
            v.Color = ColorHelper.Lerp(Color.Value, v2.Color.Value, t);
        }
        else { v.Color = Color; }
        return v;
    }
}

/// <summary>
/// Style for storing progress bar color with support for transitions.
/// </summary>
public class GuiProgressStyle : IValueTransition<GuiProgressStyle>
{
    /// <summary>
    /// The progress bar color.
    /// </summary>
    public Color Color { get; set; }

    /// <inheritdoc/>
    public GuiProgressStyle Lerp(GuiProgressStyle v2, float t)
    {
        var v = ValuePool.Get<GuiProgressStyle>();
        v.Color = ColorHelper.Lerp(Color, v2.Color, t);
        return v;
    }
}

/// <summary>
/// Style for storing margin values with support for transitions.
/// </summary>
public class GuiMarginStyle : IValueTransition<GuiMarginStyle>
{
    /// <summary>
    /// The margin thickness.
    /// </summary>
    public GuiThickness Margin { get; set; }

    /// <inheritdoc/>
    public GuiMarginStyle Lerp(GuiMarginStyle v2, float t)
    {
        var v = ValuePool.Get<GuiMarginStyle>();
        var padding = new GuiThickness
        {
            Top = MathHelper.Lerp(Margin.Top, v2.Margin.Top, t),
            Bottom = MathHelper.Lerp(Margin.Bottom, v2.Margin.Bottom, t),
            Left = MathHelper.Lerp(Margin.Left, v2.Margin.Left, t),
            Right = MathHelper.Lerp(Margin.Right, v2.Margin.Right, t),
        };
        v.Margin = padding;
        return v;
    }
}

/// <summary>
/// Style for storing padding values with support for transitions.
/// </summary>
public class GuiPaddingStyle : IValueTransition<GuiPaddingStyle>
{
    /// <summary>
    /// The padding thickness.
    /// </summary>
    public GuiThickness Padding { get; set; }

    /// <inheritdoc/>
    public GuiPaddingStyle Lerp(GuiPaddingStyle v2, float t)
    {
        var v = ValuePool.Get<GuiPaddingStyle>();
        var padding = new GuiThickness
        {
            Top = MathHelper.Lerp(Padding.Top, v2.Padding.Top, t),
            Bottom = MathHelper.Lerp(Padding.Bottom, v2.Padding.Bottom, t),
            Left = MathHelper.Lerp(Padding.Left, v2.Padding.Left, t),
            Right = MathHelper.Lerp(Padding.Right, v2.Padding.Right, t),
        };
        v.Padding = padding;
        return v;
    }
}

/// <summary>
/// Style for storing fit orientation.
/// </summary>
public class GuiFitOrientationStyle
{
    /// <summary>
    /// The fit orientation direction.
    /// </summary>
    public GuiOrientation FitOrientation { get; set; }
}

/// <summary>
/// Style for storing child spacing with support for transitions.
/// </summary>
public class GuiChildSpacingStyle : IValueTransition<GuiChildSpacingStyle>
{
    /// <summary>
    /// The spacing between child nodes.
    /// </summary>
    public float ChildSpacing { get; set; }

    /// <inheritdoc/>
    public GuiChildSpacingStyle Lerp(GuiChildSpacingStyle v2, float t)
    {
        var v = ValuePool.Get<GuiChildSpacingStyle>();
        v.ChildSpacing = MathHelper.Lerp(ChildSpacing, v2.ChildSpacing, t);
        return v;
    }
}

/// <summary>
/// Style for storing sibling spacing with support for transitions.
/// </summary>
public class GuiSiblingSpacingStyle : IValueTransition<GuiSiblingSpacingStyle>
{
    /// <summary>
    /// The spacing between sibling nodes.
    /// </summary>
    public float SiblingSpacing { get; set; }

    /// <inheritdoc/>
    public GuiSiblingSpacingStyle Lerp(GuiSiblingSpacingStyle v2, float t)
    {
        var v = ValuePool.Get<GuiSiblingSpacingStyle>();
        v.SiblingSpacing = MathHelper.Lerp(SiblingSpacing, v2.SiblingSpacing, t);
        return v;
    }
}

/// <summary>
/// Style for storing a node factory function.
/// </summary>
public class GuiNodeFactoryStyle
{
    /// <summary>
    /// The node factory function.
    /// </summary>
    public NodeFactory? Factory { get; set; }
}

/// <summary>
/// Style for storing alignment properties.
/// </summary>
public class GuiAlignmentStyle
{
    /// <summary>
    /// The horizontal alignment.
    /// </summary>
    public GuiAlignment? HorizontalAlignment { get; set; }

    /// <summary>
    /// The vertical alignment.
    /// </summary>
    public GuiAlignment? VerticalAlignment { get; set; }

    /// <summary>
    /// Whether the alignment should stretch to fill available space.
    /// </summary>
    public bool Stretch { get; set; }
}

/// <summary>
/// Style for storing size properties with support for transitions.
/// </summary>
public class GuiSizeStyle : IValueTransition<GuiSizeStyle>
{
    /// <summary>
    /// The width.
    /// </summary>
    public GuiLength? Width { get; set; }

    /// <summary>
    /// The height.
    /// </summary>
    public GuiLength? Height { get; set; }

    /// <inheritdoc/>
    public GuiSizeStyle Lerp(GuiSizeStyle v2, float t)
    {
        var v = ValuePool.Get<GuiSizeStyle>();
        if (Width.HasValue && v2.Width.HasValue && Width.Value.Mode == v2.Width.Value.Mode)
        {
            float value = MathHelper.Lerp(Width.Value.Value, v2.Width.Value.Value, t);
            v.Width = new GuiLength(value, Width.Value.Mode);
        }
        else { v.Width = Width; }
        if (Height.HasValue && v2.Height.HasValue && Height.Value.Mode == v2.Height.Value.Mode)
        {
            float value = MathHelper.Lerp(Height.Value.Value, v2.Height.Value.Value, t);
            v.Height = new GuiLength(value, Height.Value.Mode);
        }
        else { v.Height = Height; }
        return v;
    }
}

/// <summary>
/// Style for storing minimum and maximum size constraints.
/// </summary>
public class GuiMinMaxSizeStyle
{
    /// <summary>
    /// The width constraints.
    /// </summary>
    public GuiMinMaxValue? Width { get; set; }

    /// <summary>
    /// The height constraints.
    /// </summary>
    public GuiMinMaxValue? Height { get; set; }
}

/// <summary>
/// Hint text style
/// </summary>
public class GuiHintTextStyle
{
    /// <summary>
    /// Hint text
    /// </summary>
    public string? HintText { get; set; }

    /// <summary>
    /// Password mode
    /// </summary>
    public bool Password { get; set; }

    /// <summary>
    /// Multiple line mode
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    /// Text applying mode.
    /// </summary>
    public TextBoxEditSubmitMode SubmitMode { get; set; }
}

/// <summary>
/// Style for storing an input function by name or reference.
/// </summary>
public class GuiInputFunctionStyle
{
    /// <summary>
    /// The name of the input function to resolve.
    /// </summary>
    public string? FunctionName { get; set; }

    /// <summary>
    /// The direct input function reference.
    /// </summary>
    public InputFunction? Function { get; set; }

    /// <summary>
    /// Resolves the input function, using the direct reference or resolving by name.
    /// </summary>
    /// <param name="node">The ImGui node used as context for resolving the function by name.</param>
    /// <returns>The resolved input function, or null if resolution fails.</returns>
    public InputFunction? Resolve(ImGuiNode node) => Function ?? ImGuiExternal._external.ResolveInputFunction(node, FunctionName!);
}

/// <summary>
/// Style for storing a layout function by name or reference.
/// </summary>
public class GuiLayoutFunctionStyle
{
    /// <summary>
    /// The name of the layout function to resolve.
    /// </summary>
    public string? FunctionName { get; set; }

    /// <summary>
    /// The direct layout function reference.
    /// </summary>
    public LayoutFunction? Function { get; set; }

    /// <summary>
    /// Resolves the layout function, using the direct reference or resolving by name.
    /// </summary>
    /// <param name="node">The ImGui node used as context for resolving the function by name.</param>
    /// <returns>The resolved layout function, or null if resolution fails.</returns>
    public LayoutFunction? Resolve(ImGuiNode node) => Function ?? ImGuiExternal._external.ResolveLayoutFunction(node, FunctionName!);
}

/// <summary>
/// Style for storing a fit function by name or reference.
/// </summary>
public class GuiFitFunctionStyle
{
    /// <summary>
    /// The name of the fit function to resolve.
    /// </summary>
    public string? FunctionName { get; set; }

    /// <summary>
    /// The direct fit function reference.
    /// </summary>
    public FitFunction? Function { get; set; }

    /// <summary>
    /// Resolves the fit function, using the direct reference or resolving by name.
    /// </summary>
    /// <param name="node">The ImGui node used as context for resolving the function by name.</param>
    /// <returns>The resolved fit function, or null if resolution fails.</returns>
    public FitFunction? Resolve(ImGuiNode node) => Function ?? ImGuiExternal._external.ResolveFitFunction(node, FunctionName!);
}

/// <summary>
/// Style for storing a render function by name or reference.
/// </summary>
public class GuiRenderFunctionStyle
{
    /// <summary>
    /// The name of the render function to resolve.
    /// </summary>
    public string? FunctionName { get; set; }

    /// <summary>
    /// The direct render function reference.
    /// </summary>
    public RenderFunction? Function { get; set; }

    /// <summary>
    /// Resolves the render function, using the direct reference or resolving by name.
    /// </summary>
    /// <param name="node">The ImGui node used as context for resolving the function by name.</param>
    /// <returns>The resolved render function, or null if resolution fails.</returns>
    public RenderFunction? Resolve(ImGuiNode node) => Function ?? ImGuiExternal._external.ResolveRenderFunction(node, FunctionName!);
}

/// <summary>
/// Style for storing text alignment.
/// </summary>
public class GuiTextAlignmentStyle
{
    /// <summary>
    /// The text alignment.
    /// </summary>
    public GuiAlignment Alignment { get; set; }
}
