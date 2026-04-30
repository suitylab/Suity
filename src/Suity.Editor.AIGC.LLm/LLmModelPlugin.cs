using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Plugin that manages LLM model configurations, presets, and AI generation settings.
/// </summary>
public class LLmModelPlugin : EditorPlugin, IViewObject
{
    /*private static readonly ServiceStore<AigcServiceModelSender> _sender = new();*/

    /// <summary>
    /// Gets the singleton instance of the LLM model plugin.
    /// </summary>
    public static LLmModelPlugin Instance { get; private set; }


    private readonly ValueProperty<AigcModelLevel> _llmLevel
        = new(nameof(LLmLevel), "Current LLM Level", AigcModelLevel.Low);

    private readonly ValueProperty<LLmModelPresetConfig> _lowLLmPreset
        = new(nameof(LowLLmPreset), "Low-tier LLM", new(), null, SyncFlag.GetOnly | SyncFlag.NotNull);

    private readonly ValueProperty<LLmModelPresetConfig> _mediumLLmPreset
        = new(nameof(MediumLLmPreset), "Medium-tier LLM", new(), null, SyncFlag.GetOnly | SyncFlag.NotNull);

    private readonly ValueProperty<LLmModelPresetConfig> _highLLmPreset
        = new(nameof(HighLLmPreset), "High-tier LLM", new(), null, SyncFlag.GetOnly | SyncFlag.NotNull);


    private readonly AssetProperty<IEmbeddingModel> _defaultEmbedding
        = new(nameof(DefaultEmbedding), "Default Embedding Model", toolTips: "Note: Different models have different vector distributions. The project must use the same embedding model. If you need to replace the model, you will need to regenerate all vectors.");


    private readonly ValueProperty<AigcModelLevel> _imageGenLevel
        = new(nameof(ImageGenLevel), "Current Image Model Level", AigcModelLevel.Low);

    private readonly AssetProperty<IImageGenModel> _lowImageGenModel
        = new(nameof(LowImageGenModel), "Low-tier Image Model");

    private readonly AssetProperty<IImageGenModel> _mediumImageGenModel
        = new(nameof(MediumImageGenModel), "Medium-tier Image Model");

    private readonly AssetProperty<IImageGenModel> _highImageGenModel
        = new(nameof(HighImageGenModel), "High-tier Image Model");


    private readonly StringProperty _localizedLanguage
        = new(nameof(LocalizedLanguage), "Localiazed speech language of AI response.");


    private readonly ValueProperty<bool> _aiCallLog
        = new("AICallLog", "AI Call Log", false, "Store each AI call's request and response to a file in the project's root folder 'Users\\LLmLog\\'.");


    /// <summary>
    /// Gets the low-tier LLM preset configuration.
    /// </summary>
    public LLmModelPresetConfig LowLLmPreset => _lowLLmPreset.Value;

    /// <summary>
    /// Gets the medium-tier LLM preset configuration.
    /// </summary>
    public LLmModelPresetConfig MediumLLmPreset => _mediumLLmPreset.Value;

    /// <summary>
    /// Gets the high-tier LLM preset configuration.
    /// </summary>
    public LLmModelPresetConfig HighLLmPreset => _highLLmPreset.Value;

    /// <summary>
    /// Gets the low-tier image generation model.
    /// </summary>
    public IImageGenModel LowImageGenModel => _lowImageGenModel.Target;

    /// <summary>
    /// Gets the medium-tier image generation model.
    /// </summary>
    public IImageGenModel MediumImageGenModel => _mediumImageGenModel.Target;

    /// <summary>
    /// Gets the high-tier image generation model.
    /// </summary>
    public IImageGenModel HighImageGenModel => _highImageGenModel.Target;


    /// <summary>
    /// Gets the current LLM model level.
    /// </summary>
    public AigcModelLevel LLmLevel => _llmLevel.Value;

