using Suity.Editor.Flows;
using Suity.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Computation engine for AIGC flow execution, handling node running and conversation integration.
/// </summary>
public class AigcFlowComputation : FlowComputationAsync, IFlowNodeRunner
{
    /// <summary>
    /// Gets the conversation handler for this computation.
    /// </summary>
    public IConversationHandler Conversation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcFlowComputation"/> class.
    /// </summary>
    /// <param name="conversation">The conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    public AigcFlowComputation(IConversationHandler conversation, FunctionContext context = null)
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

    //protected override void OnDiagramAdded(IFlowDiagram diagram)
    //{
    //    base.OnDiagramAdded(diagram);

    //    //TODO: Find a better way to associate documents and views. Currently, documents are associated with computators, views with computators, and documents with views.

    //    var doc = diagram.GetFlowDocument() as AigcFlowDocument;
    //    doc?.SetLastComputation(this);

    //    foreach (var view in diagram.Views)
    //    {
    //        view.Computation = this;
    //    }
    //}


}