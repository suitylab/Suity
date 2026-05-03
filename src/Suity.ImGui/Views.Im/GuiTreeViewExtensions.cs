using Suity.Drawing;
using Suity.Views.Graphics;
using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating tree view controls in ImGui.
/// </summary>
public static class GuiTreeViewExtensions
{
    /// <summary>
    /// CSS class name for tree view containers.
    /// </summary>
    public const string ClassTreeView = "treeView";

    /// <summary>
    /// CSS class name for tree view rows with odd index.
    /// </summary>
    public const string ClassTreeViewRow1 = "treeRow1";

    /// <summary>
    /// CSS class name for tree view rows with even index.
    /// </summary>
    public const string ClassTreeViewRow2 = "treeRow2";

    /// <summary>
    /// CSS class name for selected tree view rows.
    /// </summary>
    public const string ClassTreeViewRowSelected = "treeRowSel";

    /// <summary>
    /// CSS class name for expand/collapse buttons.
    /// </summary>
    public const string ClassExpandButton = "expandBtn";

    /// <summary>
    /// Creates a tree view control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="scroll">The scroll orientation.</param>
    /// <param name="initClass">Initial CSS class.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode TreeView(this ImGui gui, string id, GuiOrientation scroll = GuiOrientation.Vertical, string initClass = ClassTreeView)
    {
        return gui.VirtualList(id, scroll)
            .InitInputFunctionChain(ImGuiInputSystem.TreeView)
            .InitClass(initClass);
    }

    /// <summary>
    /// Sets the tree data for a tree view node.
    /// </summary>
    public static ImGuiNode SetTreeNodeData(this ImGuiNode node, VisualTreeData data)
    {
        node.SetValue(data);
        data.CheckRefresh();
        return node.SetVirtualListData(data.ListData, TreeNodeFactory);
    }

    /// <summary>
    /// Removes tree data from a tree view node.
    /// </summary>
    public static ImGuiNode UnsetTreeNodeData(this ImGuiNode node)
    {
        node.RemoveValue<VisualTreeData>();
        node.UnsetVirtualListData();
        return node;
    }

    /// <summary>
    /// Creates a tree node title with plain text.
    /// </summary>
    public static ImGuiNode TreeNodeTitle(this ImGui gui, VisualTreeNode node, string text)
    {
        return TreeNodeTitle(gui, node, n =>
        {
            gui.Text("###text", text);
        });
    }

    /// <summary>
    /// Creates a tree node title with custom content.
    /// </summary>
    public static ImGuiNode TreeNodeTitle(this ImGui gui, VisualTreeNode node, Action<ImGuiNode> titleAction)
    {
        var currentGuiNode = gui.CurrentNode;
        currentGuiNode.SetValue(node);
        node.NodePath = currentGuiNode.FullPath;
        currentGuiNode.UpdateTreeNodeSelection(node);
        var titleNode = gui.HorizontalLayout("###title").InitFullHeight();
        titleNode.OnContent(() =>
        {
            float indent = node.Indent;
            float indentWidth = node.Tree.IndentWidth;
            if (indent > 0)
            {
                gui.EmptyFrame().SetWidth(indent * indentWidth);
            }
            if (node.CanExpand)
            {
                gui.Button("###expandBtn", currentGuiNode.Theme.ExpandImage)
                    .InitClass("expandBtn")
                    .InitInputFunction(ExpandButtonInput)
                    .InitRenderFunctionChain(RenderButtonExpandImage)
                    .InitValue(node);
            }
            else
            {
                gui.Image("###expandDummyImg", currentGuiNode.Theme.EmptyImage)
                    .InitClass("expandBtn");
            }
            titleAction(titleNode);
        });
        return titleNode;
    }

    /// <summary>
    /// Handles drag start events for tree nodes.
    /// </summary>
    public static ImGuiNode OnTreeNodeDragStart(this ImGuiNode node, Func<ImGuiNode, object?> func)
    {
        var value = node.GetValue<VisualTreeNode>();
        if (value is { DragRequesting: true })
        {
            value.DragRequesting = false;
            var obj = func(node);
            if (obj is { })
            {
                (node.Gui.Context as IGraphicDragDrop)?.DoDragDrop(obj);
            }
        }
        return node;
    }

