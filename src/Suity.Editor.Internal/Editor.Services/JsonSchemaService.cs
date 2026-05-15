using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Json;
using Suity.NodeQuery;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Suity.Editor.Services;

#region JsonSchemaService

/// <summary>
/// Service for generating JSON schemas from types and type definitions, and deserializing JSON objects.
/// </summary>
internal class JsonSchemaService : IJsonSchemaService
{
    /// <summary>
    /// Default depth limit for schema generation.
    /// </summary>
    public const int DEFAULT_DEPTH = 10;

    /// <summary>
    /// Singleton instance of the JSON schema service.
    /// </summary>
    public static JsonSchemaService Instance { get; } = new();

    private JsonSchemaService()
    {
    }

    /// <inheritdoc/>
    public object CreateSchema(Type type, IDictionary<string, string> fieldDescriptions = null)
    {
        var rootObj = new JObject();
        rootObj["name"] = type.Name;
        if (type.GetAttributeCached<DescriptionAttribute>()?.Description is string description && !string.IsNullOrWhiteSpace(description))
        {
            rootObj["description"] = description;
        }

        rootObj["schema"] = _CreateSchemaProperty(type, fieldDescriptions);

        return rootObj;
    }

    /// <inheritdoc/>
    public object CreateSchemaProperty(Type type, IDictionary<string, string> fieldDescriptions = null)
        => _CreateSchemaProperty(type, fieldDescriptions);

    /// <inheritdoc/>
    public T GetObject<T>(string jsonText) where T : class
    {
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<T>(jsonText);
    }

    /// <inheritdoc/>
    public object GetObject(Type type, string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return null;
        }

