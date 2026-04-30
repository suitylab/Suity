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
        var groups = _collection.Assets
            .Where(o => o.FileName is null && _filter.FilterAsset(o))
            .GroupBy(o => o.GetRootAsset());

        // Has Key
        foreach (var group in groups.Where(o => o.Key != null))
        {
            if (!added.Add(group.Key?.AssetKey))
            {
                continue;
            }

            // If the parent resource of this resource is also this type, set the parent resource as also selectable.
            bool selectable = group.Key.AssetTypeNames.Contains(assetTypeName);

            var node = new GroupNode(group, selectable);
            _items.Add(node);
        }


        // No Key exists, cannot Group, otherwise it will be filtered out.
        foreach (var group in groups.Where(o => o.Key is null))
        {
            foreach (var item in group)
            {
                if (!added.Add(item.AssetKey))
                {
                    continue;
                }

                _items.Add(item);
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

    /// <summary>
    /// Represents a group node in the grouped asset selection list.
    /// </summary>
    private class GroupNode : BaseSelectionNode
    {
        private readonly IGrouping<Asset, Asset> _group;
        private readonly bool _selectable;

        /// <summary>
        /// Initializes a new instance of the GroupNode class.
        /// </summary>
        /// <param name="group">The asset grouping.</param>
        /// <param name="selectable">Whether the group is selectable.</param>
        public GroupNode(IGrouping<Asset, Asset> group, bool selectable = false)
        {
            _group = group ?? throw new ArgumentNullException(nameof(group));
            _selectable = selectable;
        }

        /// <inheritdoc />
        public override string SelectionKey => _group.Key?.AssetKey;

        /// <inheritdoc />
        public override string Name => _group.Key?.Name;

        /// <inheritdoc />
        public override string DisplayText => _group.Key?.ToDisplayText() ?? "(Other)";

        /// <inheritdoc />
        public override object DisplayIcon => _group.Key?.Icon;

        /// <inheritdoc />
        public override TextStatus DisplayStatus => _group.Key?.DisplayStatus ?? TextStatus.Normal;

        /// <inheritdoc />
        public override bool Selectable => _selectable;

        /// <inheritdoc />
        public override IEnumerable<ISelectionItem> GetItems()
        {
            return _group
                .OrderBy(o => o.AssetKey);
        }

        /// <inheritdoc />
        public override ISelectionItem GetItem(string key)
        {
            return _group.FirstOrDefault(o => o.AssetKey == key);
        }
    }
}