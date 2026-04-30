using static Suity.Helpers.GlobalLocalizer;
using Suity.UndoRedos;
using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Wraps an <see cref="IValueAction"/> as an <see cref="UndoRedoAction"/> for integration
/// with the undo/redo system of the property grid.
/// </summary>
public class PropertyGridActionWrapper(ImGuiPropertyGrid grid, IValueAction action) : UndoRedoAction
{
    private readonly ImGuiPropertyGrid _grid = grid ?? throw new ArgumentNullException(nameof(grid));
    private readonly IValueAction _action = action ?? throw new ArgumentNullException(nameof(action));

    /// <inheritdoc/>
    public override string Name => _action.ToString();

    /// <inheritdoc/>
    public override void Do()
    {
        _action.DoAction();

        _grid.NotifyObjectPropertyChanged(_action.ParentObjects, _action.Name ?? string.Empty);
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _action.UndoAction();

        _grid.NotifyObjectPropertyChanged(_action.ParentObjects, _action.Name ?? string.Empty);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _action.ToString();
    }
}
