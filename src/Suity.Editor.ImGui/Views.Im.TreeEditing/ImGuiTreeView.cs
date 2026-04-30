using Suity.Editor;
using Suity.Views.Graphics;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

#region ImGuiTreeView

/// <summary>
/// Provides a base abstract class for ImGui-based tree view controls.
/// Implements menu sending and drawing context interfaces.
/// </summary>
public abstract class ImGuiTreeView : IMenuSenderContext, IDrawContext
{
    /// <summary>
    /// The default height in pixels for each row in the tree view.
    /// </summary>
    public const int DefaultRowHeight = 25;

    /// <summary>
    /// The default height in pixels for the tree view header.
    /// </summary>
    public const int DefaultHeaderHeight = 25;

    private readonly TreeViewTheme _theme;
    private RootMenuCommand? _menu;

    /// <summary>
    /// Occurs when the selection in the tree view changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Gets or sets the scroll orientation for the tree view.
    /// </summary>
    public GuiOrientation Scroll { get; set; } = GuiOrientation.Both;

    /// <summary>
    /// Gets the theme applied to the tree view.
    /// </summary>
    public TreeViewTheme Theme => _theme;

    /// <summary>
    /// Gets or sets the root menu command associated with this tree view.
    /// </summary>
    public RootMenuCommand? Menu
    {
        get => _menu;
        set
        {
            if (ReferenceEquals(_menu, value))
            {
                return;
            }

            _menu = value;

            if (_menu is not null)
            {
                EditorUtility.PrepareMenu(_menu);
            }

            OnMenuChanged();
        }
    }

    // Do not change this property, it is used in the internal menu.
    /// <summary>
    /// Gets or sets the sender object used for menu operations.
    /// </summary>
    public object? MenuSender { get; set; }

    /// <summary>
    /// Gets or sets the target object for sender operations.
    /// </summary>
    public object? SenderTarget { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the current menu selection.
    /// </summary>
    public Func<IEnumerable<object>>? MenuSelection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the mouse cursor is currently within the tree view bounds.
    /// </summary>
    public bool IsMouseIn { get; internal protected set; }

