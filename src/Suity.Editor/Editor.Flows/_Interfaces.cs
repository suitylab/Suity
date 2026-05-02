using Suity.Editor.AIGC;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows;

#region IFlowDiagram

/// <summary>
/// Event arguments for connector rename events.
/// </summary>
public class ConnectorRenamedEventArgs(FlowNode node, string oldName, string newName) : EventArgs
{
    /// <summary>
    /// Gets the flow node that contains the connector.
    /// </summary>
    public FlowNode Node { get; } = node;

    /// <summary>
    /// Gets the old name of the connector.
    /// </summary>
    public string OldName { get; } = oldName;

    /// <summary>
    /// Gets the new name of the connector.
    /// </summary>
    public string NewName { get; } = newName;
}

/// <summary>
/// Flow chart diagram view
/// </summary>
public interface IFlowDiagram
{
    /// <summary>
    /// Occurs when a node is added or updated.
    /// </summary>
    event EventHandler<FlowNode> NodeAddedOrUpdated;

    /// <summary>
    /// Occurs when a node is removed.
    /// </summary>
    event EventHandler<FlowNode> NodeRemoved;

    /// <summary>
    /// Occurs when a connector is renamed.
    /// </summary>
    event EventHandler<ConnectorRenamedEventArgs> ConnectorRenamed;

    /// <summary>
    /// Occurs when a link is added.
    /// </summary>
    event EventHandler<NodeLink> LinkAdded;

    /// <summary>
    /// Occurs when a link is removed.
    /// </summary>
    event EventHandler<NodeLink> LinkRemoved;

    /// <summary>
    /// Gets the name of the diagram.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the document content.
    /// </summary>
    object DocumentContent { get; }

    /// <summary>
    /// Gets the number of nodes.
    /// </summary>
    int NodeCount { get; }

    /// <summary>
    /// Gets all nodes.
    /// </summary>
    IEnumerable<FlowNode> Nodes { get; }

    /// <summary>
    /// Gets all links.
    /// </summary>
    IEnumerable<NodeLink> Links { get; }

    /// <summary>
    /// Gets or sets the grid span.
    /// </summary>
    int GridSpan { get; set; }



    /// <summary>
    /// Starts a view for the diagram.
    /// </summary>
    /// <param name="view">The flow view to start.</param>
    void StartView(IFlowView view);

    /// <summary>
    /// Stops a view for the diagram.
    /// </summary>
    /// <param name="view">The flow view to stop.</param>
    void StopView(IFlowView view);

    /// <summary>
    /// Gets all views.
    /// </summary>
    IEnumerable<IFlowView> Views { get; }



    /// <summary>
    /// Adds a node to the diagram.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="rect">Optional rectangle for positioning.</param>
    /// <returns>The diagram item.</returns>
    IFlowDiagramItem AddNode(FlowNode node, Rectangle? rect = null);

    /// <summary>
    /// Removes a node from the diagram.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    void RemoveNode(FlowNode node);

    /// <summary>
    /// Adds a link between two connectors.
    /// </summary>
    /// <param name="fromNode">Source node name.</param>
    /// <param name="fromConnector">Source connector name.</param>
    /// <param name="toNode">Target node name.</param>
    /// <param name="toConnector">Target connector name.</param>
    void AddLink(string fromNode, string fromConnector, string toNode, string toConnector);

    /// <summary>
    /// Removes a link between two connectors.
    /// </summary>
    /// <param name="fromNode">Source node name.</param>
    /// <param name="fromConnector">Source connector name.</param>
    /// <param name="toNode">Target node name.</param>
    /// <param name="toConnector">Target connector name.</param>
    void RemoveLink(string fromNode, string fromConnector, string toNode, string toConnector);

    /// <summary>
    /// Removes all links from a specific connector.
    /// </summary>
    /// <param name="fromNode">Source node name.</param>
    /// <param name="fromConnector">Source connector name.</param>
    void RemoveLinksByNodeFrom(string fromNode, string fromConnector);

    /// <summary>
    /// Removes all links to a specific connector.
    /// </summary>
    /// <param name="toNode">Target node name.</param>
    /// <param name="toConnector">Target connector name.</param>
    void RemoveLinksByNodeTo(string toNode, string toConnector);

