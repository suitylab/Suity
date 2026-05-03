using Suity.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.NodeGraph;

#region GraphNodeCollection

/// <summary>
/// A collection of graph nodes that maintains uniqueness and supports z-ordering.
/// </summary>
public class GraphNodeCollection : IEnumerable<GraphNode>
{
    // Use list to maintain order
    private readonly List<GraphNode> _nodes = [];

    // Use set to ensure uniqueness
    private readonly HashSet<GraphNode> _nodeSet = [];

    /// <summary>
    /// Gets the number of nodes in the collection.
    /// </summary>
    public int Count => _nodes.Count;
    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The node at the specified index.</returns>
    public GraphNode this[int index] => _nodes[index];

    /// <summary>
    /// Determines whether the collection contains the specified node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node is in the collection; otherwise, false.</returns>
    public bool Contains(GraphNode node) => _nodeSet.Contains(node);

    /// <summary>
    /// Adds a node to the collection.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void Add(GraphNode node)
    {
        if (_nodeSet.Add(node))
        {
            //if (node.IsGroup)
            //{
            //    node.UpdateHitRectangle();
            //    _nodes.InsertSorted(node, (a, b) => a.HitRectangle.Contains(b.HitRectangle) ? -1 : 1);
            //}
            //else
            //{
            //    _nodes.Add(node);
            //}

            _nodes.InsertSorted(node, (a, b) => a.HitRectangle.Contains(b.HitRectangle) ? -1 : 1);
        }
    }

    /// <summary>
    /// Adds a range of nodes to the collection.
    /// </summary>
    /// <param name="nodes">The nodes to add.</param>
    public void AddRange(IEnumerable<GraphNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (_nodeSet.Add(node))
            {
                _nodes.Add(node);
            }
        }
    }

    /// <summary>
    /// Removes a node from the collection.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was removed; otherwise, false.</returns>
    public bool Remove(GraphNode node)
    {
        if (_nodeSet.Remove(node))
        {
            _nodes.Remove(node);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all nodes that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match nodes.</param>
    /// <returns>The number of nodes removed.</returns>
    public int RemoveAll(Predicate<GraphNode> predicate)
    {
        List<GraphNode>? removes = null;
        foreach (var node in _nodes)
        {
            if (predicate(node))
            {
                (removes ??= []).Add(node);
            }
        }

        if (removes != null)
        {
            foreach (var node in removes)
            {
                if (_nodeSet.Remove(node))
                {
                    _nodes.Remove(node);
                }
            }
        }

        return removes?.Count ?? 0;
    }

    /// <summary>
    /// Clears all nodes from the collection.
    /// </summary>
    public void Clear()
    {
        _nodeSet.Clear();
        _nodes.Clear();
    }

    readonly HashSet<GraphNode> _temporarySelection = [];
    /// <summary>
    /// Moves the specified nodes to the front of the z-order.
    /// </summary>
    /// <param name="nodes">The nodes to bring to front.</param>
    public void BringToFront(IEnumerable<GraphNode> nodes)
    {
        _temporarySelection.Clear();
        _temporarySelection.AddRange(nodes);
        // Exclude nodes that are not in _nodes
        _temporarySelection.IntersectWith(_nodes);

        _nodeSet.RemoveAll(_temporarySelection.Contains);
        _nodes.RemoveAll(_temporarySelection.Contains);

        AddRange(nodes);

        _temporarySelection.Clear();

        // Debug.WriteLine("SendToBack: " + string.Join(", ", _nodes.Select(o => o.Name)));
    }

    readonly List<GraphNode> _innerSorts = [];
    /// <summary>
    /// Moves nodes that are contained within the specified nodes to the front.
    /// </summary>
    /// <param name="nodes">The parent nodes to check containment for.</param>
    public void BringInnersToFront(IEnumerable<GraphNode> nodes)
    {
        _innerSorts.Clear();
        foreach (var node in nodes)
        {
            foreach (var test in _nodes.Where(o => o != node))
            {
                if (node.HitRectangle.Contains(test.HitRectangle))
                {
                    _innerSorts.Add(test);
                }
            }
        }

        if (_innerSorts.Count > 0)
        {
            BringToFront(_innerSorts);
            _innerSorts.Clear();
        }
    }

    #region IEnumerator

    public IEnumerator<GraphNode> GetEnumerator() => ((IEnumerable<GraphNode>)_nodes).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nodes).GetEnumerator();

    #endregion
}

#endregion

#region LayeredGraphNodeCollection

/// <summary>
/// A layered collection that separates group nodes from regular nodes, rendering groups first.
/// </summary>
public class LayeredGraphNodeCollection : IEnumerable<GraphNode>
{
    readonly GraphNodeCollection _groups = [];
    readonly GraphNodeCollection _nodes = [];

