using System;

namespace Suity.UndoRedos;

/// <summary>
/// Provides information about an undo/redo action.
/// </summary>
public interface IUndoRedoActionInfo
{
    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Gets the help information associated with the action.
    /// </summary>
    HelpInfo Help { get; }
}

/// <summary>
/// Represents a base class for undo/redo actions.
/// </summary>
public abstract class UndoRedoAction : IUndoRedoActionInfo
{
    /// <summary>
    /// Gets the name of this action.
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// Gets a value indicating whether this action is a void action and should be ignored.
    /// </summary>
    public virtual bool IsVoid => false;
    /// <summary>
    /// Gets the help information associated with this action.
    /// </summary>
    public virtual HelpInfo Help => null;
    /// <summary>
    /// Gets a value indicating whether this action modifies the document.
    /// </summary>
    public virtual bool Modifying => true;

    /// <summary>
    /// Determines whether the specified action can be appended to this action.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <returns>True if the action can be appended; otherwise, false.</returns>
    public virtual bool CanAppend(UndoRedoAction action) => false;

    /// <summary>
    /// Appends the specified action to this action.
    /// </summary>
    /// <param name="action">The action to append.</param>
    /// <param name="performAction">Whether to perform the action immediately.</param>
    public virtual void Append(UndoRedoAction action, bool performAction)
    { }

    /// <summary>
    /// Executes the action.
    /// </summary>
    public abstract void Do();

    /// <summary>
    /// Undoes the action.
    /// </summary>
    public abstract void Undo();

    /// <summary>
    /// Gets or sets an action to be executed after the view is updated.
    /// </summary>
    public Action PostViewAction { get; set; }

    /// <summary>
    /// Returns the name of this action.
    /// </summary>
    public override string ToString() => Name;
}