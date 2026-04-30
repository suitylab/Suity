using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.FileBunchs;

/// <summary>
/// Command to delete selected file bunch elements from their parent file bunch.
/// </summary>
internal class DeleteBunchFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBunchFileCommand"/> class.
    /// </summary>
    public DeleteBunchFileCommand()
        : base("Delete", Editor.ProjectGui.Properties.IconCache.Delete.ToIconSmall())
    {
        AcceptType<AssetElementNode>(false);
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

        if (!view.SelectedNodes.OfType<AssetElementNode>().All(o => o.GetAsset() is IFileBunchElement))
        {
            Visible = false;
            return;
        }

        if (!view.SelectedNodes.OfType<AssetElementNode>().Select(o => o.Parent).AllEqual())
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

        HandleDelete(view);
    }

    /// <summary>
    /// Handles the delete command logic for the specified project view.
    /// </summary>
    /// <param name="view">The project GUI view containing selected nodes.</param>
    public static void HandleDelete(IProjectGui view)
    {
        if (!view.SelectedNodes.OfType<AssetElementNode>().Any())
        {
            return;
        }

        if (!view.SelectedNodes.OfType<AssetElementNode>().All(o => o.GetAsset() is IFileBunchElement))
        {
            return;
        }

        if (!view.SelectedNodes.OfType<AssetElementNode>().Select(o => o.Parent).AllEqual())
        {
            return;
        }

        var deletes = view.SelectedNodes.OfType<AssetElementNode>().Select(o => o.GetAsset() as IFileBunchElement).SkipNull().ToArray();
        foreach (var element in deletes)
        {
            element.FileBunch?.DeleteFile(element.FileId);
        }

        var parent = view.SelectedNodes.FirstOrDefault()?.Parent as PopulatePathNode;

        parent?.PopulateUpdate();
    }
}