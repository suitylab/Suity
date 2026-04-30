using Suity.Editor;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Views.Named;

/// <summary>
/// A list of named items that supports synchronization, display, and drop-in operations within a root collection.
/// </summary>
[NativeAlias]
public class NamedItemList : INamedItemList,
    IViewList, ITextDisplay, IEnumerable<NamedItem>
{
    private NamedRootCollection _root;
    internal INamedNode _node;
    private readonly List<NamedItem> _items = [];

    internal Predicate<object> CanDropInFunc { get; set; }
    internal Func<object, object> DropInConvertFunc { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemList"/> class with default drop-in behavior.
    /// </summary>
    public NamedItemList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemList"/> class with custom drop-in functions.
    /// </summary>
    /// <param name="canDropIn">Predicate to determine if an object can be dropped into the list.</param>
    /// <param name="dropInConvert">Function to convert a dropped object into a compatible item.</param>
    public NamedItemList(Predicate<object> canDropIn, Func<object, object> dropInConvert)
    {
        CanDropInFunc = canDropIn ?? throw new ArgumentNullException(nameof(canDropIn));
        DropInConvertFunc = dropInConvert ?? throw new ArgumentNullException(nameof(dropInConvert));
    }

    /// <summary>
    /// Gets or sets the root collection associated with this list.
    /// Changing the root transfers all items between the old and new root collections.
    /// </summary>
    public NamedRootCollection Root
    {
        get => _root;
        set
        {
            // NamedItemCollection oldRoot = _root;

            if (_root != value)
            {
                if (_root != null)
                {
                    foreach (var item in _items)
                    {
                        _root.InternalRemoveItem(item);
                    }
                }

                _root = value;
                if (_root != null)
                {
                    foreach (var item in _items)
                    {
                        _root.InternalAddItem(item);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the parent node associated with this list.
    /// </summary>
    public INamedNode ParentNode
    {
        get => _node;
        set => _node = value;
    }

    /// <summary>
    /// Gets the item with the specified name.
    /// </summary>
    /// <param name="name">The name of the item to find.</param>
    /// <returns>The matching <see cref="NamedItem"/>, or null if not found.</returns>
    public NamedItem GetItem(string name) => _items.Find(o => o.Name == name);

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    public NamedItem GetItemAt(int index) => _items[index];

    /// <summary>
    /// Returns the zero-based index of the specified item.
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(NamedItem item) => _items.IndexOf(item);

    /// <summary>
    /// Gets the synchronization path for the specified item.
    /// </summary>
    /// <param name="item">The item to get the path for.</param>
    /// <returns>A <see cref="SyncPath"/> representing the item's location.</returns>
    public SyncPath GetPath(NamedItem item)
    {
        if (_node != null)
        {
            return _node.GetPath().Append(IndexOf(item));
        }
        else if (_root != null)
        {
            return _root.GetPath().Append(IndexOf(item));
        }
        else
        {
            return new SyncPath(IndexOf(item));
        }
    }

    /// <summary>
    /// Adds an item to the end of the list.
    /// </summary>
    /// <param name="item">The item to add. Must not be null.</param>
    public void Add(NamedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item._parentList != null)
        {
            throw new ArgumentException();
        }

        _items.Add(item);
        OnAdded(item, _items.Count - 1);
    }

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert. Must not be null.</param>
    public void Insert(int index, NamedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item._parentList != null)
        {
            throw new ArgumentException();
        }

        _items.Insert(index, item);
        OnAdded(item, index);
    }

    /// <summary>
    /// Removes the specified item from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool Remove(NamedItem item)
    {
        if (item is null)
        {
            return false;
        }

        if (item._parentList != this)
        {
            return false;
        }

        _items.Remove(item);
        OnRemoved(item);

        return true;
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Gets all items in the list sorted, including nested items from child nodes.
    /// </summary>
    public IEnumerable<NamedItem> AllItemsSorted
    {
        get
        {
            foreach (NamedItem item in _items)
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

    /// <summary>
    /// Handles the addition of an item to the list, updating parent references and root collection membership.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    /// <param name="index">The index at which the item was added.</param>
    private void OnAdded(NamedItem item, int index)
    {
        if (item is null)
        {
            return;
        }

        if (item._parentList != null)
        {
            (item._parentList as NamedItemList)?._items.Remove(item);
        }

        item._parentList = this;

        item.Root?.InternalRemoveItem(item);
        _root?.InternalAddItem(item);
    }

    /// <summary>
    /// Handles the removal of an item from the list, updating parent references and root collection membership.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    private void OnRemoved(NamedItem item)
    {
        if (item is null)
        {
            return;
        }

        if (item._parentList == this)
        {
            item._parentList = null;
        }

        _root?.InternalRemoveItem(item);
    }

    #region IViewList

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(
            _items,
            createNew: () => _root?.InternalCreateDefaultItem(_node),
            added: OnAdded,
            removed: OnRemoved
        );
    }

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value)
    {
        if (CanDropInFunc != null)
        {
            return CanDropInFunc(value);
        }
        else
        {
            return value is NamedItem;
        }
    }

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value)
    {
        if (DropInConvertFunc != null)
        {
            return DropInConvertFunc(value);
        }
        else
        {
            return value;
        }
    }

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    string ITextDisplay.DisplayText => null;

    /// <inheritdoc/>
    object ITextDisplay.DisplayIcon => CoreIconCache.FolderDesign;

    /// <inheritdoc/>
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    #region IEnumerable&lt;NamedItem&gt;

    /// <inheritdoc/>
    IEnumerator<NamedItem> IEnumerable<NamedItem>.GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    #endregion
}
