using Suity.Helpers;
using System;

namespace Suity.Editor;

/// <summary>
/// Represents a storage location with support for different storage types and physical file paths.
/// Implements equality comparison and ordering functionality.
/// </summary>
public sealed class StorageLocation : IEquatable<StorageLocation>, IComparable<StorageLocation>
{
    /// <summary>
    /// Initializes a new instance of the StorageLocation class using a physical file name.
    /// </summary>
    /// <param name="fileName">The physical file name for the storage location.</param>
    internal StorageLocation(string fileName)
    {
        PhysicFileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FullPath = fileName;
    }

    /// <summary>
    /// Initializes a new instance of the StorageLocation class using a storage type and location.
    /// </summary>
    /// <param name="storageType">The type of storage (e.g., "local", "remote").</param>
    /// <param name="location">The specific location within the storage type.</param>
    internal StorageLocation(string storageType, string location)
    {
        if (string.IsNullOrWhiteSpace(storageType))
        {
            throw new ArgumentException($"{nameof(storageType)} is empty.");
        }

        StorageType = storageType;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        FullPath = $"//{StorageType}/{location}";
    }

    /// <summary>
    /// Gets the full path of the storage location.
    /// </summary>
    public string FullPath { get; }
    
    /// <summary>
    /// Gets the physical file name of the storage location.
    /// </summary>
    public string PhysicFileName { get; }
    
    /// <summary>
    /// Gets the type of storage (e.g., "local", "remote").
    /// </summary>
    public string StorageType { get; }
    
    /// <summary>
    /// Gets the specific location within the storage type.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Checks if the storage location exists.
    /// </summary>
    /// <returns>True if the storage location exists; otherwise, false.</returns>
    public bool Exists() => StorageManager.Current.FileExists(this);

    /// <summary>
    /// Gets the storage item associated with this location.
    /// </summary>
    /// <returns>An IStorageItem representing the storage item.</returns>
    public IStorageItem GetStorageItem() => StorageManager.Current.GetStorageItem(this);

    /// <summary>
    /// Returns a string representation of the storage location.
    /// </summary>
    /// <returns>The full path of the storage location.</returns>
    public override string ToString() => FullPath;

    /// <summary>
    /// Creates a StorageLocation instance from a full path string.
    /// </summary>
    /// <param name="fullPath">The full path to parse.</param>
    /// <returns>A new StorageLocation instance, or null if the path is invalid.</returns>
    public static StorageLocation Create(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        if (!fullPath.StartsWith("//"))
        {
            return new StorageLocation(fullPath);
        }

        fullPath = fullPath.RemoveFromFirst("//");
        int index = fullPath.IndexOf('/');
        if (index <= 0)
        {
            return null;
        }

        string storageType = fullPath[..index];
        string location = fullPath.RemoveFromFirst(storageType.Length + 1);

        return new StorageLocation(storageType, location);
    }

    /// <summary>
    /// Determines whether the current StorageLocation is equal to another StorageLocation.
    /// </summary>
    /// <param name="other">The StorageLocation to compare with.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public bool Equals(StorageLocation other)
    {
        if (other is null)
        {
            return false;
        }

        return FullPath.Equals(other.FullPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Compares the current StorageLocation with another StorageLocation.
    /// </summary>
    /// <param name="other">The StorageLocation to compare with.</param>
    /// <returns>A value indicating the relative order of the objects.</returns>
    public int CompareTo(StorageLocation other)
    {
        if (other is null)
        {
            return -1;
        }

        return FullPath.CompareTo(other.FullPath);
    }

    /// <summary>
    /// Returns the hash code for this StorageLocation.
    /// </summary>
    /// <returns>A hash code for the current StorageLocation.</returns>
    public override int GetHashCode()
    {
        return FullPath.ToLower().GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current StorageLocation.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        StorageLocation other = obj as StorageLocation;

        return Equals(other);
    }

    /// <summary>
    /// Equality operator for comparing two StorageLocation instances.
    /// </summary>
    /// <param name="v1">The first StorageLocation to compare.</param>
    /// <param name="v2">The second StorageLocation to compare.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public static bool operator ==(StorageLocation v1, StorageLocation v2)
    {
        if (v1 is null)
        {
            return v2 is null;
        }
        else
        {
            return v1.Equals(v2);
        }
    }

    /// <summary>
    /// Inequality operator for comparing two StorageLocation instances.
    /// </summary>
    /// <param name="v1">The first StorageLocation to compare.</param>
    /// <param name="v2">The second StorageLocation to compare.</param>
    /// <returns>True if the objects are not equal; otherwise, false.</returns>
    public static bool operator !=(StorageLocation v1, StorageLocation v2)
    {
        if (v1 is null)
        {
            return v2 is not null;
        }
        else
        {
            return !v1.Equals(v2);
        }
    }
}
