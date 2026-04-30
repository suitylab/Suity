using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;

namespace Suity.Editor.VirtualTree.Actions;

/// <summary>
/// Represents an undoable action that sets a property on an <see cref="ISyncObject"/>.
/// </summary>
internal class SyncObjectSetterAction : VirtualNodeSetterAction
{
    private readonly ISyncObject _obj;
    private readonly string _name;
    private readonly object _oldValue;
    private readonly object _newValue;

    /// <inheritdoc/>
    public override string Name => _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncObjectSetterAction"/> class.
    /// </summary>
    /// <param name="model">The virtual tree model this action operates on.</param>
    /// <param name="obj">The sync object whose property will be modified.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to assign to the property.</param>
    public SyncObjectSetterAction(VirtualTreeModel model, ISyncObject obj, string name, object value)
        : base(model, obj, name)
    {
        _obj = obj;
        _name = name;
        _newValue = value;

        _oldValue = obj.GetProperty(name);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        _obj.SetProperty(_name, _newValue, this);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _obj.SetProperty(_name, _oldValue, this);

        base.Undo();
    }
}

/// <summary>
/// Represents an undoable action that sets a property on an <see cref="SObject"/> using synchronization.
/// </summary>
internal class SObjectSetterAction : VirtualNodeSetterAction
{
    private readonly SObject _obj;
    private readonly string _name;
    private readonly object _oldValue;
    private readonly object _newValue;

    /// <inheritdoc/>
    public override string Name => _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="SObjectSetterAction"/> class.
    /// </summary>
    /// <param name="model">The virtual tree model this action operates on.</param>
    /// <param name="obj">The SObject whose property will be modified.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to assign to the property.</param>
    public SObjectSetterAction(VirtualTreeModel model, SObject obj, string name, object value)
        : base(model, obj, name)
    {
        _obj = obj;
        _name = name;
        _newValue = value;

        _oldValue = GetProperty(name);
    }

    /// <inheritdoc/>
    public override void Do()
    {
        SetProperty(_name, _newValue);

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        SetProperty(_name, _oldValue);

        base.Undo();
    }

    private object GetProperty(string name)
    {
        SinglePropertySync sync = SinglePropertySync.CreateGetter(name);
        OnSyncEditor(sync);

        return sync.Value;
    }

    private void SetProperty(string name, object obj)
    {
        SinglePropertySync sync = SinglePropertySync.CreateSetter(name, obj);
        OnSyncEditor(sync);
    }

    private void OnSyncEditor(IPropertySync sync)
    {
        if (_obj.Controller is IViewObject viewObj)
        {
            viewObj.Sync(sync, this);
        }
        else
        {
            //DBaseStruct structType = _obj.GetStruct();

            object value = _obj[sync.Name];

            if (sync.Mode == SyncMode.Get)
            {
                sync.Sync(sync.Name, value);
            }
            else if (sync.Mode == SyncMode.Set)
            {
                object newValue = sync.Sync(sync.Name, value);
                _obj[sync.Name] = newValue;
            }
        }
    }
}
