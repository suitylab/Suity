using System;

namespace Suity.UndoRedos;

/// <summary>
/// Provides data for undo/redo action events.
/// </summary>
public class UndoRedoActionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the action associated with this event.
    /// </summary>
    public UndoRedoAction Action { get; }
    /// <summary>
    /// Gets or sets a value indicating whether the event has been handled.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoRedoActionEventArgs"/> class.
    /// </summary>
    /// <param name="action">The action associated with this event.</param>
    public UndoRedoActionEventArgs(UndoRedoAction action)
    {
        Action = action;
    }
}

/// <summary>
/// Provides data for macro events.
/// </summary>
public class MacroEventArgs : EventArgs
{
    /// <summary>
    /// Represents an empty instance of <see cref="MacroEventArgs"/>.
    /// </summary>
    public new static readonly MacroEventArgs Empty = new MacroEventArgs();

    /// <summary>
    /// Gets the name of the macro.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroEventArgs"/> class.
    /// </summary>
    public MacroEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroEventArgs"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    public MacroEventArgs(string name)
    {
        Name = name;
    }
}