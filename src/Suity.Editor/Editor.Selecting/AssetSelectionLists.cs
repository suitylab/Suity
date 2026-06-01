using Suity.Editor.Types;
using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Selecting;

/// <summary>
/// A selection list that provides asset type items.
/// </summary>
public class AssetTypeSelectionList : ISelectionList
{
    /// <summary>
    /// Gets the singleton instance of AssetTypeSelectionList.
    /// </summary>
    public static AssetTypeSelectionList Instance { get; } = new AssetTypeSelectionList();

    #region ISelectionList Members

    /// <inheritdoc />
    public IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DAssetLink>();
        if (collection != null)
        {
            return collection.Assets;
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc />
    public ISelectionItem GetItem(string key)
    {
        return AssetManager.Instance.GetAsset<DAssetLink>(key);
    }

    #endregion
}

/// <summary>
/// A selection list for assets of type TAsset.
/// </summary>
/// <typeparam name="TAsset">The type of asset.</typeparam>
public class AssetSelectionList<TAsset> : ISelectionList
    where TAsset : Asset
{
    private readonly IGeneralAssetCollection _generalCollection;
    private readonly IAssetCollection<TAsset> _collection;
    private readonly IAssetFilter _filter;

    /// <summary>
    /// Initializes a new instance of the AssetSelectionList class.
    /// </summary>
    public AssetSelectionList()
    {
        _generalCollection = AssetManager.Instance.GetAssetCollection<TAsset>();
    }

    /// <summary>
    /// Initializes a new instance of the AssetSelectionList class with the specified collection and filter.
    /// </summary>
    /// <param name="collection">The asset collection.</param>
    /// <param name="filter">The asset filter.</param>
    public AssetSelectionList(IAssetCollection<TAsset> collection, IAssetFilter filter = null)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        _filter = filter ?? AssetFilters.Default;
    }

    /// <inheritdoc />
    public ISelectionItem GetItem(string key)
    {
        if (_collection != null)
        {
            return _collection.GetAsset(key, _filter);
        }
        else if (_generalCollection != null)
        {
            return _generalCollection.GetAsset(key, _filter);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<ISelectionItem> GetItems()
    {
        if (_collection != null)
        {
            return _collection.Assets
           .Where(o => _filter?.FilterAsset(o) ?? true)
           .OrderBy(o => o.AssetKey);
        }
        else if (_generalCollection != null)
        {
            return _generalCollection.Assets
           .Where(o => _filter?.FilterAsset(o) ?? true)
           .OrderBy(o => o.AssetKey);
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the asset filter.
    /// </summary>
    public IAssetFilter Filter => _filter;
}

/// <summary>
/// A grouped selection list for assets that organizes assets by their root asset.
/// </summary>
/// <typeparam name="TAsset">The type of asset.</typeparam>
public class GroupedAssetSelectionList<TAsset> : ISelectionList
    where TAsset : Asset
{
    private readonly IAssetCollection<TAsset> _collection;
    private readonly IAssetFilter _filter;

    private readonly List<ISelectionItem> _items = [];

    /// <summary>
    /// Initializes a new instance of the GroupedAssetSelectionList class.
    /// </summary>
    /// <param name="collection">The asset collection.</param>
    /// <param name="contentTypeName">The content type name.</param>
    /// <param name="filter">The asset filter.</param>
    public GroupedAssetSelectionList(IAssetCollection<TAsset> collection, string contentTypeName = null, IAssetFilter filter = null)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        _filter = filter ?? AssetFilters.Default;

        CollectContentTypeName(contentTypeName);
    }

    private void CollectContentTypeName(string contentTypeName)
    {
        HashSet<string> added = [];

        // Get the AssetTypeName corresponding to this type
        string assetTypeName = contentTypeName ?? typeof(TAsset).ResolveAssetTypeName();

        // Handle groups
        var parentGroups = _collection.Assets
            .Where(o => o.FileName is null && _filter.FilterAsset(o))
            .GroupBy(o => o.GetRootAsset());

        // Has Key
        foreach (var parentGroup in parentGroups.Where(o => o.Key != null))
        {
            if (!added.Add(parentGroup.Key?.AssetKey))
            {
                continue;
            }

            // If the parent resource of this resource is also this type, set the parent resource as also selectable.
            bool selectable = parentGroup.Key.AssetTypeNames.Contains(assetTypeName);

            var selGroup = new ParentAssetSelectionGroup(parentGroup, selectable);
            _items.Add(selGroup);
        }

        // No parent exists, cannot Group, otherwise it will be filtered out.
        if (parentGroups.FirstOrDefault(o => o.Key is null) is { } independentGroup)
        {
            var categoryGroups = independentGroup.GroupBy(o => (o as IHasCategory)?.Category);

            foreach (var categoryGroup in categoryGroups.Where(o => o.Key != null))
            {
                string categoryName = $"[{categoryGroup.Key}]";

                if (!added.Add(categoryName))
                {
                    continue;
                }

                var selGroup = new CategorySelectionGroup(categoryName, categoryGroup);
                _items.Add(selGroup);
            }

            if (categoryGroups.FirstOrDefault(o => o.Key is null) is { } noCategoryGroup)
            {
                foreach (var item in noCategoryGroup)
                {
                    if (!added.Add(item.AssetKey))
                    {
                        continue;
                    }

                    _items.Add(item);
                }
            }
        }

        var assets = _collection.Assets
            .Where(o => o.FileName is not null && _filter.FilterAsset(o));

        // Handle standalone files
        foreach (var asset in assets)
        {
            if (!added.Add(asset.AssetKey))
            {
                continue;
            }

            _items.Add(asset);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ISelectionItem> GetItems()
    {
        return _items;
    }

    /// <inheritdoc />
    public ISelectionItem GetItem(string key)
    {
        return _items.FirstOrDefault(o => o.SelectionKey == key);
    }
}