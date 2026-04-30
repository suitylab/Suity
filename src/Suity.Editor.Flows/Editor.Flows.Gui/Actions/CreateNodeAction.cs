using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Suity.Collections;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that creates one or more flow nodes in the diagram.
/// </summary>
internal class CreateNodeAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Create Node");

    /// <summary>
    /// The flow view that will contain the nodes.
    /// </summary>
    private readonly IFlowView _view;
    /// <summary>
    /// The nodes to create.
    /// </summary>
    private readonly FlowNode[] _nodes;
    /// <summary>
    /// Optional bounding rectangles for each node.
    /// </summary>
    private readonly Rectangle?[] _rects;

    /// <summary>
    /// The selection action to apply after creating nodes.
    /// </summary>
    private readonly NodeSmartSelectionAction _selectionAction;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateNodeAction"/> for a single node.
    /// </summary>
    /// <param name="view">The flow view that will contain the node.</param>
    /// <param name="node">The node to create.</param>
    /// <param name="rect">Optional bounding rectangle for the node.</param>
    public CreateNodeAction(IFlowView view, FlowNode node, Rectangle? rect = null)
    {
        _view = view;
        _nodes = [node];
        _rects = [rect];

        _selectionAction = new NodeSmartSelectionAction(_view);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateNodeAction"/> for multiple nodes.
    /// </summary>
    /// <param name="view">The flow view that will contain the nodes.</param>
    /// <param name="nodes">The nodes to create.</param>
    public CreateNodeAction(IFlowView view, IEnumerable<FlowNode> nodes)
    {
        _view = view;
        _nodes = nodes.ToArray();
        _selectionAction = new NodeSmartSelectionAction(_view);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_view.Diagram is { } diagram)
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                var node = _nodes[i];
                var rect = _rects.GetArrayItemSafe(i);

                diagram.AddNode(node, rect);
            }

            _selectionAction.Do();
            _view.SetSelection(_nodes);
            diagram.RefreshView();
            diagram.QueueComputeData();
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_view.Diagram is { } diagram)
        {
            _selectionAction.Undo();

            foreach (var node in _nodes)
            {
                diagram.RemoveNode(node);
            }

            diagram.RefreshView();
            diagram.QueueComputeData();
        }
    }
}