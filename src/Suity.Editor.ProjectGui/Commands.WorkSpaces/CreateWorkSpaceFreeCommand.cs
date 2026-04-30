using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to create a new free-form (user) workspace without a specific controller.
/// </summary>
internal class CreateWorkSpaceFreeCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkSpaceFreeCommand"/> class.
    /// </summary>
    public CreateWorkSpaceFreeCommand()
        : base("User Space", CoreIconCache.Asteroid.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var project = view.CurrentProject;
        //string workSpaceBaseDir = project.WorkSpaceDirectory;

        string name = await CreateWorkSpaceGroupCommand.InputNameWorkSpaceName(project);
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        bool success = false;
        FileUnwatchedAction.Do(() =>
        {
            success = WorkSpaceManager.Current.AddWorkSpace(name) != null;
        });

        if (success)
        {
            QueuedAction.Do(() => view.RefreshWorkSpaceNodes());
        }
        else
        {
            await DialogUtility.ShowMessageBoxAsync("Failed to create workspace.");
        }
    }
}
