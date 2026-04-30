using System;

namespace Suity.Views.PathTree;

/// <summary>
/// Provides extended operations for root directory nodes, including file system watching and node lookup.
/// </summary>
internal abstract class RootDirectoryNodeEx
{
    /// <summary>
    /// Starts the file system watcher for this root directory.
    /// </summary>
    public abstract void StartWatcher();

    /// <summary>
    /// Stops the file system watcher for this root directory.
    /// </summary>
    public abstract void StopWatcher();

    /// <summary>
    /// Executes an action when the directory is not being watched.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public abstract void UnwatchedAction(Action action);

    /// <summary>
    /// Handles a file system rename event for the specified node.
    /// </summary>
    /// <param name="node">The file system node being renamed.</param>
    /// <param name="newName">The new name for the node.</param>
    /// <param name="message">Whether to display a message on rename.</param>
    /// <returns>True if the rename was handled successfully.</returns>
    public abstract bool HandleFileSystemRename(FsNode node, string newName, bool message);

    /// <summary>
    /// Handles moving a path node to a new path location.
    /// </summary>
    /// <param name="node">The node being moved.</param>
    /// <param name="newPath">The new path for the node.</param>
    public abstract void HandleMove(PathNode node, string newPath);

    /// <summary>
    /// Finds a node by its path identifier.
    /// </summary>
    /// <param name="pathId">The path identifier to search for.</param>
    /// <returns>The matching path node, or null if not found.</returns>
    public abstract PathNode FindNode(string pathId);

    /// <summary>
    /// Finds a node by its full file system path.
    /// </summary>
    /// <param name="fullPath">The full path to search for.</param>
    /// <returns>The matching path node, or null if not found.</returns>
    public abstract PathNode FindNodeByFullPath(string fullPath);

    /// <summary>
    /// Finds a node by its relative path.
    /// </summary>
    /// <param name="rPath">The relative path to search for.</param>
    /// <returns>The matching path node, or null if not found.</returns>
    public abstract PathNode FindNodeByRelativePath(string rPath);
}
