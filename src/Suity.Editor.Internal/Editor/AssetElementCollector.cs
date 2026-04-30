using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Internal generic class that collects asset elements into a collection-backed asset.
/// Supports batch updates via <see cref="BeginUpdate"/> and <see cref="EndUpdate"/> to defer
/// asset rebuilds until multiple changes are complete.
/// </summary>
/// <typeparam name="TAsset">The asset type, which must have a parameterless constructor.</typeparam>
/// <typeparam name="TValue">The type of elements collected in the asset.</typeparam>
internal class AssetElementCollector<TAsset, TValue> : IAssetElementCollector<TValue>
    where TAsset : Asset, new()
{
    private readonly string _name;
    private readonly AssetBuilder<TAsset> _builder;
    private readonly Action<TAsset, ICollection<TValue>> _action;
    private readonly HashSet<TValue> _collection = [];
    private bool _typeUpdateSuspend;
    private bool _typeUpdated;

    /// <summary>
    /// Initializes a new instance of the collector.
    /// </summary>
    /// <param name="name">The name identifier for this collector.</param>
    /// <param name="builder">The asset builder used to update the underlying asset.</param>
    /// <param name="action">The action invoked to apply collected elements to the asset.</param>
    internal AssetElementCollector(string name, AssetBuilder<TAsset> builder, Action<TAsset, ICollection<TValue>> action)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Applies the current collection of elements to the specified asset using the configured action.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    internal void UpdateAsset(TAsset asset)
    {
        _action(asset, _collection);
    }

    /// <inheritdoc/>
    public bool AddItem(TValue value)
    {
        if (value == null)
        {
            return false;
        }

        if (_collection.Add(value))
        {
            if (!_typeUpdateSuspend)
            {
                _builder.TryUpdateNow(UpdateAsset);
            }
            else
            {
                _typeUpdated = true;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void SetItems(IEnumerable<TValue> values)
    {
        _collection.Clear();
        _collection.AddRange(values);

        if (!_typeUpdateSuspend)
        {
            _builder.TryUpdateNow(UpdateAsset);
        }
        else
        {
            _typeUpdated = true;
        }
    }

    /// <inheritdoc/>
    public void AddOrUpdateItem(TValue value)
    {
        if (value == null)
        {
            return;
        }

        _collection.Add(value);

        if (!_typeUpdateSuspend)
        {
            _builder.TryUpdateNow(UpdateAsset);
        }
        else
        {
            _typeUpdated = true;
        }
    }

    /// <inheritdoc/>
    public bool RemoveItem(TValue value)
    {
        if (value == null)
        {
            return false;
        }

        if (_collection.Remove(value))
        {
            if (!_typeUpdateSuspend)
            {
                _builder.TryUpdateNow(UpdateAsset);
            }
            else
            {
                _typeUpdated = true;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _collection.Clear();

        if (!_typeUpdateSuspend)
        {
            _builder.TryUpdateNow(UpdateAsset);
        }
        else
        {
            _typeUpdated = true;
        }
    }

    /// <inheritdoc/>
    public void BeginUpdate()
    {
        _typeUpdateSuspend = true;
        _typeUpdated = false;
    }

    /// <inheritdoc/>
    public void EndUpdate()
    {
        if (_typeUpdateSuspend)
        {
            _typeUpdateSuspend = false;
            if (_typeUpdated)
            {
                _typeUpdated = false;
                _builder.TryUpdateNow(UpdateAsset);
            }
        }
    }
}

/// <summary>
/// Internal generic class that collects asset elements into a dictionary-backed asset.
/// Supports batch updates via <see cref="BeginUpdate"/> and <see cref="EndUpdate"/> to defer
/// asset rebuilds until multiple changes are complete.
/// </summary>
/// <typeparam name="TAsset">The asset type, which must have a parameterless constructor.</typeparam>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
internal class AssetElementCollector<TAsset, TKey, TValue> : IAssetElementCollector<TKey, TValue>
    where TAsset : Asset, new()
{
    private readonly string _name;
    private readonly AssetBuilder<TAsset> _builder;
    private readonly Action<TAsset, IDictionary<TKey, TValue>> _action;
    private readonly Dictionary<TKey, TValue> _dic = [];
    private bool _typeUpdateSuspend;
    private bool _typeUpdated;

    /// <summary>
    /// Initializes a new instance of the dictionary-backed collector.
    /// </summary>
    /// <param name="name">The name identifier for this collector.</param>
    /// <param name="builder">The asset builder used to update the underlying asset.</param>
    /// <param name="action">The action invoked to apply collected key-value pairs to the asset.</param>
    internal AssetElementCollector(string name, AssetBuilder<TAsset> builder, Action<TAsset, IDictionary<TKey, TValue>> action)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Applies the current dictionary of key-value pairs to the specified asset using the configured action.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    internal void UpdateAsset(TAsset asset)
    {
        _action(asset, _dic);
    }

    /// <summary>
    /// Adds or updates a key-value pair in the collection and triggers an asset update.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    public void AddOrUpdateItem(TKey key, TValue value)
    {
        if (value == null)
        {
            return;
        }

        _dic[key] = value;
        if (!_typeUpdateSuspend)
        {
            _builder.TryUpdateNow(UpdateAsset);
        }
        else
        {
            _typeUpdated = true;
        }
    }

    /// <summary>
    /// Removes a key-value pair from the collection by key and triggers an asset update.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void RemoveItem(TKey key)
    {
        if (_dic.Remove(key))
        {
            if (!_typeUpdateSuspend)
            {
                _builder.TryUpdateNow(UpdateAsset);
            }
            else
            {
                _typeUpdated = true;
            }
        }
    }

    /// <summary>
    /// Begins a batch update session, deferring asset rebuilds until <see cref="EndUpdate"/> is called.
    /// </summary>
    public void BeginUpdate()
    {
        _typeUpdateSuspend = true;
        _typeUpdated = false;
    }

    /// <summary>
    /// Ends a batch update session and triggers an asset update if any changes were made.
    /// </summary>
    public void EndUpdate()
    {
        if (_typeUpdateSuspend)
        {
            _typeUpdateSuspend = false;
            if (_typeUpdated)
            {
                _typeUpdated = false;
                _builder.TryUpdateNow(UpdateAsset);
            }
        }
    }
}