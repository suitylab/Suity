using Suity.Editor.Design;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Options for schema generation.
/// </summary>
public class SchemaGenerateOptions
{
    /// <summary>
    /// Gets or sets the function context.
    /// </summary>
    public FunctionContext Context { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth for schema generation.
    /// </summary>
    public int Depth { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to enable type guiding.
    /// </summary>
    public bool TypeGuiding { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use simple fields only.
    /// </summary>
    public bool SimpleFieldOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets the excluded fields.
    /// </summary>
    public ICollection<DStructField> ExcludedFields { get; set; }
}


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
    /// Gets or sets the tooltips.
    /// </summary>
    public string Tooltips { get; init; }

    /// <summary>
    /// Gets or sets the fields.
    /// </summary>
    public SimpleField[] Fields { get; init; }

    /// <summary>
    /// Initializes a new instance of the SimpleType class.
    /// </summary>
    public SimpleType()
    {
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

/// <summary>
/// JsonSchema service
/// </summary>
public interface IJsonSchemaService
{
    /// <summary>
    /// Creates a Schema object for the provided type. The returned Schema only supports Newtonsoft.Json.
    /// If you need to use other Json libraries, do not use this method. Instead, use the SchemaGenerator class for that library.
    /// Under normal circumstances, do not use this method. Use the <see cref="CreateSchemaProperty"/> method instead, as different APIs may require different formats.
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="fieldDescriptions">By providing key-value pairs, replace the text on the [Description] tag.</param>
    /// <returns>Returns Schema object, call ToString() to get text.</returns>
    object CreateSchema(Type type, IDictionary<string, string> fieldDescriptions = null);

    /// <summary>
    /// Creates a Schema parameter object for the provided type. The returned Schema only supports Newtonsoft.Json.
    /// If you need to use other Json libraries, do not use this method. Instead, use the SchemaGenerator class for that library.
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="fieldDescriptions">By providing key-value pairs, replace the text on the [Description] tag.</param>
    /// <returns>Returns Schema parameter object, call ToString() to get text.</returns>
    object CreateSchemaProperty(Type type, IDictionary<string, string> fieldDescriptions = null);


    T GetObject<T>(string jsonText) where T : class;

    object GetObject(Type type, string jsonText);


    /// <summary>
    /// Creates a Schema for the provided type. The returned Schema supports both Newtonsoft.Json and System.Text.Json serialization.
    /// If you need to use other Json libraries, first call ToString() to get json text.
    /// Under normal circumstances, do not use this method. Use the <see cref="CreateSchemaProperty"/> method instead, as different APIs may require different formats.
    /// </summary>
    /// <param name="objType"></param>
    /// <param name="ctx"></param>
    /// <param name="depth"></param>
    /// <param name="typeGuiding"></param>
    /// <returns></returns>
    IDataWritable CreateSchema(DCompond objType, SchemaGenerateOptions options = null);

    IDataWritable CreateSchema(SimpleType objType, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(DCompond objType, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(DAbstract objType, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(DStructField field, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(TypeDefinition type, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(SimpleType objType, SchemaGenerateOptions options = null);

    IDataWritable CreateSchemaProperty(SimpleField objType, SchemaGenerateOptions options = null);


    /// <summary>
    /// Gets the Schema text for type design
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <returns></returns>
    string GetTypeEditSchemeProperty(string nameSpace);
}

/// <summary>
/// Represents a Schema cache for a <see cref="DCompond"/>
/// </summary>
public class DCompondSchema
{
    private string _schema;

    /// <summary>
    /// Initializes a new instance of the DCompondSchema class.
    /// </summary>
    /// <param name="dataType">The compound data type.</param>
    public DCompondSchema(DCompond dataType)
    {
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
    }

    /// <summary>
    /// Gets the data type.
    /// </summary>
    public DCompond DataType { get; }

    /// <summary>
    /// Gets the cached schema string.
    /// </summary>
    /// <returns>The schema string.</returns>
    public string GetSchema() => _schema ??= EditorServices.JsonSchemaService.CreateSchema(DataType)?.ToString();
}