    /// <summary>
    /// Gets the total number of nodes in the collection.
    /// </summary>
    public int Count => _groups.Count + _nodes.Count;
    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The node at the specified index.</returns>
    public GraphNode this[int index]
    {
        get
        {
            if (index < _groups.Count)
            {
                return _groups[index];
            }
            else
            {
                return _nodes[index - _groups.Count];
            }
        }
    }
    /// <summary>
    /// Adds a node to the appropriate layer (groups or regular nodes).
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void Add(GraphNode node)
    {
        if (node.IsGroup)
        {
            _groups.Add(node);
        }
        else
        {
            _nodes.Add(node);
        }
    }
    /// <summary>
    /// Adds a range of nodes to the collection.
    /// </summary>
    /// <param name="nodes">The nodes to add.</param>
    public void AddRange(IEnumerable<GraphNode> nodes)
    {
        foreach (var node in nodes)
        {
            Add(node);
        }
    }
    /// <summary>
    /// Removes a node from the collection.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was removed; otherwise, false.</returns>
    public bool Remove(GraphNode node)
    {
        if (node.IsGroup)
        {
            return _groups.Remove(node);
        }
        else
        {
            return _nodes.Remove(node);
        }
    }
    /// <summary>
    /// Removes all nodes that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match nodes.</param>
    public void RemoveAll(Predicate<GraphNode> predicate)
    {
        _groups.RemoveAll(predicate);
        _nodes.RemoveAll(predicate);
    }
    /// <summary>
    /// Clears all nodes from the collection.
    /// </summary>
    public void Clear()
    {
        _groups.Clear();
        _nodes.Clear();
    }


    private readonly List<GraphNode> _tempGroups = [];
    private readonly List<GraphNode> _tempNodes = [];
    /// <summary>
    /// Moves the specified nodes to the front of the z-order.
    /// </summary>
    /// <param name="nodes">The nodes to bring to front.</param>
    public void BringToFront(IEnumerable<GraphNode> nodes)
    {
        _tempGroups.Clear();
        _tempNodes.Clear();

        _tempGroups.AddRange(nodes.Where(o => o.IsGroup));
        _tempNodes.AddRange(nodes.Where(o => !o.IsGroup));

        if (_tempGroups.Count > 0)
        {
            _groups.BringToFront(_tempGroups);
            _groups.BringInnersToFront(_tempGroups);
            _tempGroups.Clear();
        }
        
        if (_tempNodes.Count > 0)
        {
            _nodes.BringToFront(_tempNodes);
            _nodes.BringInnersToFront(_tempNodes);
            _tempNodes.Clear();
        }
    }

    public GraphNodeCollection Groups => _groups;
    public GraphNodeCollection Nodes => _nodes;

    #region IEnumerable

