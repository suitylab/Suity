using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Provides access to the associated AssetBuilder for asset construction.
/// </summary>
internal interface IHasAssetBuilder
{
    /// <summary>
    /// Gets the target AssetBuilder instance.
    /// </summary>
    AssetBuilder TargetAssetBuilder { get; }
}

/// <summary>
/// Provides access to the associated Asset.
/// </summary>
public interface IHasAsset
{
    /// <summary>
    /// Gets the target Asset instance.
    /// </summary>
    Asset TargetAsset { get; }
}

/// <summary>
/// Collector interface for managing asset elements with single-value items.
/// </summary>
/// <typeparam name="TValue">The type of elements to collect.</typeparam>
public interface IAssetElementCollector<TValue>
{
    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the item was added successfully; otherwise, false.</returns>
    bool AddItem(TValue value);

    /// <summary>
    /// Sets all items in the collection, replacing any existing items.
    /// </summary>
    /// <param name="values">The collection of values to set.</param>
    void SetItems(IEnumerable<TValue> values);

    /// <summary>
    /// Adds a new item or updates an existing one.
    /// </summary>
    /// <param name="value">The value to add or update.</param>
    void AddOrUpdateItem(TValue value);

    /// <summary>
    /// Removes an item from the collection.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the item was removed successfully; otherwise, false.</returns>
    bool RemoveItem(TValue value);

    /// <summary>
    /// Clears all items from the collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Begins an update batch operation.
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// Ends an update batch operation.
    /// </summary>
    void EndUpdate();
}

/// <summary>
/// Collector interface for managing asset elements with key-value pairs.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify elements.</typeparam>
/// <typeparam name="TValue">The type of elements to collect.</typeparam>
public interface IAssetElementCollector<TKey, TValue>
{
    /// <summary>
    /// Adds a new item or updates an existing one with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to add or update.</param>
    void AddOrUpdateItem(TKey key, TValue value);

    /// <summary>
    /// Removes an item associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void RemoveItem(TKey key);

    /// <summary>
    /// Begins an update batch operation.
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// Ends an update batch operation.
    /// </summary>
    void EndUpdate();
}

/// <summary>
/// Interface for publishing assets.
/// </summary>
public interface IAssetPublish
{
    /// <summary>
    /// Publishes the asset.
    /// </summary>
    void Publish();
}
