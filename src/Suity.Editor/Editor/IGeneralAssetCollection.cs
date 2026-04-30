using Suity.Selecting;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// A collection of assets that provides access to individual assets by key and supports filtering.
/// </summary>
/// <typeparam name="TAsset">The type of asset contained in this collection.</typeparam>
public interface IAssetCollection<TAsset> : ISelectionList
    where TAsset : Asset
{
    /// <summary>
    /// Gets all assets in the collection.
    /// </summary>
    IEnumerable<TAsset> Assets { get; }

    /// <summary>
    /// Gets an asset by its unique key.
    /// </summary>
    /// <param name="assetKey">The unique key of the asset.</param>
    /// <returns>The asset with the specified key, or null if not found.</returns>
    TAsset GetAsset(string assetKey);

    /// <summary>
    /// Gets an asset by its unique key that matches the specified filter.
    /// </summary>
    /// <param name="assetKey">The unique key of the asset.</param>
    /// <param name="filter">The filter to apply when selecting the asset.</param>
    /// <returns>The asset matching both the key and filter, or null if not found.</returns>
    TAsset GetAsset(string assetKey, IAssetFilter filter);

    /// <summary>
    /// Gets the primary (default) asset in the collection.
    /// </summary>
    TAsset PrimaryAsset { get; }

    /// <summary>
    /// Gets the primary asset that matches the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply when selecting the primary asset.</param>
    /// <returns>The primary asset matching the filter, or null if not found.</returns>
    TAsset GetPrimaryAsset(IAssetFilter filter);

    /// <summary>
    /// Gets the number of assets in the collection.
    /// </summary>
    int Count { get; }
}

/// <summary>
/// A generic asset collection that contains base Asset objects.
/// </summary>
public interface IGeneralAssetCollection : IAssetCollection<Asset>
{
}

/// <summary>
/// An empty asset collection that contains no assets.
/// </summary>
/// <typeparam name="TAsset">The type of asset contained in this collection.</typeparam>
public class EmptyAssetCollection<TAsset> : IAssetCollection<TAsset>
    where TAsset : Asset
{
    /// <summary>
    /// Gets a singleton empty instance of the collection.
    /// </summary>
    public static readonly EmptyAssetCollection<TAsset> Empty = new();

    internal EmptyAssetCollection()
    { }

    /// <summary>
    /// Gets an empty enumeration of assets.
    /// </summary>
    public IEnumerable<TAsset> Assets => [];

    /// <summary>
    /// Gets zero as the count of assets.
    /// </summary>
    public int Count => 0;

    /// <summary>
    /// Always returns null since the collection is empty.
    /// </summary>
    /// <param name="assetKey">The asset key (ignored).</param>
    /// <returns>Always returns null.</returns>
    public TAsset GetAsset(string assetKey) => null;

    /// <summary>
    /// Always returns null since the collection is empty.
    /// </summary>
    /// <param name="assetKey">The asset key (ignored).</param>
    /// <param name="filter">The filter (ignored).</param>
    /// <returns>Always returns null.</returns>
    public TAsset GetAsset(string assetKey, IAssetFilter filter) => null;

    /// <summary>
    /// Always returns null since the collection is empty.
    /// </summary>
    /// <param name="key">The item key (ignored).</param>
    /// <returns>Always returns null.</returns>
    public ISelectionItem GetItem(string key) => null;

    /// <summary>
    /// Gets an empty enumeration of items.
    /// </summary>
    /// <returns>Always returns an empty enumeration.</returns>
    public IEnumerable<ISelectionItem> GetItems() => [];

    /// <summary>
    /// Gets null as the primary asset.
    /// </summary>
    public TAsset PrimaryAsset => null;

    /// <summary>
    /// Always returns null since the collection is empty.
    /// </summary>
    /// <param name="filter">The filter (ignored).</param>
    /// <returns>Always returns null.</returns>
    public TAsset GetPrimaryAsset(IAssetFilter filter) => null;
}

/// <summary>
/// An empty asset collection specifically for base Asset objects.
/// </summary>
public class EmptyBaseAssetCollection : EmptyAssetCollection<Asset>, IGeneralAssetCollection
{
    /// <summary>
    /// Gets a singleton empty instance of the collection.
    /// </summary>
    public new static readonly EmptyBaseAssetCollection Empty = new();

    internal EmptyBaseAssetCollection()
    { }
}