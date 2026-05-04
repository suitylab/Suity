using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Manages node selection operations including highlighting, moving, resizing, and collecting driven items.
/// </summary>
public class GraphSelectionManager
{
    private readonly GraphControl _control;
    private GraphDiagram Diagram => _control.Diagram;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphSelectionManager"/> class.
    /// </summary>
    /// <param name="control">The parent graph control.</param>
    public GraphSelectionManager(GraphControl control)
    {
        _control = control;
    }

    /// <summary>
    /// Gets the parent graph control.
    /// </summary>
    public GraphControl ParentControl => _control;

    /// <summary>
    /// Updates the highlighted state of nodes based on a selection box.
    /// </summary>
    /// <param name="multiple">True to allow multiple selections; otherwise, only the first hit is selected.</param>
    /// <param name="additional">True to preserve existing highlights; otherwise, unhit nodes are unhighlighted.</param>
    /// <param name="selectBoxOrigin">The origin of the selection box in view space.</param>
    /// <param name="selectBoxCurrent">The current corner of the selection box in view space.</param>
    /// <returns>True if a new node was selected; otherwise, false.</returns>
    public bool UpdateNodeHighlights(bool multiple, bool additional, Point selectBoxOrigin, Point selectBoxCurrent, bool normalNode = true, bool groupNode = true)
    {
        var ViewRectangle = new Rectangle();
        if (selectBoxOrigin.X > selectBoxCurrent.X)
        {
            ViewRectangle.X = selectBoxCurrent.X;
            ViewRectangle.Width = selectBoxOrigin.X - selectBoxCurrent.X;
        }
        else
        {
            ViewRectangle.X = selectBoxOrigin.X;
            ViewRectangle.Width = selectBoxCurrent.X - selectBoxOrigin.X;
        }
        if (selectBoxOrigin.Y > selectBoxCurrent.Y)
        {
            ViewRectangle.Y = selectBoxCurrent.Y;
            ViewRectangle.Height = selectBoxOrigin.Y - selectBoxCurrent.Y;
        }
        else
        {
            ViewRectangle.Y = selectBoxOrigin.Y;
            ViewRectangle.Height = selectBoxCurrent.Y - selectBoxOrigin.Y;
        }

        bool flag = true;
        bool selected = false;

        if (ViewRectangle.Width == 0 && ViewRectangle.Height == 0)
        {
            for (int i = Diagram.NodeCollection.Count - 1; i >= 0; i--)
            {
                var node = Diagram.NodeCollection[i];
                if (!groupNode && node.IsGroup)
                {
                    continue;
                }
                if (!normalNode && !node.IsGroup)
                {
                    continue;
                }

                if (ViewRectangle.IntersectsWith(node.HitRectangle) && node.CanBeSelected && flag)
                {
                    if (!node.Highlighted)
                    {
                        node.Highlighted = true;
                        selected = true;
                    }
                    if (!multiple)
                    {
                        flag = false;
                    }
                }
                else if (!additional)
                {
                    node.Highlighted = false;
                }
            }
        }
        else
        {
            for (int i = Diagram.NodeCollection.Count - 1; i >= 0; i--)
            {
                var node = Diagram.NodeCollection[i];
                if (!groupNode && node.IsGroup)
                {
                    continue;
                }
                if (!normalNode && !node.IsGroup)
                {
                    continue;
                }

                if (!node.IsGroup && ViewRectangle.IntersectsWith(node.HitRectangle) && node.CanBeSelected && flag)
                {
                    if (!node.Highlighted)
                    {
                        node.Highlighted = true;
                        selected = true;
                    }
                    if (!multiple)
                    {
                        flag = false;
                    }
                }
                else if (node.IsGroup && ViewRectangle.Contains(node.HitRectangle) && node.CanBeSelected && flag)
                {
                    if (!node.Highlighted)
                    {
                        node.Highlighted = true;
                        selected = true;
                    }
                    if (!multiple)
                    {
                        flag = false;
                    }
                }
                else if (!additional)
                {
                    node.Highlighted = false;
                }
            }
        }

        return selected;
    }

    /// <summary>
    /// Updates the highlighted state of links based on proximity to a given position.
    /// </summary>
    /// <param name="multiple">True to allow multiple selections; otherwise, only the first hit is selected.</param>
    /// <param name="additional">True to preserve existing highlights; otherwise, unhit links are unhighlighted.</param>
    /// <param name="position">The cursor position in view space to test against link paths.</param>
    /// <returns>True if a new link was selected; otherwise, false.</returns>
    public bool UpdateLinkHighlights(bool multiple, bool additional, Point position)
    {
        PointF startPos, endPos;
        RectangleF screenRect = ParentControl.Viewport.GlobalViewRect;

        var drawer = ParentControl.Drawer;

        bool flag = true;
        bool selected = false;

        foreach (GraphLink link in Diagram.Links.Where(o => o.ConnectorType != ConnectorType.Associate))
        {
            startPos = link.From.GetPosition();
            endPos = link.To.GetPosition();

            float minX = System.Math.Min(startPos.X, endPos.X);
            float minY = System.Math.Min(startPos.Y, endPos.Y);
            var viewRectangle = new RectangleF(minX, minY, System.Math.Abs(endPos.X - startPos.X), System.Math.Abs(endPos.Y - startPos.Y));
            if (!viewRectangle.IntersectsWith(screenRect))
            {
                if (!additional)
                {
                    link.Highlighted = false;
                }
                continue;
            }

            LinkShape shape = drawer.CreateLinkShape(startPos, endPos, link.ConnectorType);
            float tolerance = 3.5f * _control.Viewport.ScaledViewZoom;
            bool hit = shape.IsPointNearBezierRecursive(position, tolerance);

            if (hit && flag)
            {
                if (!link.Highlighted)
                {
                    link.Highlighted = true;
                    selected = true;
                }
                if (!multiple)
                {
                    flag = false;
                }
            }
            else if (!additional)
            {
                link.Highlighted = false;
            }
        }

        return selected;
    }

