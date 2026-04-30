using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Selecting;
using System;
using System.Collections.Generic;

namespace Suity.Editor;


/// <summary>
/// Interface for resolving assets by key using a specific asset type.
/// </summary>
public interface ITypedAssetResolver
{
    /// <summary>
    /// The type of asset this resolver handles.
    /// </summary>
    Type AssetType { get; }

    /// <summary>
    /// Resolves an asset from the given key.
    /// </summary>
    /// <param name="anyKey">The key to resolve the asset from.</param>
    /// <returns>The resolved asset object, or null if not found.</returns>
    object ResolveAsset(string anyKey);
}

/// <summary>
/// Abstract base class for typed asset resolvers.
/// </summary>
/// <typeparam name="T">The type of asset this resolver handles.</typeparam>
public abstract class TypedAssetResolver<T> : ITypedAssetResolver
{
    /// <summary>
    /// Gets the type of asset this resolver handles.
    /// </summary>
    public Type AssetType => typeof(T);

    /// <summary>
    /// Resolves an asset from the given key.
    /// </summary>
    /// <param name="anyKey">The key to resolve the asset from.</param>
    /// <returns>The resolved asset, or null if not found.</returns>
    public object ResolveAsset(string anyKey) => OnResolveAsset(anyKey);

    /// <summary>
    /// Override this method to implement custom asset resolution logic.
    /// </summary>
    /// <param name="anyKey">The key to resolve the asset from.</param>
    /// <returns>The resolved asset, or null if not found.</returns>
    protected abstract T OnResolveAsset(string anyKey);
}

/// <summary>
/// Asset manager
/// </summary>
public abstract class AssetManager
{
    /// <summary>
    /// Gets or sets the global instance of the asset manager.
    /// </summary>
    public static AssetManager Instance { get; internal set; }

    /// <summary>
    /// Event raised when an asset is updated.
    /// </summary>
    public event Action<Asset, EntryEventArgs> AssetUpdated;

    /// <summary>
    /// Initializes a new instance of the AssetManager class.
    /// </summary>
    internal AssetManager() { }

    /// <summary>
    /// Gets the file extensions that this asset manager handles.
    /// </summary>
    public abstract IEnumerable<string> AttachedAssetExtensions { get; }

    /// <summary>
    /// Adds an asset to the asset manager.
    /// </summary>
    /// <param name="asset">The asset to add.</param>
    /// <returns>The key entry for the added asset.</returns>
    internal abstract AssetKeyEntry AddAsset(Asset asset);

    /// <summary>
    /// Adds an asset of a specific type to the asset manager.
    /// </summary>
    /// <param name="assetType">The reference to the asset type.</param>
    /// <param name="asset">The asset to add.</param>
    /// <returns>A multiple item containing the asset with its key.</returns>
    internal abstract IMultipleItem<string, Asset> AddAsset(EditorAssetRef assetType, Asset asset);

    /// <summary>
    /// Adds an asset of a specific type to the asset manager.
    /// </summary>
    /// <param name="assetType">The type of the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <returns>A multiple item containing the asset with its key.</returns>
    internal abstract IMultipleItem<string, Asset> AddAsset(Type assetType, Asset asset);

    /// <summary>
    /// Gets all assets currently loaded in the asset manager.
    /// </summary>
    internal abstract IEnumerable<Asset> Assets { get; }

    /// <summary>
    /// Gets all assets that have keys starting with the specified prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to filter by.</param>
    /// <returns>An enumerable of assets matching the prefix.</returns>
    internal abstract IEnumerable<Asset> GetAssetsByPrefix(string prefix);

    /// <summary>
    /// Gets an asset by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <returns>The asset with the given ID, or null if not found.</returns>
    public abstract Asset GetAsset(Guid id);

    /// <summary>
    /// Gets an asset by its unique identifier, filtered by the specified filter.
    /// </summary>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given ID that matches the filter, or null if not found.</returns>
    public abstract Asset GetAsset(Guid id, IAssetFilter filter);

    /// <summary>
    /// Gets an asset by its unique identifier as a specific type.
    /// </summary>
    /// <typeparam name="T">The type of asset to retrieve.</typeparam>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <returns>The asset with the given ID, or null if not found.</returns>
    public abstract T GetAsset<T>(Guid id) where T : class;

