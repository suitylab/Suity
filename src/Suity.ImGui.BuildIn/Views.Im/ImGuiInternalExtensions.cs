using System.Drawing;
using System.Linq;

namespace Suity.Views.Im;

/// <summary>
/// Internal extension methods for ImGui node size calculations and alignment.
/// </summary>
internal static class ImGuiInternalExtensions
{
    /// <summary>
    /// Computes the desired size of a node based on its parent's dimensions and theme.
    /// </summary>
    /// <param name="node">The node to compute the size for.</param>
    /// <returns>The computed size, or <see cref="SizeF.Empty"/> if the node has no parent.</returns>
    public static SizeF ComputeSize(this ImGuiNode node)
    {
        var parentNode = node.Parent;
        if (parentNode is null)
        {
            return SizeF.Empty;
        }

        var theme = parentNode.Theme;
        var parentRect = parentNode.InnerRect;

        return node.CalculateSize(0, 0, parentRect.Width, parentRect.Height, theme.DefaultColumnWidth, theme.DefaultRowHeight);
    }

    /// <summary>
    /// Calculates the size of a node given specific parent dimensions.
    /// </summary>
    /// <param name="node">The node to calculate.</param>
    /// <param name="x">The X position within the parent.</param>
    /// <param name="y">The Y position within the parent.</param>
    /// <param name="parentWidth">The parent's inner width.</param>
    /// <param name="parentHeight">The parent's inner height.</param>
    /// <param name="desiredWidth">The default width to use if not specified.</param>
    /// <param name="desiredHeight">The default height to use if not specified.</param>
    /// <returns>The calculated size.</returns>
    internal static SizeF CalculateSize(this ImGuiNode node, float x, float y, float parentWidth, float parentHeight, float desiredWidth, float desiredHeight)
    {
        float? w = null;
        float? h = null;

        var scale = node.LocalScale;

        if (node.Width is { } width)
        {
            w = width.GetValue(parentWidth, x, scale)
            ?? node.GetAdaptedWidth(parentWidth, x);
        }

        if (node.Height is { } height)
        {
            h = height.GetValue(parentHeight, y, scale)
            ?? node.GetAdaptedHeight(parentHeight, y);
        }

        float sizeWidth = w ?? desiredWidth;
        float sizeHeight = h ?? desiredHeight;

        return new SizeF(sizeWidth, sizeHeight);
    }

    private static float GetAdaptedWidth(this ImGuiNode node, float width, float x)
    {
        float value = 0;
        var next = node.Next;
        float dx = x;

        while (next != null)
        {
            value += next.Width?.GetValue(width, dx) ?? 0;
            dx += value;
            next = next.Next;
        }

        float v = node.LocalReverseScaleValue(width - x - value);
        if (v < 0)
        {
            v = 0;
        }

        return v;
    }

    private static float GetAdaptedHeight(this ImGuiNode node, float height, float y)
    {
        float value = 0;
        var next = node.Next;
        float dy = y;

        while (next != null)
        {
            value += next.Height?.GetValue(height, dy) ?? 0;
            dy += value;
            next = next.Next;
        }

        float v = node.LocalReverseScaleValue(height - y - value);
        if (v < 0)
        {
            v = 0;
        }

        return v;
    }

    /// <summary>
    /// Calculates the horizontal alignment offset for a child node within its parent.
    /// </summary>
    /// <param name="childNode">The child node.</param>
    /// <param name="parentRect">The parent's inner rectangle.</param>
    /// <param name="rect">The child's rectangle.</param>
    /// <returns>The horizontal offset to apply for alignment.</returns>
    internal static float GetHorizontalAlignment(this ImGuiNode childNode, RectangleF parentRect, RectangleF rect)
    {
        var alignment = childNode.HorizontalAlignment;
        if (!alignment.HasValue)
        {
            return 0;
        }

        switch (alignment.Value)
        {
            case GuiAlignment.Near:
                return 0;

            case GuiAlignment.Center:
                if (parentRect.Width > rect.Width)
                {
                    return (parentRect.Width - rect.Width) * 0.5f;
                }
                else
                {
                    return 0;
                }
            case GuiAlignment.Far:
                if (parentRect.Width > rect.Width)
                {
                    return parentRect.Width - rect.Width;
                }
                else
                {
                    return 0;
                }
            default:
                return 0;
        }
    }

