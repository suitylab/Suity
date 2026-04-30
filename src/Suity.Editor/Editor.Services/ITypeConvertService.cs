using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Interface for type converters.
/// </summary>
public interface ITypeConverter
{
    /// <summary>
    /// Gets the source types for conversion.
    /// </summary>
    Type[] TypesFrom { get; }

    /// <summary>
    /// Gets the target types for conversion.
    /// </summary>
    Type[] TypesTo { get; }

    /// <summary>
    /// Converts an object from the source type to the target type.
    /// </summary>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="typeTo">The target type.</param>
    /// <returns>The converted object.</returns>
    object ConvertType(object objFrom, Type typeTo);
}

/// <summary>
/// Interface for type definition converters.
/// </summary>
public interface ITypeDefinitionConverter
{
    /// <summary>
    /// Gets the source type definitions for conversion.
    /// </summary>
    TypeDefinition[] TypesFrom { get; }

    /// <summary>
    /// Gets the target type definitions for conversion.
    /// </summary>
    TypeDefinition[] TypesTo { get; }

    /// <summary>
    /// Converts an object from the source type definition to the target type definition.
    /// </summary>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="typeTo">The target type definition.</param>
    /// <returns>The converted object.</returns>
    object ConvertType(object objFrom, TypeDefinition typeTo);
}

/// <summary>
/// Interface for converting between SObject and structured data types.
/// </summary>
public interface IDStructObjectConverter
{
    /// <summary>
    /// Gets the types this converter can handle.
    /// </summary>
    Type[] Types { get; }

    /// <summary>
    /// Converts an object to an SObject.
    /// </summary>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="sobjType">The target SObject type.</param>
    /// <returns>The converted SObject.</returns>
    SObject ConvertToSObject(object objFrom, TypeDefinition sobjType);

    /// <summary>
    /// Converts an SObject to an object.
    /// </summary>
    /// <param name="objFrom">The SObject to convert.</param>
    /// <param name="typeTo">The target type.</param>
    /// <returns>The converted object.</returns>
    object ConvertFromSObject(SObject objFrom, Type typeTo);
}

/// <summary>
/// Represents the state of a type conversion attempt.
/// </summary>
public enum TypeConvertState
{
    /// <summary>
    /// The value is null.
    /// </summary>
    Null,

    /// <summary>
    /// The types are the same.
    /// </summary>
    Same,

    /// <summary>
    /// The source type is assignable to the target type.
    /// </summary>
    Assignable,

    /// <summary>
    /// The types are convertible.
    /// </summary>
    Convertible,

    /// <summary>
    /// The types are not convertible.
    /// </summary>
    Unconvertible,
}

/// <summary>
/// Abstract generic type converter.
/// </summary>
public abstract class TypeConverter<TFrom, TTo> : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom => [typeof(TFrom)];

    /// <inheritdoc/>
    public Type[] TypesTo => [typeof(TTo)];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        if (objFrom is TFrom from)
        {
            return Convert(from);
        }

        return null;
    }

    /// <summary>
    /// Converts a value from TFrom to TTo.
    /// </summary>
    /// <param name="objFrom">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public abstract TTo Convert(TFrom objFrom);
}

/// <summary>
/// Abstract generic type to text converter.
/// </summary>
public abstract class TypeToTextConverter<TFrom> : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom => [typeof(TFrom)];

    /// <inheritdoc/>
    public Type[] TypesTo => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        if (objFrom is TFrom from)
        {
            string s = Convert(from);
            return TypeConvertExtensions.ConvertText(s, typeTo);
        }

        return null;
    }

    /// <summary>
    /// Converts a value to a string.
    /// </summary>
    /// <param name="objFrom">The value to convert.</param>
    /// <returns>The string representation.</returns>
    public abstract string Convert(TFrom objFrom);
}

/// <summary>
/// Abstract generic text to type converter.
/// </summary>
public abstract class TextToTypeConverter<TTo> : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public Type[] TypesTo => [typeof(TTo)];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        return Convert(objFrom?.ToString());
    }

    /// <summary>
    /// Converts a string to the target type.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The converted value.</returns>
    public abstract TTo Convert(string text);
}

