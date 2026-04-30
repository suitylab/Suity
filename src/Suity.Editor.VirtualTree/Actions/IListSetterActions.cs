using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.VirtualTree.Nodes;
using System.Collections;

namespace Suity.Editor.VirtualTree.Actions;

/// <summary>
/// Base class for actions that modify <see cref="IList"/>-based virtual nodes.
/// </summary>
internal abstract class IListNodeAction : VirtualNodeSetterAction
{
    private readonly IList _list;

    /// <summary>
    /// Initializes a new instance without a specific property name.
    /// </summary>
    /// <param name="node">The virtual node representing the list.</param>
    public IListNodeAction(VirtualNode node)
        : this(node, null)
    {
    }

    /// <summary>
    /// Initializes a new instance with an optional property name.
    /// </summary>
    /// <param name="node">The virtual node representing the list.</param>
    /// <param name="propertyName">The name of the property being modified, or null.</param>
    public IListNodeAction(VirtualNode node, string propertyName)
        : base(node, propertyName)
    {
        _list = (node as IListVirtualNode)?.DisplayedIList;
    }

    /// <summary>
    /// Gets the underlying <see cref="IList"/> instance associated with the node.
    /// </summary>
    /// <returns>The <see cref="IList"/> instance, or null if not available.</returns>
    protected IList GetIList()
    {
        return _list;
    }
}

/// <summary>
/// Represents an undoable action that edits an existing item in an <see cref="IList"/>-based virtual node.
/// </summary>
internal class IListResolverSetterAction : IListNodeAction
{
    private readonly int _index;
    private readonly object _oldValue;
    private readonly object _newValue;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="IListResolverSetterAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node containing the item to edit.</param>
    /// <param name="index">The zero-based index of the item to edit.</param>
    /// <param name="value">The new value to assign to the item.</param>
    public IListResolverSetterAction(IListVirtualNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _newValue = value;

        IList list = GetIList();
        if (list != null)
        {
            _oldValue = list[index];
        }
    }

    /// <inheritdoc/>
    public override void Do()
    {
        IList list = GetIList();
        if (list != null)
        {
            list[_index] = _newValue;
        }
        Model.NotifyListEdited(_newValue, _index, ListEditEventArgs.EditMode.Edit);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        IList list = GetIList();
        if (list != null)
        {
            list[_index] = _oldValue;
        }

        Model.NotifyListEdited(_oldValue, _index, ListEditEventArgs.EditMode.Edit);

        base.Undo();
    }
}

/// <summary>
/// Represents an undoable action that inserts a new item into an <see cref="IList"/>-based virtual node.
/// </summary>
internal class IListResolverInsertAction : IListNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="IListResolverInsertAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node to insert into.</param>
    /// <param name="index">The zero-based index at which to insert the item.</param>
    /// <param name="value">The value to insert.</param>
    public IListResolverInsertAction(IListVirtualNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _value = value;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        IList list = GetIList();
        list?.Insert(_index, _value);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        IList list = GetIList();
        list?.RemoveAt(_index);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);

        base.Undo();
    }
}

/// <summary>
/// Represents an undoable action that removes an item from an <see cref="IList"/>-based virtual node.
/// </summary>
internal class IListResolverRemoveAction : IListNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="IListResolverRemoveAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node to remove from.</param>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public IListResolverRemoveAction(IListVirtualNode node, int index)
        : base(node)
    {
        _index = index;

        IList list = GetIList();
        if (list != null)
        {
            _value = list[index];
        }
    }

    /// <inheritdoc/>
    public override void Do()
    {
        IList list = GetIList();
        list?.RemoveAt(_index);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        IList list = GetIList();
        list?.Insert(_index, _value);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);

        base.Undo();
    }
}
