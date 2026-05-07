using Suity.Views;
using System;
using System.Diagnostics;
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
/// Computation engine for AIGC flow execution, handling node running and conversation integration.
/// </summary>
public class RunnerFlowComputation : FlowComputationAsync, IFlowNodeRunner
{
    /// <summary>
    /// Gets the conversation handler for this computation.
    /// </summary>
    public IConversationHandler Conversation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RunnerFlowComputation"/> class.
    /// </summary>
    /// <param name="conversation">The conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    public RunnerFlowComputation(IConversationHandler conversation, FunctionContext context = null)
        : base(context)
    {
        Conversation = conversation ?? throw new ArgumentNullException(nameof(conversation));

        Context.SetArgument(Conversation);
    }

    /// <inheritdoc/>
    public Task<object> RunStarterNode(FlowNode starterNode, FlowNodeConnector connector, string msg, CancellationToken cancel)
    {
        if (starterNode is null)
        {
            throw new ArgumentNullException(nameof(starterNode));
        }

        if (msg != null && starterNode.GetConnector("Prompt") is { } prompt)
        {
            SetData(prompt, msg ?? string.Empty);
        }

        var state = GetOrCreateState(starterNode);
        state.State = FlowComputationStates.Running;
        UpdateViewQueued();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            return RunActionNode(starterNode, connector, true, cancel);
        }
        finally
        {
            stopwatch.Stop();
            state.ElapsedTime = stopwatch.Elapsed;
        }
    }

    /// <summary>
    /// Gets the currently running node.
    /// </summary>
    public FlowNode RunningNode => LastNode;

    #region IFlowComputation

    /// <inheritdoc/>
    public override void AddLog(TextStatus status, string message)
    {
        Conversation.AddSystemMessage(message, status);
    }

    #endregion

}