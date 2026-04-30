using Suity.Editor.Properties;
using Suity.Helpers;
using System.Drawing;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for LM Studio provider (local AI model server).
/// </summary>
public class LmStudioPlugin : BaseOpenAIPlugin<LmStudioLLmModelAsset, LmStudioImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for LM Studio (local server).
    /// </summary>
    public const string DEFAULT_URL = "http://127.0.0.1:1234";

    /// <summary>
    /// Gets the singleton instance of the LM Studio plugin.
    /// </summary>
    public static LmStudioPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for LM Studio.
    /// </summary>
    public static Bitmap LmStudioIcon { get; } = Resources.LmStudio.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="LmStudioPlugin"/> class.
    /// </summary>
    public LmStudioPlugin()
        : base(DEFAULT_URL, "LmStudio", LmStudioIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for LM Studio.
    /// </summary>
    public override string? OfficialUrl => "https://lmstudio.ai";
}

internal class LmStudioCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LmStudioCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public LmStudioCall(LmStudioLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(LmStudioPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for LM Studio provider.
/// </summary>
public class LmStudioLLmModelAsset : ThirdPartyLLmModelAsset
{
    /// <summary>
    /// Gets the default icon for LM Studio models.
    /// </summary>
    public override Image DefaultIcon => LmStudioPlugin.LmStudioIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(LmStudioPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="LmStudioCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new LmStudioCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for LM Studio provider.
/// </summary>
public class LmStudioImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for LM Studio image models.
    /// </summary>
    public override Image DefaultIcon => LmStudioPlugin.LmStudioIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(LmStudioPlugin.Instance.ApiKey);
}