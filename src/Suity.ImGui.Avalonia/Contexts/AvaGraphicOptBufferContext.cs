using Avalonia.Media;
using SkiaSharp;
using Suity.Controls;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Contexts;

/// <summary>
/// Optimized buffered graphics context for Avalonia controls implementing various graphic interfaces.
/// </summary>
internal class AvaGraphicOptBufferContext : AvaGraphicBaseContext
{
    private object _offScreenLock = new();
    private SKSurface? _bufferSurface;
    private int _bufferWidth;
    private int _bufferHeight;
    private bool _hasCachedGraphics;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicOptBufferContext"/> class.
    /// </summary>
    /// <param name="control">The parent Avalonia control.</param>
    public AvaGraphicOptBufferContext(AvaSKGraphicControl control) : base(control)
    {
    }

    /// <inheritdoc/>
    public override void RequestOutput(IEnumerable<RectangleF> clipRects)
    {
        if (SupportDirtyRect)
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

        if (RepaintAllFlag || !SupportDirtyRect || DirtyRects is null)
        {
            RepaintAllFlag = AvaOutput.RepaintAll = true;
        }
        else
        {
            RepaintAllFlag = AvaOutput.RepaintAll = false;
        }

        if (!RepaintAllFlag && DirtyRects is null)
        {
            switch (EmptyFrameOperation)
            {
                case EmptyFrameOperations.RepaintAll:
                    AvaOutput.RepaintAll = RepaintAllFlag = true;
                    break;

                case EmptyFrameOperations.Bypass:
                default:
                    DrawBuffer(canvas);
                    ClearInput();
                    AvaOutput.RepaintAll = RepaintAllFlag = false;
                    DirtyRects = null;
                    return;
            }
        }

        bool partialDraw = SupportDirtyRect && !RepaintAllFlag && DirtyRects != null;

        lock (_offScreenLock)
        {
            if (partialDraw)
            {
                if (_bufferSurface == null || _bufferSurface.Handle == nint.Zero || _bufferWidth != width || _bufferHeight != height)
                {
                    _bufferSurface?.Dispose();
                    var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                    _bufferSurface = SKSurface.Create(info);
                    _bufferWidth = width;
                    _bufferHeight = height;
                    AvaOutput.RepaintAll = RepaintAllFlag = true;
                }

                var bufferCanvas = _bufferSurface.Canvas;

                if (_hasCachedGraphics)
                {
                    AvaOutput.UpdateCanvas(context, bufferCanvas);
                    bufferCanvas.SetClipRects(DirtyRects);
                    graphicObject.HandleGraphicOutput(AvaOutput);
                    bufferCanvas.Restore();
                }
                else
                {
                    AvaOutput.RepaintAll = RepaintAllFlag = true;
                    AvaOutput.UpdateCanvas(context, bufferCanvas);
                    graphicObject.HandleGraphicOutput(AvaOutput);
                    _hasCachedGraphics = true;
                }

                DrawBuffer(canvas);
                ClearInput();
                RepaintAllFlag = false;
                DirtyRects = null;
            }
            else
            {
                AvaOutput.UpdateCanvas(context, canvas);
                graphicObject.HandleGraphicOutput(AvaOutput);
                _hasCachedGraphics = false;
                ClearInput();
                RepaintAllFlag = false;
                DirtyRects = null;
            }
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
        }
    }
}
