using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Suity;
using Suity.Collections;

namespace Suity.Editor.Flows;

/// <summary>
/// Internal implementation of a flow diagram that wraps a <see cref="FlowDocument"/>.
/// </summary>
internal class FlowDocumentDiagram : IFlowDiagram
{
    private readonly FlowDocument _document;

    private readonly List<IFlowView> _views = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowDocumentDiagram"/> class.
    /// </summary>
    /// <param name="document">The flow document to wrap.</param>
    public FlowDocumentDiagram(FlowDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }


    /// <inheritdoc/>
    public void StartView(IFlowView view)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        if (_views.Contains(view))
        {
            return;
        }

        _views.Add(view);

        // Check document here if necessary?
        CheckDocument();
    }

    /// <summary>
    /// Check if document contains invalid nodes
    /// </summary>
    private void CheckDocument()
    {
        List<IFlowDiagramItem> removal = null;
        foreach (var item in _document.DiagramItems)
        {
            if (item.Node is null)
            {
                Logs.LogError($"Cannot find flow node: {item.Name}. Document: {_document.FileName}");
                (removal ??= []).Add(item);
                continue;
            }
        }

        if (removal != null)
        {
            foreach (var item in removal)
            {
                _document.RemoveFlowNode(item);
            }
        }
    }

    /// <inheritdoc/>
    public void StopView(IFlowView view)
    {
        if (view is null)
        {
            return;
        }

        _views.Remove(view);

        foreach (var node in Nodes)
        {
            node.StopView(view);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IFlowView> Views => _views.Pass();

    #region IFlowDiagram

    /// <inheritdoc/>
    public event EventHandler<FlowNode> NodeAddedOrUpdated;
    /// <inheritdoc/>
    public event EventHandler<FlowNode> NodeRemoved;
    /// <inheritdoc/>
    public event EventHandler<ConnectorRenamedEventArgs> ConnectorRenamed;
    /// <inheritdoc/>
    public event EventHandler<NodeLink> LinkAdded;
    /// <inheritdoc/>
    public event EventHandler<NodeLink> LinkRemoved;

    /// <inheritdoc/>
    public string Name => null;

    /// <inheritdoc/>
    public object DocumentContent => _document;

    /// <inheritdoc/>
    public int NodeCount => _document.ItemCount;

    /// <inheritdoc/>
    public IEnumerable<FlowNode> Nodes => _document.DiagramItems.Select(o => o.Node).SkipNull();

    /// <inheritdoc/>
    public IEnumerable<NodeLink> Links => _document.Links.Links;

    /// <inheritdoc/>
    public int GridSpan
    {
        get => _views.FirstOrDefault()?.GridSpan ?? 0;
        set
        {
            foreach (var view in _views)
            {
                view.GridSpan = value;
            }
        }
    }

    /// <inheritdoc/>
    public IFlowDiagramItem AddNode(FlowNode node, Rectangle? rect = null)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        IFlowDiagramItem item = _document.AddFlowNode(node, rect);
        if (item != null)
        {
            item.Diagram = this;
            NodeAddedOrUpdated?.Invoke(this, node);

            return item;
        }

        return null;
    }

    /// <inheritdoc/>
    public void RemoveNode(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        //IFlowDiagramItem item = _document.AddFlowNode(node);
        //if (item?.Diagram == this)
        //{
        //    item.Diagram = null;
        //}

        var item = node.DiagramItem;
        if (ReferenceEquals(item?.Diagram, this))
        {
            item.Diagram = null;
        }

        _document.RemoveFlowNode(node);

        NodeRemoved?.Invoke(this, node);
    }

    /// <inheritdoc/>
    public void AddLink(string fromNode, string fromConnector, string toNode, string toConnector)
    {
        var link = new NodeLink(fromNode, fromConnector, toNode, toConnector);
        _document.Links.Add(link);

        LinkAdded?.Invoke(this, link);
    }

    /// <inheritdoc/>
    public void RemoveLink(string fromNode, string fromConnector, string toNode, string toConnector)
    {
        var linkcol = _document.Links;

        if (linkcol.GetLink(fromNode, fromConnector, toNode, toConnector) is { } link)
        {
            linkcol.Remove(link);

            LinkRemoved?.Invoke(this, link);
        }
    }

    /// <inheritdoc/>
    public void RemoveLinksByNodeFrom(string fromNode, string fromConnector)
    {
        var linkcol = _document.Links;

        List<NodeLink> links = [.. linkcol.GetLinksByConnectorFrom(fromNode, fromConnector)];

        foreach (var link in links)
        {
            linkcol.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }
    }

    /// <inheritdoc/>
    public void RemoveLinksByNodeTo(string toNode, string toConnector)
    {
        var linkcol = _document.Links;

        List<NodeLink> links = [.. linkcol.GetLinksByConnectorTo(toNode, toConnector)];

        foreach (var link in links)
        {
            linkcol.Remove(link);
            LinkRemoved?.Invoke(this, link);
        }
    }

    /// <inheritdoc/>
    public FlowNode GetNode(string nodeName)
        => _document.GetDiagramItem(nodeName)?.Node;

    /// <inheritdoc/>
    public FlowNodeConnector GetConnector(string nodeName, string connectorName)
        => _document.GetDiagramItem(nodeName)?.Node?.GetConnector(connectorName);

    /// <inheritdoc/>
    public IEnumerable<NodeLink> GetLinksByConnectorFrom(string fromNode, string fromConnector)
        => _document.Links.GetLinksByConnectorFrom(fromNode, fromConnector);

    /// <inheritdoc/>
    public IEnumerable<NodeLink> GetLinksByConnectorTo(string toNode, string toConnector)
        => _document.Links.GetLinksByConnectorTo(toNode, toConnector);

    /// <inheritdoc/>
    public bool GetIsLinked(FlowNodeConnector connector)
        => _document._ex.GetIsConnectorLinked(connector);

    /// <inheritdoc/>
    public int GetLinkedConnectorCount(FlowNodeConnector connector)
        => _document._ex.GetLinkedConnectorCount(connector);

    /// <inheritdoc/>
    public FlowNodeConnector GetLinkedConnector(FlowNodeConnector connector)
        => _document._ex.GetLinkedConnector(connector);

    /// <inheritdoc/>
    public FlowNodeConnector[] GetLinkedConnectors(FlowNodeConnector connector, bool sort)
        => _document._ex.GetLinkedConnectors(connector, sort);

    /// <inheritdoc/>
    public List<NodeLink> CollectInvalidConnectors(bool report = true)
        => _document._ex.CollectInvalidConnectors(report);

    /// <inheritdoc/>
    public void QueueComputeData() => _views.ForEach(o => o.QueueComputeData());

    /// <inheritdoc/>
    public void RefreshView() => _views.ForEach(o => o.RefreshView());

    /// <inheritdoc/>
    public void MarkVisit()
    {
        _document?.Entry?.MarkVisit();
    }

    /// <inheritdoc/>
    public void MarkDirty()
        => _document?.MarkDirty(this);

    /// <inheritdoc/>
    public void NotifyNodeUpdated(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        // Auto determine node status
        if (node.Diagram != null)
        {
            NodeAddedOrUpdated?.Invoke(this, node);
        }
        else
        {
            NodeRemoved?.Invoke(this, node);
        }
    }

    /// <inheritdoc/>
    public void NotifyNodeRemoved(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        NodeRemoved?.Invoke(this, node);
    }

    /// <inheritdoc/>
    public void NotifyNodeRenamed(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (string.IsNullOrWhiteSpace(node.Name))
        {
            throw new ArgumentException("node.Name is empty");
        }

        if (node.DiagramItem is not { } item)
        {
            return;
        }

        string oldName = item.Name;

        if (item != null && item.Name != node.Name)
        {
            item.Name = node.Name;

            // Setting item.Name may trigger rename due to illegal characters, need to reset to node.Name while avoiding infinite loop
            if (item.Name != node.Name)
            {
                node.Name = item.Name;
            }
        }

        string newName = node.Name;

        if (oldName != newName)
        {
            _document.Links.RenameNode(oldName, newName);
        }

        NodeAddedOrUpdated?.Invoke(this, node);
    }

    /// <inheritdoc/>
    public void NotifyConnectorRenamed(FlowNode node, string oldName, string newName)
    {
        _document.Links.RenameConnector(node.Name, oldName, newName);
        ConnectorRenamed?.Invoke(this, new ConnectorRenamedEventArgs(node, oldName, newName));
    }

    #endregion
}