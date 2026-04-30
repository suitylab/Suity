using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.NodeQuery;

/// <summary>
/// Reads hierarchical node data using Newtonsoft.Json (JObject/JArray).
/// </summary>
public class NewtonJsonNodeReader : MarshalByRefObject, INodeReader
{
    private readonly JToken _token;
    private readonly string _nodeName;

    /// <summary>
    /// Initializes a new instance wrapping the specified JToken.
    /// </summary>
    /// <param name="token">The underlying JSON token to read from.</param>
    /// <param name="nodeName">Optional name for this node.</param>
    public NewtonJsonNodeReader(JToken token, string nodeName = null)
    {
        _token = token;
        _nodeName = nodeName;
    }

    /// <inheritdoc/>
    public bool Exist => _token != null;

    /// <inheritdoc/>
    public string NodeName => _nodeName ?? string.Empty;

    /// <inheritdoc/>
    public int ChildCount
    {
        get
        {
            // Count properties for objects, elements for arrays
            if (_token is JObject obj)
                return obj.Count;
            else if (_token is JArray arr)
                return arr.Count;
            else
                return 0;
        }
    }

    /// <inheritdoc/>
    public string NodeValue
    {
        get
        {
            if (_token == null)
                return string.Empty;
            if (_token is JValue jv)
                return jv.ToString();
            else if (_token is JObject || _token is JArray)
                return _token.ToString(Formatting.None);
            else
                return string.Empty;
        }
    }

    /// <inheritdoc/>
    public object NodeValueObj => _token?.Value<object>();

    /// <inheritdoc/>
    public INodeReader Node(int index)
    {
        // Access array element by index
        if (_token is JArray arr && index >= 0 && index < arr.Count)
            return new NewtonJsonNodeReader(arr[index], index.ToString());
        return EmptyNodeReader.Empty;
    }

    /// <inheritdoc/>
    public INodeReader Node(string name)
    {
        // Access child by property name, or by index if the name is numeric
        if (_token is JObject obj && obj.TryGetValue(name, out JToken value))
            return new NewtonJsonNodeReader(value, name);
        else if (_token is JArray arr && int.TryParse(name, out int index) && index >= 0 && index < arr.Count)
            return new NewtonJsonNodeReader(arr[index], name);
        return EmptyNodeReader.Empty;
    }

    /// <inheritdoc/>
    public IEnumerable<INodeReader> Nodes(string name)
    {
        // Yield the single child matching the given name or index
        if (_token is JObject obj && obj.TryGetValue(name, out JToken value))
        {
            yield return new NewtonJsonNodeReader(value, name);
        }
        else if (_token is JArray arr && int.TryParse(name, out int index) && index >= 0 && index < arr.Count)
        {
            yield return new NewtonJsonNodeReader(arr[index], name);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<INodeReader> Nodes()
    {
        // Enumerate all child nodes (properties for objects, elements for arrays)
        if (_token is JObject obj)
        {
            foreach (var prop in obj.Properties())
                yield return new NewtonJsonNodeReader(prop.Value, prop.Name);
        }
        else if (_token is JArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
                yield return new NewtonJsonNodeReader(arr[i], i.ToString());
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> NodeNames
    {
        get
        {
            // Return property names for objects, or stringified indices for arrays
            if (_token is JObject obj)
                return obj.Properties().Select(p => p.Name);
            else if (_token is JArray arr)
                return Enumerable.Range(0, arr.Count).Select(i => i.ToString());
            else
                return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc/>
    public IEnumerable<KeyValuePair<string, string>> Attributes
    {
        get
        {
            // Yield all properties as key-value pairs
            if (_token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    string val = prop.Value.ToString(Formatting.None);
                    yield return new KeyValuePair<string, string>(prop.Name, val);
                }
            }
            else
            {
                yield break;
            }
        }
    }

    /// <inheritdoc/>
    public string GetAttribute(string name)
    {
        // Retrieve a single property value by name
        if (_token is JObject obj && obj.TryGetValue(name, out JToken value))
            return value.ToString(Formatting.None);
        return null;
    }
}
