using Suity.Editor;
using Suity.Helpers;
using Suity.Views.PathTree;
using System.IO;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides ImGui-based external backend implementation for path tree views, handling drag-and-drop operations for file system nodes.
/// </summary>
internal class ImGuiPathTreeExternalBK : ImGuiPathTreeExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ImGuiPathTreeExternalBK"/>.
    /// </summary>
    public static ImGuiPathTreeExternalBK Instance { get; } = new ImGuiPathTreeExternalBK();

    /// <inheritdoc/>
    public override ImGuiTreeViewExternal<T> CreateTreeViewEx<T>(ImGuiTreeView<T> treeView)
    {
        return new ImGuiTreeViewExternalBK<T>(treeView);
    }

    /// <inheritdoc/>
    public override bool HandleDragOver(ImGuiPathTreeView treeView, IDragEvent dropEvent)
    {
        var droppingNode = treeView.DroppingNode;
        if (droppingNode is null)
        {
            dropEvent.SetNoneEffect();
            return false;
        }

        if (dropEvent.Data.GetDataPresent(typeof(VisualTreeNode[])))
        {
            var input = dropEvent.Data.GetData(typeof(VisualTreeNode[])) as VisualTreeNode[];
            if (input is null)
            {
                dropEvent.SetNoneEffect();
                return false;
            }

            var nodes = input
                .OfType<VisualTreeNode<PathNode>>()
                .Select(o => o.Value)
                .Where(o => !droppingNode.ContainsParent(o)) // Cannot place node inside its own child nodes
                .ToArray();

            if (nodes.Length == 0)
            {
                dropEvent.SetNoneEffect();
                return false;
            }

            bool canDrop = nodes.All(o => o is FsNode && o.CanUserDrag);

            if (canDrop)
            {
                dropEvent.SetCopyEffect();
                return true;
            }
            else
            {
                dropEvent.SetNoneEffect();
                return false;
            }
        }
        else if (dropEvent.Data.GetDataPresent(DragEventData.DataFormat_File))
        {
            dropEvent.SetCopyEffect();
            return true;
        }

        dropEvent.SetNoneEffect();
        return false;
    }

    /// <inheritdoc/>
    public override async void HandleDragDrop(ImGuiPathTreeView treeView, IDragEvent dropEvent)
    {
        DirectoryNode? droppingNode = treeView.DroppingNode as DirectoryNode;
        if (droppingNode is null)
        {
            return;
        }

        if (dropEvent.Data.GetDataPresent(typeof(VisualTreeNode[])))
        {
            var input = dropEvent.Data.GetData(typeof(VisualTreeNode[])) as VisualTreeNode[];
            if (input is null)
            {
                return;
            }

            var nodes = input
                .OfType<VisualTreeNode<PathNode>>()
                .Select(o => o.Value)
                .Where(o => !droppingNode.ContainsParent(o)) // Cannot place node inside its own child nodes
                .ToArray();

            if (nodes.Length == 0)
            {
                return;
            }

            RootDirectoryNode targetRootNode = droppingNode.FindMeOrParent<RootDirectoryNode>();
            if (targetRootNode == null) return;

            bool dup = nodes.Any(o => o != null && o.Parent != droppingNode && droppingNode.NodePath.PathAppend(o.Terminal).FileOrDirectoryExists());
            if (dup)
            {
                bool ask = await DialogUtility.ShowYesNoDialogAsync("File conflict. Overwrite existing file?");

                if (!ask)
                {
                    return;
                }
            }

            if (nodes[0].FindMeOrParent<RootDirectoryNode>() == targetRootNode)
            {
                foreach (PathNode node in nodes.Where(o => o.Parent != droppingNode))
                {
                    string newPath = droppingNode.NodePath.PathAppend(node.Terminal);
                    targetRootNode.HandleMove(node, newPath);
                }
            }
            else
            {
                foreach (PathNode node in nodes.Where(o => o.Parent != droppingNode))
                {
                    string newPath = droppingNode.NodePath.PathAppend(node.Terminal);
                    if (Directory.Exists(node.NodePath))
                    {
                        DirectoryUtility.CopyDirectory(node.NodePath, newPath, true);
                        Directory.Delete(node.NodePath, true);
                    }
                    else if (File.Exists(node.NodePath))
                    {
                        if (File.Exists(newPath))
                        {
                            File.Delete(newPath);
                        }
                        File.Move(node.NodePath, newPath);
                    }
                }
            }

            droppingNode.PopulateUpdate();
        }
        else if (dropEvent.Data.GetDataPresent(DragEventData.DataFormat_File))
        {
            string[] files = (string[])dropEvent.Data.GetData(DragEventData.DataFormat_File);

            bool dup = files.Any(o => droppingNode.NodePath.PathAppend(o.GetPathTerminal()).FileOrDirectoryExists());
            if (dup)
            {
                bool ask = await DialogUtility.ShowYesNoDialogAsync("File conflict. Overwrite existing file?");
                if (!ask)
                {
                    return;
                }
            }

            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    DirectoryInfo info = new(file);
                    string destPath = Path.Combine(droppingNode.NodePath, info.Name);
                    DirectoryUtility.CopyDirectory(file, destPath, true);
                }
                else if (File.Exists(file))
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(droppingNode.NodePath, fileName);
                    File.Copy(file, destPath, true);
                }
            }

            droppingNode.PopulateUpdate();
        }
    }

    /// <inheritdoc/>
    public override PathNode? GetPathTreeDroppingNode(ImGuiNode node, ImTreeNodeDragDropMode mode)
    {
        var myValue = node.GetValue<VisualTreeNode>() as VisualTreeNode<PathNode>;
        if (myValue is null)
        {
            return null;
        }

        var dropNode = myValue.Value;
        if (mode != ImTreeNodeDragDropMode.Inside)
        {
            dropNode = dropNode.Parent;
        }
        else
        {
        }

        return dropNode;
    }
}