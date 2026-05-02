using Avalonia.Media;
using SkiaSharp;
using Suity.Controls;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Contexts;

/// <summary>
/// Single-buffered graphics context for Avalonia controls implementing various graphic interfaces.
/// </summary>
internal class AvaGraphicSingleBufferContext : AvaGraphicBaseContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AvaGraphicSingleBufferContext"/> class.
    /// </summary>
    /// <param name="control">The parent Avalonia control.</param>
    public AvaGraphicSingleBufferContext(AvaSKGraphicControl control) : base(control)
    {
    }

    /// <inheritdoc/>
    public override void RequestOutput(IEnumerable<RectangleF> clipRects)
    {
        if (!RepaintAllFlag && SupportDirtyRect)
        {
            DirtyRects = clipRects.ToArray();

            var bounds = GetBoundingBox(DirtyRects);
            if (bounds != RectangleF.Empty)
            {
                Control.Invalidate(new Avalonia.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height));
            }
            else
            {
                Control.Invalidate();
            }
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

        AvaOutput.RepaintAll = RepaintAllFlag || !SupportDirtyRect || DirtyRects == null;

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
                    goto final;
            }
        }

        bool useClip = SupportDirtyRect && !RepaintAllFlag && DirtyRects != null;
        if (useClip)
        {
            canvas.Save();
            canvas.SetClipRect(rect.ToSystemDrawingRect());
        }

        try
        {
            AvaOutput.UpdateCanvas(context, canvas);
            graphicObject.HandleGraphicOutput(AvaOutput);
        }
        finally
        {
            if (useClip)
            {
                canvas.Restore();
            }
        }

    final:
        ClearInput();
        RepaintAllFlag = false;
        DirtyRects = null;
    }
}
