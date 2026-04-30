using Suity.Editor.Services;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// A sub-flow view that renders a flow diagram within an expanded node area.
/// Supports sub-document navigation, undo/redo, and menu commands.
/// </summary>
[FlowExpandedViewUsage(typeof(FlowDocument))]
public class ImSubFlowView : FlowViewImGui,
    IDrawExpandedImGui, 
    IMenuSenderContext,
    IViewUndo
{
    private readonly ImGuiNodeRef _node = new();

    private FlowNode _parentFlowNode;

    private IInspectorContext _inspectorContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImSubFlowView"/> class.
    /// </summary>
    public ImSubFlowView()
    {
        //base.GraphPanel.ShowGrid = false;
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
            _parentFlowNode = flowNode;

            target = flowNode.ExpandedViewObject;
        }

        if (target is FlowDocument doc)
        {
            this.Document = doc;
        }

        _inspectorContext = context;

        QueuedAction.Do(() =>
        {
            if (_node.Gui is { } gui)
            {
                RestoreViewState(gui);
            }
        });
    }

    /// <inheritdoc/>
    public void ExitExpandedView()
    {
        if (_node.Gui is { } gui)
        {
            SaveViewState(gui);
        }

        // Need to set base class Document=null to execute StopView
        this.Document = null;
    }

    /// <inheritdoc/>
    public void UpdateExpandedTarget()
    {
    }

    /// <inheritdoc/>
    public ImGuiNode OnExpandedGui(ImGui gui)
    {
        if (GraphPanel.GraphicContext != gui.Context)
        {
            GraphPanel.GraphicContext = gui.Context;
        }

        var node = _node.Node = gui.VerticalLayout()
        .InitTheme(NodeGraphTheme.Default)
        .InitValue<IFlowView>(this)
        .InitFullWidth()
        .InitHeightRest(5)
        .InitPadding(4)
        .OnContent(() =>
        {
            gui.VerticalLayout()
            .InitSizeRest()
            .OnContent(() => 
            {
                GraphPanel.OnNodeGui(gui);
            });
        });

        //node.QueueRefresh();

        return node;
    }

    /// <inheritdoc/>
    public void ClearSelection()
    {
        base.SetSelection(ViewSelection.Empty);
    }

    #endregion

    /// <inheritdoc/>
    public override void RefreshView()
    {
        // Notify parent node to refresh view.
        // If this step is missing, the child view's OnGui operation will not execute
        _parentFlowNode?.QueueRefreshView();

        base.RefreshView();
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (base.GetService(serviceType) is { } o)
        {
            return o;
        }

        return _inspectorContext?.GetService(serviceType);
    }

    #region IInspectorContext

    /// <inheritdoc/>
    public override void InspectorEnter() => _inspectorContext?.InspectorEnter();

    /// <inheritdoc/>
    public override void InspectorExit() => _inspectorContext?.InspectorExit();

    /// <inheritdoc/>
    public override void InspectorBeginMacro(string name) => _inspectorContext?.InspectorBeginMacro(name);

    /// <inheritdoc/>
    public override void InspectorEndMarco(string name) => _inspectorContext?.InspectorEndMarco(name);

    /// <inheritdoc/>
    public override bool InspectorDoAction(UndoRedoAction action) => _inspectorContext?.InspectorDoAction(action) ?? base.InspectorDoAction(action);

    /// <inheritdoc/>
    public override void InspectorEditFinish()
    {
        _inspectorContext?.InspectorEditFinish();

        base.InspectorEditFinish();
    }

    /// <inheritdoc/>
    public override void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        _inspectorContext?.InspectorObjectEdited(objs, propertyName);

        base.InspectorObjectEdited(objs, propertyName);
    }

    /// <inheritdoc/>
    public override object InspectorUserData
    {
        get => _inspectorContext?.InspectorUserData;
        set
        {
            if (_inspectorContext is { } context)
            {
                context.InspectorUserData = value;
            }
        }
    }

    #endregion

    #region IMenuSenderContext
    /// <inheritdoc/>
    object IMenuSenderContext.SenderTarget => _inspectorContext;

    #endregion

    #region Flow Context
    /// <inheritdoc/>
    protected override void OnFlowBeginMacro(string name) => _inspectorContext?.InspectorBeginMacro(name);

    /// <inheritdoc/>
    protected override void OnFlowEndMacro(string name) => _inspectorContext?.InspectorEndMarco(name);

    /// <inheritdoc/>
    protected override bool OnFlowDoAction(UndoRedoAction action) => _inspectorContext?.InspectorDoAction(action) ?? base.OnFlowDoAction(action);

    /// <inheritdoc/>
    protected override void OnFlowEditFinish() => _inspectorContext?.InspectorEditFinish();

    #endregion

    #region IViewUndo

    /// <inheritdoc/>
    public bool CanUndo => _inspectorContext?.GetService<UndoRedoManager>()?.CanUndo ?? false;

    /// <inheritdoc/>
    public bool CanRedo => _inspectorContext?.GetService<UndoRedoManager>()?.CanRedo ?? false;

    /// <inheritdoc/>
    public string UndoText => _inspectorContext?.GetService<UndoRedoManager>()?.PrevActionInfo?.Name;

    /// <inheritdoc/>
    public string RedoText => _inspectorContext?.GetService<UndoRedoManager>()?.NextActionInfo?.Name;

    /// <inheritdoc/>
    public void Undo() => _inspectorContext?.GetService<UndoRedoManager>()?.Undo();

    /// <inheritdoc/>
    public void Redo() => _inspectorContext?.GetService<UndoRedoManager>()?.Redo();

    #endregion
}