using Suity.Views.PathTree;
using System;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Abstract base class for ImGui-based path tree views that display and interact with <see cref="PathNode"/> hierarchies.
/// Provides drag-and-drop support and integrates with <see cref="PathTreeModel"/> for data management.
/// </summary>
public abstract class ImGuiPathTreeView : ImGuiTreeView<PathNode>
{
    private PathTreeModel? _model;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiPathTreeView"/> class.
    /// </summary>
    protected ImGuiPathTreeView()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiPathTreeView"/> class with the specified tree model.
    /// </summary>
    /// <param name="model">The tree model providing <see cref="PathNode"/> data.</param>
    protected ImGuiPathTreeView(IImGuiTreeModel<PathNode> model)
        : base(model)
    {
    }

    /// <summary>
    /// Gets the underlying <see cref="PathTreeModel"/> associated with this view, or <c>null</c> if not set.
    /// </summary>
    public PathTreeModel? PathModel => _model;

    /// <summary>
    /// Gets or sets the <see cref="PathNode"/> currently being targeted by a drag-and-drop operation.
    /// </summary>
    public PathNode? DroppingNode { get; set; }

    /// <inheritdoc/>
    protected internal override void OnTreeModelEnter(IImGuiTreeModel<PathNode> model)
    {
        _model = model as PathTreeModel;
    }

    /// <summary>
    /// Handles the drag-over event for a tree node during a drag-and-drop operation.
    /// </summary>
    /// <param name="node">The ImGui node being dragged over.</param>
    /// <param name="dropEvent">The drag event containing drop data and state.</param>
    /// <param name="mode">The drag-and-drop mode indicating how the drop should be interpreted.</param>
    /// <returns><c>true</c> if the drag-over was handled; otherwise, <c>false</c>.</returns>
    protected bool OnDragOver(ImGuiNode node, IDragEvent dropEvent, ImTreeNodeDragDropMode mode)
    {
        var droppingNode = DroppingNode = node.GetPathTreeDroppingNode(mode);
        if (droppingNode is null)
        {
            dropEvent.SetNoneEffect();
            return false;
        }

        bool result = false;

        try
        {
            if (droppingNode is IDropTarget dropTarget)
            {
                dropTarget.DragOver(dropEvent);
                result = dropEvent.Handled;
            }
            else
            {
                result = this.HandleDragOver(dropEvent);
            }
        }
        catch (Exception err)
        {
            Logs.LogError(err);
            dropEvent.SetNoneEffect();
        }

        DroppingNode = null;

        return result;
    }

    /// <summary>
    /// Handles the drag-drop event for a tree node, completing a drag-and-drop operation.
    /// </summary>
    /// <param name="node">The ImGui node receiving the drop.</param>
    /// <param name="dropEvent">The drag event containing drop data and state.</param>
    /// <param name="mode">The drag-and-drop mode indicating how the drop should be interpreted.</param>
    protected void OnDragDrop(ImGuiNode node, IDragEvent dropEvent, ImTreeNodeDragDropMode mode)
    {
        var droppingNode = DroppingNode = node.GetPathTreeDroppingNode(mode);
        if (droppingNode is null)
        {
            return;
        }

        try
        {
            if (droppingNode is IDropTarget dropTarget)
            {
                dropTarget.DragDrop(dropEvent);
            }
            else
            {
                this.HandleDragDrop(dropEvent);
            }

            node.QueueRefresh();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }

        DroppingNode = null;
    }
}

/// <summary>
/// A concrete implementation of <see cref="ImGuiPathTreeView"/> that provides a simple, ready-to-use path tree view.
/// Supports multiple selection, row configuration via events, and displays node icons and status indicators.
/// </summary>
public class SimplePathTreeView : ImGuiPathTreeView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimplePathTreeView"/> class with a default <see cref="ImGuiPathTreeModel"/>.
    /// </summary>
    public SimplePathTreeView()
        : this(new ImGuiPathTreeModel())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimplePathTreeView"/> class with the specified model.
    /// </summary>
    /// <param name="model">The path tree model providing <see cref="PathNode"/> data.</param>
    public SimplePathTreeView(ImGuiPathTreeModel model)
        : base(model)
    {
    }

    /// <summary>
    /// Occurs when a row is being configured, allowing external code to customize the row's GUI.
    /// </summary>
    public event Action<ImGuiNode, PathNode>? ConfigRowAction;

    #region Template

    /// <inheritdoc/>
    protected internal override VisualTreeData<PathNode> CreateTreeData(VisualTreeVisitor<PathNode> visitor)
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
    protected internal override void RowGui(ImGuiNode node, VisualTreeNode<PathNode> item)
    {
        PathNode pNode = item.Value;

        node.OnContent(() =>
        {
            var gui = node.Gui;

            var titleNode = gui.TreeNodeTitle(item, n =>
            {
                if (pNode.Image != null)
                {
                    gui.Image("##icon", pNode.Image)
                    .InitClass("icon");
                }
                if (pNode.TextStatusIcon != null)
                {
                    gui.Image("##status_icon", pNode.TextStatusIcon)
                    .InitClass("icon");
                }

                var textNode = gui.Text("##title_text", item.Value.Text)
                .SetFontColor(pNode.Color)
                .InitVerticalAlignment(GuiAlignment.Center);
            }).InitFullWidth();
        })
        .InitFullWidth();

        ConfigRowAction?.Invoke(node, pNode);
    }

    #endregion
}