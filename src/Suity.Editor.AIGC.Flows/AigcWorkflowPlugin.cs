using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Plugin that provides AIGC workflow execution capability.
/// Implements <see cref="IAigcWorkflowRunner"/> to enable workflow running functionality.
/// </summary>
public class AigcWorkflowPlugin : EditorPlugin, IAigcWorkflowRunner, IViewObject
{
    public static AigcWorkflowPlugin Instance { get; private set; }

    readonly Dictionary<Type, ToolAsset> _pageToolAssets = [];

    private readonly ValueProperty<bool> _useFullName 
        = new("UseFullName", "Use Full Name", false, "Use full name in tool schema representation.");


    public AigcWorkflowPlugin()
    {
        Instance ??= this;

        _useFullName.ValueChanged += (s, e) => 
        {
            SubFlowExtensions.UseFullName = _useFullName.Value;
        };
    }

    public override string Description => "AIGC Workflow";

    public override ImageDef Icon => CoreIconCache.Workflow;

    /// <inheritdoc/>
    protected internal override void Awake(PluginContext context)
    {
        base.Awake(context);

        var commandTypes = typeof(ToolCommand<>).GetAvailableDerivedTypes();
        foreach (var inputType in commandTypes)
        {
            var outputType = inputType.BaseType.GetGenericArguments()[0];
            var assetType = typeof(ToolCommandAsset<,>).MakeGenericType(inputType, outputType);
            var asset = (ToolAsset)Activator.CreateInstance(assetType);
            _pageToolAssets.Add(inputType, asset);
        }
    }


    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _useFullName.Sync(sync);
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        _useFullName.InspectorField(setup);
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
public class ToolCommandAsset<TInput, TOutput> : ToolAsset<TInput, TOutput>
    where TInput : ToolCommand<TOutput>
    where TOutput : class, IViewObject
{
    public ToolCommandAsset()
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
        this.Description = typeof(TInput).ToDisplayText();

        ResolveId();
    }

    protected override string GetName() => typeof(TInput).Name;

    public override ImageDef GetIcon() => TypeDefinition.FromNative<TInput>()?.Target?.Icon;

    protected override Task<TOutput> RunTask(TInput input, ToolCallContext context)
    {
        context.ToolInstance.Conversation?.AddSystemMessage("Run tool", msg => 
        {
            msg.AddCode(this.ToDisplayTextL());
        });

        return input.Run(context);
    }

    public override string DisplayText => typeof(TInput).ToDisplayText();
}