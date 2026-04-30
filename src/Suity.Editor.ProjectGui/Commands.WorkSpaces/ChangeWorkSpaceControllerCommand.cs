using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to change the controller type for selected workspaces.
/// </summary>
internal class ChangeWorkSpaceControllerCommand : MenuCommand
{
    private readonly WorkSpaceControllerInfo _ctrlInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeWorkSpaceControllerCommand"/> class.
    /// </summary>
    /// <param name="ctrlItem">The controller info to switch to.</param>
    public ChangeWorkSpaceControllerCommand(WorkSpaceControllerInfo ctrlItem)
    {
        _ctrlInfo = ctrlItem;
        Text = ctrlItem.DisplayName;
        Icon = EditorUtility.GetIconByAssetKey(ctrlItem.IconKey);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        Icon = EditorUtility.GetIconByAssetKey(_ctrlInfo.IconKey);
        base.OnPopUp(selectionCount, types, commonNodeType);
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
            var workSpace = rootNode.WorkSpace;

            if (workSpace.ControllerInfo == _ctrlInfo)
            {
                continue;
            }

            workSpace.RemoveController();
            workSpace.NewController(_ctrlInfo);

            try
            {
                workSpace.Controller?.MigrateProjectFile();
            }
            catch (Exception err)
            {
                err.LogError($"Failed to migrate project file:{workSpace.Name}");
            }

            workSpace.Manager.WriteSolution();
            rootNode.PopulateUpdate();
        }

        EditorUtility.Inspector.UpdateInspector();
    }
}
