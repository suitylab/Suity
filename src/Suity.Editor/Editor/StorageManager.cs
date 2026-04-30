namespace Suity.Editor;

/// <summary>
/// Storage manager
/// </summary>
public abstract class StorageManager
{
    /// <summary>
    /// Gets or sets the current storage manager instance.
    /// </summary>
    public static StorageManager Current { get; internal set; }

    /// <summary>
    /// Get whether the file in the specified path exists
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public abstract bool FileExists(string fullPath);

    /// <summary>
    /// Get whether the file in the specified path exists
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public abstract bool FileExists(StorageLocation location);

    /// <summary>
    /// Gets the storage item at the specified full path.
    /// </summary>
    /// <param name="fullPath">The full path to the storage item.</param>
    /// <returns>The storage item at the specified path, or null if not found.</returns>
    public abstract IStorageItem GetStorageItem(string fullPath);

    /// <summary>
    /// Gets the storage item at the specified location.
    /// </summary>
    /// <param name="location">The storage location.</param>
    /// <returns>The storage item at the specified location, or null if not found.</returns>
    public abstract IStorageItem GetStorageItem(StorageLocation location);

    /// <summary>
    /// Tries to parse the provider name and location from a path.
    /// </summary>
    /// <param name="path">The path to parse.</param>
    /// <param name="providerName">When true, receives the provider name.</param>
    /// <param name="location">When true, receives the location string.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public abstract bool TryParseProvider(string path, out string providerName, out string location);

    /// <summary>
    /// Gets the storage provider with the specified name.
    /// </summary>
    /// <param name="name">The name of the provider.</param>
    /// <returns>The storage provider with the specified name.</returns>
    public abstract IStorageProvider GetProvider(string name);

    /// <summary>
    /// Get whether the specified path is in the custom storage space
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public abstract bool IsInCustomStorage(string path);
}