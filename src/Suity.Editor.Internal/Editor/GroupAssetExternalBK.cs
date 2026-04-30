using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Internal backend for <see cref="GroupAsset"/> managing child assets including add, remove, clear,
/// rename, activate, deactivate, attach, and detach operations.
/// </summary>
internal class GroupAssetExternalBK(GroupAsset groupAsset) : GroupAssetExternal
{
    private readonly GroupAsset _asset = groupAsset;

    private UniqueMultiDictionary<string, Asset> _childAssets;

    /// <inheritdoc/>
    public override IEnumerable<Asset> ChildAssets => _childAssets?.Values ?? [];

    /// <inheritdoc/>
    public override IEnumerable<string> ChildAssetLocalNames => _childAssets?.Keys ?? [];

    /// <inheritdoc/>
    public override IEnumerable<Asset> GetChildAssets(IAssetFilter filter)
    {
        if (filter != null)
        {
            return _childAssets?.Values.Where(filter.FilterAsset) ?? [];
        }
        else
        {
            return _childAssets?.Values ?? [];
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetChildAssetLocalNames(IAssetFilter filter)
    {
        if (filter != null)
        {
            return _childAssets?.Keys.Where(key => _childAssets[key].Any(filter.FilterAsset)) ?? [];
        }
        else
        {
            return _childAssets?.Keys.Where(key => _childAssets[key].Any()) ?? [];
        }
    }

    /// <inheritdoc/>
    public override int ChildAssetCount => _childAssets?.Count ?? 0;

    /// <inheritdoc/>
    public override Asset GetChildAsset(string localName)
    {
        return _childAssets?[localName].FirstOrDefault();
    }

    /// <inheritdoc/>
    public override Asset GetChildAsset(string localName, IAssetFilter filter)
    {
        if (filter != null)
        {
            return _childAssets?[localName].FirstOrDefault(filter.FilterAsset);
        }
        else
        {
            return _childAssets?[localName].FirstOrDefault();
        }
    }

    /// <inheritdoc/>
    public override bool AddOrUpdateChildAsset(Asset asset, IdResolveType resolveType = IdResolveType.Auto)
    {
        if (asset is null)
        {
            throw new ArgumentNullException(nameof(asset));
        }

        if (asset.ParentAsset == _asset)
        {
            if (_asset.Entry != null)
            {
                if (asset.ResolveId())
                {
                    _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.ElementUpdate));
                }
            }

            return false;
        }

        if (_asset.ContainsParent(asset))
        {
            throw new InvalidOperationException("Cyclic parent.");
        }

        var childAssets = _childAssets ??= new();

        if (childAssets.Add(asset.LocalName, asset))
        {
            asset.ParentAsset = _asset;
            if (_asset.Entry != null)
            {
                asset.ResolveId(resolveType);
            }
            asset.ObjectUpdated += Asset_ObjectUpdated;

            _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.Add));

            return true;
        }
        else
        {
            // Will not enter
            _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.ElementUpdate));

            return false;
        }
    }

    /// <inheritdoc/>
    public override bool RemoveChildAsset(Asset asset)
    {
        if (asset is null)
        {
            return false;
        }

        bool removed = _childAssets?.Remove(asset.LocalName, asset) ?? false;
        if (removed)
        {
            asset.ObjectUpdated -= Asset_ObjectUpdated; // Priority
            // Set Entry to null first, then set ParentAsset to null, otherwise it will cause Id re-resolution
            asset.Entry = null;
            asset.ParentAsset = null;
            _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.Remove));
        }

        return removed;
    }

    /// <summary>
    /// Removes all child assets from this group, clearing their parent references and entries.
    /// </summary>
    public override void ClearChildAssets()
    {
        if (_childAssets is null)
        {
            return;
        }

        var assets = _childAssets.Values.ToArray();
        _childAssets.Clear();

        foreach (var asset in assets)
        {
            asset.ObjectUpdated -= Asset_ObjectUpdated; // Priority
            // Set Entry to null first, then set ParentAsset to null, otherwise it will cause Id re-resolution
            asset.Entry = null;
            asset.ParentAsset = null;
            _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.Remove));
        }
    }

    /// <inheritdoc/>
    public override bool ChangeChildName(Asset childAsset, string oldName)
    {
        if (childAsset is null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(oldName))
        {
            return false;
        }

        var childAssets = _childAssets ??= new();

        if (childAssets.Remove(oldName, childAsset))
        {
            childAssets.Add(childAsset.LocalName, childAsset);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override void ActivateChildAssets()
    {
        if (_childAssets is null)
        {
            return;
        }

        foreach (var childAsset in _childAssets.Values)
        {
            childAsset._ex.UpdateAssetKey();
            childAsset._ex.UpdateResourceName();
        }
    }

    /// <inheritdoc/>
    public override void DeactivateChildAssets()
    {
        if (_childAssets is null)
        {
            return;
        }

        foreach (var childAsset in _childAssets.Values)
        {
            childAsset._ex.UpdateAssetKey();
            childAsset._ex.UpdateResourceName();
        }
    }

    /// <summary>
    /// Attaches all child assets by resolving their IDs.
    /// </summary>
    public override void AttachChildAssets()
    {
        if (_childAssets is null)
        {
            return;
        }

        foreach (var childAsset in _childAssets.Values)
        {
            childAsset.ResolveId();
        }
    }

    /// <inheritdoc/>
    public override void DetachChildAssets()
    {
        if (_childAssets is null)
        {
            return;
        }

        // Add ToArray, setting to null midway will cause collection modified error
        foreach (var childAsset in _childAssets.Values.ToArray())
        {
            childAsset.Entry = null;
        }
    }

    private void Asset_ObjectUpdated(object sender, EntryEventArgs e)
    {
        Asset asset = (Asset)sender;
        _asset.NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.ElementUpdate, e));
    }
}

