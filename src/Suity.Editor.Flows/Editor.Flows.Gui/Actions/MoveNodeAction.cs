using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows.Gui.Actions;

/// <summary>
/// Undo/redo action that moves one or more flow diagram items to new positions.
/// </summary>
internal class MoveNodeAction : UndoRedoAction
{
    /// <inheritdoc/>
    public override string Name => L("Move Node");

    /// <summary>
    /// The movement offset (used for reference; final positions account for snapping).
    /// </summary>
    private readonly Point _offset;

    /// <summary>
    /// The diagram items to move.
    /// </summary>
    private readonly IFlowDiagramItem[] _nodes;
    /// <summary>
    /// The positions before moving.
    /// </summary>
    private readonly Point[] _oldPositions;
    /// <summary>
    /// The positions after moving.
    /// </summary>
    private readonly Point[] _newPositions;

    /// <summary>
    /// Flag indicating whether an undo has been performed.
    /// </summary>
    private bool _flag;

    /// <summary>
    /// Initializes a new instance of <see cref="MoveNodeAction"/>.
    /// </summary>
    /// <param name="view">The flow view used to resolve new positions.</param>
    /// <param name="items">The diagram items to move.</param>
    /// <param name="offset">The movement offset (used for reference; final positions account for snapping).</param>
    public MoveNodeAction(IFlowView view, IEnumerable<IFlowDiagramItem> items, Point offset)
    {
        // Cannot use offset to calculate final value here, need to consider snapping factor
        _offset = offset;

        _nodes = [.. items];

        // IFlowDiagramItem stores the old value
        _oldPositions = items
            .Select(o => new Point(o.X, o.Y))
            .ToArray();

        // IFlowViewNode already has the new value
        _newPositions = items
            .Select(o => o.Node.GetViewNode(view))
            .Select(o => new Point(o.X, o.Y))
            .ToArray();
    }

    /// <inheritdoc/>
    public override void Do()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            _nodes[i].SetPosition(_newPositions[i].X, _newPositions[i].Y);
        }

        //if (_flag)
        //{
        //_panel.RefreshView();
        //}

        _nodes.FirstOrDefault()?.Diagram?.RefreshView();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _flag = true;

        for (int i = 0; i < _nodes.Length; i++)
        {
            _nodes[i].SetPosition(_oldPositions[i].X, _oldPositions[i].Y);
            _nodes[i].Diagram.RefreshView();
        }

        //_panel.RefreshView();

        _nodes.FirstOrDefault()?.Diagram?.RefreshView();
    }
}