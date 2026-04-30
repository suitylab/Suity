using Suity.Editor.Documents.Linked;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow diagram document item, used for document storage. If not defined, it will be created automatically. Inherit from this type to add more features.
/// </summary>
public class FlowDiagramItem : SNamedItem, IFlowDiagramItem
{
    private FlowNode _node;
    private int _x;
    private int _y;
    private int _width;
    private int _height;

    private IFlowDiagram _diagram;

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem.
    /// </summary>
    public FlowDiagramItem()
    { }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with a node.
    /// </summary>
    public FlowDiagramItem(FlowNode node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        node.DiagramItem = this;
    }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with an asset builder.
    /// </summary>
    public FlowDiagramItem(AssetBuilder builder)
        : base(builder)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with an asset builder and node.
    /// </summary>
    public FlowDiagramItem(AssetBuilder builder, FlowNode node)
        : base(builder)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        node.DiagramItem = this;
    }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    public int X => _x;

    /// <summary>
    /// Gets the Y position.
    /// </summary>
    public int Y => _y;

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected internal new AssetBuilder AssetBuilder => base.AssetBuilder;


    /// <summary>
    /// Gets the bounding rectangle.
    /// </summary>
    public Rectangle Bound => new(_x, _y, _width, _height);

    /// <summary>
    /// Gets the display text.
    /// </summary>
    protected override string OnGetDisplayText()
    {
        return (Node as ITextDisplay)?.DisplayText ?? base.OnGetDisplayText();
    }

    /// <summary>
    /// Gets the icon.
    /// </summary>
    protected override Image OnGetIcon()
    {
        return (Node as ITextDisplay)?.DisplayIcon as Image ?? base.OnGetIcon();
    }


    /// <summary>
    /// Sets the position.
    /// </summary>
    public void SetPosition(int x, int y, bool notify = true)
    {
        _x = x;
        _y = y;

        if (notify)
        {
            NotifyPositionUpdated();
        }
    }

    /// <summary>
    /// Sets the size.
    /// </summary>
    public void SetSize(int width, int height, bool notify = true)
    {
        _width = width;
        _height = height;

        if (notify)
        {
            NotifySizeUpdated();
        }
    }

    /// <summary>
    /// Sets the bound.
    /// </summary>
    public void SetBound(Rectangle bound, bool notify = true)
    {
        _x = bound.X;
        _y = bound.Y;

        _width = bound.Width;
        _height = bound.Height;

        if (notify)
        {
            NotifyBoundUpdated();
        }
    }

    /// <summary>
    /// Sets the expanded state.
    /// </summary>
    public void SetExpanded(bool expanded)
    {
        if (_node is { } node)
        {
            foreach (var viewNode in node.ViewNodes)
            {
                viewNode.SetExpand(expanded);
            }

            OnSizeUpdated();
        }
    }

    /// <summary>
    /// Updates the preferred size.
    /// </summary>
    public void UpdatePreferredSize(int width, int height)
    {
        _width = width;
        _height = height;

        OnSizeUpdated();
    }


    /// <summary>
    /// Notifies that the position has been updated.
    /// </summary>
    public void NotifyPositionUpdated()
    {
        if (_node is { } node)
        {
            foreach (var viewNode in node.ViewNodes)
            {
                viewNode.UpdatePosition();
            }

            OnPositionUpdated();
        }
    }

    /// <summary>
    /// Notifies that the size has been updated.
    /// </summary>
    public void NotifySizeUpdated()
    {
        if (_node is { } node)
        {
            foreach (var viewNode in node.ViewNodes)
            {
                viewNode.UpdateBound();
            }

            OnSizeUpdated();
        }
    }

    /// <summary>
    /// Notifies that the bound has been updated.
    /// </summary>
    public void NotifyBoundUpdated()
    {
        if (_node is { } node)
        {
            foreach (var viewNode in node.ViewNodes)
            {
                viewNode.UpdateBound();
            }

            OnPositionUpdated();
            OnSizeUpdated();
        }
    }


    protected internal override string OnGetSuggestedPrefix() => $"###{Node?.TypeName ?? "Node"}";

    protected internal override bool OnVerifyName(string name) => true;

    #region IFlowDiagramItem

    /// <summary>
    /// Flow diagram node
    /// </summary>
    public FlowNode Node
    {
        get => _node;
        set
        {
            if (_node == value)
            {
                return;
            }

            _node?.DiagramItem = null;
            _node = value;
            _node?.DiagramItem = this;

            OnNodeReplaced();
        }
    }

    /// <summary>
    /// Flow diagram
    /// </summary>
    public IFlowDiagram Diagram
    {
        get => _diagram;
        set => _diagram = value;
    }

    /// <summary>
    /// Notifies that the node has been updated.
    /// </summary>
    public void NotifyNodeUpdated()
    {
        OnNodeUpdated();

        if (_diagram is { } diagram && _node is { } node)
        {
            diagram.NotifyNodeUpdated(node);
        }
    }

    /// <summary>
    /// Notify that name has been updated
    /// </summary>
    public void NotifyNameUpdated()
    {
        if (_node is null)
        {
            return;
        }

        if (Name != _node.Name)
        {
            Name = _node.Name;
            if (Name != _node.Name)
            {
                _node.Name = Name;
            }
        }
    }

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    public void RefreshView()
    {
        if (_node is { } node)
        {
            foreach (var viewNode in node.ViewNodes)
            {
                viewNode.QueueRefresh();
            }
        }
    }

    #endregion

    /// <summary>
    /// Synchronizes the item properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _x = sync.SyncInt32Attribute(nameof(X), _x);
        _y = sync.SyncInt32Attribute(nameof(Y), _y);
        _width = sync.SyncInt32Attribute(nameof(Width), _width);
        _height = sync.SyncInt32Attribute(nameof(Height), _height);

        _node = sync.Sync("Node", _node, SyncFlag.NotNull);

        if (sync.IsSetterOf("Node") && _node != null)
        {
            _node.Name = this.Name;
            _node.DiagramItem = this;
            OnNodeReplaced();
        }

        if (sync.IsSetterOf(nameof(X), nameof(Y)))
        {
            OnPositionUpdated();
        }

        if (sync.IsSetterOf(nameof(Width), nameof(Height)))
        {
            OnSizeUpdated();
        }
    }

    /// <summary>
    /// Called when the name is updated.
    /// </summary>
    protected override void OnNameUpdated(string oldName, string newName)
    {
        base.OnNameUpdated(oldName, newName);

        _node?.Name = newName;
    }

    /// <summary>
    /// Flow diagram node has been replaced
    /// </summary>
    protected virtual void OnNodeReplaced()
    { }

    /// <summary>
    /// Called when the item is added.
    /// </summary>
    protected override void OnAdded()
    {
        base.OnAdded();

        Node?.OnAdded();
    }

    /// <summary>
    /// Called when the item is removed.
    /// </summary>
    protected override void OnRemoved(NamedRootCollection model)
    {
        base.OnRemoved(model);

        Node?.OnRemoved();

        this.Diagram = null;
    }

    /// <summary>
    /// Called when the node is updated.
    /// </summary>
    protected virtual void OnNodeUpdated()
    {
    }



    /// <summary>
    /// Called when the position is updated.
    /// </summary>
    protected virtual void OnPositionUpdated()
    { }

    /// <summary>
    /// Called when the size is updated.
    /// </summary>
    protected virtual void OnSizeUpdated()
    {
    }

    #region Static

    /// <summary>
    /// Creates a flow diagram item for the specified node.
    /// </summary>
    public static FlowDiagramItem CreateDiagramItem(FlowNode node)
    {
        return FlowsExternal._external.CreateFlowDiagramItem(node);
    }

    #endregion
}

