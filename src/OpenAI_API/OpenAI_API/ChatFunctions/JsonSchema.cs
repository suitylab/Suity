using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAI_API.ChatFunctions
{
    /// <summary>
    /// A schema for a structured response.
    /// </summary>
    public class JsonSchema
    {
        /// <summary>
        /// The name of the function to be called. Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 64.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The description of what the function does.
        /// </summary>
        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("schema", Required = Required.Default)]
        public object Schema { get; set; }


        /// <summary>
        /// Create a json schema which can be applied to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="schema"></param>
        public JsonSchema(string name, string description, object schema)
        {
            this.Name = name;
            this.Description = description;

            if (schema is string s)
            {
                this.Schema = JToken.Parse(s);
            }
            else
            {
                this.Schema = schema;
            }
        }

        /// <summary>
        /// Creates an empty JsonSchema object.
        /// </summary>
        public JsonSchema()
        {
        }
    }
}
