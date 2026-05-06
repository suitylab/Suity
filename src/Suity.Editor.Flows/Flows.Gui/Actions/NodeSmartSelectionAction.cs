using static Suity.Helpers.GlobalLocalizer;
using Suity.Views.NodeGraph;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that manages smart selection changes in the flow view.
/// </summary>
internal class NodeSmartSelectionAction : UndoRedoAction
{
    /// <summary>
    /// The flow view this action operates on.
    /// </summary>
    readonly IFlowView _view;
    /// <summary>
    /// The graph control used for selection operations.
    /// </summary>
    readonly GraphControl _panel;

    /// <summary>
    /// The node selection state before the action.
    /// </summary>
    readonly GraphNode[] _nodeSelectionBefore;
    /// <summary>
    /// The node selection state after the action.
    /// </summary>
    GraphNode[] _nodeSelectionAfter;

    /// <summary>
    /// The link selection state before the action.
    /// </summary>
    readonly GraphLink[] _linkSelectionBefore;
    /// <summary>
    /// The link selection state after the action.
    /// </summary>
    GraphLink[] _linkSelectionAfter;

    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> capturing the current selection.
    /// </summary>
    /// <param name="view">The flow view.</param>
    public NodeSmartSelectionAction(IFlowView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _nodeSelectionBefore = [.. _panel.Diagram.SelectedNodes];
        _linkSelectionBefore = [.. _panel.Diagram.SelectedLinks];
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> with a known previous selection.
    /// </summary>
    /// <param name="view">The flow view.</param>
    /// <param name="nodeSelectionBefore">The node selection state before the change.</param>
    /// <param name="linkSelectionBefore">The link selection state before the change.</param>
    public NodeSmartSelectionAction(IFlowView view, IEnumerable<GraphNode> nodeSelectionBefore, IEnumerable<GraphLink>? linkSelectionBefore = null)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _nodeSelectionBefore = [.. nodeSelectionBefore];
        _linkSelectionBefore = linkSelectionBefore != null ? [.. linkSelectionBefore] : [];
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> with known before and after selections.
    /// </summary>
    /// <param name="view">The flow view.</param>
    /// <param name="nodeSelectionBefore">The node selection state before the change.</param>
    /// <param name="nodeSelectionAfter">The node selection state after the change.</param>
    /// <param name="linkSelectionBefore">The link selection state before the change.</param>
    /// <param name="linkSelectionAfter">The link selection state after the change.</param>
    public NodeSmartSelectionAction(IFlowView view, IEnumerable<GraphNode> nodeSelectionBefore, IEnumerable<GraphNode> nodeSelectionAfter, IEnumerable<GraphLink>? linkSelectionBefore = null, IEnumerable<GraphLink>? linkSelectionAfter = null)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _nodeSelectionBefore = [.. nodeSelectionBefore];
        _nodeSelectionAfter = [.. nodeSelectionAfter];
        _linkSelectionBefore = linkSelectionBefore != null ? [.. linkSelectionBefore] : [];
        _linkSelectionAfter = linkSelectionAfter != null ? [.. linkSelectionAfter] : [];
    }

    /// <summary>
    /// Gets the action name displayed in the undo/redo UI.
    /// </summary>
    public override string Name => L("Select");

    /// <summary>
    /// Gets a value indicating whether this action modifies document state. Selection changes do not.
    /// </summary>
    public override bool Modifying => false;

    /// <inheritdoc/>
    public override void Do()
    {
        if (_nodeSelectionAfter != null)
        {
            _panel?.SetNodeSelection(_nodeSelectionAfter);
        }

        if (_linkSelectionAfter != null)
        {
            _panel?.SetLinkSelection(_linkSelectionAfter);
        }

        _view.InspectSelection();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _nodeSelectionAfter ??= [.. _panel.Diagram.SelectedNodes];
        _linkSelectionAfter ??= [.. _panel.Diagram.SelectedLinks];

        _panel?.SetNodeSelection(_nodeSelectionBefore);
        _panel?.SetLinkSelection(_linkSelectionBefore);
        _view.InspectSelection();
    }
}
