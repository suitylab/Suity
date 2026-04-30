using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a struct field.
/// </summary>
public class DStructField : DField, INavigableRoute
{
    private TypeDefinition _fieldType;
    private readonly EditorAssetRef<DType> _fieldTypeRef = new();

    private readonly EditorObjectRef<EditorObject> _fieldNameRef = new();

    internal object _defaultValue;
    private DStructField _arrayElementField;
    internal bool _isArrayElement;

    internal object _bindingInfo;

    internal int _index;
    internal string _description;
    internal string _detail;
    internal string _brief;
    internal string _label;
    internal string _unit;
    internal bool _showInDetail;

    internal bool _forceUpdate;

    internal AssetAccessMode _accessMode;

    internal bool _optional;

    private IAttributeGetter _attributes;
    private Color? _color;

    /// <summary>
    /// Initializes a new instance of the DStructField class.
    /// </summary>
    public DStructField()
    {
        AddUpdateRelationship(_fieldTypeRef);
        AddUpdateRelationship(_fieldNameRef);
    }

    /// <summary>
    /// Updates the attributes for this field.
    /// </summary>
    /// <param name="attributes">The new attributes.</param>
    /// <param name="notify">Whether to notify of changes.</param>
    internal void UpdateAttributes(IAttributeGetter attributes, bool notify)
    {
        //if (ReferenceEquals(_attributes, value))
        //{
        //    return;
        //}

        _attributes = attributes ?? EmptyAttributeDesign.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        _color = _attributes.GetAttribute<IViewColor>()?.ViewColor;

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    public string Label
    {
        get => _label;
        internal protected set
        {
            if (_label != value)
            {
                _label = value;
                NotifyPropertyUpdated();
            }
        }
    }

    /// <summary>
    /// Gets or sets the field type.
    /// </summary>
    public TypeDefinition FieldType
    {
        get => _fieldType;
        internal protected set
        {
            if (_fieldType == value)
            {
                return;
            }

            //var oldFieldType = _fieldType;

            _fieldType = value;

            if (_fieldType != null)
            {
                if (_fieldType.IsArray)
                {
                    _arrayElementField = new DStructField
                    {
                        _name = _name,
                        _isArrayElement = true,

                        FieldType = _fieldType.ElementType
                    };
                }

                _fieldTypeRef.Id = _fieldType.TargetId;
            }
            else
            {
                _arrayElementField = null;
                _fieldTypeRef.Id = Guid.Empty;
            }

            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Gets whether this is an array field.
    /// </summary>
    public bool IsArray => _arrayElementField != null;

    /// <summary>
    /// Gets the array element field.
    /// </summary>
    public DStructField ArrayElementField => _arrayElementField;

    /// <summary>
    /// Gets whether this is an array element.
    /// </summary>
    public bool IsArrayElement => _isArrayElement;

    /// <summary>
    /// Gets the detail.
    /// </summary>
    public string Detail => _detail;

    /// <summary>
    /// Gets the brief description.
    /// </summary>
    public string Brief => _brief;

    /// <summary>
    /// Gets the unit.
    /// </summary>
    public string Unit => _unit;

    /// <summary>
    /// Gets whether this field is optional.
    /// </summary>
    public bool Optional
    {
        get
        {
            if (_optional)
            {
                return true;
            }

            return _fieldType?.Target?.Attributes?.ContainsAttribute<DataUsageAttribute>(o => o.Usage == DataUsageMode.Nullable) == true;
        }
    }
    /// <summary>
    /// Gets whether this is a connector.
    /// </summary>
    public bool IsConnector => _attributes.ContainsAttribute<ConnectorAttribute>();

    /// <summary>
    /// Gets whether this is a consistency field.
    /// </summary>
    public bool IsConsistency => _attributes.ContainsAttribute<ConsistencyAttribute>();

    /// <summary>
    /// Gets whether this is a classifier.
    /// </summary>
    public bool IsClassifier => _attributes.ContainsAttribute<ClassifyAttribute>();

    /// <summary>
    /// Gets the auto field type.
    /// </summary>
    public AutoFieldType? AutoFieldType => _attributes.GetAutoFieldType();

    /// <summary>
    /// Gets the display value for a given value.
    /// </summary>
    public string GetDisplayValue(object value)
    {
        string v = value != null ? value.ToString() : string.Empty;
        if (!string.IsNullOrEmpty(_brief))
        {
            v = _brief.Replace("{value}", v);
        }

        return v;
    }

    /// <summary>
    /// Gets whether to show in detail view.
    /// </summary>
    public bool ShowInDetail => _showInDetail;

    /// <summary>
    /// Updates the field name reference.
    /// </summary>
    public void UpdateFieldNameRef(EditorObject obj)
    {
        _fieldNameRef.Target = obj;
    }

    /// <summary>
    /// Checks if the value equals the default value.
    /// </summary>
    public bool EqualsDefaultValue(object value)
    {
        if (value is SObject sobj && SObject.IsNullOrEmpty(sobj))
        {
            value = null;
        }

        return Equality.ObjectEquals(value, _defaultValue);
    }

    /// <inheritdoc />
    public override bool IsIdDocumented
    {
        get
        {
            if (_bindingInfo is Guid)
            {
                return false;
            }

            return base.IsIdDocumented;
        }
    }

    #region INavigableRoute

    /// <inheritdoc />
    public object GetNavigableRoute()
    {
        return _fieldNameRef.Target;
    }

    #endregion

    #region Virtual

    /// <inheritdoc />
    public override int Index => _index;

    /// <inheritdoc />
    public override string Description => _description;

    /// <inheritdoc />
    public override object BindingInfo => _bindingInfo;

    /// <inheritdoc />
    public override IAttributeGetter Attributes => _attributes ?? EmptyAttributeDesign.Empty;

    /// <inheritdoc />
    public override Color? ViewColor => _color;

    /// <inheritdoc />
    public override string DisplayText => !string.IsNullOrEmpty(_description) ? _description : _name;

    /// <inheritdoc />
    public override AssetAccessMode AccessMode => _accessMode;

    /// <inheritdoc />
    public override Image GetIcon()
    {
        //Do not display system icon
        if (!FieldType.IsValue)
        {
            return FieldType?.GetIcon();
        }
        else
        {
            return null;
        }
    }

    public override object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.TypeInfo:
                return FieldType;

            case CodeRenderProperty.IsNullable:
                return Optional;

            default:
                return base.GetProperty(property, argument);
        }
    }

    protected override void OnRelationshipUpdated(EditorObject obj, EntryEventArgs e, ref bool handled)
    {
        if (obj == _fieldTypeRef.Target && e is RenameAssetEventArgs)
        {
            NotifyUpdated();
            Parent?.NotifyUpdated();
        }
        else if (obj == _fieldNameRef.Target && e is FieldEntryEventArgs args && args.UpdateType == EntryUpdateTypes.Rename)
        {
            (Parent as DCompond)?.RenameField(args.OldName, args.Name);

            NotifyUpdated();
            Parent?.NotifyUpdated();
        }
    }

    protected override string GetName() => _name;
    #endregion
}