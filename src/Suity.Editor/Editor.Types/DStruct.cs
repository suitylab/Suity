using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

[AssetTypeBinding(AssetDefNames.Struct, "Struct")]
/// <summary>
/// Represents a struct type in the editor.
/// </summary>
public class DStruct : DCompond, IDataAsset
{
    /// <summary>
    /// Gets the default color for struct types.
    /// </summary>
    public static Color StructTypeColor { get; } = Color.FromArgb(40, 194, 255);

    private readonly EditorAssetRef<DAbstract> _baseType = new();
    private IRegistryHandle<DStruct> _baseTypeEntry;

    private readonly CompondAttributeGetter _compondAttributes = new();


    /// <summary>
    /// Initializes a new instance of the DStruct class.
    /// </summary>
    public DStruct()
    {
        UpdateAssetTypes(typeof(IDataAsset));
        AddUpdateRelationship(_baseType);
    }

    /// <summary>
    /// Initializes a new instance of the DStruct class.
    /// </summary>
    public DStruct(string name, string baseTypeKey, string iconKey)
        : this()
    {
        _baseType.AssetKey = baseTypeKey;
        IconKey = iconKey;
        LocalName = name ?? string.Empty;

        UpdateCompondAttributes();
    }

    /// <inheritdoc />
    public override Image GetIcon() => base.GetIcon() ?? _baseType.Target?.GetIcon();

    /// <inheritdoc />
    public override Image DefaultIcon => CoreIconCache.Box;

    /// <inheritdoc />
    public override Color? TypeColor => StructTypeColor;

    /// <inheritdoc />
    public override Color? ViewColor
    {
        get => base.ViewColor ?? _baseType.Target?.ViewColor;
        protected internal set => base.ViewColor = value;
    }

    /// <inheritdoc />
    public override bool RequireFormatter => true;

    /// <summary>
    /// Gets or sets the base type ID.
    /// </summary>
    public Guid BaseTypeId
    {
        get => _baseType.Id;
        internal protected set
        {
            if (_baseType.Id == value)
            {
                return;
            }

            _baseType.Id = value;
            _baseTypeEntry?.Dispose();
            _baseTypeEntry = DTypeManager.Instance.AddToBaseType(this);

            UpdateCompondAttributes();

            NotifyPropertyUpdated(nameof(BaseType));
        }
    }

    // public DSide BaseSide => _baseType.Target;

    /// <inheritdoc />
    public override DCompond BaseType => _baseType.Target;

    /// <inheritdoc />
    public override RenderType RenderType => RenderType.Struct;

    /// <inheritdoc />
    public override TypeDefinition BaseTypeDefinition => BaseType?.Definition;

    /// <inheritdoc />
    public override IAttributeGetter Attributes => _compondAttributes;


    /// <summary>
    /// Sets the base type by asset key.
    /// </summary>
    protected internal void SetBaseType(string assetKey)
    {
        if (_baseType.AssetKey == assetKey)
        {
            return;
        }

        _baseType.AssetKey = assetKey;
        _baseTypeEntry?.Dispose();
        _baseTypeEntry = DTypeManager.Instance.AddToBaseType(this);

        UpdateCompondAttributes();

        NotifyPropertyUpdated(nameof(BaseType));
    }

    internal override void InternalOnAssetActivate(string assetKey)
    {
        _baseTypeEntry?.Dispose();
        _baseTypeEntry = DTypeManager.Instance.AddToBaseType(this);

        UpdateCompondAttributes();

        base.InternalOnAssetActivate(assetKey);
    }

    internal override void InternalOnAssetDeactivate(string assetKey)
    {
        var entry = _baseTypeEntry;
        _baseTypeEntry = null;
        entry?.Dispose();

        UpdateCompondAttributes();

        base.InternalOnAssetDeactivate(assetKey);
    }

    protected override void OnIsPrimaryUpdated()
    {
        base.OnIsPrimaryUpdated();

        _baseTypeEntry?.Update();
        UpdateCompondAttributes();
    }