    /// <summary>
    /// Gets a node by name.
    /// </summary>
    /// <param name="nodeName">Name of the node.</param>
    /// <returns>The flow node, or null if not found.</returns>
    FlowNode GetNode(string nodeName);

    /// <summary>
    /// Gets a connector by node and connector name.
    /// </summary>
    /// <param name="nodeName">Node name.</param>
    /// <param name="connectorName">Connector name.</param>
    /// <returns>The flow node connector, or null if not found.</returns>
    FlowNodeConnector GetConnector(string nodeName, string connectorName);

    /// <summary>
    /// Gets all links from a specific connector.
    /// </summary>
    /// <param name="fromNode">Source node name.</param>
    /// <param name="fromConnector">Source connector name.</param>
    /// <returns>Links from the connector.</returns>
    IEnumerable<NodeLink> GetLinksByConnectorFrom(string fromNode, string fromConnector);

    /// <summary>
    /// Gets all links to a specific connector.
    /// </summary>
    /// <param name="toNode">Target node name.</param>
    /// <param name="toConnector">Target connector name.</param>
    /// <returns>Links to the connector.</returns>
    IEnumerable<NodeLink> GetLinksByConnectorTo(string toNode, string toConnector);

    /// <summary>
    /// Gets whether a connector is linked.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>True if linked.</returns>
    bool GetIsLinked(FlowNodeConnector connector);

    /// <summary>
    /// Gets the number of linked connectors.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>Number of linked connectors.</returns>
    int GetLinkedConnectorCount(FlowNodeConnector connector);

    /// <summary>
    /// Gets the first linked connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>The linked connector, or null.</returns>
    FlowNodeConnector GetLinkedConnector(FlowNodeConnector connector);

    /// <summary>
    /// Gets all linked connectors.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="sort">Whether to sort by spatial order.</param>
    /// <returns>Array of linked connectors.</returns>
    FlowNodeConnector[] GetLinkedConnectors(FlowNodeConnector connector, bool sort);

    /// <summary>
    /// Collects invalid connectors.
    /// </summary>
    /// <param name="report">Whether to report errors.</param>
    /// <returns>List of invalid links.</returns>
    List<NodeLink> CollectInvalidConnectors(bool report = true);


    /// <summary>
    /// Queues computation data update.
    /// </summary>
    void QueueComputeData();

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    void RefreshView();

    /// <summary>
    /// Marks the diagram as visited.
    /// </summary>
    void MarkVisit();

    /// <summary>
    /// Marks the diagram as dirty.
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// Notifies that a node has been updated.
    /// </summary>
    /// <param name="node">The updated node.</param>
    void NotifyNodeUpdated(FlowNode node);

    /// <summary>
    /// Notifies that a node has been renamed.
    /// </summary>
    /// <param name="node">The renamed node.</param>
    void NotifyNodeRenamed(FlowNode node);

    /// <summary>
    /// Notifies that a connector has been renamed.
    /// </summary>
    /// <param name="node">The node containing the connector.</param>
    /// <param name="oldName">Old connector name.</param>
    /// <param name="newName">New connector name.</param>
    void NotifyConnectorRenamed(FlowNode node, string oldName, string newName);
}

#endregion

#region IFlowDiagramItem

/// <summary>
/// Flow diagram item storage
/// </summary>
public interface IFlowDiagramItem : IHasId
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the diagram.
    /// </summary>
    IFlowDiagram Diagram { get; set; }

    /// <summary>
    /// Gets the flow node.
    /// </summary>
    FlowNode Node { get; }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    int X { get; }

    /// <summary>
    /// Gets the Y position.
    /// </summary>
    int Y { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// Sets position and updates view simultaneously
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void SetPosition(int x, int y, bool notify = true);

    /// <summary>
    /// Sets the size.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="notify">Whether to notify.</param>
    void SetSize(int width, int height, bool notify = true);

    /// <summary>
    /// Sets position and size, and updates view simultaneously
    /// </summary>
    /// <param name="bound">The bound rectangle.</param>
    /// <param name="notify">Whether to notify.</param>
    void SetBound(Rectangle bound, bool notify = true);

    /// <summary>
    /// Sets whether expanded and updates view simultaneously.
    /// Setting expanded will not proactively calculate the expanded size. If needed, call <see cref="UpdatePreferredSize(int, int)"/>.
    /// </summary>
    /// <param name="expanded">Whether expanded.</param>
    void SetExpanded(bool expanded);

    /// <summary>
    /// Sets preferred size, called back by view.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    void UpdatePreferredSize(int width, int height);

    /// <summary>
    /// Notifies that the node has been updated.
    /// </summary>
    void NotifyNodeUpdated();

    /// <summary>
    /// Notifies that the name has been updated.
    /// </summary>
    void NotifyNameUpdated();

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    void RefreshView();
}

