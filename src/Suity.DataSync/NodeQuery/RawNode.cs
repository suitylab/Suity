using System;
using System.Collections.Generic;
using System.Linq;
using Suity.Collections;

namespace Suity.NodeQuery;

/// <summary>
/// Represents a raw hierarchical node that can be read and written in various formats (JSON, XML, etc.)
/// </summary>
[Serializable]
public sealed class RawNode : INodeReader, IEquatable<RawNode>
{
    internal RawNode _parent;
    internal string _nodeName;
    internal string _nodeValue;


    public RawNode Parent => _parent;

    public string NodeName
    {
        get => _nodeName;
        set => _nodeName = value;
    }
    public string NodeValue
    {
        get => _nodeValue;
        set => _nodeValue = value;
    }
    public object NodeValueObj => _nodeValue;

    public bool Exist => true;

    public int ChildCount => _childNodes?.Count ?? 0;

    private List<RawNode> _childNodes;
    private Dictionary<string, string> _attributes;

    public RawNode()
    {
    }
    public RawNode(string nodeName)
    {
        _nodeName = nodeName;
    }
    public RawNode(string nodeName, string nodeValue)
        : this(nodeName)
    {
        _nodeValue = nodeValue;
    }

    public INodeReader Node(int index)
    {
        if (_childNodes != null && index >= 0 && index < _childNodes.Count)
        {
            return _childNodes[index];
        }
        else
        {
            return EmptyNodeReader.Empty;
        }
    }

    public INodeReader Node(string name)
    {
        if (_childNodes != null)
        {
            return _childNodes.Find(o => o._nodeName == name) ?? (INodeReader)EmptyNodeReader.Empty;
        }
        else
        {
            return EmptyNodeReader.Empty;
        }
    }

    public IEnumerable<INodeReader> Nodes(string name)
    {
        if (_childNodes != null)
        {
            return _childNodes.Where(o => o._nodeName == name).OfType<INodeReader>();
        }
        else
        {
            return [];
        }
    }

    public IEnumerable<INodeReader> Nodes()
    {
        if (_childNodes != null)
        {
            return _childNodes.OfType<INodeReader>();
        }
        else
        {
            return [];
        }
    }

    public IEnumerable<string> NodeNames
    {
        get
        {
            if (_childNodes != null)
            {
                return _childNodes.Select(o => o._nodeName);
            }
            else
            {
                return [];
            }
        }
    }

    public IEnumerable<KeyValuePair<string, string>> Attributes
    {
        get
        {
            if (_attributes != null)
            {
                return _attributes.Select(o => o);
            }
            else
            {
                return [];
            }
        }
    }

    public string GetAttribute(string name)
    {
        if (_attributes != null)
        {
            return _attributes.GetValueSafe(name);
        }
        else
        {
            return null;
        }
    }


    public int AttributeCount => _attributes?.Count ?? 0;