    /// <summary>
    /// Calculates the vertical alignment offset for a child node within its parent.
    /// </summary>
    /// <param name="childNode">The child node.</param>
    /// <param name="parentRect">The parent's inner rectangle.</param>
    /// <param name="rect">The child's rectangle.</param>
    /// <returns>The vertical offset to apply for alignment.</returns>
    internal static float GetVerticalAlignment(this ImGuiNode childNode, RectangleF parentRect, RectangleF rect)
    {
        var alignment = childNode.VerticalAlignment;
        if (!alignment.HasValue)
        {
            return 0;
        }

        switch (alignment.Value)
        {
            case GuiAlignment.Near:
                return 0;

            case GuiAlignment.Center:
                if (parentRect.Height > rect.Height)
                {
                    return (parentRect.Height - rect.Height) * 0.5f;
                }
                else
                {
                    return 0;
                }
            case GuiAlignment.Far:
                if (parentRect.Height > rect.Height)
                {
                    return parentRect.Height - rect.Height;
                }
                else
                {
                    return 0;
                }
            default:
                return 0;
        }
    }

    /// <summary>
    /// Calculates the total width of all child nodes.
    /// </summary>
    /// <param name="node">The parent node.</param>
    /// <returns>The width spanned by child nodes, or 0 if there are no children.</returns>
    internal static float CalculateContentWidth(this ImGuiNode node)
    {
        if (!node.ChildNodes.Any())
        {
            return 0;
        }

        return node.ChildNodes.Select(o => o.Rect.Right).Max() - node.ChildNodes.Select(o => o.Rect.Left).Min();
    }

    /// <summary>
    /// Calculates the total height of all child nodes.
    /// </summary>
    /// <param name="node">The parent node.</param>
    /// <returns>The height spanned by child nodes, or 0 if there are no children.</returns>
    internal static float CalculateContentHeight(this ImGuiNode node)
    {
        if (!node.ChildNodes.Any())
        {
            return 0;
        }

        return node.ChildNodes.Select(o => o.Rect.Bottom).Max() - node.ChildNodes.Select(o => o.Rect.Top).Min();
    }
    //internal static PointF ControlToView(this GuiViewportValue viewport, RectangleF viewRect, PointF point)
    //{
    //    float x = (point.X - viewRect.Width * 0.5f - viewRect.X) / viewport.Zoom - viewport.ViewportPosition.X;
    //    float y = (point.Y - viewRect.Height * 0.5f - viewRect.Y) / viewport.Zoom - viewport.ViewportPosition.Y;

    //    return new PointF(x, y);
    //}

    //internal static PointF ViewToControl(this GuiViewportValue viewport, RectangleF viewRect, PointF point)
    //{
    //    float x = (point.X + viewport.ViewportPosition.X) * viewport.Zoom + viewRect.Width * 0.5f + viewRect.X;
    //    float y = (point.Y + viewport.ViewportPosition.Y) * viewport.Zoom + viewRect.Height * 0.5f + viewRect.Y;

    //    return new PointF(x, y);
    //}

    //internal static RectangleF ControlToView(this GuiViewportValue viewport, RectangleF viewRect, RectangleF rect)
    //{
    //    var p = viewport.ControlToView(viewRect, new PointF(rect.X, rect.Y));
    //    var size = new SizeF(rect.Width / viewport.Zoom, rect.Height / viewport.Zoom);

    //    return new RectangleF(p, size);
    //}

    //internal static RectangleF ViewToControl(this GuiViewportValue viewport, RectangleF viewRect, RectangleF rect)
    //{
    //    var p = viewport.ViewToControl(viewRect, new PointF(rect.X, rect.Y));
    //    var size = new SizeF(rect.Width * viewport.Zoom, rect.Height * viewport.Zoom);

    //    return new RectangleF(p, size);
    //}
}