#endregion

////////////////////////////////////////////

#region IFlowView

/// <summary>
/// Flow chart view
/// </summary>
public interface IFlowView
{
    /// <summary>
    /// Gets the diagram.
    /// </summary>
    IFlowDiagram Diagram { get; }

    /// <summary>
    /// Gets or sets the grid span.
    /// </summary>
    int GridSpan { get; set; }

    /// <summary>
    /// Gets the UI object.
    /// </summary>
    object UIObject { get; }

    /// <summary>
    /// Gets or sets the computation.
    /// </summary>
    IFlowComputation Computation { get; set; }

    /// <summary>
    /// Gets the last mouse position.
    /// </summary>
    Point LastMousePosition { get; }

    /// <summary>
    /// Sets the selection.
    /// </summary>
    /// <param name="node">The node to select.</param>
    void SetSelection(FlowNode node);

    /// <summary>
    /// Sets the selection.
    /// </summary>
    /// <param name="nodes">Nodes to select.</param>
    void SetSelection(IEnumerable<FlowNode> nodes);

    /// <summary>
    /// Inspects the selection.
    /// </summary>
    void InspectSelection();

    /// <summary>
    /// Queues computation data update.
    /// </summary>
    void QueueComputeData();

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    void RefreshView();

    /// <summary>
    /// Refreshes specific nodes.
    /// </summary>
    /// <param name="nodes">Nodes to refresh.</param>
    void RefreshNodes(IEnumerable<IFlowViewNode> nodes);

    /// <summary>
    /// Gets a view node by name.
    /// </summary>
    /// <param name="name">Node name.</param>
    /// <returns>The view node, or null.</returns>
    IFlowViewNode GetViewNode(string name);
}

#endregion

#region IFlowViewNode

public interface IFlowViewNode
{
    /// <summary>
    /// Gets the flow view.
    /// </summary>
    IFlowView FlowView { get; }

    /// <summary>
    /// Gets the expanded view.
    /// </summary>
    IDrawExpandedImGui ExpandedView { get; }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    int X { get; }

    /// <summary>
    /// Gets the Y position.
    /// </summary>
    int Y { get; }

    /// <summary>
    /// Gets whether expanded.
    /// </summary>
    bool IsExpanded { get; }

    /// <summary>
    /// Sets the expand state.
    /// </summary>
    /// <param name="expand">Whether to expand.</param>
    void SetExpand(bool expand);

    /// <summary>
    /// Gets the flow node.
    /// </summary>
    FlowNode Node { get; }

    /// <summary>
    /// Gets whether the node can be deleted.
    /// </summary>
    bool CanBeDeleted { get; }

    /// <summary>
    /// Gets or sets the node computation.
    /// </summary>
    IFlowComputation NodeComputation { get; set; }

    /// <summary>
    /// Updates the position.
    /// </summary>
    void UpdatePosition();

    /// <summary>
    /// Updates the bound.
    /// </summary>
    void UpdateBound();

    /// <summary>
    /// Updates the preview text.
    /// </summary>
    /// <param name="text">Preview text.</param>
    void UpdatePreviewText(string text);

    /// <summary>
    /// Rebuilds the node.
    /// </summary>
    /// <param name="removeLink">Optional action to remove links.</param>
    void RebuildNode(Action<NodeLink> removeLink = null);

    /// <summary>
    /// Queues a refresh.
    /// </summary>
    void QueueRefresh();
}

#endregion

////////////////////////////////////////////

#region IFlowDataStyle

/// <summary>
/// Flow diagram data style
/// </summary>
public interface IFlowDataStyle
{
    /// <summary>
    /// Data type name
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// Indicates whether this data type represents an array, which may affect visual style and connection rules.
    /// </summary>
    bool IsArray { get; }

    /// <summary>
    /// Indicates whether this data type represents a key, which may affect visual style and connection rules.
    /// </summary>
    bool IsKey { get; }

