using Suity.Views.PathTree;
using System;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// A tree view control that displays <see cref="PathNode"/> items in a column-based layout.
/// Extends <see cref="ImGuiPathTreeView"/> with support for name, description, and preview columns.
/// </summary>
public class ColumnPathTreeView : ImGuiPathTreeView
{
    private readonly Column3Template<PathNode> _column;

    /// <summary>
    /// Raised when a request is made to open a <see cref="PathNode"/>.
    /// </summary>
    public event Action<PathNode>? OpenRequest;

    /// <summary>
    /// Raised when a request is made to delete a <see cref="PathNode"/>.
    /// </summary>
    public event Action<PathNode>? DeleteRequest;

    /// <summary>
    /// Optional configuration callback for customizing the icon area of each row.
    /// </summary>
    public Action<ImGui>? RowIconConfig;

    /// <summary>
    /// Gets the column template used by this tree view.
    /// </summary>
    public Column3Template<PathNode> Column => _column;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnPathTreeView"/> class with a default model.
    /// </summary>
    public ColumnPathTreeView()
        : this(new ImGuiPathTreeModel())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnPathTreeView"/> class with the specified model.
    /// </summary>
    /// <param name="model">The tree model that provides <see cref="PathNode"/> data.</param>
    public ColumnPathTreeView(IImGuiTreeModel<PathNode> model)
        : base(model)
    {
        _column = new Column3Template<PathNode>()
        {
            RowPipeline = ConfigRowPipeline,
            BeginEditAction = ConfigBeginEdit,
        };

        _column.NameColumn.RowGui = ConfigNameColumn;
        _column.DescriptionColumn.RowGui = ConfigDescriptionColumn;
        _column.PreviewColumn.RowGui = ConfigPreviewColumn;

        this.ViewTemplate = _column;
    }

    /// <summary>
    /// Configures the row pipeline for each node, setting up drag-and-drop behavior.
    /// </summary>
    /// <param name="node">The ImGui node representing the tree item.</param>
    /// <param name="vNode">The <see cref="PathNode"/> data associated with this tree item.</param>
    /// <param name="pipeline">The current rendering pipeline stage.</param>
    protected virtual void ConfigRowPipeline(ImGuiNode node, PathNode vNode, EditorImGuiPipeline pipeline)
    {
        if (pipeline == EditorImGuiPipeline.Normal)
        {
            node
            .InitFullWidth()
            .OnTreeNodeDragStart(n =>
            {
                if (TreeData is { } treeData && treeData.SelectedNodesT.Any())
                {
                    return new PathTreeDragData(treeData.SelectedNodesT);
                }
                else
                {
                    return null;
                }
            })
            .OnTreeNodeDragOver((dropEvent, mode) =>
            {
                OnDragOver(node, dropEvent, mode);
            })
            .OnTreeNodeDragDrop((dropEvent, mode) =>
            {
                OnDragDrop(node, dropEvent, mode);
            });
        }
    }

    /// <summary>
    /// Configures the name column for a tree node, rendering icons, text, and an optional inline editor.
    /// </summary>
    /// <param name="node">The ImGui node representing the tree item.</param>
    /// <param name="vNode">The <see cref="PathNode"/> data associated with this tree item.</param>
    protected virtual void ConfigNameColumn(ImGuiNode node, PathNode vNode)
    {
        var gui = node.Gui;

        if (vNode.CustomImage != null)
        {
            gui.Image("##custom_icon", vNode.CustomImage)
            .InitClass("icon");
        }
        if (vNode.Image != null)
        {
            gui.Image("##icon", vNode.Image)
            .InitClass("icon");
        }
        if (vNode.TextStatusIcon != null)
        {
            gui.Image("##status_icon", vNode.TextStatusIcon)
            .InitClass("icon");
        }

        RowIconConfig?.Invoke(gui);

        if (vNode.CanEditText)
        {
            gui.ManualStringInput("##title_edit", vNode.Text)
            .InitClass("title_edit")
            .InitWidthRest()
            .InitVerticalAlignment(GuiAlignment.Center)
            //.SetReadonly(vNode.ReadOnly)
            .SetFontColor(vNode.Color)
            .OnEdited(nEdit =>
            {
                vNode.Text = nEdit.Text ?? string.Empty;
                // Reverse Verification
                nEdit.Text = vNode.Text;
            })
            .InitInputDoubleClicked(_ => OnOpenRequest(vNode));
        }
        else
        {
            gui.Text("##title_text", vNode.Text)
            .SetFontColor(vNode.Color)
            .InitVerticalAlignment(GuiAlignment.Center);
        }
    }

    /// <summary>
    /// Configures the description column for a tree node. Override to add custom description rendering.
    /// </summary>
    /// <param name="node">The ImGui node representing the tree item.</param>
    /// <param name="vNode">The <see cref="PathNode"/> data associated with this tree item.</param>
    protected virtual void ConfigDescriptionColumn(ImGuiNode node, PathNode vNode)
    {
    }

    /// <summary>
    /// Configures the preview column for a tree node. Override to add custom preview rendering.
    /// </summary>
    /// <param name="node">The ImGui node representing the tree item.</param>
    /// <param name="vNode">The <see cref="PathNode"/> data associated with this tree item.</param>
    protected virtual void ConfigPreviewColumn(ImGuiNode node, PathNode vNode)
    {
    }

    /// <summary>
    /// Configures and initiates inline editing for a tree node by focusing the title input field.
    /// </summary>
    /// <param name="node">The ImGui node representing the tree item to begin editing.</param>
    protected virtual void ConfigBeginEdit(ImGuiNode node)
    {
        var titleStringInputNode = node.GetNodeAt(0)?.GetChildNode("##title_edit");
        if (titleStringInputNode is { })
        {
            titleStringInputNode.BeginEdit();
        }
    }

    /// <summary>
    /// Handles an open request for a <see cref="PathNode"/>, either by raising the <see cref="OpenRequest"/> event
    /// or by invoking <see cref="IViewDoubleClickAction.DoubleClick"/> on the node if it implements the interface.
    /// </summary>
    /// <param name="pathNode">The <see cref="PathNode"/> that was requested to open.</param>
    protected virtual void OnOpenRequest(PathNode pathNode)
    {
        if (OpenRequest != null)
        {
            OpenRequest(pathNode);
        }
        else if (pathNode is IViewDoubleClickAction actionNode)
        {
            actionNode.DoubleClick();
        }
    }
}