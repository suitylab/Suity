using Suity.Views.NodeGraph;
using Suity.Editor.Flows;
using Suity.Views.Graphics;
using System.Drawing;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Base class for ImGUI-based graph nodes that provide rendering and interaction.
/// </summary>
public abstract class ImGraphNode : GraphNode, IDrawNodeContext
{
    // Global ID to be allocated
    private static ulong _allocId = 0;

    private string _graphId;
    protected ImGuiGraphControl? _panel;

    private readonly ImGuiNodeRef _guiNode = new();

    /// <summary>
    /// Gets the custom editor GUI drawing function, if any.
    /// </summary>
    public virtual DrawEditorImGui? EditorGui => null;


    /// <summary>
    /// Gets the ID of the underlying ImGui node.
    /// </summary>
    public string? ImGuiNodeId => _guiNode.Node?.Id;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGraphNode"/> class.
    /// </summary>
    /// <param name="x">The X position of the node.</param>
    /// <param name="y">The Y position of the node.</param>
    /// <param name="p_View">The graph diagram that contains this node.</param>
    /// <param name="p_CanBeSelected">Indicates whether the node can be selected.</param>
    /// <param name="p_CanBeDeleted">Indicates whether the node can be deleted.</param>
    public ImGraphNode(int x, int y, GraphDiagram p_View, bool p_CanBeSelected, bool p_CanBeDeleted)
        : base(p_View, x, y, p_CanBeSelected, p_CanBeDeleted)
    {
        // Increment the globally allocatable ID
        _allocId++;

        // Allocate a global ID, which can be modified later
        _name = _graphId = GetType().Name + _allocId;

        _panel = Diagram.ParentControl as ImGuiGraphControl;
    }

    /// <summary>
    /// Gets the underlying ImGUI node instance.
    /// </summary>
    /// <returns>The ImGuiNode instance, or null if not yet created.</returns>
    public ImGuiNode? GetImGuiNode() => _guiNode.Node;

    /// <summary>
    /// Enables double layout mode for the underlying ImGui node.
    /// </summary>
    public void DoubleLayout()
    {
        if (_guiNode.Node is { } node)
        {
            node.IsDoubleLayout = true;
        }
    }

    /// <summary>
    /// Builds and returns the ImGui node for this graph node.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The rendered ImGui node.</returns>
    public virtual ImGuiNode OnGui(ImGui gui)
    {
        _panel ??= Diagram.ParentControl as ImGuiGraphControl;

        var node = _guiNode.Node = OnNodeGui(gui);
        if (node.IsInitializing)
        {
            node.SetValue(this);
            node.SetPseudoActive(this.Highlighted);

            // Add input detection to implement filtered GUI sync
            node.InitInputFunctionChain(NodeInputDetector);

            // Double layout mechanism has been implemented at the underlying layer
            node.IsDoubleLayout = true;
        }

        // Forcing double layout causes child nodes to execute Align twice, resulting in position errors, so IsDoubleLayout is now implemented at the underlying layer.
        // Double layout: the first layout calculates size, the second layout handles internal alignment
        // Set align=false to avoid child node position offsets from multiple alignments
        // node.LayoutContents(true, false);

        // Force ending sync here to obtain the node's final size
        gui.EndCurrentNode();

        // Update size
        this.Width = (int)(node.Width?.Value ?? 0);
        this.Height = (int)(node.Height?.Value ?? 0);
        this.UpdateHitRectangle();

        return node;
    }

    /// <summary>
    /// Main entry point for node GUI sync. Must be implemented by derived classes.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <returns>The rendered ImGui node.</returns>
    protected abstract ImGuiNode OnNodeGui(ImGui gui);

    /// <inheritdoc/>
    protected override void OnHighlightedChanged()
    {
        // Set highlight style
        _guiNode.Node?.SetPseudoActive(this.Highlighted);
    }

