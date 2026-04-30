using System;

namespace Suity;

/// <summary>
/// Maps parsed types to resource types.
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class AssetDefinitionTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the asset type name.
    /// </summary>
    public string AssetTypeName { get; }

    /// <summary>
    /// Initializes a new instance of the AssetDefinitionTypeAttribute class.
    /// </summary>
    /// <param name="assetTypeName">The asset type name.</param>
    public AssetDefinitionTypeAttribute(string assetTypeName)
    {
        AssetTypeName = assetTypeName;
    }
}