    /// <summary>
    /// Gets a value indicating whether the tree view is currently watching for selection changes.
    /// </summary>
    public abstract bool SelectionWatching { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiTreeView"/> class.
    /// </summary>
    protected ImGuiTreeView()
    {
        _theme = new TreeViewTheme();
        _theme.BuildTheme();
    }


    /// <summary>
    /// Renders the tree view GUI and returns the resulting ImGui node.
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering.</param>
    /// <param name="id">The unique identifier for this tree view.</param>
    /// <param name="config">An optional action to configure the resulting ImGui node.</param>
    /// <returns>The rendered <see cref="ImGuiNode"/>.</returns>
    public abstract ImGuiNode OnGui(ImGui gui, string id, Action<ImGuiNode>? config = null);

    /// <summary>
    /// Searches for and returns the root tree view node.
    /// </summary>
    /// <returns>The tree view <see cref="ImGuiNode"/>, or null if not found.</returns>
    public abstract ImGuiNode? FindTreeViewNode();

    /// <summary>
    /// Queues a refresh of the tree view and returns the updated node.
    /// </summary>
    /// <returns>The refreshed <see cref="ImGuiNode"/>, or null if not available.</returns>
    public abstract ImGuiNode? QueueRefresh();

    /// <summary>
    /// Executes an action without triggering selection change notifications.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public abstract void UnwatchedSelectionAction(Action action);

    /// <summary>
    /// Called when the menu associated with the tree view changes.
    /// </summary>
    protected virtual void OnMenuChanged()
    { }

    /// <summary>
    /// Raises the <see cref="SelectionChanged"/> event.
    /// </summary>
    protected void RaiseSelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

#endregion

#region ImGuiTreeView<T>

/// <summary>
/// Provides a generic base class for ImGui-based tree view controls with typed nodes.
/// </summary>
/// <typeparam name="T">The type of data represented by nodes in the tree view.</typeparam>
public abstract class ImGuiTreeView<T> : ImGuiTreeView
     where T : class
{
    internal readonly ImGuiTreeViewExternal<T> _ex;
    internal T? _beginEditValue;
    private ITreeViewTemplate<T>? _viewTemplate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiTreeView{T}"/> class.
    /// </summary>
    protected ImGuiTreeView()
    {
        _ex = ImGuiPathTreeExternal._external.CreateTreeViewEx<T>(this);

        this.MenuSelection = () => SelectedNodes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiTreeView{T}"/> class with the specified model.
    /// </summary>
    /// <param name="model">The tree model providing data for the view.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
    protected ImGuiTreeView(IImGuiTreeModel<T> model)
        : this()
    {
        TreeModel = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// Gets or sets the tree model that provides data for this view.
    /// </summary>
    public IImGuiTreeModel<T>? TreeModel
    {
        get => _ex.TreeModel;
        set => _ex.TreeModel = value;
    }

    /// <summary>
    /// Gets the visual tree data associated with this tree view.
    /// </summary>
    public VisualTreeData<T>? TreeData => _ex.TreeData;

    /// <summary>
    /// Gets or sets the template used for rendering the tree view.
    /// </summary>
    public ITreeViewTemplate<T>? ViewTemplate
    {
        get => _viewTemplate;
        set => _viewTemplate = value;
    }

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, string id, Action<ImGuiNode>? config = null)
        => _ex.OnGui(gui, id, config);

    /// <inheritdoc/>
    public override ImGuiNode? FindTreeViewNode()
        => _ex.FindTreeViewNode();

    /// <inheritdoc/>
    public override ImGuiNode? QueueRefresh()
        => _ex.QueueRefresh();

    /// <inheritdoc/>
    public override void UnwatchedSelectionAction(Action action)
        => _ex.UnwatchedSelectionAction(action);

    /// <inheritdoc/>
    public override bool SelectionWatching 
        => _ex.SelectionWatching;

    /// <inheritdoc/>
    protected override void OnMenuChanged()
    {
        base.OnMenuChanged();

        _ex.HandleMenuChanged();
    }

    /// <summary>
    /// Creates the visual tree data structure for rendering.
    /// </summary>
    /// <param name="visitor">The visitor used to traverse the tree.</param>
    /// <returns>A new <see cref="VisualTreeData{T}"/> instance configured for this tree view.</returns>
    protected internal virtual VisualTreeData<T> CreateTreeData(VisualTreeVisitor<T> visitor)
    {
        var data = _ex.CreateVisualTreeData(visitor, DefaultRowHeight);

        data.HeaderHeight = DefaultHeaderHeight;
        data.HeaderTemplate = HeaderGui;
        data.RowTemplate = RowGui;
        data.SelectionMode = ImTreeViewSelectionMode.Multiple;
        data.InitExpand = true;
        data.Width = 0;

        return data;
    }

    /// <summary>
    /// Called when entering a tree model context.
    /// </summary>
    /// <param name="model">The tree model being entered.</param>
    protected internal virtual void OnTreeModelEnter(IImGuiTreeModel<T> model)
    { }

    /// <summary>
    /// Called when exiting a tree model context.
    /// </summary>
    /// <param name="model">The tree model being exited.</param>
    protected internal virtual void OnTreeModelExit(IImGuiTreeModel<T> model)
    { }

    #region Template

    /// <summary>
    /// Renders the overall tree view GUI using the current template.
    /// </summary>
    /// <param name="treeView">The tree view node to render.</param>
    protected internal virtual void TreeViewGui(ImGuiNode treeView)
        => _viewTemplate?.TreeViewGui(treeView);

    /// <summary>
    /// Renders the header GUI for the tree view using the current template.
    /// </summary>
    /// <param name="node">The header node to render.</param>
    protected internal virtual void HeaderGui(ImGuiNode node)
        => _viewTemplate?.HeaderGui(node, _ex.TreeData?.HeaderHeight);

    /// <summary>
    /// Renders a single row in the tree view using the current template.
    /// </summary>
    /// <param name="node">The row node to render.</param>
    /// <param name="item">The visual tree node data for this row.</param>
    protected internal virtual void RowGui(ImGuiNode node, VisualTreeNode<T> item)
        => _viewTemplate?.RowGui(node, item);

    /// <summary>
    /// Begins editing mode for a row using the current template.
    /// </summary>
    /// <param name="node">The row node to begin editing.</param>
    protected internal virtual void BeginRowEdit(ImGuiNode node)
        => _viewTemplate?.BeginRowEdit(node);

    #endregion

    #region Selection

    /// <summary>
    /// Gets the collection of currently selected nodes.
    /// </summary>
    public IEnumerable<T> SelectedNodes =>
        _ex.TreeData?.SelectedNodesT.Select(o => o.Value).OfType<T>() ?? [];

    /// <summary>
    /// Gets the currently selected single node, or null if no node is selected.
    /// </summary>
    public T? SelectedNode => (_ex.TreeData?.SelectedNode as VisualTreeNode<T>)?.Value;

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    /// <param name="notify">If true, raises the selection changed notification.</param>
    public void ClearSelection(bool notify = true)
    {
        UnwatchedSelectionAction(() =>
        {
            _ex.TreeData?.ClearSelection();
        });

        if (notify)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Selects a specific node in the tree view.
    /// </summary>
    /// <param name="node">The node to select.</param>
    /// <param name="append">If true, adds to the existing selection; otherwise replaces it.</param>
    /// <param name="notify">If true, raises the selection changed notification.</param>
    /// <returns>The selected <see cref="VisualTreeNode"/>, or null if not found.</returns>
    public VisualTreeNode? SelectNode(T node, bool append = false, bool notify = true)
        => _ex.SelectNode(node, append, notify);

    /// <summary>
    /// Selects multiple nodes in the tree view.
    /// </summary>
    /// <param name="nodes">The collection of nodes to select.</param>
    /// <param name="notify">If true, raises the selection changed notification.</param>
    public void SelectNodes(IEnumerable<T> nodes, bool notify = true)
        => _ex.SelectNodes(nodes, notify);

    /// <summary>
    /// Called when the selection in the tree view changes.
    /// </summary>
    protected internal virtual void OnSelectionChanged()
    {
        if (SelectionWatching)
        {
            return;
        }

        RaiseSelectionChanged();
    }

    /// <summary>
    /// Gets the ImGui node corresponding to the currently selected item.
    /// </summary>
    /// <param name="treeViewNode">The root tree view node to search within.</param>
    /// <returns>The selected <see cref="ImGuiNode"/>, or null if none is selected.</returns>
    protected ImGuiNode? GetSelectedImGuiNode(ImGuiNode treeViewNode)
    {
        var selected = _ex.TreeData?.SelectedNode as VisualTreeNode<T>;
        if (selected is { })
        {
            var selNode = treeViewNode.GetChildNode(selected.Id);
            if (selNode is { })
            {
                return selNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the ImGui nodes corresponding to all currently selected items.
    /// </summary>
    /// <param name="treeViewNode">The root tree view node to search within.</param>
    /// <returns>A collection of selected <see cref="ImGuiNode"/> instances.</returns>
    protected IEnumerable<ImGuiNode> GetSelectedImGuiNodes(ImGuiNode treeViewNode)
    {
        if (treeViewNode is null)
        {
            return [];
        }

        var treeNodes = _ex.TreeData?.SelectedNodesT ?? [];

        return treeNodes.Select(o => treeViewNode.GetChildNode(o.Id)).OfType<ImGuiNode>();
    }

    #endregion

    /// <summary>
    /// Scrolls the tree view to make the specified node visible using the ImGui context.
    /// </summary>
    /// <param name="gui">The ImGui instance used for scrolling.</param>
    /// <param name="treeNode">The tree node to scroll to.</param>
    public void ScrollToPosition(ImGui gui, VisualTreeNode treeNode)
        => _ex.ScrollToPosition(gui, treeNode);

    /// <summary>
    /// Scrolls the tree view to make the specified node visible using a tree view node.
    /// </summary>
    /// <param name="treeView">The tree view node to use for scrolling.</param>
    /// <param name="treeNode">The tree node to scroll to.</param>
    /// <returns>True if the scroll operation was successful; otherwise, false.</returns>
    public bool ScrollToPosition(ImGuiNode treeView, VisualTreeNode treeNode)
        => _ex.ScrollToPosition(treeView, treeNode);

    /// <summary>
    /// Initiates inline editing mode for the specified node.
    /// </summary>
    /// <param name="vNode">The node to begin editing.</param>
    public void BeginEdit(T vNode) => _beginEditValue = vNode;

    /// <summary>
    /// Handles key down input events for the tree view.
    /// </summary>
    /// <param name="node">The tree view node receiving the input.</param>
    /// <param name="input">The graphic input containing the key event.</param>
    /// <returns>A <see cref="GuiInputState"/> indicating the result, or null if unhandled.</returns>
    protected internal virtual GuiInputState? HandleKeyDown(ImGuiNode node, IGraphicInput input)
    {
        switch (input.KeyCode)
        {
            case "Up":
                input.Handled = true;
                return HandleMoveUp(node);

            case "Down":
                input.Handled = true;
                return HandleMoveDown(node);

            default:
                return null;
        }
    }

    /// <summary>
    /// Handles moving the selection up in the tree view.
    /// </summary>
    /// <param name="treeView">The tree view node.</param>
    /// <returns>The resulting <see cref="GuiInputState"/>.</returns>
    protected GuiInputState HandleMoveUp(ImGuiNode treeView)
        => _ex.HandleMoveUp(treeView);

    /// <summary>
    /// Handles moving the selection down in the tree view.
    /// </summary>
    /// <param name="treeView">The tree view node.</param>
    /// <returns>The resulting <see cref="GuiInputState"/>.</returns>
    protected GuiInputState HandleMoveDown(ImGuiNode treeView)
        => _ex.HandleMoveDown(treeView);

    /// <summary>
    /// Finds the ImGui node corresponding to the specified data node.
    /// </summary>
    /// <param name="gui">The ImGui instance to search within.</param>
    /// <param name="vNode">The data node to find.</param>
    /// <returns>The corresponding <see cref="ImGuiNode"/>, or null if not found.</returns>
    protected internal ImGuiNode? FindNode(ImGui gui, T vNode)
    {
        var treeNode = _ex.TreeData?.EnsureNode(vNode);
        if (treeNode is { } && treeNode.NodePath is { } path)
        {
            return gui.FindNode(path);
        }

        return null;
    }

    /// <summary>
    /// Gets the child ImGui node corresponding to the specified data node.
    /// </summary>
    /// <param name="treeViewNode">The parent tree view node.</param>
    /// <param name="vNode">The data node to find.</param>
    /// <returns>The corresponding <see cref="ImGuiNode"/>, or null if not found.</returns>
    protected ImGuiNode? GetNode(ImGuiNode treeViewNode, T vNode)
    {
        var treeNode = _ex.TreeData?.EnsureNode(vNode);
        if (treeNode is { })
        {
            return treeViewNode.GetChildNode(treeNode.Id);
        }

        return null;
    }
}

#endregion
