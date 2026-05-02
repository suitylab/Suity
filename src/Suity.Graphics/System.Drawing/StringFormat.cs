namespace System.Drawing;

/// <summary>
/// Encapsulates text layout information such as alignment, line spacing, and display manipulations.
/// </summary>
public sealed class StringFormat : ICloneable, IDisposable
{
    /// <summary>
    /// Gets the default <see cref="StringFormat"/> object.
    /// </summary>
    public static StringFormat GenericDefault => new StringFormat();

    /// <summary>
    /// Gets a generic <see cref="StringFormat"/> object that represents a typographic string format.
    /// </summary>
    public static StringFormat GenericTypographic => new StringFormat(StringFormatFlags.NoFontFallback);

    /// <summary>
    /// Gets or sets the <see cref="StringAlignment"/> information for this <see cref="StringFormat"/>.
    /// </summary>
    public StringAlignment Alignment { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StringAlignment"/> information for this <see cref="StringFormat"/>.
    /// </summary>
    public StringAlignment LineAlignment { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StringFormatFlags"/> for this <see cref="StringFormat"/>.
    /// </summary>
    public StringFormatFlags FormatFlags { get; set; }

    /// <summary>
    /// Gets or sets the number of spaces between the beginning of the text string and the beginning of the text.
    /// </summary>
    public float FirstTabOffset { get; set; }

    /// <summary>
    /// Gets or sets the number of characters that can be contained in the output string.
    /// </summary>
    public int HotkeyPrefix { get; set; }

    /// <summary>
    /// Gets the tab stops for this <see cref="StringFormat"/>.
    /// </summary>
    public float[] TabStops { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StringTrimming"/> enumeration for this <see cref="StringFormat"/>.
    /// </summary>
    public StringTrimming Trimming { get; set; }

    /// <summary>
    /// Gets or sets the digit substitution language.
    /// </summary>
    public StringDigitSubstitute DigitSubstitutionLanguage { get; set; }

    /// <summary>
    /// Gets or sets the digit substitution method.
    /// </summary>
    public StringDigitSubstitute DigitSubstitutionMethod { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringFormat"/> class.
    /// </summary>
    public StringFormat()
    {
        Alignment = StringAlignment.Near;
        LineAlignment = StringAlignment.Near;
        FormatFlags = StringFormatFlags.NoClip;
        FirstTabOffset = 0f;
        HotkeyPrefix = 0;
        Trimming = StringTrimming.Word;
        DigitSubstitutionLanguage = StringDigitSubstitute.User;
        DigitSubstitutionMethod = StringDigitSubstitute.User;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringFormat"/> class with the specified <see cref="StringFormatFlags"/>.
    /// </summary>
    /// <param name="flags">A <see cref="StringFormatFlags"/> that contains formatting information.</param>
    public StringFormat(StringFormatFlags flags)
        : this()
    {
        FormatFlags = flags;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringFormat"/> class based on the specified existing <see cref="StringFormat"/> object.
    /// </summary>
    /// <param name="format">The <see cref="StringFormat"/> to copy from.</param>
    public StringFormat(StringFormat format)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        Alignment = format.Alignment;
        LineAlignment = format.LineAlignment;
        FormatFlags = format.FormatFlags;
        FirstTabOffset = format.FirstTabOffset;
        HotkeyPrefix = format.HotkeyPrefix;
        Trimming = format.Trimming;
        DigitSubstitutionLanguage = format.DigitSubstitutionLanguage;
        DigitSubstitutionMethod = format.DigitSubstitutionMethod;

        if (format.TabStops != null)
        {
            TabStops = new float[format.TabStops.Length];
            Array.Copy(format.TabStops, TabStops, format.TabStops.Length);
        }
    }

    /// <summary>
    /// Sets the measurable character ranges.
    /// </summary>
    /// <param name="characterRanges">An array of <see cref="CharacterRange"/> structures.</param>
    public void SetMeasurableCharacterRanges(CharacterRange[] characterRanges)
    {
        MeasurableCharacterRanges = characterRanges;
    }

    /// <summary>
    /// Gets or sets the measurable character ranges.
    /// </summary>
    public CharacterRange[] MeasurableCharacterRanges { get; private set; }

    /// <summary>
    /// Sets the tab stops.
    /// </summary>
    /// <param name="firstTabOffset">The number of spaces between the beginning of the text string and the first tab stop.</param>
    /// <param name="tabStops">An array of distances between successive tab stops.</param>
    public void SetTabStops(float firstTabOffset, float[] tabStops)
    {
        FirstTabOffset = firstTabOffset;
        TabStops = tabStops;
    }

    /// <summary>
    /// Creates a copy of this <see cref="StringFormat"/> object.
    /// </summary>
    /// <returns>A new <see cref="StringFormat"/> object with the same properties.</returns>
    public StringFormat Clone()
    {
        return new StringFormat(this);
    }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"StringFormat [Alignment={Alignment}, LineAlignment={LineAlignment}, FormatFlags={FormatFlags}, Trimming={Trimming}]";
    }
}

/// <summary>
/// Specifies the alignment of a text string relative to its layout rectangle.
/// </summary>
public enum StringAlignment
{
    /// <summary>
    /// Specifies that the text is aligned near the layout rectangle.
    /// </summary>
    Near = 0,

    /// <summary>
    /// Specifies that the text is centered within the layout rectangle.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Specifies that the text is aligned far from the layout rectangle.
    /// </summary>
    Far = 2,
}

/// <summary>
/// Specifies display and layout information for text strings.
/// </summary>
[Flags]
public enum StringFormatFlags
{
    /// <summary>
    /// No format flags.
    /// </summary>
    NoClip = 0x0000,

    /// <summary>
    /// Text is laid out right-to-left.
    /// </summary>
    DirectionRightToLeft = 0x0001,

    /// <summary>
    /// Text is laid out vertically.
    /// </summary>
    DirectionVertical = 0x0002,

    /// <summary>
    /// Portions of characters that fit outside the layout rectangle are clipped.
    /// </summary>
    FitBlackBox = 0x0004,

    /// <summary>
    /// No font fallback is used.
    /// </summary>
    NoFontFallback = 0x0008,

    /// <summary>
    /// No wrapping is performed.
    /// </summary>
    NoWrap = 0x0010,

    /// <summary>
    /// The text is laid out vertically.
    /// </summary>
    LineLimit = 0x0020,

    /// <summary>
    /// Only entire lines are laid out.
    /// </summary>
    MeasureTrailingSpaces = 0x0040,

    /// <summary>
    /// Controls are disabled from using the keyboard to change the text.
    /// </summary>
    NoFitBlackBox = 0x0080,
}

/// <summary>
/// Specifies how to trim characters from a string that does not completely fit into a layout shape.
/// </summary>
public enum StringTrimming
{
    /// <summary>
    /// No trimming.
    /// </summary>
    None = 0,

    /// <summary>
    /// Trimmed to the nearest character.
    /// </summary>
    Character = 1,

    /// <summary>
    /// Trimmed to the nearest word.
    /// </summary>
    Word = 2,

    /// <summary>
    /// Trimmed with an ellipsis at the end.
    /// </summary>
    EllipsisCharacter = 3,

    /// <summary>
    /// Trimmed with an ellipsis at the end of the last word.
    /// </summary>
    EllipsisWord = 4,

    /// <summary>
    /// Trimmed with an ellipsis in the middle.
    /// </summary>
    EllipsisPath = 5,
}

/// <summary>
/// Specifies how digit substitution is handled.
/// </summary>
public enum StringDigitSubstitute
{
    /// <summary>
    /// User-defined substitution.
    /// </summary>
    User = 0,

    /// <summary>
    /// No substitution.
    /// </summary>
    None = 1,

    /// <summary>
    /// National substitution.
    /// </summary>
    National = 2,

    /// <summary>
    /// Traditional substitution.
    /// </summary>
    Traditional = 3,
}

/// <summary>
/// Defines a range of characters in a string.
/// </summary>
public struct CharacterRange : IEquatable<CharacterRange>
{
    /// <summary>
    /// Gets or sets the starting position of the character range.
    /// </summary>
    public int First { get; set; }

    /// <summary>
    /// Gets or sets the length of the character range.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterRange"/> structure.
    /// </summary>
    /// <param name="first">The starting position of the character range.</param>
    /// <param name="length">The length of the character range.</param>
    public CharacterRange(int first, int length)
    {
        First = first;
        Length = length;
    }

    /// <inheritdoc/>
    public bool Equals(CharacterRange other)
    {
        return First == other.First && Length == other.Length;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is CharacterRange other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (First * 397) ^ Length;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"CharacterRange [First={First}, Length={Length}]";
    }

    /// <summary>
    /// Tests whether two <see cref="CharacterRange"/> structures are equal.
    /// </summary>
    public static bool operator ==(CharacterRange left, CharacterRange right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two <see cref="CharacterRange"/> structures are different.
    /// </summary>
    public static bool operator !=(CharacterRange left, CharacterRange right)
    {
        return !left.Equals(right);
    }
}
