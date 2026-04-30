using Suity.Editor;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Views.PathTree;

/// <summary>
/// Represents a directory node in the path tree, supporting directory enumeration, file system operations, and sorted insertion of child nodes.
/// </summary>
public class DirectoryNode : FsNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryNode"/> class.
    /// </summary>
    public DirectoryNode()
    { }

    /// <summary>
    /// Validates and sets the node path for this directory node.
    /// </summary>
    /// <param name="nodePath">The directory path to set. Must be non-null.</param>
    protected override void OnSetupNodePath(string nodePath)
    {
        base.OnSetupNodePath(nodePath);
        if (string.IsNullOrEmpty(nodePath))
        {
            throw new ArgumentNullException();
        }
    }

    /// <summary>
    /// Determines whether this directory can be populated with child nodes.
    /// </summary>
    /// <returns>True if the directory exists and contains child items; otherwise, false.</returns>
    protected override bool CanPopulate()
    {
        DirectoryInfo dirInfo = new(NodePath);
        if (!dirInfo.Exists)
        {
            return false;
        }

        //This operation can be executed without a Parent
        //Therefore, describer may not be found

        var root = FindMeOrParent<RootDirectoryNode>();

        try
        {
            if (root != null && !root.PopulateFile)
            {
                return dirInfo.EnumerateDirectories().Any();
            }
            else
            {
                return dirInfo.EnumerateDirectories().Any() || dirInfo.EnumerateFiles().Any();
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    [ThreadStatic]
    static readonly List<PathNode> _tempNodeListDir = [];

    [ThreadStatic]
    static readonly List<PathNode> _tempNodeListFile = [];

    /// <summary>
    /// Enumerates the child directories and files of this directory as path nodes.
    /// </summary>
    /// <returns>An enumerable of child path nodes representing subdirectories and files.</returns>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        var root = FindMeOrParent<RootDirectoryNode>();

        var dirInfo = new DirectoryInfo(NodePath);
        if (!dirInfo.Exists)
        {
            return [];
        }

        _tempNodeListDir.Clear();
        _tempNodeListFile.Clear();

        foreach (DirectoryInfo childDirInfo in dirInfo.EnumerateDirectories())
        {
            if (CanPopulateDirectory(childDirInfo))
            {
                DirectoryNode childDirNode = _CreateDirectoryNode();
                childDirNode.SetupNodePath(childDirInfo.FullName + "\\");
                _tempNodeListDir.Add(childDirNode);
            }
        }

        if (root == null || root.PopulateFile)
        {
            foreach (FileInfo childFileInfo in dirInfo.EnumerateFiles())
            {
                if (CanPopulateFile(childFileInfo))
                {
                    FileNode childFileNode = _CreateFileNode();
                    childFileNode.SetupNodePath(childFileInfo.FullName);
                    _tempNodeListFile.Add(childFileNode);
                }
            }
        }

        _tempNodeListDir.Sort((a, b) => string.Compare(a.NodePath, b.NodePath));
        _tempNodeListFile.Sort((a, b) => string.Compare(a.NodePath, b.NodePath));

        var ary = _tempNodeListDir.Concat(_tempNodeListFile).ToArray();

        _tempNodeListDir.Clear();
        _tempNodeListFile.Clear();

        return ary;
    }

    /// <summary>
    /// Creates a new directory node instance. Validates that the node has no existing path or parent.
    /// </summary>
    /// <returns>A new directory node.</returns>
    internal DirectoryNode _CreateDirectoryNode()
    {
        var node = CreateDirectoryNode();
        if (!string.IsNullOrEmpty(node.NodePath) || node.Parent != null)
        {
            throw new InvalidOperationException();
        }

        return node;
    }

    /// <summary>
    /// Creates a new file node instance. Validates that the node has no existing path or parent.
    /// </summary>
    /// <returns>A new file node.</returns>
    internal FileNode _CreateFileNode()
    {
        var node = CreateFileNode();
        if (!string.IsNullOrEmpty(node.NodePath) || node.Parent != null)
        {
            throw new InvalidOperationException();
        }

        return node;
    }

    /// <summary>
    /// Factory method for creating directory child nodes. Override to provide custom directory node types.
    /// </summary>
    /// <returns>A new directory node instance.</returns>
    protected virtual DirectoryNode CreateDirectoryNode() => new();

    /// <summary>
    /// Factory method for creating file child nodes. Override to provide custom file node types.
    /// </summary>
    /// <returns>A new file node instance.</returns>
    protected virtual FileNode CreateFileNode() => new();

    /// <summary>
    /// Determines whether a directory should be included during population.
    /// </summary>
    /// <param name="directory">The directory info to evaluate.</param>
    /// <returns>True if the directory should be populated; otherwise, false.</returns>
    protected virtual bool CanPopulateDirectory(DirectoryInfo directory) => true;

    /// <summary>
    /// Determines whether a file should be included during population. Excludes hidden files by default.
    /// </summary>
    /// <param name="file">The file info to evaluate.</param>
    /// <returns>True if the file should be populated; otherwise, false.</returns>
    protected virtual bool CanPopulateFile(FileInfo file)
    {
        if (file is null)
        {
            return false;
        }

        if (file.Attributes.HasFlag(FileAttributes.Hidden))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Deletes the directory associated with this node.
    /// </summary>
    /// <param name="sendToRecycleBin">If true, sends the directory to the recycle bin; otherwise, permanently deletes it.</param>
    public override void Delete(bool sendToRecycleBin)
    {
        if (sendToRecycleBin)
        {
            EditorUtility.SendToRecycleBin(NodePath);
        }
        else
        {
            Directory.Delete(NodePath);
        }
        base.Delete(sendToRecycleBin);
    }

    /// <summary>
    /// Moves the directory and all its children to a new path.
    /// </summary>
    /// <param name="newNodePath">The new directory path.</param>
    /// <param name="results">A set to collect rename results.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        //Ensure child nodes are listed before renaming child nodes
        EnsurePopulateDeep(true);

        if (Directory.Exists(NodePath))
        {
            try
            {
                Directory.Move(NodePath, newNodePath);
            }
            catch (Exception err)
            {
                err.LogError();
                return false;
            }
        }

        bool expended = Expanded;

        var nodes = NodeList.ToArray();
        foreach (PathNode childNode in nodes)
        {
            if (childNode is DummyNode) continue;

            string childNewNodePath = Path.Combine(newNodePath, childNode.Terminal);
            childNode.ChangeNodePath(childNewNodePath, results);
        }


        // This low-level operation must be placed after asset refactoring, otherwise it will cause references to become invalid.
        base.MoveNode(newNodePath, results);

        Expanded = expended;

        //Populate();
        EnsurePopulateDeep(true);

        return true;
    }

    /// <summary>
    /// Changes the node path for this directory and all its children.
    /// </summary>
    /// <param name="newNodePath">The new directory path.</param>
    /// <param name="results">A set to collect rename results.</param>
    public override void ChangeNodePath(string newNodePath, HashSet<RenameItem> results)
    {
        //Ensure child nodes are listed before renaming child nodes
        EnsurePopulate();

        var nodes = NodeList.ToArray();
        foreach (PathNode childNode in nodes)
        {
            if (childNode is DummyNode) continue;

            string childNewNodePath = Path.Combine(newNodePath, childNode.Terminal);
            childNode.ChangeNodePath(childNewNodePath, results);
        }

        // This line must be placed last, otherwise oldNodePath will become newNodePath and the oldNodePath in the foreach above will become invalid.
        base.ChangeNodePath(newNodePath, results);
    }

    /// <summary>
    /// Inserts a node into the child list in sorted order based on its type.
    /// </summary>
    /// <param name="node">The node to insert.</param>
    internal override void InsertNodeSorted(PathNode node)
    {
        if (node is FileNode fileNode)
        {
            InsertFileNodeSorted(fileNode);
        }
        else if (node is DirectoryNode directoryNode)
        {
            InsertDirectoryNodeSorted(directoryNode);
        }
        else
        {
            //throw new InvalidOperationException();
            //Default to insert at beginning
            NodeList.Insert(0, node);
        }
    }

    /// <summary>
    /// Inserts a file node into the child list in sorted order by path.
    /// </summary>
    /// <param name="fileNode">The file node to insert.</param>
    internal void InsertFileNodeSorted(FileNode fileNode)
    {
        for (int i = 0; i < NodeList.Count; i++)
        {
            FileNode currentNode = NodeList[i] as FileNode;
            if (currentNode == fileNode) continue;
            if (currentNode != null)
            {
                if (string.Compare(fileNode.NodePath, currentNode.NodePath) < 0)
                {
                    NodeList.Insert(i, fileNode);
                    return;
                }
            }
        }
        NodeList.Add(fileNode);
    }

    /// <summary>
    /// Inserts a directory node into the child list in sorted order, ensuring directories appear before files.
    /// </summary>
    /// <param name="dirNode">The directory node to insert.</param>
    internal void InsertDirectoryNodeSorted(DirectoryNode dirNode)
    {
        for (int i = 0; i < NodeList.Count; i++)
        {
            if (NodeList[i] is FileNode)
            {
                NodeList.Insert(i, dirNode);
                return;
            }

            DirectoryNode currentNode = NodeList[i] as DirectoryNode;
            if (currentNode == dirNode) continue;
            if (currentNode != null)
            {
                if (string.Compare(dirNode.NodePath, currentNode.NodePath) < 0)
                {
                    NodeList.Insert(i, dirNode);
                    return;
                }
            }
        }
        NodeList.Add(dirNode);
    }

    /// <summary>
    /// Handles a user request to change the display text, which triggers a directory rename.
    /// </summary>
    /// <param name="newName">The new name requested by the user.</param>
    protected override void OnUserRequestChangeText(string newName)
    {
        string oldName = Terminal;
        if (newName == oldName)
        {
            return;
        }

        RootDirectoryNode root = FindMeOrParent<RootDirectoryNode>();
        if (root != null)
        {
            if (root.HandleFileSystemRename(this, newName, true))
            {
                OnRenamed(oldName);
            }
        }
        else
        {
            base.OnUserRequestChangeText(newName);
        }
    }
}
