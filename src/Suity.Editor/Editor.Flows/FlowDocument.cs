using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow chart document
/// </summary>
public abstract class FlowDocument : DesignDocument
{
    /// <summary>
    /// Default grid span value.
    /// </summary>
    public const int DefaultGridSpan = 200;

    /// <summary>
    /// Minimum grid span value.
    /// </summary>
    public const int MinGridSpan = 10;

    internal FlowDocumentExternal _ex;

    /// <summary>
    /// Initializes a new instance of the FlowDocument.
    /// </summary>
    public FlowDocument()
    {
        _ex = FlowsExternal._external.CreateFlowDocumentEx(this);
    }

    /// <summary>
    /// Initializes a new instance of the FlowDocument with an asset builder.
    /// </summary>
    public FlowDocument(AssetBuilder builder)
        : base(builder)
    {
        _ex = FlowsExternal._external.CreateFlowDocumentEx(this);
    }

    /// <summary>
    /// Gets or sets the grid span for the diagram.
    /// </summary>
    public int GridSpan
    {
        get => _ex.GridSpan;
        set => _ex.GridSpan = value;
    }

    /// <summary>
    /// Gets the link collection for the diagram.
    /// </summary>
    public LinkCollection Links => _ex.Links;

    /// <summary>
    /// Gets the flow diagram.
    /// </summary>
    public IFlowDiagram Diagram => _ex.Diagram;

    #region Sync