    /// <summary>
    /// Gets the current image generation model level.
    /// </summary>
    public AigcModelLevel ImageGenLevel => _imageGenLevel.Value;

    /// <summary>
    /// Gets the default embedding model.
    /// </summary>
    public IEmbeddingModel DefaultEmbedding => _defaultEmbedding.Target;

    /// <summary>
    /// Gets the localized speech language for AI responses.
    /// </summary>
    public string LocalizedLanguage => _localizedLanguage.Text;


    /*private SuityLLmModelAsset _suityDefaultModel;*/

    /// <summary>
    /// Initializes a new instance of the <see cref="LLmModelPlugin"/> class.
    /// </summary>
    public LLmModelPlugin()
    {
        Instance ??= this;

        _lowLLmPreset.Property.WithWriteBack();
        _mediumLLmPreset.Property.WithWriteBack();
        _highLLmPreset.Property.WithWriteBack();
    }


    /// <inheritdoc/>
    public override string Description => "AI Model";

    /// <inheritdoc/>
    public override Image Icon => CoreIconCache.Model;

    /// <inheritdoc/>
    public override int Order => 1000;



    // public ManualLLmModelAsset ManualModel { get; private set; }

    /// <summary>
    /// Gets the LLM preset configuration for the specified model level.
    /// </summary>
    /// <param name="level">The model level to get the preset for.</param>
    /// <returns>The LLM preset configuration for the specified level.</returns>
    public LLmModelPresetConfig GetLLmPreset(AigcModelLevel level) => level switch
    {
        AigcModelLevel.Low => _lowLLmPreset.Value,
        AigcModelLevel.Medium => _mediumLLmPreset.Value,
        AigcModelLevel.High => _highLLmPreset.Value,
        _ => _lowLLmPreset.Value,
    };

    /// <summary>
    /// Gets the image generation model for the specified model level.
    /// </summary>
    /// <param name="level">The model level to get the image generation model for.</param>
    /// <returns>The image generation model for the specified level.</returns>
    public IImageGenModel GetImageGenModel(AigcModelLevel level) => level switch
    {
        AigcModelLevel.Low => _lowImageGenModel.Target,
        AigcModelLevel.Medium => _mediumImageGenModel.Target,
        AigcModelLevel.High => _highImageGenModel.Target,
        _ => _lowImageGenModel.Target,
    };

    /// <summary>
    /// Gets the current image generation model based on the configured level.
    /// </summary>
    /// <returns>The current image generation model.</returns>
    public IImageGenModel GetImageGenModel() => GetImageGenModel(_imageGenLevel.Value);


    /// <summary>
    /// Gets the current LLM preset configuration based on the configured level.
    /// </summary>
    /// <returns>The current LLM preset configuration.</returns>
    public LLmModelPresetConfig GetCurrentLLmPreset() => GetLLmPreset(_llmLevel.Value);

    /// <summary>
    /// Gets the current image generation model based on the configured level.
    /// </summary>
    /// <returns>The current image generation model.</returns>
    public IImageGenModel GetCurrentImageGenModel() => GetImageGenModel(_imageGenLevel.Value);


    /// <summary>
    /// Gets the LLM model of the specified type using the current preset level.
    /// </summary>
    /// <param name="type">The type of LLM model to get.</param>
    /// <returns>The LLM model of the specified type.</returns>
    public ILLmModel GetLLmModel(LLmModelType type) => GetLLmPreset(_llmLevel.Value).GetModel(type);

    /// <summary>
    /// Gets the LLM model of the specified type and level.
    /// </summary>
    /// <param name="type">The type of LLM model to get.</param>
    /// <param name="level">The model level. If Default, uses the current level.</param>
    /// <returns>The LLM model of the specified type and level.</returns>
    public ILLmModel GetLLmModel(LLmModelType type, AigcModelLevel level)
    {
        if (level == AigcModelLevel.Default)
        {
            level = _llmLevel.Value;
        }

        return GetLLmPreset(level).GetModel(type);
    }

