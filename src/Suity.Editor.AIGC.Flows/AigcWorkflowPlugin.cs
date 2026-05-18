using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Plugin that provides AIGC workflow execution capability.
/// Implements <see cref="IAigcWorkflowRunner"/> to enable workflow running functionality.
/// </summary>
public class AigcWorkflowPlugin : BackendPlugin, IAigcWorkflowRunner
{
    public static AigcWorkflowPlugin Instance { get; private set; }

    readonly Dictionary<Type, ToolAsset> _pageToolAssets = [];

    public AigcWorkflowPlugin()
    {
        Instance ??= this;
    }

    /// <inheritdoc/>
    protected internal override void Awake(PluginContext context)
    {
        base.Awake(context);

        var pageToolTypes = typeof(PageTool<>).GetAvailableDerivedTypes();
        foreach (var inputType in pageToolTypes)
        {
            var outputType = inputType.BaseType.GetGenericArguments()[0];
            var assetType = typeof(PageToolAsset<,>).MakeGenericType(inputType, outputType);
            var asset = (ToolAsset)Activator.CreateInstance(assetType);
            _pageToolAssets.Add(inputType, asset);
        }
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IAigcWorkflowRunner))
        {
            return this;
        }

        return null;
    }

    #region IAigcWorkflowRunner

    /// <inheritdoc/>
    public async Task<object> RunWorkflow(AIRequest request, AigcWorkflowOption workflowOption)
    {
        var conversation = request.Conversation;
        if (conversation is null)
        {
            return null;
        }

        var cancel = request.Cancellation;

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

    /// <inheritdoc/>
    public ILLmChatProvider ChatProvider => WorkflowChatProvider.Instance;

    #endregion
}

[NotAvailable]
public class PageToolAsset<TInput, TOutput> : ToolAsset<TInput, TOutput>
    where TInput : PageTool<TOutput>
    where TOutput : class, IViewObject
{
    public PageToolAsset()
        : base(false)
    {
        var typeDef = TypeDefinition.FromNative<TInput>();
        string typeName;
        if (!TypeDefinition.IsNullOrEmpty(typeDef))
        {
            typeName = typeDef.Target?.AssetKey ?? typeof(TInput).FullName;
        }
        else
        {
            typeName = typeof(TInput).FullName;
        }

        this.LocalName = $"*PageTool|{typeName.TrimStart('*')}";

        ResolveId();
    }

    protected override Task<TOutput> RunTask(TInput input, ToolCallContext context)
    {
        return input.RunTask(context);
    }

    public override string DisplayText => typeof(TInput).ToDisplayText();
}