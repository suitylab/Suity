using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Internal backend for <see cref="AssetBuilder{TAsset}"/> managing asset lifecycle including
/// set, ensure, detach, resolve, and update operations. Also provides auto-update actions,
/// value updates, reference updates, element collectors, and field collectors.
/// </summary>
/// <typeparam name="TAsset">The type of asset this builder manages.</typeparam>
internal class AssetBuilderExternalBK<TAsset>(AssetBuilder<TAsset> builder) : AssetBuilderExternal<TAsset>
    where TAsset : Asset, new()
{
    private TAsset _asset;

    private readonly AssetBuilder<TAsset> _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    private Dictionary<string, IAssetUpdateAction<TAsset>> _updateActions;

    /// <summary>
    /// Ensures the update actions dictionary is initialized and returns it.
    /// </summary>
    /// <returns>The initialized dictionary of update actions.</returns>
    internal Dictionary<string, IAssetUpdateAction<TAsset>> EnsureDictionary() => _updateActions ??= [];

    /// <inheritdoc/>
    public override TAsset Asset => _asset;

    /// <inheritdoc/>
    public override void SetAsset(TAsset value)
    {
        if (ReferenceEquals(_asset, value))
        {
            return;
        }

        if (value?.Builder != null)
        {
            throw new InvalidOperationException("Asset definition exist");
        }

        var old = _asset;
        var entry = old?.Entry;
        _asset = null;

        if (old != null)
        {
            old.Builder = null;
            old.Entry = null;
        }

        _asset = value;

        if (value != null)
        {
            value.Builder = _builder;
            if (entry != null)
            {
                value.Entry = entry;
            }

            value.LocalName = _builder.LocalName;
        }

        _builder.UpdateAsset();

        _builder.OnAssetChangedInternal(old, true);
        _builder.OnAssetChanged(old, true);
        _builder.InternalRaiseAssetChanged(old);
    }

    /// <inheritdoc/>
    public override TAsset EnsureAsset()
    {
        if (_asset is null)
        {
            // Need to call external SetAsset, because there is override
            _builder.SetAsset(new TAsset());
        }

        return _asset;
    }

    /// <summary>
    /// Detaches the asset from this builder, optionally also detaching its ID.
    /// </summary>
    /// <param name="detachId">If true, sets the asset to null; otherwise, only clears the builder reference.</param>
    public override void DetachAsset(bool detachId)
    {
        if (_asset is null)
        {
            return;
        }

        if (detachId)
        {
            _builder.SetAsset(null);
        }
        else
        {
            var old = _asset;
            _asset = null;
            old.Builder = null;

            _builder.OnAssetChangedInternal(old, false);
            _builder.OnAssetChanged(old, false);
            _builder.InternalRaiseAssetChanged(old);
        }
    }

    /// <inheritdoc/>
    public override TAsset ResolveAsset(IdResolveType resolveType)
    {
        if (_asset is null)
        {
            // Need to call external SetAsset, because there is override
            _builder.SetAsset(new TAsset());
        }
        _asset?.ResolveId(resolveType);
        return _asset;
    }

    /// <inheritdoc/>
    public override void UpdateAsset()
    {
        var asset = Asset;
        if (asset is null)
        {
            return;
        }

        _builder.OnUpdateAssetBase(asset);
        if (_updateActions != null)
        {
            foreach (var action in _updateActions.Values)
            {
                action.DoAction(asset);
            }
        }

        _builder.OnUpdateAssetInternal(asset);
        _builder.OnUpdateAsset(asset);
    }

    /// <inheritdoc/>
    public override IAssetUpdateAction<TAsset> GetUpdater(string name)
        => _updateActions?.GetValueSafe(name);

    /// <inheritdoc/>
    public override IValueUpdateAction AddAutoUpdate(string name, Action<TAsset> action)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var updateAction = new AssetSimpleUpdateAction<TAsset>(_builder, name, action);

        EnsureDictionary().Add(name, updateAction);

        return updateAction;
    }

    /// <inheritdoc/>
    public override IValueUpdateAction<TValue> AddValueAutoUpdate<TValue>(string name, Action<TAsset, TValue> action)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var updateAction = new AssetValueUpdateAction<TAsset, TValue>(_builder, name, action);

        EnsureDictionary().Add(name, updateAction);

        return updateAction;
    }

    /// <summary>
    /// Adds a reference-based auto-update action that caches a reference to another asset by ID.
    /// </summary>
    /// <typeparam name="TRef">The type of the referenced asset.</typeparam>
    /// <param name="name">The name of the update action.</param>
    /// <param name="action">The action to execute with the asset and reference ID.</param>
    /// <returns>The created reference update action.</returns>
    public override IRefUpdateAction<TRef> AddRefAutoUpdate<TRef>(string name, Action<TAsset, Guid> action)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var updateAction = new AssetRefUpdateAction<TAsset, TRef>(_builder, name, action);

        EnsureDictionary().Add(name, updateAction);

        return updateAction;
    }

    /// <inheritdoc/>
    public override IAssetElementCollector<TValue> AddElementCollector<TValue>(string name, Action<TAsset, ICollection<TValue>> action)
    {
        var collector = new AssetElementCollector<TAsset, TValue>(name, _builder, action);
        AddAutoUpdate(name, collector.UpdateAsset);

        return collector;
    }

    /// <inheritdoc/>
    public override IAssetElementCollector<TKey, TValue> AddElementCollector<TKey, TValue>(string name, Action<TAsset, IDictionary<TKey, TValue>> action)
    {
        var collector = new AssetElementCollector<TAsset, TKey, TValue>(name, _builder, action);
        AddAutoUpdate(name, collector.UpdateAsset);

        return collector;
    }

    /// <inheritdoc/>
    public override IAssetFieldCollector<TField> AddFieldCollector<TField>(
        string name,
        Func<TAsset, FieldObjectCollection<TField>> getCollection,
        FieldAddOrUpdateAction<TAsset, TField> addOrUpdateAction = null,
        FieldUpdateAction<TAsset, TField> updateAction = null)
    {
        var collector = new AssetFieldCollector<TAsset, TField>
            (
            name, _builder, getCollection, addOrUpdateAction, updateAction
            );

        AddAutoUpdate(name, collector.UpdateAsset);

        return collector;
    }
}