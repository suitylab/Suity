using Suity.Collections;
using Suity.Editor;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Selecting;

/// <summary>
/// Abstract base class for selection nodes that implement both <see cref="ISelectionNode"/> and display interfaces.
/// </summary>
public abstract class BaseSelectionNode :
    ISelectionNode,
    INamed,
    ITextDisplay
{
    #region ISelectionItem

    /// <summary>
    /// Gets the unique key for this selection item.
    /// </summary>
    public virtual string SelectionKey => null;

    /// <summary>
    /// Gets the name of this selection node.
    /// </summary>
    public virtual string Name => null;

    /// <summary>
    /// Gets the display text for this selection node.
    /// </summary>
    public virtual string DisplayText => null;

    /// <summary>
    /// Gets the display icon for this selection node.
    /// </summary>
    public virtual object DisplayIcon => null;


    /// <summary>
    /// Gets a value indicating whether this node can be selected.
    /// </summary>
    public virtual bool Selectable => false;

    #endregion

    #region ISelectionList

    /// <summary>
    /// Gets all child selection items in this node.
    /// </summary>
    public virtual IEnumerable<ISelectionItem> GetItems() => [];

    /// <summary>
    /// Gets a child selection item by its key.
    /// </summary>
    public virtual ISelectionItem GetItem(string key) => null;

    #endregion

    /// <summary>
    /// Gets the display status of this selection node.
    /// Returns <see cref="TextStatus.Normal"/> if selectable or has items, otherwise <see cref="TextStatus.Disabled"/>.
    /// </summary>
    public virtual TextStatus DisplayStatus
    {
        get
        {
            if (Selectable)
            {
                return TextStatus.Normal;
            }

            return GetItems().Any() ? TextStatus.Normal : TextStatus.Disabled;
        }
    }
}

/// <summary>
/// A concrete selection node that stores selection items with optional visibility conditions.
/// </summary>
public class SelectionNode : BaseSelectionNode
{
    private class SelectionStore(ISelectionItem item, Predicate<ISelectionItem> condition = null)
    {
        /// <summary>
        /// Gets the selection item.
        /// </summary>
        public ISelectionItem Item { get; } = item;
        /// <summary>
        /// Gets the condition predicate that determines if the item should be displayed.
        /// </summary>
        public Predicate<ISelectionItem> Condition { get; } = condition;
    }

    private readonly string _selectionKey;
    private readonly string _displayText;
    private readonly Image _icon;

    private readonly Dictionary<string, SelectionStore> _items = [];

    /// <summary>
    /// Initializes a new instance of <see cref="SelectionNode"/>.
    /// </summary>
    /// <param name="key">The unique selection key.</param>
    /// <param name="displayText">The display text for this node.</param>
    /// <param name="icon">The icon for this node.</param>
    public SelectionNode(string key = null, string displayText = null, Image icon = null)
    {
        _selectionKey = key;
        _displayText = displayText;
        _icon = icon;
    }

    /// <summary>
    /// Adds a selection item to this node with an optional visibility condition.
    /// </summary>
    /// <param name="item">The selection item to add.</param>
    /// <param name="condition">Optional predicate to determine if the item should be displayed.</param>
    public void Add(ISelectionItem item, Predicate<ISelectionItem> condition = null)
    {
        if (_items.ContainsKey(item.SelectionKey))
        {
            throw new InvalidOperationException($"SelectionKey is already added : {item.SelectionKey}");
        }

        var store = new SelectionStore(item, condition);
        _items.Add(item.SelectionKey, store);
    }

    /// <summary>
    /// Adds a range of selection items to this node with an optional visibility condition.
    /// </summary>
    /// <param name="items">The collection of selection items to add.</param>
    /// <param name="condition">Optional predicate to determine if items should be displayed.</param>
    public void AddRange(IEnumerable<ISelectionItem> items, Predicate<ISelectionItem> condition = null)
    {
        foreach (var item in items)
        {
            Add(item, condition);
        }
    }

    /// <summary>
    /// Gets the unique key for this selection node.
    /// </summary>
    public override string SelectionKey => _selectionKey;

    /// <summary>
    /// Gets the display text for this selection node.
    /// </summary>
    public override string DisplayText => _displayText;

