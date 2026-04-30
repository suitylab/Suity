using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
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

#region WorkSpaceManagerNode

/// <summary>
/// Root node that manages and displays all workspace nodes in the project tree.
/// </summary>
[ToolTipsText("Workspace Folder")]
public class WorkSpaceManagerNode : PopulatePathNode, IWorkSpaceManagerNode
{
    private readonly List<WorkSpaceRootNode> _workSpaceNodes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceManagerNode"/> class.
    /// </summary>
    /// <param name="path">The node path for this workspace manager.</param>
    public WorkSpaceManagerNode(string path)
    {
        SetupNodePath(path);
    }

    /// <inheritdoc/>
    protected override string OnGetText() => "Workspaces";

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.WorkSpace.ToIconSmall();

    /// <summary>
    /// Gets the collection of workspace root nodes managed by this node.
    /// </summary>
    public IEnumerable<WorkSpaceRootNode> WorkSpceNodes => _workSpaceNodes;

    /// <summary>
    /// Adds a workspace to this manager and creates its corresponding node.
    /// </summary>
    /// <param name="space">The workspace to add.</param>
    public void AddWorkSpace(WorkSpace space)
    {
        var node = new WorkSpaceRootNode(space);
        _workSpaceNodes.InsertSorted(node, CompareWorkSpaceRootNode);
    }

    /// <summary>
    /// Clears all workspace nodes from this manager.
    /// </summary>
    public void Clear()
    {
        _workSpaceNodes.Clear();
    }

    /// <summary>
    /// Removes a workspace and its corresponding node from this manager.
    /// </summary>
    /// <param name="space">The workspace to remove.</param>
    public void RemoveWorkSpace(WorkSpace space)
    {
        _workSpaceNodes.RemoveAll(o => o.WorkSpace == space);
    }

    /// <inheritdoc/>
    protected override bool CanPopulate() => _workSpaceNodes.Count > 0;

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        return _workSpaceNodes;

        //foreach (var node in _workSpaceNodes.Where(o => o.WorkSpace.Controller is PlanetController).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
        //foreach (var node in _workSpaceNodes.Where(o => o.WorkSpace.Controller is SateliteController).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
        //foreach (var node in _workSpaceNodes.Where(o => o.WorkSpace.Controller is SpaceshipController).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
        //foreach (var node in _workSpaceNodes.Where(o => o.WorkSpace.Controller is AstronautController).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
        //foreach (var node in _workSpaceNodes.Where(o => o.WorkSpace.Controller is SuityController).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
        //foreach (var node in _workSpaceNodes.Where(o => !(o.WorkSpace.Controller is CSharpController)).OrderBy(o => o.WorkSpace.Name))
        //{
        //    yield return node;
        //}
    }

    /// <summary>
    /// Compares two workspace root nodes for sorting by order and name.
    /// </summary>
    /// <param name="a">The first node to compare.</param>
    /// <param name="b">The second node to compare.</param>
    /// <returns>A negative value if a should come before b, positive if after, zero if equal.</returns>
    private int CompareWorkSpaceRootNode(WorkSpaceRootNode a, WorkSpaceRootNode b)
    {
        int orderA = a.WorkSpace.Order;
        int orderB = b.WorkSpace.Order;

        int c = orderB.CompareTo(orderA);
        if (c != 0)
        {
            return c;
        }

        return a.WorkSpace.Name.CompareTo(b.WorkSpace.Name);
    }
}

#endregion

#region WorkSpaceRootNode

