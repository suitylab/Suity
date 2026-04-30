using Suity.Editor;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Abstract base class for a list of named fields, providing management, synchronization, and view support.
/// </summary>
public abstract class NamedFieldList : IViewList,
    IEnumerable<NamedField>,
    ITextDisplay,
    ISyncTypeResolver,
    ISyncPathObject
{
    private readonly NamedItem _parentItem;
    private readonly INamedSyncList<NamedField> _items;
    private readonly Type _itemType;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList"/> class.
    /// </summary>
    public NamedFieldList()
    {
        _items = NamedExternal._external.CreateNamedSyncList<NamedField>("Name");
        _items.ValueCreaterGUI = async typeHint =>
        {
            NamedField item = await OnGuiCreateItemAsync(typeHint);
            if (item != null)
            {
                if (item.List != null)
                {
                    throw new InvalidOperationException();
                }
            }
            return item;
        };

        _items.AddItemChecker = OnDropInCheck;
        _items.ItemAdded += (item, isNew) =>
        {
            item.List?.Remove(item);
            item.List = this;
            if (string.IsNullOrEmpty(item.Name))
            {
                item.Name = GetSuggestedFieldName(item.OnGetSuggestedPrefix());
                OnItemAdded(item, true);
            }
            else
            {
                OnItemAdded(item, isNew);
            }
        };

        _items.ItemRemoved += obj =>
        {
            obj.List = null;
            OnItemRemoved(obj);
        };

        _items.PrefixSuggest = v => v.OnGetSuggestedPrefix();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList"/> class with the specified item type.
    /// </summary>
    /// <param name="itemType">The type of items in this list, must extend <see cref="NamedField"/>.</param>
    public NamedFieldList(Type itemType)
        : this()
    {
        if (itemType is null)
        {
            throw new ArgumentNullException();
        }

        if (!typeof(NamedField).IsAssignableFrom(itemType))
        {
            throw new ArgumentException($"{nameof(itemType)} must extends {nameof(NamedField)}");
        }

        _itemType = itemType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList"/> class with the specified parent item.
    /// </summary>
    /// <param name="parentItem">The parent item that owns this list.</param>
    public NamedFieldList(NamedItem parentItem)
        : this()
    {
        _parentItem = parentItem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList"/> class with the specified parent item and item type.
    /// </summary>
    /// <param name="parentItem">The parent item that owns this list.</param>
    /// <param name="itemType">The type of items in this list.</param>
    public NamedFieldList(NamedItem parentItem, Type itemType)
        : this(itemType)
    {
        _parentItem = parentItem;
    }

    /// <summary>
    /// Gets the parent item that owns this list.
    /// </summary>
    public NamedItem ParentItem => _parentItem;

    /// <summary>
    /// Gets the field at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    public NamedField this[int index] => _items[index];

    /// <summary>
    /// Adds a field to this list.
    /// </summary>
    /// <param name="item">The field to add.</param>
    public void Add(NamedField item) => _items.Add(item);

    /// <summary>
    /// Inserts a field at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index to insert at.</param>
    /// <param name="item">The field to insert.</param>
    public void Insert(int index, NamedField item) => _items.Insert(index, item);

    /// <summary>
    /// Removes a field from this list.
    /// </summary>
    /// <param name="item">The field to remove.</param>
    /// <returns>True if the field was removed; otherwise, false.</returns>
    public bool Remove(NamedField item) => _items.Remove(item);

    /// <summary>
    /// Removes all fields from this list.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Checks if a field with the specified name exists in this list.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if a field with the name exists; otherwise, false.</returns>
    public bool Contains(string name) => _items.ContainsName(name);

    /// <summary>
    /// Checks if the specified field exists in this list.
    /// </summary>
    /// <param name="item">The field to search for.</param>
    /// <returns>True if the field exists; otherwise, false.</returns>
    public bool Contains(NamedField item) => _items.Contains(item);

    /// <summary>
    /// Gets a field by name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The found <see cref="NamedField"/>, or null if not found.</returns>
    public NamedField GetItem(string name) => _items.GetValueOrDefault(name);

    /// <summary>
    /// Gets the count of fields in this list.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets the zero-based index of the specified field.
    /// </summary>
    /// <param name="item">The field to find.</param>
    /// <returns>The index of the field, or -1 if not found.</returns>
    public int IndexOf(NamedField item) => _items.IndexOf(item);

    /// <summary>
    /// Generates a suggested name for a field with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated name.</param>
    /// <param name="digiLen">The length of the numeric suffix (default is 2).</param>
    /// <returns>A suggested name string.</returns>
    public string GetSuggestedFieldName(string prefix, int digiLen = 2)
    {
        return _items.GetSuggestedName(prefix, digiLen);
    }

    /// <summary>
    /// Changes the name of a field in this list.
    /// </summary>
    /// <param name="item">The field to rename.</param>
    /// <param name="newName">The new name.</param>
    /// <returns>True if the rename was successful; otherwise, false.</returns>
    internal bool ChangeName(NamedField item, string newName)
    {
        string oldName = item.Name;

        if (_items.ChangeName(item, newName, true))
        {
            item.UpdateName(newName);
            OnItemRenamed(item, oldName);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Arranges the items in this list.
    /// </summary>
    public void ArrangeItem()
    {
        OnArrangeItem();
    }

    /// <summary>
    /// Creates a default field item.
    /// </summary>
    /// <returns>The newly created <see cref="NamedField"/>, or null if not supported.</returns>
    public NamedField CreateDefaultItem() => OnCreateNewItem();

    /// <summary>
    /// Shows a GUI for creating a new field.
    /// </summary>
    /// <param name="typeHint">A type hint for the item to create.</param>
    /// <returns>The created <see cref="NamedField"/>.</returns>
    public Task<NamedField> GuiCreateItemAsync(Type typeHint) => OnGuiCreateItemAsync(typeHint);

    #region Virtual

    /// <summary>
    /// Creates a new field item.
    /// </summary>
    /// <returns>The newly created <see cref="NamedField"/>, or null if not supported.</returns>
    protected virtual NamedField OnCreateNewItem()
    {
        if (_itemType != null)
        {
            return (NamedField)_itemType.CreateInstanceOf();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Shows a GUI for creating a new field.
    /// </summary>
    /// <param name="typeHint">A type hint for the item to create.</param>
    /// <returns>The created <see cref="NamedField"/>.</returns>
    protected virtual Task<NamedField> OnGuiCreateItemAsync(Type typeHint)
    {
        if (_itemType != null)
        {
            return Task.FromResult<NamedField>((NamedField)_itemType.CreateInstanceOf());
        }
        else
        {
            return Task.FromResult<NamedField>(null);
        }
    }

    /// <summary>
    /// Called when a field is added to this list.
    /// </summary>
    /// <param name="item">The field that was added.</param>
    /// <param name="isNew">Whether the field is newly created.</param>
    protected virtual void OnItemAdded(NamedField item, bool isNew)
    {
        _parentItem?.OnFieldListItemAdded(this, item, isNew);
    }

    /// <summary>
    /// Called when a field is removed from this list.
    /// </summary>
    /// <param name="item">The field that was removed.</param>
    protected virtual void OnItemRemoved(NamedField item)
    {
        _parentItem?.OnFieldListItemRemoved(this, item);
    }

    /// <summary>
    /// Called when a field in this list is updated.
    /// </summary>
    /// <param name="item">The field that was updated.</param>
    /// <param name="forceUpdate">Whether to force a full update.</param>
    protected internal virtual void OnItemUpdated(NamedField item, bool forceUpdate)
    {
        _parentItem?.OnFieldListItemUpdated(this, item, forceUpdate);
    }

    /// <summary>
    /// Called when a field in this list is renamed.
    /// </summary>
    /// <param name="item">The field that was renamed.</param>
    /// <param name="oldName">The previous name.</param>
    protected virtual void OnItemRenamed(NamedField item, string oldName)
    {
        _parentItem?.OnFieldListItemRenamed(this, item, oldName);
    }

    /// <summary>
    /// Called to arrange items in this list.
    /// </summary>
    protected virtual void OnArrangeItem()
    {
        _parentItem?.OnFieldListArrageItem(this);
    }

    /// <summary>
    /// Checks if the specified field can be added to this list.
    /// </summary>
    /// <param name="item">The field to check.</param>
    /// <returns>True if the field can be added; otherwise, false.</returns>
    protected virtual bool OnDropInCheck(NamedField item)
    {
        if (item is null)
        {
            return false;
        }

        if (_itemType != null)
        {
            return _itemType.IsAssignableFrom(item.GetType());
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the display text for this list.
    /// </summary>
    protected virtual string OnGetText() => this.GetType().ToDisplayText();

    /// <summary>
    /// Gets the icon to display for this list.
    /// </summary>
    protected virtual Image OnGetIcon() => this.GetType().ToDisplayIcon() ?? CoreIconCache.Field;

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    protected virtual TextStatus OnGetTextStatus() => TextStatus.Normal;

    #endregion

    #region IViewList

    /// <summary>
    /// Gets or sets the list view identifier.
    /// </summary>
    public int ListViewId { get; set; } = ViewIds.TreeView;

    /// <summary>
    /// Synchronizes this list.
    /// </summary>
    /// <param name="sync">The index synchronizer.</param>
    /// <param name="context">The sync context.</param>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        _items.Sync(sync, context);

        if (sync.IsSetter())
        {
            OnArrangeItem();
        }
    }

    /// <summary>
    /// Checks if the specified value can be dropped into this list.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value can be dropped in; otherwise, false.</returns>
    bool IDropInCheck.DropInCheck(object value)
    {
        NamedField item = value as NamedField;
        if (item is null)
        {
            return false;
        }

        return OnDropInCheck(item);
    }

    /// <summary>
    /// Converts a dropped value into a compatible item.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted object.</returns>
    object IDropInCheck.DropInConvert(object value) => value;

    #endregion

    #region IEnumerable

    /// <summary>
    /// Returns an enumerator that iterates through the fields in this list.
    /// </summary>
    IEnumerator<NamedField> IEnumerable<NamedField>.GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the fields in this list.
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();

    #endregion

    #region ITextDisplay

    /// <summary>
    /// Gets the display text for this list.
    /// </summary>
    string ITextDisplay.DisplayText => OnGetText();

    /// <summary>
    /// Gets the display icon for this list.
    /// </summary>
    object ITextDisplay.DisplayIcon => OnGetIcon();

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    TextStatus ITextDisplay.DisplayStatus => OnGetTextStatus();

    #endregion

    #region ISyncTypeResolver

    /// <summary>
    /// Resolves the type name for display purposes.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <param name="obj">The object instance.</param>
    /// <returns>The resolved type name string.</returns>
    public virtual string ResolveTypeName(Type type, object obj)
        => NamedExternal._external.ResolveTypeName(_itemType, type, obj);

    /// <summary>
    /// Resolves a type from its name and parameter.
    /// </summary>
    /// <param name="typeName">The type name to resolve.</param>
    /// <param name="parameter">Additional parameter for resolution.</param>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    public virtual Type ResolveType(string typeName, string parameter)
        => NamedExternal._external.ResolveType(_itemType, typeName, parameter);

    /// <summary>
    /// Resolves an object from its type name and parameter.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="parameter">Additional parameter.</param>
    /// <returns>The resolved object, or null.</returns>
    public virtual object ResolveObject(string typeName, string parameter) => null;

    /// <summary>
    /// Resolves the object value as a string.
    /// </summary>
    /// <param name="obj">The object to resolve.</param>
    /// <returns>The resolved string value, or null.</returns>
    public virtual string ResolveObjectValue(object obj) => null;

    /// <summary>
    /// Creates a proxy for the specified object.
    /// </summary>
    /// <param name="obj">The object to create a proxy for.</param>
    /// <returns>The proxy object, or null.</returns>
    public virtual object CreateProxy(object obj) => null;

    #endregion

    #region ISyncPathObject

    /// <summary>
    /// Gets the synchronization path for this list.
    /// </summary>
    public virtual SyncPath GetPath() 
        => _parentItem?.GetPath().Append("Fields") ?? SyncPath.Empty;

    /// <summary>
    /// Gets the synchronization path for the specified field.
    /// </summary>
    /// <param name="item">The field to get the path for.</param>
    /// <returns>The synchronization path.</returns>
    public SyncPath GetPath(NamedField item) => GetPath().Append(IndexOf(item));

    #endregion
}

/// <summary>
/// Generic version of <see cref="NamedFieldList"/> for a specific field type.
/// </summary>
/// <typeparam name="T">The type of fields in the list, must extend <see cref="NamedField"/>.</typeparam>
public class NamedFieldList<T> : NamedFieldList
    where T : NamedField
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList{T}"/> class.
    /// </summary>
    public NamedFieldList()
        : base(typeof(T))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedFieldList{T}"/> class with the specified parent item.
    /// </summary>
    /// <param name="parentItem">The parent item that owns this list.</param>
    public NamedFieldList(NamedItem parentItem)
        : base(parentItem, typeof(T))
    {
    }
}
