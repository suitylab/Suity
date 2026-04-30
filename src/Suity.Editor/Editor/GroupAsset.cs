using Suity.Editor.CodeRender;
using Suity.Selecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Group asset
/// </summary>
public class GroupAsset : Asset,
    IIdCluster
{
    private readonly GroupAssetExternal _groupEx;

    private GroupAssetSelectionNode _selectionNode;

    public GroupAsset()
    {
        _groupEx = AssetManager.Instance.CreateGroupAssetExternal(this);
    }

    public GroupAsset(string name)
        : base(name)
    {
        _groupEx = AssetManager.Instance.CreateGroupAssetExternal(this);
    }

    /// <summary>
    /// Gets the child assets of this group asset.
    /// </summary>
    public IEnumerable<Asset> ChildAssets => _groupEx.ChildAssets;

    /// <summary>
    /// Gets the local names of child assets.
    /// </summary>
    public IEnumerable<string> ChildAssetLocalNames => _groupEx.ChildAssetLocalNames;

    /// <summary>
    /// Gets child assets that match the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>Filtered child assets.</returns>
    public IEnumerable<Asset> GetChildAssets(IAssetFilter filter) => _groupEx.GetChildAssets(filter);

    /// <summary>
    /// Gets local names of child assets that match the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>Filtered child asset local names.</returns>
    public IEnumerable<string> GetChildAssetLocalNames(IAssetFilter filter) => _groupEx.GetChildAssetLocalNames(filter);

    /// <summary>
    /// Gets the count of child assets.
    /// </summary>
    public int ChildAssetCount => _groupEx.ChildAssetCount;

    /// <summary>
    /// Gets a child asset by its local name.
    /// </summary>
    /// <param name="localName">The local name of the child asset.</param>
    /// <returns>The child asset, or null if not found.</returns>
    public Asset GetChildAsset(string localName) => _groupEx.GetChildAsset(localName);

    /// <summary>
    /// Gets a child asset by its local name that also matches the filter.
    /// </summary>
    /// <param name="localName">The local name of the child asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The child asset, or null if not found.</returns>
    public Asset GetChildAsset(string localName, IAssetFilter filter) => _groupEx.GetChildAsset(localName, filter);

    /// <summary>
    /// Gets the selection node for this group asset.
    /// </summary>
    /// <returns>The group asset selection node.</returns>
    public BaseSelectionNode GetGroupSelectionNode() => _selectionNode ??= new(this);

    public override object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.ChildNodes => _groupEx.ChildAssets,
        CodeRenderProperty.ChildNode => GetChildAsset(argument as string),
        _ => base.GetProperty(property, argument),
    };

    internal override void InternalOnAssetActivate(string assetKey)
    {
        base.InternalOnAssetActivate(assetKey);

        _groupEx.ActivateChildAssets();
    }

    internal override void InternalOnAssetDeactivate(string assetKey)
    {
        base.InternalOnAssetDeactivate(assetKey);

        _groupEx.DeactivateChildAssets();
    }

    /// <summary>
    /// Adds or updates a child asset in this group.
    /// </summary>
    /// <param name="asset">The asset to add or update.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    /// <returns>True if the asset was added or updated; otherwise, false.</returns>
    protected internal bool AddOrUpdateChildAsset(Asset asset, IdResolveType resolveType = IdResolveType.Auto)
        => _groupEx.AddOrUpdateChildAsset(asset, resolveType);

    /// <summary>
    /// Removes a child asset from this group.
    /// </summary>
    /// <param name="asset">The asset to remove.</param>
    /// <returns>True if the asset was removed; otherwise, false.</returns>
    protected internal bool RemoveChildAsset(Asset asset)
        => _groupEx.RemoveChildAsset(asset);

    /// <summary>
    /// Clears all child assets from this group.
    /// </summary>
    protected internal void ClearChildAssets()
        => _groupEx.ClearChildAssets();

    internal override void InternalOnEntryAttached(Guid id)
    {
        base.InternalOnEntryAttached(id);

        _groupEx.AttachChildAssets();
    }

    internal override void InternalOnEntryDetached(Guid id)
    {
        base.InternalOnEntryDetached(id);

        _groupEx.DetachChildAssets();
    }

    protected internal override void OnParentChanged()
    {
        base.OnParentChanged();

        _groupEx.ActivateChildAssets();
    }

    /// <summary>
    /// Called when the resource name is updated.
    /// </summary>
    protected internal override void OnResourceNameUpdated()
    {
        base.OnResourceNameUpdated();

        foreach (var childAsset in _groupEx.ChildAssets)
        {
            childAsset._ex.UpdateResourceName();
        }
    }

    /// <summary>
    /// Changes the name of a child asset.
    /// </summary>
    /// <param name="childAsset">The child asset to rename.</param>
    /// <param name="oldName">The old local name.</param>
    /// <returns>True if the name was changed; otherwise, false.</returns>
    protected internal bool ChangeChildName(Asset childAsset, string oldName)
        => _groupEx.ChangeChildName(childAsset, oldName);

    private void Asset_ObjectUpdated(object sender, EntryEventArgs e)
    {
        Asset asset = (Asset)sender;
        NotifyUpdated(new GroupAssetEventArgs(asset, EntryUpdateTypes.ElementUpdate, e));
    }

    IEnumerable<Guid> IIdCluster.Ids => _groupEx.ChildAssets.Select(o => o.Id);
}

