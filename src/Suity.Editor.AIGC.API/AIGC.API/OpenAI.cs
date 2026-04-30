using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for OpenAI provider.
/// </summary>
public class OpenAIPlugin : BaseOpenAIPlugin<OpenAILLmModelAsset, OpenAIImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for OpenAI.
    /// </summary>
    public const string DEFAULT_URL = "https://api.openai.com";

    /// <summary>
    /// Gets the singleton instance of the OpenAI plugin.
    /// </summary>
    public static OpenAIPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for OpenAI.
    /// </summary>
    public static Bitmap OpenAIIcon { get; } = Resources.OpenAI.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIPlugin"/> class.
    /// </summary>
    public OpenAIPlugin()
        : base(DEFAULT_URL, "OpenAI", OpenAIIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for OpenAI.
    /// </summary>
    public override string? OfficialUrl => "https://www.openai.com";
}

internal class OpenAICall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAICall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public OpenAICall(OpenAILLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(OpenAIPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for OpenAI provider.
/// </summary>
public class OpenAILLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for OpenAI models.
    /// </summary>
    public override Image DefaultIcon => OpenAIPlugin.OpenAIIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(OpenAIPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="OpenAICall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new OpenAICall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for OpenAI provider.
/// </summary>
public class OpenAIImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for OpenAI image models.
    /// </summary>
    public override Image DefaultIcon => OpenAIPlugin.OpenAIIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(OpenAIPlugin.Instance.ApiKey);
}