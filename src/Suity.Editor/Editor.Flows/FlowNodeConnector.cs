using Suity.Editor.Types;
using Suity.Synchonizing;
using System;

namespace Suity.Editor.Flows;

/// <summary>
/// Base class for flow node connectors.
/// </summary>
public abstract class FlowNodeConnector : ISyncObject
{
    /// <summary>
    /// Initializes a new instance of the FlowNodeConnector.
    /// </summary>
    protected FlowNodeConnector()
    {
    }

    /// <summary>
    /// Gets or sets the parent node.
    /// </summary>
    public FlowNode ParentNode { get; internal set; }

    /// <summary>
    /// Gets the direction of the connector.
    /// </summary>
    public abstract FlowDirections Direction { get; }

    /// <summary>
    /// Gets the type of connection.
    /// </summary>
    public abstract FlowConnectorTypes ConnectionType { get; }

    /// <summary>
    /// Gets whether multiple connections are allowed.
    /// </summary>
    public abstract bool? AllowMultipleConnection { get; }

    /// <summary>
    /// Gets the name of the connector.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the description of the connector.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the data type name.
    /// </summary>
    public abstract string DataTypeName { get; }

    /// <summary>
    /// Gets whether the connector is valid.
    /// </summary>
    public virtual bool IsValid => true;

    /// <summary>
    /// Synchronizes the connector properties.
    /// </summary>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <summary>
    /// Gets the exported name of the connector.
    /// </summary>
    public virtual string GetExportedName() => Name;

    /// <summary>
    /// Gets whether this is a combined connector (action with custom data type).
    /// </summary>
    public bool IsCombined => ConnectionType == FlowConnectorTypes.Action && DataTypeName != FlowNode.ACTION_TYPE;

    /// <summary>
    /// Gets the display name of the connector.
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Description))
            {
                return Description;
            }

            return Name;
        }
    }

    /// <summary>
    /// Used to store associated values
    /// </summary>
    public object AssociateValue { get; set; }

    /// <summary>
    /// Returns a string representation of the connector.
    /// </summary>
    public override string ToString()
    {
        if (ParentNode != null)
        {
            return $"{ParentNode.Name}-{Name}";
        }
        else
        {
            return Name;
        }
    }
}

/// <summary>
/// Fixed node connector
/// </summary>
public class FixedNodeConnector : FlowNodeConnector
{
    private readonly FlowDirections _direction;
    private readonly FlowConnectorTypes _connectionType;
    private readonly bool? _allowMultipleConnection;
    private readonly Guid? _id;
    private readonly string _name;
    private string _description;
    private string _dataTypeName;

    /// <summary>
    /// Gets the direction.
    /// </summary>
    public override FlowDirections Direction => _direction;

    /// <summary>
    /// Gets the connection type.
    /// </summary>
    public override FlowConnectorTypes ConnectionType => _connectionType;

    /// <summary>
    /// Gets whether multiple connections are allowed.
    /// </summary>
    public override bool? AllowMultipleConnection => _allowMultipleConnection;

