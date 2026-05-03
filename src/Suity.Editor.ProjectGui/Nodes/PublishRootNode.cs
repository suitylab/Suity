using Suity.Drawing;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Root node representing the local publish folder in the project tree.
/// </summary>
[ToolTipsText("Local Publish Folder")]
internal class PublishRootNode(string path) : RootDirectoryNode(path), IDropTarget
{
    /// <inheritdoc/>
    public override ImageDef Image => CoreIconCache.Home.ToIconSmall();

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode() => new PublishDirectoryNode();

    /// <inheritdoc/>
    protected override FileNode CreateFileNode() => new PublishFileNode();

    /// <inheritdoc/>
    protected override string OnGetText() => "Publish";

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        HandleDragOver(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        HandleDragDrop(this, e);
    }

    #endregion

    /// <summary>
    /// Handles drag-over operations for publish nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static void HandleDragOver(DirectoryNode dirNode, IDragEvent e)
    {
        //var view = Device.Current.GetService<ICloudPublishView>();

        //view.HandleDragOver(e);
    }

    /// <summary>
    /// Handles drag-and-drop operations for publish nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static void HandleDragDrop(DirectoryNode dirNode, IDragEvent e)
    {
        //var view = Device.Current.GetService<ICloudPublishView>();

        //view.HandleDragDrop(e);
    }
}

/// <summary>
/// Represents a directory within the publish folder.
/// </summary>
internal class PublishDirectoryNode : DirectoryNode, IDropTarget
{
    /// <inheritdoc/>
    public override ImageDef Image => CoreIconCache.Folder.ToIconSmall();

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode()
    {
        return new PublishDirectoryNode();
    }

    /// <inheritdoc/>
    protected override FileNode CreateFileNode()
    {
        return new PublishFileNode();
    }

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        PublishRootNode.HandleDragOver(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        PublishRootNode.HandleDragDrop(this, e);
    }

    #endregion
}

/// <summary>
/// Represents a file within the publish folder.
/// </summary>
internal class PublishFileNode : FileNode
{
    /// <inheritdoc/>
    public override ImageDef Image
    {
        get
        {
            if (string.Equals(Path.GetExtension(NodePath), ".suitygalaxy", StringComparison.OrdinalIgnoreCase))
            {
                return CoreIconCache.Galaxy.ToIconSmall();
            }
            if (string.Equals(Path.GetExtension(NodePath), ".suitypackage", StringComparison.OrdinalIgnoreCase))
            {
                return CoreIconCache.Package.ToIconSmall();
            }
            if (string.Equals(Path.GetExtension(NodePath), ".suitylibrary", StringComparison.OrdinalIgnoreCase))
            {
                return CoreIconCache.Library.ToIconSmall();
            }

            return EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();
        }
    }

    /// <inheritdoc/>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        ClearPopulate();
        // If renamed and then PopulateDummy() is executed->CanPopulate() is called->Asset not found

        bool ok = base.MoveNode(newNodePath, results);

        Populate();

        return ok;
    }

    /// <summary>
    /// Gets the extended image for this node (always null).
    /// </summary>
    public ImageDef ImageEx => null;

    /// <summary>
    /// Gets the status image for this node (always null).
    /// </summary>
    public ImageDef ImageStatus => null;
}