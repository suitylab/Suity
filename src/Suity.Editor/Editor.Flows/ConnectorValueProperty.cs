using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Flows;

#region ConnectorValueProperty

/// <summary>
/// Connector value property
/// </summary>
public class ConnectorValueProperty<T> : ValueProperty<T>
{
    /// <summary>
    /// Gets the type definition.
    /// </summary>
    public TypeDefinition Type { get; } = TypeDefinition.FromNative<T>();

    /// <summary>
    /// Gets the connector.
    /// </summary>
    public FlowNodeConnector Connector { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ConnectorValueProperty.
    /// </summary>
    public ConnectorValueProperty(string name, string description = null, T value = default, string toolTips = null)
        : base(name, description, value, toolTips)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConnectorValueProperty.
    /// </summary>
    public ConnectorValueProperty(ViewProperty property)
        : base(property)
    {
    }

    /// <summary>
    /// This method should not be called.
    /// </summary>
    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an inspector field for the property.
    /// </summary>
    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    /// <summary>
    /// Adds a connector to the node.
    /// </summary>
    public virtual void AddConnector(FlowNode node)
    {
        Connector = node.AddDataInputConnector(Property.Name, Type.ToTypeName(), Property.Description);
    }

    /// <summary>
    /// This property should not be accessed.
    /// </summary>
    [Obsolete("This property should not be accessed", true)]
    public new T Value { get; }

    /// <summary>
    /// Gets the base value.
    /// </summary>
    public T BaseValue => base.Value;

    /// <summary>
    /// Gets the value from computation.
    /// </summary>
    public T GetValue(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram
            && Connector is { } connector
            && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<T>(connector);
        }
        else
        {
            return base.Value;
        }
    }

    /// <summary>
    /// Gets the value from diagram.
    /// </summary>
    public T GetValue(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<T>(connector);
        }
        else
        {
            return base.Value;
        }
    }

    /// <summary>
    /// Gets whether the connector is linked.
    /// </summary>
    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

#endregion

#region ConnectorListProperty

/// <summary>
/// Connector list property
/// </summary>
public class ConnectorListProperty<T> : ListProperty<T>
{
    /// <summary>
    /// Gets the type definition.
    /// </summary>
    public TypeDefinition Type { get; } = TypeDefinition.FromNative<T>();

    /// <summary>
    /// Gets the array type definition.
    /// </summary>
    public TypeDefinition ArrayType { get; } = TypeDefinition.FromNative<T>().MakeArrayType();

    /// <summary>
    /// Gets the connector.
    /// </summary>
    public FlowNodeConnector Connector { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ConnectorListProperty.
    /// </summary>
    public ConnectorListProperty(string name, string description = null, string toolTips = null)
        : base(name, description, toolTips)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConnectorListProperty.
    /// </summary>
    public ConnectorListProperty(ViewProperty property)
        : base(property)
    {
    }

    /// <summary>
    /// This method should not be called.
    /// </summary>
    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an inspector field for the property.
    /// </summary>
    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }


    /// <summary>
    /// Adds a connector to the node.
    /// </summary>
    public void AddConnector(FlowNode node)
    {
        // Originally using ConnectorType, but as an asset it will always have an Id
        Connector = node.AddDataInputConnector(Property.Name, ArrayType.ToTypeName(), Property.Description);
    }

    /// <summary>
    /// This property should not be accessed.
    /// </summary>
    [Obsolete("This property should not be accessed", true)]
    public new List<T> List { get; }

    /// <summary>
    /// Gets the base list.
    /// </summary>
    public List<T> BaseList => base.List;

    /// <summary>
    /// Gets values from computation.
    /// </summary>
    public IEnumerable<T> GetValues(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.List;
        }
    }

    /// <summary>
    /// Gets values from diagram.
    /// </summary>
    public IEnumerable<T> GetValues(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.List;
        }
    }

    /// <summary>
    /// Gets whether the connector is linked.
    /// </summary>
    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

#endregion

#region ConnectorStringProperty

/// <summary>
/// Connector string property
/// </summary>
public class ConnectorStringProperty : StringProperty
{
    /// <summary>
    /// Gets the connector.
    /// </summary>
    public FlowNodeConnector Connector { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ConnectorStringProperty.
    /// </summary>
    public ConnectorStringProperty(string name, string description = null, string defaultValue = null, string toolTips = null)
        : base(name, description, defaultValue ?? string.Empty, toolTips)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConnectorStringProperty.
    /// </summary>
    public ConnectorStringProperty(ViewProperty property)
        : base(property)
    {
    }

    /// <summary>
    /// This method should not be called.
    /// </summary>
    [Obsolete("This method should not be called", true)]
    public new ViewProperty InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an inspector field for the property.
    /// </summary>
    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    /// <summary>
    /// Adds a connector to the node.
    /// </summary>
    public virtual void AddConnector(FlowNode node)
    {
        Connector = node.AddDataInputConnector(Property.Name, "string", Property.Description);
    }

    /// <summary>
    /// Gets the value from computation.
    /// </summary>
    public string GetValue(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<string>(connector);
        }
        else
        {
            return base.Value ?? string.Empty; ;
        }
    }

