using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.Views.Im.Flows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Cached numerical computation for flowcharts
/// </summary>
public class FlowComputation : IFlowComputation
{
    private readonly Dictionary<FlowNodeConnector, object> _datas = [];
    private readonly Dictionary<FlowNode, FlowRunningState> _states = [];
    private FlowNode _lastNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowComputation"/> class.
    /// </summary>
    /// <param name="context">The function context to use for computation. If null, a new context is created.</param>
    public FlowComputation(FunctionContext context = null)
    {
        Context = context ?? new FunctionContext();
    }

    /// <summary>
    /// Clears all cached data and running states.
    /// </summary>
    public void Clear()
    {
        _datas.Clear();
        _states.Clear();
    }

    private void Compute(FlowNode node)
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

    private bool TryGetCachedValue(FlowNodeConnector connector, out object value)
    {
        // Get data from port
        if (_datas.TryGetValue(connector, out value))
        {
            return true;
        }

        // Get data from node's default output
        if (_states.TryGetValue(connector.ParentNode, out var state))
        {
            value = state.Result;
            return true;
        }

        value = null;

        return false;
    }

    #region IFlowComputation

    /// <summary>
    /// Gets the function context used by this computation.
    /// </summary>
    public FunctionContext Context { get; }

    // Basic node graph without action version does not support local Context
    /// <summary>
    /// Gets the local function context. Returns the same as <see cref="Context"/> in the basic version.
    /// </summary>
    public FunctionContext LocalContext => Context; 

    /// <summary>
    /// Gets the last node that was computed.
    /// </summary>
    public FlowNode LastNode => _lastNode;

    // Basic node graph without action version does not support diagram Context
    /// <summary>
    /// Gets the diagram context for the specified flow diagram. Returns the base <see cref="Context"/> in the basic version.
    /// </summary>
    /// <param name="diagram">The flow diagram to get the context for.</param>
    /// <returns>The function context associated with the diagram.</returns>
    public FunctionContext GetDiagramContext(IFlowDiagram diagram) => Context;

    /// <summary>
    /// Gets the running state of the specified node.
    /// </summary>
    /// <param name="node">The flow node to get the running state for.</param>
    /// <returns>The running state of the node, or null if not found.</returns>
    public FlowRunningState GetNodeRunningState(FlowNode node)
    {
        return _states.GetValueSafe(node);
    }

    /// <summary>
    /// Gets the computation result of the specified node.
    /// </summary>
    /// <param name="node">The flow node to get the result for.</param>
    /// <param name="compute">If true and the node hasn't been computed yet, triggers computation.</param>
    /// <returns>The result of the node computation, or null if not available.</returns>
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

        Compute(node);

