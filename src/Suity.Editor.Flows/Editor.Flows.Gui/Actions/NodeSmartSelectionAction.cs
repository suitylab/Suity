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
    /// The selection state before the action.
    /// </summary>
    readonly GraphNode[] _selectionBefore;
    /// <summary>
    /// The selection state after the action.
    /// </summary>
    GraphNode[] _selectionAfter;

    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> capturing the current selection.
    /// </summary>
    /// <param name="view">The flow view.</param>
    public NodeSmartSelectionAction(IFlowView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _selectionBefore = [.. _panel.Diagram.SelectedItems];
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> with a known previous selection.
    /// </summary>
    /// <param name="view">The flow view.</param>
    /// <param name="selectionBefore">The selection state before the change.</param>
    public NodeSmartSelectionAction(IFlowView view, IEnumerable<GraphNode> selectionBefore)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _selectionBefore = [.. selectionBefore];
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeSmartSelectionAction"/> with known before and after selections.
    /// </summary>
    /// <param name="view">The flow view.</param>
    /// <param name="selectionBefore">The selection state before the change.</param>
    /// <param name="selectionAfter">The selection state after the change.</param>
    public NodeSmartSelectionAction(IFlowView view, IEnumerable<GraphNode> selectionBefore, IEnumerable<GraphNode> selectionAfter)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _panel = _view.UIObject as GraphControl;
        _selectionBefore = [.. selectionBefore];
        _selectionAfter = [.. selectionAfter];
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
        if (_selectionAfter != null)
        {
            _panel?.SetSelection(_selectionAfter);
        }

        _view.InspectSelection();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _selectionAfter ??= [.. _panel.Diagram.SelectedItems];

        _panel?.SetSelection(_selectionBefore);
        _view.InspectSelection();
    }
}
