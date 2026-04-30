using ComputerBeacon.Json;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.VirtualTree.Actions;
using Suity.Editor.VirtualTree.Nodes;
using Suity.NodeQuery;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.VirtualTree.SValues;

/// <summary>
/// Virtual tree node that represents an <see cref="SObject"/> value,
/// providing property display, editing, and advanced operations such as XML/JSON serialization.
/// </summary>
[VirtualNodeUsage(typeof(SObject))]
public class SObjectNode : BaseObjectNode<SObject>,
    IViewObjectSetup,
    IViewSetValue,
    IViewAdvancedEdit
{
    private bool _isDynamic;
    private SObject _firstObj;

    /// <summary>
    /// Initializes a new instance of the <see cref="SObjectNode"/> class.
    /// </summary>
    public SObjectNode()
    { }

    /// <inheritdoc/>
    public override object DisplayedValue => _firstObj;

    /// <inheritdoc/>
    protected override void OnSetupObjectNode()
    {
        base.OnSetupObjectNode();

        _firstObj = GetObject();
        if (_firstObj is null)
        {
            return;
        }

        if (_firstObj.InputType.IsAbstract)
        {
            _isDynamic = true;
        }

        DCompond structType = _firstObj.GetStruct(_firstObj.GetAssetFilter());
        if (structType != null)
        {
            if (_firstObj.Controller != null)
            {
                if (_firstObj.Controller is IViewObject viewObj)
                {
                    viewObj.SetupView(this);
                }
                else
                {
                    // If the controller doesn't support IVirtualTreeObject, it should not be displayed
                }
            }
            else
            {
                foreach (DStructField field in structType.PublicStructFields)
                {
                    if (!IsFieldVisible(field))
                    {
                        continue;
                    }

                    // Ignore fields that don't expand to node view
                    if (!field.ShowInDetail)
                    {
                        continue;
                    }

                    if (field.FieldType == NativeTypes.DelegateType)
                    {
                        continue;
                    }

                    FieldOfType(field.FieldType.GetEditedType(), new ViewProperty(field.Name, field.Description));
                }
            }
        }
        else
        {
            if (_firstObj.Controller is IViewObject viewObj)
            {
                viewObj.SetupView(this);
            }
            else
            {
                foreach (string name in _firstObj.GetPropertyNames())
                {
                    object value = _firstObj[name];
                    if (value != null)
                    {
                        if ((value as SObject)?.InputType == NativeTypes.DelegateType)
                        {
                            continue;
                        }

                        FieldOfType(value.GetType(), new ViewProperty(name));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Synchronizes a property value with the editor through the provided <see cref="IPropertySync"/>.
    /// Delegates to the controller's <see cref="IViewObject"/> if available; otherwise performs
    /// direct get/set on the underlying <see cref="SObject"/>.
    /// </summary>
    /// <param name="sync">The property synchronization context containing name, mode, and value.</param>
    protected internal virtual void OnSyncEditor(IPropertySync sync)
    {
        SObject obj = DisplayedObject;
        if (obj is null)
        {
            return;
        }

        if (obj.Controller is IViewObject viewObj)
        {
            viewObj.Sync(sync, this);
        }
        else
        {
            //var structType = obj.GetStruct(obj.GetAssetFilter());

            object value = obj[sync.Name];

            if (sync.Mode == SyncMode.Get)
            {
                sync.Sync(sync.Name, value);
            }
            else if (sync.Mode == SyncMode.Set)
            {
                object newValue = sync.Sync(sync.Name, value);
                obj[sync.Name] = newValue;
            }
        }
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
        var obj = DisplayedObject;
        if (model != null && obj != null)
        {
            model.HandleSetterAction(new SObjectSetterAction(model, obj, name, value));
        }
        else
        {
            OnSetProperty(name, value);
        }
    }

    /// <inheritdoc/>
    public override async void HandleNodeAction()
    {
        if (!_isDynamic) return;

        var obj = DisplayedObject;
        if (obj is null)
        {
            return;
        }
        // If the parent is an array, no dynamic change is needed
        // TODO: How to fix a wrong reference while preserving the internal structure?
        if (obj.Parent is SArray)
        {
            return;
        }

        if (obj.InputType.IsAbstract)
        {
            ISelectionList selList = obj.GetSelectionList();
            if (selList != null)
            {
                var result = await selList.ShowSelectionGUIAsync(string.Empty, new SelectionOption { SelectedKey = obj.ObjectType.TypeCode });
                if (result.IsSuccess && result.SelectedKey != obj.ObjectType.TypeCode)
                {
                    if (AssetManager.Instance.GetAsset(result.SelectedKey, obj.GetAssetFilter()) is DCompond s)
                    {
                        var newObjRef = s.CreateObject(obj.InputType);
                        SetValue(newObjRef);
                    }
                    else
                    {
                        var newObjRef = new SObject(obj.InputType, null);
                        SetValue(newObjRef);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines whether the specified field should be visible in the property tree.
    /// </summary>
    /// <param name="field">The field to evaluate.</param>
    /// <returns><c>true</c> if the field should be displayed; otherwise, <c>false</c>.</returns>
    protected virtual bool IsFieldVisible(DField field)
    {
        return true;

        DStructField objField = field as DStructField;

        if (objField != null)
        {
            return !objField.IsHidden;
        }
        else
        {
            return false;
        }
    }

    #region Display

    /// <inheritdoc/>
    protected override Image GetMainIcon()
    {
        var obj = DisplayedObject;

        return obj?.ObjectType.GetIcon() ?? obj?.InputType.GetIcon();
    }

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        var obj = DisplayedObject;
        if (obj is null)
        {
            return TextStatus.Error;
        }

        if (obj.IsComment)
        {
            return TextStatus.Comment;
        }

        if (obj.GetStruct(obj.GetAssetFilter()) is null)
        {
            return TextStatus.Error;
        }

        var status = base.GetTextStatus();
        if (status != TextStatus.Normal)
        {
            return status;
        }

        if (obj.Controller is ITextDisplay textDisplay)
        {
            // Moved to GetPreviewTextStatus
            if (textDisplay.DisplayStatus >= TextStatus.Info)
            {
                return textDisplay.DisplayStatus;
            }
        }

        return TextStatus.Normal;
    }

    /// <inheritdoc/>
    protected override Color? GetColor()
    {
        var obj = DisplayedObject;
        if (obj is null)
        {
            return null;
        }

        if (obj.Controller is IViewColor vc)
        {
            return vc.ViewColor;
        }

        var type = obj.GetStruct(obj.GetAssetFilter());

        return type?.GetAttribute<IViewColor>()?.ViewColor;
    }

    /// <inheritdoc/>
    protected override string GetPreviewText()
    {
        //string customText = GetCustomPreviewText();
        //if (customText != null)
        //{
        //    return customText;
        //}

        //ISupportAnalysis supportAnalysis = DisplayedValue as ISupportAnalysis;
        //if (supportAnalysis?.AnalysisResult != null && supportAnalysis.AnalysisResult.Status != TextStatus.Normal)
        //{
        //    return supportAnalysis.AnalysisResult.AnalysisText ?? string.Empty;
        //}

        var obj = DisplayedObject;

        return obj?.ToBriefString() ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override TextStatus GetPreviewTextStatus()
    {
        var obj = DisplayedObject;
        if (obj != null)
        {
            if (obj.IsComment)
            {
                return TextStatus.Comment;
            }

            if (obj.Controller is ITextDisplay textDisplay)
            {
                return textDisplay.DisplayStatus;
            }
        }

        return base.GetPreviewTextStatus();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditPreviewText()
    {
        var obj = DisplayedObject;
        if (obj?.Controller is IPreviewEdit previewEdit)
        {
            return previewEdit.CanEditPreviewText;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        var obj = DisplayedObject;

        (obj?.Controller as IPreviewEdit)?.SetPreviewText(value, this);
    }

    /// <inheritdoc/>
    protected override Image GetPreviewIcon()
    {
        var customIcon = GetCustomPreviewIcon();
        if (customIcon != null)
        {
            return customIcon;
        }

        var obj = DisplayedObject;
        if (obj != null)
        {
            if (obj.Controller is IPreviewDisplay previewDisplay)
            {
                return EditorUtility.GetIcon(previewDisplay.PreviewIcon);
            }
            else
            {
                return obj.GetIcon();
            }
        }
        else
        {
            return null;
        }
    }

    #endregion

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
    object ISyncContext.Parent => ParentValue;

    /// <inheritdoc/>
    object IServiceProvider.GetService(Type serviceType)
    {
        return GetService(serviceType);
    }

    /// <inheritdoc/>
    IEnumerable<object> IViewObjectSetup.GetObjects() => [GetValue()];

    #endregion

    #region ISetValueAction

    /// <inheritdoc/>
    void IViewSetValue.SetValue(string name, object value)
    {
        PerformSetValueAction(name, value);
    }

    #endregion

    #region IViewAdvancedEdit

    /// <inheritdoc/>
    object IViewAdvancedEdit.FieldNavigationTarget => this.GetFieldInfomation(GetValue);

    /// <inheritdoc/>
    bool IViewAdvancedEdit.GetHasFeature(ViewAdvancedEditFeatures feature)
    {
        return true;
    }

    /// <inheritdoc/>
    void IViewAdvancedEdit.Repair()
    {
        if (this.ReadOnly) return;
        if (this.Disposed) return;

        var myObj = DisplayedObject;
        if (myObj is null)
        {
            return;
        }

        //DTypeCode inputType = myObj.InputType;
        //AssetFilter filter = myObj.GetAssetFilter();

        var newObj = Cloner.Clone(myObj);
        //myObj.Fix(myObj.GetAssetFilter());
        newObj.RepairDeep();

        this.SetValue(newObj);

        PerformGetValue();
    }

    /// <inheritdoc/>
    async void IViewAdvancedEdit.Relocate()
    {
        if (this.ReadOnly) return;
        if (this.Disposed) return;

        var myObj = DisplayedObject;
        if (myObj is null)
        {
            return;
        }

        //var inputType = myObj.InputType;
        //var filter = myObj.GetAssetFilter();

        var result = await myObj.GetSelectionList().ShowSelectionGUIAsync("Redirect");
        if (result.IsSuccess && !string.IsNullOrEmpty(result.SelectedKey))
        {
            var newObj = Cloner.Clone(myObj);
            newObj.ObjectType = TypeDefinition.Resolve(result.SelectedKey);

            this.SetValue(newObj);

            PerformGetValue();
        }
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
            case ViewAdvancedEditFeatures.XML:
                {
                    var firstObj = DisplayedObject;
                    if (firstObj != null)
                    {
                        var writer = new XmlNodeWriter("SuityFragment");
                        Suity.Synchonizing.Core.Serializer.Serialize(firstObj, writer, null, this, SyncIntent.DataExport);

                        return writer.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }

            case ViewAdvancedEditFeatures.Json:
                {
                    SObject firstObj = DisplayedObject;
                    if (firstObj is null)
                    {
                        return null;
                    }

                    var obj = EditorServices.JsonResource.GetJson(firstObj);

                    return obj switch
                    {
                        JsonObject jobj => jobj.ToString(true),
                        JsonArray jary => jary.ToString(true),
                        _ => null,
                    };
                }

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    void IViewAdvancedEdit.SetText(ViewAdvancedEditFeatures feature, string text)
    {
        if (this.ReadOnly)
        {
            return;
        }

        if (this.Disposed)
        {
            return;
        }

        switch (feature)
        {
            case ViewAdvancedEditFeatures.XML:
                {
                    var reader = XmlNodeReader.FromXml(text);
                    if (!reader.Exist)
                    {
                        DialogUtility.ShowMessageBoxAsync("Incorrect XML format.");
                        return;
                    }
                    if (reader.NodeName != "SuityFragment")
                    {
                        DialogUtility.ShowMessageBoxAsync("Incorrect XML format.");
                        return;
                    }

                    var obj = new SObject();
                    Suity.Synchonizing.Core.Serializer.Deserialize(obj, reader, null, this);

                    SetValue(obj);
                    PerformGetValue();
                }
                break;

            case ViewAdvancedEditFeatures.Json:
                {
                    object o;
                    SItem item;

                    try
                    {
                        o = Parser.Parse(text);
                        item = EditorServices.JsonResource.FromJson(o);
                    }
                    catch (Exception)
                    {
                        DialogUtility.ShowMessageBoxAsync("Incorrect JSON format");
                        return;
                    }

                    if (item is not SObject obj)
                    {
                        DialogUtility.ShowMessageBoxAsync("Incorrect JSON format.");
                        return;
                    }

                    SetValue(obj);
                    PerformGetValue();
                }
                break;
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnDrawAnalysisResult(ImGui gui, AnalysisResult analysis)
    {
        DrawAnalysisNumberBox(gui, analysis, common: false, error: true);
        DrawAnalysisPreview(gui, analysis);
    }

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _firstObj = null;
    }
}
