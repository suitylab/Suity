using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Encapsulates a node diagram data (nodes & links)
/// </summary>
public class GraphDiagram
{
    private GraphControl _control;

    private readonly LayeredGraphNodeCollection _nodeCollection = [];
    private readonly List<GraphNode> _selectedNodes = [];
    private readonly List<GraphNode> _drivenNodes = [];
    private readonly GraphLinkCollection _links = [];
    private readonly List<GraphLink> _selectedLinks = [];

    /// <summary>
    /// The control that contains this diagram
    /// </summary>
    public GraphControl ParentControl => _control;


    /// <summary>
    /// The node Collection contained in this diagram
    /// </summary>
    public LayeredGraphNodeCollection NodeCollection => _nodeCollection;

    /// <summary>
    /// The collection of currently Selected nodes in this diagram
    /// </summary>
    public List<GraphNode> SelectedNodes => _selectedNodes;

    /// <summary>
    /// Gets the collection of driven nodes (nodes inside groups that move with the group).
    /// </summary>
    public List<GraphNode> DrivenNodes => _drivenNodes;

    /// <summary>
    /// The collection of Links created in this diagram
    /// </summary>
    public GraphLinkCollection Links => _links;

    /// <summary>
    /// The collection of currently selected links in this diagram
    /// </summary>
    public List<GraphLink > SelectedLinks => _selectedLinks;

    /// <summary>
    /// The list of Known Data Types
    /// </summary>
    public IGraphDataTypeProvider DataTypeProvider { get; set; }

    /// <summary>
    /// Creates a new GraphDiagram in a GraphControl
    /// </summary>
    /// <param name="control"></param>
    public GraphDiagram(GraphControl control)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));
        DataTypeProvider = new DefaultGraphDataTypeProvider();
    }

    /// <summary>
    /// Finds a node by its name.
    /// </summary>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The node if found; otherwise, null.</returns>
    public GraphNode FindNode(string name)
    {
        return _nodeCollection.FirstOrDefault(o => o.Name == name);
    }

    public GraphLink FindLink(string fromNode, string fromConnector, string toNode, string toConnector)
    {
        return _links.FirstOrDefault(o =>
            o.From.Parent.Name == fromNode && o.From.Name == fromConnector &&
            o.To.Parent.Name == toNode && o.To.Name == toConnector);
    }

    /// <summary>
    /// Returns the Node Index of the GraphNode in this diagram's current selection
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public int GetSelectionNodeIndex(GraphNode node)
    {
        for (int i = 0; i < _selectedNodes.Count; i++)
        {
            if (_selectedNodes[i] == node) return i;
        }

        return -1;
    }

    /// <summary>
    /// Bring selection nodes and links to front
    /// </summary>
    public void SelectionBringToFront()
    {
        if (_selectedNodes.Count > 0)
        {
            _nodeCollection.BringToFront(_selectedNodes);
        }

        if (_selectedLinks.Count > 0)
        {
            _links.BringToFront(_selectedLinks);
        }
    }

    /// <summary>
    /// Removes a node from the diagram, including from selection and driven items.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveNode(GraphNode node)
    {
        if (node is null) return;

        _nodeCollection.Remove(node);
        _selectedNodes.Remove(node);
        _drivenNodes.Remove(node);
    }

    /// <summary>
    /// Clears all nodes, links, selection, and driven items from the diagram.
    /// </summary>
    public void Clear()
    {
        _nodeCollection.Clear();
        _selectedNodes.Clear();
        _drivenNodes.Clear();
        _links.Clear();
    }
}
