using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class AigcWorkflowPlugin : BackendPlugin, IAigcWorkflowRunner
{

    protected internal override void Awake(PluginContext context)
    {
        base.Awake(context);
    }

    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IAigcWorkflowRunner))
        {
            return this;
        }

        return null;
    }

    #region IAigcWorkflowRunner

    public async Task<object> RunWorkflow(AIRequest request, AigcWorkflowOption workflowOption)
    {
        var conversation = request.Conversation;
        if (conversation is null)
        {
            return null;
        }

        var cancel = request.Cancel;

        var ctx = request.FuncContext != null ? new FunctionContext(request.FuncContext) : new FunctionContext();
        ctx.SetArgument<IConversationHandler>(conversation);

        var runner = new RunnerFlowComputation(conversation, ctx);

        if (workflowOption.View is { } view)
        {
            view.Computation = runner;
            runner.Context.SetArgument(view);
        }

        workflowOption.Config?.Invoke(runner);

        var starterNode = workflowOption.Runnable?.GetStarterNode(ctx);
        if (starterNode is null)
        {
            return null;
        }

        // conversation.AddRunningMessage("Workflow: " + starterNode.ToDisplayTextL());

        bool errPausing = true;

        try
        {
            //TODO: Although async, it does not support multiple threads calling at the same time, need to add protection
            var result = await runner.RunStarterNode(starterNode, null, request.UserMessage, cancel);

            //if (cancel.IsCancellationRequested)
            //{
            //    conversation.AddSystemMessage("Task canceled.");
            //}
            //else
            //{
            //    conversation.AddSystemMessage("Task completed.");
            //}

            return result;
        }
        catch (TaskCanceledException)
        {
            conversation.AddSystemMessage("Task canceled.");

            return null;
        }
        catch (Exception err)
        {
            errPausing = true;

            conversation.AddException(err);

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

    public ILLmChatProvider ChatProvider => WorkflowChatProvider.Instance;

    #endregion
}
