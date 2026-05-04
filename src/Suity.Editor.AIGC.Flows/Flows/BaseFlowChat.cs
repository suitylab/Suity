using Suity.Editor.Flows;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Base class for flow-based chat interactions, managing workflow execution and conversation handling.
/// </summary>
public abstract class BaseFlowChat : BaseLLmChat
{
    private const string FlowCreateFailedMessage = "Workflow creation failed.";

    private IFlowNodeRunner _runner;
    private IFlowNodeRunner _lastRunner;

    /// <summary>
    /// Gets the current flow node runner.
    /// </summary>
    public IFlowNodeRunner Runner => _runner;

    /// <summary>
    /// Event raised when the runner changes.
    /// </summary>
    public event EventHandler RunnerChnaged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseFlowChat"/> class.
    /// </summary>
    /// <param name="name">The name of the chat.</param>
    /// <param name="text">Optional initial text.</param>
    /// <param name="context">Optional function context.</param>
    public BaseFlowChat(string name, string text = null, FunctionContext context = null)
        : base(name, text, context)
    {
    }

    /// <inheritdoc/>
    protected override async Task OnStart(CancellationToken cancel)
    {
        await InitWorkflow();
    }

    /// <inheritdoc/>
    protected override void OnStop()
    {
        DropCurrentRun();
    }

    /// <inheritdoc/>
    protected override async Task<object> HandleStart(string msg, object option, CancellationTokenSource cancelSource)
    {
        //if (string.IsNullOrWhiteSpace(msg))
        //{
        //    return null;
        //}

        return await HandleRun(msg, option, cancelSource);
    }

    /// <inheritdoc/>
    public override void OnSettingGui(ImGui gui)
    {
        var node = _runner?.LastNode ?? _lastRunner?.LastNode;

        if (node != null)
        {
            gui.Button("gotoNode", CoreIconCache.Node)
            .InitClass("toolBtn")
            .SetToolTipsL("Go to running action node")
            .OnClick(() =>
            {
                (node.GetFlowDocument()?.ShowView() as IViewSelectable)?.SetSelection(new ViewSelection(node));
            });
        }
    }

    /// <summary>
    /// Override to configure runner parameters and return the starting node of the workflow.
    /// </summary>
    /// <param name="runner">Runner, please configure runner parameters in this process.</param>
    /// <returns>Returns the starting node.</returns>
    protected abstract FlowNode GetStarterNode();

    /// <summary>
    /// Gets additional data nodes to include in the workflow.
    /// </summary>
    /// <returns>An array of additional data nodes.</returns>
    protected virtual FlowNode[] GetAdditionalDataNodes() => [];


    /// <summary>
    /// Handles the execution of the workflow with the given message and options.
    /// </summary>
    /// <param name="msg">The input message.</param>
    /// <param name="option">The workflow options.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>The result of the workflow execution.</returns>
    private async Task<object> HandleRun(string msg, object option, CancellationTokenSource cancelSource)
    {
        var runner = _runner;
        if (runner is null)
        {
            return null;
        }

        // Bind view and computation
        if (option is AigcWorkflowOption workflowOption)
        {
            if (workflowOption.View is { } view)
            {
                view.Computation = runner;
                runner.Context.SetArgument(view);
            }

            workflowOption.Config?.Invoke(runner);
        }

        var starterNode = GetStarterNode();
        if (starterNode is null)
        {
            return null;
        }

        //_conversation.AddRunningMessage("Workflow: " + starterNode.ToDisplayTextL());

        bool errPausing = true;

        try
        {
            //TODO: Although async, it does not support multiple threads calling at the same time, need to add protection
            var result = await runner.RunStarterNode(starterNode, null, msg, cancelSource.Token);

            if (cancelSource.Token.IsCancellationRequested)
            {
                _conversation.AddSystemMessage("Task canceled.");
            }
            else
            {
                _conversation.AddInfoMessage("Task completed.");
            }

            return result;
        }
        catch (TaskCanceledException)
        {
            _conversation.AddSystemMessage("Task canceled.");

            return null;
        }
        catch (Exception err)
        {
            errPausing = true;

            _conversation.AddException(err);

            return null;
        }
        finally
        {
            /*if (errPausing)
            {
                // Cancel operation
                var tcs = new TaskCompletionSource<object>();
                cancel.Register(() =>
                {
                    DropCurrentRun();

                    tcs.SetResult(null);
                });

                // Suspend
                await tcs.Task;
            }
            else
            {
                DropCurrentRun();
            }*/
        }
    }


    /// <summary>
    /// Initializes the workflow by creating a new computation runner.
    /// </summary>
    private async Task InitWorkflow()
    {
        if (_runner != null)
        {
            _conversation.AddErrorMessage("A computation is currently being processed.");

            return;
        }

        DropCurrentRun();

        var runner = _runner = new AigcFlowComputation(_conversation, _context);

        runner.Context.SetArgument<IConversationHost>(_conversation as IConversationHost);
        runner.Context.SetArgument<IConversationHostAsync>(_conversation as IConversationHostAsync);

        RunnerChnaged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disposes the current runner and clears the reference.
    /// </summary>
    private void DropCurrentRun()
    {
        _lastRunner?.Dispose();

        _lastRunner = _runner;
        _runner = null;

        RunnerChnaged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();

        RunnerChnaged = null;
    }
}