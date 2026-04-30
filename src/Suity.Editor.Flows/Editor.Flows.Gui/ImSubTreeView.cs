using Suity.Editor.Documents;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Gui.TreeGui;
using Suity.Editor.Services;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// A tree-based expanded view for flow nodes, supporting sub-document navigation and inspection.
/// Used as the default expanded view for <see cref="SNamedDocument"/> and <see cref="DesignItem"/>.
/// </summary>
[DefaultFlowExpandedView]
[FlowExpandedViewUsage(typeof(SNamedDocument))]
[FlowExpandedViewUsage(typeof(DesignItem))]
public class ImSubTreeView : TreeImGui, IDrawExpandedImGui
{
    /// <inheritdoc/>
    protected override void OnTargetUpdated()
    {
        if (Target is Document document)
        {
            RestoreDocumentViewState(document);
        }
    }

    /// <summary>
    /// Gets or sets the inspector context for this view.
    /// </summary>
    public IInspectorContext InspectorContext { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImSubTreeView"/> class with headerless options.
    /// </summary>
    /// <param name="option">The headerless tree options.</param>
    public ImSubTreeView(HeaderlessTreeOptions option)
        : base(option)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImSubTreeView"/> class with column tree options.
    /// </summary>
    /// <param name="option">The column tree options.</param>
    public ImSubTreeView(ColumnTreeOptions option)
        : base(option)
    {
    }


    #region IDrawExpandedImGui

    /// <inheritdoc/>
    public bool ResizableOnExpand => true;

    /// <inheritdoc/>
    public float? ContentScale => null;

    /// <inheritdoc/>
    public void EnterExpandedView(object target, IInspectorContext context)
    {
        if (target is FlowNode flowNode)
        {
            target = flowNode.ExpandedViewObject;
        }

        if (target is SNamedItem)
        {
            ViewId = ViewIds.DetailTreeView;
        }
        else
        {
            ViewId = ViewIds.MainTreeView;
        }

        Target = target;
        InspectorContext = context;
        TreeView.SenderTarget = context;

        if (target is Document document)
        {
            string formatName = document.Entry?.Format?.FormatName;
            if (!string.IsNullOrEmpty(formatName))
            {
                CreateMenu("#" + formatName);
            }
            else
            {
                CreateMenu(":ExpandedTree");
            }

            RestoreDocumentViewState(document);
        }
        else
        {
            ExpandAll();
        }

        RequestAnalyze();
        UpdateAnalysis();
    }

    /// <inheritdoc/>
    public void ExitExpandedView()
    {
        if (Target is Document document)
        {
            SaveDocumentViewState(document);
        }
    }

    /// <inheritdoc/>
    public void UpdateExpandedTarget()
    {
        TreeView.VirtualModel.UpdateDisplayedObject();
    }


    /// <inheritdoc/>
    public ImGuiNode OnExpandedGui(ImGui gui)
    {
        return base.OnNodeGui(gui)
            .InitHeightRest(5);
    }

    /// <inheritdoc/>
    public void ClearSelection()
    {
        base.SetSelection(ViewSelection.Empty);
    }

    #endregion

    /// <inheritdoc/>
    public override ImGuiNode OnNodeGui(ImGui gui)
    {
        return base.OnNodeGui(gui)
            .InitHeightRest(5);
    }

    #region IInspectorContext

    /// <inheritdoc/>
    public override void InspectorEnter() => InspectorContext?.InspectorEnter();

    /// <inheritdoc/>
    public override void InspectorExit() => InspectorContext?.InspectorExit();

    /// <inheritdoc/>
    public override void InspectorBeginMacro(string name) => InspectorContext?.InspectorBeginMacro(name);

    /// <inheritdoc/>
    public override void InspectorEndMarco(string name) => InspectorContext?.InspectorEndMarco(name);

    /// <inheritdoc/>
    public override bool InspectorDoAction(UndoRedoAction action) => InspectorContext?.InspectorDoAction(action) ?? base.InspectorDoAction(action);

    /// <inheritdoc/>
    public override void InspectorEditFinish()
    {
        InspectorContext?.InspectorEditFinish();

        base.InspectorEditFinish();
    }

    /// <inheritdoc/>
    public override void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        InspectorContext?.InspectorObjectEdited(objs, propertyName);

        base.InspectorObjectEdited(objs, propertyName);
    }

    /// <inheritdoc/>
    public override object InspectorUserData
    {
        get => InspectorContext?.InspectorUserData;
        set
        {
            if (InspectorContext is { } context)
            {
                context.InspectorUserData = value;
            }
        }
    }

    #endregion

    #region Tree Context
    /// <inheritdoc/>
    protected override void OnTreeBeginMacro(string name) => InspectorContext?.InspectorBeginMacro(name);

    /// <inheritdoc/>
    protected override void OnTreeEndMacro(string name) => InspectorContext?.InspectorEndMarco(name);

    /// <inheritdoc/>
    protected override bool OnTreeDoAction(UndoRedoAction action) => InspectorContext?.InspectorDoAction(action) ?? base.OnTreeDoAction(action);

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void OnTreeEditFinish() => InspectorContext?.InspectorEditFinish();

    #endregion

    #region ViewState
    /// <summary>
    /// Restores the saved GUI state for the specified document.
    /// </summary>
    /// <param name="document">The document to restore state for.</param>
    public void RestoreDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            object config = EditorServices.PluginService.GetPlugin<GuiStatePlugin>().GetGuiState<object>(asset);

            RestoreViewState(config);
        }
    }

    /// <inheritdoc/>
    public void SaveDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            var config = SaveViewState();
            if (config != null)
            {
                EditorServices.PluginService.GetPlugin<GuiStatePlugin>().SetGuiState<object>(asset, config);
            }
        }
    }
    #endregion

}
