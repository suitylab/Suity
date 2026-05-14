using Suity.Editor.Selecting;
using Suity.Selecting;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a selection of struct fields.
/// </summary>
public class DStructFieldSelection : EditorObjectSelection<DStructField>
{
    private readonly DObjectFieldSelectionList _list = new();

    /// <summary>
    /// Initializes a new instance of the DStructFieldSelection class.
    /// </summary>
    public DStructFieldSelection()
    { }

    /// <summary>
    /// Initializes a new instance of the DStructFieldSelection class with an object type.
    /// </summary>
    public DStructFieldSelection(DCompond objType)
    {
        _list.ObjectType = objType;
    }

    /// <inheritdoc />
    public override ISelectionList GetSelectionList() => _list ?? EmptySelectionList.Empty as ISelectionList;

    /// <summary>
    /// Gets or sets the object type.
    /// </summary>
    public DCompond ObjectType
    {
        get => _list.ObjectType;
        set => _list.ObjectType = value;
    }

    /// <inheritdoc />
    protected internal override Guid ResolveId(string key)
    {
        return GlobalIdResolver.Resolve(key);
    }

    /// <inheritdoc />
    protected internal override string ResolveKey(Guid id)
    {
        string key = EditorObjectManager.Instance.GetObject(id)?.FullName;
        if (string.IsNullOrEmpty(key))
        {
            key = GlobalIdResolver.RevertResolve(id);
        }

        return key;
    }

    /// <inheritdoc />
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        // List needs to be processed first, base.Sync will trigger resolution
        if (sync.Intent == SyncIntent.Clone)
        {
            _list.ObjectType = sync.Sync("ObjectType", _list.ObjectType, SyncFlag.ByRef | SyncFlag.NotNull);
        }

        base.Sync(sync, context);
    }
}

/// <summary>
/// Provides a selection list for struct fields.
/// </summary>
internal class DObjectFieldSelectionList : ISelectionList
{
    /// <summary>
    /// Gets or sets the object type.
    /// </summary>
    public DCompond ObjectType { get; set; }

    /// <summary>
    /// Initializes a new instance of the DObjectFieldSelectionList class.
    /// </summary>
    public DObjectFieldSelectionList()
    { }

    /// <summary>
    /// Initializes a new instance of the DObjectFieldSelectionList class with an object type.
    /// </summary>
    public DObjectFieldSelectionList(DCompond objType)
    {
        ObjectType = objType;
    }

    /// <inheritdoc />
    public ISelectionItem GetItem(string key)
    {
        return ObjectType?.GetPublicField(key);
    }

    /// <inheritdoc />
    public IEnumerable<ISelectionItem> GetItems()
    {
        return ObjectType?.PublicFields ?? Array.Empty<ISelectionItem>() as IEnumerable<ISelectionItem>;
    }
}