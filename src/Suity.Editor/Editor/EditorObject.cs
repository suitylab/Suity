using Suity.Helpers;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Suity.Editor;

/// <summary>
/// Specifies how an ID should be resolved.
/// </summary>
public enum IdResolveType
{
    /// <summary>
    /// Automatically determine the best resolution method.
    /// </summary>
    Auto,

    /// <summary>
    /// Resolve using the full name of the object.
    /// </summary>
    FullName,

    /// <summary>
    /// Resolve using the last known ID.
    /// </summary>
    LastId,

    /// <summary>
    /// Create a new ID.
    /// </summary>
    New,
}

/// <summary>
/// Event arguments for ID resolution events.
/// </summary>
public class IdResolvedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the IdResolvedEventArgs class.
    /// </summary>
    /// <param name="id">The resolved ID.</param>
    /// <param name="resolveType">The type of resolution used.</param>
    public IdResolvedEventArgs(Guid id, IdResolveType resolveType)
    {
        Id = id;
        ResolveType = resolveType;
    }

    /// <summary>
    /// Gets the resolved ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of ID resolution used.
    /// </summary>
    public IdResolveType ResolveType { get; }
}

/// <summary>
/// Base class for all editor objects that support identification and entry tracking.
/// </summary>
public abstract class EditorObject : Object, INamed, IHasId
{
    private ObjectEntry _entry;
    private Guid _lastId;
    private HashSet<EditorObjectRef> _updateRelationships;
    private readonly QueueOnceAction _delayedUpdateAction;

    /// <summary>
    /// Initializes a new instance of the EditorObject class.
/// </summary>
    public EditorObject()
    {
        LastUpdateTime = DateTime.Now;
        _delayedUpdateAction = new QueueOnceAction(() =>
        {
            if (EditorObjectManager.Instance.IsWatchingDisabled)
            {
                return;
            }

            _entry?.NotifyUpdated(this, DelayedEntryEventArgs.Empty);
        });
    }

    /// <summary>
    /// Gets or sets the unique identifier associated with this editor object.
    /// </summary>
    public Guid Id
    {
        get => _entry?.Id ?? Guid.Empty;
        internal protected set
        {
            if (value == Guid.Empty)
            {
                Entry = null;
            }
            else
            {
                Entry = EditorObjectManager.Instance.EnsureEntry(value);
            }
        }
    }

    /// <summary>
    /// Gets the full name of this object, typically used for identification and display purposes.
    /// </summary>
    public virtual string FullName => GetName();

    /// <summary>
    /// Gets the parent EditorObject in the hierarchy, if any.
    /// </summary>
    public virtual EditorObject Parent => null;

    /// <summary>
    /// Occurs when this object has been updated.
    /// </summary>
    public event EventHandler<EntryEventArgs> ObjectUpdated;

    /// <summary>
    /// Occurs when the ID of this object has been resolved.
    /// </summary>
    public event EventHandler<IdResolvedEventArgs> IdResolved;

    /// <summary>
    /// Occurs when an ID is attached to this object.
    /// </summary>
    public event EventHandler IdAttached;

    /// <summary>
    /// Occurs when an ID is detached from this object.
    /// </summary>
    public event EventHandler IdDetached;

    /// <summary>
    /// Gets or sets the ObjectEntry associated with this editor object.
    /// </summary>
    protected internal ObjectEntry Entry
    {
        get => _entry;
        internal set
        {
            if (_entry == value)
            {
                return;
            }

            ObjectEntry entry = _entry;

            if (entry != null)
            {
                entry.RemoveObject(this);
                InternalOnEntryDetached(entry.Id);
            }

            _entry = value;

            if (value != null)
            {
                _lastId = value.Id;
                // var current = value.Target;

                value.AddObject(this);
                InternalOnEntryAttached(value.Id);
            }
        }
    }

    /// <summary>
    /// Gets whether there is an ID conflict with this object.
    /// </summary>
    public bool IdConflict => _entry?.IdConflict ?? false;

    /// <summary>
    /// Gets the display text for this object, typically used in UI representations.
    /// </summary>
    public virtual string DisplayText => Name;

    /// <summary>
    /// Local non-UTC time
    /// </summary>
    public DateTime LastUpdateTime { get; private set; }

    /// <summary>
    /// Gets whether the ID of this object has been documented. Documented IDs do not need to be recorded in the global cache.
    /// </summary>
    public virtual bool IsIdDocumented => Parent?.IsIdDocumented ?? false;

