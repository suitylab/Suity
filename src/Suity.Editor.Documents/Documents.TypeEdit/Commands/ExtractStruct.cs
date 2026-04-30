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
/// Menu command to extract selected struct fields into a new struct type.
/// </summary>
public class ExtractStructCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractStructCommand"/> class.
    /// </summary>
    public ExtractStructCommand()
        : base("Extract Struct", CoreIconCache.Box)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        var objs = EditorUtility.Inspector.DetailTreeSelection;

        Visible = objs.Any() && objs.All(o => o is StructField);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
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

        string name = doc.TypeEditItems.GetSuggestedName("Struct");

        name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Enter new struct name", name, s =>
        {
            if (!NamingVerifier.VerifyIdentifier(s))
            {
                DialogUtility.ShowMessageBoxAsyncL("Invalid name");
                return false;
            }
            if (doc.TypeEditItems.ContainsItem(s, true))
            {
                DialogUtility.ShowMessageBoxAsyncL("Name already exists");
                return false;
            };

            return true;
        });

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var action = new ExtractStructAction(doc, fields, name);
        doc.View?.DoServiceAction<UndoRedoManager>(m => m.Do(action));
    }
}

/// <summary>
/// Undo/redo action for extracting struct fields into a new struct type.
/// </summary>
internal class ExtractStructAction : BaseRefactorAction
{
    /// <inheritdoc/>
    public override string Name => L("Extract Struct");

    private readonly TypeDesignDocument _doc;
    private readonly StructType _struct;

    private readonly List<StructField> _fields;
    private readonly List<int> _fieldIndexes;
    private List<Guid> _fieldOldIds;
    private List<Guid> _fieldNewIds;

    private readonly string _name;

    private StructType _newStruct;
    private StructField _newField;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractStructAction"/> class.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="fields">The fields to extract.</param>
    /// <param name="name">The name for the new struct.</param>
    public ExtractStructAction(TypeDesignDocument doc, IEnumerable<StructField> fields, string name)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _fields = [.. fields.OrderBy(o => o.List.IndexOf(o))];
        if (_fields.Count == 0)
        {
            throw new InvalidOperationException("Field count == 0");
        }

        _fieldIndexes = [.. _fields.Select(o => o.List.IndexOf(o))];
        _fieldOldIds = [.. _fields.Select(o => o.Id)];

        _name = name;

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

        // Create extracted struct
        _newStruct ??= new StructType { Name = _name };

        // Create extracted field
        if (_newField is null)
        {
            string fieldName = fieldList.GetSuggestedFieldName(_name + "Extracted");
            _newField = new StructField { Name = fieldName };
        }

        // Transfer fields
        foreach (StructField field in _fields)
        {
            field.List?.Remove(field);
            _newStruct.FieldList.Add(field);
        }

        // Add new struct
        _doc.TypeEditItems.AddItem(_newStruct);
        // Get Id in the new struct
        _fieldNewIds = [.. _fields.Select(o => o.Id)];

        // Add extracted field
        int index = _fieldIndexes[0];
        fieldList.Insert(index, _newField);
        fieldList.ArrangeItem();

        // Set field type to new struct
        _newField.FieldType.FieldType = TypeDefinition.Resolve(_newStruct.Id);

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

            if (obj.GetItem(_newField.Id) is not SObject newObj)
            {
                var type = TypeDefinition.Resolve(_newStruct.Id);
                newObj = new SObject(type);
                obj.SetProperty(_newField.Name, newObj);
            }

            for (int i = 0; i < _fields.Count; i++)
            {
                var f = _fields[i];
                newObj.SetProperty(f.Name, items[i]);
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
        Guid newFieldId = _newField.Id;

        // Remove new field
        fieldList.Remove(_newField);

        // Remove new struct
        _doc.TypeEditItems.RemoveItem(_newStruct);

        // Transfer fields
        for (int i = 0; i < _fields.Count; i++)
        {
            var field = _fields[i];
            var index = _fieldIndexes[i];

            _newStruct.FieldList.Remove(field);
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
            if (obj.GetItem(newFieldId) is not SObject newObj)
            {
                continue;
            }

            List<SItem> items = [.. _fieldNewIds.Select(newObj.GetItem)];
            foreach (var item in items.SkipNull())
            {
                newObj.RemoveItem(item);
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
}
