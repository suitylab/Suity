using Suity.Drawing;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System.Drawing;
using System.IO;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents an asset directory node in the project tree view.
/// </summary>
public class AssetDirectoryNode : DirectoryNode, IAssetDirectoryNode, IAssetFsNode, IDropTarget
{
    /// <inheritdoc/>
    public override ImageDef Image => CoreIconCache.Folder.ToIconSmall();

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode()
    {
        return new AssetDirectoryNode();
    }

    /// <inheritdoc/>
    protected override FileNode CreateFileNode()
    {
        return new AssetFileNode();
    }

    /// <inheritdoc/>
    protected override bool CanPopulateFile(FileInfo file)
    {
        // Hide attached files when their origin file exists
        if (file.GetIsAttachedFile())
        {
            string originFileName = file.FullName.RemoveExtension();
            if (File.Exists(originFileName))
            {
                return false;
            }
        }

        return base.CanPopulateFile(file);
    }

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        AssetRootNode.HandleProjectDragOver(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        AssetRootNode.HandleProjectDragDrop(this, e);
    }

    #endregion
}