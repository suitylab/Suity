using Suity;
using Suity.Collections;
using Suity.Editor.Types;
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
/// Menu command to collapse a struct field into its constituent fields.
/// </summary>
public class CollapseStructCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollapseStructCommand"/> class.
    /// </summary>
    public CollapseStructCommand()
        : base("Collapse Struct", CoreIconCache.Box)
    {
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        var objs = EditorUtility.Inspector.DetailTreeSelection;

        Visible = objs.CountOne() && objs.All(o => o is StructField f && f.FieldType.FieldType?.Target is DStruct);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var field = EditorUtility.Inspector.DetailTreeSelection.OfType<StructField>().FirstOrDefault();
        if (field is null)
        {
            return;
        }

        if (field.FieldType?.FieldType?.Target is not DStruct s)
        {
            return;
        }

        if (field.ParentSItem is not StructType parentStruct)
        {
            return;
        }

        if (!s.GetPublicStructFields(true).Any())
        {
            DialogUtility.ShowMessageBoxAsync($"Target struct has no fields");
            return;
        }

        foreach (var f in s.GetPublicStructFields(true))
        {
            if (parentStruct.FieldList.Contains(f.Name))
            {
                DialogUtility.ShowMessageBoxAsync(L("Struct already contains name: ") + f.Name);
                return;
            }
        }

        if (field.GetDocument() is not TypeDesignDocument doc)
        {
            return;
        }

        var action = new CollapseStructAction(doc, field);
        doc.View?.DoServiceAction<UndoRedoManager>(m => m.Do(action));
    }
}

/// <summary>
/// Undo/redo action for collapsing a struct field into its constituent fields.
/// </summary>
internal class CollapseStructAction : BaseRefactorAction
{
    /// <inheritdoc/>
    public override string Name => L("Collapse Struct");

    private readonly TypeDesignDocument _doc;

    private readonly StructType _struct;
    private readonly DStruct _collapseStruct;
    private readonly StructField _collapseField;

    private readonly List<StructField> _fields;
    private readonly List<int> _fieldIndexes;
    private readonly List<Guid> _fieldTargetIds;

    private List<Guid> _fieldIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollapseStructAction"/> class.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="collapseField">The struct field to collapse.</param>
    public CollapseStructAction(TypeDesignDocument doc, StructField collapseField)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _collapseField = collapseField ?? throw new ArgumentNullException(nameof(collapseField));

        _collapseStruct = collapseField.FieldType?.FieldType?.Target as DStruct
            ?? throw new InvalidOperationException();

        int index = collapseField.List.IndexOf(collapseField);

        _fields = [];
        _fieldIndexes = [];
        _fieldTargetIds = [];
        _fieldIds = [];

        foreach (var f in _collapseStruct.GetPublicStructFields(true))
        {
            var field = new StructField
            {
                Name = f.Name
            };
            field.FieldType.FieldType = f.FieldType;

            _fields.Add(field);
            _fieldIndexes.Add(index);
            index++;

            _fieldTargetIds.Add(f.Id);
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
            if (obj.GetItem(collapseFieldId) is not SObject collapseObj)
            {
                continue;
            }

            List<SItem> items = [.. _fieldTargetIds.Select(collapseObj.GetItem)];
            foreach (var item in items.SkipNull())
            {
                collapseObj.RemoveItem(item);
            }

            obj.RepairDeep();

            for (int i = 0; i < _fields.Count; i++)
            {
                var f = _fields[i];
                obj.SetProperty(f.Name, items[i]);
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
        HashSet<Document> docs = [];
        foreach (var obj in EnumerateSObject(_struct.Id))
        {
            List<SItem> items = [.. _fieldIds.Select(obj.GetItem)];
            foreach (var item in items.SkipNull())
            {
                obj.RemoveItem(item);
            }

            obj.RepairDeep();
            if (obj.GetItem(_collapseField.Id) is not SObject collapseObj)
            {
                var type = TypeDefinition.Resolve(_collapseStruct.Id);
                collapseObj = new SObject(type);
                obj.SetProperty(_collapseField.Name, collapseObj);
            }

            for (int i = 0; i < _fields.Count; i++)
            {
                var f = _fields[i];
                collapseObj.SetProperty(f.Name, items[i]);
            }
        }

        SaveDocuments();
    }
}
