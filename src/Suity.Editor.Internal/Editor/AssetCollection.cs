using Suity.Selecting;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Asset collection that manages primary asset tracking and selection list support.
/// </summary>
/// <typeparam name="TAsset">The type of asset, which must derive from <see cref="Asset"/>.</typeparam>
public class AssetCollection<TAsset> : IAssetCollection<TAsset>, ISelectionList
    where TAsset : Asset
{
    private readonly HashSet<TAsset> _primaryAssets = [];
    private readonly MultipleItemCollection<string, TAsset> _collection = new(/*IgnoreCaseStringComparer.Instance*/);

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetCollection{TAsset}"/> class.
    /// </summary>
    internal AssetCollection()
    {
        _collection.ValueAdded += obj =>
        {
            if (obj.IsPrimary)
            {
                _primaryAssets.Add(obj);
            }
        };

        _collection.ValueRemoved += obj =>
        {
            _primaryAssets.Remove(obj);
        };

        _collection.ValueUpdated += obj =>
        {
            if (obj.IsPrimary)
            {
                _primaryAssets.Add(obj);
            }
            else
            {
                _primaryAssets.Remove(obj);
            }
        };
    }

    /// <summary>
    /// Adds an asset to the collection with the specified key.
    /// </summary>
    /// <param name="assetKey">The key to associate with the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <returns>The multiple item entry for the added asset.</returns>
    internal MultipleItem<string, TAsset> AddAsset(string assetKey, TAsset asset)
    {
        lock (_collection)
        {
            return _collection.AddValue(assetKey, asset);
        }
    }

    /// <summary>
    /// Gets the asset associated with the specified key.
    /// </summary>
    /// <param name="assetKey">The key to look up.</param>
    /// <returns>The asset, or null if not found.</returns>
    public virtual TAsset GetAsset(string assetKey)
    {
        lock (_collection)
        {
            return _collection.GetValue(assetKey);
        }
    }

    /// <summary>
    /// Gets the asset associated with the specified key that passes the given filter.
    /// </summary>
    /// <param name="assetKey">The key to look up.</param>
    /// <param name="filter">The filter to apply, or null to skip filtering.</param>
    /// <returns>The filtered asset, or null if not found or filtered out.</returns>
    public virtual TAsset GetAsset(string assetKey, IAssetFilter filter)
    {
        lock (_collection)
        {
            TAsset asset = _collection.GetValue(assetKey);

            if (asset != null && (filter?.FilterAsset(asset) ?? true))
            {
                return asset;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the first primary asset in the collection.
    /// </summary>
    public virtual TAsset PrimaryAsset => _primaryAssets.FirstOrDefault();

    /// <summary>
    /// Gets the first primary asset that passes the given filter.
    /// </summary>
    /// <param name="filter">The filter to apply, or null to skip filtering.</param>
    /// <returns>The filtered primary asset, or null if not found or filtered out.</returns>
    public virtual TAsset GetPrimaryAsset(IAssetFilter filter)
    {
        TAsset asset = PrimaryAsset;

        if (asset != null && (filter?.FilterAsset(asset) ?? true))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all assets in the collection.
    /// </summary>
    public virtual IEnumerable<TAsset> Assets => _collection._assets.Values.Select(o => o.Value);

    /// <summary>
    /// Gets the total number of assets in the collection.
    /// </summary>
    public virtual int Count => _collection.Count;

    #region ISelectionList

    /// <summary>
    /// Gets all items available for selection.
    /// </summary>
    public virtual IEnumerable<ISelectionItem> GetItems()
    {
        lock (_collection)
        {
            // lock is ineffective for IEnumerable<INavigationItem>
            return _collection._assets.Values.Select(o => o.Value);
        }
    }

    /// <summary>
    /// Gets the selection item with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The selection item, or null if not found.</returns>
    public virtual ISelectionItem GetItem(string key)
    {
        lock (_collection)
        {
            return _collection.GetValue(key);
        }
    }

    #endregion
}

/// <summary>
/// A general-purpose asset collection that uses the base <see cref="Asset"/> type.
/// </summary>
public class GeneralAssetCollection : AssetCollection<Asset>, IGeneralAssetCollection
{
}