    /// <summary>
    /// Gets the display icon for this selection node.
    /// </summary>
    public override object DisplayIcon => _icon;

    #region ISelectionList

    /// <summary>
    /// Gets all child selection items in this node, filtering by their visibility conditions.
    /// </summary>
    public override IEnumerable<ISelectionItem> GetItems()
    {
        foreach (var store in _items.Values)
        {
            if (store.Condition is { } condition) 
            {
                try
                {
                    if (!condition(store.Item))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                }
            }

            yield return store.Item;
        }
    }

    /// <summary>
    /// Gets a child selection item by its key.
    /// </summary>
    public override ISelectionItem GetItem(string key)
        => _items.GetValueSafe(key)?.Item;

    #endregion
}

/// <summary>
/// A selection node that displays assets of a specific type <typeparamref name="T"/>.
/// </summary>
public class AssetSelectionNode<T> : BaseSelectionNode
{
    private readonly IAssetFilter _filter;
    private IGeneralAssetCollection _collection;

    /// <summary>
    /// Gets the unique key for this selection node based on the type full name.
    /// </summary>
    public override string SelectionKey => typeof(T).FullName;

    /// <summary>
    /// Gets the name of this selection node based on the type name.
    /// </summary>
    public override string Name => typeof(T).Name;

    /// <summary>
    /// Gets the display text for this selection node.
    /// </summary>
    public override string DisplayText => typeof(T).ToDisplayText();

    /// <summary>
    /// Gets the display icon for this selection node.
    /// </summary>
    public override object DisplayIcon => typeof(T).ToDisplayText();

    /// <summary>
    /// Initializes a new instance of <see cref="AssetSelectionNode{T}"/>.
    /// </summary>
    /// <param name="filter">Optional filter to apply to the assets.</param>
    public AssetSelectionNode(IAssetFilter filter = null)
    {
        _filter = filter;
    }

    /// <summary>
    /// Gets a selection item by its key from the asset collection.
    /// </summary>
    public override ISelectionItem GetItem(string key)
      => GetCollection().GetAsset(key, _filter);

    /// <summary>
    /// Gets all selection items from the asset collection, filtered and ordered by asset key.
    /// </summary>
    public override IEnumerable<ISelectionItem> GetItems()
        => GetCollection().Assets
        .Where(o => _filter?.FilterAsset(o) ?? true)
        .OrderBy(o => o.AssetKey);

    /// <summary>
    /// Gets or creates the asset collection for type <typeparamref name="T"/>.
    /// </summary>
    private IGeneralAssetCollection GetCollection()
        => _collection ??= AssetManager.Instance.GetAssetCollection<T>();
}

/// <summary>
/// A selection node that displays assets of type <typeparamref name="T"/> grouped under type <typeparamref name="TGroup"/>.
/// </summary>
public class AssetSelectionNode<TGroup, T> : BaseSelectionNode
{
    private readonly IAssetFilter _filter;
    private readonly Dictionary<Asset, ISelectionItem> _items = [];
    private readonly bool _groupSelectable;

    private IGeneralAssetCollection _collection;

    /// <summary>
    /// Initializes a new instance of <see cref="AssetSelectionNode{TGroup, T}"/>.
    /// </summary>
    /// <param name="filter">Optional filter to apply to the assets.</param>
    /// <param name="groupSelectable">Whether the group node itself is selectable.</param>
    public AssetSelectionNode(IAssetFilter filter = null, bool groupSelectable = false)
    {
        _filter = filter;
        _groupSelectable = groupSelectable;
    }

    /// <summary>
    /// Gets the unique key for this selection node based on the group type full name.
    /// </summary>
    public override string SelectionKey => typeof(TGroup).FullName;

    /// <summary>
    /// Gets the name of this selection node based on the group type name.
    /// </summary>
    public override string Name => typeof(TGroup).Name;

    /// <summary>
    /// Gets the display text for this selection node.
    /// </summary>
    public override string DisplayText => typeof(TGroup).ToDisplayText();

    /// <summary>
    /// Gets the display icon for this selection node.
    /// </summary>
    public override object DisplayIcon => typeof(TGroup).ToDisplayText();


