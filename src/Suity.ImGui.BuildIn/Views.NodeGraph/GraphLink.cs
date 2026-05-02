namespace Suity.Views.NodeGraph;

/// <summary>
/// Represents a link between two NodeGraphConnectors
/// </summary>
public class GraphLink
{
    private readonly GraphConnector _from;
    private readonly GraphConnector _to;
    private readonly ConnectorType _connectorType;
    private GraphDataType _nodeGraphDataType;


    /// <summary>
    /// The first end of the link, that's connected to an Output Connector
    /// </summary>
    public GraphConnector From => _from;

    /// <summary>
    /// The last end of the link, that's connected to an Input Connector
    /// </summary>
    public GraphConnector To => _to;

    /// <summary>
    /// Gets the type of the connector (data, action, associate, or control).
    /// </summary>
    public ConnectorType ConnectorType => _connectorType;

    /// <summary>
    /// Gets the data type associated with this link.
    /// </summary>
    public GraphDataType DataType => _nodeGraphDataType;

    /// <summary>
    /// Gets or sets a value indicating whether this link requires a type conversion.
    /// </summary>
    public bool IsConverted { get; set; }

    /// <summary>
    /// Whether the link is highlighted
    /// </summary>
    public bool Highlighted { get; set; }

    /// <summary>
    /// Creates a new Link, given input and output Connectors
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public GraphLink(GraphConnector from, GraphConnector to, ConnectorType connectorType, GraphDataType dataType)
    {
        _from = from;
        _to = to;
        _connectorType = connectorType;
        _nodeGraphDataType = dataType;
    }

    /// <summary>
    /// Validates the link against the data type provider and updates conversion status.
    /// </summary>
    /// <param name="provider">The data type provider to validate against.</param>
    /// <returns>True if the link is valid; otherwise, false.</returns>
    public bool CheckLink(IGraphDataTypeProvider provider)
    {
        if (_from == null || _to == null)
        {
            return false;
        }

        if (!provider.GetCanConnectTo(_from.DataType, _to.DataType, _to.AllowMultipleConnection == true, out bool converted))
        {
            return false;
        }

        if (_nodeGraphDataType != _from.DataType)
        {
            _nodeGraphDataType = _from.DataType;
        }

        IsConverted = converted;

        return true;
    }

    public override string ToString()
    {
        return $"{From} -> {To}";
    }
}