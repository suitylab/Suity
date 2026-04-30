using Suity.Collections;
using Suity.Editor.AIGC.Mermaid;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Plugin that manages AI assistant configuration including model presets, parameters, prompts, and component settings.
/// Implements <see cref="IAIAssistantConfig"/> and <see cref="IViewObject"/> for configuration and UI integration.
/// </summary>
public class AIAssistantPlugin : BackendPlugin, IAIAssistantConfig, IViewObject
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="AIAssistantPlugin"/>.
    /// </summary>
    public static AIAssistantPlugin Instance { get; private set; }


    #region Models

    private readonly ButtonProperty _resetModelConfig
        = new(nameof(ResetModelConfig), "Reset Model Config");

    /// <summary>
    /// Gets the default model configuration used when no specific preset is specified.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> DefaultModel { get; }
        = new(nameof(DefaultModel), "Default Model", new(), string.Empty, SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration used for regular chat conversations.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> ChatModel { get; }
        = new(nameof(ChatModel), "Chat Model", new(), "Used for regular chat conversations", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for short summary generation in 1-3 sentences.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> BriefModel { get; }
        = new(nameof(BriefModel), "Summary", new(), "Short summary generation in 1-3 sentences", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for data, code, article, and task summarization.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> SummaryModel { get; }
        = new(nameof(SummaryModel), "Summarization", new(), "Data, code, article, task summary", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for identifier generation.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> IdentifierModel { get; }
        = new(nameof(IdentifierModel), "Identifier Generation", new(), null, SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for query keyword generation.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> QueryKeywordModel { get; }
        = new(nameof(QueryKeywordModel), "Query Keyword Generation", new(), null, SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration used to filter and classify user operations.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> ClassifyModel { get; }
    = new(nameof(ClassifyModel), "Classifier", new(), "Used to filter and classify user operations", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for calling tools through generated content.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> ToolCallingModel { get; }
        = new(nameof(ToolCallingModel), "Tool Calling", new(), "Call tools through generated content", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for calling tools through creative content.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> CreativeToolCallingModel { get; }
        = new(nameof(CreativeToolCallingModel), "Creative Tool Calling", new(), "Call tools through creative content", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for calling tools through precise content.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> ExactToolCallingModel { get; }
        = new(nameof(ExactToolCallingModel), "Precise Tool Calling", new(), "Call tools through precise content", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for converting user requirements into creative writing documents.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> CreativeWritingModel { get; }
        = new(nameof(CreativeWritingModel), "Creative Writing", new(), "Convert user requirements into creative writing documents", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for converting user requirements into design writing documents.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> DesignWritingModel { get; }
        = new(nameof(DesignWritingModel), "Design Writing", new(), "Convert user requirements into design writing documents", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for converting user requirements into technical writing documents.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> TechnicalWritingModel { get; }
        = new(nameof(TechnicalWritingModel), "Technical Writing", new(), "Convert user requirements into technical writing documents", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration used for editor main data and object generation.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> DataGenerateModel { get; }
        = new(nameof(DataGenerateModel), "Data Generation", new(), "Used for editor main data and object generation", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for answering questions.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> AnswerQuestionModel { get; }
        = new(nameof(AnswerQuestionModel), "Answer Question", new(), null, SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for selection operations.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> SelectionModel { get; }
        = new(nameof(SelectionModel), "Selection", new(), null, SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration used for writing code.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> CodingModel { get; }
        = new(nameof(CodingModel), "Code Writing", new(), "Used for writing code", SyncFlag.NotNull);

    /// <summary>
    /// Gets the model configuration for program code and statement repair.
    /// </summary>
    public ValueProperty<LLmModelLevelConfig> CodeRepairModel { get; }
        = new(nameof(CodeRepairModel), "Code Repair", new(), "Program code, statement repair model", SyncFlag.NotNull);


    /// <summary>
    /// Gets the preset model configuration for the specified <paramref name="presetModel"/>.
    /// </summary>
    /// <param name="presetModel">The model preset to retrieve configuration for.</param>
    /// <returns>The <see cref="LLmModelLevelConfig"/> for the given preset, or the default config if not found.</returns>
    public LLmModelLevelConfig GetPresetModelConfig(LLmModelPreset presetModel) => presetModel switch
    {
        LLmModelPreset.Default => DefaultModel.Value,
        LLmModelPreset.Chat => ChatModel.Value,
        LLmModelPreset.Brief => BriefModel.Value,
        LLmModelPreset.Summary => SummaryModel.Value,
        LLmModelPreset.Identifier => IdentifierModel.Value,
        LLmModelPreset.QueryKeyword => QueryKeywordModel.Value,
        LLmModelPreset.Classify => ClassifyModel.Value,
        LLmModelPreset.ToolCalling => ToolCallingModel.Value,
        LLmModelPreset.CreativeToolCalling => CreativeToolCallingModel.Value,
        LLmModelPreset.ExactToolCalling => ExactToolCallingModel.Value,
        LLmModelPreset.CreativeWriting => CreativeWritingModel.Value,
        LLmModelPreset.DesignWriting => DesignWritingModel.Value,
        LLmModelPreset.TechnicalWriting => TechnicalWritingModel.Value,
        LLmModelPreset.DataGenerateToolCalling => DataGenerateModel.Value,
        LLmModelPreset.AnswerQuestion => AnswerQuestionModel.Value,
        LLmModelPreset.Selection => SelectionModel.Value,
        LLmModelPreset.Coding => CodingModel.Value,
        LLmModelPreset.CodeRepair => CodeRepairModel.Value,
        _ => DefaultModel.Value,
    };

    /// <summary>
    /// Creates a preset LLM call instance for the specified model preset and level.
    /// </summary>
    /// <param name="preset">The model preset to use.</param>
    /// <param name="level">The model capability level. Defaults to <see cref="AigcModelLevel.Default"/>.</param>
    /// <param name="ctx">Optional function context for the call.</param>
    /// <returns>An <see cref="ILLmCall"/> instance, or null if the model or config is unavailable.</returns>
    public ILLmCall CreatePresetCall(LLmModelPreset preset, AigcModelLevel level = AigcModelLevel.Default, FunctionContext ctx = null)
    {
        var config = GetPresetModelConfig(preset);
        if (config is null)
        {
            return null;
        }

        var model = LLmModelPlugin.Instance.GetLLmModel(config.ModelType.Value, level);
        if (model is null)
        {
            return null;
        }

        return model.CreateCall(config.Config.Value, ctx);
    }

    /// <summary>
    /// Gets the preset LLM model for the specified model preset.
    /// </summary>
    /// <param name="presetModel">The model preset to retrieve.</param>
    /// <returns>An <see cref="ILLmModel"/> instance, or null if unavailable.</returns>
    public ILLmModel GetPresetModel(LLmModelPreset presetModel)
    {
        var config = GetPresetModelConfig(presetModel);
        if (config is null)
        {
            return null;
        }

        return LLmModelPlugin.Instance.GetLLmModel(config.ModelType.Value);
    }

    /// <summary>
    /// Gets the LLM configuration parameters for the specified model preset.
    /// </summary>
    /// <param name="presetModel">The model preset to retrieve configuration for.</param>
    /// <returns>The <see cref="LLmModelParameter"/> configuration, or null if unavailable.</returns>
    public LLmModelParameter GetPresetModelLLmConfig(LLmModelPreset presetModel)
    {
        var config = GetPresetModelConfig(presetModel);

        return config?.Config.Value;
    }


    /// <summary>
    /// Resets all model type configurations to their default preset values.
    /// </summary>
    public void ResetModelConfig()
    {
        DefaultModel.Value.ModelType.Value = LLmModelType.Default;
        ChatModel.Value.ModelType.Value = LLmModelType.Default;
        BriefModel.Value.ModelType.Value = LLmModelType.Default;
        SummaryModel.Value.ModelType.Value = LLmModelType.Default;
        IdentifierModel.Value.ModelType.Value = LLmModelType.Default;
        QueryKeywordModel.Value.ModelType.Value = LLmModelType.Default;
        ClassifyModel.Value.ModelType.Value = LLmModelType.Lightweight;
        ToolCallingModel.Value.ModelType.Value = LLmModelType.ToolCalls;
        CreativeToolCallingModel.Value.ModelType.Value = LLmModelType.ToolCalls;
        ExactToolCallingModel.Value.ModelType.Value = LLmModelType.ToolCalls;
        CreativeWritingModel.Value.ModelType.Value = LLmModelType.Default;
        DesignWritingModel.Value.ModelType.Value = LLmModelType.Default;
        TechnicalWritingModel.Value.ModelType.Value = LLmModelType.Default;
        DataGenerateModel.Value.ModelType.Value = LLmModelType.Default;
        AnswerQuestionModel.Value.ModelType.Value = LLmModelType.Default;
        SelectionModel.Value.ModelType.Value = LLmModelType.Default;
        CodingModel.Value.ModelType.Value = LLmModelType.Coding;
        CodeRepairModel.Value.ModelType.Value = LLmModelType.Coding;
    }

    #endregion

    #region Parameters

    private readonly ButtonProperty _resetParameterConfig
        = new(nameof(ResetParameterConfig), "Reset Parameter Config");

    /// <summary>
    /// Gets or sets the default language for AI text output. If empty, uses the editor display language.
    /// </summary>
    public StringProperty SpeechLanguage { get; }
        = new(nameof(SpeechLanguage), "Output Language", "", "Default AI text output language. If not set, uses editor display language.");

    /// <summary>
    /// Gets the number of retries allowed for operations that support retrying after failure.
    /// </summary>
    public ValueProperty<int> RetryCount { get; }
        = new(nameof(RetryCount), "Retry Count", 5, "Some operations allow multiple retries. Set the number of retries after the operation fails.");

    /// <summary>
    /// Gets the maximum number of queries allowed.
    /// </summary>
    public ValueProperty<int> MaxQuery { get; }
        = new(nameof(MaxQuery), "Max Query Count", 50);

    /// <summary>
    /// Gets the maximum recursive generation depth for data substructure and linked data.
    /// </summary>
    public ValueProperty<int> MaxGenerateDepth { get; }
        = new(nameof(MaxGenerateDepth), "Max Generation Depth", 10, "Maximum recursive generation depth for data substructure and linked data.");

    /// <summary>
    /// Gets the number of memory selections for each type of data link during a single run.
    /// </summary>
    public ValueProperty<int> LinkedDataMemorySize { get; }
        = new(nameof(LinkedDataMemorySize), "Linked Data Memory Size", 10, "Number of memory selections for each type of data link during a single run.");

    /// <summary>
    /// Gets a value indicating whether to show prompts in conversation. (Obsolete)
    /// </summary>
    [Obsolete]
    public ValueProperty<bool> ShowPromptInConverasation { get; }
        = new(nameof(ShowPromptInConverasation), "Show Prompt in Conversation", false);

    /// <summary>
    /// Gets a value indicating whether to automatically add new enum values to existing enum types when found in generated data.
    /// </summary>
    public ValueProperty<bool> AutoAddNewEnumValue { get; }
        = new(nameof(AutoAddNewEnumValue), "Auto Add New Enum Value", true, "When new enum values exist in generated data, automatically add them to existing enum types.");

    /// <summary>
    /// Resets all parameter configurations to their default values.
    /// </summary>
    public void ResetParameterConfig()
    {
        SpeechLanguage.Text = "";
        RetryCount.Value = 5;
        MaxQuery.Value = 50;
        MaxGenerateDepth.Value = 10;
        LinkedDataMemorySize.Value = 10;
        ShowPromptInConverasation.Value = false;
        AutoAddNewEnumValue.Value = true;
    }

    #endregion

    #region Prompts

    /// <summary>
    /// Gets the collection of prompt overrides used to customize prompts used internally by the system.
    /// </summary>
    public ValueProperty<SArray> PromptOverrides { get; }
        = new(nameof(PromptOverrides), "Prompt Override", new(TypeDefinition.FromNative<AIPromptInfo>().MakeDataLinkType()), "Used to override prompts used internally by the system.", SyncFlag.NotNull | SyncFlag.GetOnly);


    private readonly ButtonProperty _createPromptDoc
        = new(nameof(CreatePromptDocument), "Create Prompt Override", null, CoreIconCache.Template);

    #endregion

    #region Config Components

    private readonly ButtonProperty _resetComponentConfig
        = new(nameof(ResetComponentConfig), "Reset Component Config");


    private readonly SKeyProperty<AIClissifierConfig> _classifyConfigOverride
        = new(nameof(ClassifyConfig) + "Override", "Override Config");

    private readonly ValueProperty<AIClissifierConfig> _classifyConfig
        = new(nameof(ClassifyConfig), "Classifier", new AIClissifierConfig());


    private readonly SKeyProperty<AISubdivideConfig> _subdivideConfigOverride
        = new(nameof(SubdivideConfig) + "Override", "Override Config");

    private readonly ValueProperty<AISubdivideConfig> _subdivideConfig
        = new(nameof(SubdivideConfig), "Subdivider", new AISubdivideConfig());


    private readonly SKeyProperty<AIExtractorConfig> _extractorConfigOverride
        = new(nameof(ExtractorConfig) + "Override", "Override Config");

    private readonly ValueProperty<AIExtractorConfig> _extractorConfig
        = new(nameof(ExtractorConfig), "Extractor", new AIExtractorConfig());


    private readonly SKeyProperty<AISupportConfig> _supportConfigOverride
        = new(nameof(SupportConfig) + "Override", "Override Config");

    private readonly ValueProperty<AISupportConfig> _supportConfig
        = new(nameof(SupportConfig), "Support", new AISupportConfig());


    private readonly SKeyProperty<AIKnowledgeConfig> _knowledgeConfigOverride
        = new(nameof(KnowledgeConfig) + "Override", "Override Config");

    private readonly ValueProperty<AIKnowledgeConfig> _knowledgeConfig
        = new(nameof(KnowledgeConfig), "Knowledge Base", new AIKnowledgeConfig());


    private readonly SKeyProperty<AIDataGenerationConfig> _dataGenerationConfigOverride
        = new(nameof(DataGenerationConfig) + "Override", "Override Config");

    private readonly ValueProperty<AIDataGenerationConfig> _dataGenerationConfig
        = new(nameof(DataGenerationConfig), "Data Generation", new AIDataGenerationConfig());


    /// <summary>
    /// Gets the classifier configuration, using override if available.
    /// </summary>
    public AIClissifierConfig ClassifyConfig => _classifyConfigOverride.Target ?? _classifyConfig.Value;

    /// <summary>
    /// Gets the subdivider configuration, using override if available.
    /// </summary>
    public AISubdivideConfig SubdivideConfig => _subdivideConfigOverride.Target ?? _subdivideConfig.Value;

    /// <summary>
    /// Gets the extractor configuration, using override if available.
    /// </summary>
    public AIExtractorConfig ExtractorConfig => _extractorConfigOverride.Target ?? _extractorConfig.Value;

    /// <summary>
    /// Gets the support configuration, using override if available.
    /// </summary>
    public AISupportConfig SupportConfig => _supportConfigOverride.Target ?? _supportConfig.Value;

    /// <summary>
    /// Gets the knowledge base configuration, using override if available.
    /// </summary>
    public AIKnowledgeConfig KnowledgeConfig => _knowledgeConfigOverride.Target ?? _knowledgeConfig.Value;

    /// <summary>
    /// Gets the data generation configuration, using override if available.
    /// </summary>
    public AIDataGenerationConfig DataGenerationConfig => _dataGenerationConfigOverride.Target ?? _dataGenerationConfig.Value;

    /// <summary>
    /// Resets all component configurations to their default values.
    /// </summary>
    public void ResetComponentConfig()
    {
        _classifyConfig.Value = new();
        _subdivideConfig.Value = new();
        _extractorConfig.Value = new();
        _supportConfig.Value = new();
        _knowledgeConfig.Value = new();
        _dataGenerationConfig.Value = new();
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AIAssistantPlugin"/> class.
    /// Sets up the singleton instance, configures sync flags, and applies default model settings.
    /// </summary>
    public AIAssistantPlugin()
    {
        Instance ??= this;
        AIAssistantService._config = this;

        _classifyConfig.Flag |= SyncFlag.NotNull;
        _subdivideConfig.Flag |= SyncFlag.NotNull;
        _extractorConfig.Flag |= SyncFlag.NotNull;
        _supportConfig.Flag |= SyncFlag.NotNull;
        _knowledgeConfig.Flag |= SyncFlag.NotNull;
        _dataGenerationConfig.Flag |= SyncFlag.NotNull;


        ResetModelConfig();

        _resetModelConfig.Property.WithConfirm("Reset all model config?");
        _resetParameterConfig.Property.WithConfirm("Reset all parameter config?");
        _resetComponentConfig.Property.WithConfirm("Reset all component config?");

        PromptOverrides.Property/*.WithExpand()*/.WithWriteBack();
    }

    /// <inheritdoc/>
    public override string Description => "AI Assistant";

    /// <inheritdoc/>
    public override Image Icon => CoreIconCache.Assistant;

    /// <inheritdoc/>
    public override int Order => 950;


    /// <inheritdoc/>
    protected override void Awake(PluginContext context)
    {
        base.Awake(context);

        AIAssistantServiceBK.Instance.Initialize();
        
        AIDocumentAssistantResolver.Instance.Initialize();
        AIPromptManager.Instance.Initialize();
    }

    /// <inheritdoc/>
    protected override Task StartProject()
    {
        RebuildPrompt();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the speech language for AI output, falling back to the editor display language if not set.
    /// </summary>
    /// <returns>The configured speech language or the editor's current language.</returns>
    public string GetSpeechLanguage()
    {
        string lang = SpeechLanguage;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            return lang;
        }

        return EditorServices.LocalizationService.LanguageName;
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IMermaidService))
        {
            return MermaidService.Instance;
        }

        return null;
    }

    #region Sync

    /// <summary>
    /// Synchronizes all plugin properties with the given sync context, handling reset actions and property updates.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context for executing service actions.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        // Models
        if (_resetModelConfig.Sync(sync) == ButtonValue.Clicked)
        {
            ResetModelConfig();
            context.DoServiceAction<IViewSave>(o => o.SaveView());
        }

        DefaultModel.Sync(sync);
        ChatModel.Sync(sync);
        BriefModel.Sync(sync);
        SummaryModel.Sync(sync);
        IdentifierModel.Sync(sync);
        QueryKeywordModel.Sync(sync);
        ClassifyModel.Sync(sync);
        ToolCallingModel.Sync(sync);
        CreativeToolCallingModel.Sync(sync);
        ExactToolCallingModel.Sync(sync);
        CreativeWritingModel.Sync(sync);
        DesignWritingModel.Sync(sync);
        TechnicalWritingModel.Sync(sync);
        DataGenerateModel.Sync(sync);
        AnswerQuestionModel.Sync(sync);
        SelectionModel.Sync(sync);
        CodingModel.Sync(sync);
        CodeRepairModel.Sync(sync);

        // Parameters
        if (_resetParameterConfig.Sync(sync) == ButtonValue.Clicked)
        {
            ResetParameterConfig();
            context.DoServiceAction<IViewSave>(o => o.SaveView());
        }

        SpeechLanguage.Sync(sync);
        if (sync.IsSetterOf(nameof(SpeechLanguage)))
        {
            AIRequest.DefaultSpeechLanguage = SpeechLanguage;
        }

        RetryCount.Sync(sync);
        MaxQuery.Sync(sync);
        MaxGenerateDepth.Sync(sync);
        LinkedDataMemorySize.Sync(sync);
        ShowPromptInConverasation.Sync(sync);
        AutoAddNewEnumValue.Sync(sync);

        // Components
        if (_resetComponentConfig.Sync(sync) == ButtonValue.Clicked)
        {
            ResetComponentConfig();
            context.DoServiceAction<IViewSave>(o => o.SaveView());
        }

        { // Prompts
            PromptOverrides.Sync(sync);
            if (sync.IsSetterOf(nameof(PromptOverrides)))
            {
                EditorUtility.AddDelayedAction(_rebuildPromptAction);
            }
        } // Prompts

        { // Custom
            if (_createPromptDoc.Sync(sync) == ButtonValue.Clicked)
            {
                CreatePromptDocument();
            }
        } // Custom
    }

    /// <summary>
    /// Sets up the inspector view for this plugin, organizing properties into labeled sections.
    /// </summary>
    /// <param name="setup">The view setup interface to configure.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.LabelWithIcon("Parameters", CoreIconCache.System);
        SpeechLanguage.InspectorField(setup);
        RetryCount.InspectorField(setup);
        MaxQuery.InspectorField(setup);
        MaxGenerateDepth.InspectorField(setup);
        LinkedDataMemorySize.InspectorField(setup);
        //ShowPromptInConverasation.InspectorField(setup);
        AutoAddNewEnumValue.InspectorField(setup);

        _resetParameterConfig.InspectorField(setup);


        setup.LabelWithIcon("Model Config", CoreIconCache.AI);

        DefaultModel.InspectorField(setup);
        ChatModel.InspectorField(setup);
        BriefModel.InspectorField(setup);
        SummaryModel.InspectorField(setup);
        IdentifierModel.InspectorField(setup);
        ClassifyModel.InspectorField(setup);
        QueryKeywordModel.InspectorField(setup);
        ToolCallingModel.InspectorField(setup);
        CreativeToolCallingModel.InspectorField(setup);
        ExactToolCallingModel.InspectorField(setup);
        CreativeWritingModel.InspectorField(setup);
        DesignWritingModel.InspectorField(setup);
        TechnicalWritingModel.InspectorField(setup);
        DataGenerateModel.InspectorField(setup);
        AnswerQuestionModel.InspectorField(setup);
        SelectionModel.InspectorField(setup);
        CodingModel.InspectorField(setup);
        CodeRepairModel.InspectorField(setup);

        _resetModelConfig.InspectorField(setup);


        setup.LabelWithIcon("Prompt Config", CoreIconCache.Template);
        PromptOverrides.InspectorField(setup);

        setup.LabelWithIcon("Custom", CoreIconCache.Custom);
        setup.Verbose("You can optimize interaction with language models through custom prompts.");

        _createPromptDoc.InspectorField(setup);
    }
    #endregion

    #region Prompt
    private readonly RebuildPromptAction _rebuildPromptAction = new();
    private readonly Dictionary<string, AIPromptInfo> _overridePrompts = [];

    private void RebuildPrompt()
    {
        var prompts = PromptOverrides.Value.Items
        .OfType<SKey>()
        .Select(o => o.GetTargetObject()?.Controller as AIPromptInfo)
        .SkipNull()
        .ToArray();

        _overridePrompts.Clear();

        foreach (var prompt in prompts)
        {
            _overridePrompts[prompt.PromptId.Value] = prompt;
        }

        EditorServices.SystemLog.AddLog("Rebuild overrivde prompt: " + _overridePrompts.Count);
    }


    /// <summary>
    /// Retrieves a prompt record by its ID, merging override settings with prototype defaults.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt to retrieve.</param>
    /// <returns>An <see cref="AIPromptRecord"/> containing the prompt text, model preset, and model level, or null if not found.</returns>
    public AIPromptRecord GetPrompt(string promptId)
    {
        if (string.IsNullOrWhiteSpace(promptId))
        {
            return null;
        }

        string prompt = null;
        LLmModelPreset preset = LLmModelPreset.Default;
        AigcModelLevel level = AigcModelLevel.Default;

        if (_overridePrompts.TryGetValue(promptId, out var promptInfo))
        {
            string overridePrompt = promptInfo.Prompt.Text;
            if (!string.IsNullOrWhiteSpace(overridePrompt))
            {
                prompt = overridePrompt;
            }
            if (promptInfo.ModelPreset.Value != LLmModelPreset.Default)
            {
                preset = promptInfo.ModelPreset.Value;
            }
            if (promptInfo.ModelLevel.Value != AigcModelLevel.Default)
            {
                level = promptInfo.ModelLevel.Value;
            }
        }

        if (AIPromptManager.Instance.GetPrototype(promptId) is { } prototype)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = prototype.Prompt;
            }
            if (preset == LLmModelPreset.Default)
            {
                preset = prototype.ModelPreset;
            }
            if (level == AigcModelLevel.Default)
            {
                level = prototype.ModelLevel;
            }
        }

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return new(prompt, preset, level);
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Creates a new prompt override document populated with all available prompt prototypes.
    /// </summary>
    public void CreatePromptDocument()
    {
        var docFormat = DocumentManager.Instance.GetDocumentFormat("DataEdit");
        if (docFormat is null)
        {
            return;
        }

        if (TypeDefinition.FromNative<AIPromptInfo>()?.Target is not DCompond type)
        {
            return;
        }

        if (docFormat.AutoNewDocument("PromptOverride")?.Content is not IDataGridDocument doc)
        {
            return;
        }

        doc.AddSharedType(type);

        foreach (var info in AIPromptManager.Instance.Prompts)
        {
            var obj = Cloner.Clone(info.Target);
            doc.AddData(info.Name, [obj], L(info.Title));
        }

        (doc as Document)?.SaveDelayed();
        (doc as Document)?.ShowView();
    }

    /// <summary>
    /// A delayed action that triggers a prompt rebuild when invoked.
    /// </summary>
    public class RebuildPromptAction : DelayedAction
    {
        /// <inheritdoc/>
        public override void DoAction() => Instance.RebuildPrompt();
    }


    #endregion
}
