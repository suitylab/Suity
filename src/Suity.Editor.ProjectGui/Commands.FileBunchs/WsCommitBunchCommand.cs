using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.FileBunchs;

/// <summary>
/// Command to commit a single file bunch to its workspace.
/// </summary>
internal class WsCommitBunchCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsCommitBunchCommand"/> class.
    /// </summary>
    public WsCommitBunchCommand()
        : base("Commit File Bunch", CoreIconCache.FileBunchCommit.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
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

        Visible = view.SelectedNodes.OfType<WorkSpaceReferenceNode>().All(o => o.GetReferenceAsset() is IFileBunch);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        foreach (var refNode in view.SelectedNodes.OfType<WorkSpaceReferenceNode>())
        {
            if (refNode.GetReferenceAsset() is not IFileBunch bunch)
            {
                continue;
            }

            bunch.CommitFiles(refNode.WorkSpace);
        }
    }
}