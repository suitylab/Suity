using Suity.Collections;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Documents.TypeEdit.Commands;

/// <summary>
/// Menu command to extract multiple fields of the same type into an array field.
/// </summary>
public class ExtractArrayCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractArrayCommand"/> class.
    /// </summary>
    public ExtractArrayCommand()
        : base("Extract Array", CoreIconCache.Array)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        var objs = EditorUtility.Inspector.DetailTreeSelection;

        Visible = objs.Any() && objs.All(o => o is StructField f && !f.FieldType.IsArray)
            && objs.OfType<StructField>().Select(o => o.FieldType.FieldType).AllEqual();
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var fields = EditorUtility.Inspector.DetailTreeSelection.OfType<StructField>();
        if (!fields.Any())
        {
            return;
        }

        if (fields.First().GetDocument() is not TypeDesignDocument doc)
        {
            return;
        }

        var action = new ExtractArrayAction(doc, fields);
        doc.View?.DoServiceAction<UndoRedoManager>(m => m.Do(action));
    }
}

/// <summary>
/// Undo/redo action for extracting multiple fields into an array field.
/// </summary>
internal class ExtractArrayAction : BaseRefactorAction
{
    /// <inheritdoc/>
    public override string Name => "Extract Array";

    private readonly TypeDesignDocument _doc;
    private readonly StructType _struct;

    private readonly List<StructField> _fields;
    private readonly List<int> _fieldIndexes;
    private List<Guid> _fieldOldIds;

    private StructField _newField;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractArrayAction"/> class.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="fields">The fields to extract into an array.</param>
    public ExtractArrayAction(TypeDesignDocument doc, IEnumerable<StructField> fields)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _fields = [.. fields.OrderBy(o => o.List.IndexOf(o))];
        if (_fields.Count == 0)
        {
            throw new InvalidOperationException("Field count == 0");
        }

        _fieldIndexes = [.. _fields.Select(o => o.List.IndexOf(o))];
        _fieldOldIds = [.. _fields.Select(o => o.Id)];

        _struct = _fields[0].ParentSItem as StructType
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

        // Create extracted field
        if (_newField is null)
        {
            string fieldName = fieldList.GetSuggestedFieldName("ExtractedArray");
            _newField = new StructField { Name = fieldName };
        }

        // Remove fields
        foreach (StructField field in _fields)
        {
            field.List?.Remove(field);
        }

        // Add extracted field
        int index = _fieldIndexes[0];
        fieldList.Insert(index, _newField);
        fieldList.ArrangeItem();

        // Set field type to array
        _newField.FieldType.FieldType = _fields[0].FieldType.FieldType.MakeArrayType();

        // Save
        _doc.Entry.ForceSave();

        // Refresh
        _doc.View?.RefreshView();
        EditorUtility.Inspector.UpdateInspector();

        // Select
        EditorUtility.Inspector.DetailTreeSelection = [_newField];

        // Migrate values
        foreach (var obj in EnumerateSObject(_struct.Id))
        {
            List<SItem> items = [.. _fieldOldIds.Select(obj.GetItem)];
            foreach (var item in items.SkipNull())
            {
                obj.RemoveItem(item);
            }

            obj.RepairDeep();

            if (obj.GetItem(_newField.Id) is not SArray ary)
            {
                ary = new SArray(_newField.FieldType.FieldType);
                obj.SetProperty(_newField.Name, ary);
            }

            foreach (var item in items)
            {
                ary.Add(item);
            }
            ary.RepairDeep();
        }

        SaveDocuments();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        // All affected documents need to be opened first, otherwise reopening after refactoring will cause data loss
        foreach (var docHost in ReferenceManager.Current.FindReferenceHosts(_struct.Id).OfType<DocumentReferenceHost>())
        {
            docHost.OpenDocument();
        }

        var fieldList = _struct.FieldList;
        Guid newFieldId = _newField.Id;

        // Remove new field
        fieldList.Remove(_newField);

        // Transfer fields
        for (int i = 0; i < _fields.Count; i++)
        {
            var field = _fields[i];
            var index = _fieldIndexes[i];

            fieldList.Insert(index, field);
        }
        // Execute reordering once
        _struct.FieldList.ArrangeItem();

        // Get Id in the old struct
        _fieldOldIds = [.. _fields.Select(o => o.Id)];

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
            if (obj.GetItem(newFieldId) is not SArray ary)
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
}
