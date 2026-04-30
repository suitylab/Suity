using Suity.Editor.Transferring;
using Suity.Json;
using Suity.Synchonizing.Core;
using System;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.VirtualTree.Actions;

/// <summary>
/// Represents an undoable action that writes JSON data to an object.
/// Parses the provided JSON string and applies the data to the target object using content transfer.
/// </summary>
internal class ObjectJsonSetterAction : VirtualNodeSetterAction
{
    private readonly object _obj;
    private readonly object _oldObj;
    private object _newObj;

    private readonly JsonDataReader _reader;

    /// <inheritdoc/>
    public override string Name => L("Write Json");

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectJsonSetterAction"/> class.
    /// </summary>
    /// <param name="model">The virtual tree model this action operates on.</param>
    /// <param name="obj">The target object to write JSON data to.</param>
    /// <param name="json">The JSON string to parse and apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
    public ObjectJsonSetterAction(VirtualTreeModel model, object obj, string json)
        : base(model)
    {
        _obj = obj ?? throw new ArgumentNullException(nameof(obj));
        _oldObj = Cloner.Clone(obj);

        try
        {
            _reader = new JsonDataReader(json);
        }
        catch (Exception err)
        {
            err.LogError(L("Json parsing failed"));
        }
    }

    /// <inheritdoc/>
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
                err.LogError(L("Json reading failed"));
            }

            _newObj = Cloner.Clone(_obj);
        }
        else
        {
            // Restore the previously applied new state
            Cloner.CloneProperty(_newObj, _obj);
        }

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        if (_reader is null)
        {
            return;
        }

        Cloner.CloneProperty(_oldObj, _obj);

        base.Undo();
    }
}