    /// <summary>
    /// Gets an asset by its unique identifier as a specific type, filtered by the specified filter.
    /// </summary>
    /// <typeparam name="T">The type of asset to retrieve.</typeparam>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given ID that matches the filter, or null if not found.</returns>
    public abstract T GetAsset<T>(Guid id, IAssetFilter filter) where T : class;

    /// <summary>
    /// Gets an asset by its key.
    /// </summary>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset with the given key, or null if not found.</returns>
    public abstract Asset GetAsset(string assetKey);

    /// <summary>
    /// Gets an asset by its key, filtered by the specified filter.
    /// </summary>
    /// <param name="assetKey">The key of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given key that matches the filter, or null if not found.</returns>
    public abstract Asset GetAsset(string assetKey, IAssetFilter filter);

    /// <summary>
    /// Gets an asset by its key as a specific type.
    /// </summary>
    /// <typeparam name="T">The type of asset to retrieve.</typeparam>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset with the given key, or null if not found.</returns>
    public abstract T GetAsset<T>(string assetKey) where T : class;

    /// <summary>
    /// Gets an asset by its key as a specific type, filtered by the specified filter.
    /// </summary>
    /// <typeparam name="T">The type of asset to retrieve.</typeparam>
    /// <param name="assetKey">The key of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given key that matches the filter, or null if not found.</returns>
    public abstract T GetAsset<T>(string assetKey, IAssetFilter filter) where T : class;

    /// <summary>
    /// Gets the asset key entry for a specific asset key.
    /// </summary>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset key entry, or null if not found.</returns>
    internal abstract AssetKeyEntry GetAssetEntry(string assetKey);

    /// <summary>
    /// Ensures that an asset key entry exists for a specific asset key, creating one if it does not exist.
    /// </summary>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset key entry.</returns>
    internal abstract AssetKeyEntry EnsureAssetEntry(string assetKey);

    /// <summary>
    /// Gets an asset by its asset type ID and key.
    /// </summary>
    /// <param name="assetTypeId">The unique identifier of the asset type.</param>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset with the given type ID and key, or null if not found.</returns>
    public abstract Asset GetAsset(Guid assetTypeId, string assetKey);

    /// <summary>
    /// Gets an asset by its asset type ID and key, filtered by the specified filter.
    /// </summary>
    /// <param name="assetTypeId">The unique identifier of the asset type.</param>
    /// <param name="assetKey">The key of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given type ID and key that matches the filter, or null if not found.</returns>
    public abstract Asset GetAsset(Guid assetTypeId, string assetKey, IAssetFilter filter);

    /// <summary>
    /// Gets an asset by its asset type name and key.
    /// </summary>
    /// <param name="assetType">The name of the asset type.</param>
    /// <param name="assetKey">The key of the asset.</param>
    /// <returns>The asset with the given type name and key, or null if not found.</returns>
    public abstract Asset GetAsset(string assetType, string assetKey);

    /// <summary>
    /// Gets an asset by its asset type name and key, filtered by the specified filter.
    /// </summary>
    /// <param name="assetType">The name of the asset type.</param>
    /// <param name="assetKey">The key of the asset.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The asset with the given type name and key that matches the filter, or null if not found.</returns>
    public abstract Asset GetAsset(string assetType, string assetKey, IAssetFilter filter);

    /// <summary>
    /// Gets the asset collection for a specific asset type by its ID.
    /// </summary>
    /// <param name="assetTypeId">The unique identifier of the asset type.</param>
    /// <returns>The asset collection for the given type ID, or null if not found.</returns>
    public abstract IGeneralAssetCollection GetAssetCollection(Guid assetTypeId);

    /// <summary>
    /// Gets the asset collection for a specific asset type by its name.
    /// </summary>
    /// <param name="assetType">The name of the asset type.</param>
    /// <returns>The asset collection for the given type name, or null if not found.</returns>
    public abstract IGeneralAssetCollection GetAssetCollection(string assetType);

    /// <summary>
    /// Gets the asset collection for a specific asset type.
    /// </summary>
    /// <param name="type">The asset type.</param>
    /// <returns>The asset collection for the given type, or null if not found.</returns>
    public abstract IGeneralAssetCollection GetAssetCollection(Type type);