    /// <summary>
    /// Data display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Can accept multiple inputs
    /// </summary>
    bool MultipleFromConnection { get; }

    /// <summary>
    /// Can accept multiple outputs
    /// </summary>
    bool MultipleToConnection { get; }

    /// <summary>
    /// Gets the link pen.
    /// </summary>
    Pen LinkPen { get; }

    /// <summary>
    /// Gets the link arrow brush.
    /// </summary>
    SolidBrush LinkArrowBrush { get; }

    /// <summary>
    /// Gets the connector outline pen.
    /// </summary>
    Pen ConnectorOutlinePen { get; }

    /// <summary>
    /// Gets the connector fill brush.
    /// </summary>
    SolidBrush ConnectorFillBrush { get; }

    /// <summary>
    /// Occurs when style is updated.
    /// </summary>
    event EventHandler StyleUpdated;
}

#endregion

////////////////////////////////////////////

#region FlowComputationStates

/// <summary>
/// Flow computation states
/// </summary>
public enum FlowComputationStates
{
    /// <summary>
    /// None
    /// </summary>
    None,

    /// <summary>
    /// Running
    /// </summary>
    Running,

    /// <summary>
    /// Finished
    /// </summary>
    Finished,

    /// <summary>
    /// Error
    /// </summary>
    Error,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled,
}

#endregion

#region FlowRunningState

/// <summary>
/// Flow running state
/// </summary>
public class FlowRunningState
{
    /// <summary>
    /// Gets the node.
    /// </summary>
    public FlowNode Node { get; }

    /// <summary>
    /// Gets or sets the begin connector.
    /// </summary>
    public FlowNodeConnector Begin { get; set; }

    /// <summary>
    /// Gets the result.
    /// </summary>
    public object Result { get; private set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public FlowComputationStates State { get; set; }

    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// Gets or sets the elapsed time.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public object Tag { get; set; }


    /// <summary>
    /// Initializes a new instance of the FlowRunningState.
    /// </summary>
    /// <param name="node">The flow node.</param>
    public FlowRunningState(FlowNode node)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>
    /// Sets the result.
    /// </summary>
    /// <param name="result">The result.</param>
    public void SetResult(object result)
    {
        State = FlowComputationStates.Finished;

        Result = result;
        Exception = null;
    }

    /// <summary>
    /// Sets the exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    public void SetException(Exception exception)
    {
        State = FlowComputationStates.Error;

        Result = null;
        Exception = exception;
    }

    /// <summary>
    /// Sets the cancelled state.
    /// </summary>
    public void SetCancelled()
    {
        State = FlowComputationStates.Cancelled;

        Result = null;
        Exception = null;
    }
}

#endregion

#region IFlowComputation

/// <summary>
/// Flow chart numerical computation
/// </summary>
public interface IFlowComputation
{
    /// <summary>
    /// Gets the context.
    /// </summary>
    FunctionContext Context { get; }

    /// <summary>
    /// Gets the local context.
    /// </summary>
    FunctionContext LocalContext { get; }

    /// <summary>
    /// Gets the last node.
    /// </summary>
    FlowNode LastNode { get; }

    /// <summary>
    /// Gets the diagram context.
    /// </summary>
    /// <param name="diagram">The diagram.</param>
    /// <returns>The function context.</returns>
    FunctionContext GetDiagramContext(IFlowDiagram diagram);

    /// <summary>
    /// Gets the running state for a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The running state.</returns>
    FlowRunningState GetNodeRunningState(FlowNode node);

    // int GetConnectionCount(NodeConnector node);

    /// <summary>
    /// Gets the value from a connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <returns>The value.</returns>
    object GetValue(FlowNodeConnector connector);

    /// <summary>
    /// Gets values from a connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="sort">Whether to sort.</param>
    /// <returns>Values array.</returns>
    object[] GetValues(FlowNodeConnector connector, bool sort);

    /// <summary>
    /// Gets the result from a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="compute">Whether to compute if needed.</param>
    /// <returns>The result.</returns>
    object GetResult(FlowNode node, bool compute = false);

    /// <summary>
    /// Sets the value for a connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="value">The value.</param>
    void SetValue(FlowNodeConnector connector, object value);

    /// <summary>
    /// Sets the result for a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="value">The result.</param>
    void SetResult(FlowNode node, object value);

