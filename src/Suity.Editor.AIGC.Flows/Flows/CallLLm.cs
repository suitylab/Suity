using Suity;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Flows;

#region GetGlobalLLmModel

/// <summary>
/// Node that retrieves the global LLM model based on specified level and type.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.LLm, HasHeader = false)]
[DisplayText("Get Global LLM Model", "*CoreIcon|Chat")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetGlobalLLmModel")]
public class GetGlobalLLmModel : AigcFlowNode
{
    private readonly ConnectorValueProperty<AigcModelLevel> _level = new("Level", "Level", AigcModelLevel.Default, "The level of the LLM model. Uses global setting when 'Default' is selected.");
    private readonly ConnectorValueProperty<LLmModelType> _type = new("Type", "Type", LLmModelType.Default, "The type of the LLM model usage configured in the settings.");

    readonly FlowNodeConnector _llmModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGlobalLLmModel"/> class.
    /// </summary>
    public GetGlobalLLmModel()
    {
        _level.AddConnector(this);
        _type.AddConnector(this);

        var modelType = TypeDefinition.FromNative<ILLmModel>();

        _llmModel = this.AddDataOutputConnector("LlmModel", modelType, "Language Model");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _level.Sync(sync);
        _type.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _level.InspectorField(setup, this);
        _type.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var level = _level.GetValue(compute, this);
        var type = _type.GetValue(compute, this);

        var model = LLmService.Instance.GetLLmModel(level, type);

        compute.SetValue(_llmModel, model);
    }
}

#endregion

#region LLmCallOptionEx

/// <summary>
/// Represents extended optional configuration settings for a language model call, including search and thinking options.
/// </summary>
[DisplayText("LLM Call Options")]
[NativeType(CodeBase = "AIGC", Description = "LLM Call Options")]
[NativeAlias("Suity.Editor.AIGC.Flows.LLmCallOptionEx")]
public class LLmCallOptionEx : IViewObject
{
    private readonly ValueProperty<bool> _enableSearch = new(nameof(EnableSearch), "Enable Search", false, "This feature requires language model support.");
    private readonly ValueProperty<bool> _enableThinking = new(nameof(EnableThinking), "Enable Thinking", false, "This feature requires language model support.");

    /// <summary>
    /// Gets or sets whether web search capability is enabled during the model call.
    /// </summary>
    public bool EnableSearch { get => _enableSearch.Value; set => _enableSearch.Value = value; }
    /// <summary>
    /// Gets or sets whether extended thinking/reasoning mode is enabled for the model.
    /// </summary>
    public bool EnableThinking { get => _enableThinking.Value; set => _enableThinking.Value = value; }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _enableSearch.Sync(sync);
        _enableThinking.Sync(sync);
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        _enableSearch.InspectorField(setup);
        _enableThinking.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return L(this.GetType().ToDisplayText());
    }
}

#endregion

#region CallLLm
/// <summary>
/// Main node for calling large language models. Supports model selection, function calling, output verification, and retry logic.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.LLm)]
[DisplayText("Call LLM", "*CoreIcon|Chat")]
[ToolTipsText("Main entry point for calling large language models.")]
[NativeAlias("Suity.Editor.AIGC.Flows.CallLLm")]
public class CallLLm : AigcFlowNode
{
    /// <summary>
    /// Default number of retry attempts.
    /// </summary>
    public const int DEFAULT_RETRY = 3;

    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private FlowNodeConnector _systemMessage;
    private FlowNodeConnector _messages;
    private FlowNodeConnector _functionsIn;

    private FlowNodeConnector _otherFunction;
    private FlowNodeConnector _noFunction;
    private FlowNodeConnector _result;

    private readonly StringProperty _dialogName = new("DialogName", "Dialog Name");
    private readonly ConnectorAssetProperty<ILLmModel> _chatModel = new("ChatModel", "Model", "Select a language model, if not selected, use the default model.");
    private readonly ConnectorValueProperty<LLmModelParameter> _parameter = new("Parameter", "Parameter Settings", null, "Override the default configuration for language model calls.");
    private readonly ConnectorValueProperty<LLmCallOptionEx> _callOptionEx = new("CallOptionEx", "Extended Call Options", null, "Override the extended configuration for language model calls. These features require language model support.");

