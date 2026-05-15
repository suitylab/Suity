using Suity.Drawing;
using Suity.Helpers;
using System;
using System.Linq;

namespace Suity.Editor.Types;

#region DNativeStruct

/// <summary>
/// Represents a native struct type.
/// </summary>
public class DNativeStruct : DStruct, INativeType
{
    /// <summary>
    /// Initializes a new instance of the DNativeStruct class.
    /// </summary>
    public DNativeStruct()
    {
        UpdateAssetTypes(typeof(INativeType));
    }

    /// <summary>
    /// Initializes a new instance of the DNativeStruct class with a native type.
    /// </summary>
    public DNativeStruct(Type nativeType)
        : this()
    {
        NativeType = nativeType;
    }

    /// <summary>
    /// Initializes a new instance of the DNativeStruct class with a name and native type.
    /// </summary>
    public DNativeStruct(string name, Type nativeType)
        : this(nativeType)
    {
        LocalName = name;
    }

    /// <inheritdoc />
    public override Type NativeType { get; }

    /// <inheritdoc />
    public override bool IsNative => true;

    /// <inheritdoc />
    public override bool UseNativeFields => true;

    /// <inheritdoc />
    public override Guid GetFieldId(string name)
        => TypesExternal._external.ResolveNativeFieldId(this, name);
}

#endregion

#region DNativeFunction

/// <summary>
/// Represents a native function type.
/// </summary>
public class DNativeFunction : DFunction, INativeType
{
    /// <summary>
    /// Initializes a new instance of the DNativeFunction class.
    /// </summary>
    public DNativeFunction()
    {
        UpdateAssetTypes(typeof(INativeType));
    }

    /// <summary>
    /// Initializes a new instance of the DNativeFunction class with a native type.
    /// </summary>
    public DNativeFunction(Type nativeType)
        : this()
    {
        NativeType = nativeType;
    }

    /// <summary>
    /// Initializes a new instance of the DNativeFunction class with a name and native type.
    /// </summary>
    public DNativeFunction(string name, Type nativeType)
        : this(nativeType)
    {
        LocalName = name;
    }

    /// <inheritdoc />
    public override Type NativeType { get; }

    /// <inheritdoc />
    public override bool UseNativeFields => true;

    /// <inheritdoc />
    public override Guid GetFieldId(string name)
        => TypesExternal._external.ResolveNativeFieldId(this, name);
}

#endregion

#region DNativeEnum

/// <summary>
/// Represents a native enum type.
/// </summary>
public class DNativeEnum : DEnum, INativeType
{
    /// <summary>
    /// Initializes a new instance of the DNativeEnum class.
    /// </summary>
    public DNativeEnum()
    {
        UpdateAssetTypes(typeof(INativeType));
    }

    /// <summary>
    /// Initializes a new instance of the DNativeEnum class with a native type.
    /// </summary>
    public DNativeEnum(Type nativeType)
        : this()
    {
        NativeType = nativeType;
    }

    /// <summary>
    /// Initializes a new instance of the DNativeEnum class with a name and native type.
    /// </summary>
    public DNativeEnum(string name, Type nativeType)
        : this(nativeType)
    {
        LocalName = name;
    }

    /// <inheritdoc />
    public override Type NativeType { get; }

    /// <inheritdoc />
    public override bool UseNativeFields => true;

    /// <inheritdoc />
    public override Guid GetFieldId(string name)
        => TypesExternal._external.ResolveNativeFieldId(this, name);
}

#endregion

#region DPrimative

/// <summary>
/// Represents a primitive (value) type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.NativeValueType, "Native Value")]
public sealed class DPrimative : DType, INativeType
{
    /// <inheritdoc />
    public override Type NativeType { get; }

    /// <summary>
    /// Gets the type code.
    /// </summary>
    public TypeCode TypeCode { get; }

    /// <summary>
    /// Initializes a new instance of the DPrimative class.
    /// </summary>
    internal DPrimative(string name, Type type, TypeCode typeCode, string icon)
        : base(name)
    {
        NativeType = type;
        TypeCode = typeCode;
        IconKey = icon;
    }

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.Value;

    /// <inheritdoc />
    public override bool IsNative => true;

    /// <inheritdoc />
    public override bool IsPrimitive => true;

    /// <inheritdoc />
    public override bool IsNumeric => TypeCode.GetIsNumeric();

    /// <summary>
    /// Converts a value to this type.
    /// </summary>
    public object ConvertValue(object value)
    {
        if (value == null)
        {
            return GetDefautValue();
        }

