using Suity.Collections;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.TypeEdit.Commands;

/// <summary>
/// Menu command to collapse an array field into multiple individual fields.
/// </summary>
internal class CollapseArrayCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollapseArrayCommand"/> class.
    /// </summary>
    public CollapseArrayCommand()
        : base("Collapse Array", CoreIconCache.Array)
    {
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        var objs = EditorUtility.Inspector.DetailTreeSelection;

        Visible = objs.CountOne() && objs.All(o => o is StructField f && f.FieldType.IsArray);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        var fields = EditorUtility.Inspector.DetailTreeSelection.OfType<StructField>();
        if (!fields.CountOne())
        {
            return;
        }

        var field = fields.First();
        if (field.GetDocument() is not TypeDesignDocument doc)
        {
            return;
        }

        string numStr = await DialogUtility.ShowSingleLineTextDialogAsyncL("Enter collapse field count", "1", s =>
        {
            string msg = L("Please enter a positive integer (1-100)");

            if (!int.TryParse(s, out int num) || num < 1 || num >= 100)
            {
                DialogUtility.ShowMessageBoxAsync(msg);
            }

            return true;
        });

        if (string.IsNullOrWhiteSpace(numStr))
        {
            return;
        }
        if (!int.TryParse(numStr, out int count))
        {
            return;
        }
        if (count < 1 || count >= 99)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            string name = KeyIncrementHelper.MakeKey(field.Name, 2, (ulong)(i + 1));
            if (field.List.Contains(name))
            {
                await DialogUtility.ShowMessageBoxAsync(L("Field name already exists: ") + name);
                return;
            }
        }

        var action = new CollapseArrayAction(doc, field, count);
        doc.View?.DoServiceAction<UndoRedoManager>(m => m.Do(action));
    }
}

/// <summary>
/// Undo/redo action for collapsing an array field into multiple individual fields.
/// </summary>
internal class CollapseArrayAction : BaseRefactorAction
{
    /// <inheritdoc/>
    public override string Name => L("Collapse Array");

    private readonly TypeDesignDocument _doc;
    private readonly StructType _struct;
    private readonly StructField _collapseField;

    private readonly List<StructField> _fields;
    private readonly List<int> _fieldIndexes;
    private List<Guid> _fieldIds;

    
    /// <summary>
    /// Initializes a new instance of the <see cref="CollapseArrayAction"/> class.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="collapseField">The array field to collapse.</param>
    /// <param name="count">The number of fields to create from the array.</param>
    public CollapseArrayAction(TypeDesignDocument doc, StructField collapseField, int count)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _collapseField = collapseField ?? throw new ArgumentNullException(nameof(collapseField));

        int index = collapseField.List.IndexOf(collapseField);

        _fields = [];
        _fieldIndexes = [];
        _fieldIds = [];

        string originName = collapseField.Name;
        for (int i = 0; i < count; i++)
        {
            string name = KeyIncrementHelper.MakeKey(originName, 2, (ulong)(i + 1));
            var field = new StructField
            {
                Name = name,
            };

            field.FieldType.FieldType = collapseField.FieldType.FieldType.ElementType;

            _fields.Add(field);
            _fieldIndexes.Add(index);
            index++;

            _fieldIds.Add(field.Id); // Initially no Id, just placeholder
        }

        if (_fields.Count == 0)
        {
            throw new InvalidOperationException("Field count == 0");
        }

        _struct = _collapseField.ParentSItem as StructType
            ?? throw new InvalidOperationException("Struct is null");
    }

    /// <inheritdoc/>
    public override bool Modifying => false;

    /// <inheritdoc/>
    public override void Do()
    {
        // All affected documents need to be opened first, otherwise reopening after refactoring will cause data loss
        PreopenDocument(_struct.Id);

        var fieldList = _struct.FieldList;

        Guid collapseFieldId = _collapseField.Id;

        // Remove collapsed field
        fieldList.Remove(_collapseField);

        // Transfer fields
        for (int i = 0; i < _fields.Count; i++)
        {
            var field = _fields[i];
            var index = _fieldIndexes[i];

            fieldList.Insert(index, field);
        }
        // Execute reordering once
        _struct.FieldList.ArrangeItem();

        // Get Id in the new struct
        _fieldIds = [.. _fields.Select(o => o.Id)];

        // Save
        _doc.Entry.ForceSave();

        // Refresh
        _doc.View?.RefreshView();
        EditorUtility.Inspector.UpdateInspector();

        // Select
        EditorUtility.Inspector.DetailTreeSelection = _fields;

        // Migrate values
        foreach (var obj in EnumerateSObject(_struct.Id))
        {
            if (obj.GetItem(collapseFieldId) is not SArray ary)
            {
                continue;
            }

            List<SItem> items = [.. ary.Items];
            ary.Clear();

            obj.RepairDeep();

            for (int i = 0; i < _fields.Count; i++)
            {
                var f = _fields[i];
                if (i < items.Count)
                {
                    obj.SetProperty(f.Name, items[i]);
                }
            }
        }

        SaveDocuments();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        // All affected documents need to be opened first, otherwise reopening after refactoring will cause data loss
        PreopenDocument(_struct.Id);

        var fieldList = _struct.FieldList;

        // Remove fields
        foreach (var field in _fields)
        {
            field.List?.Remove(field);
        }

        // Add original field
        fieldList.Insert(_fieldIndexes[0], _collapseField);
        // Execute reordering once
        _struct.FieldList.ArrangeItem();

        // Save
        _doc.Entry.ForceSave();

        // Refresh
        _doc.View?.RefreshView();
        EditorUtility.Inspector.UpdateInspector();

        // Select
        EditorUtility.Inspector.DetailTreeSelection = [_collapseField];

        // Migrate values
        foreach (var obj in EnumerateSObject(_struct.Id))
        {
            List<SItem> items = [.. _fieldIds.Select(obj.GetItem)];
            foreach (var item in items.SkipNull())
            {
                obj.RemoveItem(item);
            }

            obj.RepairDeep();

            if (obj.GetItem(_collapseField.Id) is not SArray ary)
            {
                ary = new SArray(_collapseField.FieldType.FieldType);
                obj.SetProperty(_collapseField.Name, ary);
            }

            foreach (var item in items)
            {
                ary.Add(item);
            }
            ary.RepairDeep();
        }

        SaveDocuments();
    }
}