    public IEnumerator<GraphNode> GetEnumerator()
    {
        foreach (var group in _groups)
        {
            yield return group;
        }

        foreach (var node in _nodes)
        {
            yield return node;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

#endregion

#region GraphLinkCollection
/// <summary>
/// A collection of graph links with efficient lookup by connector.
/// </summary>
public class GraphLinkCollection : IEnumerable<GraphLink>
{
    readonly List<GraphLink> _links = [];

    readonly UniqueMultiDictionary<GraphConnector, GraphLink> _linkDic = new();

    /// <summary>
    /// Adds a link to the collection.
    /// </summary>
    /// <param name="link">The link to add.</param>
    public void Add(GraphLink link)
    {
        _links.Add(link);

        _linkDic.Add(link.From, link);
        _linkDic.Add(link.To, link);
    }

    /// <summary>
    /// Removes a link from the collection.
    /// </summary>
    /// <param name="link">The link to remove.</param>
    public void Remove(GraphLink link)
    {
        _links.Remove(link);

        _linkDic.Remove(link.From, link);
        _linkDic.Remove(link.To, link);
    }

    /// <summary>
    /// Removes all links connected to the specified node.
    /// </summary>
    /// <param name="node">The node whose links should be removed.</param>
    public void Remove(GraphNode node)
    {
        var links = GetLinks(node);
        if (links.Any())
        {
            foreach (var link in links.ToArray())
            {
                Remove(link);
            }
        }
    }

    /// <summary>
    /// Removes all links connected to the specified connector.
    /// </summary>
    /// <param name="connector">The connector whose links should be removed.</param>
    public void Remove(GraphConnector connector)
    {
        var links = GetLinks(connector);
        if (links.Any())
        {
            foreach (var link in links.ToArray())
            {
                Remove(link);
            }
        }
    }

    /// <summary>
    /// Gets all input connectors connected to the specified output connector.
    /// </summary>
    /// <param name="output">The output connector.</param>
    /// <returns>A collection of input connectors.</returns>
    public IEnumerable<GraphConnector> GetInputs(GraphConnector output) => _linkDic[output].Select(o => o.From);

    /// <summary>
    /// Gets all output connectors connected to the specified input connector.
    /// </summary>
    /// <param name="input">The input connector.</param>
    /// <returns>A collection of output connectors.</returns>
    public IEnumerable<GraphConnector> GetOutput(GraphConnector input) => _linkDic[input].Select(o => o.To);

    /// <summary>
    /// Determines whether the specified connector has any links.
    /// </summary>
    /// <param name="connector">The connector to check.</param>
    /// <returns>True if the connector has links; otherwise, false.</returns>
    public bool IsLinked(GraphConnector connector) => _linkDic.ContainsKey(connector);

    /// <summary>
    /// Determines whether two connectors are linked to each other.
    /// </summary>
    /// <param name="connector1">The first connector.</param>
    /// <param name="connector2">The second connector.</param>
    /// <returns>True if the connectors are linked; otherwise, false.</returns>
    public bool IsLinked(GraphConnector connector1, GraphConnector connector2)
    {
        var links = GetLinks(connector1);

        return links.Any(o => o.From == connector2 || o.To == connector2);
    }

    /// <summary>
    /// Gets all links connected to the specified connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>A collection of links.</returns>
    public IEnumerable<GraphLink> GetLinks(GraphConnector connector) => _linkDic[connector];

    /// <summary>
    /// Gets all links connected to the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>A collection of links.</returns>
    public IEnumerable<GraphLink> GetLinks(GraphNode node) => node.Connectors.SelectMany(c => _linkDic[c]);

    /// <summary>
    /// Clears all links from the collection.
    /// </summary>
    public void Clear()
    {
        _links.Clear();
        _linkDic.Clear();
    }

    /// <summary>
    /// Moves the specified links to the end of the collection (front in z-order).
    /// </summary>
    /// <param name="links">The links to bring to front.</param>
    public void BringToFront(IEnumerable<GraphLink> links)
    {
        foreach (var link in links)
        {
            if (_links.Remove(link))
            {
                _links.Add(link);
            }
        }
    }

    public IEnumerator<GraphLink> GetEnumerator() => ((IEnumerable<GraphLink>)_links).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_links).GetEnumerator();
}
#endregion

#region GraphConnectorCollection

/// <summary>
/// A collection of connectors belonging to a single node.
/// </summary>
public class GraphConnectorCollection : IEnumerable<GraphConnector>
{
    private readonly List<GraphConnector> _connectors = [];


    /// <summary>
    /// Gets a value indicating whether the collection contains any normal (data or action) connectors.
    /// </summary>
    public bool HasNormalConnector { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the collection contains any associate connectors.
    /// </summary>
    public bool HasAssociateConnector { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the collection contains any control output connectors.
    /// </summary>
    public bool HasControlOuputConnector { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the collection contains any control input connectors.
    /// </summary>
    public bool HasControlInputConnector { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphConnectorCollection"/> class.
    /// </summary>
    public GraphConnectorCollection()
    {
    }

    /// <summary>
    /// Gets the number of connectors in the collection.
    /// </summary>
    public int Count => _connectors.Count;

    /// <summary>
    /// Adds a connector to the collection.
    /// </summary>
    /// <param name="connector">The connector to add.</param>
    public void Add(GraphConnector connector)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        _connectors.Add(connector);

        switch (connector.ConnectorType)
        {
            case ConnectorType.Data:
            case ConnectorType.Action:
                HasNormalConnector = true;
                break;

            case ConnectorType.Associate:
                HasAssociateConnector = true;
                break;

            case ConnectorType.Control:
                if (connector.Direction == GraphDirection.Output)
                {
                    HasControlOuputConnector = true;
                }
                else
                {
                    HasControlInputConnector = true;
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Clears all connectors from the collection and resets type flags.
    /// </summary>
    public void Clear()
    {
        _connectors.Clear();

        HasNormalConnector = false;
        HasAssociateConnector = false;
        HasControlOuputConnector = false;
        HasControlInputConnector = false;
    }

    /// <summary>
    /// Gets the connector at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The connector at the specified index.</returns>
    public GraphConnector this[int index] => _connectors[index];

    /// <summary>
    /// Finds a connector by its name.
    /// </summary>
    /// <param name="name">The name of the connector.</param>
    /// <returns>The connector if found; otherwise, null.</returns>
    public GraphConnector Find(string name) => _connectors.FirstOrDefault(o => o.Name == name);

    #region IEnumerator
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_connectors).GetEnumerator();

    public IEnumerator<GraphConnector> GetEnumerator() => ((IEnumerable<GraphConnector>)_connectors).GetEnumerator();

    #endregion
}

#endregion
