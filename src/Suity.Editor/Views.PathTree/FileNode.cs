using Suity.Editor;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Suity.Views.PathTree;

/// <summary>
/// Represents a file node in the path tree, supporting file operations such as delete, move, and rename.
/// </summary>
public class FileNode : FsNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileNode"/> class.
    /// </summary>
    public FileNode()
    { }

    /// <summary>
    /// Deletes the file associated with this node.
    /// </summary>
    /// <param name="sendToRecycleBin">If true, sends the file to the recycle bin; otherwise, permanently deletes it.</param>
    public override void Delete(bool sendToRecycleBin)
    {
        if (sendToRecycleBin)
        {
            EditorUtility.SendToRecycleBin(NodePath);
        }
        else
        {
            File.Delete(NodePath);
        }
        base.Delete(sendToRecycleBin);
    }

    /// <summary>
    /// Moves the file to a new path and updates the node accordingly.
    /// </summary>
    /// <param name="newNodePath">The new file path.</param>
    /// <param name="results">A set to collect rename results.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        if (File.Exists(NodePath))
        {
            try
            {
                File.Move(NodePath, newNodePath);
            }
            catch (Exception e)
            {
                e.LogError($"Failed to rename {NodePath}");
                return false;
            }
        }

        if (base.MoveNode(newNodePath, results))
        {
            return true;
        }
        else
        {
            Populate();
            return false;
        }
    }

    /// <summary>
    /// Validates and sets the node path for this file node.
    /// </summary>
    /// <param name="nodePath">The file path to set. Must be non-null and have a valid file name.</param>
    protected override void OnSetupNodePath(string nodePath)
    {
        if (string.IsNullOrEmpty(nodePath))
        {
            throw new ArgumentNullException();
        }
        if (string.IsNullOrEmpty(Path.GetFileName(nodePath)))
        {
            throw new ArgumentException();
        }

        base.OnSetupNodePath(nodePath);
    }

    /// <summary>
    /// Gets the type name of this file, derived from its file extension.
    /// </summary>
    public override string TypeName => Path.GetExtension(NodePath).TrimStart('.');

    /// <summary>
    /// Whether this node hides the file extension.
    /// If hidden, the file extension will be automatically added during renaming.
    /// </summary>
    public virtual bool ExtensionHidden => false;

    /// <summary>
    /// Handles a user request to change the display text, which triggers a file rename.
    /// </summary>
    /// <param name="newName">The new name requested by the user.</param>
    protected override void OnUserRequestChangeText(string newName)
    {
        string oldName = Path.GetFileNameWithoutExtension(Terminal);
        if (newName == oldName)
        {
            return;
        }

        if (ExtensionHidden)
        {
            newName = newName + Path.GetExtension(Terminal);
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

    /// <summary>
    /// Returns the display text for this file node, optionally excluding the file extension.
    /// </summary>
    /// <returns>The file name with or without extension, based on <see cref="ExtensionHidden"/>.</returns>
    protected override string OnGetText()
    {
        if (ExtensionHidden)
        {
            return Path.GetFileNameWithoutExtension(Terminal); 
        }
        else
        {
            return Path.GetFileName(Terminal);
        }
    }

    /// <summary>
    /// Gets the icon image associated with this file based on its path.
    /// </summary>
    public override Image Image => EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();
}
