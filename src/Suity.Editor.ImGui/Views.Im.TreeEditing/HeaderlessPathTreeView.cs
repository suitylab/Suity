using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.PathTree;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// A path tree view without a header row, supporting inline editing, drag-and-drop,
/// and open/delete requests on path nodes.
/// </summary>
public class HeaderlessPathTreeView : ImGuiPathTreeView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderlessPathTreeView"/> class
    /// with a default <see cref="ImGuiPathTreeModel"/>.
    /// </summary>
    public HeaderlessPathTreeView()
        : this(new ImGuiPathTreeModel())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderlessPathTreeView"/> class
    /// with the specified tree model.
    /// </summary>
    /// <param name="model">The tree model that provides path node data.</param>
    public HeaderlessPathTreeView(IImGuiTreeModel<PathNode> model)
        : base(model)
    {
        Theme.ClassStyle("title_edit")
            .SetColor(Theme.ColorScheme.EditorBG.MultiplyAlpha(0f))
            .SetCornerRound(0)
            .SetBorder(0);

        ContentGui = DefaultContentGui;
    }

    /// <summary>
    /// Occurs when an open request is made for a path node (e.g., via double-click or Enter key).
    /// </summary>
    public event Action<PathNode>? OpenRequest;

    /// <summary>
    /// Occurs when a delete request is made for a path node (e.g., via Delete key).
    /// </summary>
    public event Action<PathNode>? DeleteRequest;

    /// <summary>
    /// Optional configuration action for row icons. Invoked during row rendering to allow
    /// additional icon setup on the ImGui context.
    /// </summary>
    public Action<ImGui>? RowIconConfig;

    /// <summary>
    /// Gets or sets the custom content rendering action for each row.
    /// If null, <see cref="DefaultContentGui"/> is used instead.
    /// </summary>
    public Action<ImGuiNode, PathNode, IDrawContext>? ContentGui { get; set; }

    /// <inheritdoc/>
    protected internal override GuiInputState? HandleKeyDown(ImGuiNode n, IGraphicInput input)
    {
        if (base.HandleKeyDown(n, input) is { } baseState)
        {
            return baseState;
        }

        switch (input.KeyCode)
        {
            case "Return":
                {
                    if (SelectedNode is { } pathNode)
                    {
                        OnOpenRequest(pathNode);
                    }
                    return GuiInputState.Render;
                }
            case "Delete":
                {
                    if (SelectedNode is { } pathNode)
                    {
                        DeleteRequest?.Invoke(pathNode);
                    }
                    return GuiInputState.Render;
                }
            case "F2":
                {
                    var editNode = GetSelectedImGuiNode(n);
                    if (editNode != null)
                    {
                        BeginRowEdit(editNode);
                    }
                    input.Handled = true;
                }
                return GuiInputState.Render;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    protected internal override void BeginRowEdit(ImGuiNode rowNode)
    {
        var titleStringInputNode = rowNode.GetNodeAt(0)?.FindNodeInChildren("##title_edit");
        titleStringInputNode?.BeginEdit();
    }

    /// <summary>
    /// Raises the <see cref="OpenRequest"/> event for the specified path node.
    /// If no subscriber is attached, attempts to invoke <see cref="IViewDoubleClickAction.DoubleClick"/>
    /// on the node if it implements <see cref="IViewDoubleClickAction"/>.
    /// </summary>
    /// <param name="pathNode">The path node for which the open request is raised.</param>
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
        PathNode pathNode = item.Value;

        node
        .SetColor(Color.Green)
        .InitFullWidth()
        .OnContent(() =>
        {
            var gui = node.Gui;

            var titleNode = gui.TreeNodeTitle(item, n =>
            {
                n.InitFullWidth();

                if (ContentGui is { } action)
                {
                    action(n, pathNode, this);
                }
                else
                {
                    DefaultContentGui(n, pathNode, this);
                }
            });
        })
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

        // Already handled by OnOpenRequest in HeaderlessPathTreeView
        node.InitInputDoubleClicked(n => OnOpenRequest(pathNode));
    }

    /// <summary>
    /// Renders the default content for a path node row, including icons, editable title, and tooltips.
    /// </summary>
    /// <param name="n">The ImGui node used for rendering.</param>
    /// <param name="pathNode">The path node whose content is being rendered.</param>
    /// <param name="context">The draw context for the current frame.</param>
    public virtual void DefaultContentGui(ImGuiNode n, PathNode pathNode, IDrawContext context)
    {
        var gui = n.Gui;

        if (pathNode.CustomImage is { } customImg)
        {
            gui.Image("##custom_icon", customImg)
            .InitClass("icon");
        }

        if (pathNode.Image is { } img)
        {
            gui.Image("##icon", img)
            .InitClass("icon");
        }

        if (pathNode.TextStatusIcon is { } statusIcon)
        {
            gui.Image("##status_icon", statusIcon)
            .InitClass("icon");
        }

        RowIconConfig?.Invoke(gui);

        ImGuiNode? titleNode = null;

        if (pathNode.CanEditText)
        {
            titleNode = gui.ManualStringInput("##title_edit", pathNode.Text)
            .InitClass("title_edit")
            .InitWidthRest()
            .InitVerticalAlignment(GuiAlignment.Center)
            //.SetReadonly(vNode.ReadOnly)
            .SetFontColor(pathNode.Color)
            .OnEdited(nEdit =>
            {
                pathNode.Text = nEdit.Text ?? string.Empty;
                // Reverse Verification
                nEdit.Text = pathNode.Text;
            })
            .InitInputDoubleClicked(_ => OnOpenRequest(pathNode));
        }
        else
        {
            titleNode = gui.Text("##title_text", L(pathNode.Text))
            .SetFontColor(pathNode.Color)
            .InitVerticalAlignment(GuiAlignment.Center);
        }

        if (pathNode.ToToolTipsTextL() is { } toolTips && !string.IsNullOrWhiteSpace(toolTips))
        {
            titleNode?.SetToolTips(toolTips);
        }
    }

    /// <summary>
    /// Renders the default tree node GUI for a path node, including icons and title text.
    /// Used as a fallback when no custom content GUI is provided.
    /// </summary>
    /// <param name="node">The ImGui node used for rendering.</param>
    /// <param name="vNode">The path node whose content is being rendered.</param>
    protected virtual void DefaultTreeNodeGui(ImGuiNode node, PathNode vNode)
    {
        var gui = node.Gui;

        gui.OverlayLayout()
        .InitFullHeight()
        .InitWidthRest()
        .OnContent(() =>
        {
            gui.HorizontalLayout()
            .InitFullSize()
            .OnContent(() =>
            {
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

                gui.Text("##title_text", vNode.Text)
                .SetFontColor(vNode.Color)
                .InitVerticalAlignment(GuiAlignment.Center);
            });
        });
    }
    #endregion
}