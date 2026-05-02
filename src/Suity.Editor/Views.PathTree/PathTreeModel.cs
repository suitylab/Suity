using Suity.Collections;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// Provides an abstract base implementation for a path tree model, managing nodes and their structure.
/// </summary>
public abstract class PathTreeModel : IPathTreeModel
{
    private readonly PathNode _rootNode;
    private readonly UniqueMultiDictionary<string, PathNode> _pathToNodes;
    private int _suspend;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathTreeModel"/> class.
    /// </summary>
    protected PathTreeModel()
    {
        _rootNode = new DummyNode(this);
        _pathToNodes = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all top-level nodes in the tree.
    /// </summary>
    public IEnumerable<PathNode> Nodes => _rootNode.NodeList.Nodes.Pass();

    /// <summary>
    /// Gets the root dummy node of the tree.
    /// </summary>
    public PathNode RootNode => _rootNode;

    /// <summary>
    /// Adds a node to the root of the tree.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void Add(PathNode node)
    {
        _rootNode.NodeList.Add(node);
        node.NotifyView();

        (node as PopulatePathNode)?.PopulateUpdate();
    }

    /// <summary>
    /// Removes a node from the root of the tree.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was removed; otherwise, false.</returns>
    public bool Remove(PathNode node)
    {
        return _rootNode.NodeList.Remove(node);
    }

    /// <summary>
    /// Removes a node at the specified index from the root.
    /// </summary>
    /// <param name="index">The zero-based index of the node to remove.</param>
    public void RemoveAt(int index)
    {
        _rootNode.NodeList.RemoveAt(index);
    }

    /// <summary>
    /// Removes a range of nodes from the root.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to remove.</param>
    /// <param name="count">The number of nodes to remove.</param>
    public void RemoveRange(int index, int count)
    {
        SuspendLayout();

        _rootNode.NodeList.RemoveRange(index, count);

        ResumeLayout();
    }

    /// <summary>
    /// Removes all nodes from the root that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The delegate that defines the conditions of the elements to remove.</param>
    public void RemoveAll(Predicate<PathNode> predicate)
    {
        _rootNode.NodeList.RemoveAll(predicate);
    }

    /// <summary>
    /// Clears all nodes and path registrations from the tree.
    /// </summary>
    public void Clear()
    {
        SuspendLayout();

        _rootNode.NodeList.Clear();
        _pathToNodes.Clear();

        ResumeLayout();
    }

    /// <summary>
    /// Gets the number of top-level nodes in the tree.
    /// </summary>
    public int Count => _rootNode.NodeList.Count;

    /// <summary>
    /// Triggers a populate update for all top-level nodes.
    /// </summary>
    public void PopulateUpdate()
    {
        foreach (var node in _rootNode.NodeList)
        {
            (node as PopulatePathNode)?.PopulateUpdate();
        }
    }

    /// <summary>
    /// Triggers a deep populate update for all top-level nodes and their children.
    /// </summary>
    public void PopulateUpdateDeep()
    {
        foreach (var node in _rootNode.NodeList)
        {
            (node as PopulatePathNode)?.PopulateUpdateDeep();
        }
    }

    /// <summary>
    /// Triggers a deep populate update for all top-level nodes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of nodes to update.</typeparam>
    public void PopulateUpdateDeep<T>()
    {
        foreach (var node in _rootNode.NodeList)
        {
            (node as PopulatePathNode)?.PopulateUpdateDeep<T>();
        }
    }

    /// <summary>
    /// Sets the expanded state of the specified node.
    /// </summary>
    /// <param name="node">The node to expand or collapse.</param>
    /// <param name="expand">True to expand; false to collapse.</param>
    public void SetIsExpanded(PathNode node, bool expand)
    {
        if (node is PopulatePathNode pNode)
        {
            pNode.Expanded = expand;
        }
    }

    /// <summary>
    /// Expands all nodes in the tree recursively.
    /// </summary>
    public void ExpandAll()
    {
        PopulateUpdateDeep();

        foreach (var childNode in _rootNode.NodeList)
        {
            if (childNode is PopulatePathNode pNode)
            {
                ExpandDeep(pNode, true);
            }
        }
    }

    private void ExpandDeep(PopulatePathNode node, bool expand)
    {
        node.Expanded = expand;

        foreach (var childNode in node.NodeList)
        {
            if (childNode is PopulatePathNode pNode)
            {
                ExpandDeep(pNode, expand);
            }
        }
    }


    /// <summary>
    /// Suspends layout updates to batch multiple changes.
    /// </summary>
    protected internal void SuspendLayout()
    {
        _suspend++;

        OnSuspendLayout();
    }

    /// <summary>
    /// Called when layout updates are suspended.
    /// </summary>
    protected virtual void OnSuspendLayout()
    { }

    /// <summary>
    /// Resumes layout updates and triggers a refresh if all suspensions are cleared.
    /// </summary>
    protected internal void ResumeLayout()
    {
        if (_suspend == 0)
        {
            return;
        }

        _suspend--;
        if (_suspend == 0)
        {
            OnResumeLayout();
        }
    }

    /// <summary>
    /// Called when layout updates are resumed.
    /// </summary>
    protected virtual void OnResumeLayout()
    { }

    /// <summary>
    /// Gets a value indicating whether layout updates are currently suspended.
    /// </summary>
    protected internal bool Suspended => _suspend > 0;

    #region IPathTreeModel

    public virtual PathNode GetNode(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string id = path.GetPathId();

        return _pathToNodes[id].FirstOrDefault();
    }

    public virtual object GetPathObject(PathNode node) => null;

    public virtual void OnNodeChanged(PathNode node)
    { }

    public virtual void OnStructureChanged()
    { }

    public virtual void OnStructureChanged(PathNode node)
    { }

    public virtual void OnNodeInserted(PathNode parent, int index, PathNode node)
    { }

    public virtual void OnNodeRemoved(PathNode parent, int index, PathNode node)
    { }

    #endregion

    #region Path

    protected void RegisterPath(PathNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException();
        }

        if (string.IsNullOrEmpty(node.NodeId))
        {
            return;
        }
        //if (_pathToNodes.ContainsKey(node.NodeKey))
        //{
        //    throw new InvalidOperationException();
        //}

        _pathToNodes.Add(node.NodeId, node);

        foreach (var childNode in node.NodeList)
        {
            RegisterPath(childNode);
        }
    }

    protected void UnregisterPath(PathNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException();
        }

        if (string.IsNullOrEmpty(node.NodeId))
        {
            return;
        }

        //PathNode curNode = _pathToNodes.GetValueOrDefault(node.NodeKey);
        //if (curNode != null && curNode != node)
        //{
        //    throw new InvalidOperationException();
        //}

        _pathToNodes.Remove(node.NodeId, node);

        foreach (var childNode in node.NodeList)
        {
            UnregisterPath(childNode);
        }
    }

    public bool ContainsNodeKey(string nodePath)
    {
        if (string.IsNullOrEmpty(nodePath))
        {
            return false;
        }

        string nodeKey = nodePath.GetPathId();

        return _pathToNodes.ContainsKey(nodeKey);
    }

    #endregion
}