using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Types;

/// <summary>
/// Type relationships
/// </summary>
public enum TypeRelationships
{
    None,
    Value,
    Struct,
    Array,
    Enum,
    DataLink,
    AssetLink,
    Delegate,
    AbstractFunction,
    AbstractStruct,
    AbstractNumeric,
}

/// <summary>
/// Type
/// </summary>
public abstract class TypeDefinition : IEquatable<TypeDefinition>
{
    internal TypeDefinition()
    { }

    /// <summary>
    /// Full type name
    /// </summary>
    public abstract string TypeCode { get; }

    /// <summary>
    /// Whether the type is immutable. Immutable types are cached in the type pool.
    /// </summary>
    public abstract bool IsImmutable { get; }

    /// <summary>
    /// Gets the target DType.
    /// </summary>
    public virtual DType Target { get; }

    /// <summary>
    /// Gets the presented target DType.
    /// </summary>
    public virtual DType PresentedTarget => Target;

    /// <summary>
    /// Gets the target ID.
    /// </summary>
    public virtual Guid TargetId => Guid.Empty;

    /// <summary>
    /// Gets the presented target ID.
    /// </summary>
    public virtual Guid PresentedTargetId => TargetId;

    /// <summary>
    /// Gets the target DType filtered by the asset filter.
    /// </summary>
    /// <param name="filter">The asset filter.</param>
    /// <returns>The filtered target DType.</returns>
    public DType GetTarget(IAssetFilter filter)
    {
        var target = Target;

        return filter.FilterAsset(target) ? target : null;
    }
    public DType GetPresentedTarget(IAssetFilter filter)
    {
        var target = PresentedTarget;

        return filter.FilterAsset(target) ? target : null;
    }

    /// <summary>
    /// Gets the type relationship.
    /// </summary>
    public virtual TypeRelationships Relationship { get; }

    /// <summary>
    /// Gets whether this is an empty type.
    /// </summary>
    public virtual bool IsEmpty => false;

    /// <summary>
    /// Gets whether this type is broken.
    /// </summary>
    public virtual bool IsBroken => false;

    /// <summary>
    /// Whether it is a primitive type (has no element type)
    /// </summary>
    public virtual bool IsOrigin => false;

    /// <summary>
    /// Whether it is a native type
    /// </summary>
    public virtual bool IsNative => false;

    /// <summary>
    /// Gets whether this is a primitive type.
    /// </summary>
    public virtual bool IsPrimitive => false;

    /// <summary>
    /// Gets whether this is a numeric type.
    /// </summary>
    public virtual bool IsNumeric => false;

    /// <summary>
    /// Gets whether it is a function.
    /// </summary>
    public virtual bool IsFunction => false;

    /// <summary>
    /// Gets whether it is an array.
    /// </summary>
    public bool IsArray => Relationship == TypeRelationships.Array;

    /// <summary>
    /// Gets whether it is a data link.
    /// </summary>
    public bool IsDataLink => Relationship == TypeRelationships.DataLink;

    /// <summary>
    /// Gets whether it is an asset link.
    /// </summary>
    public bool IsAssetLink => Relationship == TypeRelationships.AssetLink;

    /// <summary>
    /// Gets whether it is any link (data or asset).
    /// </summary>
    public bool IsLink => IsDataLink || IsAssetLink;

    /// <summary>
    /// Gets whether it is a value type.
    /// </summary>
    public bool IsValue => Relationship == TypeRelationships.Value;

    /// <summary>
    /// Gets whether it is a struct.
    /// </summary>
    public bool IsStruct => Relationship == TypeRelationships.Struct;

    /// <summary>
    /// Gets whether it is any struct (regular or abstract).
    /// </summary>
    public bool IsAnyStruct => Relationship == TypeRelationships.Struct || Relationship == TypeRelationships.AbstractStruct;

    /// <summary>
    /// Gets whether it is an enum.
    /// </summary>
    public bool IsEnum => Relationship == TypeRelationships.Enum;

    /// <summary>
    /// Gets whether it is a delegate.
    /// </summary>
    public bool IsDelegate => Relationship == TypeRelationships.Delegate;

    /// <summary>
    /// Gets whether it is an abstract struct.
    /// </summary>
    public bool IsAbstractStruct => Relationship == TypeRelationships.AbstractStruct;

