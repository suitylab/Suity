using Suity.Editor.Flows;
using System.Drawing;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Provides extension methods for flow node graph rendering operations.
/// </summary>
public static class NodeGraphExtensions
{
    internal static NodeGraphExternal _external;

    /// <summary>
    /// Creates a fitted node frame that sizes to its content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="node">The drawing node context.</param>
    /// <returns>An ImGui node representing the fitted frame.</returns>
    public static ImGuiNode FlowFittedNodeFrame(this ImGui gui, IDrawNodeContext node)
        => _external.FittedNodeFrame(gui, node);

    /// <summary>
    /// Creates a header frame for a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="node">The drawing node context.</param>
    /// <param name="id">The unique identifier for the header.</param>
    /// <param name="fit">Whether the header should fit its content.</param>
    /// <returns>An ImGui node representing the header frame.</returns>
    public static ImGuiNode FlowHeaderFrame(this ImGui gui, IDrawNodeContext node, string id, bool fit)
        => _external.HeaderFrame(gui, node, id, fit);

    /// <summary>
    /// Renders the title text section of a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="node">The drawing node context.</param>
    public static void FlowTitleTextSection(this ImGui gui, IDrawNodeContext node)
        => _external.TitleTextSection(gui, node);

    /// <summary>
    /// Renders the title preview section of a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="node">The drawing node context.</param>
    public static void FlowTitlePreviewSection(this ImGui gui, IDrawNodeContext node)
        => _external.TitlePreviewSection(gui, node);

    /// <summary>
    /// Creates an icon indicating whether a node is in an invalid state.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="isValid">Whether the node is valid.</param>
    /// <returns>An ImGui node for the invalid icon, or <c>null</c> if not applicable.</returns>
    public static ImGuiNode? FlowInvalidIcon(this ImGui gui, bool isValid)
        => _external.InvalidIcon(gui, isValid);

    /// <summary>
    /// Creates a horizontal box layout.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the box.</param>
    /// <param name="fit">Whether the box should fit its content.</param>
    /// <returns>An ImGui node representing the horizontal box.</returns>
    public static ImGuiNode FlowHorizontalBox(this ImGui gui, string id, bool fit)
        => _external.HorizontalBox(gui, id, fit);

    /// <summary>
    /// Creates a vertical box layout.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the box.</param>
    /// <param name="fit">Whether the box should fit its content.</param>
    /// <returns>An ImGui node representing the vertical box.</returns>
    public static ImGuiNode FlowVerticalBox(this ImGui gui, string id, bool fit)
        => _external.VerticalBox(gui, id, fit);

    /// <summary>
    /// Creates a body frame for a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the body frame.</param>
    /// <returns>An ImGui node representing the body frame.</returns>
    public static ImGuiNode FlowBodyFrame(this ImGui gui, string id)
        => _external.BodyFrame(gui, id);

    /// <summary>
    /// Creates a connector box for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <returns>An ImGui node representing the connector box.</returns>
    public static ImGuiNode FlowConnectorBox(this ImGui gui, FlowNodeConnector connector, IDrawContext context)
        => _external.ConnectorBox(gui, connector, context);

    /// <summary>
    /// Creates a connector row for displaying a connector with optional inner content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <param name="multipleIcon">Whether to display a multiple connection icon.</param>
    /// <param name="innerDraw">Optional inner drawing callback.</param>
    /// <returns>An ImGui node representing the connector row, or <c>null</c> if not applicable.</returns>
    public static ImGuiNode? FlowConnectorRow(this ImGui gui, FlowNodeConnector connector, IDrawContext context, bool multipleIcon, DrawImGui? innerDraw = null)
        => _external.ConnectorRow(gui, connector, context, multipleIcon, innerDraw);

    /// <summary>
    /// Creates a connector point for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <param name="id">The unique identifier for the connector point.</param>
    /// <returns>An ImGui node representing the connector point, or <c>null</c> if not applicable.</returns>
    public static ImGuiNode? FlowConnectorPoint(this ImGui gui, FlowNodeConnector connector, IDrawContext context, string id)
        => _external.ConnectorPoint(gui, connector, context, id);

    /// <summary>
    /// Renders the text section for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    public static void FlowConnectorTextSection(this ImGui gui, FlowNodeConnector connector)
        => _external.ConnectorTextSection(gui, connector);

    /// <summary>
    /// Triggers a one-time flashing effect on a connector.
    /// </summary>
    /// <param name="connector">The flow node connector to flash.</param>
    /// <param name="context">The drawing context.</param>
    public static void FlashingOnce(this FlowNodeConnector connector, IDrawContext? context = null)
        => _external.FlashingOnce(connector, context);

    /// <summary>
    /// Queues a refresh request for the specified flow node.
    /// </summary>
    /// <param name="node">The flow node to refresh.</param>
    public static void QueueRefresh(this FlowNode node)
        => _external.QueueRefresh(node);

    /// <summary>
    /// Creates a simple frame with optional text, image, and editor GUI content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="direction">The flow direction of the frame.</param>
    /// <param name="drawContext">The drawing node context.</param>
    /// <param name="text">Optional text to display.</param>
    /// <param name="image">Optional image to display.</param>
    /// <param name="editorGui">Optional editor GUI callback.</param>
    /// <returns>An ImGui node representing the simple frame.</returns>
    public static ImGuiNode FlowSimpleFrame(this ImGui gui, FlowDirections direction, IDrawNodeContext drawContext,
        string? text = null, Image? image = null, DrawEditorImGui? editorGui = null)
        => _external.SimpleFrame(gui, direction, drawContext, text, image, editorGui);

    /// <summary>
    /// Creates a frame for a single connector with optional text, image, and editor GUI content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="drawContext">The drawing node context.</param>
    /// <param name="text">Optional text to display.</param>
    /// <param name="image">Optional image to display.</param>
    /// <param name="editorGui">Optional editor GUI callback.</param>
    /// <returns>An ImGui node representing the single connector frame.</returns>
    public static ImGuiNode FlowSingleConnectorFrame(this ImGui gui, FlowNodeConnector connector, IDrawNodeContext drawContext, 
        string? text = null, Image? image = null, DrawEditorImGui? editorGui = null)
        => _external.SingleConnectorFrame(gui, connector, drawContext, text, image, editorGui);
}
