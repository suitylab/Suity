using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;


namespace Suity.Editor.Flows;


/// <summary>
/// Flow chart node
/// </summary>
public abstract class FlowNode : IViewObject, IViewEditNotify, ITextDisplay
{
    /// <summary>
    /// Action type identifier.
    /// </summary>
    public const string ACTION_TYPE = "<Action>";

    /// <summary>
    /// Event type identifier.
    /// </summary>
    public const string EVENT_TYPE = "<Event>";

    /// <summary>
    /// Unknown type identifier.
    /// </summary>
    public const string UNKNOWN_TYPE = "<Unknown>";


    private IFlowDiagramItem _diagramItem;

    private QueueOnceAction _updateConnectionAction;
    private QueueOnceAction _updateAction;

    private INamedSyncList<FlowNodeConnector> _connectors;
    private List<FlowNodeConnector> _updatingConnectors;

        // Indicates whether to build connectors statically. If static, UpdateConnectors() will not automatically add new connectors
    private bool _staticConnectorBuild;

    private int _updating;
    private string _name;

    private bool _hasActionInputConnector;
    private bool _hasActionOutputConnector;

    internal FlowNodeDrawDelegate _legacyCustomDraw;
    internal DrawFlowNodeImGui _flowNodeGui;
    internal DrawEditorImGui _editorGui;

    private readonly List<IFlowViewNode> _viewNodes = [];

    /// <summary>
    /// Initializes a new instance of the FlowNode.
    /// </summary>
    public FlowNode()
    {
        _connectors = NamedExternal._external.CreateNamedSyncList<FlowNodeConnector>("Name");
        _connectors.ItemAdded += _connectors_ItemAdded;
        _connectors.ItemRemoved += _connectors_ItemRemoved;

        // Queue update connectors during construction for subsequent initialization
        UpdateConnectorQueued();
    }

    /// <summary>
    /// Gets or sets the name of the node.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _name = value;

