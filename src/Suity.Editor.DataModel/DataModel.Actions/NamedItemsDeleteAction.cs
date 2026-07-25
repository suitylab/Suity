using Suity.Editor.AIGC.Assistants;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.DataModel.Actions;

/// <summary>
/// Action that deletes one or more named items from the document.
/// </summary>
class NamedItemsDeleteAction : AIGenerativeApplyAction
{
    readonly List<DeleteRecord> _records = [];
    readonly string _actionName;

    /// <summary>
    /// Record for tracking a single item deletion.
    /// </summary>
    private class DeleteRecord
    {
        public NamedItem Item;
        public INamedItemList List;
        public int Index;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemsDeleteAction"/> class for a single item.
    /// </summary>
    /// <param name="item">The named item to delete.</param>
    public NamedItemsDeleteAction(NamedItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _records.Add(new DeleteRecord
        {
            Item = item,
            List = item.ParentList,
            Index = item.GetIndex(),
        });

        _actionName = $"Delete {item.Name}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemsDeleteAction"/> class for multiple items.
    /// </summary>
    /// <param name="items">The named items to delete.</param>
    public NamedItemsDeleteAction(IEnumerable<NamedItem> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            if (item is null)
                continue;

            _records.Add(new DeleteRecord
            {
                Item = item,
                List = item.ParentList,
                Index = item.GetIndex(),
            });
        }

        if (_records.Count == 0)
            throw new ArgumentException("No valid items to delete.");

        _actionName = _records.Count == 1
            ? $"Delete {_records[0].Item.Name}"
            : $"Delete {_records.Count} items";
    }

    /// <summary>
    /// Gets the display name of this action.
    /// </summary>
    public override string Name => _actionName;

    /// <summary>
    /// Gets the objects that were affected by this action.
    /// </summary>
    /// <returns>An empty array since the items are deleted.</returns>
    public override object[] GetAppliedObjects() => [];

    /// <summary>
    /// Executes the delete action by removing the items from their parent lists.
    /// </summary>
    public override void Do()
    {
        foreach (var record in _records)
        {
            record.List?.Remove(record.Item);
        }
    }

    /// <summary>
    /// Undoes the delete action by reinserting the items at their original positions.
    /// </summary>
    public override void Undo()
    {
        // Undo in reverse order to maintain correct indices
        foreach (var record in _records.AsEnumerable().Reverse())
        {
            if (record.List is null)
                continue;

            if (record.Index >= 0 && record.Index <= record.List.Count)
            {
                record.List.Insert(record.Index, record.Item);
            }
            else
            {
                record.List.Add(record.Item);
            }
        }
    }
}
