using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Types;

#region BackendTypeDefinition

/// <summary>
/// Base class for backend type definitions, providing common type metadata and relationship information.
/// </summary>
public abstract class TypeDefinitionBK : TypeDefinition
{
    internal TypeDefinitionBK(string typeCode)
    {
        TypeCode = typeCode;
    }

    /// <inheritdoc/>
    public override string TypeCode { get; }

    /// <inheritdoc/>
    public override bool IsImmutable => ElementType?.IsImmutable ?? true;

    /// <inheritdoc/>
    public override bool IsBroken => string.IsNullOrEmpty(TypeCode) || Target == null;

    /// <inheritdoc/>
    public override bool IsAbstract => Relationship switch
    {
        TypeRelationships.AbstractFunction or TypeRelationships.AbstractStruct or TypeRelationships.AbstractNumeric => true,
        _ => false,
    };

    /// <inheritdoc/>
    public override bool IsAbstractArray => IsArray && (ElementType?.IsAbstract == true);

    /// <inheritdoc/>
    public override bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter = null)
    {
        //TODO: Handle filter parameter of IsAssignableFrom
        return implementType == this;
    }
}

#endregion

#region TypeRefDefinition

/// <summary>
/// Represents a type reference definition that resolves to a <see cref="DType"/> asset by ID.
/// </summary>
public class TypeRefDefinition : TypeDefinitionBK
{
    private readonly EditorObjectRef<DType> _typeRef = new();

    /// <inheritdoc/>
    public override bool IsImmutable { get; }

    /// <inheritdoc/>
    public override bool IsOrigin => true;

    /// <inheritdoc/>
    public override TypeRelationships Relationship => _typeRef.Target?.Relationship ?? TypeRelationships.None;

    /// <inheritdoc/>
    public override bool IsNative => _typeRef.Target?.IsNative ?? false;
    /// <inheritdoc/>
    public override bool IsPrimitive => _typeRef.Target?.IsPrimitive ?? false;
    /// <inheritdoc/>
    public override bool IsNumeric => _typeRef.Target?.IsNumeric ?? false;
    /// <inheritdoc/>
    public override bool IsFunction => _typeRef.Target is DFunction;

    /// <inheritdoc/>
    public override DType Target => _typeRef.Target;
    /// <inheritdoc/>
    public override Guid TargetId => _typeRef.Id;

    /// <inheritdoc/>
    public override TypeDefinition BaseAbstractType => _typeRef.Target?.BaseTypeDefinition;
    /// <inheritdoc/>
    public override TypeDefinition PrimaryType => _typeRef.Target?.PrimaryTypeDefinition;
    /// <inheritdoc/>
    public override Type NativeType => _typeRef.Target?.NativeType;

    /// <inheritdoc/>
    public override TypeDefinition ElementType => _typeRef.Target?.ElementTypeDefinition;
    /// <inheritdoc/>
    public override TypeDefinition OriginType => this;

    internal TypeRefDefinition(Guid id, bool isImmutable)
        : base(id.ToString())
    {
        _typeRef.Id = id;
        IsImmutable = isImmutable;
    }

    internal TypeRefDefinition(DType target, bool isImmutable)
        : this(target.Id, isImmutable)
    {
    }

    /// <inheritdoc/>
    public override bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType))
        {
            return false;
        }

        if (this.IsArray != implementType.IsArray)
        {
            return false;
        }

        return Target?.IsAssignableFrom(implementType) == true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _typeRef.Target?.ToString() ?? _typeRef.ToString();
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return _typeRef.Target?.AssetKey ?? _typeRef.Id.ToString() ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return _typeRef.Target?.ToDataId(simplified) ?? _typeRef.Id.ToString() ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return _typeRef.Target?.DisplayText ?? _typeRef.Id.ToString() ?? string.Empty;
    }

    internal override void VisitReferenceSync(SyncPath path, IReferenceSync sync, ref bool matchRename, Func<string> messageGetter = null)
    {
        sync.SyncId(path, _typeRef.Id, messageGetter?.Invoke());

        if (sync.Mode == ReferenceSyncMode.Redirect && _typeRef.Id == sync.OldId)
        {
            matchRename = true;
        }
    }
}

