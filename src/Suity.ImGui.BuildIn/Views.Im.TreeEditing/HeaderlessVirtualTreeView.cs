using Suity.Editor.Services;
using Suity.Editor.VirtualTree;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents a virtual tree view without a header, providing a simplified single-column tree display.
/// </summary>
public class HeaderlessVirtualTreeView : ImGuiVirtualTreeView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderlessVirtualTreeView"/> class with a view ID.
    /// </summary>
    /// <param name="viewId">The ID of the view.</param>
    /// <param name="menuName">Optional name for the context menu.</param>
    public HeaderlessVirtualTreeView(int viewId, string? menuName = null)
        : this(new ImGuiVirtualTreeModel { ViewId = viewId }, menuName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderlessVirtualTreeView"/> class with a tree model.
    /// </summary>
    /// <param name="model">The tree model to display.</param>
    /// <param name="menuName">Optional name for the context menu.</param>
    public HeaderlessVirtualTreeView(IImGuiTreeModel<VirtualNode> model, string? menuName = null)
        : base(model, menuName)
    {
        Theme.ClassStyle("title_edit")
            .SetColor(Theme.ColorScheme.EditorBG.MultiplyAlpha(0f))
            .SetCornerRound(0)
            .SetBorder(0);

        ContentGui = DrawNameColumn;
    }


    /// <summary>
    /// Gets or sets the pipeline action executed for each row during rendering.
    /// </summary>
    public Action<ImGuiNode, VirtualNode, EditorImGuiPipeline>? RowPipeline { get; set; }

    /// <summary>
    /// Gets or sets the content GUI action for rendering row content.
    /// </summary>
    public Action<ImGuiNode, VirtualNode?> ContentGui { get; set; }



    /// <inheritdoc/>
    protected internal override void BeginRowEdit(ImGuiNode node)
    {
        var titleStringInputNode = node.GetNodeAt(0)?.GetChildNode("##title_edit");
        if (titleStringInputNode is { })
        {
            titleStringInputNode.BeginEdit();
        }
    }

    #region Template

    /// <inheritdoc/>
    protected internal override VisualTreeData<VirtualNode> CreateTreeData(VisualTreeVisitor<VirtualNode> visitor)
    {
        var data = _ex.CreateVisualTreeData(visitor, defaultHeight: DefaultRowHeight);

        data.HeaderHeight = null;
        data.HeaderTemplate = null;
        data.RowTemplate = RowGui;
        data.SelectionMode = ImTreeViewSelectionMode.Multiple;
        data.InitExpand = true;
        data.Width = null;

        return data;
    }

    /// <inheritdoc/>
    protected internal override void RowGui(ImGuiNode node, VisualTreeNode<VirtualNode> item)
    {
        var gui = node.Gui;
        var vNode = item.Value;

        DrawRowPipeline(node, vNode, EditorImGuiPipeline.Normal);

        node
        .InitFullWidth()
        .OnContent(() =>
        {
            var rootTarget = vNode.Tag as PropertyTarget;
            if (rootTarget is null || !ReferenceEquals(rootTarget.GetValues().FirstOrDefault(), vNode.DisplayedValue))
            {
                vNode.Tag = rootTarget = CreateRootTarget(vNode.DisplayedValue);
            }

            // Reset Target at start
            if (rootTarget != null)
            {
                rootTarget.ClearFields();
                rootTarget.PopulatePath(SyncPath.Empty, true);
            }

            DrawRowPipeline(node, vNode, EditorImGuiPipeline.Begin);

            var titleNode = gui.TreeNodeTitle(item, n =>
            {
                n.InitFitHorizontal();

                if (ContentGui is { } action)
                {
                    action(n, vNode);
                }
                else
                {
                    DrawNameColumn(n, vNode);
                }
            });

            DrawRowPipeline(node, vNode, EditorImGuiPipeline.Preview);
            DrawRowPipeline(node, vNode, EditorImGuiPipeline.End);
        })
        .SetReadonly(vNode.ReadOnly)
        .OnTreeNodeDragStart(n =>
        {
            if (TreeData is { } treeData && treeData.SelectedNodesT.Any())
            {
                return new VirtualTreeDragData(treeData.SelectedNodesT);
            }
            else
            {
                return null;
            }
        })
        .OnTreeNodeDragOver((dropEvent, mode) =>
        {
            node.HandleDragOver(this, dropEvent, mode);
        })
        .OnTreeNodeDragDrop((dropEvent, mode) =>
        {
            node.HandleDragDrop(this, dropEvent, mode);
        });
    }

    

    private bool DrawRowPipeline(ImGuiNode node, VirtualNode vNode, EditorImGuiPipeline pipeline)
    {
        if (vNode is IDrawEditorImGui drawNode)
        {
            try
            {
                // vNode.DisplayedValue is handled inside the following pipeline.
                if (drawNode.OnEditorGui(node.Gui, pipeline, this))
                {
                    return true;
                }
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
                if (drawValue.OnEditorGui(node.Gui, pipeline, this))
                {
                    return true;
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return false;
    }

    private PropertyTarget CreateRootTarget(object obj, int viewId = 0)
    {
        object[] objs = [obj];
        var target = PropertyTargetUtility.CreatePropertyTarget(objs);
        target.ViewId = viewId;
        target.ServiceProvider = this;

        // Try CacheValues to improve rendering efficiency
        //target.CacheValues = false;

        return target;
    }

    #endregion
}
