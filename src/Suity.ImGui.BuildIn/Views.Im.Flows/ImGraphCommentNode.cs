using Suity.Editor.Flows;
using Suity.Views.NodeGraph;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.Flows;

/// <summary>
/// Base class for comment nodes in the graph that display editable text annotations.
/// </summary>
public abstract class ImGraphCommentNode : ImGraphNode, IDrawNodeContext
{
    /// <summary>
    /// Default header color with slight transparency.
    /// </summary>
    public readonly Color DefaultHeaderColor = Color.FromArgb(40, 0, 0, 0);

    private readonly ImGuiNodeRef _nodeRef = new();
    private readonly ImGuiNodeRef _CommentNodeRef = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGraphCommentNode"/> class.
    /// </summary>
    /// <param name="x">The X position of the node.</param>
    /// <param name="y">The Y position of the node.</param>
    /// <param name="view">The graph diagram that contains this node.</param>
    /// <param name="_canBeSelected">Indicates whether the node can be selected.</param>
    /// <param name="_canBeDeleted">Indicates whether the node can be deleted.</param>
    protected ImGraphCommentNode(int x, int y, GraphDiagram view, bool _canBeSelected, bool _canBeDeleted)
        : base(x, y, view, _canBeSelected, _canBeDeleted)
    {
        _width = 100;
        _height = 100;

        UpdateHitRectangle();
    }

    /// <inheritdoc/>
    public override bool Resizable => true;

    /// <inheritdoc/>
    public override bool HasHeader => false;

    //public override RectangleF? GetHeaderArea()
    //{
    //    // When node is expanded and content is visible, return Header area; when node content is not visible, return the entire node area
    //    if (_CommentNodeRef.Node is { } node && node.GlobalScale >= ImGraphExtensions.NodeHiddenScale)
    //    {
    //        return node.GlobalRect;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    /// <summary>
    /// Gets or sets the comment text displayed in the node.
    /// </summary>
    public abstract string CommentText { get; set; }


    /// <inheritdoc/>
    protected override ImGuiNode OnNodeGui(ImGui gui)
    {
        var editorGui = EditorGui;

        var color = BackgroundColor;
        if (color == Color.Empty)
        {
            color = null;
        }

        var node = _nodeRef.Node = NodeGraphExternalBK.Instance.ResizableNodeFrame(gui, this)
        .SetSize(this._width, this._height)
        .OverridePadding(5)
        .OverrideColor(color)
        .OnContent(n =>
        {
            if (_connectors.HasControlOuputConnector)
            {
                OnControlConnectorGui(gui, GraphDirection.Output, editorGui);
            }

            _CommentNodeRef.Node = gui.DoubleClickTextAreaInput("#comment", CommentText)
            .InitSizeRest()
            .InitOverrideBorder(0)
            .OverrideColor(color)
            .OnEdited(n => 
            {
                CommentText = n.Text ?? string.Empty;
            });

            if (_connectors.HasControlInputConnector)
            {
                gui.VerticalLayout("#dummy")
                .InitHeightRest(10);

                OnControlConnectorGui(gui, GraphDirection.Input, editorGui);
            }
        });

        return node;
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