    /// <summary>
    /// Gets the value from diagram.
    /// </summary>
    public string GetValue(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<string>(connector);
        }
        else
        {
            return base.Value ?? string.Empty;
        }
    }

    /// <summary>
    /// This property should not be accessed.
    /// </summary>
    [Obsolete("This property should not be accessed", true)]
    public new string Value => throw new NotImplementedException();

    /// <summary>
    /// Gets the base value.
    /// </summary>
    public string BaseValue => base.Value;

    /// <summary>
    /// Gets whether the connector is linked.
    /// </summary>
    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

#endregion

#region ConnectorTextBlockProperty

public class ConnectorTextBlockProperty : TextBlockProperty
{
    public FlowNodeConnector Connector { get; private set; }

    public ConnectorTextBlockProperty(string name, string description = null, string defaultValue = null, string toolTips = null)
        : base(name, description, defaultValue, toolTips)
    {
    }

    public ConnectorTextBlockProperty(ViewProperty property)
        : base(property)
    {
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }


    public virtual void AddConnector(FlowNode node)
    {
        Connector = node.AddDataInputConnector(Property.Name, "string", Property.Description);
    }

    public string GetText(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<string>(connector);
        }
        else
        {
            return base.Value?.Text ?? string.Empty; ;
        }
    }

    public string GetText(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<string>(connector);
        }
        else
        {
            return base.Value?.Text ?? string.Empty;
        }
    }

    [Obsolete("This property should not be accessed", true)]
    public new string Value => throw new NotImplementedException();

    [Obsolete("This property should not be accessed", true)]
    public new string Text => throw new NotImplementedException();

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

#endregion

#region ConnectorSKeyProperty

public class ConnectorSKeysProperty<T> : SKeyArrayProperty<T>
    where T : SObjectController
{
    public FlowNodeConnector Connector { get; private set; }

    public ConnectorSKeysProperty(string name, string description = null, string toolTips = null)
        : base(name, description, toolTips)
    {
    }

    public ConnectorSKeysProperty(ViewProperty property)
        : base(property)
    {
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    public void AddConnector(FlowNode node)
    {
        var type = DataType;
        if (type.IsDataLink)
        {
            type = type.ElementType;
        }

        var aryType = type.MakeArrayType();

        Connector = node.AddDataInputConnector(Property.Name, aryType.ToTypeName(), Property.Description);
    }

    [Obsolete("This property should not be accessed", true)]
    public new SArray Array { get; }

    public SArray BaseArray => base.Array;

    [Obsolete("This property should not be accessed", true)]
    public new IEnumerable<T> Items { get; }

    public IEnumerable<T> BaseItems => base.Items;


    public IEnumerable<T> GetValues(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.Items;
        }
    }

    public IEnumerable<T> GetValues(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.Items;
        }
    }

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

public class ConnectorSKeysProperty<T, TConnector> : SKeyArrayProperty<T>
    where T : SObjectController
{
    public TypeDefinition ConnectorType { get; }

    public FlowNodeConnector Connector { get; private set; }

    public ConnectorSKeysProperty(string name, string description = null, string toolTips = null)
        : base(name, description, toolTips)
    {
        ConnectorType = TypeDefinition.FromNative<TConnector>();
    }

    public ConnectorSKeysProperty(ViewProperty property)
        : base(property)
    {
        ConnectorType = TypeDefinition.FromNative<TConnector>();
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    public void AddConnector(FlowNode node)
    {
        var type = ConnectorType ?? DataType;
        if (type.IsDataLink)
        {
            type = type.ElementType;
        }

        var aryType = type.MakeArrayType();

        Connector = node.AddDataInputConnector(Property.Name, aryType.ToTypeName(), Property.Description);
    }

    [Obsolete("This property should not be accessed", true)]
    public new SArray Array { get; }

    public SArray BaseArray => base.Array;

    [Obsolete("This property should not be accessed", true)]
    public new IEnumerable<T> Items { get; }

    public IEnumerable<T> BaseItems => base.Items;


    public IEnumerable<TConnector> GetValues(IFlowComputation compute, FlowNode node, Func<T, TConnector> convert)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<TConnector>(connector, true) ?? [];
        }
        else
        {
            return base.Items.Select(o => convert(o));
        }
    }

    public IEnumerable<TConnector> GetValues(IFlowComputation compute, IFlowDiagram diagram, Func<T, TConnector> convert)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<TConnector>(connector, true) ?? [];
        }
        else
        {
            return base.Items.Select(o => convert(o));
        }
    }

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}
#endregion

#region ConnectorAssetProperty

