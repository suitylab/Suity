using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Represents a brush data structure used for filling the interiors of shapes. Contains fill properties without actual rendering functionality.
/// </summary>
public abstract class Brush
{
    /// <summary>
    /// Creates a clone of this <see cref="Brush"/> object.
    /// </summary>
    /// <returns>A new <see cref="Brush"/> object with the same properties as this one.</returns>
    public abstract Brush Clone();
}

/// <summary>
/// Represents a solid fill brush with a single color.
/// </summary>
public sealed class SolidBrush : Brush
{
    /// <summary>
    /// Gets or sets the color of this <see cref="SolidBrush"/>.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidBrush"/> class with the specified color.
    /// </summary>
    /// <param name="color">A <see cref="Color"/> structure that represents the color of this brush.</param>
    public SolidBrush(Color color)
    {
        Color = color;
    }

    /// <inheritdoc/>
    public override Brush Clone()
    {
        return new SolidBrush(Color);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"SolidBrush [Color={Color}]";
    }
}

/// <summary>
/// Represents a brush that fills an area with a linear gradient.
/// </summary>
public sealed class LinearGradientBrush : Brush
{
    /// <summary>
    /// Gets the starting point of the gradient.
    /// </summary>
    public PointF Point1 { get; }

    /// <summary>
    /// Gets the ending point of the gradient.
    /// </summary>
    public PointF Point2 { get; }

    /// <summary>
    /// Gets or sets the starting color of the gradient.
    /// </summary>
    public Color Color1 { get; set; }

    /// <summary>
    /// Gets or sets the ending color of the gradient.
    /// </summary>
    public Color Color2 { get; set; }

    /// <summary>
    /// Gets or sets the angle of the gradient, in degrees.
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    /// Gets or sets the gradient mode.
    /// </summary>
    public LinearGradientMode Mode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientBrush"/> class with two colors and a direction vector.
    /// </summary>
    /// <param name="point1">A <see cref="PointF"/> that represents the starting point of the gradient.</param>
    /// <param name="point2">A <see cref="PointF"/> that represents the ending point of the gradient.</param>
    /// <param name="color1">A <see cref="Color"/> that represents the starting color of the gradient.</param>
    /// <param name="color2">A <see cref="Color"/> that represents the ending color of the gradient.</param>
    public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
    {
        Point1 = point1;
        Point2 = point2;
        Color1 = color1;
        Color2 = color2;
        Angle = 0f;
        Mode = LinearGradientMode.Linear;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientBrush"/> class with a rectangle, two colors, and an angle.
    /// </summary>
    /// <param name="rect">A <see cref="RectangleF"/> that specifies the bounds of the gradient.</param>
    /// <param name="color1">A <see cref="Color"/> that represents the starting color of the gradient.</param>
    /// <param name="color2">A <see cref="Color"/> that represents the ending color of the gradient.</param>
    /// <param name="angle">The angle, in degrees, of the gradient direction.</param>
    public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle)
    {
        Point1 = new PointF(rect.Left, rect.Top);
        Point2 = new PointF(rect.Right, rect.Bottom);
        Color1 = color1;
        Color2 = color2;
        Angle = angle;
        Mode = LinearGradientMode.Linear;
    }

    /// <inheritdoc/>
    public override Brush Clone()
    {
        return new LinearGradientBrush(Point1, Point2, Color1, Color2)
        {
            Angle = Angle,
            Mode = Mode,
        };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"LinearGradientBrush [Color1={Color1}, Color2={Color2}, Angle={Angle}]";
    }
}

/// <summary>
/// Represents a brush that fills an area with a hatch pattern.
/// </summary>
public sealed class HatchBrush : Brush
{
    /// <summary>
    /// Gets the hatch style of this <see cref="HatchBrush"/>.
    /// </summary>
    public HatchStyle HatchStyle { get; }

    /// <summary>
    /// Gets or sets the foreground color of this <see cref="HatchBrush"/>.
    /// </summary>
    public Color ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of this <see cref="HatchBrush"/>.
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HatchBrush"/> class with the specified hatch style and foreground color.
    /// </summary>
    /// <param name="hatchStyle">A <see cref="HatchStyle"/> that represents the pattern of this brush.</param>
    /// <param name="foreColor">A <see cref="Color"/> that represents the foreground color of this brush.</param>
    public HatchBrush(HatchStyle hatchStyle, Color foreColor)
    {
        HatchStyle = hatchStyle;
        ForegroundColor = foreColor;
        BackgroundColor = Color.Black;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HatchBrush"/> class with the specified hatch style, foreground color, and background color.
    /// </summary>
    /// <param name="hatchStyle">A <see cref="HatchStyle"/> that represents the pattern of this brush.</param>
    /// <param name="foreColor">A <see cref="Color"/> that represents the foreground color of this brush.</param>
    /// <param name="backColor">A <see cref="Color"/> that represents the background color of this brush.</param>
    public HatchBrush(HatchStyle hatchStyle, Color foreColor, Color backColor)
    {
        HatchStyle = hatchStyle;
        ForegroundColor = foreColor;
        BackgroundColor = backColor;
    }

    /// <inheritdoc/>
    public override Brush Clone()
    {
        return new HatchBrush(HatchStyle, ForegroundColor, BackgroundColor);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"HatchBrush [Style={HatchStyle}, Foreground={ForegroundColor}, Background={BackgroundColor}]";
    }
}

/// <summary>
/// Specifies the pattern of a <see cref="HatchBrush"/> object.
/// </summary>
public enum HatchStyle
{
    /// <summary>
    /// Horizontal lines.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical lines.
    /// </summary>
    Vertical,