    /// <summary>
    /// Synchronizes the document properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _ex.Sync(sync, context);
    }

    #endregion

    #region DiagramItem

    /// <summary>
    /// Gets the number of items in the document.
    /// </summary>
    public int ItemCount => ItemCollection.Count;

    /// <summary>
    /// Gets the flow diagram item storage by name
    /// </summary>
    /// <param name="name">The name of the diagram item</param>
    /// <returns>The flow diagram item</returns>
    public IFlowDiagramItem GetDiagramItem(string name) => ItemCollection.GetItemAll(name) as FlowDiagramItem;

    /// <summary>
    /// Gets all flow diagram item storage
    /// </summary>
    public IEnumerable<IFlowDiagramItem> DiagramItems => ItemCollection.AllItems.OfType<FlowDiagramItem>();


    #endregion

    #region FlowNode

    /// <summary>
    /// Gets all flow nodes in the document.
    /// </summary>
    public IEnumerable<FlowNode> FlowNodes => DiagramItems.Select(o => o.Node).SkipNull();

    /// <summary>
    /// Gets a flow node by name.
    /// </summary>
    public FlowNode GetFlowNode(string name) => GetDiagramItem(name)?.Node;

    /// <summary>
    /// Adds a flow node
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <param name="rect">Optional rectangle for positioning</param>
    /// <returns>Returns the flow diagram item storage</returns>
    public IFlowDiagramItem AddFlowNode(FlowNode node, Rectangle? rect = null) => _ex.AddFlowNode(node, rect);

    /// <summary>
    /// Removes a flow node
    /// </summary>
    /// <param name="node">The node to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveFlowNode(FlowNode node) => _ex.RemoveFlowNode(node);

    /// <summary>
    /// Removes a flow node by item
    /// </summary>
    /// <param name="item">The diagram item to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveFlowNode(IFlowDiagramItem item) => _ex.RemoveFlowNode(item);

    #endregion

    #region Connector

    /// <summary>
    /// Gets whether the connector has connections
    /// </summary>
    /// <param name="connector">The connector to check</param>
    /// <returns>True if the connector is linked</returns>
    public bool GetIsConnectorLinked(FlowNodeConnector connector)
        => _ex.GetIsConnectorLinked(connector);

    /// <summary>
    /// Gets the number of connections for a connector
    /// </summary>
    /// <param name="connector">The connector</param>
    /// <returns>The number of connections</returns>
    public int GetLinkedConnectorCount(FlowNodeConnector connector)
        => _ex.GetLinkedConnectorCount(connector);

    /// <summary>
    /// Gets the first linked connector
    /// </summary>
    /// <param name="connector">Local connector</param>
    /// <returns>The linked connector, or null</returns>
    public FlowNodeConnector GetLinkedConnector(FlowNodeConnector connector)
        => _ex.GetLinkedConnector(connector);

    /// <summary>
    /// Gets linked connectors
    /// </summary>
    /// <param name="connector">Local connector</param>
    /// <param name="sort">Whether to sort by spatial order</param>
    /// <returns>Array of linked connectors</returns>
    public FlowNodeConnector[] GetLinkedConnectors(FlowNodeConnector connector, bool sort)
        => _ex.GetLinkedConnectors(connector, sort);

    /// <summary>
    /// Collects all invalid connectors.
    /// </summary>
    public List<NodeLink> CollectInvalidConnectors(bool report = true)
        => _ex.CollectInvalidConnectors(report);


    /// <summary>
    /// Flushes any queued connections for each node in the diagram items.
    /// </summary>
    /// <remarks>This method iterates through all diagram items and attempts to flush queued connections for
    /// each item's node. If an error occurs during the flush operation, it is logged for further
    /// investigation.</remarks>
    public void FlushQueuedConnection()
    {
        _ex.OnLoaded();

        foreach (var item in DiagramItems)
        {
            try
            {
                item.Node?.FlushQueuedConnection();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    #endregion

    #region Virtual

    /// <summary>
    /// Real-time preview of computation results
    /// </summary>
    public virtual bool PreviewComputeEnabled => true;

    /// <summary>
    /// Called when the document is loaded.
    /// </summary>
    protected internal override void OnLoaded(DocumentLoadingIntent intent)
    {
        base.OnLoaded(intent);

        _ex.OnLoaded(intent);

        // Do not execute RemoveInvalidConnectors during loading,
        // because some dynamic connectors cannot be created at initialization time.
    }

    /// <summary>
    /// Called when the document is saved.
    /// </summary>
    protected internal override bool SaveDocument(IStorageItem op)
    {
        _ex.RemoveInvalidConnectors();

        return base.SaveDocument(op);
    }

    /// <summary>
    /// Determines if a node type can be created.
    /// </summary>
    protected virtual bool GetCanCreateNode(Type type) => true;

    internal void InternalOnLinkAdded(NodeLink link)
    {
        GetDiagramItem(link.FromNode)?.Node?.OnLinkUpdated();
        GetDiagramItem(link.ToNode)?.Node?.OnLinkUpdated();

        OnLinkAdded(link);
    }

    internal void InternalOnLinkRemoved(NodeLink link)
    {
        GetDiagramItem(link.FromNode)?.Node?.OnLinkUpdated();
        GetDiagramItem(link.ToNode)?.Node?.OnLinkUpdated();

        OnLinkRemoved(link);
    }

    /// <summary>
    /// Called when a link is added.
    /// </summary>
    protected internal virtual void OnLinkAdded(NodeLink link)
    { }

    /// <summary>
    /// Called when a link is removed.
    /// </summary>
    protected internal virtual void OnLinkRemoved(NodeLink link)
    { }

    /// <summary>
    /// Called when the view is shown.
    /// </summary>
    protected internal override void OnShowView()
    {
        base.OnShowView();

        var diagramView = View.GetService<IFlowView>()?.Diagram;
        if (diagramView != null)
        {
            diagramView.GridSpan = _ex.GridSpan; ;
        }
    }

    /// <summary>
    /// Called when an item is added.
    /// </summary>
    protected internal override void OnItemAdded(SNamedRootCollection items, NamedItem item, bool isNew)
    {
        base.OnItemAdded(items, item, isNew);

        if (item is FlowDiagramItem flowItem)
        {
            flowItem.Diagram = _ex.Diagram;

            if (flowItem.Node is { } node)
            {
                OnFlowNodeAdded(node, isNew);
            }
            else
            {
                Logs.LogError($"Flow node not found: {item.Name}. Document: {this.FileName}");
            }
        }
    }

    /// <summary>
    /// Called when an item is removed.
    /// </summary>
    protected internal override void OnItemRemoved(SNamedRootCollection items, NamedItem item)
    {
        base.OnItemRemoved(items, item);

        if (item is FlowDiagramItem flowItem)
        {
            flowItem.Diagram = null;

            if (flowItem.Node is { } node)
            {
                OnFlowNodeRemoved(node);
            }
        }
    }

    /// <summary>
    /// Called when a flow node is added.
    /// </summary>
    protected virtual void OnFlowNodeAdded(FlowNode node, bool isNew)
    {
        // Notify view to update
        _ex.Diagram?.NotifyNodeUpdated(node);
    }

    /// <summary>
    /// Called when a flow node is removed.
    /// </summary>
    protected virtual void OnFlowNodeRemoved(FlowNode node)
    {
        // Notify view to update, should automatically check if Diagram is null to enter deletion flow
        _ex.Diagram?.NotifyNodeUpdated(node);
    }

    /// <summary>
    /// Reverse data flow
    /// </summary>
    public virtual bool ReverseDataFlow => false;


    #endregion

    #region Functions

    /// <summary>
    /// Gets the bounding rectangle of all diagram items.
    /// </summary>
    public Rectangle GetBound()
        => GetBound(ItemCollection.AllItems.OfType<IFlowDiagramItem>());

    /// <summary>
    /// Gets the bounding rectangle of the specified items.
    /// </summary>
    public static Rectangle GetBound(IEnumerable<IFlowDiagramItem> items)
    {
        var items2 = items?.SkipNull() ?? [];
        if (!items2.Any())
        {
            return Rectangle.Empty;
        }

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var item in items2)
        {
            minX = Math.Min(minX, item.X);
            minY = Math.Min(minY, item.Y);
            maxX = Math.Max(maxX, item.X + item.Width);
            maxY = Math.Max(maxY, item.Y + item.Height);
        }

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    #endregion

    #region Factory

    /// <summary>
    /// Gets the flow node creation list. After user selection, creates the node based on <see cref="SelectionResult.SelectedKey"/> string and calls <see cref="CreateFlowNode(string)"/>.
    /// Override to implement custom creation list.
    /// </summary>
    /// <returns>The selection list for node creation</returns>
    public virtual ISelectionList GetFactoryNodeList() => EmptySelectionList.Empty;

    /// <summary>
    /// Gets the data style definition for the specified type
    /// </summary>
    /// <param name="dataType">The data type name</param>
    /// <returns>The data style, or null</returns>
    public virtual IFlowDataStyle GetDataStyle(string dataType) => null;

    /// <summary>
    /// Gets whether to revert data array.
    /// </summary>
    public virtual bool RevertDataArray => !ReverseDataFlow;

    /// <summary>
    /// Creates a flow node based on <paramref name="key"/>.
    /// By default, resolves the <see cref="FlowNode"/> type using <paramref name="key"/> as the type name and attempts to create it.
    /// Override this method to use additional ways to create <see cref="FlowNode"/>.
    /// </summary>
    /// <param name="key">The key to resolve the node type</param>
    /// <returns>The created flow node, or null</returns>
    public virtual FlowNode CreateFlowNode(string key)
    {
        Type nodeType = key?.ResolveType();
        if (nodeType is null)
        {
            return null;
        }

        if (!GetCanCreateNode(nodeType))
        {
            return null;
        }

        FlowNode node = (FlowNode)nodeType.CreateInstanceOf();

        return node;
    }

    /// <summary>
    /// Creates a flow diagram item storage.
    /// By default, uses standard <see cref="FlowDiagramItem"/> to create storage items.
    /// Override this method to use additional ways to create <see cref="IFlowDiagramItem"/>.
    /// </summary>
    /// <param name="node">Flow node</param>
    /// <returns>The created diagram item</returns>
    public virtual IFlowDiagramItem CreateDiagramItem(FlowNode node)
    {
        return FlowDiagramItem.CreateDiagramItem(node);
    }

    /// <summary>
    /// Gets the preview text for a value.
    /// </summary>
    public virtual string GetValuePreviewText(object value)
    {
        return value?.ToString() ?? string.Empty;
    }

    #endregion
}

/// <summary>
/// Generic flow document with typed asset builder.
/// </summary>
public abstract class FlowDocument<TAssetBuilder> : FlowDocument
     where TAssetBuilder : AssetBuilder, new()
{
    /// <summary>
    /// Initializes a new instance of the FlowDocument.
    /// </summary>
    public FlowDocument() : base(new TAssetBuilder())
    {
    }

    /// <summary>
    /// Gets the typed asset builder.
    /// </summary>
    protected internal new TAssetBuilder AssetBuilder => (TAssetBuilder)base.AssetBuilder;
}