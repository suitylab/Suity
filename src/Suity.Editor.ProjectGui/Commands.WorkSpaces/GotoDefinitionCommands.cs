using Suity.Editor;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to navigate to the definition of a workspace file, reference, or assembly.
/// </summary>
internal class GotoDefinitionCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoDefinitionCommand"/> class.
    /// </summary>
    public GotoDefinitionCommand()
        : base("Go To Definition", CoreIconCache.GotoDefination.ToIconSmall())
    {
        AcceptType<WorkSpaceFileNode>(false);
        AcceptType<WorkSpaceReferenceNode>(false);
        AcceptType<WorkSpaceAssemblyNode>(false);
        AcceptType<RenderTargetNode>(false);

        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        PathNode node = view.SelectedNode;

        switch (node)
        {
            case WorkSpaceFileNode workSpaceFileNode:
                HandleWsFileNode(workSpaceFileNode);
                break;

            case WorkSpaceReferenceNode workSpaceReferenceNode:
                HandleWsRefNode(workSpaceReferenceNode);
                break;

            case WorkSpaceAssemblyNode asmNode:
                HandleWsAsmNode(asmNode);
                break;

            case RenderTargetNode renderTargetNode:
                EditorUtility.LocateInProject(renderTargetNode.NodePath);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Handles navigation for a workspace file node by selecting its source reference nodes.
    /// </summary>
    /// <param name="node">The workspace file node to process.</param>
    private void HandleWsFileNode(WorkSpaceFileNode node)
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var rootNode = node.FindMeOrParent<WorkSpaceRootNode>();
        var groupNode = rootNode?.NodeList.FirstOrDefault(o => o is WorkSpaceReferenceGroupNode) as WorkSpaceReferenceGroupNode;
        if (groupNode is null)
        {
            return;
        }

        groupNode.Expanded = true;

        List<PathNode> nodes = [];
        string rFileName = node.NodePath.MakeRalativePath(rootNode.WorkSpace.MasterDirectory);
        //foreach (Asset asset in rootNode.WorkSpace.GetDependency(rFileName).OfType<Asset>())
        //{
        //    PathNode refNode = groupNode.Nodes.OfType<WorkSpaceReferenceNode>().FirstOrDefault(o => o.Id == asset.Id);
        //    if (refNode != null)
        //    {
        //        nodes.Add(refNode);
        //    }
        //}

        foreach (var renderTarget in rootNode.WorkSpace.GetAffactedRenderTargets(rFileName))
        {
            if (renderTarget.Tag is not IWorkSpaceRefItem refItem)
            {
                continue;
            }

            var refNode = groupNode.NodeList.OfType<WorkSpaceReferenceNode>()
                .FirstOrDefault(o => o.GetReferenceItem() == refItem);

            if (refNode != null)
            {
                nodes.Add(refNode);
            }
        }
        if (nodes.Count > 0)
        {
            view.SelectNodes(nodes);
        }
    }

    /// <summary>
    /// Handles navigation for a workspace reference node.
    /// </summary>
    /// <param name="node">The workspace reference node to process.</param>
    private void HandleWsRefNode(WorkSpaceReferenceNode node)
    {
        EditorUtility.LocateInProjectOrDefinition(node.Id);
    }

    /// <summary>
    /// Handles navigation for a workspace assembly node.
    /// </summary>
    /// <param name="node">The workspace assembly node to process.</param>
    private void HandleWsAsmNode(WorkSpaceAssemblyNode node)
    {
        EditorUtility.LocateInProjectOrDefinition(node.RefItem.Id);
    }
}

/// <summary>
/// Command to navigate to the model definition associated with a workspace file.
/// </summary>
internal class GotoModelDefinitionCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoModelDefinitionCommand"/> class.
    /// </summary>
    public GotoModelDefinitionCommand()
        : base("Go To Model Definition", CoreIconCache.Box.ToIconSmall())
    {
        AcceptType<WorkSpaceFileNode>(false);

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

        var target = (view.SelectedNode as WorkSpaceFileNode)?.GetRenderTarget();

        Visible = target != null;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var target = (view.SelectedNode as WorkSpaceFileNode)?.GetRenderTarget();

        if (target?.Item?.Object != null)
        {
            EditorUtility.NavigateTo(target.Item.Object);
        }
    }
}

/// <summary>
/// Command to navigate to the material definition associated with a workspace file.
/// </summary>
internal class GotoMaterialDefinitionCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoMaterialDefinitionCommand"/> class.
    /// </summary>
    public GotoMaterialDefinitionCommand()
        : base("Go To Material Definition", CoreIconCache.Material.ToIconSmall())
    {
        AcceptType<WorkSpaceFileNode>(false);

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

        var target = (view.SelectedNode as WorkSpaceFileNode)?.GetRenderTarget();

        Visible = target?.Material != null;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var target = (view.SelectedNode as WorkSpaceFileNode)?.GetRenderTarget();

        if (target?.Material != null)
        {
            EditorUtility.NavigateTo(target.Material.Id);
        }
    }
}
