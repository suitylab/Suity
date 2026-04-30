using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows;

/// <summary>
/// Asynchronous computation engine for flowcharts, supporting action nodes and data flow execution.
/// </summary>
public class FlowComputationAsync : IFlowComputationAsync, IDisposable
{
    /// <summary>
    /// Represents the context associated with a flow diagram during computation.
    /// </summary>
    class DiagramContext(IFlowDiagram diagram, FunctionContext parentContext = null)
    {
        /// <summary>
        /// Gets the flow diagram associated with this context.
        /// </summary>
        public IFlowDiagram Diagram { get; } = diagram;
        /// <summary>
        /// Gets the function context for this diagram, optionally inheriting from a parent context.
        /// </summary>
        public FunctionContext Context { get; } = parentContext != null ? new FunctionContext(parentContext) : new FunctionContext();
    }

    // Action nodes can run repeatedly.
    // After an action node runs, all data nodes connected to its outputs should run completely; input nodes that haven't run don't need to execute, just return empty.
    // When an action node executes, it won't continue to execute other action nodes in the task.
    // After all data nodes connected to the action node's outputs have executed, execute the output action of this action node.
    // Only one node can execute at a time

    // The document system needs to add a Hold mechanism so that documents being run won't be released when closed.

    private readonly Stack<FlowNode> _runningNodes = new();
    private FlowNode _lastNode;

    private readonly Dictionary<IFlowDiagram, DiagramContext> _diagrams = [];
    private readonly Stack<FunctionContext> _callingStack = new();

    private readonly Dictionary<FlowNode, FlowRunningState> _states = [];
    private readonly Dictionary<FlowNodeConnector, object> _datas = [];

    private readonly QueueOnceAction _updateViewAction;

    private DisposeCollector _collector;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowComputationAsync"/> class.
    /// </summary>
    /// <param name="context">The global function context. If null, a new context is created.</param>
    public FlowComputationAsync(FunctionContext context = null)
    {
        Context = context ?? new FunctionContext();

        // Build local Context by passing in global Context, lookup Argument can automatically search upward.
        // No need to create default layer here, because running one action creates it.
        // _callingStack.Push(new FunctionContext(Context));

        _updateViewAction = new QueueOnceAction(() =>
        {
            foreach (var diagram in _diagrams.Values)
            {
                diagram.Diagram.RefreshView();
            }
            
            EditorUtility.Inspector.UpdateInspector();
        });
    }

    #region IFlowComputation

    /// <summary>
    /// Gets the global function context used by this computation.
    /// </summary>
    public FunctionContext Context { get; }

    /// <summary>
    /// Gets the current local function context from the calling stack.
    /// </summary>
    public FunctionContext LocalContext => _callingStack.Peek();

    /// <summary>
    /// Gets the diagram-specific function context, or returns the global context if not found.
    /// </summary>
    /// <param name="diagram">The flow diagram to retrieve the context for.</param>
    /// <returns>The function context for the specified diagram.</returns>
    public FunctionContext GetDiagramContext(IFlowDiagram diagram) 
        => _diagrams.GetValueSafe(diagram)?.Context ?? Context;

    /// <inheritdoc/>
    public FlowRunningState GetNodeRunningState(FlowNode node)
    {
        return _states.GetValueSafe(node);
    }

    /// <inheritdoc/>
    public object GetValue(FlowNodeConnector connector)
    {
        if (connector is null)
        {
            return null;
        }

        //if (connector.ConnectionType == ConnectionTypes.Action)
        //{
        //    return null;
        //}

        if (connector.Direction == FlowDirections.Output)
        {
            return _datas.GetValueSafe(connector);
        }
        else if (_datas.TryGetValue(connector, out var combinedPush))
        {
            // Combined port actively pushes to input
            return combinedPush;
        }

        var diagram = connector.ParentNode.DiagramItem.Diagram;

        var output = diagram.GetLinkedConnector(connector);
        if (output != null)
        {
            var value = _datas.GetValueSafe(output);

            return ConvertValue(output, connector, value);
        }

