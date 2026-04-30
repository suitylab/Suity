using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to keep all files that are pending deletion by unbinding them.
/// </summary>
internal class WsCfg_UnbindAllRemovingFilesCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCfg_UnbindAllRemovingFilesCommand"/> class.
    /// </summary>
    public WsCfg_UnbindAllRemovingFilesCommand()
        : base("Keep All Pending Delete Files", CoreIconCache.Binding.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        if (Sender is not IProjectGui view)
        {
            return;
        }

        var space = (view.SelectedNode as WorkSpaceRootNode)?.WorkSpace;
        Visible = space?.ContainsRemovingFiles() == true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var space = (view.SelectedNode as WorkSpaceRootNode)?.WorkSpace;
        space.UnbindAllRemovingFiles();
    }
}