    /// <summary>
    /// Gets the asset collection for a specific asset type.
    /// </summary>
    /// <typeparam name="T">The asset type.</typeparam>
    /// <returns>The asset collection for the given type, or null if not found.</returns>
    public abstract IGeneralAssetCollection GetAssetCollection<T>();

    /// <summary>
    /// Gets all assets of a specific asset type by its ID, filtered by the specified filter.
    /// </summary>
    /// <param name="assetTypeId">The unique identifier of the asset type.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>An enumerable of assets matching the criteria.</returns>
    public abstract IEnumerable<Asset> GetAssets(Guid assetTypeId, IAssetFilter filter);

    /// <summary>
    /// Gets all assets of a specific asset type by its name, filtered by the specified filter.
    /// </summary>
    /// <param name="assetTypeName">The name of the asset type.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>An enumerable of assets matching the criteria.</returns>
    public abstract IEnumerable<Asset> GetAssets(string assetTypeName, IAssetFilter filter);

    /// <summary>
    /// Gets all assets of a specific asset type, filtered by the specified filter.
    /// </summary>
    /// <param name="type">The asset type.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>An enumerable of assets matching the criteria.</returns>
    public abstract IEnumerable<Asset> GetAssets(Type type, IAssetFilter filter);

    /// <summary>
    /// Gets all assets of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of assets to retrieve.</typeparam>
    /// <returns>An enumerable of assets of the specified type.</returns>
    public abstract IEnumerable<T> GetAssets<T>();

    /// <summary>
    /// Gets all assets of a specific type, filtered by the specified filter.
    /// </summary>
    /// <typeparam name="T">The type of assets to retrieve.</typeparam>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>An enumerable of assets of the specified type matching the filter.</returns>
    public abstract IEnumerable<T> GetAssets<T>(IAssetFilter filter);

    /// <summary>
    /// Gets the asset link for a specific type.
    /// </summary>
    /// <typeparam name="T">The type of asset to get the link for.</typeparam>
    /// <returns>The asset link for the specified type.</returns>
    public abstract DAssetLink GetAssetLink<T>() where T : class;

    /// <summary>
    /// Gets the asset link for a specific type.
    /// </summary>
    /// <param name="type">The type of asset to get the link for.</param>
    /// <returns>The asset link for the specified type.</returns>
    public abstract DAssetLink GetAssetLink(Type type);

    /// <summary>
    /// Gets a selection list for assets of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of assets to include in the selection list.</typeparam>
    /// <param name="filter">The optional filter to apply.</param>
    /// <returns>A selection list containing the filtered assets.</returns>
    public ISelectionList GetAssetSelectionList<T>(IAssetFilter filter = null)
    {
        var collection = GetAssetCollection<T>();
        if (collection != null)
        {
            if (filter is null || filter == AssetFilters.All)
            {
                return collection;
            }
            else
            {
                return new AssetSelectionList<Asset>(collection, filter);
            }
        }
        else
        {
            return EmptySelectionList.Empty;
        }
    }

    /// <summary>
    /// Gets a selection list for assets of a specific type.
    /// </summary>
    /// <param name="type">The type of assets to include in the selection list.</param>
    /// <param name="filter">The optional filter to apply.</param>
    /// <returns>A selection list containing the filtered assets.</returns>
    public ISelectionList GetAssetSelectionList(Type type, IAssetFilter filter = null)
    {
        var collection = GetAssetCollection(type);
        if (collection != null)
        {
            if (filter is null || filter == AssetFilters.All)
            {
                return collection;
            }
            else
            {
                return new AssetSelectionList<Asset>(collection, filter);
            }
        }
        else
        {
            return EmptySelectionList.Empty;
        }
    }

    #region AssetTypeBindings

    /// <summary>
    /// Resolves the asset type name for a given type.
    /// </summary>
    /// <param name="type">The type to resolve the asset type name for.</param>
    /// <returns>The name of the asset type.</returns>
    public abstract string ResolveAssetTypeName(Type type);

