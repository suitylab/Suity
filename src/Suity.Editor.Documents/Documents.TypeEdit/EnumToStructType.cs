using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

#region CachedFieldIdCollection

/// <summary>
/// Caches field IDs by field name for persistence across serialization.
/// </summary>
internal class CachedFieldIdCollection : ISyncObject
{
    private readonly Dictionary<string, Guid> _cache = [];

    /// <summary>
    /// Sets or removes a field ID in the cache.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="id">The field ID, or Guid.Empty to remove.</param>
    public void SetField(string fieldName, Guid id)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return;
        }

        if (id != Guid.Empty)
        {
            _cache[fieldName] = id;
        }
        else
        {
            _cache.Remove(fieldName);
        }
    }

    /// <summary>
    /// Removes a field from the cache.
    /// </summary>
    /// <param name="fieldName">The field name to remove.</param>
    public void RemoveField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return;
        }

        _cache.Remove(fieldName);
    }

    /// <summary>
    /// Clears all cached field IDs.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the cached ID for a field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The cached field ID, or null if not found.</returns>
    public Guid? GetFieldId(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        if (_cache.TryGetValue(fieldName, out var id) && id != Guid.Empty)
        {
            return id;
        }

        return null;
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.Mode == SyncMode.GetAll)
        {
            foreach (var item in _cache)
            {
                sync.Sync(item.Key, item.Value);
            }
        }
        else if (sync.Mode == SyncMode.SetAll)
        {
            _cache.Clear();
            foreach (var name in sync.Names)
            {
                var id = sync.Sync(name, Guid.Empty);
                if (!string.IsNullOrWhiteSpace(name) && id != Guid.Empty)
                    _cache[name] = id;
            }
        }
    }
}
#endregion

#region EnumToStructFieldSetup

/// <summary>
/// Provides field attribute configuration for enum-to-struct types.
/// </summary>
public class EnumToStructFieldSetup : IDesignObject, IViewEditNotify
{
    private readonly SArrayAttributeDesign _attributes = new();

    /// <summary>
    /// Event raised when the field setup is updated.
    /// </summary>
    public event EventHandler Updated;

    /// <summary>
    /// Gets the attribute design for field attributes.
    /// </summary>
    public IAttributeDesign Attributes => _attributes;

    #region IViewObject

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        sync.Sync("Attributes", _attributes.Array, SyncFlag.GetOnly);

        if (sync.IsSetter())
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        if (setup.SupportExtended())
        {
            setup.ExtendedField(_attributes.Array, new ViewProperty("Attributes", "Attributes"));
        }
    }

    #endregion

    #region IDesignObject

    /// <inheritdoc/>
    SArray IDesignObject.DesignItems => _attributes.Array;

    /// <inheritdoc/>
    string IDesignObject.DesignPropertyName => "Attributes";

    /// <inheritdoc/>
    string IDesignObject.DesignPropertyDescription => "Attributes";

    #endregion

    /// <inheritdoc/>
    public override string ToString() => "Field Attributes";

    #region IViewEditNotify

    /// <inheritdoc/>
    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
#endregion

#region EnumToStructType

/// <summary>
/// A struct type that is generated from an enum, with additional field type and value configuration.
/// </summary>
[NativeAlias]
[DisplayText("EnumToStruct", "*CoreIcon|Box")]
[DisplayOrder(800)]
public class EnumToStructType : StructTypeBase<EnumToStructBuilder>
{
    private AssetSelection<DEnum> _baseEnum = new();
    private readonly FieldTypeDesign _fieldType;
    private object _defaultValue;
    private bool _optional;
    private readonly EnumToStructFieldSetup _fieldSetup = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumToStructType"/> class.
    /// </summary>
    public EnumToStructType()
    {
        _fieldType = new(this);
        _fieldType.BaseType.SelectedKey = "*System|String";
        _fieldType.FieldTypeChanged += (s, e) =>
        {
            AssetBuilder.FieldTypeUpdater.UpdateValue(_fieldType.FieldType);
        };

        _fieldSetup.Updated += (s, e) =>
        {
            AssetBuilder.FieldAttributeUpdater.UpdateValue(_fieldSetup.Attributes);
        };
    }

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon()
    {
        return base.OnGetIcon() ?? _baseEnum.Target?.GetIcon() ?? CoreIconCache.Box;
    }