    /// <summary>
    /// Gets a selection item by its key from the asset collection.
    /// </summary>
    public override ISelectionItem GetItem(string key)
    {
        var asset = GetCollection().GetAsset(key, _filter);

        return GetOrCreateSelectionItem(asset);
    }

    /// <summary>
    /// Gets all selection items from the asset collection, filtered and ordered by asset key.
    /// </summary>
    public override IEnumerable<ISelectionItem> GetItems()
        => GetCollection().Assets
        .Where(o => _filter?.FilterAsset(o) ?? true)
        .OrderBy(o => o.AssetKey)
        .Select(GetOrCreateSelectionItem)
        .SkipNull();

    /// <summary>
    /// Gets or creates the asset collection for type <typeparamref name="TGroup"/>.
    /// </summary>
    private IGeneralAssetCollection GetCollection()
        => _collection ??= AssetManager.Instance.GetAssetCollection<TGroup>();

    /// <summary>
    /// Gets or creates a selection item for the specified asset.
    /// Creates a <see cref="GroupAssetSelectionNode{T}"/> for group assets, or returns the asset directly if it matches type <typeparamref name="T"/>.
    /// </summary>
    private ISelectionItem GetOrCreateSelectionItem(Asset asset)
    {
        if (asset is null)
        {
            return null;
        }

        return _items.GetOrAdd(asset, o =>
        {
            if (asset is GroupAsset groupAsset)
            {
                return new GroupAssetSelectionNode<T>(groupAsset, _filter, _groupSelectable);
            }
            else if (asset is T)
            {
                return asset;
            }
            else
            {
                return null;
            }
        });
    }
}

/// <summary>
/// A selection item that represents a <see cref="Type"/> with display and preview capabilities.
/// </summary>
public class TypedSelectionItem : ISelectionItem, ITextDisplay, IPreviewDisplay
{
    private readonly Type _type;

    private readonly Predicate<Type> _condition;

    /// <summary>
    /// Gets the type that this selection item represents.
    /// </summary>
    public Type Type => _type;

    /// <summary>
    /// Initializes a new instance of <see cref="TypedSelectionItem"/>.
    /// </summary>
    /// <param name="type">The type to represent.</param>
    /// <param name="condition">Optional predicate to determine if the item should be enabled.</param>
    public TypedSelectionItem(Type type, Predicate<Type> condition = null)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _condition = condition;
    }

    /// <summary>
    /// Gets the unique key for this selection item based on the type full name.
    /// </summary>
    public virtual string SelectionKey => _type.FullName;

    /// <summary>
    /// Gets the display text for this selection item.
    /// </summary>
    public virtual string DisplayText => _type.ToDisplayText() ?? _type.Name;

    /// <summary>
    /// Gets the display icon for this selection item.
    /// </summary>
    public virtual object DisplayIcon => _type.ToDisplayIcon();

    /// <summary>
    /// Gets the display status of this selection item.
    /// Returns <see cref="TextStatus.Disabled"/> if the condition predicate evaluates to false, otherwise <see cref="TextStatus.Normal"/>.
    /// </summary>
    public virtual TextStatus DisplayStatus
    {
        get
        {
            if (_condition != null)
            {
                try
                {
                    return _condition(_type) ? TextStatus.Normal : TextStatus.Disabled;
                }
                catch (Exception)
                {
                    return TextStatus.Disabled;
                }
            }
            else
            {
                return TextStatus.Normal;
            }
        }
    }

    /// <summary>
    /// Gets the preview text for this selection item.
    /// </summary>
    public virtual string PreviewText => _type.ToPreviewText() ?? _type.ToToolTipsText() ?? string.Empty;

    /// <summary>
    /// Gets the preview icon for this selection item (always null).
    /// </summary>
    public virtual object PreviewIcon => null;
}

/// <summary>
/// A generic selection item that represents a type <typeparamref name="T"/> with display and preview capabilities.
/// </summary>
public class TypedSelectionItem<T> : TypedSelectionItem
where T : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="TypedSelectionItem{T}"/>.
    /// </summary>
    /// <param name="condition">Optional predicate to determine if the item should be enabled.</param>
    public TypedSelectionItem(Predicate<Type> condition = null) : base(typeof(T), condition)
    {
    }
}