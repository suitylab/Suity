using Suity.Collections;
using Suity.Editor.VirtualTree.Adapters;
using Suity.Editor.VirtualTree.Nodes;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents the model for a virtual tree structure, managing nodes, display objects, and interactions.
/// </summary>
public abstract class VirtualTreeModel
{
    /// <summary>
    /// Priority constant indicating no editor priority.
    /// </summary>
    public const int EditorPriority_None = 0;

    /// <summary>
    /// Priority constant indicating general editor priority.
    /// </summary>
    public const int EditorPriority_General = 20;

    /// <summary>
    /// Priority constant indicating specialized editor priority.
    /// </summary>
    public const int EditorPriority_Specialized = 50;

    /// <summary>
    /// Priority constant indicating override editor priority (highest).
    /// </summary>
    public const int EditorPriority_Override = 100;

    /// <summary>
    /// Maximum number of preview columns allowed.
    /// </summary>
    public const int MaxPreviewColumn = 32;

    #region MainProvider

    private class MainVirtualNodeProvider : IVirtualNodeProvider
    {
        private readonly List<IVirtualNodeProvider> _subProviders = [];

        public MainVirtualNodeProvider()
        {
        }

        public void RegisterSubProvider(IVirtualNodeProvider provider)
        {
            if (provider is null) throw new ArgumentNullException();
            if (provider == this) throw new ArgumentException();

            if (_subProviders.Contains(provider))
            {
                return;
            }

            _subProviders.Add(provider);
        }

        public int IsResponsibleFor(Type baseType, ProviderContext context)
        {
            return EditorPriority_General;
        }

        public VirtualNode CreateNode(Type baseType, ProviderContext context)
        {
            VirtualNode e = null;

            do
            {
                if (baseType is null)
                {
                    e = new EmptyNode();
                    break;
                }
                
                if (baseType == typeof(string))
                {
                    e = new StringNode();
                    break;
                }

                var availSubProviders =
                    from p in this._subProviders
                    where p.IsResponsibleFor(baseType, context) != EditorPriority_None
                    orderby p.IsResponsibleFor(baseType, context) descending
                    select p;

                IVirtualNodeProvider subProvider = availSubProviders.FirstOrDefault();
                if (subProvider != null)
                {
                    e = subProvider.CreateNode(baseType, context);
                    if (e != null) 
                    {
                        break;
                    }
                }

                if (typeof(System.Collections.IList).IsAssignableFrom(baseType))
                {
                    EditorTypeHelper.GetBestEditorType(typeof(IListAdapter), baseType, context, out int bestScore, out Type bestEditorType);
                    if (bestEditorType != null)
                    {
                        e = new IListVirtualNode((IListAdapter)bestEditorType.CreateInstanceOf());
                    }
                    else
                    {
                        e = new IListVirtualNode(EmptyIListAdapter.Instance);
                    }
                    break;
                }

                //Default empty node
                e = new ToStringNode();

            } while (false);

            if (e != null)
            {
                e.EditedType = baseType;
            }
            
            return e;
        }
    }

    #endregion

    private readonly RootNode _rootNode;
    private VirtualNode _root;
    private object _object;
    private readonly MainVirtualNodeProvider _mainProvider = new();
    private IServiceProvider _syncServiceProvider;
    private int _viewId;
    internal int _suspendDepth;
    private readonly List<PreviewPath> _previewPaths = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTreeModel"/> class.
    /// </summary>
    protected VirtualTreeModel()
    {
        _rootNode = new RootNode
        {
            Model = this
        };

        //Register a user node provider
        RegisterVirtualNodeProvider(new UserVirtualNodeProvider());
    }

    /// <summary>
    /// Gets the root node of the virtual tree.
    /// </summary>
    protected RootNode Root => _rootNode;

    #region Props

    /// <summary>
    /// Gets or sets a value indicating whether custom preview text is enabled.
    /// </summary>
    public bool CustomPreviewText { get; set; }

    #endregion

    #region Event

    /// <summary>
    /// Occurs when a set value operation begins.
    /// </summary>
    public event EventHandler<MacroEventArgs> BeginSetValue;

    /// <summary>
    /// Occurs when a set value operation ends.
    /// </summary>
    public event EventHandler<MacroEventArgs> EndSetValue;

    /// <summary>
    /// Occurs when an undo/redo action is requested.
    /// </summary>
    public event EventHandler<UndoRedoActionEventArgs> RequestDoAction;

    /// <summary>
    /// Occurs when a node expansion is requested.
    /// </summary>
    public event EventHandler<VirtualNodeEventArgs> RequestExpand;

