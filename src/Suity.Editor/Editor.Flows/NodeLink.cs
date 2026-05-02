using Suity.Synchonizing;
using System;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow chart node connection
/// </summary>
public sealed class NodeLink : ISyncObject
{
    /// <summary>
    /// Gets or sets the source node name.
    /// </summary>
    public string FromNode { get; set; }

    /// <summary>
    /// Gets or sets the source connector name.
    /// </summary>
    public string FromConnector { get; set; }

    /// <summary>
    /// Gets or sets the target node name.
    /// </summary>
    public string ToNode { get; set; }

    /// <summary>
    /// Gets or sets the target connector name.
    /// </summary>
    public string ToConnector { get; set; }

    /// <summary>
    /// Initializes a new instance of the NodeLink.
    /// </summary>
    public NodeLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the NodeLink with connection details.
    /// </summary>
    public NodeLink(string fromNode, string fromConnector, string toNode, string toConnector)
    {
        FromNode = fromNode;
        FromConnector = fromConnector;
        ToNode = toNode;
        ToConnector = toConnector;
    }

    /// <summary>
    /// Synchronizes the link properties.
    /// </summary>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        FromNode = sync.Sync("FromNode", FromNode, SyncFlag.NotNull);
        FromConnector = sync.Sync("FromConnector", FromConnector, SyncFlag.NotNull);
        ToNode = sync.Sync("ToNode", ToNode, SyncFlag.NotNull);
        ToConnector = sync.Sync("ToConnector", ToConnector, SyncFlag.NotNull);
    }

    /// <summary>
    /// Clones the link.
    /// </summary>
    /// <returns>A new instance of <see cref="NodeLink"/> with the same properties.</returns>
    public NodeLink Clone()
    {
        return new NodeLink(FromNode, FromConnector, ToNode, ToConnector);
    }

    /// <summary>
    /// Returns a string representation of the link.
    /// </summary>
    public override string ToString()
    {
        return $"{FromNode}:{FromConnector} -> {ToNode}:{ToConnector}";
    }

    /// <summary>
    /// Returns a string representation with optional auto-conversion of IDs to names.
    /// </summary>
    public string ToString(bool autoConvert)
    {
        if (!autoConvert)
        {
            return ToString();
        }

        string fromConn = FromConnector;
        string toConn = ToConnector;

        if (Guid.TryParse(fromConn, out var fromId))
        {
            fromConn = EditorObjectManager.Instance.GetObject(fromId)?.Name ?? fromConn;
        }

        if (Guid.TryParse(toConn, out var toId))
        {
            toConn = EditorObjectManager.Instance.GetObject(toId)?.Name ?? toConn;
        }

        return $"{FromNode}:{fromConn} -> {ToNode}:{toConn}";
    }


    /// <summary>
    /// Creates a NodeLink from two connectors.
    /// </summary>
    public static NodeLink CreateFromConnector(FlowNodeConnector fromConnector, FlowNodeConnector toConnector)
    {
        return new NodeLink(
            fromConnector.ParentNode?.Name,
            fromConnector.Name,
            toConnector.ParentNode?.Name,
            toConnector.Name);
    }

    /// <summary>
    /// Creates a NodeLink from two connectors using exported names.
    /// </summary>
    public static NodeLink CreateFromConnectorExported(FlowNodeConnector fromConnector, FlowNodeConnector toConnector)
    {
        return new NodeLink(
            fromConnector.ParentNode?.Name,
            fromConnector.GetExportedName(),
            toConnector.ParentNode?.Name,
            toConnector.GetExportedName());
    }
}