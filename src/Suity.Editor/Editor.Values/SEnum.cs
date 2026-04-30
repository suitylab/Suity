using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;

namespace Suity.Editor.Values;

[NativeAlias]
/// <summary>
/// Represents an enum value.
/// </summary>
public class SEnum : SItem, ISyncObject
{
    private readonly EditorObjectRef<DEnumField> _value = new();


    /// <summary>
    /// Creates an empty SEnum.
    /// </summary>
    public SEnum()
    { }

    /// <summary>
    /// Creates an SEnum with the specified input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public SEnum(TypeDefinition inputType)
        : base(inputType)
    {
        _value.Target = inputType.Target?.FirstField as DEnumField;
    }

    /// <summary>
    /// Creates an SEnum with the specified input type and value.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="value">The enum value name.</param>
    public SEnum(TypeDefinition inputType, string value)
        : base(inputType)
    {
        _value.Target = inputType.Target?.GetPublicField(value) as DEnumField;
    }

    /// <summary>
    /// Creates an SEnum with the specified input type and field.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="field">The enum field.</param>
    public SEnum(TypeDefinition inputType, DEnumField field)
        : base(inputType)
    {
        _value.Target = field;
    }


    /// <summary>
    /// Gets or sets the enum field.
    /// </summary>
    public DEnumField Field
    {
        get => _value.Target;
        set => _value.Target = value;
    }

    /// <summary>
    /// Gets or sets the enum value name.
    /// </summary>
    public string Value
    {
        get => _value.Target?.Name;
        set => _value.Target = InputType?.Target?.GetPublicField(value) as DEnumField;
    }

    /// <summary>
    /// Gets or sets the value ID.
    /// </summary>
    public Guid ValueId
    {
        get
        {
            if (_value.Target != null)
            {
                return _value.Target.Id;
            }
            else
            {
                return _value.Id;
            }
        }
        set
        {
            _value.Id = value;
        }
    }

    /// <summary>
    /// Gets the display text.
    /// </summary>
    public string DisplayText => _value.Target?.DisplayText ?? _value.ToString();

    /// <summary>
    /// Gets whether the value is valid.
    /// </summary>
    public bool IsValid => _value.Target != null;

    /// <summary>
    /// Auto converts the value based on the input type.
    /// </summary>
    public override void AutoConvertValue()
    {
        var type = InputType?.Target;

        if (type != null && _value.Target?.ParentType == type)
        {
            // Valid
            return;
        }

        // Default field
        _value.Target ??= type?.FirstField as DEnumField;
    }

    #region Comparison

    /// <summary>
    /// Compares the value equality with another object.
    /// </summary>
    /// <param name="other">The other object.</param>
    public override bool ValueEquals(object other)
    {
        if (other is SEnum sEnum)
        {
            return _value.Id == sEnum._value.Id;
        }

        return false;
    }

    /// <summary>
    /// Gets whether this value is blank.
    /// </summary>
    public override bool GetIsBlank()
    {
        return ValueId == Guid.Empty || string.IsNullOrWhiteSpace(Value);
    }

    #endregion

    #region ISyncObject

    /// <summary>
    /// Synchronizes the enum properties.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    /// <param name="context">The sync context.</param>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.IsSetter())
        {
            if (sync.SyncSetTypeDefinition(Attribute_InputType, InputType, out TypeDefinition newInputType, out string newTypeId))
            {
                InputType = newInputType;
                _value.Target = InputType.Target?.FirstField as DEnumField;
            }
            sync.SyncSetObjectRef(_value, newTypeId);
        }
        else
        {
            sync.SyncGetTypeDefinition(Attribute_InputType, InputType);
            sync.SyncGetEditorObjectRef(_value);
        }
    }

    #endregion

    /// <summary>
    /// Synchronizes references for this enum.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync object.</param>
    public override void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        base.ReferenceSync(path, sync);

        ValueId = sync.SyncId(path, ValueId, null);
    }

    /// <summary>
    /// Returns a string representation of this enum value.
    /// </summary>
    public override string ToString() => L(DisplayText);
}