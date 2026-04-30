using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// Represents the root directory node in a path tree, providing file system watching and node operations.
/// </summary>
public class RootDirectoryNode : DirectoryNode
{
    internal static Func<RootDirectoryNode, RootDirectoryNodeEx> _exFactory;

    private readonly RootDirectoryNodeEx _ex;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootDirectoryNode"/> class with the specified node path.
    /// </summary>
    /// <param name="nodePath">The path for this root directory node.</param>
    public RootDirectoryNode(string nodePath)
    {
        SetupNodePath(nodePath);
        _ex = _exFactory(this);
    }

    /// <summary>
    /// Event raised when a user requests to rename a node.
    /// </summary>
    public event EventHandler<UserRenamingEventArgs> UserRenaming;

    #region Watcher

    /// <summary>
    /// Called when this node is added to the tree. Starts the file system watcher.
    /// </summary>
    protected internal override void OnAdded()
    {
        base.OnAdded();

        _ex.StartWatcher();
    }

    /// <summary>
    /// Called when this node is removed from the tree. Stops the file system watcher.
    /// </summary>
    /// <param name="fromParent">The parent node from which this node was removed.</param>
    protected internal override void OnRemoved(PathNode fromParent)
    {
        base.OnRemoved(fromParent);

        _ex.StopWatcher();
    }

    /// <summary>
    /// Called when this node is renamed. Restarts the file system watcher with the new path.
    /// </summary>
    /// <param name="oldName">The previous name of this node.</param>
    protected override void OnRenamed(string oldName)
    {
        base.OnRenamed(oldName);

        // Root path changed, refresh Watcher
        _ex.StopWatcher();

        if (Parent != null)
        {
            _ex.StartWatcher();
        }
    }

    /// <summary>
    /// Executes an action while the file system watcher is stopped, then restarts it.
    /// </summary>
    /// <param name="action">The action to execute without watching.</param>
    public void UnwatchedAction(Action action) => _ex.UnwatchedAction(action);

    #endregion

    #region Node Operations

    /// <summary>
    /// Handles a file system rename operation for the specified node.
    /// </summary>
    /// <param name="node">The file system node being renamed.</param>
    /// <param name="newName">The new name for the node.</param>
    /// <param name="message">Whether to show a message on failure.</param>
    /// <returns>True if the rename was handled successfully; otherwise, false.</returns>
    internal bool HandleFileSystemRename(FsNode node, string newName, bool message)
        => _ex.HandleFileSystemRename(node, newName, message);

    /// <summary>
    /// Handles moving a path node to a new location.
    /// </summary>
    /// <param name="node">The node to move.</param>
    /// <param name="newPath">The new path for the node.</param>
    public void HandleMove(PathNode node, string newPath)
        => _ex.HandleMove(node, newPath);

    #endregion

    #region Virtual

    /// <summary>
    /// Gets a value indicating whether the text of this node can be edited. Always returns false for root nodes.
    /// </summary>
    public override bool CanEditText => false;
    /// <summary>
    /// Gets a value indicating whether file nodes should be populated under this root. Default is true.
    /// </summary>
    public virtual bool PopulateFile => true;

    /// <summary>
    /// Called when the user requests to change the text of this node.
    /// </summary>
    /// <param name="newName">The new text value.</param>
    protected override void OnUserRequestChangeText(string newName)
    {
        //bool watching = _watcher != null;

        //if (watching)
        //{
        //    StopWatcher();
        //}
        base.OnUserRequestChangeText(newName);
        //if (watching)
        //{
        //    StartWatcher();
        //}
    }

    /// <summary>
    /// Called when a file system change occurs at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the changed file or directory.</param>
    protected internal virtual void OnFileSystemChanged(string fullPath)
    { }

    /// <summary>
    /// Called when a file system item is created at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the created item.</param>
    protected internal virtual void OnFileSystemCreated(string fullPath)
    { }

    /// <summary>
    /// Called when a file system item is deleted at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the deleted item.</param>
    protected internal virtual void OnFileSystemDeleted(string fullPath)
    { }

    /// <summary>
    /// Called when a file system item is renamed.
    /// </summary>
    /// <param name="fullPath">The new full path of the renamed item.</param>
    /// <param name="oldFullPath">The previous full path of the item.</param>
    protected internal virtual void OnFileSystemRenamed(string fullPath, string oldFullPath)
    { }

    #endregion

    /// <summary>
    /// Finds a node by its path ID.
    /// </summary>
    /// <param name="pathId">The path ID to search for.</param>
    /// <returns>The found node, or null if not found.</returns>
    public PathNode FindNode(string pathId)
        => _ex.FindNode(pathId);

    /// <summary>
    /// Finds a node by its full path.
    /// </summary>
    /// <param name="fullPath">The full path to search for.</param>
    /// <returns>The found node, or null if not found.</returns>
    public PathNode FindNodeByFullPath(string fullPath)
        => _ex.FindNodeByFullPath(fullPath);

    /// <summary>
    /// Finds a node by its relative path.
    /// </summary>
    /// <param name="rPath">The relative path to search for.</param>
    /// <returns>The found node, or null if not found.</returns>
    public PathNode FindNodeByRelativePath(string rPath)
        => _ex.FindNodeByRelativePath(rPath);

    /// <summary>
    /// Raises the UserRenaming event for the specified node.
    /// </summary>
    /// <param name="node">The node being renamed.</param>
    /// <param name="newPath">The new path for the node.</param>
    protected internal void RaiseUserRenaming(PathNode node, string newPath)
    {
        if (UserRenaming != null)
        {
            UnwatchedAction(() =>
            {
                UserRenamingEventArgs args = new UserRenamingEventArgs(node.NodePath, newPath, () =>
                {
                    HashSet<RenameItem> renameItems = new HashSet<RenameItem>();
                    node.MoveNode(newPath, renameItems);
                    return renameItems.ToArray();
                });
                UserRenaming(this, args);
            });
        }
        else
        {
            UnwatchedAction(() =>
            {
                node.MoveNode(newPath, null);
            });
        }
    }
}