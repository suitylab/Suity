using Suity.Selecting;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Provides external type definition services and operations.
/// </summary>
internal abstract class TypesExternal
{
    internal static TypesExternal _external;

    /// <summary>
    /// Gets the empty type definition.
    /// </summary>
    public abstract TypeDefinition Empty { get; }

    /// <summary>
    /// Gets the unknown type definition.
    /// </summary>
    public abstract TypeDefinition Unknown { get; }

    /// <summary>
    /// Synchronizes references for a type definition.
    /// </summary>
    /// <param name="definition">The type definition to sync.</param>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync handler.</param>
    /// <param name="messageGetter">Optional message getter function.</param>
    /// <returns>The synced type definition.</returns>
    public abstract TypeDefinition ReferenceSync(TypeDefinition definition, SyncPath path, IReferenceSync sync, Func<string> messageGetter = null);

    #region Make

    /// <summary>
    /// Creates a type definition from a DType.
    /// </summary>
    /// <param name="type">The DType to create definition from.</param>
    /// <returns>The type definition.</returns>
    public abstract TypeDefinition MakeDefinition(DType type);

    /// <summary>
    /// Creates a data link type from the given type.
    /// </summary>
    /// <param name="type">The base type.</param>
    /// <returns>The data link type definition.</returns>
    public abstract TypeDefinition MakeDataLinkType(TypeDefinition type);

    /// <summary>
    /// Creates an asset link type from the given type.
    /// </summary>
    /// <param name="type">The base type.</param>
    /// <returns>The asset link type definition.</returns>
    public abstract TypeDefinition MakeAssetLinkType(TypeDefinition type);

    /// <summary>
    /// Creates an array type from the given type.
    /// </summary>
    /// <param name="type">The element type.</param>
    /// <returns>The array type definition.</returns>
    public abstract TypeDefinition MakeArrayType(TypeDefinition type);

    /// <summary>
    /// Creates an abstract function type from the given type.
    /// </summary>
    /// <param name="type">The return type.</param>
    /// <returns>The abstract function type definition.</returns>
    public abstract TypeDefinition MakeAbstractFunctionType(TypeDefinition type);

    /// <summary>
    /// Creates an abstract function array type from the given type.
    /// </summary>
    /// <param name="type">The element type.</param>
    /// <returns>The abstract function array type definition.</returns>
    public abstract TypeDefinition MakeAbstractFunctionArrayType(TypeDefinition type);

    /// <summary>
    /// Creates a generic type with the specified parameters.
    /// </summary>
    /// <param name="type">The generic type definition.</param>
    /// <param name="parameters">The type parameters.</param>
    /// <returns>The constructed generic type.</returns>
    public abstract TypeDefinition MakeGenericType(TypeDefinition type, params TypeDefinition[] parameters);

    /// <summary>
    /// Creates a generic type with the specified parameters.
    /// </summary>
    /// <param name="type">The generic type definition.</param>
    /// <param name="parameters">The type parameters.</param>
    /// <returns>The constructed generic type.</returns>
    public abstract TypeDefinition MakeGenericType(TypeDefinition type, IEnumerable<TypeDefinition> parameters);

    #endregion

    #region Resolve

    /// <summary>
    /// Resolves a type definition by name.
    /// </summary>
    /// <param name="name">The type name.</param>
    /// <param name="resolveExportedName">Whether to resolve exported names.</param>
    /// <param name="resolveResource">Whether to resolve resources.</param>
    /// <returns>The resolved type definition.</returns>
    public abstract TypeDefinition Resolve(string name, bool resolveExportedName, bool resolveResource);

    /// <summary>
    /// Resolves a type definition by GUID.
    /// </summary>
    /// <param name="id">The type GUID.</param>
    /// <returns>The resolved type definition.</returns>
    public abstract TypeDefinition Resolve(Guid id);

    /// <summary>
    /// Gets the prefix for a type relationship.
    /// </summary>
    /// <param name="relationship">The type relationship.</param>
    /// <returns>The prefix string.</returns>
    public abstract string GetPrefix(TypeRelationships relationship);

    /// <summary>
    /// Splits a prefixed name into prefix and origin name.
    /// </summary>
    /// <param name="name">The full name with prefix.</param>
    /// <param name="prefix">The extracted prefix.</param>
    /// <param name="originName">The original name without prefix.</param>
    public abstract void SplitPrefix(string name, out string prefix, out string originName);

