using Suity.Editor;
using Suity.Editor.CodeRender;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui.Nodes;

#region WorkSpaceReferenceGroupNode

/// <summary>
/// Group node that contains all code generation reference items for a workspace.
/// </summary>
public class WorkSpaceReferenceGroupNode : PopulatePathNode, IWorkSpaceReferenceGroupNode, IDropTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceReferenceGroupNode"/> class.
    /// </summary>
    /// <param name="workSpace">The workspace this group belongs to.</param>
    public WorkSpaceReferenceGroupNode(WorkSpace workSpace)
    {
        WorkSpace = workSpace;
    }

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.Code.ToIconSmall();

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override TextStatus TextColorStatus => TextStatus.Reference;

    /// <summary>
    /// Gets the workspace associated with this reference group.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <inheritdoc/>
    protected override bool CanPopulate() => WorkSpace.ReferenceIds.Any();

    /// <inheritdoc/>
    protected override string OnGetText() => "Code Generation";

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        foreach (var id in WorkSpace.ReferenceIds)
        {
            var node = new WorkSpaceReferenceNode(WorkSpace, id);
            node.SetupNodePath(NodePath + "/" + id);

            yield return node;
        }
    }

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        bool hasFolder = e.GetDraggingNodes<DirectoryNode>().Any();

        // Type commonType = e.GetDraggingNodeCommonType();
        var referencable = from o in e.GetDraggingNodes<AssetFileNode>().Select(o => o.GetAsset())
                           where o != null && (o is IRenderable || o is IRenderTargetLibrary || o is IFileBunch)
                           select o;

        var referencable2 = from o in e.GetDraggingNodes<AssetElementNode>().Select(o => o.GetAsset())
                            where o != null && (o is IRenderable || o is IRenderTargetLibrary || o is IFileBunch)
                            select o;

        if (hasFolder || referencable.Any() || referencable2.Any())
        {
            e.SetLinkEffect();
        }
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        var project = Project.Current;

        var referencable = from o in e.GetDraggingNodes<AssetFileNode>().Select(o => o.GetAsset())
                           where o != null && (o is IRenderable || o is IRenderTargetLibrary || o is IFileBunch)
                           select o;

        var referencable2 = from o in e.GetDraggingNodes<AssetElementNode>().Select(o => o.GetAsset())
                            where o != null && (o is IRenderable || o is IRenderTargetLibrary || o is IFileBunch)
                            select o;

        foreach (var folder in e.GetDraggingNodes<DirectoryNode>())
        {
            var dirInfo = new DirectoryInfo(folder.NodePath);
            if (!dirInfo.Exists)
            {
                return;
            }

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                var o = FileAssetManager.Current.GetAsset(file.FullName);
                if (o != null && (o is IRenderable || o is IRenderTargetLibrary || o is IFileBunch))
                {
                    WorkSpace.AddReferenceItem(o.Id);
                }
            }
        }

        foreach (var reference in referencable.Union(referencable2))
        {
            WorkSpace.AddReferenceItem(reference.Id);
        }

        PopulateUpdate();

        var view = Device.Current.GetService<IProjectGui>();
        view.SelectNode(this, false);
        view.InspectSelectedNodes();
    }

    #endregion
}

#endregion

#region WorkSpaceReferenceNode

