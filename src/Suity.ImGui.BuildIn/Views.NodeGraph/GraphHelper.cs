using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Provides extension methods for graph-related operations.
/// </summary>
public static class GraphHelper
{
    /// <summary>
    /// Determines which resize side of a rectangle is closest to the specified point.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <param name="point">The point to test against.</param>
    /// <returns>The resize side that is closest to the point.</returns>
    public static GraphResizeSide GetResizeSide(this Rectangle rect, Point point)
    {
        if (rect.Contains(point))
        {
            if (point.X < rect.Left + 5)
            {
                return GraphResizeSide.Left;
            }

            if (point.Y < rect.Top + 5)
            {
                return GraphResizeSide.Top;
            }

            if (point.X > rect.Right - 10 && point.Y > rect.Bottom - 10)
            {
                return GraphResizeSide.Corner;
            }

            if (point.X > rect.Right - 5)
            {
                return GraphResizeSide.Right;
            }

            if (point.Y > rect.Bottom - 5)
            {
                return GraphResizeSide.Bottom;
            }

            return GraphResizeSide.Inside;
        }

        return GraphResizeSide.Outside;
    }

    /// <summary>
    /// Determines whether the resize side indicates a resize operation (not inside or outside).
    /// </summary>
    /// <param name="side">The resize side to check.</param>
    /// <returns>True if the side requires a resize operation; otherwise, false.</returns>
    public static bool NeedResize(this GraphResizeSide side)
    {
        return side != GraphResizeSide.Inside && side != GraphResizeSide.Outside;
    }

    /// <summary>
    /// Gets the appropriate cursor type for a resize side.
    /// </summary>
    /// <param name="side">The resize side.</param>
    /// <returns>The cursor type for the resize side.</returns>
    public static GuiCursorTypes GetCursor(this GraphResizeSide side) => side switch
    {
        GraphResizeSide.Corner => GuiCursorTypes.SizeNWSE,
        GraphResizeSide.Left or GraphResizeSide.Right => GuiCursorTypes.SizeWE,
        GraphResizeSide.Top or GraphResizeSide.Bottom => GuiCursorTypes.SizeNS,
        _ => GuiCursorTypes.Default,
    };

    /// <summary>
    /// Determines whether the connector type is a normal connector (data or action).
    /// </summary>
    /// <param name="type">The connector type.</param>
    /// <returns>True if the type is a normal connector; otherwise, false.</returns>
    public static bool GetIsNormalConnector(this ConnectorType type) => type switch
    {
        ConnectorType.Data or 
        ConnectorType.Action => true,
        _ => false,
    };

    /// <summary>
    /// Determines whether the hit type indicates a node or node move area.
    /// </summary>
    /// <param name="hitType">The hit type.</param>
    /// <returns>True if the hit type is Node or NodeMoveArea; otherwise, false.</returns>
    public static bool GetIsNodeOrMoveArea(this HitType hitType)
    {
        return hitType == HitType.Node || hitType == HitType.NodeMoveArea;
    }

    /// <summary>
    /// Calculates the bounding box that encompasses all specified rectangles.
    /// </summary>
    /// <param name="rects">The rectangles to calculate the bounding box for.</param>
    /// <returns>The bounding box rectangle.</returns>
    public static Rectangle GetBoundingBox(IEnumerable<Rectangle> rects)
    {
        if (rects == null) throw new ArgumentNullException(nameof(rects));

        Rectangle boundingBox = Rectangle.Empty;
        foreach (var rect in rects)
        {
            if (boundingBox.IsEmpty)
            {
                boundingBox = rect;
            }
            else
            {
                boundingBox = Rectangle.Union(boundingBox, rect);
            }
        }
        return boundingBox;
    }
}