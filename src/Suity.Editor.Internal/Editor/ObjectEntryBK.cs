using Suity.Collections;
using Suity.Editor.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// A multiple item specialization for editor objects that logs ID conflict warnings.
/// </summary>
internal class ObjectMultipleItem : MultipleItem<EditorObject>
{
    /// <inheritdoc/>
    protected override void OnMultipleValue(EditorObject value)
    {
        //var item = new ObjectLogCoreItem($"Id conflict detected: {value}", value.Id);
        //Logs.LogWarning(item);
        EditorServices.SystemLog?.AddLog($"Id conflict detected: {value.Id} - {value.Name}");
    }
}

/// <summary>
/// Internal implementation of <see cref="ObjectEntry"/> that manages editor object identity,
/// handles object addition/removal, listener notification, and conflict detection.
/// </summary>
internal sealed class ObjectEntryBK : ObjectEntry
{
    private const int MaxEventArgsStack = 50;

    private static readonly ConcurrentPool<HashSet<IEditorObjectListener>> _listenerPool
        = new(() => []);

    private static readonly ConcurrentPool<List<IEditorObjectListener>> _notifyPool
        = new(() => []);

    internal Guid _id;
    internal readonly ObjectMultipleItem _ref = new();
    private HashSet<IEditorObjectListener> _listeners;
    private bool _locked;

    /// <summary>
    /// Initializes a new instance without a predefined ID.
    /// </summary>
    internal ObjectEntryBK()
    { }

    /// <summary>
    /// Initializes a new instance with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for this entry.</param>
    internal ObjectEntryBK(Guid id)
    { _id = id; }

    /// <inheritdoc/>
    public override Guid Id => _id;

    /// <inheritdoc/>
    public override EditorObject Target => _ref.Value;
    /// <inheritdoc/>
    public override bool IdConflict => _ref.Count > 1;

    /// <inheritdoc/>
    internal override IEnumerable<EditorObject> Targets => _ref.Values;
    /// <inheritdoc/>
    internal override int TargetCount => _ref.Count;

    /// <inheritdoc/>
    public override bool Locked => _locked;

    /// <summary>
    /// Notifies listeners of an update to the editor object. Supports delayed or immediate notification.
    /// </summary>
    /// <param name="obj">The editor object that was updated.</param>
    /// <param name="args">Event arguments describing the update.</param>
    internal override void NotifyUpdated(EditorObject obj, EntryEventArgs args)
    {
        if (EditorObjectManager.Instance.IsWatchingDisabled)
        {
            return;
        }

        if (args.StackCount > MaxEventArgsStack)
        {
            Logs.LogWarning($"EntryEventArgs stack count reached limit number : {MaxEventArgsStack}");
            return;
        }

        if (args is null)
        {
            return;
        }

        if (args.Delayed)
        {
            EditorUtility.AddDelayedAction(new DelayedUpdateAction(obj, args));
        }
        else
        {
            QueuedAction.Do(() => InternalNotifyUpdated(obj, args));
        }
    }

    /// <inheritdoc/>
    public override void AddListener(IEditorObjectListener listener)
    {
        if (listener is null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        lock (_ref)
        {
            (_listeners ??= _listenerPool.Acquire()).Add(listener);
        }
    }

    /// <inheritdoc/>
    public override void RemoveListener(IEditorObjectListener listener)
    {
        if (listener is null)
        {
            return;
        }

        lock (_ref)
        {
            if (_listeners != null)
            {
                _listeners.Remove(listener);

                if (_listeners.Count == 0)
                {
                    var pool = _listeners;
                    _listeners = null;
                    _listenerPool.Release(pool);
                }
            }
        }
    }

    /// <summary>
    /// Removes an editor object from this entry and notifies listeners of the replacement.
    /// </summary>
    /// <param name="obj">The object to remove.</param>
    /// <returns>True if the object was removed; otherwise, false.</returns>
    internal override bool RemoveObject(EditorObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (_locked)
        {
            return false;
        }

        bool removed;
        EditorObject result = null;

        lock (_ref)
        {
            removed = _ref.Remove(obj);
            if (removed)
            {
                result = _ref.Value;
            }
        }

        if (removed)
        {
            InternalNotifyUpdated(result, new ReplaceEntryEventArgs(obj, result));
        }

        return removed;
    }

    /// <summary>
    /// Adds an editor object to this entry and notifies listeners of the replacement.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    /// <returns>True if the object was added; otherwise, false.</returns>
    internal override bool AddObject(EditorObject obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (_locked)
        {
            return false;
        }

        bool added;
        EditorObject last;

        lock (_ref)
        {
            last = _ref.Value;
            added = _ref.Add(obj);
        }

        if (added)
        {
            InternalNotifyUpdated(last, new ReplaceEntryEventArgs(last, obj));
        }

        return added;
    }

    /// <summary>
    /// Gets all reference hosts registered as listeners on this entry.
    /// </summary>
    /// <returns>An array of reference hosts, or an empty array if none exist.</returns>
    internal override IReferenceHost[] GetReferenceHosts()
    {
        lock (_ref)
        {
            return _listeners?.OfType<IReferenceHost>().ToArray() ?? [];
        }
    }

    // [DebuggerHidden]
    /// <summary>
    /// Performs the actual notification to all listeners about an object update.
    /// Triggers reference manager update and invokes each listener's HandleObjectUpdate.
    /// </summary>
    /// <param name="obj">The editor object that was updated.</param>
    /// <param name="args">Event arguments describing the update.</param>
    internal override void InternalNotifyUpdated(EditorObject obj, EntryEventArgs args)
    {
        ReferenceManager.Current.Update();

        Guid id = Id;
        EditorObject target = Target;

        List<IEditorObjectListener> listeners = null;

        lock (_ref)
        {
            if (_listeners != null)
            {
                listeners = _notifyPool.Acquire();
                listeners.Clear();
                listeners.AddRange(_listeners);
            }
        }

        if (listeners != null)
        {
            bool handled = false;
            foreach (var listner in listeners)
            {
                try
                {
                    listner.HandleObjectUpdate(id, target, args, ref handled);
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            listeners.Clear();
            _notifyPool.Release(listeners);
        }

        obj?.InternalRaiseObjectUpdated(args);

        if (EditorPlugin.RuntimeLogging)
        {
            EditorServices.SystemLog.AddLog($"ObjectUpdate-{Target?.Name ?? Id.ToString()} : {args}");
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Id.ToString();
    }

    #region DelayedUpdateAction

    private class DelayedUpdateAction : DelayedAction<EditorObject, EntryEventArgs>
    {
        public DelayedUpdateAction(EditorObject obj, EntryEventArgs args) : base(obj, args)
        {
        }

        public override void DoAction()
        {
            Value1.Entry?.InternalNotifyUpdated(Value1, Value2);
        }
    }

    #endregion
}