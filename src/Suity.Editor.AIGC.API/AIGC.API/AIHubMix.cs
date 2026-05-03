using Suity.Drawing;
using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for AIHubMix AI provider.
/// </summary>
public class AIHubMixPlugin : BaseOpenAIPlugin<AIHubMixLLmModelAsset, AIHubMixImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for AIHubMix.
    /// </summary>
    public const string DEFAULT_URL = "https://aihubmix.com";

    /// <summary>
    /// Gets the singleton instance of the AIHubMix plugin.
    /// </summary>
    public static AIHubMixPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for AIHubMix.
    /// </summary>
    public static BitmapDef AIHubMixIcon { get; } = Resources.AIHubMix.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="AIHubMixPlugin"/> class.
    /// </summary>
    public AIHubMixPlugin()
        : base(DEFAULT_URL, "AIHubMix", AIHubMixIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for AIHubMix.
    /// </summary>
    public override string? OfficialUrl => "https://aihubmix.com/";
}

internal class AIHubMixCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIHubMixCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public AIHubMixCall(AIHubMixLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(AIHubMixPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for AIHubMix provider.
/// </summary>
public class AIHubMixLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for AIHubMix models.
    /// </summary>
    public override ImageDef DefaultIcon => AIHubMixPlugin.AIHubMixIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(AIHubMixPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="AIHubMixCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new AIHubMixCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for AIHubMix provider.
/// </summary>
public class AIHubMixImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for AIHubMix image models.
    /// </summary>
    public override ImageDef DefaultIcon => AIHubMixPlugin.AIHubMixIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(AIHubMixPlugin.Instance.ApiKey);
}