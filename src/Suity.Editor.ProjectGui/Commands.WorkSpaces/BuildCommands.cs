using Suity.Collections;
using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to perform a full re-render of selected workspaces.
/// </summary>
internal class RenderCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderCommand"/> class.
    /// </summary>
    public RenderCommand()
        : base("Re-Render", CoreIconCache.Render.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var workSpaces = view.SelectedNodes
            .OfType<WorkSpaceRootNode>()
            .Select(o => o.WorkSpace)
            .ToArray();

        EditorUtility.StartBuildTask(() =>
        {
            CodeRenderUtility.RenderWorkSpace(workSpaces, false);

            foreach (var rootNode in view.SelectedNodes.OfType<WorkSpaceRootNode>())
            {
                rootNode.PopulateUpdateDeep();
            }
        });
    }
}

/// <summary>
/// Command to perform an incremental render of selected workspaces.
/// </summary>
internal class IncrementalRenderCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncrementalRenderCommand"/> class.
    /// </summary>
    public IncrementalRenderCommand()
        : base("Render", CoreIconCache.Render.ToIconSmall())
    {
        AcceptType<WorkSpaceRootNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var workSpaces = view.SelectedNodes
            .OfType<WorkSpaceRootNode>()
            .Select(o => o.WorkSpace)
            .ToArray();

        EditorUtility.StartBuildTask(() =>
        {
            CodeRenderUtility.RenderWorkSpace(workSpaces, true);

            foreach (var rootNode in view.SelectedNodes.OfType<WorkSpaceRootNode>())
            {
                rootNode.PopulateUpdateDeep();
            }
        });
    }
}

/// <summary>
/// Command to render specific workspace files.
/// </summary>
internal class RenderFileCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderFileCommand"/> class.
    /// </summary>
    public RenderFileCommand()
        : base("Render File", CoreIconCache.Render.ToIconSmall())
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

        Visible = view.SelectedNodes.All(o => (o as WorkSpaceFileNode)?.IsRendering == true);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        UniqueMultiDictionary<WorkSpace, RenderTarget> targets = new UniqueMultiDictionary<WorkSpace, RenderTarget>();

        if (Sender is not IProjectGui view)
        {
            return;
        }

        EditorUtility.StartBuildTask(() =>
        {
            foreach (var node in view.SelectedNodes.OfType<WorkSpaceFileNode>())
            {
                WorkSpace workSpace = node.FindWorkSpace();
                if (workSpace == null)
                {
                    continue;
                }

                foreach (var item in node.GetRenderTargets())
                {
                    targets.Add(workSpace, item);
                }
            }

            CodeRenderUtility.RenderWorkSpace(targets);
        });
    }
}
