using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Documents;
using Suity.Editor.Transferring;
using Suity.Json;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using System;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents an undo/redo action that applies JSON data to a document object.
/// </summary>
internal class DocumentJsonSetterAction : UndoRedoAction
{
    private readonly Document _document;
    private readonly object _obj;
    private readonly object _oldObj;
    private readonly IDataReader _reader;

    private object _newObj;

    /// <summary>
    /// Gets the display name of this action.
    /// </summary>
    public override string Name => L("Write Json");

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentJsonSetterAction"/> class with a JSON string.
    /// </summary>
    /// <param name="document">The target document to refresh after applying changes.</param>
    /// <param name="obj">The object whose properties will be updated.</param>
    /// <param name="json">The JSON string containing the new property values.</param>
    public DocumentJsonSetterAction(Document document, object obj, string json)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _obj = obj ?? throw new System.ArgumentNullException(nameof(obj));
        _oldObj = Cloner.Clone(obj);

        try
        {
            _reader = new JsonDataReader(json);
        }
        catch (Exception err)
        {
            err.LogError(L("JSON parse failed"));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentJsonSetterAction"/> class with a data reader.
    /// </summary>
    /// <param name="document">The target document to refresh after applying changes.</param>
    /// <param name="obj">The object whose properties will be updated.</param>
    /// <param name="reader">The data reader providing the new property values.</param>
    public DocumentJsonSetterAction(Document document, object obj, IDataReader reader)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _obj = obj ?? throw new ArgumentNullException(nameof(obj));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        _oldObj = Cloner.Clone(obj);
    }

    /// <summary>
    /// Applies the new data to the target object and refreshes the document view.
    /// </summary>
    public override void Do()
    {
        if (_reader is null)
        {
            return;
        }

        if (_newObj is null)
        {
            try
            {
                ContentTransfer<DataRW>.GetAndInput(_obj, new DataRW { Reader = _reader }, true);
            }
            catch (Exception err)
            {
                err.LogError(L("JSON read failed"));
            }

            _newObj = Cloner.Clone(_obj);
        }
        else
        {
            Cloner.CloneProperty(_newObj, _obj);
        }

        _document.View?.RefreshView();
    }

    /// <summary>
    /// Restores the original object state and refreshes the document view.
    /// </summary>
    public override void Undo()
    {
        if (_reader is null)
        {
            return;
        }

        Cloner.CloneProperty(_oldObj, _obj);

        _document.View?.RefreshView();
    }
}