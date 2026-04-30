using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Library asset
/// </summary>
public abstract class LibraryAsset : Asset
{
    /// <summary>
    /// The storage type identifier for library assets.
    /// </summary>
    public const string LibraryStorageType = "Library";

    /// <summary>
    /// Gets the name of the library.
    /// </summary>
    public abstract string LibraryName { get; }

    /// <summary>
    /// Gets the version of the library.
    /// </summary>
    public abstract string LibraryVersion { get; }

    /// <summary>
    /// Gets the collection of content assets contained in this library.
    /// </summary>
    public abstract IEnumerable<Asset> ContentAssets { get; }

    /// <summary>
    /// Retrieves a content asset by its key.
    /// </summary>
    /// <param name="assetKey">The key identifier of the asset.</param>
    /// <returns>The asset with the specified key, or null if not found.</returns>
    public abstract Asset GetContentAsset(string assetKey);

    /// <summary>
    /// Retrieves a content asset by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the asset.</param>
    /// <returns>The asset with the specified ID, or null if not found.</returns>
    public abstract Asset GetContentAsset(Guid id);
}