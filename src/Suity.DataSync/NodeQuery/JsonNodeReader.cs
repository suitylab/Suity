using ComputerBeacon.Json;
using System;
using System.Collections.Generic;

namespace Suity.NodeQuery;

/// <summary>
/// INodeReader implementation for reading from JSON objects
/// </summary>
public class JsonNodeReader : MarshalByRefObject, INodeReader
{
    private readonly object _data;
    private readonly string _nodeName;

    public JsonNodeReader(object data, string nodeName = null)
    {
        _data = data;
        _nodeName = nodeName;
    }

    public bool Exist => _data != null;

    public string NodeName => _nodeName ?? string.Empty;

    public int ChildCount
    {
        get
        {
            if (_data is JsonObject obj)
                return obj.Count;
            else if (_data is JsonArray arr)
                return arr.Count;
            else
                return 0;
        }
    }

    public string NodeValue
    {
        get
        {
            if (_data is string str)
                return str;
            else if (_data is int || _data is double || _data is bool)
                return _data.ToString();
            else if (_data is JsonObject || _data is JsonArray)
                return ((IJsonContainer)_data).ToString();
            else
                return string.Empty;
        }
    }

    public object NodeValueObj => _data;

    public INodeReader Node(int index)
    {
        if (_data is JsonArray arr && index >= 0 && index < arr.Count)
            return new JsonNodeReader(arr[index], index.ToString());
        return EmptyNodeReader.Empty;
    }

    public INodeReader Node(string name)
    {
        if (_data is JsonObject obj && obj.ContainsKey(name))
            return new JsonNodeReader(obj[name], name);
        return EmptyNodeReader.Empty;
    }

    public IEnumerable<INodeReader> Nodes(string name)
    {
        if (_data is JsonObject obj && obj.ContainsKey(name))
        {
            yield return new JsonNodeReader(obj[name], name);
        }
        else if (_data is JsonArray arr)
        {
            if (int.TryParse(name, out int index) && index >= 0 && index < arr.Count)
                yield return new JsonNodeReader(arr[index], index.ToString());
        }
    }

    public IEnumerable<INodeReader> Nodes()
    {
        if (_data is JsonObject obj)
        {
            foreach (var key in obj.Keys)
                yield return new JsonNodeReader(obj[key], key);
        }
        else if (_data is JsonArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
                yield return new JsonNodeReader(arr[i], i.ToString());
        }
    }

    public IEnumerable<string> NodeNames
    {
        get
        {
            if (_data is JsonObject obj)
            {
                foreach (var key in obj.Keys)
                {
                    yield return key;
                }
            }
            else if (_data is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                    yield return i.ToString();
            }
            else
            {
                yield break;
            }
        }
    }

    public IEnumerable<KeyValuePair<string, string>> Attributes
    {
        get
        {
            if (_data is JsonObject obj)
            {
                foreach (var key in obj.Keys)
                {
                    var value = obj[key];
                    if (value is string str)
                    {
                        yield return new KeyValuePair<string, string>(key, str);
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>(key, value.ToString());
                    }
                }
            }
            else
            {
                yield break;
            }
        }
    }

    public string GetAttribute(string name)
    {
        if (_data is JsonObject obj && obj.ContainsKey(name))
        {
            var value = obj[name];
            return value is string str ? str : value.ToString();
        }
        return null;
    }
}