    /// <summary>
    /// Diagonal lines from upper-left to lower-right.
    /// </summary>
    ForwardDiagonal,

    /// <summary>
    /// Diagonal lines from upper-right to lower-left.
    /// </summary>
    BackwardDiagonal,

    /// <summary>
    /// Cross-hatch pattern.
    /// </summary>
    Cross,

    /// <summary>
    /// Diagonal cross-hatch pattern.
    /// </summary>
    DiagonalCross,

    /// <summary>
    /// 5-percent hatch pattern.
    /// </summary>
    Percent05,

    /// <summary>
    /// 10-percent hatch pattern.
    /// </summary>
    Percent10,

    /// <summary>
    /// 20-percent hatch pattern.
    /// </summary>
    Percent20,

    /// <summary>
    /// 25-percent hatch pattern.
    /// </summary>
    Percent25,

    /// <summary>
    /// 30-percent hatch pattern.
    /// </summary>
    Percent30,

    /// <summary>
    /// 40-percent hatch pattern.
    /// </summary>
    Percent40,

    /// <summary>
    /// 50-percent hatch pattern.
    /// </summary>
    Percent50,

    /// <summary>
    /// 60-percent hatch pattern.
    /// </summary>
    Percent60,

    /// <summary>
    /// 70-percent hatch pattern.
    /// </summary>
    Percent70,

    /// <summary>
    /// 75-percent hatch pattern.
    /// </summary>
    Percent75,

    /// <summary>
    /// 80-percent hatch pattern.
    /// </summary>
    Percent80,

    /// <summary>
    /// 90-percent hatch pattern.
    /// </summary>
    Percent90,

    /// <summary>
    /// Light downward diagonal lines.
    /// </summary>
    LightDownwardDiagonal,

    /// <summary>
    /// Light upward diagonal lines.
    /// </summary>
    LightUpwardDiagonal,

    /// <summary>
    /// Dark downward diagonal lines.
    /// </summary>
    DarkDownwardDiagonal,

    /// <summary>
    /// Dark upward diagonal lines.
    /// </summary>
    DarkUpwardDiagonal,

    /// <summary>
    /// Wide downward diagonal lines.
    /// </summary>
    WideDownwardDiagonal,

    /// <summary>
    /// Wide upward diagonal lines.
    /// </summary>
    WideUpwardDiagonal,

    /// <summary>
    /// Vertical weave pattern.
    /// </summary>
    VerticalBrick,

    /// <summary>
    /// Horizontal weave pattern.
    /// </summary>
    HorizontalBrick,

    /// <summary>
    /// Weave pattern.
    /// </summary>
    Weave,

    /// <summary>
    /// Plaid pattern.
    /// </summary>
    Plaid,

    /// <summary>
    /// Divot pattern.
    /// </summary>
    Divot,

    /// <summary>
    /// Dotted grid pattern.
    /// </summary>
    DottedGrid,

    /// <summary>
    /// Checkerboard pattern.
    /// </summary>
    CheckerBoard,

    /// <summary>
    /// Small checkerboard pattern.
    /// </summary>
    SmallCheckerBoard,

    /// <summary>
    /// Large checkerboard pattern.
    /// </summary>
    LargeCheckerBoard,

    /// <summary>
    /// Outlined diamond pattern.
    /// </summary>
    OutlinedDiamond,

    /// <summary>
    /// Solid diamond pattern.
    /// </summary>
    SolidDiamond,

    /// <summary>
    /// Small grid pattern.
    /// </summary>
    SmallGrid,

    /// <summary>
    /// Small confetti pattern.
    /// </summary>
    SmallConfetti,

    /// <summary>
    /// Large confetti pattern.
    /// </summary>
    LargeConfetti,

    /// <summary>
    /// Zigzag pattern.
    /// </summary>
    ZigZag,

    /// <summary>
    /// Wave pattern.
    /// </summary>
    Wave,

    /// <summary>
    /// Trellis pattern.
    /// </summary>
    Trellis,

    /// <summary>
    /// Sphere pattern.
    /// </summary>
    Sphere,

    /// <summary>
    /// Small ellipse pattern.
    /// </summary>
    SmallEllipsoid,

    /// <summary>
    /// Large ellipse pattern.
    /// </summary>
    LargeEllipsoid,

    /// <summary>
    /// Diamond pattern.
    /// </summary>
    Diamond,

    /// <summary>
    /// 0-percent solid pattern (transparent).
    /// </summary>
    Min,

    /// <summary>
    /// Maximum hatch style value.
    /// </summary>
    Max,
}

/// <summary>
/// Specifies the direction of a linear gradient.
/// </summary>
public enum LinearGradientMode
{
    /// <summary>
    /// A gradient from left to right.
    /// </summary>
    Horizontal,

    /// <summary>
    /// A gradient from top to bottom.
    /// </summary>
    Vertical,

    /// <summary>
    /// A gradient from upper-left to lower-right.
    /// </summary>
    ForwardDiagonal,

    /// <summary>
    /// A gradient from upper-right to lower-left.
    /// </summary>
    BackwardDiagonal,

    /// <summary>
    /// A linear gradient based on an angle.
    /// </summary>
    Linear,
}
