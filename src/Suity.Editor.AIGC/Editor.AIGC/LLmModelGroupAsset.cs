using static Suity.Helpers.GlobalLocalizer;
using System;
using System.Drawing;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents an LLM model group asset that contains model provider information and associated icon.
/// </summary>
public class LLmModelGroupAsset : GroupAsset
{
    internal Image _manufatureIcon;

    /// <summary>
    /// Gets or sets the name of the model manufacturer/provider.
    /// </summary>
    public string Manufacture { get; internal protected set; }

    /// <summary>
    /// Gets the default icon associated with this model group.
    /// </summary>
    public override Image DefaultIcon => _manufatureIcon;
}

/// <summary>
/// Builder class for creating and configuring LLM model group assets.
/// </summary>
public class LLmModelAssetGroupBuilder : GroupAssetBuilder<LLmModelGroupAsset>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LLmModelAssetGroupBuilder"/> class with the specified manufacturer information.
    /// </summary>
    /// <param name="manufacture">The name of the model manufacturer/provider. Cannot be null or whitespace.</param>
    /// <param name="description">An optional description for the model group.</param>
    /// <param name="icon">An optional icon image for the model group.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="manufacture"/> is null or whitespace.</exception>
    public LLmModelAssetGroupBuilder(string manufacture, string description = null, Image icon = null)
    {
        if (string.IsNullOrWhiteSpace(manufacture))
        {
            throw new ArgumentException(L($"'{nameof(manufacture)}' cannot be null or white space."), nameof(manufacture));
        }

        string name = manufacture;
        if (!name.StartsWith("*"))
        {
            name = "*" + manufacture;
        }

        SetLocalName(name);

        if (Asset is null)
        {
            NewAsset();
        }

        Asset.Manufacture = manufacture;
        Asset.Description = description ?? string.Empty;
        Asset._manufatureIcon = icon;

        ResolveId();
    }
}