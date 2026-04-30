using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Views.Im.Flows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Provides helper extension methods for building and managing flow view nodes.
/// </summary>
public static class FlowViewHelper
{
    /// <summary>
    /// Converts a <see cref="FlowConnectorTypes"/> value to a <see cref="ConnectorType"/>.
    /// </summary>
    /// <param name="type">The flow connector type to convert.</param>
    /// <returns>The corresponding <see cref="ConnectorType"/> value.</returns>
    public static ConnectorType GetConnectorType(this FlowConnectorTypes type) => type switch
    {
        FlowConnectorTypes.Action => ConnectorType.Action,
        FlowConnectorTypes.Data => ConnectorType.Data,
        FlowConnectorTypes.Associate => ConnectorType.Associate,
        FlowConnectorTypes.Control => ConnectorType.Control,
        _ => ConnectorType.Action,
    };

    /// <summary>
    /// Converts a <see cref="FlowDirections"/> value to a <see cref="GraphDirection"/>.
    /// </summary>
    /// <param name="direction">The flow direction to convert.</param>
    /// <returns>The corresponding <see cref="GraphDirection"/> value.</returns>
    public static GraphDirection GetDirection(this FlowDirections direction) => direction switch
    {
        FlowDirections.Input => GraphDirection.Input,
        FlowDirections.Output => GraphDirection.Output,
        _ => GraphDirection.Input,
    };


    /// <summary>
    /// Rebuilds the connectors and display information for a flow view node.
    /// </summary>
    /// <param name="viewNode">The graph view node to rebuild.</param>
    /// <param name="_node">The underlying flow node providing connector data.</param>
    /// <param name="_connectorDic">Dictionary caching connectors by name for this view node.</param>
    /// <param name="removeLink">Optional callback to remove obsolete links when connectors are removed.</param>
    public static void RebuildeViewNode(this ImGraphNode viewNode, FlowNode _node, Dictionary<string, GraphConnector> _connectorDic, Action<NodeLink> removeLink = null)
    {
        if (_node is null)
        {
            return;
        }

        var item = _node.DiagramItem as FlowDiagramItem;
        if (item is null)
        {
            return;
        }

        viewNode.Name = _node.Name;

        HashSet<GraphConnector> cache = [];
        int inputNum = 0;
        int outputNum = 0;

        try
        {
            // Thread errors often occur here, try using ToArray()
            foreach (var connector in _node.Connectors)
            {
                if (string.IsNullOrEmpty(connector.Name))
                {
                    continue;
                }

                GraphConnector ngConnector = _connectorDic.GetOrAdd(connector.Name,
                    _ => new GraphConnector(
                    connector.Name,
                    viewNode,
                    connector.ConnectionType.GetConnectorType(),
                    connector.Direction.GetDirection(),
                    inputNum,
                    connector.DataTypeName));

                ngConnector.DataType = viewNode.Diagram.DataTypeProvider.GetDataType(connector.DataTypeName);
                ngConnector.Description = connector.Description;
                ngConnector.AllowMultipleConnection = connector.AllowMultipleConnection;
                // When port type is Action type but has data type defined, treat as combined port
                ngConnector.IsCombined = connector.ConnectionType == FlowConnectorTypes.Action && connector.DataTypeName != FlowNode.ACTION_TYPE;
                ngConnector.AssociateValue = connector.AssociateValue;

                if (ngConnector.Direction == GraphDirection.Input)
                {
                    ngConnector.ConnectorIndex = inputNum;
                    inputNum++;
                }
                else
                {
                    ngConnector.ConnectorIndex = outputNum;
                    outputNum++;
                }

                cache.Add(ngConnector);
            }
        }
        catch (Exception err)
        {
            err.LogError();

            return;
        }

        if (removeLink != null)
        {
            //First delete connections
            foreach (GraphConnector remove in _connectorDic.Values.Where(o => !cache.Contains(o)))
            {
                if (remove.Direction == GraphDirection.Input)
                {
                    foreach (var link in item.Diagram.GetLinksByConnectorFrom(viewNode.Name, remove.Name).ToArray())
                    {
                        removeLink(link);
                    }
                }
                else
                {
                    foreach (var link in item.Diagram.GetLinksByConnectorTo(viewNode.Name, remove.Name).ToArray())
                    {
                        removeLink(link);
                    }
                }
            }
        }

        //Add
        var connectors = viewNode.Connectors;
        connectors.Clear();
        foreach (var viewConnector in cache)
        {
            connectors.Add(viewConnector);
        }

        //Then remove extras
        var removes = _connectorDic.RemoveAllByValueAndGet(o => !cache.Contains(o));
        foreach (var remove in removes)
        {
            viewNode.Diagram?.Links.Remove(remove);
        }

        //int num = Math.Max(inputNum, outputNum);
        //
        //if (_style?.Height is int h && h > 0)
        //{
        //    Height = h;
        //}
        //else
        //{
        //    if (HasHeader)
        //    {
        //        Height = ParentView.ParentPanel.NodeHeaderSize + 6 + num * 16;
        //    }
        //    else
        //    {
        //        Height = num * 16;
        //    }
        //}

        viewNode.Diagram?.ParentControl?.LinkManager.UpdateAssociate(viewNode);
    }
}
