using ComputerBeacon.Json;
using Suity.Json;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Transferring;

/// <summary>
/// Data transfer context for reading and writing data using IDataReader and IDataWriter.
/// </summary>
public class DataRW
{
    /// <summary>
    /// Gets or sets the data reader.
    /// </summary>
    public IDataReader Reader { get; set; }
    /// <summary>
    /// Gets or sets the data writer.
    /// </summary>
    public IDataWriter Writer { get; set; }
    /// <summary>
    /// Gets or sets the options for the transfer.
    /// </summary>
    public object Options { get; set; }

    /// <summary>
    /// Gets the list of newly created objects during transfer.
    /// </summary>
    public List<object> NewObjects;

    /// <summary>
    /// Gets the content transfer for the specified type.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <returns>The content transfer instance.</returns>
    public static ContentTransfer<DataRW> GetTransfer(Type type)
        => ContentTransfer<DataRW>.GetTransfer(type);

    /// <summary>
    /// Creates a data transfer context with a JSON reader from a string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The data transfer context.</returns>
    public static DataRW CreateJsonReader(string json, object options = null)
    {
        var reader = new JsonDataReader(json);

        return new DataRW { Reader = reader, Options = options };
    }

    /// <summary>
    /// Creates a data transfer context with a JSON reader from a JsonObject.
    /// </summary>
    /// <param name="jobj">The JSON object.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The data transfer context.</returns>
    public static DataRW CreateJsonReader(JsonObject jobj, object options = null)
    {
        var reader = new JsonDataReader(jobj);

        return new DataRW { Reader = reader, Options = options };
    }

    /// <summary>
    /// Creates a data transfer context with a JSON writer.
    /// </summary>
    /// <param name="options">Optional options.</param>
    /// <returns>The data transfer context.</returns>
    public static DataRW CreateJsonWriter(object options = null)
    {
        var writer = new JsonDataWriter();

        return new DataRW { Writer = writer, Options = options };
    }

    /// <summary>
    /// Inputs JSON data to the target object.
    /// </summary>
    /// <param name="source">The target object.</param>
    /// <param name="jobj">The JSON object.</param>
    /// <param name="options">Optional options.</param>
    public static void InputJson(object source, JsonObject jobj, object options = null)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType())
            ?? throw new InvalidOperationException($"No transfer for type '{source.GetType()}'");

        var data = CreateJsonReader(jobj, options);
        transfer.Input(source, data);
    }

    /// <summary>
    /// Inputs JSON data to the target object from a string.
    /// </summary>
    /// <param name="source">The target object.</param>
    /// <param name="json">The JSON string.</param>
    /// <param name="options">Optional options.</param>
    public static void InputJson(object source, string json, object options = null)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType())
            ?? throw new InvalidOperationException($"No transfer for type '{source.GetType()}'");

        var data = CreateJsonReader(json, options);
        transfer.Input(source, data);
    }

    /// <summary>
    /// Outputs JSON string from the source object.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="selection">Objects to output.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The JSON string.</returns>
    public static string OutputJsonString(object source, ICollection<object> selection = null, object options = null)
    {
        return OutputJson(source, selection, options)?.ToString();
    }

    /// <summary>
    /// Outputs JSON object from the source object.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="selection">Objects to output.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject OutputJson(object source, ICollection<object> selection = null, object options = null)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType())
            ?? throw new InvalidOperationException($"No transfer for type '{source.GetType()}'");

        var data = CreateJsonWriter(options);

        transfer.Output(source, data, selection);

        return (data.Writer as JsonDataWriter)?.Value as JsonObject;
    }

    /// <summary>
    /// Outputs JSON string from the source object preserving object structure.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="selection">Objects to output.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The JSON string.</returns>
    public static string OutputObjectJsonString(object source, ICollection<object> selection = null, object options = null)
    {
        return OutputObjectJson(source, selection, options)?.ToString();
    }

    /// <summary>
    /// Outputs JSON object from the source object preserving object structure.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="selection">Objects to output.</param>
    /// <param name="options">Optional options.</param>
    /// <returns>The JSON object.</returns>
    public static JsonObject OutputObjectJson(object source, ICollection<object> selection = null, object options = null)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var type = source.GetType();

        var transfer = GetTransfer(type)
            ?? throw new InvalidOperationException($"No transfer for type '{type}'");

        var data = CreateJsonWriter(options);

        transfer.Output(source, data, selection);

        return (data.Writer as JsonDataWriter)?.Value as JsonObject;
    }
}

/// <summary>
/// Base class for data read/write transfers with a specific source type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
public abstract class DataRWTransfer<TSource> : ContentTransfer<TSource, DataRW>
{
}