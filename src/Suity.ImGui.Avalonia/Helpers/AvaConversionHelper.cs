using Avalonia.Media.Imaging;
using SkiaSharp;
using Suity.Editor.Services;
using System.Drawing.Imaging;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for converting between Avalonia, System.Drawing, and SkiaSharp types.
/// </summary>
public static class AvaConversionHelper
{

    /// <summary>
    /// Converts a System.Drawing.Image to an Avalonia Bitmap with caching.
    /// </summary>
    /// <param name="drawingImage">The source image.</param>
    /// <returns>The cached Avalonia bitmap.</returns>
    public static Bitmap? ToAvaloniaBitmapCached(this System.Drawing.Image drawingImage)
    {
        if (drawingImage is not System.Drawing.Bitmap bmp || bmp.Data is null)
        {
            return null;
        }

        var resolver = bmp.Resolver as AvaBitmapResolver;
        if (resolver?.AvaBmp is { } avaBmp)
        {
            return avaBmp;
        }

        using var ms = new MemoryStream(bmp.Data);
        ms.Seek(0, SeekOrigin.Begin);

        avaBmp = new Bitmap(ms);

        if (resolver is null)
        {
            resolver = new AvaBitmapResolver(avaBmp);
            bmp.Resolver = resolver;
        }

        resolver.AvaBmp = avaBmp;

        return avaBmp;
    }

    /// <summary>
    /// Convert Avalonia color to System.Drawing color
    /// Returns System.Drawing.Color.Empty if ARGB are all zero
    /// </summary>
    public static System.Drawing.Color ToSystemDrawingColor(this Avalonia.Media.Color color)
    {
        // Check if all components are zero
        if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
        {
            return System.Drawing.Color.Empty;
        }

        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    /// Convert System.Drawing color to Avalonia color
    /// </summary>
    public static Avalonia.Media.Color ToAvaloniaColor(this System.Drawing.Color color)
    {
        // If Empty, return fully transparent black (0,0,0,0)
        if (color.IsEmpty)
        {
            return Avalonia.Media.Color.FromArgb(0, 0, 0, 0);
        }

        return Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }


    public static Avalonia.Rect ToAvaloniaRect(this System.Drawing.RectangleF rectangle)
    {
        return new Avalonia.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    /// <summary>
    /// Converts an Avalonia Rect to a System.Drawing RectangleF.
    /// </summary>
    public static System.Drawing.RectangleF ToSystemDrawingRect(this Avalonia.Rect rect)
    {
        return new System.Drawing.RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
    }

    /// <summary>
    /// Converts an Avalonia Rect to a SkiaSharp SKRect.
    /// </summary>
    public static SKRect ToSKRect(this Avalonia.Rect rect)
    {
        return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
    }

}