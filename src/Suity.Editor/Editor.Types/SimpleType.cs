using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Json;
using System.Collections.Generic;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a simple type definition for schema generation.
/// </summary>
public class SimpleType
{
    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the full name of the type, which can be used for more detailed schema representations.
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets or sets the tooltips.
    /// </summary>
    public string Tooltips { get; init; }

    /// <summary>
    /// Gets or sets the fields.
    /// </summary>
    public IReadOnlyList<SimpleField> Fields { get; init; }

    /// <summary>
    /// Initializes a new instance of the SimpleType class.
    /// </summary>
    public SimpleType()
    {
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    /// <param name="newName">The new name for the cloned instance. If null, the original name is used.</param>
    /// <returns>A new instance of SimpleType with the same values.</returns>
    public SimpleType Clone(string newName = null)
    {
        return new SimpleType
        {
            Name = newName ?? Name,
            FullName = FullName,
            Description = Description,
            Tooltips = Tooltips,
            Fields = Fields
        };
    }

    /// <summary>
    /// Converts this type to a data writable schema.
    /// </summary>
    /// <returns>A data writable schema.</returns>
    public IDataWritable ToDataWritable()
    {
        return EditorServices.JsonSchemaService.CreateSchema(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDataWritable()?.ToString();
    }

    public string ToString(bool niceFormat)
    {
        var dataWritable = ToDataWritable();

        var jsonWriter = new JsonDataWriter();
        dataWritable.WriteData(jsonWriter);
        return jsonWriter.ToString(niceFormat);
    }
}

/// <summary>
/// Represents a simple field definition for schema generation.
/// </summary>
public class SimpleField
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets or sets the tooltips.
    /// </summary>
    public string Tooltips { get; init; }

    /// <summary>
    /// Gets or sets the type definition.
    /// </summary>
    public TypeDefinition Type { get; init; }

    /// <summary>
    /// Gets or sets whether the field is optional.
    /// </summary>
    public bool Optional { get; init; }

    /// <summary>
    /// Gets or sets the numeric range attribute.
    /// </summary>
    public NumericRangeAttribute Range { get; init; }

    /// <summary>
    /// Gets or sets the selection design attribute.
    /// </summary>
    public SelectionDesignAttribute Selection { get; init; }

    /// <summary>
    /// Converts this field to a data writable schema property.
    /// </summary>
    /// <returns>A data writable schema property.</returns>
    public IDataWritable ToDataWritable()
    {
        return EditorServices.JsonSchemaService.CreateSchemaProperty(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDataWritable()?.ToString();
    }
}
