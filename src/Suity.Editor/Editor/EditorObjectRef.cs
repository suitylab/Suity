using System;

namespace Suity.Editor;

/// <summary>
/// Delegate for handling editor object events with a handled flag.
/// </summary>
/// <typeparam name="TEventArgs">The type of event arguments.</typeparam>
public delegate void EditorObjectEventHandler<TEventArgs>(object sender, TEventArgs e, ref bool handled);

/// <summary>
/// Abstract base class for referencing editor objects with event handling capabilities.
/// </summary>
public abstract class EditorObjectRef : IEditorObjectListener, IHasId
{
    private Guid _id;
    private ObjectEntry _entry;
    private bool _listen;

    /// <summary>
    /// Gets or sets the unique identifier of the referenced editor object.
    /// </summary>
    public Guid Id
    {
        get => _entry?.Id ?? _id;
        set
        {
            if (_id == value)
            {
                return;
            }

            _id = value;

            if (value != Guid.Empty)
            {
                SetEntry(EditorObjectManager.Instance.EnsureEntry(value));
            }
            else
            {
                SetEntry(null);
            }
        }
    }

    /// <summary>
    /// Gets or sets the target editor object.
    /// </summary>
    public EditorObject Target
    {
        get
        {
            if (_entry is { } entry)
            {
                return entry.Target;
            }
            else if (_id != Guid.Empty)
            {
                // Should not enter this process
                SetEntry(EditorObjectManager.Instance.EnsureEntry(_id));
                return _entry?.Target;
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (value != null)
            {
                if (_id != value.Id)
                {
                    _id = value.Id;
                    SetEntry(value.Entry);
                }
            }
            else
            {
                if (_id != Guid.Empty)
                {
                    _id = Guid.Empty;
                    SetEntry(null);
                }
            }
        }
    }


    /// <summary>
    /// Gets or sets whether event listening is enabled.
    /// </summary>
    public bool ListenEnabled
    {
        get => _listen;
        set
        {
            _listen = value;

            var entry = _entry;
            if (entry != null)
            {
                if (_listen)
                {
                    entry.AddListener(this);
                }
                else
                {
                    entry.RemoveListener(this);
                }
            }
        }
    }

    /// <summary>
    /// Event raised when the target object is updated.
    /// </summary>
    public event EditorObjectEventHandler<EntryEventArgs> TargetUpdated;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef"/> class.
    /// </summary>
    public EditorObjectRef()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef"/> class with the specified id.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public EditorObjectRef(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef"/> class with the specified target.
    /// </summary>
    /// <param name="target">The editor object to reference.</param>
    public EditorObjectRef(EditorObject target)
    {
        Id = target?.Id ?? Guid.Empty;
    }

    /// <summary>
    /// Resets the reference to empty.
    /// </summary>
    public void Reset()
    {
        Id = Guid.Empty;
    }

    private void SetEntry(ObjectEntry value)
    {
        // Don't make pre-judgment, make sure the Listener is refreshed once
        //if (_entry == value)
        //{
        //    return;
        //}

        var oldObj = _entry?.Target;

        _entry?.RemoveListener(this);
        _entry = value;

        if (_entry != null && _listen)
        {
            _entry.AddListener(this);
        }

        var id = Id;
        var obj = _entry?.Target;
        var eventArgs = new ReplaceEntryEventArgs(oldObj, obj);
        bool handled = false;

        HandleObjectUpdate(id, obj, eventArgs, ref handled);
    }

    /// <summary>
    /// Returns a string representation of this reference.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => _id.ToName();

    /// <summary>
    /// Handles object update events.
    /// </summary>
    /// <param name="id">The object id.</param>
    /// <param name="obj">The editor object.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="handled">Flag indicating if the event was handled.</param>
    public virtual void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    {
        TargetUpdated?.Invoke(this, args, ref handled);
    }
}

/// <summary>
/// Generic base class for strongly-typed editor object references.
/// </summary>
/// <typeparam name="T">The type of editor object.</typeparam>
public class EditorObjectRef<T> : EditorObjectRef 
    where T : EditorObject
{
    /// <summary>
    /// Gets or sets the target editor object as the specified type.
    /// </summary>
    public virtual new T Target
    {
        get => base.Target as T;
        set
        {
            base.Target = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef{T}"/> class.
    /// </summary>
    public EditorObjectRef()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef{T}"/> class with the specified id.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public EditorObjectRef(Guid id)
        : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectRef{T}"/> class with the specified target.
    /// </summary>
    /// <param name="target">The editor object to reference.</param>
    public EditorObjectRef(T target)
    {
        Id = target?.Id ?? Guid.Empty;
    }
}