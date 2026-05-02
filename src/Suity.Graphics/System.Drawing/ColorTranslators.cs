using System.Collections.Generic;

namespace System.Drawing;

/// <summary>
/// Provides methods to translate colors to and from HTML color representations.
/// </summary>
public static class ColorTranslators
{
    /// <summary>
    /// Translates an HTML color representation to a <see cref="Color"/> structure.
    /// </summary>
    /// <param name="htmlColor">The HTML color string to translate (e.g., "#RRGGBB", "#RGB", or a named color like "red").</param>
    /// <returns>The <see cref="Color"/> structure that represents the translated color.</returns>
    /// <exception cref="ArgumentException">Thrown when the color string is invalid or unknown.</exception>
    public static Color FromHtml(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            throw new ArgumentException("Color cannot be null or empty.", nameof(htmlColor));

        htmlColor = htmlColor.Trim();

        if (htmlColor.StartsWith("#", StringComparison.Ordinal))
        {
            return FromHex(htmlColor);
        }

        if (htmlColor.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
        {
            return FromRgb(htmlColor);
        }

        return FromName(htmlColor);
    }

    /// <summary>
    /// Translates an HTML color representation to a <see cref="Color"/> structure, returning a fallback color if the input is invalid.
    /// </summary>
    /// <param name="htmlColor">The HTML color string to translate (e.g., "#RRGGBB", "#RGB", or a named color like "red").</param>
    /// <param name="fallBack">The fallback <see cref="Color"/> to return if the input is invalid.</param>
    /// <returns>The <see cref="Color"/> structure that represents the translated color, or the fallback color if the input is invalid.</returns>
    public static Color FromHtmlSafe(string htmlColor, Color fallBack = default)
    {
        try
        {
            return FromHtml(htmlColor);
        }
        catch
        {
            return fallBack;
        }
    }

    private static Color FromHex(string hex)
    {
        hex = hex.Substring(1);
        int r, g, b, a = 255;

        switch (hex.Length)
        {
            case 3:
                r = Convert.ToInt32($"{hex[0]}{hex[0]}", 16);
                g = Convert.ToInt32($"{hex[1]}{hex[1]}", 16);
                b = Convert.ToInt32($"{hex[2]}{hex[2]}", 16);
                break;
            case 4:
                a = Convert.ToInt32($"{hex[0]}{hex[0]}", 16);
                r = Convert.ToInt32($"{hex[1]}{hex[1]}", 16);
                g = Convert.ToInt32($"{hex[2]}{hex[2]}", 16);
                b = Convert.ToInt32($"{hex[3]}{hex[3]}", 16);
                break;
            case 6:
                r = Convert.ToInt32(hex.Substring(0, 2), 16);
                g = Convert.ToInt32(hex.Substring(2, 2), 16);
                b = Convert.ToInt32(hex.Substring(4, 2), 16);
                break;
            case 8:
                a = Convert.ToInt32(hex.Substring(0, 2), 16);
                r = Convert.ToInt32(hex.Substring(2, 2), 16);
                g = Convert.ToInt32(hex.Substring(4, 2), 16);
                b = Convert.ToInt32(hex.Substring(6, 2), 16);
                break;
            default:
                throw new ArgumentException("Invalid hex color format.", nameof(hex));
        }

        return Color.FromArgb(a, r, g, b);
    }

    private static Color FromRgb(string rgb)
    {
        var content = rgb.Substring(4, rgb.Length - 5);
        var parts = content.Split(',');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid RGB color format.", nameof(rgb));

        int r = int.Parse(parts[0].Trim());
        int g = int.Parse(parts[1].Trim());
        int b = int.Parse(parts[2].Trim());

        return Color.FromArgb(255, r, g, b);
    }

    /// <summary>
    /// Translates a <see cref="Color"/> structure to an HTML color representation.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> structure to translate.</param>
    /// <returns>A string representing the HTML color (e.g., "#RRGGBB" or a named color like "red").</returns>
    public static string ToHtml(Color color)
    {
        foreach (var kvp in KnownColors)
        {
            if (kvp.Value == color)
            {
                return kvp.Key;
            }
        }

        if (color.A == 255)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static Color FromName(string name)
    {
        if (KnownColors.TryGetValue(name, out var color))
            return color;

        throw new ArgumentException($"Unknown color name: {name}", nameof(name));
    }

    private static readonly Dictionary<string, Color> KnownColors = new(StringComparer.OrdinalIgnoreCase)
    {
        { "black", Color.Black }, { "white", Color.White }, { "red", Color.Red }, { "green", Color.Green },
        { "blue", Color.Blue }, { "yellow", Color.Yellow }, { "cyan", Color.Cyan }, { "magenta", Color.Magenta },
        { "gray", Color.Gray }, { "grey", Color.Gray }, { "silver", Color.Silver }, { "maroon", Color.Maroon },
        { "olive", Color.Olive }, { "lime", Color.Lime }, { "teal", Color.Teal }, { "navy", Color.Navy },
        { "fuchsia", Color.Fuchsia }, { "purple", Color.Purple }, { "orange", Color.Orange },
        { "aliceblue", Color.AliceBlue }, { "antiquewhite", Color.AntiqueWhite }, { "aqua", Color.Aqua },
        { "aquamarine", Color.Aquamarine }, { "azure", Color.Azure }, { "beige", Color.Beige },
        { "bisque", Color.Bisque }, { "blanchedalmond", Color.BlanchedAlmond }, { "blueviolet", Color.BlueViolet },
        { "brown", Color.Brown }, { "burlywood", Color.BurlyWood }, { "cadetblue", Color.CadetBlue },
        { "chartreuse", Color.Chartreuse }, { "chocolate", Color.Chocolate }, { "coral", Color.Coral },
        { "cornflowerblue", Color.CornflowerBlue }, { "cornsilk", Color.Cornsilk }, { "crimson", Color.Crimson },
        { "darkblue", Color.DarkBlue }, { "darkcyan", Color.DarkCyan }, { "darkgoldenrod", Color.DarkGoldenrod },
        { "darkgray", Color.DarkGray }, { "darkgreen", Color.DarkGreen }, { "darkgrey", Color.DarkGray },
        { "darkkhaki", Color.DarkKhaki }, { "darkmagenta", Color.DarkMagenta }, { "darkolivegreen", Color.DarkOliveGreen },
        { "darkorange", Color.DarkOrange }, { "darkorchid", Color.DarkOrchid }, { "darkred", Color.DarkRed },
        { "darksalmon", Color.DarkSalmon }, { "darkseagreen", Color.DarkSeaGreen }, { "darkslateblue", Color.DarkSlateBlue },
        { "darkslategray", Color.DarkSlateGray }, { "darkslategrey", Color.DarkSlateGray }, { "darkturquoise", Color.DarkTurquoise },
        { "darkviolet", Color.DarkViolet }, { "deeppink", Color.DeepPink }, { "deepskyblue", Color.DeepSkyBlue },
        { "dimgray", Color.DimGray }, { "dimgrey", Color.DimGray }, { "dodgerblue", Color.DodgerBlue },
        { "firebrick", Color.Firebrick }, { "floralwhite", Color.FloralWhite }, { "forestgreen", Color.ForestGreen },
        { "gainsboro", Color.Gainsboro }, { "ghostwhite", Color.GhostWhite }, { "gold", Color.Gold },
        { "goldenrod", Color.Goldenrod }, { "greenyellow", Color.GreenYellow }, { "honeydew", Color.Honeydew },
        { "hotpink", Color.HotPink }, { "indianred", Color.IndianRed }, { "indigo", Color.Indigo },
        { "ivory", Color.Ivory }, { "khaki", Color.Khaki }, { "lavender", Color.Lavender },
        { "lavenderblush", Color.LavenderBlush }, { "lawngreen", Color.LawnGreen }, { "lemonchiffon", Color.LemonChiffon },
        { "lightblue", Color.LightBlue }, { "lightcoral", Color.LightCoral }, { "lightcyan", Color.LightCyan },
        { "lightgoldenrodyellow", Color.LightGoldenrodYellow }, { "lightgray", Color.LightGray }, { "lightgreen", Color.LightGreen },
        { "lightgrey", Color.LightGray }, { "lightpink", Color.LightPink }, { "lightsalmon", Color.LightSalmon },
        { "lightseagreen", Color.LightSeaGreen }, { "lightskyblue", Color.LightSkyBlue }, { "lightslategray", Color.LightSlateGray },
        { "lightslategrey", Color.LightSlateGray }, { "lightsteelblue", Color.LightSteelBlue }, { "lightyellow", Color.LightYellow },
        { "limegreen", Color.LimeGreen }, { "linen", Color.Linen }, { "mediumaquamarine", Color.MediumAquamarine },
        { "mediumblue", Color.MediumBlue }, { "mediumorchid", Color.MediumOrchid }, { "mediumpurple", Color.MediumPurple },
        { "mediumseagreen", Color.MediumSeaGreen }, { "mediumslateblue", Color.MediumSlateBlue }, { "mediumspringgreen", Color.MediumSpringGreen },
        { "mediumturquoise", Color.MediumTurquoise }, { "mediumvioletred", Color.MediumVioletRed }, { "midnightblue", Color.MidnightBlue },
        { "mintcream", Color.MintCream }, { "mistyrose", Color.MistyRose }, { "moccasin", Color.Moccasin },
        { "navajowhite", Color.NavajoWhite }, { "oldlace", Color.OldLace }, { "olivedrab", Color.OliveDrab },
        { "orangered", Color.OrangeRed }, { "orchid", Color.Orchid }, { "palegoldenrod", Color.PaleGoldenrod },
        { "palegreen", Color.PaleGreen }, { "paleturquoise", Color.PaleTurquoise }, { "palevioletred", Color.PaleVioletRed },
        { "papayawhip", Color.PapayaWhip }, { "peachpuff", Color.PeachPuff }, { "peru", Color.Peru },
        { "pink", Color.Pink }, { "plum", Color.Plum }, { "powderblue", Color.PowderBlue },
        { "rosybrown", Color.RosyBrown }, { "royalblue", Color.RoyalBlue }, { "saddlebrown", Color.SaddleBrown },
        { "salmon", Color.Salmon }, { "sandybrown", Color.SandyBrown }, { "seagreen", Color.SeaGreen },
        { "seashell", Color.SeaShell }, { "sienna", Color.Sienna }, { "skyblue", Color.SkyBlue },
        { "slateblue", Color.SlateBlue }, { "slategray", Color.SlateGray }, { "slategrey", Color.SlateGray },
        { "snow", Color.Snow }, { "springgreen", Color.SpringGreen }, { "steelblue", Color.SteelBlue },
        { "tan", Color.Tan }, { "thistle", Color.Thistle }, { "tomato", Color.Tomato },
        { "turquoise", Color.Turquoise }, { "violet", Color.Violet }, { "wheat", Color.Wheat },
        { "whitesmoke", Color.WhiteSmoke }, { "yellowgreen", Color.YellowGreen }
    };
}
