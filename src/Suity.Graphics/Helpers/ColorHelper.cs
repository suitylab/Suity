using Suity.Drawing;
using System.Drawing;

namespace Suity.Helpers;

/// <summary>
/// Helper methods for color operations and conversions.
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// Converts a Color to a nullable Color, returning null if empty.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The nullable color.</returns>
    public static Color? ToNullable(this Color color)
    {
        if (color == Color.Empty)
        {
            return null;
        }

        return color;
    }

    /// <summary>
    /// Converts a nullable Color to null if empty or has no value.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The nullable color.</returns>
    public static Color? ToNullable(this Color? color)
    {
        if (!color.HasValue || color == Color.Empty)
        {
            return null;
        }

        return color;
    }

    /// <summary>
    /// Converts an RGB integer value to a Color.
    /// </summary>
    /// <param name="rgb">The RGB value (0xRRGGBB).</param>
    /// <returns>The color with full alpha.</returns>
    public static Color IntToColor(int rgb)
    {
        return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    /// <summary>
    /// Converts an RGB integer value to a Color with specified alpha.
    /// </summary>
    /// <param name="rgb">The RGB value (0xRRGGBB).</param>
    /// <param name="alpha">The alpha value (0-255).</param>
    /// <returns>The color with specified alpha.</returns>
    public static Color IntToColor(int rgb, byte alpha)
    {
        return Color.FromArgb(alpha, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    /// <summary>
    /// Multiplies the RGB components of a color by a factor.
    /// </summary>
    /// <param name="color">The color to modify.</param>
    /// <param name="m">The multiplication factor.</param>
    /// <returns>The modified color.</returns>
    public static Color Multiply(this Color color, float m)
    {
        byte r = (byte)(color.R * m);
        byte g = (byte)(color.G * m);
        byte b = (byte)(color.B * m);

        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Multiplies the alpha component of a color by a factor.
    /// </summary>
    /// <param name="color">The color to modify.</param>
    /// <param name="m">The multiplication factor.</param>
    /// <returns>The modified color.</returns>
    public static Color MultiplyAlpha(this Color color, float m)
    {
        int a = (int)(color.A * m);

        return Color.FromArgb(a, color);
    }

    /// <summary>
    /// Adds values to the RGB components of a color.
    /// </summary>
    /// <param name="color">The color to modify.</param>
    /// <param name="red">The red component addition.</param>
    /// <param name="green">The green component addition.</param>
    /// <param name="blue">The blue component addition.</param>
    /// <returns>The modified color.</returns>
    public static Color Add(this Color color, int red, int green, int blue)
    {
        int r = color.R + red;
        int g = color.G + green;
        int b = color.B + blue;

        if (r > 255) r = 255;
        if (r < 0) r = 0;

        if (g > 255) g = 255;
        if (g < 0) g = 0;

        if (b > 255) b = 255;
        if (b < 0) b = 0;

        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Multiplies the color towards white by a factor (inverse multiplication).
    /// </summary>
    /// <param name="color">The color to modify.</param>
    /// <param name="m">The multiplication factor.</param>
    /// <returns>The modified color.</returns>
    public static Color MultiplyRevert(this Color color, float m)
    {
        byte r = (byte)(255 - (255 - color.R) * m);
        byte g = (byte)(255 - (255 - color.G) * m);
        byte b = (byte)(255 - (255 - color.B) * m);

        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="v1">The first color.</param>
    /// <param name="v2">The second color.</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>The interpolated color.</returns>
    public static Color Lerp(Color v1, Color v2, float t)
    {
        var a = (byte)(v1.A + (v2.A - v1.A) * t);
        var r = (byte)(v1.R + (v2.R - v1.R) * t);
        var g = (byte)(v1.G + (v2.G - v1.G) * t);
        var b = (byte)(v1.B + (v2.B - v1.B) * t);

        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Linearly interpolates between three colors based on a factor.
    /// </summary>
    /// <param name="v1">The first color (t=0).</param>
    /// <param name="v2">The second color (t=0.5).</param>
    /// <param name="v3">The third color (t=1).</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>The interpolated color.</returns>
    public static Color Lerp(Color v1, Color v2, Color v3, float t)
    {
        if (t < 0.5f)
        {
            return Lerp(v1, v2, t * 2f);
        }
        else
        {
            return Lerp(v2, v3, (t - 0.5f) * 2f);
        }
    }


    /// <summary>
    /// Tries to parse an HTML color code into a Color structure.
    /// </summary>
    /// <param name="htmlColor">The HTML color code string (e.g., "#FF5733", "#FFF", "red").</param>
    /// <param name="color">The parsed Color structure if the parsing is successful.</param>
    /// <returns>True if parsing succeeds; otherwise, false.</returns>
    public static bool TryParseHtmlColor(string htmlColor, out Color color)
    {
        // Initialize output color to default
        color = Color.Empty;

        // Check if the input is null or empty
        if (string.IsNullOrWhiteSpace(htmlColor))
        {
            return false;
        }

        // Normalize the color string
        htmlColor = htmlColor.Trim();

        try
        {
            // Use ColorTranslator to handle HTML color parsing
            color = ColorTranslators.FromHtml(htmlColor);
            return true;
        }
        catch
        {
            // Swallow exceptions and return false
            return false;
        }
    }

    /// <summary>
    /// Parses an HTML color code into a nullable Color.
    /// </summary>
    /// <param name="htmlColor">The HTML color code string.</param>
    /// <returns>The parsed color, or null if parsing fails.</returns>
    public static Color? ParseHtmlColor(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
        {
            return null;
        }

        if (TryParseHtmlColor(htmlColor, out var c))
        {
            return c;
        }

        return null;
    }
}