namespace Suity.Views.PathTree;

/// <summary>
/// Defines the interface for a path tree model, providing node lookup and structure change notifications.
/// </summary>
public interface IPathTreeModel
{
    /// <summary>
    /// Gets the node associated with the specified node key.
    /// </summary>
    /// <param name="nodeKey">The node key to search for.</param>
    /// <returns>The node if found; otherwise, null.</returns>
    PathNode GetNode(string nodeKey);

    /// <summary>
    /// Gets the path object associated with the specified node.
    /// </summary>
    /// <param name="node">The node to get the path object for.</param>
    /// <returns>The path object, or null.</returns>
    object GetPathObject(PathNode node);

    /// <summary>
    /// Called when a node's properties have changed.
    /// </summary>
    /// <param name="node">The node that changed.</param>
    void OnNodeChanged(PathNode node);

    /// <summary>
    /// Called when the overall tree structure has changed.
    /// </summary>
    void OnStructureChanged();

    /// <summary>
    /// Called when the tree structure has changed at the specified node.
    /// </summary>
    /// <param name="node">The node where the structure changed.</param>
    void OnStructureChanged(PathNode node);

    /// <summary>
    /// Called when a node has been inserted into the tree.
    /// </summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="index">The index at which the node was inserted.</param>
    /// <param name="node">The inserted node.</param>
    void OnNodeInserted(PathNode parent, int index, PathNode node);

    /// <summary>
    /// Called when a node has been removed from the tree.
    /// </summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="index">The index from which the node was removed.</param>
    /// <param name="node">The removed node.</param>
    void OnNodeRemoved(PathNode parent, int index, PathNode node);
}