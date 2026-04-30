using Suity.Views.Im;
using System;

namespace Suity.UndoRedos;

/// <summary>
/// An undo/redo action that wraps an <see cref="IValueAction"/> for value-based operations.
/// </summary>
public class UndoRedoValueAction : UndoRedoAction
{
    private readonly IValueAction _action;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoRedoValueAction"/> class.
    /// </summary>
    /// <param name="action">The value action to wrap.</param>
    public UndoRedoValueAction(IValueAction action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Gets the name of this action.
    /// </summary>
    public override string Name => _action.ToString();

    /// <summary>
    /// Executes the action.
    /// </summary>
    public override void Do()
    {
        _action.DoAction();
    }

    /// <summary>
    /// Undoes the action.
    /// </summary>
    public override void Undo()
    {
        _action.UndoAction();
    }

    public override string ToString()
    {
        return _action.ToString();
    }
}