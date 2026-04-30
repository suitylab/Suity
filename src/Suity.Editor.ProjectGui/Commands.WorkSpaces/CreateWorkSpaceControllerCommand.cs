using Suity.Editor.WorkSpaces;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to create a new workspace with a specific controller type.
/// </summary>
internal class CreateWorkSpaceControllerCommand : MenuCommand
{
    private readonly WorkSpaceControllerInfo _ctrlInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkSpaceControllerCommand"/> class.
    /// </summary>
    /// <param name="ctrlInfo">The controller info to use for the new workspace.</param>
    public CreateWorkSpaceControllerCommand(WorkSpaceControllerInfo ctrlInfo)
    {
        _ctrlInfo = ctrlInfo;
        Text = ctrlInfo.DisplayName;
        Icon = EditorUtility.GetIconByAssetKey(ctrlInfo.IconKey);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        Icon = EditorUtility.GetIconByAssetKey(_ctrlInfo.IconKey);
        base.OnPopUp(selectionCount, types, commonNodeType);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        Project project = view.CurrentProject;
        //string workSpaceBaseDir = project.WorkSpaceDirectory;

        string name = await CreateWorkSpaceGroupCommand.InputNameWorkSpaceName(project);
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        bool success = false;
        FileUnwatchedAction.Do(() =>
        {
            success = WorkSpaceManager.Current.AddWorkSpace(name, _ctrlInfo) != null;
        });

        // The view is already auto-listening, no need to refresh the view
        if (!success)
        {
            await DialogUtility.ShowMessageBoxAsync("Failed to create workspace.");
        }
    }
}
