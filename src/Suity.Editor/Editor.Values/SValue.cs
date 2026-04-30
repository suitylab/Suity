using Suity.Editor.Types;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Values;

#region SValue
/// <summary>
/// Base class for primitive value items.
/// </summary>
public abstract class SValue : SItem, ISyncObject
{
    #region Type

    private static readonly Dictionary<Type, TypeDefinition> _valueTypes = [];

    static SValue()
    {
        _valueTypes.Add(typeof(bool), NativeTypes.BooleanType);
        _valueTypes.Add(typeof(byte), NativeTypes.ByteType);
        _valueTypes.Add(typeof(sbyte), NativeTypes.SByteType);
        _valueTypes.Add(typeof(short), NativeTypes.Int16Type);
        _valueTypes.Add(typeof(ushort), NativeTypes.UInt16Type);
        _valueTypes.Add(typeof(int), NativeTypes.Int32Type);
        _valueTypes.Add(typeof(uint), NativeTypes.UInt32Type);
        _valueTypes.Add(typeof(long), NativeTypes.Int64Type);
        _valueTypes.Add(typeof(ulong), NativeTypes.UInt64Type);
        _valueTypes.Add(typeof(float), NativeTypes.SingleType);
        _valueTypes.Add(typeof(double), NativeTypes.DoubleType);
        _valueTypes.Add(typeof(string), NativeTypes.StringType);
        _valueTypes.Add(typeof(TextBlock), NativeTypes.TextBlockType);
    }

    /// <summary>
    /// Determines if a type is supported as a value type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    public static bool SupportValueType(Type type)
    {
        return _valueTypes.ContainsKey(type);
    }

    private static TypeDefinition GetTypeDefinition(Type type)
    {
        if (_valueTypes.TryGetValue(type, out TypeDefinition typeCode))
        {
            return typeCode;
        }
        else
        {
            return TypeDefinition.Empty;
        }
    }

    private static TypeDefinition GetObjectTypeCode(object value)
    {
        if (value is null)
        {
            return TypeDefinition.Empty;
        }

        if (value is SObject obj)
        {
            return obj.ObjectType;
        }
        else if (value is SItem item)
        {
            return item.InputType;
        }
        else
        {
            return GetTypeDefinition(value.GetType());
        }
    }

    private static TypeDefinition GetInputTypeDefinition(object value)
    {
        if (value is null)
        {
            return TypeDefinition.Empty;
        }

        if (value is SItem item)
        {
            return item.InputType;
        }
        else
        {
            return GetTypeDefinition(value.GetType());
        }
    }

    #endregion

    private object _value;

    /// <summary>
    /// Creates an empty SValue.
    /// </summary>
    public SValue()
    { }

    /// <summary>
    /// Creates an SValue with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SValue(object value)
    {
        ValidateValue(value);

        // Do not pass InputType to base class constructor to avoid locking
        InputType = GetInputTypeDefinition(value);
        _value = value;
    }

    public SValue(TypeDefinition type)
        : base(type)
    {
        var value = type.NativeType?.CreateInstanceOf();
        ValidateValue(value);

        _value = value;
    }

    /// <summary>
    /// Creates an SValue with the specified type definition and value.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="value">The value.</param>
    public SValue(TypeDefinition type, object value)
      : base(type)
    {
        ValidateValue(value);

        _value = value;
    }