    /// <summary>
    /// Gets whether it is an abstract function.
    /// </summary>
    public bool IsAbstractFunction => Relationship == TypeRelationships.AbstractFunction;

    /// <summary>
    /// Gets whether it is an abstract struct or function.
    /// </summary>
    public bool IsAbstractStructOrFunction => Relationship == TypeRelationships.AbstractStruct || Relationship == TypeRelationships.AbstractFunction;

    /// <summary>
    /// Gets whether it is an abstract numeric type.
    /// </summary>
    public bool IsAbstractNumeric => Relationship == TypeRelationships.AbstractNumeric;

    /// <summary>
    /// Gets whether it is abstract.
    /// </summary>
    public abstract bool IsAbstract { get; }

    /// <summary>
    /// Gets whether it is an abstract array.
    /// </summary>
    public abstract bool IsAbstractArray { get; }

    /// <summary>
    /// Gets the underlying element type of this type, returns null if this type has no element type.
    /// </summary>
    public virtual TypeDefinition ElementType => null;

    /// <summary>
    /// Gets the original type of the type, this method retrieves recursively until the bottommost original type, will not be null.
    /// </summary>
    public virtual TypeDefinition OriginType => this;

    /// <summary>
    /// Abstract type
    /// </summary>
    public virtual TypeDefinition BaseAbstractType => null;

    /// <summary>
    /// Gets the preferred implementation type when used as an abstract type
    /// </summary>
    public virtual TypeDefinition PrimaryType => null;

    /// <summary>
    /// Gets the system type
    /// </summary>
    public virtual Type NativeType => null;

    /// <summary>
    /// Gets the generic parameter types.
    /// </summary>
    /// <returns>Array of type definitions.</returns>
    public virtual TypeDefinition[] GetGenericParameters() => [];

    /// <summary>
    /// Gets the number of generic parameters.
    /// </summary>
    public virtual int GenericParameterCount => 0;

    #region IReferencer

    /// <summary>
    /// Visits reference synchronization.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync handler.</param>
    /// <param name="matchRename">Whether to match rename.</param>
    /// <param name="messageGetter">Optional message getter function.</param>
    internal virtual void VisitReferenceSync(SyncPath path, IReferenceSync sync, ref bool matchRename, Func<string> messageGetter = null)
    {
    }

    /// <summary>
    /// Synchronizes references for a type definition.
    /// </summary>
    /// <param name="definition">The type definition to sync.</param>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync handler.</param>
    /// <param name="messageGetter">Optional message getter function.</param>
    /// <returns>The synced type definition.</returns>
    public static TypeDefinition ReferenceSync(TypeDefinition definition, SyncPath path, IReferenceSync sync, Func<string> messageGetter = null)
    {
        return TypesExternal._external.ReferenceSync(definition, path, sync, messageGetter);
    }

    #endregion

    #region ToString

    /// <summary>
    /// Converts to type name string.
    /// </summary>
    /// <returns>The type name.</returns>
    public virtual string ToTypeName() => string.Empty;

    /// <summary>
    /// Converts to export string.
    /// </summary>
    /// <param name="simplified">Whether to use simplified format.</param>
    /// <returns>The export string.</returns>
    public virtual string ToExportString(bool simplified) => string.Empty;

    /// <summary>
    /// Converts to display string.
    /// </summary>
    /// <returns>The display string.</returns>
    public virtual string ToDisplayString() => string.Empty;

    #endregion

    #region Equality

    public override int GetHashCode()
    {
        return TypeCode.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not TypeDefinition other)
        {
            return false;
        }

