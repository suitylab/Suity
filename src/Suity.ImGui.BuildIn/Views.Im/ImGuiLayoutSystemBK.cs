using Suity.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Default layout system for ImGui, providing horizontal, vertical, fill, overlay, and viewport layout functions.
/// </summary>
public class ImGuiLayoutSystemBK : ImGuiLayoutSystem
{
    private static ImGuiLayoutSystemBK? _instance;

    /// <summary>
    /// Gets the singleton instance of the layout system.
    /// </summary>
    public static ImGuiLayoutSystemBK Instance => _instance ??= new ImGuiLayoutSystemBK();

    private readonly Dictionary<string, LayoutFunction> _functions = new();

    /// <summary>
    /// Initializes a new layout system and registers all built-in layout functions.
    /// </summary>
    public ImGuiLayoutSystemBK()
    {
        _functions[Fill] = FillLayout;
        _functions[Horizontal] = HorizontalLayout;
        _functions[Vertical] = VerticalLayout;
        _functions[HorizontalReverse] = HorizontalReverseLayout;
        _functions[VerticalReverse] = VerticalReverseLayout;
        _functions[Overlay] = OverlayLayout;
        _functions[Viewport] = ViewportLayout;

        _functions[nameof(GuiCommonExtensions.Viewport)] = ViewportLayout;
    }

    /// <inheritdoc/>
    public override LayoutFunction? GetLayoutFunction(string name)
    {
        var func = _functions.GetValueSafe(name);

        if (func is null)
        {
            Debug.WriteLine($"{nameof(ImGuiLayoutSystemBK)} function not found : {name}");
        }

        return func;
    }

