using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.PathTree;

/// <summary>
/// Represents an abstract base class for all nodes in a path tree, providing tree structure, navigation, and view data.
/// </summary>
public abstract class PathNode
{
    private string _nodePath;
    private string _nodeId;
    private string _terminal;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathNode"/> class.
    /// </summary>
    protected PathNode()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathNode"/> class with the specified node path.
    /// </summary>
    /// <param name="nodePath">The path for this node.</param>
    protected PathNode(string nodePath)
    {
        SetupNodePath(nodePath);
    }

    internal PathNode(IPathTreeModel model)
    {
        Model = model;
    }

    /// <summary>
    /// Gets the full path of this node.
    /// </summary>
    public string NodePath => _nodePath;
    /// <summary>
    /// Gets the unique identifier for this node, derived from the node path.
    /// </summary>
    public string NodeId => _nodeId;
    /// <summary>
    /// Gets the terminal (file or directory name) portion of the node path.
    /// </summary>
    public string Terminal => _terminal;

    #region Tree

    private IPathTreeModel _model;
    internal PathNodeCollection _nodeList;
    internal PathNode _parent;
    internal int _index = -1;

    internal IPathTreeModel Model 
    {
        get => _model; 
        set => _model = value; 
    }

    /// <summary>
    /// Gets the collection of child nodes for this node.
    /// </summary>
    public PathNodeCollection NodeList => _nodeList ?? (_nodeList = PathNodeCollection._factory(this));

    /// <summary>
    /// Gets or sets the parent node. Setting this will automatically remove the node from its current parent and add it to the new parent.
    /// </summary>
    public PathNode Parent
    {
        get => _parent;
        internal set
        {
            if (value != _parent)
            {
                if (_parent != null)
                {
                    _parent.NodeList.Remove(this);
                }

                if (value != null)
                {
                    value.NodeList.Add(this);
                }
            }
        }
    }

