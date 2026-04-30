namespace Suity.Views.NodeGraph;

/// <summary>
/// Represents a link between two NodeGraphConnectors
/// </summary>
public class GraphLink
{
    private readonly GraphConnector _inputConnector;
    private readonly GraphConnector _outputConnector;
    private readonly ConnectorType _connectorType;
    private GraphDataType _nodeGraphDataType;


    /// <summary>
    /// The first end of the link, that's connected to an Output Connector
    /// </summary>
    public GraphConnector Input => _inputConnector;

    /// <summary>
    /// The last end of the link, that's connected to an Input Connector
    /// </summary>
    public GraphConnector Output => _outputConnector;

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
    /// Creates a new Link, given input and output Connectors
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    public GraphLink(GraphConnector input, GraphConnector output, ConnectorType connectorType, GraphDataType dataType)
    {
        _inputConnector = input;
        _outputConnector = output;
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
        if (_inputConnector == null || _outputConnector == null)
        {
            return false;
        }

        if (!provider.GetCanConnectTo(_inputConnector.DataType, _outputConnector.DataType, _outputConnector.AllowMultipleConnection == true, out bool converted))
        {
            return false;
        }

        if (_nodeGraphDataType != _inputConnector.DataType)
        {
            _nodeGraphDataType = _inputConnector.DataType;
        }

        IsConverted = converted;

        return true;
    }
}