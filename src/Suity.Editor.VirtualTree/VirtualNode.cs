using Suity.Views;
using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents an abstract node in a virtual tree structure.
/// </summary>
public abstract partial class VirtualNode : MarshalByRefObject
{
    [ThreadStatic]
    private static StringBuilder _idBuilder;

    /// <summary>
    /// Context menu key for array owner nodes.
    /// </summary>
    public const string ContextMenu_ArrayOwner = "Array";

    /// <summary>
    /// Context menu key for array member nodes.
    /// </summary>
    public const string ContextMenu_ArrayMember = "ArrayMember";

    private VirtualTreeModel _model;
    private NodeCollection _nodes;
    private VirtualNode _parent;
    private int _index = -1;

    private bool _disposed;
    private string _propertyName = string.Empty;
    private string _description = string.Empty;
    private Image _icon;
    private Type _editedType;
    private bool _contentInit;
    private bool _updating;
    private Func<object> _getter;
    private Action<object> _setter;

    #region Tree

    internal VirtualTreeModel Model
    {
        get => _model;
        set => _model = value;
    }

    /// <summary>
    /// Gets the collection of child nodes.
    /// </summary>
    public NodeCollection Nodes => _nodes ??= new(this);

    /// <summary>
    /// Gets or sets the parent node. Setting this automatically manages the node collection.
    /// </summary>
    public VirtualNode Parent
    {
        get => _parent;
        internal set
        {
            if (value != _parent)
            {
                _parent?.Nodes.Remove(this);

                value?.Nodes.Add(this);
            }
        }
    }