/// <summary>
/// Interface for building group assets.
/// </summary>
public interface IGroupAssetBuilder
{
    /// <summary>
    /// Adds or updates a builder to the group.
    /// </summary>
    /// <param name="builder">The asset builder to add or update.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    void AddOrUpdate(AssetBuilder builder, IdResolveType resolveType = IdResolveType.Auto);

    /// <summary>
    /// Removes a builder from the group.
    /// </summary>
    /// <param name="builder">The asset builder to remove.</param>
    /// <returns>True if the builder was removed; otherwise, false.</returns>
    bool Remove(AssetBuilder builder);

    /// <summary>
    /// Clears all builders from the group.
    /// </summary>
    void Clear();
}

/// <summary>
/// Builder for group assets.
/// </summary>
public class GroupAssetBuilder<TGroupAsset> : AssetBuilder<TGroupAsset>, IGroupAssetBuilder
    where TGroupAsset : GroupAsset, new()
{
    private readonly GroupAssetBuilderExternal<TGroupAsset> _groupEx;

    public GroupAssetBuilder()
    {
        _groupEx = AssetManager.Instance.CreateGroupBuilderExternal(this);
    }

    /// <summary>
    /// Adds or updates a builder to the group.
    /// </summary>
    /// <param name="builder">The asset builder to add or update.</param>
    /// <param name="resolveType">The ID resolve type.</param>
    public void AddOrUpdate(AssetBuilder builder, IdResolveType resolveType = IdResolveType.Auto)
        => _groupEx.AddOrUpdate(builder, resolveType);

    /// <summary>
    /// Removes a builder from the group.
    /// </summary>
    /// <param name="builder">The asset builder to remove.</param>
    /// <returns>True if the builder was removed; otherwise, false.</returns>
    public bool Remove(AssetBuilder builder)
        => _groupEx.Remove(builder);

    /// <summary>
    /// Clears all builders from the group.
    /// </summary>
    public void Clear() => _groupEx.Clear();

    /// <summary>
    /// Gets the child builders of this group builder.
    /// </summary>
    public IEnumerable<AssetBuilder> ChildBuilders => _groupEx.ChildBuilders;

    /// <summary>
    /// Gets the count of child builders.
    /// </summary>
    public int ChildCount => _groupEx.ChildCount;

    internal override void OnUpdateAssetInternal(TGroupAsset groupAsset)
        => _groupEx.UpdateProduct(groupAsset);

    internal override void SetAsset(TGroupAsset value)
    {
        base.SetAsset(value);

        if (value is null)
        {
            _groupEx.DetachChildProducts(true);
        }
    }

    public override void DetachAsset(bool detachId)
    {
        base.DetachAsset(detachId);

        _groupEx.DetachChildProducts(false);
    }
}

