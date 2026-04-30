using Suity.Synchonizing;
using System.Collections.Generic;

namespace Suity.Editor.Flows;

/// <summary>
/// Abstract collection of node links in a flow diagram.
/// </summary>
public abstract class LinkCollection
{
    /// <summary>
    /// Gets the number of links in the collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Event raised when a link is added.
    /// </summary>
    public event EventArgsHandler<NodeLink> LinkAdded;

    /// <summary>
    /// Event raised when a link is removed.
    /// </summary>
    public event EventArgsHandler<NodeLink> LinkRemoved;

    /// <summary>
    /// Adds a link to the collection.
    /// </summary>
    public abstract bool Add(NodeLink link);

    /// <summary>
    /// Removes a link from the collection.
    /// </summary>
    public abstract bool Remove(NodeLink link);

    /// <summary>
    /// Renames a node in all links.
    /// </summary>
    public abstract void RenameNode(string oldNodeName, string newNodeName);

    /// <summary>
    /// Renames a connector in all links.
    /// </summary>
    public abstract void RenameConnector(string nodeName, string oldName, string newName);

    /// <summary>
    /// Gets a link by connector names.
    /// </summary>
    public abstract NodeLink GetLink(string fromNode, string fromConnector, string toNode, string toConnector);

    /// <summary>
    /// Gets a link by connectors.
    /// </summary>
    public abstract NodeLink GetLink(FlowNodeConnector input, FlowNodeConnector output);

    /// <summary>
    /// Gets all links in the collection.
    /// </summary>
    public abstract IEnumerable<NodeLink> Links { get; }

    /// <summary>
    /// Gets links by source connector.
    /// </summary>
    public abstract IEnumerable<NodeLink> GetLinksByConnectorFrom(string fromNode, string fromConnector);

    /// <summary>
    /// Gets links by target connector.
    /// </summary>
    public abstract IEnumerable<NodeLink> GetLinksByConnectorTo(string toNode, string toConnector);

    /// <summary>
    /// Gets all links for a node.
    /// </summary>
    public abstract IEnumerable<NodeLink> GetLinks(string node);

    /// <summary>
    /// Collects links for multiple nodes.
    /// </summary>
    public abstract void CollectLinks(IEnumerable<string> nodes, ICollection<NodeLink> collection);

    /// <summary>
    /// Removes a link by connectors.
    /// </summary>
    public abstract bool Remove(FlowNodeConnector input, FlowNodeConnector output);

    /// <summary>
    /// Synchronizes the collection.
    /// </summary>
    public abstract void Sync(IIndexSync sync, ISyncContext context);

    /// <summary>
    /// Raises the LinkAdded event.
    /// </summary>
    protected void RaiseLinkedAdded(NodeLink link)
        => LinkAdded?.Invoke(this, new EventArgs<NodeLink>(link));

    /// <summary>
    /// Raises the LinkRemoved event.
    /// </summary>
    protected void RaiseLinkedRemoved(NodeLink link)
        => LinkRemoved?.Invoke(this, new EventArgs<NodeLink>(link));
}