    /// <summary>
    /// Gets the asset type for a given name.
    /// </summary>
    /// <param name="name">The name of the asset type.</param>
    /// <returns>The type associated with the given name.</returns>
    public abstract Type GetAssetType(string name);

    #endregion

    #region ResourceName & LocalName

    /// <summary>
    /// Updates the resource name of an asset.
    /// </summary>
    /// <param name="asset">The asset to update.</param>
    /// <param name="name">The new resource name.</param>
    /// <returns>A named multiple item containing the updated asset.</returns>
    internal abstract INamedMultipleItem<Asset> UpdateResourceName(Asset asset, string name);

    /// <summary>
    /// Gets an asset by custom resource name.
    /// The default resource name is of the form 'xxx.xxx', which consists of an object's namespace (or ImportId) and the object's local name.
    /// </summary>
    /// <param name="name">The resource name of the asset.</param>
    /// <returns>The asset with the given resource name, or null if not found.</returns>
    public abstract Asset GetAssetByResourceName(string name);

    /// <summary>
    /// Gets an asset by custom resource name and type.
    /// The default resource name is of the form 'xxx.xxx', which consists of an object's namespace (or ImportId) and the object's local name.
    /// </summary>
    /// <param name="type">The type of asset to search for.</param>
    /// <param name="name">The resource name of the asset.</param>
    /// <returns>The asset with the given resource name and type, or null if not found.</returns>
    public abstract Asset GetAssetByResourceName(Type type, string name);

    /// <summary>
    /// Gets an asset by custom resource name as a specific type.
    /// The default resource name is of the form 'xxx.xxx', which consists of an object's namespace (or ImportId) and the object's local name.
    /// </summary>
    /// <typeparam name="T">The type of asset to retrieve.</typeparam>
    /// <param name="name">The resource name of the asset.</param>
    /// <returns>The asset with the given resource name, or null if not found.</returns>
    public abstract T GetAssetByResourceName<T>(string name) where T : class;

    /// <summary>
    /// Gets all assets matching a custom resource name.
    /// The default resource name is of the form 'xxx.xxx', which consists of an object's namespace (or ImportId) and the object's local name.
    /// </summary>
    /// <param name="name">The resource name to search for.</param>
    /// <returns>A named multiple item containing all assets matching the resource name.</returns>
    public abstract INamedMultipleItem<Asset> GetAssetsByResourceName(string name);

    #endregion

    /// <summary>
    /// Notifies that an asset has been updated.
    /// </summary>
    /// <param name="asset">The asset that was updated.</param>
    /// <param name="eventArgs">The event arguments.</param>
    internal void NotifyAssetUpdated(Asset asset, EntryEventArgs eventArgs)
    {
        AssetUpdated?.Invoke(asset, eventArgs);
    }

    /// <summary>
    /// Creates an external representation of an asset.
    /// </summary>
    /// <param name="asset">The asset to create an external for.</param>
    /// <returns>The asset external representation.</returns>
    internal abstract AssetExternal CreateAssetExternal(Asset asset);

    /// <summary>
    /// Creates an external representation of a group asset.
    /// </summary>
    /// <param name="groupAsset">The group asset to create an external for.</param>
    /// <returns>The group asset external representation.</returns>
    internal abstract GroupAssetExternal CreateGroupAssetExternal(GroupAsset groupAsset);

    /// <summary>
    /// Creates an external representation of an asset builder.
    /// </summary>
    /// <typeparam name="TAsset">The type of asset being built.</typeparam>
    /// <param name="builder">The asset builder.</param>
    /// <returns>The asset builder external representation.</returns>
    internal abstract AssetBuilderExternal<TAsset> CreateBuilderExternal<TAsset>(AssetBuilder<TAsset> builder)
        where TAsset : Asset, new();

    /// <summary>
    /// Creates an external representation of a group asset builder.
    /// </summary>
    /// <typeparam name="TGroupAsset">The type of group asset being built.</typeparam>
    /// <param name="groupBuilder">The group asset builder.</param>
    /// <returns>The group asset builder external representation.</returns>
    internal abstract GroupAssetBuilderExternal<TGroupAsset> CreateGroupBuilderExternal<TGroupAsset>(GroupAssetBuilder<TGroupAsset> groupBuilder)
        where TGroupAsset : GroupAsset, new();
}