#endregion

#region GenericTypeRefDefinition

/// <summary>
/// Represents a generic type reference definition with type parameters.
/// </summary>
public sealed class GenericTypeRefDefinition : TypeDefinitionBK
{
    private readonly TypeDefinition[] _parameters;

    /// <inheritdoc/>
    public override TypeRelationships Relationship => ElementType?.Relationship ?? TypeRelationships.None;

    /// <inheritdoc/>
    public override bool IsNative => ElementType?.IsNative ?? false;
    /// <inheritdoc/>
    public override bool IsFunction => ElementType?.IsFunction ?? false;

    /// <inheritdoc/>
    public override TypeDefinition ElementType { get; }
    /// <inheritdoc/>
    public override TypeDefinition OriginType => this;

    /// <inheritdoc/>
    public override TypeDefinition[] GetGenericParameters() => [.. _parameters];

    /// <inheritdoc/>
    public override int GenericParameterCount => _parameters.Length;

    internal GenericTypeRefDefinition(TypeDefinition baseType, IEnumerable<TypeDefinition> parameters)
        : base($"{baseType.TypeCode}<{string.Join(",", parameters.Select(o => o.TypeCode))}>")
    {
        ElementType = baseType ?? throw new ArgumentNullException(nameof(baseType));
        _parameters = [.. parameters];
        if (_parameters.Any(o => o is null))
        {
            throw new ArgumentNullException(nameof(parameters));
        }
    }

    /// <inheritdoc/>
    public override bool IsImmutable => ElementType.IsImmutable || _parameters.Any(o => !o.IsImmutable);


    /// <inheritdoc/>
    public override string ToString()
    {
        string p = string.Join(",", _parameters.Select(o => o.ToString()));

        return $"{ElementType}<{p}>";
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        string p = string.Join(",", _parameters.Select(o => o.ToTypeName()));

        return $"{ElementType.ToTypeName()}<{p}>";
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        string p = string.Join(",", _parameters.Select(o => o.ToExportString(simplified)));

        return $"{ElementType.ToExportString(simplified)}<{p}>";
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        string p = string.Join(",", _parameters.Select(o => o.ToDisplayString()));

        return $"{ElementType.ToDisplayString()}<{p}>";
    }

    internal override void VisitReferenceSync(SyncPath path, IReferenceSync sync, ref bool matchRename, Func<string> messageGetter = null)
    {
        base.VisitReferenceSync(path, sync, ref matchRename, messageGetter);

        foreach (var p in _parameters)
        {
            p.VisitReferenceSync(path, sync, ref matchRename, null);
        }
    }
}

#endregion

#region DataLinkTypeDefinition

/// <summary>
/// Represents a data link type definition, which wraps another type as a data link reference.
/// </summary>
public sealed class DataLinkTypeDefinition : TypeDefinitionBK
{
    /// <inheritdoc/>
    public override TypeRelationships Relationship => TypeRelationships.DataLink;

    /// <inheritdoc/>
    public override TypeDefinition ElementType { get; }
    /// <inheritdoc/>
    public override TypeDefinition OriginType => ElementType.OriginType;

    /// <inheritdoc/>
    public override DType Target => ElementType.Target;
    /// <inheritdoc/>
    public override Guid TargetId => ElementType.TargetId;

    /// <inheritdoc/>
    public override DType PresentedTarget => NativeTypes.StringType.Target;
    /// <inheritdoc/>
    public override Guid PresentedTargetId => NativeTypes.StringType.TargetId;

    internal DataLinkTypeDefinition(TypeDefinition elementType)
        : base($"@{elementType.TypeCode}")
    {
        ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
    }

    /// <inheritdoc/>
    public override bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter = null)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType) || !implementType.IsDataLink)
        {
            return false;
        }

        return this.ElementType?.Target?.IsAssignableFrom(implementType.ElementType) ?? false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"@{ElementType}";
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return $"@{ElementType.ToTypeName()}";
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return $"@{ElementType.ToExportString(simplified)}";
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return $"@{ElementType.ToDisplayString()}";
    }
}

