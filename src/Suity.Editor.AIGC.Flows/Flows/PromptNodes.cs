using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;
using System.Linq;


namespace Suity.Editor.AIGC.Flows;

#region ReplaceOnePrompt

/// <summary>
/// Node that replaces a single keyword placeholder in a prompt with specified content.
/// </summary>
[DisplayText("Replace Single Prompt", "*CoreIcon|Prompt")]
public class ReplaceOnePrompt : AigcFlowNode
{
    FlowNodeConnector _in;
    FlowNodeConnector _promptIn;
    FlowNodeConnector _keyword;
    FlowNodeConnector _content;

    FlowNodeConnector _out;
    FlowNodeConnector _promptOut;

    readonly ValueProperty<bool> _clone = new("Clone", "Clone Prompt Object", false);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceOnePrompt"/> class.
    /// </summary>
    public ReplaceOnePrompt()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _clone.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _clone.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var promptType = TypeDefinition.FromNative<PromptBuilder>();

        _in = AddActionInputConnector("In", "Input");
        _promptIn = AddDataInputConnector("PromptIn", promptType, "Prompt");
        _keyword = AddDataInputConnector("Keyword", "string", "Keyword");
        _content = AddDataInputConnector("Content", "string", "Content");

        _out = AddActionOutputConnector("Out", "Output");
        _promptOut = AddDataOutputConnector("PromptOut", promptType, "Prompt");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var prompt = compute.GetValue<PromptBuilder>(_promptIn);

        if (_clone.Value)
        {
            prompt = prompt?.Clone();
        }

        string keyword = compute.GetValue<string>(_keyword);
        keyword = keyword?.Trim();

        string content = compute.GetValue<string>(_content) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            prompt.Replace(keyword, content);
        }

        compute.SetValue(_promptOut, prompt);
        compute.SetResult(this, _out);
    }
}

#endregion

#region BuildPrompt

/// <summary>
/// Node that builds a prompt from a template by replacing keyword placeholders with input values.
/// </summary>
[DisplayText("Build Prompt", "*CoreIcon|Prompt")]
[NativeAlias("Suity.Editor.AIGC.Flows.BuildPrompts")]
public class BuildPrompt : AigcFlowNode
{
    FlowNodeConnector _in;

    FlowNodeConnector _out;
    FlowNodeConnector _promptOut;

    readonly AssetProperty<PromptAsset> _promptTemplate = new("PromptTemplate", "Prompt Template");
    readonly ListProperty<string> _keywords = new("Keywords", "Keywords");
    readonly StringProperty _missingContent = new("MissingContent", "Missing Keyword Fill", "---", "If there are unreplaced placeholders in the prompt, replace them with this content.");

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildPrompt"/> class.
    /// </summary>
    public BuildPrompt()
    {
        UpdateConnector();

        _keywords.ValueChanged += (s, e) => UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _promptTemplate.Sync(sync);
        _keywords.Sync(sync);
        _missingContent.Sync(sync);

        if (sync.Sync("UpdateKeywords", ButtonValue.Empty) == ButtonValue.Clicked)
        {
            ParseFromPrompt();
        }
    }

    private void ParseFromPrompt()
    {
        if (_promptTemplate.Target is { } asset && asset.GetText() is { } text)
        {
            var keywords = PromptBuilder.ExtractKeywords(text);

            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].Trim(' ', '{', '}');
            }

            _keywords.List.Clear();
            _keywords.List.AddRange(keywords);

            UpdateConnectorQueued();
            this.GetFlowDocument()?.MarkDirty(this);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _promptTemplate.InspectorField(setup);
        _keywords.InspectorField(setup);
        _missingContent.InspectorField(setup);

        setup.Button("UpdateKeywords", "Update Keywords");
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var promptType = TypeDefinition.FromNative<PromptBuilder>();

        _in = AddActionInputConnector("In", "Input");

        _out = AddActionOutputConnector("Out", "Output");
        _promptOut = AddDataOutputConnector("PromptOut", promptType, "Prompt");

        var keywords = GetKeywords();
        foreach (var keyword in keywords)
        {
            AddDataInputConnector("k_" + keyword, "string", keyword);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var prompt = (_promptTemplate.Target?.CreatePromptBuilder()) 
            ?? throw new NullReferenceException("Prompt is null");

        var keywords = GetKeywords();
        foreach (var keyword in keywords)
        {
            if (this.GetConnector("k_" + keyword) is { } connector)
            {
                string r = compute.GetValue<string>(connector) ?? string.Empty;
                prompt.Replace(keyword, r);
            }
        }

        prompt.MissingContent = _missingContent.Text;

        compute.SetValue(_promptOut, prompt);
        compute.SetResult(this, _out);
    }

