using Suity.Editor.Types;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.Values;

/// <summary>
/// Back-end implementation of <see cref="SItemExternal"/> that handles SItem creation,
/// type resolution, value conversion, and path building.
/// </summary>
internal class SItemExternalBK : SItemExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="SItemExternalBK"/>.
    /// </summary>
    public static readonly SItemExternalBK Instance = new();

    /// <summary>
    /// Initializes the external system by registering this instance.
    /// </summary>
    public void Initialize()
    {
        SItemExternal._external = this;
    }

    /// <inheritdoc/>
    public override SObjectExternal CreateSObjectEx(SObject obj)
        => new SObjectExternalBK(obj);

    /// <inheritdoc/>
    public override SObjectExternal CreateSObjectEx(SObject obj, SObjectController controller)
        => new SObjectExternalBK(obj, controller);

    /// <inheritdoc/>
    public override SObjectExternal CreateSObjectEx(SObject obj, TypeDefinition type)
        => new SObjectExternalBK(obj, type);

    /// <inheritdoc/>
    public override SObjectExternal CreateSObjectEx(SObject obj, TypeDefinition type, SObjectController controller)
        => new SObjectExternalBK(obj, type, controller);

    /// <inheritdoc/>
    public override SArrayExternal CreateSArrayEx(SArray ary)
        => new SArrayExternalBK(ary);

    /// <inheritdoc/>
    public override SArrayExternal CreateSArrayEx(SArray ary, IEnumerable<object> values)
        => new SArrayExternalBK(ary, values);

    /// <inheritdoc/>
    public override Type ResolveSType(Type type)
    {
        if (type is null)
        {
            return typeof(SValue);
        }

        if (typeof(SItem).IsAssignableFrom(type))
        {
            return type;
        }

        TypeCode typeCode = Type.GetTypeCode(type);

        switch (typeCode)
        {
            case TypeCode.Boolean:
                return typeof(SBoolean);

            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return typeof(SNumeric);

            case TypeCode.DateTime:
                return typeof(SDateTime);

            case TypeCode.String:
            case TypeCode.Char:
                return typeof(SString);

            case TypeCode.Object:
                if (typeof(TextBlock).IsAssignableFrom(type))
                {
                    return typeof(STextBlock);
                }
                else
                {
                    return typeof(SUnknownValue);
                }

            case TypeCode.Empty:
            case TypeCode.DBNull:
            default:
                return typeof(SNull);
        }
    }

    /// <inheritdoc/>
    public override SItem ResolveSItem(object value)
    {
        if (value is SItem item)
        {
            return item;
        }

        if (value is SObjectController ctrl)
        {
            if (ctrl.Target is null)
            {
                new SObject(ctrl);
            }

            return ctrl.Target;
        }

        if (value is null)
        {
            return new SNull();
        }

        TypeCode typeCode = Type.GetTypeCode(value.GetType());

        switch (typeCode)
        {
            case TypeCode.Boolean:
                return new SBoolean(value);

            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return new SNumeric(value);

            case TypeCode.DateTime:
                return new SDateTime(value);

            case TypeCode.String:
                return new SString(value);

            case TypeCode.Char:
                return new SString(value.ToString());

            case TypeCode.Object:
                {
                    if (value is TextBlock block)
                    {
                        return new STextBlock(block);
                    }
                    else
                    {
                        return new SUnknownValue(value);
                    }
                }
                
            case TypeCode.Empty:
            case TypeCode.DBNull:
            default:
                return new SNull();
        }
    }

    /// <inheritdoc/>
    public override object ResolveValue(SItem item, ICondition context = null)
    {
        if (item is SValue sValue)
        {
            return sValue.GetValue(context);
        }
        else
        {
            return item;
        }
    }

    /// <inheritdoc/>
    public override object ResolveValue(object value, ICondition context = null)
    {
        if (value is SValue sValue)
        {
            return sValue.GetValue(context);
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc/>
    public override object ResolveOriginValue(object value, ICondition context = null)
    {
        if (value is SValue sValue)
        {
            return sValue.Value;
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc/>
    public override bool IsMeOrParent(SItem item, SItem parent)
    {
        while (item != null)
        {
            if (ReferenceEquals(item, parent))
            {
                return true;
            }
            item = item.Parent;
        }
        return false;
    }

    [ThreadStatic]
    private static StringBuilder _dotPathBuilder;

    /// <inheritdoc/>
    public override string GetPath(SItem item)
    {
        if (item.Parent is null)
        {
            return string.Empty;
        }

        _dotPathBuilder ??= new StringBuilder();

        _dotPathBuilder.Clear();
        BuildPath(item, _dotPathBuilder);
        
        return _dotPathBuilder.ToString();
    }

    /// <summary>
    /// Recursively builds the dot-path representation of an item's position in the hierarchy.
    /// </summary>
    /// <param name="item">The item to build the path for.</param>
    /// <param name="builder">The string builder to append to.</param>
    private void BuildPath(SItem item, StringBuilder builder)
    {
        if (item.IsRoot)
        {
            return;
        }

        if (!item.Parent.IsRoot)
        {
            builder.Append('.');
        }

        int index = item.Index;
        if (index >= 0)
        {
            builder.Append('[');
            builder.Append(index);
            builder.Append(']');
            return;
        }

        string name = item.Name;
        if (!string.IsNullOrEmpty(name))
        {
            builder.Append(name);
        }
        else
        {
            builder.Append("???");
        }
    }
}
