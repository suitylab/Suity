using Suity.Collections;
using Suity.Editor;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that displays analysis problems for a selected asset file.
/// </summary>
internal class ShowProblemCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowProblemCommand"/> class.
    /// </summary>
    public ShowProblemCommand()
        : base("View Problems", CoreIconCache.Problems.ToIconSmall())
    {
        AcceptType<AssetFileNode>(false);
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

        Visible = view.SelectedNodes.Any(o => (o as AssetFileNode)?.TargetAsset?.Problems?.Count > 0);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        base.DoCommand();

        if (Sender is not IProjectGui view)
        {
            return;
        }

        if (!view.SelectedNodes.CountOne())
        {
            return;
        }

        var problems = (view.SelectedNode as AssetFileNode)?.TargetAsset.Problems;
        if (problems?.Count > 0)
        {
            EditorServices.AnalysisService.ShowProblems(problems);
        }
    }
}