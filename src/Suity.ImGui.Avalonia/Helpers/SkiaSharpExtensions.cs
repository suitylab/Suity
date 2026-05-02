using SkiaSharp;
using Suity;
using Suity.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Suity.Helpers;

/// <summary>
/// Cache key for typeface lookups.
/// </summary>
public readonly record struct TypefaceCacheKey(string FamilyName, SKFontStyleWeight Weight, SKFontStyleSlant Style);
/// <summary>
/// Cache key for font lookups.
/// </summary>
public readonly record struct FontCacheKey(SKTypeface Typeface, float Size);

/// <summary>
/// Provides extension methods for converting System.Drawing types to SkiaSharp types and caching fonts.
/// </summary>
public static class SkiaSharpExtensions
{
    /// <summary>
    /// Gets or sets the default font name used for rendering.
    /// </summary>
    public static string DefaultFontName = "msyh.ttc";
    /// <summary>
    /// Gets or sets the font scale factor applied to all fonts.
    /// </summary>
    public static float FontScale = 1f;

    private static readonly Dictionary<Font, SKFont> _fontCache = [];
    private static readonly Dictionary<Image, SKImage> _imageCache = [];

    private static SKTypeface _defaultTypeFace;
    private static SKFont _defaultFont;

    [ThreadStatic]
    private static SKPaint _defaultPaint;

    private static readonly Dictionary<TypefaceCacheKey, SKTypeface> _typeFaceCache = [];
    /// <summary>
    /// Gets a cached typeface for the specified key, creating it if not found.
    /// </summary>
    /// <param name="key">The typeface cache key.</param>
    /// <returns>The cached or newly created typeface.</returns>
    public static SKTypeface GetCachedTypeFace(TypefaceCacheKey key)
    {
        if (!_typeFaceCache.TryGetValue(key, out var tf))
        {
            tf = SKTypeface.FromFamilyName(key.FamilyName, key.Weight, SKFontStyleWidth.Normal, key.Style);
            _typeFaceCache[key] = tf;
        }
        return tf;
    }

    private static readonly Dictionary<FontCacheKey, SKPaint> _fontPaintCache = [];
    /// <summary>
    /// Gets a cached font paint for the specified typeface and size, creating it if not found.
    /// </summary>
    /// <param name="typeface">The typeface.</param>
    /// <param name="size">The font size.</param>
    /// <returns>The cached or newly created paint.</returns>
    public static SKPaint GetCachedFontPaint(SKTypeface typeface, float size)
    {
        var key = new FontCacheKey(typeface, size);
        if (!_fontPaintCache.TryGetValue(key, out var paint))
        {
            var font = new SKFont(typeface, size);
            paint = new SKPaint(font);
            _fontPaintCache[key] = paint;
        }
        return paint;
    }