    /// <summary>
    /// Resolves an exported type definition by key.
    /// </summary>
    /// <param name="key">The exported key.</param>
    /// <param name="prefix">The extracted prefix.</param>
    /// <param name="originKey">The original key.</param>
    /// <returns>The resolved type definition.</returns>
    public abstract TypeDefinition ResolveExportedDefinition(string key, out string prefix, out string originKey);

    /// <summary>
    /// Resolves a native field name from its ID.
    /// </summary>
    /// <param name="type">The DType containing the field.</param>
    /// <param name="id">The field ID.</param>
    /// <returns>The field name.</returns>
    public abstract string ResolveNativeFieldName(DType type, Guid id);

    /// <summary>
    /// Resolves a native field ID from its name.
    /// </summary>
    /// <param name="type">The DType containing the field.</param>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The field GUID.</returns>
    public abstract Guid ResolveNativeFieldId(DType type, string fieldName);

    #endregion

    #region Naming

    /// <summary>
    /// Gets the short type name for a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="alias">Whether to use alias.</param>
    /// <returns>The short type name.</returns>
    public abstract string GetShortTypeName(TypeDefinition typeInfo, bool alias = false);

    /// <summary>
    /// Attempts to get the short type name for a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="alias">Whether to use alias.</param>
    /// <param name="shortTypeName">The short type name if successful.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public abstract bool TryGetShortTypeName(TypeDefinition typeInfo, bool alias, out string shortTypeName);

    /// <summary>
    /// Gets the full type name text.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="alias">Whether to use alias.</param>
    /// <returns>The full type name.</returns>
    public abstract string GetFullTypeNameText(TypeDefinition typeInfo, bool alias = false);

    /// <summary>
    /// Gets the full type name.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <param name="alias">Whether to use alias.</param>
    /// <returns>The full type name.</returns>
    public abstract string GetFullTypeName(TypeDefinition typeInfo, bool alias = false);

    /// <summary>
    /// Gets the implementations of a type.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="filter">Optional asset filter.</param>
    /// <returns>The selection list of implementations.</returns>
    public abstract ISelectionList GetImplementations(TypeDefinition type, IAssetFilter filter = null);

    /// <summary>
    /// Gets the icon for a type definition.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <returns>The icon image.</returns>
    public abstract Image GetIcon(TypeDefinition typeInfo);

    #endregion
}

/// <summary>
/// Provides external native type services.
/// </summary>
internal class NativeTypeExternal
{
    internal static NativeTypeExternal _external = new();

    /// <summary>
    /// Gets the built-in type definition by full name.
    /// </summary>
    /// <param name="fullName">The full type name.</param>
    /// <returns>The type definition.</returns>
    public virtual TypeDefinition GetBuildInTypeDefinition(string fullName) => TypeDefinition.Empty;

    /// <summary>
    /// Gets the type definition for a native type.
    /// </summary>
    /// <param name="type">The native type.</param>
    /// <returns>The type definition.</returns>
    public virtual TypeDefinition GetTypeDefinition(Type type) => TypeDefinition.Empty;

    /// <summary>
    /// Gets the asset link definition for a native type.
    /// </summary>
    /// <param name="type">The native type.</param>
    /// <returns>The asset link type definition.</returns>
    public virtual TypeDefinition GetAssetLinkDefinition(Type type) => TypeDefinition.Empty;

    /// <summary>
    /// Gets the full name of a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The full name.</returns>
    public virtual string GetFullName(string typeName) => string.Empty;

    /// <summary>
    /// Gets the alias for a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The type alias.</returns>
    public virtual string GetNativeTypeAlias(string typeName) => string.Empty;

    /// <summary>
    /// Gets the short name for a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The short name.</returns>
    public virtual string GetNativeTypeShortName(string typeName) => string.Empty;

    /// <summary>
    /// Determines whether the type is a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>True if native; otherwise, false.</returns>
    public virtual bool GetIsNativeType(string typeName) => false;

    /// <summary>
    /// Gets the native type by local name.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <returns>The native type.</returns>
    public virtual Type GetNativeTypeByLocalName(TypeDefinition typeInfo) => null;

    /// <summary>
    /// Gets the native type by name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The native type.</returns>
    public virtual Type GetNativeType(string typeName) => null;
}