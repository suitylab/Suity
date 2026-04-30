using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Documents;
using Suity.Editor.Transferring;
using Suity.Json;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Represents an undo/redo action for AI assistant operations on document objects.
/// </summary>
internal class AssistantAction : UndoRedoAction
{
    private readonly IDocumentView _docView;
    private readonly object _target;
    private readonly IDataReader _reader;

    private DataRW _undoData;
    private List<object> _newObjects;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistantAction"/> class.
    /// </summary>
    /// <param name="docView">The document view to refresh after the action.</param>
    /// <param name="target">The target object to apply the action to.</param>
    /// <param name="reader">The data reader containing the new data to apply.</param>
    public AssistantAction(IDocumentView docView, object target, IDataReader reader)
    {
        _docView = docView ?? throw new ArgumentNullException(nameof(docView));
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    /// <inheritdoc/>
    public override string Name => L("AI Assistant: ") + _target.ToDisplayText();

    /// <inheritdoc/>
    public override void Do()
    {
        var transfer = DataRW.GetTransfer(_target.GetType());
        if (transfer is null)
        {
            Logs.LogError(L("Object does not support data transfer operation: ") + _target.ToDisplayText());
            return;
        }

        // Execute Preload, get selected objects and created objects
        var preloadRW = new DataRW { Reader = _reader };
        List<object> affactedObjs = [];
        transfer.PreInput(_target, preloadRW, affactedObjs);

        // Record newly created objects. Every time Do is executed, record newly created objects because each Do execution may create new objects.
        _newObjects = preloadRW.NewObjects;

        if (_undoData is null)
        {
            var writer = new JsonDataWriter();
            _undoData = new DataRW { Writer = writer };
            // Record current object state, only record selected objects
            transfer.Output(_target, _undoData, affactedObjs);

            // Create reader for undo
            _undoData.Reader = new JsonDataReader(writer.Value);
        }

        transfer.Input(_target, new DataRW { Reader = _reader }, true);

        _docView.RefreshView();
        EditorUtility.Inspector.UpdateInspector();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        var transfer = DataRW.GetTransfer(_target.GetType());
        if (transfer is null)
        {
            Logs.LogError(L("Object does not support data transfer operation: ") + _target.ToDisplayText());
            return;
        }

        if (_undoData != null)
        {
            transfer.Input(_target, _undoData, true);
        }

        if (_newObjects?.Count > 0)
        {
            transfer.Delete(_target, new DataRW { }, _newObjects);
        }

        _docView.RefreshView();
        EditorUtility.Inspector.UpdateInspector();
    }
}