    /// <summary>
    /// Gets the index of this node within its parent's collection.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Gets the previous sibling node, or null if this is the first child.
    /// </summary>
    public VirtualNode PreviousNode
    {
        get
        {
            int index = Index;
            if (index > 0)
            {
                return _parent.Nodes[index - 1];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the next sibling node, or null if this is the last child.
    /// </summary>
    public VirtualNode NextNode
    {
        get
        {
            int index = Index;
            if (index >= 0 && index < _parent.Nodes.Count - 1)
            {
                return _parent.Nodes[index + 1];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this node is a leaf (has no children).
    /// </summary>
    public virtual bool IsLeaf => false;

    /// <summary>
    /// Gets the full hierarchical ID of this node.
    /// </summary>
    /// <returns>The full path-based identifier.</returns>
    public virtual string GetFullId()
    {
        _idBuilder ??= new StringBuilder();

        _idBuilder.Clear();
        BuildFullId(_idBuilder);

        return _idBuilder.ToString();
    }

    /// <summary>
    /// Gets the local ID of this node (without parent path).
    /// </summary>
    /// <returns>The local identifier.</returns>
    public virtual string GetId()
    {
        _idBuilder ??= new StringBuilder();

        _idBuilder.Clear();

        BuildId(_idBuilder);

        return _idBuilder.ToString();
    }

    private void BuildFullId(StringBuilder builder)
    {
        if (_parent != null)
        {
            _parent?.BuildFullId(builder);
            builder.Append('/');
        }

        BuildId(builder);
    }

    private void BuildId(StringBuilder builder)
    {
        if (!string.IsNullOrEmpty(_propertyName))
        {
            builder.Append(_propertyName);
        }
        else
        {
            builder.Append(_index);
        }

        if (_editedType != null)
        {
            builder.Append('@');
            builder.Append(_editedType.FullName);
        }
        else
        {
            builder.Append(":");
            builder.Append(this.GetType().FullName);
        }
    }

    /// <summary>
    /// Finds the model associated with this node by traversing up the tree.
    /// </summary>
    /// <returns>The tree model, or null if not found.</returns>
    public VirtualTreeModel FindModel()
    {
        if (_model != null)
        {
            return _model;
        }

        VirtualNode node = this;
        while (node != null)
        {
            if (node._model != null)
            {
                _model = node._model;

                return _model;
            }
            node = node.Parent;
        }

        return null;
    }

    /// <summary>
    /// Notifies the model that this node has changed.
    /// </summary>
    protected void NotifyModel()
    {
        FindModel()?.NotifyNodeChanged(this);
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Finalizer for cleanup.
    /// </summary>
    ~VirtualNode()
    {
        this.Dispose(false);
    }

    /// <summary>
    /// Releases all resources used by this node.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources used by this node.
    /// </summary>
    /// <param name="manually">True if called from user code, false if from finalizer.</param>
    private void Dispose(bool manually)
    {
        if (this._disposed) return;

        foreach (VirtualNode node in Nodes)
        {
            node.Dispose();
        }

        if (manually)
        {
            // System garbage collection cannot use Nodes.Clear()
            Nodes.Clear();
        }

        this.OnDisposing(manually);
        this._disposed = true;
    }

    /// <summary>
    /// Called when the node is being disposed. Override to add custom cleanup.
    /// </summary>
    /// <param name="manually">True if called from user code, false if from finalizer.</param>
    protected virtual void OnDisposing(bool manually)
    { }

    /// <summary>
    /// Gets a value indicating whether this node has been disposed.
    /// </summary>
    public bool Disposed => _disposed;

    #endregion

    #region Editor

    internal bool IsContentInitialized => _contentInit;

    /// <summary>
    /// Initializes the content of this node, creating child nodes.
    /// </summary>
    internal void InitContent()
    {
        if (_contentInit) return;

        this._contentInit = true;

        Nodes.Clear();
        OnClearContent();
        OnInitContent();
    }

    /// <summary>
    /// Clears the content of this node, removing child nodes.
    /// </summary>
    internal void ClearContent()
    {
        if (!_contentInit) return;

        this._contentInit = false;

        Nodes.Clear();
        OnClearContent();
    }

    /// <summary>
    /// Initializes content for this node and all descendants.
    /// </summary>
    internal void InitContentAll()
    {
        InitContent();
        foreach (var node in Nodes)
        {
            node.InitContent();
        }
    }

    /// <summary>
    /// Called when content is initialized. Override to create child nodes.
    /// </summary>
    protected virtual void OnInitContent()
    {
        PerformGetValue();
    }

    /// <summary>
    /// Called when content is cleared. Override to clean up child nodes.
    /// </summary>
    protected virtual void OnClearContent()
    {
    }

    /// <summary>
    /// Called when this node is added to a parent.
    /// </summary>
    protected virtual void OnAdded()
    {
    }

    /// <summary>
    /// Called when this node is removed from a parent.
    /// </summary>
    protected virtual void OnRemoved()
    {
    }

    /// <summary>
    /// Gets or sets the property name associated with this node.
    /// </summary>
    public string PropertyName
    {
        get => _propertyName;
        set
        {
            if (_propertyName != value)
            {
                _propertyName = value;
                NotifyModel();
            }
        }
    }

    /// <summary>
    /// Gets or sets the type being edited by this node.
    /// </summary>
    public Type EditedType
    {
        get => _editedType;
        set
        {
            if (_editedType != value)
            {
                this._editedType = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this node is read-only.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node is temporarily expanded.
    /// </summary>
    public bool TempExpanded { get; set; }

    /// <summary>
    /// Finds a child node by property name.
    /// </summary>
    /// <param name="propertyName">The property name to search for.</param>
    /// <returns>The matching child node, or null if not found.</returns>
    public VirtualNode FindChildNode(string propertyName)
    {
        return Nodes.FirstOrDefault(o => o.PropertyName == propertyName);
    }

    /// <summary>
    /// Gets a child node at the specified index.
    /// </summary>
    /// <param name="index">The index of the child node.</param>
    /// <returns>The child node at the index, or null if out of range.</returns>
    public VirtualNode GetChildNodeAt(int index)
    {
        return Nodes.GetItemAtSafe(index);
    }

    /// <summary>
    /// Checks whether this node is currently expanded.
    /// </summary>
    /// <returns>True if expanded, false otherwise.</returns>
    public bool IsExpanded()
    {
        var model = FindModel();
        if (model != null)
        {
            return model.IsExpanded(this);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Requests expansion of this node.
    /// </summary>
    public void Expand()
    {
        var model = FindModel();
        model?.Expand(this);
    }

    /// <summary>
    /// Expands this node and all descendant nodes.
    /// </summary>
    public void ExpandAll()
    {
        var model = FindModel();
        if (model != null)
        {
            _ExpandAll(model);
        }
    }

    private void _ExpandAll(VirtualTreeModel model)
    {
        model.Expand(this);
        if (_nodes != null)
        {
            foreach (var childNode in _nodes)
            {
                childNode._ExpandAll(model);
            }
        }
    }

    /// <summary>
    /// Handles a node action (e.g., double-click). Override to customize behavior.
    /// </summary>
    public virtual void HandleNodeAction()
    {
        (DisplayedValue as IViewDoubleClickAction)?.DoubleClick();
    }

    /// <summary>
    /// Handles a key down event. Override to respond to keyboard input.
    /// </summary>
    /// <param name="key">The key code pressed.</param>
    public virtual void HandleKeyDown(int key)
    {
    }

    /// <summary>
    /// Gets the context menu key for this node.
    /// </summary>
    /// <returns>The context menu key string.</returns>
    public virtual string GetContextMenuKey()
    {
        if (Parent != null)
        {
            return Parent.GetChildContextMenuKey(this);
        }
        else
        {
            return ContextMenu_ArrayOwner;
        }
    }

    /// <summary>
    /// Gets the context menu key for a child node. Override to customize child context menus.
    /// </summary>
    /// <param name="childNode">The child node to get the context menu key for.</param>
    /// <returns>The context menu key string.</returns>
    public virtual string GetChildContextMenuKey(VirtualNode childNode)
    {
        return null;
    }

    /// <summary>
    /// Checks whether a node can be dropped into this node.
    /// </summary>
    /// <param name="node">The node being dragged.</param>
    /// <returns>True if the drop is allowed, false otherwise.</returns>
    public virtual bool CanDropIn(VirtualNode node)
    {
        return false;
    }

    /// <summary>
    /// Checks whether an external object can be dropped into this node.
    /// </summary>
    /// <param name="obj">The object being dragged.</param>
    /// <returns>True if the drop is allowed, false otherwise.</returns>
    public virtual bool CanDropInExternal(object obj)
    {
        return false;
    }

    /// <summary>
    /// Converts a dropped object to a suitable value for this node.
    /// </summary>
    /// <param name="obj">The object being dropped.</param>
    /// <returns>The converted value.</returns>
    public virtual object DropInConvert(object obj)
    {
        return obj;
    }

    /// <summary>
    /// Gets the view identifier from the model.
    /// </summary>
    public int ViewId
    {
        get
        {
            var model = FindModel();
            return model?.ViewId ?? 0;
        }
    }

    /// <summary>
    /// Checks whether a specific view ID is supported by this node.
    /// </summary>
    /// <param name="viewId">The view ID to check.</param>
    /// <returns>True if the view is supported, false otherwise.</returns>
    public bool IsViewIdSupported(int viewId)
    {
        if (viewId == 0 || viewId == ViewIds.TreeView)
        {
            return true;
        }

        var model = FindModel();
        if (model != null)
        {
            return viewId == model.ViewId;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region ParentInfos

    /// <summary>
    /// Gets a value of type T from the parent node's displayed value.
    /// </summary>
    /// <typeparam name="T">The type to retrieve.</typeparam>
    /// <returns>The parent value cast to T, or default if not matching.</returns>
    public T GetParentValue<T>()
    {
        object value = Parent?.DisplayedValue;
        return value is T tValue ? tValue : default;
    }

    /// <summary>
    /// Gets the displayed value of the parent node.
    /// </summary>
    public object ParentValue => Parent?.DisplayedValue;

    /// <summary>
    /// Gets a service from the model's service provider.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>The service instance, or null if not found.</returns>
    public virtual object GetService(Type serviceType)
    {
        if (serviceType.IsAssignableFrom(this.GetType()))
        {
            return this;
        }

        var model = FindModel();
        var provider = model?.ServiceProvider;
        return provider?.GetService(serviceType);
    }

    /// <summary>
    /// Checks whether the specified node is an ancestor of this node.
    /// </summary>
    /// <param name="parent">The potential ancestor node.</param>
    /// <returns>True if the node is an ancestor, false otherwise.</returns>
    public bool ContainsParent(VirtualNode parent)
    {
        var p = this;
        while (p != null)
        {
            if (p == parent)
            {
                return true;
            }

            p = p._parent;
        }

        return false;
    }

    #endregion

    #region Get/Set/Display Value

    /// <summary>
    /// Sets the getter function for retrieving the value.
    /// </summary>
    public Func<object> Getter { set => _getter = value; }

    /// <summary>
    /// Sets the setter action for updating the value.
    /// </summary>
    public Action<object> Setter { set => _setter = value; }

    /// <summary>
    /// Gets the value displayed by this node. Must be implemented by derived classes.
    /// </summary>
    public abstract object DisplayedValue { get; }

    /// <summary>
    /// Performs a value retrieval operation, updating display information.
    /// </summary>
    public void PerformGetValue()
    {
        VirtualTreeModel model = FindModel();
        // Suspend update
        if (model?._suspendDepth > 0)
        {
            return;
        }

        if (_updating)
        {
            return;
        }

        try
        {
            _updating = true;

            OnGetValue();

            if (IsExpanded())
            {
                foreach (var node in Nodes)
                {
                    node.InitContent();
                }
            }

            UpdateDisplay();
            NotifyModel();
        }
        finally
        {
            _updating = false;
        }
    }

    /// <summary>
    /// Performs a value setting operation.
    /// </summary>
    public void PerformSetValue()
    {
        if (_updating)
        {
            return;
        }

        try
        {
            _updating = true;
            OnSetValue();
        }
        finally
        {
            _updating = false;
        }
    }

    /// <summary>
    /// Called when getting a value. Override to customize value retrieval.
    /// </summary>
    protected virtual void OnGetValue()
    {
    }

    /// <summary>
    /// Called when setting a value. Override to customize value storage.
    /// </summary>
    protected virtual void OnSetValue()
    {
        SetValue(DisplayedValue);
    }

    /// <summary>
    /// Retrieves the current value using the getter.
    /// </summary>
    /// <returns>The current value, or null if no getter is set.</returns>
    protected object GetValue()
    {
        return _getter != null ? _getter() : null;
    }

    /// <summary>
    /// Sets the value using the setter and refreshes the display.
    /// </summary>
    /// <param name="value">The value to set.</param>
    protected void SetValue(object value)
    {
        var model = FindModel();

        try
        {
            model?.BeginSetterAction();
            _setter?.Invoke(value);
            PerformGetValue();
        }
        finally
        {
            model?.EndSetterAction();
        }
    }

    /// <summary>
    /// Finds a value of type T in this node or any ancestor.
    /// </summary>
    /// <typeparam name="T">The type to search for.</typeparam>
    /// <returns>The first matching value, or default if not found.</returns>
    public T FindValueOrParent<T>()
    {
        VirtualNode node = this;
        while (node != null)
        {
            object value = node.DisplayedValue;
            if (value is T t)
            {
                return t;
            }

            node = node.Parent;
        }

        return default;
    }

    #endregion

    /// <summary>
    /// Notifies the model that this node has been updated.
    /// </summary>
    public void NotifyUpdated()
    {
        NotifyModel();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Text;
    }
}