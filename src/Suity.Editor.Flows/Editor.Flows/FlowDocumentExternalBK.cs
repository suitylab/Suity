using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Expressions;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Enhanced implementation of the flows external with additional features.
/// </summary>
internal class FlowsExternalBK : FlowsExternal
{
    /// <summary>
    /// Gets the singleton instance of the flows external.
    /// </summary>
    public static readonly FlowsExternalBK Instance = new();

    /// <summary>
    /// Initializes the flows external by setting it as the active external implementation.
    /// </summary>
    public void Initialize()
    {
        FlowsExternal._external = this;
    }

    /// <inheritdoc/>
    public override FlowDiagramItem CreateFlowDiagramItem(FlowNode node)
    {
        if (node is null)
        {
            return null;
        }

        return FlowDiagramItemResolver.Instance.CreateNode(node);
    }

    /// <inheritdoc/>
    public override FlowDocumentExternal CreateFlowDocumentEx(FlowDocument document)
    {
        FlowConnectorAliasManager.Initialize();

        return new FlowDocumentExternalBK(document);
    }

    /// <inheritdoc/>
    public override void CollectOutputNetwork(FlowDocument document, FlowNode node, HashSet<FlowNode> collection)
    {
        if (node is null)
        {
            return;
        }

        if (!collection.Add(node))
        {
            return;
        }

        foreach (var output in node.Connectors.Where(o => o.Direction == FlowDirections.Output))
        {
            foreach (var input in document.GetLinkedConnectors(output, false))
            {
                var inputNode = input.ParentNode;
                CollectOutputNetwork(document, inputNode, collection);
            }
        }
    }

    /// <inheritdoc/>
    public override void CollectInputNetwork(FlowDocument document, FlowNode node, HashSet<FlowNode> collection)
    {
        if (node is null)
        {
            return;
        }

        if (!collection.Add(node))
        {
            return;
        }

        foreach (var input in node.Connectors.Where(o => o.Direction == FlowDirections.Input))
        {
            foreach (var output in document.GetLinkedConnectors(input, false))
            {
                var inputNode = output.ParentNode;
                CollectOutputNetwork(document, inputNode, collection);
            }
        }
    }

    /// <inheritdoc/>
    public override void SetFlowAutoValue(SObject obj, FlowDiagramItem item, PositionAutomationMode mode)
    {
        var doc = item?.GetDocument() as FlowDocument;
        if (doc is null)
        {
            return;
        }

        if (mode == PositionAutomationMode.None)
        {
            return;
        }

        var type = obj.ObjectType?.Target;
        if (type is null)
        {
            return;
        }

        int gridSpan = doc.GridSpan;

        foreach (var field in type.Fields.OfType<DStructField>())
        {
            var autoField = field.AutoFieldType;
            if (autoField.HasValue)
            {
                switch (autoField.Value)
                {
                    case AutoFieldType.X:
                        obj.SetProperty(field.Name, item.X);
                        obj.GetPropertyFormatted(field.Name);
                        break;

                    case AutoFieldType.Y:
                        obj.SetProperty(field.Name, item.Y);
                        obj.GetPropertyFormatted(field.Name);
                        break;

                    case AutoFieldType.GridX:
                        if (gridSpan > 0)
                        {
                            obj.SetProperty(field.Name, (int)Math.Floor(item.X / (double)gridSpan));
                            obj.GetPropertyFormatted(field.Name);
                        }
                        else
                        {
                            obj.SetProperty(field.Name, 0);
                            obj.GetPropertyFormatted(field.Name);
                        }
                        break;

                    case AutoFieldType.GridY:
                        if (gridSpan > 0)
                        {
                            obj.SetProperty(field.Name, (int)Math.Floor(item.Y / (double)gridSpan));
                            obj.GetPropertyFormatted(field.Name);
                        }
                        else
                        {
                            obj.SetProperty(field.Name, 0);
                            obj.GetPropertyFormatted(field.Name);
                        }
                        break;
                }
            }
        }
    }

    /// <inheritdoc/>
    public override IFlowComputation CreateComputation(FlowDocument document, FunctionContext context = null)
    {
        return new FlowComputation(context);
    }

    /// <inheritdoc/>
    public override string ResolveConnectorName(Type nodeType, string aliasName, out bool renamed, out bool resolved)
    {
        return FlowConnectorAliasManager.ResolveName(nodeType, aliasName, out renamed, out resolved);
    }
}