    public override object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.BaseTypeInfo => BaseType?.Definition ?? TypeDefinition.Empty,
        _ => base.GetProperty(property, argument),
    };

    internal override void UpdateAttributes(IAttributeGetter value, bool notify)
    {
        _compondAttributes.Getter = value;

        base.UpdateAttributes(value, notify);
    }

    private void UpdateCompondAttributes()
    {
        _compondAttributes.Getter = base._attributes;
        _compondAttributes.BaseGetter = _baseType;
    }

    #region IDataRowContext

    public TypeDefinition[] GetDataTypes() => [];

    public bool SupportType(TypeDefinition type) => false;

    public IDataItem GetData(bool tryLoadStorage) => null;

    #endregion

    #region Fields

    public override IEnumerable<EditorObject> FieldObjects => _baseType.Target is DAbstract s ? s.FieldObjects.Concat(base.FieldObjects) : base.FieldObjects;

    public override EditorObject GetFieldObject(string name) => base.GetFieldObject(name) ?? _baseType.Target?.GetFieldObject(name);

    public override DField GetField(Guid id) => base.GetField(id) ?? _baseType.Target?.GetField(id);

    public override DField GetField(string name) => base.GetField(name) ?? _baseType.Target?.GetField(name);

    public override IEnumerable<DField> Fields => _baseType.Target is DAbstract s ? s.Fields.Concat(base.Fields) : base.Fields;

    public override int FieldCount => base.FieldCount + _baseType.Target?.FieldCount ?? 0;

    public override IEnumerable<DStructField> AllStructFields
    {
        get
        {
            if (_baseType.Id != Guid.Empty && _baseType.Target is null)
            {
                Logs.LogWarning($"{this.Name} base class missing:" + _baseType.Id);
            }

            if (_baseType.Target is DAbstract s)
            {
                return s.AllStructFields.Concat(base.AllStructFields);
            }
            else
            {
                return base.AllStructFields;
            }
        }
    }
    public override IEnumerable<DStructField> PublicStructFields
    {
        get
        {
            if (_baseType.Id != Guid.Empty && _baseType.Target is null)
            {
                Logs.LogWarning($"{this.Name} base class missing:" + _baseType.Id);
            }

            if (_baseType.Target is DAbstract s)
            {
                return s.AllStructFields.Concat(base.PublicStructFields);
            }
            else
            {
                return base.PublicStructFields;
            }
        }
    }

    public override IEnumerable<DStructField> GetPublicStructFields(bool includeBaseFields)
    {
        if (includeBaseFields)
        {
            return this.PublicStructFields;
        }
        else
        {
            return base.PublicStructFields;
        }
    }

    #endregion
}

#region DStructBuilder
public class DStructBuilder : DBaseStructBuilder<DStruct>
{
    private readonly EditorAssetRef<DAbstract> _baseType = new();

    public DStructBuilder()
    {
        AddAutoUpdate(nameof(DStruct.BaseType), o =>
        {
            o.BaseTypeId = _baseType.Id;
        });
    }

    public DStructBuilder(string name, string iconKey)
        : this()
    {
        SetLocalName(name);
        SetIconKey(iconKey);
    }

    public void UpdateBaseType(DAbstract baseType)
    {
        _baseType.Target = baseType;
        UpdateAuto(nameof(DStruct.BaseType));
    }

    public void UpdateBaseType(Guid id)
    {
        _baseType.Id = id;
        UpdateAuto(nameof(DStruct.BaseType));
    }

    public void UpdateBaseType(string assetKey)
    {
        _baseType.AssetKey = assetKey;
        UpdateAuto(nameof(DStruct.BaseType));
    }
}
#endregion

#region CompondAttributeGetter

public class CompondAttributeGetter : IAttributeGetter
{
    private IAttributeGetter _getter;
    private EditorAssetRef<DAbstract> _baseGetter;

    public CompondAttributeGetter()
    {
        _getter = EmptyAttributeGetter.Empty;
    }

    public CompondAttributeGetter(IAttributeGetter getter, EditorAssetRef<DAbstract> baseGetter = null)
    {
        _getter = getter ?? EmptyAttributeGetter.Empty;
        _baseGetter = baseGetter;
    }

    public IAttributeGetter Getter
    {
        get => _getter;
        set => _getter = value ?? EmptyAttributeGetter.Empty;
    }

    public EditorAssetRef<DAbstract> BaseGetter
    {
        get => _baseGetter;
        set => _baseGetter = value;
    }

    public IEnumerable<object> GetAttributes()
    {
        if (_baseGetter?.Target?.Attributes is { } b)
        {
            return _getter.GetAttributes().Concat(b.GetAttributes());
        }
        else
        {
            return _getter.GetAttributes();
        }
    }

    public IEnumerable<object> GetAttributes(string typeName)
    {
        if (_baseGetter?.Target?.Attributes is { } b)
        {
            return _getter.GetAttributes(typeName).Concat(b.GetAttributes(typeName));
        }
        else
        {
            return _getter.GetAttributes(typeName);
        }
    }

    public IEnumerable<T> GetAttributes<T>() where T : class
    {
        if (_baseGetter?.Target?.Attributes is { } b)
        {
            return _getter.GetAttributes<T>().Concat(b.GetAttributes<T>());
        }
        else
        {
            return _getter.GetAttributes<T>();
        }
    }
}

#endregion