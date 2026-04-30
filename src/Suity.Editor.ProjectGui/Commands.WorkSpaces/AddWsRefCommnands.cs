using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

#region AddWsRefRenderableCommnand

/// <summary>
/// Command to add a renderable model reference to a workspace.
/// </summary>
internal class AddWsRefRenderableCommnand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsRefRenderableCommnand"/> class.
    /// </summary>
    public AddWsRefRenderableCommnand()
        : base("Add Model Reference", CoreIconCache.Model.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceReferenceGroupNode)view.SelectedNode;
        var renderable = await DialogUtility.ShowAssetSelectionGUIAsync<IRenderable>(L("Add Model Reference"), new SelectionOption { HideEmptySelection = true });
        if (renderable != null)
        {
            node.WorkSpace.AddReferenceItem(renderable.Id);
        }

        node.PopulateUpdate();
    }
}

#endregion

#region AddWsRefRenderTargetLibraryCommnand

/// <summary>
/// Command to add a render flow reference library to a workspace.
/// </summary>
internal class AddWsRefRenderTargetLibraryCommnand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsRefRenderTargetLibraryCommnand"/> class.
    /// </summary>
    public AddWsRefRenderTargetLibraryCommnand()
        : base("Add Render Flow Reference", CoreIconCache.RenderFlow.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceReferenceGroupNode)view.SelectedNode;
        var library = await DialogUtility.ShowAssetSelectionGUIAsync<IRenderTargetLibrary>(L("Add Render Flow Reference"), new SelectionOption { HideEmptySelection = true });
        if (library != null)
        {
            node.WorkSpace.AddReferenceItem(library.Id);
        }
        node.PopulateUpdate();
    }
}

#endregion

#region AddWsRefFileBunchCommnand

/// <summary>
/// Command to add a file bunch reference to a workspace.
/// </summary>
internal class AddWsRefFileBunchCommnand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsRefFileBunchCommnand"/> class.
    /// </summary>
    public AddWsRefFileBunchCommnand()
        : base("Add File Bunch Reference", CoreIconCache.FileBunch.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceReferenceGroupNode)view.SelectedNode;
        var renderable = await DialogUtility.ShowAssetSelectionGUIAsync<IFileBunch>(L("Add File Bunch Reference"), new SelectionOption { HideEmptySelection = true });
        if (renderable != null)
        {
            node.WorkSpace.AddReferenceItem(renderable.Id);
        }

        node.PopulateUpdate();
    }
}

#endregion

#region AddWsRefUserFileCommand

/// <summary>
/// Command to add a user code library reference to a workspace.
/// </summary>
internal class AddWsRefUserFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsRefUserFileCommand"/> class.
    /// </summary>
    public AddWsRefUserFileCommand()
        : base("Add User File Reference", CoreIconCache.Restore.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceReferenceGroupNode)view.SelectedNode;
        var renderable = await DialogUtility.ShowAssetSelectionGUIAsync<ICodeLibrary>(L("Add User File Reference"), new SelectionOption { HideEmptySelection = true });
        if (renderable != null)
        {
            node.WorkSpace.AddReferenceItem(renderable.Id);
        }

        node.PopulateUpdate();
    }
}

#endregion

#region WsRefDeleteCommand

/// <summary>
/// Command to delete workspace reference or assembly nodes.
/// </summary>
internal class WsRefDeleteCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsRefDeleteCommand"/> class.
    /// </summary>
    public WsRefDeleteCommand()
        : base("Delete", Editor.ProjectGui.Properties.IconCache.Delete.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
        AcceptType<WorkSpaceAssemblyNode>(false);
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
    /// Handles the deletion of selected reference and assembly nodes.
    /// </summary>
    /// <param name="view">The project GUI view instance.</param>
    public static void HandleDelete(IProjectGui view)
    {
        PopulatePathNode parent = null;

        foreach (var node in view.SelectedNodes.OfType<WorkSpaceReferenceNode>().ToArray())
        {
            parent ??= node.Parent as PopulatePathNode;
            node.RemoveReference();
        }

        foreach (var node in view.SelectedNodes.OfType<WorkSpaceAssemblyNode>().ToArray())
        {
            parent ??= node.Parent as PopulatePathNode;
            node.RemoveReference();
        }

        parent?.PopulateUpdate();
    }
}

#endregion

#region WsRefSelectAffectedFilesCommand

/// <summary>
/// Command to select all files generated by a workspace reference.
/// </summary>
internal class WsRefSelectAffectedFilesCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WsRefSelectAffectedFilesCommand"/> class.
    /// </summary>
    public WsRefSelectAffectedFilesCommand()
        : base("Select Generated Files", CoreIconCache.Select.ToIconSmall())
    {
        AcceptType<WorkSpaceReferenceNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleCommand(view);
    }

    /// <summary>
    /// Handles selecting all generated files for the current reference node.
    /// </summary>
    /// <param name="view">The project GUI view instance.</param>
    public static void HandleCommand(IProjectGui view)
    {
        var node = (WorkSpaceReferenceNode)view.SelectedNode;

        var rootNode = node.FindMeOrParent<WorkSpaceRootNode>();

        List<PathNode> nodes = [];
        foreach (var fileName in node.WorkSpace.GetAffectedFileNames(node.Id))
        {
            var fileNode = rootNode.FindNodeByFullPath(fileName.PhysicFullPath);
            if (fileNode != null)
            {
                nodes.Add(fileNode);
            }
        }

        if (nodes.Count > 0)
        {
            QueuedAction.Do(() => view.SelectNodes(nodes));
        }
    }
}

#endregion
