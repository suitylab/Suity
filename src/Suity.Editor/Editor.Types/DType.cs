using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Selecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Editor type
/// </summary>
[DisplayText("Design Type")]
public abstract class DType : GroupAsset,
    IFieldGroup<DField>,
    IAttributeGetter,
    IHasToolTips,
    ICodeRenderElement
{
    private object _bindingInfo;

    private IRegistryHandle<DType> _dtypeEntry;
    private TypeDefinition _definition;
    internal IAttributeGetter _attributes = EmptyAttributeGetter.Empty;
    //private Color? _attrColor;

    private DTypeFieldSelectionNode _selectionNode;

    public DType()
    { }

    public DType(string name)
        : base(name)
    {
    }

    #region Infos

    /// <summary>
    /// Gets or sets the binding info for this type.
    /// </summary>
    public object BindingInfo
    {
        get => _bindingInfo;
        internal protected set
        {
            if (_bindingInfo == value)
            {
                return;
            }
            _bindingInfo = value;
            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Gets the attributes associated with this type.
    /// </summary>
    public virtual IAttributeGetter Attributes => _attributes;

    internal protected void UpdateAttributes(IAttributeGetter value)
    {
        if (ReferenceEquals(_attributes, value))
        {
            return;
        }

        UpdateAttributes(value, true);
    }

    internal virtual void UpdateAttributes(IAttributeGetter value, bool notify)
    {
        _attributes = value ?? EmptyAttributeGetter.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        //_attrColor = _attributes.GetAttribute<IViewColor>()?.ViewColor;
        //if (_attrColor == Color.Empty)
        //{
        //    _attrColor = null;
        //}

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }

    /// <summary>
    /// Gets the type relationship.
    /// </summary>
    public virtual TypeRelationships Relationship => TypeRelationships.None;

    /// <summary>
    /// Gets whether this is a native type.
    /// </summary>
    public virtual bool IsNative => false;

    /// <summary>
    /// Gets whether this is a value type.
    /// </summary>
    public bool IsValueType => Relationship == TypeRelationships.Value;

    /// <summary>
    /// Gets whether this is a primitive type.
    /// </summary>
    public virtual bool IsPrimitive => false;

    /// <summary>
    /// Gets whether this is a numeric type.
    /// </summary>
    public virtual bool IsNumeric => false;

    /// <summary>
    /// Gets the type category color.
    /// </summary>
    public virtual Color? TypeColor => null;

    #endregion

    #region IViewColor

    //public override Color? ViewColor
    //{
    //    get => _attrColor ?? base.ViewColor;
    //    internal protected set => base.ViewColor = value;
    //}

    #endregion

    #region IFieldGroup

    /// <summary>
    /// Gets the field objects.
    /// </summary>
    public virtual IEnumerable<EditorObject> FieldObjects => [];

    /// <summary>
    /// Gets a field object by name.
    /// </summary>
    public virtual EditorObject GetFieldObject(string name) => null;

    #endregion

    #region IFieldGroup<DField>

    /// <summary>
    /// Gets a field ID by name.
    /// </summary>
    public virtual Guid GetFieldId(string name) => GetField(name)?.Id ?? Guid.Empty;

    /// <summary>
    /// Gets a field by name.
    /// </summary>
    public virtual DField GetField(string name) => null;

    /// <summary>
    /// Gets a field by ID.
    /// </summary>
    public virtual DField GetField(Guid id) => null;

    /// <summary>
    /// Gets all fields.
    /// </summary>
    public virtual IEnumerable<DField> Fields => [];

    /// <summary>
    /// Gets the field count.
    /// </summary>
    public virtual int FieldCount => 0;

    #endregion

    #region Field

    /// <summary>
    /// Gets whether this type uses native fields.
    /// </summary>
    public virtual bool UseNativeFields => false;

    /// <summary>
    /// Gets the first field.
    /// </summary>
    public virtual DField FirstField => null;

    /// <summary>
    /// Gets a public field by name.
    /// </summary>
    public DField GetPublicField(string name) => GetField(name)?.CheckPublic();

    /// <summary>
    /// Gets a public field by ID.
    /// </summary>
    public DField GetPublicField(Guid id) => GetField(id)?.CheckPublic();

    /// <summary>
    /// Gets all public fields.
    /// </summary>
    public IEnumerable<DField> PublicFields => Fields.Where(o => o.AccessMode == AssetAccessMode.Public);

    /// <summary>
    /// Gets the display text for a field.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The display text.</returns>
    public string GetFieldDisplayText(string name)
    {
        DField field = GetField(name);
        return field != null ? field.DisplayText : name;
    }

    #endregion

    #region Type

    /// <summary>
    /// Type definition
    /// </summary>
    public TypeDefinition Definition => _definition;

    /// <summary>
    /// Element type, used in DAbstractFunction
    /// </summary>
    public virtual TypeDefinition ElementTypeDefinition => null;

    /// <summary>
    /// Abstract base type
    /// </summary>
    public virtual TypeDefinition BaseTypeDefinition => null;

    /// <summary>
    /// Primary type
    /// </summary>
    public virtual TypeDefinition PrimaryTypeDefinition => null;

    /// <summary>
    /// Native type
    /// </summary>
    public virtual Type NativeType => null;

    #endregion

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => Attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => Attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => Attributes.GetAttributes<T>();

    #endregion

    #region IHasToolTips

    public string ToolTips => Attributes.GetAttribute<ToolTipsAttribute>()?.ToolTips;

    #endregion

    /// <summary>
    /// Determines whether this type is assignable from the specified type.
    /// </summary>
    /// <param name="implementType">The type to check.</param>
    /// <returns>True if assignable; otherwise, false.</returns>
    public virtual bool IsAssignableFrom(TypeDefinition implementType)
    {
        return Definition == implementType;
    }

    /// <inheritdoc />
    public override object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.TypeInfo:
                return Definition;

            case CodeRenderProperty.Attributes:
                if (argument is string typeName)
                {
                    return Attributes?.GetAttributes(typeName);
                }
                else if (argument is null)
                {
                    // Do a conversion here, get all attributes when no argument
                    return Attributes?.GetAttributes();
                }
                else
                {
                    return Array.Empty<object>();
                }
            default:
                return base.GetProperty(property, argument);
        }
    }

    /// <summary>
    /// Gets the selection node for this type.
    /// </summary>
    /// <returns>The selection node.</returns>
    public BaseSelectionNode GetDTypeSelectionNode()
    {
        return _selectionNode ??= new DTypeFieldSelectionNode(this);
    }

    /// <inheritdoc />
    internal override void InternalOnAssetActivate(string assetKey)
    {
        _dtypeEntry?.Dispose();
        _dtypeEntry = DTypeManager.Instance.AddType(this);
        _definition = this.MakeDefinition();

        base.InternalOnAssetActivate(assetKey);
    }

    /// <inheritdoc />
internal override void InternalOnAssetDeactivate(string assetKey)
    {
        var entry = _dtypeEntry;
        _dtypeEntry = null;
        entry?.Dispose();
        _definition = null;

        base.InternalOnAssetDeactivate(assetKey);
    }
}



