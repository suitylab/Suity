using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Specifies how enum field IDs are generated.
/// </summary>
public enum IdAutomations
{
    /// <summary>
    /// By index
    /// </summary>
    Index,

    /// <summary>
    /// By index (starting from 1)
    /// </summary>
    IndexOne,

    /// <summary>
    /// By definition
    /// </summary>
    Defined,

    /// <summary>
    /// By index binary flag
    /// </summary>
    IndexBinaryFlag,

    /// <summary>
    /// By index binary flag (starting from 1)
    /// </summary>
    IndexOneBinaryFlag,

    /// <summary>
    /// By defined binary flag
    /// </summary>
    DefinedBinaryFlag,
}

/// <summary>
/// Represents an enum type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Enum, "Enum")]
public class DEnum : DType, IFieldGroup<DEnumField>, IDataAsset
{
    /// <summary>
    /// The icon key for enum types.
    /// </summary>
    public const string DEnumIconKey = "*CoreIcon|Enum";

    /// <summary>
    /// Gets the default color for enum types.
    /// </summary>
    public static Color EnumTypeColor { get; } = Color.FromArgb(82, 190, 160);

    internal readonly FieldObjectCollection<DEnumField> _fields;
    private IdAutomations _idAutomation;
    private DEnumField _first;

    /// <summary>
    /// Initializes a new instance of the DEnum class.
    /// </summary>
    public DEnum()
    {
        _fields = EditorObjectManager.Instance.CreateFieldCollection<DEnumField>(this);

        UpdateAssetTypes(typeof(IDataAsset));
    }