    /// <summary>
    /// Validates the value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    protected virtual void ValidateValue(object value)
    {
        if (value != null && !_valueTypes.ContainsKey(value.GetType()))
        {
            //throw new NotSupportedException("Value type is not supported: " + value.GetType().Name);
        }

        if (value is SItem)
        {
            throw new InvalidOperationException("Value can not be SItem");
        }

        if (/*value is Type || value is DType ||*/ value is TypeDefinition)
        {
            throw new InvalidOperationException("Value can not be Type or TypeDefinition");
        }
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public virtual object Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value))
            {
                return;
            }

            ValidateValue(value);

            _value = value;

            InputType ??= GetInputTypeDefinition(value);

            OnValueChanged();
        }
    }

    /// <summary>
    /// Gets the value with optional condition.
    /// </summary>
    /// <param name="condition">The condition context.</param>
    public virtual object GetValue(ICondition condition = null) => _value;

    /// <summary>
    /// Called when the value changes.
    /// </summary>
    protected virtual void OnValueChanged()
    { }

    public override void Find(ValidationContext context, string find, SearchOption findOption)
    {
        if (_value != null)
        {
            string source = _value.ToString();
            if (Validator.Compare(_value.ToString(), find, findOption))
            {
                context.Report(source, this);
            }
        }
    }

    /// <summary>
    /// Auto converts the value based on the input type.
    /// </summary>
    public override void AutoConvertValue()
    {
        bool nullable = this.GetParentField()?.Optional == true;

        var oldValue = _value;
        var newValue = InputType?.ConvertValue(Value, nullable) ?? _value;
        if (!Equals(oldValue, newValue))
        {
            _value = newValue;
            OnValueChanged();
        }
    }

    /// <summary>
    /// Gets whether this is the default value.
    /// </summary>
    public override bool GetIsDefault()
    {
        if (_value is null)
        {
            return true;
        }

        if (this.GetField() is DStructField field)
        {
            return field.EqualsDefaultValue(_value);
        }
        else
        {
            return GetIsBlank();
        }
    }

    public override bool GetIsBlank()
    {
        if (_value is null)
        {
            return true;
        }

        var type = _value.GetType();

        if (type.IsValueType)
        {
            return _value.Equals(Activator.CreateInstance(type));
        }
        else
        {
            return false;
        }
    }

    #region ValueEquals

    public override bool ValueEquals(object obj)
    {
        if (obj is SValue sValue)
        {
            return Equals(_value, sValue._value);
        }

        return false;
    }

    #endregion

    #region ISyncObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        InputType ??= TypeDefinition.Empty;

        if (sync.IsSetter())
        {
            // There is an order dependency: setting Value will also set InputType, then override InputType
            Value = sync.Sync("Value", Value);
            if (sync.SyncSetTypeDefinition(Attribute_InputType, InputType, out TypeDefinition newInputType, out string newTypeId))
            {
                InputType = newInputType;
            }
        }
        else
        {
            // Do not set Value, otherwise it will update InputType during read
            sync.Sync("Value", Value);
            sync.SyncGetTypeDefinition(Attribute_InputType, InputType);
        }

        OnSync(sync, context);
    }

    /// <summary>
    /// Override this method to implement custom synchronization logic.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    #endregion

    public override string ToString() => _value?.ToString();
}
#endregion

#region SUnknownValue
/// <summary>
/// Represents an unknown value type.
/// </summary>
[NativeAlias]
public class SUnknownValue : SValue
{
    public SUnknownValue()
    { }

    public SUnknownValue(object v) : base(v)
    {
    }

    public SUnknownValue(TypeDefinition def) : base(def)
    {
    }

    public SUnknownValue(TypeDefinition def, object v) : base(def, v)
    {
    }

}
#endregion

#region SString
/// <summary>
/// Represents a string value.
/// </summary>
[NativeAlias]
public class SString : SValue
{
    /// <summary>
    /// Creates an empty SString.
    /// </summary>
    public SString() : base(NativeTypes.StringType)
    {
    }

    /// <summary>
    /// Creates an SString with the specified value.
    /// </summary>
    /// <param name="value">The string value.</param>
    public SString(string value) : base(NativeTypes.StringType, value)
    {
    }

    /// <summary>
    /// Creates an SString with the specified object value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SString(object value) : base(value)
    {
    }

    /// <summary>
    /// Gets or sets the string value.
    /// </summary>
    public string StringValue
    {
        get => base.Value as string;
        set => base.Value = value;
    }

    public override bool GetIsBlank() => string.IsNullOrWhiteSpace(StringValue);
}
#endregion