/// <summary>
/// Interface for building struct types.
/// </summary>
public interface IStructBuilder : IDesignBuilder
{
    /// <summary>
    /// Adds or updates a field.
    /// </summary>
    void AddOrUpdateField(
        string name,
        TypeDefinition type,
        AssetAccessMode accessMode,
        object defaultValue,
        bool nullable,
        object bindingInfo,
        IAttributeGetter attribute,
        bool forceUpdate = false,
        bool isNew = false,
        Guid? recorededId = null);

    /// <summary>
    /// Updates field display properties.
    /// </summary>
    void UpdateFieldDisplay(string name,
        int index = -1,
        string description = null,
        string detail = null,
        string brief = null,
        string label = null,
        string unit = null,
        bool showInDetail = false);

    /// <summary>
    /// Renames a field.
    /// </summary>
    void RenameField(string oldName, string newName);

    /// <summary>
    /// Removes a field.
    /// </summary>
    void RemoveField(string name);

    /// <summary>
    /// Sorts fields.
    /// </summary>
    void Sort();
}

/// <summary>
/// Base class for building type instances.
/// </summary>
public abstract class DTypeBuilder<TType> : GroupAssetBuilder<TType>, IDesignBuilder
    where TType : DType, new()
{
    private object _bindingInfo;
    private IAttributeDesign _attribute = EmptyAttributeDesign.Empty;

    /// <summary>
    /// Initializes a new instance of the DTypeBuilder class.
    /// </summary>
    public DTypeBuilder()
    {
        AddAutoUpdate(nameof(DType.BindingInfo), o => o.BindingInfo = _bindingInfo);
        AddAutoUpdate(nameof(DType.Attributes), o => o.UpdateAttributes(_attribute, false));
    }

    #region IDesignBuilder

    /// <inheritdoc />
    public void SetBindingInfo(object bindingInfo)
    {
        _bindingInfo = bindingInfo;
        UpdateAuto(nameof(DType.BindingInfo));
    }

    /// <inheritdoc />
    public void UpdateAttributes(IAttributeDesign attributes)
    {
        _attribute = attributes ?? EmptyAttributeDesign.Empty;
        TryUpdateNow(d => d.UpdateAttributes(_attribute, true));
    }
    #endregion
}

/// <summary>
/// Selection node for DType fields.
/// </summary>
internal class DTypeFieldSelectionNode : BaseSelectionNode
{
    private readonly DType _asset;
    private readonly bool _publicOnly;

    /// <summary>
    /// Initializes a new instance of the DTypeFieldSelectionNode class.
    /// </summary>
    /// <param name="asset">The DType.</param>
    /// <param name="publicOnly">Whether to only show public fields.</param>
    public DTypeFieldSelectionNode(DType asset, bool publicOnly = true)
    {
        _asset = asset ?? throw new ArgumentNullException(nameof(asset));
        _publicOnly = publicOnly;
    }

    /// <inheritdoc />
    public override string SelectionKey => _asset._ex.AssetKey;

    /// <inheritdoc />
    public override object DisplayIcon => _asset.Icon;

    /// <inheritdoc />
    public override string DisplayText => _asset.DisplayText;

    /// <inheritdoc />
    public override IEnumerable<ISelectionItem> GetItems()
    {
        if (_publicOnly)
        {
            return _asset.PublicFields;
        }
        else
        {
            return _asset.Fields;
        }
    }

    /// <inheritdoc />
    public override ISelectionItem GetItem(string key)
    {
        var code = new FieldCode(key);

        if (string.IsNullOrEmpty(code.FieldName))
        {
            return null;
        }

        if (_publicOnly)
        {
            return _asset.GetPublicField(code.FieldName);
        }
        else
        {
            return _asset.GetField(code.FieldName);
        }
    }
}