using Suity.Collections;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Abstract base class for managing synchronization reference hosts. Provides reference tracking,
/// dirty marking, synchronization dispatch, and navigation support for editor objects.
/// </summary>
public abstract class SyncReferenceHost : IReferenceHost, INavigable
{
    private static readonly ConcurrentPool<Stack<object>> _stackPool = new(() => new());

    /// <summary>
    /// Gets the target object referenced by this host.
    /// </summary>
    public abstract object Target { get; }

    /// <summary>
    /// Gets or loads the target object, ensuring it is available for operations.
    /// </summary>
    /// <returns>The target object, or null if it cannot be loaded.</returns>
    public virtual object GetOrLoadTarget() => Target;

    /// <summary>
    /// Marks this reference host as dirty, signaling that its data needs to be re-synchronized.
    /// </summary>
    public void MarkDirty()
    {
        ReferenceManager.Current.MarkDirty(this);
    }

    /// <summary>
    /// Removes this reference host from the reference manager.
    /// </summary>
    public void Remove()
    {
        ReferenceManager.Current.Remove(this);
    }

    /// <summary>
    /// Synchronizes references along the given path by visiting all <see cref="IReference"/> objects
    /// in the target and dispatching the sync operation.
    /// </summary>
    /// <param name="path">The synchronization path to traverse.</param>
    /// <param name="sync">The synchronization handler to apply.</param>
    public void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        // Force open document
        object obj = GetOrLoadTarget();
        if (obj is null)
        {
            return;
        }

        Visitor.Visit<IReference>(obj, (referencer, pathContext) =>
        {
            try
            {
                referencer.ReferenceSync(pathContext.GetPath(), sync);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        });
    }

    /// <summary>
    /// Handles object update events by finding all matching references and notifying listeners
    /// along the reference path using a bubble-up dispatch strategy.
    /// </summary>
    /// <param name="id">The unique identifier of the updated object.</param>
    /// <param name="obj">The editor object that was updated.</param>
    /// <param name="args">Event arguments describing the update.</param>
    /// <param name="handled">Reference to a flag indicating whether the event has been handled.</param>
    public void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    {
        foreach (var item in ReferenceManager.Current.FindReference(this, id))
        {
            bool itemHandled = false;

            NotifyItem(id, obj, args, item, ref itemHandled);

            if (itemHandled)
            {
                handled = true;
            }
        }
    }

    /// <summary>
    /// Notifies listeners along the synchronization path by traversing from the target object
    /// down to the specific item, then bubbling events back up to the root.
    /// </summary>
    /// <param name="id">The unique identifier of the updated object.</param>
    /// <param name="entryObject">The editor object that was updated.</param>
    /// <param name="args">Event arguments describing the update.</param>
    /// <param name="item">The path report item describing the reference location.</param>
    /// <param name="handled">Reference to a flag indicating whether the event has been handled.</param>
    private void NotifyItem(Guid id, EditorObject entryObject, EntryEventArgs args, SyncPathReportItem item, ref bool handled)
    {
        object obj = Target;
        if (obj is null)
        {
            return;
        }

        Stack<object> stack = _stackPool.Acquire();

        try
        {
            stack.Clear();

            stack.Push(obj);

            SyncPath path = item.Path;
            for (int i = 0; i < path.Length; i++)
            {
                obj = path[i] switch
                {
                    string name => Member.GetProperty(obj, name),
                    int index => Member.GetItem(obj, index),
                    _ => null,
                };

                if (obj != null)
                {
                    stack.Push(obj);
                }
                else
                {
                    break;
                }
            }

            while (stack.Count > 0)
            {
                // Bubble event dispatch
                (stack.Pop() as IEditorObjectListener)?.HandleObjectUpdate(id, entryObject, args, ref handled);
                if (handled)
                {
                    break;
                }
            }
        }
        finally
        {
            _stackPool.Release(stack);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Target?.ToString() ?? base.ToString();
    }

    /// <inheritdoc/>
    public virtual object GetNavigationTarget()
    {
        return Target;
    }
}

/// <summary>
/// Reference host that holds a strong reference to the target object.
/// The target will not be garbage collected while this host is alive.
/// </summary>
public class ObjectReferenceHost(object target) : SyncReferenceHost
{
    private readonly object _target = target ?? throw new ArgumentNullException(nameof(target));

    /// <inheritdoc/>
    public override object Target => _target;
}

/// <summary>
/// Reference host that holds a weak reference to the target object.
/// Allows the target to be garbage collected if no other strong references exist.
/// </summary>
public class WeakReferenceHost : SyncReferenceHost
{
    private readonly WeakReference<object> _targetRef;

    /// <summary>
    /// Initializes a new instance with a weak reference to the specified target.
    /// </summary>
    /// <param name="target">The target object to weakly reference.</param>
    public WeakReferenceHost(object target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        _targetRef = new WeakReference<object>(target);
    }

    /// <inheritdoc/>
    public override object Target
    {
        get
        {
            if (_targetRef.TryGetTarget(out object owner))
            {
                return owner;
            }
            else
            {
                return null;
            }
        }
    }
}

/// <summary>
/// Reference host that resolves its target by asset ID through the <see cref="AssetManager"/>.
/// The target is loaded on demand and can change as assets are loaded or unloaded.
/// </summary>
public class AssetReferenceHost(Guid id) : SyncReferenceHost
{
    private Guid _id = id;

    /// <inheritdoc/>
    public override object Target => AssetManager.Instance.GetAsset(_id);

    /// <inheritdoc/>
    public override object GetNavigationTarget() => _id;

    /// <inheritdoc/>
    public override string ToString() => Target?.ToString() ?? _id.ToString();
}