/// <summary>
/// Root node representing a single workspace in the project tree.
/// TODO: Listen to file change workflow.
/// </summary>
public class WorkSpaceRootNode : RootDirectoryNode, IWorkSpaceRootNode, IDropTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceRootNode"/> class.
    /// </summary>
    /// <param name="space">The workspace this node represents.</param>
    public WorkSpaceRootNode(WorkSpace space)
        : base(space.MasterDirectory)
    {
        Debug.Assert(space != null);

        WorkSpace = space;
        space.RenderTargetUpdated += Space_RenderTargetUpdated;
        space.DependencyRenamed += Space_DependencyRenamed;
        space.DependencyUpdated += Space_DependencyUpdated;
        space.MasterBasePathUpdated += Space_MasterBasePathUpdated;
        EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(this));
    }

    /// <inheritdoc/>
    public override bool CanEditText => true;

    /// <inheritdoc/>
    public override Image Image => WorkSpace?.Icon?.ToIconSmall() ?? CoreIconCache.Folder.ToIconSmall();

    /// <inheritdoc/>
    public override Image TextStatusIcon
    {
        get
        {
            if (WorkSpace is null)
            {
                return null;
            }

            var state = WorkSpace.GetDirectoryStatus(string.Empty);
            if (state != FileState.None)
            {
                return WorkSpaceFileNode.GetIconByFileState(state);
            }

            if (WorkSpace.Controller?.IsProjectDirty == true)
            {
                return CoreIconCache.Config;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the workspace associated with this node.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <summary>
    /// Handles drag-and-drop operations for workspace project nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static void HandleProjectDragDrop(DirectoryNode dirNode, IDragEvent e)
    {
        var view = Device.Current.GetService<IProjectGui>();

        var nodes = e.GetDraggingNodes<PathNode>();
        if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
        {
            view.DragDrop(e);
        }
        else
        {
            e.SetNoneEffect();
        }
    }

    /// <summary>
    /// Handles drag-over operations for workspace project nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static void HandleProjectDragOver(DirectoryNode dirNode, IDragEvent e)
    {
        var view = Device.Current.GetService<IProjectGui>();

        var nodes = e.GetDraggingNodes<PathNode>();
        if (nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
        {
            view.DragOver(e);
        }
        else
        {
            e.SetNoneEffect();
        }
    }

    /// <inheritdoc/>
    public override void Delete(bool sendToRecycleBin)
    {
        WorkSpace.Manager.DeleteWorkSpace(WorkSpace.Name);
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        WorkSpaceRootNode.HandleProjectDragDrop(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        WorkSpaceRootNode.HandleProjectDragOver(this, e);
    }


    private bool _canPopLast;

    /// <inheritdoc/>
    protected override bool CanPopulateDirectory(DirectoryInfo directory)
    {
        WorkSpace space = WorkSpace;
        if (space is null || space.Controller is null)
        {
            return false;
        }

        if (space.Controller.GetIsPathHidden(directory.FullName))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override bool CanPopulateFile(FileInfo file)
    {
        // Hide workspace config and database files
        if (string.Compare(file.Name, WorkSpace.DefaultWorkSpaceConfigFileName, true) == 0)
        {
            return false;
        }

        if (string.Compare(file.Name, WorkSpace.DefaultWorkSpaceDbFileName, true) == 0)
        {
            return false;
        }

        WorkSpace space = WorkSpace;
        if (space is null || space.Controller is null)
        {
            return false;
        }

        if (space.Controller.GetIsPathHidden(file.FullName))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode() => new WorkSpaceDirectoryNode();

    /// <inheritdoc/>
    protected override FileNode CreateFileNode() => new WorkSpaceFileNode();

    /// <inheritdoc/>
    protected internal override void OnAdded()
    {
        base.OnAdded();
    }

    /// <inheritdoc/>
    protected internal override void OnRemoved(PathNode fromParent)
    {
        base.OnRemoved(fromParent);
    }

    /// <inheritdoc/>
    protected internal override void OnFileSystemChanged(string fullPath)
    {
        WorkSpace.MarkFileAsModified(fullPath.MakeRalativePath(WorkSpace.MasterDirectory));
    }

    /// <inheritdoc/>
    protected internal override void OnFileSystemCreated(string fullPath)
    {
        WorkSpace.MarkFileAsModified(fullPath.MakeRalativePath(WorkSpace.MasterDirectory));
    }

    /// <inheritdoc/>
    protected internal override void OnFileSystemDeleted(string fullPath)
    {
        WorkSpace.MarkFileAsModified(fullPath.MakeRalativePath(WorkSpace.MasterDirectory));
    }

    /// <inheritdoc/>
    protected internal override void OnFileSystemRenamed(string fullPath, string oldFullPath)
    {
        WorkSpace.MarkFileAsModified(fullPath.MakeRalativePath(WorkSpace.MasterDirectory));
        WorkSpace.MarkFileAsModified(oldFullPath.MakeRalativePath(WorkSpace.MasterDirectory));
    }

    /// <inheritdoc/>
    protected override string OnGetText()
    {
        //var count = WorkSpace.DirtyRenderTargetCount;
        //if (count > 0)
        //{
        //    return $"{WorkSpace.Name} ({count})";
        //}
        //else
        //{
        return WorkSpace.Name;
        //}
    }

    /// <inheritdoc/>
    protected override bool CanPopulate()
    {
        WorkSpace space = WorkSpace;
        if (space is null || space.Controller is null || space.IsFailure)
        {
            return false;
        }

        _canPopLast = true;

        return true;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        WorkSpace space = WorkSpace;
        if (space is null || space.Controller is null)
        {
            yield break;
        }

        // Reference group and assembly group nodes are currently disabled
        /*        var refGroup = new WorkSpaceReferenceGroupNode(space);
        refGroup.SetupNodePath(NodePath + "/$ref");
        yield return refGroup;

        if (space.AssemblyReferenceEnabled)
        {
            var assemblyGroup = new WorkSpaceAssemblyGroupNode(space);
            assemblyGroup.SetupNodePath(NodePath + "/$assembly");
            yield return assemblyGroup;
        }*/

        foreach (var item in base.OnPopulate())
        {
            yield return item;
        }

        if (space != null)
        {
            string rPath = string.Empty;

            // Yield render directories that don't physically exist yet
            foreach (string dir in space.GetRenderDirectoryByDirectory(rPath))
            {
                if (string.IsNullOrEmpty(dir))
                {
                    continue;
                }

                string dirTerminal = dir.GetPathTerminal();
                string fullPath = NodePath.PathAppend(dirTerminal);
                if (!Directory.Exists(fullPath))
                {
                    var dirNode = new WorkSpaceDirectoryNode();
                    dirNode.SetupNodePath(NodePath.PathAppend(dirTerminal));
                    yield return dirNode;
                }
            }

            // Yield render targets that don't physically exist yet
            foreach (var renderTarget in space.GetRenderTargetsByDirectory(rPath))
            {
                if (string.IsNullOrEmpty(renderTarget.FileName.PhysicFullPath))
                {
                    continue;
                }

                if (!File.Exists(renderTarget.FileName.PhysicFullPath))
                {
                    var fileNode = new WorkSpaceFileNode();
                    fileNode.SetupNodePath(renderTarget.FileName.PhysicFullPath);
                    yield return fileNode;
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnUserRequestChangeText(string newName)
    {
        if (newName == WorkSpace.Name)
        {
            return;
        }

        string newPath = WorkSpace.Manager.BasePath.PathAppend(newName);
        if (Directory.Exists(newPath))
        {
            DialogUtility.ShowMessageBoxAsyncL("Folder already exists.");
            return;
        }

        if (!WorkSpace.Manager.CanRenameWorkSpace(WorkSpace.Name, newName))
        {
            DialogUtility.ShowMessageBoxAsyncL("Cannot rename workspace.");
            return;
        }

        QueuedAction.Do(() =>
        {
            WorkSpace.Manager.RenameWorkSpace(WorkSpace.Name, newName);
        });
    }

    /// <summary>
    /// Handles the render target updated event from the workspace.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    private void Space_RenderTargetUpdated(object sender, EventArgs args)
    {
        EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(this));
    }

    /// <summary>
    /// Handles the dependency renamed event from the workspace.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    private void Space_DependencyRenamed(object sender, EventArgs args)
    {
        PopulateUpdateDeep();
    }

    /// <summary>
    /// Handles the dependency updated event from the workspace.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    private void Space_DependencyUpdated(object sender, EventArgs args)
    {
        //EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(this));
    }

    /// <summary>
    /// Handles the master base path updated event from the workspace.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    private void Space_MasterBasePathUpdated(object sender, EventArgs args)
    {
        ClearPopulate();
        ChangeNodePath(WorkSpace.MasterDirectory, null);
        Populate();
    }
}

#endregion

#region WorkSpaceDirectoryNode

/// <summary>
/// Represents a directory node within a workspace in the project tree.
/// </summary>
public class WorkSpaceDirectoryNode : DirectoryNode, IWorkSpaceDirectoryNode, IDropTarget
{
    private bool _cached;

    private Image _imageEx;

    private Image _imageStatus;

    // Because finding WorkSpace takes time, the filter using WorkSpace is cached
    private Predicate<string> _tempPopuldateFilter;

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.Folder.ToIconSmall();

    /// <inheritdoc/>
    public override Image CustomImage
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }
            return _imageEx;
        }
    }

    /// <inheritdoc/>
    public override Image TextStatusIcon
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }
            return _imageStatus;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this directory does not physically exist yet.
    /// </summary>
    public bool IsNew
    {
        get
        {
            return !Directory.Exists(NodePath);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this directory contains render targets.
    /// </summary>
    public bool IsRendering
    {
        get
        {
            var space = FindWorkSpace();
            if (space != null)
            {
                string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
                return space.ContainsRenderDirectory(rPath);
            }

            return false;
        }
    }

    /// <inheritdoc/>
    WorkSpace IWorkSpaceNode.WorkSpace => FindWorkSpace();

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        WorkSpaceRootNode.HandleProjectDragDrop(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        WorkSpaceRootNode.HandleProjectDragOver(this, e);
    }

    /// <summary>
    /// Finds the workspace that contains this directory node.
    /// </summary>
    /// <returns>The parent workspace, or null if not found.</returns>
    public WorkSpace FindWorkSpace()
    {
        return FindMeOrParent<WorkSpaceRootNode>()?.WorkSpace;
    }

    /// <inheritdoc/>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        bool ok = base.MoveNode(newNodePath, results);

        if (Parent is PopulatePathNode populateNode)
        {
            EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(populateNode));
        }

        return ok;
    }

    /// <inheritdoc/>
    public override void UpdateStatus()
    {
        _cached = false;
    }

    /// <inheritdoc/>
    protected override bool CanPopulate()
    {
        if (base.CanPopulate())
        {
            return true;
        }
        WorkSpace space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            return space.GetRenderTargetsByDirectory(rPath).Any() || space.GetRenderDirectoryByDirectory(rPath).Any();
        }

        return false;
    }

    /// <inheritdoc/>
    protected override bool CanPopulateDirectory(DirectoryInfo directory)
    {
        return _tempPopuldateFilter?.Invoke(directory.FullName) ?? true;
    }

    /// <inheritdoc/>
    protected override bool CanPopulateFile(FileInfo file)
    {
        return _tempPopuldateFilter?.Invoke(file.FullName) ?? true;
    }

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode()
    {
        return new WorkSpaceDirectoryNode();
    }

    /// <inheritdoc/>
    protected override FileNode CreateFileNode()
    {
        return new WorkSpaceFileNode();
    }

    /// <inheritdoc/>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        WorkSpace space = FindWorkSpace();

        _tempPopuldateFilter = fileName => !space.Controller.GetIsPathHidden(fileName);
        foreach (var item in base.OnPopulate())
        {
            yield return item;
        }

        _tempPopuldateFilter = null;

        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);

            // Yield render directories that don't physically exist yet
            foreach (var dir in space.GetRenderDirectoryByDirectory(rPath))
            {
                string dirTerminal = dir.GetPathTerminal();
                string fullPath = NodePath.PathAppend(dirTerminal);
                if (!Directory.Exists(fullPath))
                {
                    var dirNode = new WorkSpaceDirectoryNode();
                    dirNode.SetupNodePath(NodePath.PathAppend(dirTerminal));

                    yield return dirNode;
                }
            }

            // Yield render targets that don't physically exist yet
            foreach (var renderTarget in space.GetRenderTargetsByDirectory(rPath))
            {
                if (!File.Exists(renderTarget.FileName.PhysicFullPath))
                {
                    var fileNode = new WorkSpaceFileNode();
                    fileNode.SetupNodePath(renderTarget.FileName.PhysicFullPath);

                    yield return fileNode;
                }
            }
        }
    }

    /// <summary>
    /// Creates cached image data for this node.
    /// </summary>
    private void CreateCache()
    {
        _cached = true;
        _imageEx = GetImageEx()?.ToIconSmall();
        _imageStatus = GetImageStatus()?.ToIconSmall();
    }

    /// <summary>
    /// Gets the extended image indicating rendering status.
    /// </summary>
    /// <returns>The extended image, or null if not rendering.</returns>
    private Image GetImageEx()
    {
        if (IsRendering)
        {
            return Suity.Editor.ProjectGui.Properties.IconCache.Rendering;
        }
        return null;
    }

    /// <summary>
    /// Gets the status image based on file state.
    /// </summary>
    /// <returns>The status image, or null if no status to display.</returns>
    private Image GetImageStatus()
    {
        if (!Directory.Exists(NodePath))
        {
            return CoreIconCache.New;
        }

        var space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            FileState state = space.GetDirectoryStatus(rPath);
            return WorkSpaceFileNode.GetIconByFileState(state);
        }

        return null;
    }
}

