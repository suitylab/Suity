using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Root node representing the assets folder in the project tree.
/// </summary>
[ToolTipsText("Asset Folder")]
public class AssetRootNode : RootDirectoryNode, IProjectAssetRootNode, IDropTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetRootNode"/> class.
    /// </summary>
    /// <param name="path">The path to the assets directory.</param>
    public AssetRootNode(string path)
        : base(path)
    {
    }

    /// <inheritdoc/>
    protected override string OnGetText() => "Assets";

    /// <inheritdoc/>
    public override Image Image => CoreIconCache.Project.ToIconSmall();

    /// <inheritdoc/>
    protected override DirectoryNode CreateDirectoryNode() => new AssetDirectoryNode();

    /// <inheritdoc/>
    protected override FileNode CreateFileNode() => new AssetFileNode();

    /// <inheritdoc/>
    protected override bool CanPopulateFile(FileInfo file)
    {
        // Files attached to the main asset are not displayed
        if (file.GetIsAttachedFile())
        {
            string originFileName = file.FullName.RemoveExtension();
            if (File.Exists(originFileName))
            {
                return false;
            }
        }

        return base.CanPopulateFile(file);
    }

    #region IDropTarget

    /// <inheritdoc/>
    void IDropTarget.DragOver(IDragEvent e)
    {
        HandleProjectDragOver(this, e);
    }

    /// <inheritdoc/>
    void IDropTarget.DragDrop(IDragEvent e)
    {
        HandleProjectDragDrop(this, e);
    }

    #endregion

    /// <summary>
    /// Handles drag-over operations for asset nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static void HandleProjectDragOver(DirectoryNode dirNode, IDragEvent e)
    {
        var view = Device.Current.GetService<IProjectGui>();

        var nodes = e.GetDraggingNodes<PathNode>();
        if (nodes.Any() && nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
        {
            if (CanCreateFileBunch(nodes))
            {
                e.SetCopyEffect();
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else if (nodes.Any() && nodes.All(o => o is WorkSpaceRootNode))
        {
            if (nodes.CountOne())
            {
                e.SetCopyEffect();
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else if (nodes.Any() && nodes.All(o => o is WorkSpaceReferenceNode) && nodes.IsParentSame())
        {
            e.SetCopyEffect();
        }
        else
        {
            view.DragOver(e);
        }
    }

    /// <summary>
    /// Handles drag-and-drop operations for asset nodes.
    /// </summary>
    /// <param name="dirNode">The target directory node.</param>
    /// <param name="e">The drag event data.</param>
    public static async void HandleProjectDragDrop(DirectoryNode dirNode, IDragEvent e)
    {
        var nodes = e.GetDraggingNodes<PathNode>();
        if (nodes.Any() && nodes.All(o => o is WorkSpaceDirectoryNode || o is WorkSpaceFileNode))
        {
            if (CanCreateFileBunch(nodes))
            {
                await HandleCreateFileBunch(nodes, dirNode);
            }
        }
        else if (nodes.Any() && nodes.All(o => o is WorkSpaceRootNode))
        {
            if (nodes.CountOne())
            {
                e.SetCopyEffect();
            }
            else
            {
                e.SetNoneEffect();
            }
        }
        else if (nodes.Any() && nodes.All(o => o is WorkSpaceReferenceNode) && nodes.IsParentSame())
        {
            HandleCreateCodeLibrary(nodes.OfType<WorkSpaceReferenceNode>(), dirNode);
        }
        else
        {
            var view = Device.Current.GetService<IProjectGui>();
            view.DragDrop(e);
        }
    }

    /// <summary>
    /// Determines whether a file bunch can be created from the given nodes.
    /// </summary>
    /// <param name="nodes">The nodes to evaluate.</param>
    /// <returns>True if a file bunch can be created; otherwise, false.</returns>
    public static bool CanCreateFileBunch(IEnumerable<PathNode> nodes)
    {
        if (!nodes.Any())
        {
            return false;
        }

        if (!nodes.Select(o => o.FindMeOrParent<WorkSpaceRootNode>()).AllEqual())
        {
            return false;
        }

        if (nodes.OfType<IWorkSpaceFsNode>().Any(o => o.IsNew || o.IsRendering))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a new file bunch from the given workspace nodes.
    /// </summary>
    /// <param name="nodes">The nodes to include in the bunch.</param>
    /// <param name="targetDirNode">The target directory node.</param>
    /// <returns>True if the bunch was created successfully; otherwise, false.</returns>
    public static async Task<bool> HandleCreateFileBunch(IEnumerable<PathNode> nodes, DirectoryNode targetDirNode)
    {
        if (!nodes.Select(o => o.FindMeOrParent<WorkSpaceRootNode>()).AllEqual())
        {
            return false;
        }

        WorkSpace workSpace = nodes.Select(o => o.FindMeOrParent<WorkSpaceRootNode>()).FirstOrDefault()?.WorkSpace;
        if (workSpace is null)
        {
            return false;
        }

        var fileBunchService = Device.Current.GetService<IFileBunchService>();
        if (fileBunchService is null)
        {
            return false;
        }

        Dictionary<string, FileBunchUpdate> files = [];
        Dictionary<string, FileBunchUpdate> renderingFiles = [];
        foreach (var node in nodes)
        {
            CollectBunchFiles(workSpace, node, files, renderingFiles);
        }

        await CheckCombineRenderingFiles(files, renderingFiles);
        if (files.Count == 0)
        {
            return false;
        }

        string name;
        if (nodes.CountOne())
        {
            name = nodes.First().Terminal;
        }
        else
        {
            name = workSpace.Name;
        }

        string fileName = targetDirNode.NodePath.PathAppend(name);
        string bunchFileName = FileUtils.GetAutoNewFileName(fileName, "sbunch");

        bool success = fileBunchService.CreateOrUpdate(files.Values, bunchFileName);
        if (!success)
        {
            return false;
        }

        var project = workSpace.Manager.OwnerProject;

        //string bunchAssetKey = project.Library.MakeAssetName(bunchFileName);
        //oneRootNode.WorkSpace.SetupFileBunch(bunchAssetKey);

        QueuedAction.Do(() =>
        {
            if (FileAssetManager.Current.GetAsset(bunchFileName) is IFileBunch bunch)
            {
                workSpace.SetupFileBunch(bunch);
                targetDirNode.PopulateUpdate();
            }
        });

        return true;
    }

    /// <summary>
    /// Creates or updates a file bunch with the specified nodes and target file name.
    /// </summary>
    /// <param name="nodes">The nodes to include in the bunch.</param>
    /// <param name="bunchFileName">The target file name for the bunch.</param>
    /// <returns>True if the bunch was created or updated successfully; otherwise, false.</returns>
    public static async Task<bool> HandleCreateOrUpdateFileBunch(IEnumerable<PathNode> nodes, string bunchFileName)
    {
        if (!nodes.Select(o => o.FindMeOrParent<WorkSpaceRootNode>()).AllEqual())
        {
            return false;
        }
        WorkSpace workSpace = nodes.Select(o => o.FindMeOrParent<WorkSpaceRootNode>()).FirstOrDefault()?.WorkSpace;
        if (workSpace is null)
        {
            return false;
        }

        var service = Device.Current.GetService<IFileBunchService>();
        if (service is null)
        {
            return false;
        }

        Dictionary<string, FileBunchUpdate> files = [];
        Dictionary<string, FileBunchUpdate> renderingFiles = [];
        foreach (var node in nodes)
        {
            CollectBunchFiles(workSpace, node, files, renderingFiles);
        }

        await CheckCombineRenderingFiles(files, renderingFiles);
        if (files.Count == 0)
        {
            return false;
        }

        bool success = service.CreateOrUpdate(files.Values, bunchFileName);
        if (!success)
        {
            return false;
        }

        var project = workSpace.Manager.OwnerProject;

        //string bunchAssetKey = project.Library.MakeAssetName(bunchFileName);
        //oneRootNode.WorkSpace.SetupFileBunch(bunchAssetKey);

        QueuedAction.Do(() =>
        {
            if (FileAssetManager.Current.GetAsset(bunchFileName) is IFileBunch bunch)
            {
                workSpace.SetupFileBunch(bunch);
            }
        });

        return true;
    }

    /// <summary>
    /// Creates a new code library from the given reference nodes.
    /// </summary>
    /// <param name="nodes">The reference nodes to include.</param>
    /// <param name="targetDirNode">The target directory node.</param>
    /// <returns>True if the code library was created successfully; otherwise, false.</returns>
    public static bool HandleCreateCodeLibrary(IEnumerable<WorkSpaceReferenceNode> nodes, DirectoryNode targetDirNode)
    {
        ICodeLibraryService service = Device.Current.GetService<ICodeLibraryService>();
        if (service is null)
        {
            return false;
        }

        WorkSpace workSpace = nodes.FirstOrDefault()?.WorkSpace;
        if (workSpace is null)
        {
            return false;
        }

        var refs = nodes.Select(o => o.GetReferenceItem()).OfType<IWorkSpaceRefItem>();
        var targets = refs.Where(o => o?.Enabled == true)
            .SelectMany(o => o.GetRenderTargets())
            .OfType<RenderTarget>();

        string initFileName = null;
        if (nodes.CountOne())
        {
            initFileName = targetDirNode.NodePath.PathAppend(Path.GetFileNameWithoutExtension(nodes.First().Text));
        }
        else
        {
            initFileName = targetDirNode.NodePath.PathAppend(workSpace.Name);
        }

        string fileName = FileUtils.GetAutoNewFileName(initFileName, "scode");

        service.StoreUserCode(fileName, targets);
        targetDirNode.PopulateUpdate();

        return true;
    }

    /// <summary>
    /// Merges user code from reference nodes into an existing code library file.
    /// </summary>
    /// <param name="nodes">The reference nodes to merge from.</param>
    /// <param name="fileNode">The target code library file node.</param>
    /// <returns>True if the merge was successful; otherwise, false.</returns>
    public static async Task<bool> HandleMergeUserCodeLibrary(IEnumerable<WorkSpaceReferenceNode> nodes, AssetFileNode fileNode)
    {
        var service = Device.Current.GetService<ICodeLibraryService>();
        if (service is null)
        {
            return false;
        }

        var workSpace = nodes.FirstOrDefault()?.WorkSpace;
        if (workSpace is null)
        {
            return false;
        }

        var refs = nodes.Select(o => o.GetReferenceItem()).OfType<IWorkSpaceRefItem>();
        var targets = refs.Where(o => o?.Enabled == true)
            .SelectMany(o => o.GetRenderTargets())
            .OfType<RenderTarget>();

        string codeFileName = fileNode.NodePath;
        if (!File.Exists(codeFileName))
        {
            await DialogUtility.ShowMessageBoxAsyncL("File does not exist");
            return false;
        }

        bool merge = await DialogUtility.ShowYesNoDialogAsyncL($"Merge user code into {Path.GetFileNameWithoutExtension(codeFileName)}?");
        if (!merge)
        {
            return false;
        }

        service.StoreUserCode(codeFileName, targets);
        fileNode.PopulateUpdate();

        return true;
    }

    /// <summary>
    /// Collects files for a file bunch from a workspace node.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="node">The node to collect files from.</param>
    /// <param name="files">The dictionary to populate with regular files.</param>
    /// <param name="renderingFiles">The dictionary to populate with rendering files.</param>
    private static void CollectBunchFiles(WorkSpace workSpace, PathNode node, Dictionary<string, FileBunchUpdate> files, Dictionary<string, FileBunchUpdate> renderingFiles)
    {
        if (node is WorkSpaceFileNode)
        {
            CollectBunchFiles(workSpace, node.NodePath, files, renderingFiles);
        }
        else if (node is WorkSpaceDirectoryNode)
        {
            var dirInfo = new DirectoryInfo(node.NodePath);
            foreach (var fileInfo in dirInfo.GetAllFiles(true))
            {
                CollectBunchFiles(workSpace, fileInfo.FullName, files, renderingFiles);
            }
        }
    }

    /// <summary>
    /// Collects a single file for a file bunch, categorizing it as regular or rendering.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="fullName">The full path of the file.</param>
    /// <param name="files">The dictionary to populate with regular files.</param>
    /// <param name="renderingFiles">The dictionary to populate with rendering files.</param>
    private static void CollectBunchFiles(WorkSpace workSpace, string fullName, Dictionary<string, FileBunchUpdate> files, Dictionary<string, FileBunchUpdate> renderingFiles)
    {
        string rFileName = fullName.MakeRalativePath(workSpace.MasterDirectory);
        string lowId = rFileName.GetPathLowId();
        if (files.ContainsKey(lowId) || renderingFiles.ContainsKey(lowId))
        {
            return;
        }

        var vFileInfo = new FileBunchUpdate
        {
            FileId = rFileName,
            FullName = fullName
        };

        if (workSpace.GetRenderTargets(rFileName).Any())
        {
            renderingFiles.Add(lowId, vFileInfo);
        }
        else
        {
            files.Add(lowId, vFileInfo);
        }
    }

    /// <summary>
    /// Prompts the user about combining rendering files and validates file selection.
    /// </summary>
    /// <param name="files">The regular files dictionary.</param>
    /// <param name="renderingFiles">The rendering files dictionary.</param>
    /// <returns>A task representing the async operation.</returns>
    private static async Task CheckCombineRenderingFiles(Dictionary<string, FileBunchUpdate> files, Dictionary<string, FileBunchUpdate> renderingFiles)
    {
        if (renderingFiles.Count > 0)
        {
            bool result = await DialogUtility.ShowYesNoDialogAsyncL("The selected files contain render targets. Ignore these files?");
            if (!result)
            {
                files.AddRange(renderingFiles);
            }
        }

        if (files.Count == 0)
        {
            await DialogUtility.ShowMessageBoxAsyncL("No files selected.");
        }
    }
}