    private static void Initialize()
    {
        _defaultTypeFace = SKFontManager.Default.MatchCharacter('中');

        _defaultFont = new SKFont(_defaultTypeFace);

        _defaultPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 255),
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
        };
    }

    /// <summary>
    /// Gets the default font for rendering.
    /// </summary>
    public static SKFont DefaultFont
    {
        get
        {
            if (_defaultFont == null)
            {
                Initialize();
            }

            return _defaultFont;
        }
    }

    /// <summary>
    /// Converts an array of <see cref="PointF"/> to an array of <see cref="SKPoint"/>.
    /// </summary>
    /// <param name="points">The source points.</param>
    /// <returns>An array of SkiaSharp points.</returns>
    public static SKPoint[] ToSKPoints(this PointF[] points)
    {
        SKPoint[] skPoints = new SKPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            skPoints[i] = points[i].ToSKPoint();
        }

        return skPoints;
    }

    [ThreadStatic]
    private static SKPaint? _penPaint;
    /// <summary>
    /// Converts a <see cref="Pen"/> to a <see cref="SKPaint"/> for stroking operations.
    /// </summary>
    /// <param name="pen">The System.Drawing pen.</param>
    /// <returns>A SkiaSharp paint configured for stroking.</returns>
    public static SKPaint ToSKPaint(this Pen pen)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_penPaint == null || _penPaint.Handle == nint.Zero)
        {
            _penPaint = new SKPaint
            {
                IsAntialias = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        _penPaint.Color = pen.Color.ToSKColor();
        _penPaint.Style = SKPaintStyle.Stroke;
        _penPaint.StrokeWidth = pen.Width;

        ConfigDashStyle(_penPaint, pen.DashStyle, null/*pen.DashPattern*/, pen.Width);

        return _penPaint;
    }

    /// <summary>
    /// Configures the dash style on a SkiaSharp paint object.
    /// </summary>
    /// <param name="skPaint">The SkiaSharp paint.</param>
    /// <param name="dashStyle">The dash style.</param>
    /// <param name="dashPattern">The custom dash pattern (used when dashStyle is Custom).</param>
    /// <param name="width">The line width used to scale dash patterns.</param>
    public static void ConfigDashStyle(this SKPaint skPaint, DashStyle dashStyle, float[] dashPattern, float width)
    {
        // If Pen has dash style, convert to SkiaSharp PathEffect
        if (dashStyle != DashStyle.Solid)
        {
            // Get dash pattern
            dashPattern = GetDashPatternFromDashStyle(dashStyle, dashPattern);

            // Apply dash pattern to PathEffect
            if (dashPattern != null)
            {
                for (int i = 0; i < dashPattern.Length; i++)
                {
                    dashPattern[i] *= width;
                }
                skPaint.PathEffect = SKPathEffect.CreateDash(dashPattern, 0);
            }
            else
            {
                skPaint.PathEffect = null;
            }
        }
        else
        {
            skPaint.PathEffect = null;
        }
    }


    private static float[]? GetDashPatternFromDashStyle(DashStyle dashStyle, float[] dashPattern = null)
    {
        return dashStyle switch
        {
            DashStyle.Dash => [3, 1],           // Long dash
            DashStyle.Dot => [1, 1 /*2*/],            // Dot dash
            DashStyle.DashDot => [4, 2, 1, 2],  // Dash-dot
            DashStyle.DashDotDot => [4, 2, 1, 2, 1, 2], // Dash-dot-dot
            DashStyle.Custom => [.. dashPattern], // Custom dash pattern
            _ => null
        };
    }

    [ThreadStatic]
    private static SKPaint? _brushPaint;
    /// <summary>
    /// Converts a <see cref="SolidBrush"/> to a <see cref="SKPaint"/> for filling operations.
    /// </summary>
    /// <param name="brush">The System.Drawing solid brush.</param>
    /// <returns>A SkiaSharp paint configured for filling.</returns>
    public static SKPaint ToSKPaint(this SolidBrush brush)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_brushPaint == null || _brushPaint.Handle == nint.Zero)
        {
            _brushPaint = new SKPaint
            {
                IsAntialias = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        _brushPaint.Color = brush.Color.ToSKColor();
        _brushPaint.Style = SKPaintStyle.Fill;

        return _brushPaint;
    }

    /// <summary>
    /// Converts a <see cref="Brush"/> to a <see cref="SKPaint"/>, handling solid brushes specially.
    /// </summary>
    /// <param name="brush">The System.Drawing brush.</param>
    /// <returns>A SkiaSharp paint configured for the brush type.</returns>
    public static SKPaint ToSKPaint(this Brush brush)
    {
        if (brush is SolidBrush solidBrush)
        {
            return solidBrush.ToSKPaint();
        }
        else
        {
            if (_defaultPaint == null)
            {
                Initialize();
            }

            return _defaultPaint;
        }
    }

    /// <summary>
    /// Creates a SkiaSharp path representing a cubic Bezier curve.
    /// </summary>
    /// <param name="pt1">Start point.</param>
    /// <param name="pt2">First control point.</param>
    /// <param name="pt3">Second control point.</param>
    /// <param name="pt4">End point.</param>
    /// <returns>A SkiaSharp path containing the Bezier curve.</returns>
    public static SKPath GetBezierPath(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
    {
        var path = new SKPath();
        path.MoveTo(pt1.X, pt1.Y);
        path.CubicTo(pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);

        return path;
    }

    /// <summary>
    /// Creates a SkiaSharp path representing a closed polygon from the specified points.
    /// </summary>
    /// <param name="points">The polygon vertices.</param>
    /// <returns>A closed SkiaSharp path.</returns>
    public static SKPath ToPolygonPath(this PointF[] points)
    {
        var path = new SKPath();
        path.MoveTo(points[0].ToSKPoint());
        for (int i = 1; i < points.Length; i++)
        {
            path.LineTo(points[i].ToSKPoint());
        }
        path.Close();

        return path;
    }

    /// <summary>
    /// Converts a <see cref="Font"/> to a <see cref="SKFont"/>, using caching for performance.
    /// </summary>
    /// <param name="font">The System.Drawing font.</param>
    /// <returns>A cached SkiaSharp font.</returns>
    public static SKFont ToSKFont(this Font font)
    {
        if (font == null)
        {
            return DefaultFont;
        }

        return _fontCache.GetOrAdd(font, _ =>
        {
            SKFontStyle style = null;
            if (font.Bold)
            {
                if (font.Italic)
                {
                    style = SKFontStyle.BoldItalic;
                }
                else
                {
                    style = SKFontStyle.Bold;
                }
            }
            else
            {
                if (font.Italic)
                {
                    style = SKFontStyle.Italic;
                }
                else
                {
                    style = SKFontStyle.Normal;
                }
            }

            if (_defaultTypeFace == null)
            {
                Initialize();
            }

            //SKTypeface typeface = _defaultTypeFace;
            SKTypeface typeface = SKTypeface.FromFamilyName(_defaultTypeFace.FamilyName, style);

            return new SKFont(typeface, font.Size * FontScale);
        });
    }

    /// <summary>
    /// Converts an <see cref="Image"/> to a <see cref="SKImage"/>, using caching for performance.
    /// </summary>
    /// <param name="bitmap">The System.Drawing image.</param>
    /// <returns>A cached SkiaSharp image.</returns>
    public static SKImage ToSKImageCached(this Image bitmap)
    {
        return _imageCache.GetOrAdd(bitmap, _ => bitmap.ToSKImage());
    }

    /// <summary>
    /// Draws a line on the canvas using the specified pen and points.
    /// </summary>
    public static void DrawLine(this SKCanvas canvas, Pen pen, PointF pt1, PointF pt2)
    {
        canvas.DrawLine(pt1.X, pt1.Y, pt2.X, pt2.Y, pen.ToSKPaint());
    }

    /// <summary>
    /// Fills a rectangle on the canvas using the specified brush.
    /// </summary>
    public static void FillRectangle(this SKCanvas canvas, Brush brush, RectangleF rect)
    {
        canvas.DrawRect(rect.ToSKRect(), brush.ToSKPaint());
    }

    /// <summary>
    /// Draws a rectangle outline on the canvas using the specified pen.
    /// </summary>
    public static void DrawRectangle(this SKCanvas canvas, Pen pen, RectangleF rect)
    {
        canvas.DrawRect(rect.ToSKRect(), pen.ToSKPaint());
    }

    /// <summary>
    /// Fills a rounded rectangle on the canvas using the specified brush.
    /// </summary>
    public static void FillRoundRectangle(this SKCanvas canvas, Brush brush, RectangleF rect, float cornerRadius)
    {
        canvas.DrawRoundRect(rect.ToSKRect(), cornerRadius, cornerRadius, brush.ToSKPaint());
    }

    /// <summary>
    /// Draws a rounded rectangle outline on the canvas using the specified pen.
    /// </summary>
    public static void DrawRoundRectangle(this SKCanvas canvas, Pen pen, RectangleF rect, float cornerRadius)
    {
        canvas.DrawRoundRect(rect.ToSKRect(), cornerRadius, cornerRadius, pen.ToSKPaint());
    }

    /// <summary>
    /// Draws an ellipse outline on the canvas using the specified pen.
    /// </summary>
    public static void DrawEllipse(this SKCanvas canvas, Pen pen, RectangleF rect)
    {
        canvas.DrawOval(rect.ToSKRect(), pen.ToSKPaint());
    }

    /// <summary>
    /// Fills an ellipse on the canvas using the specified brush.
    /// </summary>
    public static void FillEllipse(this SKCanvas canvas, Brush brush, RectangleF rect)
    {
        canvas.DrawOval(rect.ToSKRect(), brush.ToSKPaint());
    }

    /// <summary>
    /// Draws a Bezier curve on the canvas using the specified pen.
    /// </summary>
    public static void DrawBezier(this SKCanvas canvas, Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
    {
        SKPath path = GetBezierPath(pt1, pt2, pt3, pt4);
        canvas.DrawPath(path, pen.ToSKPaint());
    }

    /// <summary>
    /// Draws a string on the canvas at the specified point.
    /// </summary>
    public static void DrawString(this SKCanvas canvas, string s, Font font, Brush brush, PointF point)
    {
        if (string.IsNullOrEmpty(s))
        {
            return;
        }

        var skFont = font.ToSKFont();
        var paint = brush.ToSKPaint();
        paint.TextEncoding = SKTextEncoding.Utf8;

        canvas.DrawText(s, point.X, point.Y, skFont, paint);
    }

    /// <summary>
    /// Draws a string on the canvas at the specified coordinates.
    /// </summary>
    public static void DrawString(this SKCanvas canvas, string s, Font font, Brush brush, float x, float y)
    {
        if (string.IsNullOrEmpty(s))
        {
            return;
        }

        var skFont = font.ToSKFont();
        var paint = brush.ToSKPaint();
        paint.TextEncoding = SKTextEncoding.Utf8;

        canvas.DrawText(s, x, y, skFont, paint);
    }

    /// <summary>
    /// Draws a string on the canvas with the specified alignment.
    /// </summary>
    public static void DrawString(this SKCanvas canvas, string s, Font font, Brush brush, PointF point, StringFormat format)
    {
        if (string.IsNullOrEmpty(s))
        {
            return;
        }

        var skFont = font.ToSKFont();
        var paint = brush.ToSKPaint();
        paint.TextEncoding = SKTextEncoding.Utf8;

        switch (format.Alignment)
        {
            case StringAlignment.Near:
                paint.TextAlign = SKTextAlign.Left;
                break;

            case StringAlignment.Center:
                paint.TextAlign = SKTextAlign.Center;
                break;

            case StringAlignment.Far:
                paint.TextAlign = SKTextAlign.Right;
                break;

            default:
                break;
        }
        canvas.DrawText(s, point.X, point.Y, skFont, paint);
    }

    /// <summary>
    /// Draws a polygon outline on the canvas using the specified pen.
    /// </summary>
    public static void DrawPolygon(this SKCanvas canvas, Pen pen, PointF[] points)
    {
        canvas.DrawPath(points.ToPolygonPath(), pen.ToSKPaint());
    }

    /// <summary>
    /// Fills a polygon on the canvas using the specified brush.
    /// </summary>
    public static void FillPolygon(this SKCanvas canvas, Brush brush, PointF[] points)
    {
        //canvas.DrawPoints(SKPointMode.Polygon, points.ToSKPoints(), brush.ToSKPaint());
        canvas.DrawPath(points.ToPolygonPath(), brush.ToSKPaint());
    }

    [ThreadStatic]
    private static SKPaint? _imagePaint;
    /// <summary>
    /// Draws an image on the canvas with optional color tint, using cached image conversion.
    /// </summary>
    public static void DrawImageCached(this SKCanvas canvas, Image bitmap, RectangleF rect, Color? color)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_imagePaint == null || _imagePaint.Handle == nint.Zero)
        {
            _imagePaint = new SKPaint
            {
                IsAntialias = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        SKImage skImage = bitmap.ToSKImageCached();

        if (color.HasValue)
        {
            var c = color.Value.ToSKColor();
            using var cf = SKColorFilter.CreateBlendMode(c, SKBlendMode.SrcATop);

            _imagePaint.ColorFilter = cf;
            _imagePaint.IsAntialias = true;
            _imagePaint.Color = c;
            canvas.DrawImage(skImage, rect.ToSKRect(), _imagePaint);
        }
        else
        {
            canvas.DrawImage(skImage, rect.ToSKRect());
        }
    }

    /// <summary>
    /// Draws an image on the canvas with optional color tint.
    /// </summary>
    public static void DrawImage(this SKCanvas canvas, Image bitmap, RectangleF rect, Color? color)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_imagePaint == null || _imagePaint.Handle == nint.Zero)
        {
            _imagePaint = new SKPaint
            {
                IsAntialias = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        SKImage skImage = bitmap.ToSKImage();

        if (color.HasValue)
        {
            var c = color.Value.ToSKColor();
            using var cf = SKColorFilter.CreateBlendMode(c, SKBlendMode.SrcATop);

            _imagePaint.ColorFilter = cf;
            _imagePaint.IsAntialias = true;
            _imagePaint.Color = c;
            canvas.DrawImage(skImage, rect.ToSKRect(), _imagePaint);
        }
        else
        {
            canvas.DrawImage(skImage, rect.ToSKRect());
        }
    }

    /// <summary>
    /// Sets a clip rectangle on the canvas and saves the current state.
    /// </summary>
    /// <returns>The save count for restoring.</returns>
    public static int SetClipRect(this SKCanvas canvas, RectangleF rect)
    {
        int value = canvas.Save();
        canvas.ClipRect(rect.ToSKRect());

        return value;
    }

    /// <summary>
    /// Sets multiple clip rectangles on the canvas using a region and saves the current state.
    /// </summary>
    /// <returns>The save count for restoring.</returns>
    public static int SetClipRects(this SKCanvas canvas, IEnumerable<RectangleF> rects)
    {
        int value = canvas.Save();

        var region = new SKRegion();
        region.SetRects(rects.Select(o => o.ToSKRectI()).ToArray());

        canvas.ClipRegion(region);

        return value;
    }

    /// <summary>
    /// Restores the canvas clip to the specified save count.
    /// </summary>
    public static void RestoreClip(this SKCanvas canvas, int count)
    {
        canvas.RestoreToCount(count);
    }

    /// <summary>
    /// Restores the canvas clip to the previous state.
    /// </summary>
    public static void RestoreClip(this SKCanvas canvas)
    {
        canvas.Restore();
    }

    /// <summary>
    /// Measures the size of the specified text when rendered with the given font.
    /// </summary>
    /// <param name="canvas">The canvas (used for measurement context).</param>
    /// <param name="text">The text to measure.</param>
    /// <param name="font">The font.</param>
    /// <returns>The measured size of the text.</returns>
    public static SizeF MeasureString(this SKCanvas canvas, string text, Font font)
    {
        if (string.IsNullOrEmpty(text)) return System.Drawing.SizeF.Empty;

        // 1. Get cached Typeface
        var tfKey = new TypefaceCacheKey(
            font.FontFamily.Name,
            font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright
        );
        var typeface = GetCachedTypeFace(tfKey);

        // 2. Get cached SKFont
        var paint = GetCachedFontPaint(typeface, font.Size);

        // 3. Measure (only need to pass Paint to get anti-aliasing flags, if only measuring width, can even pass null)
        // Note: skFont.MeasureText returns width, height can be obtained by iteration,
        // but a simpler way is to use our existing TextLayout logic, as it can handle complex letter spacing.

        // For some reason, adding this value makes the display effect correct
        float len = paint.MeasureText(text) + font.Size;
        return new SizeF(len, font.Size);
    }
}