public class ConnectorAssetProperty<T> : AssetProperty<T>
 where T : class
{
    public TypeDefinition AssetLinkType { get; }

    public TypeDefinition ConnectorType { get; }

    public FlowNodeConnector Connector { get; private set; }

    public ConnectorAssetProperty(string name, string description = null, string toolTips = null)
        : base(name, description, toolTips)
    {
        AssetLinkType = AssetManager.Instance.GetAssetLink<T>()?.Definition ?? TypeDefinition.Empty;
        ConnectorType = TypeDefinition.FromNative<T>() ?? AssetLinkType;
    }

    public ConnectorAssetProperty(ViewProperty property)
        : base(property)
    {
        AssetLinkType = AssetManager.Instance.GetAssetLink<T>()?.Definition ?? TypeDefinition.Empty;
        ConnectorType = TypeDefinition.FromNative<T>() ?? AssetLinkType;
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup);
    }

    public virtual void AddConnector(FlowNode node)
    {
        // Originally using ConnectorType, but as an asset it will always have an Id
        Connector = node.AddDataInputConnector(Property.Name, AssetLinkType, Property.Description);
    }

    [Obsolete("This property should not be accessed", true)]
    public new T Target { get; }

    public T BaseTarget => base.Target;

    public T GetTarget(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<T>(connector);
        }
        else
        {
            return base.Target;
        }
    }

    public T GetTarget(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<T>(connector);
        }
        else
        {
            return base.Target;
        }
    }

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

public class ConnectorAssetProperty<T, TConnector> : AssetProperty<T>
     where T : class
{
    public TypeDefinition AssetLinkType { get; }

    public TypeDefinition ConnectorType { get; }

    public FlowNodeConnector Connector { get; private set; }

    public ConnectorAssetProperty(string name, string description = null, string toolTips = null)
        : base(name, description, toolTips)
    {
        AssetLinkType = AssetManager.Instance.GetAssetLink<T>()?.Definition ?? TypeDefinition.Empty;
        ConnectorType = TypeDefinition.FromNative<TConnector>();
    }

    public ConnectorAssetProperty(ViewProperty property)
        : base(property)
    {
        AssetLinkType = AssetManager.Instance.GetAssetLink<T>()?.Definition ?? TypeDefinition.Empty;
        ConnectorType = TypeDefinition.FromNative<TConnector>();
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    public virtual void AddConnector(FlowNode node)
    {
        Connector = node.AddDataInputConnector(Property.Name, ConnectorType.ToTypeName(), Property.Description);
    }

    [Obsolete("This property should not be accessed", true)]
    public new T Target { get; }

    public T BaseTarget => base.Target;

    public TConnector GetTarget(IFlowComputation compute, FlowNode node, Func<T, TConnector> convert)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<TConnector>(connector);
        }
        else
        {
            return base.Target is { } target ? convert(target) : default;
        }
    }

    public TConnector GetTarget(IFlowComputation compute, IFlowDiagram diagram, Func<T, TConnector> convert)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValue<TConnector>(connector);
        }
        else
        {
            return base.Target is { } target ? convert(target) : default;
        }
    }

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

public class ConnectorSAssetKeysProperty<T> : SAssetKeyArrayProperty<T>
    where T : class
{
    public FlowNodeConnector Connector { get; private set; }

    public ConnectorSAssetKeysProperty(string name, string description = null)
        : base(name, description)
    {
    }

    public ConnectorSAssetKeysProperty(ViewProperty property)
        : base(property)
    {
    }

    [Obsolete("This method should not be called", true)]
    public new void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        throw new NotImplementedException();
    }

    public void InspectorField(IViewObjectSetup setup, FlowNode node, Action<ViewProperty> config = null)
    {
        Property.ConfigConnected(node.Diagram, Connector);

        base.InspectorField(setup, config);
    }

    public void AddConnector(FlowNode node)
    {
        var type = Type;
        if (type.IsAssetLink)
        {
            type = type.ElementType;
        }

        var aryType = type.MakeArrayType();

        Connector = node.AddDataInputConnector(Property.Name, aryType.ToTypeName(), Property.Description);
    }

    [Obsolete("This property should not be accessed", true)]
    public new SArray Array { get; }

    public SArray BaseArray => base.Array;

    [Obsolete("This property should not be accessed", true)]
    public new IEnumerable<T> Items { get; }

    public IEnumerable<T> BaseItems => base.Items;


    public IEnumerable<T> GetValues(IFlowComputation compute, FlowNode node)
    {
        if (node.Diagram is { } diagram && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.Items;
        }
    }

    public IEnumerable<T> GetValues(IFlowComputation compute, IFlowDiagram diagram)
    {
        if (diagram is { } && Connector is { } connector && diagram.GetIsLinked(connector))
        {
            return compute.GetValues<T>(connector, true) ?? [];
        }
        else
        {
            return base.Items;
        }
    }

    public bool GetIsLinked(FlowNode node) => node.Diagram?.GetIsLinked(Connector) == true;
}

#endregion

#region ConnectorPropertyExtensions

static class ConnectorPropertyExtensions
{
    public static void ConfigConnected(this ViewProperty property, IFlowDiagram diagram, FlowNodeConnector connector)
    {
        bool connected = diagram?.GetIsLinked(connector) == true;

        property.ReadOnly = connected;
        property.Status = connected ? TextStatus.Reference : TextStatus.Normal;
        property.Icon = connected ? CoreIconCache.Connected : CoreIconCache.Connect;
    }
}

#endregion
