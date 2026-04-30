using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to cancel (unset) the external master path for a workspace.
/// </summary>
internal class WsCfg_UnsetExternalPathCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCfg_UnsetExternalPathCommand"/> class.
    /// </summary>
    public WsCfg_UnsetExternalPathCommand() : base("Cancel External Path", CoreIconCache.Cancel.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        WorkSpaceRootNode node = view.SelectedNode as WorkSpaceRootNode;
        Enabled = node?.WorkSpace.IsExternalMasterDirectory == true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
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

        node.WorkSpace.UnsetExternalMasterPath();
    }
}
