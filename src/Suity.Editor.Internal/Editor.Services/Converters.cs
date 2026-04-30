using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Converts between text-related types (string, TextBlock, STextBlock).
/// </summary>
public class TextToTextConverter : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public Type[] TypesTo => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        return TypeConvertExtensions.ConvertText(objFrom, typeTo);
    }
}

/// <summary>
/// Converts between base value types (primitives, string, DateTime, Decimal).
/// </summary>
public class BaseValueConverter : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom =>
        [
        typeof(Boolean),
        typeof(Byte),
        typeof(Int16),
        typeof(Int32),
        typeof(Int64),
        typeof(SByte),
        typeof(UInt16),
        typeof(UInt32),
        typeof(UInt64),
        typeof(Single),
        typeof(Double),
        typeof(String),
        typeof(Decimal),
        typeof(DateTime),
        ];

    /// <inheritdoc/>
    public Type[] TypesTo =>
        [
        typeof(Boolean),
        typeof(Byte),
        typeof(Int16),
        typeof(Int32),
        typeof(Int64),
        typeof(SByte),
        typeof(UInt16),
        typeof(UInt32),
        typeof(UInt64),
        typeof(Single),
        typeof(Double),
        typeof(String),
        typeof(Decimal),
        typeof(DateTime),
        ];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        return Convert.ChangeType(objFrom, typeTo);
    }
}


/// <summary>
/// Converts text content to SObject by deserializing JSON.
/// </summary>
public class TextToSObjectConverter : ITypeConverter
{
    /// <inheritdoc/>
    public Type[] TypesFrom => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public Type[] TypesTo => [typeof(SObject)];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, Type typeTo)
    {
        try
        {
            string s = objFrom.ToString() ?? string.Empty;

            var sobj = EditorServices.JsonResource.FromJson(s) as SObject;

            return sobj;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

/// <summary>
/// Converts SObject to text by serializing to JSON.
/// </summary>
public class SObjectToTextConverter : TypeToTextConverter<SObject>
{
    /// <inheritdoc/>
    public override string Convert(SObject sobj)
    {
        try
        {
            return EditorServices.JsonResource.GetJson(sobj)?.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }
}

/// <summary>
/// Converts between text content and DStruct SObject types.
/// </summary>
public class TextToDStructObjectConverter : IDStructObjectConverter
{
    /// <inheritdoc/>
    public Type[] Types => [typeof(string), typeof(TextBlock), typeof(STextBlock)];

    /// <inheritdoc/>
    public SObject ConvertToSObject(object objFrom, TypeDefinition sobjType)
    {
        try
        {
            string s = objFrom.ToString() ?? string.Empty;
            var option = new SItemResourceOptions
            {
                TypeHint = sobjType,
            };
            var sobj = EditorServices.JsonResource.FromJson(s, option) as SObject;

            return sobj;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public object ConvertFromSObject(SObject objFrom, Type typeTo)
    {
        try
        {
            string s = EditorServices.JsonResource.GetJson(objFrom)?.ToString() ?? string.Empty;
            return TypeConvertExtensions.ConvertText(s, typeTo);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