    /// <summary>
    /// Gets the default LLM model parameters for the current preset.
    /// </summary>
    public LLmModelParameter DefaultParameters => GetLLmPreset(_llmLevel.Value).DefaultParameters.Value;

    /// <summary>
    /// Gets the default LLM model for the current preset.
    /// </summary>
    public ILLmModel DefaultModel => GetLLmPreset(_llmLevel.Value).Default.Target;

    /// <summary>
    /// Gets a value indicating whether AI call logging is enabled.
    /// </summary>
    public bool AICallLog => _aiCallLog.Value;

    /// <summary>
    /// Gets or sets the project configuration state for AIGC.
    /// </summary>
    internal AigcConfigState ProjectConfig { get; set; } = new();


    /// <inheritdoc/>
    protected override void Awake(PluginContext context)
    {
        base.Awake(context);

        LLmServiceBK.Instance.Initialize();
    }

    /// <inheritdoc/>
    protected override void AwakeProject()
    {
        base.AwakeProject();

        if (GetProjectState() is AigcConfigState state)
        {
            ProjectConfig = state;
            //AigcStartupWindow.Instance.LoadConfig(ProjectConfig);
            AigcChatToolWindow.Instance.LoadConfig(ProjectConfig);
        }
        else
        {
            AigcChatToolWindow.Instance.SetDefaultConfig();
        }

        //ManualModel = new ManualLLmModelAsset();
    }

    /// <inheritdoc/>
    protected override async Task StartProject()
    {
        //var sender = _sender.Get();

        //try
        //{
        //    var models = await sender.SendGetAigcModels(new GetAigcModels { }).ToTask(TimeSpan.FromSeconds(10));
        //    foreach (var model in models.Models.SkipNull().Where(o => !string.IsNullOrWhiteSpace(o.ModelId)))
        //    {
        //        try
        //        {
        //            var suityModel = new SuityLLmModelAsset(model);
        //            _suityDefaultModel ??= suityModel;
        //        }
        //        catch (Exception err)
        //        {
        //            err.LogError($"Create llm model asset failed : {model.ModelId}");
        //        }
        //    }
        //}
        //catch (Exception err)
        //{
        //    err.LogError();
        //}

/*        if (_lowLLmPreset.Value.Default.Target is null)
        {
            _lowLLmPreset.Value.Default.Target = _suityDefaultModel; // ?? (ILLmModel)ManualModel;
        }
        if (_mediumLLmPreset.Value.Default.Target is null)
        {
            _mediumLLmPreset.Value.Default.Target = _suityDefaultModel; // ?? (ILLmModel)ManualModel;
        }
        if (_highLLmPreset.Value.Default.Target is null)
        {
            _highLLmPreset.Value.Default.Target = _suityDefaultModel; // ?? (ILLmModel)ManualModel;
        }*/

        UpdateBaseLLmCall();
    }

    /// <inheritdoc/>
    protected override void StopProject()
    {
        base.StopProject();

        if (ProjectConfig != null)
        {
            //AigcStartupWindow.Instance.SaveConfig(ProjectConfig);
            AigcChatToolWindow.Instance.SaveConfig(ProjectConfig);

            SetProjectState(ProjectConfig);
        }
    }

    /// <summary>
    /// Synchronizes plugin properties with the specified sync context.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _llmLevel.Sync(sync);
        _lowLLmPreset.Sync(sync);
        _mediumLLmPreset.Sync(sync);
        _highLLmPreset.Sync(sync);

        _defaultEmbedding.Sync(sync);
        _lowImageGenModel.Sync(sync);
        _mediumImageGenModel.Sync(sync);
        _highImageGenModel.Sync(sync);

        _imageGenLevel.Sync(sync);

        _localizedLanguage.Sync(sync);

