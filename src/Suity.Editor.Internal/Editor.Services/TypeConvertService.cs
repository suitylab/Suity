using Suity.Collections;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Represents a pair of source and target types for type conversion lookups.
/// </summary>
public record TwoType(Type TypeFrom, Type TypeTo);

/// <summary>
/// Represents a pair of source and target type definitions for type conversion lookups.
/// </summary>
public record TwoTypeDef(TypeDefinition TypeFrom, TypeDefinition TypeTo);

/// <summary>
/// Service that manages type conversions between different types and type definitions.
/// </summary>
internal class TypeConvertService : ITypeConvertService
{
    /// <summary>
    /// Singleton instance of the type convert service.
    /// </summary>
    public static TypeConvertService Instance { get; } = new();

    readonly Dictionary<TwoType, ITypeConverter> _typeConverters = [];
    readonly Dictionary<TwoTypeDef, ITypeDefinitionConverter> _typeDefConverters = [];
    readonly Dictionary<Type, IDStructObjectConverter> _dstructConverters = [];

    /// <summary>
    /// Gets the type definition for SAssetKey.
    /// </summary>
    public TypeDefinition SAssetKeyType { get; } = TypeDefinition.FromNative<SAssetKey>();

    private TypeConvertService()
    {
        EditorUtility.EditorStart.AddListener(Initialize);
    }

    /// <summary>
    /// Initializes the service by scanning and registering all available type converters.
    /// </summary>
    private void Initialize()
    {
        foreach (var converter in DerivedTypeHelper.CreateDerivedClasses<ITypeConverter>())
        {
            var typesFrom = converter.TypesFrom ?? [];
            var typesTo = converter.TypesTo ?? [];

            foreach (var typeFrom in typesFrom.SkipNull())
            {
                foreach (var typeTo in typesTo.SkipNull())
                {
                    if (typeFrom == typeTo)
                    {
                        continue;
                    }

                    var twoType = new TwoType(typeFrom, typeTo);
                    if (_typeConverters.ContainsKey(twoType))
                    {
                        Logs.LogWarning($"Duplicate {nameof(ITypeConverter)}: {converter.GetType().FullName}, from={typeFrom.FullName}, to={typeTo.FullName}");
                        continue;
                    }

                    _typeConverters.Add(twoType, converter);
                }
            }
        }

        foreach (var converter in DerivedTypeHelper.CreateDerivedClasses<ITypeDefinitionConverter>())
        {
            var typesFrom = converter.TypesFrom ?? [];
            var typesTo = converter.TypesTo ?? [];

            foreach (var typeFrom in typesFrom.SkipNull())
            {
                foreach (var typeTo in typesTo.SkipNull())
                {
                    if (typeFrom == typeTo)
                    {
                        continue;
                    }

                    var twoType = new TwoTypeDef(typeFrom, typeTo);
                    if (_typeDefConverters.ContainsKey(twoType))
                    {
                        Logs.LogWarning($"Duplicate {nameof(ITypeDefinitionConverter)}: " + converter.GetType().FullName);
                    }

                    _typeDefConverters.Add(twoType, converter);
                }
            }
        }

        foreach (var converter in DerivedTypeHelper.CreateDerivedClasses<IDStructObjectConverter>())
        {
            var types = converter.Types ?? [];
            foreach (var type in types.SkipNull())
            {
                if (_dstructConverters.ContainsKey(type))
                {
                    Logs.LogWarning($"Duplicate {nameof(IDStructObjectConverter)}: " + converter.GetType().FullName);
                }

                _dstructConverters.Add(type, converter);
            }
        }
    }

