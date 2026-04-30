using System;

namespace Suity.Editor.Types;

/// <summary>
/// Filters assets based on their abstract type.
/// </summary>
public class AbstractTypeFilter : IAssetFilter
{
    private readonly string _abstractType;

    /// <summary>
    /// Initializes a new instance of the AbstractTypeFilter class.
    /// </summary>
    public AbstractTypeFilter(string abstractType)
    {
        if (string.IsNullOrWhiteSpace(abstractType))
        {
            throw new ArgumentNullException(nameof(abstractType));
        }

        _abstractType = abstractType;
    }

    /// <inheritdoc />
    public bool FilterAsset(Asset asset)
    {
        if (!AssetFilters.Default.FilterAsset(asset))
        {
            return false;
        }

        if (asset is not DStruct s)
        {
            return false;
        }

        return s.BaseType?.AssetKey == _abstractType;
    }
}