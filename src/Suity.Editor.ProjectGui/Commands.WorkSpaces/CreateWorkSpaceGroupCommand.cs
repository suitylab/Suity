using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command group for creating new workspaces with different controller types.
/// </summary>
internal class CreateWorkSpaceGroupCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkSpaceGroupCommand"/> class.
    /// Populates submenu items for each available workspace controller type.
    /// </summary>
    public CreateWorkSpaceGroupCommand()
        : base("Create Workspace", CoreIconCache.WorkSpace.ToIconSmall())
    {
        HashSet<WorkSpaceControllerInfo> infos = [.. WorkSpaceController.ControllerInfos];

/*        foreach (var ctrlInfo in infos.Where(o => typeof(ServerCSharpController).IsAssignableFrom(o.ControllerType)).OrderByDescending(o => o.Order).ToArray())
        {
            AddCommand(new CreateWorkSpaceControllerCommand(ctrlInfo));
            infos.Remove(ctrlInfo);
        }
        AddSeparator();

        foreach (var ctrlInfo in infos.Where(o => typeof(ManagedCSharpController).IsAssignableFrom(o.ControllerType)).OrderByDescending(o => o.Order).ToArray())
        {
            AddCommand(new CreateWorkSpaceControllerCommand(ctrlInfo));
            infos.Remove(ctrlInfo);
        }
        AddSeparator();*/

        foreach (var ctrlInfo in infos.OrderByDescending(o => o.Order))
        {
            AddCommand(new CreateWorkSpaceControllerCommand(ctrlInfo));
        }
        AcceptType<WorkSpaceManagerNode>(false);
    }

    /// <summary>
    /// Shows a dialog to input and validate a new workspace name.
    /// </summary>
    /// <param name="project">The current project context.</param>
    /// <returns>The validated workspace name, or empty if cancelled.</returns>
    public static async Task<string> InputNameWorkSpaceName(Project project)
    {
        string name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Create Workspace", "", str =>
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (!StringCharVarifier.FileNameVarifier.Varify(str))
            {
                //DialogUtility.ShowMessageBoxAsyncL("Name contains illegal characters.");
                return false;
            }

            if (WorkSpaceManager.Current.ContainsWorkSpace(str))
            {
                //DialogUtility.ShowMessageBoxAsyncL("Workspace already exists.");
                return false;
            }

            return true;
        });

        return name;
    }
}
