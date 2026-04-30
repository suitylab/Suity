using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to change the workspace controller type for selected workspaces.
/// </summary>
internal class ChangeWorkSpaceGroupCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeWorkSpaceGroupCommand"/> class.
    /// Populates submenu items for each available workspace controller type.
    /// </summary>
    public ChangeWorkSpaceGroupCommand()
        : base("Change Workspace Type", CoreIconCache.WorkSpace.ToIconSmall())
    {
        HashSet<WorkSpaceControllerInfo> infos = [.. WorkSpaceController.ControllerInfos];

/*        foreach (var ctrlInfo in infos.Where(o => typeof(ServerCSharpController).IsAssignableFrom(o.ControllerType)).OrderByDescending(o => o.Order).ToArray())
        {
            AddCommand(new ChangeWorkSpaceControllerCommand(ctrlInfo));
            infos.Remove(ctrlInfo);
        }
        AddSeparator();

        foreach (var ctrlInfo in infos.Where(o => typeof(ManagedCSharpController).IsAssignableFrom(o.ControllerType)).OrderBy(o => o.DisplayName).ToArray())
        {
            AddCommand(new ChangeWorkSpaceControllerCommand(ctrlInfo));
            infos.Remove(ctrlInfo);
        }
        AddSeparator();*/

        foreach (var ctrlInfo in infos.OrderByDescending(o => o.Order))
        {
            AddCommand(new ChangeWorkSpaceControllerCommand(ctrlInfo));
        }
        AcceptType<WorkSpaceRootNode>(false);
    }
}
