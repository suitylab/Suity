using System;
using System.Collections.Generic;

namespace Suity.NodeQuery;

/// <summary>
/// Empty implementation of INodeReader that returns no data
/// </summary>
public class EmptyNodeReader : MarshalByRefObject, INodeReader
{
    private static EmptyNodeReader InternalInstance { get; } = new();
    public static EmptyNodeReader Empty { get; } = InternalInstance;

    public bool Exist => false;
    public string NodeName => string.Empty;
    public int ChildCount => 0;
    public string NodeValue => string.Empty;
    public object NodeValueObj => null;

    public INodeReader Node(int index) => this;

    public INodeReader Node(string name) => this;

    public string GetAttribute(string name) => string.Empty;

    public IEnumerable<INodeReader> Nodes(string name) => [];

    public IEnumerable<INodeReader> Nodes() => [];

    public IEnumerable<string> NodeNames => [];

    public IEnumerable<KeyValuePair<string, string>> Attributes => [];

    public override string ToString()
    {
        return string.Empty;
    }

}