/// <summary>
/// Builder for group assets (non-generic).
/// </summary>
public class GroupAssetBuilder : GroupAssetBuilder<GroupAsset>
{
}

/// <summary>
/// Selection node for group assets.
/// </summary>
public class GroupAssetSelectionNode : BaseSelectionNode, IHasAsset
{
    private readonly GroupAsset _groupAsset;
    private readonly IAssetFilter _filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupAssetSelectionNode"/> class.
    /// </summary>
    /// <param name="asset">The group asset.</param>
    /// <param name="filter">The optional asset filter.</param>
    public GroupAssetSelectionNode(GroupAsset asset, IAssetFilter filter = null)
    {
        _groupAsset = asset ?? throw new ArgumentNullException(nameof(asset));
        _filter = filter;
    }

    /// <inheritdoc/>
    public override string SelectionKey => _groupAsset._ex.AssetKey;
    /// <inheritdoc/>
    public override object DisplayIcon => _groupAsset.Icon;
    /// <inheritdoc/>
    public override string DisplayText => _groupAsset.DisplayText;

    /// <inheritdoc/>
    public override IEnumerable<ISelectionItem> GetItems()
    {
        return _groupAsset.GetChildAssets(_filter);
    }

    /// <inheritdoc/>
    public override ISelectionItem GetItem(string key)
    {
        FieldCode code = new FieldCode(key);

        if (string.IsNullOrEmpty(code.FieldName))
        {
            return null;
        }

        return _groupAsset.GetChildAsset(code.FieldName);
    }

    #region IAssetContext

    /// <inheritdoc/>
    public Asset TargetAsset => _groupAsset;

    #endregion
}

/// <summary>
/// Generic selection node for group assets.
/// </summary>
/// <typeparam name="T">The type of child assets.</typeparam>
public class GroupAssetSelectionNode<T> : BaseSelectionNode, IHasAsset
{
    private readonly GroupAsset _groupAsset;
    private readonly IAssetFilter _filter;
    private readonly bool _groupSelectable;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupAssetSelectionNode{T}"/> class.
    /// </summary>
    /// <param name="groupAsset">The group asset.</param>
    /// <param name="filter">The optional asset filter.</param>
    /// <param name="groupSelectable">Whether the group itself is selectable.</param>
    public GroupAssetSelectionNode(GroupAsset groupAsset, IAssetFilter filter = null, bool groupSelectable = false)
    {
        _groupAsset = groupAsset ?? throw new ArgumentNullException(nameof(groupAsset));
        _filter = filter;
        _groupSelectable = groupSelectable;
    }

    /// <inheritdoc/>
    public override string SelectionKey => _groupAsset._ex.AssetKey;
    /// <inheritdoc/>
    public override object DisplayIcon => _groupAsset.Icon;
    /// <inheritdoc/>
    public override string DisplayText => _groupAsset.DisplayText;
    /// <inheritdoc/>
    public override bool Selectable => _groupSelectable;

    /// <inheritdoc/>
    public override IEnumerable<ISelectionItem> GetItems()
    {
        return _groupAsset.GetChildAssets(_filter)
            .OfType<T>()
            .OfType<ISelectionItem>();
    }

    /// <inheritdoc/>
    public override ISelectionItem GetItem(string key)
    {
        FieldCode code = new FieldCode(key);

        if (string.IsNullOrEmpty(code.FieldName))
        {
            return null;
        }

        return _groupAsset.GetChildAsset(code.FieldName, _filter);
    }

    #region IAssetContext

    /// <inheritdoc/>
    public Asset TargetAsset => _groupAsset;

    #endregion
}