    /// <summary>
    /// Gets the list of keywords defined for this node.
    /// </summary>
    /// <returns>An array of trimmed, non-empty, distinct keyword strings.</returns>
    public string[] GetKeywords()
    {
        return _keywords.List
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToArray();
    }
}

#endregion

#region ReplaceMultiplePrompts

/// <summary>
/// Node that replaces multiple keyword placeholders in a prompt with input values.
/// </summary>
[DisplayText("Replace Multiple Prompts", "*CoreIcon|Prompt")]
public class ReplaceMultiplePrompts : AigcFlowNode
{
    FlowNodeConnector _in;
    FlowNodeConnector _promptIn;

    FlowNodeConnector _out;
    FlowNodeConnector _promptOut;

    readonly ValueProperty<bool> _clone = new("Clone", "Clone Prompt Object", false);
    readonly ListProperty<string> _keywords = new("Keywords", "Keywords");
    readonly StringProperty _missingContent = new("MissingContent", "Missing Keyword Fill", "---", "If there are unreplaced placeholders in the prompt, replace them with this content.");

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceMultiplePrompts"/> class.
    /// </summary>
    public ReplaceMultiplePrompts()
    {
        UpdateConnector();

        _keywords.ValueChanged += _keywords_ValueChanged;
    }

    private void _keywords_ValueChanged(object sender, System.EventArgs e)
    {
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _clone.Sync(sync);
        _keywords.Sync(sync);
        _missingContent.Sync(sync);

        if (sync.Sync("ParseFromPrompt", ButtonValue.Empty) == ButtonValue.Clicked)
        {
            ParseFromPrompt();
        }
    }

    private async void ParseFromPrompt()
    {
        var list = new AssetSelectionList<PromptAsset>();
        var result = await list.ShowSelectionGUIAsync("Select prompt to parse");
        if (result?.IsSuccess == true && result.Item is PromptAsset asset && asset.GetText() is { } text)
        {
            var keywords = PromptBuilder.ExtractKeywords(text);

            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].Trim(' ', '{', '}');
            }

            _keywords.List.Clear();
            _keywords.List.AddRange(keywords);

            UpdateConnectorQueued();
            this.GetFlowDocument()?.MarkDirty(this);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _clone.InspectorField(setup);
        _keywords.InspectorField(setup);
        _missingContent.InspectorField(setup);

        setup.Button("ParseFromPrompt", "Parse Keywords from Prompt");
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var promptType = TypeDefinition.FromNative<PromptBuilder>();

        _in = AddActionInputConnector("In", "Input");
        _promptIn = AddDataInputConnector("PromptIn", promptType, "Prompt");

        _out = AddActionOutputConnector("Out", "Output");
        _promptOut = AddDataOutputConnector("PromptOut", promptType, "Prompt");

        var keywords = GetKeywords();
        foreach (var keyword in keywords)
        {
            AddDataInputConnector("k_" + keyword, "string", keyword);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var prompt = compute.GetValue<PromptBuilder>(_promptIn);
        if (prompt is null)
        {
            throw new NullReferenceException("Prompt is null");
        }

        if (_clone.Value)
        {
            prompt = prompt?.Clone();
        }

        var keywords = GetKeywords();
        foreach (var keyword in keywords)
        {
            if (this.GetConnector("k_" + keyword) is { } connector)
            {
                string r = compute.GetValue<string>(connector) ?? string.Empty;
                prompt.Replace(keyword, r);
            }
        }

        prompt.MissingContent = _missingContent.Text;

        compute.SetValue(_promptOut, prompt);
        compute.SetResult(this, _out);
    }

    /// <summary>
    /// Gets the list of keywords defined for this node.
    /// </summary>
    /// <returns>An array of trimmed, non-empty, distinct keyword strings.</returns>
    public string[] GetKeywords()
    {
        return _keywords.List
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToArray();
    }
}

#endregion

#region GetSpeechLanguage

/// <summary>
/// Node that outputs the localized speech language name for the current environment.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Local Language Name", "*CoreIcon|Prompt")]
public class GetSpeechLanguage : AigcFlowNode
{
    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSpeechLanguage"/> class.
    /// </summary>
    public GetSpeechLanguage()
    {
        _out = AddDataOutputConnector("Out", "string", "Output");
        base.FlowNodeGui = OnGui;
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string lang = LLmService.Instance.LocalizedSpeechLanguage;
        
        compute.SetValue(_out, lang);
    }

    private ImGuiNode OnGui(ImGui gui, IDrawNodeContext context)
    {
        string lang = LLmService.Instance.LocalizedSpeechLanguage;

        return gui.FlowSingleConnectorFrame(_out, context, lang);
    }
}

#endregion
