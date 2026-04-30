using Suity;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Im;
using Suity.Views.Im.TreeEditing;
using Suity.Views.PathTree;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Packaging;

/// <summary>
/// ImGui-based UI for previewing package contents in a tree view with checkboxes and status columns.
/// </summary>
internal class PackagePreviewImGui : IDrawImGui
{
    private readonly ColumnPathTreeView _treeView;

    private PackageTypes _packageType = PackageTypes.SuityPackage;

    private readonly PackagePathTreeModel _model = new();
    private PackagePreviewDirectoryNode _rootAssetNode;
    private PackagePreviewDirectoryNode _rootWorkspaceNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackagePreviewImGui"/> class with a configured tree view.
    /// </summary>
    public PackagePreviewImGui()
    {
        _treeView = new ColumnPathTreeView(_model);

        _treeView.Column.NameColumn.Title = "File";
        _treeView.Column.PreviewColumn.Title = "Preview";

        _treeView.Column.NameColumnWidth = 600;
        _treeView.Column.PreviewColumnWidth = 200;

        _treeView.Column.NameColumn.RowGui = ConfigNameColumn;
        _treeView.Column.PreviewColumn.RowGui = ConfigPreviewColumn;

        _treeView.TreeData.SelectionMode = ImTreeViewSelectionMode.Multiple;
    }

    /// <summary>
    /// Configures the name column GUI for a tree node, rendering checkbox, status icon, custom icon, file icon, and text.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual path node associated with this row.</param>
    private void ConfigNameColumn(ImGuiNode node, PathNode vNode)
    {
        var gui = node.Gui;

        if (vNode is IPackagePreviewNode previewNode)
        {
            gui.CheckBoxAdvanced("##enabled", previewNode.EnableState)
            .InitClass("propInput")
            .OnChecked((n, v) =>
            {
                CheckState state = v ? CheckState.Checked : CheckState.Unchecked;

                previewNode.EnableState = state;
                ApplyCheckOnSelection(state);

                _treeView.QueueRefresh();
            });
        }

        if (vNode.TextStatusIcon != null)
        {
            gui.Image("##status_icon", vNode.TextStatusIcon)
            .InitClass("icon");
        }

        if (vNode.CustomImage != null)
        {
            gui.Image("##custom_icon", vNode.CustomImage)
            .InitClass("icon");
        }

        if (vNode.Image != null)
        {
            gui.Image("##icon", vNode.Image)
            .InitClass("icon");
        }

        gui.Text("##title_text", vNode.Text)
        .SetFontColor(vNode.Color)
        .InitVerticalAlignment(GuiAlignment.Center);
    }

    /// <summary>
    /// Toggles the check state of all currently selected preview nodes.
    /// </summary>
    private void ToggleCheckOnSelection()
    {
        var state = _treeView.SelectedNodes.OfType<IPackagePreviewNode>().FirstOrDefault()?.EnableState ?? CheckState.Unchecked;

        if (state == CheckState.Checked)
        {
            state = CheckState.Unchecked;
        }
        else
        {
            state = CheckState.Checked;
        }

        ApplyCheckOnSelection(state);
    }

    /// <summary>
    /// Applies the specified check state to all currently selected preview nodes.
    /// </summary>
    /// <param name="state">The check state to apply.</param>
    private void ApplyCheckOnSelection(CheckState state)
    {
        foreach (var previewNode in _treeView.SelectedNodes.OfType<IPackagePreviewNode>())
        {
            previewNode.EnableState = state;
        }
    }

    /// <summary>
    /// Configures the preview column GUI for a tree node, rendering the status text for item nodes.
    /// </summary>
    /// <param name="node">The ImGui node being rendered.</param>
    /// <param name="vNode">The virtual path node associated with this row.</param>
    private void ConfigPreviewColumn(ImGuiNode node, PathNode vNode)
    {
        var gui = node.Gui;

        if (vNode is PackagePreviewItemNode itemNode)
        {
            gui.Text("##status_text", L(itemNode.StatusText))
            .SetFontColor(vNode.Color)
            .InitVerticalAlignment(GuiAlignment.Center);
        }
    }

    /// <summary>
    /// Gets or sets the package type, propagating the change to all root nodes and refreshing the view.
    /// </summary>
    public PackageTypes PackageType
    {
        get => _packageType;
        set
        {
            if (_packageType == value)
            {
                return;
            }

            _packageType = value;
            if (_rootAssetNode != null)
            {
                _rootAssetNode.PackageType = _packageType;
            }

            if (_rootWorkspaceNode != null)
            {
                _rootWorkspaceNode.PackageType = _packageType;
            }

            _treeView.QueueRefresh();
        }
    }

