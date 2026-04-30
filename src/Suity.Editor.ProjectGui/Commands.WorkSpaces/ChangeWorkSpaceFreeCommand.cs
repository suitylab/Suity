using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to change a workspace to a free-form (user) space by removing its controller.
/// </summary>
internal class ChangeWorkSpaceFreeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeWorkSpaceFreeCommand"/> class.
    /// </summary>
    public ChangeWorkSpaceFreeCommand()
        : base("User Space", CoreIconCache.Asteroid.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        foreach (WorkSpaceRootNode rootNode in view.SelectedNodes.OfType<WorkSpaceRootNode>().ToArray())
        {
            if (rootNode.WorkSpace.ControllerInfo == null)
            {
                continue;
            }

            rootNode.WorkSpace.RemoveController();
            rootNode.WorkSpace.Manager.WriteSolution();
            rootNode.PopulateUpdate();
        }
    }
}
