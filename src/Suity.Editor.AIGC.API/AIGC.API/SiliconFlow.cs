using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for SiliconFlow AI provider.
/// </summary>
public class SiliconFlowPlugin : BaseOpenAIPlugin<SiliconFlowLLmModelAsset, SiliconFlowImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for SiliconFlow.
    /// </summary>
    public const string DEFAULT_URL = "https://api.siliconflow.cn";

    /// <summary>
    /// Gets the singleton instance of the SiliconFlow plugin.
    /// </summary>
    public static SiliconFlowPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for SiliconFlow.
    /// </summary>
    public static Bitmap SiliconFlowIcon { get; } = Resources.SiliconFlow.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="SiliconFlowPlugin"/> class.
    /// </summary>
    public SiliconFlowPlugin()
        : base(DEFAULT_URL, "SiliconFlow", SiliconFlowIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for SiliconFlow.
    /// </summary>
    public override string? OfficialUrl => "https://siliconflow.cn";
}

internal class SiliconFlowCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiliconFlowCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public SiliconFlowCall(SiliconFlowLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(SiliconFlowPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for SiliconFlow provider.
/// </summary>
public class SiliconFlowLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for SiliconFlow models.
    /// </summary>
    public override Image DefaultIcon => SiliconFlowPlugin.SiliconFlowIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(SiliconFlowPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="SiliconFlowCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new SiliconFlowCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for SiliconFlow provider.
/// </summary>
public class SiliconFlowImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for SiliconFlow image models.
    /// </summary>
    public override Image DefaultIcon => SiliconFlowPlugin.SiliconFlowIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(SiliconFlowPlugin.Instance.ApiKey);
}