    /// <summary>
    /// Resolves the ID for this object using the specified resolve type.
    /// </summary>
    /// <param name="resolveType">The type of ID resolution to use.</param>
    /// <returns>True if the ID was successfully resolved; otherwise, false.</returns>
    protected internal bool ResolveId(IdResolveType resolveType = IdResolveType.Auto)
    {
        if (Entry is not null)
        {
            // It seems that OnIdResolved is executed repeatedly here
            // OnIdResolved(current.Id, resolveType);

            return false;
        }

        Guid lastId = _lastId;
        if (lastId == Guid.Empty)
        {
            lastId = OnGetRecordedId();
        }

        var entry = EditorObjectManager.Instance.ResolveEntry(resolveType, FullName, lastId);

        Entry = entry;

        if (entry != null)
        {
            OnIdResolved(entry.Id, resolveType);
            NotifyUpdated(EntryResolvedEventArgs.Instance);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Detaches the ID from this object.
    /// </summary>
    protected internal void DetachId()
    {
        Entry = null;
    }

    /// <summary>
    /// Sets the ID for this object and records it in the global resolver.
    /// </summary>
    /// <param name="id">The GUID to assign to this object.</param>
    protected internal void SetId(Guid id)
    {
        var entry = EditorObjectManager.Instance.EnsureEntry(id);
        Entry = entry;

        string fullName = FullName;
        if (!string.IsNullOrEmpty(fullName))
        {
            GlobalIdResolver.Record(fullName, id);
        }

        if (entry != null)
        {
            OnIdResolved(entry.Id, IdResolveType.FullName);
            NotifyUpdated(EntryResolvedEventArgs.Instance);
        }
    }

    /// <summary>
    /// Gets the storage object associated with this editor object.
    /// </summary>
    /// <param name="tryLoadStorage">Whether to attempt loading the storage if not already loaded.</param>
    /// <returns>The storage object, or null if not available.</returns>
    public virtual object GetStorageObject(bool tryLoadStorage = true) => null;

    /// <summary>
    /// Internally called when an ObjectEntry is attached to this object.
    /// </summary>
    /// <param name="id">The ID of the attached entry.</param>
    internal virtual void InternalOnEntryAttached(Guid id)
    {
        //Debug.WriteLine($"Entry attached : {FullName}");

        if (_updateRelationships != null)
        {
            foreach (var refObj in _updateRelationships)
            {
                refObj.ListenEnabled = true;
            }
        }

        OnIdAttached(id);
    }

    /// <summary>
    /// Internally called when an ObjectEntry is detached from this object.
    /// </summary>
    /// <param name="id">The ID of the detached entry.</param>
    internal virtual void InternalOnEntryDetached(Guid id)
    {
        //Debug.WriteLine($"Entry detached : {FullName}");

        if (_updateRelationships != null)
        {
            foreach (var refObj in _updateRelationships)
            {
                refObj.ListenEnabled = false;
            }
        }

        OnIdDetached(id);
    }

    /// <summary>
    /// Gets the recorded ID for this object, which may be used for persistence or lookup purposes.
    /// </summary>
    public Guid RecordedId => OnGetRecordedId();

    /// <summary>
    /// Gets the recorded ID for this object. Override to provide custom ID recording behavior.
    /// </summary>
    /// <returns>The recorded GUID, or Guid.Empty if none.</returns>
    internal virtual Guid OnGetRecordedId() => Guid.Empty;

    /// <summary>
    /// Called when an ID is attached to this object. Raises the IdAttached event.
    /// </summary>
    /// <param name="id">The attached ID.</param>
    protected virtual void OnIdAttached(Guid id)
    {
        IdAttached?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when an ID is detached from this object. Raises the IdDetached event.
    /// </summary>
    /// <param name="id">The detached ID.</param>
    protected virtual void OnIdDetached(Guid id)
    {
        IdDetached?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the ID has been resolved. Raises the IdResolved event.
    /// </summary>
    /// <param name="id">The resolved ID.</param>
    /// <param name="resolveType">The type of resolution used.</param>
    protected virtual void OnIdResolved(Guid id, IdResolveType resolveType)
    {
        IdResolved?.Invoke(this, new IdResolvedEventArgs(id, resolveType));
    }

    /// <summary>
    /// Notifies that this object has been updated with the specified event arguments.
    /// </summary>
    /// <param name="args">The event arguments describing the update.</param>
    protected internal void NotifyUpdated(EntryEventArgs args)
    {
        LastUpdateTime = DateTime.Now;

        if (EditorObjectManager.Instance.IsWatchingDisabled)
        {
            return;
        }

        _entry?.NotifyUpdated(this, args);
    }

    /// <summary>
    /// Notifies that this object has been updated, using delayed notification.
    /// </summary>
    public void NotifyUpdated()
    {
        LastUpdateTime = DateTime.Now;

        if (EditorObjectManager.Instance.IsWatchingDisabled)
        {
            return;
        }

        _delayedUpdateAction.DoQueuedAction();
    }

    /// <summary>
    /// Notifies that this object has been updated.
    /// </summary>
    /// <param name="delayed">Whether to use delayed notification.</param>
    public void NotifyUpdated(bool delayed)
    {
        LastUpdateTime = DateTime.Now;

        if (EditorObjectManager.Instance.IsWatchingDisabled)
        {
            return;
        }

        if (delayed)
        {
            _delayedUpdateAction.DoQueuedAction();
        }
        else
        {
            _entry?.NotifyUpdated(this, EntryEventArgs.Empty);
        }
    }

    /// <summary>
    /// Notifies that a property on this object has been updated.
    /// </summary>
    /// <param name="propertyName">The name of the property that was updated.</param>
    protected internal void NotifyPropertyUpdated([CallerMemberName] string propertyName = null)
    {
        LastUpdateTime = DateTime.Now;

        if (EditorObjectManager.Instance.IsWatchingDisabled)
        {
            return;
        }

        var entry = _entry;
        if (entry != null)
        {
            EntryEventArgs args = new PropertyEntryEventArgs(propertyName);
            entry.NotifyUpdated(this, args);
        }
    }

    /// <summary>
    /// Internally raises the ObjectUpdated event with the specified event arguments.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    internal virtual void InternalRaiseObjectUpdated(EntryEventArgs args)
    {
        ObjectUpdated?.Invoke(this, args);
        OnUpdated(args);
    }

    /// <summary>
    /// Called when this object has been updated. Override to handle update events.
    /// </summary>
    /// <param name="args">The event arguments describing the update.</param>
    protected virtual void OnUpdated(EntryEventArgs args)
    {
    }

    #region Build Update Relationship

    /// <summary>
    /// Adds an update relationship with another editor object. When the referenced object is updated, this object will also be notified.
    /// </summary>
    /// <param name="editorObjectRef">The reference to the editor object to watch.</param>
    protected void AddUpdateRelationship(EditorObjectRef editorObjectRef)
    {
        if (editorObjectRef is null)
        {
            throw new ArgumentNullException(nameof(editorObjectRef));
        }

        if ((_updateRelationships ??= []).Add(editorObjectRef))
        {
            editorObjectRef.TargetUpdated += RefUpdated;
            editorObjectRef.ListenEnabled = _entry != null;
        }
    }

    /// <summary>
    /// Removes an update relationship with the specified editor object.
    /// </summary>
    /// <param name="editorObjectRef">The reference to the editor object to remove.</param>
    protected void RemoveUpdateRelationship(EditorAssetRef editorObjectRef)
    {
        if (editorObjectRef is null)
        {
            return;
        }

        if (_updateRelationships is null)
        {
            return;
        }

        if (_updateRelationships.Remove(editorObjectRef))
        {
            editorObjectRef.TargetUpdated -= RefUpdated;
            editorObjectRef.ListenEnabled = false;

            if (_updateRelationships.Count == 0)
            {
                _updateRelationships = null;
            }
        }
    }

    /// <summary>
    /// Called when a related editor object has been updated. Override to handle relationship update events.
    /// </summary>
    /// <param name="obj">The editor object that was updated.</param>
    /// <param name="e">The event arguments.</param>
    /// <param name="handled">Set to true to indicate the event has been handled.</param>
    protected virtual void OnRelationshipUpdated(EditorObject obj, EntryEventArgs e, ref bool handled)
    {
        NotifyUpdated(false);
    }

    private void RefUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        EditorObjectRef objRef = (EditorObjectRef)sender;
        OnRelationshipUpdated(objRef.Target, e, ref handled);
    }

    #endregion
}