/// <summary>
/// Extended external implementation for flow documents, managing links, diagrams, and node operations.
/// </summary>
internal class FlowDocumentExternalBK : FlowDocumentExternal
{
    private readonly FlowDocument _document;
    private readonly FlowDocumentDiagram _diagram;

    private readonly LinkCollectionBK _links;
    private int _gridSpan = FlowDocument.DefaultGridSpan;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowDocumentExternalBK"/> class.
    /// </summary>
    /// <param name="document">The flow document to manage.</param>
    public FlowDocumentExternalBK(FlowDocument document)
    {
        _links = new LinkCollectionBK(this);

        _document = document ?? throw new ArgumentNullException(nameof(document));
        _diagram = new FlowDocumentDiagram(_document);

        Setup();
    }

    private void Setup()
    {
        _links.LinkAdded += (sender, e) => InternalOnLinkAdded(e.Value);
        _links.LinkRemoved += (sender, e) => InternalOnLinkRemoved(e.Value);

        _document.ItemCollection.FieldName = "Nodes";
        _document.ItemCollection.FieldDescription = "Node";
        _document.ItemCollection.FieldIcon = CoreIconCache.Flow;

        _document.PreviewFieldName = "Preview";

        _document.ItemCollection.AddItemType<FlowDiagramItem>("Node");
    }

    /// <summary>
    /// Gets the flow document managed by this external.
    /// </summary>
    public FlowDocument Document => _document;

    /// <inheritdoc/>
    public override IFlowDiagram Diagram => _diagram;

    /// <inheritdoc/>
    public override int GridSpan
    {
        get => _gridSpan;
        set
        {
            if (value < FlowDocument.MinGridSpan)
            {
                value = FlowDocument.MinGridSpan;
            }

            if (_gridSpan == value)
            {
                return;
            }

            _gridSpan = value;

            var diagramView = _document.View.GetService<IFlowView>()?.Diagram;
            if (diagramView != null)
            {
                diagramView.GridSpan = value;
            }
        }
    }

