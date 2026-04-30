using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to bind all user-occupied files back to render bindings.
/// </summary>
internal class WsCfg_BindAllRenderFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCfg_BindAllRenderFileCommand"/> class.
    /// </summary>
    public WsCfg_BindAllRenderFileCommand()
        : base("Add All Render Bindings", CoreIconCache.Binding.ToIconSmall())
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
        Visible = space?.ContainsUserOccupiedFiles() == true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var space = (view.SelectedNode as WorkSpaceRootNode)?.WorkSpace;
        space.BindAllUserFiles();
    }
}
