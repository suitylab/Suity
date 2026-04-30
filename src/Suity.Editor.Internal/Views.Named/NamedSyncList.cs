using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// A synchronized list of named items that supports add/remove operations, name conflict resolution, and GUI-based object creation.
/// </summary>
/// <typeparam name="TValue">The type of items in the list, must be a class implementing <see cref="ISyncObject"/>.</typeparam>
public class NamedSyncList<TValue> : INamedSyncList<TValue>, IDropInCheck
    where TValue : class, ISyncObject
{
    private readonly string _keyPropertyName;
    private readonly List<TValue> _list = [];
    private readonly TwoWayDictionary<string, TValue> _dic = [];

    /// <summary>
    /// Occurs when an item is added to the list.
    /// </summary>
    public event Action<TValue, bool> ItemAdded;

    /// <summary>
    /// Occurs when an item is removed from the list.
    /// </summary>
    public event Action<TValue> ItemRemoved;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedSyncList{TValue}"/> class.
    /// </summary>
    /// <param name="keyPropertyName">The property name used as the key for items. Must not be null or empty.</param>
    public NamedSyncList(string keyPropertyName)
    {
        if (string.IsNullOrEmpty(keyPropertyName))
        {
            throw new ArgumentNullException();
        }
        _keyPropertyName = keyPropertyName;
    }

    /// <summary>
    /// Gets or sets a function that suggests a prefix for new item names.
    /// </summary>
    public Func<TValue, string> PrefixSuggest { get; set; }
    /// <summary>
    /// Gets or sets a function that resolves name conflicts.
    /// </summary>
    public Func<string, string> ConflictResolver { get; set; }
    /// <summary>
    /// Gets or sets a predicate that checks whether an item can be added to the list.
    /// </summary>
    public Predicate<TValue> AddItemChecker { get; set; }
    /// <summary>
    /// Gets or sets a GUI-based factory function for creating new values.
    /// </summary>
    public GuiObjectCreation ValueCreaterGUI { get; set; }

    /// <summary>
    /// Gets or sets the default prefix used for generating item names.
    /// </summary>
    public string DefaultPrefix { get; set; }

    /// <summary>
    /// Adds an item to the list, automatically resolving name conflicts if necessary.
    /// </summary>
    /// <param name="item">The item to add. Must not be null.</param>
    /// <returns>True if the item was added successfully; otherwise, false.</returns>
    public bool Add(TValue item)
    {
        if (item is null)
        {
            throw new ArgumentNullException();
        }

        string name = item.GetProperty<string>(_keyPropertyName);

        bool isNew = false;
        if (string.IsNullOrEmpty(name))
        {
            isNew = true;
        }

        if (string.IsNullOrEmpty(name) || _dic.ContainsKey(name))
        {
            string newName = InternalResolveConflictItemName(item, name);
            if (newName != name)
            {
                name = newName;
                isNew = true;
            }

            if (name is null || _dic.ContainsKey(name))
            {
                return false;
            }
            else
            {
                item.SetProperty<string>(_keyPropertyName, name);
            }
        }

        if (_dic.ContainsValue(item))
        {
            return false;
        }

        _dic.Add(name, item);
        _list.Add(item);

        OnItemAdded(item, isNew);

        return true;
    }

    /// <summary>
    /// Removes an item from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool Remove(TValue item)
    {
        if (_dic.RemoveValue(item))
        {
            _list.Remove(item);
            OnItemRemoved(item);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes an item by its key name.
    /// </summary>
    /// <param name="key">The key name of the item to remove.</param>
    /// <returns>True if the item was found and removed; otherwise, false.</returns>
    public bool RemoveByName(string key)
    {
        if (key is null) return false;

        if (_dic.TryGetValue(key, out TValue value))
        {
            _dic.Remove(key);
            _list.Remove(value);
            OnItemRemoved(value);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    public TValue this[int index]
    {
        get => _list[index];
        set => SetValue(index, value);
    }

    /// <summary>
    /// Gets the item with the specified name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    public TValue this[string name]
    {
        get
        {
            if (_dic.TryGetValue(name, out TValue value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }
    }

    /// <summary>
    /// Sets the value at the specified index, replacing the existing item.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <param name="value">The new value. Must not be null.</param>
    /// <returns>True if the value was set successfully; otherwise, false.</returns>
    public bool SetValue(int index, TValue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException();
        }

        // Check for same object
        if (ReferenceEquals(_list.GetListItemSafe(index), value))
        {
            return true;
        }

        string name = value.GetProperty<string>(_keyPropertyName);

        bool isNew = false;
        if (string.IsNullOrEmpty(name))
        {
            isNew = true;
        }

        if (string.IsNullOrEmpty(name) || _dic.ContainsKey(name))
        {
            string newName = InternalResolveConflictItemName(value, name);
            if (newName != name)
            {
                name = newName;
                isNew = true;
            }

            if (name is null || _dic.ContainsKey(name))
            {
                return false;
            }
            else
            {
                value.SetProperty<string>(_keyPropertyName, name);
            }
        }

        if (_dic.ContainsValue(value)) return false;

        if (index < 0 || index >= _list.Count) return false;

        TValue current = _list[index];
        _dic.RemoveValue(current);
        _list[index] = value;
        _dic.Add(name, value);

        OnItemRemoved(current);
        OnItemAdded(value, isNew);

        return true;
    }

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert. Must not be null.</param>
    /// <returns>True if the item was inserted successfully; otherwise, false.</returns>
    public bool Insert(int index, TValue item)
    {
        if (item is null)
        {
            throw new ArgumentNullException();
        }

        string name = item.GetProperty<string>(_keyPropertyName);

        bool isNew = false;
        if (string.IsNullOrEmpty(name))
        {
            isNew = true;
        }

        if (string.IsNullOrEmpty(name) || _dic.ContainsKey(name))
        {
            string newName = InternalResolveConflictItemName(item, name);
            if (newName != name)
            {
                name = newName;
                isNew = true;
            }

            if (name is null || _dic.ContainsKey(name))
            {
                return false;
            }
            else
            {
                item.SetProperty<string>(_keyPropertyName, name);
            }
        }
        if (_dic.ContainsValue(item)) return false;

        if (index >= 0 && index <= _list.Count)
        {
            _dic.Add(name, item);
            _list.Insert(index, item);

            OnItemAdded(item, isNew);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        var current = _list[index];
        _list.RemoveAt(index);
        _dic.RemoveValue(current);

        OnItemRemoved(current);
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        var items = _dic.Values.ToArray();
        _list.Clear();
        _dic.Clear();

        foreach (var item in items)
        {
            OnItemRemoved(item);
        }
    }

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Checks if the list contains an item with the specified key name.
    /// </summary>
    /// <param name="key">The key name to search for.</param>
    /// <returns>True if an item with the key exists; otherwise, false.</returns>
    public bool ContainsName(string key)
    {
        return _dic.ContainsKey(key);
    }

    /// <summary>
    /// Checks if the list contains the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public bool Contains(TValue item)
    {
        return _dic.ContainsValue(item);
    }

    /// <summary>
    /// Returns the zero-based index of the specified item.
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(TValue item)
    {
        return _list.IndexOf(item);
    }

    /// <summary>
    /// Changes the name of an item.
    /// </summary>
    /// <param name="item">The item to rename. Must not be null.</param>
    /// <param name="newKey">The new key name.</param>
    /// <param name="setNameProperty">Whether to also update the name property on the item.</param>
    /// <returns>True if the name was changed successfully; otherwise, false.</returns>
    public bool ChangeName(TValue item, string newKey, bool setNameProperty)
    {
        if (item is null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(newKey))
        {
            return false;
        }

        if (_dic.ChangeKey(item, newKey))
        {
            if (setNameProperty)
            {
                item.SetProperty<string>(_keyPropertyName, newKey);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to get the item with the specified key.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">When this method returns, contains the item associated with the key, or the default value if not found.</param>
    /// <returns>True if the key was found; otherwise, false.</returns>
    public bool TryGetValue(string key, out TValue value)
    {
        if (key is null)
        {
            value = default;
            return false;
        }

        return _dic.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the item with the specified key, or the default value if not found.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>The item associated with the key, or the default value if not found.</returns>
    public TValue GetValueOrDefault(string key)
    {
        if (key != null && _dic.TryGetValue(key, out TValue value))
        {
            return value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Generates a suggested name with the specified prefix, avoiding existing names.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 1).</param>
    /// <returns>A non-conflicting suggested name.</returns>
    public string GetSuggestedName(string prefix, int digiLen = 1)
    {
        prefix ??= string.Empty;

        ulong num = 1;
        while (true)
        {
            string name = KeyIncrementHelper.MakeKey(prefix, digiLen, num);
            if (!ContainsName(name))
            {
                return name;
            }
            else
            {
                num++;
            }
        }
    }

    /// <summary>
    /// Resolves a name conflict by generating a non-conflicting name with an incremented suffix.
    /// </summary>
    /// <param name="name">The original name that may conflict.</param>
    /// <returns>A non-conflicting name.</returns>
    public string ResolveConflictItemName(string name)
    {
        KeyIncrementHelper.ParseKey(name, out string prefix, out int digiLen, out ulong digiValue);

        while (true)
        {
            digiValue++;
            name = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue);
            if (!ContainsName(name))
            {
                return name;
            }
        }
    }

    /// <summary>
    /// Internally resolves a name conflict for a specific value, using prefix suggestion or default prefix.
    /// </summary>
    /// <param name="value">The value to resolve the name for.</param>
    /// <param name="name">The original name.</param>
    /// <returns>A resolved non-conflicting name.</returns>
    private string InternalResolveConflictItemName(TValue value, string name)
    {
        string prefix = PrefixSuggest?.Invoke(value) ?? DefaultPrefix;
        if (string.IsNullOrEmpty(prefix))
        {
            prefix = "Item";
        }

        if (string.IsNullOrEmpty(name))
        {
            name = prefix + "00";
        }

        if (ConflictResolver != null)
        {
            return ConflictResolver(name);
        }
        else
        {
            return ResolveConflictItemName(name);
        }
    }

    /// <summary>
    /// Raises the <see cref="ItemAdded"/> event.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    /// <param name="isNew">Indicates whether this is a newly created item.</param>
    protected virtual void OnItemAdded(TValue item, bool isNew)
    {
        ItemAdded?.Invoke(item, isNew);
    }

    /// <summary>
    /// Raises the <see cref="ItemRemoved"/> event.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    protected virtual void OnItemRemoved(TValue item)
    {
        ItemRemoved?.Invoke(item);
    }

    #region IEnumerable

    /// <inheritdoc/>
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    #endregion

    #region ISyncList

    /// <inheritdoc/>
    public void Sync(IIndexSync sync, ISyncContext context)
    {
        TValue value;

        switch (sync.Mode)
        {
            case SyncMode.RequestElementType:
                sync.Sync(0, typeof(TValue));
                break;

            case SyncMode.Get:
                if (sync.Index >= 0 && sync.Index < _list.Count)
                {
                    sync.Sync(sync.Index, _list[sync.Index]);
                }
                break;

            case SyncMode.Set:
                if (sync.Index >= 0 && sync.Index < _list.Count)
                {
                    value = (TValue)sync.Value;
                    if (value is null)
                    {
                        Logs.LogWarning($"SyncKeyList Set sync {typeof(TValue).Name} failed. value = null.");
                        break;
                    }
                    if (AddItemChecker is null || AddItemChecker(value))
                    {
                        SetValue(sync.Index, value);
                    }
                }
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < _list.Count; i++)
                {
                    sync.Sync(i, _list[i]);
                }
                break;

            case SyncMode.SetAll:
                Clear();
                for (int i = 0; i < sync.Count; i++)
                {
                    value = sync.Sync<TValue>(i, default);
                    if (AddItemChecker is null || AddItemChecker(value))
                    {
                        if (value is null)
                        {
                            Logs.LogWarning($"SyncKeyList SetAll sync {typeof(TValue).Name} failed on {i}. value = null.");
                            continue;
                        }
                        Add(value);
                    }
                }
                break;

            case SyncMode.Insert:
                value = (TValue)sync.Value;
                if (value is null)
                {
                    Logs.LogWarning($"SyncKeyList Insert sync {typeof(TValue).Name} failed. value = null.");
                    break;
                }
                if (AddItemChecker is null || AddItemChecker(value))
                {
                    Insert(sync.Index, value);
                }
                break;

            case SyncMode.RemoveAt:
                RemoveAt(sync.Index);
                break;

            case SyncMode.CreateNew:
                sync.Sync<TValue>(0, (TValue)typeof(TValue).CreateInstanceOf());
                break;

            default:
                break;
        }
    }

    #endregion

    #region IList&lt;T&gt;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    void IList<TValue>.Insert(int index, TValue item)
    {
        Insert(index, item);
    }

    /// <inheritdoc/>
    void ICollection<TValue>.Add(TValue item)
    {
        Add(item);
    }

    /// <inheritdoc/>
    public void CopyTo(TValue[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    #endregion

    #region IDropInCheck

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value)
    {
        if (value is not TValue v)
        {
            return false;
        }

        return AddItemChecker?.Invoke(v) == true;
    }

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value)
    {
        if (value is not TValue v)
        {
            return null;
        }

        return v;
    }

    #endregion

    #region IHasObjectCreationGUI

    /// <inheritdoc/>
    public IEnumerable<ObjectCreationOption> CreationOptions => null;

    /// <inheritdoc/>
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        if (ValueCreaterGUI is { } creater)
        {
            return await creater(typeHint);
        }
        else
        {
            return null;
        }
    }

    #endregion
}
