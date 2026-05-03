using OpenAI_API.Models;
using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Abstract base class for OpenAI-compatible API plugins.
/// </summary>
public abstract class BaseOpenAIPlugin : ApiPlugin
{
    /// <summary>
    /// Gets the default base URL for the API endpoint.
    /// </summary>
    public string DefaultBaseUrl { get; }

    /// <summary>
    /// Gets the unique identifier for the AI manufacturer/provider.
    /// </summary>
    public string ManufacturerId { get; }

    /// <summary>
    /// Gets the icon image representing the manufacturer.
    /// </summary>
    public ImageDef? ManufactureIcon { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseOpenAIPlugin"/> class.
    /// </summary>
    /// <param name="defaultUrl">The default API base URL.</param>
    /// <param name="manufacturerId">The unique manufacturer identifier.</param>
    /// <param name="manufactureIcon">Optional icon image for the manufacturer.</param>
    protected BaseOpenAIPlugin(string defaultUrl, string manufacturerId, ImageDef? manufactureIcon = null)
    {
        DefaultBaseUrl = defaultUrl;
        ManufacturerId = manufacturerId;
        ManufactureIcon = manufactureIcon;
    }

    /// <summary>
    /// Gets the API key for authentication with the provider.
    /// </summary>
    public abstract string ApiKey { get; }

    /// <summary>
    /// Gets the base URL for API requests.
    /// </summary>
    public abstract string BaseUrl { get; }

    /// <summary>
    /// Gets the official website URL for the AI provider, or null if not applicable.
    /// </summary>
    public virtual string? OfficialUrl => null;
}

/// <summary>
/// Generic abstract base class for OpenAI-compatible API plugins with specific LLM and image model asset types.
/// </summary>
/// <typeparam name="TLLm">The type of LLM model asset.</typeparam>
/// <typeparam name="TImage">The type of image generation model asset.</typeparam>
public abstract class BaseOpenAIPlugin<TLLm, TImage> : BaseOpenAIPlugin, IViewObject
    where TLLm : LLmModelAsset, new() where TImage : ImageGenAsset, new()
{
    private readonly LLmModelAssetGroupBuilder _llmGroup;
    private readonly LLmModelAssetGroupBuilder _imageGenGroup;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseOpenAIPlugin{TLLm, TImage}"/> class.
    /// </summary>
    /// <param name="defaultUrl">The default API base URL.</param>
    /// <param name="manufacturerId">The unique manufacturer identifier.</param>
    /// <param name="manufactureIcon">Optional icon image for the manufacturer.</param>
    protected BaseOpenAIPlugin(string defaultUrl, string manufacturerId, ImageDef? manufactureIcon = null)
        : base(defaultUrl, manufacturerId, manufactureIcon)
    {
        ApiKeyProperty = new(nameof(ApiKey), "Api Key", toolTips: "Api Key configured in the backend.");
        BaseUrlProperty = new(nameof(BaseUrl), "Api Url", DefaultBaseUrl, toolTips: "Default Api address. Use the original address if not filled in.", true);

        _llmGroup = new(ManufacturerId + "LLm", Description, ManufactureIcon);
        _imageGenGroup = new(ManufacturerId + "Image", Description, ManufactureIcon);
    }

    /// <summary>
    /// Gets the property for configuring the API key.
    /// </summary>
    public StringProperty ApiKeyProperty { get; }

    /// <summary>
    /// Gets the property for configuring the base URL.
    /// </summary>
    public StringProperty BaseUrlProperty { get; }

    /// <summary>
    /// Gets the current API key value from the property.
    /// </summary>
    public override string ApiKey => ApiKeyProperty.Text;

    /// <summary>
    /// Gets the current base URL value from the property.
    /// </summary>
    public override string BaseUrl => BaseUrlProperty.Text;

    /// <summary>
    /// Gets the description of the plugin, defaults to the manufacturer ID.
    /// </summary>
    public override string Description => ManufacturerId;

    /// <summary>
    /// Gets the icon image for the plugin.
    /// </summary>
    public override ImageDef? Icon => ManufactureIcon;

    /// <summary>
    /// Initializes the plugin when a project is loaded, including loading cached model lists.
    /// </summary>
    protected override void AwakeProject()
    {
        base.AwakeProject();

        var list = OkGoDoItHelper.LoadModelList(ManufacturerId);
        if (list != null)
        {
            UpdateModelList(list);
        }
    }

    private void UpdateModelList(List<Model> list)
    {
        if (list is null)
        {
            return;
        }

        _llmGroup.Clear();
        _imageGenGroup.Clear();

        foreach (var model in list)
        {
            AddLLmModel(model.ModelID);
            AddImageModel(model.ModelID);
        }
    }

    private void AddLLmModel(string modelId, string? description = null, bool reasoning = false)
    {
        new LLmModelAssetBuilder<TLLm>()
            .WithModelId(modelId)
            .WithDescription(description)
            .WithReasoning(reasoning)
            .WithToolCalling(!reasoning)
            .WithStreaming(true)
            .WithGroupBuilder(_llmGroup)
            .ResolveAsset();
    }

    private void AddImageModel(string modelId, string? description = null)
    {
        new ImageGenAssetBuilder<TImage>()
            .WithModelId(modelId)
            .WithDescription(description)
            .WithGroupBuilder(_imageGenGroup)
            .ResolveAsset();
    }

    /// <summary>
    /// Synchronizes plugin properties with the view system.
    /// </summary>
    /// <param name="sync">The property synchronization interface.</param>
    /// <param name="context">The synchronization context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        ApiKeyProperty.Sync(sync);
        BaseUrlProperty.Sync(sync);

        if (!string.IsNullOrWhiteSpace(OfficialUrl))
        {
            if (sync.Intent == SyncIntent.View && sync.IsSetterOf("#OfficialWebsite"))
            {
                EditorUtility.OpenBrowser(OfficialUrl);
            }
        }

        if (sync.Intent == SyncIntent.View)
        {
            sync.Sync<int>("ModelCount", _llmGroup?.ChildCount ?? 0);

            if (sync.IsSetterOf("#UpdateModelList"))
            {
                DownloadModelList(context);
            }
        }
    }

