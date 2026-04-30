using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Node that calls an LLM to classify input text into predefined categories.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.LLm)]
[DisplayText("LLM Classifier", "*CoreIcon|Chat")]
[ToolTipsText("Call large language model to perform classification.")]
public class LLmClassifier : AigcFlowNode
{
    /// <summary>
    /// Default number of retry attempts.
    /// </summary>
    public const int DEFAULT_RETRY = 3;

    private FlowNodeConnector _in;

    private FlowNodeConnector _prompt;

    private FlowNodeConnector _result;

    private readonly ConnectorAssetProperty<ILLmModel> _chatModel = new("ChatModel", "Model", "Select a language model, if not selected, use the default model.");
    private readonly ValueProperty<LLmModelParameter> _overrideConfig = new("OverrideConfig", "Parameter Settings", new LLmModelParameter(), "Override the default configuration for language model calls.");
    private readonly ListProperty<string> _options = new("Options", "Options", "Classifier options, call large language model to determine which option the prompt should belong to.");
    private readonly ValueProperty<int> _retry = new("Retry", "Retry Count", DEFAULT_RETRY, "Number of retry attempts, retry execution after call failure or verification failure. If set to <=0, defaults to 3 attempts.");

    /// <summary>
    /// Initializes a new instance of the <see cref="LLmClassifier"/> class.
    /// </summary>
    public LLmClassifier()
    {
        _options.Property.WithWriteBack();
        _overrideConfig.Property.WithOptional();

        UpdateConnector();
    }

    /// <inheritdoc/>
    public override Image Icon
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

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _chatModel.Sync(sync);
        _overrideConfig.Sync(sync);

        _options.Sync(sync);
        _retry.Sync(sync);

        if (sync.IsSetterOf("Options"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        _chatModel.InspectorField(setup, this);
        _overrideConfig.InspectorField(setup);

        _options.InspectorField(setup);
        _retry.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        _in = AddActionInputConnector("In", "Input");
        _chatModel.AddConnector(this);

        _prompt = AddConnector("Prompt", "string", FlowDirections.Input, FlowConnectorTypes.Data, false, "Prompt");

        _result = AddDataOutputConnector("Result", "string", "Result");

        for (int i = 0; i < _options.List.Count; i++)
        {
            string name = $"Option-{_options.List[i]}";
            AddActionOutputConnector(name, _options.List[i]);
        }
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var local = compute.LocalContext;
        var workFlow = compute.Context.GetArgument<IWorkflowSetup>();

        var nodeModel = _chatModel.GetTarget(compute, this);
        var model = CallLLm.SelectModel(compute, nodeModel, out string sourceMsg);
        if (model is null)
        {
            throw new InvalidOperationException($"Model not specified.");
        }

        if (model.IsManual)
        {
            compute.AddLog(TextStatus.Info, $"Manual execution {ToString()}");
        }

        if (model is not LLmModelAsset)
        {
            throw new InvalidOperationException($"Model resource must inherit from {nameof(LLmModelAsset)}");
        }

        var call = model.CreateCall(_overrideConfig.Value, compute.Context)
            ?? throw new InvalidOperationException($"Failed to create model:{model}");

        compute.InvalidateOutputs(this);

        var c = compute.Context.GetArgument<IConversationHandler>();

        string msg = compute.GetValue<string>(_prompt);
        if (string.IsNullOrWhiteSpace(msg))
        {
            throw new NullReferenceException("Prompt is empty");
        }

        int retryCount = _retry.Value;
        if (retryCount <= 0)
        {
            retryCount = DEFAULT_RETRY;
        }

        string title = $"Calling {model.ToDisplayText()}{sourceMsg}...";
        var connector = await RetryHelper.DoRetryAction<FlowNodeConnector>(title, async () => 
        {
            call.NewMessage();

            string sysMsg = @"
Please classify the following user instructions into one of the following categories based on their main content: 
{{OPTION_LIST}}

Please return only the category name.
";
            var builder = new StringBuilder();
            for (int i = 0; i < _options.List.Count; i++)
            {
                builder.AppendLine(_options.List[i]);
            }
            sysMsg = sysMsg.Replace("{{OPTION_LIST}}", builder.ToString());


            call.AppendSystemMessage(sysMsg);
            call.AppendUserMessage(msg);
            string result = await call.Call(cancel, _overrideConfig.Value);
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new AigcException("Classification result is empty");
            }

            string name = $"Option-{result}";

            var connector = GetConnector(name);

            if (connector is null)
            {
                throw new AigcException("Connector port not found.");
            }

            return connector;

        }, false, retryCount, c, cancel);


        if (connector is null)
        {
            throw new NullReferenceException("Classification execution failed");
        }

        compute.SetValue(_result, connector.Description ?? string.Empty);

        return connector;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_chatModel.GetIsLinked(this))
        {
            return $"{_chatModel.BaseTarget} Classifier";
        }
        else
        {
            return DisplayText;
        }
    }
}
