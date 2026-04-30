using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for OpenRouter AI provider.
/// </summary>
public class OpenRouterPlugin : BaseOpenAIPlugin<OpenRouterLLmModelAsset, OpenRouterImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for OpenRouter.
    /// </summary>
    public const string DEFAULT_URL = "https://openrouter.ai/api";

    /// <summary>
    /// Gets the singleton instance of the OpenRouter plugin.
    /// </summary>
    public static OpenRouterPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for OpenRouter.
    /// </summary>
    public static Bitmap OpenRouterIcon { get; } = Resources.OpenRouter.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterPlugin"/> class.
    /// </summary>
    public OpenRouterPlugin()
        : base(DEFAULT_URL, "OpenRouter", OpenRouterIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for OpenRouter.
    /// </summary>
    public override string? OfficialUrl => "https://openrouter.ai/";
}

internal class OpenRouterCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenRouterCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public OpenRouterCall(OpenRouterLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(OpenRouterPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for OpenRouter provider.
/// </summary>
public class OpenRouterLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for OpenRouter models.
    /// </summary>
    public override Image DefaultIcon => OpenRouterPlugin.OpenRouterIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(OpenRouterPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="OpenRouterCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new OpenRouterCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for OpenRouter provider.
/// </summary>
public class OpenRouterImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for OpenRouter image models.
    /// </summary>
    public override Image DefaultIcon => OpenRouterPlugin.OpenRouterIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(OpenRouterPlugin.Instance.ApiKey);
}