using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Im;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Packaging.Exporting;

/// <summary>
/// ImGui-based UI dialog for selecting files and configuring export options for package creation.
/// </summary>
internal class ExportImGui : IDrawImGui
{
    private ReferenceManager _refManager;

    private readonly PackagePreviewImGui _previewGui;

    private readonly GuiDropDownValue _packageTypes;

    private bool _init;
    private bool _closing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportImGui"/> class with an export-direction preview and package type selector.
    /// </summary>
    public ExportImGui()
    {
        _packageTypes = new GuiDropDownValue("Package", "Library");

        _previewGui = new PackagePreviewImGui();

        _refManager = Device.Current.GetService<ReferenceManager>();
        //exportTypeCombo.SelectedItem = "Package";
        _previewGui.SetupNode(PackageDirection.Export);
    }

    /// <summary>
    /// Gets a value indicating whether the user confirmed the export operation.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the currently selected package type.
    /// </summary>
    public PackageTypes PackageType => _previewGui.PackageType;

    /// <summary>
    /// Adds all project asset files to the preview tree.
    /// </summary>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddProjectFiles(bool enabled)
    {
        _previewGui.AddProjectAssetFiles(enabled);
    }

    /// <summary>
    /// Adds all project workspace directories to the preview tree.
    /// </summary>
    /// <param name="enabled">Whether the workspaces should be initially enabled.</param>
    public void AddProjectWorkspaces(bool enabled)
    {
        _previewGui.AddProjectWorkspaces(enabled);
    }

    /// <summary>
    /// Adds multiple asset files to the preview tree.
    /// </summary>
    /// <param name="fileNames">The file paths to add.</param>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddFiles(IEnumerable<string> fileNames, bool enabled)
    {
        _previewGui.AddAssetFiles(fileNames, enabled);
    }

    /// <summary>
    /// Adds a workspace and all its master files to the preview tree recursively.
    /// </summary>
    /// <param name="workSpace">The workspace to add.</param>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    public void AddWorkspace(WorkSpace workSpace, bool enabled)
    {
        //packagePreviewControl1.AddWorkspaces(fileNames, PackageDirection.Export, enabled);
        var node = _previewGui.AddWorkSpaceMasterFile(workSpace, workSpace.ConfigFileName, false, enabled);
        if (node != null)
        {
            node.IsImportantFile = true;
        }

        if (!workSpace.IsExternalMasterDirectory)
        {
            var dir = new DirectoryInfo(workSpace.MasterDirectory);
            AddWorkSpaceFilesRecursive(workSpace, dir, enabled);
        }
        else
        {
            var dir = new DirectoryInfo(workSpace.MasterDirectory);
            AddWorkSpaceFilesRecursive(workSpace, dir, enabled);
        }
    }

    /// <summary>
    /// Recursively adds all files from a workspace directory to the preview tree, marking render targets and export-disabled files.
    /// </summary>
    /// <param name="workSpace">The workspace these files belong to.</param>
    /// <param name="dir">The directory to scan.</param>
    /// <param name="enabled">Whether the files should be initially enabled.</param>
    private void AddWorkSpaceFilesRecursive(WorkSpace workSpace, DirectoryInfo dir, bool enabled)
    {
        if (!dir.Exists)
        {
            return;
        }

        if (workSpace.Controller?.CanExport(dir.FullName) == false)
        {
            return;
        }

        foreach (string fileName in dir.EnumerateFiles().Select(o => o.FullName))
        {
            var node = _previewGui.AddWorkSpaceMasterFile(workSpace, fileName, true, enabled);
            if (node is null)
            {
                continue;
            }

            string rPath = fileName.MakeRalativePath(workSpace.MasterDirectory);
            bool hasRenderTarget = workSpace.GetRenderTargets(rPath).Any();
            if (hasRenderTarget)
            {
                node.IsRenderTarget = true;
                node.Enabled = false;
            }

            if (workSpace.Controller?.CanExport(fileName) == false)
            {
                node.ExportDisabled = true;
                node.Enabled = false;
            }
        }

        foreach (var subDir in dir.EnumerateDirectories())
        {
            AddWorkSpaceFilesRecursive(workSpace, subDir, enabled);
        }
    }

    /// <summary>
    /// Gets a suggested asset path derived from the enabled items in the preview tree.
    /// </summary>
    /// <returns>A suggested asset path string.</returns>
    public string GetSuggestedAssetPath()
    {
        return _previewGui.GetSuggestedAssetPath();
    }

    /// <summary>
    /// Gets the paths of all enabled asset files.
    /// </summary>
    /// <returns>A collection of enabled asset file paths.</returns>
    public IEnumerable<string> GetFiles()
    {
        return _previewGui.GetFiles();
    }

    /// <summary>
    /// Gets the names of all enabled workspace directories.
    /// </summary>
    /// <returns>A collection of enabled workspace names.</returns>
    public IEnumerable<string> GetWorkspaces()
    {
        return _previewGui.GetWorkspaces();
    }

    /// <summary>
    /// Gets the details of all enabled workspace files.
    /// </summary>
    /// <returns>A collection of <see cref="WorkSpaceFile"/> entries for enabled workspace files.</returns>
    public IEnumerable<WorkSpaceFile> GetWorkspaceFiles()
    {
        return _previewGui.GetWorkspaceFiles();
    }

