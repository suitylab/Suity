using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// A specialized collection for <see cref="DFunction"/> assets that combines specific functions with general functions.
/// </summary>
public class DFunctionCollection : AssetCollection<DFunction>
{
    private readonly IAssetCollection<DFunction> _generalFuncs;

    /// <summary>
    /// Creates a new instance of <see cref="DFunctionCollection"/>.
    /// </summary>
    /// <param name="generalFuncs">The general function collection to fall back to.</param>
    public DFunctionCollection(IAssetCollection<DFunction> generalFuncs)
    {
        _generalFuncs = generalFuncs ?? throw new ArgumentNullException(nameof(generalFuncs));
    }

    /// <inheritdoc/>
    public override DFunction GetAsset(string assetKey)
    {
        var asset = base.GetAsset(assetKey);

        return asset ?? _generalFuncs.GetAsset(assetKey);
    }

    /// <inheritdoc/>
    public override DFunction GetAsset(string assetKey, IAssetFilter filter)
    {
        var asset = base.GetAsset(assetKey, filter);

        return asset ?? _generalFuncs.GetAsset(assetKey, filter);
    }

    /// <inheritdoc/>
    public override IEnumerable<DFunction> Assets
    {
        get
        {
            return base.Assets.Concat(_generalFuncs.Assets);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<ISelectionItem> GetItems()
    {
        return base.GetItems().Concat(_generalFuncs.GetItems());
    }

    /// <inheritdoc/>
    public override ISelectionItem GetItem(string key)
    {
        var asset = base.GetItem(key);

        return asset ?? _generalFuncs.GetAsset(key);
    }

    /// <inheritdoc/>
    public override DFunction PrimaryAsset
    {
        get
        {
            var func = base.PrimaryAsset;
            if (func != null)
            {
                return func;
            }

            return _generalFuncs?.PrimaryAsset;
        }
    }
}