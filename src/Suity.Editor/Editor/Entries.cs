using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Listener interface for editor object changes.
/// </summary>
public interface IEditorObjectListener
{
    /// <summary>
    /// Called when an editor object is updated.
    /// </summary>
    /// <param name="id">The unique identifier of the object.</param>
    /// <param name="obj">The editor object that was updated.</param>
    /// <param name="args">Event arguments containing update details.</param>
    /// <param name="handled">Flag indicating whether the event has been handled.</param>
    void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled);
}

/// <summary>
/// Base class for object entries in the editor system.
/// </summary>
public abstract class ObjectEntry
{
    internal ObjectEntry()
    { }

    /// <summary>
    /// Gets the unique identifier of this entry.
    /// </summary>
    public abstract Guid Id { get; }

    /// <summary>
    /// Gets the primary target editor object.
    /// </summary>
    public abstract EditorObject Target { get; }

    /// <summary>
    /// Gets whether there is an ID conflict with another entry.
    /// </summary>
    public abstract bool IdConflict { get; }

    internal abstract IEnumerable<EditorObject> Targets { get; }
    internal abstract int TargetCount { get; }

    /// <summary>
    /// Indicates whether this identity is locked. If locked, this identity cannot be added or abnormal editor objects.
    /// </summary>
    public abstract bool Locked { get; }

    internal abstract void NotifyUpdated(EditorObject obj, EntryEventArgs args);

    /// <summary>
    /// Adds a listener to receive object update notifications.
    /// </summary>
    /// <param name="listener">The listener to add.</param>
    public abstract void AddListener(IEditorObjectListener listener);

    /// <summary>
    /// Removes a listener from receiving object update notifications.
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    public abstract void RemoveListener(IEditorObjectListener listener);

    internal abstract bool RemoveObject(EditorObject obj);

    internal abstract bool AddObject(EditorObject obj);

    internal abstract IReferenceHost[] GetReferenceHosts();

    // [DebuggerHidden]
    internal abstract void InternalNotifyUpdated(EditorObject obj, EntryEventArgs args);
}

/// <summary>
/// Asset key entry.
/// </summary>
public abstract class AssetKeyEntry
{
    internal AssetKeyEntry()
    { }

    /// <summary>
    /// Gets the primary target asset.
    /// </summary>
    public abstract Asset Target { get; }
    internal abstract IEnumerable<Asset> Targets { get; }
    internal abstract int TargetCount { get; }

    /// <summary>
    /// Gets whether there is an asset key conflict with another entry.
    /// </summary>
    public abstract bool AssetKeyConflict { get; }

    /// <summary>
    /// Gets the target asset filtered by the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The filtered target asset.</returns>
    public abstract Asset GetTarget(IAssetFilter filter);

    internal abstract void Add(Asset value);

    internal abstract bool Remove(Asset value);
}