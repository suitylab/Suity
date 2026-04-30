using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Represents a root collection of named items in a document.
/// </summary>
public class SNamedRootCollection : NamedRootCollection, 
    IViewRedirect,
    IHasObjectCreationGUI
{
    private readonly Dictionary<string, ObjectCreationOption> _itemTypes = [];

    private readonly SNamedDocument _document;
    private string _fieldName = "Model";
    private string _fieldDescription = null;
    private Image _fieldIcon;
    private SyncPath _rootPath = SyncPath.Empty;
    private readonly AssetBuilder _builder;

    public SNamedRootCollection(SNamedDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _rootPath = new SyncPath(_fieldName);
    }

    public SNamedRootCollection(SNamedDocument document, AssetBuilder builder)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _builder.Owner = document;
        _rootPath = new SyncPath(_fieldName);
    }

    /// <summary>
    /// Gets the document.
    /// </summary>
    public SNamedDocument Document => _document;

    /// <summary>
    /// Gets the asset builder.
    /// </summary>
    protected internal AssetBuilder AssetBuilder => _builder;

    /// <summary>
    /// Gets or sets whether to auto manage group assets.
    /// </summary>
    protected bool AutoManageGroupAsset { get; set; } = true;

    public override string Name => FieldName;

    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string FieldName
    {
        get => _fieldName;
        set
        {
            if (_fieldName == value)
            {
                return;
            }
            if (string.IsNullOrEmpty(_fieldName))
            {
                throw new ArgumentNullException(nameof(value));
            }

            _fieldName = value;
            _rootPath = new SyncPath(_fieldName);
        }
    }

    /// <summary>
    /// Gets or sets the field description.
    /// </summary>
    public string FieldDescription
    {
        get => _fieldDescription;
        set => _fieldDescription = value;
    }

    /// <summary>
    /// Gets or sets the field icon.
    /// </summary>
    public Image FieldIcon
    {
        get => _fieldIcon;
        set => _fieldIcon = value;
    }

    /// <summary>
    /// Adds an item type to the collection.
    /// </summary>
    /// <param name="type">The item type.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="creation">The GUI object creation.</param>
    public void AddItemType(Type type, string displayName = null, GuiObjectCreation creation = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!typeof(NamedItem).IsAssignableFrom(type))
        {
            throw new ArgumentException($"{type.Name} must inherits from {nameof(NamedItem)}.");
        }

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = type.Name;
        }

        var option = new ObjectCreationOption(type, displayName, creation);

        _itemTypes.Add(displayName, option);
    }

    /// <summary>
    /// Adds an item type to the collection.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    /// <param name="creation">The GUI object creation.</param>
    public void AddItemType<T>(string displayName = null, GuiObjectCreation creation = null) where T : NamedItem
    {
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = typeof(T).Name;
        }

        var option = new ObjectCreationOption(typeof(T), displayName, creation);

        _itemTypes.Add(displayName, option);
    }

    /// <summary>
    /// Checks if the value type matches any registered item type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if type matches.</returns>
    public bool IsItemTypeMatched(object value)
    {
        if (value is null)
        {
            return false;
        }

        Type valueType = value.GetType();

        foreach (var option in _itemTypes.Values)
        {
            if (option.Type?.IsAssignableFrom(valueType) == true)
            {
                return true;
            }
        }

        return false;
    }

    protected internal override void OnItemAdded(NamedItem item, bool isNew)
    {
        base.OnItemAdded(item, isNew);

        if (AutoManageGroupAsset)
        {
            var op = item as IHasAssetBuilder;
            if (op?.TargetAssetBuilder != null)
            {
                (_builder as IGroupAssetBuilder)?.AddOrUpdate(op.TargetAssetBuilder, isNew ? IdResolveType.New : IdResolveType.Auto);
            }
        }

        _document.OnItemAdded(this, item, isNew);
    }

    protected internal override void OnItemRemoved(NamedItem item)
    {
        base.OnItemRemoved(item);

        if (AutoManageGroupAsset)
        {
            if (item is IHasAssetBuilder op && op.TargetAssetBuilder != null)
            {
                (_builder as IGroupAssetBuilder)?.Remove(op.TargetAssetBuilder);
            }
        }

        _document.OnItemRemoved(this, item);
    }

    protected internal override void OnItemRenamed(NamedItem item, string oldName)
    {
        base.OnItemRenamed(item, oldName);

        _document.OnItemRenamed(this, item, oldName);
    }

    protected internal override NamedItem OnCreateDefaultItem(INamedNode parentNode)
    {
        var p = parentNode as NamedNode;

        var item = NamedExternal._external.CreateDefaultItem(p,
            () => _document.OnCreateDefaultItem(this));

        return item;
    }

    protected internal override Task<NamedItem[]> OnGuiCreateItems(INamedNode parentNode, Type type)
    {
        var p = parentNode as NamedNode;

        return NamedExternal._external.GuiCreateItem(p,
            () => _document.OnGuiCreateItems(this, type),
            item => _document.OnGuiConfigNewItem(this, parentNode, item));
    }

    protected internal override bool OnDropInCheck(object value)
    {
        if (value is null)
        {
            return false;
        }

        return _document.OnDropInCheck(this, value);
    }

    protected internal override object OnDropInConvert(object value) => _document.OnDropInConvert(this, value);

    internal virtual void InternalCreate(string fullPath)
    { }

    protected internal override string OnGetSuggestedName(string prefix, int digiLen = 2)
        => _document.OnGetSuggestedName(this, prefix, digiLen);

    protected internal override string OnResolveConflictName(string name) 
        => _document.OnResolveConflictName(this, name);

    public override SyncPath GetPath() => _rootPath;

    protected internal override string OnGetText() => _fieldDescription ?? _fieldName ?? string.Empty;

    protected internal override Image OnGetIcon() => _fieldIcon ?? base.OnGetIcon();

    #region IViewRedirect
    object IViewRedirect.GetRedirectedObject(int viewId)
    {
        if (viewId == ViewIds.MainTreeView)
        {
            return this;
        }

        return null;
    }

    #endregion

    #region IHasObjectCreationGUI

    /// <summary>
    /// Gets the creation options.
    /// </summary>
    public IEnumerable<ObjectCreationOption> CreationOptions => _itemTypes.Values;

    /// <summary>
    /// Creates an object using GUI.
    /// </summary>
    /// <param name="typeHint">The type hint.</param>
    /// <returns>The created object.</returns>
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        return await OnGuiCreateItems(null, typeHint);
    }

    #endregion
}