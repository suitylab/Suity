using System.Collections.Generic;

namespace Suity.NodeQuery;

/// <summary>
/// Interface for reading hierarchical node data (JSON, XML, etc.)
/// </summary>
public interface INodeReader
{
    bool Exist { get; }

    string NodeName { get; }

    int ChildCount { get; }

    string NodeValue { get; }

    object NodeValueObj { get; }

    INodeReader Node(int index);

    INodeReader Node(string name);

    IEnumerable<INodeReader> Nodes(string name);

    IEnumerable<INodeReader> Nodes();

    IEnumerable<string> NodeNames { get; }

    IEnumerable<KeyValuePair<string, string>> Attributes { get; }

    string GetAttribute(string name);
}