    public RawNode this[int index]
    {
        get => _childNodes?.GetListItemSafe(index);
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            value.Parent?.RemoveNode(value);

            _childNodes ??= [];

            while (_childNodes.Count < index)
            {
                AddNode("Item");
            }

            RawNode current = _childNodes[index];
            if (current != null)
            {
                current._parent = null;
            }

            _childNodes[index] = value;
            value._parent = this;
        }
    }

    public void AddNode(RawNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (node.Parent == this)
        {
            return;
        }

        node._parent?.RemoveNode(node);
        node._parent = this;

        (_childNodes ??= []).Add(node);
    }
    public RawNode AddNode(string nodeName)
    {
        var node = new RawNode(nodeName);
        AddNode(node);

        return node;
    }
    public bool RemoveNode(RawNode node)
    {
        if (_childNodes?.Remove(node) == true)
        {
            node._parent = null;
            if (_childNodes.Count == 0)
            {
                _childNodes = null;
            }

            return true;
        }

        return false;
    }
    public int RemoveAll(string nodeName)
    {
        if (_childNodes is null)
        {
            return 0;
        }

        int count = 0;
        for (int i = _childNodes.Count - 1; i >= 0; i--)
        {
            if (_childNodes[i]._nodeName == nodeName)
            {
                _childNodes[i]._parent = null;
                _childNodes.RemoveAt(i);
                count++;
            }
        }

        return count;
    }
    public void SetAttribute(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        (_attributes ??= [])[name] = value;
    }
    public void SetAttribute(string name, object objectToString)
    {
        if (objectToString != null)
        {
            SetAttribute(name, objectToString.ToString());
        }
        else
        {
            UnsetAttribute(name);
        }
    }
    public void UnsetAttribute(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (_attributes != null)
        {
            _attributes.Remove(name);

            if (_attributes.Count == 0)
            {
                _attributes = null;
            }
        }
    }
    public void Clear()
    {
        if (_childNodes != null)
        {
            foreach (var node in _childNodes)
            {
                node._parent = null;
                node.Clear();
            }
            _childNodes.Clear();
            _childNodes = null;
        }

        _attributes?.Clear();
        _attributes = null;
    }

    public void Read(INodeReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        Clear();

        _nodeName = reader.NodeName;
        _nodeValue = reader.NodeValueObj?.ToString();
        foreach (var attr in reader.Attributes)
        {
            _attributes ??= new Dictionary<string, string>
                {
                    [attr.Key] = attr.Value
                };
        }

        foreach (var childReader in reader.Nodes())
        {
            var childNode = new RawNode(childReader.NodeName);
            childNode.Read(childReader);
            (_childNodes ??= []).Add(childNode);
        }
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(_nodeName) ? _nodeName : base.ToString();
    }

    public void ClonePropertyFrom(RawNode other)
    {
        if (other is null || other.Equals(this))
        {
            return;
        }

        _childNodes?.Clear();
        _attributes?.Clear();

        _nodeName = other._nodeName;
        _nodeValue = other._nodeValue;
        
        if (other._childNodes?.Count > 0)
        {
            _childNodes ??= [];

            foreach (var otherChildNode in other._childNodes)
            {
                var childNode = new RawNode();
                childNode.ClonePropertyFrom(otherChildNode);
                _childNodes.Add(childNode);
            }
        }
        if (other._attributes?.Count > 0)
        {
            _attributes ??= [];

            foreach (var pair in other._attributes)
            {
                _attributes.Add(pair.Key, pair.Value);
            }
        }
    }
    public RawNode Clone()
    {
        var node = new RawNode();
        node.ClonePropertyFrom(this);

        return node;
    }

    public static RawNode FromReader(INodeReader reader)
    {
        var node = new RawNode();
        if (reader != null)
        {
            node.Read(reader);
        }

        return node;
    }

    public bool Equals(RawNode other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (_nodeName != other._nodeName)
        {
            return false;
        }

        if (_nodeValue != other._nodeValue)
        {
            return false;
        }

        if ((_childNodes != null) != (other._childNodes != null))
        {
            return false;
        }

        if (_childNodes != null)
        {
            if (_childNodes.Count != other._childNodes.Count)
            {
                return false;
            }

            for (int i = 0; i < _childNodes.Count; i++)
            {
                if (!_childNodes[i].Equals(other._childNodes[i]))
                {
                    return false;
                }
            }
        }

        if ((_attributes != null) != (other._attributes != null))
        {
            return false;
        }

        if (_attributes != null)
        {
            if (_attributes.Count != other._attributes.Count)
            {
                return false;
            }

            foreach (var pair in _attributes)
            {
                if (pair.Value != other.GetAttribute(pair.Key))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public string ToXml(bool declaration = true)
    {
        var writer = new XmlNodeWriter(_nodeName, declaration);
        WriteTo(writer);

        return writer.ToString();
    }

    public void WriteTo(INodeWriter writer)
    {
        if (_nodeValue != null)
        {
            writer.SetValue(_nodeValue);
        }

        if (_attributes != null)
        {
            foreach (var attrPair in _attributes)
            {
                writer.SetAttribute(attrPair.Key, attrPair.Value);
            }
        }

        if (_childNodes != null)
        {
            foreach (var node in _childNodes)
            {
                writer.SetElement(node.NodeName, childWriter =>
                {
                    node.WriteTo(childWriter);
                });
            }
        }
    }
}

/// <summary>
/// INodeWriter implementation that writes to RawNode objects
/// </summary>
public class RawNodeWriter : INodeWriter
{
    RawNode _currentNode;

    public RawNode Result => _currentNode;

    public RawNodeWriter(string rootName)
    {
        _currentNode = new RawNode(rootName);
    }
    public RawNodeWriter(RawNode rootNode)
    {
        _currentNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
    }

    public void SetElement(string name, Action<INodeWriter> action)
    {
        var childNode = new RawNode(name);
        var lastNode = _currentNode;

        _currentNode.AddNode(childNode);
        _currentNode = childNode;

        try
        {
            action(this);
        }
        finally
        {
            _currentNode = lastNode;
        }
    }

    public void AddArrayItem(Action<INodeWriter> action)
    {
        var childNode = new RawNode("Item");
        var lastNode = _currentNode;

        _currentNode.AddNode(childNode);
        _currentNode = childNode;

        try
        {
            action(this);
        }
        finally
        {
            _currentNode = lastNode;
        }
    }

    public void SetAttribute(string name, object valueToString)
    {
        if (valueToString != null)
        {
            _currentNode.SetAttribute(name, valueToString.ToString());
        }
        else
        {
            _currentNode.UnsetAttribute(name);
        }
    }

    public void SetValue(string value)
    {
        _currentNode._nodeValue = value;
    }

    public void SetValueObj(object value)
    {
        _currentNode._nodeValue = value?.ToString();
    }
}