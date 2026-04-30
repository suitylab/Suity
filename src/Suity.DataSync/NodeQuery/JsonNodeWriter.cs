using ComputerBeacon.Json;
using System;

namespace Suity.NodeQuery;

/// <summary>
/// INodeWriter implementation for writing JSON objects
/// </summary>
public class JsonNodeWriter : MarshalByRefObject, INodeWriter
{
    private JsonObject _root;
    private JsonObject _currentNode;

    public JsonNodeWriter(string rootName)
    {
        _root = new JsonObject();
        _root[rootName] = new JsonObject();
        _currentNode = (JsonObject)_root[rootName];
    }

    public void SetElement(string name, Action<INodeWriter> action)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        var newElement = new JsonObject();
        _currentNode[name] = newElement;

        var previousNode = _currentNode;
        _currentNode = newElement;

        try
        {
            action(this);
        }
        finally
        {
            _currentNode = previousNode;
        }
    }

    public void AddArrayItem(Action<INodeWriter> action)
    {
        if (!_currentNode.ContainsKey("Items"))
        {
            _currentNode["Items"] = new JsonArray();
        }

        var array = (JsonArray)_currentNode["Items"];
        var newItem = new JsonObject();
        array.Add(newItem);

        var previousNode = _currentNode;
        _currentNode = newItem;

        try
        {
            action(this);
        }
        finally
        {
            _currentNode = previousNode;
        }
    }

    public void SetValue(string value)
    {
        _currentNode["Value"] = value;
    }

    public void SetValueObj(object value)
    {
        _currentNode["Value"] = value != null ? value.ToString() : null;
    }

    public void SetAttribute(string name, object valueToString)
    {
        if (valueToString == null) throw new ArgumentNullException(nameof(valueToString));
        _currentNode[$"@{name}"] = valueToString.ToString();
    }

    public override string ToString()
    {
        return _root.ToString();
    }
}