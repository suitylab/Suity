using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a design field in the editor.
/// </summary>
public class DesignField : SNamedField,
    IDesignObject,
    IHasAttributeDesign,
    IViewEditNotify,
    IViewColor,
    IDrawEditorImGui,
    IHasToolTips
{
    private readonly SArrayAttributeDesign _attributes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignField"/> class.
    /// </summary>
    public DesignField()
    { }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Attributes", _attributes.Array, SyncFlag.GetOnly);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportExtended())
        {
            setup.ExtendedField(_attributes.Array, new ViewProperty("Attributes", "Property"));
        }
    }

    protected override TextStatus OnGetTextStatus()
    {
        var status = base.OnGetTextStatus();
        if (status != TextStatus.Normal)
        {
            return status;
        }

        if (_attributes.GetIsHiddenOrDisabled())
        {
            return TextStatus.Disabled;
        }

        return TextStatus.Normal;
    }

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    #region IAttributeDesign

    public IAttributeDesign Attributes => _attributes;


    #endregion

    #region IViewDesignObject

    SArray IDesignObject.DesignItems => _attributes.Array;
    string IDesignObject.DesignPropertyName => "Attributes";
    string IDesignObject.DesignPropertyDescription => "Property";

    #endregion

    #region IViewHandleEdit

    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        if (obj is SItem item && item.Root == _attributes.Array)
        {
            NotifyFieldUpdated(true);
        }
        else if (obj is IDesignValue design && design.Value.Root == _attributes.Array)
        {
            NotifyFieldUpdated(true);
        }

        OnNotifyViewEdited(obj, propertyName);
    }

    protected virtual void OnNotifyViewEdited(object obj, string propertyName)
    {
    }

    #endregion

    #region IViewColor

    Color? IViewColor.ViewColor => _attributes.GetAttributes<IViewColor>()?.FirstOrDefault()?.ViewColor;

    #endregion

    #region IDrawEditorImGUi

    bool IDrawEditorImGui.OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
            //string p = GetPreviewText();
            //if (!string.IsNullOrWhiteSpace(p))
            //{
            //    var node = gui.HorizontalFrame("preview")
            //    .InitClass("refBox")
            //    .InitFitBoth()
            //    .SetColor(EditorManagement.ColorConfig.GetStatusColor(TextStatus.Preview))
            //    .InitOverridePadding(0, 0, 5, 5)
            //    .OnContent(() =>
            //    {
            //        gui.Text("text", p).InitClass("numBoxText").SetFontColor(Color.Black);
            //    });
            //}

            try
            {
                OnDrawPreviewImGui(gui);
            }
            catch (Exception err)
            {
                err.LogError();
            }

            int num = 0;
            foreach (var item in _attributes.Array.Items.OfType<SItem>())
            {
                if (item.GetIcon() is { } icon)
                {
                    gui.Image($"#icon{num}", icon)
                    .InitClass("icon")
                    .SetToolTipsL(AssetManager.Instance.GetAsset(item.TypeId)?.ToDisplayText());

                    num++;
                }
            }

            EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
            return true;
        }

        EditorServices.ImGuiService.DrawItem(gui, this, pipeline, context);
        return false;
    }

    protected virtual void OnDrawPreviewImGui(ImGui gui)
    { }

    #endregion

    #region IHasToolTips

    string IHasToolTips.ToolTips => this.Attributes.GetAttribute<ToolTipsAttribute>()?.ToolTips;


    #endregion

}