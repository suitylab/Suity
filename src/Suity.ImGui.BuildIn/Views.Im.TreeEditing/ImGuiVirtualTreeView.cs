using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.VirtualTree;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents an abstract base class for ImGui-based virtual tree views, providing selection, clipboard, comment, and preview path functionality.
/// </summary>
public abstract class ImGuiVirtualTreeView : ImGuiTreeView<VirtualNode>,
    IViewSelectionInfo,
    IViewClipboard,
    IViewComment,
    IServiceProvider,
    IDrawContext
{
    // Menu building is slow, so cache it
    readonly static Dictionary<string, VirtualTreeRootMenu> _menus = [];

    private VirtualPath[] _lastSelection = [];

    private VirtualTreeModel? _model;

    protected bool _showDisplayText;
    protected bool _statusIconAtTheEnd;

    public event EventHandler<ValueActionEventArgs>? RequestDoAction;


    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiVirtualTreeView"/> class with default model.
    /// </summary>
    /// <param name="menuName">Optional name for the context menu.</param>
    protected ImGuiVirtualTreeView(string? menuName = null)
        : this(new ImGuiVirtualTreeModel(), menuName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiVirtualTreeView"/> class with a specified model.
    /// </summary>
    /// <param name="model">The tree model to display.</param>
    /// <param name="menuName">Optional name for the context menu.</param>
    protected ImGuiVirtualTreeView(IImGuiTreeModel<VirtualNode> model, string? menuName = null)
        : base(model)
    {
        if (menuName is not null && !string.IsNullOrWhiteSpace(menuName))
        {
            Menu = new VirtualTreeRootMenu(menuName);
        }

        this.MenuSelection = () => SelectedNodes.Select(o => o.DisplayedValue);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show display text instead of node text.
    /// </summary>
    public bool ShowDisplayText
    {
        get => _showDisplayText;
        set => _showDisplayText = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to render the status icon at the end of the row.
    /// </summary>
    public bool StatusIconAtTheEnd
    {
        get => _statusIconAtTheEnd;
        set => _statusIconAtTheEnd = value;
    }

    /// <summary>
    /// Creates and assigns a context menu with the specified name.
    /// </summary>
    /// <param name="menuName">The name of the menu to create.</param>
    public void CreateMenu(string menuName)
    {
        if (string.IsNullOrWhiteSpace(menuName))
        {
            throw new ArgumentNullException(nameof(menuName));
        }

        Menu = _menus.GetOrAdd(menuName, n => new(n));
    }

    /// <summary>
    /// Gets the virtual tree model associated with this view.
    /// </summary>
    public VirtualTreeModel? VirtualModel => _model;
    
    /// <summary>
    /// Gets or sets a value indicating whether selection actions are enabled.
    /// </summary>
    public bool SelectionActionEnabled { get; set; }
    

    /// <summary>
    /// Gets or sets the ID of the view.
    /// </summary>
    public int ViewId
    {
        get => _model?.ViewId ?? 0;
        set
        {
            if (_model is { } model)
            {
                model.ViewId = value;
            }
        }
    }

    /// <inheritdoc/>
    protected internal override void OnTreeModelEnter(IImGuiTreeModel<VirtualNode> model)
    {
        _model = model as VirtualTreeModel;
    }

    /// <inheritdoc/>
    protected internal override GuiInputState? HandleKeyDown(ImGuiNode node, IGraphicInput input)
    {
        if (base.HandleKeyDown(node, input) is { } baseState)
        {
            return baseState;
        }

        switch (input.KeyCode)
        {
            case "Insert":
                this.HandleInsertAuto();
                input.Handled = true;
                return GuiInputState.Render;

            case "Delete":
                this.HandleItemRemove();
                input.Handled = true;
                return GuiInputState.Render;

            case "F2":
                {
                    if (GetSelectedImGuiNode(node) is { } editNode)
                    {
                        BeginRowEdit(editNode);
                    }
                    input.Handled = true;
                }
                return GuiInputState.Render;

            case "C" when input.ControlKey:
                this.HandleArraySetClipboard(true);
                input.Handled = true;
                return GuiInputState.Render;

            case "X" when input.ControlKey:
                this.HandleArraySetClipboard(false);
                input.Handled = true;
                return GuiInputState.Render;

            case "V" when input.ControlKey:
                this.HandleArrayPaste();
                input.Handled = true;
                return GuiInputState.Render;

            default:
                return null;
        }
    }

    /// <summary>
    /// Raises the <see cref="RequestDoAction"/> event or executes the action directly if no handlers are registered.
    /// </summary>
    /// <param name="action">The value action to execute.</param>
    protected void RaiseRequestDoAction(IValueAction action)
    {
        if (RequestDoAction != null)
        {
            var args = new ValueActionEventArgs(action);
            RequestDoAction(this, args);
            if (!args.Handled)
            {
                action.DoAction();
            }
        }
        else
        {
            action.DoAction();
        }
    }

    #region Gui
    /// <summary>
    /// Draws the name column for a tree node, including icons, text, and edit functionality.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual node data.</param>
    protected virtual void DrawNameColumn(ImGuiNode node, VirtualNode vNode)
    {
        var gui = node.Gui;

        if (DrawPipeline(node, vNode, EditorImGuiPipeline.Name))
        {
            return;
        }

        if (!_statusIconAtTheEnd)
        {
            if (vNode.StatusIcon is { } statusIcon)
            {
                gui.Image("##status_icon", statusIcon)
                .InitClass("icon");
            }
        }

        if (vNode.CustomIcon is { } customIton)
        {
            gui.Image("##custom_icon", customIton)
            .InitClass("icon");
        }

        if (vNode.Icon is { } icon)
        {
            gui.Image("##icon", icon)
            .InitClass("icon");
        }

        Color? color;
        Color? previewColor = null;
        if (vNode.TextStatus != TextStatus.Normal)
        {
            color = EditorServices.ColorConfig.GetStatusColor(vNode.TextStatus);
            previewColor = vNode.ViewColor;
        }
        else
        {
            previewColor = color = vNode.ViewColor;
        }

        if (vNode.CanEditText)
        {
            var input = gui.DoubleClickStringInput("##title_edit", vNode.Text)
            //.InitWidthRest()
            .InitFitHorizontal()
            .InitVerticalAlignment(GuiAlignment.Center)
            .SetReadonly(vNode.ReadOnly)
            .SetFontColor(color)
            .OverridePadding(3)
            .OnEdited(nEdit =>
            {
                vNode.Text = nEdit.Text ?? string.Empty;
                // Reverse validation
                nEdit.Text = vNode.Text;
            });

            if (previewColor.HasValue)
            {
                input.OverrideBorder(1, previewColor);
            }
            else
            {
                input.OverrideBorder(0, null);
            }
        }
        else
        {
            string text = _showDisplayText ? vNode.DisplayText : vNode.Text;

            if (previewColor.HasValue)
            {
                gui.Frame("##title_frame")
                .InitFitHorizontal()
                .OverrideColor(previewColor.Value.MultiplyAlpha(0.3f))
                .OverrideBorder(1, previewColor)
                .OverridePadding(3)
                .OnContent(() =>
                {
                    gui.Text("##title_text", text)
                    .SetFontColor(color)
                    .InitVerticalAlignment(GuiAlignment.Center)
                    .InitInputDoubleClicked(n =>
                    {
                        vNode.HandleNodeAction();
                    });
                });
            }
            else
            {
                gui.Text("##title_text", text)
                .SetFontColor(color)
                .InitVerticalAlignment(GuiAlignment.Center)
                .InitInputDoubleClicked(n =>
                {
                    vNode.HandleNodeAction();
                });
            }
        }

        if (_statusIconAtTheEnd)
        {
            if (vNode.StatusIcon is { } statusIcon)
            {
                gui.Image("##status_icon", statusIcon)
                .InitClass("icon");
            }
        }
    }

    /// <summary>
    /// Draws the description column for a tree node.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual node data.</param>
    protected virtual void DrawDescriptionColumn(ImGuiNode node, VirtualNode vNode)
    {
        var gui = node.Gui;

        if (DrawPipeline(node, vNode, EditorImGuiPipeline.Description))
        {
            return;
        }

        Color? color;
        if (vNode.TextStatus != TextStatus.Normal)
        {
            color = EditorServices.ColorConfig.GetStatusColor(vNode.TextStatus);
        }
        else
        {
            color = vNode.ViewColor;
        }

        gui.Text("##desc", vNode.Description)
        .SetFontColor(color)
        .InitVerticalAlignment(GuiAlignment.Center);
    }

    /// <summary>
    /// Draws the preview column for a tree node, including preview text and icon.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual node data.</param>
    protected virtual void DrawPreviewColumn(ImGuiNode node, VirtualNode vNode)
    {
        var gui = node.Gui;

        if (DrawPipeline(node, vNode, EditorImGuiPipeline.Preview))
        {
            return;
        }

        if (vNode.CanEditPreviewText)
        {
            gui.DoubleClickStringInput("##preview_edit", vNode.PreviewText)
            .InitVerticalAlignment(GuiAlignment.Center)
            .SetReadonly(vNode.ReadOnly)
            .SetFontColor(vNode.PreviewTextStatus)
            .OnEdited(n =>
            {
                vNode.PreviewText = n.Text ?? string.Empty;
                // Reverse validation
                n.Text = vNode.PreviewText;
            });
        }
        else
        {
            gui.Text("##preview_text", vNode.PreviewText)
            .InitVerticalAlignment(GuiAlignment.Center)
            .SetFontColor(vNode.PreviewTextStatus);
        }

        if (vNode.PreviewIcon != null)
        {
            gui.Image("##preview_icon", vNode.PreviewIcon)
            .InitClass("icon");
        }
    }

    /// <summary>
    /// Executes the editor GUI pipeline for a node or its displayed value.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual node data.</param>
    /// <param name="pipeline">The pipeline stage to execute.</param>
    /// <returns>True if the pipeline was handled; otherwise, false.</returns>
    protected bool DrawPipeline(ImGuiNode node, VirtualNode vNode, EditorImGuiPipeline pipeline)
    {
        if (vNode is IDrawEditorImGui drawNode)
        {
            try
            {
                return drawNode.OnEditorGui(node.Gui, pipeline, this);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        if (vNode.DisplayedValue is IDrawEditorImGui drawValue)
        {
            try
            {
                return drawValue.OnEditorGui(node.Gui, pipeline, this);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return false;
    }

    #endregion

    #region IViewSelectionInfo

    /// <inheritdoc/>
    public IEnumerable<object> SelectedObjects =>
        TreeData?.SelectedNodesT
            .Select(o => o.Value.DisplayedValue)
            .OfType<object>() ?? [];

    /// <inheritdoc/>
    public IEnumerable<T> FindSelectionOrParent<T>(bool distinct = true) where T : class
    {
        if (distinct)
        {
            return TreeData?.SelectedNodesT
                .Select(o => o.Value.FindValueOrParent<T>())
                .OfType<T>()
                .Distinct() ?? [];
        }
        else
        {
            return TreeData?.SelectedNodesT
                .Select(o => o.Value.FindValueOrParent<T>()) ?? [];
        }
    }

    #endregion

    #region IViewClipboard

    /// <inheritdoc/>
    public void ClipboardCopy() => this.HandleArraySetClipboard(true);

    /// <inheritdoc/>
    public void ClipboardCut() => this.HandleArraySetClipboard(false);

    /// <inheritdoc/>
    public void ClipboardPaste() => this.HandleArrayPaste();

    #endregion

    #region IViewComment

    /// <inheritdoc/>
    public bool CanComment => this.GetCanComment();

    /// <inheritdoc/>
    public bool IsComment
    {
        get => this.GetIsComment();
        set => this.HandleComment(value);
    }

    #endregion

    #region IServiceProvider

    /// <inheritdoc/>
    public object? GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        var obj = DisplayedObject;
        if (obj is null)
        {
            return null;
        }

        if (serviceType.IsAssignableFrom(obj.GetType()))
        {
            return obj;
        }

        return null;
    }

    #endregion

    #region Selection

    /// <summary>
    /// Gets the previous selection state.
    /// </summary>
    public VirtualPath[] LastSelection => _lastSelection;

    /// <summary>
    /// Creates an array of virtual paths representing the current selection.
    /// </summary>
    /// <returns>An array of <see cref="VirtualPath"/> objects for the selected nodes.</returns>
    public VirtualPath[] MakeSelection()
    {
        if (_model is null)
        {
            return [];
        }

        List<VirtualPath> sels = [];
        foreach (var node in SelectedNodes)
        {
            sels.Add(_model.GetVirtualPath(node));
        }

        return [.. sels];
    }

    /// <summary>
    /// Sets the selection using virtual paths.
    /// </summary>
    /// <param name="paths">The virtual paths to select.</param>
    /// <param name="append">True to append to the current selection; false to replace it.</param>
    /// <param name="notify">True to raise selection changed notification; otherwise, false.</param>
    public void SetSelection(IEnumerable<VirtualPath> paths, bool append = false, bool notify = true)
    {
        UnwatchedSelectionAction(() =>
        {
            if (!append)
            {
                ClearSelection(false);
            }
            foreach (var selection in paths)
            {
                var node = _model?.GetNodeByVirtualPath(selection);
                if (node != null)
                {
                    SelectNode(node, true, false);
                }
            }

            UpdateImGuiSelection();
        });

        if (notify)
        {
            OnSelectionChanged();
        }
        // Record previous selection even without notification
        _lastSelection = MakeSelection();
    }

    /// <summary>
    /// Sets the selection using virtual nodes.
    /// </summary>
    /// <param name="nodes">The virtual nodes to select.</param>
    /// <param name="append">True to append to the current selection; false to replace it.</param>
    /// <param name="notify">True to raise selection changed notification; otherwise, false.</param>
    public void SetSelection(IEnumerable<VirtualNode> nodes, bool append = false, bool notify = true)
    {
        UnwatchedSelectionAction(() =>
        {
            if (!append)
            {
                ClearSelection(false);
            }
            foreach (var node in nodes)
            {
                if (node is not null)
                {
                    SelectNode(node, true, false);
                }
            }

            UpdateImGuiSelection();
        });

        if (notify)
        {
            OnSelectionChanged();
        }
        // Record previous selection even without notification
        _lastSelection = MakeSelection();
    }

    /// <summary>
    /// Sets the selection using a sync path.
    /// </summary>
    /// <param name="path">The sync path to select.</param>
    /// <param name="rest">The remaining path that could not be resolved.</param>
    /// <param name="notify">True to raise selection changed notification; otherwise, false.</param>
    public void SetSelection(SyncPath path, out SyncPath rest, bool notify = true)
    {
        rest = SyncPath.Empty;

        VirtualNode? virtualNode = _model?.GetNodeBySyncPath(path, out rest);
        if (virtualNode != null)
        {
            SelectNode(virtualNode, false, notify);
            // Record previous selection even without notification
            _lastSelection = MakeSelection();

            UpdateImGuiSelection();
        }
    }

    /// <summary>
    /// Sets the selection using displayed values.
    /// </summary>
    /// <param name="values">The values to select.</param>
    /// <param name="notify">True to raise selection changed notification; otherwise, false.</param>
    /// <returns>True if at least one node was selected; otherwise, false.</returns>
    public bool SetSelection(IEnumerable<object> values, bool notify = true)
    {
        if (values is null)
        {
            return false;
        }

        if (_model is null)
        {
            return false;
        }

        var nodes = values.Select(o => _model.FindNode(o)).OfType<VirtualNode>().Distinct().ToArray();

        if (nodes.Length > 0)
        {
            UnwatchedSelectionAction(() =>
            {
                ClearSelection(false);
                SelectNodes(nodes, false);

                UpdateImGuiSelection();
            });

        if (notify)
        {
            OnSelectionChanged();
        }
            // Record previous selection even without notification
            _lastSelection = MakeSelection();

            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateImGuiSelection()
    {
        var treeNode = FindTreeViewNode();
        if (treeNode is not null)
        {
            GuiTreeViewExtensions.UpdateTreeNodeSelections(treeNode);
            treeNode.Gui.QueueInputState(GuiInputState.Render);
        }
    }

    /// <inheritdoc/>
    protected internal override void OnSelectionChanged()
    {
        if (SelectionWatching)
        {
            return;
        }

        if (SelectionActionEnabled)
        {
            _model?.HandleSetterAction(new SingleTreeSelectionAction(this));
        }

        _lastSelection = MakeSelection();

        RaiseSelectionChanged();
    }

    #endregion

    #region Expanding

    /// <summary>
    /// Expands the root node of the tree.
    /// </summary>
    public void ExpandRoot() => _model?.RootNode?.Expand();

    /// <summary>
    /// Expands all nodes in the tree.
    /// </summary>
    public void ExpandAll() => _model?.RootNode?.ExpandAll();

    /// <summary>
    /// Expands the specified virtual node.
    /// </summary>
    /// <param name="listNode">The node to expand.</param>
    internal void ExpandNode(VirtualNode listNode) => listNode.Expand();

    #endregion

    #region Preview

    /// <summary>
    /// Adds a preview path to the tree model.
    /// </summary>
    /// <param name="path">The preview path to add.</param>
    /// <returns>True if the path was added successfully; otherwise, false.</returns>
    public bool AddPreviewPath(PreviewPath path)
    {
        if (_model is null)
        {
            return false;
        }

        if (!_model.AddPreviewPath(path))
        {
            return false;
        }

        _model.UpdateDisplayedObject();
        OnColumnUpdated();

        return true;
    }

    /// <summary>
    /// Removes a preview path from the tree model.
    /// </summary>
    /// <param name="path">The preview path to remove.</param>
    /// <returns>True if the path was removed successfully; otherwise, false.</returns>
    public bool RemovePreviewPath(PreviewPath path)
    {
        if (_model is null)
        {
            return false;
        }

        if (!_model.RemovePreviewPath(path))
        {
            return false;
        }

        _model.UpdateDisplayedObject();
        OnColumnUpdated();

        return true;
    }

    /// <summary>
    /// Removes a preview path at the specified index.
    /// </summary>
    /// <param name="index">The index of the preview path to remove.</param>
    /// <returns>True if the path was removed successfully; otherwise, false.</returns>
    public bool RemovePreviewPathAt(int index)
    {
        if (_model is null)
        {
            return false;
        }

        if (!_model.RemovePreviewPathAt(index))
        {
            return false;
        }

        OnColumnRemoved(index);
        _model.UpdateDisplayedObject();
        OnColumnUpdated();

        return true;
    }

    /// <summary>
    /// Swaps two preview paths by index.
    /// </summary>
    /// <param name="index">The index of the first preview path.</param>
    /// <param name="indexTo">The index of the second preview path.</param>
    /// <returns>True if the paths were swapped successfully; otherwise, false.</returns>
    public bool SwapPreviewPath(int index, int indexTo)
    {
        if (_model is null)
        {
            return false;
        }

        if (!_model.SwapPreviewPath(index, indexTo))
        {
            return false;
        }

        OnColumnSwap(index, indexTo);
        _model.UpdateDisplayedObject();
        OnColumnUpdated();

        return true;
    }

    /// <summary>
    /// Removes a preview path and inserts it at a new position.
    /// </summary>
    /// <param name="indexFrom">The original index of the preview path.</param>
    /// <param name="indexInsert">The new index to insert the preview path.</param>
    /// <returns>True if the operation was successful; otherwise, false.</returns>
    public bool RemoveInsertPreviewPath(int indexFrom, int indexInsert)
    {
        if (_model is null)
        {
            return false;
        }

        if (!_model.RemoveInsertPreviewPath(indexFrom, indexInsert))
        {
            return false;
        }

        OnColumnRemoveInsert(indexFrom, indexInsert);
        _model.UpdateDisplayedObject();
        OnColumnUpdated();

        return true;
    }

    /// <summary>
    /// Clears all preview paths from the tree model.
    /// </summary>
    public void ClearPreviewPath()
    {
        if (_model is null)
        {
            return;
        }

        if (_model.ClearPreviewPath())
        {
            _model.UpdateDisplayedObject();
            OnColumnUpdated();
        }
    }

    /// <summary>
    /// Adds multiple preview paths to the tree model, replacing any existing paths.
    /// </summary>
    /// <param name="paths">The collection of preview paths to add.</param>
    public void AddPreviewPaths(IEnumerable<PreviewPath> paths)
    {
        if (_model is null)
        {
            return;
        }

        _model.ClearPreviewPath();

        foreach (var path in paths.SkipNull())
        {
            _model.AddPreviewPath(path);
        }

        _model.UpdateDisplayedObject();
        OnColumnUpdated();
    }

    /// <summary>
    /// Called when a preview column is removed.
    /// </summary>
    /// <param name="index">The index of the removed column.</param>
    protected virtual void OnColumnRemoved(int index)
    {
    }

    /// <summary>
    /// Called when two preview columns are swapped.
    /// </summary>
    /// <param name="index">The index of the first column.</param>
    /// <param name="indexTo">The index of the second column.</param>
    protected virtual void OnColumnSwap(int index, int indexTo)
    {
    }

    /// <summary>
    /// Called when a preview column is removed and inserted at a new position.
    /// </summary>
    /// <param name="indexFrom">The original index of the column.</param>
    /// <param name="indexInsert">The new index to insert the column.</param>
    protected virtual void OnColumnRemoveInsert(int indexFrom, int indexInsert)
    {
    }

    /// <summary>
    /// Called when the preview columns are updated.
    /// </summary>
    protected virtual void OnColumnUpdated()
    {
    }

    #endregion

    #region Display

    /// <summary>
    /// Gets or sets the currently displayed object in the tree view.
    /// </summary>
    public object? DisplayedObject
    {
        get => _model?.DisplayedObject;
        set
        {
            if (!(_model is { } model))
            {
                return;
            }

            if (model.DisplayedObject == value)
            {
                return;
            }

            (model.DisplayedObject as IViewListener)?.NotifyViewExit(model.ViewId);
            TreeData?.CleanUpPool();
            model.SetDisplayedObject(value);
            TreeData?.Refresh();
            (model.DisplayedObject as IViewListener)?.NotifyViewEnter(model.ViewId);
        }
    }

    /// <summary>
    /// Updates the displayed object to reflect any model changes.
    /// </summary>
    public void UpdateDisplayedObject() => _model?.UpdateDisplayedObject();

    /// <summary>
    /// Gets the root virtual node of the displayed object.
    /// </summary>
    public VirtualNode? DisplayedNode => _model?.RootNode;

    #endregion
}