using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using SkiaSharp;
using Suity.Helpers;
using System.Diagnostics;
using System.Numerics;

namespace Suity.Controls;

/// <summary>
/// Delegate for SkiaSharp drawing callbacks.
/// </summary>
/// <param name="context">The Avalonia immediate drawing context.</param>
/// <param name="canvas">The SkiaSharp canvas.</param>
/// <param name="bounds">The bounds available for drawing.</param>
public delegate void SKDraw(ImmediateDrawingContext? context, SKCanvas canvas, Rect bounds);

/// <summary>
/// Avalonia control that uses the composition layer for efficient SkiaSharp rendering.
/// </summary>
public class AvaSKCompositionControl : UserControl
{
    private AvaSKCompositionHandler? customHandler;
    private CompositionCustomVisual? _customVisual;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaSKCompositionControl"/> class.
    /// </summary>
    public AvaSKCompositionControl()
    {
        Background = Brushes.Transparent;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // 1. Get Compositor
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor == null) return;

        // 2. Create CustomVisual and associate Handler
        customHandler = new AvaSKCompositionHandler();
        _customVisual = compositor.CreateCustomVisual(customHandler);

        // 3. Set CustomVisual as child visual object of current control
        ElementComposition.SetElementChildVisual(this, _customVisual);

        // 4. Sync initial size and send draw callback
        UpdateVisualSize();
        _customVisual.SendHandlerMessage(new SKDraw(OnSKDraw));
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateVisualSize();
    }

    private void UpdateVisualSize()
    {
        if (_customVisual != null)
        {
            // Convert Avalonia Size to Vector2 and pass to Composition layer
            _customVisual.Size = new Vector2((float)Bounds.Width, (float)Bounds.Height);
        }
    }

    /// <summary>
    /// Invalidates the entire control, triggering a full repaint.
    /// </summary>
    public void Invalidate()
    {
        _customVisual?.SendHandlerMessage("REPAINT_SIGNAL");
    }

    /// <summary>
    /// Invalidates a specific rectangular region of the control.
    /// </summary>
    /// <param name="rect">The region to invalidate in Avalonia coordinates.</param>
    public void Invalidate(Rect rect)
    {
        _customVisual?.SendHandlerMessage(rect);
    }

    /// <summary>
    /// Invalidates a specific rectangular region of the control.
    /// </summary>
    /// <param name="rect">The region to invalidate in System.Drawing coordinates.</param>
    public void Invalidate(System.Drawing.RectangleF rect)
    {
        _customVisual?.SendHandlerMessage(rect.ToAvaloniaRect());
    }

    /// <summary>
    /// Drawing logic. Note: This method will eventually be called on the [rendering thread].
    /// Do not access any UI thread variables or control properties here.
    /// </summary>
    /// <param name="context">The Avalonia immediate drawing context.</param>
    /// <param name="canvas">The SkiaSharp canvas.</param>
    /// <param name="bounds">The bounds available for drawing.</param>
    protected virtual void OnSKDraw(ImmediateDrawingContext? context, SKCanvas canvas, Rect bounds)
    {
        //// Example drawing logic
        //using var paint = new SKPaint
        //{
        //    Color = SKColors.Orange,
        //    IsAntialias = true,
        //    Style = SKPaintStyle.Fill
        //};

        //canvas.DrawRect(0, 0, (float)bounds.Width, (float)bounds.Height, paint);

        //paint.Color = SKColors.White;
        //canvas.DrawCircle((float)bounds.Width / 2, (float)bounds.Height / 2, 40, paint);
    }
}

/// <summary>
/// Handler running on the rendering thread for composition-based SkiaSharp rendering.
/// </summary>
public class AvaSKCompositionHandler : CompositionCustomVisualHandler
{
    private SKDraw? _drawCallback;
    private Rect _bounds;
    private Rect? _dirtyRect;

    /// <inheritdoc/>
    public override void OnMessage(object message)
    {
        if (message is "REPAINT_SIGNAL")
        {
            _dirtyRect = null;
            this.Invalidate();
        }
        else if (message is Rect rect)
        {
            _dirtyRect = rect;
            this.Invalidate(rect);
        }
        else if (message is SKDraw newCallback)
        {
            _drawCallback = newCallback;
            // Request redraw after receiving new message
            //RegisterForNextAnimationFrameUpdate();
        }
    }

    /// <inheritdoc/>
    public override void OnRender(ImmediateDrawingContext context)
    {
        if (_drawCallback == null) return;

        // Directly construct current instantaneous bounds
        _bounds = _dirtyRect ?? new Rect(0, 0, EffectiveSize.X, EffectiveSize.Y);

        // Get SkiaSharp access
        var skiaFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (skiaFeature == null) return;
        
        using var lease = skiaFeature.Lease();
        var canvas = lease.SkCanvas;

        if (canvas != null && canvas.Handle != nint.Zero)
        {
            // Execute drawing
            _drawCallback(context, canvas, _bounds);
        }
    }

    /// <inheritdoc/>
    public override void OnAnimationFrameUpdate()
    {
        //// EffectiveSize here is the size of CompositionCustomVisual
        //_bounds = new Rect(0, 0, EffectiveSize.X, EffectiveSize.Y);
        //Invalidate(); // Mark that OnRender needs to be called again

        //.WriteLine("OKOK");
        //RegisterForNextAnimationFrameUpdate();
    }
}