        return _states.GetValueSafe(connector.ParentNode)?.Result;
    }

    /// <inheritdoc/>
    public object[] GetValues(FlowNodeConnector connector, bool sort)
    {
        if (connector is null)
        {
            return null;
        }

        //if (connector.ConnectionType == ConnectionTypes.Action)
        //{
        //    return null;
        //}

        if (connector.Direction == FlowDirections.Output)
        {
            if (_datas.TryGetValue(connector, out object value))
            {
                return [value];
            }
            else
            {
                return null;
            }
        }

        var diagram = connector.ParentNode.DiagramItem.Diagram;

        FlowNodeConnector[] outputs = diagram.GetLinkedConnectors(connector, sort);
        var values = outputs.SelectMany<FlowNodeConnector, object>(output =>
        {
            if (_datas.TryGetValue(output, out object value2))
            {
                if (value2 is IEnumerable ary)
                {
                    List<object> list = [];
                    foreach (var item in ary)
                    {
                        var obj = item;
                        if (obj is SItem sItem)
                        {
                            obj = SItem.ResolveValue(sItem);
                        }

                        list.Add(obj);
                    }

                    var converted = ConvertValue(output, connector, list);
                    if (converted is not string && converted is System.Collections.IEnumerable e)
                    {
                        return e.OfType<object>();
                    }
                    else
                    {
                        return [converted];
                    }
                }
                else
                {
                    var converted = ConvertValue(output, connector, value2);
                    return [converted];
                }
            }

            return [];
        }).ToArray();

        return values;
    }

    private object ConvertValue(FlowNodeConnector connFrom, FlowNodeConnector connTo, object value)
    {
        var state = EditorServices.TypeConvertService.TryConvert(connFrom, connTo, value, out var result);

        return result;
    }

    /// <inheritdoc/>
    public object GetResult(FlowNode node, bool compute = false)
    {
        if (node is null)
        {
            return null;
        }

        if (_states.TryGetValue(node, out var result))
        {
            return result.Result;
        }

        if (!compute)
        {
            return null;
        }

        ComputeSimple(node);

        return _states.GetValueSafe(node)?.Result;
    }

    /// <summary>
    /// Sets the value for the specified connector. For combined output connectors, also pushes the value to all connected inputs.
    /// </summary>
    /// <param name="connector">The connector to set the value for.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(FlowNodeConnector connector, object value)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        _datas[connector] = value;

        var diagram = connector.ParentNode.DiagramItem.Diagram;
        AddDiagram(diagram);

