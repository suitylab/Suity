using Suity.Collections;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Internal link collection that manages node connections with synchronization support.
/// </summary>
internal class LinkCollectionBK(FlowDocumentExternalBK parent) : LinkCollection, ISyncList
{
    private static readonly HashSet<string> _nodeCache = [];
    private static readonly HashSet<NodeLink> _linkCache = [];

    private readonly FlowDocumentExternalBK _parent = parent ?? throw new ArgumentNullException(nameof(parent));

    private readonly List<NodeLink> _links = [];
    private readonly UniqueMultiDictionary<string, NodeLink> _linksByNode = new();

    /// <inheritdoc/>
    public override int Count => _links.Count;

    /// <inheritdoc/>
    public override bool Add(NodeLink link)
    {
        if (link.ToConnector is null)
        {
            return false;
        }

        if (link.FromConnector is null)
        {
            return false;
        }

        if (GetLink(link.FromNode, link.FromConnector, link.ToNode, link.ToConnector) != null)
        {
            return false;
        }

        _links.Add(link);
        OnLinkAdded(link, _links.Count - 1);

        return true;
    }

    /// <inheritdoc/>
    public override bool Remove(NodeLink link)
    {
        if (_links.Remove(link))
        {
            OnLinkRemoved(link);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override void RenameNode(string oldNodeName, string newNodeName)
    {
        foreach (var link in _linksByNode[oldNodeName])
        {
            if (link.FromNode == oldNodeName)
            {
                link.FromNode = newNodeName;
            }

            if (link.ToNode == oldNodeName)
            {
                link.ToNode = newNodeName;
            }
        }
        _linksByNode.RenameCombineKey(oldNodeName, newNodeName);
    }

    /// <inheritdoc/>
    public override void RenameConnector(string nodeName, string oldName, string newName)
    {
        foreach (var link in _linksByNode[nodeName].Where(o => o.ToConnector == oldName))
        {
            link.ToConnector = newName;
        }
    }

    /// <inheritdoc/>
    public override NodeLink GetLink(string fromNode, string fromConnector, string toNode, string toConnector)
    {
        return _linksByNode[fromNode].Where(
            o => o.FromConnector == fromConnector &&
            o.ToNode == toNode
            && o.ToConnector == toConnector).FirstOrDefault();
    }

    /// <inheritdoc/>
    public override NodeLink GetLink(FlowNodeConnector input, FlowNodeConnector output)
    {
        if (output is null)
        {
            return null;
        }

        if (input is null)
        {
            return null;
        }

        return GetLink(input.ParentNode?.Name, input.Name, output.ParentNode?.Name, output.Name);
    }

    /// <inheritdoc/>
    public override IEnumerable<NodeLink> Links => _links.Select(o => o);

    /// <inheritdoc/>
    public override IEnumerable<NodeLink> GetLinksByConnectorFrom(string fromNode, string fromConnector)
    {
        return _linksByNode[fromNode].Where(o => o.ToNode == fromNode && o.ToConnector == fromConnector);
    }

    /// <inheritdoc/>
    public override IEnumerable<NodeLink> GetLinksByConnectorTo(string toNode, string toConnector)
    {
        return _linksByNode[toNode].Where(o => o.FromNode == toNode && o.FromConnector == toConnector);
    }

    /// <inheritdoc/>
    public override IEnumerable<NodeLink> GetLinks(string node)
    {
        return _linksByNode[node];
    }

    /// <inheritdoc/>
    public override void CollectLinks(IEnumerable<string> nodes, ICollection<NodeLink> collection)
    {
        _nodeCache.Clear();
        _linkCache.Clear();

        _nodeCache.AddRange(nodes);

        foreach (string node in nodes)
        {
            _linkCache.AddRange(_linksByNode[node].Where(o => _nodeCache.Contains(o.FromNode) && _nodeCache.Contains(o.ToNode)));
        }

        try
        {
            collection.AddRange(_linkCache);
        }
        finally
        {
            _nodeCache.Clear();
            _linkCache.Clear();
        }
    }

    public override bool Remove(FlowNodeConnector input, FlowNodeConnector output)
    {
        NodeLink link = GetLink(input, output);

        if (link != null)
        {
            _links.Remove(link);
            OnLinkRemoved(link);

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Sync(IIndexSync sync, ISyncContext context)
    {
        if (sync.Intent == SyncIntent.DataExport)
        {
            var exportLinks = CreateDataExportNodeLinks();

            sync.SyncGenericIList(exportLinks);
        }
        else
        {
            sync.SyncGenericIList(
                _links,
                null,
                null,
                () => new NodeLink(),
                OnLinkAdded,
                OnLinkRemoved
            );
        }
    }

    private List<NodeLink> CreateDataExportNodeLinks()
    {
        var doc = _parent.Document;

        List<NodeLink> list = [];
        foreach (NodeLink link in _links)
        {
            var fromConn = doc.GetFlowNode(link.FromNode)?.GetConnector(link.FromConnector);
            var toConn = doc.GetFlowNode(link.ToNode)?.GetConnector(link.ToConnector);

            if (fromConn != null && toConn != null)
            {
                NodeLink newLink = NodeLink.CreateFromConnectorExported(fromConn, toConn);
                list.Add(newLink);
            }
        }

        return list;
    }

    private void OnLinkAdded(NodeLink link, int index)
    {
        if (!string.IsNullOrWhiteSpace(link.FromNode) && !string.IsNullOrWhiteSpace(link.FromConnector))
        {
            _linksByNode.Add(link.FromNode, link);
        }

        if (!string.IsNullOrWhiteSpace(link.ToNode) && !string.IsNullOrWhiteSpace(link.ToConnector))
        {
            _linksByNode.Add(link.ToNode, link);
        }

        RaiseLinkedAdded(link);
    }

    private void OnLinkRemoved(NodeLink link)
    {
        if (!string.IsNullOrWhiteSpace(link.FromNode) && !string.IsNullOrWhiteSpace(link.FromConnector))
        {
            _linksByNode.Remove(link.FromNode, link);
        }

        if (!string.IsNullOrWhiteSpace(link.ToNode) && !string.IsNullOrWhiteSpace(link.ToConnector))
        {
            _linksByNode.Remove(link.ToNode, link);
        }

        RaiseLinkedRemoved(link);
    }
}