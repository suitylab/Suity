using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Helpers;
using Suity.Views.Graphics;
using System.Drawing;
using System.Linq;
using Suity.Drawing;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Provides extension methods for ImGui graph rendering and input handling.
/// </summary>
public static class ImGraphExtensions
{
    public const float ThresholdVeryFar = 0.25f;
    public const float ThresholdFar = 0.5f;
    public const float ThresholdMedium = 0.750f;
    public const float ThresholdNear = 1f;

    /// <summary>
    /// Scale threshold below which nodes are hidden from interaction and rendering.
    /// </summary>
    public static float NodeHiddenScale = 0.35f;

    readonly static BrushDef _NodeBG = new SolidBrushDef(EditorColorScheme.Default.Background);

    readonly static Color ColorRunning = Color.Cyan.MultiplyAlpha(0.5f);
    readonly static Color ColorFinished = Color.DarkGreen.MultiplyAlpha(0.5f);
    readonly static Color ColorError = Color.Red.MultiplyAlpha(0.5f);
    readonly static Color ColorCancelled = Color.Gray.MultiplyAlpha(0.5f);

    /// <summary>
    /// Creates a node frame with input blocking at low scale and custom rendering.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the node frame.</param>
    /// <returns>An ImGuiNode configured as a node frame.</returns>
    public static ImGuiNode NodeFrame(this ImGui gui, string id)
    {
        var node = gui.VerticalFrame(id);
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain(ScaleInputBlockerFar);
            node.SetRenderFunction(DrawNodeFrame);
        }