    /// <summary>
    /// Sets the exception for a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="error">The exception.</param>
    void SetException(FlowNode node, Exception error);

    /// <summary>
    /// Sets the exception for a connector.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="error">The exception.</param>
    void SetException(FlowNodeConnector connector, Exception error);

    /// <summary>
    /// Invalidates a node.
    /// </summary>
    /// <param name="node">The node.</param>
    void InvalidateNode(FlowNode node);

    /// <summary>
    /// Invalidates all input nodes, causing input nodes to need recalculation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="collector">Optional collector for nodes.</param>
    void InvalidateInputs(FlowNode node, Action<FlowNode> collector = null);

    /// <summary>
    /// Invalidates all output nodes, causing output nodes to need recalculation.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="collector">Optional collector for nodes.</param>
    void InvalidateOutputs(FlowNode node, Action<FlowNode> collector = null);

    /// <summary>
    /// Adds a log message.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="message">The message.</param>
    void AddLog(TextStatus status, string message);
}

#endregion

#region IFlowComputationAsync

public interface IFlowComputationAsync : IFlowComputation
{
    /// <summary>
    /// Runs the action node connected from this connector.
    /// </summary>
    /// <param name="outputConnector">Output connection connector.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Result object.</returns>
    Task<object> RunAction(FlowNodeConnector outputConnector, CancellationToken cancel);

    /// <summary>
    /// Computes the value of a data node.
    /// </summary>
    /// <param name="dataNode">Data node.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Result object.</returns>
    Task<object> ComputeData(FlowNode dataNode, CancellationToken cancel);


    /// <summary>
    /// Runs multiple action nodes connected from this connector.
    /// </summary>
    /// <param name="outputConnector">Output connection connector.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task RunActions(FlowNodeConnector outputConnector, CancellationToken cancel);

    /// <summary>
    /// Runs an action node.
    /// </summary>
    /// <param name="node">Node to run.</param>
    /// <param name="begin">Begin connector.</param>
    /// <param name="createContext">Whether to create context.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Result object.</returns>
    Task<object> RunActionNode(FlowNode node, FlowNodeConnector begin, bool createContext, CancellationToken cancel);

    /// <summary>
    /// Recomputes input nodes.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task RecomputeInputNodes(FlowNode node, CancellationToken cancel);

    /// <summary>
    /// Gets the running action node.
    /// </summary>
    FlowNode RunningActionNode { get; }

    /// <summary>
    /// Gets the calling stack.
    /// </summary>
    int CallingStack { get; }
}

#endregion

#region IFlowNodeComputeAsync

/// <summary>
/// Flow node async computation
/// </summary>
public interface IFlowNodeComputeAsync
{
    /// <summary>
    /// Computes asynchronously.
    /// </summary>
    /// <param name="compute">The computation.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <returns>Result object.</returns>
    Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel);
}

#endregion

#region ISObjectFlowNode

/// <summary>
/// SObject flow node
/// </summary>
public interface ISObjectFlowNode
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public SObject Data { get; set; }
}

#endregion

#region FlowComputationException

/// <summary>
/// Flow computation exception
/// </summary>
[Serializable]
public class FlowComputaionException : Exception
{
    /// <summary>
    /// Gets the flow node.
    /// </summary>
    public FlowNode FlowNode { get; }

    /// <summary>
    /// Initializes a new instance of the FlowComputaionException.
    /// </summary>
    /// <param name="flowNode">The flow node.</param>
    public FlowComputaionException(FlowNode flowNode)
    {
        FlowNode = flowNode;
    }

    /// <summary>
    /// Initializes a new instance of the FlowComputaionException.
    /// </summary>
    /// <param name="flowNode">The flow node.</param>
    /// <param name="message">The message.</param>
    public FlowComputaionException(FlowNode flowNode, string message) : base(message)
    {
        FlowNode = flowNode;
    }

    /// <summary>
    /// Initializes a new instance of the FlowComputaionException.
    /// </summary>
    /// <param name="flowNode">The flow node.</param>
    /// <param name="message">The message.</param>
    /// <param name="inner">Inner exception.</param>
    public FlowComputaionException(FlowNode flowNode, string message, Exception inner) : base(message, inner)
    {
        FlowNode = flowNode;
    }

    /// <summary>
    /// Serialization constructor.
    /// </summary>
    protected FlowComputaionException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

#endregion
