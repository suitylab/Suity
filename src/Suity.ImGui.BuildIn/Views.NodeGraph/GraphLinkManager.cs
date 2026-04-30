using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.NodeGraph;

/// <summary>
/// Manages link operations including creation, deletion, validation, and associate link updates.
/// </summary>
public class GraphLinkManager
{
    private readonly GraphControl _control;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphLinkManager"/> class.
    /// </summary>
    /// <param name="control">The parent graph control.</param>
    public GraphLinkManager(GraphControl control)
    {
        _control = control;
    }

    /// <summary>
    /// Gets the parent graph control.
    /// </summary>
    public GraphControl ParentControl => _control;
    /// <summary>
    /// Gets the diagram associated with this link manager.
    /// </summary>
    public GraphDiagram Diagram => _control.Diagram;

    /// <summary>
    /// Deletes all links connected to the specified connector.
    /// </summary>
    /// <param name="connector">The connector whose links should be deleted.</param>
    /// <param name="onLinkDestroyed">Optional callback invoked after links are destroyed.</param>
    public void DeleteLinkConnectors(GraphConnector connector, Action<IEnumerable<GraphLink>>? onLinkDestroyed = null)
    {
        var linksToDelete = Diagram.Links.GetLinks(connector).ToList();

        foreach (var link in linksToDelete)
        {
            Diagram.Links.Remove(link);
        }

        onLinkDestroyed?.Invoke(linksToDelete);

        _control.RefreshView();
    }

    /// <summary>
    /// Determines whether two connectors can be connected.
    /// </summary>
    /// <param name="fromConnector">The source connector.</param>
    /// <param name="toConnector">The target connector.</param>
    /// <param name="converted">When this method returns, indicates whether a type conversion is required.</param>
    /// <returns>True if the connectors can be connected; null if the connection is invalid.</returns>
    public bool? GetCanConnect(GraphConnector fromConnector, GraphConnector toConnector, out bool converted)
    {
        converted = false;

        if (toConnector is null || fromConnector is null || toConnector == fromConnector)
        {
            return null;
        }

        if (fromConnector != null &&
            toConnector != null &&
            fromConnector != toConnector &&
            fromConnector.Direction != toConnector.Direction &&
            !Diagram.Links.IsLinked(fromConnector, toConnector))
        {
            if (fromConnector.ConnectorType == ConnectorType.Action
                && toConnector.ConnectorType == ConnectorType.Action)
            {
                return true;
            }

            GraphConnector fromConn;
            GraphConnector toConn;

            if (fromConnector.Direction == GraphDirection.Output)
            {
                fromConn = fromConnector;
                toConn = toConnector;
            }
            else
            {
                fromConn = toConnector;
                toConn = fromConnector;
            }

            var provider = Diagram.DataTypeProvider;
            bool canConnect = provider.GetCanConnectTo(fromConn.DataType, toConn.DataType, toConnector.AllowMultipleConnection == true, out converted);

            return canConnect;
        }

        return null;
    }

