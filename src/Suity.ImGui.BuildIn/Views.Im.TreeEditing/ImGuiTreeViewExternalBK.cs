using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides an ImGui-based external backend implementation for tree views, managing tree model lifecycle, selection, and rendering.
/// </summary>
/// <typeparam name="T">The type of values stored in the tree nodes. Must be a reference type.</typeparam>
internal class ImGuiTreeViewExternalBK<T> : ImGuiTreeViewExternal<T>
    where T : class
{
    private readonly ImGuiTreeView<T> _treeView;

    protected internal readonly ImGuiNodeRef _guiNodeRef = new();
    protected ImGui? _gui;

    private IImGuiTreeModel<T>? _model;
    private VisualTreeData<T>? _treeData;
    private bool _selectionWatching;

    private Action<ImGui>? _postAction;

    private bool _menuChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiTreeViewExternalBK{T}"/> class.
    /// </summary>
    /// <param name="treeView">The tree view to provide external backend for.</param>
    public ImGuiTreeViewExternalBK(ImGuiTreeView<T> treeView)
    {
        _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
    }

    /// <inheritdoc/>
    public override IImGuiTreeModel<T>? TreeModel
    {
        get => _model;
        set
        {
            if (ReferenceEquals(_model, value))
            {
                return;
            }

            if (_treeData != null)
            {
                _treeData.SelectionChanged -= _data_SelectionChanged;
                _treeData = null;
            }
            if (_model != null)
            {
                _model.TreeChanged -= _model_TreeChanged;

                _treeView.OnTreeModelExit(_model);
                // _model.TreeData = null;
            }

            _model = value;

            if (_model != null)
            {
                _model.TreeChanged += _model_TreeChanged;

                _treeData = _model.TreeData;
                if (_treeData is null || _treeData.Visitor != _model)
                {
                    _treeData = _treeView.CreateTreeData(_model);
                    if (_treeData is null)
                    {
                        throw new NullReferenceException();
                    }

                    if (_treeData.Visitor != _model)
                    {
                        throw new InvalidOperationException();
                    }

                    _model.TreeData = _treeData;
                }

                _treeData.SelectionChanged += _data_SelectionChanged;

                _treeView.OnTreeModelEnter(_model);
            }
        }
    }

    /// <inheritdoc/>
    public override VisualTreeData<T>? TreeData => _treeData;

    /// <inheritdoc/>
    public override bool SelectionWatching => _selectionWatching;

    /// <inheritdoc/>
    public override VisualTreeData<T> CreateVisualTreeData(VisualTreeVisitor<T> visitor, int defaultHeight)
    {
        return new RangedVisualTreeData<T>(visitor, defaultHeight);
    }

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, string id, Action<ImGuiNode>? config)
    {
        if (!ReferenceEquals(_gui, gui))
        {
            _gui = gui;
            _menuChanged = true;
        }

        if (_menuChanged && _treeView.Menu is { } menu)
        {
            (gui.Context as IGraphicContextMenu)?.RegisterContextMenu(menu);
        }

        var treeView = _guiNodeRef.Node = gui.TreeView(id, _treeView.Scroll)
        .InitTheme(_treeView.Theme)
        .InitInputMouseUp((n, btn) =>
        {
            if (btn == GuiMouseButtons.Right)
            {
                _treeView.OnSelectionChanged();

                IEnumerable<object> sel = _treeView.MenuSelection?.Invoke() ?? _treeData?.SelectedNodesT ?? (IEnumerable<object>)[];
                if (/*sel.Any() && */_treeView.Menu is { } menu)
                {
                    menu.ApplySender(_treeView.MenuSender ?? _treeView);
                    //menu.PopUp(sel.Count(), new Type[] { typeof(object) }, typeof(object));
                    (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu, sel);
                }
            }
            else if (btn == GuiMouseButtons.Left)
            {
                _treeView.OnSelectionChanged();
            }

            return GuiInputState.None;
        });

        _treeView.IsMouseIn = treeView.IsMouseIn;

        treeView.InitKeyDownInput(_treeView.HandleKeyDown);
        //.OnKeyDown((n, input) => HandleKeyDown(n, input));

        // Order is fixed
        _treeView.TreeViewGui(treeView);
        config?.Invoke(treeView);

        // Execute Layout once to avoid issues when minimizing/maximizing
        treeView.Layout();

        if (_treeData is { } treeData)
        {
            treeView.SetTreeNodeData(treeData);
        }
        else
        {
            treeView.UnsetTreeNodeData();
        }

        if (_treeView._beginEditValue is { } beginEditValue)
        {
            _treeView._beginEditValue = null;
            var beginEditNode = _treeView.FindNode(gui, beginEditValue);

            if (beginEditNode is { })
            {
                _treeView.BeginRowEdit(beginEditNode);
            }
        }

        if (_postAction is { } postAction)
        {
            _postAction = null;
            postAction(gui);
        }

        return treeView;
    }

    /// <inheritdoc/>
    public override ImGuiNode? FindTreeViewNode() => _guiNodeRef.Node;

    /// <inheritdoc/>
    public override VisualTreeNode? SelectNode(T node, bool append, bool notify)
    {
        if (!(_treeData is { } treeData))
        {
            return null;
        }

        treeData.CheckRefresh();

        //TODO: Use Ensure to prevent nodes from being collapsed.
        VisualTreeNode<T>? viewNode = treeData.EnsureNode(node);
        if (viewNode is null)
        {
            return null;
        }

        UnwatchedSelectionAction(() =>
        {
            if (!append)
            {
                treeData.ClearSelection();
            }

            treeData.AppendSelection(viewNode);

            var parentNode = viewNode.ParentNode;
            while (parentNode != null)
            {
                parentNode.Expanded = true;
                parentNode = parentNode.ParentNode;
            }
        });

        if (notify)
        {
            _treeView.OnSelectionChanged();
        }

        //TODO: Implement view scrolling to node aTreeView.EnsureVisible(viewNode);
        _postAction = gui =>
        {
            ScrollToPosition(gui, viewNode);
        };

        return viewNode;
    }

    /// <inheritdoc/>
    public override void SelectNodes(IEnumerable<T> nodes, bool notify)
    {
        if (!(_treeData is { } treeData))
        {
            return;
        }

        treeData.CheckRefresh();
        treeData.ClearSelection();

        VisualTreeNode<T>? lastNode = null;

        UnwatchedSelectionAction(() =>
        {
            foreach (var node in nodes)
            {
                VisualTreeNode<T>? viewNode = treeData.EnsureNode(node);
                if (viewNode is null)
                {
                    continue;
                }

                treeData.AppendSelection(viewNode);

                var parentNode = viewNode.ParentNode;
                while (parentNode != null)
                {
                    parentNode.Expanded = true;
                    parentNode = parentNode.ParentNode;
                }

                lastNode = viewNode;
            }
        });

        if (notify)
        {
            _treeView.OnSelectionChanged();
        }

        if (lastNode is { })
        {
            _postAction = gui =>
            {
                ScrollToPosition(gui, lastNode);
            };
        }
    }

    /// <inheritdoc/>
    public override void UnwatchedSelectionAction(Action action)
    {
        if (_selectionWatching)
        {
            action();
            return;
        }

        try
        {
            _selectionWatching = true;

            action();

            if (!_selectionWatching)
            {
                throw new InvalidOperationException();
            }
        }
        finally
        {
            _selectionWatching = false;
        }
    }

    /// <inheritdoc/>
    public override bool ScrollToPosition(ImGui gui, VisualTreeNode treeNode)
    {
        var treeView = _guiNodeRef.Node;
        if (treeView is null)
        {
            return false;
        }

        if (ScrollToPosition(treeView, treeNode))
        {
            treeView.QueueRefresh();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool ScrollToPosition(ImGuiNode treeView, VisualTreeNode treeNode)
    {
        _treeData?.CheckRefresh();

        var scroll = treeView.GetValue<GuiScrollableValue>();
        if (scroll is null)
        {
            return false;
        }

        var tvRect = treeView.Rect;

        float x = tvRect.X;
        float y = tvRect.Y + treeNode.Position;

        var rect = new RectangleF(x, y, tvRect.Width, treeNode.Height);

        return treeView.ScrollToPositionY(rect, false);
    }

    /// <inheritdoc/>
    public override GuiInputState HandleMoveUp(ImGuiNode treeView)
    {
        if (!(_treeView.TreeData is { } treeData))
        {
            return GuiInputState.None;
        }

        var vtNode = treeData.SelectedNode;
        if (vtNode is null || vtNode.Index <= 0)
        {
            return GuiInputState.None;
        }

        var prevNode = treeData.ListData.GetItemAt(vtNode.Index - 1) as VisualTreeNode;

        if (prevNode is { })
        {
            treeData.SetSelection(prevNode);
            UpdateSelections();
            bool scrolled = ScrollToPosition(treeView, prevNode);
            _treeView.OnSelectionChanged();
            //treeView.Gui.QueueStyleRefresh();

            return scrolled ? GuiInputState.FullSync : GuiInputState.Render;
        }

        return GuiInputState.None;
    }

    /// <inheritdoc/>
    public override GuiInputState HandleMoveDown(ImGuiNode treeView)
    {
        if (!(_treeView.TreeData is { } treeData))
        {
            return GuiInputState.None;
        }

        var vtNode = treeData.SelectedNode;
        if (vtNode is null || vtNode.Index >= treeData.ListData.Count - 1)
        {
            return GuiInputState.None;
        }

        var nextNode = treeData.ListData.GetItemAt(vtNode.Index + 1) as VisualTreeNode;

        if (nextNode is { })
        {
            treeData.SetSelection(nextNode);
            UpdateSelections();
            bool scrolled = ScrollToPosition(treeView, nextNode);
            _treeView.OnSelectionChanged();
            //treeView.Gui.QueueRefresh();

            return scrolled ? GuiInputState.FullSync : GuiInputState.Render;
        }

        return GuiInputState.None;
    }

    /// <inheritdoc/>
    public override ImGuiNode? QueueRefresh()
    {
        var node = FindTreeViewNode();
        if (node is not null)
        {
            node.MarkRenderDirty();
            node.QueueRefresh();
        }

        return node;
    }

    /// <inheritdoc/>
    public override void HandleMenuChanged()
    {
        _menuChanged = true;
    }

    private void UpdateSelections()
    {
        var node = FindTreeViewNode();
        if (node is not null)
        {
            GuiTreeViewExtensions.UpdateTreeNodeSelections(node);
        }
    }

    private void _data_SelectionChanged(object sender, EventArgs e)
    {
        //OnSelectionChanged();
    }

    private void _model_TreeChanged(object sender, EventArgs e)
    {
        QueueRefresh();
    }
}