    /// <summary>
    /// Occurs when an expansion state query is made.
    /// </summary>
    public event EventHandler<VirtualNodeQueryEventArgs> ExpandQuery;

    /// <summary>
    /// Occurs when a value in the tree is edited.
    /// </summary>
    public event EventHandler<TreeValueEditEventArgs> ValueEdited;

    /// <summary>
    /// Occurs when a list item is edited.
    /// </summary>
    public event EventHandler<ListEditEventArgs> ListEdited;

    #endregion

    #region Tree

    private VirtualNode.NodeCollection Nodes => _rootNode.Nodes;

    private void Add(VirtualNode node)
    {
        _rootNode.Nodes.Add(node);
    }

    private bool Remove(VirtualNode node)
    {
        return _rootNode.Nodes.Remove(node);
    }

    private void Clear()
    {
        _rootNode.Nodes.Clear();
    }

    #endregion

    #region ITreeModel Members

    /// <summary>
    /// Notifies that a node has changed. Must be implemented by derived classes.
    /// </summary>
    /// <param name="node">The node that changed.</param>
    public abstract void NotifyNodeChanged(VirtualNode node);

    /// <summary>
    /// Notifies that a node has been inserted. Must be implemented by derived classes.
    /// </summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="index">The index where the node was inserted.</param>
    /// <param name="node">The inserted node.</param>
    public abstract void NotifyNodeInserted(VirtualNode parent, int index, VirtualNode node);

    /// <summary>
    /// Notifies that a node has been removed. Must be implemented by derived classes.
    /// </summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="index">The index where the node was removed from.</param>
    /// <param name="node">The removed node.</param>
    public abstract void NotifyNodeRemoved(VirtualNode parent, int index, VirtualNode node);

    #endregion

    #region Selection

    /// <summary>
    /// Registers a virtual node provider to create nodes for specific types.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public void RegisterVirtualNodeProvider(IVirtualNodeProvider provider)
    {
        _mainProvider.RegisterSubProvider(provider);
    }

    /// <summary>
    /// Creates a node for the specified edited type.
    /// </summary>
    /// <param name="editedType">The type to create a node for.</param>
    /// <param name="parentNode">The parent node context.</param>
    /// <returns>The created virtual node.</returns>
    public VirtualNode CreateNode(Type editedType, VirtualNode parentNode)
    {
        var context = new ProviderContext(this, parentNode);
        var node = _mainProvider.CreateNode(editedType, context);
        node.EditedType = editedType;

        return node;
    }

    /// <summary>
    /// Sets the object to be displayed in the tree.
    /// </summary>
    /// <param name="obj">The object to display.</param>
    public void SetDisplayedObject(object obj)
    {
        _object = obj;

        UpdateDisplayedObject();
    }

    /// <summary>
    /// Updates the displayed object, refreshing the tree if necessary.
    /// </summary>
    public void UpdateDisplayedObject()
    {
        if (_object != null)
        {
            Type type = _object.GetType();
            if (_root is null || _root.EditedType != type)
            {
                InitVirtualNode(type);
            }
            else
            {
                UpdateVirtualNode();
            }

            _root.InitContent();
        }
        else
        {
            DisposeVirtualNode();
        }
    }

    /// <summary>
    /// Gets the currently displayed object.
    /// </summary>
    public object DisplayedObject => _object;

    /// <summary>
    /// Configures a node after creation. Override to customize node setup.
    /// </summary>
    /// <param name="node">The node to configure.</param>
    public void ConfigureNode(VirtualNode node)
    {
    }

    /// <summary>
    /// Gets the virtual path for a node by traversing up to the root.
    /// </summary>
    /// <param name="node">The node to get the path for.</param>
    /// <returns>The virtual path representing the node's position.</returns>
    public VirtualPath GetVirtualPath(VirtualNode node)
    {
        Stack<string> stack = new();

        while (node != null && node != _root && !string.IsNullOrEmpty(node.PropertyName))
        {
            stack.Push(node.PropertyName);
            node = node.Parent;
        }

        return new VirtualPath([.. stack]);
    }

    /// <summary>
    /// Gets a node by its virtual path.
    /// </summary>
    /// <param name="path">The virtual path to locate the node.</param>
    /// <returns>The node at the specified path, or null if not found.</returns>
    public VirtualNode GetNodeByVirtualPath(VirtualPath path)
    {
        if (path is null)
        {
            return null;
        }

        if (path.Path.Length == 0)
        {
            return _root;
        }

        VirtualNode node = _root;
        if (node is null)
        {
            return null;
        }

        for (int i = 0; i < path.Path.Length; i++)
        {
            string propName = path.Path[i];
            //TODO: Nodes based on Index are no longer valid, this needs to be unified to use SyncPath
            node = node.FindChildNode(propName);

            if (node is null)
            {
                return null;
            }
        }

        return node;
    }

