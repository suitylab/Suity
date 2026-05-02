namespace System.Drawing;

/// <summary>
/// Represents a font data structure used for rendering text. Contains font properties without actual rendering functionality.
/// </summary>
public sealed class Font
{
    /// <summary>
    /// Gets the <see cref="FontFamily"/> of this <see cref="Font"/>.
    /// </summary>
    public FontFamily FontFamily { get; }

    /// <summary>
    /// Gets the face name of this <see cref="Font"/>.
    /// </summary>
    public string Name => FontFamily.Name;

    /// <summary>
    /// Gets the size of this <see cref="Font"/> in the units specified by the <see cref="Unit"/> property.
    /// </summary>
    public float Size { get; }

    /// <summary>
    /// Gets the style information for this <see cref="Font"/>.
    /// </summary>
    public FontStyle Style { get; }

    /// <summary>
    /// Gets the unit of measure for this <see cref="Font"/>.
    /// </summary>
    public GraphicsUnit Unit { get; }

    /// <summary>
    /// Gets the height, in pixels, of this <see cref="Font"/>.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Font"/> is bold.
    /// </summary>
    public bool Bold => (Style & FontStyle.Bold) != 0;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Font"/> is italic.
    /// </summary>
    public bool Italic => (Style & FontStyle.Italic) != 0;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Font"/> is underlined.
    /// </summary>
    public bool Underline => (Style & FontStyle.Underline) != 0;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Font"/> is strikethrough.
    /// </summary>
    public bool Strikeout => (Style & FontStyle.Strikeout) != 0;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Font"/> is regular.
    /// </summary>
    public bool Regular => Style == FontStyle.Regular;

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font family and size.
    /// </summary>
    /// <param name="family">The <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font in points.</param>
    public Font(FontFamily family, float emSize)
        : this(family, emSize, FontStyle.Regular, GraphicsUnit.Point)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font family, size, and style.
    /// </summary>
    /// <param name="family">The <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font.</param>
    /// <param name="style">A <see cref="FontStyle"/> that contains style information for the new font.</param>
    public Font(FontFamily family, float emSize, FontStyle style)
        : this(family, emSize, style, GraphicsUnit.Point)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font family, size, style, and unit.
    /// </summary>
    /// <param name="family">The <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font.</param>
    /// <param name="style">A <see cref="FontStyle"/> that contains style information for the new font.</param>
    /// <param name="unit">A <see cref="GraphicsUnit"/> that specifies the unit of measure for the new font.</param>
    public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
    {
        FontFamily = family ?? throw new ArgumentNullException(nameof(family));
        Size = emSize;
        Style = style;
        Unit = unit;
        Height = emSize * 1.2f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font name and size.
    /// </summary>
    /// <param name="familyName">A string representation of the <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font in points.</param>
    public Font(string familyName, float emSize)
        : this(new FontFamily(familyName), emSize, FontStyle.Regular, GraphicsUnit.Point)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font name, size, and style.
    /// </summary>
    /// <param name="familyName">A string representation of the <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font.</param>
    /// <param name="style">A <see cref="FontStyle"/> that contains style information for the new font.</param>
    public Font(string familyName, float emSize, FontStyle style)
        : this(new FontFamily(familyName), emSize, style, GraphicsUnit.Point)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class with the specified font name, size, style, and unit.
    /// </summary>
    /// <param name="familyName">A string representation of the <see cref="FontFamily"/> of the new <see cref="Font"/>.</param>
    /// <param name="emSize">The em-size of the new font.</param>
    /// <param name="style">A <see cref="FontStyle"/> that contains style information for the new font.</param>
    /// <param name="unit">A <see cref="GraphicsUnit"/> that specifies the unit of measure for the new font.</param>
    public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
        : this(new FontFamily(familyName), emSize, style, unit)
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Font [Family={FontFamily.Name}, Size={Size}{GetUnitAbbreviation()}, Style={Style}]";
    }

    private string GetUnitAbbreviation()
    {
        return Unit switch
        {
            GraphicsUnit.Point => "pt",
            GraphicsUnit.Pixel => "px",
            GraphicsUnit.Inch => "in",
            GraphicsUnit.Document => "doc",
            GraphicsUnit.Millimeter => "mm",
            GraphicsUnit.Display => "disp",
            _ => string.Empty,
        };
    }
}

/// <summary>
/// Specifies style information applied to text.
/// </summary>
[Flags]
public enum FontStyle
{
    /// <summary>
    /// Normal text.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Bold text.
    /// </summary>
    Bold = 1,

    /// <summary>
    /// Italic text.
    /// </summary>
    Italic = 2,

    /// <summary>
    /// Underlined text.
    /// </summary>
    Underline = 4,

    /// <summary>
    /// Text with a line through the middle.
    /// </summary>
    Strikeout = 8,
}

/// <summary>
/// Specifies the unit of measure for the specified data.
/// </summary>
public enum GraphicsUnit
{
    /// <summary>
    /// Specifies the world coordinate system unit (often 0.01 inch).
    /// </summary>
    World,

    /// <summary>
    /// Specifies the unit of measure of the display device. Typically pixels for video displays, and 1/100 inch for printers.
    /// </summary>
    Display,

    /// <summary>
    /// Specifies a device pixel as the unit of measure.
    /// </summary>
    Pixel,

    /// <summary>
    /// Specifies a printer's point (1/72 inch) as the unit of measure.
    /// </summary>
    Point,

    /// <summary>
    /// Specifies 1 inch as the unit of measure.
    /// </summary>
    Inch,

    /// <summary>
    /// Specifies the document unit (1/300 inch) as the unit of measure.
    /// </summary>
    Document,

    /// <summary>
    /// Specifies a millimeter as the unit of measure.
    /// </summary>
    Millimeter,
}
