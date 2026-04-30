using Suity.Collections;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

#region ReferenceItem

/// <summary>
/// Represents a single reference item with a sync path, target ID, and associated message.
/// </summary>
internal class ReferenceItem(SyncPath path, Guid id, string message)
{
    /// <summary>
    /// The synchronization path where the reference was found.
    /// </summary>
    public readonly SyncPath Path = path ?? SyncPath.Empty;

    /// <summary>
    /// The target GUID identifier being referenced.
    /// </summary>
    public readonly Guid Id = id;

    /// <summary>
    /// An optional message associated with this reference.
    /// </summary>
    public readonly string Message = message;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Path.GetHashCode() ^ Id.GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (Object.ReferenceEquals(this, obj))
        {
            return true;
        }
        ReferenceItem other = obj as ReferenceItem;
        if (Equals(other, null))
        {
            return false;
        }
        return Path == other.Path && Id == other.Id;
    }

    /// <summary>
    /// Equality operator for ReferenceItem.
    /// </summary>
    public static bool operator ==(ReferenceItem v1, ReferenceItem v2)
    {
        if (Equals(v1, null)) return Equals(v2, null); else return v1.Equals(v2);
    }

    /// <summary>
    /// Inequality operator for ReferenceItem.
    /// </summary>
    public static bool operator !=(ReferenceItem v1, ReferenceItem v2)
    {
        if (Equals(v1, null)) return !Equals(v2, null); else return !v1.Equals(v2);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Id} {Path}";
    }
}

#endregion

#region ReferenceBuildSync

/// <summary>
/// Synchronization mode that builds a complete reference index for a host.
/// </summary>
internal class ReferenceBuildSync(IReferenceHost host) : IReferenceSync
{
    private readonly IReferenceHost _host = host;
    private readonly HashSet<ReferenceItem> _allItems = [];
    private readonly Dictionary<Guid, HashSet<ReferenceItem>> _guids = [];

    /// <inheritdoc/>
    public IReferenceHost Host => _host;

    /// <summary>
    /// Gets all unique GUIDs referenced by this host.
    /// </summary>
    public IEnumerable<Guid> Ids => _guids.Keys;

    /// <summary>
    /// Gets all reference items for a specific GUID.
    /// </summary>
    /// <param name="id">The GUID to look up.</param>
    /// <returns>Collection of reference items for the given GUID.</returns>
    public IEnumerable<ReferenceItem> GetReferenceItems(Guid id)
    {
        return _guids.GetValueSafe(id) ?? (IEnumerable<ReferenceItem>)[];
    }

    /// <summary>
    /// Gets the count of reference items for a specific GUID.
    /// </summary>
    /// <param name="id">The GUID to count references for.</param>
    /// <returns>Number of reference items.</returns>
    public int GetReferenceItemCount(Guid id)
    {
        return _guids.GetValueSafe(id)?.Count ?? 0;
    }

    /// <summary>
    /// Gets all reference items across all GUIDs.
    /// </summary>
    public IEnumerable<ReferenceItem> AllItems => _guids.Keys.SelectMany(o => _guids[o]);

    /// <inheritdoc/>
    public ReferenceSyncMode Mode => ReferenceSyncMode.Build;

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public Guid OldId => Guid.Empty;

    /// <inheritdoc/>
    public Guid SyncId(SyncPath path, Guid id, string message)
    {
        var item = new ReferenceItem(path, id, message);

        if (_allItems.Add(item))
        {
            _guids.GetOrAdd(item.Id, _ => []).Add(item);
        }

        return id;
    }

    /// <summary>
    /// Clears all collected reference items.
    /// </summary>
    public void Clear()
    {
        _allItems.Clear();
        _guids.Clear();
    }
}

#endregion

#region ReferenceFindSync

/// <summary>
/// Synchronization mode that finds all locations where a specific GUID is referenced.
/// </summary>
internal class ReferenceFindSync(object owner, Guid find) : IReferenceSync
{
    private readonly object _owner = owner;
    private readonly Guid _id = find;
    private readonly List<SyncPathReportItem> _results = [];

    /// <summary>
    /// Gets the collected results of reference locations.
    /// </summary>
    public IEnumerable<SyncPathReportItem> Results => _results;

    /// <inheritdoc/>
    public ReferenceSyncMode Mode => ReferenceSyncMode.Find;

    /// <inheritdoc/>
    public Guid Id => _id;

    /// <inheritdoc/>
    public Guid OldId => Guid.Empty;

    /// <inheritdoc/>
    public Guid SyncId(SyncPath path, Guid id, string message)
    {
        if (_id == id)
        {
            _results.Add(new SyncPathReportItem(_owner, path));
        }

        return id;
    }
}

#endregion

#region ReferenceRedirectSync

/// <summary>
/// Synchronization mode that redirects references from one GUID to another.
/// </summary>
internal class ReferenceRedirectSync(Guid oldId, Guid newId) : IReferenceSync
{
    /// <summary>
    /// Delegate invoked when a reference is changed during redirection.
    /// </summary>
    /// <param name="path">The sync path where the change occurred.</param>
    /// <param name="id">The old GUID that was replaced.</param>
    /// <param name="message">Optional message about the change.</param>
    public delegate void ChangeCallBackDelegate(SyncPath path, Guid id, string message);

    private readonly Guid _oldId = oldId;
    private readonly Guid _newId = newId;

    private ChangeCallBackDelegate _callBack;

    /// <summary>
    /// Gets or sets the callback invoked when a reference is redirected.
    /// </summary>
    public ChangeCallBackDelegate ChangeCallBack
    {
        get => _callBack;
        set => _callBack = value;
    }

    /// <inheritdoc/>
    public ReferenceSyncMode Mode => ReferenceSyncMode.Redirect;

    /// <inheritdoc/>
    public Guid Id => _newId;

    /// <inheritdoc/>
    public Guid OldId => _oldId;

    /// <inheritdoc/>
    public Guid SyncId(SyncPath path, Guid id, string message)
    {
        if (id == _oldId)
        {
            _callBack?.Invoke(path, id, message);

            return _newId;
        }
        else
        {
            return id;
        }
    }
}

#endregion

#region ReferenceFindMissingSync

/// <summary>
/// Synchronization mode that collects all referenced GUIDs for dependency analysis.
/// </summary>
internal class ReferenceCollectSync(Action<SyncPath, Guid> collect) : IReferenceSync
{
    private readonly Action<SyncPath, Guid> _collect = collect ?? throw new ArgumentNullException(nameof(collect));

    /// <inheritdoc/>
    public ReferenceSyncMode Mode => ReferenceSyncMode.Find;

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public Guid OldId => Guid.Empty;

    /// <inheritdoc/>
    public Guid SyncId(SyncPath path, Guid id, string message)
    {
        _collect?.Invoke(path, id);
        return id;
    }
}

#endregion