    /// <summary>
    /// Gets a node by its sync path, returning any remaining path segments.
    /// </summary>
    /// <param name="path">The sync path to traverse.</param>
    /// <param name="rest">The remaining path segments that couldn't be resolved.</param>
    /// <returns>The deepest node that could be resolved.</returns>
    public VirtualNode GetNodeBySyncPath(SyncPath path, out SyncPath rest)
    {
        rest = SyncPath.Empty;
        if (path is null)
        {
            return null;
        }

        if (path.Length == 0)
        {
            return _root;
        }

        VirtualNode node = _root;
        if (node is null)
        {
            rest = path;

            return null;
        }

        VirtualNode childNode = null;

        for (int i = 0; i < path.Length; i++)
        {
            node.InitContent();

            object item = path[i];
            if (item is string name)
            {
                childNode = node.FindChildNode(name);
            }
            else if (item is int index)
            {
                childNode = node.GetChildNodeAt(index);
            }

            if (childNode is null)
            {
                rest = path.SubPath(i, path.Length - i);

                return node;
            }

            node = childNode;
        }

        return node;
    }

    /// <summary>
    /// Finds a node that displays the specified value.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>The node displaying the value, or null if not found.</returns>
    public VirtualNode FindNode(object value)
    {
        if (_root != null)
        {
            return FindNode(_root, value);
        }
        else
        {
            return null;
        }
    }

