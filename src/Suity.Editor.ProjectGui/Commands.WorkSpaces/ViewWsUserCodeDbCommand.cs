using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to view the user code tags/database for a workspace file.
/// </summary>
internal class ViewWsUserCodeDbCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewWsUserCodeDbCommand"/> class.
    /// </summary>
    public ViewWsUserCodeDbCommand()
        : base("View Tags", CoreIconCache.Tag.ToIconSmall())
    {
        AcceptType<WorkSpaceFileNode>(false);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (selectionCount != 1)
        {
            Visible = false;
            return;
        }
        base.OnPopUp(selectionCount, types, commonNodeType);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        if (view.SelectedNode is not WorkSpaceFileNode fileNode)
        {
            return;
        }

        var space = fileNode.FindWorkSpace();
        if (space is null)
        {
            return;
        }

        space.ShowUserCodeEditor(fileNode.GetRenderTarget());
    }
}