        return node;
    }

    /// <summary>
    /// Creates a connector point node with custom rendering for graph connections.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the connector point.</param>
    /// <returns>An ImGuiNode representing a connector point.</returns>
    public static ImGuiNode ConnectorPoint(this ImGui gui, string id)
    {
        var node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.SetRenderFunction(DrawConnectorPoint);
        }

        node.Layout();

        return node;
    }



    /// <summary>
    /// Renders the node frame with background, border, and computation state visualization.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="baseAction">The base render function to call for child rendering.</param>
    private static void DrawNodeFrame(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var rect = node.GlobalRect;
        // Uniformly shrink by 1 pixel to avoid rendering artifacts
        var rect_1 = rect.Offset(-1);

        var color = node.Color;
        float borderWidth = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);
        float cornerRound = node.GetFrameCornerRound(true);

        if (borderWidth > 0)
        {
            rect_1 = rect_1.OffsetHalf(-borderWidth);
        }

        if (color is { A: > 0 })
        {
            if (cornerRound > 0)
            {
                output.FillRoundRectangle(_NodeBG, rect, cornerRound);
                output.FillRoundRectangle(new SolidBrushDef(color.Value), rect_1, cornerRound);
            }
            else
            {
                output.FillRectangle(_NodeBG, rect);
                output.FillRectangle(new SolidBrushDef(color.Value), rect_1);
            }
        }

        if (node.GlobalScale < NodeHiddenScale)
        {
            baseAction(GuiPipeline.Blocked);
        }
        else
        {
            baseAction(pipeline);
        }

        // Border drawn afterwards to avoid being covered

        var imGraphNode = node.GetValue<ImGraphNode>();
        if (imGraphNode != null)
        {
            var computation = imGraphNode.Computation;

            Color? stateColor = imGraphNode.ComputationState switch
            {
                FlowComputationStates.None => null,
                FlowComputationStates.Running => ColorRunning,
                FlowComputationStates.Finished => ColorFinished,
                FlowComputationStates.Error => ColorError,
                FlowComputationStates.Cancelled => ColorCancelled,
                _ => null
            };

            if (stateColor is { } colorV)
            {
                var rectV = rect.OffsetHalf(-6);

                if (cornerRound > 0)
                {
                    output.DrawRoundRectangle(new PenDef(colorV, 6), rectV, cornerRound);
                }
                else
                {
                    output.DrawRectangle(new PenDef(colorV, 6), rectV);
                }
            }
        }

        if (borderWidth > 0)
        {
            if (cornerRound > 0)
            {
                output.DrawRoundRectangle(new PenDef(borderColor, borderWidth), rect_1, cornerRound);
            }
            else
            {
                output.DrawRectangle(new PenDef(borderColor, borderWidth), rect_1);
            }
        }
    }

    /// <summary>
    /// Renders a connector point with color, link state, and flashing animation.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    private static void DrawConnectorPoint(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        var rect = node.GlobalRect;
        Color? color = node.Color;
        // Border controls MouseIn event in the style
        float border = node.ScaledBorderWidth;
        Color borderColor = node.BorderColor ?? node.Theme.Colors.GetColor(ColorStyle.Border);

        bool isLinked = false;

        var connector = node.GetValue<GraphConnector>();
        if (connector != null)
        {
            color = connector.DataType.ConnectorFillBrush.Color;

            var view = connector.Parent.Diagram;
            isLinked = view.Links.IsLinked(connector);
        }

        if (color is { } colorV)
        {
            if (node.Animation is ConnectorPointFlashing flashing)
            {
                if (flashing.GetValue(node.AnimationStartTime, node.Gui.Time))
                {
                    colorV = colorV.Multiply(0.5f);
                }
            }

            var brush = new SolidBrushDef(colorV);

            if (connector?.ConnectorType == ConnectorType.Action)
            {
                if (isLinked)
                {
                    output.FillPolygon(brush, BuildActionTriangle(rect));
                }
                else
                {
                    output.DrawPolygon(new PenDef(colorV, rect.Width * 0.15f), BuildActionTriangle(rect.Scale(0.75f)));
                }
            }
            else if (connector?.DataType?.IsKey == true)
            {
                if (connector.DataType.IsArray)
                {
                    var outerDiamond = BuildKeyDiamond(rect.Scale(0.8f));
                    var innerDiamond = BuildKeyDiamond(isLinked ? rect.Scale(0.45f) : rect.Scale(0.35f));
                    var penWidth = rect.Width * 0.1f;

                    if (isLinked)
                    {
                        output.DrawPolygon(new PenDef(colorV, penWidth), outerDiamond);
                        output.FillPolygon(brush, innerDiamond);
                    }
                    else
                    {
                        output.DrawPolygon(new PenDef(colorV, penWidth), outerDiamond);
                        output.DrawPolygon(new PenDef(colorV, penWidth), innerDiamond);
                    }
                }
                else
                {
                    var diamondRect = isLinked ? rect : rect.Scale(0.75f);
                    var diamond = BuildKeyDiamond(diamondRect);

                    if (isLinked)
                    {
                        output.FillPolygon(brush, diamond);
                    }
                    else
                    {
                        output.DrawPolygon(new PenDef(colorV, rect.Width * 0.2f), diamond);
                    }
                }
            }
            else
            {
                if (connector?.DataType?.IsArray == true)
                {
                    var outerRect = rect.Scale(0.8f);
                    var innerRect = rect.Scale(0.45f);
                    var penWidth = rect.Width * 0.15f;

                    if (isLinked)
                    {
                        output.DrawEllipse(new PenDef(colorV, penWidth), outerRect);
                        output.FillEllipse(brush, innerRect);
                    }
                    else
                    {
                        output.DrawEllipse(new PenDef(colorV, penWidth), outerRect);
                        output.DrawEllipse(new PenDef(colorV, penWidth), innerRect);
                    }
                }
                else
                {
                    if (isLinked)
                    {
                        output.FillEllipse(brush, rect);
                    }
                    else
                    {
                        output.DrawEllipse(new PenDef(colorV, rect.Width * 0.2f), rect.Scale(0.8f));
                    }
                }
            }
        }

        if (connector?.IsCombined == true)
        {
            var combinedRect = isLinked ? rect.Scale(0.5f) : rect.Scale(0.6f);

            if (connector.ConnectorType == ConnectorType.Action)
            {
                if (isLinked)
                {
                    output.FillPolygon(new SolidBrushDef(Color.White), BuildActionTriangle(combinedRect));
                }
                else
                {
                    output.DrawPolygon(new PenDef(Color.White, rect.Width * 0.1f), BuildActionTriangle(combinedRect.Scale(0.75f)));
                }
            }
            else if (connector.DataType?.IsKey == true)
            {
                if (connector.DataType.IsArray)
                {
                    var outerDiamond = BuildKeyDiamond(rect.Scale(0.7f));
                    var innerDiamond = BuildKeyDiamond(isLinked ? rect.Scale(0.45f) : rect.Scale(0.35f));
                    var penWidth = rect.Width * 0.1f;

                    if (isLinked)
                    {
                        output.DrawPolygon(new PenDef(Color.White, penWidth), outerDiamond);
                        output.FillPolygon(new SolidBrushDef(Color.White), innerDiamond);
                    }
                    else
                    {
                        output.DrawPolygon(new PenDef(Color.White, penWidth), outerDiamond);
                        output.DrawPolygon(new PenDef(Color.White, penWidth), innerDiamond);
                    }
                }
                else
                {
                    var diamondRect = isLinked ? combinedRect.Scale(1.1f) : combinedRect;
                    var diamond = BuildKeyDiamond(diamondRect);

                    if (isLinked)
                    {
                        output.FillPolygon(new SolidBrushDef(Color.White), diamond);
                    }
                    else
                    {
                        output.DrawPolygon(new PenDef(Color.White, rect.Width * 0.1f), diamond);
                    }
                }
            }
            else
            {
                if (connector.DataType?.IsArray == true)
                {
                    var outerRect = rect.Scale(0.9f);
                    var innerRect = rect.Scale(0.5f);
                    var penWidth = rect.Width * 0.1f;

                    if (isLinked)
                    {
                        output.DrawEllipse(new PenDef(Color.White, penWidth), outerRect);
                        output.FillEllipse(new SolidBrushDef(Color.White), innerRect);
                    }
                    else
                    {
                        output.DrawEllipse(new PenDef(Color.White, penWidth), outerRect);
                        output.DrawEllipse(new PenDef(Color.White, penWidth), innerRect);
                    }
                }
                else
                {
                    if (isLinked)
                    {
                        output.FillEllipse(new SolidBrushDef(Color.White), combinedRect);
                    }
                    else
                    {
                        output.DrawEllipse(new PenDef(Color.White, rect.Width * 0.1f), combinedRect);
                    }
                }
            }
        }

        if (border > 0)
        {
            var borderRect = rect.Offset(-border * 0.5f);

            if (connector?.ConnectorType == ConnectorType.Action)
            {
                output.DrawPolygon(new PenDef(borderColor, border), BuildActionTriangle(borderRect));
            }
            else if (connector?.DataType?.IsKey == true)
            {
                output.DrawPolygon(new PenDef(borderColor, border), BuildKeyDiamond(borderRect));

                if (connector.DataType.IsArray)
                {
                    var innerDiamond = BuildKeyDiamond(borderRect.Scale(0.6f));
                    output.DrawPolygon(new PenDef(borderColor, border * 0.5f), innerDiamond);
                }
            }
            else if (connector?.DataType?.IsArray == true)
            {
                output.DrawEllipse(new PenDef(borderColor, border), borderRect);
                output.DrawEllipse(new PenDef(borderColor, border * 0.5f), borderRect.Scale(0.6f));
            }
            else
            {
                output.DrawEllipse(new PenDef(borderColor, border), borderRect);
            }
        }
    }

    /// <summary>
    /// Builds a right-pointing equilateral triangle for action connector points.
    /// </summary>
    /// <param name="rect">The bounding rectangle for the triangle.</param>
    /// <returns>An array of three points defining the triangle.</returns>
    private static PointF[] BuildActionTriangle(RectangleF rect)
    {
        var cx = rect.X + rect.Width / 2;
        var cy = rect.Y + rect.Height / 2;
        var r = rect.Width / 2;
        var h = r * 1.5f;
        var halfBase = h * 0.57735f;

        return
        [
            new PointF(cx + r, cy),
            new PointF(cx - h / 2, cy - halfBase),
            new PointF(cx - h / 2, cy + halfBase),
        ];
    }

    /// <summary>
    /// Builds a diamond shape for key connector points.
    /// </summary>
    /// <param name="rect">The bounding rectangle for the diamond.</param>
    /// <returns>An array of four points defining the diamond.</returns>
    private static PointF[] BuildKeyDiamond(RectangleF rect)
    {
        var cx = rect.X + rect.Width / 2;
        var cy = rect.Y + rect.Height / 2;
        var hw = rect.Width / 2;
        var hh = rect.Height / 2;

        return
        [
            new PointF(cx, cy - hh),
            new PointF(cx + hw, cy),
            new PointF(cx, cy + hh),
            new PointF(cx - hw, cy),
        ];
    }



    /// <summary>
    /// Input handler for node resizing that changes cursor based on mouse position near edges.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    public static GuiInputState NodeResizeInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.MouseOut:
                node.Gui.SetCursor(GuiCursorTypes.Default);
                break;

            case GuiEventTypes.MouseMove:
                {
                    if (input.MouseLocation is { } mousePos)
                    {
                        var rect = node.GlobalRect;
                        if (mousePos.Y < rect.Top + 5 || mousePos.Y > rect.Bottom - 5)
                        {
                            node.Gui.SetCursor(GuiCursorTypes.HSplit);
                        }
                        else if (mousePos.X < rect.Left + 5 || mousePos.X > rect.Right - 5)
                        {
                            node.Gui.SetCursor(GuiCursorTypes.VSplit);
                        }
                        else
                        {
                            node.Gui.SetCursor(GuiCursorTypes.Default);
                        }
                    }
                }
                break;
        }

        return GuiInputState.None;
    }

    /// <summary>
    /// Input blocker that prevents interaction when node scale is at the Very Far threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="childAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    public static GuiInputState ScaleInputBlockerVeryFar(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childAction)
    {
        if (node.GlobalScale < ThresholdVeryFar)
        {
            childAction(GuiPipeline.Blocked);
            return GuiInputState.None;
        }
        else
        {
            return childAction(pipeline);
        }
    }

    /// <summary>
    /// Input blocker that prevents interaction when node scale is at the Far threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="childAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    public static GuiInputState ScaleInputBlockerFar(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childAction)
    {
        if (node.GlobalScale < ThresholdFar)
        {
            childAction(GuiPipeline.Blocked);
            return GuiInputState.None;
        }
        else
        {
            return childAction(pipeline);
        }
    }

    /// <summary>
    /// Input blocker that prevents interaction when node scale is at the Medium threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="childAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    public static GuiInputState ScaleInputBlockerMedium(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childAction)
    {
        if (node.GlobalScale < ThresholdMedium)
        {
            childAction(GuiPipeline.Blocked);
            return GuiInputState.None;
        }
        else
        {
            return childAction(pipeline);
        }
    }

    /// <summary>
    /// Input blocker that prevents interaction when node scale is at the Near threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="childAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    public static GuiInputState ScaleInputBlockerNear(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction childAction)
    {
        if (node.GlobalScale < ThresholdNear)
        {
            childAction(GuiPipeline.Blocked);
            return GuiInputState.None;
        }
        else
        {
            return childAction(pipeline);
        }
    }


    /// <summary>
    /// Render blocker that skips rendering when node scale is at the Very Far threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    public static void ScaleRenderBlockerVeryFar(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        if (node.GlobalScale < ThresholdVeryFar)
        {
            childAction(GuiPipeline.Blocked);
        }
        else
        {
            childAction(pipeline);
        }
    }

    /// <summary>
    /// Render blocker that skips rendering when node scale is at the Far threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    public static void ScaleRenderBlockerFar(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        if (node.GlobalScale < ThresholdFar)
        {
            childAction(GuiPipeline.Blocked);
        }
        else
        {
            childAction(pipeline);
        }
    }

    /// <summary>
    /// Render blocker that skips rendering when node scale is at the Medium threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    public static void ScaleRenderBlockerMedium(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        if (node.GlobalScale < ThresholdMedium)
        {
            childAction(GuiPipeline.Blocked);
        }
        else
        {
            childAction(pipeline);
        }
    }

    /// <summary>
    /// Render blocker that skips rendering when node scale is at the Near threshold.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="output">The graphic output to render to.</param>
    /// <param name="dirtyMode">Indicates whether rendering in dirty (partial) mode.</param>
    /// <param name="childAction">The base render function to call for child rendering.</param>
    public static void ScaleRenderBlockerNear(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction childAction)
    {
        if (node.GlobalScale < ThresholdNear)
        {
            childAction(GuiPipeline.Blocked);
        }
        else
        {
            childAction(pipeline);
        }
    }

    /// <summary>
    /// Determines whether a connector should display the multiple connection indicator icon.
    /// </summary>
    /// <param name="connector">The graph connector to check.</param>
    /// <returns><c>true</c> if the multiple connection icon should be shown; otherwise, <c>false</c>.</returns>
    public static bool ShowMultiple(this GraphConnector connector)
    {
        var dataProvider = connector.Parent?.Diagram?.DataTypeProvider;

        if (connector.AllowMultipleConnection == true)
        {
            bool showMultiple;

            var actionDataType = dataProvider?.ActionDataType;

            if (connector.DataType == actionDataType)
            {
                showMultiple = connector.Direction == GraphDirection.Output;
            }
            else
            {
                bool reverse = dataProvider?.RevertDataArray == true;
                if (reverse)
                {
                    showMultiple = connector.Direction == GraphDirection.Input;
                }
                else
                {
                    showMultiple = connector.Direction == GraphDirection.Output;
                }
            }

            if (showMultiple)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Draws the multiple connection indicator icon if applicable.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The graph connector to draw the icon for.</param>
    /// <returns><c>true</c> if the icon was drawn; otherwise, <c>false</c>.</returns>
    public static bool DrawMultipleIcon(this ImGui gui, GraphConnector connector)
    {
        if (connector.ShowMultiple())
        {
            gui.Image("multiple", CoreIconCache.Multiple)
            .SetImageFilter(connector.DataType.ConnectorFillBrush.Color)
            .InitClass("iconSmall")
            .SetToolTipsL("Supports multiple connections, sorted by Y axis.");

            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the view-side graph connector from a flow node connector using the provided context.
    /// </summary>
    /// <param name="connector">The flow node connector to resolve.</param>
    /// <param name="context">The draw context used to locate the view node.</param>
    /// <returns>The resolved <see cref="GraphConnector"/>, or null if not found.</returns>
    public static GraphConnector? ResolveViewConnector(this FlowNodeConnector connector, IDrawContext context)
    {
        if (context is GraphNode viewNode)
        {
            return viewNode.Connectors.Find(connector.Name);
        }
        else if (context is ImGuiGraphControl panel)
        {
            var viewNode2 = connector.ParentNode?.ViewNodes.FirstOrDefault(o => o.FlowView == panel) as GraphNode;

            return viewNode2?.Connectors.Find(connector.Name);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves all view-side graph connectors from a flow node connector across all view nodes.
    /// </summary>
    /// <param name="connector">The flow node connector to resolve.</param>
    /// <returns>An array of all resolved <see cref="GraphConnector"/> instances.</returns>
    public static GraphConnector[] ResolveViewConnectors(this FlowNodeConnector connector)
    {
        var viewNode = connector.ParentNode?.ViewNodes?.OfType<GraphNode>() ?? [];

        return viewNode.Select(o => o.Connectors.Find(connector.Name))
            .SkipNull()
            .ToArray();
    }
}