#endregion

#region AssetLinkTypeDefinition

/// <summary>
/// Represents an asset link type definition, which wraps another type as an asset reference.
/// </summary>
public sealed class AssetLinkTypeDefinition : TypeDefinitionBK
{
    /// <inheritdoc/>
    public override TypeRelationships Relationship => TypeRelationships.AssetLink;

    /// <inheritdoc/>
    public override TypeDefinition ElementType { get; }
    /// <inheritdoc/>
    public override TypeDefinition OriginType => ElementType.OriginType;

    /// <inheritdoc/>
    public override DType Target => ElementType.Target;
    /// <inheritdoc/>
    public override Guid TargetId => ElementType.TargetId;

    /// <inheritdoc/>
    public override DType PresentedTarget => NativeTypes.StringType.Target;
    /// <inheritdoc/>
    public override Guid PresentedTargetId => NativeTypes.StringType.TargetId;

    /// <inheritdoc/>
    public override Type NativeType => ElementType.Target?.NativeType;

    internal AssetLinkTypeDefinition(TypeDefinition elementType)
        : base($"&{elementType.TypeCode}")
    {
        ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"&{ElementType}";
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return $"&{ElementType.ToTypeName()}";
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return $"&{ElementType.ToExportString(simplified)}";
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return $"&{ElementType.ToDisplayString()}";
    }
}

#endregion

#region AbstractFunctionTypeDefinition

/// <summary>
/// Represents an abstract function type definition, used for function return type matching.
/// </summary>
public sealed class AbstractFunctionTypeDefinition : TypeDefinitionBK
{
    /// <inheritdoc/>
    public override TypeRelationships Relationship => TypeRelationships.AbstractFunction;

    /// <inheritdoc/>
    public override TypeDefinition ElementType { get; }
    /// <inheritdoc/>
    public override TypeDefinition OriginType => ElementType.OriginType;

    /// <inheritdoc/>
    public override DType Target => ElementType.Target;
    /// <inheritdoc/>
    public override Guid TargetId => ElementType.TargetId;
    /// <inheritdoc/>
    public override DType PresentedTarget => null;
    /// <inheritdoc/>
    public override Guid PresentedTargetId => Guid.Empty;
    /// <inheritdoc/>
    public override bool IsBroken => ElementType.IsBroken;

    /// <inheritdoc/>
    public override TypeDefinition PrimaryType => DTypeManager.Instance.GetFunctionsByReturnType(ElementType)?.PrimaryAsset?.Definition;

    internal AbstractFunctionTypeDefinition(TypeDefinition elementType)
        : base($"#{elementType.TypeCode}")
    {
        ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
    }

    /// <inheritdoc/>
    public override bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType))
        {
            return false;
        }

        if (!implementType.IsFunction)
        {
            return false;
        }

        if (ElementType == implementType)
        {
            return true;
        }

        //if (implementType.BaseAbstractType != null && TypeDefinition.IsNullOrEmpty(implementType.BaseAbstractType.ElementType))
        //{
        //    // Dynamic type
        //    return true;
        //}

        if (implementType.BaseAbstractType == this)
        {
            return true;
        }

        if (ElementType.IsAssignableFrom(implementType.BaseAbstractType.ElementType))
        {
            return true;
        }

        if (implementType.Target is DFunction dfunc)
        {
            // Dynamic type
            if (ElementType.IsArray || ElementType.IsAbstractArray)
            {
                if ((dfunc.ReturnTypeBinding & DReturnTypeBinding.Array) != DReturnTypeBinding.None)
                {
                    return true;
                }
            }
            else
            {
                if ((dfunc.ReturnTypeBinding & DReturnTypeBinding.Object) != DReturnTypeBinding.None)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"#{ElementType}";
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return $"#{ElementType.ToTypeName()}";
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return $"#{ElementType.ToExportString(simplified)}";
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return $"#{ElementType.ToDisplayString()}";
    }
}

#endregion

#region ArrayTypeDefinition