        return _states.GetValueSafe(node)?.Result;
    }

    /// <summary>
    /// Gets the value from the specified connector, computing upstream nodes if necessary.
    /// </summary>
    /// <param name="connector">The connector to get the value from.</param>
    /// <returns>The value at the connector, or null if not available.</returns>
    public object GetValue(FlowNodeConnector connector)
    {
        if (connector is null)
        {
            return null;
        }

        if (connector.ConnectionType == FlowConnectorTypes.Action)
        {
            return null;
        }

        if (_datas.TryGetValue(connector, out object value))
        {
            OnConnectorValueResolved(connector);

            return value;
        }

        if (connector.Direction == FlowDirections.Output)
        {
            Compute(connector.ParentNode);
            OnConnectorValueResolved(connector);

            return _datas.GetValueSafe(connector);
        }
        else
        {
            if (connector.ParentNode?.Diagram is not { } diagram)
            {
                return null;
            }

            var outConnector = diagram.GetLinkedConnector(connector);
            if (outConnector is null)
            {
                return null;
            }

            if (!TryGetCachedValue(outConnector, out value))
            {
                Compute(outConnector.ParentNode);
            }

            if (!TryGetCachedValue(outConnector, out value))
            {
                return null;
            }

            OnConnectorValueResolved(connector);

            return ConvertValue(outConnector, connector, value);
        }
    }

    /// <summary>
    /// Gets all values connected to the specified connector, supporting multiple upstream connections.
    /// </summary>
    /// <param name="connector">The connector to get values from.</param>
    /// <param name="sort">Whether to sort the output connectors before collecting values.</param>
    /// <returns>An array of values from all connected outputs, or null if the connector is invalid.</returns>
    public object[] GetValues(FlowNodeConnector connector, bool sort)
    {
        if (connector is null)
        {
            return null;
        }

        if (connector.ConnectionType == FlowConnectorTypes.Action)
        {
            return null;
        }

        if (_datas.TryGetValue(connector, out object value))
        {
            OnConnectorValueResolved(connector);

            return [value];
        }

        if (connector.Direction == FlowDirections.Output)
        {
            Compute(connector.ParentNode);
            OnConnectorValueResolved(connector);

            return [_datas.GetValueSafe(connector)];
        }

        if (connector.ParentNode?.Diagram is not { } diagram)
        {
            return null;
        }

        FlowNodeConnector[] outputs = diagram.GetLinkedConnectors(connector, sort);

        var values = outputs.SelectMany<FlowNodeConnector, object>(output =>
        {
            if (!_datas.TryGetValue(output, out object value2))
            {
                Compute(output.ParentNode);
            }

            if (!_datas.TryGetValue(output, out value2))
            {
                return [];
            }

            OnConnectorValueResolved(connector);

            if (value2 is not string && value2 is IEnumerable ary)
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
                if (converted is not string && converted is IEnumerable e)
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
        }).ToArray();

        return values;
    }

    private object ConvertValue(FlowNodeConnector connFrom, FlowNodeConnector connTo, object value)
    {
        var state = EditorServices.TypeConvertService.TryConvert(connFrom, connTo, value, out var result);

        return result;
    }

    #endregion

    #region IFlowComputationEvents

    /// <summary>
    /// Gets whether the specified node has already been computed.
    /// </summary>
    /// <param name="node">The flow node to check.</param>
    /// <returns>True if the node has been computed; otherwise, false.</returns>
    public bool GetIsNodeComputed(FlowNode node)
    {
        return _states.ContainsKey(node);
    }

    /// <summary>
    /// Sets the computation result for the specified node.
    /// </summary>
    /// <param name="node">The flow node to set the result for.</param>
    /// <param name="value">The result value to set.</param>
    public void SetResult(FlowNode node, object value)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var state = _states.GetOrAdd(node, n => new FlowRunningState(n));
        state.SetResult(value);
    }

    /// <summary>
    /// Sets the value for the specified connector.
    /// </summary>
    /// <param name="connector">The connector to set the value for.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(FlowNodeConnector connector, object value)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        if (connector.ParentNode is null)
        {
            throw new ArgumentNullException(nameof(connector.ParentNode));
        }

        var state = _states.GetOrAdd(connector.ParentNode, n => new FlowRunningState(n));
        state.SetResult(value);

        _datas[connector] = value;

        OnConnectorValueResolved(connector);
    }

    /// <summary>
    /// Sets an exception on the specified node and propagates it to all output connectors.
    /// </summary>
    /// <param name="node">The flow node that encountered the exception.</param>
    /// <param name="error">The exception to set.</param>
    public void SetException(FlowNode node, Exception error)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var state = _states.GetOrAdd(node, n => new FlowRunningState(n));
        state.SetException(error);

        foreach (var connector in node.Connectors.Where(o => o.ConnectionType == FlowConnectorTypes.Data && o.Direction == FlowDirections.Output))
        {
            _datas[connector] = error;
        }
    }

    /// <summary>
    /// Sets an exception on the specified connector.
    /// </summary>
    /// <param name="connector">The connector to set the exception on.</param>
    /// <param name="error">The exception to set.</param>
    public void SetException(FlowNodeConnector connector, Exception error)
    {
        if (connector is null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        _datas[connector] = error;
    }

    /// <summary>
    /// Invalidates the cached state and data for the specified node.
    /// </summary>
    /// <param name="node">The flow node to invalidate.</param>
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
    }

    /// <summary>
    /// Invalidates all upstream input data nodes connected to the specified node.
    /// </summary>
    /// <param name="node">The flow node to start invalidation from.</param>
    /// <param name="collector">Optional callback invoked for each invalidated node.</param>
    public void InvalidateInputs(FlowNode node, Action<FlowNode> collector = null)
    {
        HashSet<FlowNode> nodes = null;

        // To clear cache, only need to clear nodes forward, no need to deep search all nodes
        CollectConnectedDataNodes(node, ref nodes, true, false);

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
        }
    }

    /// <summary>
    /// Invalidates all downstream output data nodes connected from the specified node.
    /// </summary>
    /// <param name="node">The flow node to start invalidation from.</param>
    /// <param name="collector">Optional callback invoked for each invalidated node.</param>
    public void InvalidateOutputs(FlowNode node, Action<FlowNode> collector = null)
    {
        HashSet<FlowNode> nodes = null;

        // To clear cache, only need to clear nodes forward, no need to deep search all nodes
        CollectConnectedDataNodes(node, ref nodes, false, true);

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
        }
    }

    /// <summary>
    /// Adds a log message with the specified status level.
    /// </summary>
    /// <param name="status">The status level of the log message.</param>
    /// <param name="message">The log message.</param>
    public void AddLog(TextStatus status, string message)
    {
        AddLogDefault(status, message);
    }

    #endregion




    /// <summary>
    /// Called when a connector's value has been resolved. Triggers a visual flash effect.
    /// </summary>
    /// <param name="connector">The connector whose value was resolved.</param>
    protected virtual void OnConnectorValueResolved(FlowNodeConnector connector)
    {
        connector.FlashingOnce();
    }


    /// <summary>
    /// Collects connected data nodes from a given node, traversing input and/or output connections.
    /// </summary>
    /// <param name="node">The starting flow node.</param>
    /// <param name="nodes">The collection to add discovered nodes to (initialized if null).</param>
    /// <param name="input">Whether to traverse input connectors upstream.</param>
    /// <param name="output">Whether to traverse output connectors downstream.</param>
    /// <param name="deepToAll">If true, recursively collects all connected nodes in both directions; otherwise, follows the input/output parameters.</param>
    public static void CollectConnectedDataNodes(FlowNode node, ref HashSet<FlowNode> nodes, bool input, bool output, bool deepToAll = false)
    {
        if (node is null)
        {
            return;
        }

        var diagram = node.DiagramItem.Diagram;

        if (output)
        {
            foreach (var c in node.Connectors.Where(o => o.ConnectionType == FlowConnectorTypes.Data && o.Direction == FlowDirections.Output))
            {
                foreach (var inputConn in diagram.GetLinkedConnectors(c, false))
                {
                    var inputNode = inputConn.ParentNode;
                    if (inputNode != null && inputNode.IsDataNode && (nodes ??= []).Add(inputNode))
                    {
                        if (deepToAll)
                        {
                            CollectConnectedDataNodes(inputNode, ref nodes, true, true, true);
                        }
                        else
                        {
                            CollectConnectedDataNodes(inputNode, ref nodes, input, output, false);
                        }
                    }
                }
            }
        }

        if (input)
        {
            foreach (var c in node.Connectors.Where(o => o.ConnectionType == FlowConnectorTypes.Data && o.Direction == FlowDirections.Input))
            {
                foreach (var outputConn in diagram.GetLinkedConnectors(c, false))
                {
                    var outputNode = outputConn.ParentNode;
                    if (outputNode != null && outputNode.IsDataNode && (nodes ??= []).Add(outputNode))
                    {
                        if (deepToAll)
                        {
                            CollectConnectedDataNodes(outputNode, ref nodes, true, true, true);
                        }
                        else
                        {
                            CollectConnectedDataNodes(outputNode, ref nodes, input, output, false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Logs a message using the default logging mechanism based on the specified status level.
    /// </summary>
    /// <param name="status">The status level that determines the log method.</param>
    /// <param name="message">The message to log.</param>
    public static void AddLogDefault(TextStatus status, string message)
    {
        switch (status)
        {
            case TextStatus.Info:
                Logs.LogInfo(message);
                break;

            case TextStatus.Warning:
                Logs.LogWarning(message);
                break;

            case TextStatus.Error:
                Logs.LogError(message);
                break;

            default:
                Logs.LogDebug(message);
                break;
        }
    }
}