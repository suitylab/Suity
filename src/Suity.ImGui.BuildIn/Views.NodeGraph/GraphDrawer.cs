using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Represents a complete Bezier curve shape for link rendering.
/// </summary>
public struct LinkShape
{
    public PointF StartPos;
    public PointF EndPos;
    public PointF StartPosBezier;
    public PointF EndPosBezier;

    public LinkShape(PointF startPos, PointF endPos, PointF startPosBezier, PointF endPosBezier)
    {
        StartPos = startPos;
        EndPos = endPos;
        StartPosBezier = startPosBezier;
        EndPosBezier = endPosBezier;
    }
}

/// <summary>
/// Responsible for rendering the node graph, including grid, links, nodes, selection box, and debug information.
/// </summary>
public class GraphDrawer
{
    private readonly GraphControl _control;
    private GraphDiagram Diagram => _control.Diagram;
    private GraphViewport Viewport => _control.Viewport;
    private GraphControlTheme Theme => _control.Theme;
    private GraphInputManager InputManager => _control.InputManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphDrawer"/> class.
    /// </summary>
    /// <param name="control">The parent graph control.</param>
    public GraphDrawer(GraphControl control)
    {
        _control = control;
    }

    /// <summary>
    /// Gets the parent graph control.
    /// </summary>
    public GraphControl ParentControl => _control;

    /// <summary>
    /// Gets or sets a value indicating whether debug information should be rendered.
    /// </summary>
    public bool EnableDrawDebug { get; set; } = false;

    /// <summary>
    /// Gets or sets the visual style used for drawing links.
    /// </summary>
    public LinkVisualStyle LinkVisualStyle
    {
        get => _linkVisualStyle;
        set { _linkVisualStyle = value; _control.RequestOutput(); }
    }
    private LinkVisualStyle _linkVisualStyle = LinkVisualStyle.Curve;

    /// <summary>
    /// Gets or sets a value indicating whether smooth zoom behavior is enabled.
    /// </summary>
    public bool SmoothBehavior { get; set; }

