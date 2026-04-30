using System;

namespace Suity.NodeQuery;

/// <summary>
/// Empty implementation of INodeWriter that does nothing
/// </summary>
public sealed class EmptyNodeWriter : INodeWriter
{
    public static EmptyNodeWriter Empty { get; } = new();

    private EmptyNodeWriter()
    {
    }

    public void SetElement(string name, Action<INodeWriter> action)
    {
    }

    public void AddArrayItem(Action<INodeWriter> action)
    {
    }

    public void SetAttribute(string name, object valueToString)
    {
    }

    public void SetValue(string value)
    {
    }

    public void SetValueObj(object value)
    {
    }
}