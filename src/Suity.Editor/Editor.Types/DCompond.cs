using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Specifies the return type binding for functions.
/// </summary>
[Flags]
public enum DReturnTypeBinding
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Object.
    /// </summary>
    Object = 0x1,

    /// <summary>
    /// Array.
    /// </summary>
    Array = 0x2,
}

/// <summary>
/// Represents a composite type with fields.
/// </summary>
[DisplayText("Composite Design Type")]
public abstract class DCompond : DType, IFieldGroup<DStructField>
{
    /// <summary>
    /// Gets or sets whether to check field integrity.
    /// </summary>
    public static bool CheckFieldIntegrity = false;

    internal readonly FieldObjectCollection<DStructField> _fields;

    private bool _isValueStruct;

    private string _detail;
    private string _brief;

    /// <summary>
    /// Initializes a new instance of the DCompond class.
    /// </summary>
    protected DCompond()
    {
        _fields = EditorObjectManager.Instance.CreateFieldCollection<DStructField>(this);
    }

    /// <summary>
    /// Initializes a new instance of the DCompond class with a name.
    /// </summary>
    public DCompond(string name)
        : base(name)
    {
        _fields = EditorObjectManager.Instance.CreateFieldCollection<DStructField>(this);
    }

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.Struct;

    /// <summary>
    /// Gets or sets whether this is a value struct.
    /// </summary>
    public bool IsValueStruct
    {
        get => _isValueStruct;
        internal protected set
        {
            if (_isValueStruct == value)
            {
                return;
            }
            _isValueStruct = value;
            NotifyPropertyUpdated();
        }
    }

    public string Detail
    {
        get => _detail;
        internal protected set
        {
            if (_detail == value)
            {
                return;
            }
            _detail = value;
            NotifyPropertyUpdated(nameof(Detail));
        }
    }

    public string Brief
    {
        get => _brief;
        internal protected set
        {
            if (_brief == value)
            {
                return;
            }
            _brief = value;
            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Gets the base type.
    /// </summary>
    public virtual DCompond BaseType => null;

    /// <summary>
    /// Gets whether a formatter is required.
    /// </summary>
    public virtual bool RequireFormatter => false;

    #region Fields

    public override IEnumerable<EditorObject> FieldObjects => _fields.Fields;

    public override EditorObject GetFieldObject(string name) => _fields.GetField(name);

    public override DField GetField(string name) => _fields.GetField(name);

    public override DField GetField(Guid id) => _fields.GetField(id);

    public DStructField GetPublicStructField(Guid id) => _fields.GetField(id)?.CheckPublic();

    public DStructField GetPublicStructField(string name) => _fields.GetField(name)?.CheckPublic();

    public DStructField GetPublicStructFieldFromBase(Guid id)
    {
        var s = this;
        while (s != null)
        {
            var field = s.GetPublicStructField(id);
            if (field != null)
            {
                return field.CheckPublic();
            }

            s = s.BaseType;
        }

        return null;
    }

    public DStructField GetPublicStructFieldFromBase(string name)
    {
        var s = this;
        while (s != null)
        {
            var field = s.GetPublicStructField(name);
            if (field != null)
            {
                return field.CheckPublic();
            }

            s = s.BaseType;
        }

        return null;
    }

    public override IEnumerable<DField> Fields
        => _fields.Fields;

    public override int FieldCount
        => _fields.FieldCount;

    public virtual IEnumerable<DStructField> AllStructFields
        => _fields.Fields;

    public virtual IEnumerable<DStructField> PublicStructFields
        => _fields.Fields.Where(o => o.AccessMode == AssetAccessMode.Public);

    public virtual IEnumerable<DStructField> VisibleStructFields
        => _fields.Fields.Where(o => o.AccessMode == AssetAccessMode.Public && !o.IsHidden);

    public virtual IEnumerable<DStructField> PreviewStructFields
        => _fields.Fields.Where(o => o.AccessMode == AssetAccessMode.Public && o.IsPreview);

    public virtual IEnumerable<DStructField> GetPublicStructFields(bool includeBbaseFields)
        => PublicStructFields;

    protected internal DStructField AddOrUpdateField(
        string name,
        TypeDefinition type,
        AssetAccessMode accessMode,
        object defaultValue,
        bool isNullable,
        object bindingInfo,
        IAttributeGetter attribute,
        bool forceUpdate = false,
        Guid? recorededId = null,
        IdResolveType resolveType = IdResolveType.Auto)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is empty.", nameof(name));
        }

        DStructField field = _fields.GetOrAddField(name, resolveType, recorededId);
        bool modified = false;

        field.FieldType = type;

        // Object comparison should use Equals
        if (!ValueEqualUtility.ValueEquals(field._defaultValue, defaultValue))
        {
            // The default value will not be updated for now
            field._defaultValue = defaultValue;
            modified = true;
        }

        if (field._optional != isNullable)
        {
            field._optional = isNullable;
            modified = true;
        }

        if (field._bindingInfo != bindingInfo)
        {
            field._bindingInfo = bindingInfo;
            modified = true;
        }

        if (field._accessMode != accessMode)
        {
            field._accessMode = accessMode;
            modified = true;
        }

        // Forced update required
        field.UpdateAttributes(attribute, true);

        if (modified || forceUpdate)
        {
            field.NotifyUpdated();
        }

        return field;
    }