        _aiCallLog.Sync(sync);

        if (sync.IsSetter())
        {
            if (_llmLevel.Value == AigcModelLevel.Default)
            {
                _llmLevel.Value = AigcModelLevel.Low;
            }

            UpdateBaseLLmCall();
        }
    }

    /// <summary>
    /// Sets up the view inspector fields for the plugin properties.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.LabelWithIcon("LLM", CoreIconCache.Model);

        _llmLevel.InspectorField(setup);

        _lowLLmPreset.Property.WithStatus(TextStatus.Normal);
        _mediumLLmPreset.Property.WithStatus(TextStatus.Normal);
        _highLLmPreset.Property.WithStatus(TextStatus.Normal);

        switch (_llmLevel.Value)
        {
            case AigcModelLevel.Medium:
                _mediumLLmPreset.Property.WithStatus(TextStatus.Checked);
                break;

            case AigcModelLevel.High:
                _highLLmPreset.Property.WithStatus(TextStatus.Checked);
                break;

            case AigcModelLevel.Default:
            case AigcModelLevel.Low:
            default:
                _lowLLmPreset.Property.WithStatus(TextStatus.Checked);
                break;
        }

        _lowLLmPreset.InspectorField(setup);
        _mediumLLmPreset.InspectorField(setup);
        _highLLmPreset.InspectorField(setup);

        // Temporarily disabled embedding model
        // setup.LabelWithIcon("Embedding Model", CoreIconCache.Knowledge);
        // _defaultEmbedding.InspectorField(setup);

        setup.LabelWithIcon("Image Model", CoreIconCache.Image);
        _imageGenLevel.InspectorField(setup);
        _lowImageGenModel.InspectorField(setup);
        _mediumImageGenModel?.InspectorField(setup);
        _highImageGenModel?.InspectorField(setup);

        setup.LabelWithIcon("Localization", CoreIconCache.World);
        string lang = EditorServices.LocalizationService.LanguageName;
        if (string.IsNullOrWhiteSpace(lang))
        {
            lang = "English";
        }
        _localizedLanguage.Property.WithHintText(lang);
        _localizedLanguage.InspectorField(setup);

        setup.LabelWithIcon("Debug", CoreIconCache.Debug);
        _aiCallLog.InspectorField(setup);
    }

    /// <summary>
    /// Validates all model configurations and collects any error messages.
    /// </summary>
    /// <param name="message">The list to collect error messages.</param>
    /// <returns>True if all configurations are valid; otherwise, false.</returns>
    public bool GetAllModelConfigValid(ref List<string> message) 
        => _lowLLmPreset.Value?.GetIsValud(ref message) == true
            && _mediumLLmPreset.Value?.GetIsValud(ref message) == true
            && _highLLmPreset.Value?.GetIsValud(ref message) == true;

    /// <summary>
    /// Validates the current model configuration and collects any error messages.
    /// </summary>
    /// <param name="message">The list to collect error messages.</param>
    /// <returns>True if the current configuration is valid; otherwise, false.</returns>
    public bool GetCurrentModelConfigValid(ref List<string> message)
        => GetCurrentLLmPreset()?.GetIsValud(ref message) == true;


    private void UpdateBaseLLmCall()
    {
        if (DefaultParameters is { } param)
        {
            BaseLLmCall.DefaultLLmConfig = Cloner.Clone(param);
        }
        else
        {
            BaseLLmCall.DefaultLLmConfig = new LLmModelParameter();
        }

        BaseLLmCall._aiCallLog = _aiCallLog.Value;
    }
}

/// <summary>
/// Represents the configuration state for AIGC features in a project.
/// </summary>
public class AigcConfigState
{
    /// <summary>
    /// Gets or sets the ID of the last startup asset.
    /// </summary>
    public Guid LastStartupAssetId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the last chat asset.
    /// </summary>
    public Guid LastChatAssetId { get; set; }
}