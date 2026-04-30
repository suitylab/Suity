using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to set an external master path for a workspace.
/// </summary>
internal class WsCfg_SetExternalPathCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCfg_SetExternalPathCommand"/> class.
    /// </summary>
    public WsCfg_SetExternalPathCommand()
        : base("Set External Path", CoreIconCache.External.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceRootNode)view.SelectedNode;
        if (node is null)
        {
            return;
        }

        string folder = node.WorkSpace.MasterDirectory;
        string result = await DialogUtility.ShowOpenFolderAsync(folder);
        if (result is null)
        {
            return;
        }

        string rPath = result.MakeRalativePath(node.WorkSpace.BaseDirectory);
        bool changed = false;

        if (rPath.StartsWith("../") || WorkSpaceManager.AbsoluteExternalMasterPath)
        {
            changed = node.WorkSpace.SetExternalMasterPath(result);
        }
        else if (rPath.IgnoreCaseEquals(WorkSpace.DefaultMasterDirectory))
        {
            changed = node.WorkSpace.UnsetExternalMasterPath();
        }
        else
        {
            await DialogUtility.ShowMessageBoxAsync("Path not supported: " + rPath);
        }

        // Changed && managed project
/*        if (changed && node.WorkSpace.Controller is ManagedCSharpController)
        {
            node.WorkSpace.Manager.WriteSolution();
        }*/
    }
}