        try
        {
            return Convert.ChangeType(value, TypeCode);
        }
        catch (Exception)
        {
            return GetDefautValue();
        }
    }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object GetDefautValue() => TypeCode switch
    {
        TypeCode.Empty => null,
        TypeCode.Object => null,
        TypeCode.DBNull => null,
        TypeCode.Boolean => default(bool),
        TypeCode.Char => default(char),
        TypeCode.SByte => default(sbyte),
        TypeCode.Byte => default(byte),
        TypeCode.Int16 => default(short),
        TypeCode.UInt16 => default(ushort),
        TypeCode.Int32 => default(int),
        TypeCode.UInt32 => default(uint),
        TypeCode.Int64 => default(long),
        TypeCode.UInt64 => default(ulong),
        TypeCode.Single => default(float),
        TypeCode.Double => default(double),
        TypeCode.Decimal => default(decimal),
        TypeCode.DateTime => default(DateTime),
        TypeCode.String => string.Empty,
        _ => null,
    };

    /// <summary>
    /// Creates a default value.
    /// </summary>
    public object CreateDefaultValue()
    {
        Type type = NativeType;
        return type.IsValueType ? System.Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Determines whether an object is a numeric type.
    /// </summary>
    public static bool IsNumericType(object o)
    {
        if (o is null)
        {
            return false;
        }

        return Type.GetTypeCode(o.GetType()) switch
        {
            System.TypeCode.Byte or System.TypeCode.SByte or System.TypeCode.UInt16 or System.TypeCode.UInt32 or System.TypeCode.UInt64 or System.TypeCode.Int16 or System.TypeCode.Int32 or System.TypeCode.Int64 or System.TypeCode.Decimal or System.TypeCode.Double or System.TypeCode.Single => true,
            _ => false,
        };
    }

    public override string ToDataId(bool simplified = false)
    {
        if (!simplified)
        {
            return base.ToDataId();
        }

        return TypeCode switch
        {
            TypeCode.Boolean => "bool",
            TypeCode.Byte => "byte",
            TypeCode.Char => "char",
            TypeCode.DateTime => "System.DateTime",
            TypeCode.DBNull => "null",
            TypeCode.Decimal => "decimal",
            TypeCode.Double => "double",
            TypeCode.Empty => "void",
            TypeCode.Int16 => "short",
            TypeCode.Int32 => "int",
            TypeCode.Int64 => "long",
            TypeCode.Object => "object",
            TypeCode.SByte => "sbyte",
            TypeCode.Single => "float",
            TypeCode.String => "string",
            TypeCode.UInt16 => "ushort",
            TypeCode.UInt32 => "uint",
            TypeCode.UInt64 => "ulong",
            _ => "void",
        };
    }
}

#endregion

#region DNativeObject

/// <summary>
/// Represents a native type in the editor.
/// </summary>
public class DNativeType : DType, INativeType
{
    private readonly Type _type;

    /// <summary>
    /// Initializes a new instance of the DNativeType class.
    /// </summary>
    internal DNativeType(string name, Type type, string icon = null, string description = null)
        : base(name)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));

        LocalName = name;
        IconKey = icon;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the DNativeType class from an attribute.
    /// </summary>
    public DNativeType(Type type, NativeTypeAttribute attr)
        : base(attr.Name ?? type.Name)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));

        LocalName = attr.Name ?? type.Name;
        IconKey = attr.Icon;
        Description = attr.Description;

        if (!string.IsNullOrWhiteSpace(attr.Color))
        {
            try
            {
                this.ViewColor = ColorTranslators.FromHtml(attr.Color);
            }
            catch (Exception)
            {
                this.ViewColor = null;
            }
        }
    }

    /// <inheritdoc />
    public override Type NativeType => _type;

    /// <inheritdoc />
    public override bool IsNative => true;

    /// <inheritdoc />
    public override bool IsAssignableFrom(TypeDefinition implementType)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType))
        {
            return false;
        }

        if (_type == typeof(object))
        {
            return true;
        }

        var otherType = implementType.Target?.NativeType;
        if (otherType is null)
        {
            return false;
        }

        return _type.IsAssignableFrom(otherType);
    }

    protected override void OnAssetActivate(string assetKey)
    {
        base.OnAssetActivate(assetKey);

        Guid id = this.Id;
        if (id != Guid.Empty)
        {
            //if (_alias != null)
            //{
            //    foreach (var alias in _alias)
            //    {
            //        GlobalIdResolver.Record(alias, id, false);
            //    }
            //}

            var aliasAttrs = _type.GetAttributesCached<NativeAliasAttribute>();
            if (aliasAttrs.Any())
            {
                foreach (var alias in aliasAttrs)
                {
                    if (!string.IsNullOrWhiteSpace(alias.AliasName))
                    {
                        var legacyId = GlobalIdResolver.Resolve(alias.AliasName);
                        if (legacyId != Guid.Empty)
                        {
                            //GlobalIdResolver.Record(alias.AliasName, id, false);
                            EditorObjectManager.Instance.RegisterSystemAlias(legacyId, this);
                        }
                    }
                }
            }
        }
    }
}

#endregion