/// <summary>
/// Abstract converter from asset link to a typed object.
/// </summary>
public abstract class AssetLinkToTypeConverter<TFrom, TTo> : ITypeDefinitionConverter
{
    /// <inheritdoc/>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromAssetLink<TFrom>()];

    /// <inheritdoc/>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromNative<TTo>()];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        TFrom c;

        if (objFrom is TFrom obj)
        {
            c = obj;
        }
        else if (objFrom is SAssetKey sAssetKey && sAssetKey.TargetAsset is TFrom obj2)
        {
            c = obj2;
        }
        else
        {
            c = default;
        }

        var result = Convert(c);

        return result;
    }

    /// <summary>
    /// Converts an asset link to the target type.
    /// </summary>
    /// <param name="objFroms">The asset link value.</param>
    /// <returns>The converted value.</returns>
    public abstract TTo Convert(TFrom objFroms);
}

/// <summary>
/// Abstract converter from typed object to asset link.
/// </summary>
public abstract class TypeToAssetLinkConverter<TFrom, TTo> : ITypeDefinitionConverter
    where TTo : IHasId
{
    /// <inheritdoc/>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromNative<TFrom>()];

    /// <inheritdoc/>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromAssetLink<TTo>()];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        if (objFrom is not TFrom f)
        {
            return null;
        }

        var result = Convert(f);

        var type = TypeDefinition.FromAssetLink<TTo>();
        return new SAssetKey(type, result.Id);
    }

    /// <summary>
    /// Converts the source value to an asset link.
    /// </summary>
    /// <param name="objFroms">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public abstract TTo Convert(TFrom objFroms);
}

/// <summary>
/// Abstract converter from asset link to text.
/// </summary>
public abstract class AssetLinkToTextConverter<TFrom> : ITypeDefinitionConverter
{
    /// <inheritdoc/>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromAssetLink<TFrom>()];

    /// <inheritdoc/>
    public TypeDefinition[] TypesTo => [NativeTypes.StringType, NativeTypes.TextBlockType];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        TFrom c;

        if (objFrom is TFrom obj)
        {
            c = obj;
        }
        else if (objFrom is SAssetKey sAssetKey && sAssetKey.TargetAsset is TFrom obj2)
        {
            c = obj2;
        }
        else
        {
            c = default;
        }

        if (c is null)
        {
            return null;
        }

        string s = Convert(c);

        if (typeTo == NativeTypes.StringType)
        {
            return s;
        }
        else if (typeTo == NativeTypes.TextBlockType)
        {
            return new TextBlock(s);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Converts an asset link to a string.
    /// </summary>
    /// <param name="objFroms">The asset link value.</param>
    /// <returns>The string representation.</returns>
    public abstract string Convert(TFrom objFroms);
}

/// <summary>
/// Abstract converter from asset link array to text.
/// </summary>
public abstract class AssetLinkArrayToTextConverter<TFrom> : ITypeDefinitionConverter
{
    /// <inheritdoc/>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromAssetLink<TFrom>().MakeArrayType()];

    /// <inheritdoc/>
    public TypeDefinition[] TypesTo => [NativeTypes.StringType, NativeTypes.TextBlockType];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        string s = string.Empty;

        if (objFrom is not string && objFrom is System.Collections.IEnumerable c)
        {
            List<TFrom> list = [];
            foreach (var item in c)
            {
                if (item is TFrom obj)
                {
                    list.Add(obj);
                }
                else if (item is SAssetKey sAssetKey && sAssetKey.TargetAsset is TFrom obj2)
                {
                    list.Add(obj2);
                }
            }

            s = Convert([.. list]);
        }

        if (typeTo == NativeTypes.StringType)
        {
            return s;
        }
        else if (typeTo == NativeTypes.TextBlockType)
        {
            return new TextBlock(s);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Converts an array of asset links to a string.
    /// </summary>
    /// <param name="objFroms">The array of asset links.</param>
    /// <returns>The string representation.</returns>
    public abstract string Convert(TFrom[] objFroms);
}


/// <summary>
/// Service interface for type conversion operations.
/// </summary>
public interface ITypeConvertService
{
    /// <summary>
    /// Checks if conversion is possible between two types.
    /// </summary>
    /// <param name="typeFrom">The source type.</param>
    /// <param name="typeTo">The target type.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState CanConvert(Type typeFrom, Type typeTo);

