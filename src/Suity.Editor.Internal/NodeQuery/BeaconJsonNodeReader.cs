using ComputerBeacon.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.NodeQuery;

/// <summary>
/// Reads hierarchical node data using the ComputerBeacon.Json library.
/// </summary>
public class BeaconJsonNodeReader : INodeReader
{
    /// <summary>Key used to identify an array container node.</summary>
    public const string PropArray = "__@ary__";
    /// <summary>Key used to identify a single array item.</summary>
    public const string PropArrayItem = "__@item__";
    /// <summary>Key used to store the node name.</summary>
    public const string PropName = "__@name__";
    /// <summary>Key used to store the node value.</summary>
    public const string PropValue = "__@value__";
    /// <summary>Key used to identify an attribute-only node.</summary>
    public const string PropAttrNode = "__@attr__";

    private readonly object _obj;
    private readonly JsonObject _jobj;
    private readonly JsonArray _jary;
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance wrapping the specified parsed JSON object.
    /// </summary>
    /// <param name="obj">The underlying JSON object, array, or primitive value.</param>
    /// <param name="name">Optional name for this node.</param>
    public BeaconJsonNodeReader(object obj, string name = null)
    {
        _obj = obj;
        _jobj = obj as JsonObject;
        _jary = obj as JsonArray;

        // Clear _obj when it is a JsonObject or JsonArray to avoid ambiguity
        if (_jobj != null || _jary != null)
        {
            _obj = null;
        }

        _name = name;
    }

    /// <inheritdoc/>
    public bool Exist => true;

    /// <inheritdoc/>
    public string NodeName => _name ?? _jobj?[PropName]?.ToString();

    /// <inheritdoc/>
    public int ChildCount => _jary?.Count ?? 0;

    /// <inheritdoc/>
    public string NodeValue => _obj?.ToString() ?? _jobj?[PropValue]?.ToString();

    /// <inheritdoc/>
    public object NodeValueObj => _obj ?? _jobj?[PropValue];

    /// <inheritdoc/>
    public IEnumerable<string> NodeNames
    {
        get
        {
            // Enumerate property keys for objects, or node names for array items
            if (_jobj != null)
            {
                foreach (var item in _jobj)
                {
                    yield return item.Key;
                }
            }
            else if (_jary != null)
            {
                IEnumerable<object> ary = _jary;
                // Skip the attribute node at index 0 if present
                if (GetAttrObj() != null)
                {
                    ary = ary.Skip(1);
                }

                foreach (var obj in ary)
                {
                    if (obj is JsonObject jobj)
                    {
                        yield return jobj[PropName]?.ToString();
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<KeyValuePair<string, string>> Attributes
    {
        get
        {
            // Yield only properties that start with "@" (attribute properties)
            var attrObj = GetAttrObj();
            if (attrObj != null)
            {
                foreach (var item in _jobj)
                {
                    if (item.Key.StartsWith("@"))
                    {
                        yield return new KeyValuePair<string, string>(item.Key, item.Value?.ToString());
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public string GetAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var attrObj = GetAttrObj();
        if (attrObj != null)
        {
            // Normalize the attribute name to ensure it has the "@" prefix
            if (!name.StartsWith("@"))
            {
                name = "@" + name;
            }

            return attrObj[name]?.ToString();
        }

        return null;
    }

    /// <inheritdoc/>
    public INodeReader Node(int index)
    {
        // Access array element by index
        var obj = _jary?[index];
        if (obj != null)
        {
            return new BeaconJsonNodeReader(obj);
        }

        return EmptyNodeReader.Empty;
    }

    /// <inheritdoc/>
    public INodeReader Node(string name)
    {
        // Access child by property name on the current JsonObject
        var childObj = _jobj?[name];
        if (childObj != null)
        {
            return new BeaconJsonNodeReader(childObj);
        }

        return EmptyNodeReader.Empty;
    }

    /// <inheritdoc/>
    public IEnumerable<INodeReader> Nodes(string name)
    {
        // Filter child nodes by the specified name
        return Nodes().Where(o => o.NodeName == name);
    }

    /// <inheritdoc/>
    public IEnumerable<INodeReader> Nodes()
    {
        // Enumerate all child nodes (properties for objects, elements for arrays)
        if (_jobj != null)
        {
            foreach (var item in _jobj)
            {
                yield return new BeaconJsonNodeReader(item.Value, item.Key);
            }
        }
        else if (_jary != null)
        {
            IEnumerable<object> ary = _jary;
            // Skip the attribute node at index 0 if present
            if (GetAttrObj() != null)
            {
                ary = ary.Skip(1);
            }

            foreach (var item in ary)
            {
                yield return new BeaconJsonNodeReader(item);
            }
        }
    }

    /// <summary>
    /// Retrieves the JsonObject that holds attribute data.
    /// For a plain object, returns itself. For an array, returns the first element
    /// if it is marked as an attribute node (PropArrayItem).
    /// </summary>
    /// <returns>The attribute JsonObject, or null if none exists.</returns>
    private JsonObject GetAttrObj()
    {
        if (_jobj != null)
        {
            return _jobj;
        }
        else if (_jary?.Count > 0 && _jary[0] is JsonObject jobj && jobj[PropName]?.ToString() == PropArrayItem)
        {
            return jobj;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a JSON string into an <see cref="INodeReader"/>.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="safe">
    /// If true, returns <see cref="EmptyNodeReader.Empty"/> on null input or parse errors.
    /// If false, throws exceptions for invalid input.
    /// </param>
    /// <returns>An <see cref="INodeReader"/> wrapping the parsed JSON, or an empty reader on failure.</returns>
    public static INodeReader FromJson(string json, bool safe = true)
    {
        if (string.IsNullOrEmpty(json))
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw new ArgumentNullException(nameof(json));
            }
        }

        try
        {
            var obj = ComputerBeacon.Json.Parser.Parse(json);
            return new BeaconJsonNodeReader(obj);
        }
        catch (Exception)
        {
            if (safe)
            {
                return EmptyNodeReader.Empty;
            }
            else
            {
                throw;
            }
        }
    }
}
