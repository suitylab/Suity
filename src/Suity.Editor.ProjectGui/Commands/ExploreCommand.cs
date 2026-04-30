using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that opens the selected node's location in the system file explorer.
/// </summary>
internal class ExploreCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExploreCommand"/> class.
    /// </summary>
    public ExploreCommand()
        : base("Open in Explorer", CoreIconCache.Explore.ToIconSmall())
    {
        AcceptType<AssetRootNode>(false);
        AcceptType<AssetDirectoryNode>(false);
        AcceptType<AssetFileNode>(false);

        AcceptType<WorkSpaceManagerNode>(false);
        AcceptType<WorkSpaceRootNode>(false);
        AcceptType<WorkSpaceDirectoryNode>(false);
        AcceptType<WorkSpaceFileNode>(false);
        AcceptType<RenderTargetNode>(false);

        AcceptType<PublishRootNode>(false);
        AcceptType<PublishDirectoryNode>(false);
        AcceptType<PublishFileNode>(false);

        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        PathNode selectedNode = view.SelectedNodes.First();
        if (selectedNode is FileNode)
        {
            TextFileHelper.NavigateFile(selectedNode.NodePath);
        }
        else if (selectedNode is DirectoryNode || selectedNode is WorkSpaceManagerNode)
        {
            TextFileHelper.NavigateFolder(selectedNode.NodePath);
        }
    }
}