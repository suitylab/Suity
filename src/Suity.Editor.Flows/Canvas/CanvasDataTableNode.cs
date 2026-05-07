using Suity.Editor.Flows;
using Suity.Editor.Flows.Gui;
using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Documents.Canvas;

// Temporarily removed inheritance from CanvasSwitchableNode
/// <summary>
/// Represents a canvas node that displays and manages data table assets in a tree view.
/// </summary>
public class CanvasDataTableNode : CanvasAssetNode<DataTableAsset>, 
    IDrawExpandedImGui,
    IViewSelectable,
    IViewSelectionInfo
{
    private ImSubTreeView _treeView;

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasDataTableNode"/> class.
    /// </summary>
    public CanvasDataTableNode()
    {
    }

/*
    protected override object GetTransportObject() => _treeView?.SelectedObjects.FirstOrDefault();
*/

    #region IDrawExpandedImGui

    /// <summary>
    /// Gets a value indicating whether the node is resizable when expanded.
    /// </summary>
    bool IDrawExpandedImGui.ResizableOnExpand => true;

    /// <summary>
    /// Gets the content scale for the expanded view. Returns null for default scaling.
    /// </summary>
    float? IDrawExpandedImGui.ContentScale => null;

    /// <summary>
    /// Enters the expanded view mode and initializes the tree view for displaying data.
    /// </summary>
    /// <param name="target">The target object to display.</param>
    /// <param name="context">The inspector context.</param>
    void IDrawExpandedImGui.EnterExpandedView(object target, IInspectorContext context)
    {
        if (_treeView is null)
        {
            _treeView = new(new ColumnTreeOptions());
            _treeView.SelectionChanged += TreeView_SelectionChanged;
        }

        _treeView.EnterExpandedView(target, context);
    }

    /// <summary>
    /// Exits the expanded view mode and cleans up the tree view resources.
    /// </summary>
    void IDrawExpandedImGui.ExitExpandedView()
    {
        if (_treeView is not null)
        {
            _treeView.SelectionChanged -= TreeView_SelectionChanged;

            _treeView.ExitExpandedView();
            _treeView = null;
/*
            DataTransportConnector.UpdateDescription(DEFAULT_TRANSPORT_TITLE);
*/
        }
    }

    /// <summary>
    /// Updates the target object in the expanded view.
    /// </summary>
    void IDrawExpandedImGui.UpdateExpandedTarget()
    {
        _treeView?.UpdateExpandedTarget();
    }

    /// <summary>
    /// Renders the expanded view GUI and returns the ImGui node.
    /// </summary>
    /// <param name="gui">The ImGui instance to render with.</param>
    /// <returns>The rendered ImGui node.</returns>
    ImGuiNode IDrawExpandedImGui.OnExpandedGui(ImGui gui)
    {
        return _treeView?.OnExpandedGui(gui);
    }

    /// <summary>
    /// Clears the current selection in the expanded tree view.
    /// </summary>
    void IDrawExpandedImGui.ClearSelection()
    {
        _treeView?.ClearSelection();
    }

    private void TreeView_SelectionChanged(object sender, EventArgs e)
    {
/*
        var obj = _treeView.SelectedObjects.FirstOrDefault();

        if (obj is DataRow dataRow)
        {
            DataTransportConnector.UpdateDescription(dataRow?.ToDisplayText());
        }
        else
        {
            DataTransportConnector.UpdateDescription(DEFAULT_TRANSPORT_TITLE);
        }
*/
    }

    #endregion

    #region IViewSelectable

    /// <summary>
    /// Gets the current view selection from the tree view.
    /// </summary>
    public ViewSelection GetSelection() => _treeView?.GetSelection();

    /// <summary>
    /// Sets the view selection in the tree view.
    /// </summary>
    /// <param name="selection">The selection to set.</param>
    /// <returns>True if the selection was set successfully; otherwise, false.</returns>
    public bool SetSelection(ViewSelection selection) => _treeView?.SetSelection(selection) ?? false;

    #endregion

    #region IViewSelectionInfo

    /// <summary>
    /// Gets the currently selected objects in the tree view.
    /// </summary>
    public IEnumerable<object> SelectedObjects => _treeView?.SelectedObjects ?? [];

    /// <summary>
    /// Finds selection items or their parents of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to search for.</typeparam>
    /// <param name="distinct">If true, returns only distinct results.</param>
    /// <returns>An enumerable collection of matching items.</returns>
    public IEnumerable<T> FindSelectionOrParent<T>(bool distinct = true) where T : class => _treeView?.FindSelectionOrParent<T>(distinct) ?? [];

    #endregion
}