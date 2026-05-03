using Suity.Drawing;
using Suity.Editor.Flows;
using System;
using System.Drawing;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Provides abstract definitions for external node graph rendering operations.
/// </summary>
internal abstract class NodeGraphExternal
{
    /// <summary>
    /// Creates a fitted node frame that sizes to its content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="context">The drawing node context.</param>
    /// <returns>An ImGui node representing the fitted frame.</returns>
    public abstract ImGuiNode FittedNodeFrame(ImGui gui, IDrawNodeContext context);

    /// <summary>
    /// Creates a resizable node frame that can be adjusted by the user.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="context">The drawing node context.</param>
    /// <returns>An ImGui node representing the resizable frame.</returns>
    public abstract ImGuiNode ResizableNodeFrame(ImGui gui, IDrawNodeContext context);

    /// <summary>
    /// Creates a header frame for a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="context">The drawing node context.</param>
    /// <param name="id">The unique identifier for the header.</param>
    /// <param name="fit">Whether the header should fit its content.</param>
    /// <returns>An ImGui node representing the header frame.</returns>
    public abstract ImGuiNode HeaderFrame(ImGui gui, IDrawNodeContext context, string id, bool fit);

    /// <summary>
    /// Renders the title text section of a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="context">The drawing node context.</param>
    public abstract void TitleTextSection(ImGui gui, IDrawNodeContext context);

    /// <summary>
    /// Renders the title preview section of a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="context">The drawing node context.</param>
    public abstract void TitlePreviewSection(ImGui gui, IDrawNodeContext context);

    /// <summary>
    /// Creates an icon indicating whether a node is in an invalid state.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="isValid">Whether the node is valid.</param>
    /// <returns>An ImGui node for the invalid icon, or <c>null</c> if not applicable.</returns>
    [Obsolete]
    public abstract ImGuiNode? InvalidIcon(ImGui gui, bool isValid);

    /// <summary>
    /// Creates a horizontal box layout.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the box.</param>
    /// <param name="fit">Whether the box should fit its content.</param>
    /// <returns>An ImGui node representing the horizontal box.</returns>
    public abstract ImGuiNode HorizontalBox(ImGui gui, string id, bool fit);

    /// <summary>
    /// Creates a vertical box layout.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the box.</param>
    /// <param name="fit">Whether the box should fit its content.</param>
    /// <returns>An ImGui node representing the vertical box.</returns>
    public abstract ImGuiNode VerticalBox(ImGui gui, string id, bool fit);

    /// <summary>
    /// Creates a body frame for a node.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the body frame.</param>
    /// <returns>An ImGui node representing the body frame.</returns>
    public abstract ImGuiNode BodyFrame(ImGui gui, string id);

    /// <summary>
    /// Creates an overlay body frame that renders on top of other content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the overlay frame.</param>
    /// <returns>An ImGui node representing the overlay body frame.</returns>
    public abstract ImGuiNode OverlayBodyFrame(ImGui gui, string id);

    /// <summary>
    /// Creates a connector box for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <returns>An ImGui node representing the connector box.</returns>
    public abstract ImGuiNode ConnectorBox(ImGui gui, FlowNodeConnector connector, IDrawContext context);

    /// <summary>
    /// Creates a connector row for displaying a connector with optional inner content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <param name="multipleIcon">Whether to display a multiple connection icon.</param>
    /// <param name="innerDraw">Optional inner drawing callback.</param>
    /// <returns>An ImGui node representing the connector row, or <c>null</c> if not applicable.</returns>
    public abstract ImGuiNode? ConnectorRow(ImGui gui, FlowNodeConnector connector, IDrawContext context, bool multipleIcon, DrawImGui? innerDraw = null);

    /// <summary>
    /// Creates a connector point for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing context.</param>
    /// <param name="id">The unique identifier for the connector point.</param>
    /// <returns>An ImGui node representing the connector point, or <c>null</c> if not applicable.</returns>
    public abstract ImGuiNode? ConnectorPoint(ImGui gui, FlowNodeConnector connector, IDrawContext context, string id);

    /// <summary>
    /// Renders the text section for a flow node connector.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    public abstract void ConnectorTextSection(ImGui gui, FlowNodeConnector connector);

    /// <summary>
    /// Triggers a one-time flashing effect on a connector.
    /// </summary>
    /// <param name="connector">The flow node connector to flash.</param>
    /// <param name="context">The drawing context.</param>
    public abstract void FlashingOnce(FlowNodeConnector connector, IDrawContext? context);

    /// <summary>
    /// Queues a refresh request for the specified flow node.
    /// </summary>
    /// <param name="node">The flow node to refresh.</param>
    public abstract void QueueRefresh(FlowNode node);


    /// <summary>
    /// Creates a simple frame with optional text, image, and editor GUI content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="direction">The flow direction of the frame.</param>
    /// <param name="context">The drawing node context.</param>
    /// <param name="text">Optional text to display.</param>
    /// <param name="image">Optional image to display.</param>
    /// <param name="editorGui">Optional editor GUI callback.</param>
    /// <returns>An ImGui node representing the simple frame.</returns>
    public abstract ImGuiNode SimpleFrame(ImGui gui, FlowDirections direction, IDrawNodeContext context,
        string? text = null, ImageDef? image = null, DrawEditorImGui? editorGui = null);

    /// <summary>
    /// Creates a frame for a single connector with optional text, image, and editor GUI content.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="connector">The flow node connector.</param>
    /// <param name="context">The drawing node context.</param>
    /// <param name="text">Optional text to display.</param>
    /// <param name="image">Optional image to display.</param>
    /// <param name="editorGui">Optional editor GUI callback.</param>
    /// <returns>An ImGui node representing the single connector frame.</returns>
    public abstract ImGuiNode SingleConnectorFrame(ImGui gui, FlowNodeConnector connector, IDrawNodeContext context, 
        string? text = null, ImageDef? image = null, DrawEditorImGui? editorGui = null);
}
