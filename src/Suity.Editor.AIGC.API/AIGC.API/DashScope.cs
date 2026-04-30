using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for DashScope (Alibaba Cloud Bailian) AI provider.
/// </summary>
public class DashScopePlugin : BaseOpenAIPlugin<DashScopeLLmModelAsset, DashScopeImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for DashScope.
    /// </summary>
    public const string DEFAULT_URL = "https://dashscope.aliyuncs.com/compatible-mode";

    /// <summary>
    /// Gets the singleton instance of the DashScope plugin.
    /// </summary>
    public static DashScopePlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for DashScope.
    /// </summary>
    public static Bitmap DashScopeIcon { get; } = Resources.Bailian.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="DashScopePlugin"/> class.
    /// </summary>
    public DashScopePlugin()
        : base(DEFAULT_URL, "DashScope", DashScopeIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for DashScope (Alibaba Cloud Bailian console).
    /// </summary>
    public override string? OfficialUrl => "https://bailian.console.aliyun.com/";
}

internal class DashScopeCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashScopeCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public DashScopeCall(DashScopeLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(DashScopePlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for DashScope provider.
/// </summary>
public class DashScopeLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for DashScope models.
    /// </summary>
    public override Image DefaultIcon => DashScopePlugin.DashScopeIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(DashScopePlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="DashScopeCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new DashScopeCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for DashScope provider.
/// </summary>
public class DashScopeImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for DashScope image models.
    /// </summary>
    public override Image DefaultIcon => DashScopePlugin.DashScopeIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(DashScopePlugin.Instance.ApiKey);
}