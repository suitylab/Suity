using Suity.Collections;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.FileBunchs;

/// <summary>
/// Command to remove selected file nodes from their associated file bunch.
/// </summary>
internal class WsUnbindBunchFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsUnbindBunchFileCommand"/> class.
    /// </summary>
    public WsUnbindBunchFileCommand()
        : base("Remove From File Bunch", CoreIconCache.Delete.ToIconSmall())
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

        if (!view.SelectedNodes.OfType<WorkSpaceFileNode>().Select(o => o.FindWorkSpace()).AllEqual())
        {
            Visible = false;
            return;
        }

        if (!view.SelectedNodes.All(o => (o as WorkSpaceFileNode)?.GetFileBunch() != null))
        {
            Visible = false;
            return;
        }

        Visible = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleUnbindCommand(view);
    }

    /// <summary>
    /// Handles the unbind command logic for the specified project view.
    /// </summary>
    /// <param name="view">The project GUI view containing selected nodes.</param>
    public static void HandleUnbindCommand(IProjectGui view)
    {
        if (!view.SelectedNodes.OfType<WorkSpaceFileNode>().Any())
        {
            return;
        }

        if (!view.SelectedNodes.OfType<WorkSpaceFileNode>().Select(o => o.FindWorkSpace()).AllEqual())
        {
            return;
        }

        if (!view.SelectedNodes.All(o => (o as WorkSpaceFileNode)?.GetFileBunch() != null))
        {
            return;
        }

        foreach (var node in view.SelectedNodes.OfType<WorkSpaceFileNode>())
        {
            var space = node.FindWorkSpace();
            if (space is null)
            {
                continue;
            }

            string fileId = space.MakeFileBunchFileId(node.NodePath);
            var fileBunch = space.GetFileBunch(fileId);

            if (fileBunch is null)
            {
                continue;
            }

            node.UnbindRenderFile();
            fileBunch.DeleteFile(fileId);
        }
    }
}