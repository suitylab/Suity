using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Provides methods for resolving object identifiers (GUIDs) from string keys and vice versa.
/// </summary>
public interface IObjectIdResolver
{
    /// <summary>
    /// Generates a new unique GUID.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    Guid NewGuid();

    /// <summary>
    /// Resolves a key to a GUID, creating a new GUID if the key does not exist.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom names.</param>
    /// <returns>The resolved GUID.</returns>
    Guid Resolve(string key, bool resolveCustomName);

    /// <summary>
    /// Resolves a key to a GUID for entry purposes, creating a new GUID if the key does not exist.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom names.</param>
    /// <returns>The resolved GUID.</returns>
    Guid ResolveEntry(string key, bool resolveCustomName);

    /// <summary>
    /// Attempts to resolve a key to a GUID.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="id">The resolved GUID if found; otherwise, <see cref="Guid.Empty"/>.</param>
    /// <returns>True if the key was resolved; otherwise, false.</returns>
    bool TryResolve(string key, out Guid id);

    /// <summary>
    /// Reverts a GUID to its original key.
    /// </summary>
    /// <param name="id">The GUID to revert.</param>
    /// <returns>The original key if found; otherwise, null.</returns>
    string RevertResolve(Guid id);

    /// <summary>
    /// Records a key-GUID pair for later resolution.
    /// </summary>
    /// <param name="key">The string key.</param>
    /// <param name="id">The GUID to associate with the key.</param>
    /// <param name="reverse">Whether to record the reverse mapping.</param>
    void Record(string key, Guid id, bool reverse = true);

    /// <summary>
    /// Renames a key while preserving its associated GUID.
    /// </summary>
    /// <param name="key">The existing key.</param>
    /// <param name="newKey">The new key name.</param>
    void Rename(string key, string newKey);
}

/// <summary>
/// Default implementation of <see cref="IObjectIdResolver"/> that uses in-memory dictionaries for storage.
/// </summary>
public class DefaultObjectIdResolver : IObjectIdResolver
{
    /// <summary>
    /// Gets the default singleton instance of <see cref="DefaultObjectIdResolver"/>.
    /// </summary>
    public static DefaultObjectIdResolver Default { get; } = new DefaultObjectIdResolver();

    private readonly object _syncRoot = new object();
    private readonly Dictionary<string, Guid> _nameResolvers = new Dictionary<string, Guid>();
    private readonly Dictionary<Guid, string> _revertResolvers = new Dictionary<Guid, string>();

    /// <summary>
    /// Generates a new unique GUID.
    /// </summary>
    /// <returns>A new <see cref="Guid"/>.</returns>
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Records a key-GUID pair for later resolution.
    /// </summary>
    /// <param name="key">The string key.</param>
    /// <param name="id">The GUID to associate with the key.</param>
    /// <param name="reverse">Whether to record the reverse mapping.</param>
    public void Record(string key, Guid id, bool reverse = true)
    {
        lock (_syncRoot)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _nameResolvers[key] = id;
            }
        }
    }

    /// <summary>
    /// Renames a key while preserving its associated GUID.
    /// </summary>
    /// <param name="key">The existing key.</param>
    /// <param name="newKey">The new key name.</param>
    public void Rename(string key, string newKey)
    {
        lock (_syncRoot)
        {
            if (_nameResolvers.TryGetValue(key, out Guid id))
            {
                _nameResolvers[newKey] = id;
            }
        }
    }

    /// <summary>
    /// Resolves a key to a GUID, creating a new GUID if the key does not exist.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom names.</param>
    /// <returns>The resolved GUID.</returns>
    public Guid Resolve(string key, bool resolveCustomName)
    {
        lock (_syncRoot)
        {
            return _nameResolvers.GetOrAdd(key, k =>
            {
                var id = Guid.NewGuid();
                _revertResolvers[id] = k;

                return id;
            });
        }
    }

    /// <summary>
    /// Resolves a key to a GUID for entry purposes, creating a new GUID if the key does not exist.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom names.</param>
    /// <returns>The resolved GUID.</returns>
    public Guid ResolveEntry(string key, bool resolveCustomName)
        => Resolve(key, resolveCustomName);

    /// <summary>
    /// Reverts a GUID to its original key.
    /// </summary>
    /// <param name="id">The GUID to revert.</param>
    /// <returns>The original key if found; otherwise, null.</returns>
    public string RevertResolve(Guid id)
    {
        lock (_syncRoot)
        {
            return _revertResolvers.GetValueSafe(id);
        }
    }

    /// <summary>
    /// Attempts to resolve a key to a GUID.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="id">The resolved GUID if found; otherwise, <see cref="Guid.Empty"/>.</param>
    /// <returns>True if the key was resolved; otherwise, false.</returns>
    public bool TryResolve(string key, out Guid id)
    {
        lock (_syncRoot)
        {
            if (_nameResolvers.TryGetValue(key, out id))
            {
                return true;
            }
        }

        id = Guid.Empty;

        return false;
    }
}