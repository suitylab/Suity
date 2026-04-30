using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.VirtualTree;
using Suity.UndoRedos;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents an undo/redo action for single tree view selection changes.
/// </summary>
public class SingleTreeSelectionAction : UndoRedoAction
{
    private readonly ImGuiVirtualTreeView _view;
    private readonly VirtualPath[] _selectionBefore;
    private readonly VirtualPath[] _selectionAfter;
    private bool _firstDo = false;
    private readonly string? _name;

    /// <summary>
    /// Gets the selection state before the action.
    /// </summary>
    public VirtualPath[] SelectionBefore => _selectionBefore;

    /// <summary>
    /// Gets the selection state after the action.
    /// </summary>
    public VirtualPath[] SelectionAfter => _selectionAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleTreeSelectionAction"/> class with the current view selection.
    /// </summary>
    /// <param name="view">The tree view associated with this selection action.</param>
    public SingleTreeSelectionAction(ImGuiVirtualTreeView view)
    {
        _view = view;
        _selectionBefore = _view.LastSelection;
        _selectionAfter = _view.MakeSelection();
        _firstDo = true;
        _name = GetSelectionPathTips(_selectionAfter);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleTreeSelectionAction"/> class with specified after selection.
    /// </summary>
    /// <param name="view">The tree view associated with this selection action.</param>
    /// <param name="after">The selection state after the action.</param>
    public SingleTreeSelectionAction(ImGuiVirtualTreeView view, VirtualPath[] after)
    {
        _view = view;
        _selectionBefore = _view.LastSelection;
        _selectionAfter = after;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleTreeSelectionAction"/> class with specified before and after selections.
    /// </summary>
    /// <param name="view">The tree view associated with this selection action.</param>
    /// <param name="before">The selection state before the action.</param>
    /// <param name="after">The selection state after the action.</param>
    public SingleTreeSelectionAction(ImGuiVirtualTreeView view, VirtualPath[] before, VirtualPath[] after)
    {
        _view = view;
        _selectionBefore = before;
        _selectionAfter = after;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this action can be considered void when selections are equal.
    /// </summary>
    public bool CanBeVoid { get; set; }

    /// <inheritdoc/>
    public override string Name => _name ?? L("Selection");

    /// <inheritdoc/>
    public override bool IsVoid
    {
        get
        {
            if (!VirtualPath.SelectionEqual(_selectionBefore, _selectionAfter))
            {
                return false;
            }

            return CanBeVoid;
        }
    }

    /// <inheritdoc/>
    public override bool Modifying => false;

    /// <inheritdoc/>
    public override void Do()
    {
        if (!_firstDo)
        {
            _view.SetSelection(_selectionAfter, false);
        }
        _firstDo = false;
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _view.SetSelection(_selectionBefore, false);
    }

    /// <summary>
    /// Gets a descriptive tips string for the given selection paths.
    /// </summary>
    /// <param name="path">The array of virtual paths representing the selection.</param>
    /// <returns>A human-readable string describing the selection.</returns>
    internal string GetSelectionPathTips(VirtualPath[] path)
    {
        if (path.Length > 1)
        {
            return string.Format("Select {0} items", path.Length);
        }
        else if (path.Length == 1)
        {
            var node = _view.VirtualModel?.GetNodeByVirtualPath(path[0]);
            if (node != null)
            {
                return string.Format("Select {0}", node.Text);
            }
            else
            {
                return string.Format("Select {0}", path[0]);
            }
        }
        else
        {
            return "Clear selection";
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Select {_selectionAfter?.Length ?? 0} items";
    }
}