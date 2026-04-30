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

/// <summary>
/// Interface for running flow nodes asynchronously, extending the flow computation with execution capabilities.
/// </summary>
public interface IFlowNodeRunner : IFlowComputation, IDisposable
{
    /// <summary>
    /// Runs the starter node of the workflow.
    /// </summary>
    /// <param name="starterNode">The node to start execution from.</param>
    /// <param name="connector">Optional connector to start from.</param>
    /// <param name="msg">Optional message to pass as input.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, returning the result connector.</returns>
    Task<object> RunStarterNode(FlowNode starterNode, FlowNodeConnector connector = null, string msg = null, CancellationToken cancel = default);
}