    /// <inheritdoc/>
    protected override void OnPositionUpdated()
    {
        if (_guiNode.Node is { } node)
        {
            if (node.GetValue<GuiPositionValue>() is { } pos)
            {
                float ox = X - pos.Position.X;
                float oy = Y - pos.Position.Y;

                // Update position using deep offset
                node.OffsetPositionDeep(ox, oy);

                // Update position information
                node.SetPosition(X, Y);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnSizeUpdated()
    {
        if (_guiNode.Node is { } node)
        {
            // Update size information
            node.SetSize(Width, Height);
            // Re-layout
            node.Gui.LayoutNodeContent(node);
        }
    }

    /// <summary>
    /// Gets the computation instance associated with this node.
    /// </summary>
    public virtual IFlowComputation? Computation 
        => (Diagram?.ParentControl as ImGuiGraphControl)?.OwnerFlowView?.Computation;

    /// <summary>
    /// Gets the current computation state of this node.
    /// </summary>
    public virtual FlowComputationStates ComputationState => FlowComputationStates.None;

    /// <summary>
    /// Clears any cached rendering data for this node.
    /// </summary>
    public virtual void ClearCache()
    {
    }

    /// <summary>
    /// Renames a connector from the old name to the new name.
    /// </summary>
    /// <param name="oldName">The current name of the connector.</param>
    /// <param name="newName">The new name for the connector.</param>
    public virtual void RenameConnector(string oldName, string newName)
    {
    }

    #region Connector

    /// <inheritdoc/>
    public override RectangleF GetConnectorArea(GraphConnector connector)
    {
        if (connector.ConnectorType == ConnectorType.Associate)
        {
            // If the connection type is Associate, use the center point
            if (_guiNode.Node is { } node)
            {
                return node.GlobalRect;
            }
        }
        else
        {
            // If the connection type is Connect, use the connector point
            if (connector.Tag is ImGuiNodeRef { Node: { } node })
            {
                return node.GlobalRect;
            }
        }

        return RectangleF.Empty;
    }

    /// <inheritdoc/>
    public override RectangleF GetConnectorHitArea(GraphConnector connector) 
        => GetConnectorArea(connector);

    /// <inheritdoc/>
    public override PointF GetConnectorPosition(GraphConnector connector)
    {
        if (connector.ConnectorType == ConnectorType.Associate)
        {
            if (_guiNode.Node is { } node)
            {
                var rect = node.GlobalRect;

                float x = rect.X + rect.Width * 0.5f;
                float y = rect.Y + rect.Height * 0.5f;

                return new PointF(x, y);
            }
        }
        else
        {
            if (connector.Tag is ImGuiNodeRef { Node: { } node })
            {
                var rect = node.GlobalRect;

                float x = rect.X + rect.Width * 0.5f;
                float y = rect.Y + rect.Height * 0.5f;

                return new PointF(x, y);
            }
        }

        return PointF.Empty;
    } 

    #endregion

    #region IFlowNodeDrawContext

    /// <inheritdoc/>
    public virtual string Id => _graphId;

    /// <inheritdoc/>
    public virtual Color? BackgroundColor => null;

    /// <inheritdoc/>
    public virtual Color? TitleColor => null;

    /// <inheritdoc/>
    public virtual void DrawConnectors(IGraphicOutput output)
    {
    }

    /// <inheritdoc/>
    public virtual void DrawHeader(IGraphicOutput output, float zoom, Rectangle rect)
    {
    }

    /// <inheritdoc/>
    public virtual void DrawPanel(IGraphicOutput output, float zoom, Rectangle rect)
    {
    }

    /// <inheritdoc/>
    public virtual void DrawPreviewText(IGraphicOutput output, float zoom, Rectangle rect, string text)
    {
    }

    /// <inheritdoc/>
    public virtual void DrawShadow(IGraphicOutput output)
    {
    }

    #endregion

    /// <summary>
    /// Node input detection. If input is detected, starts filtered-mode GUI sync. In this mode, other nodes will not perform GUI sync and rendering.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline.</param>
    /// <param name="node">The ImGui node being processed.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call for child processing.</param>
    /// <returns>The input state indicating how to proceed.</returns>
    private GuiInputState NodeInputDetector(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (pipeline != GuiPipeline.Main)
        {
            return state;
        }

        // Current operated node sync mechanism to avoid repeatedly operating many other nodes
        if (state >= GuiInputState.PartialSync)
        {
            //Debug.WriteLine($"================== Node input detected : {this.Id}");
            _panel?.BeginPartialGuiSync(this);
        }
        else
        {
            //Debug.WriteLine($"================== ===================== {state}");
        }

        return state;
    }
}
