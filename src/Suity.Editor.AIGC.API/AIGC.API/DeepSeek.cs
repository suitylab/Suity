using Suity.Drawing;
using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for DeepSeek AI provider.
/// </summary>
public class DeepSeekPlugin : BaseOpenAIPlugin<DeepSeekLLmModelAsset, DeepSeekImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for DeepSeek.
    /// </summary>
    public const string DEFAULT_URL = "https://api.deepseek.com";

    /// <summary>
    /// Gets the singleton instance of the DeepSeek plugin.
    /// </summary>
    public static DeepSeekPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for DeepSeek.
    /// </summary>
    public static BitmapDef DeepSeekIcon { get; } = Resources.DeepSeek.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeepSeekPlugin"/> class.
    /// </summary>
    public DeepSeekPlugin()
        : base(DEFAULT_URL, "DeepSeek", DeepSeekIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for DeepSeek.
    /// </summary>
    public override string? OfficialUrl => "https://deepseek.com/";
}

internal class DeepSeekCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeepSeekCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public DeepSeekCall(DeepSeekLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(DeepSeekPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for DeepSeek provider.
/// </summary>
public class DeepSeekLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for DeepSeek models.
    /// </summary>
    public override ImageDef DefaultIcon => DeepSeekPlugin.DeepSeekIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(DeepSeekPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="DeepSeekCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new DeepSeekCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for DeepSeek provider.
/// </summary>
public class DeepSeekImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for DeepSeek image models.
    /// </summary>
    public override ImageDef DefaultIcon => DeepSeekPlugin.DeepSeekIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(DeepSeekPlugin.Instance.ApiKey);
}