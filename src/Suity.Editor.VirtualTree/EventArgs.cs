using System;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Event arguments for requesting a context menu for a node.
/// </summary>
public class RequestContextMenuEventArgs : EventArgs
{
    /// <summary>
    /// Gets the node requesting the context menu.
    /// </summary>
    public VirtualNode Node { get; }

    /// <summary>
    /// Gets or sets the context menu object.
    /// </summary>
    public object ContextMenu { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified node.
    /// </summary>
    /// <param name="node">The node requesting the context menu.</param>
    public RequestContextMenuEventArgs(VirtualNode node)
    {
        Node = node;
    }
}

/// <summary>
/// Event arguments for handling a menu action.
/// </summary>
public class HandleMenuActionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the menu action key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the sender identifier.
    /// </summary>
    public string Sender { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the action was handled.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified key and sender.
    /// </summary>
    /// <param name="key">The menu action key.</param>
    /// <param name="sender">The sender identifier.</param>
    public HandleMenuActionEventArgs(string key, string sender)
    {
        Key = key;
        Sender = sender;
    }
}

/// <summary>
/// Event arguments for selection changes.
/// </summary>
public class SelectionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the array of selected virtual paths.
    /// </summary>
    public VirtualPath[] Selections { get; }

    /// <summary>
    /// Initializes a new instance with the specified selections.
    /// </summary>
    /// <param name="selections">The array of selected paths.</param>
    public SelectionEventArgs(VirtualPath[] selections)
    {
        Selections = selections;
    }
}

/// <summary>
/// Event arguments for virtual node events.
/// </summary>
public class VirtualNodeEventArgs : EventArgs
{
    /// <summary>
    /// Gets the node associated with this event.
    /// </summary>
    public VirtualNode Node { get; }

    /// <summary>
    /// Initializes a new instance with the specified node.
    /// </summary>
    /// <param name="node">The node associated with the event.</param>
    public VirtualNodeEventArgs(VirtualNode node)
    {
        Node = node;
    }
}

/// <summary>
/// Event arguments for virtual node queries that expect a response value.
/// </summary>
public class VirtualNodeQueryEventArgs : EventArgs
{
    /// <summary>
    /// Gets the node being queried.
    /// </summary>
    public VirtualNode Node { get; }

    /// <summary>
    /// Gets or sets the query response value.
    /// </summary>
    public bool Value { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified node.
    /// </summary>
    /// <param name="node">The node being queried.</param>
    public VirtualNodeQueryEventArgs(VirtualNode node)
    {
        Node = node;
    }
}

/// <summary>
/// Event arguments for tree value editing.
/// </summary>
public class TreeValueEditEventArgs : EventArgs
{
    /// <summary>
    /// Gets the edited value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets the property name that was edited.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance with the specified value and property name.
    /// </summary>
    /// <param name="value">The edited value.</param>
    /// <param name="propertyName">The property name.</param>
    public TreeValueEditEventArgs(object value, string propertyName)
    {
        Value = value;
        PropertyName = propertyName;
    }

    /// <summary>
    /// Finds a value of type T in the node or its ancestors.
    /// </summary>
    /// <typeparam name="T">The type to search for.</typeparam>
    /// <returns>The matching value, or default if not found.</returns>
    public virtual T FindValueOrParent<T>()
    {
        return default;
    }
}

/// <summary>
/// Event arguments for virtual node value editing with node reference.
/// </summary>
public class VirtualNodeValueEditEventArgs : TreeValueEditEventArgs
{
    /// <summary>
    /// Gets the node being edited.
    /// </summary>
    internal VirtualNode Node { get; }

    /// <summary>
    /// Initializes a new instance with the specified node, value, and property name.
    /// </summary>
    /// <param name="node">The node being edited.</param>
    /// <param name="value">The edited value.</param>
    /// <param name="propertyName">The property name.</param>
    public VirtualNodeValueEditEventArgs(VirtualNode node, object value, string propertyName)
        : base(value, propertyName)
    {
        Node = node;
    }

    /// <inheritdoc/>
    public override T FindValueOrParent<T>()
    {
        return Node.FindValueOrParent<T>();
    }
}

/// <summary>
/// Event arguments for list item editing.
/// </summary>
public class ListEditEventArgs : EventArgs
{
    /// <summary>
    /// Represents the mode of list editing.
    /// </summary>
    public enum EditMode
    {
        /// <summary>
        /// Editing an existing item.
        /// </summary>
        Edit,

        /// <summary>
        /// Adding a new item.
        /// </summary>
        Add,

        /// <summary>
        /// Removing an item.
        /// </summary>
        Remove,
    }

    /// <summary>
    /// Gets the value associated with the edit.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets the index of the edited item.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the edit mode.
    /// </summary>
    public EditMode Mode { get; }

    /// <summary>
    /// Initializes a new instance with the specified value, index, and mode.
    /// </summary>
    /// <param name="value">The value associated with the edit.</param>
    /// <param name="index">The index of the edited item.</param>
    /// <param name="mode">The edit mode.</param>
    public ListEditEventArgs(object value, int index, EditMode mode)
    {
        Value = value;
        Index = index;
        Mode = mode;
    }
}