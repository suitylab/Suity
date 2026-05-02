using Avalonia.Media;
using SkiaSharp;
using Suity.Controls;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Contexts;

/// <summary>
/// Double-buffered graphics context for Avalonia controls implementing various graphic interfaces.
/// </summary>
internal class AvaGraphicDoubleBufferContext : AvaGraphicBaseContext
{
    private object _offScreenLock = new();
    private SKSurface? _bufferSurface;
    private int _bufferWidth;
    private int _bufferHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicDoubleBufferContext"/> class.
    /// </summary>
    /// <param name="control">The parent Avalonia control.</param>
    public AvaGraphicDoubleBufferContext(AvaSKGraphicControl control) : base(control)
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
        lock (_offScreenLock)
        {
            AvaOutput.InternalRelease();

            _bufferSurface?.Dispose();
            _bufferSurface = null;
            _bufferWidth = 0;
            _bufferHeight = 0;
        }
    }

    /// <inheritdoc/>
    public override void Paint(ImmediateDrawingContext? context, SKCanvas canvas, Avalonia.Rect rect)
    {
        if (GraphicObject is not { } graphicObject)
        {
            return;
        }

        if (canvas.Handle == nint.Zero)
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
            _bufferSurface?.Dispose();
            var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            _bufferSurface = SKSurface.Create(info);
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
                    DrawBuffer(canvas);
                    return;
            }
        }

        lock (_offScreenLock)
        {
            if (_bufferSurface is null || _bufferSurface.Handle == nint.Zero)
            {
                ClearInput();
                RepaintAllFlag = true;
                DirtyRects = null;
                return;
            }

            AvaOutput.UpdateCanvas(context, bufferCanvas);

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

            DrawBuffer(canvas);
        }
    }

    private void DrawBuffer(SKCanvas canvas)
    {
        lock (_offScreenLock)
        {
            if (_bufferSurface is null || _bufferSurface.Handle == nint.Zero)
            {
                return;
            }

            using var snapshot = _bufferSurface.Snapshot();
            canvas.DrawImage(snapshot, 0, 0);

            ClearInput();
            RepaintAllFlag = false;
            DirtyRects = null;
        }
    }
}