    protected internal void UpdateFieldDisplay(
        string name,
        int index,
        string description = null,
        string detail = null,
        string brief = null,
        string label = null,
        string unit = null,
        bool showInDetail = false)
    {
        var field = _fields.GetField(name);

        if (field != null)
        {
            field._index = index;
            field._description = description;
            field._detail = detail;
            field._brief = brief;
            field._label = label;
            field._unit = unit;
            field._showInDetail = showInDetail;
        }
    }

    protected internal DStructField RenameField(string oldName, string newName)
    {
        return _fields.RenameField(oldName, newName);
    }

    protected internal bool RemoveField(string name)
    {
        return _fields.RemoveField(name);
    }

    protected internal void ClearFields()
    {
        _fields.Clear();
    }

    protected internal void SortFields()
    {
        _fields.Sort((v1, v2) => v1._index.CompareTo(v2._index));
    }

    #endregion

    #region IFieldGroup<DObjectField>

    IEnumerable<DStructField> IFieldGroup<DStructField>.Fields => _fields.Fields;

    int IFieldGroup<DStructField>.FieldCount => _fields.FieldCount;

    Guid IFieldGroup<DStructField>.GetFieldId(string name)
    {
        return _fields.GetField(name)?.Id ?? Guid.Empty;
    }

    DStructField IFieldGroup<DStructField>.GetField(string name)
    {
        return _fields.GetField(name);
    }

    DStructField IFieldGroup<DStructField>.GetField(Guid id)
    {
        return _fields.GetField(id);
    }

    #endregion

    public override object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.ChildNodes:
                return _fields.Fields;

            default:
                return base.GetProperty(property, argument);
        }
    }
}

public abstract class DBaseStructBuilder<TType> : DTypeBuilder<TType>, IStructBuilder
    where TType : DCompond, new()
{
    private readonly IAssetFieldCollector<DStructField> _fieldCollector;
    private bool _isValueStruct;
    private string _detail;
    private string _brief;

    public DBaseStructBuilder(bool fieldCollector = true)
    {
        AddAutoUpdate(nameof(DCompond.IsValueStruct), o => o.IsValueStruct = _isValueStruct);
        AddAutoUpdate(nameof(DCompond.Detail), o => o.Detail = _detail);
        AddAutoUpdate(nameof(DCompond.Brief), o => o.Brief = _brief);

        if (fieldCollector)
        {
            _fieldCollector = AddFieldCollector(
                nameof(DCompond.Fields),
                type => type._fields,

                (type, field, resolveType) =>
                {
                    type.AddOrUpdateField(
                        field.Name,
                        field.FieldType,
                        field._accessMode,
                        field._defaultValue,
                        field.Optional,
                        field.BindingInfo,
                        field.Attributes,
                        field._forceUpdate,
                        field._recordedId,
                        resolveType);
                },

                (type, field) =>
                {
                    type.UpdateFieldDisplay(
                        field.Name,
                        field._index,
                        field.Description,
                        field.Detail,
                        field.Brief,
                        field.Label,
                        field.Unit,
                        field.ShowInDetail);
                });
        }
    }

    public void SetIsValueStruct(bool isValueStruct)
    {
        _isValueStruct = isValueStruct;
        UpdateAuto(nameof(DCompond.IsValueStruct));
    }

    public void SetDetail(string detail)
    {
        _detail = detail;
        UpdateAuto(nameof(DCompond.Detail));
    }

    public void SetBrief(string brief)
    {
        _brief = brief;
        UpdateAuto(nameof(DCompond.Brief));
    }

    public void AddOrUpdateField(
        string name,
        TypeDefinition type,
        AssetAccessMode accessMode,
        object defaultValue,
        bool isNullable,
        object bindingInfo,
        IAttributeGetter attribute,
        bool forceUpdate = false,
        bool isNew = false,
        Guid? recorededId = null)
    {
        _fieldCollector?.AddOrUpdatedField(
            name,
            field =>
            {
                field.FieldType = type;
                field._accessMode = accessMode;
                field._defaultValue = defaultValue;
                field._optional = isNullable;
                field._bindingInfo = bindingInfo;
                field._forceUpdate = forceUpdate;
                field.UpdateAttributes(attribute, true);
            },
            resolveType: isNew ? IdResolveType.New : IdResolveType.Auto,
            recordedId: recorededId
        );
    }

    public void UpdateFieldDisplay(string name,
        int index = -1,
        string description = null,
        string detail = null,
        string brief = null,
        string label = null,
        string unit = null,
        bool showInDetail = false)
    {
        _fieldCollector?.UpdateField(name, field =>
        {
            if (index >= 0)
            {
                field._index = index;
            }
            field._description = description;
            field._detail = detail;
            field._brief = brief;
            field._label = label;
            field._unit = unit;
            field._showInDetail = showInDetail;
        });
    }

    public void RenameField(string oldName, string newName)
    {
        _fieldCollector?.RenameField(oldName, newName);
    }

    public void RemoveField(string name)
    {
        _fieldCollector?.RemoveField(name);
    }

    public void Sort()
    {
        TryUpdateNow(v => v.SortFields());
    }

    internal override void OnUpdateAssetInternal(TType o)
    {
        base.OnUpdateAssetInternal(o);

        Sort();
    }
}