    /// <summary>
    /// Recursively collects all dependency files for a given asset file, including transitive dependencies, meta files, and user code files.
    /// </summary>
    /// <param name="file">The source file to analyze.</param>
    /// <param name="depFiles">The set to accumulate dependency file paths into.</param>
    private void CollectDependency(string file, HashSet<string> depFiles)
    {
        var doc = DocumentManager.Instance.OpenDocument(file)?.Content;
        if (doc is null)
        {
            return;
        }

        List<string> newDepFiles = null;

        var dep = _refManager.CollectDependencies(doc)
            .Select(EditorObjectManager.Instance.GetObject)
            .SkipNull()
            .Select(o => o.GetStorageLocation()?.PhysicFileName)
            .SkipNull()
            .Distinct().ToArray();

        foreach (var depfile in dep)
        {
            if (depFiles.Add(depfile))
            {
                (newDepFiles ??= []).Add(depfile);
            }
        }

        var asset = doc.GetAsset();
        string userCodeFile = asset.GetAttachedUserLibraryFileName();
        // Add auto restore dependency
        if (!string.IsNullOrEmpty(userCodeFile) && File.Exists(userCodeFile) && depFiles.Add(userCodeFile))
        {
            (newDepFiles ??= []).Add(userCodeFile);
        }

        string metaFile = asset.GetAttachedMetaFileName();
        // Add auto restore dependency
        if (!string.IsNullOrEmpty(metaFile) && File.Exists(metaFile) && depFiles.Add(metaFile))
        {
            (newDepFiles ??= []).Add(metaFile);
        }

        if (newDepFiles != null)
        {
            foreach (var newDepFile in newDepFiles)
            {
                CollectDependency(newDepFile, depFiles);
            }
        }
    }

    /// <summary>
    /// Recursively collects all dependency files for a given workspace, including transitive asset dependencies.
    /// </summary>
    /// <param name="name">The workspace name to analyze.</param>
    /// <param name="depFiles">The set to accumulate dependency file paths into.</param>
    private void CollectWorkspaceDependency(string name, HashSet<string> depFiles)
    {
        Project project = Project.Current;
        WorkSpace workSpace = WorkSpaceManager.Current.GetWorkSpace(name);
        if (workSpace is null)
        {
            return;
        }

        List<string> newDepFiles = null;

        var dep = _refManager.CollectDependencies(workSpace)
            .Select(EditorObjectManager.Instance.GetObject)
            .SkipNull()
            .Select(o => o.GetStorageLocation()?.PhysicFileName)
            .SkipNull()
            .Distinct().ToArray();

        foreach (var depfile in dep)
        {
            if (depFiles.Add(depfile))
            {
                (newDepFiles ??= []).Add(depfile);
            }
        }

        if (newDepFiles != null)
        {
            foreach (var newDepFile in newDepFiles)
            {
                CollectDependency(newDepFile, depFiles);
            }
        }
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        if (gui is null)
        {
            return;
        }

        if (!_init)
        {
            _init = true;
            _previewGui.RefreshExpandAll();
        }

        gui.Frame()
        .InitClass("editorBg")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.OverlayLayout()
            .InitClass("headerBar")
            .InitHeight(35)
            .OnContent(() =>
            {
                gui.HorizontalLayout().InitFullWidth().OnContent(() =>
                {
                    gui.Button("depBtn", L("Select Dependency Files"))
                    .InitClass("mainBtn")
                    .OnClick(() =>
                    {
                        Project project = Project.Current;
                        ReferenceManager refManager = Device.Current.GetService<ReferenceManager>();

                        HashSet<string> depFiles = [];

                        foreach (var file in GetFiles())
                        {
                            CollectDependency(file, depFiles);
                        }

                        foreach (var workSpace in GetWorkspaces())
                        {
                            CollectWorkspaceDependency(workSpace, depFiles);
                        }

                        if (depFiles.Count > 0)
                        {
                            _previewGui.AddAssetFiles(depFiles, true);
                            _previewGui.RefreshExpandAll();
                        }
                    });
                });
                gui.HorizontalReverseLayout().InitFullWidth().OnContent(() =>
                {
                    gui.Button("exportBtn", L("Export"))
                    .InitClass("mainBtn")
                    .OnClick(async () =>
                    {
                        //if (_previewGui.PackageType == PackageTypes.SuityLibrary &&
                        //    !ServiceInternals._license.GetCapability(EditorCapabilities.ExportLibrary))
                        //{
                        //    DialogUtility.ShowMessageBoxAsync(ServiceInternals._license.GetFailedMessage(EditorCapabilities.ExportLibrary));
                        //    return;
                        //}

                        if (_previewGui.ContainsError())
                        {
                            await DialogUtility.ShowMessageBoxAsyncL("The exported project contains errors.");
                            return;
                        }

                        OkClose();
                    });
                    gui.DropDownButton("mode", "Package")
                    .InitValue(_packageTypes)
                    .InitClass("mainBtn")
                    .InitWidth(120)
                    .OnEdited(n =>
                    {
                        switch (_packageTypes.SelectedValue?.ToString())
                        {
                            case "Package":
                                _previewGui.PackageType = PackageTypes.SuityPackage;
                                break;

                            case "Library":
                                _previewGui.PackageType = PackageTypes.SuityLibrary;
                                break;

                            default:
                                break;
                        }
                    });
                });
            });

            gui.VerticalLayout()
            .InitFullWidth()
            .InitHeightRest()
            .OnContent(() =>
            {
                _previewGui.OnGui(gui);
            });
        });

        if (_closing)
        {
            gui.IsClosing = true;
        }
    }

    /// <summary>
    /// Marks the dialog as successful and triggers closing.
    /// </summary>
    private void OkClose()
    {
        IsSuccess = true;
        _closing = true;
    }
}