        // When acting as a combined port, actively push to all inputs to indicate the specified output value
        if (connector.Direction == FlowDirections.Output && connector.IsCombined)
        {
            foreach (var inputConn in diagram.GetLinkedConnectors(connector, false))
            {
                _datas[inputConn] = value;
            }
        }
    }

    /// <inheritdoc/>
    public void SetResult(FlowNode node, object value)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var state = _states.GetOrAdd(node, n => new FlowRunningState(n));
        state.SetResult(value);

        //foreach (var connector in node.Connectors.Where(o => o.ConnectionType == ConnectionTypes.Data && o.Direction == ConnectionDirections.Output))
        //{
        //    _datas[connector] = value;
        //}
    }

    /// <inheritdoc/>
    public void SetException(FlowNode node, Exception error)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        foreach (var connector in node.Connectors.Where(o => o.ConnectionType == FlowConnectorTypes.Data && o.Direction == FlowDirections.Output))
        {
            _datas[connector] = error; ;
        }
    }

    /// <inheritdoc/>
    public void SetException(FlowNodeConnector connector, Exception error)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        _datas[connector] = error;
    }

    /// <inheritdoc/>
    public void InvalidateNode(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        _states.Remove(node);
        foreach (var c in node.Connectors)
        {
            _datas.Remove(c);
        }

        _updateViewAction.DoQueuedAction();
    }

    /// <inheritdoc/>
    public void InvalidateInputs(FlowNode node, Action<FlowNode> collector = null)
    {
        HashSet<FlowNode> nodes = null;

        // To clear cache, only need to clear nodes forward, no need to deep search all nodes
        FlowComputation.CollectConnectedDataNodes(node, ref nodes, true, false);

        if (nodes != null)
        {
            foreach (var n in nodes)
            {
                _states.Remove(n);
                foreach (var c in n.Connectors)
                {
                    _datas.Remove(c);
                }

                collector?.Invoke(n);
            }

            _updateViewAction.DoQueuedAction();
        }
    }

    /// <inheritdoc/>
    public void InvalidateOutputs(FlowNode node, Action<FlowNode> collector = null)
    {
        HashSet<FlowNode> nodes = null;

        // To clear cache, only need to clear nodes forward, no need to deep search all nodes
        FlowComputation.CollectConnectedDataNodes(node, ref nodes, false, true);

        if (nodes != null)
        {
            foreach (var n in nodes)
            {
                _states.Remove(n);
                foreach (var c in n.Connectors)
                {
                    _datas.Remove(c);
                }

                collector?.Invoke(n);
            }

            _updateViewAction.DoQueuedAction();
        }
    }

    /// <inheritdoc/>
    public virtual void AddLog(TextStatus status, string message)
    {
        FlowComputation.AddLogDefault(status, message);
    }


    private void ComputeSimple(FlowNode node)
    {
        if (node is null)
        {
            return;
        }

        lock (_states)
        {
            if (_states.ContainsKey(node))
            {
                var e = new FlowComputaionException(node, "Cyclic computation detected.");

                e.LogWarning();
                SetException(node, e);

                return;
            }

            _states.Add(node, new FlowRunningState(node));

            try
            {
                _lastNode = node;
                node.Compute(this);
            }
            catch (Exception err)
            {
                // Logs.LogError(err);

                foreach (var connector in node.Connectors.Where(o => o.Direction == FlowDirections.Output))
                {
                    _datas[connector] = err;
                }
            }
        }
    }

    #endregion

    #region IFlowComputationAsync

    /// <summary>
    /// Runs an action starting from the specified output connector and returns the result.
    /// </summary>
    /// <param name="outputConnector">The output action connector to start from.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the result value.</returns>
    public Task<object> RunAction(FlowNodeConnector outputConnector, CancellationToken cancel)
    {
        if (outputConnector is null)
        {
            throw new ArgumentNullException(nameof(outputConnector));
        }

        if (outputConnector.ConnectionType != FlowConnectorTypes.Action)
        {
            throw new InvalidOperationException("Action connector is required.");
        }

        if (outputConnector.Direction != FlowDirections.Output)
        {
            throw new InvalidOperationException("Output connector is required.");
        }

        //// Whether or not connected to the next action node, all output data nodes will run first
        //var node = outputConnector.ParentNode;
        //if (node != null)
        //{
        //    // Before computing next action node, need to clear cache first
        //    InvalidateOutputs(node);

        //    // Before computing next action node, need to resolve all output data of this node first
        //    var nodes = new HashSet<FlowNode>();
        //    FlowComputation.CollectConnectedDataNodes(Document, node, nodes, false, true);

        //    await RunNodes(nodes, cancel);
        //}

        _updateViewAction.DoQueuedAction();
        if (cancel.IsCancellationRequested)
        {
            throw new TaskCanceledException();
        }

        var diagram = outputConnector.ParentNode.DiagramItem.Diagram;
        AddDiagram(diagram);

        var begin = diagram.GetLinkedConnector(outputConnector);
        var nextNode = begin?.ParentNode;
        if (nextNode is null)
        {
            return Task.FromResult<object>(null);
        }

        return RunActionNode(nextNode, begin, true, cancel);
    }

    /// <summary>
    /// Computes the data for the specified data node asynchronously.
    /// </summary>
    /// <param name="dataNode">The data node to compute.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the result value.</returns>
    public Task<object> ComputeData(FlowNode dataNode, CancellationToken cancel = default)
    {
        if (dataNode is null)
        {
            throw new ArgumentNullException(nameof(dataNode));
        }

        if (GetState(dataNode) is { } state)
        {
            return Task.FromResult(state.Result);
        }

        state = GetOrCreateState(dataNode);
        state.State = FlowComputationStates.Running;
        UpdateViewQueued();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            return RunActionNode(dataNode, null, true, cancel);
        }
        finally
        {
            stopwatch.Stop();
            state.ElapsedTime = stopwatch.Elapsed;
        }
    }

    /// <summary>
    /// Runs all action nodes connected to the specified output connector sequentially.
    /// </summary>
    /// <param name="outputConnector">The output action connector to start from.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunActions(FlowNodeConnector outputConnector, CancellationToken cancel)
    {
        if (outputConnector is null)
        {
            throw new ArgumentNullException(nameof(outputConnector));
        }

        if (outputConnector.ConnectionType != FlowConnectorTypes.Action)
        {
            throw new InvalidOperationException("Action connector is required.");
        }

        if (outputConnector.Direction != FlowDirections.Output)
        {
            throw new InvalidOperationException("Output connector is required.");
        }

        //// Whether or not connected to the next action node, all output data nodes will run first
        //var node = outputConnector.ParentNode;
        //if (node != null)
        //{
        //    // Before computing next action node, need to resolve all output data of this node first
        //    var nodes = new HashSet<FlowNode>();
        //    FlowComputation.CollectConnectedDataNodes(Document, node, nodes, false, true);
        //    await RunNodes(nodes, cancel);
        //}

        _updateViewAction.DoQueuedAction();
        if (cancel.IsCancellationRequested)
        {
            throw new TaskCanceledException();
        }

        var diagram = outputConnector.ParentNode.DiagramItem.Diagram;
        AddDiagram(diagram);

        // Sort by top-to-bottom order
        var begins = diagram.GetLinkedConnectors(outputConnector, true);

        foreach (var begin in begins)
        {
            var nextNode = begin.ParentNode;

            await RunActionNode(nextNode, begin, true, cancel);

            if (cancel.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
    }

    /// <summary>
    /// Runs a single action node asynchronously, following the execution chain.
    /// </summary>
    /// <param name="node">The flow node to execute.</param>
    /// <param name="begin">The connector where execution begins (can be null).</param>
    /// <param name="createContext">Whether to create a new local function context for this execution.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the result value.</returns>
    public async Task<object> RunActionNode(FlowNode node, FlowNodeConnector begin, bool createContext, CancellationToken cancel)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (begin != null && begin.ParentNode != node)
        {
            throw new ArgumentException("Connector is not belong to node.");
        }

        // Get diagram
        var diagram = node.Diagram
            ?? throw new NullReferenceException("Node's Diagram is null: " + node.GetType().Name);

        var diagramContext = AddDiagram(diagram);

        var runNode = node;
        FlowNodeConnector runConnector = begin;

        // Compute all input data before computing an action node
        var dataNodes = new HashSet<FlowNode>();
        object result = null;

        if (createContext)
        {
            // Build local Context by passing diagram Context, lookup Argument can automatically search upward in sequence.
            // Need to add another diagram Context here to build three-level relationship.
            _callingStack.Push(new FunctionContext(diagramContext.Context));
        }

        _runningNodes.Push(node);
        _lastNode = node;

        Stopwatch stopwatch = new();

        try
        {
            while (runNode != null)
            {
                result = null;

                await ComputeInputDataNodes(runNode, cancel, dataNodes);

                // Get state
                var state = _states.GetOrAdd(runNode, n => new FlowRunningState(n));
                state.Begin = runConnector;
                state.State = FlowComputationStates.Running;
                _updateViewAction.DoQueuedAction();
                // Check cancellation
                if (cancel.IsCancellationRequested)
                {
                    state.SetCancelled();
                    _updateViewAction.DoQueuedAction();

                    result = null;
                    break;
                }

                stopwatch.Reset();
                stopwatch.Start();

                try
                {
                    _runningNodes.Push(runNode);
                    _lastNode = runNode;

                    // Run node
                    if (runNode is IFlowNodeComputeAsync nodeAsync)
                    {
                        result = await nodeAsync.ComputeAsync(this, cancel);
                    }
                    else
                    {
                        runNode.Compute(this);
                        result = state.Result;
                    }

                    // Check cancellation and break
                    if (cancel.IsCancellationRequested)
                    {
                        state.SetCancelled();
                        break;
                    }

                    // Set result and update view
                    state.SetResult(result);
                    _updateViewAction.DoQueuedAction();

                    // Check sequence
                    if (result is FlowNodeConnector conn)
                    {
                        // Final result does not retain connection port
                        result = null;

                        if (conn.Direction == FlowDirections.Output &&
                            conn.ConnectionType == FlowConnectorTypes.Action &&
                            // conn.DataTypeName == FlowNode.ActionDataType &&
                            (conn.AllowMultipleConnection is null || conn.AllowMultipleConnection == false))
                        {
                            runConnector = diagram.GetLinkedConnector(conn);
                            runNode = runConnector?.ParentNode;
                        }
                    }
                    else
                    {
                        runNode = null;
                        // Do not clear result here, result can be returned.
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    state.SetCancelled();
                    _updateViewAction.DoQueuedAction();

                    throw;
                }
                catch (Exception err)
                {
                    state.SetException(err);
                    _updateViewAction.DoQueuedAction();

                    throw;
                }
                finally
                {
                    _runningNodes.Pop();

                    stopwatch.Stop();
                    state.ElapsedTime = stopwatch.Elapsed;
                }
            }

            if (cancel.IsCancellationRequested)
            {
                _updateViewAction.DoQueuedAction();

                return null;
            }

            // Remove NodeConnector that does not meet requirements
            if (result is FlowNodeConnector)
            {
                result = null;
            }
        }
        finally
        {
            if (createContext)
            {
                _callingStack.Pop();
            }

            _runningNodes.Pop();
        }

        return result;
    }

    

    /// <summary>
    /// Recomputes all input data nodes connected to the specified node.
    /// </summary>
    /// <param name="node">The flow node whose inputs should be recomputed.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RecomputeInputNodes(FlowNode node, CancellationToken cancel)
    {
        return ComputeInputDataNodes(node, cancel);
    }

    /// <summary>
    /// Gets the currently running action node, or null if none is running.
    /// </summary>
    public FlowNode RunningActionNode => _runningNodes.Count > 0 ? _runningNodes.Peek() : null;

    /// <summary>
    /// Gets the last node that was computed.
    /// </summary>
    public FlowNode LastNode => _lastNode;

    /// <summary>
    /// Gets the current depth of the calling stack.
    /// </summary>
    public int CallingStack => _callingStack.Count;

    /// <summary>
    /// Computes all input data nodes for the given run node.
    /// </summary>
    /// <param name="runNode">The node whose inputs need to be computed.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <param name="reusableCache">Reusable hashset for collecting nodes (cleared internally).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ComputeInputDataNodes(FlowNode runNode, CancellationToken cancel, HashSet<FlowNode> reusableCache = null)
    {
        reusableCache ??= [];

        // Cleanup
        reusableCache.Clear();

        // Compute input nodes
        FlowComputation.CollectConnectedDataNodes(runNode, ref reusableCache, true, false);

        // Remove all cached data since some data nodes read variables, and these variables may change, causing subsequent nodes to update.
        foreach (var n in reusableCache)
        {
            _states.Remove(n);
            foreach (var c in n.Connectors)
            {
                _datas.Remove(c);
            }
        }

        try
        {
            await RunNodes(reusableCache, cancel);
        }
        catch (Exception)
        {
            _updateViewAction.DoQueuedAction();

            throw;
        }

        // Cleanup
        reusableCache.Clear();
    }

    #endregion

    #region Data & State

    /// <summary>
    /// Sets the cached data value for the specified connector.
    /// </summary>
    /// <param name="connector">The connector to set data for.</param>
    /// <param name="data">The data value to set.</param>
    protected void SetData(FlowNodeConnector connector, object data)
    {
        _datas[connector] = data;
    }

    /// <summary>
    /// Gets the cached data value for the specified connector.
    /// </summary>
    /// <param name="connector">The connector to get data from.</param>
    /// <returns>The cached data value, or null if not found.</returns>
    protected object GetData(FlowNodeConnector connector)
    {
        return _datas.GetValueSafe(connector);
    }

    /// <summary>
    /// Gets the running state for the specified node.
    /// </summary>
    /// <param name="node">The flow node to get the state for.</param>
    /// <returns>The running state, or null if not found.</returns>
    protected FlowRunningState GetState(FlowNode node)
    {
        return _states.GetValueSafe(node);
    }

    /// <summary>
    /// Gets the running state for the specified node, creating one if it doesn't exist.
    /// </summary>
    /// <param name="node">The flow node to get or create the state for.</param>
    /// <returns>The running state for the node.</returns>
    protected FlowRunningState GetOrCreateState(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return _states.GetOrAdd(node, n => new FlowRunningState(n));
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        var diagrams = _diagrams.Values.ToArray();

        // View operations should not be performed in Dispose
        //QueuedAction.Do(() => 
        //{
        //    foreach (var diagram in diagrams)
        //    {
        //        diagram.Diagram.RefreshView();
        //    }

        //    EditorUtility.Inspector.UpdateInspector();
        //});

        _diagrams.Clear();
        _states.Clear();
        _datas.Clear();
        _collector?.Dispose();
    }

    #endregion

    /// <summary>
    /// Called when a new diagram is added to the computation engine.
    /// </summary>
    /// <param name="diagram">The diagram that was added.</param>
    protected virtual void OnDiagramAdded(IFlowDiagram diagram)
    {
        var doc = diagram.GetFlowDocument();
        if (doc?.Entry is { } entry)
        {
            var token = new DocumentUsageToken(nameof(FlowComputationAsync));
            _collector += token;

            entry.MarkUsage(token);
        }
    }

    /// <summary>
    /// Queues a view update action to be executed once per frame.
    /// </summary>
    protected void UpdateViewQueued()
    {
        _updateViewAction.DoQueuedAction();
    }

    /// <summary>
    /// Determines whether the specified data node is ready to be computed.
    /// A data node is computable when all its input data nodes have already been computed.
    /// </summary>
    /// <param name="node">The flow node to check.</param>
    /// <returns>True if the node can be computed; otherwise, false.</returns>
    protected bool CanComputeDataNode(FlowNode node)
    {
        if (node is null)
        {
            return false;
        }

        if (!node.IsDataNode)
        {
            return false;
        }

        var diagram = node.DiagramItem.Diagram;

        foreach (var c in node.Connectors.Where(o => o.Direction == FlowDirections.Input))
        {
            foreach (var c2 in diagram.GetLinkedConnectors(c, false))
            {
                var output = c2?.ParentNode;
                // Has connected output node && output node is data node && node not computed
                if (output != null && output.IsDataNode && !_states.ContainsKey(output))
                {
                    // Input node not yet computed.
                    return false;
                }
            }
        }

        // All input nodes have been computed.
        return true;
    }

    /// <summary>
    /// Runs all computable data nodes from the given set in dependency order.
    /// </summary>
    /// <param name="nodes">The set of nodes to run (modified during execution).</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task RunNodes(HashSet<FlowNode> nodes, CancellationToken cancel)
    {
        while (nodes.Count > 0)
        {
            var dataNode = FindComputableDataNode(nodes);
            if (dataNode is null)
            {
                break;
            }

            try
            {
                var item = await RunOneDataNode(dataNode, cancel);
                if (item is null)
                {
                    // lambda node
                    nodes.Remove(dataNode);
                    continue;
                }

                if (item.Exception != null)
                {
                    break;
                }
            }
            catch (Exception)
            {
                _updateViewAction.DoQueuedAction();

                //QueuedAction.Do(() => Conversation.AddErrorMessage(err.Message));
                //break;

                throw;
            }

            nodes.Remove(dataNode);

            if (cancel.IsCancellationRequested)
            {
                _updateViewAction.DoQueuedAction();
                return;
            }
        }

        if (nodes.Count > 0)
        {
            AddLog(TextStatus.Error, $"There are still {nodes.Count} data nodes not computed.");
        }
    }

    /// <summary>
    /// Runs a single data node and returns its running state.
    /// </summary>
    /// <param name="dataNode">The data node to execute.</param>
    /// <param name="cancel">Cancellation token to abort the operation.</param>
    /// <returns>The running state after execution.</returns>
    protected async Task<FlowRunningState> RunOneDataNode(FlowNode dataNode, CancellationToken cancel)
    {
        if (dataNode is null)
        {
            throw new ArgumentNullException(nameof(dataNode));
        }

        if (!dataNode.IsDataNode)
        {
            throw new InvalidOperationException("Node has not a data node.");
        }

        var state = _states.GetValueSafe(dataNode);

        //if (_runningDataNode != null)
        //{
        //    if (_runningDataNode == state)
        //    {
        //        return state;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("Other data node is running.");
        //    }
        //}

        if (state != null)
        {
            return state;
        }

        state = new FlowRunningState(dataNode)
        {
            State = FlowComputationStates.Running
        };

        //_runningDataNode = state;
        _updateViewAction.DoQueuedAction();
        _states[dataNode] = state;

        _lastNode = dataNode;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        try
        {
            if (dataNode is IFlowNodeComputeAsync aigcNode)
            {
                var result = await aigcNode.ComputeAsync(this, cancel);

                if (cancel.IsCancellationRequested)
                {
                    state.SetCancelled();
                }
                else
                {
                    state.SetResult(result);
                }
            }
            else
            {
                dataNode.Compute(this);
                state.SetResult(null);
            }

            //_runningDataNode = null;
            _updateViewAction.DoQueuedAction();

            return state;
        }
        catch (TaskCanceledException)
        {
            state.SetCancelled();
            _updateViewAction.DoQueuedAction();

            throw;
        }
        catch (Exception err)
        {
            //QueuedAction.Do(() => Conversation.AddErrorMessage(err.Message));

            state.SetException(err);

            //_runningDataNode = null;
            _updateViewAction.DoQueuedAction();

            throw;
        }
        finally
        {
            stopwatch.Stop();
            state.ElapsedTime = stopwatch.Elapsed;
        }
    }

    /// <summary>
    /// Finds the first computable data node from the given set.
    /// </summary>
    /// <param name="nodes">The set of candidate nodes.</param>
    /// <returns>The first node that is ready to compute, or null if none.</returns>
    protected FlowNode FindComputableDataNode(HashSet<FlowNode> nodes)
    {
        return nodes.FirstOrDefault(CanComputeDataNode);
    }


    private DiagramContext AddDiagram(IFlowDiagram diagram)
    {
        if (diagram is null)
        {
            throw new ArgumentNullException(nameof(diagram));
        }

        // Auto mark visit
        diagram.MarkVisit();

        if (_diagrams.TryGetValue(diagram, out var current))
        {
            return current;
        }

        int maxDiagramCount = ServiceInternals.License.MaxDiagramCount;
        int diagramCount = _diagrams.Count + 1;
        if (maxDiagramCount >= 0 && diagramCount > ServiceInternals.License.MaxDiagramCount)
        {
            string msg = $"Cannot add diagram {diagram.Name}, maximum diagram count ({diagramCount}/{maxDiagramCount}) exceeded.";
            Logs.LogError(msg);
            throw new InvalidOperationException(msg);
        }

        int maxNodeCount = ServiceInternals.License.MaxNodeCount;
        int nodeCount = diagram.NodeCount + _diagrams.Keys.Sum(o => o.NodeCount);
        if (maxNodeCount >= 0 && nodeCount >= ServiceInternals.License.MaxNodeCount)
        {
            string msg = $"Cannot add diagram {diagram.Name}, maximum node count ({nodeCount}/{maxNodeCount}) exceeded.";
            Logs.LogError(msg);
            throw new InvalidOperationException(msg);
        }

        var context = _diagrams.GetOrAdd(diagram, d => new DiagramContext(d, Context), out bool added);
        if (added)
        {
            OnDiagramAdded(diagram);
        }

        return context;
    }
}