    /// <inheritdoc/>
    public TypeConvertState CanConvert(Type typeFrom, Type typeTo)
    {
        if (typeFrom is null || typeTo is null)
        {
            return TypeConvertState.Unconvertible;
        }

        if (typeFrom == typeTo)
        {
            return TypeConvertState.Same;
        }

        if (typeTo.IsAssignableFrom(typeFrom))
        {
            return TypeConvertState.Assignable;
        }

        if (_typeConverters.ContainsKey(new TwoType(typeFrom, typeTo)))
        {
            return TypeConvertState.Convertible;
        }

        if (typeTo == typeof(string))
        {
            return TypeConvertState.Convertible;
        }

        if (typeTo == typeof(object))
        {
            return TypeConvertState.Convertible;
        }

        return TypeConvertState.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertState CanConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple)
    {
        if (TypeDefinition.IsNullOrEmpty(typeDefFrom) || TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            return TypeConvertState.Unconvertible;
        }

        if (typeDefFrom == typeDefTo)
        {
            return TypeConvertState.Same;
        }

        // Array to multi-connection port conversion
        if (typeDefFrom.IsArray && typeDefFrom.ElementType == typeDefTo && toMultiple)
        {
            return TypeConvertState.Same;
        }

        // AssetLink array to multi-connection port conversion
        /*        if (typeDefFrom.IsArray && typeDefFrom.ElementType.IsAssetLink  && toMultiple)
                {
                    var nativeType = typeDefFrom.ElementType.NativeType;
                    if (nativeType == typeDefTo.NativeType)
                    {
                        return TypeConvertState.Convertible;
                    }
                }*/

        if (typeDefTo.IsAssignableFrom(typeDefFrom))
        {
            return TypeConvertState.Assignable;
        }

        if (_typeDefConverters.ContainsKey(new TwoTypeDef(typeDefFrom, typeDefTo)))
        {
            // var converter = _typeDefConverters[new TwoTypeDef(typeDefFrom, typeDefTo)];

            return TypeConvertState.Convertible;
        }

        if (GetIsArrayConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertState.Convertible;
        }

        if (GetIsAssetLinkConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertState.Convertible;
        }

        if (GetIsDStructConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertState.Convertible;
        }

        if (typeDefFrom.IsArray && typeDefTo.IsArray)
        {
            return CanConvert(typeDefFrom.ElementType, typeDefTo.ElementType, false);
        }

        if (typeDefTo == NativeTypes.ObjectType)
        {
            return TypeConvertState.Convertible;
        }

        if (typeDefFrom.IsAssetLink && typeDefTo == SAssetKeyType)
        {
            return TypeConvertState.Convertible;
        }
        else if (typeDefFrom == SAssetKeyType && typeDefTo.IsAssetLink)
        {
            return TypeConvertState.Convertible;
        }

        if (typeDefFrom.ElementType is null && typeDefTo.ElementType is null)
        {
            return CanConvert(typeDefFrom.NativeType, typeDefTo.NativeType);
        }

        return TypeConvertState.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertState CanConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo)
    {
        if (connectorFrom is null || connectorTo is null)
        {
            return TypeConvertState.Unconvertible;
        }

        if (!string.IsNullOrWhiteSpace(connectorFrom.DataTypeName) 
            && connectorFrom.DataTypeName == connectorTo.DataTypeName)
        {
            return TypeConvertState.Same;
        }

        var typeDefFrom = TypeDefinition.Resolve(connectorFrom.DataTypeName);
        var typeDefTo = TypeDefinition.Resolve(connectorTo.DataTypeName);

        return CanConvert(typeDefFrom, typeDefTo, connectorTo.AllowMultipleConnection == true);
    }

