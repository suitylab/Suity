using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.NodeGraph;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// View node that renders a comment node in the flow diagram.
/// </summary>
internal class ImFlowCommentViewNode : ImGraphCommentNode, IFlowViewNode
{
    private readonly FlowNode _node;
    private readonly CommentFlowNode _commentNode;
    private readonly FlowNodeStyle _style;
    private readonly Dictionary<string, GraphConnector> _connectorDic = [];

    internal string _previewText;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImFlowCommentViewNode"/> class.
    /// </summary>
    /// <param name="groupNode">The flow node this view represents.</param>
    /// <param name="x">The initial X position on the diagram.</param>
    /// <param name="y">The initial Y position on the diagram.</param>
    /// <param name="view">The parent graph diagram view.</param>
    public ImFlowCommentViewNode(FlowNode groupNode, int x, int y, GraphDiagram view)
    : base(x, y, view, true, true)
    {
        _node = groupNode ?? throw new ArgumentNullException(nameof(groupNode));
        _commentNode = _node as CommentFlowNode;
        _style = FlowNodeStyle.GetStyle(groupNode.GetType());

        _width = groupNode.Width;
        _height = groupNode.Height;

        if (_width <= 0)
        {
            _width = 100;
        }

        if (_height <= 0)
        {
            _height = 100;
        }

        if (Node?.DiagramItem is { } diagramItem)
        {
            diagramItem.SetBound(new Rectangle(_x, _y, _width, _height));
        }

        UpdateHitRectangle();

        RebuildNode();

        groupNode.StartView(this);
    }

    /// <inheritdoc/>
    public override string CommentText 
    {
        get => _commentNode?.CommentText ?? string.Empty;
        set => _commentNode?.CommentText = value;
    }

    /// <inheritdoc/>
    public override Color? BackgroundColor => _commentNode?.BackgroundColor;

    #region IFlowViewNode
    /// <summary>
    /// Gets or sets the computation engine associated with this node.
    /// </summary>
    public IFlowComputation NodeComputation { get; set; }

    /// <summary>
    /// Gets the flow view that owns this node.
    /// </summary>
    public IFlowView FlowView => _panel?.OwnerFlowView;

    /// <summary>
    /// Gets the underlying flow node.
    /// </summary>
    public FlowNode Node => _node;

    /// <summary>
    /// Gets the expanded view for this node. Comment nodes do not support expanded views.
    /// </summary>
    public IDrawExpandedImGui ExpandedView => null;

    /// <summary>
    /// Gets a value indicating whether this node is expanded. Always returns <c>false</c> for comment nodes.
    /// </summary>
    public bool IsExpanded => false;

    /// <summary>
    /// Queues a refresh request for this node in the parent panel.
    /// </summary>
    public void QueueRefresh() => _panel?.RefreshNode(this);

    /// <summary>
    /// Rebuilds the node's connector information from the underlying flow node.
    /// </summary>
    /// <param name="removeLink">Optional callback to remove obsolete links.</param>
    public void RebuildNode(Action<NodeLink> removeLink = null)
    {
        var item = _node.DiagramItem as FlowDiagramItem;
        if (item is null)
        {
            return;
        }

        this.RebuildeViewNode(_node, _connectorDic, removeLink);
    }

    /// <summary>
    /// Sets the expand state of this node. No-op for comment nodes.
    /// </summary>
    /// <param name="expand">Whether to expand the node.</param>
    public void SetExpand(bool expand)
    {
    }

    /// <summary>
    /// Updates the position and size of this view node to match the underlying diagram item.
    /// </summary>
    public void UpdateBound()
    {
        if (Node?.DiagramItem is { } diagramItem)
        {
            _x = diagramItem.X;
            _y = diagramItem.Y;
            _width = Node.Width;
            _height = Node.Height;

            UpdateHitRectangle(true, true);
        }
    }

    /// <summary>
    /// Updates the position of this view node to match the underlying diagram item.
    /// </summary>
    public void UpdatePosition()
    {
        if (Node?.DiagramItem is { } diagramItem)
        {
            _x = diagramItem.X;
            _y = diagramItem.Y;

            UpdateHitRectangle(true, false);
        }
    }

    /// <summary>
    /// Updates the preview text displayed on this node.
    /// </summary>
    /// <param name="text">The new preview text.</param>
    public void UpdatePreviewText(string text)
    {
        if (_previewText != text)
        {
            _previewText = text;

            // After updating preview text, need to refresh view
            Diagram?.ParentControl?.RefreshNode(this);
        }
    }
    #endregion
}
