using Suity.Editor.Services;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC;

/// <summary>
/// Defines a contract for function call types that can provide metadata and schema information.
/// </summary>
public interface IFunctionCallType
{
    /// <summary>
    /// Gets the fully qualified name of the function call type.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// Gets a description of the function call type.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Generates and returns the schema object for this function call type.
    /// </summary>
    /// <param name="context">Optional context used during schema generation.</param>
    /// <returns>The schema object representing the function call type.</returns>
    object GetSchema(FunctionContext context = null);
}

/// <summary>
/// Represents a compound function call type that wraps a <see cref="DCompond"/> and provides schema generation capabilities.
/// </summary>
public class DCompondFunctionCallType : IFunctionCallType
{
    /// <summary>
    /// Gets the underlying <see cref="DCompond"/> type.
    /// </summary>
    public DCompond Type { get; }

    /// <summary>
    /// Gets the fully qualified name of the compound type.
    /// </summary>
    public string FullName => Type.FullName;

    /// <summary>
    /// Gets a description of the compound type.
    /// </summary>
    public string Description => Type.Description;

    /// <summary>
    /// Gets a value indicating whether only simple fields should be included in the schema.
    /// </summary>
    public bool SimpleFieldOnly { get; }

    /// <summary>
    /// Gets or sets the collection of fields to exclude from schema generation.
    /// </summary>
    public ICollection<DStructField> ExcludedFields { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DCompondFunctionCallType"/> class.
    /// </summary>
    /// <param name="type">The compound type to wrap. Must not be null.</param>
    /// <param name="simpleFieldOnly">Indicates whether only simple fields should be included. Defaults to true.</param>
    public DCompondFunctionCallType(DCompond type, bool simpleFieldOnly = true)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        SimpleFieldOnly = simpleFieldOnly;
    }

    /// <summary>
    /// Generates and returns the JSON schema for the compound type.
    /// </summary>
    /// <param name="context">The function context used during schema generation.</param>
    /// <returns>The schema object representing the compound type.</returns>
    public object GetSchema(FunctionContext context)
    {
        var option = new SchemaGenerateOptions
        {
            Context = context,
            TypeGuiding = false,
            SimpleFieldOnly = SimpleFieldOnly,
            ExcludedFields = ExcludedFields,
        };

        return EditorServices.JsonSchemaService.CreateSchemaProperty(Type, option);
    }
}
