using Suity.Views.Named;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows;

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

/// <summary>
/// Interface for AI-generated content (AIGC) workflow execution that extends INamed interface
/// </summary>
public interface IFlowRunnable : INamed
{
    /// <summary>
    /// Gets the starter node of the workflow
    /// </summary>
    /// <param name="ctx">The function context containing necessary information for workflow execution</param>
    /// <returns>The starting flow node of the workflow</returns>
    FlowNode GetStarterNode(FunctionContext ctx);
}