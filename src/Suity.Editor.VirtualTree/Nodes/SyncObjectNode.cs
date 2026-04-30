using Suity.Editor.Transferring;
using Suity.Editor.VirtualTree.Actions;
using Suity.Json;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A virtual tree node that represents a synchronizable object.
/// Implements view object setup, value setting, and advanced editing capabilities
/// for objects that support synchronization via <see cref="ISyncObject"/>.
/// </summary>
public class SyncObjectNode : BaseObjectNode,
    IViewObjectSetup,
    IViewSetValue,
    IViewAdvancedEdit
{
    private IViewObject _obj;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncObjectNode"/> class.
    /// </summary>
    public SyncObjectNode()
    {
    }

    /// <inheritdoc/>
    public override object DisplayedValue => _obj;

    /// <inheritdoc/>
    protected override void OnSetupObjectNode()
    {
        base.OnSetupObjectNode();

        _obj = GetValue() as IViewObject;
        _obj?.SetupView(this);
        //((IVirtualTreeObject)GetValue()).SetupVirtualTree(this);
    }

    /// <summary>
    /// Synchronizes the displayed object using the specified property sync mechanism.
    /// </summary>
    /// <param name="sync">The property sync instance to use for synchronization.</param>
    protected internal virtual void OnSyncEditor(IPropertySync sync)
    {
        ISyncObject obj = DisplayedValue as ISyncObject;
        obj?.Sync(sync, this);
    }

    /// <inheritdoc/>
    protected internal override object OnGetProperty(string name)
    {
        SinglePropertySync sync = SinglePropertySync.CreateGetter(name);
        OnSyncEditor(sync);

        return sync.Value;
    }

    /// <inheritdoc/>
    protected internal override void OnSetProperty(string name, object obj)
    {
        SinglePropertySync sync = SinglePropertySync.CreateSetter(name, obj);
        OnSyncEditor(sync);
    }

    /// <inheritdoc/>
    protected internal override void PerformSetValueAction(string name, object value)
    {
        var model = FindModel();

        if (model != null && DisplayedValue is ISyncObject obj)
        {
            model.HandleSetterAction(new SyncObjectSetterAction(model, obj, name, value));
        }
        else
        {
            OnSetProperty(name, value);
        }
    }

    /// <inheritdoc/>
    public override void HandleNodeAction()
    {
        IViewDoubleClickAction action = DisplayedValue as IViewDoubleClickAction;
        action?.DoubleClick();
    }

    #region IViewObjectSetup

    /// <inheritdoc/>
    bool IViewObjectSetup.IsTypeSupported(Type type)
    {
        return type.IsAssignableFrom(EditedType);
    }

    /// <inheritdoc/>
    void IViewObjectSetup.AddField(Type type, ViewProperty property)
    {
        base.FieldOfType(type, property);
    }

    /// <inheritdoc/>
    IEnumerable<object> IViewObjectSetup.GetObjects() => [GetValue()];

    #endregion

    #region IViewAction

    /// <inheritdoc/>
    void IViewSetValue.SetValue(string name, object value)
    {
        PerformSetValueAction(name, value);
    }

    #endregion

    #region IViewAdvancedEdit

    /// <inheritdoc/>
    object IViewAdvancedEdit.FieldNavigationTarget => throw new NotImplementedException();

    /// <inheritdoc/>
    bool IViewAdvancedEdit.GetHasFeature(ViewAdvancedEditFeatures feature) => feature switch
    {
        ViewAdvancedEditFeatures.Json => ContentTransfer<DataRW>.GetTransfer(DisplayedValue?.GetType()) != null,
        _ => false,
    };

    /// <inheritdoc/>
    void IViewAdvancedEdit.Repair()
    {
    }

    /// <inheritdoc/>
    void IViewAdvancedEdit.Relocate()
    {
    }

    /// <inheritdoc/>
    void IViewAdvancedEdit.SetDynamicAction(Type type)
    {
    }

    /// <inheritdoc/>
    string IViewAdvancedEdit.GetText(ViewAdvancedEditFeatures feature)
    {
        switch (feature)
        {
            case ViewAdvancedEditFeatures.Json:
                {
                    if (ContentTransfer<DataRW>.GetTransfer(DisplayedValue?.GetType()) is { } dw)
                    {
                        try
                        {
                            var writer = new JsonDataWriter();
                            writer.Node("@format").WriteString("SuityJson");
                            dw.Output(DisplayedValue, new DataRW { Writer = writer });

                            return writer.ToString(true);
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    }
                }
                break;
        }

        return null;
    }

    /// <inheritdoc/>
    void IViewAdvancedEdit.SetText(ViewAdvancedEditFeatures feature, string text)
    {
        var model = FindModel();
        if (model is null)
        {
            return;
        }

        switch (feature)
        {
            case ViewAdvancedEditFeatures.Json:
                {
                    if (ContentTransfer<DataRW>.GetTransfer(DisplayedValue?.GetType()) is { })
                    {
                        try
                        {
                            //var reader = new JsonDataReader(text);
                            //readable.ReadData(reader);
                            var action = new ObjectJsonSetterAction(model, DisplayedValue, text);
                            model.HandleSetterAction(action);
                        }
                        catch (Exception err)
                        {
                            err.LogError();
                        }
                    }
                }
                break;
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _obj = null;
    }
}