    /// <summary>
    /// Checks if conversion is possible between two type definitions.
    /// </summary>
    /// <param name="typeDefFrom">The source type definition.</param>
    /// <param name="typeDefTo">The target type definition.</param>
    /// <param name="toMultiple">Whether conversion to multiple values is allowed.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState CanConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple);

    /// <summary>
    /// Checks if conversion is possible between two flow node connectors.
    /// </summary>
    /// <param name="connectorFrom">The source connector.</param>
    /// <param name="connectorTo">The target connector.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState CanConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo);

    /// <summary>
    /// Attempts to convert an object from one type to another.
    /// </summary>
    /// <param name="typeFrom">The source type.</param>
    /// <param name="typeTo">The target type.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState TryConvert(Type typeFrom, Type typeTo, object objFrom, out object result);

    /// <summary>
    /// Attempts to convert an object from one type definition to another.
    /// </summary>
    /// <param name="typeDefFrom">The source type definition.</param>
    /// <param name="typeDefTo">The target type definition.</param>
    /// <param name="toMultiple">Whether conversion to multiple values is allowed.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState TryConvert(TypeDefinition typeDefFrom, TypeDefinition typeDefTo, bool toMultiple, object objFrom, out object result);

    /// <summary>
    /// Attempts to convert an object between flow node connectors.
    /// </summary>
    /// <param name="connectorFrom">The source connector.</param>
    /// <param name="connectorTo">The target connector.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState TryConvert(FlowNodeConnector connectorFrom, FlowNodeConnector connectorTo, object objFrom, out object result);

    /// <summary>
    /// Attempts to convert an object to a flow node connector type.
    /// </summary>
    /// <param name="connectorTo">The target connector.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState TryConvert(FlowNodeConnector connectorTo, object objFrom, out object result);

    /// <summary>
    /// Attempts to convert an object to a type definition.
    /// </summary>
    /// <param name="typeDefTo">The target type definition.</param>
    /// <param name="toMultiple">Whether conversion to multiple values is allowed.</param>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>The conversion state.</returns>
    TypeConvertState TryConvert(TypeDefinition typeDefTo, bool toMultiple, object objFrom, out object result);
}

/// <summary>
/// Extension methods for type conversion.
/// </summary>
public static class TypeConvertExtensions
{
    /// <summary>
    /// Converts a value from one type to another.
    /// </summary>
    /// <typeparam name="TFrom">The source type.</typeparam>
    /// <typeparam name="TTo">The target type.</typeparam>
    /// <param name="objFrom">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="Exception">Thrown when conversion is not possible.</exception>
    public static TTo Convert<TFrom, TTo>(TFrom objFrom)
    {
        var state = EditorServices.TypeConvertService.TryConvert(typeof(TFrom), typeof(TTo), objFrom, out var result);

        if (state != TypeConvertState.Unconvertible && result is TTo to)
        {
            return to;
        }
        else
        {
            throw new Exception($"Can not convert from {typeof(TFrom)} to {typeof(TTo)}");
        }
    }

    /// <summary>
    /// Converts an object to text representation for specific types.
    /// </summary>
    /// <param name="objFrom">The object to convert.</param>
    /// <param name="typeTo">The target type (string, TextBlock, or STextBlock).</param>
    /// <returns>The text representation, or null if conversion is not possible.</returns>
    public static object ConvertText(object objFrom, Type typeTo)
    {
        string s;
        try
        {
            s = objFrom.ToString() ?? string.Empty;
        }
        catch (Exception)
        {
            return null;
        }

        if (typeTo == typeof(string))
        {
            return s;
        }
        else if (typeTo == typeof(TextBlock))
        {
            return new TextBlock(s);
        }
        else if (typeTo == typeof(STextBlock))
        {
            return new STextBlock(s);
        }

        return null;
    }
}