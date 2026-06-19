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
    public TypeConvertResult CanConvert(Type typeFrom, Type typeTo)
    {
        if (typeFrom is null || typeTo is null)
        {
            return TypeConvertResult.Unconvertible;
        }

        if (typeFrom == typeTo)
        {
            return TypeConvertResult.Same;
        }

        if (typeTo.IsAssignableFrom(typeFrom))
        {
            return TypeConvertResult.Assignable;
        }

        if (_typeConverters.GetValueSafe(new TwoType(typeFrom, typeTo)) is { } converter)
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.Converter, converter);
        }

        if (typeTo == typeof(string))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.ToString);
        }

        if (typeTo == typeof(object))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.ToObject);
        }

        return TypeConvertResult.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertResult CanConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple)
    {
        if (TypeDefinition.IsNullOrEmpty(typeDefFrom) || TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            return TypeConvertResult.Unconvertible;
        }

        if (typeDefFrom == typeDefTo)
        {
            return TypeConvertResult.Same;
        }

        // Array to multi-connection port conversion
        if (typeDefFrom.IsArray && typeDefFrom.ElementType == typeDefTo && toMultiple)
        {
            return TypeConvertResult.Same;
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
            return TypeConvertResult.Assignable;
        }

        if (_typeDefConverters.GetValueSafe(new TwoTypeDef(typeDefFrom, typeDefTo)) is { } converter)
        {
            // var converter = _typeDefConverters[new TwoTypeDef(typeDefFrom, typeDefTo)];

            return TypeConvertResult.FromConvert(TypeConvertModes.Converter, converter);
        }

        if (GetIsArrayConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.Array);
        }

        if (GetIsAssetLinkConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.AssetLink);
        }

        if (GetIsDStructConvertible(typeDefFrom, typeDefTo))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.DStruct);
        }

        if (typeDefFrom.IsArray && typeDefTo.IsArray)
        {
            return CanConvert(typeDefFrom.ElementType, typeDefTo.ElementType, false);
        }

        if (typeDefTo == NativeTypes.ObjectType)
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.ToObject);
        }

        if (typeDefFrom.IsAssetLink && typeDefTo == SAssetKeyType)
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.LinkToKey);
        }
        else if (typeDefFrom == SAssetKeyType && typeDefTo.IsAssetLink)
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.KeyToLink);
        }

        if (typeDefFrom.IsArray && typeDefFrom.ElementType?.ElementType is null && typeDefTo.ElementType is null)
        {
            var aryType = typeDefFrom.ElementType?.NativeType?.MakeArrayType();
            return CanConvert(aryType, typeDefTo.NativeType);
        }
        else if (typeDefFrom.ElementType is null && typeDefTo.IsArray && typeDefTo.ElementType?.ElementType is null)
        {
            var aryType = typeDefTo.ElementType?.NativeType?.MakeArrayType();
            return CanConvert(typeDefFrom.NativeType, aryType);
        }
        else if (typeDefFrom.ElementType is null && typeDefTo.ElementType is null)
        {
            return CanConvert(typeDefFrom.NativeType, typeDefTo.NativeType);
        }


        return TypeConvertResult.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertResult CanConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo)
    {
        if (connectorFrom is null || connectorTo is null)
        {
            return TypeConvertResult.Unconvertible;
        }

        if (!string.IsNullOrWhiteSpace(connectorFrom.DataTypeName) 
            && connectorFrom.DataTypeName == connectorTo.DataTypeName)
        {
            return TypeConvertResult.Same;
        }

        var typeDefFrom = TypeDefinition.Resolve(connectorFrom.DataTypeName);
        var typeDefTo = TypeDefinition.Resolve(connectorTo.DataTypeName);

        return CanConvert(typeDefFrom, typeDefTo, connectorTo.AllowMultipleConnection == true);
    }

    /// <inheritdoc/>
    public TypeConvertResult TryConvert(Type typeFrom, Type typeTo, object objFrom)
    {
        if (objFrom is null)
        {
            return TypeConvertResult.Null;
        }

        if (typeFrom is null || typeTo is null)
        {
            return TypeConvertResult.Unconvertible;
        }

        //if (!typeFrom.IsAssignableFrom(objFrom.GetType()))
        //{
        //    return TypeConvertState.Unconvertible;
        //}

        if (typeFrom == typeTo)
        {
            return TypeConvertResult.FromSame(objFrom);
        }

        if (typeTo.IsAssignableFrom(typeFrom))
        {
            return TypeConvertResult.FromAssignable(objFrom);
        }

        var twoType = new TwoType(typeFrom, typeTo);
        if (_typeConverters.TryGetValue(twoType, out var converter))
        {
            try
            {
                var result = converter.ConvertType(objFrom, typeTo);
                return TypeConvertResult.FromConvert(TypeConvertModes.Converter, converter, objFrom, result);
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (typeTo == typeof(string))
        {
            try
            {
                string result = objFrom.ToString() ?? string.Empty;
                return TypeConvertResult.FromConvert(TypeConvertModes.ToString, null, objFrom, result);
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (typeTo == typeof(object))
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.ToObject, null, objFrom, objFrom);
        }

        return TypeConvertResult.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertResult TryConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple, object objFrom)
    {
        if (objFrom is null)
        {
            return TypeConvertResult.Null;
        }

        if (TypeDefinition.IsNullOrEmpty(typeDefFrom) || TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            return TypeConvertResult.Unconvertible;
        }

        if (typeDefTo == typeDefFrom)
        {
            return TypeConvertResult.FromSame(objFrom);
        }

        // Array to multi-connection port conversion
        if (typeDefFrom.IsArray && typeDefFrom.ElementType == typeDefTo && toMultiple)
        {
            return TypeConvertResult.FromSame(objFrom);
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
            return TypeConvertResult.FromAssignable(objFrom);
        }

        var twoType = new TwoTypeDef(typeDefFrom, typeDefTo);
        if (_typeDefConverters.TryGetValue(twoType, out var converter))
        {
            try
            {
                var result = converter.ConvertType(objFrom, typeDefTo);
                return TypeConvertResult.FromConvert(TypeConvertModes.Converter, converter, objFrom, result);
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (GetIsArrayConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                if (ConvertArray(typeDefFrom, typeDefTo, objFrom, out var result))
                {
                    return TypeConvertResult.FromConvert(TypeConvertModes.Array, null, objFrom, result);
                }
                else
                {
                    return TypeConvertResult.Unconvertible;
                }
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (GetIsAssetLinkConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                if (ConvertAssetLink(typeDefFrom, typeDefTo, objFrom, out var result))
                {
                    return TypeConvertResult.FromConvert(TypeConvertModes.AssetLink, null, objFrom, result);
                }
                else
                {
                    return TypeConvertResult.Unconvertible;
                }
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (GetIsDStructConvertible(typeDefFrom, typeDefTo))
        {
            try
            {
                if ( ConvertDStruct(typeDefFrom, typeDefTo, objFrom, out var result)) 
                {
                    return TypeConvertResult.FromConvert(TypeConvertModes.DStruct, null, objFrom, result);
                }
                else
                {
                    return TypeConvertResult.Unconvertible;
                }
            }
            catch (Exception)
            {
                return TypeConvertResult.Unconvertible;
            }
        }


        if (typeDefFrom.IsArray && typeDefTo.IsArray)
        {
            if (objFrom is not string && objFrom is System.Collections.IEnumerable ary)
            {
                var newSary = new SArray(typeDefTo);
                TypeConvertResult elementResult = default;
                foreach (var item in ary)
                {
                    elementResult = TryConvert(typeDefFrom.ElementType, typeDefTo.ElementType, false, item);
                    newSary.Add(elementResult.To);
                }

                // Use last element's conversion result as overall result (could be improved by accumulating states)
                return TypeConvertResult.FromConvert(elementResult.Mode, elementResult.Converter, objFrom, newSary);
            }
            else
            {
                return TypeConvertResult.Unconvertible;
            }
        }

        if (typeDefTo == NativeTypes.ObjectType)
        {
            return TypeConvertResult.FromConvert(TypeConvertModes.ToObject, null, objFrom, objFrom);
        }

        if (typeDefFrom.IsAssetLink && typeDefTo == SAssetKeyType)
        {
            var result = Cloner.Clone(objFrom as SAssetKey);
            return TypeConvertResult.FromConvert(TypeConvertModes.LinkToKey, null, objFrom, result);
        }
        else if (typeDefFrom == SAssetKeyType && typeDefTo.IsAssetLink)
        {
            var result = new SAssetKey(typeDefTo, (objFrom as SAssetKey)?.Id ?? Guid.Empty);
            return TypeConvertResult.FromConvert(TypeConvertModes.KeyToLink, null, objFrom, result);
        }

        if (typeDefFrom.IsArray && typeDefFrom.ElementType?.ElementType is null && typeDefTo.ElementType is null)
        {
            var aryType = typeDefFrom.ElementType?.NativeType?.MakeArrayType();
            return TryConvert(aryType, typeDefTo.NativeType, objFrom);
        }
        else if (typeDefFrom.ElementType is null && typeDefTo.IsArray && typeDefTo.ElementType?.ElementType is null)
        {
            var aryType = typeDefTo.ElementType?.NativeType?.MakeArrayType();
            return TryConvert(typeDefFrom.NativeType, aryType, objFrom);
        }
        else if (typeDefFrom.ElementType is null && typeDefTo.ElementType is null)
        {
            return TryConvert(typeDefFrom.NativeType, typeDefTo.NativeType, objFrom);
        }

        return TypeConvertResult.Unconvertible;
    }

    /// <inheritdoc/>
    public TypeConvertResult TryConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo, object objFrom)
    {
        if (objFrom is null)
        {
            return TypeConvertResult.Null;
        }

        if (connectorFrom is null || connectorTo is null)
        {
            return TypeConvertResult.Unconvertible;
        }

        if (!string.IsNullOrWhiteSpace(connectorFrom.DataTypeName)
            && connectorFrom.DataTypeName == connectorTo.DataTypeName)
        {
            return TypeConvertResult.FromSame(objFrom);
        }

        var typeDefFrom = TypeDefinition.Resolve(connectorFrom?.DataTypeName);
        var typeDefTo = TypeDefinition.Resolve(connectorTo?.DataTypeName);

        return TryConvert(typeDefFrom, typeDefTo, connectorTo.AllowMultipleConnection == true, objFrom);
    }


    /// <inheritdoc/>
    public TypeConvertResult TryConvert(FlowNodeConnector connectorTo, object objFrom)
    {
        if (connectorTo is null || objFrom is null)
        {
            return TypeConvertResult.Null;
        }

        var typeDefTo = TypeDefinition.Resolve(connectorTo.DataTypeName);
        if (TypeDefinition.IsNullOrEmpty(typeDefTo))
        {
            return TypeConvertResult.Null;
        }

        return TryConvert(typeDefTo, connectorTo.AllowMultipleConnection == true, objFrom);
    }

    /// <inheritdoc/>
    public TypeConvertResult TryConvert(TypeDefinition typeDefTo, bool toMultiple, object objFrom)
    {
        if (TypeDefinition.IsNullOrEmpty(typeDefTo) || objFrom is null)
        {
            return TypeConvertResult.Null;
        }

        if (typeDefTo.ElementType is null && typeDefTo.NativeType is { } typeTo && typeTo.IsAssignableFrom(objFrom.GetType()))
        {
            return TypeConvertResult.FromAssignable(objFrom);
        }

        var typeDefFrom = TypeDefinition.ResolveNative(objFrom);
        if (TypeDefinition.IsNullOrEmpty(typeDefFrom))
        {
            return TypeConvertResult.Null;
        }

        return TryConvert(typeDefFrom, typeDefTo, toMultiple, objFrom);
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
