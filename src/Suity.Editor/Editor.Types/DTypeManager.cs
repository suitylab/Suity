using System;

namespace Suity.Editor.Types;

/// <summary>
/// Manages type definitions in the editor.
/// </summary>
public abstract class DTypeManager
{
    /// <summary>
    /// Gets or sets the singleton instance.
    /// </summary>
    public static DTypeManager Instance { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the DTypeManager class.
    /// </summary>
    internal DTypeManager()
    { }

    /// <summary>
    /// Gets a DType by asset key.
    /// </summary>
    public abstract T GetDType<T>(string assetKey, IAssetFilter filter = null) where T : DType;

    /// <summary>
    /// Gets a DType by ID.
    /// </summary>
    public abstract T GetDType<T>(Guid id, IAssetFilter filter = null) where T : DType;

    /// <summary>
    /// Gets all types of a specific type.
    /// </summary>
    public abstract IAssetCollection<DType> GetTypes<T>() where T : DType;

    /// <summary>
    /// Gets types by native type.
    /// </summary>
    public abstract IAssetCollection<DType> GetTypes(Type type);

    /// <summary>
    /// Gets non-native struct types.
    /// </summary>
    public abstract IAssetCollection<DType> GetNonNativeStructs();

    /// <summary>
    /// Gets structs by base type ID.
    /// </summary>
    public abstract IAssetCollection<DStruct> GetStructsByBaseType(Guid baseId);

    /// <summary>
    /// Gets structs by base type.
    /// </summary>
    public abstract IAssetCollection<DStruct> GetStructsByBaseType(DAbstract baseType);

    /// <summary>
    /// Gets structs by base type asset key.
    /// </summary>
    public abstract IAssetCollection<DStruct> GetStructsByBaseType(string assetKey);

    /// <summary>
    /// Gets functions by return type.
    /// </summary>
    public abstract IAssetCollection<DFunction> GetFunctionsByReturnType(TypeDefinition typeDef);

    /// <summary>
    /// Gets the native DType for a type.
    /// </summary>
    public abstract DType GetNativeDType(Type type);

    /// <summary>
    /// Gets the native object type for a type.
    /// </summary>
    public abstract DCompond GetNativeObjectType(Type type);

    /// <summary>
    /// Creates a type design.
    /// </summary>
    public abstract ITypeDesign CreateTypeDesign(object owner, bool optional = false);

    /// <summary>
    /// Creates a type design selection.
    /// </summary>
    public abstract ITypeDesignSelection CreateTypeDesignSelection();

    /// <summary>
    /// Creates a data link type design selection.
    /// </summary>
    public abstract ITypeDesignSelection CreateDataLinkTypeDesignSelection();

    /// <summary>
    /// Resolves a field by asset field key.
    /// </summary>
    public abstract DField ResolveField(string assetFieldKey);

    /// <summary>
    /// Adds a type to the type manager.
    /// </summary>
    internal abstract IRegistryHandle<DType> AddType(DType dType);

    /// <summary>
    /// Adds a struct to base type tracking.
    /// </summary>
    internal abstract IRegistryHandle<DStruct> AddToBaseType(DStruct type);

    /// <summary>
    /// Adds an abstract type to base type tracking.
    /// </summary>
    internal abstract IRegistryHandle<DAbstract> AddToBaseType(DAbstract abstractType);

    /// <summary>
    /// Adds a function to return type tracking.
    /// </summary>
    internal abstract IRegistryHandle<DFunction> AddToReturnType(DFunction func);
}