/// <summary>
/// Generic flow diagram item with typed node.
/// </summary>
public abstract class FlowDiagramItem<T> : FlowDiagramItem
    where T : FlowNode
{
    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem.
    /// </summary>
    public FlowDiagramItem()
    { }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with a node.
    /// </summary>
    public FlowDiagramItem(FlowNode node) : base(node)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with an asset builder.
    /// </summary>
    public FlowDiagramItem(AssetBuilder builder)
        : base(builder)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with an asset builder and node.
    /// </summary>
    public FlowDiagramItem(AssetBuilder builder, FlowNode node)
        : base(builder, node)
    {
    }

    /// <summary>
    /// Gets the typed node.
    /// </summary>
    public new T Node => base.Node as T;
}

/// <summary>
/// Generic flow diagram item with typed node and asset builder.
/// </summary>
public abstract class FlowDiagramItem<TNode, TBuilder> : FlowDiagramItem<TNode>
    where TNode : FlowNode
    where TBuilder : AssetBuilder, new()
{
    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem.
    /// </summary>
    public FlowDiagramItem()
        : base(new TBuilder())
    {
    }

    /// <summary>
    /// Initializes a new instance of the FlowDiagramItem with a node.
    /// </summary>
    public FlowDiagramItem(TNode node)
        : base(new TBuilder(), node)
    {
    }

    /// <summary>
    /// Gets the typed asset builder.
    /// </summary>
    protected internal new TBuilder AssetBuilder => (TBuilder)base.AssetBuilder;

    /// <summary>
    /// Called when the node is updated.
    /// </summary>
    protected override void OnNodeUpdated()
    {
        base.OnNodeUpdated();

        AssetBuilder?.NotifyUpdated();
    }
}