using System.Drawing;

namespace Suity.Views.NodeGraph;

/// <summary>
/// GraphDataType contains information for validating links inside the graph. The class contains color information in order to
/// present nodes & links in a pleasant way.
/// GraphDataType is the abstract base for link typing. In order to use default type use GraphDataTypeBase.
/// </summary>
public abstract class GraphDataType
{
    /// <summary>
    /// Gets a value indicating whether multiple outgoing connections are allowed.
    /// </summary>
    public bool AllowMultipleFromConnection => _allowMultipleFromConnection;

    /// <summary>
    /// Gets a value indicating whether multiple incoming connections are allowed.
    /// </summary>
    public bool AllowMultipleToConnection => _allowMultipleToConnection;

    /// <summary>
    /// Gets the pen used for drawing links of this data type.
    /// </summary>
    public Pen LinkPen => _linkPen;

    /// <summary>
    /// Gets the brush used for drawing link arrows of this data type.
    /// </summary>
    public SolidBrush LinkArrowBrush => _linkArrowBrush;

    /// <summary>
    /// Gets the pen used for drawing connector outlines of this data type.
    /// </summary>
    public Pen ConnectorOutlinePen => _connectorOutlinePen;

    /// <summary>
    /// Gets the brush used for filling connectors of this data type.
    /// </summary>
    public SolidBrush ConnectorFillBrush => _connectorFillBrush;

    /// <summary>
    /// Gets the name of this data type.
    /// </summary>
    public string TypeName => _typeName;

    /// <summary>
    /// Gets a value indicating whether this data type represents an array.
    /// </summary>
    public bool IsArray => _isArray;

    /// <summary>
    /// Gets a value indicating whether this data type represents a key.
    /// </summary>
    public bool IsKey => _isKey;

    protected bool _allowMultipleFromConnection;
    protected bool _allowMultipleToConnection;

    protected Pen _linkPen;
    protected SolidBrush _linkArrowBrush;
    protected Pen _connectorOutlinePen;
    protected SolidBrush _connectorFillBrush;

    protected string _typeName;
    protected bool _isArray;
    protected bool _isKey;
}

/// <summary>
/// GraphDataTypeBase is the generic GraphDataType for link typing. It has a name of "Generic" and colored feedback as black.
/// It serves as a base for generic node creation and as an example implementation. As it is the base this type is default registered into GraphView.KnownDataTypes
/// </summary>
public class GraphDataTypeBase : GraphDataType
{
    public static GraphDataTypeBase Instance { get; } = new();

    public GraphDataTypeBase()
    {
        _linkPen = new Pen(Color.FromArgb(120, 120, 120));
        _linkArrowBrush = new SolidBrush(Color.FromArgb(120, 120, 120));
        _connectorOutlinePen = new Pen(Color.FromArgb(60, 60, 60));
        _connectorFillBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
        _typeName = "Generic";
    }

    public override string ToString()
    {
        return _typeName;
    }
}