using Suity.Collections;
using System;
using System.Collections.Concurrent;

namespace Suity.Editor.Types;

/// <summary>
/// Backend implementation for managing DType assets, including type registration, querying, and function collections.
/// </summary>
internal class DTypeManagerBK : DTypeManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="DTypeManagerBK"/>.
    /// </summary>
    public new static readonly DTypeManagerBK Instance = new();

    private readonly ConcurrentDictionary<Type, AssetCollection<DType>> _allTypes = new();
    private readonly ConcurrentDictionary<Guid, AssetCollection<DStruct>> _structsByBaseType = new();
    private readonly ConcurrentDictionary<Guid, AssetCollection<DAbstract>> _sidesByBaseType = new();

    private readonly AssetCollection<DType> _nonNativeStructs = new();

    internal readonly AssetCollection<DFunction> _generalObjFuncs = new();
    internal readonly AssetCollection<DFunction> _generalAryFuncs = new();
    private readonly AssetCollection<DFunction> _allFuncs = new();
    private readonly DFunctionCollection _numericFuncs;
    private readonly DFunctionCollection _emptyObjectFuncs;
    private readonly DFunctionCollection _emptyArrayFuncs;
    private readonly ConcurrentDictionary<TypeDefinition, DFunctionCollection> _funcsByReturnType = new();

    private readonly MultipleItemCollection<Type, DType> _nativeTypes = new();

    private DTypeManagerBK()
    {
        _numericFuncs = new DFunctionCollection(_generalObjFuncs);
        _emptyObjectFuncs = new DFunctionCollection(_generalObjFuncs);
        _emptyArrayFuncs = new DFunctionCollection(_generalAryFuncs);
    }

    /// <summary>
    /// Initializes the backend by setting it as the type manager instance.
    /// </summary>
    internal void Initialize()
    {
        DTypeManager.Instance = this;
    }

    /// <inheritdoc/>
    public override T GetDType<T>(string assetKey, IAssetFilter filter = null)
    {
        return AssetManager.Instance.GetAsset<T>(assetKey, filter ?? AssetFilters.Default);
    }

    /// <inheritdoc/>
    public override T GetDType<T>(Guid id, IAssetFilter filter = null)
    {
        return AssetManager.Instance.GetAsset<T>(id, filter ?? AssetFilters.Default);
    }

    /// <inheritdoc/>
    public override IAssetCollection<DType> GetTypes<T>()
    {
        return _allTypes.GetValueSafe(typeof(T)) as IAssetCollection<DType> ?? EmptyAssetCollection<DType>.Empty;
    }

    /// <inheritdoc/>
    public override IAssetCollection<DType> GetTypes(Type type)
    {
        return _allTypes.GetValueSafe(type) as IAssetCollection<DType> ?? EmptyAssetCollection<DType>.Empty;
    }

    /// <inheritdoc/>
    public override IAssetCollection<DType> GetNonNativeStructs()
    {
        return _nonNativeStructs;
    }

    /// <inheritdoc/>
    public override IAssetCollection<DStruct> GetStructsByBaseType(Guid baseTypeId)
    {
        return _structsByBaseType.GetValueSafe(baseTypeId) as IAssetCollection<DStruct> ?? EmptyAssetCollection<DStruct>.Empty;
    }

    /// <inheritdoc/>
    public override IAssetCollection<DStruct> GetStructsByBaseType(DAbstract baseType)
    {
        if (baseType is null)
        {
            return EmptyAssetCollection<DStruct>.Empty;
        }

        return _structsByBaseType.GetValueSafe(baseType.Id) as IAssetCollection<DStruct> ?? EmptyAssetCollection<DStruct>.Empty;
    }

    /// <inheritdoc/>
    public override IAssetCollection<DStruct> GetStructsByBaseType(string assetKey)
    {
        if (string.IsNullOrEmpty(assetKey))
        {
            return EmptyAssetCollection<DStruct>.Empty;
        }

        if (AssetManager.Instance.GetAsset(assetKey) is DAbstract baseType)
        {
            return _structsByBaseType.GetValueSafe(baseType.Id) as IAssetCollection<DStruct> ?? EmptyAssetCollection<DStruct>.Empty;
        }
        else
        {
            Guid id = GlobalIdResolver.Resolve(assetKey);

            return _structsByBaseType.GetValueSafe(id) as IAssetCollection<DStruct> ?? EmptyAssetCollection<DStruct>.Empty;
            //return null;
        }
    }

    /// <inheritdoc/>
    public override IAssetCollection<DFunction> GetFunctionsByReturnType(TypeDefinition typeDef)
    {
        if (typeDef is null)
        {
            return EmptyAssetCollection<DFunction>.Empty;
        }

        if (typeDef.IsAbstractFunction)
        {
            if (TypeDefinition.IsNullOrEmpty(typeDef.ElementType) || typeDef.ElementType == NativeTypes.ObjectType)
            {
                return _allFuncs;
            }
            else if (typeDef.ElementType.IsAbstractNumeric)
            {
                return _numericFuncs;
            }
            else
            {
                if (typeDef.ElementType.IsArray)
                {
                    return _funcsByReturnType.GetValueSafe(typeDef.ElementType) as IAssetCollection<DFunction> ?? _emptyArrayFuncs;
                }
                else
                {
                    return _funcsByReturnType.GetValueSafe(typeDef.ElementType) as IAssetCollection<DFunction> ?? _emptyObjectFuncs;
                }
            }
        }
        else
        {
            if (typeDef.IsArray)
            {
                return _funcsByReturnType.GetValueSafe(typeDef) as IAssetCollection<DFunction> ?? _emptyArrayFuncs;
            }
            else
            {
                return _funcsByReturnType.GetValueSafe(typeDef) as IAssetCollection<DFunction> ?? _emptyObjectFuncs;
            }
        }
    }

    /// <inheritdoc/>
    public override DType GetNativeDType(Type type)
    {
        return _nativeTypes.GetValue(type);
    }

    /// <inheritdoc/>
    public override DCompond GetNativeObjectType(Type type)
    {
        return _nativeTypes.GetValue(type) as DCompond;
    }

    /// <inheritdoc/>
    public override ITypeDesign CreateTypeDesign(object owner, bool optional) => new FieldTypeDesign(owner, optional);

    /// <inheritdoc/>
    public override ITypeDesignSelection CreateTypeDesignSelection() => new TypeDesignSelection();

    /// <inheritdoc/>
    public override ITypeDesignSelection CreateDataLinkTypeDesignSelection() => new DataLinkTypeDesignSelection();

    /// <inheritdoc/>
    public override DField ResolveField(string assetFieldKey)
    {
        var fieldCode = FieldCode.ParseFullFieldCode(assetFieldKey);
        var type = GetDType<DType>(fieldCode.MainName);

        return type?.GetField(fieldCode.FieldName);
    }

    internal override IRegistryHandle<DType> AddType(DType dType)
    {
        if (dType is null)
        {
            return null;
        }

        if (dType.Id == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrEmpty(dType.AssetKey))
        {
            return null;
        }

        var entryChain = new EntryChainRegHandle<DType>(dType);

        Type current = dType.GetType();
        while (current != null && typeof(DType).IsAssignableFrom(current))
        {
            var collection = _allTypes.GetOrAdd(current, t => new AssetCollection<DType>());
            var entry = collection.AddAsset(dType.AssetKey, dType);
            if (entry != null)
            {
                entryChain.AddEntry(entry);
            }
            current = current.BaseType;
        }

        if (dType is INativeType nativeType && nativeType.NativeType is Type nt)
        {
            var nativeEntry = _nativeTypes.AddValue(nt, dType);
            if (nativeEntry != null)
            {
                entryChain.AddEntry(nativeEntry);
            }
        }

        if (dType is DStruct s && !s.IsNative)
        {
            var nonNativeEntry = _nonNativeStructs.AddAsset(dType.AssetKey, s);
            if (nonNativeEntry != null)
            {
                entryChain.AddEntry(nonNativeEntry);
            }
        }

        return entryChain;
    }

    internal override IRegistryHandle<DStruct> AddToBaseType(DStruct type)
    {
        if (type is null)
        {
            return null;
        }

        if (type.Id == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrEmpty(type.AssetKey))
        {
            return null;
        }

        Guid id = type.BaseTypeId;
        if (id != Guid.Empty)
        {
            var collection = _structsByBaseType.GetOrAdd(id, _ => new AssetCollection<DStruct>());
            var item = collection.AddAsset(type.AssetKey, type);
            if (item != null)
            {
                return new MultipleItemRegHandle<DStruct>(item, type);
            }
        }

        return null;
    }

    internal override IRegistryHandle<DAbstract> AddToBaseType(DAbstract abstractType)
    {
        if (abstractType is null)
        {
            return null;
        }

        if (abstractType.Id == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrEmpty(abstractType.AssetKey))
        {
            return null;
        }

        Guid id = abstractType.BaseTypeId;
        if (id != Guid.Empty)
        {
            var collection = _sidesByBaseType.GetOrAdd(id, _ => new AssetCollection<DAbstract>());
            var item = collection.AddAsset(abstractType.AssetKey, abstractType);
            if (item != null)
            {
                return new MultipleItemRegHandle<DAbstract>(item, abstractType);
            }
        }

        return null;
    }

    internal override IRegistryHandle<DFunction> AddToReturnType(DFunction func)
    {
        if (func is null)
        {
            return null;
        }

        if (func.Id == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrEmpty(func.AssetKey))
        {
            return null;
        }

        var entryChain = new EntryChainRegHandle<DFunction>(func);

        var all = _allFuncs.AddAsset(func.AssetKey, func);
        if (all != null)
        {
            entryChain.AddEntry(all);
        }

        TypeDefinition returnType = func.ReturnType;

        if (func.ReturnTypeBinding != DReturnTypeBinding.None)
        {
            if ((func.ReturnTypeBinding & DReturnTypeBinding.Object) == DReturnTypeBinding.Object)
            {
                var entry = _generalObjFuncs.AddAsset(func.AssetKey, func);
                if (func != null)
                {
                    entryChain.AddEntry(entry);
                }
            }
            if ((func.ReturnTypeBinding & DReturnTypeBinding.Array) == DReturnTypeBinding.Array)
            {
                var entry = _generalAryFuncs.AddAsset(func.AssetKey, func);
                if (func != null)
                {
                    entryChain.AddEntry(entry);
                }
            }
        }
        else if (returnType != null)
        {
            DFunctionCollection collection;
            if (returnType.IsArray || returnType.IsAbstractArray)
            {
                collection = _funcsByReturnType.GetOrAdd(returnType, _ => new DFunctionCollection(_generalAryFuncs));
            }
            else
            {
                collection = _funcsByReturnType.GetOrAdd(returnType, _ => new DFunctionCollection(_generalObjFuncs));
            }

            var entry = collection.AddAsset(func.AssetKey, func);
            if (entry != null)
            {
                entryChain.AddEntry(entry);
            }

            if (returnType.IsNumeric)
            {
                var numericEntry = _numericFuncs.AddAsset(func.AssetKey, func);
                if (numericEntry != null)
                {
                    entryChain.AddEntry(numericEntry);
                }
            }
        }

        return entryChain;
    }
}