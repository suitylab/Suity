using Suity.Helpers;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Manages the viewport transformation, including pan, zoom, and coordinate conversion between control and view space.
/// </summary>
public class GraphViewport
{
    private GraphControl _control;

    /// <summary>
    /// Gets or sets the horizontal pan offset in view space.
    /// </summary>
    public int ViewX { get; set; } = 0;
    /// <summary>
    /// Gets or sets the vertical pan offset in view space.
    /// </summary>
    public int ViewY { get; set; } = 0;
    /// <summary>
    /// Gets or sets the current zoom level.
    /// </summary>
    public float ViewZoom { get; set; } = 1.0f;
    /// <summary>
    /// Gets or sets the smoothed zoom level used for animated transitions.
    /// </summary>
    public float SmoothViewZoom { get; set; } = 1.0f;

    internal float _movingViewX;
    internal float _movingViewY;

    /// <summary>
    /// Gets or sets the global rectangle representing the visible area in control space.
    /// </summary>
    public RectangleF GlobalViewRect { get; set; }
    /// <summary>
    /// Gets or sets the width of the viewport in control space.
    /// </summary>
    public float Width { get; set; }
    /// <summary>
    /// Gets or sets the height of the viewport in control space.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// The rectangle representing the viewport in view space.
    /// </summary>
    public Rectangle ViewSpaceRectangle;