        return TypeCode == other.TypeCode;
    }

    public bool Equals(TypeDefinition other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return TypeCode == other.TypeCode;
    }

    public static bool operator ==(TypeDefinition v1, TypeDefinition v2)
    {
        if (Equals(v1, null)) return Equals(v2, null); else return v1.Equals(v2);
    }

    public static bool operator !=(TypeDefinition v1, TypeDefinition v2)
    {
        if (Equals(v1, null)) return !Equals(v2, null); else return !v1.Equals(v2);
    }

    #endregion

    #region Make

    /// <summary>
    /// Creates a data link type from this type.
    /// </summary>
    /// <returns>The data link type.</returns>
    public TypeDefinition MakeDataLinkType() => TypesExternal._external.MakeDataLinkType(this);

    /// <summary>
    /// Creates an asset link type from this type.
    /// </summary>
    /// <returns>The asset link type.</returns>
    public TypeDefinition MakeAssetLinkType() => TypesExternal._external.MakeAssetLinkType(this);

    /// <summary>
    /// Creates an array type from this type.
    /// </summary>
    /// <returns>The array type.</returns>
    public TypeDefinition MakeArrayType() => TypesExternal._external.MakeArrayType(this);

    /// <summary>
    /// Creates an abstract function type from this type.
    /// </summary>
    /// <returns>The abstract function type.</returns>
    public TypeDefinition MakeAbstractFunctionType() => TypesExternal._external.MakeAbstractFunctionType(this);

    /// <summary>
    /// Creates an abstract function array type from this type.
    /// </summary>
    /// <returns>The abstract function array type.</returns>
    public TypeDefinition MakeAbstractFunctionArrayType() => TypesExternal._external.MakeAbstractFunctionArrayType(this);

    /// <summary>
    /// Creates a generic type with the specified parameters.
    /// </summary>
    /// <param name="parameters">The type parameters.</param>
    /// <returns>The constructed generic type.</returns>
    public TypeDefinition MakeGenericType(params TypeDefinition[] parameters) => TypesExternal._external.MakeGenericType(this, parameters);

    /// <summary>
    /// Creates a generic type with the specified parameters.
    /// </summary>
    /// <param name="parameters">The type parameters.</param>
    /// <returns>The constructed generic type.</returns>
    public TypeDefinition MakeGenericType(IEnumerable<TypeDefinition> parameters) => TypesExternal._external.MakeGenericType(this, parameters);

    #endregion

    #region Validation

    public abstract bool IsAssignableFrom(TypeDefinition implementType, IAssetFilter filter = null);

    public static TypeDefinition Empty => TypesExternal._external.Empty;
    public static TypeDefinition Unknown => TypesExternal._external.Unknown;

    public static bool IsNullOrEmpty(TypeDefinition type)
    {
        return type is null || string.IsNullOrEmpty(type.TypeCode);
    }

    public static bool IsNullOrBroken(TypeDefinition type)
    {
        return type is null || string.IsNullOrEmpty(type.TypeCode) || type.IsBroken;
    }

    #endregion

    #region Resolve

    /// <summary>
    /// Resolves type definition
    /// </summary>
    /// <param name="name">Name, can be Id string, native type name, or exported resource name</param>
    /// <param name="resolveExportedName">Try to resolve exported resource name</param>
    /// <returns></returns>
    public static TypeDefinition Resolve(string name, bool resolveExportedName = true)
        => TypesExternal._external.Resolve(name, resolveExportedName, false);

    /// <summary>
    /// Resolves type definition by type name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TypeDefinition ResolveWithTypeName(string name)
        => TypesExternal._external.Resolve(name, false, true);

    /// <summary>
    /// Resolves type definition
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static TypeDefinition Resolve(Guid id)
        => TypesExternal._external.Resolve(id);

    public static string GetPrefix(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        if (type.IsArray)
        {
            return TypesExternal._external.GetPrefix(type.ElementType.Relationship);
        }
        else
        {
            return TypesExternal._external.GetPrefix(type.Relationship);
        }
    }

    public static void SplitPrefix(string name, out string prefix, out string originName)
        => TypesExternal._external.SplitPrefix(name, out prefix, out originName);

    public static TypeDefinition ResolveExportedDefinition(string key, out string prefix, out string originKey)
        => TypesExternal._external.ResolveExportedDefinition(key, out prefix, out originKey);

    public static TypeDefinition FromNative<T>()
        => NativeTypeExternal._external.GetTypeDefinition(typeof(T));

    public static TypeDefinition FromNative(Type type)
        => NativeTypeExternal._external.GetTypeDefinition(type);

    public static TypeDefinition FromAssetLink<T>()
         => NativeTypeExternal._external.GetAssetLinkDefinition(typeof(T));

    public static TypeDefinition FromAssetLink(Type type)
        => NativeTypeExternal._external.GetAssetLinkDefinition(type);

    #endregion
}