    /// <summary>
    /// Gets the name.
    /// </summary>
    public override string Name
    {
        get
        {
            if (_id is Guid id)
            {
                return id.ToString();
            }
            else
            {
                return _name ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description => _description;

    /// <summary>
    /// Gets the data type name.
    /// </summary>
    public override string DataTypeName => _dataTypeName;

    public FixedNodeConnector(
        string name,
        string dataTypeName,
        FlowDirections direction,
        FlowConnectorTypes connectionType,
        bool? allowMultipleConnection = null,
        string description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        _name = name;
        _dataTypeName = dataTypeName;
        _direction = direction;
        _connectionType = connectionType;
        _description = description;
        _allowMultipleConnection = allowMultipleConnection;
    }

    public FixedNodeConnector(
        Guid id,
        string dataTypeName,
        FlowDirections direction,
        FlowConnectorTypes connectionType,
        bool? allowMultipleConnection = null,
        string description = null)
    {
        _id = id;
        _dataTypeName = dataTypeName;
        _direction = direction;
        _connectionType = connectionType;
        _description = description;
        _allowMultipleConnection = allowMultipleConnection;
    }

    public FixedNodeConnector(
        string name,
        TypeDefinition dataType,
        FlowDirections direction,
        FlowConnectorTypes connectionType,
        bool? allowMultipleConnection = null,
        string description = null)
    {
        _name = name;
        _dataTypeName = FlowNode.GetDataTypeString(dataType);
        _direction = direction;
        _connectionType = connectionType;
        _description = description;
        _allowMultipleConnection = allowMultipleConnection;
    }

    public FixedNodeConnector(
        Guid id,
        TypeDefinition dataType,
        FlowDirections direction,
        FlowConnectorTypes connectionType,
        bool? allowMultipleConnection = null,
        string description = null)
    {
        _id = id;
        _dataTypeName = FlowNode.GetDataTypeString(dataType);
        _direction = direction;
        _connectionType = connectionType;
        _description = description;
        _allowMultipleConnection = allowMultipleConnection;
    }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        // The synchronization here is mainly to fulfill the functionality of INamedSyncList<NodeConnector>.

        if (_id is { } id)
        {
            //sync.SyncId(ref id, context);
            sync.Sync(nameof(Name), id.ToString(), SyncFlag.NotNull);
        }
        else
        {
            sync.Sync(nameof(Name), _name, SyncFlag.NotNull | SyncFlag.GetOnly);
            //if (name != _name)
            //{
            //    Logs.LogWarning($"Connector {ParentNode?.GetType().Name}-{_name} renamed to {name}");

            //    _name = name;
            //}
        }
    }

    public override string GetExportedName()
    {
        if (_id is { } id)
        {
            string name = AssetManager.Instance.GetAsset(id)?.AssetKey;
            if (string.IsNullOrWhiteSpace(name))
            {
                var obj = EditorObjectManager.Instance.GetObject(id);
                if (obj != null)
                {
                    name = obj.FullName;
                }
                else
                {
                    name = GlobalIdResolver.RevertResolve(id);
                }
            }

            return name ?? string.Empty;
        }
        else
        {
            return _name;
        }
    }

    //TODO: Fixed should remove the UpdateDataType mechanism
    /// <summary>
    /// Dynamically update data type
    /// </summary>
    /// <param name="dataTypeName"></param>
    public void UpdateDataType(string dataTypeName)
    {
        _dataTypeName = dataTypeName;
        if (string.IsNullOrWhiteSpace(_dataTypeName))
        {
            _dataTypeName = FlowNode.UNKNOWN_TYPE;
        }

        ParentNode?.UpdateQueued();
    }

    /// <summary>
    /// Dynamically update data type
    /// </summary>
    /// <param name="dataType"></param>
    public void UpdateDataType(TypeDefinition dataType)
    {
        _dataTypeName = FlowNode.GetDataTypeString(dataType);

        ParentNode?.UpdateQueued();
    }

    /// <summary>
    /// Dynamically update description
    /// </summary>
    /// <param name="description"></param>
    public void UpdateDescription(string description)
    {
        _description = description;

        ParentNode?.DiagramItem?.NotifyNodeUpdated();
    }


    public static FixedNodeConnector CreateActionInput(string name, string description = null)
        => new(name, FlowNode.ACTION_TYPE, FlowDirections.Input, FlowConnectorTypes.Action, true, description);

    public static FixedNodeConnector CreateActionInput(Guid id, string description = null)
        => new(id, FlowNode.ACTION_TYPE, FlowDirections.Input, FlowConnectorTypes.Action, true, description);

    public static FixedNodeConnector CreateActionOutput(string name, string description = null)
        => new(name, FlowNode.ACTION_TYPE, FlowDirections.Output, FlowConnectorTypes.Action, false, description);

    public static FixedNodeConnector CreateActionOutput(Guid id, string description = null)
        => new(id, FlowNode.ACTION_TYPE, FlowDirections.Output, FlowConnectorTypes.Action, false, description);



    public static FixedNodeConnector CreateDataInput(string name, string dataType, string description = null)
        => new(name, dataType, FlowDirections.Input, FlowConnectorTypes.Data, false, description);

    public static FixedNodeConnector CreateDataInput(string name, TypeDefinition dataType, string description = null)
        => new(name, FlowNode.GetDataTypeString(dataType), FlowDirections.Input, FlowConnectorTypes.Data, false, description);

    public static FixedNodeConnector CreateDataInput(Guid id, string dataType, string description = null)
        => new(id, dataType, FlowDirections.Input, FlowConnectorTypes.Data, false, description);

    public static FixedNodeConnector CreateDataInput(Guid id, TypeDefinition dataType, string description = null)
        => new(id, FlowNode.GetDataTypeString(dataType), FlowDirections.Input, FlowConnectorTypes.Data, false, description);



    public static FixedNodeConnector CreateDataOutput(string name, string dataType, string description = null)
        => new(name, dataType, FlowDirections.Output, FlowConnectorTypes.Data, true, description);

    public static FixedNodeConnector CreateDataOutput(string name, TypeDefinition dataType, string description = null)
        => new(name, FlowNode.GetDataTypeString(dataType), FlowDirections.Output, FlowConnectorTypes.Data, true, description);

    public static FixedNodeConnector CreateDataOutput(Guid id, string dataType, string description = null)
        => new(id, dataType, FlowDirections.Output, FlowConnectorTypes.Data, true, description);

    public static FixedNodeConnector CreateDataOutput(Guid id, TypeDefinition dataType, string description = null)
        => new(id, FlowNode.GetDataTypeString(dataType), FlowDirections.Output, FlowConnectorTypes.Data, true, description);



    public static FixedNodeConnector CreateControlInput(string name, string dataType, string description = null)
        => new(name, dataType, FlowDirections.Input, FlowConnectorTypes.Control, false, description);

    public static FixedNodeConnector CreateControlInput(string name, TypeDefinition dataType, string description = null)
        => new(name, FlowNode.GetDataTypeString(dataType), FlowDirections.Input, FlowConnectorTypes.Control, false, description);

    public static FixedNodeConnector CreateControlInput(Guid id, string dataType, string description = null)
        => new(id, dataType, FlowDirections.Input, FlowConnectorTypes.Control, false, description);

    public static FixedNodeConnector CreateControlInput(Guid id, TypeDefinition dataType, string description = null)
        => new(id, FlowNode.GetDataTypeString(dataType), FlowDirections.Input, FlowConnectorTypes.Control, false, description);



    public static FixedNodeConnector CreateControlOutput(string name, string dataType, string description = null)
        => new(name, dataType, FlowDirections.Output, FlowConnectorTypes.Control, true, description);

    public static FixedNodeConnector CreateControlOutput(string name, TypeDefinition dataType, string description = null)
        => new(name, FlowNode.GetDataTypeString(dataType), FlowDirections.Output, FlowConnectorTypes.Control, true, description);

    public static FixedNodeConnector CreateControlOutput(Guid id, string dataType, string description = null)
        => new(id, dataType, FlowDirections.Output, FlowConnectorTypes.Control, true, description);

    public static FixedNodeConnector CreateControlOutput(Guid id, TypeDefinition dataType, string description = null)
        => new(id, FlowNode.GetDataTypeString(dataType), FlowDirections.Output, FlowConnectorTypes.Control, true, description);
}