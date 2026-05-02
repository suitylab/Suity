using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Suity.Controls;
using Suity.Helpers;
using Suity.Views.Graphics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Suity.Contexts;

/// <summary>
/// Avalonia-specific implementation of the graphic output interface for rendering to SkiaSharp canvases.
/// </summary>
internal class AvaGraphicOutput : IGraphicOutput
{
    private readonly Control _control;
    private Avalonia.Media.ImmediateDrawingContext? _drawingContext;
    private SKCanvas _canvas;

    private int _width;
    private int _height;
    private int _clipDepth;

    private readonly HashSet<AvaSnapshot> _snapshots = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicOutput"/> class.
    /// </summary>
    /// <param name="control">The parent control.</param>
    public AvaGraphicOutput(Control control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));
    }


    /// <summary>
    /// Updates the internal drawing context and canvas references.
    /// </summary>
    /// <param name="drawingContext">The Avalonia immediate drawing context.</param>
    /// <param name="canvas">The SkiaSharp canvas.</param>
    public void UpdateCanvas(Avalonia.Media.ImmediateDrawingContext? drawingContext, SKCanvas canvas)
    {
        _drawingContext = drawingContext;
        _canvas = canvas;
    }

    /// <summary>
    /// Sets the output dimensions.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <inheritdoc/>
    public bool IsEmpty => false;

    /// <inheritdoc/>
    public int Width => _width;

    /// <inheritdoc/>
    public int Height => _height;

    /// <inheritdoc/>
    public void Clear(Color color)
    {
        _canvas.Clear(color.ToSKColor());
    }

    /// <inheritdoc/>
    public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
    {
        _canvas.DrawBezier(pen, pt1, pt2, pt3, pt4);
    }

    [ThreadStatic]
    private static SKPaint? _bezierPaint;
    /// <summary>
    /// Draws a Bezier curve with optional gradient coloring and dash styling.
    /// </summary>
    /// <param name="color1">Start color.</param>
    /// <param name="color2">End color (if different from color1, creates a gradient).</param>
    /// <param name="width">Line width.</param>
    /// <param name="pt1">Start point.</param>
    /// <param name="pt2">First control point.</param>
    /// <param name="pt3">Second control point.</param>
    /// <param name="pt4">End point.</param>
    /// <param name="dashStyle">The dash style.</param>
    /// <param name="dashPattern">Custom dash pattern.</param>
    public void DrawBezier(Color color1, Color color2, float width, PointF pt1, PointF pt2, PointF pt3, PointF pt4, DashStyle dashStyle = DashStyle.Solid, float[] dashPattern = null)
    {
        // Add .Handle check to prevent unmanaged objects from holding references after being accidentally released
        if (_bezierPaint == null || _bezierPaint.Handle == nint.Zero)
        {
            _bezierPaint = new SKPaint
            {
                IsAntialias = true,
                HintingLevel = SKPaintHinting.Normal
            };
        }

        if (color1 == color2)
        {
            // Create paint using shader
            _bezierPaint.Color = color1.ToSKColor();
            _bezierPaint.Shader = null;
            _bezierPaint.Style = SKPaintStyle.Stroke; // Only draw lines
            _bezierPaint.StrokeWidth = width;         // Set line width
            _bezierPaint.IsAntialias = true;           // Enable anti-aliasing

            _bezierPaint.ConfigDashStyle(dashStyle, dashPattern, width);

            SKPath path = SkiaSharpExtensions.GetBezierPath(pt1, pt2, pt3, pt4);
            _canvas.DrawPath(path, _bezierPaint);
        }
        else
        {
            SKColor[] colors = [color1.ToSKColor(), color2.ToSKColor()];
            using var shader = SKShader.CreateLinearGradient(pt1.ToSKPoint(), pt4.ToSKPoint(), colors, SKShaderTileMode.Clamp);
            // Create paint using shader

            _bezierPaint.Color = color1.ToSKColor();
            _bezierPaint.Shader = shader;
            _bezierPaint.Style = SKPaintStyle.Stroke; // Only draw lines
            _bezierPaint.StrokeWidth = width;         // Set line width
            _bezierPaint.IsAntialias = true;           // Enable anti-aliasing

            _bezierPaint.ConfigDashStyle(dashStyle, dashPattern, width);

            SKPath path = SkiaSharpExtensions.GetBezierPath(pt1, pt2, pt3, pt4);
            _canvas.DrawPath(path, _bezierPaint);
        }

    }

    /// <inheritdoc/>
    public void DrawImage(System.Drawing.Image bitmap, RectangleF rect, Color? color)
    {
        _canvas.DrawImage(bitmap, rect, color);
    }

    /// <inheritdoc/>
    public void DrawImageCached(System.Drawing.Image bitmap, RectangleF rect, Color? color)
    {
        _canvas.DrawImageCached(bitmap, rect, color);
    }

    /// <inheritdoc/>
    public void DrawLine(Pen pen, PointF pt1, PointF pt2)
    {
        _canvas.DrawLine(pen, pt1, pt2);
    }

    /// <inheritdoc/>
    public void DrawRectangle(Pen pen, RectangleF rect)
    {
        _canvas.DrawRectangle(pen, rect);
    }

    /// <inheritdoc/>
    public void DrawRoundRectangle(Pen pen, RectangleF rect, float cornerRadius)
    {
        _canvas.DrawRoundRectangle(pen, rect, cornerRadius);
    }

    /// <inheritdoc/>
    public void DrawEllipse(Pen pen, RectangleF rect)
    {
        _canvas.DrawEllipse(pen, rect);
    }

    /// <inheritdoc/>
    public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle, bool useCenter)
    {
        _canvas.DrawArc(rect.ToSKRect(), startAngle, sweepAngle, useCenter, pen.ToSKPaint());
    }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, PointF point)
    {
        //_canvas.DrawString(s, font, brush, point);
        _canvas.DrawRichSingleLineText(s, font, brush, new SKPoint(point.X, point.Y), StringAlignment.Near);
    }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, float x, float y)
    {
        //_canvas.DrawString(s, font, brush, x, y);
        _canvas.DrawRichSingleLineText(s, font, brush, new SKPoint(x, y), StringAlignment.Near);
    }

    /// <inheritdoc/>
    public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
    {
        //_canvas.DrawString(s, font, brush, point, format);
        var alignment = format.Alignment;
        _canvas.DrawRichSingleLineText(s, font, brush, new SKPoint(point.X, point.Y), alignment);
    }

    /// <inheritdoc/>
    public void DrawTextArea(string s, Font font, Color color, RectangleF rect)
    {
        _canvas.DrawRichTextArea(s, font, color, rect);
    }

    /// <inheritdoc/>
    public void FillEllipse(Brush brush, RectangleF rect)
    {
        _canvas.FillEllipse(brush, rect);
    }

    /// <inheritdoc/>
    public void DrawPolygon(Pen pen, PointF[] points)
    {
        _canvas.DrawPolygon(pen, points);
    }

    /// <inheritdoc/>
    public void FillPolygon(Brush brush, PointF[] points)
    {
        _canvas.FillPolygon(brush, points);
    }

    /// <inheritdoc/>
    public void FillRectangle(Brush brush, RectangleF rect)
    {
        _canvas.FillRectangle(brush, rect);
    }

    /// <inheritdoc/>
    public void FillRoundRectangle(Brush brush, RectangleF rect, float cornerRadius)
    {
        _canvas.FillRoundRectangle(brush, rect, cornerRadius);

        //_drawingContext.FillRectangle(brush.ToAvaloniaImmutable(), rect.ToAvaloniaRect(), cornerRadius);

    }

    /// <inheritdoc/>
    public SizeF MeasureString(string text, Font font)
    {
        var size = AvaDrawingHelper.MeasureSingleLineString(text, font);
        size.Width += font.Size * 0.2f;
        return size;
    }

    /// <inheritdoc/>
    public SizeF MeasureTextArea(string text, Font font, float maxLineWidth)
    {
        var size = AvaDrawingHelper.MeasureTextArea(text, font, maxLineWidth);
        return size;
    }

    /// <inheritdoc/>
    public int ClipDepth => _clipDepth;

    /// <inheritdoc/>
    public bool IsClipped => ClipDepth > 0;

    /// <summary>
    /// Gets or sets a value indicating whether a full repaint is required.
    /// </summary>
    public bool RepaintAll { get; internal set; }

    /// <inheritdoc/>
    public void SetClipRect(RectangleF rect)
    {
        _clipDepth++;
        _canvas.SetClipRect(rect);
    }

    /// <inheritdoc/>
    public void SetClipRects(IEnumerable<RectangleF> rects)
    {
        _clipDepth++;
        _canvas.SetClipRects(rects);
    }

    /// <inheritdoc/>
    public void RestoreClip()
    {
        _canvas.RestoreClip();
        _clipDepth--;
    }

    /// <inheritdoc/>
    public void RestoreClipTo(int count)
    {
        int c = Math.Min(count, _clipDepth);
        if (c > 0)
        {
            _canvas.RestoreClip(count);
            //count -= c;
        }
    }

    /// <inheritdoc/>
    public void RestoreAll()
    {
        if (_clipDepth > 0)
        {
            _canvas.RestoreClip(_clipDepth);
            _clipDepth = 0;
        }
    }

    /// <inheritdoc/>
    public ISnapshot? Snapshot()
    {
        if (_control is null)
        {
            return null;
        }

        // 1. Get scaling ratio (DPI Scaling)
        var topLevel = TopLevel.GetTopLevel(_control);
        double scaling = topLevel?.RenderScaling ?? 1.0;

        // 2. Get logical size of control
        // Note: Use Bounds.Size, Size may be (0,0) if control is not yet loaded
        var layoutSize = _control.Bounds.Size;

        if (layoutSize.Width <= 0 || layoutSize.Height <= 0)
        {
            // If control has no size, try using measured desired size
            layoutSize = _control.DesiredSize;
        }

        // 3. Calculate physical pixel size
        var pixelSize = new PixelSize(
            (int)Math.Max(1, layoutSize.Width * scaling),
            (int)Math.Max(1, layoutSize.Height * scaling));

        // 4. Create RenderTargetBitmap
        // Set corresponding DPI vector to ensure screenshot clarity
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96 * scaling, 96 * scaling));
            // 5. Directly render full-size content
            // Full-area rendering does not need PushTransform, as default origin is (0,0)
            bitmap.Render(_control);

        return new AvaSnapshot(bitmap, _snapshots);
    }

    /// <inheritdoc/>
    public ISnapshot? Snapshot(RectangleF region)
    {
        if (_control is null)
        {
            return null;
        }

        // 1. Get current window scaling ratio (DPI Scaling)
        var topLevel = TopLevel.GetTopLevel(_control);
        double scaling = topLevel?.RenderScaling ?? 1.0;

        // 2. Calculate physical pixel size of target bitmap
        // Physical pixels = logical size * scaling ratio
        var pixelSize = new PixelSize(
            (int)Math.Max(1, region.Width * scaling),
            (int)Math.Max(1, region.Height * scaling));

        // 3. Create RenderTargetBitmap and specify correct DPI (96 is standard unit)
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96 * scaling, 96 * scaling));
        using (var ctx = bitmap.CreateDrawingContext())
        {
            // 4. Core logic: Translate coordinate system
            // Move drawing context up and left so region.Location aligns to (0,0)
            var transform = Avalonia.Matrix.CreateTranslation(-region.X, -region.Y);

            using (ctx.PushTransform(transform))
            {
                // 5. Render control content to bitmap
                // Only content within (0,0) to (region.Width, region.Height) will be recorded
                bitmap.Render(_control);
            }
        }

        return new AvaSnapshot(bitmap, _snapshots);
    }

    /// <inheritdoc/>
    public void DrawSnapshot(ISnapshot snapshot, RectangleF rect)
    {
        if (snapshot == null) return;

        if (snapshot is AvaSnapshot skSnapshot && skSnapshot.Image is { } image)
        {
            // 1. Convert System.Drawing.RectangleF to Avalonia.Rect
            // Avalonia draw methods receive Rect type (logical units)
            var destRect = new Rect(rect.X, rect.Y, rect.Width, rect.Height);

            // 2. Define source region (full image sampling)
            // If you want to fill the target rectangle with the entire snapshot, source region is the full size of the snapshot
            var srcRect = new Rect(0, 0, image.Size.Width, image.Size.Height);

            // 3. Execute drawing
            // Parameters: (image, source rectangle, target rectangle)
            _drawingContext.DrawBitmap(image, srcRect, destRect);
        }
    }

    /// <summary>
    /// Releases internal resources and disposes all cached snapshots.
    /// </summary>
    internal void InternalRelease()
    {
        if (_snapshots.Count == 0)
        {
            return;
        }

        var snapshots = _snapshots.ToArray();
        _snapshots.Clear();

        foreach (var snapshot in snapshots)
        {
            snapshot.Dispose();
        }

        _drawingContext = null;
        _canvas = null;
    }
}

/// <summary>
/// Represents a snapshot of the control's rendered content.
/// </summary>
internal class AvaSnapshot : ISnapshot
{
    private readonly ICollection<AvaSnapshot>? _collection;

    private readonly RenderTargetBitmap _image;

    /// <summary>
    /// Gets the underlying Avalonia RenderTargetBitmap.
    /// </summary>
    public RenderTargetBitmap Image => _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaSnapshot"/> class.
    /// </summary>
    /// <param name="image">The rendered bitmap.</param>
    /// <param name="collection">Optional collection to track this snapshot.</param>
    public AvaSnapshot(RenderTargetBitmap image, ICollection<AvaSnapshot>? collection = null)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));

        _collection = collection;
        _collection?.Add(this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _image.Dispose();

        _collection?.Remove(this);
    }
}