/// <summary>
/// Internal backend for <see cref="GroupAssetBuilder{TGroupAsset}"/> managing child asset builders
/// including add, remove, clear, detach, and product update operations.
/// </summary>
/// <typeparam name="TGroupAsset">The type of group asset this builder manages.</typeparam>
internal class GroupAssetBuilderExternalBK<TGroupAsset> : GroupAssetBuilderExternal<TGroupAsset>
    where TGroupAsset : GroupAsset, new()
{
    private readonly GroupAssetBuilder<TGroupAsset> _groupBuilder;
    private readonly object _syncRoot = new();

    private HashSet<AssetBuilder> _childBuilders;

    /// <summary>
    /// Ensures the child builders collection is initialized and returns it.
    /// </summary>
    /// <returns>The initialized set of child builders.</returns>
    internal HashSet<AssetBuilder> EnsureChildBuilders()
    {
        lock (_syncRoot)
        {
            return _childBuilders ??= [];
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="GroupAssetBuilderExternalBK{TGroupAsset}"/>.
    /// </summary>
    /// <param name="groupBuilder">The group builder this backend manages.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="groupBuilder"/> is null.</exception>
    public GroupAssetBuilderExternalBK(GroupAssetBuilder<TGroupAsset> groupBuilder)
    {
        _groupBuilder = groupBuilder ?? throw new ArgumentNullException(nameof(groupBuilder));
    }

    /// <inheritdoc/>
    public override void AddOrUpdate(AssetBuilder builder, IdResolveType resolveType)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (builder.Parent != null && builder.Parent != _groupBuilder)
        {
            builder.Parent.Remove(builder);
        }

        bool added = EnsureChildBuilders().Add(builder);
        builder.Parent = _groupBuilder;

        if (_groupBuilder.Asset != null)
        {
            builder.WithAsset();
        }

        if (added)
        {
            builder.AssetChanged += Builder_ProductReplaced;
        }

        _groupBuilder.TryUpdateNow(o =>
        {
            var childAsset = builder.TargetAsset;
            if (childAsset != null)
            {
                o.AddOrUpdateChildAsset(childAsset, resolveType);
            }
        });
    }

    /// <inheritdoc/>
    public override bool Remove(AssetBuilder builder)
    {
        if (builder is null)
        {
            return false;
        }

        if (_groupBuilder is null)
        {
            return false;
        }

        lock (_syncRoot)
        {
            if (_childBuilders is null || !_childBuilders.Remove(builder))
            {
                return false;
            }
        }

        builder.AssetChanged -= Builder_ProductReplaced;
        builder.Parent = null;

        _groupBuilder.TryUpdateNow(o =>
        {
            var childAsset = builder.TargetAsset;
            if (childAsset != null)
            {
                _groupBuilder.Asset.RemoveChildAsset(childAsset);
                childAsset.Entry = null;
            }
        });

        return true;
    }

    /// <inheritdoc/>
    public override void UpdateProduct(TGroupAsset groupAsset)
    {
        foreach (var childBuilder in ChildBuilders)
        {
            if (childBuilder.TargetAsset != null)
            {
                // Builder has its own asset, update
                groupAsset.AddOrUpdateChildAsset(childBuilder.TargetAsset);
            }
            else
            {
                // Builder has no asset, query existing child assets
                var current = groupAsset.GetChildAsset(childBuilder.LocalName);
                if (current != null)
                {
                    // Has child asset, try to attach
                    if (!childBuilder.AttachAsset(current))
                    {
                        // Attachment failed, remove current asset and recreate
                        groupAsset.RemoveChildAsset(current);
                        childBuilder.NewAsset();
                        groupAsset.AddOrUpdateChildAsset(childBuilder.TargetAsset);
                    }
                }
                else
                {
                    // No child asset, create new one
                    childBuilder.NewAsset();
                    groupAsset.AddOrUpdateChildAsset(childBuilder.TargetAsset);
                }
            }
        }

        // Remove extra assets
        List<Asset> removes = null;
        foreach (var childAsset in groupAsset.ChildAssets)
        {
            if (childAsset.Builder?.Parent != _groupBuilder)
            {
                (removes ??= []).Add(childAsset);
            }
        }

        if (removes != null)
        {
            foreach (var remove in removes)
            {
                remove.SetParent(null);
                remove.Builder?.DetachAsset(true);
                remove.Entry = null;
            }
        }
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        foreach (var childBuilder in ChildBuilders.ToArray())
        {
            Remove(childBuilder);
        }
    }

    /// <inheritdoc/>
    public override void DetachChildProducts(bool detachId)
    {
        foreach (var childBuilder in ChildBuilders)
        {
            childBuilder.DetachAsset(detachId);
        }
    }

    /// <inheritdoc/>
    public override AssetBuilder[] ChildBuilders
    {
        get
        {
            lock (_syncRoot)
            {
                return _childBuilders?.ToArray() ?? [];
            }
        }
    }

    /// <inheritdoc/>
    public override int ChildCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _childBuilders?.Count ?? 0;
            }
        }
    }

    private void Builder_ProductReplaced(object sender, ReplaceEntryEventArgs e)
    {
        _groupBuilder.TryUpdateNow(o =>
        {
            if (e.OldObject is Asset oldAsset)
            {
                _groupBuilder.Asset.RemoveChildAsset(oldAsset);
            }

            if (e.NewObject is Asset newAsset)
            {
                _groupBuilder.Asset.AddOrUpdateChildAsset(newAsset);
            }

            // Cancel replacement event
            // Product.NotifyUpdated(new GroupAssetEventArgs(newAsset, EntryUpdateTypes.ElementUpdate, e));
        });
    }
}