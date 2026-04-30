using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenAI_API.ChatFunctions
{
    /// <summary>
    ///     Function parameter is a JSON Schema object.
    ///     https://json-schema.org/understanding-json-schema/reference/object.html
    /// </summary>
    public class FunctionProperty
    {
        /// <summary>
        /// Funcion object type
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
        public string Type { get; set; } = "object";

        /// <summary>
        ///     Optional. List of "function arguments", as a dictionary that maps from argument name
        ///     to an object that describes the type, maybe possible enum values, and so on.
        /// </summary>
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, FunctionProperty> Properties { get; set; }

        /// <summary>
        ///     Optional. List of "function arguments" which are required.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Required { get; set; }

        /// <summary>
        ///     Optional. Whether additional properties are allowed. Default value is true.
        /// </summary>
        [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AdditionalProperties { get; set; }

        /// <summary>
        ///     Optional. Argument description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        ///     Optional. List of allowed values for this argument.
        /// </summary>
        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Enum { get; set; }

        /// <summary>
        ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
        ///     these must be a non-negative integer.
        /// </summary>
        [JsonProperty("minProperties", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinProperties { get; set; }

        /// <summary>
        ///     The number of properties on an object can be restricted using the minProperties and maxProperties keywords. Each of
        ///     these must be a non-negative integer.
        /// </summary>
        [JsonProperty("maxProperties", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxProperties { get; set; }

        /// <summary>
        ///     If type is "array", this specifies the element type for all items in the array.
        ///     If type is not "array", this should be null.
        ///     For more details, see https://json-schema.org/understanding-json-schema/reference/array.html
        /// </summary>
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public FunctionProperty Items { get; set; }

        /// <summary>
        /// Minimum value
        /// </summary>
        [JsonProperty("minimum", NullValueHandling = NullValueHandling.Ignore)]
        public float? Minimum { get; set; } 

        /// <summary>
        /// Maximum value
        /// </summary>
        [JsonProperty("maximum", NullValueHandling = NullValueHandling.Ignore)]
        public float? Maximum { get; set; }


        public static FunctionProperty DefineArray(FunctionProperty arrayItems = null)
        {
            return new FunctionProperty
            {
                Items = arrayItems,
                Type = ConvertTypeToString(FunctionObjectTypes.Array)
            };
        }

        public static FunctionProperty DefineEnum(List<string> enumList, string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Enum = enumList,
                Type = ConvertTypeToString(FunctionObjectTypes.String)
            };
        }

        public static FunctionProperty DefineInteger(string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Type = ConvertTypeToString(FunctionObjectTypes.Integer)
            };
        }

        public static FunctionProperty DefineNumber(string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Type = ConvertTypeToString(FunctionObjectTypes.Number)
            };
        }

        public static FunctionProperty DefineString(string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Type = ConvertTypeToString(FunctionObjectTypes.String)
            };
        }

        public static FunctionProperty DefineBoolean(string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Type = ConvertTypeToString(FunctionObjectTypes.Boolean)
            };
        }

        public static FunctionProperty DefineNull(string description = null)
        {
            return new FunctionProperty
            {
                Description = description,
                Type = ConvertTypeToString(FunctionObjectTypes.Null)
            };
        }

        public static FunctionProperty DefineObject(IDictionary<string, FunctionProperty>? properties, IList<string> required, bool? additionalProperties, string description, IList<string> @enum)
        {
            return new FunctionProperty
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
    }
}
