using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Abstract base class for a named node that can contain child items, extending <see cref="NamedItem"/> with hierarchical capabilities.
/// </summary>
public abstract class NamedNode : NamedItem,
    INamedNode,
    IViewNode
{
    private readonly INamedItemList _itemList;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedNode"/> class.
    /// </summary>
    public NamedNode()
    {
        _itemList = NamedExternal._external.CreateItemList(OnDropInCheck, OnDropInConvert);
        _itemList.ParentNode = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedNode"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name for the node.</param>
    public NamedNode(string name)
        : base(name)
    {
        _itemList = NamedExternal._external.CreateItemList(OnDropInCheck, OnDropInConvert);
        _itemList.ParentNode = this;
    }

    /// <summary>
    /// Called when this node is internally added to a collection.
    /// </summary>
    internal override void OnInternalAdded()
    {
        _itemList.Root = Root;
        base.OnInternalAdded();
    }

    /// <summary>
    /// Called when this node is internally removed from a collection.
    /// </summary>
    /// <param name="root">The root collection the node was removed from.</param>
    internal override void OnInternalRemoved(NamedRootCollection root)
    {
        _itemList.Root = Root;
        base.OnInternalRemoved(root);
    }

    /// <summary>
    /// Called during synchronization to sync the child item list.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        sync.Sync("List", _itemList, SyncFlag.GetOnly | SyncFlag.Element | SyncFlag.PathHidden);
    }

    /// <summary>
    /// Gets the top-level child items of this node.
    /// </summary>
    public IEnumerable<NamedItem> Items => _itemList;

    /// <summary>
    /// Gets all child items of this node, including nested items.
    /// </summary>
    public IEnumerable<NamedItem> AllItems => _itemList.AllItemsSorted;

    /// <summary>
    /// Gets all child items of this node sorted by name.
    /// </summary>
    public IEnumerable<NamedItem> AllItemsSorted => _itemList.AllItemsSorted;

    /// <summary>
    /// Creates a default child item for this node.
    /// </summary>
    /// <returns>The newly created <see cref="NamedItem"/>, or null if not supported.</returns>
    public NamedItem CreateDefaultItem() => OnCreateDefaultItem();

    /// <summary>
    /// Creates a default child item for this node.
    /// </summary>
    protected internal virtual NamedItem OnCreateDefaultItem() => null;

    /// <summary>
    /// Shows a GUI for creating child items.
    /// </summary>
    /// <returns>An array of created <see cref="NamedItem"/> instances.</returns>
    public Task<NamedItem[]> GuiCreateItems() => OnGuiCreateItems();

    /// <summary>
    /// Shows a GUI for creating child items.
    /// </summary>
    protected internal virtual Task<NamedItem[]> OnGuiCreateItems() => Task.FromResult<NamedItem[]>(null);

    /// <summary>
    /// Shows a GUI for configuring the specified child item.
    /// </summary>
    /// <param name="item">The item to configure.</param>
    /// <returns>False to allow automatic transfer to Document for configuration.</returns>
    public Task<bool> GuiConfigItem(NamedItem item) => OnGuiConfigItem(item);

    /// <summary>
    /// Shows a GUI for configuring the specified child item.
    /// </summary>
    /// <param name="item">The item to configure.</param>
    /// <returns>False to allow automatic transfer to Document for configuration.</returns>
    protected internal virtual Task<bool> OnGuiConfigItem(NamedItem item)
    {
        return Task.FromResult<bool>(false);
    }

    /// <summary>
    /// Adds a child item to this node.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    /// <exception cref="ArgumentException">Thrown when item already has a parent list.</exception>
    public void AddItem(NamedItem item)
    {
        if (item == null) throw new ArgumentNullException();
        if (item._parentList != null) throw new ArgumentException();
        _itemList.Add(item);
    }

    /// <summary>
    /// Removes a child item from this node.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool RemoveItem(NamedItem item) => _itemList.Remove(item);

    /// <summary>
    /// Gets the count of top-level child items.
    /// </summary>
    public int Count => _itemList.Count;

    /// <summary>
    /// Gets a child item by name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <returns>The found <see cref="NamedItem"/>, or null if not found.</returns>
    public NamedItem GetItem(string name) => _itemList.GetItem(name);

    /// <summary>
    /// Gets a child item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item.</param>
    /// <returns>The <see cref="NamedItem"/> at the specified index.</returns>
    public NamedItem GetItemAt(int index) => _itemList.GetItemAt(index);

    /// <summary>
    /// Removes all child items from this node.
    /// </summary>
    public void Clear() => _itemList.Clear();

    #region IViewNode

    /// <summary>
    /// Gets the list view identifier for this node.
    /// </summary>
    int IViewNode.ListViewId => ViewIds.MainTreeView;

    /// <summary>
    /// Gets the synchronized list of child items.
    /// </summary>
    ISyncList ISyncNode.GetList() => _itemList;

    /// <summary>
    /// Checks if the specified value can be dropped into this node.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    bool IDropInCheck.DropInCheck(object value)
    {
        if (!((IDropInCheck)_itemList).DropInCheck(value))
        {
            return false;
        }

        return OnDropInCheck(value);
    }

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    object IDropInCheck.DropInConvert(object value) => OnDropInConvert(value);

    #endregion

    /// <summary>
    /// Checks if the specified value can be dropped into this node.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    protected internal virtual bool OnDropInCheck(object value)
    {
        if (Root != null)
        {
            return Root.OnDropInCheck(value);
        }
        else
        {
            return value is NamedItem;
        }
    }

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    protected internal virtual object OnDropInConvert(object value)
    {
        if (Root != null)
        {
            return Root.OnDropInConvert(value);
        }
        else
        {
            return value;
        }
    }
}
