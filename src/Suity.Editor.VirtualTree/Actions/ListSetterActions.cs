using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.VirtualTree.Nodes;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;

namespace Suity.Editor.VirtualTree.Actions;

/// <summary>
/// Base class for actions that modify list nodes in the virtual tree.
/// Provides helper methods for setting, inserting, and removing list items via synchronization.
/// </summary>
internal abstract class ListNodeAction : VirtualNodeSetterAction
{
    internal readonly IViewNode _treeNode;
    internal readonly ISyncList _syncList;

    /// <summary>
    /// Initializes a new instance without a specific property name.
    /// </summary>
    /// <param name="node">The virtual node representing the list.</param>
    public ListNodeAction(VirtualNode node)
        : this(node, null)
    {
    }

    /// <summary>
    /// Initializes a new instance with an optional property name.
    /// </summary>
    /// <param name="node">The virtual node representing the list.</param>
    /// <param name="propertyName">The name of the property being modified, or null.</param>
    public ListNodeAction(VirtualNode node, string propertyName)
        : base(node, propertyName)
    {
        object obj = node.DisplayedValue;

        _treeNode = obj as IViewNode;
        _syncList = obj as ISyncList;
    }

    /// <summary>
    /// Sets the value of a list item at the specified index via synchronization.
    /// </summary>
    /// <param name="index">The zero-based index of the item to set.</param>
    /// <param name="value">The new value to assign.</param>
    protected void SetItem(int index, object value)
    {
        IIndexSync sync = SingleIndexSync.CreateSetter(index, value);

        _treeNode?.GetList()?.Sync(sync, this);
        _syncList?.Sync(sync, this);
    }

    /// <summary>
    /// Inserts a new item into the list at the specified index via synchronization.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the item.</param>
    /// <param name="value">The value to insert.</param>
    protected void Insert(int index, object value)
    {
        IIndexSync sync = SingleIndexSync.CreateInserter(index, value);

        _treeNode?.GetList()?.Sync(sync, this);
        _syncList?.Sync(sync, this);
    }

    /// <summary>
    /// Removes the item at the specified index from the list via synchronization.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    protected void RemoveAt(int index)
    {
        IIndexSync sync = SingleIndexSync.CreateRemover(index);

        _treeNode?.GetList()?.Sync(sync, this);
        _syncList?.Sync(sync, this);
    }
}

/// <summary>
/// Represents an undoable action that edits an existing item in a list-based virtual node.
/// </summary>
internal class ListEditorSetterAction : ListNodeAction
{
    private readonly int _index;
    private readonly object _oldValue;
    private readonly object _newValue;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListEditorSetterAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node containing the item to edit.</param>
    /// <param name="index">The zero-based index of the item to edit.</param>
    /// <param name="value">The new value to assign to the item.</param>
    public ListEditorSetterAction(ListVirtualNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _newValue = value;
        _oldValue = node.Adapter.GetItem(index);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        SetItem(_index, _newValue);
        Model.NotifyListEdited(_newValue, _index, ListEditEventArgs.EditMode.Edit);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        SetItem(_index, _oldValue);
        Model.NotifyListEdited(_oldValue, _index, ListEditEventArgs.EditMode.Edit);

        base.Undo();
    }
}

/// <summary>
/// Represents an undoable action that inserts a new item into a list-based virtual node.
/// </summary>
internal class ListEditorInsertAction : ListNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListEditorInsertAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node to insert into.</param>
    /// <param name="index">The zero-based index at which to insert the item.</param>
    /// <param name="value">The value to insert.</param>
    public ListEditorInsertAction(ListVirtualNode node, int index, object value)
        : base(node)
    {
        _index = index;
        _value = value;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        Insert(_index, _value);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        RemoveAt(_index);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);

        base.Undo();
    }
}

/// <summary>
/// Represents an undoable action that removes an item from a list-based virtual node.
/// </summary>
internal class ListEditorRemoveAction : ListNodeAction
{
    private readonly int _index;
    private readonly object _value;

    /// <inheritdoc/>
    public override string Name => string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListEditorRemoveAction"/> class.
    /// </summary>
    /// <param name="node">The list virtual node to remove from.</param>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public ListEditorRemoveAction(ListVirtualNode node, int index)
        : base(node)
    {
        _index = index;
        _value = node.Adapter.GetItem(index);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        RemoveAt(_index);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Remove);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        Insert(_index, _value);
        Model.NotifyListEdited(_value, _index, ListEditEventArgs.EditMode.Add);

        base.Undo();
    }
}