            DiagramItem?.Diagram?.NotifyNodeRenamed(this);
        }
    }

    /// <summary>
    /// Flow diagram item storage
    /// </summary>
    public IFlowDiagramItem DiagramItem
    {
        get => _diagramItem;
        set
        {
            if (ReferenceEquals(_diagramItem, value))
            {
                return;
            }

            _diagramItem = value;

            OnDiagramItemUpdated();
        }
    }

    /// <summary>
    /// Gets the flow diagram.
    /// </summary>
    public IFlowDiagram Diagram => _diagramItem?.Diagram;

    /// <summary>
    /// Gets the asset associated with this node.
    /// </summary>
    public Asset GetAsset() => (_diagramItem as SNamedItem)?.TargetAsset;

    #region Draw

    /// <summary>
    /// Gets or sets the custom draw delegate for the node.
    /// </summary>
    public FlowNodeDrawDelegate CustomDraw { get => _legacyCustomDraw; set => _legacyCustomDraw = value; }

    /// <summary>
    /// Gets or sets the FlowNode GUI drawer.
    /// </summary>
    public DrawFlowNodeImGui FlowNodeGui { get => _flowNodeGui; set => _flowNodeGui = value; }

    /// <summary>
    /// Gets or sets the editor GUI drawer.
    /// </summary>
    public DrawEditorImGui EditorGui { get => _editorGui; set => _editorGui = value; }
    #endregion

    #region Caching

    /// <summary>
    /// Whether the cache is expanded. Setting this has no effect. To update the view, execute <see cref="IFlowDiagramItem.SetExpanded(bool)"/>
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets the X position of the node.
    /// </summary>
    public int X => _diagramItem?.X ?? 0;

    /// <summary>
    /// Gets the Y position of the node.
    /// </summary>
    public int Y => _diagramItem?.Y ?? 0;

    /// <summary>
    /// Gets the width of the node.
    /// </summary>
    public int Width => _diagramItem?.Width ?? 0;

    /// <summary>
    /// Gets the height of the node.
    /// </summary>
    public int Height => _diagramItem?.Height ?? 0;


    /// <summary>
    /// Starts a view for this node.
    /// </summary>
    public void StartView(IFlowViewNode viewNode)
    {
        if (viewNode is null)
        {
            throw new ArgumentNullException(nameof(viewNode));
        }

        if (_viewNodes.Contains(viewNode))
        {
            return;
        }

        _viewNodes.RemoveAll(o => o.FlowView == viewNode.FlowView);

        _viewNodes.Add(viewNode);
    }

    /// <summary>
    /// Stops a view for this node.
    /// </summary>
    public void StopView(IFlowView view)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        _viewNodes.RemoveAll(o => o.FlowView == view);
    }

    /// <summary>
    /// Gets a view node for the specified view.
    /// </summary>
    public IFlowViewNode GetViewNode(IFlowView view)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        var viewNode = _viewNodes.FirstOrDefault(x => x.FlowView == view);
        if (viewNode != null)
        {
            return viewNode;
        }
        else
        {
            // Logs.LogError($"{nameof(IFlowViewNode)} not found in {nameof(FlowNode)}.");
            return null;
        }
    }

    /// <summary>
    /// Gets all view nodes for this node.
    /// </summary>
    public IEnumerable<IFlowViewNode> ViewNodes => _viewNodes.Pass();

    /// <summary>
    /// Queues a refresh for all view nodes.
    /// </summary>
    public void QueueRefreshView()
    {
        foreach (var viewNode in _viewNodes)
        {
            viewNode.QueueRefresh();
        }
    }

    #endregion

    #region Connector

    /// <summary>
    /// Gets all connectors of this node.
    /// </summary>
    public IEnumerable<FlowNodeConnector> Connectors => _connectors;

    /// <summary>
    /// Gets a connector by name.
    /// </summary>
    /// <param name="name">Connector name.</param>
    /// <returns>The connector.</returns>
    public FlowNodeConnector GetConnector(string name)
    {
        return GetConnector(name, true);
    }

    /// <summary>
    /// Gets a connector by ID.
    /// </summary>
    /// <param name="id">Connector ID.</param>
    /// <returns>The connector.</returns>
    public FlowNodeConnector GetConnector(Guid id)
    {
        return _connectors[id.ToString()];
    }

    /// <summary>
    /// Gets a connector by name, optionally resolving aliases.
    /// </summary>
    /// <param name="name">Connector name.</param>
    /// <param name="resolveAlias">Whether to resolve aliases.</param>
    /// <returns>The connector.</returns>
    public FlowNodeConnector GetConnector(string name, bool resolveAlias)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        var connector = _connectors[name];
        if (connector != null)
        {
            return connector;
        }

        if (resolveAlias)
        {
            var originName = FlowsExternal._external.ResolveConnectorName(this.GetType(), name, out _, out _);
            if (originName != null)
            {
                //Logs.LogInfo($"Port name {name} updated to: {originName}");

                connector = _connectors[originName];

                if (connector != null)
                {
                    return connector;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets whether this node has any action connector.
    /// </summary>
    public bool HasActionConnector => _hasActionInputConnector || _hasActionOutputConnector;

    /// <summary>
    /// Gets whether this node has an action input connector.
    /// </summary>
    public bool HasActionInputConnector => _hasActionInputConnector;

    /// <summary>
    /// Gets whether this node has an action output connector.
    /// </summary>
    public bool HasActionOutputConnector => _hasActionOutputConnector;

    /// <summary>
    /// Gets whether this node has an action connector in the specified direction.
    /// </summary>
    public bool GetHasActionConnector(FlowDirections direction) => direction switch
    {
        FlowDirections.Input => _hasActionInputConnector,
        FlowDirections.Output => _hasActionOutputConnector,
        _ => false,
    };

    public void UpdateConnector()
    {
        if (_staticConnectorBuild)
        {
            return;
        }

        int updating = Interlocked.CompareExchange(ref _updating, 1, 0);
        if (updating != 0)
        {
            return;
        }

        try
        {
            OnUpdateConnector();
        }
        catch (Exception err)
        {
            err.LogError();
        }

        var updatingConnectors = _updatingConnectors;
        _updatingConnectors = null;

        // Cannot skip update check because nodes can have connectors reduced to 0.
        //if (updatingConnector is null)
        //{
        //    // No trigger update
        //    _updating = 0;
        //    return;
        //}

        var oldConnectors = _connectors;

        // Override mode, do not modify original list to avoid multi-threading errors.
        var connectors = NamedExternal._external.CreateNamedSyncList<FlowNodeConnector>("Name");
        connectors.ItemAdded += _connectors_ItemAdded;
        connectors.ItemRemoved += _connectors_ItemRemoved;
        if (updatingConnectors != null)
        {
            connectors.AddRange(updatingConnectors);
        }

        _connectors = connectors;
        _hasActionInputConnector = _connectors.Any(o => o.ConnectionType == FlowConnectorTypes.Action && o.Direction == FlowDirections.Input);
        _hasActionOutputConnector = _connectors.Any(o => o.ConnectionType == FlowConnectorTypes.Action && o.Direction == FlowDirections.Output);

        if (oldConnectors != null)
        {
            oldConnectors.ItemAdded -= _connectors_ItemAdded;
            oldConnectors.ItemRemoved -= _connectors_ItemRemoved;

            // Do not modify the list because there are multi-threading issues.
        }

        _updating = 0;

        UpdateQueued();
    }

    public void UpdateConnectorQueued()
    {
        if (_staticConnectorBuild)
        {
            return;
        }

        _updateConnectionAction ??= new QueueOnceAction(() => 
        {
            _updateConnectionAction = null;
            UpdateConnector();
        });

        _updateConnectionAction.DoQueuedAction();
    }

    protected virtual void OnUpdateConnector()
    {
    }

    private void _connectors_ItemAdded(FlowNodeConnector v, bool isNew)
    {
        v.ParentNode = this;
    }

    private void _connectors_ItemRemoved(FlowNodeConnector v)
    {
        v.ParentNode = null;
    }

    protected internal void ClearConnectors()
    {
        if (_updating != 0)
        {
            _updatingConnectors ??= [];
            _updatingConnectors.Clear();
        }
        else
        {
            _connectors.Clear();
            UpdateQueued();
        }
    }

    protected internal void RemoveConnector(FlowNodeConnector connector)
    {
        if (connector is null)
        {
            return;
        }

        if (_updating != 0)
        {
            _updatingConnectors ??= [];
            _updatingConnectors.Remove(connector);
        }
        else
        {
            _connectors.Remove(connector);
            UpdateQueued();
        }
    }

    protected internal void RemoveConnector(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (_updating != 0)
        {
            _updatingConnectors ??= [];
            _updatingConnectors.RemoveAll(o => o.Name == name);
        }
        else
        {
            _connectors.RemoveByName(name);
            UpdateQueued();
        }
    }

    protected internal FlowNodeConnector AddConnector(FlowNodeConnector connector)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        if (connector.ParentNode != null && connector.ParentNode != this)
        {
            throw new ArgumentException(nameof(connector.ParentNode));
        }

        if (_updating != 0)
        {
            _updatingConnectors ??= [];
            _updatingConnectors.Add(connector);
        }
        else
        {
            // Static build connectors
            _staticConnectorBuild = true;

            connector.ParentNode = this;

            _connectors.Add(connector);
            _hasActionInputConnector = _connectors.Any(o => o.ConnectionType == FlowConnectorTypes.Action && o.Direction == FlowDirections.Input);
            _hasActionOutputConnector = _connectors.Any(o => o.ConnectionType == FlowConnectorTypes.Action && o.Direction == FlowDirections.Output);

            UpdateQueued();
        }

        return connector;
    }

    protected internal FlowNodeConnector AddActionInputConnector(string name, string description = null)
        => AddConnector(FixedNodeConnector.CreateActionInput(name, description));

    protected internal FlowNodeConnector AddActionInputConnector(Guid id, string description = null)
        => AddConnector(FixedNodeConnector.CreateActionInput(id, description));

    protected internal FlowNodeConnector AddActionOutputConnector(string name, string description = null)
        => AddConnector(FixedNodeConnector.CreateActionOutput(name, description));

    protected internal FlowNodeConnector AddActionOutputConnector(Guid id, string description = null)
        => AddConnector(FixedNodeConnector.CreateActionOutput(id, description));

    protected internal FlowNodeConnector AddDataInputConnector(string name, string dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataInput(name, dataType, description));

    protected internal FlowNodeConnector AddDataInputConnector(string name, TypeDefinition dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataInput(name, dataType, description));

    protected internal FlowNodeConnector AddDataInputConnector(Guid id, string dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataInput(id, dataType, description));

    protected internal FlowNodeConnector AddDataInputConnector(Guid id, TypeDefinition dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataInput(id, dataType, description));

    protected internal FlowNodeConnector AddDataOutputConnector(string name, string dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataOutput(name, dataType, description));

    protected internal FlowNodeConnector AddDataOutputConnector(string name, TypeDefinition dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataOutput(name, dataType, description));

    protected internal FlowNodeConnector AddDataOutputConnector(Guid id, string dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataOutput(id, dataType, description));

    protected internal FlowNodeConnector AddDataOutputConnector(Guid id, TypeDefinition dataType, string description = null)
        => AddConnector(FixedNodeConnector.CreateDataOutput(id, dataType, description));

    protected internal FlowNodeConnector AddAssociateConnector(Guid id, string dataType, FlowDirections direction, object value = null, string description = null)
    {
        var connector = AddConnector(id, dataType, direction, FlowConnectorTypes.Associate, true, description);
        connector.AssociateValue = value;

        return connector;
    }

    protected internal FlowNodeConnector AddAssociateConnector(string name, string dataType, FlowDirections direction, object value = null, string description = null)
    {
        var connector = AddConnector(name, dataType, direction, FlowConnectorTypes.Associate, true, description);
        connector.AssociateValue = value;

        return connector;
    }

    protected internal FlowNodeConnector AddAssociateInputConnector(string name, string dataType, object value = null, string description = null)
    {
        var connector = AddConnector(name, dataType, FlowDirections.Input, FlowConnectorTypes.Associate, true, description);
        connector.AssociateValue = value;

        return connector;
    }

    protected internal FlowNodeConnector AddAssociateOutputConnector(string name, string dataType, object value = null, string description = null)
    { 
        var connector = AddConnector(name, dataType, FlowDirections.Output, FlowConnectorTypes.Associate, true, description);
        connector.AssociateValue = value;

        return connector;
    }

    protected internal FixedNodeConnector AddConnector(
        string name,
        string dataTypeName,
        FlowDirections direction,
        FlowConnectorTypes connectionType = FlowConnectorTypes.Data,
        bool? allowMultipleConnection = null,
        string description = null
        )
    {
        var connector = new FixedNodeConnector(
            name,
            dataTypeName,
            direction,
            connectionType,
            allowMultipleConnection,
            description);

        AddConnector(connector);

        return connector;
    }

    protected internal FixedNodeConnector AddConnector(
        string name,
        TypeDefinition dataType,
        FlowDirections direction,
        FlowConnectorTypes connectionType = FlowConnectorTypes.Data,
        bool? allowMultipleConnection = null,
        string description = null
        )
    {
        var connector = new FixedNodeConnector(
            name,
            GetDataTypeString(dataType),
            direction,
            connectionType,
            allowMultipleConnection,
            description);

        AddConnector(connector);

        return connector;
    }

    protected internal FixedNodeConnector AddConnector(
        Guid id,
        string dataTypeName,
        FlowDirections direction,
        FlowConnectorTypes connectionType = FlowConnectorTypes.Data,
        bool? allowMultipleConnection = null,
        string description = null
    )
    {
        var connector = new FixedNodeConnector(
            id,
            dataTypeName,
            direction,
            connectionType,
            allowMultipleConnection,
            description);

        AddConnector(connector);

        return connector;
    }

    protected internal FixedNodeConnector AddConnector(
        Guid id,
        TypeDefinition dataType,
        FlowDirections direction,
        FlowConnectorTypes connectionType = FlowConnectorTypes.Data,
        bool? allowMultipleConnection = null,
        string description = null
    )
    {
        var connector = new FixedNodeConnector(
            id,
            GetDataTypeString(dataType),
            direction,
            connectionType,
            allowMultipleConnection,
            description);

        AddConnector(connector);

        return connector;
    }

    #endregion

    #region IViewObject

    /// <summary>
    /// Sets up the view for the node.
    /// </summary>
    public virtual void SetupView(IViewObjectSetup setup)
    {
        if (ShowFullTypeName)
        {
            setup.InspectorFieldOf<string>(new ViewProperty("###FullTypeName", "Full Type Name", CoreIconCache.Debug)
                .WithStatus(TextStatus.Disabled).WithReadOnly());
        }

        OnSetupView(setup);
    }

    /// <summary>
    /// Synchronizes the properties of the node.
    /// </summary>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.Intent == SyncIntent.View && ShowFullTypeName)
        {
            sync.Sync("###FullTypeName", this.GetType().FullName, SyncFlag.GetOnly);
        }

        IsExpanded = sync.Sync("Expand", IsExpanded);

        OnSync(sync, context);
    }

    /// <summary>
    /// Called during synchronization to process custom properties.
    /// </summary>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    {
    }

    /// <summary>
    /// Sets up the view properties for the inspector.
    /// </summary>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    #endregion

    #region IViewEditNotify

    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        OnViewEdited(obj, propertyName);
    }

    /// <summary>
    /// Called when a property is edited in the view.
    /// </summary>
    protected virtual void OnViewEdited(object obj, string propertyName)
    {
    }

    #endregion

    #region IDisplayText

    /// <summary>
    /// Gets the display text for the node.
    /// </summary>
    public virtual string DisplayText => EditorUtility.ToDisplayText(this.GetType());

    /// <summary>
    /// Gets the display icon for the node.
    /// </summary>
    public virtual object DisplayIcon => Icon;

    /// <summary>
    /// Gets the display status for the node.
    /// </summary>
    public virtual TextStatus DisplayStatus => TextStatus.Normal;


    #endregion

    #region Virtual

    /// <summary>
    /// Type name, used to identify this object type in options, and provides reference for <seealso cref="NamedItem.OnGetSuggestedPrefix"/>.
    /// </summary>
    public virtual string TypeName => this.GetType().Name;

    /// <summary>
    /// Gets the intuitive display name corresponding to the type name
    /// </summary>

    /// <summary>
    /// Icon
    /// </summary>
    public virtual ImageDef Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Flow;

    /// <summary>
    /// Gets the background color for the node.
    /// </summary>
    public virtual Color? BackgroundColor => null;

    /// <summary>
    /// Gets the title color for the node.
    /// </summary>
    public virtual Color? TitleColor => null;

    /// <summary>
    /// Whether it can be deleted
    /// </summary>
    public virtual bool CanBeDeleted => true;

    /// <summary>
    /// Preview computation value in view
    /// </summary>
    public virtual bool PreviewValue => true;



    /// <summary>
    /// Gets whether the node is expandable.
    /// </summary>
    public virtual bool Expandable => true;

    /// <summary>
    /// Gets the object to display when expanded.
    /// </summary>
    public virtual object ExpandedViewObject => null;


    /// <summary>
    /// Determines whether this node is a pure data node
    /// </summary>
    public virtual bool IsDataNode => !_hasActionInputConnector;

    /// <summary>
    /// Called when diagram item is set
    /// </summary>
    protected virtual void OnDiagramItemUpdated()
    {
    }

    /// <summary>
    /// Called when connection line updates
    /// </summary>
    protected internal virtual void OnLinkUpdated()
    { }

    /// <summary>
    /// Called when the node is added to a diagram.
    /// </summary>
    protected internal virtual void OnAdded()
    { }

    /// <summary>
    /// Called when the node is removed from a diagram.
    /// </summary>
    protected internal virtual void OnRemoved()
    { }

    /// <summary>
    /// Called when the node is updated.
    /// </summary>
    protected internal virtual void OnUpdated()
    { }

    /// <summary>
    /// Called when the node is loaded.
    /// </summary>
    protected internal virtual void OnLoaded()
    { }

    /// <summary>
    /// Called when the node is double-clicked.
    /// </summary>
    protected internal virtual void OnDoubleClick()
    { }

    /// <summary>
    /// Executes any pending action associated with the queued connection, if one is available.
    /// </summary>
    /// <remarks>Call this method to ensure that all queued connection-related actions are processed promptly.
    /// This is typically used to flush pending updates or operations that have been deferred.</remarks>
    public void FlushQueuedConnection()
    {
        int count = 0;

        while (_updateConnectionAction != null)
        {
            var action = _updateConnectionAction;
            _updateConnectionAction = null;
            action.DoAction();

            count++;
            if (count > 100)
            {
                // Prevent infinite loop, theoretically this should not happen.
                Logs.LogError($"{nameof(FlushQueuedConnection)} executed too many times, there may be a problem.");
                break;
            }
        }
    }

    /// <summary>
    /// Executes any pending update action.
    /// </summary>
    public void FlushQueuedUpdate()
    {
        int count = 0;

        while (_updateAction != null)
        {
            var action = _updateAction;
            _updateAction = null;
            action.DoAction();

            count++;
            if (count > 100)
            {
                // Prevent infinite loop, theoretically this should not happen.
                Logs.LogError($"{nameof(FlushQueuedUpdate)} executed too many times, there may be a problem.");
                break;
            }
        }
    }

    /// <summary>
    /// Computes the node using the provided computation context.
    /// </summary>
    public virtual void Compute(IFlowComputation compute)
    { }

    #endregion



    /// <summary>
    /// Notifies that this node has been updated, and triggers update of diagram and view
    /// </summary>
    public void UpdateQueued()
    {
        if (_updating != 0)
        {
            return;
        }

        _updateAction ??= new QueueOnceAction(() =>
        {
            _updateAction = null;

            // Update self
            OnUpdated();
            // Execute in the following order:
            // 1 Update diagram DiagramItem.NotifyNodeUpdated()
            // 2 Update diagram Diagram.NotifyNodeUpdated(node)
            // 3 Update view _view?.AddOrUpdateNode(node)
            // 4 View node restructures view connectors RebuildNode()
            // 5 View node refreshes expanded content UpdateExpandedObject()
            // 6 Request sync and draw view
            DiagramItem?.NotifyNodeUpdated();
        });

        // Solve multi-threading issue, external debugger will access data resources from other threads
        _updateAction?.DoQueuedAction();
    }

    /// <summary>
    /// Returns a string representation of the node.
    /// </summary>
    public override string ToString()
    {
        string s = DisplayText;
        if (!string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        return TypeName;
    }


    /// <summary>
    /// Global setting for whether to show the full type name of the node
    /// </summary>
    public static bool ShowFullTypeName { get; internal set; }

    /// <summary>
    /// Gets the string representation of the data port for the type definition
    /// </summary>
    /// <param name="type">The type definition</param>
    /// <returns>The type name string</returns>
    public static string GetDataTypeString(TypeDefinition type)
    {
        string str = type?.ToTypeName();
        if (string.IsNullOrWhiteSpace(str))
        {
            str = UNKNOWN_TYPE;
        }

        return str;
    }
}