/// <summary>
/// Represents an array type definition wrapping an element type.
/// </summary>
public sealed class ArrayTypeDefinition : TypeDefinitionBK
{
    /// <inheritdoc/>
    public override TypeRelationships Relationship => TypeRelationships.Array;

    /// <inheritdoc/>
    public override TypeDefinition ElementType { get; }
    /// <inheritdoc/>
    public override TypeDefinition OriginType => ElementType.OriginType;

    /// <inheritdoc/>
    public override DType Target => ElementType.Target;
    /// <inheritdoc/>
    public override Guid TargetId => ElementType.TargetId;
    /// <inheritdoc/>
    public override DType PresentedTarget => null;
    /// <inheritdoc/>
    public override Guid PresentedTargetId => Guid.Empty;

    internal ArrayTypeDefinition(TypeDefinition elementType)
        : base(MakeName(elementType, elementType.TypeCode))
    {
        if (elementType is ArrayTypeDefinition)
        {
            throw new InvalidOperationException();
        }

        ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return MakeName(ElementType, ElementType.ToString());
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return MakeName(ElementType, ElementType.ToTypeName());
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return MakeName(ElementType, ElementType.ToExportString(simplified));
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return MakeName(ElementType, ElementType.ToDisplayString());
    }

    /// <summary>
    /// Creates a type code name for the array type based on the element type.
    /// </summary>
    /// <param name="elementType">The element type of the array.</param>
    /// <param name="name">The base name to transform.</param>
    /// <returns>The array type code name.</returns>
    internal static string MakeName(TypeDefinition elementType, string name)
    {
        if (elementType.IsAbstractFunction)
        {
            return $"([]){name}";
        }
        else
        {
            return $"{name}[]";
        }
    }
}

#endregion

#region NumericTypeDefinition

/// <summary>
/// Represents an abstract numeric type that matches all numeric types.
/// </summary>
public sealed class NumericTypeDefinition : TypeDefinitionBK
{
    /// <summary>
    /// Gets the singleton instance of <see cref="NumericTypeDefinition"/>.
    /// </summary>
    public static NumericTypeDefinition Instance { get; } = new NumericTypeDefinition();

    internal NumericTypeDefinition() : base(NativeTypes.NumericTypeName)
    {
    }

    /// <inheritdoc/>
    public override TypeRelationships Relationship => TypeRelationships.AbstractNumeric;

    /// <inheritdoc/>
    public override bool IsBroken => false;

    /// <inheritdoc/>
    public override bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter = null)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType))
        {
            return false;
        }

        return implementType.IsNumeric;
    }

    /// <inheritdoc/>
    public override string ToTypeName()
    {
        return NativeTypes.NumericTypeName;
    }

    /// <inheritdoc/>
    public override string ToExportString(bool simplified)
    {
        return NativeTypes.NumericTypeName;
    }

    /// <inheritdoc/>
    public override string ToDisplayString()
    {
        return "Numeric Type";
    }
}

#endregion

#region EmptyTypeDefinition

/// <summary>
/// Represents an empty type definition indicating no type.
/// </summary>
public sealed class EmptyTypeDefinition : TypeDefinitionBK
{
    internal EmptyTypeDefinition()
        : base(string.Empty)
    {
    }

    /// <inheritdoc/>
    public override bool IsEmpty => true;

    /// <inheritdoc/>
    public override DType Target => null;

    /// <inheritdoc/>
    public override Guid TargetId => Guid.Empty;

    /// <inheritdoc/>
    public override Guid PresentedTargetId => Guid.Empty;

    /// <inheritdoc/>
    public override TypeDefinition OriginType => this;
}

#endregion

#region UnknownTypeDefinition

/// <summary>
/// Represents an unknown type definition indicating a type that could not be resolved.
/// </summary>
public sealed class UnknownTypeDefinition : TypeDefinitionBK
{
    internal UnknownTypeDefinition()
        : base("<Unknown>")
    {
    }

    /// <inheritdoc/>
    public override bool IsEmpty => true;

    /// <inheritdoc/>
    public override bool IsBroken => true;

    /// <inheritdoc/>
    public override TypeDefinition OriginType => this;
}

#endregion
