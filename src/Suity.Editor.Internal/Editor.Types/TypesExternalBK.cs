using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Backend implementation for external type operations, providing type resolution, creation, and naming services.
/// </summary>
internal class TypesExternalBK : TypesExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="TypesExternalBK"/>.
    /// </summary>
    public static TypesExternalBK Instance { get; } = new TypesExternalBK();

    private TypesExternalBK()
    { }

    private readonly EmptyTypeDefinition _empty = new();
    private readonly UnknownTypeDefinition _unknown = new();

    /// <summary>
    /// Dictionary caching resolved type definitions by their type code.
    /// </summary>
    internal readonly ConcurrentDictionary<string, TypeDefinition> _typeDic = new();

    /// <summary>
    /// Initializes the backend by setting it as the external type provider.
    /// </summary>
    internal void Intialize()
    {
        TypesExternal._external = this;
    }

    /// <inheritdoc/>
    public override TypeDefinition Empty => _empty;
    /// <inheritdoc/>
    public override TypeDefinition Unknown => _unknown;

    /// <inheritdoc/>
    public override TypeDefinition ReferenceSync(TypeDefinition definition, SyncPath path, IReferenceSync sync, Func<string> messageGetter = null)
    {
        bool matchRename = false;
        definition.VisitReferenceSync(path, sync, ref matchRename, messageGetter);

        if (sync.Mode == ReferenceSyncMode.Redirect && matchRename)
        {
            string newTypeId = definition.TypeCode.Replace(sync.OldId.ToString(), sync.Id.ToString());
            definition = Resolve(newTypeId, true, false);
        }

        return definition;
    }

    #region Make

    /// <inheritdoc/>
    public override TypeDefinition MakeDefinition(DType type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.Id == Guid.Empty)
        {
            throw new ArgumentException("DType id is empty.", nameof(type));
        }

        if (type is DAssetLink assetLink)
        {
            string typeId = "&" + assetLink.Id.ToString();

            return Resolve(typeId, true, false);
        }
        else
        {
            string typeId = type.Id.ToString();

            return GetOrAddType(typeId, () => new TypeRefDefinition(type, true));
        }
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeDataLinkType(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        // Native types, array types, data link types, enum types do not need to be data link types
        if (type.IsPrimitive || type.IsArray || type.IsDataLink || type.IsEnum)
        {
            // Note: When TypeDefinition is initially created, it is impossible to determine the Relationship, so it cannot be distinguished before loading.
            return type;
        }

        if (type.ElementType != null)
        {
            throw new ArgumentException("TypeDefinition element type must be empty.");
        }

        string typeId = $"@{type.TypeCode}";

        return GetOrAddType(typeId, () => new DataLinkTypeDefinition(type));
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeAssetLinkType(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        // Native types, array types, data link types, enum types do not need to be data link types
        if (type.IsPrimitive || type.IsArray || type.IsDataLink || type.IsEnum)
        {
            // Note: When TypeDefinition is initially created, it is impossible to determine Relationship, so it cannot be distinguished before loading.
            return type;
        }

        if (type.ElementType != null)
        {
            throw new ArgumentException("TypeDefinition element type must be empty.");
        }

        string typeId = $"&{type.TypeCode}";

        return GetOrAddType(typeId, () => new AssetLinkTypeDefinition(type));
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeArrayType(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        if (type.IsArray)
        {
            return type;
        }

        string typeId = ArrayTypeDefinition.MakeName(type, type.TypeCode);

        return GetOrAddType(typeId, () => new ArrayTypeDefinition(type));
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeAbstractFunctionType(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        if (type.IsAbstractFunction)
        {
            return type;
        }

        if (type.IsFunction)
        {
            return type.BaseAbstractType ?? TypeDefinition.Empty;
        }
        //if (IsKeyLink || IsAssetLink)
        //{
        //    // Force conversion to string
        //    return NativeTypes.StringType.MakeAbstractFunctionType();
        //}

        string typeId;

        typeId = $"#{type.TypeCode}";
        return GetOrAddType(typeId, () => new AbstractFunctionTypeDefinition(type));
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeAbstractFunctionArrayType(TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        if (type.IsArray && type.ElementType.IsAbstractFunction)
        {
            return type;
        }

        if (type.IsFunction)
        {
            return type.BaseAbstractType?.MakeArrayType() ?? TypeDefinition.Empty.MakeArrayType();
        }

        //if (IsKeyLink || IsAssetLink)
        //{
        //     Force conversion to string
        //    return NativeTypes.StringType.MakeAbstractFunctionType().MakeArrayType();
        //}

        if (type.IsArray)
        {
            return type.ElementType.MakeAbstractFunctionType().MakeArrayType();
        }
        else
        {
            return type.MakeAbstractFunctionType().MakeArrayType();
        }

        //string typeId = $"([])#{TypeId}";

        return type.MakeAbstractFunctionType().MakeArrayType();
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeGenericType(TypeDefinition type, params TypeDefinition[] parameters)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        if (type.ElementType != null)
        {
            throw new ArgumentException("TypeDefinition element type must be empty.");
        }

        if (parameters.Length == 0)
        {
            return type;
        }

        string typeId = $"{type.TypeCode}<{string.Join(",", parameters.Select(o => o.TypeCode))}>";

        return GetOrAddType(typeId, () => new GenericTypeRefDefinition(type, parameters));
    }

    /// <inheritdoc/>
    public override TypeDefinition MakeGenericType(TypeDefinition type, IEnumerable<TypeDefinition> parameters)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return Empty;
        }

        if (type.ElementType != null)
        {
            throw new ArgumentException("TypeDefinition element type must be empty.");
        }

        if (!parameters.Any())
        {
            return type;
        }

        string typeId = $"{type.TypeCode}<{string.Join(",", parameters.Select(o => o.TypeCode))}>";

        return GetOrAddType(typeId, () => new GenericTypeRefDefinition(type, parameters));
    }

    #endregion

    #region Resolve

    /// <inheritdoc/>
    public override TypeDefinition Resolve(string name, bool resolveExportedName, bool resolveResource)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Empty;
        }

        return GetOrAddType(name, () => Create(name, null, resolveExportedName, resolveResource));
    }

    /// <inheritdoc/>
    public override TypeDefinition Resolve(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Empty;
        }

        string typeId = id.ToString();
        return GetOrAddType(typeId, () => new TypeRefDefinition(id, true));
    }

    internal TypeDefinition Resolve(string name, IObjectIdResolver resolver, bool resolveExportedName = true, bool resolveResource = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Empty;
        }

        return GetOrAddType(name, () => Create(name, resolver, resolveExportedName));
    }

    private TypeDefinition Create(string name, IObjectIdResolver resolver, bool resolveExportedName = true, bool resolveResource = true)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Empty;
        }

        string innerTypeName;
        TypeDefinition innerType;

        if (name.StartsWith("([])#")) // Function array
        {
            innerTypeName = name[4..];
            innerType = Resolve(innerTypeName, resolver, resolveExportedName);
            return innerType?.MakeArrayType();
        }
        else if (name.StartsWith("#")) // Function
        {
            innerTypeName = name[1..];
            innerType = Resolve(innerTypeName, resolver, resolveExportedName);
            return innerType?.MakeAbstractFunctionType();
        }
        else if (name.EndsWith("[]")) // Array
        {
            innerTypeName = name[..^2];
            innerType = Resolve(innerTypeName, resolver, resolveExportedName);
            return innerType?.MakeArrayType();
        }
        else if (name.StartsWith("?")) // Resource link
        {
            // Old version method is no longer used
            innerTypeName = name[1..];
            return Resolve(innerTypeName, resolver, resolveExportedName);
        }
        else if (name.StartsWith("%")) // Resource link
        {
            // Old version method is no longer used
            innerTypeName = name[1..];
            return Resolve(innerTypeName, resolver, resolveExportedName);
        }
        else if (name.StartsWith("@")) // Key link
        {
            innerTypeName = name[1..];
            innerType = Resolve(innerTypeName, resolver, resolveExportedName);
            return innerType?.MakeDataLinkType();
        }
        else if (name.StartsWith("&")) // Resource link
        {
            innerTypeName = name[1..];
            innerType = Resolve(innerTypeName, resolver, resolveExportedName);
            return innerType?.MakeAssetLinkType();
        }
        else if (name.EndsWith(">")) // Generic
        {
            if (TryParseGenericType(name, resolver, resolveExportedName, out innerTypeName, out TypeDefinition[] genericParameters))
            {
                innerType = Resolve(innerTypeName, resolver, resolveExportedName);
                return new GenericTypeRefDefinition(innerType, genericParameters);
            }
            else
            {
                return Empty;
            }
        }
        else
        {
            string fullName = NativeTypes.GetFullName(name);
            
            bool isImmutable;

            TypeDefinition nativeType = NativeTypeExternal._external.GetBuildInTypeDefinition(fullName);
            if (nativeType != null)
            {
                return nativeType;
            }

            if (Guid.TryParseExact(name, "D", out Guid id))
            {
                // Resolving by Id, type is always immutable
                isImmutable = true;
            }
            else
            {
                // Resolving by name, type is mutable
                isImmutable = false;

                // Can automatically resolve standard keys
                if (resolveExportedName)
                {
                    // Try to resolve exported name
                    string exportedName = SyncExportExtensions.GetExportedName(name);
                    if (!string.IsNullOrWhiteSpace(exportedName) && exportedName != name)
                    {
                        name = exportedName;
                    }
                }

                Guid? idGet = resolver?.Resolve(name, true);

                if (idGet is null && resolveExportedName)
                {
                    idGet = AssetManager.Instance.GetAsset<DType>(name)?.Id;

                    // Note: Global Id resolver will automatically create a new Id when resolution fails
                    idGet ??= GlobalIdResolver.Resolve(name);
                }

                // Note: If resolveExportedName is set to true, the following statement will never execute
                // because idGet is already not null
                if (idGet is null && resolveResource)
                {
                    // Try to resolve resource name, resource name is mutable, set isImmutable to false
                    idGet ??= AssetManager.Instance.GetAssetByResourceName<DType>(name)?.Id;
                }

                id = idGet ?? Guid.Empty;
            }

            if (id != Guid.Empty)
            {
                if (!isImmutable)
                {
                    var type = AssetManager.Instance.GetAsset<DType>(id);
                    if (type?.IsNative == true)
                    {
                        // If type is native type, then type is immutable
                        isImmutable = true;
                    }
                }

                return new TypeRefDefinition(id, isImmutable);
            }
            else
            {
                return Empty;
            }
        }
    }

    private bool TryParseGenericType(string name, IObjectIdResolver resolver, bool resolveExportedName, out string typeName, out TypeDefinition[] parameters)
    {
        typeName = name;
        parameters = [];

        int startIndex = name.IndexOf('<');
        if (startIndex <= 0)
        {
            return false;
        }

        typeName = name.Substring(0, startIndex);
        List<string> pList = [];

        int indent = 1;
        int paramStartIndex = startIndex + 1;

        for (int i = startIndex + 1; i < name.Length; i++)
        {
            char c = name[i];
            switch (c)
            {
                case '<':
                    indent++;
                    break;

                case '>':
                    indent--;
                    if (indent == 0)
                    {
                        if (i != name.Length - 1)
                        {
                            return false;
                        }
                        string parameter = name.Substring(paramStartIndex, i - paramStartIndex).Trim();
                        pList.Add(parameter);
                        break;
                    }
                    break;

                case ',':
                    if (indent == 1)
                    {
                        string parameter = name.Substring(paramStartIndex, i - paramStartIndex).Trim();
                        pList.Add(parameter);
                        paramStartIndex = i + 1;
                    }
                    break;

                default:
                    break;
            }
        }

        if (indent != 0)
        {
            return false;
        }

        parameters = pList.Select(s => Resolve(s, resolver, resolveExportedName)).ToArray();

        return true;
    }

    /// <inheritdoc/>
    public override string GetPrefix(TypeRelationships relationship) => relationship switch
    {
        TypeRelationships.DataLink => "@",
        TypeRelationships.AssetLink => "&",
        TypeRelationships.AbstractFunction => "#",
        _ => string.Empty,
    };

    /// <inheritdoc/>
    public override void SplitPrefix(string name, out string prefix, out string originName)
    {
        if (string.IsNullOrEmpty(name))
        {
            prefix = string.Empty;
            originName = string.Empty;
            return;
        }

        char c = name[0];

        switch (c)
        {
            case '#':
            case '@':
            case '%':
            case '?':
            case '~':
            case '&':
                prefix = c.ToString();
                originName = name.Substring(1, name.Length - 1);
                break;

            default:
                prefix = string.Empty;
                originName = name;
                break;
        }
    }

    /// <inheritdoc/>
    public override TypeDefinition ResolveExportedDefinition(string key, out string prefix, out string originKey)
    {
        if (string.IsNullOrEmpty(key))
        {
            prefix = string.Empty;
            originKey = string.Empty;

            return TypeDefinition.Empty;
        }

        SplitPrefix(key, out prefix, out originKey);

        originKey = NativeTypes.GetFullName(originKey);
        switch (prefix)
        {
            case "?":
            case "%":
                prefix = string.Empty;
                break;

            default:
                break;
        }

        if (AssetManager.Instance.GetAsset(originKey) != null)
        {
            return Resolve(prefix + originKey, true, false);
        }

        if (originKey.StartsWith("*"))
        {
            return Resolve(prefix + originKey, true, false);
        }

        var keyCode = new KeyCode(originKey);
        string mainKey = keyCode.MainKey.GetPathId();
        originKey = KeyCode.Combine(mainKey, keyCode.ElementKey);

        return Resolve(prefix + originKey, true, false);
    }

    /// <inheritdoc/>
    public override string ResolveNativeFieldName(DType type, Guid id)
    {
        return NativeFieldResolver.Instance.ResolveFieldName(type, id);
    }

    /// <inheritdoc/>
    public override Guid ResolveNativeFieldId(DType type, string fieldName)
    {
        return NativeFieldResolver.Instance.ResolveFieldId(type, fieldName);
    }

    private TypeDefinition GetOrAddType(string name, Func<TypeDefinition> creator)
    {
        var type = _typeDic.GetValueSafe(name);
        if (!TypeDefinition.IsNullOrEmpty(type))
        {
            return type;
        }

        type = creator();

        // Never add empty types to dictionary.
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return TypeDefinition.Empty;
        }

        // Because types that fail to resolve may resolve successfully later, they need to be re-resolved and cannot be cached.
        // Types that resolve successfully will not be resolved again later, so they can be cached.
        // Only immutable types can be cached because immutable types do not change.
        if (type.IsImmutable)
        {
            _typeDic[name] = type;
        }

        return type;
    }

    #endregion

    #region Naming

    /// <inheritdoc/>
    public override string GetShortTypeName(TypeDefinition typeInfo, bool alias = false)
    {
        var type = typeInfo.Target;
        if (type != null)
        {
            if (type.IsNative)
            {
                if (alias)
                {
                    return NativeTypes.GetNativeTypeAlias(type.LocalName);
                }
                else
                {
                    return NativeTypes.GetNativeTypeShortName(type.LocalName);
                }
            }
            else
            {
                return (type.ShortTypeName ?? string.Empty);
            }
        }
        else
        {
            Asset asset = AssetManager.Instance.GetAsset(typeInfo.TargetId);
            if (asset != null)
            {
                return asset.ShortTypeName;
            }
            else
            {
                string missing = (GlobalIdResolver.RevertResolve(typeInfo.TargetId) ?? string.Empty).Trim('.', '*', ':');

                return $"!!!MISSING_SHORT_TYPE|{missing}";
            }
        }
    }

    /// <inheritdoc/>
    public override bool TryGetShortTypeName(TypeDefinition typeInfo, bool alias, out string shortTypeName)
    {
        var type = typeInfo.Target;
        if (type != null)
        {
            if (type.IsNative)
            {
                if (alias)
                {
                    shortTypeName = NativeTypes.GetNativeTypeAlias(type.LocalName);
                    return true;
                }
                else
                {
                    shortTypeName = NativeTypes.GetNativeTypeShortName(type.LocalName);
                    return true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(type.ShortTypeName))
                {
                    shortTypeName = type.ShortTypeName;
                    return true;
                }
                else
                {
                    shortTypeName = null;
                    return false;
                }
            }
        }
        else
        {
            Asset asset = AssetManager.Instance.GetAsset(typeInfo.TargetId);
            if (asset != null)
            {
                shortTypeName = asset.ShortTypeName;
                return true;
            }
            else
            {
                shortTypeName = null;
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public override string GetFullTypeNameText(TypeDefinition typeInfo, bool alias = false)
    {
        string name = GetFullTypeName(typeInfo, alias);

        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (typeInfo.TargetId != null)
        {
            return typeInfo.TargetId.ToString();
        }

        return "???";
    }

    /// <inheritdoc/>
    public override string GetFullTypeName(TypeDefinition typeInfo, bool alias = false)
    {
        string name;

        var type = typeInfo.Target;
        if (type != null)
        {
            if (type.IsNative)
            {
                if (alias)
                {
                    return NativeTypes.GetNativeTypeAlias(type.LocalName);
                }
                else
                {
                    return NativeTypes.GetNativeTypeShortName(type.LocalName);
                }
            }
            else
            {
                name = type.FullTypeName;
            }
        }
        else
        {
            Asset asset = AssetManager.Instance.GetAsset(typeInfo.TargetId);
            if (asset != null)
            {
                name = asset.FullTypeName;
            }
            else
            {
                // return AssetExtensions.CombineName(baseNameSpace, GlobalIdResolver.RevertResolve(typeInfo.TargetId) ?? "???");
                name = GlobalIdResolver.RevertResolve(typeInfo.TargetId);
            }
        }

        if (name != null)
        {
            return name.Replace('/', '.').Trim('.', ':', '*');
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override ISelectionList GetImplementations(TypeDefinition type, IAssetFilter filter = null)
    {
        if (type is null)
        {
            return EmptyBaseAssetCollection.Empty;
        }

        filter ??= AssetFilters.Default;

        switch (type.Relationship)
        {
            case TypeRelationships.AbstractFunction:
                return DTypeManager.Instance.GetFunctionsByReturnType(type)?.WithFilter(filter)
                    ?? EmptySelectionList.Empty;

            case TypeRelationships.AbstractStruct:
                return DTypeManager.Instance.GetStructsByBaseType(type.Target as DAbstract)?.WithFilter(filter)
                    ?? EmptySelectionList.Empty;

            default:
                if (type == NativeTypes.ObjectType)
                {
                    return DTypeManager.Instance.GetNonNativeStructs().WithFilter(filter);
                }
                else
                {
                    return EmptySelectionList.Empty;
                }
        }
    }

    /// <inheritdoc/>
    public override ImageDef GetIcon(TypeDefinition typeInfo)
    {
        if (typeInfo.IsNative)
        {
            return CoreIconCache.System;
        }

        switch (typeInfo.Relationship)
        {
            case TypeRelationships.None:
                break;

            case TypeRelationships.Value:
                break;

            case TypeRelationships.Struct:
                break;

            case TypeRelationships.Array:
                return CoreIconCache.Array;

            case TypeRelationships.Enum:
                break;

            case TypeRelationships.DataLink:
                return CoreIconCache.Link;

            case TypeRelationships.AssetLink:
                return CoreIconCache.Link;

            case TypeRelationships.Delegate:
                return CoreIconCache.Delegate;

            case TypeRelationships.AbstractFunction:
                break;

            case TypeRelationships.AbstractStruct:
                break;

            default:
                break;
        }

        DType dType = typeInfo.Target;

        return dType?.Icon;
    }

    #endregion
}