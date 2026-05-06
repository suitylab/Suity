using System.Threading.Tasks;
using System.Threading;
using System;
using Suity.Editor.Flows;

namespace Suity.Editor.AIGC.Flows;


/// <summary>
/// Interface for configuring and setting up an AIGC workflow.
/// </summary>
public interface IWorkflowSetup
{
    /// <summary>
    /// Gets the default language model for the workflow.
    /// </summary>
    ILLmModel DefaultModel { get; }

    /// <summary>
    /// Gets the diagrams included in this workflow.
    /// </summary>
    [Obsolete]
    AigcDiagramAsset[] IncludeDiagrams { get; }

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI calls.
    /// </summary>
    bool PauseOnAICall { get; }

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI log output.
    /// </summary>
    bool PauseOnAILog { get; }
}