    /// <inheritdoc/>
    public TypeConvertState TryConvert(Type typeFrom, Type typeTo, object objFrom, out object result)
    {
        result = null;
        if (objFrom is null)
        {
            return TypeConvertState.Null;
        }

        if (typeFrom is null || typeTo is null)
        {
            return TypeConvertState.Unconvertible;
        }

        //if (!typeFrom.IsAssignableFrom(objFrom.GetType()))
        //{
        //    return TypeConvertState.Unconvertible;
        //}

        if (typeFrom == typeTo)
        {
            result = objFrom;
            return TypeConvertState.Same;
        }

        if (typeTo.IsAssignableFrom(typeFrom))
        {
            result = objFrom;
            return TypeConvertState.Assignable;
        }

        var twoType = new TwoType(typeFrom, typeTo);
        if (_typeConverters.TryGetValue(twoType, out var converter))
        {
            try
            {
                result = converter.ConvertType(objFrom, typeTo);
                return TypeConvertState.Convertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (typeTo == typeof(string))
        {
            try
            {
                result = objFrom.ToString() ?? string.Empty;
                return TypeConvertState.Convertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (typeTo == typeof(object))
        {
            result = objFrom;
            return TypeConvertState.Convertible;
        }

        return TypeConvertState.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertState TryConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple, object objFrom, out object result)
    {
        result = null;
        if (objFrom is null)
        {
            return TypeConvertState.Null;
        }

        if (TypeDefinition.IsNullOrEmpty(typeDefFrom) || TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            return TypeConvertState.Unconvertible;
        }

        if (typeDefTo == typeDefFrom)
        {
            result = objFrom;
            return TypeConvertState.Same;
        }

        // Array to multi-connection port conversion
        if (typeDefFrom.IsArray && typeDefFrom.ElementType == typeDefTo && toMultiple)
        {
            result = objFrom;
            return TypeConvertState.Same;
        }

        // AssetLink array to multi-connection port
        /*        if (typeDefFrom.IsArray && typeDefFrom.ElementType.IsAssetLink && toMultiple)
                {
                    var nativeType = typeDefFrom.ElementType.NativeType;
                    if (nativeType == typeDefTo.NativeType)
                    {
                        result = objFrom;
                        return TypeConvertState.Convertible;
                    }
                }*/

        if (typeDefTo.IsAssignableFrom(typeDefFrom))
        {
            result = objFrom;
            return TypeConvertState.Assignable;
        }

        var twoType = new TwoTypeDef(typeDefFrom, typeDefTo);
        if (_typeDefConverters.TryGetValue(twoType, out var converter))
        {
            try
            {
                result = converter.ConvertType(objFrom, typeDefTo);
                return TypeConvertState.Convertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (GetIsArrayConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                return ConvertArray(typeDefFrom, typeDefTo, objFrom, out result)
                    ? TypeConvertState.Convertible : TypeConvertState.Unconvertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (GetIsAssetLinkConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                return ConvertAssetLink(typeDefFrom, typeDefTo, objFrom, out result)
                    ? TypeConvertState.Convertible : TypeConvertState.Unconvertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (GetIsDStructConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                return ConvertDStruct(typeDefFrom, typeDefTo, objFrom, out result)
                    ? TypeConvertState.Convertible : TypeConvertState.Unconvertible;
            }
            catch (Exception)
            {
                return TypeConvertState.Unconvertible;
            }
        }


        if (typeDefFrom.IsArray && typeDefTo.IsArray)
        {
            if (objFrom is not string && objFrom is System.Collections.IEnumerable ary)
            {
                var newSary = new SArray(typeDefTo);
                foreach (var item in ary)
                {
                    TryConvert(typeDefFrom.ElementType, typeDefTo.ElementType, false, item, out result);
                    newSary.Add(result);
                }

                result = newSary;
                return TypeConvertState.Convertible;
            }
            else
            {
                return TypeConvertState.Unconvertible;
            }
        }

        if (typeDefTo == NativeTypes.ObjectType)
        {
            result = objFrom;
            return TypeConvertState.Convertible;
        }

        if (typeDefFrom.IsAssetLink && typeDefTo == SAssetKeyType)
        {
            result = Cloner.Clone(objFrom as SAssetKey);
            return TypeConvertState.Convertible;
        }
        else if (typeDefFrom == SAssetKeyType && typeDefTo.IsAssetLink)
        {
            result = new SAssetKey(typeDefTo, (result as SAssetKey)?.Id ?? Guid.Empty);
            return TypeConvertState.Convertible;
        }

        if (typeDefFrom.ElementType is null && typeDefTo.ElementType is null)
        {
            return TryConvert(typeDefFrom.NativeType, typeDefTo.NativeType, objFrom, out result);
        }

        result = null;
        return TypeConvertState.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertState TryConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo, object objFrom, out object result)
    {
        result = null;
        if (objFrom is null)
        {
            return TypeConvertState.Null;
        }

        if (connectorFrom is null || connectorTo is null)
        {
            return TypeConvertState.Unconvertible;
        }

        if (!string.IsNullOrWhiteSpace(connectorFrom.DataTypeName)
            && connectorFrom.DataTypeName == connectorTo.DataTypeName)
        {
            result = objFrom;
            return TypeConvertState.Same;
        }

        var typeDefFrom = TypeDefinition.Resolve(connectorFrom?.DataTypeName);
        var typeDefTo = TypeDefinition.Resolve(connectorTo?.DataTypeName);

        return TryConvert(typeDefFrom, typeDefTo, connectorTo.AllowMultipleConnection == true, objFrom, out result);
    }


    /// <inheritdoc/>
    public TypeConvertState TryConvert(FlowNodeConnector connectorTo, object objFrom, out object result)
    {
        if (connectorTo is null || objFrom is null)
        {
            result = null;
            return TypeConvertState.Null;
        }

        var typeDefTo = TypeDefinition.Resolve(connectorTo.DataTypeName);
        if (TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            result = null;
            return TypeConvertState.Null;
        }

        return TryConvert(typeDefTo, connectorTo.AllowMultipleConnection == true, objFrom, out result);
    }

    /// <inheritdoc/>
    public TypeConvertState TryConvert(TypeDefinition typeDefTo, bool toMultiple, object objFrom, out object result)
    {
        if (TypeDefinition.IsNullOrEmpty(typeDefTo) || objFrom is null)
        {
            result = null;
            return TypeConvertState.Null;
        }

        if (typeDefTo.ElementType is null && typeDefTo.NativeType is { } typeTo && typeTo.IsAssignableFrom(objFrom.GetType()))
        {
            result = objFrom;
            return TypeConvertState.Assignable;
        }

        TypeDefinition typeDefFrom;
        if (objFrom is SItem sItem)
        {
            typeDefFrom = sItem.InputType;
        }
        else
        {
            typeDefFrom = TypeDefinition.FromNative(objFrom.GetType());
        }

        if (TypeDefinition.IsNullOrEmpty(typeDefFrom))
        {
            result = null;
            return TypeConvertState.Null;
        }

        return TryConvert(typeDefFrom, typeDefTo, toMultiple, objFrom, out result);
    }

    #region Array -> Single / Single -> Array Conversion

    /// <summary>
    /// Determines if conversion between array and single element types is possible.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <returns>True if array conversion is possible.</returns>
    private bool GetIsArrayConvertible(TypeDefinition typeDefFrom, TypeDefinition typeDefTo)
    {
        if (typeDefFrom.IsArray && !typeDefTo.IsArray && typeDefFrom.ElementType == typeDefTo)
        {
            return true;
        }

        if (typeDefTo.IsArray && !typeDefFrom.IsArray && typeDefTo.ElementType == typeDefFrom)
        {
            return true; 
        }

        return false;
    }

    /// <summary>
    /// Performs conversion between array and single element types.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result.</param>
    /// <returns>True if conversion succeeded.</returns>
    private bool ConvertArray(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, object objFrom, out object result)
    {
        result = null;

        if (typeDefTo.IsArray)
        {
            if (objFrom is SArray sary)
            {
                result = sary;
                return true;
            }
            else if (objFrom is Array ary)
            {
                result = ary;
                return true;
            }
            else
            {
                var sary2 = new SArray(typeDefTo);
                sary2.Add(objFrom);

                result = sary2;
                return true;
            }
        }
        else
        {
            if (objFrom is SArray sary)
            {
                result = sary.GetIListItemSafe(0);
                return true;
            }
            else if (objFrom is Array ary && ary.Length > 0)
            {
                result = ary.GetValue(0);
                return true;
            }
            else
            {
                result = objFrom;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region DStruct Conversion

    /// <summary>
    /// Determines if DStruct conversion is possible between the given types.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <returns>True if DStruct conversion is possible.</returns>
    private bool GetIsDStructConvertible(TypeDefinition typeDefFrom, TypeDefinition typeDefTo)
    {
        if (typeDefFrom.IsAnyStruct && typeDefFrom != NativeTypes.TextBlockType && GetDStructConverter(typeDefTo) != null)
        {
            return true;
        }

        if (typeDefTo.IsAnyStruct && typeDefTo != NativeTypes.TextBlockType && GetDStructConverter(typeDefFrom) != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Performs DStruct conversion between SObject and native types.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result.</param>
    /// <returns>True if conversion succeeded.</returns>
    private bool ConvertDStruct(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, object objFrom, out object result)
    {
        if (typeDefFrom.IsAnyStruct && GetDStructConverter(typeDefTo) is { } dTo && objFrom is SObject sobj)
        {
            try
            {
                result = dTo.ConvertFromSObject(sobj, typeDefTo.NativeType);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        if (typeDefTo.IsAnyStruct && GetDStructConverter(typeDefFrom) is { } dFrom)
        {
            try
            {
                result = dFrom.ConvertToSObject(objFrom, typeDefTo);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Gets the DStruct converter for the specified type definition.
    /// </summary>
    /// <param name="typeDef">The type definition to find a converter for.</param>
    /// <returns>The converter, or null if not found.</returns>
    private IDStructObjectConverter GetDStructConverter(TypeDefinition typeDef)
    {
        if (typeDef?.NativeType is not { } ntype)
        {
            return null;
        }

        return _dstructConverters.GetValueSafe(ntype);
    }
    #endregion

    #region AssetLink Conversion

    /// <summary>
    /// Determines if AssetLink conversion is possible between the given types.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <returns>True if AssetLink conversion is possible.</returns>
    private static bool GetIsAssetLinkConvertible(TypeDefinition typeDefFrom, TypeDefinition typeDefTo)
    {
        if (typeDefFrom.IsArray && typeDefTo.IsArray)
        {
            return GetIsAssetLinkConvertible(typeDefFrom.ElementType, typeDefTo.ElementType);
        }

        if (typeDefFrom.NativeType != null && typeDefFrom.NativeType == typeDefTo.NativeType)
        {
            if (typeDefFrom.IsAssetLink && !typeDefTo.IsAssetLink)
            {
                return true;
            }
            else if (typeDefTo.IsAssetLink && !typeDefFrom.IsAssetLink)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Performs AssetLink conversion between asset key references and actual asset objects.
    /// </summary>
    /// <param name="typeDefFrom">Source type definition.</param>
    /// <param name="typeDefTo">Target type definition.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result.</param>
    /// <returns>True if conversion succeeded.</returns>
    private static bool ConvertAssetLink(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, object objFrom, out object result)
    {
        result = null;
        if (typeDefFrom.IsArray != typeDefTo.IsArray)
        {
            return false;
        }

        bool isAssetLinkFrom = GetIsAssetLink(typeDefFrom);
        bool isAssetLinkTo = GetIsAssetLink(typeDefTo);

        if (isAssetLinkFrom && !isAssetLinkTo)
        {
            if (typeDefFrom.IsArray)
            {
                List<object> list = [];

                if (objFrom is not string && objFrom is System.Collections.IEnumerable sary)
                {
                    foreach (var item in sary)
                    {
                        if (item is SAssetKey k)
                        {
                            list.Add(k.TargetAsset);
                        }
                        else
                        {
                            list.Add(item);
                        }
                    }
                }

                result = list;
            }
            else
            {
                if (objFrom is SAssetKey k)
                {
                    result = k.TargetAsset;
                }
                else
                {
                    result = objFrom;
                }
            }

            return true;
        }
        else if (!isAssetLinkFrom && isAssetLinkTo)
        {
            if (typeDefFrom.IsArray)
            {
                List<object> list = [];

                if (objFrom is not string && objFrom is System.Collections.IEnumerable sary)
                {
                    foreach (var item in sary)
                    {
                        if (item is SAssetKey k)
                        {
                            list.Add(k);
                        }
                        else if (item is Asset asset)
                        {
                            list.Add(new SAssetKey(typeDefTo.ElementType, asset.Id));
                        }
                        else if (objFrom is IHasAsset hasAsset)
                        {
                            var targetAsset = hasAsset.TargetAsset;
                            list.Add(new SAssetKey(typeDefTo, targetAsset?.Id ?? Guid.Empty));
                        }
                        else if (objFrom is IHasId hasId)
                        {
                            list.Add(new SAssetKey(typeDefTo.ElementType, hasId.Id));
                        }
                        else
                        {
                            list.Add(new SAssetKey(typeDefTo.ElementType, Guid.Empty));
                        }
                    }
                }

                result = list;
            }
            else
            {
                if (objFrom is SAssetKey k)
                {
                    result = k;
                }
                else if (objFrom is Asset asset)
                {
                    result = new SAssetKey(typeDefTo, asset.Id);
                }
                else if (objFrom is IHasAsset hasAsset)
                {
                    var targetAsset = hasAsset.TargetAsset;
                    result = new SAssetKey(typeDefTo, targetAsset?.Id ?? Guid.Empty);
                }
                else if (objFrom is IHasId hasId)
                {
                    result = new SAssetKey(typeDefTo, hasId.Id);
                }
                else
                {
                    result = new SAssetKey(typeDefTo, Guid.Empty);
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if a type definition represents an AssetLink type.
    /// </summary>
    /// <param name="type">The type definition to check.</param>
    /// <returns>True if the type is an AssetLink.</returns>
    private static bool GetIsAssetLink(TypeDefinition type)
    {
        if (type.IsArray)
        {
            return type.ElementType.IsAssetLink;
        }
        else
        {
            return type.IsAssetLink;
        }
    }

    #endregion

}