    /// <summary>
    /// Gets the package operation direction (export or import).
    /// </summary>
    public PackageDirection Direction { get; private set; }

    /// <summary>
    /// Sets up the tree model with root asset and workspace directory nodes for the specified direction.
    /// </summary>
    /// <param name="direction">The package operation direction.</param>
    public void SetupNode(PackageDirection direction)
    {
        Direction = direction;

        _model.Clear();
        //var project = Project.CurrentProject;

        _rootAssetNode = new PackagePreviewDirectoryNode(FileAssetManager.Current.DirectoryBasePath, direction, FileLocations.Asset)
        {
            PackageType = _packageType,
        };
        _model.Add(_rootAssetNode);

        _rootWorkspaceNode = new PackagePreviewDirectoryNode(WorkSpaceManager.Current.BasePath, direction, FileLocations.WorkSpace)
        {
            PackageType = _packageType,
        };
        _model.Add(_rootWorkspaceNode);
    }

    /// <summary>
    /// Adds all project asset files to the preview tree.
    /// </summary>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddProjectAssetFiles(bool enabled)
    {
        //Project project = Project.CurrentProject;

        var dir = new DirectoryInfo(FileAssetManager.Current.DirectoryBasePath);
        foreach (var file in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
            string rFileName = FileAssetManager.Current.MakeRelativePath(file.FullName);

            _rootAssetNode.AddItem(rFileName, enabled);
        }

        //TODO: treeViewAdv1.ExpandAll(_rootAssetNode);
    }

    /// <summary>
    /// Adds all project workspace directories to the preview tree.
    /// </summary>
    /// <param name="enabled">Whether the workspaces should be initially enabled.</param>
    public void AddProjectWorkspaces(bool enabled)
    {
        //Project project = Project.CurrentProject;

        foreach (var workspace in WorkSpaceManager.Current.WorkSpaces)
        {
            _rootWorkspaceNode.AddItem(workspace.Name, enabled);
        }

        //TODO: treeViewAdv1.ExpandAll(_workspaceManagerNode);
    }

    /// <summary>
    /// Adds multiple asset files to the preview tree and updates their enable states.
    /// </summary>
    /// <param name="fileNames">The file paths to add.</param>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddAssetFiles(IEnumerable<string> fileNames, bool enabled)
    {
        //Project project = Project.CurrentProject;

        foreach (var fileName in fileNames)
        {
            string rFileName = FileAssetManager.Current.MakeRelativePath(fileName);

            _rootAssetNode.AddItem(rFileName, enabled);
        }

        //TODO: treeViewAdv1.ExpandAll(_rootAssetNode);

        // UpdateEnableStateDeep needs to expand nodes and execute Populate first
        _rootAssetNode.UpdateEnableStateDeep();
    }

    /// <summary>
    /// Adds a single asset file to the preview tree.
    /// </summary>
    /// <param name="fileName">The file path to add.</param>
    /// <param name="enabled">Whether the file should be initially enabled.</param>
    public void AddAssetFile(string fileName, bool enabled)
    {
        //Project project = Project.CurrentProject;

        string rFileName = FileAssetManager.Current.MakeRelativePath(fileName);

        _rootAssetNode.AddItem(rFileName, enabled);
    }

    /// <summary>
    /// Adds multiple workspace entries to the preview tree and updates their enable states.
    /// </summary>
    /// <param name="workspaces">The workspace names to add.</param>
    /// <param name="enabled">Whether the workspaces should be initially enabled.</param>
    public void AddWorkspaces(IEnumerable<string> workspaces, bool enabled)
    {
        //Project project = Project.CurrentProject;

        foreach (var workspace in workspaces)
        {
            _rootWorkspaceNode.AddItem(workspace, enabled);
        }

        //TODO: treeViewAdv1.ExpandAll(_workspaceManagerNode);

        // UpdateEnableStateDeep needs to expand nodes and execute Populate first
        _rootWorkspaceNode.UpdateEnableStateDeep();
    }

    /// <summary>
    /// Adds a single workspace entry to the preview tree.
    /// </summary>
    /// <param name="workspace">The workspace name to add.</param>
    /// <param name="enabled">Whether the workspace should be initially enabled.</param>
    public void AddWorkspace(string workspace, bool enabled)
    {
        //Project project = Project.CurrentProject;

        _rootWorkspaceNode.AddItem(workspace, enabled);
    }

