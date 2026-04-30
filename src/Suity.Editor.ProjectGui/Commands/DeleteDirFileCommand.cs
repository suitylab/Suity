using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that deletes selected directories, files, or workspace nodes after confirmation.
/// </summary>
internal class DeleteDirFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDirFileCommand"/> class.
    /// </summary>
    public DeleteDirFileCommand()
        : base("Delete", CoreIconCache.Delete.ToIconSmall())
    {
        AcceptType<AssetDirectoryNode>(false);
        AcceptType<AssetFileNode>(false);

        AcceptType<WorkSpaceDirectoryNode>(false);
        AcceptType<WorkSpaceFileNode>(false);
        AcceptType<WorkSpaceRootNode>(false);

        AcceptType<PublishDirectoryNode>(false);
        AcceptType<PublishFileNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleDelete(view);
    }

    /// <summary>
    /// Handles the deletion of selected nodes in the project view.
    /// </summary>
    /// <param name="view">The project GUI view containing selected nodes to delete.</param>
    public static async void HandleDelete(IProjectGui view)
    {
        HashSet<PathNode> selectedNodes = new(view.SelectedNodes);
        if (selectedNodes.Count == 0)
        {
            return;
        }

        var commonType = selectedNodes.GetCommonType();
        if (commonType != typeof(AssetDirectoryNode) &&
            commonType != typeof(AssetFileNode) &&
            commonType != typeof(WorkSpaceDirectoryNode) &&
            commonType != typeof(WorkSpaceFileNode) &&
            commonType != typeof(WorkSpaceRootNode) &&
            commonType != typeof(PublishDirectoryNode) &&
            commonType != typeof(PublishFileNode) &&
            commonType != typeof(FsNode))
        {
            if (selectedNodes.Select(o => o.GetType()).All(type => typeof(IAssetFsNode).IsAssignableFrom(type)))
            {
                commonType = typeof(IAssetFsNode);
            }
            else
            {
                return;
            }
        }

        PopulatePathNode nodeToRefresh = null;

        if (selectedNodes.Count == 1)
        {
            string name = selectedNodes.First().Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = selectedNodes.First().NodePath.GetPathTerminal();
            }

            bool result = await DialogUtility.ShowYesNoDialogAsyncL($"Confirm delete {name}?");
            if (!result)
            {
                return;
            }
        }
        else
        {
            bool result = await DialogUtility.ShowYesNoDialogAsyncL($"Confirm delete these {selectedNodes.Count} items?");
            if (!result)
            {
                return;
            }
        }

        if (commonType == typeof(WorkSpaceDirectoryNode) || commonType == typeof(WorkSpaceFileNode))
        {
            nodeToRefresh = selectedNodes.First().FindMeOrParent<WorkSpaceRootNode>();
        }
        else if (commonType == typeof(WorkSpaceRootNode))
        {
            nodeToRefresh = selectedNodes.First().FindMeOrParent<WorkSpaceManagerNode>();
        }

        // Collect all files
        HashSet<string> files = [];
        foreach (var node in selectedNodes)
        {
            PopuplateFile(node.NodePath, files);
        }

        foreach (var file in files)
        {
            DocumentManager.Instance.GetDocument(file)?.MarkDelete();
            DocumentManager.Instance.CloseDocument(file);
        }

        view.BeginUpdate();
        foreach (var node in selectedNodes)
        {
            node.Delete(true);
        }
        view.EndUpdate();

        if (nodeToRefresh != null)
        {
            EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(nodeToRefresh));
            nodeToRefresh.PopulateUpdate();
        }

        EditorUtility.Inspector.InspectObject(null);
    }

    private static void PopuplateFile(string path, HashSet<string> paths)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (File.Exists(path)) 
        {
            paths.Add(path);
        }
        else if (Directory.Exists(path))
        {
            foreach (var subDir in Directory.GetDirectories(path))
            {
                PopuplateFile(subDir, paths);
            }

            foreach (var subFile in Directory.GetFiles(path))
            {
                PopuplateFile(subFile, paths);
            }
        }
    }
}