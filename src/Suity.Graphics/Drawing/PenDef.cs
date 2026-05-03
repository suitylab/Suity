using System.Drawing;

namespace Suity.Drawing;

/// <summary>
/// Represents a pen data structure used for drawing lines and curves. Contains stroke properties without actual rendering functionality.
/// </summary>
public sealed class PenDef
{
    /// <summary>
    /// Gets or sets the color of this <see cref="PenDef"/>.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets or sets the width of this <see cref="PenDef"/>, in units of the graphics object.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the dash style for lines drawn by this <see cref="PenDef"/>.
    /// </summary>
    public DashStyle DashStyle { get; set; }

    /// <summary>
    /// Gets or sets the custom dash pattern for lines drawn by this <see cref="PenDef"/>.
    /// </summary>
    public float[] DashPattern { get; set; }

    /// <summary>
    /// Gets or sets the line cap style for the start of lines drawn by this <see cref="PenDef"/>.
    /// </summary>
    public LineCap StartCap { get; set; }

    /// <summary>
    /// Gets or sets the line cap style for the end of lines drawn by this <see cref="PenDef"/>.
    /// </summary>
    public LineCap EndCap { get; set; }

    /// <summary>
    /// Gets or sets the line join style for the ends of lines drawn by this <see cref="PenDef"/>.
    /// </summary>
    public LineJoin LineJoin { get; set; }

    /// <summary>
    /// Gets or sets the miter limit for mitered joins.
    /// </summary>
    public float MiterLimit { get; set; }

    /// <summary>
    /// Gets or sets the alignment of the pen relative to the path.
    /// </summary>
    public PenAlignment Alignment { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PenDef"/> class with the specified color.
    /// </summary>
    /// <param name="color">A <see cref="Color"/> structure that indicates the color of this pen.</param>
    public PenDef(Color color)
    {
        Color = color;
        Width = 1f;
        DashStyle = DashStyle.Solid;
        StartCap = LineCap.Flat;
        EndCap = LineCap.Flat;
        LineJoin = LineJoin.Miter;
        MiterLimit = 10f;
        Alignment = PenAlignment.Center;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PenDef"/> class with the specified color and width.
    /// </summary>
    /// <param name="color">A <see cref="Color"/> structure that indicates the color of this pen.</param>
    /// <param name="width">A value indicating the width of this pen.</param>
    public PenDef(Color color, float width)
    {
        Color = color;
        Width = width;
        DashStyle = DashStyle.Solid;
        StartCap = LineCap.Flat;
        EndCap = LineCap.Flat;
        LineJoin = LineJoin.Miter;
        MiterLimit = 10f;
        Alignment = PenAlignment.Center;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Pen [Color={Color}, Width={Width}, DashStyle={DashStyle}]";
    }
}

/// <summary>
/// Specifies the style of dashed lines drawn with a <see cref="PenDef"/> object.
/// </summary>
public enum DashStyle
{
    /// <summary>
    /// A solid line.
    /// </summary>
    Solid,

    /// <summary>
    /// A dashed line.
    /// </summary>
    Dash,

    /// <summary>
    /// A dotted line.
    /// </summary>
    Dot,

    /// <summary>
    /// A line consisting of alternating dashes and dots.
    /// </summary>
    DashDot,

    /// <summary>
    /// A line consisting of alternating dashes and double dots.
    /// </summary>
    DashDotDot,

    /// <summary>
    /// A user-defined custom dash style.
    /// </summary>
    Custom,
}

/// <summary>
/// Specifies the line cap style for the start or end of a line drawn with a <see cref="PenDef"/> object.
/// </summary>
public enum LineCap
{
    /// <summary>
    /// A flat line cap.
    /// </summary>
    Flat,

    /// <summary>
    /// A square line cap.
    /// </summary>
    Square,

    /// <summary>
    /// A rounded line cap.
    /// </summary>
    Round,

    /// <summary>
    /// A triangular line cap.
    /// </summary>
    Triangle,

    /// <summary>
    /// A flat line cap that extends beyond the endpoint.
    /// </summary>
    NoAnchor,

    /// <summary>
    /// A square line cap that extends beyond the endpoint.
    /// </summary>
    SquareAnchor,

    /// <summary>
    /// A rounded line cap that extends beyond the endpoint.
    /// </summary>
    RoundAnchor,

    /// <summary>
    /// A diamond-shaped line cap.
    /// </summary>
    DiamondAnchor,

    /// <summary>
    /// An arrow-shaped line cap.
    /// </summary>
    ArrowAnchor,

    /// <summary>
    /// A mask specifying all anchor caps.
    /// </summary>
    AnchorMask,

    /// <summary>
    /// A custom line cap.
    /// </summary>
    Custom,
}

/// <summary>
/// Specifies how to join consecutive line or curve segments in a figure.
/// </summary>
public enum LineJoin
{
    /// <summary>
    /// A mitered join. Produces a sharp corner or a clipped corner.
    /// </summary>
    Miter,

    /// <summary>
    /// A beveled join. Produces a diagonal corner.
    /// </summary>
    Bevel,

    /// <summary>
    /// A rounded join. Produces a smooth, circular arc between the lines.
    /// </summary>
    Round,

    /// <summary>
    /// A mitered join that becomes a beveled join when the miter limit is exceeded.
    /// </summary>
    MiterClipped,
}

/// <summary>
/// Specifies the alignment of a <see cref="PenDef"/> object relative to the path being drawn.
/// </summary>
public enum PenAlignment
{
    /// <summary>
    /// The pen is centered on the path.
    /// </summary>
    Center,

    /// <summary>
    /// The pen is inset from the path.
    /// </summary>
    Inset,
}