    /// <summary>
    /// Adds multiple workspace files to the preview tree.
    /// </summary>
    /// <param name="fileNames">The file paths to add.</param>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddWorkSpaceFiles(IEnumerable<string> fileNames, bool enabled)
    {
        foreach (var fileName in fileNames)
        {
            string rFileName = WorkSpaceManager.Current.MakeRelativePath(fileName);
            _rootWorkspaceNode.AddItem(rFileName, enabled);
        }
    }

    /// <summary>
    /// Adds a workspace file to the preview tree, resolving its workspace and master status.
    /// </summary>
    /// <param name="rFileName">The relative file name within the workspace root.</param>
    /// <param name="enabled">Whether the file should be initially enabled.</param>
    /// <returns>The created item node.</returns>
    public PackagePreviewItemNode AddWorkSpaceFile(string rFileName, bool enabled)
    {
        string workSpaceName = rFileName.FindAndGetBefore('/', true);
        var workSpace = WorkSpaceManager.Current.GetWorkSpace(workSpaceName);

        string localFileName = rFileName.RemoveFromFirst(workSpaceName.Length + 1);
        bool inMaster = localFileName.StartsWith(WorkSpace.DefaultMasterDirectory);

        var workSpaceNode = _rootWorkspaceNode.AddDirectory(workSpaceName);
        workSpaceNode.WorkSpace = workSpace;

        var fileNode = workSpaceNode.AddItem(localFileName, enabled);
        fileNode.InMaster = inMaster;

        if (workSpace != null)
        {
            string fileName;

            if (inMaster)
            {
                localFileName = localFileName.RemoveFromFirst(WorkSpace.DefaultMasterDirectory.Length + 1);
                fileName = workSpace.MakeMasterFullPath(localFileName);
            }
            else
            {
                fileName = workSpace.MakeBaseFullPath(localFileName);
            }

            fileNode.LocalFileName = localFileName;
            fileNode.FileExists = File.Exists(fileName);
        }

        return fileNode;
    }

    /// <summary>
    /// Adds a workspace master file to the preview tree with explicit master status.
    /// </summary>
    /// <param name="workSpace">The workspace this file belongs to.</param>
    /// <param name="fileName">The full file path.</param>
    /// <param name="inMaster">Whether the file resides in the master directory.</param>
    /// <param name="enabled">Whether the file should be initially enabled.</param>
    /// <returns>The created item node.</returns>
    public PackagePreviewItemNode AddWorkSpaceMasterFile(WorkSpace workSpace, string fileName, bool inMaster, bool enabled)
    {
        string localFileName;
        string rFileName;
        if (inMaster)
        {
            localFileName = workSpace.MakeMasterRelativePath(fileName);
            rFileName = workSpace.Name.PathAppend(localFileName);
        }
        else
        {
            rFileName = WorkSpaceManager.Current.MakeRelativePath(fileName);
            localFileName = rFileName.RemoveFromFirst(workSpace.Name.Length + 1);
        }

        var workSpaceNode = _rootWorkspaceNode.AddDirectory(workSpace.Name);
        workSpaceNode.WorkSpace = workSpace;

        PackagePreviewItemNode fileNode;
        if (inMaster)
        {
            fileNode = workSpaceNode.AddItem(WorkSpace.DefaultMasterDirectory.PathAppend(localFileName), enabled);
        }
        else
        {
            fileNode = workSpaceNode.AddItem(localFileName, enabled);
        }

        fileNode.InMaster = inMaster;
        fileNode.LocalFileName = localFileName;

        return fileNode;
    }