/// <summary>
/// Represents a single code generation reference item within a workspace.
/// </summary>
public class WorkSpaceReferenceNode : PathNode, IWorkSpaceReferenceNode, IDropTarget, IViewDoubleClickAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceReferenceNode"/> class.
    /// </summary>
    /// <param name="workSpace">The workspace this reference belongs to.</param>
    /// <param name="id">The asset ID of the reference.</param>
    public WorkSpaceReferenceNode(WorkSpace workSpace, Guid id)
    {
        Debug.Assert(workSpace != null);
        WorkSpace = workSpace;
        Id = id;
    }

    /// <summary>
    /// Gets the unique identifier of this reference asset.
    /// </summary>
    public Guid Id { get; }

    /// <inheritdoc/>
    public override Image Image
    {
        get
        {
            Asset context = GetReferenceAsset();
            if (context != null)
            {
                return context.Icon?.ToIconSmall();
            }
            else
            {
                return CoreIconCache.Warning.ToIconSmall();
            }
        }
    }

    /// <summary>
    /// Gets the extended image indicating special reference properties.
    /// </summary>
    public Image ImageEx
    {
        get
        {
            var item = GetReferenceItem();
            if (item != null)
            {
                if (item.UploadMode)
                {
                    return CoreIconCache.DataBaseUpdate.ToIconSmall();
                }
                else if (item.AutoRestoreUserCode && item.UserCode != null)
                {
                    return CoreIconCache.Restore.ToIconSmall();
                }
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override TextStatus TextColorStatus
    {
        get
        {
            Asset context = GetReferenceAsset();
            if (context != null)
            {
                IWorkSpaceRefItem r = GetReferenceItem();
                if (r?.Enabled == true)
                {
                    if (r.Suspended)
                    {
                        return TextStatus.Comment;
                    }
                    else
                    {
                        return TextStatus.Normal;
                    }
                }
                else
                {
                    return TextStatus.Disabled;
                }
            }
            else
            {
                return TextStatus.Error;
            }
        }
    }

    /// <summary>
    /// Gets the workspace associated with this reference.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <summary>
    /// Gets the asset object for this reference.
    /// </summary>
    /// <returns>The asset, or null if not found.</returns>
    public Asset GetReferenceAsset() => AssetManager.Instance.GetAsset(Id);

    /// <summary>
    /// Gets the workspace reference item.
    /// </summary>
    /// <returns>The reference item, or null if not found.</returns>
    public IWorkSpaceRefItem GetReferenceItem() => WorkSpace.GetReferenceItem(Id);

    /// <summary>
    /// Removes this reference from the workspace.
    /// </summary>
    public void RemoveReference()
    {
        WorkSpace.RemoveReferenceItem(Id);
    }

    /// <inheritdoc/>
    protected override string OnGetText() => Id.ToDescriptionText();

    #region IDropTarget

    /// <inheritdoc/>
    async void IDropTarget.DragDrop(IDragEvent e)
    {
        var nodes = e.GetDraggingNodes<PathNode>();

        if (GetReferenceItem() is FileBunchRefItem bunchItem)
        {
            if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
            {
                if (AssetRootNode.CanCreateFileBunch(nodes))
                {
                    string fileName = Project.Current.AssetDirectory.PathAppend(bunchItem.Key);

                    await AssetRootNode.HandleCreateOrUpdateFileBunch(nodes, fileName);
                }
                else
                {
                    e.SetNoneEffect();
                }
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else
        {
            var view = Device.Current.GetService<IProjectGui>();
            view.DragDrop(e);
        }
    }

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        var nodes = e.GetDraggingNodes<PathNode>();

        if (GetReferenceItem() is FileBunchRefItem)
        {
            if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
            {
                if (AssetRootNode.CanCreateFileBunch(nodes))
                {
                    e.SetCopyEffect();
                }
                else
                {
                    e.SetNoneEffect();
                }
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else
        {
            var view = Device.Current.GetService<IProjectGui>();
            view.DragOver(e);
        }
    }

    #endregion

    #region IViewDoubleClickAction

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        EditorUtility.GotoDefinition(this.GetReferenceAsset());
    }

    #endregion
}

#endregion

#region WorkSpaceAssemblyGroupNode

/// <summary>
/// Group node that contains all assembly reference items for a workspace.
/// </summary>
public class WorkSpaceAssemblyGroupNode : PopulatePathNode, IDropTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceAssemblyGroupNode"/> class.
    /// </summary>
    /// <param name="workSpace">The workspace this group belongs to.</param>
    public WorkSpaceAssemblyGroupNode(WorkSpace workSpace)
    {
        WorkSpace = workSpace;
    }

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.Assembly.ToIconSmall();

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override TextStatus TextColorStatus => TextStatus.FileReference;

    /// <summary>
    /// Gets the workspace associated with this assembly group.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <inheritdoc/>
    protected override bool CanPopulate() => WorkSpace.AssemblyReferenceItems.Any();

    /// <inheritdoc/>
    protected override string OnGetText() => "Assembly References";

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        foreach (var refItem in WorkSpace.AssemblyReferenceItems)
        {
            var node = new WorkSpaceAssemblyNode(WorkSpace, refItem);
            node.SetupNodePath(NodePath + "/" + refItem.Key);

            yield return node;
        }
    }

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        var project = Project.Current;

        var referencable = from o in e.GetDraggingNodes<AssetFileNode>().Select(o => o.GetAsset())
                           where o != null && (o is IAssemblyReference)
                           select o;

        var referencable2 = from o in e.GetDraggingNodes<AssetElementNode>().Select(o => o.GetAsset())
                            where o != null && (o is IAssemblyReference)
                            select o;

        foreach (DirectoryNode folder in e.GetDraggingNodes<DirectoryNode>())
        {
            var dirInfo = new DirectoryInfo(folder.NodePath);
            if (!dirInfo.Exists)
            {
                return;
            }

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                var o = FileAssetManager.Current.GetAsset(file.FullName);
                if (o != null && (o is IAssemblyReference))
                {
                    WorkSpace.AddAssemblyReference(o.Id);
                }
            }
        }

        foreach (var reference in referencable.Union(referencable2))
        {
            WorkSpace.AddAssemblyReference(reference.Id);
        }

        PopulateUpdate();

        var view = Device.Current.GetService<IProjectGui>();
        view.SelectNode(this, false);
        view.InspectSelectedNodes();
    }

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        bool hasFolder = e.GetDraggingNodes<DirectoryNode>().Any();

        // Type commonType = e.GetDraggingNodeCommonType();
        var referencable = from o in e.GetDraggingNodes<AssetFileNode>().Select(o => o.GetAsset())
                           where o != null && (o is IAssemblyReference)
                           select o;

        var referencable2 = from o in e.GetDraggingNodes<AssetElementNode>().Select(o => o.GetAsset())
                            where o != null && (o is IAssemblyReference)
                            select o;

        if (hasFolder || referencable.Any() || referencable2.Any())
        {
            e.SetLinkEffect();
        }
    }

    #endregion
}

