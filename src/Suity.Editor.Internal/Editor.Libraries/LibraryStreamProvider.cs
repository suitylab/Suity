using System;

namespace Suity.Editor.Libraries;

/// <summary>
/// Provides storage access for library assets, implementing read-only storage provider functionality.
/// </summary>
public class LibraryStreamProvider : IStorageProvider
{
    /// <summary>
    /// Initializes a new instance of the library stream provider.
    /// </summary>
    public LibraryStreamProvider()
    {
    }

    /// <summary>
    /// Gets the name of the storage provider, matching the library storage type.
    /// </summary>
    public string Name => LibraryAsset.LibraryStorageType;

    /// <summary>
    /// Gets a value indicating whether this provider is read-only.
    /// </summary>
    public bool IsReadonly => true;

    /// <summary>
    /// Checks if a storage item exists at the specified location.
    /// </summary>
    /// <param name="location">The location to check.</param>
    /// <returns>True if an asset exists at the location; otherwise, false.</returns>
    public bool Exists(string location)
    {
        return LibraryAssetBK.TryGetAssetByLocation(location, out LibraryAsset lib, out Asset asset);
    }

    /// <summary>
    /// Gets the storage item at the specified location.
    /// </summary>
    /// <param name="location">The location of the storage item.</param>
    /// <returns>The storage item, or null if not found.</returns>
    public IStorageItem GetStorageItem(string location)
    {
        if (LibraryAssetBK.TryParseLocation(location, out Guid id, out string assetKey))
        {
            var asset = AssetManager.Instance.GetAsset(id) as LibraryAssetBK;

            return asset?.GetStream(assetKey);
        }

        return null;
    }

    /// <summary>
    /// Gets the asset associated with the specified location.
    /// </summary>
    /// <param name="location">The location to resolve.</param>
    /// <returns>The associated asset, or null if not found.</returns>
    public Asset GetAsset(string location)
    {
        if (LibraryAssetBK.TryGetAssetByLocation(location, out LibraryAsset lib, out Asset asset))
        {
            return asset;
        }
        else
        {
            return null;
        }
    }
}
