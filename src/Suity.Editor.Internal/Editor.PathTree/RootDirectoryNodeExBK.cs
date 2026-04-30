using Suity.Editor;
using Suity.Helpers;
using System;
using System.IO;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// Extended root directory node with file system watching and external notification capabilities.
/// </summary>
internal class RootDirectoryNodeExBK(RootDirectoryNode node) : RootDirectoryNodeEx
{
    private readonly RootDirectoryNode _node = node ?? throw new ArgumentNullException(nameof(node));
    private EditorFileSystemWatcher _watcher;

    private bool _pausing;

    #region Watcher

    /// <inheritdoc/>
    public override void StartWatcher()
    {
        if (_watcher != null)
        {
            return;
        }

        try
        {
            if (Directory.Exists(_node.NodePath))
            {
                _watcher = new EditorFileSystemWatcher(_node.NodePath, this)
                {
                    IncludeSubdirectories = true,
                    Delayed = false,
                };

                _watcher.Changed += watcher_Changed;
                _watcher.Created += watcher_Created;
                _watcher.Deleted += watcher_Deleted;
                _watcher.Renamed += watcher_Renamed;

                _watcher.EnableRaisingEvents = true;
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <inheritdoc/>
    public override void StopWatcher()
    {
        if (_watcher != null)
        {
            _watcher.Changed -= watcher_Changed;
            _watcher.Created -= watcher_Created;
            _watcher.Deleted -= watcher_Deleted;
            _watcher.Renamed -= watcher_Renamed;

            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    /// <summary>
    /// Handles file change events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the changed file or directory.</param>
    private void watcher_Changed(string fullPath)
    {
        QueuedAction.Do(() => _node.OnFileSystemChanged(fullPath));
    }

    /// <summary>
    /// Handles file creation events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the created file or directory.</param>
    private void watcher_Created(string fullPath)
    {
        QueuedAction.Do(() => _node.OnFileSystemCreated(fullPath));
        QueuedAction.Do(() => ExternalNotifyAddNode(fullPath));
    }

    /// <summary>
    /// Handles file deletion events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the deleted file or directory.</param>
    private void watcher_Deleted(string fullPath)
    {
        QueuedAction.Do(() => _node.OnFileSystemDeleted(fullPath));
        QueuedAction.Do(() => ExternalNotifyRemoveNode(fullPath));
    }

    /// <summary>
    /// Handles file rename events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The new full path after renaming.</param>
    /// <param name="oldFullPath">The original full path before renaming.</param>
    private void watcher_Renamed(string fullPath, string oldFullPath)
    {
        QueuedAction.Do(() => _node.OnFileSystemRenamed(fullPath, oldFullPath));
        QueuedAction.Do(() => ExternalNotifyRenameNode(fullPath, oldFullPath));
    }

    /// <summary>
    /// Notifies the tree model to add a new node for the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the new file or directory.</param>
    public void ExternalNotifyAddNode(string fullPath)
    {
        var model = _node.FindModel();
        if (model == null)
        {
            return;
        }

        if (model.GetNode(fullPath) != null)
        {
            return;
        }

        bool isFile;
        if (Directory.Exists(fullPath))
        {
            isFile = false;
        }
        else if (File.Exists(fullPath))
        {
            isFile = true;
        }
        else
        {
            return;
        }

        string parentNodePath = Path.GetDirectoryName(fullPath);
        if (model.GetNode(parentNodePath) is DirectoryNode parentDirNode)
        {
            if (isFile)
            {
                if (_node.PopulateFile)
                {
                    FileNode newFileNode = parentDirNode._CreateFileNode();
                    newFileNode.SetupNodePath(fullPath);
                    parentDirNode.InsertFileNodeSorted(newFileNode);
                }
            }
            else
            {
                DirectoryNode newDirNode = parentDirNode._CreateDirectoryNode();
                newDirNode.SetupNodePath(fullPath);
                parentDirNode.InsertDirectoryNodeSorted(newDirNode);
            }
        }
    }

    /// <summary>
    /// Notifies the tree model to remove the node at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path of the node to remove.</param>
    public void ExternalNotifyRemoveNode(string fullPath)
    {
        var model = _node.FindModel();
        if (model == null) return;

        PathNode node = model.GetNode(fullPath);
        node?.Parent?.NodeList.Remove(node);
    }

    /// <summary>
    /// Notifies the tree model to rename a node from one path to another.
    /// </summary>
    /// <param name="fullPath">The new full path after renaming.</param>
    /// <param name="oldFullPath">The original full path before renaming.</param>
    public void ExternalNotifyRenameNode(string fullPath, string oldFullPath)
    {
        var model = _node.FindModel();
        if (model == null) return;

        if (string.IsNullOrEmpty(fullPath)) return;
        if (string.IsNullOrEmpty(oldFullPath)) return;

        fullPath = fullPath.TrimEnd('\\');
        oldFullPath = oldFullPath.TrimEnd('\\');

        PathNode node = model.GetNode(oldFullPath);
        node?.ChangeNodePath(fullPath, null);
    }

    /// <inheritdoc/>
    public override void UnwatchedAction(Action action)
    {
        if (_watcher == null || !_watcher.EnableRaisingEvents)
        {
            action();
            return;
        }

        try
        {
            _watcher.EnableRaisingEvents = false;
            action();
        }
        catch (Exception e)
        {
            e.LogError("UnwatchedAction failed.");
            throw;
        }
        finally
        {
            _watcher.EnableRaisingEvents = true;
        }
    }

    #endregion

    #region Node Operations

    /// <inheritdoc/>
    public override bool HandleFileSystemRename(FsNode node, string newName, bool message)
    {
        if (!NamingVerifier.VerifyFileName(newName))
        {
            if (message)
            {
                DialogUtility.ShowMessageBoxAsync("Invalid file name");
            }

            return false;
        }

        string parentPath = Path.GetDirectoryName(node.NodePath) ?? string.Empty;
        string newPath = Path.Combine(parentPath, newName);

        if (!string.Equals(newName, node.Terminal, StringComparison.InvariantCultureIgnoreCase))
        {
            if (File.Exists(newPath))
            {
                if (message)
                {
                    DialogUtility.ShowMessageBoxAsync("File already exists");
                }

                return false;
            }

            if (Directory.Exists(newPath))
            {
                if (message)
                {
                    DialogUtility.ShowMessageBoxAsync("Folder already exists");
                }

                return false;
            }
        }

        _node.RaiseUserRenaming(node, newPath);

        return true;
    }

    /// <inheritdoc/>
    public override void HandleMove(PathNode node, string newPath)
    {
        if (File.Exists(newPath) || Directory.Exists(newPath))
        {
            return;
        }

        _node.RaiseUserRenaming(node, newPath);
    }

    #endregion

    /// <inheritdoc/>
    public override PathNode FindNode(string pathId)
    {
        var node = _FindNode(pathId);
        if (node != null)
        {
            return node;
        }

        // Handle scode nested search
        if (pathId.FileExtensionEquals(Asset.CodeLibraryExtension))
        {
            string name = Path.GetFileName(pathId);
            string originFileId = pathId.RemoveExtension();

            var originNode = _FindNode(originFileId);

            if (originNode != null)
            {
                (originNode as PopulatePathNode)?.EnsurePopulate();

                var scodeNode = originNode.NodeList.Where(o => o.Terminal == name).FirstOrDefault();
                if (scodeNode != null)
                {
                    return scodeNode;
                }
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override PathNode FindNodeByFullPath(string fullPath)
    {
        string pathId = fullPath.MakeRalativePath(_node.NodePath)?.Replace('/', '\\') ?? string.Empty;

        return FindNode(pathId);
    }

    /// <inheritdoc/>
    public override PathNode FindNodeByRelativePath(string rPath)
    {
        string pathId = rPath?.Replace('/', '\\') ?? string.Empty;

        return FindNode(pathId);
    }

    /// <summary>
    /// Finds a node by traversing the path hierarchy from the root.
    /// </summary>
    /// <param name="pathId">The path identifier to search for.</param>
    /// <returns>The found node, or null if not found.</returns>
    private PathNode _FindNode(string pathId)
    {
        string[] split = pathId.Split('\\');

        PathNode currentNode = _node;
        int index = 0;

        while (index < split.Length)
        {
            if (currentNode is PopulatePathNode populateNode)
            {
                // Force list child nodes to search
                populateNode.EnsurePopulate();
            }

            bool foundChildNode = false;

            foreach (PathNode childNode in currentNode.NodeList)
            {
                if (String.Equals(childNode.Terminal, split[index], StringComparison.OrdinalIgnoreCase))
                {
                    if (index == split.Length - 1)
                    {
                        return childNode;
                    }
                    else
                    {
                        currentNode = childNode;
                        foundChildNode = true;
                        break;
                    }
                }
            }

            if (foundChildNode)
            {
                index++;
            }
            else
            {
                break;
            }
        }

        return null;
    }
}