    /// <summary>
    /// Initializes a new instance of the <see cref="GraphViewport"/> class.
    /// </summary>
    /// <param name="control">The parent graph control.</param>
    public GraphViewport(GraphControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));
    }

    /// <summary>
    /// Gets the parent graph control.
    /// </summary>
    public GraphControl ParentControl => _control;

    /// <summary>
    /// Converts a point from control space to view space.
    /// </summary>
    /// <param name="point">The point in control space.</param>
    /// <returns>The point in view space.</returns>
    public Point ControlToView(Point point) => ControlToView(new PointF(point.X, point.Y)).ToInt();

    /// <summary>
    /// Converts a point from view space to control space.
    /// </summary>
    /// <param name="point">The point in view space.</param>
    /// <returns>The point in control space.</returns>
    public Point ViewToControl(Point point) => ViewToControl(new PointF(point.X, point.Y)).ToInt();

    /// <summary>
    /// Converts a point from control space to view space.
    /// </summary>
    /// <param name="point">The point in control space.</param>
    /// <returns>The point in view space.</returns>
    public virtual PointF ControlToView(PointF point)
    {
        var zoom = SmoothViewZoom;
        float x = (point.X - (Width / 2)) / zoom - ViewX;
        float y = (point.Y - (Height / 2)) / zoom - ViewY;
        return new PointF(x, y);
    }

    /// <summary>
    /// Converts a point from view space to control space.
    /// </summary>
    /// <param name="point">The point in view space.</param>
    /// <returns>The point in control space.</returns>
    public virtual PointF ViewToControl(PointF point)
    {
        var zoom = SmoothViewZoom;
        float x = (point.X + ViewX) * zoom + (Width / 2);
        float y = (point.Y + ViewY) * zoom + (Height / 2);
        return new PointF(x, y);
    }

    /// <summary>
    /// Converts a rectangle from control space to view space.
    /// </summary>
    /// <param name="rect">The rectangle in control space.</param>
    /// <returns>The rectangle in view space.</returns>
    public virtual RectangleF ControlToView(RectangleF rect)
    {
        var zoom = ScaledViewZoom;
        var point = ControlToView(new PointF(rect.X, rect.Y));
        var size = new SizeF(rect.Width / zoom, rect.Height / zoom);
        return new RectangleF(point, size);
    }

    /// <summary>
    /// Converts a rectangle from view space to control space.
    /// </summary>
    /// <param name="rect">The rectangle in view space.</param>
    /// <returns>The rectangle in control space.</returns>
    public virtual RectangleF ViewToControl(RectangleF rect)
    {
        var zoom = ScaledViewZoom;
        var point = ViewToControl(new PointF(rect.X, rect.Y));
        var size = new SizeF(rect.Width * zoom, rect.Height * zoom);
        return new RectangleF(point, size);
    }

    /// <summary>
    /// Gets the scaled view zoom factor.
    /// </summary>
    public virtual float ScaledViewZoom => ViewZoom;


    #region Hit

    /// <summary>
    /// Performs a hit test on selected nodes at the specified cursor location.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <param name="node">When this method returns, contains the hit node if found; otherwise, null.</param>
    /// <returns>The type of hit result.</returns>
    public HitType HitSelected(Point cursorLocation, out GraphNode? node)
    {
        var HitTest = ControlToView(cursorLocation);

        foreach (var n in _control.Diagram.SelectedItems)
        {
            if (n.HitRectangle.Contains(HitTest))
            {
                var hitConnector = n.GetConnectorMouseHit(cursorLocation);
                if (hitConnector is { } && hitConnector.ConnectorType != ConnectorType.Associate)
                {
                    node = null;
                    return HitType.Connector;
                }
                else if (n.GetHeaderArea() is { } headerArea)
                {
                    node = n;
                    return headerArea.Contains(cursorLocation) ? HitType.NodeMoveArea : HitType.Node;
                }
                else
                {
                    node = n;
                    return HitType.NodeMoveArea;
                }
            }
        }

        node = null;
        return HitType.Nothing;
    }

    /// <summary>
    /// Determines whether the specified location is within the movable area of a node.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <param name="node">The node to test.</param>
    /// <returns>True if the location is within the node's movable area; otherwise, false.</returns>
    public bool HitMovableArea(Point cursorLocation, GraphNode node)
    {
        var HitTest = ControlToView(cursorLocation);
        if (node.HitRectangle.Contains(HitTest))
        {
            return node.GetHeaderArea() is { } headerArea ? headerArea.Contains(cursorLocation) : true;
        }
        return false;
    }

    /// <summary>
    /// Performs a hit test on all nodes at the specified cursor location.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <returns>The type of hit result.</returns>
    public HitType HitAll(Point cursorLocation)
    {
        var HitTest = new Rectangle(ControlToView(cursorLocation), new Size());
        var collection = _control.Diagram.NodeCollection;

        for (int i = collection.Count - 1; i >= 0; i--)
        {
            var n = collection[i];
            if (HitTest.IntersectsWith(n.HitRectangle))
            {
                var hitConnector = n.GetConnectorMouseHit(cursorLocation);
                return hitConnector is { } && hitConnector.ConnectorType != ConnectorType.Associate ? HitType.Connector : HitType.Node;
            }
        }

        return HitType.Nothing;
    }

    /// <summary>
    /// Determines which resize side is being hovered at the specified cursor location.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <returns>The resize side that is being hovered.</returns>
    public GraphResizeSide HitSide(Point cursorLocation)
    {
        if (_control.Diagram.SelectedItems.Count != 1)
        {
            return GraphResizeSide.Outside;
        }

        var item = _control.Diagram.SelectedItems[0];
        if (!item.Resizable)
        {
            return GraphResizeSide.Outside;
        }

        var mousePos = ControlToView(cursorLocation);
        return item.HitRectangle.GetResizeSide(mousePos);
    }

    /// <summary>
    /// Gets the connector that is hit at the specified cursor location, if any.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <returns>The hit connector, or null if none.</returns>
    public GraphConnector? GetHitConnector(Point cursorLocation)
    {
        var HitTest = new Rectangle(ControlToView(cursorLocation), new Size());
        var collection = _control.Diagram.NodeCollection;

        for (int i = collection.Count - 1; i >= 0; i--)
        {
            var n = collection[i];
            if (HitTest.IntersectsWith(n.HitRectangle))
            {
                return n.GetConnectorMouseHit(cursorLocation);
            }
        }

        return null;
    }

    #endregion

    #region Focus

    /// <summary>
    /// Centers the viewport on the currently selected nodes.
    /// </summary>
    public void FocusSelection()
    {
        var diagram = _control.Diagram;

        int avgX = (int)diagram.SelectedItems.Average(o => o._x + o.Width * 0.5f);
        int avgY = (int)diagram.SelectedItems.Average(o => o._y + o.Height * 0.5f);

        ViewX = -avgX;
        ViewY = -avgY;

        _control.RequestOutput();
    }

    /// <summary>
    /// Adjusts the viewport to focus on the specified rectangle, fitting it within the view.
    /// </summary>
    /// <param name="rect">The rectangle in view space to focus on.</param>
    public void FocusToRect(RectangleF rect)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            ViewX = -(int)rect.X;
            ViewY = -(int)rect.Y;
            _control.RequestOutput();
            return;
        }

        float zoomX = Width / rect.Width;
        float zoomY = Height / rect.Height;
        float newZoom = Math.Min(zoomX, zoomY) * 0.95f;

        if (newZoom < 0.1f) newZoom = 0.1f;
        if (newZoom > 20.0f) newZoom = 20.0f;

        ViewZoom = newZoom;

        float rectCenterX = rect.X + rect.Width / 2.0f;
        float rectCenterY = rect.Y + rect.Height / 2.0f;

        ViewX = -(int)rectCenterX;
        ViewY = -(int)rectCenterY;

        _control.RequestOutput();
    }

    #endregion

    /// <summary>
    /// Gets a value indicating whether the mouse cursor is inside the viewport.
    /// </summary>
    public bool IsMouseInside => GlobalViewRect.Contains(_control.InputManager.ScreenSpaceCursorLocation);


    /// <summary>
    /// Marks the current viewport position as the starting point for a panning operation.
    /// </summary>
    public void MarkMovingPosition()
    {
        _movingViewX = ViewX;
        _movingViewY = ViewY;
    }

    /// <summary>
    /// Determines whether the specified cursor location is near the edge of the viewport (scroll area).
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    /// <returns>True if the cursor is in the scroll area; otherwise, false.</returns>
    public bool IsInScrollArea(Point cursorLocation)
    {
        var rect = GlobalViewRect;
        return !(cursorLocation.X > rect.Left + 32
            && cursorLocation.X < rect.Right - 32
            && cursorLocation.Y > rect.Top + 32
            && cursorLocation.Y < rect.Bottom - 32);
    }

    /// <summary>
    /// Updates the viewport position based on the cursor being near the scroll area edges.
    /// </summary>
    /// <param name="cursorLocation">The cursor location in control space.</param>
    internal void UpdateScrollInternal(Point cursorLocation)
    {
        var rect = GlobalViewRect;
        int scrollMargins = 32;
        int scrollMarginsValue = 10;

        if (cursorLocation.X < rect.Left + scrollMargins)
        {
            ViewX += (int)(scrollMarginsValue / SmoothViewZoom);
        }
        else if (cursorLocation.X > rect.Right - scrollMargins)
        {
            ViewX -= (int)(scrollMarginsValue / SmoothViewZoom);
        }
        else if (cursorLocation.Y < rect.Top + scrollMargins)
        {
            ViewY += (int)(scrollMarginsValue / SmoothViewZoom);
        }
        else if (cursorLocation.Y > rect.Bottom - scrollMargins)
        {
            ViewY -= (int)(scrollMarginsValue / SmoothViewZoom);
        }
    }
}