    /// <summary>
    /// Gets or sets the hardness factor for link bezier curves. Lower values create sharper curves.
    /// </summary>
    public float LinkHardness { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets a value indicating whether links should be colored based on their data type.
    /// </summary>
    public bool UseLinkColoring { get; set; } = true;

    private int _gridPadding = 100;
    /// <summary>
    /// Gets or sets the spacing between major grid lines in view space.
    /// </summary>
    public int GridPadding
    {
        get => _gridPadding;
        set { _gridPadding = value; _gridPadding10 = value / 10; }
    }
    internal int _gridPadding10 = 10;

    /// <summary>
    /// Gets or sets a value indicating whether the background grid is visible.
    /// </summary>
    public bool ShowGrid { get; set; }
    /// <summary>
    /// Gets or sets the alpha transparency value for grid lines.
    /// </summary>
    public byte GridAlpha { get; set; } = 32;


    /// <summary>
    /// Performs pre-rendering operations including clearing the background, smoothing zoom transitions, and drawing the grid and associate links.
    /// </summary>
    /// <param name="output">The graphic output surface.</param>
    public void PreGraphicOutput(IGraphicOutput output)
    {
        output.Clear(_control.Theme.BackColor);

        if (SmoothBehavior)
        {
            Viewport.SmoothViewZoom += (Viewport.ViewZoom - Viewport.SmoothViewZoom) * 0.08f;
            if (System.Math.Abs(Viewport.SmoothViewZoom - Viewport.ViewZoom) < 0.005)
            {
                Viewport.SmoothViewZoom = Viewport.ViewZoom;
                _control.UpdateFontSize();
            }
            else
            {
                _control.UpdateFontSize();
            }
        }
        else
        {
            Viewport.SmoothViewZoom = Viewport.ViewZoom;
            _control.UpdateFontSize();
        }

        DrawGrid(output);
        DrawAllAssociateLinks(output);
    }

    /// <summary>
    /// Performs secondary pre-rendering operations, drawing all links and nodes.
    /// </summary>
    /// <param name="output">The graphic output surface.</param>
    public void PreGraphicOutput2(IGraphicOutput output)
    {
        DrawAllLinks(output);

        foreach (GraphNode node in Diagram.NodeCollection)
        {
            node.Draw(output);
        }
    }

    /// <summary>
    /// Performs post-rendering operations including drawing the selection box, editable link preview, and optional debug info.
    /// </summary>
    /// <param name="output">The graphic output surface.</param>
    public void PostGraphicOutput(IGraphicOutput output)
    {
        if (InputManager.ViewZooming)
        {
            InputManager.ViewZooming = false;
        }

        DrawSelectionBox(output);
        DrawLinkEditable(output);

        if (EnableDrawDebug)
        {
            DrawDebug(output);
        }
    }

    /// <summary>
    /// Creates a link shape between two points with a specified connector type.
    /// </summary>
    /// <param name="startPos">Start position of the link.</param>
    /// <param name="endPos">End position of the link.</param>
    /// <param name="connectorType">Connector type for the link.</param>
    /// <returns>Returns a new LinkShape object.</returns>
    public LinkShape CreateLinkShape(PointF startPos, PointF endPos, ConnectorType connectorType)
    {
        PointF startPosBezier, endPosBezier;

        float minD = 70 * _control.Viewport.ScaledViewZoom;

        if (connectorType.GetIsNormalConnector())
        {
            float d = endPos.X - startPos.X;
            if (d > 0)
            {
                if (d < minD) d = minD;
            }
            else
            {
                if (d > -minD) d = -minD;
            }

            float f = endPos.X < startPos.X ? -d : 0;
            startPosBezier = new PointF(startPos.X + (d / LinkHardness) + f, startPos.Y);
            endPosBezier = new PointF(endPos.X - (d / LinkHardness) - f, endPos.Y);
        }
        else
        {
            float d = endPos.Y - startPos.Y;
            if (d > 0)
            {
                if (d < minD) d = minD;
            }
            else
            {
                if (d > -minD) d = -minD;
            }

            float f = endPos.Y > startPos.Y ? -d : 0;
            startPosBezier = new PointF(startPos.X, startPos.Y + (d / LinkHardness) + f);
            endPosBezier = new PointF(endPos.X, endPos.Y - (d / LinkHardness) - f);
        }

        return new LinkShape(startPos, endPos, startPosBezier, endPosBezier);
    }

    private void DrawGrid(IGraphicOutput output)
    {
        if (!ShowGrid)
        {
            return;
        }

        var viewport = _control.Viewport;

        Color backColor = _control.Theme.BackColor;
        Color gridColor;
        int bgLum = (backColor.R + backColor.G + backColor.B) / 3;
        if (bgLum < 128)
        {
            gridColor = Color.FromArgb(GridAlpha, 255, 255, 255);
        }
        else
        {
            gridColor = Color.FromArgb(GridAlpha, 0, 0, 0);
        }

        var gridPen = new Pen(gridColor);

        var globalRect = Viewport.GlobalViewRect;
        int minGridX, maxGridX, minGridY, maxGridY;
        PointF viewTopLeft = viewport.ControlToView(new PointF(globalRect.X, globalRect.Y));
        PointF viewBottomRight = viewport.ControlToView(new PointF(globalRect.X + globalRect.Width, globalRect.Y + globalRect.Height));
        minGridX = (int)(viewTopLeft.X - (viewTopLeft.X % GridPadding));
        minGridY = (int)(viewTopLeft.Y - (viewTopLeft.Y % GridPadding));
        maxGridX = (int)(viewBottomRight.X + (viewBottomRight.X % GridPadding));
        maxGridY = (int)(viewBottomRight.Y + (viewBottomRight.Y % GridPadding));

        if (viewTopLeft.X > 0)
        {
            minGridX -= GridPadding;
        }
        if (viewTopLeft.Y > 0)
        {
            minGridY -= GridPadding;
        }
        if (viewBottomRight.X < 0)
        {
            maxGridX += GridPadding;
        }
        if (viewBottomRight.Y < 0)
        {
            maxGridY += GridPadding;
        }

        PointF currentGridIn, currentGridOut;

        for (int i = minGridX; i < maxGridX; i += GridPadding)
        {
            currentGridIn = viewport.ViewToControl(new PointF(i, viewTopLeft.Y));
            currentGridOut = viewport.ViewToControl(new PointF(i, viewBottomRight.Y));
            output.DrawLine(gridPen, currentGridIn, currentGridOut);
        }

        for (int j = minGridY; j < maxGridY; j += GridPadding)
        {
            currentGridIn = viewport.ViewToControl(new PointF(viewTopLeft.X, j));
            currentGridOut = viewport.ViewToControl(new PointF(viewBottomRight.X, j));
            output.DrawLine(gridPen, currentGridIn, currentGridOut);
        }

        float zoom = viewport.ScaledViewZoom;

        if (zoom > 1)
        {
            int gridPadding10 = GridPadding / 10;
            for (int i = minGridX - GridPadding; i < maxGridX; i += gridPadding10)
            {
                currentGridIn = viewport.ViewToControl(new PointF(i, viewTopLeft.Y));
                currentGridOut = viewport.ViewToControl(new PointF(i, viewBottomRight.Y));
                output.DrawLine(gridPen, currentGridIn, currentGridOut);
            }

            for (int j = minGridY - GridPadding; j < maxGridY; j += gridPadding10)
            {
                currentGridIn = viewport.ViewToControl(new PointF(viewTopLeft.X, j));
                currentGridOut = viewport.ViewToControl(new PointF(viewBottomRight.X, j));
                output.DrawLine(gridPen, currentGridIn, currentGridOut);
            }
        }

        PointF zero = viewport.ViewToControl(new PointF(0, 0));
        int len = (int)(20 * zoom);

        output.DrawEllipse(gridPen, new RectangleF(zero.X - len, zero.Y - len, len * 2, len * 2));
    }

    private void DrawSelectionBox(IGraphicOutput output)
    {
        if (InputManager.EditMode == GraphEditMode.SelectingBox)
        {
            var viewRectangle = new RectangleF();
            if (InputManager.SelectBoxOrigin.X > InputManager.SelectBoxCurrent.X)
            {
                viewRectangle.X = InputManager.SelectBoxCurrent.X;
                viewRectangle.Width = InputManager.SelectBoxOrigin.X - InputManager.SelectBoxCurrent.X;
            }
            else
            {
                viewRectangle.X = InputManager.SelectBoxOrigin.X;
                viewRectangle.Width = InputManager.SelectBoxCurrent.X - InputManager.SelectBoxOrigin.X;
            }
            if (InputManager.SelectBoxOrigin.Y > InputManager.SelectBoxCurrent.Y)
            {
                viewRectangle.Y = InputManager.SelectBoxCurrent.Y;
                viewRectangle.Height = InputManager.SelectBoxOrigin.Y - InputManager.SelectBoxCurrent.Y;
            }
            else
            {
                viewRectangle.Y = InputManager.SelectBoxOrigin.Y;
                viewRectangle.Height = InputManager.SelectBoxCurrent.Y - InputManager.SelectBoxOrigin.Y;
            }

            output.FillRectangle(Theme.SelectionFill, _control.Viewport.ViewToControl(viewRectangle));
            output.DrawRectangle(Theme.SelectionOutline, _control.Viewport.ViewToControl(viewRectangle));
        }
    }

    private void DrawLinkEditable(IGraphicOutput output)
    {
        if (InputManager.EditMode == GraphEditMode.Linking && InputManager.FromConnector != null)
        {
            PointF startPos = InputManager.FromConnector.GetPosition();
            PointF endPos = _control.Viewport.ViewToControl(new PointF(InputManager.ViewSpaceCursorLocation.X, InputManager.ViewSpaceCursorLocation.Y));

            PointF startPosBezier;
            PointF endPosBezier;

            if (InputManager.FromConnector.ConnectorType.GetIsNormalConnector())
            {
                startPosBezier = new(startPos.X + (endPos.X - startPos.X) / LinkHardness, startPos.Y);
                endPosBezier = new(endPos.X - (endPos.X - startPos.X) / LinkHardness, endPos.Y);
            }
            else
            {
                startPosBezier = new(startPos.X, startPos.Y + (endPos.Y - startPos.Y) / LinkHardness);
                endPosBezier = new(endPos.X, endPos.Y - (endPos.Y - startPos.Y) / LinkHardness);
            }

            var scale = _control.Viewport.ScaledViewZoom;
            InputManager.FromConnector.DataType.LinkPen.Width = 3f * scale;

            switch (LinkVisualStyle)
            {
                case LinkVisualStyle.Curve:
                    if (InputManager.FromConnector.IsCombined)
                    {
                        if (Theme.LinkCombinedPen != null)
                        {
                            Theme.LinkCombinedPen.Width = 5f * scale;
                            output.DrawBezier(Theme.LinkCombinedPen, startPos, startPosBezier, endPosBezier, endPos);
                        }
                    }
                    output.DrawBezier(InputManager.FromConnector.DataType.LinkPen, startPos, startPosBezier, endPosBezier, endPos);
                    break;

                case LinkVisualStyle.Direct:
                    output.DrawLine(InputManager.FromConnector.DataType.LinkPen, startPos, endPos);
                    break;

                case LinkVisualStyle.Rectangle:
                    output.DrawLine(InputManager.FromConnector.DataType.LinkPen, startPos, startPosBezier);
                    output.DrawLine(InputManager.FromConnector.DataType.LinkPen, startPosBezier, endPosBezier);
                    output.DrawLine(InputManager.FromConnector.DataType.LinkPen, endPosBezier, endPos);
                    break;

                default: break;
            }

            PointF typeHintPos = new(endPos.X, endPos.Y - 10);
            PointF typeHintShadowPos = new(typeHintPos.X + 1, typeHintPos.Y + 1);

            string str = $"{InputManager.FromConnector.DisplayName} {InputManager.FromConnector.DataType}";

            output.DrawString(str, Theme.NodePreviewFont, Theme.NodeTextShadow, typeHintShadowPos);
            output.DrawString(str, Theme.NodePreviewFont, InputManager.FromConnector.DataType.LinkArrowBrush, typeHintPos);
        }
    }

    private void DrawAllAssociateLinks(IGraphicOutput output)
    {
        PointF startPos, endPos;
        RectangleF screenRect = Viewport.GlobalViewRect;

        foreach (GraphLink link in Diagram.Links.Where(o => o.ConnectorType == ConnectorType.Associate))
        {
            startPos = link.From.GetPosition();
            endPos = link.To.GetPosition();

            float minX = System.Math.Min(startPos.X, endPos.X);
            float minY = System.Math.Min(startPos.Y, endPos.Y);
            var viewRectangle = new RectangleF(minX, minY, System.Math.Abs(endPos.X - startPos.X), System.Math.Abs(endPos.Y - startPos.Y));
            if (!viewRectangle.IntersectsWith(screenRect))
            {
                continue;
            }

            DrawAssociateLink(output, startPos, endPos, link);
        }
    }

    private void DrawAllLinks(IGraphicOutput output)
    {
        PointF startPos, endPos;
        RectangleF screenRect = Viewport.GlobalViewRect;

        foreach (GraphLink link in Diagram.Links)
        {
            startPos = link.From.GetPosition();
            endPos = link.To.GetPosition();

            float minX = System.Math.Min(startPos.X, endPos.X);
            float minY = System.Math.Min(startPos.Y, endPos.Y);
            var viewRectangle = new RectangleF(minX, minY, System.Math.Abs(endPos.X - startPos.X), System.Math.Abs(endPos.Y - startPos.Y));
            if (!viewRectangle.IntersectsWith(screenRect))
            {
                continue;
            }

            switch (link.ConnectorType)
            {
                case ConnectorType.Data:
                case ConnectorType.Action:
                case ConnectorType.Control:
                    DrawLink(output, startPos, endPos, link, link.ConnectorType);
                    break;
            }
        }
    }

    private void DrawLink(IGraphicOutput output, PointF startPos, PointF endPos, GraphLink link, ConnectorType connectorType)
    {
        LinkShape shape = CreateLinkShape(startPos, endPos, connectorType);
        Pen linkPen;

        if (UseLinkColoring)
        {
            linkPen = link.DataType.LinkPen;
        }
        else
        {
            linkPen = Theme.Link;
        }

        if (!link.CheckLink(Diagram.DataTypeProvider))
        {
            linkPen = Theme.LinkError;
        }

        float scale = _control.Viewport.ScaledViewZoom;
        var dashStyle = link.IsConverted ? DashStyle.Dot : DashStyle.Solid;
        if (scale < ImGraphExtensions.ThresholdMedium)
        {
            dashStyle = DashStyle.Solid;
        }

        switch (LinkVisualStyle)
        {
            case LinkVisualStyle.Curve:
                if (link.From.IsCombined)
                {
                    if (link.Highlighted)
                    {
                        Theme.LinkSelectedCombinedPen.Width = 7f * scale;
                        output.DrawBezier(Theme.LinkSelectedCombinedPen, shape.StartPos, shape.StartPosBezier, shape.EndPosBezier, shape.EndPos);
                    }

                    var colorInput = link.From.DataType.LinkPen.Color;
                    var colorOutput = link.To.DataType.LinkPen.Color;
                    output.DrawBezier(colorInput, colorOutput, 5f * scale, shape.StartPos, shape.StartPosBezier, shape.EndPosBezier, shape.EndPos, dashStyle);

                    Theme.LinkCombinedPen.Width = 2f * scale;
                    output.DrawBezier(Theme.LinkCombinedPen, shape.StartPos, shape.StartPosBezier, shape.EndPosBezier, shape.EndPos);
                }
                else
                {
                    if (link.Highlighted)
                    {
                        Theme.LinkSelectedPen.Width = 5f * scale;
                        output.DrawBezier(Theme.LinkSelectedPen, shape.StartPos, shape.StartPosBezier, shape.EndPosBezier, shape.EndPos);
                    }

                    var colorInput = link.From.DataType.LinkPen.Color;
                    var colorOutput = link.To.DataType.LinkPen.Color;
                    output.DrawBezier(colorInput, colorOutput, 3f * scale, shape.StartPos, shape.StartPosBezier, shape.EndPosBezier, shape.EndPos, dashStyle);
                }
                break;

            case LinkVisualStyle.Direct:
                linkPen.Width = 3f * scale;
                output.DrawLine(linkPen, shape.StartPos, shape.EndPos);
                break;

            case LinkVisualStyle.Rectangle:
                linkPen.Width = 3f * scale;
                output.DrawLine(linkPen, shape.StartPos, shape.StartPosBezier);
                output.DrawLine(linkPen, shape.StartPosBezier, shape.EndPosBezier);
                output.DrawLine(linkPen, shape.EndPosBezier, shape.EndPos);
                break;

            default: break;
        }
    }

    private void DrawAssociateLink(IGraphicOutput output, PointF startPos, PointF endPos, GraphLink link)
    {
        Color color;

        if (UseLinkColoring)
        {
            color = link.DataType.LinkPen.Color;
        }
        else
        {
            color = Theme.Link.Color;
        }

        Pen linkPen;
        if (!link.CheckLink(Diagram.DataTypeProvider))
        {
            linkPen = Theme.LinkError;
        }
        else
        {
            linkPen = new Pen(color.MultiplyAlpha(0.3f), 7f * _control.Viewport.ScaledViewZoom)
            {
                DashStyle = DashStyle.Dash
            };
        }

        output.DrawLine(linkPen, startPos, endPos);
    }

    private void DrawDebug(IGraphicOutput output)
    {
        var viewport = _control.Viewport;

        var debugFont = new Font(System.Drawing.FontFamily.GenericSansSerif, 8.0f);
        output.DrawString("Edit Mode:" + InputManager.EditMode.ToString(), debugFont, Theme.NodeText, new PointF(0.0f, 0.0f));
        output.DrawString("ViewX: " + Viewport.ViewX.ToString(), debugFont, Theme.NodeText, new PointF(0.0f, 10.0f));
        output.DrawString("ViewY: " + Viewport.ViewY.ToString(), debugFont, Theme.NodeText, new PointF(0.0f, 20.0f));
        output.DrawString("ViewZoom: " + Viewport.ViewZoom.ToString(), debugFont, Theme.NodeText, new PointF(0.0f, 30.0f));

        output.DrawString("ViewSpace Cursor Location:" + InputManager.ViewSpaceCursorLocation.X.ToString() + " : " + InputManager.ViewSpaceCursorLocation.Y.ToString(), debugFont, Theme.NodeText, new PointF(0.0f, 50.0f));

        var originPen = new Pen(Color.Lime);
        output.DrawLine(originPen, viewport.ViewToControl(new Point(-100, 0)), viewport.ViewToControl(new Point(100, 0)));
        output.DrawLine(originPen, viewport.ViewToControl(new Point(0, -100)), viewport.ViewToControl(new Point(0, 100)));

        output.DrawBezier(originPen, viewport.ViewToControl(InputManager.SelectBoxOrigin), viewport.ViewToControl(InputManager.SelectBoxOrigin), viewport.ViewToControl(InputManager.SelectBoxCurrent), viewport.ViewToControl(InputManager.SelectBoxCurrent));
    }
}