    /// <summary>
    /// Initializes a new instance of the DEnum class with a name.
    /// </summary>
    public DEnum(string name)
        : this()
    {
        LocalName = name ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the ID automation mode.
    /// </summary>
    public IdAutomations IdAutomation
    {
        get => _idAutomation;
        internal protected set
        {
            if (_idAutomation == value)
            {
                return;
            }

            _idAutomation = value;
            NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc />
    public override Image DefaultIcon => CoreIconCache.Enum;

    /// <inheritdoc />
    public override Color? TypeColor => EnumTypeColor;

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.Enum;

    /// <inheritdoc />
    public override RenderType RenderType => RenderType.Enum;

    #region IDataRowContext

    /// <inheritdoc />
    public TypeDefinition[] GetDataTypes() => [];

    /// <inheritdoc />
    public bool SupportType(TypeDefinition type) => false;

    /// <inheritdoc />
    public IDataItem GetData(bool tryLoadStorage) => null;

    #endregion

    #region Field

    /// <inheritdoc />
    public override IEnumerable<EditorObject> FieldObjects => _fields.Fields;

    /// <inheritdoc />
    public override EditorObject GetFieldObject(string name) => _fields.GetField(name);

    /// <inheritdoc />
    public override IEnumerable<DField> Fields => _fields.Fields;

    /// <summary>
    /// Gets all enum fields.
    /// </summary>
    public IEnumerable<DEnumField> EnumFields => _fields.Fields;

    /// <inheritdoc />
    public override DField GetField(string name) => _fields.GetField(name);

    /// <inheritdoc />
    public override DField GetField(Guid id) => _fields.GetField(id);

    /// <summary>
    /// Resolves an enum field by value.
    /// </summary>
    public DEnumField ResolveField(object value, bool resolveDescription = false)
    {
        if (value == null)
        {
            return null;
        }

        DEnumField field = null;

        if (value is Guid id)
        {
            field = GetField(id) as DEnumField;
            if (field != null)
            {
                return field;
            }
        }
        else if (DPrimative.IsNumericType(value))
        {
            try
            {
                int index = Convert.ToInt32(value);
                return _fields.Fields.Where(o => o.Index == index).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        string s = value.ToString();
        field = GetField(s) as DEnumField;
        if (field != null)
        {
            return field;
        }

        if (resolveDescription)
        {
            field = _fields.Fields.Where(o => o.Description == s).FirstOrDefault();
            if (field != null)
            {
                return field;
            }
        }

        if (GlobalIdResolver.TryResolve(s, out Guid resolvedId))
        {
            return GetField(s) as DEnumField;
        }

        return null;
    }

    /// <inheritdoc />
    public override int FieldCount => _fields.FieldCount;

    /// <inheritdoc />
    public override DField FirstField => _first;

    /// <summary>
    /// Gets the first field name.
    /// </summary>
    public string FirstName => _first?.Name;

    protected internal DEnumField AddOrUpdateField(
        string name, int id, string description, object bindingInfo,
        IAttributeGetter attribute,
        bool forceUpdate = false,
        Guid? recoredId = null,
        IdResolveType resolveType = IdResolveType.Auto)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is empty.", nameof(name));
        }

        DEnumField field = _fields.GetOrAddField(name, resolveType, recoredId);
        bool modified = false;

        if (field._value != id)
        {
            field._value = id;
            modified = true;
        }

        if (field._description != description)
        {
            field._description = description;
            modified = true;
        }

        //if (field._index != index)
        //{
        //    field._index = index;
        //    modified = true;
        //}

        if (field._bindingInfo != bindingInfo)
        {
            field._bindingInfo = bindingInfo;
            modified = true;
        }

        //Need to forcibly trigger update
        field.UpdateAttributes(attribute, true);

        if (_fields.FieldCount == 1)
        {
            if (_first != field)
            {
                _first = field;
                NotifyUpdated();
            }
        }

        if (modified || forceUpdate)
        {
            field.NotifyUpdated();
        }

        return field;
    }

    protected internal bool UpdateField(string name, int index)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        DEnumField field = _fields.GetField(name);
        if (field == null)
        {
            return false;
        }

        bool modified = false;

        if (field._index != index)
        {
            field._index = index;
            modified = true;
        }

        if (modified)
        {
            field.NotifyUpdated();
        }

        return modified;
    }

    protected internal DEnumField RenameField(string oldName, string newName)
    {
        return _fields.RenameField(oldName, newName);
    }

    protected internal bool RemoveField(string name)
    {
        return _fields.RemoveField(name);
    }

    protected internal bool SetFirstName(string name)
    {
        if (name == null)
        {
            return false;
        }

        DEnumField field = _fields.GetField(name);

        if (field != null)
        {
            if (_first != field)
            {
                _first = field;
                NotifyUpdated();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    protected internal void SortFields()
    {
        _fields.Sort((v1, v2) => v1._index.CompareTo(v2._index));
    }

    #endregion

    #region IAssetGroup<DEnumField>

    IEnumerable<DEnumField> IFieldGroup<DEnumField>.Fields => _fields.Fields;

    int IFieldGroup<DEnumField>.FieldCount => _fields.FieldCount;

    Guid IFieldGroup<DEnumField>.GetFieldId(string name) => _fields.GetField(name)?.Id ?? Guid.Empty;

    DEnumField IFieldGroup<DEnumField>.GetField(string name) => _fields.GetField(name);

    DEnumField IFieldGroup<DEnumField>.GetField(Guid id) => _fields.GetField(id);

    #endregion

    public override object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.ChildNodes => _fields.Fields,
        _ => base.GetProperty(property, argument),
    };
}

public class DEnumBuilder : DTypeBuilder<DEnum>
{
    private readonly IAssetFieldCollector<DEnumField> _fieldCollector;
    private IdAutomations _idAutomation;
    private string _firstName;

    public DEnumBuilder()
    {
        AddAutoUpdate(nameof(DEnum.IdAutomation), o => o.IdAutomation = _idAutomation);
        AddAutoUpdate(nameof(DEnum.FirstName), o => o.SetFirstName(_firstName));
        _fieldCollector = AddFieldCollector(
            nameof(DCompond.Fields),
            type => type._fields,
            (enm, field, resolveType) =>
            {
                enm.AddOrUpdateField(
                    field.Name,
                    field._value,
                    field._description,
                    field._bindingInfo,
                    field.Attributes,
                    field._forceUpdate,
                    field._recordedId,
                    resolveType);
            },
            (type, field) =>
            {
                type.UpdateField(field.Name, field._index);
            });
    }

    public DEnumBuilder(string name, string iconKey)
        : this()
    {
        SetLocalName(name);
        SetIconKey(iconKey);
    }

    public void SetIdAutomation(IdAutomations auto)
    {
        _idAutomation = auto;

        UpdateAuto(nameof(DEnum.IdAutomation));
    }

    public void AddOrUpdateField(
        string name,
        int value,
        string description = null,
        object bindingInfo = null,
        IAttributeGetter attribute = null,
        bool forceUpdate = false,
        bool isNew = false,
        Guid? recorededId = null)
    {
        _fieldCollector.AddOrUpdatedField(
            name,
            field =>
            {
                field._value = value;
                field._description = description;
                field._bindingInfo = bindingInfo;
                field.UpdateAttributes(attribute, true);
                field._forceUpdate = forceUpdate;
            },
            resolveType: isNew ? IdResolveType.New : IdResolveType.Auto,
            recordedId: recorededId
            );
    }

    public void UpdateField(string name, int index)
    {
        _fieldCollector.UpdateField(name, field =>
        {
            field._index = index;
        });
    }

    public void RenameField(string oldName, string newName)
    {
        _fieldCollector.RenameField(oldName, newName);
    }

    public void RemoveField(string name)
    {
        _fieldCollector.RemoveField(name);
    }

    public void SetFirstName(string name)
    {
        _firstName = name;

        UpdateAuto(nameof(DEnum.FirstName));
    }

    public void Sort()
    {
        TryUpdateNow(v => v.SortFields());
    }
}