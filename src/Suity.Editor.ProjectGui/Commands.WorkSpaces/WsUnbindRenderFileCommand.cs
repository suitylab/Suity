using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to unbind (keep) render files that are marked for removal.
/// </summary>
internal class WsUnbindRenderFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsUnbindRenderFileCommand"/> class.
    /// </summary>
    public WsUnbindRenderFileCommand()
        : base("Keep File", CoreIconCache.Binding.ToIconSmall())
    {
        AcceptType<WorkSpaceFileNode>(false);
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

        Visible = view.SelectedNodes.OfType<WorkSpaceFileNode>().All(o => o.SpaceFileStatus == FileState.Remove);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        view.SelectedNodes.OfType<WorkSpaceFileNode>().Foreach(o => o.UnbindRenderFile());
    }
}
