using Suity.Drawing;
using Suity.Editor.Properties;
using Suity.Helpers;

namespace Suity.Editor.AIGC.API;

/// <summary>
/// Plugin for MiMo AI provider.
/// </summary>
public class MiMoPlugin : BaseOpenAIPlugin<MiMoLLmModelAsset, MiMoImageGenModelAsset>
{
    /// <summary>
    /// The default API base URL for MiMo.
    /// </summary>
    public const string DEFAULT_URL = "https://api.xiaomimimo.com";

    /// <summary>
    /// Gets the singleton instance of the MiMo plugin.
    /// </summary>
    public static MiMoPlugin Instance { get; private set; }

    /// <summary>
    /// Gets the icon image for MiMo.
    /// </summary>
    public static BitmapDef MiMoIcon { get; } = Resources.mi.ToBitmap();

    /// <summary>
    /// Initializes a new instance of the <see cref="MiMoPlugin"/> class.
    /// </summary>
    public MiMoPlugin()
        : base(DEFAULT_URL, "MiMo", MiMoIcon)
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the official website URL for MiMo.
    /// </summary>
    public override string? OfficialUrl => "https://mimo.mi.com/";
}

internal class MiMoCall : BaseOpenAICall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MiMoCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model asset.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    public MiMoCall(MiMoLLmModelAsset model, LLmModelParameter? config, FunctionContext? context = null)
        : base(MiMoPlugin.Instance, model, config, context)
    {
    }
}

/// <summary>
/// LLM model asset for MiMo provider.
/// </summary>
[NotAvailable]
public class MiMoLLmModelAsset : ThirdPartyLLmModelAsset
{
    public MiMoLLmModelAsset()
    {

    }

    /// <summary>
    /// Gets the default icon for MiMo models.
    /// </summary>
    public override ImageDef DefaultIcon => MiMoPlugin.MiMoIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(MiMoPlugin.Instance.ApiKey);

    /// <summary>
    /// Creates a new call instance for this model.
    /// </summary>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new <see cref="MiMoCall"/> instance.</returns>
    public override ILLmCall CreateCall(LLmModelParameter? config = null, FunctionContext? context = null)
    {
        return new MiMoCall(this, config, context);
    }
}

/// <summary>
/// Image generation model asset for MiMo provider.
/// </summary>
public class MiMoImageGenModelAsset : ThirdPartyImageGenAsset
{
    /// <summary>
    /// Gets the default icon for MiMo image models.
    /// </summary>
    public override ImageDef DefaultIcon => MiMoPlugin.MiMoIcon;

    /// <summary>
    /// Gets a value indicating whether the API key is valid.
    /// </summary>
    public override bool ApiKeyValid => !string.IsNullOrWhiteSpace(MiMoPlugin.Instance.ApiKey);
}