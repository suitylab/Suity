using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Encapsulates a node diagram data (nodes & links)
/// </summary>
public class GraphDiagram
{
    private readonly LayeredGraphNodeCollection _nodeCollection = [];
    private readonly List<GraphNode> _selectedItems = [];
    private readonly List<GraphNode> _drivenItems = [];
    private readonly GraphLinkCollection _links = [];

    private GraphControl _control;

    /// <summary>
    /// The node Collection contained in this diagram
    /// </summary>
    public LayeredGraphNodeCollection NodeCollection => _nodeCollection;

    /// <summary>
    /// The collection of currently Selected nodes in this diagram
    /// </summary>
    public List<GraphNode> SelectedItems => _selectedItems;

    /// <summary>
    /// Gets the collection of driven items (nodes inside groups that move with the group).
    /// </summary>
    public List<GraphNode> DrivenItems => _drivenItems;

    /// <summary>
    /// The collection of Links created in this view
    /// </summary>
    public GraphLinkCollection Links => _links;

    /// <summary>
    /// The control that contains this view
    /// </summary>
    public GraphControl ParentControl => _control;

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

    /// <summary>
    /// Returns the Node Index of the GraphNode in this view's current selection
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public int GetSelectionNodeIndex(GraphNode node)
    {
        for (int i = 0; i < _selectedItems.Count; i++)
        {
            if (_selectedItems[i] == node) return i;
        }

        return -1;
    }

    /// <summary>
    /// Bring selection nodes to front
    /// </summary>
    public void SelectionBringToFront()
    {
        if (_selectedItems.Count > 0)
        {
            _nodeCollection.BringToFront(_selectedItems);
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
        _selectedItems.Remove(node);
        _drivenItems.Remove(node);
    }

    /// <summary>
    /// Clears all nodes, links, selection, and driven items from the diagram.
    /// </summary>
    public void Clear()
    {
        _nodeCollection.Clear();
        _selectedItems.Clear();
        _drivenItems.Clear();
        _links.Clear();
    }
}