        return JsonConvert.DeserializeObject(jsonText, type);
    }


    public SimpleType GetViewObjectSimpleType(IViewObject viewObject)
    {
        var setup = new GetFieldSetup();
        viewObject.SetupView(setup);

        List<SimpleField> fields = [];
        foreach (var fieldInfo in setup.Fields)
        {
            var prop = fieldInfo.Property;
            if (prop.ReadOnly)
            {
                continue;
            }

            var typeDef = TypeDefinition.FromNative(fieldInfo.Type);
            var field = new SimpleField
            {
                Name = prop.Name,
                Description = prop.Description,
                Tooltips = prop.Styles.GetToolTips(),
                Type = typeDef,
                Optional = prop.Optional,
                Range = prop.Attributes?.GetAttribute<NumericRangeAttribute>(),
                Selection = prop.Attributes?.GetAttribute<SelectionDesignAttribute>(),
            };

            fields.Add(field);
        }

        var simpleType = new SimpleType
        {
            Name = viewObject.GetType().FullName,
            Description = viewObject.ToDisplayTextL(),
            Tooltips = viewObject.ToToolTipsTextL(),
            Fields = [.. fields],
        };

        return simpleType;
    }

    /// <inheritdoc/>
    public IDataWritable CreateSchema(DCompond type, SchemaGenerateOptions options = null)
    {
        var prop = _CreateSchemaProperty(type, options?.Depth ?? DEFAULT_DEPTH, options);
        string desc = type.GetAttribute<ToolTipsAttribute>()?.ToolTips;
        if (string.IsNullOrWhiteSpace(desc))
        {
            desc = null;
        }

        return new ObjectSchema(type.Name, desc, prop);
    }

    /// <inheritdoc/>
    public IDataWritable CreateSchema(SimpleType type, SchemaGenerateOptions options = null)
    {
        var prop = _CreateSchemaProperty(type, options?.Depth ?? DEFAULT_DEPTH, options);
        string desc = type.Tooltips;
        if (string.IsNullOrWhiteSpace(desc))
        {
            desc = null;
        }

        return new ObjectSchema(type.Name, desc, prop);
    }

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(DCompond objType, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(objType, options?.Depth ?? DEFAULT_DEPTH, options);

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(DAbstract objType, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(objType, options?.Depth ?? DEFAULT_DEPTH, options);

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(DStructField field, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(field, options?.Depth ?? DEFAULT_DEPTH, options);

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(TypeDefinition type, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(type, options?.Depth ?? DEFAULT_DEPTH, null, options);

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(SimpleType type, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(type, options?.Depth ?? DEFAULT_DEPTH, options);

    /// <inheritdoc/>
    public IDataWritable CreateSchemaProperty(SimpleField field, SchemaGenerateOptions options = null)
        => _CreateSchemaProperty(field, options?.Depth ?? DEFAULT_DEPTH, options);

    /// <inheritdoc/>
    public string GetTypeEditSchemeProperty(string nameSpace)
        => Suity.Properties.Resources.SchemaTypeEdit.Replace("{{NameSpace}}", nameSpace ?? "NameSpace");


    /// <summary>
    /// Creates a JSON schema property from a .NET type using JSchemaGenerator.
    /// </summary>
    /// <param name="type">The .NET type to generate schema for.</param>
    /// <param name="fieldDescriptions">Optional dictionary mapping field names to descriptions.</param>
    /// <returns>A JObject representing the JSON schema.</returns>
    private JObject _CreateSchemaProperty(Type type, IDictionary<string, string> fieldDescriptions = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }


        var generator = new JSchemaGenerator
        {
            SchemaReferenceHandling = SchemaReferenceHandling.None,
        };

        generator.GenerationProviders.Add(new StringEnumGenerationProvider());

        var schema = generator.Generate(type);

        // Convert JSchema to JObject
        JObject jObject = JObject.FromObject(schema);

        // Perform replacement operation
        if (fieldDescriptions != null)
        {
            ReplaceStringValues(jObject, fieldDescriptions);
        }

        return jObject;
    }


    /// <summary>
    /// Creates a schema property from a compound type definition.
    /// </summary>
    /// <param name="objType">The compound type definition.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(DCompond objType, int depth, SchemaGenerateOptions options = null)
    {
        if (objType is null)
        {
            return null;
        }

        var prop = new ObjectSchemaProperty
        {
            Properties = new Dictionary<string, ObjectSchemaProperty>(),
            Required = [],
            Description = options?.Description ?? objType.Description,
            Type = ObjectSchemaProperty.ConvertTypeToString(ObjectSchemaProperty.FunctionObjectTypes.Object)
        };

        bool typeGuiding = options?.TypeGuiding == true;

        ObjectSchemaProperty typeFieldProp = null;
        if (objType.Definition == NativeTypes.ObjectType || objType.Definition.IsAbstract)
        {
            typeFieldProp = ObjectSchemaProperty.DefineString($"Please fill in the name of this type");
        }
        else if (typeGuiding)
        {
            typeFieldProp = ObjectSchemaProperty.DefineString($"Please fill in '{objType.FullTypeName}'");
        }

        if (typeFieldProp != null && typeGuiding)
        {
            prop.Properties["@type"] = typeFieldProp;
            prop.Required.Add("@type");
        }

        foreach (var field in objType.PublicStructFields.Where(o => !o.IsHiddenOrDisabled))
        {
            ObjectSchemaProperty fieldProp = _CreateSchemaProperty(field, depth, options);
            if (fieldProp is null)
            {
                continue;
            }

            // Value range
            if (field.GetAttribute<NumericRangeAttribute>() is { } range)
            {
                fieldProp.Minimum = (float)range.Min;
                fieldProp.Maximum = (float)range.Max;

                fieldProp.Description ??= string.Empty;
                fieldProp.Description += $" (Range: {range.Min} ~ {range.Max})";
            }

            prop.Properties[field.Name] = fieldProp;
            if (!field.Optional)
            {
                prop.Required.Add(field.Name);
            }
        }

        return prop;
    }

    /// <summary>
    /// Creates a schema property from an abstract type definition.
    /// </summary>
    /// <param name="objType">The abstract type definition.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(DAbstract objType, int depth, SchemaGenerateOptions options = null)
    {
        if (objType is null)
        {
            return null;
        }

        var prop = new ObjectSchemaProperty
        {
            Properties = new Dictionary<string, ObjectSchemaProperty>(),
            Required = [],
            Description = options?.Description,
            Type = ObjectSchemaProperty.ConvertTypeToString(ObjectSchemaProperty.FunctionObjectTypes.Object)
        };

        if (options?.TypeGuiding == true)
        {
            var typeFieldProp = ObjectSchemaProperty.DefineString($"Please fill in the name of this type");
            prop.Properties["@type"] = typeFieldProp;
            prop.Required.Add("@type");
        }

        return prop;
    }

    /// <summary>
    /// Creates a schema property from a struct field definition.
    /// </summary>
    /// <param name="field">The struct field.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(DStructField field, int depth, SchemaGenerateOptions options = null)
    {
        var type = field.FieldType;

        if (options?.SimpleFieldOnly == true)
        {
            if (!field.SupportSimpleGeneration())
            {
                return null;
            }
        }
        else
        {
            if (!field.SupportGeneration())
            {
                return null;
            }
        }

        if (options?.ExcludedFields is { } exFields && exFields.Contains(field))
        {
            return null;
        }

        string desc = field.ToolTips;
        if (string.IsNullOrWhiteSpace(desc))
        {
            desc = field.Description;
        }

        // Check auto enum
        if (type == NativeTypes.StringType && field.GetAttribute<SelectionDesignAttribute>() is { } s)
        {
            var list = s.GetSelectionList(options?.Context).GetItems().Select(o => o.SelectionKey).ToList();

            return ObjectSchemaProperty.DefineEnum(list, desc);
        }

        return _CreateSchemaProperty(type, depth, desc, options);
    }

    /// <summary>
    /// Creates a schema property from a type definition.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="description">Optional description for the property.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(TypeDefinition type, int depth, string description = null, SchemaGenerateOptions options = null)
    {
        if (TypeDefinition.IsNullOrBroken(type))
        {
            return null;
        }

        depth--;
        if (depth < 0)
        {
            return null;
        }

        description ??= options?.Description ?? string.Empty;

        ObjectSchemaProperty fieldProp = null;

        if (type == NativeTypes.BooleanType)
        {
            fieldProp = ObjectSchemaProperty.DefineBoolean(description);
        }
        else if (type == NativeTypes.StringType)
        {
            fieldProp = ObjectSchemaProperty.DefineString(description);
        }
        else if (type == NativeTypes.TextBlockType)
        {
            fieldProp = ObjectSchemaProperty.DefineString(description);
        }
        else if (type.IsNumeric)
        {
            if ((type.Target as DPrimative)?.TypeCode.GetIsInteger() == true)
            {
                fieldProp = ObjectSchemaProperty.DefineInteger(description);
            }
            else
            {
                fieldProp = ObjectSchemaProperty.DefineNumber(description);
            }
        }
        else if (type.IsEnum && type.Target is DEnum e)
        {
            fieldProp = ObjectSchemaProperty.DefineEnum(e.EnumFields.Select(o => o.Name).ToList(), description);
        }
        else if (type.IsStruct && type.Target is DCompond s)
        {
            fieldProp = _CreateSchemaProperty(s, depth, options);
        }
        else if (type.IsAbstractStruct && type.Target is DAbstract abs)
        {
            fieldProp = _CreateSchemaProperty(abs, depth, options);
        }
        else if (type.IsArray)
        {
            fieldProp = ObjectSchemaProperty.DefineArray(_CreateSchemaProperty(type.ElementType, depth, description, options));
            // Transfer description
            if (fieldProp.Items != null)
            {
                fieldProp.Description = fieldProp.Items.Description;
                fieldProp.Items.Description = null;
            }
            else
            {
                fieldProp.Description = description;
            }
        }
        else if (type.IsLink)
        {
            // Use string to replace link field
            fieldProp = ObjectSchemaProperty.DefineString(description);
        }

        return fieldProp;
    }

    /// <summary>
    /// Creates a schema property from a simple type definition.
    /// </summary>
    /// <param name="objType">The simple type.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(SimpleType objType, int depth, SchemaGenerateOptions options = null)
    {
        if (objType is null)
        {
            return null;
        }

        var prop = new ObjectSchemaProperty
        {
            Properties = new Dictionary<string, ObjectSchemaProperty>(),
            Required = [],
            Description = options?.Description ?? objType.Tooltips,
            Type = ObjectSchemaProperty.ConvertTypeToString(ObjectSchemaProperty.FunctionObjectTypes.Object)
        };

        bool typeGuiding = options?.TypeGuiding == true;

        ObjectSchemaProperty typeFieldProp = null;
        if (typeGuiding)
        {
            typeFieldProp = ObjectSchemaProperty.DefineString($"Please fill in '{objType.Name}'");
        }

        if (typeFieldProp != null && typeGuiding)
        {
            prop.Properties["@type"] = typeFieldProp;
            prop.Required.Add("@type");
        }

        foreach (var field in objType.Fields ?? [])
        {
            ObjectSchemaProperty fieldProp = _CreateSchemaProperty(field, depth, options);
            if (fieldProp is null)
            {
                continue;
            }

            // Value range
            if (field.Range is { } range)
            {
                fieldProp.Minimum = (float)range.Min;
                fieldProp.Maximum = (float)range.Max;

                fieldProp.Description ??= string.Empty;
                fieldProp.Description += $" (Range: {range.Min} ~ {range.Max})";
            }

            prop.Properties[field.Name] = fieldProp;
            if (!field.Optional)
            {
                prop.Required.Add(field.Name);
            }
        }

        return prop;
    }

    /// <summary>
    /// Creates a schema property from a simple field definition.
    /// </summary>
    /// <param name="field">The simple field.</param>
    /// <param name="depth">Remaining recursion depth.</param>
    /// <param name="options">Schema generation options.</param>
    /// <returns>The generated schema property.</returns>
    private ObjectSchemaProperty _CreateSchemaProperty(SimpleField field, int depth, SchemaGenerateOptions options = null)
    {
        var type = field.Type;


        string desc = field.Tooltips;

        // Check auto enum
        if (type == NativeTypes.StringType && field.Selection is { } s)
        {
            var list = s.GetSelectionList(options?.Context).GetItems().Select(o => o.SelectionKey).ToList();

            return ObjectSchemaProperty.DefineEnum(list, desc);
        }

        return _CreateSchemaProperty(type, depth, desc, options);
    }


    /// <summary>
    /// Recursively finds and replaces string values in a JObject based on a replacement dictionary.
    /// </summary>
    /// <param name="obj">The JObject to process.</param>
    /// <param name="replacements">Dictionary of string values to replace.</param>
    private static void ReplaceStringValues(JObject obj, IDictionary<string, string> replacements)
    {
        foreach (var property in obj.Properties())
        {
            if (property.Value.Type == JTokenType.String)
            {
                // Get current string value
                string value = property.Value.ToString();

                // If there is a matching value in the dictionary, perform replacement
                if (replacements.ContainsKey(value))
                {
                    property.Value = replacements[value];
                }
            }
            else if (property.Value.Type == JTokenType.Object)
            {
                // If value is JObject type, recursive call
                ReplaceStringValues((JObject)property.Value, replacements);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                // If value is JArray type, recursively iterate array
                foreach (var item in (JArray)property.Value)
                {
                    if (item.Type == JTokenType.String)
                    {
                        string value = item.ToString();
                        if (replacements.ContainsKey(value))
                        {
                            item.Replace(replacements[value]);
                        }
                    }
                    else if (item.Type == JTokenType.Object)
                    {
                        ReplaceStringValues((JObject)item, replacements);
                    }
                }
            }
        }
    }


} 
#endregion

#region ObjectSchemaProperty

/// <summary>
///     Function parameter is a JSON Schema object.
///     https://json-schema.org/understanding-json-schema/reference/object.html
/// </summary>
public class ObjectSchemaProperty : IDataWritable
{
    /// <summary>
    /// Function object type enumeration for JSON Schema.
    /// </summary>
    public enum FunctionObjectTypes
    {
        String,
        Integer,
        Number,
        Object,
        Array,
        Boolean,
        Null
    }

    /// <summary>
    ///     Required. Function parameter object type. Default value is "object".
    /// </summary>
    [JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
    [System.Text.Json.Serialization.JsonPropertyName("type")]
    [System.Text.Json.Serialization.JsonRequired]
    public string Type { get; set; } = "object";

    /// <summary>
    ///     Optional. List of "function arguments", as a dictionary that maps from argument name
    ///     to an object that describes the type, maybe possible enum values, and so on.
    /// </summary>
    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("properties")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, ObjectSchemaProperty> Properties { get; set; }

    /// <summary>
    ///     Optional. List of "function arguments" which are required.
    /// </summary>
    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("required")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IList<string> Required { get; set; }

    /// <summary>
    ///     Optional. Whether additional properties are allowed. Default value is true.
    /// </summary>
    [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("additionalProperties")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public bool? AdditionalProperties { get; set; }

    /// <summary>
    ///     Optional. Argument description.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    ///     Optional. List of allowed values for this argument.
    /// </summary>
    [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("enum")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IList<string> Enum { get; set; }

    /// <summary>
    ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
    ///     these must be a non-negative integer.
    /// </summary>
    [JsonProperty("minProperties", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("minProperties")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? MinProperties { get; set; }

    /// <summary>
    ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
    ///     these must be a non-negative integer.
    /// </summary>
    [JsonProperty("maxProperties", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("maxProperties")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxProperties { get; set; }

    /// <summary>
    ///     If type is "array", this specifies the element type for all items in the array.
    ///     If type is not "array", this should be null.
    ///     For more details, see https://json-schema.org/understanding-json-schema/reference/array.html
    /// </summary>
    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("items")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public ObjectSchemaProperty Items { get; set; }

    /// <summary>
    /// Minimum value for numeric types.
    /// </summary>
    [JsonProperty("minimum", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("minimum")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public float? Minimum { get; set; }

    /// <summary>
    /// Maximum value for numeric types.
    /// </summary>
    [JsonProperty("maximum", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public float? Maximum { get; set; }


    /// <inheritdoc/>
    public void WriteData(IDataWriter writer)
    {
        writer.Node("type").WriteString(Type);

        if (Properties?.Count > 0)
        {
            var propsWriter = writer.Node("properties");
            foreach (var pair in Properties)
            {
                var propWriter = propsWriter.Node(pair.Key);
                pair.Value.WriteData(propWriter);
            }
        }

        if (Required?.Count > 0)
        {
            var aryWriter = writer.Nodes("required", Required.Count);
            foreach (var required in Required)
            {
                aryWriter.Item().WriteString(required);
            }
            aryWriter.Finish();
        }

        if (AdditionalProperties is { } addProp)
        {
            writer.Node("additionalProperties").WriteBoolean(addProp);
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            writer.Node("description").WriteString(Description);
        }

        if (Enum?.Count > 0)
        {
            var aryWriter = writer.Nodes("enum", Enum.Count);
            foreach (var en in Enum)
            {
                aryWriter.Item().WriteString(en);
            }
            aryWriter.Finish();
        }

        if (MinProperties is { } minProp)
        {
            writer.Node("minProperties").WriteInt32(minProp);
        }

        if (MaxProperties is { } maxProp)
        {
            writer.Node("maxProperties").WriteInt32(maxProp);
        }

        if (Items != null)
        {
            var itemWriter = writer.Node("items");
            Items.WriteData(itemWriter);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var jsonWriter = new JsonDataWriter();
        WriteData(jsonWriter);
        return jsonWriter.ToString();
    }

    #region static

    /// <summary>
    /// Creates a schema property for an array type.
    /// </summary>
    /// <param name="arrayItems">The schema for array items.</param>
    /// <returns>A new array schema property.</returns>
    public static ObjectSchemaProperty DefineArray(ObjectSchemaProperty arrayItems = null)
    {
        return new ObjectSchemaProperty
        {
            Items = arrayItems,
            Type = ConvertTypeToString(FunctionObjectTypes.Array)
        };
    }

    /// <summary>
    /// Creates a schema property for an enum type with allowed values.
    /// </summary>
    /// <param name="enumList">List of allowed enum values.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A new enum schema property.</returns>
    public static ObjectSchemaProperty DefineEnum(List<string> enumList, string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Enum = enumList,
            Type = ConvertTypeToString(FunctionObjectTypes.String)
        };
    }

    /// <summary>
    /// Creates a schema property for an integer type.
    /// </summary>
    /// <param name="description">Optional description.</param>
    /// <returns>A new integer schema property.</returns>
    public static ObjectSchemaProperty DefineInteger(string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Integer)
        };
    }

    /// <summary>
    /// Creates a schema property for a number (floating-point) type.
    /// </summary>
    /// <param name="description">Optional description.</param>
    /// <returns>A new number schema property.</returns>
    public static ObjectSchemaProperty DefineNumber(string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Number)
        };
    }

    /// <summary>
    /// Creates a schema property for a string type.
    /// </summary>
    /// <param name="description">Optional description.</param>
    /// <returns>A new string schema property.</returns>
    public static ObjectSchemaProperty DefineString(string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.String)
        };
    }

    /// <summary>
    /// Creates a schema property for a boolean type.
    /// </summary>
    /// <param name="description">Optional description.</param>
    /// <returns>A new boolean schema property.</returns>
    public static ObjectSchemaProperty DefineBoolean(string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Boolean)
        };
    }

    /// <summary>
    /// Creates a schema property for a null type.
    /// </summary>
    /// <param name="description">Optional description.</param>
    /// <returns>A new null schema property.</returns>
    public static ObjectSchemaProperty DefineNull(string description = null)
    {
        return new ObjectSchemaProperty
        {
            Description = description,
            Type = ConvertTypeToString(FunctionObjectTypes.Null)
        };
    }

    /// <summary>
    /// Creates a schema property for an object type with specified properties.
    /// </summary>
    /// <param name="properties">Dictionary of property names to schema properties.</param>
    /// <param name="required">List of required property names.</param>
    /// <param name="additionalProperties">Whether additional properties are allowed.</param>
    /// <param name="description">Object description.</param>
    /// <param name="enum">List of allowed values.</param>
    /// <returns>A new object schema property.</returns>
    public static ObjectSchemaProperty DefineObject(IDictionary<string, ObjectSchemaProperty> properties, IList<string> required, bool? additionalProperties, string description, IList<string> @enum)
    {
        return new ObjectSchemaProperty
        {
            Properties = properties,
            Required = required,
            AdditionalProperties = additionalProperties,
            Description = description,
            Enum = @enum,
            Type = ConvertTypeToString(FunctionObjectTypes.Object)
        };
    }

    /// <summary>
    ///     Converts a FunctionObjectTypes enumeration value to its corresponding string representation.
    /// </summary>
    /// <param name="type">The type to convert</param>
    /// <returns>The string representation of the given type</returns>
    public static string ConvertTypeToString(FunctionObjectTypes type)
    {
        return type switch
        {
            FunctionObjectTypes.String => "string",
            FunctionObjectTypes.Integer => "integer",
            FunctionObjectTypes.Number => "number",
            FunctionObjectTypes.Object => "object",
            FunctionObjectTypes.Array => "array",
            FunctionObjectTypes.Boolean => "boolean",
            FunctionObjectTypes.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown type: {type}")
        };
    }

    #endregion
}
#endregion

#region ObjectSchema

/// <summary>
/// Represents a Function object for the OpenAI API.
/// A Function contains information about the function to be called, its description and parameters.
/// </summary>
/// <remarks>
/// The 'Name' property represents the name of the function and must consist of alphanumeric characters, underscores, or dashes, with a maximum length of 64.
/// The 'Description' property is an optional field that provides a brief explanation about what the function does.
/// The 'Parameters' property describes the parameters that the function accepts, which are represented as a JSON Schema object. 
/// Various types of input are acceptable for the 'Parameters' property, such as a JObject, a Dictionary of string and object, an anonymous object, or any other serializable object. 
/// If the object is not a JObject, it will be converted into a JObject. 
/// Refer to the 'Parameters' property setter for more details.
/// Refer to the OpenAI API <see href="https://platform.openai.com/docs/guides/gpt/function-calling">guide</see> and the 
/// JSON Schema <see href="https://json-schema.org/understanding-json-schema/">reference</see> for more details on the format of the parameters.
/// </remarks>
public class ObjectSchema : IDataWritable
{
    /// <summary>
    /// The name of the function to be called. Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 64.
    /// </summary>
    [JsonProperty("name", Required = Required.Always)]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    [System.Text.Json.Serialization.JsonRequired]
    public string Name { get; set; }

    /// <summary>
    /// The description of what the function does.
    /// </summary>
    [JsonProperty("description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    /// <summary>
    /// The parameters that the function accepts, described as a JSON Schema object.
    /// The JSON Schema defines the type and structure of the data. It should be compatible with the JSON Schema standard.
    /// This property can accept values in various forms which can be serialized into a JSON format:
    /// 1. A JSON string, which will be parsed into a JObject.
    /// 2. A JObject, which represents a JSON object, is assigned directly.
    /// 3. A Dictionary of string and object, where keys are property names and values are their respective data.
    /// 4. An anonymous object, which gets converted into a JObject.
    /// 5. Any other object that can be serialized into a JSON format, which will be converted into a JObject.
    /// If the value cannot be converted into a JSON object, an exception will be thrown.
    /// Refer to the <see href="https://platform.openai.com/docs/guides/gpt/function-calling">guide</see> for examples and the 
    /// <see href="https://json-schema.org/understanding-json-schema/">JSON Schema reference</see> for detailed documentation about the format.
    /// </summary>
    [JsonProperty("schema", Required = Required.Default)]
    [System.Text.Json.Serialization.JsonPropertyName("schema")]
    public ObjectSchemaProperty Schema { get; set; }

    /// <summary>
    /// Creates a function schema with the specified name, description, and schema property.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="description">The function description.</param>
    /// <param name="schemaProperty">The schema property defining parameters.</param>
    public ObjectSchema(string name, string description, ObjectSchemaProperty schemaProperty)
    {
        Name = name;
        Description = description;
        Schema = schemaProperty;
    }

    /// <summary>
    /// Creates an empty Function object.
    /// </summary>
    public ObjectSchema()
    {
    }



    /// <inheritdoc/>
    public void WriteData(IDataWriter writer)
    {
        writer.Node("name").WriteString(Name);

        if (!string.IsNullOrWhiteSpace(Description))
        {
            writer.Node("description").WriteString(Description);
        }

        if (Schema != null)
        {
            var paramWriter = writer.Node("schema");
            Schema.WriteData(paramWriter);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var jsonWriter = new JsonDataWriter();
        WriteData(jsonWriter);
        return jsonWriter.ToString();
    }
}
#endregion

#region GetFieldSetup

class GetFieldSetup : IViewObjectSetup
{
    public record FieldInfo(Type Type, ViewProperty Property);

    readonly List<FieldInfo> _fields = [];

    public GetFieldSetup()
    {
    }

    public FieldInfo[] Fields => _fields.ToArray();


    #region IViewObjectSetup
    public INodeReader Styles => EmptyNodeReader.Empty;

    public object Parent => null;

    public void AddField(Type type, ViewProperty property)
    {
        var typeDef = TypeDefinition.FromNative(type);
        if (TypeDefinition.IsNullOrEmpty(typeDef))
        {
            return;
        }

        var field = new FieldInfo(type, property);
    }

    public IEnumerable<object> GetObjects() => [];

    public object GetService(Type serviceType) => null;

    public bool IsTypeSupported(Type type) => true;

    public bool IsViewIdSupported(int viewId) => viewId == ViewIds.Inspector; 
    #endregion
}

#endregion
