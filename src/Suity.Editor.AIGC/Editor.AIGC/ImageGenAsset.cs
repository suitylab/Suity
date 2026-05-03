using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using System.Drawing;
using Suity.Drawing;

namespace Suity.Editor.AIGC;

/// <summary>
/// Abstract base class for AI image generation assets that provides model configuration and call creation.
/// </summary>
[NativeType(Name = "ImageGenAsset", Description = "AI Image Generation Model", CodeBase = "*AIGC", Icon = "*CoreIcon|Image")]
public abstract class ImageGenAsset : StandaloneAsset<IImageGenModel>, IImageGenModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenAsset"/> class with default inactive state.
    /// </summary>
    protected ImageGenAsset()
        : base(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenAsset"/> class with the specified name and active state.
    /// </summary>
    /// <param name="name">The display name of the asset.</param>
    /// <param name="active">Whether the asset is active by default.</param>
    internal ImageGenAsset(string name, bool active = true)
        : base(name, active)
    {
    }

    /// <summary>
    /// Gets the default icon for this image generation asset.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.Image;

    /// <summary>
    /// Gets or sets the unique identifier for the AI model used for image generation.
    /// </summary>
    public string ModelId { get; internal protected set; }

    /// <summary>
    /// Gets a value indicating whether the API key is valid for this model.
    /// </summary>
    public virtual bool ApiKeyValid => true;

    /// <summary>
    /// Gets the display text for this asset, using the description or model ID as fallback.
    /// </summary>
    public override string DisplayText
    {
        get
        {
            var text = this.Description;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = ModelId ?? string.Empty;
            }

            return text;
        }
    }

    /// <summary>
    /// Gets the display status indicating whether the API key is valid.
    /// </summary>
    public override TextStatus DisplayStatus => ApiKeyValid ? TextStatus.Normal : TextStatus.Disabled;



    #region IImageGenModel

    /// <summary>
    /// Creates a new image generation call with the specified function context.
    /// </summary>
    /// <param name="context">The context for the function call.</param>
    /// <returns>A new <see cref="IImageGenCall"/> instance, or null if not supported.</returns>
    public virtual IImageGenCall CreateCall(FunctionContext context = null) => null;

    #endregion
}

/// <summary>
/// Abstract base class for third-party AI image generation assets.
/// </summary>
public abstract class ThirdPartyImageGenAsset : ImageGenAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyImageGenAsset"/> class with default settings.
    /// </summary>
    protected ThirdPartyImageGenAsset()
         : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyImageGenAsset"/> class with the specified name and active state.
    /// </summary>
    /// <param name="name">The display name of the asset.</param>
    /// <param name="active">Whether the asset is active by default.</param>
    protected ThirdPartyImageGenAsset(string name, bool active = true)
        : base(name, active) { }
}

/// <summary>
/// Builder class for creating and configuring <see cref="ImageGenAsset"/> instances.
/// </summary>
/// <typeparam name="T">The type of image generation asset to build.</typeparam>
public class ImageGenAssetBuilder<T> : AssetBuilder<T>
    where T : ImageGenAsset, new()
{
    /// <summary>
    /// Gets the model identifier for the image generation asset.
    /// </summary>
    public string ModelId { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGenAssetBuilder{T}"/> class.
    /// </summary>
    public ImageGenAssetBuilder()
    {
        AddAutoUpdate(nameof(ModelId), o => o.ModelId = ModelId);
    }

    /// <summary>
    /// Sets the model identifier for the image generation asset.
    /// </summary>
    /// <param name="modelId">The unique identifier for the AI model.</param>
    public void SetModelId(string modelId)
    {
        if (ModelId == modelId) { return; }

        ModelId = modelId;

        TryUpdateNow(o => o.ModelId = ModelId);

        SetLocalName(modelId);
    }
}