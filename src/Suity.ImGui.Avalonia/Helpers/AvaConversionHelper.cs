using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Drawing.Imaging;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for converting between Avalonia, System.Drawing, and SkiaSharp types.
/// </summary>
public static class AvaConversionHelper
{
    // Cache dictionary: Key is the original Image object, Value is the converted Avalonia Bitmap
    // Note: If your Image is loaded from a file, it's recommended to use the file path as Key.
    // If it's an in-memory object, using WeakReference here would be safer to prevent memory leaks.
    private static readonly Dictionary<System.Drawing.Image, Bitmap> _bitmapCache = [];

    /// <summary>
    /// Converts a System.Drawing.Image to an Avalonia Bitmap with caching.
    /// </summary>
    /// <param name="drawingImage">The source image.</param>
    /// <returns>The cached Avalonia bitmap.</returns>
    public static Bitmap ToAvaloniaBitmapCached(this System.Drawing.Image drawingImage)
    {
        if (drawingImage == null) return null;

        // 1. Try to get from cache
        if (_bitmapCache.TryGetValue(drawingImage, out var cachedBitmap))
        {
            return cachedBitmap;
        }

        // 2. Execute conversion logic
        using var ms = new MemoryStream();
        // Save System.Drawing.Image to memory stream (PNG recommended to preserve transparency)
        drawingImage.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);

        var bitmap = new Bitmap(ms);

        // 3. Store in cache
        _bitmapCache[drawingImage] = bitmap;

        return bitmap;
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

    /// <summary>
    /// Clear cache to prevent memory overflow
    /// </summary>
    public static void ClearCache()
    {
        foreach (var bitmap in _bitmapCache.Values)
        {
            bitmap.Dispose();
        }
        _bitmapCache.Clear();
    }
}