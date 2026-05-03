using Suity.Views.NodeGraph;
using Suity.Collections;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Drawing;
using Suity.Drawing;

namespace Suity.Editor.Flows.Gui;

#region ActionNodeGraphDataType

/// <summary>
/// Represents the graph data type for action connectors in flow diagrams.
/// </summary>
public class ActionNodeGraphDataType : GraphDataType
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ActionNodeGraphDataType"/>.
    /// </summary>
    public static ActionNodeGraphDataType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionNodeGraphDataType"/> class.
    /// </summary>
    public ActionNodeGraphDataType()
    {
        _allowMultipleToConnection = true;
        _linkPen = new PenDef(Color.FromArgb(255, 255, 255), 4);
        _linkArrowBrush = new SolidBrushDef(Color.FromArgb(255, 255, 255));
        _connectorOutlinePen = new PenDef(Color.FromArgb(255, 255, 255), 3);
        _connectorFillBrush = new SolidBrushDef(Color.FromArgb(255, 255, 255));
        _typeName = FlowNode.ACTION_TYPE;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Action";
    }
}

#endregion

#region EventNodeGraphDataType

/// <summary>
/// Represents the graph data type for event connectors in flow diagrams.
/// </summary>
public class EventNodeGraphDataType : GraphDataType
{
    /// <summary>
    /// Gets the singleton instance of <see cref="EventNodeGraphDataType"/>.
    /// </summary>
    public static EventNodeGraphDataType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventNodeGraphDataType"/> class.
    /// </summary>
    public EventNodeGraphDataType()
    {
        _allowMultipleToConnection = true;
        _linkPen = new PenDef(Color.FromArgb(255, 255, 0), 4);
        _linkArrowBrush = new SolidBrushDef(Color.FromArgb(255, 255, 0));
        _connectorOutlinePen = new PenDef(Color.FromArgb(255, 255, 0), 3);
        _connectorFillBrush = new SolidBrushDef(Color.FromArgb(255, 255, 0));
        _typeName = FlowNode.EVENT_TYPE;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Event";
    }
}

#endregion

#region UnknownNodeGraphDataType

/// <summary>
/// Represents the graph data type for unknown or unresolved connectors in flow diagrams.
/// </summary>
public class UnknownNodeGraphDataType : GraphDataType
{
    /// <summary>
    /// Gets the singleton instance of <see cref="UnknownNodeGraphDataType"/>.
    /// </summary>
    public static UnknownNodeGraphDataType Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownNodeGraphDataType"/> class.
    /// </summary>
    public UnknownNodeGraphDataType()
    {
        _linkPen = new PenDef(Color.FromArgb(255, 0, 0), 3);
        _linkArrowBrush = new SolidBrushDef(Color.FromArgb(255, 0, 0));
        _connectorOutlinePen = new PenDef(Color.FromArgb(255, 0, 0), 3);
        _connectorFillBrush = new SolidBrushDef(Color.FromArgb(180, 0, 0));
        _typeName = FlowNode.UNKNOWN_TYPE;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _typeName;
    }
}

#endregion

#region StyledDataType

/// <summary>
/// Represents a graph data type that is styled based on an <see cref="IFlowDataStyle"/>.
/// </summary>
public class StyledDataType : GraphDataType
{
    private readonly IFlowDataStyle _style;

    /// <summary>
    /// Initializes a new instance of the <see cref="StyledDataType"/> class.
    /// </summary>
    /// <param name="style">The flow data style to apply.</param>
    public StyledDataType(IFlowDataStyle style)
    {
        _style = style ?? throw new ArgumentNullException(nameof(style));
        _style.StyleUpdated += _style_StyleUpdated;

        UpdateStyle();
    }

    /// <summary>
    /// Gets the underlying flow data style.
    /// </summary>
    public IFlowDataStyle Sytle => _style;

    private void _style_StyleUpdated(object sender, EventArgs e)
    {
        UpdateStyle();
    }

    /// <summary>
    /// Updates the visual style properties from the underlying <see cref="IFlowDataStyle"/>.
    /// </summary>
    public void UpdateStyle()
    {
        _allowMultipleFromConnection = _style.MultipleFromConnection;
        _allowMultipleToConnection = _style.MultipleToConnection;
        _linkPen = _style.LinkPen;
        _linkArrowBrush = _style.LinkArrowBrush;
        _connectorOutlinePen = _style.ConnectorOutlinePen;
        _connectorFillBrush = _style.ConnectorFillBrush;
        _typeName = _style.TypeName;
        _isArray = _style.IsArray;
        _isKey = _style.IsKey;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _style.DisplayName;
    }
}

#endregion

#region TypeDefinitionDataType

/// <summary>
/// Represents a graph data type based on a <see cref="TypeDefinition"/>.
/// </summary>
public class TypeDefinitionDataType : StyledDataType
{
    /// <summary>
    /// Gets the type definition associated with this data type.
    /// </summary>
    public TypeDefinition TypeDef { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDefinitionDataType"/> class.
    /// </summary>
    /// <param name="dataType">The type definition to represent.</param>
    public TypeDefinitionDataType(TypeDefinition dataType)
        : base(new TypeDefinitionFlowDataStyle(dataType, true, false))
    {
        TypeDef = dataType;
    }
}

#endregion

#region CustomGraphDataType

/// <summary>
/// Abstract base class for custom graph data types that can be discovered and registered dynamically.
/// </summary>
//TODO: Should be publicly exposed to allow external inheritance to implement multiple data types
public abstract class CustomGraphDataType : GraphDataType
{
    static Dictionary<string, CustomGraphDataType> _types;

    /// <summary>
    /// Gets a custom graph data type by its name. Types are discovered dynamically from derived types.
    /// </summary>
    /// <param name="name">The type name to look up.</param>
    /// <returns>The matching <see cref="CustomGraphDataType"/>, or <c>null</c> if not found.</returns>
    public static CustomGraphDataType GetCustomDataType(string name)
    {
        if (_types != null)
        {
            return _types.GetValueSafe(name);
        }

        _types = [];

        foreach (var type in typeof(CustomGraphDataType).GetDerivedTypes())
        {
            try
            {
                var dataType = (CustomGraphDataType)type.CreateInstanceOf();
                string typeName = dataType.TypeName;
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    _types[typeName] = dataType;
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return _types.GetValueSafe(name);
    }
}

#endregion
