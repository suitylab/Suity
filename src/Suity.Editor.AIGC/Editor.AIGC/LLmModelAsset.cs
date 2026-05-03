using Suity.Drawing;
using Suity.Editor.Types;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

[NativeType(Name = "LLmModelAsset", Description = "AI Language Model", CodeBase = "*AIGC", Icon = "*CoreIcon|AI")]
public abstract class LLmModelAsset : StandaloneAsset<ILLmModel>, ILLmModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LLmModelAsset"/> class with default settings.
    /// </summary>
    protected LLmModelAsset()
        : base(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LLmModelAsset"/> class with the specified name and active state.
    /// </summary>
    /// <param name="name">The name of the model asset.</param>
    /// <param name="active">Whether the model is active by default.</param>
    internal LLmModelAsset(string name, bool active = true)
        : base(name, active)
    {
    }

    /// <summary>
    /// Gets the default icon for this model asset.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.AI;

    /// <summary>
    /// Gets or sets the unique identifier of the language model.
    /// </summary>
    public string ModelId { get; internal protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether the model supports reasoning capabilities.
    /// </summary>
    public bool SupportReasoning { get; internal protected set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the model supports tool calling.
    /// </summary>
    public bool SupportToolCalling { get; internal protected set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the model supports streaming responses.
    /// </summary>
    public bool SupportStreaming { get; internal protected set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the model supports multimodal input.
    /// </summary>
    public bool SupportMultimodel { get; internal protected set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the model supports web search capabilities.
    /// </summary>
    public bool WebSearch { get; internal protected set; }

    /// <summary>
    /// Gets or sets the context window size in thousands of tokens.
    /// </summary>
    public int ContextSizeK { get; internal protected set; }

    /// <summary>
    /// Gets a value indicating whether this model requires manual configuration.
    /// </summary>
    public virtual bool IsManual => false;

    /// <summary>
    /// Gets a value indicating whether the API key is valid for this model.
    /// </summary>
    public virtual bool ApiKeyValid => true;

    /// <summary>
    /// Sets whether this model is active and available for use.
    /// </summary>
    /// <param name="active">True to activate the model, false to deactivate it.</param>
    public void SetModelActive(bool active)
    {
        if (active)
        {
            base.ResolveId();
        }
        else
        {
            base.DetachId();
        }
    }

    /// <summary>
    /// Gets the display text for this model, including capability indicators.
    /// </summary>
    public override string DisplayText
    {
        get
        {
            var text = this.Description;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = ModelId ?? string.Empty;
            }

            if (SupportReasoning)
            {
                text = $"{text}({L("Reasoning")})";
            }

            if (WebSearch)
            {
                text = $"{text}({L("Web Search")})";
            }

            return text;
        }
    }

    /// <summary>
    /// Gets the display status indicating whether the model is enabled or disabled based on API key validity.
    /// </summary>
    public override TextStatus DisplayStatus => ApiKeyValid ? TextStatus.Normal : TextStatus.Disabled;

    /// <summary>
    /// Creates a new language model call instance for single-turn completion.
    /// </summary>
    /// <param name="config">Optional configuration parameters for the model.</param>
    /// <param name="context">Optional function context for tool execution.</param>
    /// <returns>A new <see cref="ILLmCall"/> instance, or null if not supported.</returns>
    public virtual ILLmCall CreateCall(LLmModelParameter config = null, FunctionContext context = null) => null;

    /// <summary>
    /// Creates a new language model chat instance for multi-turn conversation.
    /// </summary>
    /// <param name="config">Optional configuration parameters for the model.</param>
    /// <param name="context">Optional function context for tool execution.</param>
    /// <returns>A new <see cref="ILLmChat"/> instance, or null if not supported.</returns>
    public virtual ILLmChat CreateConversation(LLmModelParameter config = null, FunctionContext context = null) => null;
}

/// <summary>
/// Base class for third-party language model assets that require external API access.
/// </summary>
public abstract class ThirdPartyLLmModelAsset : LLmModelAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyLLmModelAsset"/> class with default settings.
    /// </summary>
    protected ThirdPartyLLmModelAsset()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyLLmModelAsset"/> class with the specified name and active state.
    /// </summary>
    /// <param name="name">The name of the model asset.</param>
    /// <param name="active">Whether the model is active by default.</param>
    protected ThirdPartyLLmModelAsset(string name, bool active = true)
        : base(name, active) { }

}

/// <summary>
/// Builder class for constructing <see cref="LLmModelAsset"/> instances with fluent configuration.
/// </summary>
/// <typeparam name="T">The type of <see cref="LLmModelAsset"/> to build.</typeparam>
public class LLmModelAssetBuilder<T> : AssetBuilder<T>
    where T : LLmModelAsset, new()
{
    /// <summary>
    /// Gets the unique identifier of the language model.
    /// </summary>
    public string ModelId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the model supports reasoning capabilities.
    /// </summary>
    public bool SupportReasoning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the model supports tool calling.
    /// </summary>
    public bool SupportToolCalling { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the model supports streaming responses.
    /// </summary>
    public bool SupportStreaming { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the model supports multimodal input.
    /// </summary>
    public bool SupportMultimodel { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the model supports web search capabilities.
    /// </summary>
    public bool WebSearch { get; private set; }

    /// <summary>
    /// Gets the context window size in thousands of tokens.
    /// </summary>
    public int ContextSizeK { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LLmModelAssetBuilder{T}"/> class
    /// and registers auto-update handlers for all model properties.
    /// </summary>
    public LLmModelAssetBuilder()
    {
        AddAutoUpdate(nameof(ModelId), o => o.ModelId = ModelId);
        AddAutoUpdate(nameof(SupportReasoning), o => o.SupportReasoning = SupportReasoning);
        AddAutoUpdate(nameof(SupportToolCalling), o => o.SupportToolCalling = SupportToolCalling);
        AddAutoUpdate(nameof(SupportStreaming), o => o.SupportStreaming = SupportStreaming);
        AddAutoUpdate(nameof(SupportMultimodel), o => o.SupportMultimodel = SupportMultimodel);
        AddAutoUpdate(nameof(WebSearch), o => o.WebSearch = WebSearch);
        AddAutoUpdate(nameof(ContextSizeK), o => o.ContextSizeK = ContextSizeK);
    }

    /// <summary>
    /// Sets the unique identifier for the language model.
    /// </summary>
    /// <param name="modelId">The model identifier to set.</param>
    public void SetModelId(string modelId)
    {
        if (ModelId == modelId) { return; }

        ModelId = modelId;

        TryUpdateNow(o => o.ModelId = ModelId);

        SetLocalName(modelId);
    }

    /// <summary>
    /// Sets whether the model supports reasoning capabilities.
    /// </summary>
    /// <param name="reasoning">True if the model supports reasoning, false otherwise.</param>
    public void SetSupportReasoning(bool reasoning)
    {
        if (SupportReasoning == reasoning) { return; }

        SupportReasoning = reasoning;

        TryUpdateNow(o => o.SupportReasoning = SupportReasoning);
    }

    /// <summary>
    /// Sets whether the model supports multimodal input.
    /// </summary>
    /// <param name="multimodel">True if the model supports multimodal input, false otherwise.</param>
    public void SetSupportMultimodel(bool multimodel)
    {
        if (SupportMultimodel == multimodel) { return; }

        SupportMultimodel = multimodel;

        TryUpdateNow(o => o.SupportMultimodel = SupportMultimodel);
    }

    /// <summary>
    /// Sets whether the model supports tool calling.
    /// </summary>
    /// <param name="toolCalling">True if the model supports tool calling, false otherwise.</param>
    public void SetSupportToolCalling(bool toolCalling)
    {
        if (SupportToolCalling == toolCalling) { return; }

        SupportToolCalling = toolCalling;

        TryUpdateNow(o => o.SupportToolCalling = SupportToolCalling);
    }

    /// <summary>
    /// Sets whether the model supports streaming responses.
    /// </summary>
    /// <param name="streaming">True if the model supports streaming, false otherwise.</param>
    public void SetSupportStreaming(bool streaming)
    {
        if (SupportStreaming == streaming) { return; }

        SupportStreaming = streaming;

        TryUpdateNow(o => o.SupportStreaming = SupportStreaming);
    }

    /// <summary>
    /// Sets whether the model supports web search capabilities.
    /// </summary>
    /// <param name="webSearch">True if the model supports web search, false otherwise.</param>
    public void SetWebSearch(bool webSearch)
    {
        if (WebSearch == webSearch) { return; }

        WebSearch = webSearch;

        TryUpdateNow(o => o.WebSearch = WebSearch);
    }

    /// <summary>
    /// Sets the context window size in thousands of tokens.
    /// </summary>
    /// <param name="contextSizeK">The context size in thousands of tokens.</param>
    public void SetContextSizeK(int contextSizeK)
    {
        if (ContextSizeK == contextSizeK) { return; }

        ContextSizeK = contextSizeK;

        TryUpdateNow(o => o.ContextSizeK = ContextSizeK);
    }
}