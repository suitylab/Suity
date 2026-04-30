using System;
using Newtonsoft.Json.Linq;

namespace Suity.NodeQuery;

/// <summary>
/// Writes hierarchical node data using Newtonsoft.Json (JObject/JArray).
/// </summary>
public class NewtonJsonNodeWriter : MarshalByRefObject, INodeWriter
{
    private readonly JObject _root;
    private JToken _currentNode;

    /// <summary>
    /// Initializes a new instance with the specified root element name.
    /// </summary>
    /// <param name="rootName">The name of the root element.</param>
    public NewtonJsonNodeWriter(string rootName)
    {
        _root = new JObject { ["rootName"] = rootName };
        _currentNode = _root;
    }

    /// <inheritdoc/>
    public void SetElement(string name, Action<INodeWriter> action)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        // Create a new child object and attach it to the current node
        var newElement = new JObject();
        if (_currentNode is JObject obj)
        {
            obj[name] = newElement;
        }
        else
        {
            throw new InvalidOperationException("Cannot add an element to a non-object node.");
        }

        // Push the new element onto the stack
        var previousNode = _currentNode;
        _currentNode = newElement;

        try
        {
            action(this);
        }
        finally
        {
            // Restore the previous node
            _currentNode = previousNode;
        }
    }

    /// <inheritdoc/>
    public void AddArrayItem(Action<INodeWriter> action)
    {
        // Create or reuse the "Items" array on the current object
        if (!(_currentNode is JArray array))
        {
            array = new JArray();

            if (_currentNode is JObject obj)
            {
                obj["Items"] = array;
            }
            else
            {
                throw new InvalidOperationException("Cannot add array items to a non-object node.");
            }
        }

        // Create a new item and push it onto the stack
        var newItem = new JObject();
        array.Add(newItem);

        var previousNode = _currentNode;
        _currentNode = newItem;

        try
        {
            action(this);
        }
        finally
        {
            // Restore the previous node
            _currentNode = previousNode;
        }
    }

    /// <inheritdoc/>
    public void SetValue(string value)
    {
        // Set value on a JValue directly, or store under "Value" key for objects
        if (_currentNode is JValue valueNode)
        {
            valueNode.Value = value;
        }
        else if (_currentNode is JObject obj)
        {
            obj["Value"] = value;
        }
        else
        {
            throw new InvalidOperationException("Cannot set a value on this node type.");
        }
    }

    /// <inheritdoc/>
    public void SetValueObj(object value)
    {
        SetValue(value?.ToString());
    }

    /// <inheritdoc/>
    public void SetAttribute(string name, object valueToString)
    {
        // Attributes are prefixed with "@" and stored on the current JObject
        if (_currentNode is JObject obj)
        {
            obj[$"@{name}"] = valueToString != null ? valueToString.ToString() : "";
        }
        else
        {
            throw new InvalidOperationException("Cannot set an attribute on this node type.");
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _root.ToString();
    }

    /// <summary>
    /// Returns the serialized JSON string of the entire document.
    /// </summary>
    /// <returns>The JSON representation of the root node.</returns>
    public string GetResult()
    {
        return _root.ToString();
    }
}
