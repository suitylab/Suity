using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;
using System.Collections.Generic;
using System.Drawing;
using Suity.Drawing;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// View node that renders a group node in the flow diagram, containing child nodes.
/// </summary>
public class ImFlowGroupViewNode : ImGraphGroupNode, IFlowViewNode
{
    private readonly FlowNode _node;
    private readonly IGroupFlowNode _groupNode;
    private readonly FlowNodeStyle _style;
    private readonly Dictionary<string, GraphConnector> _connectorDic = [];

    internal string _previewText;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImFlowGroupViewNode"/> class.
    /// </summary>
    /// <param name="groupNode">The flow node this view represents.</param>
    /// <param name="x">The initial X position on the diagram.</param>
    /// <param name="y">The initial Y position on the diagram.</param>
    /// <param name="view">The parent graph diagram view.</param>
    public ImFlowGroupViewNode(FlowNode groupNode, int x, int y, GraphDiagram view)
        : base(x, y, view, true, true)
    {
        _node = groupNode ?? throw new ArgumentNullException(nameof(groupNode));
        _groupNode = _node as IGroupFlowNode;
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

    /// <summary>
    /// Gets the display name of the group.
    /// </summary>
    public override string GroupName
    {
        get
        {
            string? s = _groupNode?.GroupName;
            if (string.IsNullOrWhiteSpace(s))
            {
                s = _node.Name;
            }

            return s ?? string.Empty;
        }
    }

    #region Virtual

    /// <inheritdoc/>
    public override string Id => _name ?? base.Id;

    /// <inheritdoc/>
    public override bool HasHeader => _style?.HasHeader ?? base.HasHeader;

    /// <inheritdoc/>
    public override Color? BackgroundColor
    {
        get
        {
            if (_node.BackgroundColor is { } color && color != Color.Empty)
            {
                return color;
            }

            return _style?.BackgroundColor ?? EditorColorScheme.Default.Header;
        }
    }

    /// <inheritdoc/>
    public override Color? TitleColor => _node.TitleColor;

    /// <inheritdoc/>
    public override BrushDef NodeFillBrush
    {
        get
        {
            if (Node.TitleColor is Color c && c != Color.Empty)
            {
                return new SolidBrushDef(c);
            }

            return _style?.NodeFillBrush ?? base.NodeFillBrush;
        }
    }

    /// <inheritdoc/>
    public override BrushDef NodeHeaderFillBrush => _style?.NodeHeaderFillBrush ?? base.NodeHeaderFillBrush;
    /// <inheritdoc/>
    public override PenDef NodeOutlinePen => _style?.NodeOutlinePen ?? base.NodeOutlinePen;

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            string s = (Node as ITextDisplay)?.DisplayText;
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            s = Node?.ToString();
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            s = Node?.Name;
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public override string PreviewText => _previewText;
    /// <inheritdoc/>
    public override ImageDef Icon => _node.Icon;

    /// <inheritdoc/>
    public override void UpdateHitRectangle(bool notifyPosition = false, bool notifySize = false)
    {
        base.UpdateHitRectangle(notifyPosition, notifySize);

        if (Node?.DiagramItem is { } diagramItem)
        {
            diagramItem.SetSize(HitRectangle.Width, HitRectangle.Height, false);
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override ImGuiNode OnNodeGui(ImGui gui)
    {
        if (_node._flowNodeGui is { } customGui)
        {
            return customGui(gui, this)
                .InitValue<IFlowViewNode>(this);
        }
        else
        {
            return base.OnNodeGui(gui)
                .InitValue<IFlowViewNode>(this);
        }
    }

    /// <inheritdoc/>
    public override void HandleDoubleClick()
    {
        _node.OnDoubleClick();
    }

    /// <inheritdoc/>
    public override TextStatus DisplayStatus => _node.DisplayStatus;

    /// <inheritdoc/>
    public override DrawEditorImGui EditorGui => _node._editorGui;

    /// <inheritdoc/>
    public override FlowComputationStates ComputationState
        => Computation?.GetNodeRunningState(Node)?.State ?? FlowComputationStates.None;

    /// <inheritdoc/>
    public override void RenameConnector(string oldName, string newName)
    {
        var connector = _connectorDic.RemoveAndGet(oldName);
        if (connector != null)
        {
            connector.Name = newName;
            _connectorDic.Add(newName, connector);
        }
    }


    #region IFlowViewNode
    /// <inheritdoc/>
    public IFlowComputation NodeComputation { get; set; }


    /// <inheritdoc/>
    public IFlowView FlowView => _panel?.OwnerFlowView;

    /// <inheritdoc/>
    public FlowNode Node => _node;

    /// <inheritdoc/>
    public IDrawExpandedImGui ExpandedView => null;

    /// <inheritdoc/>
    public bool IsExpanded => false;

    /// <inheritdoc/>
    public void QueueRefresh() => _panel?.RefreshNode(this);

    /// <inheritdoc/>
    public void RebuildNode(Action<NodeLink> removeLink = null)
    {
        var item = _node.DiagramItem as FlowDiagramItem;
        if (item is null)
        {
            return;
        }

        this.RebuildeViewNode(_node, _connectorDic, removeLink);
    }

    /// <inheritdoc/>
    public void SetExpand(bool expand)
    {
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void UpdatePosition()
    {
        if (Node?.DiagramItem is { } diagramItem)
        {
            _x = diagramItem.X;
            _y = diagramItem.Y;

            UpdateHitRectangle(true, false);
        }
    }

    /// <inheritdoc/>
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
