using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Node bound to FlowNode
/// </summary>
public class ImFlowViewNode : ImExpandableNode,
    IFlowViewNode, 
    IDrawNodeContext
{
    private readonly FlowNode _node;
    private readonly FlowNodeStyle _style;
    private readonly Dictionary<string, GraphConnector> _connectorDic = [];

    internal string _previewText;
    internal FlowDocument _cachedDoc;

    /// <summary>
    /// Gets the flow view that owns this node.
    /// </summary>
    public IFlowView FlowView => _panel?.OwnerFlowView;

    /// <summary>
    /// Gets the underlying flow node.
    /// </summary>
    public FlowNode Node => _node;

    /// <summary>
    /// Gets or sets the computation engine associated with this node.
    /// </summary>
    public IFlowComputation NodeComputation { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImFlowViewNode"/> class.
    /// </summary>
    /// <param name="node">The flow node this view represents.</param>
    /// <param name="x">The initial X position on the diagram.</param>
    /// <param name="y">The initial Y position on the diagram.</param>
    /// <param name="view">The parent graph diagram view.</param>
    public ImFlowViewNode(FlowNode node, int x, int y, GraphDiagram view)
         : base(x, y, view, true, true)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));

        _name = node.Name;

        _style = FlowNodeStyle.GetStyle(node.GetType());

        var width = _style?.Width;
        if (width.HasValue)
        {
            this.Width = width.Value;
        }

        var height = _style?.Height;
        if (height.HasValue)
        {
            this.Height = height.Value;
        }

        UpdateHitRectangle();

        RebuildNode();

        node.StartView(this);
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
    public override Brush NodeFillBrush
    {
        get
        {
            if (_node.TitleColor is Color c && c != Color.Empty)
            {
                return new SolidBrush(c);
            }

            return _style?.NodeFillBrush ?? base.NodeFillBrush;
        }
    }

    /// <inheritdoc/>
    public override Brush NodeHeaderFillBrush => _style?.NodeHeaderFillBrush ?? base.NodeHeaderFillBrush;
    /// <inheritdoc/>
    public override Pen NodeOutlinePen => _style?.NodeOutlinePen ?? base.NodeOutlinePen;

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            string s = (_node as ITextDisplay)?.DisplayText;
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            s = _node.ToString();
            if (!string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            s = _node.Name;
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
    public override Image Icon => _node.Icon;

    /// <inheritdoc/>
    public override bool RenderMutiple(GraphDirection type)
    {
        if (type == GraphDirection.Input)
        {
            return _style?.RenderInputMultiple == true;
        }
        else
        {
            return _style?.RenderOutputMultiple == true;
        }
    }

    /// <inheritdoc/>
    public override void UpdateHitRectangle(bool notifyPosition = false, bool notifySize = false)
    {
        base.UpdateHitRectangle(notifyPosition, notifySize);

        if (Node?.DiagramItem is { } diagramItem)
        {
            if (Node is IDrawExpandedImGui draw)
            {
                if (!draw.ResizableOnExpand)
                {
                    diagramItem.SetSize(HitRectangle.Width, HitRectangle.Height, false);
                }
            }
            else
            {
                diagramItem.SetSize(HitRectangle.Width, HitRectangle.Height, false);
            }
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override ImGuiNode OnNodeGui(ImGui gui)
    {
        if (_node.FlowNodeGui is { } customGui)
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
    public override IInspectorContext InspectorContext
        => _node as IInspectorContext ??
        _cachedDoc?.View as IInspectorContext;

    /// <inheritdoc/>
    public override FlowComputationStates ComputationState
        => Computation?.GetNodeRunningState(_node)?.State ?? FlowComputationStates.None;

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

    /// <inheritdoc/>
    protected override IDrawExpandedImGui CreateExpandedView()
    {
        if (_node is null)
        {
            return null;
        }

        // If the node supports it, use the node's own implementation
        if (_node is IDrawExpandedImGui view)
        {
            return view;
        }

        // Create expanded view by node type
        var gui = DrawExpandedImGuiResolver.Instance.CreateView(_node.GetType());
        if (gui != null)
        {
            return gui;
        }

        // Create expanded view by node's view object
        if (_node.ExpandedViewObject is { } obj)
        {
            gui = DrawExpandedImGuiResolver.Instance.CreateView(obj.GetType());
            if (gui != null)
            {
                return gui;
            }
        }

        return base.CreateExpandedView();
    }


    #region Expand & Resize

    /// <inheritdoc/>
    public override bool IsExpanded
    {
        get => _node.IsExpanded;
        set
        {
            if (_node.IsExpanded != value)
            {
                _node.IsExpanded = value;
                _cachedDoc?.MarkDirty(this);
            }
        }
    }

    /// <inheritdoc/>
    public override bool Expandable => _node.Expandable;

    /// <inheritdoc/>
    public override object ExpandedObject => _node;

    /// <inheritdoc/>
    public override Size PreferredSize => new(_node.Width, _node.Height);

    /// <inheritdoc/>
    public override void UpdatePreferredSize()
    {
        if (_node.DiagramItem is { } item && (item.Width != _width || item.Height != _height))
        {
            item.UpdatePreferredSize(_width, _height);
            _cachedDoc?.MarkDirty(this);
        }
    }

    #endregion

    /// <summary>
    /// Rebuilds the node's connector information from the underlying flow node.
    /// </summary>
    /// <param name="removeLink">Optional callback to remove obsolete links when connectors are removed.</param>
    public void RebuildNode(Action<NodeLink> removeLink = null)
    {
        var item = _node.DiagramItem as FlowDiagramItem;
        if (item is null)
        {
            return;
        }

        _cachedDoc = item.GetDocument() as FlowDocument;

        this.RebuildeViewNode(_node, _connectorDic, removeLink);
    }

    /// <summary>
    /// Updates the position of this view node to match the underlying diagram item.
    /// </summary>
    public void UpdatePosition()
    {
        if (_node.DiagramItem is { } diagramItem)
        {
            _x = diagramItem.X;
            _y = diagramItem.Y;

            UpdateHitRectangle(true, false);
        }
    }

    /// <summary>
    /// Updates the position and size of this view node to match the underlying diagram item.
    /// </summary>
    public void UpdateBound()
    {
        if (_node.DiagramItem is { } diagramItem)
        {
            _x = diagramItem.X;
            _y = diagramItem.Y;
            _width = _node.Width;
            _height = _node.Height;

            UpdateHitRectangle(true, true);
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

    /// <summary>
    /// Queues a refresh request for this node in the parent panel.
    /// </summary>
    public void QueueRefresh() => _panel?.RefreshNode(this);
}