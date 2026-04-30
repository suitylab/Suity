using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor;
using Suity.Synchonizing;

namespace Suity.Selecting;

/// <summary>
/// Represents a single selection from a selection list, with synchronization support.
/// </summary>
public sealed class Selection : ISelection, ISyncObject
{
    ISelectionList _list;
    bool _optional;

    private string _key;

    /// <summary>
    /// Initializes a new empty instance of <see cref="Selection"/>.
    /// </summary>
    public Selection()
    {
        _list = EmptySelectionList.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Selection"/> with the specified list.
    /// </summary>
    /// <param name="list">The selection list to use.</param>
    /// <param name="optional">Whether an empty selection is considered valid.</param>
    public Selection(ISelectionList list, bool optional = true)
    {
        _list = list ?? throw new System.ArgumentNullException(nameof(list));
        _optional = optional;
    }

    #region ISelection

    /// <summary>
    /// Gets the selection list associated with this selection.
    /// </summary>
    public ISelectionList GetList() => _list ?? EmptySelectionList.Empty;

    /// <summary>
    /// Gets or sets the key of the currently selected item.
    /// </summary>
    public string SelectedKey
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current selection is valid.
    /// Returns true if the key is empty and the selection is optional, or if the key corresponds to an existing item.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_key))
            {
                return _optional;
            }

            return _list?.GetItem(_key) != null;
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets the selection list.
    /// </summary>
    public ISelectionList List
    {
        get => _list;
        set => _list = value ?? EmptySelectionList.Empty;
    }

    /// <summary>
    /// Gets the currently selected item, or null if no item is selected.
    /// </summary>
    public ISelectionItem SelectedItem => _list?.GetItem(_key);

    /// <summary>
    /// Synchronizes this selection with the specified property sync context.
    /// </summary>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        SelectedKey = sync.Sync("Key", SelectedKey, SyncFlag.None, string.Empty);

        if (sync.Intent == SyncIntent.Clone)
        {
            _list = sync.Sync("List", _list, SyncFlag.ByRef);
            _optional = sync.Sync("Optional", _optional);
        }
    }

    /// <summary>
    /// Gets the display text of the currently selected item.
    /// </summary>
    public string DisplayText => _list?.GetItem(_key).ToDisplayText() ?? string.Empty;

    /// <summary>
    /// Returns the localized display text of the selected item.
    /// </summary>
    public override string ToString() => L(DisplayText);
}

/// <summary>
/// Represents a strongly-typed single selection from a selection list, with synchronization support.
/// </summary>
public sealed class Selection<T> : ISelection, ISyncObject
    where T : class, ISelectionItem
{
    ISelectionList _list;
    bool _optional;

    private string _key;

    /// <summary>
    /// Initializes a new empty instance of <see cref="Selection{T}"/>.
    /// </summary>
    public Selection()
    {
        _list = new SelectionList<T>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Selection{T}"/>.
    /// </summary>
    /// <param name="optional">Whether an empty selection is considered valid.</param>
    public Selection(bool optional)
    {
        _list = new SelectionList<T>();
        _optional = optional;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Selection{T}"/> with the specified list.
    /// </summary>
    /// <param name="list">The selection list to use.</param>
    /// <param name="optional">Whether an empty selection is considered valid.</param>
    public Selection(SelectionList<T> list, bool optional = true)
    {
        _list = list ?? throw new System.ArgumentNullException(nameof(list));
        _optional = optional;
    }

    #region ISelection

    /// <summary>
    /// Gets the selection list associated with this selection.
    /// </summary>
    public ISelectionList GetList() => _list  ?? EmptySelectionList.Empty;

    /// <summary>
    /// Gets or sets the key of the currently selected item.
    /// </summary>
    public string SelectedKey
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current selection is valid.
    /// Returns true if the key is empty and the selection is optional, or if the key corresponds to an existing item.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_key))
            {
                return _optional;
            }

            return _list?.GetItem(_key) != null;
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets the selection list.
    /// </summary>
    public ISelectionList List
    {
        get => _list;
        set => _list = value ?? EmptySelectionList.Empty;
    }

    /// <summary>
    /// Gets the currently selected item as type <typeparamref name="T"/>, or null if no item is selected.
    /// </summary>
    public T SelectedItem => _list?.GetItem(_key) as T;

    /// <summary>
    /// Synchronizes this selection with the specified property sync context.
    /// </summary>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        SelectedKey = sync.Sync("Key", SelectedKey, SyncFlag.None, string.Empty);

        if (sync.Intent == SyncIntent.Clone)
        {
            _list = sync.Sync("List", _list, SyncFlag.ByRef);
            _optional = sync.Sync("Optional", _optional);
        }
    }

    /// <summary>
    /// Gets the display text of the currently selected item.
    /// </summary>
    public string DisplayText => _list?.GetItem(_key).ToDisplayText() ?? string.Empty;

    /// <summary>
    /// Returns the localized display text of the selected item.
    /// </summary>
    public override string ToString() => L(DisplayText);
}