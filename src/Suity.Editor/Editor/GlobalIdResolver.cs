using Suity.Collections;
using Suity.Editor.Types;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Provides global resolution of string keys to unique GUID identifiers and vice versa.
/// Maintains a registry of name-to-GUID mappings that can be recorded, resolved, and reverted.
/// </summary>
public static class GlobalIdResolver
{
    private static readonly Dictionary<string, Guid> _nameResolvers = [];

    private static readonly HashSet<string> _idLostToReports = [];
    private static readonly QueueOnceAction _lostWarningAction = new(_ReportIdLost);

    private static IObjectIdResolver _current;

    /// <summary>
    /// Gets the current object ID resolver used for resolving names to GUIDs.
    /// </summary>
    public static IObjectIdResolver Current
    {
        get => _current;
        internal set
        {
            lock (_nameResolvers)
            {
                if (_current == value)
                {
                    return;
                }

                _current = value;

                if (_current != null)
                {
                    foreach (var pair in _nameResolvers)
                    {
                        _current.Record(pair.Key, pair.Value);
                    }
                    _nameResolvers.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Generates a new unique GUID using the current resolver, or creates a random GUID if no resolver is active.
    /// </summary>
    public static Guid NewGuid()
    {
        return _current?.NewGuid() ?? Guid.NewGuid();
    }

    /// <summary>
    /// Resolves a string key to a GUID identifier.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom resource names.</param>
    /// <returns>The resolved GUID, or Guid.Empty if resolution fails.</returns>
    public static Guid Resolve(string key, bool resolveCustomName = true)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Guid.Empty;
        }

        if (_current != null)
        {
            return _current.Resolve(key, resolveCustomName);
        }

        Guid id;

        // Normally this part is not executed

        if (AssetManager.Instance is AssetManager assetManager)
        {
            id = assetManager.GetAsset(key)?.Id ?? Guid.Empty;
            if (id != Guid.Empty)
            {
                return id;
            }

            if (resolveCustomName)
            {
                id = assetManager.GetAssetByResourceName(key)?.Id ?? Guid.Empty;
                if (id != Guid.Empty)
                {
                    return id;
                }
            }
        }

        if (Guid.TryParseExact(key, "D", out id))
        {
            return id;
        }

        lock (_nameResolvers)
        {
            id = _nameResolvers.GetOrAdd(key, _ => Guid.NewGuid());
        }

        return id;
    }

    /// <summary>
    /// Attempts to resolve an object value to a GUID identifier.
    /// </summary>
    /// <param name="value">The object to resolve (can be a Guid or string).</param>
    /// <param name="id">When successful, contains the resolved GUID; otherwise Guid.Empty.</param>
    /// <returns>True if resolution succeeded; otherwise false.</returns>
    public static bool TryResolve(object value, out Guid id)
    {
        if (value is null)
        {
            id = Guid.Empty;
            return false;
        }

        if (value is Guid valueId)
        {
            id = valueId;
            return true;
        }

        return TryResolve(value.ToString(), out id);
    }

    /// <summary>
    /// Attempts to resolve a string key to a GUID identifier.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="id">When successful, contains the resolved GUID; otherwise Guid.Empty.</param>
    /// <returns>True if resolution succeeded; otherwise false.</returns>
    public static bool TryResolve(string key, out Guid id)
    {
        if (string.IsNullOrEmpty(key))
        {
            id = Guid.Empty;
            return false;
        }

        if (_current != null)
        {
            return _current.TryResolve(key, out id);
        }

        if (AssetManager.Instance is AssetManager assetManager)
        {
            // Resolve asset key
            id = assetManager.GetAsset(key)?.Id ?? Guid.Empty;
            if (id != Guid.Empty)
            {
                return true;
            }

            // Resolve data Id
            id = assetManager.GetAssetByResourceName(key)?.Id ?? Guid.Empty;
            if (id != Guid.Empty)
            {
                return true;
            }

            // Resolves native types, including short names
            if (NativeTypes.GetNativeDType(key) is DType dtype)
            {
                id = dtype.Id;
                return true;
            }
        }

        if (Guid.TryParseExact(key, "D", out id))
        {
            return true;
        }

        id = Guid.Empty;

        return false;
    }

    /// <summary>
    /// Reverts a GUID to its original string key using the current resolver.
    /// </summary>
    /// <param name="id">The GUID to revert.</param>
    /// <returns>The original string key, or null if not found.</returns>
    public static string RevertResolve(Guid id)
    {
        return _current?.RevertResolve(id);
    }

    /// <summary>
    /// Records a key-to-GUID mapping in the current resolver or queues it for later registration.
    /// </summary>
    /// <param name="key">The string key to record.</param>
    /// <param name="id">The GUID identifier to associate with the key.</param>
    /// <param name="reverseRecord">Reverse records GUID-to-key mapping</param>
    internal static void Record(string key, Guid id, bool reverse = true)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (id == Guid.Empty)
        {
            return;
        }

        if (_current != null)
        {
            _current.Record(key, id, reverse);
            return;
        }

        lock (_nameResolvers)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _nameResolvers[key] = id;
            }
        }
    }

    /// <summary>
    /// Renames a recorded key to a new key while preserving its associated GUID.
    /// </summary>
    /// <param name="key">The original key.</param>
    /// <param name="newKey">The new key to assign.</param>
    internal static void Rename(string key, string newKey)
    {
        if (_current != null)
        {
            _current.Rename(key, newKey);
            return;
        }

        if (key is null || newKey is null)
        {
            return;
        }

        lock (_nameResolvers)
        {
            if (_nameResolvers.TryGetValue(key, out Guid id))
            {
                _nameResolvers[newKey] = id;
            }
        }
    }

    /// <summary>
    /// Resolves a key to an ObjectEntry, creating a new entry if one doesn't exist.
    /// </summary>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom resource names.</param>
    /// <returns>The resolved ObjectEntry.</returns>
    internal static ObjectEntry ResolveEntry(string key, bool resolveCustomName = false)
    {
        Guid id = _current?.ResolveEntry(key, resolveCustomName) ?? Guid.Empty;

        if (id == Guid.Empty)
        {
            id = AssetManager.Instance.GetAsset(key)?.Id ?? Guid.Empty;
        }

        if (id != Guid.Empty)
        {
            return EditorObjectManager.Instance.EnsureEntry(id);
        }
        else
        {
            return EditorObjectManager.Instance.NewEntry();
        }
    }

    /// <summary>
    /// Resolves a key to an ObjectEntry using a local resolver first, then falling back to the default resolution.
    /// </summary>
    /// <param name="localResolver">The local resolver to use first.</param>
    /// <param name="key">The string key to resolve.</param>
    /// <param name="resolveCustomName">Whether to resolve custom resource names.</param>
    /// <returns>The resolved ObjectEntry.</returns>
    internal static ObjectEntry ResolveEntry(IObjectIdResolver localResolver, string key, bool resolveCustomName = false)
    {
        if (localResolver != null)
        {
            Guid id = localResolver.Resolve(key, resolveCustomName);
            if (id != Guid.Empty)
            {
                return EditorObjectManager.Instance.EnsureEntry(id);
            }
        }

        return ResolveEntry(key, resolveCustomName);
    }

    /// <summary>
    /// Fixes a potentially stale GUID by reverting and re-resolving it.
    /// </summary>
    /// <param name="id">The GUID to fix.</param>
    /// <returns>The re-resolved GUID, or Guid.Empty if the key is no longer valid.</returns>
    internal static Guid FixId(Guid id)
    {
        string key = RevertResolve(id);
        if (!string.IsNullOrEmpty(key))
        {
            return Resolve(key);
        }
        else
        {
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Reports that a GUID has lost its associated key mapping.
    /// </summary>
    /// <param name="id">The GUID that lost its mapping.</param>
    internal static void ReportIdLost(Guid id)
    {
        string key = RevertResolve(id);
        if (!string.IsNullOrEmpty(key))
        {
            lock (_idLostToReports)
            {
                _idLostToReports.Add(key);
                _lostWarningAction.DoQueuedAction();
            }
        }
    }

    /// <summary>
    /// Logs warnings for all IDs that have been reported as lost.
    /// </summary>
    internal static void _ReportIdLost()
    {
        string[] keys = null;

        lock (_idLostToReports)
        {
            keys = _idLostToReports.OrderBy(o => o).ToArray();
            _idLostToReports.Clear();
        }

        if (keys != null)
        {
            foreach (var key in keys)
            {
                Logs.LogWarning($"Id lost : {key}");
            }
        }
    }
}