    private void FillLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var rect = node.Parent!.InnerRect;

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            node.Rect = rect;
        }
    }

    private void HorizontalLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            if (pos.Position.X > 0)
            {
                pos.Position.X += node.SiblingSpacing;
            }

            var theme = parentNode.Theme;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth;
            float dHeight = theme.DefaultRowHeight; // pInnerRect.Height;

            var size = node.CalculateSize(pos.Position.X, 0, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            float x = pInnerRect.X + pos.Position.X;
            float y = pInnerRect.Y + parentNode.InitialLayoutPosition.Y;
            var rect = new RectangleF(x, y, size.Width, size.Height);

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            //float align = childNode.GetVerticalAlignment(pInnerRect, rect);
            //if (align > 0)
            //{
            //    rect.Y += align;
            //}

            //if (childNode.AlignmentStretch)
            //{
            //    if (rect.Y > pInnerRect.Y) rect.Y = pInnerRect.Y;
            //    if (rect.Height < pInnerRect.Height) rect.Height = pInnerRect.Height;
            //}

            node.Rect = rect;
            var outerRect = node.LocalOuterRect;

            pos.Position.X += outerRect.Width;
            if (outerRect.Height > pos.Position.Y)
            {
                pos.Position.Y = outerRect.Height;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Align))
        {
            HandleVerticalAlign(node);
        }
    }

    private void VerticalLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            if (pos.Position.Y > 0)
            {
                pos.Position.Y += node.SiblingSpacing;
            }

            var theme = parentNode.Theme;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth; // pInnerRect.Width;
            float dHeight = theme.DefaultRowHeight;

            var size = node.CalculateSize(0, pos.Position.Y, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            float x = pInnerRect.X + parentNode.InitialLayoutPosition.X;
            float y = pInnerRect.Y + pos.Position.Y;
            var rect = new RectangleF(x, y, size.Width, size.Height);

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            //float align = childNode.GetHorizontalAlignment(pInnerRect, rect);
            //if (align > 0)
            //{
            //    rect.X += align;
            //}

            //if (childNode.AlignmentStretch)
            //{
            //    if (rect.X > pInnerRect.X) rect.X = pInnerRect.X;
            //    if (rect.Width < pInnerRect.Width) rect.Width = pInnerRect.Width;
            //}

            node.Rect = rect;
            var outerRect = node.LocalOuterRect;

            pos.Position.Y += outerRect.Height;
            if (outerRect.Width > pos.Position.X)
            {
                pos.Position.X = outerRect.Width;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Align))
        {
            HandleHorizontalAlign(node);
        }
    }

    private void HorizontalReverseLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            if (pos.Position.X > 0)
            {
                pos.Position.X += node.SiblingSpacing;
            }

            var theme = parentNode.Theme;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth;
            float dHeight = theme.DefaultRowHeight; // innerRect.Height;

            var size = node.CalculateSize(pos.Position.X, 0, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            float x = pInnerRect.Right - pos.Position.X - size.Width;
            float y = pInnerRect.Y + parentNode.InitialLayoutPosition.Y;
            var rect = new RectangleF(x, y, size.Width, size.Height);

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            //float align = childNode.GetVerticalAlignment(innerRect, rect);
            //if (align > 0)
            //{
            //    rect.Y += align;
            //}

            //if (childNode.AlignmentStretch)
            //{
            //    if (rect.Y > innerRect.Y) rect.Y = innerRect.Y;
            //    if (rect.Height < innerRect.Height) rect.Height = innerRect.Height;
            //}

            node.Rect = rect;
            var outerRect = node.LocalOuterRect;

            pos.Position.X += outerRect.Width;
            if (outerRect.Height > pos.Position.Y)
            {
                pos.Position.Y = outerRect.Height;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Align))
        {
            HandleVerticalAlign(node);
        }
    }

    private void VerticalReverseLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            if (pos.Position.Y > 0)
            {
                pos.Position.Y += node.SiblingSpacing;
            }

            var theme = parentNode.Theme;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth; // innerRect.Width;
            float dHeight = theme.DefaultRowHeight;

            var size = node.CalculateSize(0, pos.Position.Y, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            float x = pInnerRect.X + parentNode.InitialLayoutPosition.X;
            float y = pInnerRect.Bottom - pos.Position.Y - size.Height;
            var rect = new RectangleF(x, y, size.Width, size.Height);

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            //float align = childNode.GetHorizontalAlignment(innerRect, rect);
            //if (align > 0)
            //{
            //    rect.X += align;
            //}

            //if (childNode.AlignmentStretch)
            //{
            //    if (rect.X > innerRect.X) rect.X = innerRect.X;
            //    if (rect.Width < innerRect.Width) rect.Width = innerRect.Width;
            //}

            node.Rect = rect;
            var outerRect = node.LocalOuterRect;

            pos.Position.Y += outerRect.Height;
            if (outerRect.Width > pos.Position.X)
            {
                pos.Position.X = outerRect.Width;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Align))
        {
            HandleHorizontalAlign(node);
        }
    }

    private void OverlayLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            var theme = parentNode.Theme;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth; // innerRect.Width;
            float dHeight = theme.DefaultRowHeight; // innerRect.Height;

            var size = node.CalculateSize(pos.Position.X, pos.Position.Y, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            float x = pInnerRect.X + parentNode.InitialLayoutPosition.X;
            float y = pInnerRect.Y + parentNode.InitialLayoutPosition.Y;
            var rect = new RectangleF(x, y, size.Width, size.Height);

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            //float alignX = childNode.GetHorizontalAlignment(innerRect, rect);
            //if (alignX > 0)
            //{
            //    rect.X += alignX;
            //}
            //float alignY = childNode.GetVerticalAlignment(innerRect, rect);
            //if (alignY > 0)
            //{
            //    rect.Y += alignY;
            //}

            //if (childNode.AlignmentStretch)
            //{
            //    if (rect.X > innerRect.X) rect.X = innerRect.X;
            //    if (rect.Width < innerRect.Width) rect.Width = innerRect.Width;

            //    if (rect.Y > innerRect.Y) rect.Y = innerRect.Y;
            //    if (rect.Height < innerRect.Height) rect.Height = innerRect.Height;
            //}

            node.Rect = rect;
            var outerRect = node.LocalOuterRect;

            if (outerRect.Right > pos.Position.X)
            {
                pos.Position.X = outerRect.Right;
            }
            if (outerRect.Bottom > pos.Position.Y)
            {
                pos.Position.Y = outerRect.Bottom;
            }
        }

        if (pipeline.HasFlag(GuiPipeline.Align))
        {
            var parentNode = node.Parent!;

            var alignment = node.Alignment;

            if (alignment is { VerticalAlignment: { } } or { HorizontalAlignment: { } })
            {
                var innerRect = parentNode.InnerRect;
                var rect = node.Rect;
                float alignX = node.GetHorizontalAlignment(innerRect, rect);
                float alignY = node.GetVerticalAlignment(innerRect, rect);

                if (alignX > 0 || alignY > 0)
                {
                    float vX = innerRect.X + parentNode.InitialLayoutPosition.X;
                    float vX2 = rect.X - vX;

                    float vY = innerRect.Y + parentNode.InitialLayoutPosition.Y;
                    float vY2 = rect.Y - vY;

                    if (vY2 != alignY || vX2 != alignX)
                    {
                        node.OffsetPositionDeep(alignX - vX2, alignY - vY2);
                    }
                }
            }
        }
    }

    private void ViewportLayout(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition pos, ChildLayoutFunction baseAction)
    {
        if (pipeline.HasFlag(GuiPipeline.Main))
        {
            var parentNode = node.Parent!;

            var theme = parentNode.Theme;

            var nodePos = node.GetValue<GuiPositionValue>();
            float x = parentNode.InitialLayoutPosition.X + nodePos?.Position.X ?? 0;
            float y = parentNode.InitialLayoutPosition.Y + nodePos?.Position.Y ?? 0;

            RectangleF pInnerRect = parentNode.InnerRect;
            float dWidth = theme.DefaultColumnWidth;
            float dHeight = theme.DefaultRowHeight;


            var size = node.CalculateSize(x, y, pInnerRect.Width, pInnerRect.Height, dWidth, dHeight);
            var rect = new RectangleF(x, y, size.Width, size.Height);

            rect.X += pInnerRect.X;
            rect.Y += pInnerRect.Y;

            if (node.Margin is { } margin)
            {
                rect = margin.Shrink(rect);
            }

            node.Rect = rect;

            if (parentNode.GetValue<GuiViewportValue>() is { } viewport)
            {
                viewport.ApplyChildNode(node);
            }
        }
    }

    private void HandleVerticalAlign(ImGuiNode node)
    {
        var parentNode = node.Parent!;

        if (node.Alignment is { VerticalAlignment: { } } alignment)
        {
            var innerRect = parentNode.InnerRect;
            var rect = node.Rect;
            float alignY = node.GetVerticalAlignment(innerRect, rect);

            if (alignY > 0)
            {
                float vY = innerRect.Y + parentNode.InitialLayoutPosition.Y;
                float vY2 = rect.Y - vY;

                if (vY2 != alignY)
                {
                    // Double layout causes child node double displacement, because the main node executes Layout once more which can reset rect, but child nodes don't.
                    node.OffsetPositionDeep(0, alignY - vY2);
                }
            }

            if (alignment.Stretch)
            {
                rect = node.Rect;
                rect.Y = innerRect.Y;
                rect.Height = innerRect.Height;
                if (node.Rect != rect)
                {
                    node.Rect = rect;
                    parentNode.MarkRenderDirty();
                }
            }
        }
    }

    private void HandleHorizontalAlign(ImGuiNode node)
    {
        var parentNode = node.Parent!;

        if (node.Alignment is { HorizontalAlignment: { } } alignment)
        {
            var innerRect = parentNode.InnerRect;
            var rect = node.Rect;
            float alignX = node.GetHorizontalAlignment(innerRect, rect);

            if (alignX > 0)
            {
                float vX = innerRect.X + parentNode.InitialLayoutPosition.X;
                float vX2 = rect.X - vX;

                if (vX2 != alignX)
                {
                    // Double layout causes child node double displacement, because the main node executes Layout once more which can reset rect, but child nodes don't.
                    node.OffsetPositionDeep(alignX - vX2, 0);
                }
            }

            if (alignment.Stretch)
            {
                rect = node.Rect;
                rect.X = innerRect.X;
                rect.Width = innerRect.Width;
                if (node.Rect != rect)
                {
                    node.Rect = rect;
                    parentNode.MarkRenderDirty();
                }
            }
        }
    }
}