using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SkiaSharp;
using Suity.Contexts;
using Suity.Views.Graphics;
using System.Diagnostics;

namespace Suity.Controls;

/// <summary>
/// Avalonia control that provides a double-buffered SkiaSharp rendering surface with full input handling.
/// </summary>
public class AvaSKGraphicControl : AvaSKBitmapControl
{
    private readonly AvaGraphicBitmapBufferContext _context;

    bool _loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaSKGraphicControl"/> class.
    /// </summary>
    public AvaSKGraphicControl()
    {
        _context = new(this)
        {
            SupportDirtyRect = true,
        };

        this.Tapped += OnTapped;
        this.DoubleTapped += OnDoubleTapped;

        // Listen for Bounds property changes
        this.GetObservable(BoundsProperty).Subscribe(OnResize);

        this.Focusable = true;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AvaSKGraphicControl"/> class with a specified graphic object.
    /// </summary>
    /// <param name="graphicObject">The graphic object to render.</param>
    public AvaSKGraphicControl(IGraphicObject graphicObject)
        : this()
    {
        this.GraphicObject = graphicObject;
    }

    /// <summary>
    /// Gets or sets the graphic object to be rendered by this control.
    /// </summary>
    public IGraphicObject? GraphicObject
    {
        get => _context.GraphicObject;
        set => _context.GraphicObject = value;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Occurs first, has parent, no size, triggers every time added to tree, register events, start logic, get TopLevel
        base.OnAttachedToVisualTree(e);

        // Get top-level window and request first frame
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            topLevel.RequestAnimationFrame(OnTick);
        }
        else
        {
            Debug.WriteLine("TopLevel not found.");
        }
    }

    /// <inheritdoc/>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        // Occurs last, has parent, has size, triggers when layout is complete and ready to display, initialize size-related graphics, UI focus
        _loaded = true;
        _context.HandleCreated();
    }

    /// <inheritdoc/>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _loaded = false;
        Dispatcher.UIThread.Post(_context.HandleReleased);
    }

    /// <inheritdoc/>
    protected override void OnSKDraw(ImmediateDrawingContext? context, SKCanvas canvas, Rect bounds)
    {
        if (!_loaded)
        {
            return;
        }

        _context.Paint(context, canvas, bounds);

        //TestSkDraw(context, canvas);
    }

    public override void Render(DrawingContext context)
    {
        var rect = this.Bounds;
        _context.Paint(context, rect);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _context.HandlePointerEvent(e, GuiEventTypes.MouseDown, focus: true);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _context.HandlePointerEvent(e, GuiEventTypes.MouseUp);
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        _context.HandleTappedEvent(e, GuiEventTypes.MouseClick, focus: true);
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        _context.HandleTappedEvent(e, GuiEventTypes.MouseDoubleClick, focus: true);
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        _context.HandlePointerEvent(e, GuiEventTypes.MouseMove);
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        _context.HandlePointerEvent(e, GuiEventTypes.MouseWheel);
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        _context.HandleInput(CommonGraphicInput.MouseIn);
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        _context.ClearInput();
        _context.HandleInput(CommonGraphicInput.MouseOut);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        _context.HandleKeyEvent(e, GuiEventTypes.KeyDown);
    }

    /// <inheritdoc/>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        _context.HandleKeyEvent(e, GuiEventTypes.KeyUp);
    }

    private void OnResize(Rect bounds)
    {
        _context.HandleResizeEvent();
        // Always execute a full refresh
        _context.RequestOutput();
    }

    private void OnTick(TimeSpan span)
    {
        _context.HandleTimerEvent();

        // Note: Only continue when control is still on visual tree to avoid memory leaks
        if (this.IsAttachedToVisualTree())
        {
            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnTick);
        }
    }
    
}
