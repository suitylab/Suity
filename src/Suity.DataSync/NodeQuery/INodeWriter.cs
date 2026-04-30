using System;

namespace Suity.NodeQuery;

/// <summary>
/// Interface for writing hierarchical node data (JSON, XML, etc.)
/// </summary>
public interface INodeWriter
{
    void SetElement(string name, Action<INodeWriter> action);

    void AddArrayItem(Action<INodeWriter> action);

    void SetValue(string value);

    void SetValueObj(object value);

    void SetAttribute(string name, object valueToString);
}