    /// <summary>
    /// Handles drag over events for tree nodes.
    /// </summary>
    public static ImGuiNode OnTreeNodeDragOver(this ImGuiNode node, Action<IDragEvent, ImTreeNodeDragDropMode> action)
    {
        VisualTreeNode? value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return node;
        }
        if (value.Tree.DroppingNode != value)
        {
            return node;
        }
        return node.OnDragOver(data => action(data, value.Tree.DroppingMode));
    }

    /// <summary>
    /// Handles drag drop events for tree nodes.
    /// </summary>
    public static ImGuiNode OnTreeNodeDragDrop(this ImGuiNode node, Action<IDragEvent, ImTreeNodeDragDropMode> action)
    {
        VisualTreeNode? value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return node;
        }
        if (value.Tree.DroppingNode != value)
        {
            return node;
        }
        return node.OnDragDrop(data =>
        {
            action(data, value.Tree.DroppingMode);
            value.Tree.Refresh();
            node.Gui.QueueAction(() =>
            {
                node.MarkRenderDirty();
                node.Parent?.MarkRenderDirty();
            });
        });
    }

    /// <summary>
    /// Updates the selection state for all child nodes.
    /// </summary>
    public static void UpdateTreeNodeSelections(this ImGuiNode? parentNode)
    {
        if (parentNode is null)
        {
            return;
        }
        foreach (var node in parentNode.ChildNodes)
        {
            var value = node.GetValue<VisualTreeNode>();
            if (value is null)
            {
                continue;
            }
            UpdateTreeNodeSelection(node, value);
        }
    }

    /// <summary>
    /// Updates the selection state for a single tree node.
    /// </summary>
    /// <param name="node">The ImGui node to update the CSS classes on.</param>
    /// <param name="value">The visual tree node containing selection state.</param>
    public static void UpdateTreeNodeSelection(this ImGuiNode node, VisualTreeNode value)
    {
        if (value.IsSelected)
        {
            node.SwapClass(ClassTreeViewRow1, ClassTreeViewRowSelected);
            node.SwapClass(ClassTreeViewRow2, ClassTreeViewRowSelected);
        }
        else
        {
            if (value.Index % 2 == 0)
            {
                node.SwapClass(ClassTreeViewRowSelected, ClassTreeViewRow1);
            }
            else
            {
                node.SwapClass(ClassTreeViewRowSelected, ClassTreeViewRow2);
            }
        }
    }

    private static ImGuiNode TreeNodeFactory(ImGui gui, string? id, object? data)
    {
        id ??= $"##list_item_{gui.CurrentNode.CurrentLayoutIndex}";
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "TreeNode";
            node.SetInputFunction(ImGuiInputSystem.TreeNode);
            node.SetLayoutFunction(ImGuiLayoutSystem.Horizontal);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.SetRenderFunction(nameof(GuiCommonExtensions.Frame));
            node.InitRenderFunctionChain(TreeNodeDragDropRender);
            node.FitOrientation = GuiOrientation.Vertical;
            if (data is VisualTreeNode value)
            {
                if (value.Index % 2 == 0)
                {
                    node.SwapClass(ClassTreeViewRowSelected, ClassTreeViewRow1);
                }
                else
                {
                    node.SwapClass(ClassTreeViewRowSelected, ClassTreeViewRow2);
                }
            }
            else
            {
                node.SetClass(ClassTreeViewRow1);
            }
        }
        node.Layout();
        return node;
    }

    private static readonly BrushDef _dragDropBrush = new SolidBrushDef(ImGuiTheme.DefaultDragColor);
    private static readonly PenDef _dragDropPen = new(ImGuiTheme.DefaultDragColor, 3);

    private static void TreeNodeDragDropRender(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        baseAction(pipeline);
        if (node.Gui.Input.DragEvent is null)
        {
            return;
        }
        if (node.Gui.Input.EventType == GuiEventTypes.DragDrop)
        {
            return;
        }
        var value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return;
        }
        if (!(node.Gui.Input.MouseLocation is { } pos))
        {
            return;
        }
        var rect = node.GlobalRect;
        if (!rect.Contains(pos))
        {
            return;
        }
        float h4 = rect.Height * 0.25f;
        float top = rect.Top + h4;
        float bottom = rect.Bottom - h4;
        float indent = (value.Indent + 1) * value.Tree.IndentWidth;
        float x = rect.X + indent;
        float w = rect.Width - indent;
        if (pos.Y < top)
        {
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Y, w, 3));
        }
        else if (pos.Y > bottom)
        {
            output.FillRectangle(_dragDropBrush, new RectangleF(x, rect.Bottom - 3, w, 3));
        }
        else
        {
            output.DrawRoundRectangle(_dragDropPen, new RectangleF(x + 2, rect.Y + 2, w - 4, rect.Height - 4), 5);
        }
    }

    private static void RenderButtonExpandImage(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var value = node.GetValue<VisualTreeNode>();
        if (value != null)
        {
            node.Image = value.Expanded ? node.Theme.ExpandImage : node.Theme.CollapseImage;
        }
        else
        {
            node.Image = node.Theme.EmptyImage;
        }
        baseAction(pipeline);
    }

    private static GuiInputState ExpandButtonInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);
        var value = node.GetValue<VisualTreeNode>();
        if (value is null)
        {
            return state;
        }
        switch (input.EventType)
        {
            case GuiEventTypes.MouseDown:
                node.Pseudo = ImGuiNode.PseudoMouseDown;
                node.MarkRenderDirty();
                value.Expanded = !value.Expanded;
                value.Refresh();
                return GuiInputState.FullSync;
            case GuiEventTypes.MouseUp:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;
            case GuiEventTypes.MouseIn:
                node.Pseudo = ImGuiNode.PseudoMouseIn;
                node.MarkRenderDirty();
                return GuiInputState.Render;
            case GuiEventTypes.MouseOut:
                node.Pseudo = null;
                node.MarkRenderDirty();
                return GuiInputState.Render;
            default:
                return GuiInputState.None;
        }
    }
}
