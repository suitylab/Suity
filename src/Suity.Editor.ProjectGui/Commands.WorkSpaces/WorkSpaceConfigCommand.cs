using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command group for workspace configuration settings.
/// </summary>
internal class WorkSpaceConfigCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceConfigCommand"/> class.
    /// </summary>
    public WorkSpaceConfigCommand() : base("Workspace Settings", CoreIconCache.WorkSpace.ToIconSmall())
    {
        AddCommand(new WsCfg_SetExternalPathCommand());
        AddCommand(new WsCfg_UnsetExternalPathCommand());
        AddSeparator();
        AddCommand(new WsCfg_UnbindAllRemovingFilesCommand());
        AddCommand(new WsCfg_BindAllRenderFileCommand());

        AcceptType<WorkSpaceRootNode>(false);
        AcceptOneItemOnly = true;
    }
}

/// <summary>
/// Command to open the project file associated with a workspace.
/// </summary>
internal class OpenProjectFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenProjectFileCommand"/> class.
    /// </summary>
    public OpenProjectFileCommand() : base("Open Project File", CoreIconCache.Project.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceRootNode)view.SelectedNode;
        if (node == null)
        {
            return;
        }

        var fileName = node.WorkSpace?.Controller?.GetProjectFileName();

        if (string.IsNullOrEmpty(fileName))
        {
            DialogUtility.ShowMessageBoxAsyncL("This workspace has no project file.");
            return;
        }

        EditorUtility.OpenFile(fileName);
    }
}