#region STextBlock
/// <summary>
/// Represents a text block value.
/// </summary>
[NativeAlias]
public class STextBlock : SValue
{
    /// <summary>
    /// Creates an empty STextBlock.
    /// </summary>
    public STextBlock()
        : base(NativeTypes.TextBlockType, new TextBlock())
    {
    }

    /// <summary>
    /// Creates an STextBlock with the specified TextBlock.
    /// </summary>
    /// <param name="value">The TextBlock value.</param>
    public STextBlock(TextBlock value)
        : base(NativeTypes.TextBlockType, value)
    {
    }

    /// <summary>
    /// Creates an STextBlock with the specified text.
    /// </summary>
    /// <param name="value">The text value.</param>
    public STextBlock(string value)
        : base(NativeTypes.TextBlockType, new TextBlock(value))
    {
    }

    internal STextBlock(TypeDefinition inputType)
        : base(inputType)
    {
    }


    /// <summary>
    /// Gets or sets the TextBlock value.
    /// </summary>
    public TextBlock Block
    {
        get => base.Value as TextBlock;
        set => base.Value = value;
    }

    /// <summary>
    /// Gets or sets the text value.
    /// </summary>
    public string TextValue
    {
        get => (base.Value as TextBlock)?.Text ?? string.Empty;
        set => base.Value = new TextBlock { Text = value };
    }

    public override bool GetIsBlank() => string.IsNullOrWhiteSpace(TextValue);

    public override string ToString()
    {
        return TextValue;
    }
}
#endregion

#region SNumeric
/// <summary>
/// Represents a numeric value.
/// </summary>
[NativeAlias]
public class SNumeric : SValue
{
    /// <summary>
    /// Creates an empty SNumeric.
    /// </summary>
    public SNumeric()
    { }

    /// <summary>
    /// Creates an SNumeric with the specified input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public SNumeric(TypeDefinition inputType) : base(inputType)
    {
    }

    /// <summary>
    /// Creates an SNumeric with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SNumeric(object value) : base(value)
    {
    }

    /// <summary>
    /// Creates an SNumeric with the specified input type and value.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    /// <param name="value">The value.</param>
    public SNumeric(TypeDefinition inputType, object value) : base(inputType, value)
    {
    }
}
#endregion

#region MyRegion
/// <summary>
/// Represents a null value.
/// </summary>
[NativeAlias]
public class SNull : SValue
{
    /// <summary>
    /// Creates an empty SNull.
    /// </summary>
    public SNull()
    { }

    /// <summary>
    /// Creates an SNull with the specified type definition.
    /// </summary>
    /// <param name="def">The type definition.</param>
    public SNull(TypeDefinition def) : base(def)
    {
    }


    public override bool GetIsDefault() => true;

    public override bool GetIsBlank() => true;
}
#endregion

#region SDateTime
/// <summary>
/// Represents a DateTime value.
/// </summary>
[NativeAlias]
public class SDateTime : SValue
{
    /// <summary>
    /// Creates an empty SDateTime.
    /// </summary>
    public SDateTime() : base(NativeTypes.DateTimeType)
    {
    }

    /// <summary>
    /// Creates an SDateTime with the specified DateTime value.
    /// </summary>
    /// <param name="value">The DateTime value.</param>
    public SDateTime(DateTime value) : base(NativeTypes.DateTimeType, value)
    {
    }

    /// <summary>
    /// Creates an SDateTime with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SDateTime(object value) : base(value)
    {
    }
}
#endregion

#region SBoolean
/// <summary>
/// Represents a boolean value.
/// </summary>
[NativeAlias]
public class SBoolean : SValue
{
    /// <summary>
    /// Creates an empty SBoolean.
    /// </summary>
    public SBoolean() : base(NativeTypes.BooleanType)
    {
    }

    /// <summary>
    /// Creates an SBoolean with the specified boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public SBoolean(bool value) : base(NativeTypes.BooleanType, value)
    {
    }

    /// <summary>
    /// Creates an SBoolean with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SBoolean(object value) : base(value)
    {
    }
}
#endregion
