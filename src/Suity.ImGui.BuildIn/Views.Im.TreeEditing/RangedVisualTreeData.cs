using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Represents a ranged visual tree data structure that manages tree node pooling, selection, and visualization.
/// </summary>
/// <typeparam name="T">The type of values stored in the tree nodes. Must be a reference type.</typeparam>
public class RangedVisualTreeData<T> : VisualTreeData<T>
    where T : class
{
    private class VListData : RangedVisualListData<VisualTreeNode<T>>
    {
        private readonly RangedVisualTreeData<T> _parent;

        public VListData(
            RangedVisualTreeData<T> parent,
            LengthGetter<VisualTreeNode<T>> heightGetter,
            float defaultLen = 16)
            : base(heightGetter, defaultLen)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        protected override void SetIndex(VisualTreeNode<T> value, int index)
        {
            value.Index = index;
        }

        protected override string GetId(VisualTreeNode<T> value, int index)
        {
            return value.Id;
        }
    }

    private readonly VListData _listData;

    private readonly Dictionary<string, VisualTreeNode<T>> _pool = [];

    private readonly Dictionary<string, VisualTreeNode<T>> _dic = [];
    private readonly Dictionary<string, VisualTreeNode<T>> _creation = [];

    private VisualTreeNode<T>? _selectedNode;
    private readonly HashSet<VisualTreeNode<T>> _selection = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RangedVisualTreeData{T}"/> class.
    /// </summary>
    /// <param name="visitor">The visitor used to traverse tree nodes.</param>
    /// <param name="defaultHeight">The default height for tree nodes.</param>
    public RangedVisualTreeData(
        VisualTreeVisitor<T> visitor,
        float defaultHeight = RangedVisualListData<VisualTreeNode<T>>.DefaultItemHeight)
        : base(visitor)
    {
        _listData = new(this, n => n.Height, defaultHeight);
    }

    /// <inheritdoc/>
    public override ContentTemplate? HeaderTemplate
    {
        get => _listData.HeaderTemplate;
        set => _listData.HeaderTemplate = value;
    }

    /// <inheritdoc/>
    public override ContentTemplate<VisualTreeNode<T>>? RowTemplate
    {
        get => _listData.RowTemplate;
        set => _listData.RowTemplate = value;
    }

    /// <inheritdoc/>
    public override float Spacing
    {
        get => _listData.Spacing;
        set => _listData.Spacing = value;
    }

    /// <inheritdoc/>
    public override float? Width
    {
        get => _listData.Width;
        set => _listData.Width = value;
    }

    /// <inheritdoc/>
    public override float? HeaderHeight
    {
        get => _listData.HeaderHeight;
        set => _listData.HeaderHeight = value;
    }

    #region ImVirtualTreeData

    /// <inheritdoc/>
    public override VisualListData ListData => _listData;

    /// <inheritdoc/>
    public override VisualTreeNode? SelectedNode => _selectedNode;

    /// <inheritdoc/>
    public override IEnumerable<VisualTreeNode> SelectedNodes => _selection;

    /// <inheritdoc/>
    public override VisualTreeNode<T>? SelectedNodeT => _selectedNode;

    /// <inheritdoc/>
    public override IEnumerable<VisualTreeNode<T>> SelectedNodesT => _selection;

    /// <inheritdoc/>
    public override void Refresh()
    {
        RefreshRequired = false;

        _listData.Clear();
        _creation.Clear();

        // Populate nodes from root children recursively
        var values = Visitor.GetChildNodes() ?? [];

        foreach (var value in values)
        {
            if (value is { })
            {
                var node = PopulateNode(value);
                if (node != null && node.Value is null)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        // Identify and pool nodes that no longer exist in the data model
        HashSet<VisualTreeNode<T>>? removes = null;
        foreach (var pair in _dic)
        {
            if (!_creation.ContainsKey(pair.Key))
            {
                (removes ??= []).Add(pair.Value);
            }
        }

        // Move removed nodes to pool for reuse and clean up their state
        if (removes is { })
        {
            foreach (var remove in removes)
            {
                _dic.Remove(remove.Id);
                _selection.Remove(remove);

                if (_selectedNode == remove)
                {
                    _selectedNode = null;
                }

                remove.Value = null!;
                remove.ParentNode = null;
                remove._pooled = true;
                _pool[remove.Id] = remove;
            }
        }

        _creation.Clear();
    }

    /// <inheritdoc/>
    public override void CleanUpPool()
    {
        _pool.Clear();
    }

    /// <inheritdoc/>
    public override VisualTreeNode GetNode(string id)
    {
        return _dic.GetValueSafe(id);
    }

    #endregion

    #region VirtualTreeData Selection

    /// <inheritdoc/>
    public override void AppendSelection(VisualTreeNode node)
    {
        if (node is VisualTreeNode<T> tNode)
        {
            if (tNode._pooled)
            {
                throw new InvalidOperationException();
            }

            if (_selection.Add(tNode))
            {
                tNode.IsSelected = true;

                if (_selection.Count == 1)
                {
                    _selectedNode = tNode;
                }

                OnSelectionChanged();
            }
            else
            {
                _selectedNode = tNode;
            }
        }
    }

    /// <inheritdoc/>
    public override void SetSelection(VisualTreeNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (node._pooled)
        {
            throw new InvalidOperationException();
        }

        if (_selection.Count == 1 && _selection.Contains(node))
        {
            return;
        }

        foreach (var sel in _selection)
        {
            sel.IsSelected = false;
        }
        _selection.Clear();

        if (node is VisualTreeNode<T> tNode && _selection.Add(tNode))
        {
            tNode.IsSelected = true;
            _selectedNode = tNode;
        }

        OnSelectionChanged();
    }

    /// <inheritdoc/>
    public override void SetSelections(IEnumerable<VisualTreeNode> nodes)
    {
        if (nodes.Any(o => o._pooled))
        {
            throw new InvalidOperationException();
        }

        if (_selection.Count == nodes.Count() && nodes.All(o => _selection.Contains(o)))
        {
            return;
        }

        foreach (var sel in _selection)
        {
            sel.IsSelected = false;
        }
        _selection.Clear();

        var selections = nodes.OfType<VisualTreeNode<T>>().Where(o => o.TreeModel == this);
        //VirtualTreeNode<T>? last = null;
        foreach (var sel in selections)
        {
            //last = sel;

            _selection.Add(sel);
            sel.IsSelected = true;
        }

        OnSelectionChanged();
    }

    /// <inheritdoc/>
    public override void SetSelections(int fromIndex, int toIndex)
    {
        if (fromIndex > toIndex)
        {
            (toIndex, fromIndex) = (fromIndex, toIndex);
        }

        List<VisualTreeNode<T>> list = [];

        for (int i = fromIndex; i <= toIndex; i++)
        {
            var value = _listData[i];
            list.Add(value);
        }

        SetSelections(list);
    }

    /// <inheritdoc/>
    public override void ToggleSelection(VisualTreeNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (node._pooled)
        {
            throw new InvalidOperationException();
        }

        if (node is VisualTreeNode<T> tNode)
        {
            if (_selection.Contains(tNode))
            {
                _selection.Remove(tNode);
                tNode.IsSelected = false;
                _selectedNode = _selection.FirstOrDefault();
            }
            else
            {
                _selection.Add(tNode);
                tNode.IsSelected = true;
                _selectedNode = tNode;
            }

            OnSelectionChanged();
        }
    }

    /// <inheritdoc/>
    public override void ClearSelection()
    {
        if (_selection.Count == 0)
        {
            return;
        }

        foreach (var node in _selection)
        {
            node.IsSelected = false;
        }

        _selectedNode = null;
        _selection.Clear();

        OnSelectionChanged();
    }

    #endregion

    #region VirtualTreeData<T>

    /// <inheritdoc/>
    public override VisualTreeNode<T> EnsureNode(T value)
    {
        string id = Visitor.GetId(value);

        var node = _dic.GetValueSafe(id);
        if (node is { })
        {
            return node;
        }

        T? parentValue = Visitor.GetParent(value);
        if (parentValue is { })
        {
            var parent = EnsureNode(parentValue);

            node = new(id, this, value)
            {
                Expanded = Visitor.GetIsExpanded(value) ?? InitExpand,
                ParentNode = parent,
                Indent = parent.Indent + 1,
                Height = Visitor.GetHeight(value) ?? _listData.DefaultHeight,
            };
        }
        else
        {
            node = new(id, this, value)
            {
                Expanded = Visitor.GetIsExpanded(value) ?? InitExpand,
                ParentNode = null,
                Indent = 0,
                Height = Visitor.GetHeight(value) ?? _listData.DefaultHeight,
            };
        }

        _dic[id] = node;

        return node;
    }

    #endregion

    private VisualTreeNode<T>? PopulateNode(T value, VisualTreeNode<T>? parent = null, int indent = 0)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        string id = Visitor.GetId(value);
        if (string.IsNullOrEmpty(id))
        {
            Logs.LogWarning($"Id is null or empty for {value.GetType().Name}");
            return null;
        }

        if (_creation.ContainsKey(id))
        {
            Logs.LogWarning($"Id {id} already exists in creation for {value.GetType().Name}");
            return null;
        }

        // Try to get existing node from dictionary, then from pool, or create new one
        VisualTreeNode<T>? node = _dic.GetValueSafe(id);

        node ??= _pool.RemoveAndGet(id);
        node ??= new(id, this, value)
        {
            Expanded = Visitor.GetIsExpanded(value) ?? InitExpand,
            ParentNode = parent,
            Indent = indent,
            Height = Visitor.GetHeight(value) ?? _listData.DefaultHeight,
        };

        // Update node properties regardless of source
        node.Value = value;
        node.ParentNode = parent;
        node.Indent = indent;
        node._pooled = false;

        var height = Visitor.GetHeight(value);
        if (height.HasValue)
        {
            node.Height = height.Value;
        }

        var expanded = Visitor.GetIsExpanded(value);
        if (expanded.HasValue)
        {
            node.Expanded = expanded.Value;
        }

        node.IsSelected = _selection.Contains(node);

        _creation[node.Id] = node;
        _dic[node.Id] = node;

        int totalLen = _listData.AppendItem(node);
        node.Position = totalLen;

        node.CanExpand = Visitor.GetCanExpand(value);

        // Recursively populate child nodes if expanded and expandable
        if (node.Expanded)
        {
            indent++;

            var childValues = Visitor.GetChildNodes(value) ?? [];
            if (node.CanExpand)
            {
                foreach (var childValue in childValues!)
                {
                    var childNode = PopulateNode(childValue, node, indent);
                    if (childNode != null && childNode.Value is null)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        return node;
    }
}