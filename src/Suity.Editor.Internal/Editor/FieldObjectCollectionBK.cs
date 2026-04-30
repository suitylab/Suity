using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Generic implementation of <see cref="FieldObjectCollection{T}"/> that manages field objects
/// by name and ID with ordering, resolution, sorting, and owner lifecycle events.
/// </summary>
/// <typeparam name="T">The type of field object, which must inherit from <see cref="FieldObject"/> and have a parameterless constructor.</typeparam>
public class FieldObjectCollectionBK<T> : FieldObjectCollection<T>
    where T : FieldObject, new()
{
    private readonly EditorObject _owner;

    private readonly Dictionary<string, T> _fields = [];
    private readonly Dictionary<Guid, T> _fieldsById = [];
    private readonly List<T> _fieldOrdered = [];

    /// <summary>
    /// Initializes a new instance of <see cref="FieldObjectCollectionBK{T}"/> with the specified owner.
    /// </summary>
    /// <param name="owner">The owner editor object. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="owner"/> is null.</exception>
    public FieldObjectCollectionBK(EditorObject owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _owner.IdResolved += _owner_IdResolved;
        _owner.IdDetached += _owner_IdDetached;
    }

    /// <inheritdoc/>
    public override T GetField(string name) => _fields.GetValueSafe(name);

    /// <inheritdoc/>
    public override T GetField(Guid id) => _fieldsById.GetValueSafe(id);

    /// <inheritdoc/>
    public override IEnumerable<T> Fields => _fieldOrdered;
    /// <inheritdoc/>
    public override int FieldCount => _fields.Count;

    /// <inheritdoc/>
    public override T GetOrAddField(string name, IdResolveType resolveType = IdResolveType.Auto, Guid? recoredId = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        bool isNew = false;
        T field = _fields.GetOrAdd(name, _ =>
        {
            isNew = true;
            var newField = new T
            {
                _name = name,
                _parent = _owner,
                _recordedId = recoredId ?? Guid.Empty,
            };

            if (_owner.Entry != null)
            {
                newField.ResolveId(resolveType);
            }

            return newField;
        });

        if (field.Id == Guid.Empty)
        {
            // When field has no Id record but a recorded Id is provided, save this recorded Id
            if (recoredId.HasValue)
            {
                field._recordedId = recoredId.Value;
            }

            // When Owner has Entry, try to resolve Id
            if (_owner.Entry != null)
            {
                field.ResolveId(resolveType);
            }
        }

        if (isNew)
        {
            field.ObjectUpdated += Field_ObjectUpdated;
            AddFieldById(field);
            _fieldOrdered.Add(field);
            _owner.NotifyUpdated(new FieldEntryEventArgs(name, EntryUpdateTypes.Add));

            if (field.Id != Guid.Empty)
            {
                GlobalIdResolver.Record(field.FullName, field.Id);
            }
        }
        else
        {
            // Why update?
            //field.NotifyUpdated();
        }

        return field;
    }

    /// <inheritdoc/>
    public override bool RemoveField(string name)
    {
        if (name is null)
        {
            return false;
        }

        T field = _fields.RemoveAndGet(name);

        if (field != null)
        {
            _fieldsById.Remove(field.Id);
            _fieldOrdered.Remove(field);
            field.ObjectUpdated -= Field_ObjectUpdated;
            field._parent = null;
            field.Entry = null;

            _owner.NotifyUpdated(new FieldEntryEventArgs(name, EntryUpdateTypes.Remove));

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override T RenameField(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentException("New name is empty.", nameof(newName));
        }

        if (_fields.ContainsKey(newName))
        {
            //Logs.LogWarning($"FieldObjectCollection rename field FAILED (already contained new name) : {oldName}->{newName}");
            return null;
        }

        var field = _fields.RemoveAndGet(oldName);
        if (field != null)
        {
            //Logs.LogDebug($"AssetFieldCollector rename field : {oldName}->{newName}");

            string oldFullName = field.FullName;
            field._name = newName;
            _fields.Add(newName, field);
            string newFullName = field.FullName;

            field.NotifyUpdated(new FieldEntryEventArgs(oldName, newName, EntryUpdateTypes.Rename));
            _owner.NotifyUpdated();
            GlobalIdResolver.Rename(oldFullName, newFullName);

            return field;
        }
        else
        {
            //Logs.LogWarning($"FieldObjectCollection rename field FAILED (old name not found) : {oldName}->{newName}");
            return null;
        }
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        if (_fields.Count > 0)
        {
            foreach (var field in _fields.Values.ToArray())
            {
                RemoveField(field.Name);
            }
        }

        _fields.Clear();
        _fieldsById.Clear();
        _fieldOrdered.Clear();
    }

    /// <inheritdoc/>
    public override void ResolveFieldsId(IdResolveType resolveType)
    {
        _fieldsById.Clear();
        foreach (var field in _fields.Values)
        {
            field.ResolveId(resolveType);
            AddFieldById(field);
        }
    }

    /// <inheritdoc/>
    public override void Sort(Comparison<T> comparison) => _fieldOrdered.Sort(comparison);

    private bool AddFieldById(T field)
    {
        if (field.Id != Guid.Empty)
        {
            if (!_fieldsById.ContainsKey(field.Id))
            {
                _fieldsById.Add(field.Id, field);
                return true;
            }
            else
            {
                //Logs.LogWarning($"Field id duplicated :{field.Name}");
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void _owner_IdResolved(object sender, IdResolvedEventArgs e)
    {
        _fieldsById.Clear();
        foreach (var field in _fields.Values)
        {
            field.ResolveId(e.ResolveType);
            AddFieldById(field);
        }
    }

    private void _owner_IdDetached(object sender, EventArgs e)
    {
        _fieldsById.Clear();
        foreach (var field in _fields.Values)
        {
            field.Entry = null;
        }
    }

    private void Field_ObjectUpdated(object sender, EntryEventArgs e)
    {
        var field = (FieldObject)sender;

        _owner?.NotifyUpdated(new FieldEntryEventArgs(field._name, EntryUpdateTypes.Update, e));
    }
}