using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents an asset link type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Asset, "Asset Link")]
public class DAssetLink : DType
{
    Image _icon;

    /// <summary>
    /// Initializes a new instance of the DAssetLink class.
    /// </summary>
    public DAssetLink(Type assetType)
    {
        AssetType = assetType ?? throw new ArgumentNullException(nameof(assetType));

        UpdateStyle(assetType);
    }

    /// <summary>
    /// Initializes a new instance of the DAssetLink class with a name.
    /// </summary>
    public DAssetLink(Type assetType, string name)
        : base(name)
    {
        AssetType = assetType ?? throw new ArgumentNullException(nameof(assetType));

        UpdateStyle(assetType);
    }

    /// <summary>
    /// Initializes a new instance of the DAssetLink class with a name and description.
    /// </summary>
    public DAssetLink(Type assetType, string name, string description)
        : base(name)
    {
        AssetType = assetType ?? throw new ArgumentNullException(nameof(assetType));
        Description = description;

        UpdateStyle(assetType);
    }

    /// <summary>
    /// Gets the asset type.
    /// </summary>
    public Type AssetType { get; }

    /// <inheritdoc />
    public override Type NativeType => AssetType;

    /// <summary>
    /// Gets the asset link type name.
    /// </summary>
    public string AssetLinkTypeName => LocalName;

    /// <inheritdoc />
    public override Color? TypeColor => this.ViewColor;

    /// <inheritdoc />
    public override Image GetIcon() => _icon ?? base.GetIcon();

    private void UpdateStyle(Type type)
    {
        string name = type.Name;


        if (type.GetAttributeCached<NativeTypeAttribute>() is { } attr)
        {
            Description ??= attr.Description ?? EditorUtility.ToDisplayText(type);
            if (ColorHelper.TryParseHtmlColor(attr.Color, out var color))
            {
                ViewColor = color;
            }
            else
            {
                ViewColor = EditorUtility.ToNativeColor(type);
            }
        }
        else
        {
            Description ??= EditorUtility.ToDisplayText(type);
            ViewColor = EditorUtility.ToNativeColor(type);
        }

        _icon = type.ToDisplayIcon();
    }

    
}