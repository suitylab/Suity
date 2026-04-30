using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ComputerBeacon.Json;

/// <summary>
/// Default JSON serializer and deserializer. Inherit this class to support of custom types
/// </summary>
public class Serializer
{
    private static readonly Serializer defaultSerializer = new Serializer();

    /// <summary>
    /// Serializes an object to its JSON representation
    /// </summary>
    /// <param name="o">object to be serialized</param>
    /// <returns>JSON string that represents the object</returns>
    public static string Serialize(object o)
    {
        return Serialize(o, defaultSerializer);
    }

    /// <summary>
    /// Serializes an object to its JSON representation
    /// </summary>
    /// <param name="o">object to be serialized</param>
    /// <param name="serializer">serializer to convert object instances to strings</param>
    /// <returns>JSON string representation of object</returns>
    public static string Serialize(object o, Serializer serializer)
    {
        CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        StringBuilder sb = new StringBuilder();
        serializer.SerializeObject(sb, o);

        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        //restore culture

        return sb.ToString();
    }

    /// <summary>
    /// Deserializes a JSON string to a specified type
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="jsonString">JSON string containing data</param>
    /// <returns></returns>
    public static T Deserialize<T>(string jsonString)
    {
        return (T)defaultSerializer.DeserializeObject(typeof(T), Parser.Parse(jsonString));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="targetType">Type to deserialize to</param>
    /// <param name="jsonString">JSON string containing data</param>
    /// <returns></returns>
    public static object Deserialize(Type targetType, string jsonString)
    {
        return defaultSerializer.DeserializeObject(targetType, Parser.Parse(jsonString));
    }

    /// <summary>
    /// Deserializes a JsonObject or JsonArray instance to the specified type
    /// </summary>
    /// <param name="targetType">Type to be serialized to</param>
    /// <param name="jsonString">JSON string containing data</param>
    /// <param name="serializer">Deserializer to convert instance types</param>
    /// <returns></returns>
    public static object Deserialize(Type targetType, string jsonString, Serializer serializer)
    {
        return serializer.DeserializeObject(targetType, Parser.Parse(jsonString));
    }

    /// <summary>
    /// Recursive method for serialization
    /// </summary>
    /// <param name="sb">StringBuilder to write to</param>
    /// <param name="o">object to serialize</param>
    public virtual void SerializeObject(StringBuilder sb, object o)
    {
        if (o == null)
        {
            sb.Append("null"); return;
        }

        Type type = o.GetType();

        if (type.IsArray)
        {
            if (type.GetArrayRank() != 1) throw new NotSupportedException("Only one-dimension arrays are supported");
            Array a = o as Array;
            sb.Append('[');
            for (int i = 0; i < a.Length; i++)
            {
                if (i != 0) sb.Append(',');
                SerializeObject(sb, a.GetValue(i));
            }
            sb.Append(']');
        }
        else if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            sb.Append('[');
            bool firstObject = false;
            foreach (var item in o as System.Collections.IEnumerable)
            {
                if (firstObject) sb.Append(',');
                SerializeObject(sb, item);
                firstObject = true;
            }
            sb.Append(']');
        }

        else if (type.IsPrimitive)
        {
            if (type == typeof(bool)) sb.Append((bool)o ? "true" : "false");
            else sb.Append(o.ToString());
        }
        else if (type.IsEnum)
        {
            sb.Append(o.ToString());
        }
        else if (type == typeof(string)) Stringifier.writeEscapedString(sb, o as string);
        else if (type.IsClass)
        {
            sb.Append('{');
            bool hasFirstValue = false;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                if (hasFirstValue) sb.Append(',');
                Stringifier.writeEscapedString(sb, fields[i].Name);
                sb.Append(':');
                SerializeObject(sb, fields[i].GetValue(o));
                hasFirstValue = true;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].CanRead) continue;
                if (hasFirstValue) sb.Append(',');
                Stringifier.writeEscapedString(sb, properties[i].Name);
                sb.Append(':');
                SerializeObject(sb, properties[i].GetValue(o, null));
                hasFirstValue = true;
            }

            sb.Append('}');
        }
        else throw new NotSupportedException("Cannot serialize type " + type.FullName);
    }

    /// <summary>
    /// Recursive method for deserialization
    /// </summary>
    /// <param name="targetType">Type to deserialize to</param>
    /// <param name="jsonValue">JSON value as parsed by parser</param>
    /// <returns>An object of the target type</returns>
    public virtual object DeserializeObject(Type targetType, object jsonValue)
    {
        if (jsonValue == null) return null;
        if (targetType.IsInterface) throw new InvalidOperationException("Cannot deserialize to interface type");
        if (targetType == typeof(object) || targetType == typeof(string)) return jsonValue;
        if (targetType.IsPrimitive) return Convert.ChangeType(jsonValue, targetType, System.Globalization.CultureInfo.InvariantCulture);

        if (targetType.IsArray)
        {
            if (targetType.GetArrayRank() != 1) throw new NotSupportedException("Only one-dimension arrays are supported");
            var ja = jsonValue as JsonArray;
            if (ja == null) throw new InvalidCastException("Cannot deserialize non-JsonArray value \"" + targetType.ToString() + "\" to an array");

            var array = Array.CreateInstance(targetType.GetElementType(), ja.Count);
            for (int i = 0; i < ja.Count; i++) array.SetValue(DeserializeObject(targetType.GetElementType(), ja[i]), i);
            return array;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var ja = jsonValue as JsonArray;
            if (ja == null) throw new InvalidCastException("Cannot deserialize non-JsonArray value \"" + targetType.ToString() + "\" to List<>");

            var list = Activator.CreateInstance(targetType);
            var m = targetType.GetMethod("Add");
            var elementType = targetType.GetGenericArguments()[0];
            foreach (var v in ja) m.Invoke(list, new object[] { DeserializeObject(elementType, v) });
            return list;
        }

        if (targetType.IsClass)
        {
            JsonObject jo = jsonValue as JsonObject;
            if (jo == null) throw new InvalidCastException("Cannot deserialize non-JsonObject value \"" + jsonValue.ToString() + "\" to a class instance of type " + targetType.Name);
            var target = Activator.CreateInstance(targetType);

            var fields = targetType.GetFields();
            foreach (var f in fields)
            {
                if (!jo.ContainsKey(f.Name)) continue;
                f.SetValue(target, DeserializeObject(f.FieldType, jo[f.Name]));
            }
            var properties = targetType.GetProperties();
            foreach (var p in properties)
            {
                if (!jo.ContainsKey(p.Name) || !p.CanWrite) continue;
                p.SetValue(target, DeserializeObject(p.PropertyType, jo[p.Name]), null);
            }
            return target;
        }

        throw new NotSupportedException("Cannot deserialize to type " + targetType.FullName);
    }
}