    /// <summary>
    /// Configures the view layout and behavior for the plugin inspector.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        bool enabled = EditorServices.LicenseService.GetFeature(EditorFeatures.AigcThirdPartiModel);

        if (!string.IsNullOrWhiteSpace(OfficialUrl))
        {
            setup.Verbose("Please apply for the API Key on the official website before use.");
            setup.Button("#OfficialWebsite", "Official website navigation", Icon);
        }

        ApiKeyProperty.InspectorField(setup, o =>
           o.WithEnabeld(enabled)
           .WithStatus(string.IsNullOrWhiteSpace(ApiKeyProperty.Text) ? TextStatus.Warning : TextStatus.Normal)
           );

        BaseUrlProperty.InspectorField(setup, o => o.WithEnabeld(enabled));

        setup.InspectorFieldOf<int>(new ViewProperty("ModelCount", "Model Count").WithReadOnly());

        setup.Button("#UpdateModelList", "Update model list");
    }

    /// <summary>
    /// Asynchronously downloads the available model list from the API provider.
    /// </summary>
    public async void DownloadModelList(ISyncContext context)
    {
        var modelList = await OkGoDoItHelper.DownloadModelList(BaseUrl, ApiKeyProperty.Text);
        if (modelList is null || modelList.Count == 0)
        {
            await DialogUtility.ShowMessageBoxAsync("Model list download failed.");
            return;
        }

        QueuedAction.Do(() =>
        {
            UpdateModelList(modelList);
            OkGoDoItHelper.SaveModelList(ManufacturerId, modelList);

            DialogUtility.ShowMessageBoxAsync("Model list update successful.");

            context.DoServiceAction<IViewRefresh>(o => o.QueueRefreshView());
        });
    }

}