    /// <inheritdoc/>
    public override LinkCollection Links => _links;

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        GridSpan = sync.Sync(nameof(GridSpan), GridSpan, SyncFlag.None, FlowDocument.DefaultGridSpan);
        sync.Sync("Links", _links, SyncFlag.GetOnly | SyncFlag.NotNull);
    }

    /// <inheritdoc/>
    public override IFlowDiagramItem GetFlowItem(string name)
    {
        return _document.ItemCollection.GetItemAll(name) as FlowDiagramItem;
    }

    /// <inheritdoc/>
    public override IFlowDiagramItem AddFlowNode(FlowNode node, Rectangle? rect = null)
    {
        IFlowDiagramItem item = node.DiagramItem;
        if (item is null)
        {
            item = _document.CreateDiagramItem(node);
            if (item is null)
            {
                throw new NullReferenceException("Create node item failed.");
            }
        }

        if (rect is { } aRect)
        {
            item.SetBound(aRect);
        }

        var nLinkedItem = item as SNamedItem
            ?? throw new InvalidOperationException("CreateNodeItem must return a NLinkedItem");

        nLinkedItem.Name = node.Name;
        _document.ItemCollection.AddItem(nLinkedItem);

        return item;
    }

    /// <inheritdoc/>
    public override bool RemoveFlowNode(FlowNode node)
    {
        if (node?.DiagramItem is not FlowDiagramItem item)
        {
            return false;
        }

        return _document.ItemCollection.RemoveItem(item);
    }

    /// <inheritdoc/>
    public override bool RemoveFlowNode(IFlowDiagramItem item)
    {
        if (item is not FlowDiagramItem f)
        {
            return false;
        }

        return _document.ItemCollection.RemoveItem(f);
    }

    /// <inheritdoc/>
    public override bool GetIsConnectorLinked(FlowNodeConnector connector)
    {
        if (connector is null || connector.ParentNode is null)
        {
            return false;
        }

        IEnumerable<NodeLink> links;
        if (connector.Direction == FlowDirections.Input)
        {
            links = _links.GetLinksByConnectorFrom(connector.ParentNode.Name, connector.Name);

            if (links != null)
            {
                return links
                    .Select(link => GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector))
                    .SkipNull()
                    .Any();
            }
        }
        else
        {
            links = _links.GetLinksByConnectorTo(connector.ParentNode.Name, connector.Name);
            if (links != null)
            {
                return links.
                    Select(link => GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector))
                    .SkipNull()
                    .Any();
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetLinkedConnectorCount(FlowNodeConnector connector)
    {
        if (connector is null || connector.ParentNode is null)
        {
            return 0;
        }

        IEnumerable<NodeLink> links;
        if (connector.Direction == FlowDirections.Input)
        {
            links = _links.GetLinksByConnectorFrom(connector.ParentNode.Name, connector.Name);

            if (links != null)
            {
                return links
                    .Select(link => GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector))
                    .SkipNull()
                    .Count();
            }
        }
        else
        {
            links = _links.GetLinksByConnectorTo(connector.ParentNode.Name, connector.Name);
            if (links != null)
            {
                return links.
                    Select(link => GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector))
                    .SkipNull()
                    .Count();
            }
        }

        return 0;
    }

    /// <inheritdoc/>
    public override FlowNodeConnector GetLinkedConnector(FlowNodeConnector connector)
    {
        if (connector is null || connector.ParentNode is null)
        {
            return null;
        }

        NodeLink link;

        if (connector.Direction == FlowDirections.Input)
        {
            link = _links.GetLinksByConnectorFrom(connector.ParentNode.Name, connector.Name).FirstOrDefault();
            if (link != null)
            {
                return GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector);
            }
        }
        else
        {
            link = _links.GetLinksByConnectorTo(connector.ParentNode.Name, connector.Name).FirstOrDefault();
            if (link != null)
            {
                return GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector);
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override FlowNodeConnector[] GetLinkedConnectors(FlowNodeConnector connector, bool sort)
    {
        if (connector is null || connector.ParentNode is null)
        {
            return [];
        }

        IEnumerable<NodeLink> links;
        if (connector.Direction == FlowDirections.Input)
        {
            links = _links.GetLinksByConnectorFrom(connector.ParentNode.Name, connector.Name);

            if (links != null)
            {
                if (sort)
                {
                    if (connector.ConnectionType == FlowConnectorTypes.Control)
                    {
                        return links
                            .Select(link => GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector))
                            .SkipNull()
                            .OrderBy(o => o.ParentNode?.DiagramItem?.X ?? 0)
                            .ToArray();
                    }
                    else
                    {
                        return links
                            .Select(link => GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector))
                            .SkipNull()
                            .OrderBy(o => o.ParentNode?.DiagramItem?.Y ?? 0)
                            .ToArray();
                    }
                }
                else
                {
                    return links
                        .Select(link => GetFlowItem(link.FromNode)?.Node?.GetConnector(link.FromConnector))
                        .SkipNull()
                        .ToArray();
                }
            }
        }
        else
        {
            links = _links.GetLinksByConnectorTo(connector.ParentNode.Name, connector.Name);
            if (links != null)
            {
                if (sort)
                {
                    if (connector.ConnectionType == FlowConnectorTypes.Control)
                    {
                        return links.
                            Select(link => GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector))
                            .SkipNull()
                            .OrderBy(o => o.ParentNode?.DiagramItem?.X ?? 0)
                            .ToArray();
                    }
                    else
                    {
                        return links.
                            Select(link => GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector))
                            .SkipNull()
                            .OrderBy(o => o.ParentNode?.DiagramItem?.Y ?? 0)
                            .ToArray();
                    }
                }
                else
                {
                    return links.
                        Select(link => GetFlowItem(link.ToNode)?.Node?.GetConnector(link.ToConnector))
                        .SkipNull()
                        .ToArray();
                }
            }
        }

        return [];
    }

    /// <summary>
    /// Internal handler called when a link is added. Notifies affected nodes and the document.
    /// </summary>
    /// <param name="link">The link that was added.</param>
    internal void InternalOnLinkAdded(NodeLink link)
    {
        GetFlowItem(link.FromNode)?.Node?.OnLinkUpdated();
        GetFlowItem(link.ToNode)?.Node?.OnLinkUpdated();

        _document.OnLinkAdded(link);
    }

    /// <summary>
    /// Internal handler called when a link is removed. Notifies affected nodes and the document.
    /// </summary>
    /// <param name="link">The link that was removed.</param>
    internal void InternalOnLinkRemoved(NodeLink link)
    {
        GetFlowItem(link.FromNode)?.Node?.OnLinkUpdated();
        GetFlowItem(link.ToNode)?.Node?.OnLinkUpdated();

        _document.OnLinkRemoved(link);
    }

    /// <inheritdoc/>
    public override void OnLoaded(DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        foreach (var item in _document.DiagramItems)
        {
            try
            {
                item.Diagram = Diagram;
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        foreach (var item in _document.DiagramItems)
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

        foreach (var item in _document.DiagramItems)
        {
            try
            {
                item.Node?.OnLoaded();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        //foreach (var item in _document.DiagramItems)
        //{
        //    try
        //    {
        //        item.Node?.FlushQueuedConnection();
        //    }
        //    catch (Exception err)
        //    {
        //        err.LogError();
        //    }
        //}

        // Fix
        // Some FlowNode's Connector are added via QueueedAction, so need to wait until all Connectors are added before fixing
        //EditorUtility.AddDelayedAction(new CheckLinkDelayedAction(_document));
        FlowConnectorAliasManager.CheckDocument(_document, intent);

    }

    /// <inheritdoc/>
    public override void OnShowView()
    {
        var diagramView = _document.View.GetService<IFlowView>()?.Diagram;
        if (diagramView != null)
        {
            diagramView.GridSpan = _gridSpan;
        }
    }

    /// <inheritdoc/>
    public override void RemoveInvalidConnectors()
    {
        var removes = CollectInvalidConnectors();
        if (removes != null)
        {
            foreach (NodeLink link in removes)
            {
                _links.Remove(link);
            }
        }
    }

    /// <inheritdoc/>
    public override List<NodeLink> CollectInvalidConnectors(bool report = true)
    {
        List<NodeLink> removes = null;

        foreach (NodeLink link in _links.Links)
        {
            var fromNode = GetFlowItem(link.FromNode);
            if (fromNode is null)
            {
                if (report)
                {
                    string nodeName = ResolveNodeName(link.FromNode);
                    Logs.LogWarning($"Source node of connection line missing:{nodeName}");
                }

                (removes ??= []).Add(link);
                continue;
            }

            var toNode = GetFlowItem(link.ToNode);
            if (toNode is null)
            {
                if (report)
                {
                    string nodeName = ResolveNodeName(link.ToNode);
                    Logs.LogWarning($"Target node of connection line missing:{nodeName}");
                }

                (removes ??= []).Add(link);
                continue;
            }

            var fromConnector = fromNode.Node?.GetConnector(link.FromConnector);
            if (fromConnector is null || fromConnector.Direction != FlowDirections.Output)
            {
                if (report)
                {
                    string nodeName = ResolveNodeName(link.FromNode);
                    string cName = ResolveConnectorName(link.FromConnector);
                    Logs.LogWarning($"Source port of connection line missing:{nodeName}.{cName}");
                }

                (removes ??= []).Add(link);
                continue;
            }

            var toConnector = toNode.Node?.GetConnector(link.ToConnector);
            if (toConnector is null || toConnector.Direction != FlowDirections.Input)
            {
                if (report)
                {
                    string nodeName = ResolveNodeName(link.ToNode);
                    string cName = ResolveConnectorName(link.ToConnector);
                    Logs.LogWarning($"Target connection missing:{nodeName}.{cName}, document:{_document?.FileName}");
                }

                (removes ??= []).Add(link);
                continue;
            }
        }

        return removes;
    }

    private string ResolveNodeName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "(null)";
        }

        var item = GetFlowItem(name);
        if (item is null)
        {
            return name;
        }

        string typeName = item.Node?.TypeName;
        if (!string.IsNullOrEmpty(typeName))
        {
            return $"{name}({typeName})";
        }

        return name;
    }

    private string ResolveConnectorName(string connector)
    {
        if (string.IsNullOrEmpty(connector))
        {
            return "(null)";
        }

        if (Guid.TryParse(connector, out Guid guid))
        {
            return GlobalIdResolver.RevertResolve(guid);
        }

        return connector;
    }

    class CheckLinkDelayedAction : DelayedAction<FlowDocument>
    {
        public CheckLinkDelayedAction(FlowDocument value) : base(value)
        {
        }

        public override void DoAction()
        {
            FlowConnectorAliasManager.CheckDocument(this.Value);
        }
    }

}