    /// <summary>
    /// Determines whether two connectors can be connected and creates the link if valid.
    /// </summary>
    /// <param name="fromConnector">The source connector.</param>
    /// <param name="toConnector">The target connector.</param>
    /// <param name="converted">When this method returns, indicates whether a type conversion is required.</param>
    /// <param name="onLinkDestroyed">Optional callback invoked when existing links are destroyed to make room.</param>
    /// <returns>True if the link was created; null if the connection is invalid.</returns>
    public bool? GetCanConnect(GraphConnector fromConnector, GraphConnector toConnector, out bool converted, Action<IEnumerable<GraphLink>>? onLinkDestroyed = null)
    {
        converted = false;

        if (fromConnector != null &&
            toConnector != null &&
            fromConnector != toConnector &&
            fromConnector.Direction != toConnector.Direction &&
            !Diagram.Links.IsLinked(fromConnector, toConnector))
        {
            bool canConnect = GetCanConnect(fromConnector, toConnector, out converted) == true;
            if (canConnect)
            {
                if (fromConnector.Direction == GraphDirection.Output)
                {
                    if (!toConnector.GetAllowMultipleToConnection() && Diagram.Links.IsLinked(toConnector))
                    {
                        DeleteLinkConnectors(toConnector, onLinkDestroyed);
                    }

                    if (!fromConnector.GetAllowMultipleFromConnection() && Diagram.Links.IsLinked(fromConnector))
                    {
                        DeleteLinkConnectors(fromConnector, onLinkDestroyed);
                    }

                    var link = new GraphLink(fromConnector, toConnector, fromConnector.ConnectorType, fromConnector.DataType)
                    {
                        IsConverted = converted,
                    };

                    Diagram.Links.Add(link);

                    return true;
                }
                else
                {
                    if (!fromConnector.GetAllowMultipleToConnection() && Diagram.Links.IsLinked(fromConnector))
                    {
                        DeleteLinkConnectors(fromConnector, onLinkDestroyed);
                    }

                    if (!toConnector.GetAllowMultipleFromConnection() && Diagram.Links.IsLinked(toConnector))
                    {
                        DeleteLinkConnectors(toConnector, onLinkDestroyed);
                    }

                    var link = new GraphLink(toConnector, fromConnector, toConnector.ConnectorType, toConnector.DataType)
                    {
                        IsConverted = converted,
                    };

                    Diagram.Links.Add(link);

                    return true;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all input connectors connected to the specified output connector.
    /// </summary>
    /// <param name="linkOutConnector">The output connector.</param>
    /// <returns>A collection of input connectors connected to the specified output.</returns>
    public IEnumerable<GraphConnector> GetInputs(GraphConnector linkOutConnector)
    {
        return Diagram.Links.GetInputs(linkOutConnector);
    }

    /// <summary>
    /// Determines whether the specified connector has any links.
    /// </summary>
    /// <param name="connector">The connector to check.</param>
    /// <returns>True if the connector has links; otherwise, false.</returns>
    public bool IsLinked(GraphConnector connector)
    {
        return Diagram.Links.IsLinked(connector);
    }

    /// <summary>
    /// Gets all links connected to the specified connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>A collection of links connected to the connector.</returns>
    public IEnumerable<GraphLink> GetLinks(GraphConnector connector)
    {
        return Diagram.Links.GetLinks(connector);
    }

    /// <summary>
    /// Determines whether two connectors are linked to each other.
    /// </summary>
    /// <param name="connector1">The first connector.</param>
    /// <param name="connector2">The second connector.</param>
    /// <returns>True if the connectors are linked; otherwise, false.</returns>
    public bool IsLinked(GraphConnector connector1, GraphConnector connector2)
    {
        return Diagram.Links.IsLinked(connector1, connector2);
    }

    /// <summary>
    /// Finds a link between the specified input and output connectors.
    /// </summary>
    /// <param name="input">The input connector.</param>
    /// <param name="output">The output connector.</param>
    /// <returns>The link if found; otherwise, null.</returns>
    public GraphLink? FindLink(GraphConnector input, GraphConnector output)
    {
        return Diagram.Links.FirstOrDefault(o => o.Input == input && o.Output == output);
    }

    /// <summary>
    /// Adds a link between connectors identified by node and connector names.
    /// </summary>
    /// <param name="fromNodeName">The name of the source node.</param>
    /// <param name="fromConnectorName">The name of the source connector.</param>
    /// <param name="toNodeName">The name of the target node.</param>
    /// <param name="toConnectorName">The name of the target connector.</param>
    /// <returns>True if the link was added or already exists; otherwise, false.</returns>
    public bool AddLink(string fromNodeName, string fromConnectorName, string toNodeName, string toConnectorName)
    {
        var fromConnector = Diagram.FindNode(fromNodeName)?.FindConnector(fromConnectorName);
        var toConnector = Diagram.FindNode(toNodeName)?.FindConnector(toConnectorName);

        if (FindLink(fromConnector, toConnector) != null)
        {
            return true;
        }

        if (fromConnector != null &&
            toConnector != null &&
            fromConnector != toConnector &&
            fromConnector.Direction != toConnector.Direction &&
            !IsLinked(fromConnector, toConnector))
        {
            var provider = Diagram.DataTypeProvider;

            if (fromConnector.Direction == GraphDirection.Output)
            {
                bool canConnect = provider.GetCanConnectTo(fromConnector.DataType, toConnector.DataType, toConnector.AllowMultipleConnection == true, out bool converted);

                var link = new GraphLink(fromConnector, toConnector, fromConnector.ConnectorType, fromConnector.DataType)
                {
                    IsConverted = converted,
                };

                Diagram.Links.Add(link);
            }
            else
            {
                bool canConnect = provider.GetCanConnectTo(toConnector.DataType, fromConnector.DataType, toConnector.AllowMultipleConnection == true, out bool converted);

                var link = new GraphLink(toConnector, fromConnector, fromConnector.ConnectorType, fromConnector.DataType)
                {
                    IsConverted = converted,
                };

                Diagram.Links.Add(link);
            }

            _control.RequestOutput();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Deletes a link between connectors identified by node and connector names.
    /// </summary>
    /// <param name="fromNodeName">The name of the source node.</param>
    /// <param name="fromConnectorName">The name of the source connector.</param>
    /// <param name="toNodeName">The name of the target node.</param>
    /// <param name="toConnectorName">The name of the target connector.</param>
    public void DeleteLink(string fromNodeName, string fromConnectorName, string toNodeName, string toConnectorName)
    {
        var link = Diagram.Links.FirstOrDefault(
            o => o.Input.Parent.Name == fromNodeName &&
            o.Input.Name == fromConnectorName &&
            o.Output.Parent.Name == toNodeName &&
            o.Output.Name == toConnectorName);

        if (link != null)
        {
            Diagram.Links.Remove(link);
            _control.RequestOutput();
        }
    }

    /// <summary>
    /// Updates associate links for the specified node based on associate connector values.
    /// </summary>
    /// <param name="node">The node whose associate links should be updated.</param>
    public void UpdateAssociate(GraphNode node)
    {
        var links = Diagram.Links.GetLinks(node).Where(o => o.ConnectorType == ConnectorType.Associate);
        if (links.Any())
        {
            foreach (var link in links.ToArray())
            {
                Diagram.Links.Remove(link);
            }
        }

        foreach (var connector in node.Connectors.Where(o => o.ConnectorType == ConnectorType.Associate))
        {
            if (connector.Direction == GraphDirection.Input)
            {
                foreach (var nodeOther in Diagram.NodeCollection.Where(o => o != node))
                {
                    foreach (var connOut in nodeOther.Connectors.Where(o => o.ConnectorType == ConnectorType.Associate && o.Direction == GraphDirection.Output))
                    {
                        if (Diagram.DataTypeProvider.GetCanAssociate(connector.DataType, connector.AssociateValue, connOut.DataType, connOut.AssociateValue))
                        {
                            var link = new GraphLink(connector, connOut, ConnectorType.Associate, connector.DataType);
                            Diagram.Links.Add(link);
                        }
                    }
                }
            }
            else
            {
                foreach (var nodeOther in Diagram.NodeCollection.Where(o => o != node))
                {
                    foreach (var connIn in nodeOther.Connectors.Where(o => o.ConnectorType == ConnectorType.Associate && o.Direction == GraphDirection.Input))
                    {
                        if (Diagram.DataTypeProvider.GetCanAssociate(connector.DataType, connector.AssociateValue, connIn.DataType, connIn.AssociateValue))
                        {
                            var link = new GraphLink(connector, connIn, ConnectorType.Associate, connector.DataType);
                            Diagram.Links.Add(link);
                        }
                    }
                }
            }
        }
    }
}
