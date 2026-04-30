using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Caches scaled fonts based on a base font and scale factor.
/// </summary>
internal class FontSizeCache
{
    /// <summary>
    /// Gets the base font used for scaling.
    /// </summary>
    public Font BaseFont { get; }

    private readonly Dictionary<float, Font> _sizeCache = [];

    /// <summary>
    /// Initializes a new font size cache with the specified base font.
    /// </summary>
    /// <param name="baseFont">The base font to scale from.</param>
    public FontSizeCache(Font baseFont)
    {
        BaseFont = baseFont ?? throw new ArgumentNullException(nameof(baseFont));
    }

    /// <summary>
    /// Gets a font scaled by the given factor, creating and caching it if necessary.
    /// </summary>
    /// <param name="scale">The scale factor. Returns the base font if null or 1.</param>
    /// <returns>A scaled font instance.</returns>
    public Font GetFont(float? scale)
    {
        var baseFont = BaseFont;

        if (scale is { } s && s != 1f)
        {
            float size = s * baseFont.Size;
            return _sizeCache.GetOrAdd(size, _ => new Font(BaseFont.Name, size, BaseFont.Style));
        }

        return baseFont;
    }
}

/// <summary>
/// Represents a node in a transform hierarchy for position and rectangle transformations.
/// </summary>
internal class TransformNode
{
    /// <summary>
    /// The node that owns this transform.
    /// </summary>
    public readonly ImGuiNode Owner;

    /// <summary>
    /// The parent transform in the hierarchy.
    /// </summary>
    public TransformNode? ParentTransform;

    /// <summary>
    /// The transform applied at this node.
    /// </summary>
    public GuiTransform Transform;

    /// <summary>
    /// Initializes a new transform node with just an owner.
    /// </summary>
    /// <param name="owner">The owning ImGui node.</param>
    public TransformNode(ImGuiNode owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// Initializes a new transform node with an owner and transform.
    /// </summary>
    /// <param name="owner">The owning ImGui node.</param>
    /// <param name="transform">The transform to apply.</param>
    public TransformNode(ImGuiNode owner, GuiTransform transform)
    {
        Owner = owner;
        Transform = transform;
    }

    /// <summary>
    /// Initializes a new transform node with an owner, parent transform, and transform.
    /// </summary>
    /// <param name="owner">The owning ImGui node.</param>
    /// <param name="parent">The parent transform in the hierarchy.</param>
    /// <param name="transform">The transform to apply.</param>
    public TransformNode(ImGuiNode owner, TransformNode parent, GuiTransform transform)
    {
        Owner = owner;
        ParentTransform = parent ?? throw new ArgumentNullException(nameof(parent));
        Transform = transform;
    }

    /// <summary>
    /// Computes the combined transform from this node up through the hierarchy.
    /// </summary>
    /// <returns>The accumulated global transform.</returns>
    public GuiTransform GetGlobalTransform()
    {
        if (ParentTransform is { } parent)
        {
            var parentTransform = parent.GetGlobalTransform();

            return Transform.AddTransform(parentTransform);
        }
        else
        {
            return Transform;
        }
    }

    /// <summary>
    /// Transforms a point through the entire transform hierarchy.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public PointF TransformPointInHierarchy(PointF point)
    {
        point = TransformPoint(point);

        if (ParentTransform is { } pt)
        {
            point = pt.TransformPointInHierarchy(point);
        }

        return point;
    }

    /// <summary>
    /// Transforms a rectangle through the entire transform hierarchy.
    /// </summary>
    /// <param name="rect">The rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public RectangleF TransformRectInHierarchy(RectangleF rect)
    {
        rect = TransformRect(rect);

        if (ParentTransform is { } pt)
        {
            rect = pt.TransformRectInHierarchy(rect);
        }

        return rect;
    }

    /// <summary>
    /// Reverts a point through the transform hierarchy in reverse order.
    /// </summary>
    /// <param name="point">The point to revert.</param>
    /// <returns>The reverted point.</returns>
    public PointF RevertTransformPointInHierarchy(PointF point)
    {
        if (ParentTransform is { } pt)
        {
            point = pt.RevertTransformPointInHierarchy(point);
        }

        point = RevertTransformPoint(point);

        return point;
    }

    /// <summary>
    /// Reverts a rectangle through the transform hierarchy in reverse order.
    /// </summary>
    /// <param name="rect">The rectangle to revert.</param>
    /// <returns>The reverted rectangle.</returns>
    public RectangleF RevertTransformRectInHierarchy(RectangleF rect)
    {
        if (ParentTransform is { } pt)
        {
            rect = pt.RevertTransformRectInHierarchy(rect);
        }

        rect = RevertTransformRect(rect);
        return rect;
    }


    /// <summary>
    /// Transforms a point using this node's transform, relative to the owner's position.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public PointF TransformPoint(PointF point)
    {
        var ownerRect = Owner.Rect;

        point.X -= ownerRect.X;
        point.Y -= ownerRect.Y;

        point = Transform.Transform(point);
        point.X += ownerRect.X;
        point.Y += ownerRect.Y;

        return point;
    }

    /// <summary>
    /// Transforms a rectangle using this node's transform, relative to the owner's position.
    /// </summary>
    /// <param name="rect">The rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public RectangleF TransformRect(RectangleF rect)
    {
        var ownerRect = Owner.Rect;

        rect.X -= ownerRect.X;
        rect.Y -= ownerRect.Y;

        rect = Transform.Transform(rect);
        rect.X += ownerRect.X;
        rect.Y += ownerRect.Y;

        return rect;
    }

    /// <summary>
    /// Reverts a point using this node's transform, relative to the owner's position.
    /// </summary>
    /// <param name="point">The point to revert.</param>
    /// <returns>The reverted point.</returns>
    public PointF RevertTransformPoint(PointF point)
    {
        var ownerRect = Owner.Rect;

        point.X -= ownerRect.X;
        point.Y -= ownerRect.Y;

        point = Transform.RevertTransform(point);
        point.X += ownerRect.X;
        point.Y += ownerRect.Y;

        return point;
    }

    /// <summary>
    /// Reverts a rectangle using this node's transform, relative to the owner's position.
    /// </summary>
    /// <param name="rect">The rectangle to revert.</param>
    /// <returns>The reverted rectangle.</returns>
    public RectangleF RevertTransformRect(RectangleF rect)
    {
        var ownerRect = Owner.Rect;

        rect.X -= ownerRect.X;
        rect.Y -= ownerRect.Y;

        rect = Transform.RevertTransform(rect);
        rect.X += ownerRect.X;
        rect.Y += ownerRect.Y;

        return rect;
    }
}
