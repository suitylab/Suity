using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using System;
using System.Diagnostics;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to compile/build the current solution.
/// </summary>
internal class CompileSolutionCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompileSolutionCommand"/> class.
    /// </summary>
    public CompileSolutionCommand()
        : base("Build Solution", CoreIconCache.Build.ToIconSmall())
    {
        AcceptType<IWorkSpaceManagerNode>();
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        EditorUtility.CompileSolution();
    }
}

/// <summary>
/// Command to open the solution file in the default IDE.
/// </summary>
internal class OpenSolutionCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenSolutionCommand"/> class.
    /// </summary>
    public OpenSolutionCommand()
        : base("Open Solution", CoreIconCache.VisualStudio.ToIconSmall())
    {
        AcceptType<WorkSpaceManagerNode>(false);
        AcceptType<WorkSpaceRootNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = view.SelectedNode?.FindMeOrParent<WorkSpaceManagerNode>();
        if (node != null)
        {
            string solutionFileName = view.CurrentProject?.SolutionFile;
            if (solutionFileName != null)
            {
                try
                {
                    Process.Start(solutionFileName);
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }
        }
    }
}
