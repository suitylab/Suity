using Suity.Drawing;
using Suity.Editor.VirtualTree.Actions;
using Suity.Editor.VirtualTree.Adapters;
using Suity.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A virtual tree node that represents an <see cref="IList"/>-based collection.
/// Provides list item management including insertion, removal, drag-and-drop,
/// and element type resolution through the <see cref="IListAdapter"/>.
/// </summary>
public class IListVirtualNode : VirtualNode, IVirtualNodeListOperation
{
    private readonly IListAdapter _adapter;
    private IList _list;

    /// <summary>
    /// Initializes a new instance of the <see cref="IListVirtualNode"/> class with an empty adapter.
    /// </summary>
    public IListVirtualNode()
    {
        _adapter = EmptyIListAdapter.Instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IListVirtualNode"/> class with the specified adapter.
    /// </summary>
    /// <param name="adapter">The list adapter to use for list operations. Must not be null.</param>
    public IListVirtualNode(IListAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _adapter.SetupNode(this);
    }

    /// <summary>
    /// Gets the list adapter used by this node.
    /// </summary>
    internal IListAdapter Adapter => _adapter;

    /// <inheritdoc/>
    public override object DisplayedValue => _list;

    #region Editor

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        base.OnGetValue();

        _list = GetIList();
        if (_list is null)
        {
            ClearContent();
        }
        else
        {
            if (IsContentInitialized)
            {
                UpdateElementNodes(_list);
            }
            else
            {
                ClearContent();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnAdded()
    {
        _adapter?.OnAdded();
    }

    /// <inheritdoc/>
    protected override void OnRemoved()
    {
        _adapter?.OnRemoved();
    }

    /// <inheritdoc/>
    public override void HandleNodeAction()
    {
        _adapter?.HandleNodeAction();
    }

    /// <inheritdoc/>
    public override void HandleKeyDown(int key)
    {
        _adapter?.HandleKeyDown(key);
    }

    /// <inheritdoc/>
    public override string GetContextMenuKey()
    {
        return VirtualNode.ContextMenu_ArrayOwner;
    }

    /// <inheritdoc/>
    public override string GetChildContextMenuKey(VirtualNode childNode)
    {
        return VirtualNode.ContextMenu_ArrayMember;
    }

    /// <inheritdoc/>
    public override bool CanDropIn(VirtualNode node)
    {
        if (ReadOnly)
        {
            return false;
        }

        // Cannot pass null value
        if (node.DisplayedValue is null)
        {
            return false;
        }

        // Allow moving within own list
        if (node.Parent == this)
        {
            return true;
        }

        if (_adapter != null)
        {
            return _adapter.CanDropIn(node.DisplayedValue);
        }
        else
        {
            Type elementType = ReflectionHelper.GetIListElementType(EditedType);
            return elementType.IsAssignableFrom(node.EditedType);
        }
    }

    /// <inheritdoc/>
    public override bool CanDropInExternal(object obj)
    {
        if (ReadOnly)
        {
            return false;
        }

        if (obj is null)
        {
            return false;
        }

        if (_adapter != null)
        {
            return _adapter.CanDropIn(obj);
        }
        else
        {
            Type elementType = ReflectionHelper.GetIListElementType(EditedType);
            return elementType.IsAssignableFrom(obj.GetType());
        }
    }

    /// <inheritdoc/>
    public override object DropInConvert(object obj)
    {
        if (ReadOnly)
        {
            return null;
        }

        return _adapter?.DropInConvert(obj);
    }

    #endregion

    #region Display

    /// <inheritdoc/>
    protected override string GetText()
    {
        return _adapter?.Text ?? base.GetText();
    }

    /// <inheritdoc/>
    protected override void SetText(string value)
    {
        if (_adapter != null)
        {
            _adapter.Text = value;
        }
        else
        {
            base.SetText(value);
        }
    }

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        if (_list?.Count > 0)
        {
            return TextStatus.Normal;
        }
        else
        {
            return Styles?.GetEmptyListGray() == true ? TextStatus.Disabled : TextStatus.Normal;
        }
    }

    /// <inheritdoc/>
    protected override string GetDescription()
    {
        return _adapter?.Description ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override string GetPreviewText()
    {
        string customText = GetCustomPreviewText();
        if (customText != null)
        {
            return customText;
        }

        return _adapter?.PreviewText ?? base.GetPreviewText();
    }

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        if (_adapter != null)
        {
            _adapter.PreviewText = value;
        }
        else
        {
            base.SetPreviewText(value);
        }
    }

    /// <inheritdoc/>
    protected override ImageDef GetMainIcon()
    {
        return _adapter?.Icon ?? base.GetMainIcon();
    }

    /// <inheritdoc/>
    protected override ImageDef GetPreviewIcon()
    {
        var customIcon = GetCustomPreviewIcon();
        if (customIcon != null)
        {
            return customIcon;
        }

        if (_adapter != null)
        {
            return _adapter.PreviewIcon ?? base.GetPreviewIcon();
        }
        else
        {
            return base.GetPreviewIcon();
        }
    }

    /// <inheritdoc/>
    protected override string GetFieldDisplayName()
    {
        return _adapter?.FieldDisplayName ?? base.GetFieldDisplayName();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditText()
    {
        return _adapter?.CanEditText ?? base.GetCanEditText();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditPreviewText()
    {
        return _adapter?.CanEditPreviewText ?? base.GetCanEditPreviewText();
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates the child element nodes to match the current state of the specified list.
    /// Creates, replaces, or removes nodes as needed to synchronize with the list's item count.
    /// </summary>
    /// <param name="list">The list to synchronize child nodes with.</param>
    protected void UpdateElementNodes(IList list)
    {
        Type elementType = GetElementType();
        Type reflectedElementType = ReflectionHelper.ReflectDynamicType(elementType, ReflectionHelper.GetIListElementType(list.GetType()));

        var model = FindModel();

        for (int i = 0; i < list.Count; i++)
        {
            VirtualNode elementNode;

            if (i < this.Nodes.Count)
            {
                elementNode = Nodes[i];
                if (elementNode.EditedType != reflectedElementType)
                {
                    VirtualNode oldNode = elementNode;
                    elementNode = model.CreateNode(reflectedElementType, this);

                    this.Nodes.Add(elementNode, oldNode);
                    this.Nodes.Remove(oldNode);

                    model.ConfigureNode(elementNode);
                }
            }
            else
            {
                elementNode = model.CreateNode(reflectedElementType, this);
                this.Nodes.Add(elementNode);
                model.ConfigureNode(elementNode);
            }

            elementNode.PropertyName = string.Format("[{0}]", i);
            elementNode.Getter = this.CreateElementValueGetter(i);
            elementNode.Setter = this.CreateElementValueSetter(i);

            elementNode.PerformGetValue();
        }

        if (this.Nodes.Count > list.Count)
        {
            for (int i = this.Nodes.Count - 1; i >= list.Count; i--)
            {
                VirtualNode oldNode = Nodes[i];
                Nodes.RemoveAt(i);
                oldNode.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates a getter function for retrieving the value at the specified list index.
    /// </summary>
    /// <param name="index">The index of the list element.</param>
    /// <returns>A function that returns the value at the specified index.</returns>
    protected Func<object> CreateElementValueGetter(int index)
    {
        return () =>
        {
            IList list = this.DisplayedIList;
            return list?[index];
        };
    }

    /// <summary>
    /// Creates a setter action for updating the value at the specified list index.
    /// </summary>
    /// <param name="index">The index of the list element.</param>
    /// <returns>An action that sets the value at the specified index.</returns>
    protected Action<object> CreateElementValueSetter(int index)
    {
        return (object value) =>
        {
            var model = FindModel();
            if (model != null)
            {
                model.HandleSetterAction(new IListResolverSetterAction(this, index, value));
            }
            else
            {
                IList list = this.DisplayedIList;
                if (list != null)
                {
                    list[index] = value;
                }
            }
        };
    }

    #endregion

    /// <summary>
    /// Gets the element type of the list, either from the adapter or by reflection.
    /// </summary>
    /// <returns>The type of elements contained in the list.</returns>
    public Type GetElementType()
    {
        if (_adapter != null)
        {
            return _adapter.GetListElementType();
        }
        else
        {
            return ReflectionHelper.GetIListElementType(this.EditedType);
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="IList"/> instance, resolved through the adapter or directly from the displayed value.
    /// </summary>
    public IList DisplayedIList
    {
        get { return _adapter != null ? _adapter.ResolveIList() : DisplayedValue as IList; }
    }

    /// <summary>
    /// Resolves and returns the <see cref="IList"/> instance from the current value or adapter.
    /// </summary>
    /// <returns>The resolved IList instance, or null if not available.</returns>
    protected IList GetIList()
    {
        return _adapter != null ? _adapter.ResolveIList() : GetValue() as IList;
    }

    #region IVirtualNodeListOperation

    /// <inheritdoc/>
    public Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        if (ReadOnly)
        {
            return null;
        }

        if (_adapter != null)
        {
            return Task.FromResult<object>(_adapter.CreateNewItem());
        }
        else
        {
            IList list = this.DisplayedIList;
            Type elementType = GetElementType();
            Type reflectedElementType = ReflectionHelper.ReflectDynamicType(elementType, ReflectionHelper.GetIListElementType(list.GetType()));

            return Task.FromResult<object>(reflectedElementType.CreateInstanceOf());
        }
    }

    /// <inheritdoc/>
    public async Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        object obj = await GuiCreateItemAsync(typeHint);
        if (obj != null)
        {
            return [obj];
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public bool CanAddItem(object value)
    {
        if (ReadOnly)
        {
            return false;
        }

        IList list = this.DisplayedIList;
        if (list is null)
        {
            return false;
        }

        Type elementType = GetElementType();
        Type reflectedElementType = ReflectionHelper.ReflectDynamicType(elementType, ReflectionHelper.GetIListElementType(list.GetType()));

        return value != null && reflectedElementType.IsInstanceOfType(value);
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            IList list = this.DisplayedIList;
            return list?.Count ?? 0;
        }
    }

    /// <inheritdoc/>
    public VirtualNode InsertListItem(int index, object value, bool config)
    {
        if (ReadOnly)
        {
            return null;
        }

        if (_adapter != null && !_adapter.CanDropIn(value))
        {
            return null;
        }

        value = _adapter.DropInConvert(value);
        if (value is null)
        {
            return null;
        }

        var model = FindModel();
        IList list = this.DisplayedIList;
        if (list is null) return null;

        Type elementType = GetElementType();
        Type reflectedElementType = ReflectionHelper.ReflectDynamicType(elementType, ReflectionHelper.GetIListElementType(list.GetType()));

        if (value != null && !reflectedElementType.IsInstanceOfType(value))
        {
            throw new InvalidOperationException();
        }

        model.PerformValueAction(new IListResolverInsertAction(this, index, value));

        // Synchronize editor nodes
        // VirtualNode newNode = model.CreateNode(reflectedElementType, this);
        VirtualNode newNode = value != null ?
            model.CreateNode(reflectedElementType, this) :
            model.CreateNode(value.GetType(), this);
        this.Nodes.Insert(index, newNode);
        model.ConfigureNode(newNode);

        newNode.Getter = this.CreateElementValueGetter(index);
        newNode.Setter = this.CreateElementValueSetter(index);

        PerformGetValue();
        newNode.PerformGetValue();

        return Nodes[index];
    }

    /// <inheritdoc/>
    public void RemoveListItem(VirtualNode node)
    {
        if (ReadOnly)
        {
            return;
        }

        if (node.Parent != this)
        {
            return;
        }

        var model = FindModel();

        int index = node.Index;
        // Nodes.Remove(node);
        // node.Dispose();

        model.SuspendGetValue();
        model.PerformValueAction(new IListResolverRemoveAction(this, index));
        // Synchronized removal
        Nodes.RemoveAt(index);
        model.ResumeGetValue(false);

        PerformGetValue();
    }

    /// <inheritdoc/>
    public void RemoveListItems(IEnumerable<VirtualNode> nodes)
    {
        if (ReadOnly)
        {
            return;
        }

        var removeNodes = nodes.Where(o => o.Parent == this);
        if (!removeNodes.Any())
        {
            return;
        }

        var model = FindModel();
        IEnumerable<int> indexes = removeNodes.OrderByDescending(o => o.Index).Select(o => o.Index);

        model.SuspendGetValue();
        foreach (var index in indexes)
        {
            model.PerformValueAction(new IListResolverRemoveAction(this, index));
            // Synchronized removal
            Nodes.RemoveAt(index);
        }
        model.ResumeGetValue(false);

        PerformGetValue();
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _list = null;
    }
}