    /// <summary>
    /// Clears the highlighted state of all links in the diagram.
    /// </summary>
    public void ClearLinkHighlights()
    {
        foreach (GraphLink link in Diagram.Links)
        {
            link.Highlighted = false;
        }
    }

    /// <summary>
    /// Clears the highlighted state of all nodes in the diagram.
    /// </summary>
    public void ClearAllNodeHighlights()
    {
        foreach (GraphNode node in Diagram.NodeCollection)
        {
            node.Highlighted = false;
        }
    }

    public void ClearGroupNodeHighlights()
    {
        foreach (GraphNode node in Diagram.NodeCollection.Groups)
        {
            node.Highlighted = false;
        }
    }


    /// <summary>
    /// Moves the selected nodes by the offset between origin and target.
    /// </summary>
    /// <param name="origin">The starting cursor position in view space.</param>
    /// <param name="target">The current cursor position in view space.</param>
    /// <param name="snapping">True to snap positions to a grid; otherwise, false.</param>
    public void MoveSelection(Point origin, Point target, bool snapping)
    {
        int ox = target.X - origin.X;
        int oy = target.Y - origin.Y;

        foreach (var n in Diagram.SelectedNodes.Concat(Diagram.DrivenNodes))
        {
            var p = n._movingPoint;

            if (snapping)
            {
                n._x = p.X + (int)Math.Round(ox * 0.1f) * 10;
                n._y = p.Y + (int)Math.Round(oy * 0.1f) * 10;
            }
            else
            {
                n._x = p.X + ox;
                n._y = p.Y + oy;
            }

            n.UpdateHitRectangle(true, false);
        }
    }

    /// <summary>
    /// Resizes the selected nodes based on the drag offset and resize side.
    /// </summary>
    /// <param name="origin">The starting cursor position in view space.</param>
    /// <param name="target">The current cursor position in view space.</param>
    /// <param name="resizeSide">The side of the node being resized.</param>
    /// <param name="snapping">True to snap sizes to a grid; otherwise, false.</param>
    public void ResizeSelection(Point origin, Point target, GraphResizeSide resizeSide, bool snapping)
    {
        int ox = target.X - origin.X;
        int oy = target.Y - origin.Y;

        int minX = 30;
        int minY = 30;

        foreach (var n in Diagram.SelectedNodes)
        {
            var p = n._movingPoint;
            var s = n._resizingSize;

            switch (resizeSide)
            {
                case GraphResizeSide.Left:
                    if (snapping)
                    {
                        n._x = p.X + (int)Math.Round(ox * 0.1f) * 10;
                        n._width = s.Width - (int)Math.Round(ox * 0.1f) * 10;
                    }
                    else
                    {
                        n._x = p.X + ox;
                        n._width = s.Width - ox;
                    }

                    if (n._width < minX)
                    {
                        int d = minX - n._width;
                        n._width = minX;
                        n._x += d;
                    }
                    break;

                case GraphResizeSide.Top:
                    if (snapping)
                    {
                        n._y = p.Y + (int)Math.Round(oy * 0.1f) * 10;
                        n._height = s.Height - (int)Math.Round(oy * 0.1f) * 10;
                    }
                    else
                    {
                        n._y = p.Y + oy;
                        n._height = s.Height - oy;
                    }

                    if (n._height < minY)
                    {
                        int d = minY - n._height;
                        n._height = minY;
                        n._y += d;
                    }
                    break;

                case GraphResizeSide.Corner:
                    if (snapping)
                    {
                        n._width = s.Width + (int)Math.Round(ox * 0.1f) * 10;
                        n._height = s.Height + (int)Math.Round(oy * 0.1f) * 10;
                    }
                    else
                    {
                        n._width = s.Width + ox;
                        n._height = s.Height + oy;
                    }

                    if (n._width < minX)
                    {
                        n._width = minX;
                    }
                    if (n._height < minY)
                    {
                        n._height = minY;
                    }
                    break;

                case GraphResizeSide.Right:
                    if (snapping)
                    {
                        n._width = s.Width + (int)Math.Round(ox * 0.1f) * 10;
                    }
                    else
                    {
                        n._width = s.Width + ox;
                    }

                    if (n._width < minX)
                    {
                        n._width = minX;
                    }
                    break;

                case GraphResizeSide.Bottom:
                    if (snapping)
                    {
                        n._height = s.Height + (int)Math.Round(oy * 0.1f) * 10;
                    }
                    else
                    {
                        n._height = s.Height + oy;
                    }

                    if (n._height < minY)
                    {
                        n._height = minY;
                    }
                    break;
            }

            n.UpdateHitRectangle(true, true);
        }
    }

    /// <summary>
    /// Collects nodes that are driven by selected group nodes (nodes contained within groups).
    /// </summary>
    public void CollectDrivenItems()
    {
        Diagram.DrivenNodes.Clear();

        foreach (var item in Diagram.SelectedNodes.Where(o => o.IsGroup))
        {
            foreach (var test in Diagram.NodeCollection.Where(o => o != item))
            {
                if (test.IsGroup)
                {
                    if (item.HitRectangle.Contains(test.HitRectangle))
                    {
                        Diagram.DrivenNodes.Add(test);
                    }
                }
                else
                {
                    if (item.HitRectangle.IntersectsWith(test.HitRectangle))
                    {
                        Diagram.DrivenNodes.Add(test);
                    }
                }
            }
        }
    }
}
