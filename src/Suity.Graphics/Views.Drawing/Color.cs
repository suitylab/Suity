using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Represents an ARGB (alpha, red, green, blue) color.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>
    /// Gets the alpha component value of this <see cref="Color"/> structure.
    /// </summary>
    public byte A { get; }

    /// <summary>
    /// Gets the red component value of this <see cref="Color"/> structure.
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Gets the green component value of this <see cref="Color"/> structure.
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Gets the blue component value of this <see cref="Color"/> structure.
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Color"/> structure is uninitialized.
    /// </summary>
    public bool IsEmpty => this == default;

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> structure from the specified ARGB components.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    public Color(byte alpha, byte red, byte green, byte blue)
    {
        A = alpha;
        R = red;
        G = green;
        B = blue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> structure from the specified RGB components with full alpha.
    /// </summary>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    public Color(byte red, byte green, byte blue)
    {
        A = 255;
        R = red;
        G = green;
        B = blue;
    }

    /// <summary>
    /// Creates a <see cref="Color"/> structure from the specified ARGB components.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    /// <returns>The <see cref="Color"/> structure that this method creates.</returns>
    public static Color FromArgb(byte alpha, byte red, byte green, byte blue)
    {
        return new Color(alpha, red, green, blue);
    }

    /// <summary>
    /// Creates a <see cref="Color"/> structure from the specified ARGB components.
    /// </summary>
    /// <param name="alpha">The alpha component (0-255).</param>
    /// <param name="baseColor">The base <see cref="Color"/> to copy RGB components from.</param>
    /// <returns>The <see cref="Color"/> structure that this method creates.</returns>
    public static Color FromArgb(byte alpha, Color baseColor)
    {
        return new Color(alpha, baseColor.R, baseColor.G, baseColor.B);
    }

    /// <summary>
    /// Creates a <see cref="Color"/> structure from the specified RGB components.
    /// </summary>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    /// <returns>The <see cref="Color"/> structure that this method creates.</returns>
    public static Color FromArgb(byte red, byte green, byte blue)
    {
        return new Color(255, red, green, blue);
    }

    /// <summary>
    /// Creates a <see cref="Color"/> structure from a 32-bit ARGB value.
    /// </summary>
    /// <param name="argb">A 32-bit ARGB value.</param>
    /// <returns>The <see cref="Color"/> structure that this method creates.</returns>
    public static Color FromArgb(int argb)
    {
        return new Color(
            (byte)((argb >> 24) & 0xFF),
            (byte)((argb >> 16) & 0xFF),
            (byte)((argb >> 8) & 0xFF),
            (byte)(argb & 0xFF));
    }

    /// <summary>
    /// Gets the 32-bit ARGB value of this <see cref="Color"/> structure.
    /// </summary>
    /// <returns>The 32-bit ARGB value of this <see cref="Color"/> structure.</returns>
    public int ToArgb()
    {
        return (A << 24) | (R << 16) | (G << 8) | B;
    }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) hue value, in degrees, for this <see cref="Color"/> structure.
    /// </summary>
    /// <returns>The hue, in degrees, of this <see cref="Color"/> structure.</returns>
    public float GetHue()
    {
        if (R == G && G == B)
            return 0f;

        float r = R / 255f;
        float g = G / 255f;
        float b = B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;

        float hue;
        if (max == r)
            hue = 60f * ((g - b) / delta % 6);
        else if (max == g)
            hue = 60f * ((b - r) / delta + 2);
        else
            hue = 60f * ((r - g) / delta + 4);

        if (hue < 0)
            hue += 360f;

        return hue;
    }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) saturation value for this <see cref="Color"/> structure.
    /// </summary>
    /// <returns>The saturation, from 0.0 to 1.0, of this <see cref="Color"/> structure.</returns>
    public float GetSaturation()
    {
        float r = R / 255f;
        float g = G / 255f;
        float b = B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));

        if (max == 0)
            return 0f;

        return (max - min) / max;
    }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) brightness value for this <see cref="Color"/> structure.
    /// </summary>
    /// <returns>The brightness, from 0.0 to 1.0, of this <see cref="Color"/> structure.</returns>
    public float GetBrightness()
    {
        float r = R / 255f;
        float g = G / 255f;
        float b = B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));

        return (max + min) / 2f;
    }

    /// <inheritdoc/>
    public bool Equals(Color other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Color other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return ToArgb();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return IsEmpty ? "Color [Empty]" : $"Color [A={A}, R={R}, G={G}, B={B}]";
    }

    /// <summary>
    /// Tests whether two specified <see cref="Color"/> structures are equivalent.
    /// </summary>
    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Tests whether two specified <see cref="Color"/> structures are different.
    /// </summary>
    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Represents a color that is null.
    /// </summary>
    public static Color Empty => default;

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Transparent => new Color(0, 0, 0, 0);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Black => new Color(255, 0, 0, 0);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color White => new Color(255, 255, 255, 255);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Red => new Color(255, 255, 0, 0);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Green => new Color(255, 0, 128, 0);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Blue => new Color(255, 0, 0, 255);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Yellow => new Color(255, 255, 255, 0);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Cyan => new Color(255, 0, 255, 255);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Magenta => new Color(255, 255, 0, 255);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color Gray => new Color(255, 128, 128, 128);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color DarkGray => new Color(255, 64, 64, 64);

    /// <summary>
    /// Gets a system-defined color.
    /// </summary>
    public static Color LightGray => new Color(255, 192, 192, 192);
}
