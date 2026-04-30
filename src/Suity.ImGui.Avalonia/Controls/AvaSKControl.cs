using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Suity.Controls;

/// <summary>
/// Base Avalonia control that provides SkiaSharp rendering through a custom draw operation.
/// </summary>
public class AvaSKControl : Control
{
    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        //var typeFace = new Typeface("Segoe UI");
        //var formattedText = new FormattedText("Hello OKOK", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeFace, 16, null);
        //context.DrawText(formattedText, new Point(10, 30));
        
        Rect clip = new Rect(base.Bounds.Size);
        using (base.ClipToBounds ? context.PushClip(clip) : default)
        {
            var bound = new Rect(0.0, 0.0, clip.Width, clip.Height);
            context.Custom(new SKDrawOP(bound, OnSKDraw));
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == Visual.ClipToBoundsProperty)
        {
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Called when SkiaSharp drawing is required. Override this method to implement custom rendering.
    /// </summary>
    /// <param name="context">The Avalonia immediate drawing context.</param>
    /// <param name="canvas">The SkiaSharp canvas.</param>
    /// <param name="bounds">The bounds available for drawing.</param>
    protected virtual void OnSKDraw(ImmediateDrawingContext context, SKCanvas canvas, Rect bounds)
    {
    }
}

/// <summary>
/// Custom draw operation that bridges Avalonia rendering with SkiaSharp canvas access.
/// </summary>
public class SKDrawOP : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly Action<ImmediateDrawingContext, SKCanvas, Rect> _draw;

    /// <inheritdoc/>
    public Rect Bounds => _bounds;


    /// <summary>
    /// Initializes a new instance of the <see cref="SKDrawOP"/> class.
    /// </summary>
    /// <param name="bounds">The bounds of the draw operation.</param>
    /// <param name="draw">The callback to execute for drawing.</param>
    public SKDrawOP(Rect bounds, Action<ImmediateDrawingContext, SKCanvas, Rect> draw)
    {
        _draw = draw;
        _bounds = bounds;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public bool HitTest(Point p) => _bounds.Contains(p);

    /// <inheritdoc/>
    public bool Equals(ICustomDrawOperation? other) => false;

    /// <inheritdoc/>
    public void Render(ImmediateDrawingContext context)
    {
        if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } skiaSharpApiLeaseFeature)
        {
            return;
        }

        using (var skiaSharpApiLease = skiaSharpApiLeaseFeature.Lease())
        {
            if (skiaSharpApiLease?.SkCanvas is { } skCanvas)
            {
                if (skCanvas.Handle == nint.Zero)
                {
                    return;
                }

                //using (var paint = new SKPaint { Color = SKColors.Orange, IsAntialias = true })
                //{
                //    skCanvas.DrawRect(0, 0, (float)Bounds.Width, (float)Bounds.Height, paint);
                //}

                _draw(context, skCanvas, _bounds);

                //TestSkDraw(context, skCanvas);
            }
        }
    }


    /// <summary>
    /// Test drawing method for verifying SkiaSharp rendering.
    /// </summary>
    /// <param name="context">The Avalonia immediate drawing context.</param>
    /// <param name="canvas">The SkiaSharp canvas.</param>
    protected void TestSkDraw(ImmediateDrawingContext context, SKCanvas canvas)
    {
        // Execute your SkiaSharp drawing logic
        using (var paint = new SKPaint { Color = SKColors.Orange, IsAntialias = true })
        {
            canvas.DrawRect(0, 0, (float)Bounds.Width, (float)Bounds.Height, paint);
            canvas.DrawCircle(100, 100, 50, new SKPaint { Color = SKColors.White });
        }

        //context.DrawEllipse(new ImmutableSolidColorBrush(Colors.Green), null, new Point(50, 50), 50, 50);
    }

}