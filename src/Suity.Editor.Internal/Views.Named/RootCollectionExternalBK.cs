using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Helpers;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Backend implementation of <see cref="RootCollectionExternal"/> that manages named items with unique name tracking and synchronization support.
/// </summary>
internal class RootCollectionExternalBK : RootCollectionExternal
{
    private readonly NamedRootCollection _collection;

    private readonly UniqueMultiDictionary<string, NamedItem> _allItems = new();
    private readonly NamedItemList _itemList;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootCollectionExternalBK"/> class.
    /// </summary>
    /// <param name="collection">The root collection to manage. Must not be null.</param>
    public RootCollectionExternalBK(NamedRootCollection collection)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        _itemList = new NamedItemList(_collection.OnDropInCheck, _collection.OnDropInConvert)
        {
            Root = _collection,
            ParentNode = _collection,
        };
    }

    /// <inheritdoc/>
    public override IEnumerable<NamedItem> AllItems => _allItems.SoloValues;

    /// <inheritdoc/>
    public override IEnumerable<NamedItem> AllItemsSorted
    {
        get
        {
            foreach (NamedItem item in _itemList)
            {
                yield return item;
                if (item is NamedNode namedNode)
                {
                    foreach (var nodeItem in namedNode.AllItemsSorted)
                    {
                        yield return nodeItem;
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<NamedItem> Items => _itemList.Pass();

    /// <inheritdoc/>
    public override void AddItem(NamedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (ReferenceEquals(item._parentList, _itemList))
        {
            return;
        }

        if (item._parentList != null)
        {
            throw new ArgumentException("Item is in another list.");
        }

        if (!CanDropIn(item))
        {
            throw new ArgumentException("Item cannot be dropped in.");
        }

        _itemList.Add(item);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        foreach (var item in _allItems.Values)
        {
            item.OnInternalRemoved(_collection);
        }

        _allItems.Clear();
        _itemList.Clear();
    }

    /// <inheritdoc/>
    public override bool ContainsItem(string name, bool inAllItems)
    {
        if (name is null) return false;

        if (inAllItems)
        {
            return _allItems.ContainsKey(name);
        }
        else
        {
            return _itemList.GetItem(name) != null;
        }
    }

    /// <inheritdoc/>
    public override bool ContainsItem(NamedItem item, bool inAllItems)
    {
        if (item is null) return false;

        if (inAllItems)
        {
            return _allItems.Contains(item.Name, item);
        }
        else
        {
            return _itemList.GetItem(item.Name) == item;
        }
    }

    /// <inheritdoc/>
    public override NamedItem GetItem(string name, bool inAllItems)
    {
        if (name is null) return null;

        if (inAllItems)
        {
            return _allItems.GetFirstOrDefault(name);
        }
        else
        {
            return _itemList.GetItem(name);
        }
    }

    /// <inheritdoc/>
    public override NamedItem GetItemAt(int index)
    {
        return _itemList.GetItemAt(index);
    }

    /// <inheritdoc/>
    public override bool RemoveItem(NamedItem item)
    {
        return _itemList.Remove(item);
    }

    /// <inheritdoc/>
    public override bool Rename(NamedItem item, string newName)
    {
        if (item is null)
        {
            throw new ArgumentNullException();
        }

        if (string.IsNullOrEmpty(newName))
        {
            return false;
        }

        if (item.Root != _collection)
        {
            return false;
        }

        //TODO: Rename algorithm is not robust
        if (_allItems.ContainsKey(newName))
        {
            return false;
        }

        string oldName = item.Name;

        if (_allItems.Remove(item.Name, item))
        {
            item.UpdateName(newName);
            _allItems.Add(newName, item);
            _collection.OnItemRenamed(item, oldName);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override string GetSuggestedName(NamedItem item, int digiLen = 2)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return GetSuggestedName(item.OnGetSuggestedPrefix(), digiLen);
    }

    /// <inheritdoc/>
    public override string GetSuggestedName(string prefix, int digiLen = 2)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException(L($"\"{nameof(prefix)}\" cannot be null or whitespace."), nameof(prefix));
        }

        string customResolve = _collection.OnGetSuggestedName(prefix, digiLen);
        if (!string.IsNullOrWhiteSpace(customResolve) && !_allItems.ContainsKey(customResolve))
        {
            return customResolve;
        }

        ulong num = 1;
        while (true)
        {
            string name = KeyIncrementHelper.MakeKey(prefix, digiLen, num);
            if (!_allItems.ContainsKey(name))
            {
                return name;
            }
            else
            {
                num++;
            }
        }
    }

    /// <inheritdoc/>
    public override string ResolveConflictName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(L($"\"{nameof(name)}\" cannot be null or whitespace."), nameof(name));
        }

        if (!_allItems.ContainsKey(name))
        {
            return name;
        }

        string customResolve = _collection.OnResolveConflictName(name);
        if (!string.IsNullOrWhiteSpace(customResolve) && !_allItems.ContainsKey(customResolve))
        {
            return customResolve;
        }

        KeyIncrementHelper.ParseKey(name, out string prefix, out int digiLen, out ulong digiValue);

        while (true)
        {
            digiValue++;
            name = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue);
            if (!_allItems.ContainsKey(name))
            {
                return name;
            }
        }
    }

    /// <inheritdoc/>
    public override bool InternalAddItem(NamedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException();
        }

        bool isNew = false;
        if (string.IsNullOrEmpty(item.Name))
        {
            string newName = GetSuggestedName(item.OnGetSuggestedPrefix());
            item.UpdateName(newName);
            isNew = true;
        }

        if (_allItems.ContainsKey(item.Name))
        {
            string newName = ResolveConflictName(item.Name);
            item.UpdateName(newName);
            isNew = true;
        }

        if (_allItems.Add(item.Name, item))
        {
            item.Root = _collection;
            _collection.OnItemAdded(item, isNew);
            item.OnInternalAdded();

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override NamedItem InternalCreateDefaultItem(INamedNode parentNode)
    {
        var item = _collection.OnCreateDefaultItem(parentNode);
        if (item is null)
        {
            return null;
        }

        if (item.Root != null)
        {
            throw new InvalidOperationException();
        }

        // If the implementer configures the class name, bypass the suggested name
        if (!string.IsNullOrEmpty(item.Name))
        {
            if (_allItems.ContainsKey(item.Name))
            {
                string newName = GetSuggestedName(item.Name);
                item.UpdateName(newName);
            }
        }
        // Only by not setting the field here can the subsequent InternalAddItem process determine this is a new object.

        return item;
    }

    /// <inheritdoc/>
    public override async Task<NamedItem[]> InternalGuiCreateItems(INamedNode parentNode, Type type)
    {
        NamedItem[] items = await _collection.OnGuiCreateItems(parentNode, type);
        if (items is null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item.Root != null)
            {
                throw new InvalidOperationException();
            }
            // If the implementer configures the class name, bypass the suggested name
            if (!string.IsNullOrEmpty(item.Name))
            {
                if (_allItems.ContainsKey(item.Name))
                {
                    string newName = GetSuggestedName(item.Name);
                    item.UpdateName(newName);
                }
            }
            // Only by not setting the field here can the subsequent InternalAddItem process determine this is a new object.
        }

        return items;
    }

    /// <inheritdoc/>
    public override bool InternalRemoveItem(NamedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException();
        }

        if (item.Name is null)
        {
            return false;
        }

        if (_allItems.Remove(item.Name, item))
        {
            item.OnInternalRemoved(_collection);
            item.Root = null;
            _collection.OnItemRemoved(item);

            return true;
        }
        else
        {
            return false;
        }
    }

    #region IViewList

    /// <inheritdoc/>
    public override int Count => _itemList.Count;

    /// <inheritdoc/>
    public override bool CanDropIn(object value) => ((IDropInCheck)_itemList).DropInCheck(value);

    /// <inheritdoc/>
    public override object DropInConvert(object value) => ((IDropInCheck)_itemList).DropInConvert(value);

    /// <inheritdoc/>
    public override void Sync(IIndexSync sync, ISyncContext context) => ((ISyncList)_itemList).Sync(sync, context);

    #endregion
}