    /// <summary>
    /// Checks whether a file at the specified path is currently enabled in the preview tree.
    /// </summary>
    /// <param name="fileName">The file path to check.</param>
    /// <returns>True if the file node exists and is enabled; otherwise, false.</returns>
    public bool GetIsFileChecked(string fileName)
    {
        if (_model.GetNode(fileName) is PackagePreviewItemNode fileNode)
        {
            return fileNode.Enabled;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Refreshes the tree population, updates enable states, and expands all nodes.
    /// </summary>
    public void RefreshExpandAll()
    {
        //TODO: treeViewAdv1.ExpandAll();

        _rootAssetNode.PopulateUpdateDeep();
        _rootWorkspaceNode.PopulateUpdateDeep();

        _rootAssetNode.UpdateEnableStateDeep();
        _rootWorkspaceNode.UpdateEnableStateDeep();

        _rootAssetNode.ExpandDeep();
        _rootWorkspaceNode.ExpandDeep();
    }

    /// <summary>
    /// Gets a suggested asset path derived from the enabled items in the root asset node.
    /// </summary>
    /// <returns>A suggested asset path string with the "Assets" prefix removed.</returns>
    public string GetSuggestedAssetPath()
    {
        if (_rootAssetNode is null)
        {
            return string.Empty;
        }

        return _rootAssetNode.GetSuggestedAssetPath().RemoveFromFirst("Assets").TrimStart('.');
    }

    /// <summary>
    /// Checks whether any enabled item in the preview tree has an error status.
    /// </summary>
    /// <returns>True if any error is found; otherwise, false.</returns>
    public bool ContainsError()
    {
        if (_rootAssetNode?.ContainsError() == true)
        {
            return true;
        }

        if (_rootWorkspaceNode?.ContainsError() == true)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the paths of all enabled asset files.
    /// </summary>
    /// <returns>A collection of enabled asset file paths.</returns>
    public IEnumerable<string> GetFiles()
    {
        if (_rootAssetNode is null)
        {
            return [];
        }
        else
        {
            List<PackagePreviewItemNode> list = [];
            _rootAssetNode.CollectEnabledItemsDeep(list);
            return list.Select(o => o.NodePath);
        }
    }

    /// <summary>
    /// Gets the names of all enabled workspace directories.
    /// </summary>
    /// <returns>A collection of enabled workspace names.</returns>
    public IEnumerable<string> GetWorkspaces()
    {
        if (_rootWorkspaceNode is null)
        {
            return [];
        }
        else
        {
            List<PackagePreviewDirectoryNode> list = [];
            _rootWorkspaceNode.CollectEnabledDirectories(list);

            return list.Select(o => o.Terminal);
        }
    }

    /// <summary>
    /// Gets the details of all enabled workspace files.
    /// </summary>
    /// <returns>A collection of <see cref="WorkSpaceFile"/> entries for enabled workspace files.</returns>
    public IEnumerable<WorkSpaceFile> GetWorkspaceFiles()
    {
        if (_rootWorkspaceNode is null)
        {
            return [];
        }

        List<PackagePreviewItemNode> list = [];
        _rootWorkspaceNode.CollectEnabledItemsDeep(list);

        return list.Select(o => new WorkSpaceFile
        {
            FileName = o.NodePath,
            LocalFileName = o.LocalFileName,
            WorkSpace = o.FindWorkSpace()?.Name,
            InMaster = o.InMaster,
        });
    }

    /// <summary>
    /// Gets the paths of all enabled workspace files.
    /// </summary>
    /// <returns>A collection of enabled workspace file paths.</returns>
    public IEnumerable<string> GetWorkspaceFileNames()
    {
        if (_rootWorkspaceNode is null)
        {
            return [];
        }

        List<PackagePreviewItemNode> list = [];
        _rootWorkspaceNode.CollectEnabledItemsDeep(list);

        return list.Select(o => o.NodePath);
    }

    //public IEnumerable<string> GetWorkSpacePhysicsFiles()
    //{
    //    if (_workspaceManagerNode is null)
    //    {
    //        return [];
    //    }

    //    List<PackagePreviewItemNode> list = new List<PackagePreviewItemNode>();
    //    _workspaceManagerNode.CollectEnabledItemsDeep(list);
    //    return list.Select(o =>
    //    {
    //        var workSpace = o.FindWorkSpace();
    //        if (workSpace is null)
    //        {
    //            return o.NodePath;
    //        }

    //        if (o.InMaster)
    //        {
    //            return workSpace.MakeMasterFullPath(o.LocalFileName);
    //        }
    //        else
    //        {
    //            return workSpace.MakeMasterRelativePath(o.LocalFileName);
    //        }
    //    });
    //}

    /// <summary>
    /// Renders the package preview tree view with keyboard support for toggling selection.
    /// </summary>
    /// <param name="gui">The ImGui context to render into.</param>
    public void OnGui(ImGui gui)
    {
        _treeView.OnGui(gui, "tree_view", n =>
        {
            n.InitFullSize();
            n.InitKeyDownInput((_, input) =>
            {
                if (input.KeyCode == "Space")
                {
                    ToggleCheckOnSelection();

                    return GuiInputState.FullSync;
                }

                return null;
            });
        });
    }
}
