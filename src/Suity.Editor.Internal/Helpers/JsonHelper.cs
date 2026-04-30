using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for JSON serialization and deserialization with type name shortening.
/// Uses Newtonsoft.Json internally.
/// </summary>
public static class JsonHelper
{
    private class ShortTypeBinder : ISerializationBinder
    {
        private readonly Assembly _assembly;

        public ShortTypeBinder(Assembly assembly)
        {
            _assembly = assembly;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Serializes an object to a formatted JSON string with shortened type names.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A formatted JSON string representation of the object.</returns>
    public static string Serialize<T>(T obj)
    {
        var setting = new JsonSerializerSettings
        {
            SerializationBinder = new ShortTypeBinder(typeof(T).Assembly)
        };

        return JsonConvert.SerializeObject(obj, Formatting.Indented, setting);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of the specified type with shortened type name support.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public static T Deserialize<T>(string value)
    {
        var setting = new JsonSerializerSettings
        {
            SerializationBinder = new ShortTypeBinder(typeof(T).Assembly)
        };

        return JsonConvert.DeserializeObject<T>(value);
    }
}
