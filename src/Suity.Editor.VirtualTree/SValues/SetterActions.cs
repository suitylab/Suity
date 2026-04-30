using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Values;

namespace Suity.Editor.VirtualTree.SValues;

/// <summary>
/// Base class for undo/redo actions that operate on an <see cref="SArrayNode"/>.
/// Holds a reference to the underlying <see cref="SArray"/> for convenience.
/// </summary>
internal abstract class EditorArrayNodeAction : VirtualNodeSetterAction
{
    /// <summary>
    /// The <see cref="SArray"/> displayed by the associated node.
    /// </summary>
    internal SArray _list;

    /// <summary>
    /// Initializes a new instance with a node reference.
    /// </summary>
    /// <param name="node">The array node to operate on.</param>
    public EditorArrayNodeAction(SArrayNode node)
        : this(node, null)
    {
    }

    /// <summary>
    /// Initializes a new instance with a node and property name.
    /// </summary>
    /// <param name="node">The array node to operate on.</param>
    /// <param name="propertyName">The name of the property being modified.</param>
    public EditorArrayNodeAction(SArrayNode node, string propertyName)
        : base(node, propertyName)
    {
        _list = node.DisplayedArray;
    }
}

/// <summary>
/// Action that sets (replaces) the value of an element at a specific index in an <see cref="SArray"/>.
/// </summary>
internal class EditorArrayValueSetterAction : EditorArrayNodeAction
{
    private readonly int _index;
    private readonly object _oldValue;
    private readonly object _newValue;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance with the target index and new value.
    /// </summary>
    /// <param name="node">The array node whose value is being set.</param>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="value">The new value to assign.</param>
    public EditorArrayValueSetterAction(SArrayNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _newValue = value;

        if (_list != null)
        {
            _oldValue = _list[index];
        }
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_list != null)
        {
            _list[_index] = _newValue;
            Model.NotifyListEdited(_newValue, _index, ListEditEventArgs.EditMode.Edit);
        }

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_oldValue is SItem sItem)
        {
            sItem.Unparent();
        }

        if (_list != null)
        {
            _list[_index] = _oldValue;
            Model.NotifyListEdited(_oldValue, _index, ListEditEventArgs.EditMode.Edit);
        }

        base.Undo();
    }
}

/// <summary>
/// Action that inserts a new element at a specific index in an <see cref="SArray"/>.
/// </summary>
internal class EditorArrayInsertSetterAction : EditorArrayNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance with the insertion index and value.
    /// </summary>
    /// <param name="node">The array node to insert into.</param>
    /// <param name="index">The zero-based index at which to insert the element.</param>
    /// <param name="value">The value to insert.</param>
    public EditorArrayInsertSetterAction(SArrayNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _value = value;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_list != null)
        {
            _list.Insert(_index, _value);
            Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);
        }

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_list != null)
        {
            _list.RemoveAt(_index);
            Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);
        }

        base.Undo();
    }
}

/// <summary>
/// Action that removes an element at a specific index from an <see cref="SArray"/>.
/// </summary>
internal class EditorArrayRemoveSetterAction : EditorArrayNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance with the removal index.
    /// </summary>
    /// <param name="node">The array node to remove from.</param>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public EditorArrayRemoveSetterAction(SArrayNode node, int index)
        : base(node)
    {
        _index = index;

        if (_list != null)
        {
            _value = _list[index];
        }
    }

    /// <inheritdoc/>
    public override void Do()
    {
        if (_list != null)
        {
            _list.RemoveAt(_index);
            Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);
        }

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_list != null)
        {
            _list.Insert(_index, _value);
            Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);
        }

        base.Undo();
    }
}