    private VirtualNode FindNode(VirtualNode node, object value)
    {
        if (node is null) return null;
        if (node.DisplayedValue == value)
        {
            return node;
        }

        foreach (var childNode in node.Nodes)
        {
            var result = FindNode(childNode, value);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void InitVirtualNode(Type type)
    {
        if (_root != null)
        {
            Remove(_root);
            _root.Dispose();
        }

        _root = _mainProvider.CreateNode(type, new ProviderContext(this));
        Add(_root);
        UpdateVirtualNode();
        ConfigureNode(_root);

        _rootNode.InternalNotifyModel();
    }

    private void UpdateVirtualNode()
    {
        if (_root is null) return;

        _root.Getter = () => _object;
        _root.Setter = val => { };
        _root.PerformGetValue();
    }

    private void DisposeVirtualNode()
    {
        if (_root is null)
        {
            return;
        }

        Remove(_root);
        _root.Dispose();
        _root = null;

        _rootNode.InternalNotifyModel();
    }

    #endregion

    #region Get

    /// <summary>
    /// Suspends automatic value retrieval, allowing batch operations.
    /// </summary>
    public void SuspendGetValue()
    {
        _suspendDepth++;
    }

    /// <summary>
    /// Resumes automatic value retrieval after suspension.
    /// </summary>
    /// <param name="autoGetValue">Whether to automatically refresh values after resuming.</param>
    public void ResumeGetValue(bool autoGetValue = true)
    {
        if (_suspendDepth > 0)
        {
            _suspendDepth--;
            if (_suspendDepth == 0 && autoGetValue)
            {
                _root?.PerformGetValue();
            }
        }
    }

    #endregion

    #region Preview Path

    /// <summary>
    /// Adds a preview path to the model.
    /// </summary>
    /// <param name="path">The preview path to add.</param>
    /// <returns>True if the path was added, false otherwise.</returns>
    public bool AddPreviewPath(PreviewPath path)
    {
        if (path is null)
        {
            return false;
        }

        if (_previewPaths.Count >= MaxPreviewColumn)
        {
            return false;
        }

        PreviewPath current = _previewPaths.Find(o => o.Path == path.Path);

        if (current != null)
        {
            _previewPaths.Remove(current);
        }

        _previewPaths.Add(path);

        return true;
    }

    /// <summary>
    /// Removes a preview path from the model.
    /// </summary>
    /// <param name="path">The preview path to remove.</param>
    /// <returns>True if the path was removed, false otherwise.</returns>
    public bool RemovePreviewPath(PreviewPath path)
    {
        PreviewPath current = _previewPaths.Find(o => o.Path == path.Path);
        if (current != null)
        {
            _previewPaths.Remove(current);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a preview path at the specified index.
    /// </summary>
    /// <param name="index">The index of the path to remove.</param>
    /// <returns>True if the path was removed, false otherwise.</returns>
    public bool RemovePreviewPathAt(int index)
    {
        return _previewPaths.RemoveAtSafe(index);
    }

    /// <summary>
    /// Swaps two preview paths.
    /// </summary>
    /// <param name="index">The index of the first path.</param>
    /// <param name="indexTo">The index of the second path.</param>
    /// <returns>True if the swap was successful, false otherwise.</returns>
    public bool SwapPreviewPath(int index, int indexTo)
    {
        return _previewPaths.SwapListItem(index, indexTo);
    }

    /// <summary>
    /// Removes a preview path and inserts it at a new position.
    /// </summary>
    /// <param name="indexFrom">The index to remove from.</param>
    /// <param name="indexInsert">The index to insert at.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    public bool RemoveInsertPreviewPath(int indexFrom, int indexInsert)
    {
        return _previewPaths.RemoveInserListItem(indexFrom, indexInsert);
    }

    /// <summary>
    /// Clears all preview paths.
    /// </summary>
    /// <returns>True if paths were cleared, false if there were none.</returns>
    public bool ClearPreviewPath()
    {
        if (_previewPaths.Count > 0)
        {
            _previewPaths.Clear();

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the read-only list of preview paths.
    /// </summary>
    public IReadOnlyList<PreviewPath> PreviewPaths => _previewPaths;

    #endregion

    /// <summary>
    /// Gets or sets the service provider for synchronization services.
    /// </summary>
    public IServiceProvider ServiceProvider
    {
        get => _syncServiceProvider;
        set => _syncServiceProvider = value;
    }

    /// <summary>
    /// Gets or sets the view identifier for this model.
    /// </summary>
    public int ViewId
    {
        get => _viewId;
        set => _viewId = value;
    }

    /// <summary>
    /// Performs a value retrieval operation on the root node.
    /// </summary>
    public void PerformGetValue()
    {
        _root?.PerformGetValue();
    }

    /// <summary>
    /// Performs a setter action within the model's transaction context.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    public void PerformValueAction(VirtualNodeSetterAction action)
    {
        try
        {
            BeginSetterAction();
            HandleSetterAction(action);
        }
        finally
        {
            EndSetterAction();
        }
    }

    /// <summary>
    /// Gets the root node of the tree.
    /// </summary>
    public VirtualNode RootNode => _root;

    /// <summary>
    /// Begins a setter action transaction.
    /// </summary>
    /// <param name="name">Optional name for the transaction.</param>
    public void BeginSetterAction(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            BeginSetValue?.Invoke(this, MacroEventArgs.Empty);
        }
        else
        {
            BeginSetValue?.Invoke(this, new MacroEventArgs(name));
        }
    }

    /// <summary>
    /// Ends a setter action transaction.
    /// </summary>
    /// <param name="name">Optional name for the transaction.</param>
    public void EndSetterAction(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            EndSetValue?.Invoke(this, MacroEventArgs.Empty);
        }
        else
        {
            EndSetValue?.Invoke(this, new MacroEventArgs(name));
        }
    }

    /// <summary>
    /// Handles an undo/redo action, delegating to registered handlers if available.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void HandleSetterAction(UndoRedoAction action)
    {
        if (RequestDoAction != null)
        {
            var args = new UndoRedoActionEventArgs(action);
            RequestDoAction(this, args);
            if (!args.Handled)
            {
                action.Do();
            }
        }
        else
        {
            action.Do();
        }
    }

    internal void NotifyNodeEdited(object value, string propertyName)
    {
        ValueEdited?.Invoke(this, new VirtualNodeValueEditEventArgs(null, value, propertyName));
    }

    internal void NotifyListEdited(object value, int index, ListEditEventArgs.EditMode mode)
    {
        ListEdited?.Invoke(this, new ListEditEventArgs(value, index, mode));
    }

    /// <summary>
    /// Requests expansion of the specified node.
    /// </summary>
    /// <param name="virtualNode">The node to expand.</param>
    public virtual void Expand(VirtualNode virtualNode)
    {
        RequestExpand?.Invoke(this, new VirtualNodeEventArgs(virtualNode));
    }

    /// <summary>
    /// Checks whether the specified node is currently expanded.
    /// </summary>
    /// <param name="virtualNode">The node to check.</param>
    /// <returns>True if the node is expanded, false otherwise.</returns>
    public virtual bool IsExpanded(VirtualNode virtualNode)
    {
        if (ExpandQuery != null)
        {
            var args = new VirtualNodeQueryEventArgs(virtualNode);
            ExpandQuery(this, args);

            return args.Value;
        }
        else
        {
            return false;
        }
    }
}