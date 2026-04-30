using Suity.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Internal singleton implementation of <see cref="EditorObjectManager"/> that manages
/// editor object entries, object creation, entry resolution, and field collection creation.
/// </summary>
internal class EditorObjectManagerBK : EditorObjectManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="EditorObjectManagerBK"/>.
    /// </summary>
    public new static readonly EditorObjectManagerBK Instance = new();

    private readonly ConcurrentDictionary<Guid, ObjectEntry> _dic = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorObjectManagerBK"/> class.
    /// Private constructor enforces singleton pattern.
    /// </summary>
    private EditorObjectManagerBK()
    {
    }

    /// <summary>
    /// Initializes the object manager by setting this instance as the active <see cref="EditorObjectManager"/>.
    /// </summary>
    internal void Initialize()
    {
        EditorObjectManager.Instance = this;
    }

    /// <summary>
    /// Creates an object using the specified factory function, ensuring it has an entry.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <param name="creation">The factory function that creates the object.</param>
    /// <returns>The created object with an assigned entry.</returns>
    internal override T Create<T>(Func<T> creation)
    {
        T obj = creation();

        obj.Entry ??= NewEntry();

        return obj;
    }

    /// <inheritdoc/>
    internal override T Create<T>(Func<T> creation, out ObjectEntry entry)
    {
        entry = NewEntry();

        T obj = creation();
        obj.Entry = entry;

        return obj;
    }

    /// <inheritdoc/>
    internal override ObjectEntry NewEntry()
    {
        var entry = new ObjectEntryBK();

        while (true)
        {
            Guid id = Guid.NewGuid();
            if (_dic.TryAdd(id, entry))
            {
                entry._id = id;
                break;
            }
        }

        return entry;
    }

    /// <inheritdoc/>
    public override EditorObject GetObject(Guid id)
    {
        return _dic.GetValueSafe(id)?.Target;
    }

    /// <inheritdoc/>
    public override ObjectEntry GetEntry(Guid id)
    {
        return _dic.GetValueSafe(id);
    }

    /// <inheritdoc/>
    internal override ObjectEntry EnsureEntry(Guid id)
    {
        return _dic.GetOrAdd(id, aId => new ObjectEntryBK(aId));
    }

    /// <inheritdoc/>
    internal override IEnumerable<ObjectEntry> Entries => _dic.Values;
    /// <inheritdoc/>
    internal override IEnumerable<EditorObject> Objects => _dic.Values.Select(o => o.Target);
    /// <inheritdoc/>
    internal override IEnumerable<EditorObject> AllObjects => _dic.Values.SelectMany(o => o.Targets);

    /// <inheritdoc/>
    public override int Count => _dic.Count;

    /// <inheritdoc/>
    internal override ObjectEntry ResolveEntry(IdResolveType resolveType, string fullName, Guid lastId)
    {
        ObjectEntry entry = null;

        switch (resolveType)
        {
            case IdResolveType.Auto:
                if (lastId == Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        entry = GlobalIdResolver.ResolveEntry(fullName, false);
                    }
                    else
                    {
                        entry = NewEntry();
                    }
                }
                else
                {
                    goto case IdResolveType.LastId;
                }
                break;

            case IdResolveType.FullName:
                if (!string.IsNullOrEmpty(fullName))
                {
                    entry = GlobalIdResolver.ResolveEntry(fullName, false);
                }
                break;

            case IdResolveType.LastId:
                if (lastId != Guid.Empty)
                {
                    entry = EnsureEntry(lastId);
                }
                break;

            case IdResolveType.New:
                entry = NewEntry();
                break;

            default:
                break;
        }

        return entry;
    }

    /// <inheritdoc/>
    public override FieldObjectCollection<T> CreateFieldCollection<T>(EditorObject owner)
    {
        return new FieldObjectCollectionBK<T>(owner);
    }
}