#endregion

#region WorkSpaceFileNode

/// <summary>
/// Represents a file node within a workspace in the project tree.
/// </summary>
public class WorkSpaceFileNode : FileNode, IWorkSpaceFileNode
{
    private bool _cached;
    private Image _image;
    private Image _imageEx;
    private Image _imageStatus;
    private TextStatus _textStatus;

    /// <inheritdoc/>
    public override Image Image
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }

            return _image;
        }
    }

    /// <inheritdoc/>
    public override Image CustomImage
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }

            return _imageEx;
        }
    }

    /// <inheritdoc/>
    public override Image TextStatusIcon
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }

            return _imageStatus;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this file does not physically exist yet.
    /// </summary>
    public bool IsNew => !File.Exists(NodePath);

    /// <summary>
    /// Gets a value indicating whether this file has render targets.
    /// </summary>
    public bool IsRendering
    {
        get
        {
            var space = FindWorkSpace();
            if (space != null)
            {
                string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
                return space.GetRenderTargets(rPath).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Gets all render targets associated with this file.
    /// </summary>
    /// <returns>An array of render targets, or null if no workspace is found.</returns>
    public RenderTarget[] GetRenderTargets()
    {
        var space = FindWorkSpace();
        if (space is null)
        {
            return null;
        }

        string rPath = NodePath.MakeRalativePath(space.MasterDirectory);

        return space.GetRenderTargets(rPath).ToArray();
    }

    /// <summary>
    /// Gets the first render target associated with this file.
    /// </summary>
    /// <returns>The first render target, or null if none found.</returns>
    public RenderTarget GetRenderTarget()
    {
        var space = FindWorkSpace();
        if (space is null)
        {
            return null;
        }

        string rPath = NodePath.MakeRalativePath(space.MasterDirectory);

        return space.GetRenderTargets(rPath).FirstOrDefault();
    }

    /// <summary>
    /// Gets the file bunch associated with this file's render target.
    /// </summary>
    /// <returns>The file bunch, or null if not available.</returns>
    public IFileBunch GetFileBunch() => GetRenderTarget()?.FileBunch;

    /// <summary>
    /// Gets the file state within the workspace.
    /// </summary>
    public FileState SpaceFileStatus
    {
        get
        {
            var space = FindWorkSpace();
            if (space != null)
            {
                string rPath = NodePath.MakeRalativePath(space.MasterDirectory);

                return space.GetFileStatus(rPath);
            }
            else
            {
                return FileState.None;
            }
        }
    }

    /// <inheritdoc/>
    public override TextStatus TextColorStatus
    {
        get
        {
            if (!_cached)
            {
                CreateCache();
            }

            return _textStatus;
        }
    }

    /// <inheritdoc/>
    WorkSpace IWorkSpaceNode.WorkSpace => FindWorkSpace();

    /// <summary>
    /// Binds this file as a render file in the workspace.
    /// </summary>
    public void BindRenderFile()
    {
        var space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            space.BindRenderFile(rPath);
        }
    }

    /// <summary>
    /// Finds the workspace that contains this file node.
    /// </summary>
    /// <returns>The parent workspace, or null if not found.</returns>
    public WorkSpace FindWorkSpace()
    {
        return FindMeOrParent<WorkSpaceRootNode>()?.WorkSpace;
    }

    /// <inheritdoc/>
    public override bool MoveNode(string newNodePath, HashSet<RenameItem> results)
    {
        ClearPopulate();
        // If renamed and then PopulateDummy() is executed->CanPopulate() is called->Asset not found

        bool ok = base.MoveNode(newNodePath, results);

        Populate();

        if (Parent is PopulatePathNode populateNode)
        {
            EditorUtility.AddDelayedAction(new DelayRefreshNodeDeepAction(populateNode));
        }

        return ok;
    }

    /// <summary>
    /// Unbinds this file as a render file in the workspace.
    /// </summary>
    public void UnbindRenderFile()
    {
        var space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            space.UnbindRenderFile(rPath);
        }
    }

    /// <inheritdoc/>
    public override void UpdateStatus()
    {
        _cached = false;
    }

    /// <summary>
    /// Gets the icon corresponding to a file state.
    /// </summary>
    /// <param name="state">The file state.</param>
    /// <returns>The icon for the given state, or null if none.</returns>
    internal static Image GetIconByFileState(FileState state) => state switch
    {
        FileState.None => null,
        FileState.User => null,
        FileState.Add => CoreIconCache.New.ToIconSmall(),
        FileState.Update => CoreIconCache.Receive.ToIconSmall(),
        FileState.Remove => CoreIconCache.Disable.ToIconSmall(),
        FileState.Exist => null,
        FileState.Duplicated => CoreIconCache.Duplicated.ToIconSmall(),
        FileState.UserOccupied => CoreIconCache.Occupied.ToIconSmall(),
        FileState.Warning => CoreIconCache.Warning.ToIconSmall(),
        FileState.Modified => CoreIconCache.Modify.ToIconSmall(),
        _ => null,
    };

    /// <summary>
    /// Gets the document entry associated with this file.
    /// </summary>
    /// <returns>The document entry, or null if not found.</returns>
    internal DocumentEntry GetDocument()
    {
        return DocumentManager.Instance.GetDocument(NodePath);
    }

    /// <summary>
    /// Creates cached image and status data for this node.
    /// </summary>
    private void CreateCache()
    {
        _cached = true;
        _image = GetImage();
        _imageEx = GetImageEx();
        _imageStatus = GetImageStatus();
        _textStatus = GetTextStatus();
    }

    /// <summary>
    /// Gets the default file icon.
    /// </summary>
    /// <returns>The file icon image.</returns>
    private Image GetImage()
    {
        return EditorUtility.GetIconForFileExact(NodePath)?.ToIconSmall();
    }

    /// <summary>
    /// Gets the extended image indicating rendering status.
    /// </summary>
    /// <returns>The extended image, or null if not rendering.</returns>
    private Image GetImageEx()
    {
        RenderTarget target = GetRenderTarget();
        if (target != null)
        {
            if (target.FileBunch != null)
            {
                return Suity.Editor.ProjectGui.Properties.IconCache.RenderingBunch.ToIconSmall();
            }
            else
            {
                return Suity.Editor.ProjectGui.Properties.IconCache.Rendering.ToIconSmall();
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the status image based on file state and render status.
    /// </summary>
    /// <returns>The status image, or null if no status to display.</returns>
    private Image GetImageStatus()
    {
        //if (!File.Exists(NodePath))
        //{
        //    return CoreIcons.New;
        //}

        var space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            FileState state = space.GetFileStatus(rPath);

            if (state == FileState.Exist)
            {
                RenderStatus op = space.GetFileRenderStatus(rPath);
                return op switch
                {
                    RenderStatus.SameAndDbUpdated => CoreIconCache.DataBaseUpdate.ToIconSmall(),
                    RenderStatus.ContainsDbLegacy => CoreIconCache.Warning.ToIconSmall(),
                    RenderStatus.Same => CoreIconCache.Equal.ToIconSmall(),
                    RenderStatus.Success => CoreIconCache.Save.ToIconSmall(),
                    RenderStatus.ErrorInterrupt or RenderStatus.ErrorContinue => CoreIconCache.Error.ToIconSmall(),
                    _ => null,
                };
            }
            else
            {
                return GetIconByFileState(state);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the text color status based on render target type.
    /// </summary>
    /// <returns>The text status for this file node.</returns>
    private TextStatus GetTextStatus()
    {
        var space = FindWorkSpace();
        if (space != null)
        {
            string rPath = NodePath.MakeRalativePath(space.MasterDirectory);
            var target = space.GetRenderTargets(rPath).FirstOrDefault();
            if (target != null)
            {
                if (target.FileBunch != null)
                {
                    return TextStatus.FileReference;
                }
                else
                {
                    return TextStatus.Reference;
                }
            }
        }
        return TextStatus.Normal;
    }
}

#endregion
