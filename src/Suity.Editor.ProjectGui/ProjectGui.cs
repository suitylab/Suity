using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.ProjectGui.Commands;
using Suity.Editor.ProjectGui.Commands.FileBunchs;
using Suity.Editor.ProjectGui.Commands.WorkSpaces;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Gui;
using Suity.Views.Im;
using Suity.Views.Im.TreeEditing;
using Suity.Views.Named;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// Main GUI component for the project view, displaying the project tree and handling file/workspace navigation.
/// </summary>
public partial class ProjectGui :
    IProjectGui,
    IToolWindow,
    IDrawImGui,
    IInspectorContext,
    IDrawContext
{
    private const string ToolWindowId = "Project";

    /// <summary>
    /// Gets the unique identifier for this tool window.
    /// </summary>
    public static readonly string InitPath = string.Empty;

    private readonly ImGuiNodeRef _guiRef = new();
    private ImGuiTheme _theme;
    private HeaderlessPathTreeView _treeView;

    private Project _project;

    private readonly ImGuiPathTreeModel _model = new();
    private AssetRootNode _assetRootNode;
    private WorkSpaceManagerNode _workSpaceManagerNode;
    private PublishRootNode _publishRootNode;

    private readonly List<object> _inspectorObjects = [];
    private readonly HashSet<Type> _selectionTypes = [];

    private PathNodeExpandState _expandState = new();

    private DelayRefreshProject _refreshAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectGui"/> class, setting up the tree view, event handlers, and command mappings.
    /// </summary>
    public ProjectGui()
    {
        _theme = EditorUtility.GetEditorImGuiTheme();

        var menu = new ProjectRootCommand();

        _treeView = new HeaderlessPathTreeView(_model)
        {
            ContentGui = ItemContentGui,

            Menu = menu,
            MenuSender = this,
            MenuSelection = () => _treeView.SelectedNodes,
        };

        _treeView.SelectionChanged += (s, e) => InspectSelectedNodes();
        _treeView.OpenRequest += pathNode => HandlePathViewDoubleClick(pathNode);
        _treeView.DeleteRequest += pathNode =>
        {
            DeleteDirFileCommand.HandleDelete(this);
            WsRefDeleteCommand.HandleDelete(this);
            DeleteBunchFileCommand.HandleDelete(this);
        };

        _model.TreeChanged += (s, e) => _guiRef.QueueRefresh();

        EditorRexes.UIStarted.AddActionListener(HandleUIStart);
        EditorRexes.ProjectOpened.AddActionListener(HandleProjectOpened);
        EditorRexes.ProjectClosing.AddActionListener(HandleProjectClosing);
        EditorServices.FileUpdateService.UpdateFinished += (s, e) => OnProjectFileUpdated();

        EditorCommands.Mapper.ProvideHandler<LocateInProjectVReq>(HandleLocateInProjectVReq);
        EditorCommands.Mapper.ProvideHandler<LocateWorkSpaceVReq>(HandleLocateWorkSpaceVReq);
        EditorCommands.Mapper.ProvideHandler<LocateProjectNodeVReq>(HandleLocateProjectNodeVReq);

        DocumentManager.Instance.DocumentNew += Current_DocumentNew;

        ICodeRenderInfoService buildInfo = Device.Current.GetService<ICodeRenderInfoService>();
        if (buildInfo != null)
        {
            buildInfo.RenderInfoUpdated += BuildInfo_BuildInfoUpdated;
        }

        AssetManager.Instance.AssetUpdated += Project_AssetUpdated;

        _refreshAction = new DelayRefreshProject(this);

        EditorRexes.RefreshProjectView.AddQueuedActionListener(() =>
        {
            RefreshProjectNodes();
            RefreshWorkSpaceNodes();
        });

        EditorRexes.Mapper.Provide<IProjectGui>(this);
    }


    /// <summary>
    /// Gets the underlying path tree model for the project view.
    /// </summary>
    public PathTreeModel Model => _model;

    /// <summary>
    /// Gets the currently opened project.
    /// </summary>
    public Project CurrentProject => _project;

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        if (_guiRef.Node is null)
        {
            // Pre-generate menu
            (gui.Context as IGraphicContextMenu)?.RegisterContextMenu(_treeView.Menu);
        }

        _guiRef.Node = gui.Frame("projectView")
        .OnInitialize(n =>
        {
            n.InitTheme(_theme);
            n.InitClass("editorBg");
            n.InitFullWidth();

            // For some reason, it's not fully displayed
            // n.QueueRefresh();
        })
        .OnContent(() =>
        {
            var treeView = _treeView.OnGui(gui, "project_tree_view", n =>
            {
                n.InitFullWidth();
                n.InitFullHeight();
            });
        });
    }

    private void ItemContentGui(ImGuiNode node, PathNode vNode, IDrawContext context)
    {
        var gui = node.Gui;

        var draw = vNode as IDrawEditorImGui;
        draw ??= (vNode as AssetFileNode)?.GetAsset() as IDrawEditorImGui;

        if (draw != null)
        {
            draw.OnEditorGui(gui, EditorImGuiPipeline.Prefix, this);

            gui.OverlayLayout()
            .InitFullHeight()
            .InitWidthRest()
            .OnContent(() =>
            {
                gui.HorizontalLayout()
                .InitFullSize()
                .OnContent(n =>
                {
                    _treeView.DefaultContentGui(n, vNode, context);
                });

                gui.HorizontalReverseLayout()
                .InitFullSize()
                .OnContent(() =>
                {
                    draw.OnEditorGui(gui, EditorImGuiPipeline.Preview, this);
                });
            });
        }
        else
        {
            _treeView.DefaultContentGui(node, vNode, context);
        }
    }


    #region IToolWindow

    /// <inheritdoc/>
    string IToolWindow.WindowId => ToolWindowId;

    /// <inheritdoc/>
    string IToolWindow.Title => "Project";

    /// <inheritdoc/>
    Image IToolWindow.Icon => Editor.ProjectGui.Properties.IconCache.Project;

    /// <inheritdoc/>
    DockHint IToolWindow.DockHint => DockHint.Left;

    /// <inheritdoc/>
    bool IToolWindow.CanDockDocument => false;

    /// <inheritdoc/>
    object IToolWindow.GetUIObject()
    {
        return null;
    }

    /// <inheritdoc/>
    void IToolWindow.NotifyShow()
    {
        _guiRef.QueueRefresh(true);
    }

    /// <inheritdoc/>
    void IToolWindow.NotifyHide()
    {
    }

    #endregion

    #region IInspectorContext

    /// <inheritdoc/>
    public void InspectorEnter()
    {
    }

    /// <inheritdoc/>
    public void InspectorExit()
    {
    }

    /// <inheritdoc/>
    public void InspectorBeginMacro(string name)
    {
    }

    /// <inheritdoc/>
    public void InspectorEndMarco(string name)
    {
    }

    /// <inheritdoc/>
    public bool InspectorDoAction(UndoRedoAction action)
    {
        return false;
    }

    /// <inheritdoc/>
    public void InspectorEditFinish()
    {
    }

    /// <inheritdoc/>
    public void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
        foreach (var doc in objs.OfType<Document>())
        {
            doc.MarkDirty(this);
            EditorUtility.AddDelayedAction(new DelaySaveDocument(doc));
        }
    }

    /// <inheritdoc/>
    object IServiceProvider.GetService(Type serviceType)
    {
        return null;
    }

    /// <inheritdoc/>
    object IInspectorContext.InspectorUserData { get; set; }

    #endregion

    #region Project Events

    private void HandleUIStart()
    {
        if (_project != null)
        {
            Populate();
        }
    }

    private void HandleProjectOpened(Project project)
    {
        SetProject(project);
    }

    private void HandleProjectClosing(Project project)
    {
        CloseProject();
    }

    private void Project_AssetUpdated(Asset asset, EntryEventArgs e)
    {
        var node = _model.GetNode(asset.FileName?.PhysicFileName);
        if (node is PopulatePathNode popNode)
        {
            popNode.PopulateUpdate();
            popNode.NotifyView();
        }
    }

    private void WorkSpaces_WorkSpaceAdded(object sender, WorkSpaceEventArgs args)
    {
        // Newly added workspaces provide controllers, delay one frame operation.
        QueuedAction.Do(() =>
        {
            _workSpaceManagerNode.AddWorkSpace(args.WorkSpace);
            //EditorUtility.AddDelayedAction(new DelayRefreshWorkSpace(this));
            _workSpaceManagerNode.PopulateUpdate();
        });
    }

    private void WorkSpaces_WorkSpaceRemoved(object sender, WorkSpaceEventArgs args)
    {
        QueuedAction.Do(() =>
        {
            _workSpaceManagerNode.RemoveWorkSpace(args.WorkSpace);
            // Needs immediate release, otherwise FileSystemWatcher will be occupied.
            _workSpaceManagerNode.PopulateUpdate();
        });
    }

    private void _projectNode_UserRenaming(object sender, UserRenamingEventArgs e)
    {
        _project?.HandleExternalRename(e.DoRenameAction);
    }

    #endregion

    #region DocumentManager Events

    private void Current_DocumentNew(DocumentEntry obj)
    {
        QueuedAction.Do(() => 
        {
            _model.PopulateUpdateDeep();
            _guiRef.QueueRefresh();

            // Because document creation has a delay, update with delay
            // EditorUtility.AddDelayedAction(_refreshAction);
        });
    }

    #endregion

    #region UI Events

    private bool HandlePathViewDoubleClick(PathNode node)
    {
        if (node is IViewDoubleClickAction actionNode)
        {
            actionNode.DoubleClick();
            return true;
        }
        else if (node is RenderTargetNode renderTargetNode)
        {
            QueuedAction.Do(() =>
            {
                EditorUtility.LocateInProject(renderTargetNode.NodePath);
            });

            return true;
        }
        else
        {
            return OpenFileCommand.HandleOpen(this, node);
        }
    }

    private void BuildInfo_BuildInfoUpdated(object sender, EventArgs e)
    {
        _assetRootNode?.PopulateUpdateDeep<AssetFileNode>();
    }

    private void OnProjectFileUpdated()
    {
        _assetRootNode?.PopulateUpdateDeep<DirectoryNode>();
    }

    #endregion

    #region Selection

    /// <summary>
    /// Gets all currently selected nodes in the project tree view.
    /// </summary>
    public IEnumerable<PathNode> SelectedNodes => _treeView.SelectedNodes;

    /// <summary>
    /// Gets the collection of types represented by the currently selected nodes.
    /// </summary>
    public ICollection<Type> SelectedTypes => _selectionTypes;

    /// <summary>
    /// Gets the selected directory node, or null if the selection is not a directory.
    /// </summary>
    public DirectoryNode SelectedDirectory => _treeView.SelectedNode as DirectoryNode;

    /// <summary>
    /// Gets the primary selected node in the project tree view.
    /// </summary>
    public PathNode SelectedNode => _treeView.SelectedNode;

    private bool _inspectDisabled = false;

    /// <summary>
    /// Inspects the currently selected nodes and displays their properties in the inspector panel.
    /// </summary>
    public void InspectSelectedNodes()
    {
        // Do not process during locating
        if (_inspectDisabled)
        {
            return;
        }

        _inspectorObjects.Clear();

        // The collection may be modified dynamically
        foreach (var node in SelectedNodes.ToArray())
        {
            if (node is AssetFileNode assetFileNode)
            {
                Asset asset = assetFileNode.GetAsset();
                if (asset != null)
                {
                    if (asset.ShowStorageProperty && asset.GetDocumentEntry(false)?.Content is Document doc)
                    {
                        _inspectorObjects.Add(doc);
                    }
                    else
                    {
                        _inspectorObjects.Add(asset);
                    }

                    continue;
                }
                else if (assetFileNode.IsMeta)
                {
                    asset = (assetFileNode.Parent as AssetFileNode)?.GetAsset();
                    if (asset != null)
                    {
                        asset.CheckLoadMetaFile();
                        if (asset.MetaInfo != null)
                        {
                            _inspectorObjects.Add(asset.MetaInfo);
                        }
                    }
                    continue;
                }

                var format = DocumentManager.Instance.GetDocumentFormatByPath(node.NodePath);
                var fileInfo = new CommonFileInfo(node.NodePath, format != null ? format.DisplayText : string.Empty);
                _inspectorObjects.Add(fileInfo);
            }
            else if (node is FileNode)
            {
                var format = DocumentManager.Instance.GetDocumentFormatByPath(node.NodePath);
                var fileInfo = new CommonFileInfo(node.NodePath, format != null ? format.DisplayText : string.Empty);
                _inspectorObjects.Add(fileInfo);
            }
            else if (node is WorkSpaceRootNode workSpaceRootNode)
            {
                WorkSpace workSpace = workSpaceRootNode.WorkSpace;
                if (workSpace != null)
                {
                    _inspectorObjects.Add(workSpace);
                }
                else
                {
                    _inspectorObjects.Add(new CommonFileInfo(node.NodePath, string.Empty));
                }
            }
            else if (node is DirectoryNode)
            {
                var fileInfo = new CommonFileInfo(node.NodePath, string.Empty);
                _inspectorObjects.Add(fileInfo);
            }
            else if (node is AssetElementNode projectElementNode)
            {
                var obj = projectElementNode.GetAsset();
                if (obj != null)
                {
                    _inspectorObjects.Add(obj);
                }
            }
            else if (node is AssetFieldNode projectFieldNode)
            {
                var obj = projectFieldNode.GetFieldObject();
                if (obj != null)
                {
                    _inspectorObjects.Add(obj);
                }
            }
            else if (node is WorkSpaceReferenceNode workSpaceReferenceNode)
            {
                object setup = workSpaceReferenceNode.GetReferenceItem();
                if (setup != null)
                {
                    _inspectorObjects.Add(setup);
                }
            }
        }

        EditorUtility.Inspector.InspectObjects(_inspectorObjects, this);
    }

    /// <summary>
    /// Selects a single node in the project tree view.
    /// </summary>
    /// <param name="node">The node to select.</param>
    /// <param name="beginEdit">Whether to begin rename editing on the node after selection.</param>
    public void SelectNode(PathNode node, bool beginEdit)
    {
        _treeView.SelectNode(node);
        if (beginEdit)
        {
            _treeView.BeginEdit(node);
        }

        _guiRef.QueueRefresh();
    }

    /// <summary>
    /// Selects multiple nodes in the project tree view.
    /// </summary>
    /// <param name="nodes">The collection of nodes to select.</param>
    public void SelectNodes(IEnumerable<PathNode> nodes)
    {
        _treeView.SelectNodes(nodes);

        _guiRef.QueueRefresh();
    }

    #endregion

    #region Config

    /// <summary>
    /// Sets the current project and updates the tree view accordingly.
    /// </summary>
    /// <param name="project">The project to set. Must not be null.</param>
    public void SetProject(Project project)
    {
        if (project is null)
        {
            throw new ArgumentNullException();
        }

        if (_project == project)
        {
            return;
        }

        WorkSpaceManager.Current.WorkSpaceAdded -= WorkSpaces_WorkSpaceAdded;
        WorkSpaceManager.Current.WorkSpaceRemoved -= WorkSpaces_WorkSpaceRemoved;

        if (_project != null)
        {
            CloseProject();
        }

        _project = project;

        WorkSpaceManager.Current.WorkSpaceAdded += WorkSpaces_WorkSpaceAdded;
        WorkSpaceManager.Current.WorkSpaceRemoved += WorkSpaces_WorkSpaceRemoved;
    }

    /// <summary>
    /// Populates the tree model with nodes for the current project, including assets, workspaces, and publish directories.
    /// </summary>
    private void Populate()
    {
        _model.Clear();

        if (_project is null)
        {
            return;
        }
        string assetPath = FileAssetManager.Current.DirectoryBasePath;
        string workSpacePath = _project.WorkSpaceDirectory;

        if (!Directory.Exists(assetPath))
        {
            return;
        }

        if (_assetRootNode != null)
        {
            _assetRootNode.UserRenaming -= _projectNode_UserRenaming;
        }

        _assetRootNode = new AssetRootNode(assetPath);
        _assetRootNode.UserRenaming += _projectNode_UserRenaming;

        _workSpaceManagerNode = new WorkSpaceManagerNode(workSpacePath);
        foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
        {
            _workSpaceManagerNode.AddWorkSpace(workSpace);
        }

        _publishRootNode = new PublishRootNode(_project.PublishDirectory);

        _model.Add(_assetRootNode);
        _model.Add(_workSpaceManagerNode);
        _model.Add(_publishRootNode);

        _assetRootNode.Expanded = true;
        _workSpaceManagerNode.Expanded = true;
        _publishRootNode.Expanded = true;

        _expandState.Restore(_model);
    }

    /// <summary>
    /// Closes the current project and clears the tree model.
    /// </summary>
    public void CloseProject()
    {
        if (_project != null)
        {
            _project = null;

            _model.Clear();
        }
    }

    /// <summary>
    /// Loads the saved configuration state and restores the expanded tree node paths.
    /// </summary>
    /// <param name="config">The configuration to load.</param>
    public void LoadConfig(ProjectViewConfig config)
    {
        _expandState.SetExpandedPaths(config.Expands);
        _expandState.Restore(_model);
    }

    /// <summary>
    /// Saves the current expanded tree node paths to the specified configuration.
    /// </summary>
    /// <param name="config">The configuration to save to.</param>
    public void SaveConfig(ProjectViewConfig config)
    {
        _expandState.Backup(_model);

        config.Expands.Clear();
        config.Expands.AddRange(_expandState.GetExpandedPaths());
    }

    /// <summary>
    /// Refreshes all asset-related project nodes by re-populating the tree.
    /// </summary>
    public void RefreshProjectNodes()
    {
        //EditorUtility.AddDelayedAction(new DelayRefreshWorkSpace(this));
        _assetRootNode?.EnsurePopulateDeep(true);
    }

    /// <summary>
    /// Refreshes all workspace-related project nodes by re-populating the tree.
    /// </summary>
    public void RefreshWorkSpaceNodes()
    {
        //EditorUtility.AddDelayedAction(new DelayRefreshWorkSpace(this));
        _workSpaceManagerNode?.EnsurePopulateDeep(true);
    }

    #endregion

    #region Update

    /// <inheritdoc/>
    public void BeginUpdate()
    { }

    /// <inheritdoc/>
    public void EndUpdate()
    { }

    #endregion

    #region Drag Drop

    /// <inheritdoc/>
    public void DragOver(IDragEvent e)
    {
        _treeView.HandleDragOver(e);
    }

    /// <inheritdoc/>
    public void DragDrop(IDragEvent e)
    {
        if (e.Data.GetDataPresent(DragEventData.DataFormat_File))
        {
            string[] files = (string[])e.Data.GetData(DragEventData.DataFormat_File);
            if (files.Length == 1)
            {
                if (string.Equals(Path.GetExtension(files[0]), ".suitypackage", StringComparison.OrdinalIgnoreCase))
                {
                    e.SetCopyEffect();
                    QueuedAction.Do(() => ImportPackageCommand.HandleImport(files[0]));

                    return;
                }
            }
        }

        _treeView.HandleDragDrop(e);
    }

    #endregion

    #region VReq

    private bool HandleLocateInProjectVReq(LocateInProjectVReq req)
    {
        try
        {
            _inspectDisabled = true;

            PathNode node = FindFileNode(req.FileName);
            if (node != null)
            {
                EditorUtility.ShowToolWindow(ToolWindowId, false);
                if (req.Item is INamed named &&node is PopulatePathNode popNode)
                {
                    string name = named.Name;

                    popNode.EnsurePopulate();
                    var childNode = node.NodeList.Nodes.FirstOrDefault(o => o.Terminal == name);
                    if (childNode != null)
                    {
                        SelectNode(childNode, false);
                        req.Successful = true;
                        return true;
                    }
                }

                SelectNode(node, false);
                req.Successful = true;
            }

            return true;
        }
        finally
        {
            _inspectDisabled = false;
        }
    }

    private bool HandleLocateWorkSpaceVReq(LocateWorkSpaceVReq req)
    {
        try
        {
            _inspectDisabled = true;

            WorkSpaceRootNode rootNode = _workSpaceManagerNode.WorkSpceNodes.Where(o => o.WorkSpace == req.WorkSpace).FirstOrDefault();
            if (rootNode is null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(req.RelativeFileName))
            {
                PathNode fileNode = rootNode.FindNodeByRelativePath(req.RelativeFileName);
                if (fileNode != null)
                {
                    SelectNode(fileNode, false);
                    req.Successful = true;
                }
            }
            else
            {
                SelectNode(rootNode, false);
                req.Successful = true;
            }

            EditorUtility.ShowToolWindow(ToolWindowId, false);

            return true;

        }
        finally
        {
            _inspectDisabled = false;
        }
    }

    private bool HandleLocateProjectNodeVReq(LocateProjectNodeVReq req)
    {
        try
        {
            _inspectDisabled = true;

            if (req.ViewNode is PathNode node)
            {
                EditorUtility.ShowToolWindow(ToolWindowId, false);
                SelectNode(node, false);
                req.Successful = true;

                _guiRef.QueueRefresh();
            }

            return true;
        }
        finally
        {
            _inspectDisabled = false;
        }


    }

    #endregion

    #region Find

    /// <summary>
    /// Finds a file node in the project tree by its full file path, expanding parent nodes as needed.
    /// </summary>
    /// <param name="fileName">The full path of the file to locate.</param>
    /// <returns>The matching <see cref="PathNode"/>, or null if not found.</returns>
    public PathNode FindFileNode(string fileName)
    {
        if (_assetRootNode is null)
        {
            return null;
        }

        PathNode node;

        do
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            _assetRootNode.EnsurePopulate();
            node = _assetRootNode.FindNodeByFullPath(fileName);
            if (node != null)
            {
                break;
            }

            _workSpaceManagerNode.EnsurePopulate();
            foreach (var workSpaceNode in _workSpaceManagerNode.WorkSpceNodes)
            {
                node = workSpaceNode.FindNodeByFullPath(fileName);
                if (node != null)
                {
                    break;
                }
            }
        } while (false);

        if (node != null)
        {
            var parent = node.Parent;
            while (parent is PopulatePathNode p)
            {
                p.Expanded = true;
                parent = parent.Parent;
            }
        }

        return node;
    }

    /// <summary>
    /// Finds the workspace root node associated with the specified workspace.
    /// </summary>
    /// <param name="workSpace">The workspace to locate.</param>
    /// <returns>The matching <see cref="WorkSpaceRootNode"/>, or null if not found.</returns>
    public WorkSpaceRootNode FindWorkSpaceNode(WorkSpace workSpace)
    {
        return _workSpaceManagerNode.WorkSpceNodes.Where(o => o.WorkSpace == workSpace).FirstOrDefault();
    }

    #endregion

    #region DelayRefreshProject

    /// <summary>
    /// Delayed action that refreshes the entire project model and triggers a UI refresh.
    /// </summary>
    private class DelayRefreshProject(ProjectGui value) : DelayedAction<ProjectGui>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value._model.PopulateUpdateDeep();
            Value._guiRef.QueueRefresh();
        }
    }

    #endregion

    #region DelayRefreshAsset

    /// <summary>
    /// Delayed action that updates the asset root node and triggers a UI refresh.
    /// </summary>
    private class DelayRefreshAsset(ProjectGui value) : DelayedAction<ProjectGui>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value._assetRootNode.PopulateUpdateDeep();
            Value._guiRef.QueueRefresh();
        }
    }

    #endregion

    #region DelayRefreshWorkSpace

    /// <summary>
    /// Delayed action that updates the workspace manager node and triggers a UI refresh.
    /// </summary>
    private class DelayRefreshWorkSpace(ProjectGui value) : DelayedAction<ProjectGui>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value._workSpaceManagerNode.PopulateUpdateDeep();
            Value._guiRef.QueueRefresh();
        }
    }

    #endregion

    #region DelaySaveDocument

    /// <summary>
    /// Delayed action that saves a document.
    /// </summary>
    private class DelaySaveDocument(Document value) : DelayedAction<Document>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Save();
        }
    }

    #endregion
}