    /// <summary>
    /// Gets the zero-based index of this node within its parent's child collection.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Gets the sibling node that appears before this node, or null if this is the first child.
    /// </summary>
    public PathNode PreviousNode
    {
        get
        {
            int index = Index;
            if (index > 0)
            {
                return _parent.NodeList[index - 1];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the sibling node that appears after this node, or null if this is the last child.
    /// </summary>
    public PathNode NextNode
    {
        get
        {
            int index = Index;
            if (index >= 0 && index < _parent.NodeList.Count - 1)
            {
                return _parent.NodeList[index + 1];
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
    /// Traverses up the parent chain to find the nearest model associated with this node or any of its ancestors.
    /// </summary>
    internal IPathTreeModel FindModel()
    {
        PathNode node = this;
        while (node != null)
        {
            if (node.Model != null)
            {
                return node.Model;
            }

            node = node.Parent;
        }
        return null;
    }

    /// <summary>
    /// Notifies the model that this node has changed, unless the parent's node list is suspended.
    /// </summary>
    protected void NotifyChanged()
    {
        // Ignore after parent suspension
        if (_parent?.NodeList.Suspended == true)
        {
            return;
        }

        FindModel()?.OnNodeChanged(this);
    }

    /// <summary>
    /// Notifies the model that the tree structure has changed, unless the parent's node list is suspended.
    /// </summary>
    protected void NotifyStructureChanged()
    {
        // Ignore after parent suspension
        if (_parent?.NodeList.Suspended == true)
        {
            return;
        }

        FindModel()?.OnStructureChanged(this);
    }

    #endregion

    /// <summary>
    /// Sets the path information for this node.
    /// </summary>
    /// <param name="nodePath">The new path for this node.</param>
    public void SetupNodePath(string nodePath)
    {
        if (nodePath is null)
        {
            nodePath = string.Empty;
        }

        _nodePath = nodePath.TrimEnd('\\', '/');
        _nodeId = _nodePath.GetPathId();

        _terminal = _nodePath.GetPathTerminal();

        OnSetupNodePath(nodePath);
    }

    #region Virtual

    /// <summary>
    /// Deletes this node, optionally sending it to the recycle bin.
    /// </summary>
    /// <param name="sendToRecycleBin">Whether to send the item to the recycle bin.</param>
    public virtual void Delete(bool sendToRecycleBin) => Parent = null;

    /// <summary>
    /// Moves this node to a new path.
    /// </summary>
    /// <param name="newNodePath">The new path for this node.</param>
    /// <param name="results">A collection to store rename results.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    public virtual bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        var model = FindModel();
        if (model is null)
        {
            SetupNodePath(newNodePath);

            return true;
        }

        string oldNodePath = NodePath;

        PathNode currentParent = Parent;
        string newParentPath = newNodePath.GetParentPath();
        PathNode newParent = model.GetNode(newParentPath);

        if (currentParent != null)
        {
            currentParent.NodeList.Remove(this);
        }

        SetupNodePath(newNodePath);
        newParent?.InsertNodeSorted(this);

        results?.Add(new RenameItem(newNodePath, oldNodePath));

        return true;
    }

    /// <summary>
    /// Changes the node path and updates the parent relationship accordingly.
    /// </summary>
    /// <param name="newNodePath">The new path for this node.</param>
    /// <param name="results">A collection to store rename results.</param>
    public virtual void ChangeNodePath(string newNodePath, HashSet<RenameItem> results)
    {
        string oldNodePath = NodePath;

        PathNode parent = Parent;

        if (parent != null)
        {
            parent.NodeList.Remove(this);
        }

        SetupNodePath(newNodePath);

        parent?.InsertNodeSorted(this);

        results?.Add(new RenameItem(newNodePath, oldNodePath));
    }

    public virtual bool CanExpand => false;
    public virtual bool CanUserDrag => true;
    public virtual bool CanEditText => false;
    public virtual bool Reusable => false;
    public virtual object DisplayedValue => null;

    /// <summary>
    /// Called when the node path is set up, allowing derived classes to perform additional initialization.
    /// </summary>
    protected virtual void OnSetupNodePath(string nodePath)
    { }

    /// <summary>
    /// Called to retrieve the display text for this node.
    /// </summary>
    protected virtual string OnGetText() => _terminal;

    /// <summary>
    /// Called when the user requests to change the display text of this node.
    /// </summary>
    protected virtual void OnUserRequestChangeText(string text)
    { }

    /// <summary>
    /// Called when this node is added to a parent's node list.
    /// </summary>
    protected internal virtual void OnAdded()
    { }

    /// <summary>
    /// Called when this node is removed from a parent's node list.
    /// </summary>
    protected internal virtual void OnRemoved(PathNode fromParent)
    { }

    /// <summary>
    /// Inserts a node into this node's child collection in sorted order based on the node path.
    /// </summary>
    internal virtual void InsertNodeSorted(PathNode node)
    {
        if (node is null) throw new ArgumentNullException();

        for (int i = 0; i < NodeList.Count; i++)
        {
            PathNode currentNode = NodeList[i];
            if (currentNode == node)
            {
                return;
            }

            if (currentNode != null)
            {
                if (string.Compare(node.NodePath, currentNode.NodePath) < 0)
                {
                    NodeList.Insert(i, node);

                    return;
                }
            }
        }
        NodeList.Add(node);
    }

    /// <summary>
    /// Called to update the position of a child node within this node's collection.
    /// </summary>
    internal virtual void UpdateNodeOrder(PathNode node)
    { }

    #endregion

    #region View data access

    /// <summary>
    /// Gets or sets the display text for this node.
    /// </summary>
    public string Text
    {
        get => OnGetText();
        set => OnUserRequestChangeText(value);
    }

    /// <summary>
    /// Gets the text color status indicating how this node's text should be displayed.
    /// </summary>
    public virtual TextStatus TextColorStatus => TextStatus.Normal;
    /// <summary>
    /// Gets the color associated with this node's current status.
    /// </summary>
    public virtual Color? Color => EditorServices.ColorConfig.GetStatusColor(TextColorStatus);

    /// <summary>
    /// Gets the type name for this node.
    /// </summary>
    public virtual string TypeName => null;
    /// <summary>
    /// Gets the image icon associated with this node.
    /// </summary>
    public virtual ImageDef Image => null;

    /// <summary>
    /// Gets the status icon displayed alongside this node's text.
    /// </summary>
    public virtual ImageDef TextStatusIcon
    {
        get
        {
            switch (TextColorStatus)
            {
                case TextStatus.Info:
                    return CoreIconCache.LogInfo.ToIconSmall();

                case TextStatus.Warning:
                    return CoreIconCache.Warning.ToIconSmall();

                case TextStatus.Error:
                    return CoreIconCache.Error.ToIconSmall();

                case TextStatus.Comment:
                    return CoreIconCache.Comment.ToIconSmall();

                case TextStatus.Anonymous:
                    //return CoreIcons.FunctionAnonymous.GetIcon16();
                    return null;

                case TextStatus.Disabled:
                    //return CoreIcons.Disable.GetIcon16()
                    return null;

                case TextStatus.Import:
                    return CoreIconCache.Import.ToIconSmall();

                case TextStatus.Tag:
                    return CoreIconCache.Tag.ToIconSmall();

                case TextStatus.Reference:
                    return CoreIconCache.Reference;

                case TextStatus.FileReference:
                    return CoreIconCache.Reference.ToIconSmall();

                case TextStatus.EnumReference:
                    return CoreIconCache.Reference.ToIconSmall();

                case TextStatus.Add:
                    return CoreIconCache.New.ToIconSmall();

                case TextStatus.Remove:
                    return CoreIconCache.Disable.ToIconSmall();

                case TextStatus.Modify:
                    return CoreIconCache.Receive.ToIconSmall();

                case TextStatus.Normal:
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Gets a custom image for this node, overriding the default image.
    /// </summary>
    public virtual ImageDef CustomImage => null;

    #endregion

    /// <summary>
    /// Notify to update view
    /// </summary>
    public void NotifyView() => NotifyChanged();

    /// <summary>
    /// Update self status
    /// </summary>
    public virtual void UpdateStatus()
    { }

    /// <summary>
    /// Gets the underlying path object associated with this node from the model.
    /// </summary>
    public object GetPathObject()
    {
        var model = FindModel();

        return model?.GetPathObject(this);
    }

    /// <summary>
    /// Searches this node and its ancestors to find the first node of the specified type.
    /// </summary>
    public T FindMeOrParent<T>() where T : PathNode
    {
        PathNode node = this;
        while (node != null)
        {
            if (node is T t)
            {
                return t;
            }
            else
            {
                node = node.Parent;
            }
        }

        return default;
    }

    /// <summary>
    /// Determines whether the specified node is an ancestor of this node.
    /// </summary>
    public bool ContainsParent(PathNode parent)
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

    /// <summary>
    /// Returns a string representation of this node, which is the node path.
    /// </summary>
    public override string ToString() => NodePath;
}