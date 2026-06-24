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

public class RetryConfig : IViewObject
{
    private readonly ValueProperty<int> _retryCount
        = new("RetryCount", "Retry Count", 5, "Retry count when failed. <=0 means always retry.");

    private readonly ValueProperty<float> _delay
        = new("RetryDelay", "Retry Delay", 1.0f, "Retry delay when failed in seconds.");

    private readonly ValueProperty<float> _delayMultiplier
        = new("DelayMultiplier", "Delay Multiplier", 1.0f, "Delay multiplier for each retry.");

    private readonly ValueProperty<float> _maxDelay
        = new("MaxDelay", "Max Delay", 60.0f, "Max delay when failed in seconds.");

    public int RetryCount => _retryCount.Value;
    public float Delay => _delay.Value;
    public float DelayMultiplier => _delayMultiplier.Value;
    public float MaxDelay => _maxDelay.Value;

    public RetryConfig()
    {
        _delay.Property.WithUnit("s");
        _delayMultiplier.Property.WithUnit("x");
        _maxDelay.Property.WithUnit("s");
    }

    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _retryCount.Sync(sync);
        _delay.Sync(sync);
        _delayMultiplier.Sync(sync);
        _maxDelay.Sync(sync);

        if (sync.IsSetter())
        {
            if (_delay.Value < 1)
            {
                _delay.Value = 1;
            }

            if (_delayMultiplier.Value < 1)
            {
                _delayMultiplier.Value = 1;
            }

            if (_maxDelay.Value < 1)
            {
                _maxDelay.Value = 1;
            }
        }
    }

    public void SetupView(IViewObjectSetup setup)
    {
        _retryCount.InspectorField(setup);
        _delay.InspectorField(setup);
        _delayMultiplier.InspectorField(setup);
        _maxDelay.InspectorField(setup);
    }

    public override string ToString()
    {
        if (_retryCount.Value <= 0)
        {
            return $"Retry always with delay {_delay.Value}s, multiplier {_delayMultiplier.Value}x, max delay {_maxDelay.Value}s.";
        }
        else
        {
            return $"Retry {_retryCount.Value} times with delay {_delay.Value}s, multiplier {_delayMultiplier.Value}x, max delay {_maxDelay.Value}s.";
        }
    }
}

/// <summary>
/// Plugin that provides AIGC workflow execution capability.
/// Implements <see cref="IAigcWorkflowRunner"/> to enable workflow running functionality.
/// </summary>
public class AigcWorkflowPlugin : EditorPlugin, IAigcWorkflowRunner, IViewObject
{
    public const string PROMPT_WORKSPACE = @"Create a workspace name based on the user input:
{{INPUT}}

# Output format: PascalCase Identifier.
# Output the workspace name and nothing else.
";

    public static AigcWorkflowPlugin Instance { get; private set; }

    readonly Dictionary<Type, ToolAsset> _pageToolAssets = [];

    private readonly ValueProperty<bool> _useFullName 
        = new("UseFullName", "Use Full Name", false, "Use full name in tool schema representation.");

    private readonly ValueProperty<bool> _minimalToolSchema
        = new("MinimalToolSchema", "Minimal Tool Schema", true, "Use minimal tool schema representation.");

    private readonly ValueProperty<RetryConfig> _retry
        = new("Retry", "Retry", new(), "Retry when failed.");

    private readonly StringProperty _fixedWorkSpaceName
        = new("FixedWorkSpaceName", "Fixed WorkSpace Name", "", "Fixed workspace name for testing, leave blank for automatic creation based on user input.");

    private readonly TextBlockProperty _promptWorkSpace
        = new("PromptWorkSpace", "WorkSpace Prompt");

    public AigcWorkflowPlugin()
    {
        Instance ??= this;

        SubFlowExtensions.UseFullName = false;
        IPageAssetToTextConverter.MinimalFormat = true;

        _useFullName.ValueChanged += (s, e) => 
        {
            SubFlowExtensions.UseFullName = _useFullName.Value;
        };

        _minimalToolSchema.ValueChanged += (s, e) =>
        {
            IPageAssetToTextConverter.MinimalFormat = _minimalToolSchema.Value;
        };

        _retry.Property.WithOptional().WithExpand();

        _promptWorkSpace.Property.WithHintText(PROMPT_WORKSPACE);
    }

    public override string Description => "AIGC Workflow";

    public override ImageDef Icon => CoreIconCache.Workflow;


    public RetryConfig Retry => _retry.Value;


    public string FixedWorkSpaceName => _fixedWorkSpaceName.Text;
    public string PromptWorkSpace
    {
        get
        {
            string prompt = _promptWorkSpace.Text;
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                return prompt;
            }

            return PROMPT_WORKSPACE;
        }
    }

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
        _minimalToolSchema.Sync(sync);
        _retry.Sync(sync);

        _fixedWorkSpaceName.Sync(sync);
        _promptWorkSpace.Sync(sync);
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        _useFullName.InspectorField(setup);
        _minimalToolSchema.InspectorField(setup);
        _retry.InspectorField(setup);

        setup.LabelWithIcon("Prompt", CoreIconCache.Prompt);
        _fixedWorkSpaceName.InspectorField(setup);
        _promptWorkSpace.InspectorField(setup);
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

        //TODO: Although async, it does not support multiple threads calling at the same time, need to add protection
        var result = await runner.RunStarterNode(starterNode, null, request.UserMessage, cancel);
        return result;
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