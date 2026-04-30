using ComputerBeacon.Json;
using System;

namespace Suity.NodeQuery;

/// <summary>
/// Writes hierarchical node data using the ComputerBeacon.Json library.
/// </summary>
public class BeaconJsonNodeWriter : INodeWriter
{
    private readonly Action<object> _setter;
    private JsonObject _jobj;
    private JsonArray _jary;
    private object _value;

    /// <summary>
    /// Initializes a new instance with an optional setter callback for committing the written data.
    /// </summary>
    /// <param name="setter">Callback invoked when a value or structure is finalized.</param>
    public BeaconJsonNodeWriter(Action<object> setter = null)
    {
        _setter = setter;
    }

    /// <inheritdoc/>
    public void SetElement(string name, Action<INodeWriter> action)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        // Lazily create the root JsonObject if not yet initialized
        if (_jobj is null)
        {
            _jobj = new JsonObject();
            if (_value != null)
            {
                _jobj[BeaconJsonNodeReader.PropValue] = _value;
            }
            _setter?.Invoke(_jobj);
        }

        if (action != null)
        {
            // Create a child writer that writes back into the parent object under the given name
            BeaconJsonNodeWriter childWriter = new BeaconJsonNodeWriter(o =>
            {
                _jobj[name] = o;
            });

            action(childWriter);
        }
    }

    /// <inheritdoc/>
    public void AddArrayItem(Action<INodeWriter> action)
    {
        // Lazily create the root JsonArray if not yet initialized
        if (_jary is null)
        {
            _jary = new JsonArray();
            _setter?.Invoke(_jary);

            if (_jobj != null)
            {
                // Mark the existing object as an attribute node and move it into the array
                _jobj[BeaconJsonNodeReader.PropName] = BeaconJsonNodeReader.PropAttrNode;
                _jary.Add(_jobj);
            }
        }

        if (action != null)
        {
            // Reserve a slot and create a child writer for the new array item
            int index = _jary.Count;
            _jary.Add(null);

            BeaconJsonNodeWriter childWriter = new BeaconJsonNodeWriter(o =>
            {
                _jary[index] = o;
            });

            action(childWriter);
        }
    }

    /// <inheritdoc/>
    public void SetAttribute(string name, object valueToString)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        // Ensure attribute names are prefixed with "@"
        if (!name.StartsWith("@"))
        {
            name = "@" + name;
        }

        // Lazily create the root JsonObject if not yet initialized
        if (_jobj is null)
        {
            _jobj = new JsonObject();
            if (_value != null)
            {
                _jobj[BeaconJsonNodeReader.PropValue] = _value;
            }
            _setter?.Invoke(_jobj);

            if (_jary != null)
            {
                // Insert as the first element (attribute node) in the array
                _jobj[BeaconJsonNodeReader.PropName] = BeaconJsonNodeReader.PropAttrNode;
                _jary.Insert(0, _jobj);
            }
        }

        _jobj[name] = valueToString;
    }

    /// <inheritdoc/>
    public void SetValue(string value)
    {
        _value = value;

        // Store the value on the current JsonObject, or pass it directly via the setter
        if (_jobj != null)
        {
            _jobj[BeaconJsonNodeReader.PropValue] = value;
            _setter?.Invoke(_jobj);
        }
        else
        {
            _setter?.Invoke(value);
        }
    }

    /// <inheritdoc/>
    public void SetValueObj(object value)
    {
        _value = value;

        // Store the object value on the current JsonObject, or pass it directly via the setter
        if (_jobj != null)
        {
            _jobj[BeaconJsonNodeReader.PropValue] = value;
            _setter?.Invoke(_jobj);
        }
        else
        {
            _setter?.Invoke(value);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        // Serialize the root object, array, or return empty
        if (_jobj != null)
        {
            return _jobj.ToString(true);
        }
        else if (_jary != null)
        {
            return _jary.ToString(true);
        }
        else
        {
            return string.Empty;
        }
    }
}
