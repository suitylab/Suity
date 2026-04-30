using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Represents the context in which a setter action is executed.
/// </summary>
public interface ISetterContext
{
}

/// <summary>
/// Represents an action that can be performed and undone on a value, typically used in property editors.
/// </summary>
public interface IValueAction
{
    /// <summary>
    /// Gets the collection of parent objects associated with this action.
    /// </summary>
    IEnumerable<object> ParentObjects { get; }

    /// <summary>
    /// Gets the name of the action, used for display or identification purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the action should preview its effects before committing.
    /// </summary>
    bool Preview { get; set; }

    /// <summary>
    /// Executes the action, applying the intended changes.
    /// </summary>
    void DoAction();

    /// <summary>
    /// Undoes the action, reverting the changes made by DoAction.
    /// </summary>
    void UndoAction();
}

/// <summary>
/// Represents a record that holds setter data for property editing operations.
/// </summary>
public interface ISetterDataRecord
{
    /// <summary>
    /// Gets or sets the data associated with this setter record.
    /// </summary>
    object Data { get; set; }
}

/// <summary>
/// Represents an empty value action that performs no operations. Used as a default or placeholder.
/// </summary>
public class EmptyValueAction : IValueAction
{
    /// <summary>
    /// Gets the singleton instance of the empty value action.
    /// </summary>
    public static readonly EmptyValueAction Empty = new();

    private EmptyValueAction()
    {
    }

    /// <summary>
    /// Gets an empty collection of parent objects.
    /// </summary>
    public IEnumerable<object> ParentObjects => [];

    /// <summary>
    /// Gets null, indicating this action has no name.
    /// </summary>
    public string Name => null;

    /// <summary>
    /// Gets true, indicating preview mode is always enabled for the empty action. Setting has no effect.
    /// </summary>
    public bool Preview { get => true; set { } }

    /// <summary>
    /// Does nothing. This is a no-op implementation.
    /// </summary>
    public void DoAction()
    {
    }

    /// <summary>
    /// Does nothing. This is a no-op implementation.
    /// </summary>
    public void UndoAction()
    {
    }
}