    /// <inheritdoc/>
    public override string PreviewText => "Enum Struct";

    /// <inheritdoc/>
    public override Color? TypeColor => DStruct.StructTypeColor;

    /// <inheritdoc/>
    public override ImageDef TypeIcon => CoreIconCache.Box;

    /// <summary>
    /// Gets or sets the base enum type selection.
    /// </summary>
    public AssetSelection<DEnum> BaseEnum
    {
        get => _baseEnum;
        set
        {
            var v = value ?? new AssetSelection<DEnum>();
            if (_baseEnum.Id == v.Id)
            {
                return;
            }

            _baseEnum = v;
            AssetBuilder.BaseEnumUpdater.UpdateId(_baseEnum.Id);
        }
    }

    /// <summary>
    /// Gets the enum-to-struct builder for this type.
    /// </summary>
    internal new EnumToStructBuilder AssetBuilder => base.AssetBuilder;

    /// <summary>
    /// Gets the field type design for the generated fields.
    /// </summary>
    public FieldTypeDesign FieldType => _fieldType;

    /// <summary>
    /// Gets or sets the default value for generated fields.
    /// </summary>
    public object DefaultValue
    {
        get => _defaultValue;
        set
        {
            if (_defaultValue != value)
            {
                _defaultValue = value;
                AssetBuilder.DefaultValueUpdater.UpdateValue(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether generated fields are optional.
    /// </summary>
    public bool Optional
    {
        get => _optional;
        set
        {
            if (_optional != value)
            {
                _optional = value;
                AssetBuilder.OptionalUpdater.UpdateValue(value);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        BaseEnum = sync.Sync("BaseEnum", BaseEnum, SyncFlag.NotNull);
        sync.Sync("FieldSetup", _fieldSetup, SyncFlag.GetOnly | SyncFlag.NotNull);
        sync.Sync("FieldType", _fieldType, SyncFlag.GetOnly | SyncFlag.AffectsOthers);

        DefaultValue = sync.Sync("DefaultValue", DefaultValue);
        UpdateDefaultValue();

        Optional = sync.Sync("Nullable", Optional); //TODO: Change Nullable to Optional

        if (sync.IsSetter())
        {
            AssetBuilder.FieldTypeUpdater.UpdateValue(_fieldType.FieldType);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        if (setup.SupportDetailTreeView())
        {
            setup.DetailTreeViewField(_fieldSetup, new ViewProperty("FieldSetup", "Field Attributes"));
        }

        base.OnSetupView(setup);

        UpdateDefaultValue();

        if (setup.SupportInspector())
        {
            setup.InspectorField(BaseEnum, new ViewProperty("BaseEnum", "Base Enum Type"));
            setup.InspectorField(FieldType, new ViewProperty("FieldType", "Field Type") { Expand = true });
            setup.InspectorFieldOfType(FieldType.FieldType, new ViewProperty("DefaultValue", "Default Value"));
            setup.InspectorField(Optional, new ViewProperty("Nullable", "Optional")); //TODO: Change to Optional
        }
    }

    /// <inheritdoc/>
    protected override void OnIsValueTypeChanged()
    {
        base.OnIsValueTypeChanged();

        AssetBuilder.SetIsValueStruct(IsValueType);
    }

    /// <inheritdoc/>
    protected override void OnBaseTypeChanged()
    {
        base.OnBaseTypeChanged();

        AssetBuilder.BaseTypeUpdater.UpdateId(BaseType?.Id ?? Guid.Empty);
    }

    /// <summary>
    /// Updates the default value based on the current field type.
    /// </summary>
    private void UpdateDefaultValue()
    {
        if (!TypeDefinition.IsNullOrBroken(FieldType.FieldType))
        {
            DefaultValue = FieldType.SyncDefaultValue(DefaultValue, this.GetAssetFilter());
        }
    }
}
#endregion

#region DEnumToStruct

/// <summary>
/// Asset representation of an enum-to-struct type, auto-generating fields from an enum.
/// </summary>
public class DEnumToStruct : DStruct
{
    private readonly EditorAssetRef<DEnum> _enum = new();
    private TypeDefinition _fieldType;
    private IAttributeDesign _fieldAttrs;
    private object _defaultValue;
    private bool _optional;

    private readonly CachedFieldIdCollection _cachedFieldIds = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DEnumToStruct"/> class.
    /// </summary>
    public DEnumToStruct()
    {
        AddUpdateRelationship(_enum);
    }

    /// <summary>
    /// Gets the base enum asset.
    /// </summary>
    public DEnum BaseEnum => _enum.Target;

    /// <summary>
    /// Gets or sets the ID of the base enum.
    /// </summary>
    public Guid BaseEnumId
    {
        get => _enum.Id;
        internal protected set
        {
            if (_enum.Id != value)
            {
                _enum.Id = value;
                UpdateFields();
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Gets or sets the type for generated fields.
    /// </summary>
    public TypeDefinition FieldType
    {
        get => _fieldType;
        internal protected set
        {
            if (_fieldType != value)
            {
                _fieldType = value;
                UpdateFields();
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Gets or sets the field attributes applied to generated fields.
    /// </summary>
    public IAttributeDesign FieldAttributes
    {
        get => _fieldAttrs;
        internal protected set
        {
            _fieldAttrs = value;
            UpdateFields();
            NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc/>
    public override Color? TypeColor => DStruct.StructTypeColor;

    /// <summary>
    /// Gets or sets the default value for generated fields.
    /// </summary>
    public object DefaultValue
    {
        get => _defaultValue;
        internal protected set
        {
            if (_defaultValue != value)
            {
                _defaultValue = value;
                UpdateFields();
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether generated fields are optional.
    /// </summary>
    public bool Optional
    {
        get => _optional;
        internal protected set
        {
            if (_optional != value)
            {
                _optional = value;
                UpdateFields();
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Sets the base type ID.
    /// </summary>
    /// <param name="id">The base type ID.</param>
    internal void SetBaseTypeId(Guid id) => base.BaseTypeId = id;

    /// <inheritdoc/>
    protected override void OnRelationshipUpdated(EditorObject obj, EntryEventArgs e, ref bool handled)
    {
        base.OnRelationshipUpdated(obj, e, ref handled);

        if (obj == _enum.Target)
        {
            UpdateFields();
        }
    }

    /// <inheritdoc/>
    protected override void OnAssetActivate(string assetKey)
    {
        base.OnAssetActivate(assetKey);

        UpdateFields();
    }

    /// <inheritdoc/>
    protected override void OnAssetDeactivate(string assetKey)
    {
        base.OnAssetDeactivate(assetKey);
    }

    /// <inheritdoc/>
    protected override void OnStartup()
    {
        base.OnStartup();

        QueuedAction.Do(() => UpdateFields(true));
    }

    /// <summary>
    /// Updates the generated fields based on the current enum and field configuration.
    /// </summary>
    /// <param name="startup">Whether this is called during startup.</param>
    private void UpdateFields(bool startup = false)
    {
        var enm = _enum.Target;
        if (enm is null)
        {
            ClearFields();
            return;
        }
        var type = FieldType;
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            ClearFields();
            return;
        }
        var defaultValue = DefaultValue;

        foreach (var enmField in enm.EnumFields)
        {
            IAttributeGetter attr = enmField.Attributes ?? EmptyAttributeDesign.Empty;
            if (_fieldAttrs != null)
            {
                attr = new CombinedAttributes(_fieldAttrs, attr);
            }

            var recordedId = _cachedFieldIds.GetFieldId(enmField.Name);

            var field = AddOrUpdateField(
                enmField.Name, type, AssetAccessMode.Public,
                defaultValue, _optional,
                enmField.Id, attr, false, recordedId);

            if (field.Id != Guid.Empty)
            {
                _cachedFieldIds.SetField(field.Name, field.Id);
            }

            UpdateFieldDisplay(enmField.Name, enmField.Index, enmField.Description);

            field.UpdateFieldNameRef(enmField);
        }

        List<DStructField> removes = null;

        foreach (var field in AllStructFields)
        {
            if (enm.GetField(field.Name) == null)
            {
                (removes ??= []).Add(field);
            }
        }

        if (removes != null)
        {
            foreach (var field in removes)
            {
                RemoveField(field.Name);
            }
        }

        SortFields();
    }

    /// <inheritdoc/>
    protected override void OnIdResolved(Guid id, IdResolveType resolveType)
    {
        // Call base class method to trigger field Id resolution
        base.OnIdResolved(id, resolveType);

        // After field Id resolution, record to cache
        foreach (var field in this.Fields)
        {
            _cachedFieldIds.SetField(field.Name, field.Id);
        }
    }

    /// <inheritdoc/>
    public override object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.BaseEnumTypeInfo:
                return BaseEnum?.Definition;

            case CodeRenderProperty.ReturnTypeInfo:
                return _fieldType;

            default:
                break;
        }

        return base.GetProperty(property, argument);
    }
}
#endregion

#region EnumToStructBuilder

/// <summary>
/// Builder for creating and updating <see cref="DEnumToStruct"/> assets.
/// </summary>
public class EnumToStructBuilder : DBaseStructBuilder<DEnumToStruct>
{
    /// <summary>
    /// Gets the updater for the base type reference.
    /// </summary>
    public IRefUpdateAction<DAbstract> BaseTypeUpdater { get; }

    /// <summary>
    /// Gets the updater for the base enum reference.
    /// </summary>
    public IRefUpdateAction<DEnum> BaseEnumUpdater { get; }

    /// <summary>
    /// Gets the updater for the field type value.
    /// </summary>
    public IValueUpdateAction<TypeDefinition> FieldTypeUpdater { get; }

    /// <summary>
    /// Gets the updater for the field attributes value.
    /// </summary>
    public IValueUpdateAction<IAttributeDesign> FieldAttributeUpdater { get; }

    /// <summary>
    /// Gets the updater for the default value.
    /// </summary>
    public IValueUpdateAction<object> DefaultValueUpdater { get; }

    /// <summary>
    /// Gets the updater for the optional flag.
    /// </summary>
    public IValueUpdateAction<bool> OptionalUpdater { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumToStructBuilder"/> class.
    /// </summary>
    public EnumToStructBuilder()
        : base(false)
    {
        BaseTypeUpdater = AddRefAutoUpdate<DAbstract>(nameof(DEnumToStruct.BaseType), (o, id) => o.SetBaseTypeId(id));
        BaseEnumUpdater = AddRefAutoUpdate<DEnum>(nameof(DEnumToStruct.BaseEnum), (o, id) => o.BaseEnumId = id);
        FieldTypeUpdater = AddValueAutoUpdate<TypeDefinition>(nameof(DEnumToStruct.FieldType), (o, v) => o.FieldType = v);
        FieldAttributeUpdater = AddValueAutoUpdate<IAttributeDesign>(nameof(DEnumToStruct.FieldAttributes), (o, v) => o.FieldAttributes = v);
        DefaultValueUpdater = AddValueAutoUpdate<object>(nameof(DEnumToStruct.DefaultValue), (o, v) => o.DefaultValue = v);
        OptionalUpdater = AddValueAutoUpdate<bool>(nameof(DEnumToStruct.Optional), (o, v) => o.Optional = v);
    }
}
#endregion