#endregion

#region WorkSpaceAssemblyNode

/// <summary>
/// Represents a single assembly reference item within a workspace.
/// </summary>
public class WorkSpaceAssemblyNode : PathNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceAssemblyNode"/> class.
    /// </summary>
    /// <param name="workSpace">The workspace this assembly reference belongs to.</param>
    /// <param name="refItem">The assembly reference item.</param>
    public WorkSpaceAssemblyNode(WorkSpace workSpace, IAssemblyReferenceItem refItem)
    {
        Debug.Assert(workSpace != null);
        WorkSpace = workSpace ?? throw new ArgumentNullException(nameof(workSpace));
        RefItem = refItem ?? throw new ArgumentNullException(nameof(refItem));
    }

    /// <summary>
    /// Gets the assembly reference item.
    /// </summary>
    public IAssemblyReferenceItem RefItem { get; }

    /// <inheritdoc/>
    public override Image Image
    {
        get
        {
            if (RefItem.IsValid)
            {
                return RefItem.Icon?.ToIconSmall();
            }
            else
            {
                return CoreIconCache.Warning.ToIconSmall();
            }
        }
    }

    /// <summary>
    /// Gets the extended image for this node (always null).
    /// </summary>
    public Image ImageEx => null;

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override TextStatus TextColorStatus => RefItem.IsValid ? TextStatus.Normal : TextStatus.Error;

    /// <summary>
    /// Gets the workspace associated with this assembly reference.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <summary>
    /// Removes this assembly reference from the workspace.
    /// </summary>
    public virtual void RemoveReference()
    {
        WorkSpace.RemoveAssemblyReference(RefItem.Key);
    }

    /// <inheritdoc/>
    protected override string OnGetText() => RefItem.Name;
}

#endregion