    private readonly AssetListProperty<DStruct> _functions = new("Functions", "Functions", "Specify multiple selectable functions.");
    private readonly AssetProperty<DStruct> _functionCall = new("FunctionCall", "Function Call", "Specify the function to call explicitly.");

    private LLmOutputVerify _outputVerify;
    private readonly ValueProperty<int> _retry = new("Retry", "Retry Count", DEFAULT_RETRY, "Number of retry attempts, retry execution after call failure or verification failure. If set to <=0, defaults to 3 attempts.");
    private readonly ValueProperty<bool> _combineMessages = new("CombineMessages", "Combine Messages", false, "Combine all chat history messages into a single message.");

    /// <summary>
    /// Initializes a new instance of the <see cref="CallLLm"/> class.
    /// </summary>
    public CallLLm()
    {
        _parameter.Property.WithOptional();
        _callOptionEx.Property.WithOptional();

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override ImageDef Icon
    {
        get
        {
            if (!_chatModel.GetIsLinked(this) && _chatModel.BaseTarget is Asset { Icon: { } icon })
            {
                return icon;
            }

            return base.Icon;
        }
    }

    /// <summary>
    /// Gets or sets the output verification strategy for the LLM response.
    /// </summary>
    public LLmOutputVerify OutputVerify
    {
        get => _outputVerify;
        set
        {
            if (ReferenceEquals(_outputVerify, value))
            {
                return;
            }

            _outputVerify = value;
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _dialogName.Sync(sync);

        _chatModel.Sync(sync);
        _parameter.Sync(sync);
        _callOptionEx.Sync(sync);

        _functions.Sync(sync);
        _functionCall.Sync(sync);

        OutputVerify = sync.Sync(nameof(OutputVerify), OutputVerify);
        _retry.Sync(sync);
        _combineMessages.Sync(sync);

        if (sync.IsSetterOf("Functions"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _dialogName.InspectorField(setup);

        _chatModel.InspectorField(setup, this);
        _parameter.InspectorField(setup, this);
        _callOptionEx.InspectorField(setup, this);

        setup.Label("Output Verification");
        _functions.InspectorField(setup);
        _functionCall.InspectorField(setup);

        setup.Label("Verification");
        setup.InspectorFieldOf<LLmOutputVerify>(new ViewProperty(nameof(OutputVerify), "Output Result Verification"));
        _retry.InspectorField(setup);

        setup.Label("Others");
        _combineMessages.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        var toolParamAryType = TypeDefinition.FromAssetLink<DStruct>().ElementType.MakeArrayType();

        _in = AddActionInputConnector("In", "Input");
        _chatModel.AddConnector(this);
        _parameter.AddConnector(this);
        _callOptionEx.AddConnector(this);

        var msgType = TypeDefinition.FromNative<LLmMessage>();

        _systemMessage = AddDataInputConnector("SystemMessage", "string", "System Message");
        _messages = AddConnector("Messages", msgType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Prompt");
        _functionsIn = AddDataInputConnector("FunctionsIn", toolParamAryType.ToTypeName(), "Functions");


        foreach (var function in _functions.List.Select(o => o?.Target).SkipNull().Take(10))
        {
            string typeKey = function.Definition?.ToTypeName() ?? UNKNOWN_TYPE;

            AddConnector(function.Id, typeKey, FlowDirections.Output, FlowConnectorTypes.Action, false, $"{function.DisplayText} Call");
        }

        _otherFunction = AddConnector("OtherAction", "object", FlowDirections.Output, FlowConnectorTypes.Action, false, "Other Function Call");
        _noFunction = AddActionOutputConnector("NoAction", "No Function Call");

        _out = AddActionOutputConnector("Out", "Text Result");
        _result = AddDataOutputConnector("Result", "string", "Answer Text");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var local = compute.LocalContext;
        var workFlow = compute.Context.GetArgument<IWorkflowSetup>();

        var nodeModel = _chatModel.GetTarget(compute, this);
        var model = SelectModel(compute, nodeModel, out string sourceMsg);
        if (model is null)
        {
            throw new InvalidOperationException($"Model not specified.");
        }

        if (model.IsManual)
        {
            compute.AddLog(TextStatus.Info, $"Manual execution {ToString()}");
        }
        else
        {
            bool pause = workFlow?.PauseOnAICall == true;
            if (pause)
            {
                bool ok = await compute.PauseDialog($"Preparing to {ToString()} (can cancel this message via project settings)", cancel);
                cancel.ThrowIfCancellationRequested();

                if (!ok)
                {
                    return null;
                }
            }
        }

        if (model is not LLmModelAsset)
        {
            throw new InvalidOperationException($"Model resource must inherit from {nameof(LLmModelAsset)}");
        }

        var c = compute.Context.GetArgument<IConversationHandler>();

        var parameter = _parameter.GetValue(compute, this);
        var call = model.CreateCall(parameter, compute.Context)
            ?? throw new InvalidOperationException($"Failed to create model:{model}");

        compute.InvalidateOutputs(this);

        string sysMsg = compute.GetValue<string>(_systemMessage);

        var msgs = compute.GetValues<LLmMessage>(_messages, true);
        if (msgs.Length == 0 || msgs.All(o => string.IsNullOrWhiteSpace(o?.Message)))
        {
            compute.AddLog(TextStatus.Warning, "Prompt is empty, please check the input.");
            compute.SetValue(_result, null);
            return _out;
        }

        if (_combineMessages.Value)
        {
            msgs = [LLmMessage.Combine(msgs)];
        }

        int retryCount = _retry.Value;
        if (retryCount <= 0)
        {
            retryCount = DEFAULT_RETRY;
        }

        var callOptionEx = _callOptionEx.GetValue(compute, this);
        var option = new LLmCallOption
        {
            EnableSearch = callOptionEx?.EnableSearch,
            EnableThinking = callOptionEx?.EnableThinking,
        };

        string title = $"Calling {model.ToDisplayText()}{sourceMsg}...";
        string result = await RetryHelper.DoRetryAction<string>(title, async () =>
        {
            call.NewMessage();

            if (!string.IsNullOrWhiteSpace(sysMsg))
            {
                call.AppendSystemMessage(sysMsg);
            }

            foreach (var msg in msgs)
            {
                call.AppendMessage(msg);
            }

            foreach (var function in GetFunctions(compute))
            {
                string desc = function.GetAttribute<ToolTipsAttribute>()?.ToolTips;
                call.AddFunction(function.Name, function, desc);
            }

            var funcCallStruct = _functionCall.Target;
            if (funcCallStruct != null)
            {
                call.FunctionCall = funcCallStruct.Name;
            }
            var verify = _outputVerify;

            var result = await call.Call(cancel, parameter, option);
            if (verify != null)
            {
                if (verify.Verify(call))
                {
                    return result;
                }
                else
                {
                    throw new AigcException("Result verification failed.");
                }
            }
            else
            {
                return result;
            }

        }, false, retryCount, c, cancel);



        if (cancel.IsCancellationRequested)
        {
            return null;
        }

        compute.SetValue(_result, result);

        return GetReturnConnector(compute, LLmService.Instance.ResolveSObjectOutput(call));
    }

    /// <summary>
    /// Determines the return connector based on the function call result.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="funcCall">The resolved function call object.</param>
    /// <returns>The appropriate output connector for the result.</returns>
    private FlowNodeConnector GetReturnConnector(IFlowComputationAsync compute, SObject funcCall)
    {
        if (funcCall != null)
        {
            if (funcCall.ObjectType?.Target?.Id is { } id)
            {
                var funcAction = GetConnector(id);
                if (funcAction != null)
                {
                    compute.SetValue(funcAction, funcCall);
                    return funcAction;
                }
                else
                {
                    compute.SetValue(_otherFunction, funcCall);
                    return _otherFunction;
                }
            }
            else
            {
                return _noFunction;
            }
        }
        else
        {
            return _out;
        }
    }

    /// <summary>
    /// Gets the combined list of functions from both node configuration and external input.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <returns>An array of struct definitions representing available functions.</returns>
    private DStruct[] GetFunctions(IFlowComputationAsync compute)
    {
        var functions = _functions.List.Select(o => o?.Target).SkipNull();

        var functionsEx = compute.GetValueConvert<DStruct[]>(_functionsIn) ?? [];

        return functions.Concat(functionsEx).ToArray();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_dialogName.Value is { } name && !string.IsNullOrWhiteSpace(name))
        {
            return $"{name} Dialog";
        }
        else if (!_chatModel.GetIsLinked(this) && _chatModel.BaseTarget is { } model)
        {
            return "Call: " + model.ModelId ?? string.Empty;
        }
        else
        {
            return EditorUtility.ToDisplayText(this.GetType());
        }
    }


    /// <summary>
    /// Selects the appropriate LLM model based on priority: node model, caller, agent, team, workflow, or global config.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="nodeModel">The model specified on the node.</param>
    /// <param name="sourceMsg">Output parameter describing where the model was sourced from.</param>
    /// <returns>The selected LLM model, or null if no model is available.</returns>
    public static ILLmModel SelectModel(IFlowComputationAsync compute, ILLmModel nodeModel, out string sourceMsg)
    {
        var local = compute.LocalContext;

        var workFlow = compute.Context.GetArgument<IWorkflowSetup>();

        sourceMsg = null;

        ILLmModel model;
        do
        {
            model = nodeModel;
            if (model != null)
            {
                sourceMsg = "(From node)";
                break;
            }

            model = local.GetArgument<ILLmModel>();
            if (model != null)
            {
                sourceMsg = "(From caller)";
                break;
            }

            model = workFlow?.DefaultModel;
            if (model != null)
            {
                sourceMsg = "(From flowchart)";
                break;
            }

            model = LLmService.Instance.GetLLmModel(AigcModelLevel.Default, LLmModelType.Default);
            if (model != null)
            {
                sourceMsg = "(From global config)";
                break;
            }

        } while (false);


        return model;
    }

}

/// <summary>
/// Abstract base class for verifying LLM output results.
/// </summary>
public abstract class LLmOutputVerify : IViewObject, ITextDisplay
{
    /// <summary>
    /// Verifies the output of an LLM call.
    /// </summary>
    /// <param name="call">The LLM call to verify.</param>
    /// <returns>True if the output passes verification; otherwise, false.</returns>
    public abstract bool Verify(ILLmCall call);

    #region IViewObject
    /// <inheritdoc/>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <inheritdoc/>
    public virtual void SetupView(IViewObjectSetup setup)
    {
    }
    #endregion

    #region ITextDisplay
    /// <inheritdoc/>
    public virtual string DisplayText => GetType().ToDisplayText();

    /// <inheritdoc/>
    public virtual object DisplayIcon => GetType().ToDisplayIcon();

    /// <inheritdoc/>
    public virtual TextStatus DisplayStatus => TextStatus.Normal;
    #endregion

    /// <inheritdoc/>
    public override string ToString() => L(DisplayText);
}

/// <summary>
/// Verifies that the LLM output is a single word without invalid characters.
/// </summary>
[DisplayText("Single Word Output", "*CoreIcon|Verify")]
public class SingleWordOutputVerify : LLmOutputVerify
{
    static readonly HashSet<char> _invalidChars = [' ', '\t', '\n', '\r', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'];

    readonly ValueProperty<int> _lengthLimit = new("LengthLimit", "Length Limit", 0, "Limit output length, if value <=0 then no limit.");

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _lengthLimit.Sync(sync);
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        _lengthLimit.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override bool Verify(ILLmCall call)
    {
        if (LLmService.Instance.ResolveSObjectOutput(call) != null)
        {
            return false;
        }

        var resp = call.LastTextOutput?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(resp))
        {
            return false;
        }

        int lengthLimit = _lengthLimit.Value;
        if (lengthLimit > 0 && resp.Length > lengthLimit)
        {
            return false;
        }

        foreach (char c in resp)
        {
            if (_invalidChars.Contains(c))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Verifies that the LLM output contains a structured (function call) result.
/// </summary>
[DisplayText("Structured Output Verification", "*CoreIcon|Verify")]
public class StructuredOutputVerify : LLmOutputVerify
{
    /// <inheritdoc/>
    public override bool Verify(ILLmCall call)
    {
        return call.LastFunctionOutput != null;
    }
} 
#endregion