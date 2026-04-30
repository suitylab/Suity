using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// An abstract base class for path nodes that support lazy population of child nodes with expand/collapse functionality.
/// </summary>
public abstract class PopulatePathNode : PathNode
{
    private PopulateState _state;
    private bool _expanded;

    /// <summary>
    /// Gets the current populate state of this node's children.
    /// </summary>
    public PopulateState State => _state;

    /// <summary>
    /// Gets a value indicating whether this node can be expanded.
    /// </summary>
    public override bool CanExpand => NodeList.Count > 0 || _state == PopulateState.PopulateDummy;

    /// <summary>
    /// Gets or sets a value indicating whether this node requires an update during the next population.
    /// </summary>
    public bool RequireUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node is expanded. Setting to true triggers population of child nodes.
    /// </summary>
    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
            {
                return;
            }

            _expanded = value;
            if (_expanded)
            {
                // If child items have already been listed, ignore
                if (_state != PopulateState.Populated)
                {
                    _Populate();
                }
            }
            else
            {
                _SetDummy();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulatePathNode"/> class.
    /// </summary>
    protected PopulatePathNode() 
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulatePathNode"/> class with the specified node path.
    /// </summary>
    /// <param name="nodePath">The path identifier for this node.</param>
    protected PopulatePathNode(string nodePath)
        : base(nodePath)
    {
    }

    /// <summary>
    /// Called when this node is added to a parent. Automatically triggers population.
    /// </summary>
    protected internal override void OnAdded()
    {
        base.OnAdded();
        Populate();
    }

    /// <summary>
    /// Populates the child nodes of this node based on its current expanded state.
    /// </summary>
    public void Populate()
    {
        if (RequireUpdate)
        {
            RequireUpdate = false;
        }
        else if (_state == PopulateState.Populated)
        {
            return;
        }

        if (_expanded)
        {
            _Populate();
        }
        else
        {
            if (_state != PopulateState.PopulateDummy)
            {
                _SetDummy();
            }
            else
            {
                NodeList.Clear();
                _state = PopulateState.None;
            }
        }
    }

    /// <summary>
    /// Forces a re-population of this node's children based on its current expanded state.
    /// </summary>
    public void PopulateUpdate()
    {
        if (_expanded)
        {
            _Populate();
        }
        else
        {
            _SetDummy();
        }
    }

    /// <summary>
    /// Forces a re-population of this node and all descendant nodes that are <see cref="PopulatePathNode"/> instances.
    /// </summary>
    public void PopulateUpdateDeep()
    {
        PopulateUpdate();

        foreach (PopulatePathNode node in NodeList.OfType<PopulatePathNode>())
        {
            node.PopulateUpdateDeep();
        }
    }

    /// <summary>
    /// Forces a re-population of this node and all descendant nodes that are <see cref="PopulatePathNode"/> instances,
    /// but only updates nodes that match the specified type T.
    /// </summary>
    /// <typeparam name="T">The type of nodes to update.</typeparam>
    public void PopulateUpdateDeep<T>()
    {
        if (this is T)
        {
            PopulateUpdate();
        }

        foreach (PopulatePathNode node in NodeList.OfType<PopulatePathNode>())
        {
            node.PopulateUpdateDeep<T>();
        }
    }

    /// <summary>
    /// Force listing of child nodes
    /// </summary>
    public void EnsurePopulate(bool force = false)
    {
        if (force)
        {
            _Populate();
        }
        else
        {
            if (_state != PopulateState.Populated)
            {
                _Populate();
            }
        }
    }

    /// <summary>
    /// Ensures this node and all descendant nodes are fully populated.
    /// </summary>
    /// <param name="force">If true, forces re-population even if already populated.</param>
    public void EnsurePopulateDeep(bool force = false)
    {
        EnsurePopulate(force);

        foreach (PopulatePathNode node in NodeList.OfType<PopulatePathNode>())
        {
            node.EnsurePopulateDeep(force);
        }
    }



    /// <summary>
    /// Clears all populated child nodes and resets the populate state to None.
    /// </summary>
    public void ClearPopulate()
    {
        NodeList.Clear();
        _state = PopulateState.None;
    }

    /// <summary>
    /// Expands this node and all descendant nodes recursively.
    /// </summary>
    public void ExpandDeep()
    {
        Expanded = true;
        foreach (PopulatePathNode node in NodeList.OfType<PopulatePathNode>())
        {
            node.ExpandDeep();
        }
    }

    /// <summary>
    /// Collapses this node and all descendant nodes recursively.
    /// </summary>
    public void CollapseDeep()
    {
        Expanded = false;
        foreach (PopulatePathNode node in NodeList.OfType<PopulatePathNode>())
        {
            node.CollapseDeep();
        }
    }

    private void _SetDummy()
    {
        RequireUpdate = false;
        NodeList.Clear();

        if (CanPopulate())
        {
            NodeList.Add(new DummyNode());
            _state = PopulateState.PopulateDummy;
        }
        else
        {
            _state = PopulateState.None;
        }
    }

    private void _Populate()
    {
        RequireUpdate = false;

        try
        {
            NodeList.SuspendLayout();

            var model = FindModel();

            if (CanPopulate())
            {
                PathNode[] nodes = [.. OnPopulate()];
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i] is null)
                        {
                            nodes[i] = new DummyNode();
                        }

                        var node = nodes[i];
                        if (node == this)
                        {
                            throw new InvalidOperationException("Populated node is self.");
                        }

                        if (i < NodeList.Count)
                        {
                            var current = NodeList[i];
                            if (current.GetType() == node.GetType() && node.NodePath != null && current.NodePath == node.NodePath && node.Reusable)
                            {
                                current.UpdateStatus();
                            }
                            else
                            {
                                NodeList[i] = node;
                            }
                        }
                        else
                        {
                            NodeList.Add(node);
                        }
                    }

                    while (NodeList.Count > nodes.Length)
                    {
                        NodeList.RemoveAt(NodeList.Count - 1);
                    }
                }
                _state = PopulateState.Populated;
            }
            else
            {
                NodeList.Clear();
                _state = PopulateState.None;
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
        finally
        {
            NodeList.ResumeLayout();
        }
    }

    /// <summary>
    /// Determines whether this node can be populated with child nodes.
    /// </summary>
    /// <returns>True if the node can be populated; otherwise, false.</returns>
    protected virtual bool CanPopulate() => false;

    /// <summary>
    /// Provides the collection of child nodes when this node is populated.
    /// </summary>
    /// <returns>An enumerable of child path nodes.</returns>
    protected virtual IEnumerable<PathNode> OnPopulate()
    {
        yield break;
    }

    /// <summary>
    /// Called when this node is removed from its parent. Resets the populate state.
    /// </summary>
    /// <param name="fromParent">The parent node from which this node was removed.</param>
    protected internal override void OnRemoved(PathNode fromParent)
    {
        base.OnRemoved(fromParent);

        _nodeList.Clear();

        // State needs to be reset on removal
        _state = PopulateState.None;
    }
}
