using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.FileBunchs;

/// <summary>
/// Command to commit all file bunches within the selected workspaces.
/// </summary>
internal class WsCommitAllBunchCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCommitAllBunchCommand"/> class.
    /// </summary>
    public WsCommitAllBunchCommand()
        : base("Commit All File Bunches", CoreIconCache.FileBunchCommit.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
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

        Visible = view.SelectedNodes.OfType<WorkSpaceRootNode>().Any(o => o.WorkSpace.ContainsFileBunches());
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        foreach (var wsNode in view.SelectedNodes.OfType<WorkSpaceRootNode>())
        {
            foreach (var bunch in wsNode.WorkSpace.GetReferences<IFileBunch>())
            {
                bunch.CommitFiles(wsNode.WorkSpace);
            }
        }
    }
}