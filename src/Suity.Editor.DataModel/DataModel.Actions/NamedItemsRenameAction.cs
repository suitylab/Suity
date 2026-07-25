using Suity.Editor.AIGC.Assistants;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.DataModel.Actions;

/// <summary>
/// Action that renames one or more named items in the document.
/// </summary>
class NamedItemsRenameAction : AIGenerativeApplyAction
{
    readonly List<RenameRecord> _records = [];
    readonly string _actionName;

    /// <summary>
    /// Record for tracking a single item rename.
    /// </summary>
    private class RenameRecord
    {
        public NamedItem Item;
        public string OldName;
        public string NewName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemsRenameAction"/> class for a single item.
    /// </summary>
    /// <param name="item">The named item to rename.</param>
    /// <param name="newName">The new name for the item.</param>
    public NamedItemsRenameAction(NamedItem item, string newName)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _records.Add(new RenameRecord
        {
            Item = item,
            OldName = item.Name,
            NewName = newName,
        });

        _actionName = L($"Rename {_records[0].OldName} > {_records[0].NewName}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItemsRenameAction"/> class for multiple items.
    /// </summary>
    /// <param name="renames">A collection of (item, newName) pairs.</param>
    public NamedItemsRenameAction(IEnumerable<(NamedItem Item, string NewName)> renames)
    {
        if (renames is null)
            throw new ArgumentNullException(nameof(renames));

        foreach (var (item, newName) in renames)
        {
            if (item is null)
                continue;

            _records.Add(new RenameRecord
            {
                Item = item,
                OldName = item.Name,
                NewName = newName,
            });
        }

        if (_records.Count == 0)
            throw new ArgumentException("No valid items to rename.");

        _actionName = _records.Count == 1
            ? L($"Rename {_records[0].OldName} > {_records[0].NewName}")
            : L($"Rename {_records.Count} items");
    }

    /// <summary>
    /// Gets the display name of this action.
    /// </summary>
    public override string Name => _actionName;

    /// <summary>
    /// Gets the objects that were affected by this action.
    /// </summary>
    /// <returns>An array containing the renamed items.</returns>
    public override object[] GetAppliedObjects() => _records.Select(r => r.Item).ToArray();

    /// <summary>
    /// Executes the rename action by setting the new names.
    /// </summary>
    public override void Do()
    {
        foreach (var record in _records)
        {
            record.Item.Name = record.NewName;
        }
    }

    /// <summary>
    /// Undoes the rename action, restoring the original names.
    /// </summary>
    public override void Undo()
    {
        for (int i = _records.Count - 1; i >= 0; i--)
        {
            _records[i].Item.Name = _records[i].OldName;
        }
    }
}
