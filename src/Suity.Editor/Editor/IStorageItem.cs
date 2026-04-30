using System;
using System.IO;

namespace Suity.Editor;

/// <summary>
/// Provides access to a storage system for reading and writing assets.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Gets the name of the storage provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the storage is read-only.
    /// </summary>
    bool IsReadonly { get; }

    /// <summary>
    /// Checks if an asset exists at the specified location.
    /// </summary>
    /// <param name="location">The location of the asset.</param>
    /// <returns>True if the asset exists; otherwise, false.</returns>
    bool Exists(string location);

    /// <summary>
    /// Gets a storage item for the specified location.
    /// </summary>
    /// <param name="location">The location of the storage item.</param>
    /// <returns>The storage item, or null if not found.</returns>
    IStorageItem GetStorageItem(string location);

    /// <summary>
    /// Gets an asset from the specified location.
    /// </summary>
    /// <param name="location">The location of the asset.</param>
    /// <returns>The asset, or null if not found.</returns>
    Asset GetAsset(string location);
}

/// <summary>
/// Represents a storage item that provides streams for reading and writing data.
/// </summary>
public interface IStorageItem : IDisposable
{
    /// <summary>
    /// Gets the file name of the storage item.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Gets an input stream for reading data from the storage item.
    /// </summary>
    /// <returns>The input stream.</returns>
    Stream GetInputStream();

    /// <summary>
    /// Gets an output stream for writing data to the storage item.
    /// </summary>
    /// <returns>The output stream.</returns>
    Stream GetOutputStream();
}