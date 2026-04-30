using Suity.Views.NodeGraph;
using Suity.Editor.Flows;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Base class for group nodes that can contain and visually organize other nodes.
/// </summary>
public abstract class ImGraphGroupNode : ImGraphNode, IDrawNodeContext
{
    /// <summary>
    /// Default header color with slight transparency.
    /// </summary>
    public readonly Color DefaultHeaderColor = Color.FromArgb(40, 0, 0, 0);

    private readonly ImGuiNodeRef _nodeRef = new();
    private readonly ImGuiNodeRef _headerNodeRef = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGraphGroupNode"/> class.
    /// </summary>
    /// <param name="x">The X position of the node.</param>
    /// <param name="y">The Y position of the node.</param>
    /// <param name="view">The graph diagram that contains this node.</param>
    /// <param name="_canBeSelected">Indicates whether the node can be selected.</param>
    /// <param name="_canBeDeleted">Indicates whether the node can be deleted.</param>
    protected ImGraphGroupNode(int x, int y, GraphDiagram view, bool _canBeSelected, bool _canBeDeleted)
        : base(x, y, view, _canBeSelected, _canBeDeleted)
    {
        _width = 100;
        _height = 100;

        UpdateHitRectangle();
    }

    /// <inheritdoc/>
    public override bool IsGroup => true;

    /// <inheritdoc/>
    public override bool Resizable => true;

    /// <inheritdoc/>
    public override bool HasHeader => true;

    /// <inheritdoc/>
    public override RectangleF? GetHeaderArea()
    {
        // When node is expanded and content is visible, return Header area; when node content is not visible, return the entire node area
        if (_headerNodeRef.Node is { } node && node.GlobalScale >= ImGraphExtensions.NodeHiddenScale)
        {
            return node.GlobalRect;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the display name of the group.
    /// </summary>
    public abstract string GroupName { get; }
    

    /// <inheritdoc/>
    protected override ImGuiNode OnNodeGui(ImGui gui)
    {
        var editorGui = EditorGui;

        _nodeRef.Node = NodeGraphExternalBK.Instance.ResizableNodeFrame(gui, this)
        .SetSize(this._width, this._height)
        //.OverridePadding(5)
        .OnContent(n => 
        {
            if (_connectors.HasControlOuputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Output, editorGui);
            }

            _headerNodeRef.Node = NodeGraphExternalBK.Instance.HeaderFrame(gui, this, "title", true)
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() => 
            {
                gui.HorizontalLayout("#left")
                .InitFit()
                .OnContent(() => 
                {
                    if (Icon is { } icon)
                    {
                        gui.Image(icon).InitClass("icon");
                    }

                    if (GroupName is { } groupName && !string.IsNullOrWhiteSpace(groupName))
                    {
                        gui.Text(groupName).InitClass("titleText");
                    }

                    editorGui?.Invoke(gui, EditorImGuiPipeline.Preview, this);
                });
            });

            if (_connectors.HasControlInputConnector)
            {
                gui.VerticalLayout("#dummy")
                .InitHeightRest(10);

                OnControlConnectorGui(gui, GraphDirection.Input, editorGui);
            }
        });

        return _nodeRef.Node;
    }

    /// <summary>
    /// Draws the control connectors (execution flow) for the specified direction.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="direction">The direction of the connectors (Input or Output).</param>
    /// <param name="editorGui">Optional custom editor GUI drawing function.</param>
    private void OnControlConnectorGui(ImGui gui, GraphDirection direction, DrawEditorImGui? editorGui)
    {
        gui.HorizontalLayout("#control-" + direction.ToString())
        .OnInitialize(n =>
        {
            n.InitClass("debug");
            n.InitFit();
            n.InitHorizontalAlignment(GuiAlignment.Center);
            n.InitPadding(1);
        })
        .OnContent(() =>
        {
            var pipeline = direction == GraphDirection.Input ? EditorImGuiPipeline.Input : EditorImGuiPipeline.Output;

            if (editorGui is null || !editorGui(gui, pipeline, this))
            {
                var connectors = _connectors.Where(o =>
                    o.ConnectorType == ConnectorType.Control &&
                    o.Direction == direction);

                foreach (var connector in connectors)
                {
                    NodeGraphExternalBK.Instance.ConnectorPoint(gui, connector, connector.Name);
                }
            }
        });
    }

    /// <inheritdoc/>
    public override Color? TitleColor => DefaultHeaderColor;
}
