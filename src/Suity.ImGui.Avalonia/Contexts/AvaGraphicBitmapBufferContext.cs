using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Suity.Controls;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Contexts;

/// <summary>
/// Double-buffered graphics context for Avalonia controls implementing various graphic interfaces.
/// </summary>
internal class AvaGraphicBitmapBufferContext : AvaGraphicBaseContext
{
    private WriteableBitmap? _bitmap;
    private SKSurface? _bufferSurface;
    private int _bufferWidth;
    private int _bufferHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicDoubleBufferContext"/> class.
    /// </summary>
    /// <param name="control">The parent Avalonia control.</param>
    public AvaGraphicBitmapBufferContext(AvaSKGraphicControl control) : base(control)
    {
    }

    /// <inheritdoc/>
    public override void RequestOutput(IEnumerable<RectangleF> clipRects)
    {
        if (!RepaintAllFlag && SupportDirtyRect)
        {
            DirtyRects = clipRects.ToArray();
            Control.Invalidate();
        }
        else
        {
            DirtyRects = null;
            RepaintAllFlag = true;
            Control.Invalidate();
        }
    }

    /// <inheritdoc/>
    public override void HandleReleased()
    {
        AvaOutput.InternalRelease();

        _bitmap?.Dispose();
        _bitmap = null;
        _bufferSurface?.Dispose();
        _bufferSurface = null;
        _bufferWidth = 0;
        _bufferHeight = 0;
    }

    public void Paint(DrawingContext context, Rect rect)
    {
        if (GraphicObject is not { } graphicObject)
        {
            return;
        }

        int width = (int)rect.Width;
        int height = (int)rect.Height;

        if (width <= 0 || height <= 0)
        {
            width = (int)Control.Width;
            height = (int)Control.Height;
        }

        if (_bufferSurface == null || _bufferSurface.Handle == nint.Zero || _bufferWidth != width || _bufferHeight != height)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _bufferSurface?.Dispose();

            double dpi = 96; // 
            _bitmap = new WriteableBitmap(
            new PixelSize(Math.Max(1, width), Math.Max(1, height)),
            new Vector(dpi, dpi),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);

            using (var fb = _bitmap.Lock())
            {
                var info = new SKImageInfo(fb.Size.Width, fb.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                _bufferSurface = SKSurface.Create(info, fb.Address, fb.RowBytes);
            }

            _bufferWidth = width;
            _bufferHeight = height;
            RepaintAllFlag = true;
        }

        var bufferCanvas = _bufferSurface.Canvas;

        if (RepaintAllFlag || !SupportDirtyRect || DirtyRects is null)
        {
            AvaOutput.RepaintAll = true;
        }
        else
        {
            AvaOutput.RepaintAll = false;
        }

        if (!RepaintAllFlag && DirtyRects is null)
        {
            switch (EmptyFrameOperation)
            {
                case EmptyFrameOperations.RepaintAll:
                    RepaintAllFlag = true;
                    AvaOutput.RepaintAll = true;
                    break;

                case EmptyFrameOperations.Bypass:
                default:
                    if (_bitmap != null)
                    {
                        using (_bitmap.Lock())
                        {
                            DrawBuffer(context, rect, _bitmap);
                        }
                    }
                    
                    return;
            }
        }

        if (_bufferSurface is null || _bufferSurface.Handle == nint.Zero || _bitmap is null)
        {
            ClearInput();
            RepaintAllFlag = true;
            DirtyRects = null;
            return;
        }

        using (var fb = _bitmap.Lock())
        {
            AvaOutput.UpdateCanvas(null, bufferCanvas);

            if (SupportDirtyRect && !RepaintAllFlag && DirtyRects != null)
            {
                bufferCanvas.SetClipRects(DirtyRects);
                graphicObject.HandleGraphicOutput(AvaOutput);
                bufferCanvas.Restore();
            }
            else
            {
                graphicObject.HandleGraphicOutput(AvaOutput);
            }

            DrawBuffer(context, rect, _bitmap);
        }
    }

    public override void Paint(ImmediateDrawingContext? context, SKCanvas canvas, Rect rect)
    {
        throw new NotImplementedException();
    }

    private void DrawBuffer(DrawingContext context, Rect rect, WriteableBitmap bitmap)
    {
        // Get the current bounds of the control, or specify a specific Rect
        var destRect = new Rect(0, 0, rect.Width, rect.Height);
        var sourceRect = new Rect(0, 0, bitmap.Size.Width, bitmap.Size.Height);

        // Draw the WriteableBitmap to the current context
        context.DrawImage(bitmap, sourceRect, destRect);

        ClearInput();
        RepaintAllFlag = false;
        DirtyRects = null;
    }
}
