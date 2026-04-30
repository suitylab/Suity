using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that deletes one or more flow nodes from the diagram.
/// </summary>
class DeleteNodeAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Delete Node");

    /// <summary>
    /// The flow view that contains the nodes.
    /// </summary>
    readonly IFlowView _view;
    /// <summary>
    /// The nodes to delete.
    /// </summary>
    readonly FlowNode[] _nodes;

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteNodeAction"/>.
    /// </summary>
    /// <param name="view">The flow view that contains the nodes.</param>
    /// <param name="nodes">The nodes to delete.</param>
    public DeleteNodeAction(IFlowView view, IEnumerable<FlowNode> nodes)
    {
        _view = view;
        _nodes = [.. nodes];
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_view.Diagram is { } diagram)
        {
            foreach (var node in _nodes)
            {
                diagram.RemoveNode(node);
            }

            _view.SetSelection([]);
            diagram.RefreshView();
            diagram.QueueComputeData();
        }
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_view.Diagram is { } diagram)
        {
            foreach (var node in _nodes)
            {
                diagram.AddNode(node);
            }

            _view.SetSelection(_nodes);
            diagram.RefreshView();
            diagram.QueueComputeData();
        }
    }
}
