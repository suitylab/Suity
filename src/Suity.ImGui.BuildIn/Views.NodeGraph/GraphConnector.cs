using System.Drawing;

namespace Suity.Views.NodeGraph;


/// <summary>
/// Represents a connector on a node
/// </summary>
public class GraphConnector
{
    private string _name;
    private GraphNode _parentNode;
    private GraphDiagram _view;
    private ConnectorType _connectorType;
    private GraphDirection _direction;
    private int _connectorIndex;
    private GraphDataType _dataType;

    //Mob by simage: Support multiple connections per connector
    private bool? _allowMultipleConnection;
    //Mob by simage: Combined port
    private bool _isCombined = false;


    /// <summary>
    /// The parent node that contains the connector
    /// </summary>
    public GraphNode Parent => _parentNode;

    /// <summary>
    /// Name of the connector that will be displayed
    /// </summary>
    public string Name {  get => _name;  set => _name = value;  }

    /// <summary>
    /// Gets or sets the description of the connector. If set, this is used as the display name.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets the display name of the connector (description if set, otherwise name).
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(Description) ? Description : _name;

    /// <summary>
    /// Type of the connector (input/output)
    /// </summary>
    public GraphDirection Direction { get => _direction;  set => _direction = value;  }

    public ConnectorType ConnectorType { get => _connectorType;  set => _connectorType = value;  }

    public int ConnectorIndex { get => _connectorIndex;  set => _connectorIndex = value; }

    /// <summary>
    /// Data type object (from NodeGraphView.KnownDataTypes)
    /// </summary>
    public GraphDataType DataType { get => _dataType; set => _dataType = value; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple connections are allowed on this connector.
    /// </summary>
    public bool? AllowMultipleConnection { get => _allowMultipleConnection; set => _allowMultipleConnection = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a combined (merged) port.
    /// </summary>
    public bool IsCombined { get => _isCombined; set => _isCombined = value; }

    /// <summary>
    /// Gets whether multiple outgoing connections are allowed from this connector.
    /// </summary>
    /// <returns>True if multiple connections are allowed; otherwise, false.</returns>
    public bool GetAllowMultipleFromConnection() => _allowMultipleConnection ?? DataType?.AllowMultipleFromConnection ?? false;

    /// <summary>
    /// Gets whether multiple incoming connections are allowed to this connector.
    /// </summary>
    /// <returns>True if multiple connections are allowed; otherwise, false.</returns>
    public bool GetAllowMultipleToConnection() => _allowMultipleConnection ?? DataType?.AllowMultipleToConnection ?? false;

    /// <summary>
    /// Gets or sets a custom tag object associated with this connector.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets or sets the value used for associate link matching.
    /// </summary>
    public object AssociateValue { get; set; }

    /// <summary>
    /// Creates a new Connector, given a name, a parent container, type and index
    /// </summary>
    /// <param name="name">The display name of the connector</param>
    /// <param name="parent">Reference to the parent Node</param>
    /// <param name="direction">Type of the connector (input/output)</param>
    /// <param name="connectorIndex">Connector Index</param>
    public GraphConnector(string name, GraphNode parent, ConnectorType connectorType, GraphDirection direction, int connectorIndex)
    {
        _name = name;
        _parentNode = parent;
        _view = parent.Diagram;
        _connectorType = connectorType;
        _direction = direction;
        _connectorIndex = connectorIndex;
        _dataType = parent.Diagram.DataTypeProvider.GetDataType("Generic");
    }

    /// <summary>
    /// Creates a new Connector, given a name, a parent container, type, index and DataType
    /// </summary>
    /// <param name="name">The display name of the connector</param>
    /// <param name="parent">Reference to the parent Node</param>
    /// <param name="direction">Type of the connector (input/output)</param>
    /// <param name="connectorIndex">Connector Index</param>
    public GraphConnector(string name, GraphNode parent, ConnectorType connectorType, GraphDirection direction, int connectorIndex, string nodeGraphDataTypeName)
    {
        _name = name;
        _parentNode = parent;
        _view = parent.Diagram;
        _direction = direction;
        _connectorType = connectorType;
        _connectorIndex = connectorIndex;
        _dataType = parent.Diagram.DataTypeProvider.GetDataType(nodeGraphDataTypeName);
    }

    /// <summary>
    /// Returns the visible area of the connector
    /// </summary>
    /// <returns>a rectangle determining the visible area of the connector</returns>
    public RectangleF GetArea() => _parentNode.GetConnectorArea(this);

    /// <inheritdoc/>
    public PointF GetPosition() => _parentNode.GetConnectorPosition(this);

    /// <summary>
    /// Returns the Click Area of the connector
    /// </summary>
    /// <returns>a rectangle determining the Click area of the connector</returns>
    public RectangleF GetHitArea() => _parentNode.GetConnectorHitArea(this);


    public override string ToString()
    {
        if (_parentNode != null)
        {
            return _parentNode.Name + "." + Name;
        }
        else
        {
            return Name;
        }
    }
}