using Suity.Editor;
using Suity.Editor.Packaging.Exporting;
using Suity.Editor.Packaging.Importing;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Packaging;

/// <summary>
/// Editor plugin that provides package export and import functionality, implementing both <see cref="IPackageExport"/> and <see cref="IPackageImport"/> services.
/// </summary>
public class PackagerPlugin : EditorPlugin, IPackageExport, IPackageImport
{
    /// <inheritdoc/>
    public override string Description => "Package Management";

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IPackageExport))
        {
            return this;
        }

        if (serviceType == typeof(IPackageImport))
        {
            return this;
        }

        return null;
    }

    /// <summary>
    /// Shows the export package dialog with the specified files and workspaces pre-selected.
    /// </summary>
    /// <param name="files">The asset files to include in the export.</param>
    /// <param name="workSpaces">The workspace names to include in the export.</param>
    /// <param name="onComplete">An optional callback invoked when the export completes.</param>
    public async void ShowExportPackage(IEnumerable<string> files, IEnumerable<string> workSpaces, Action onComplete = null)
    {
        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.Export))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.Export));

        //    return;
        //}

        files ??= [];
        workSpaces ??= [];

        var exportForm = new ExportImGui();

        //exportForm.AddProjectFiles(false);
        //exportForm.AddProjectWorkspaces(false);

        exportForm.AddAssetFiles(files, true);

        foreach (var workSpaceName in workSpaces)
        {
            WorkSpace workSpace = WorkSpaceManager.Current.GetWorkSpace(workSpaceName);
            if (workSpace != null)
            {
                exportForm.AddWorkspace(workSpace, true);
            }
        }

        exportForm.AddSystemDirectory(false);
        exportForm.AddLibraryDirectory(false);

        await DialogUtility.CreateImGuiDialog(exportForm, "Export", 883, 827);

        if (!exportForm.IsSuccess)
        {
            return;
        }

        string[] exportAssetFiles = [.. exportForm.GetAssetFiles()];
        WorkSpaceFile[] exportWorkspaceFiles = [.. exportForm.GetWorkspaceFiles()];
        string[] exportSystemFiles = [.. exportForm.GetSystemFiles()];
        string[] exportLibraryFiles = [.. exportForm.GetLibraryFiles()];

        if (exportAssetFiles.Length == 0 && exportWorkspaceFiles.Length == 0 && exportSystemFiles.Length == 0 && exportLibraryFiles.Length == 0)
        {
            await DialogUtility.ShowMessageBoxAsyncL("No files selected");

            return;
        }

        string suggestedName;
        if (workSpaces.FirstOrDefault() is string s && !string.IsNullOrWhiteSpace(s))
        {
            suggestedName = s;
        }
        else
        {
            if (exportAssetFiles.Length == 1)
            {
                suggestedName = Path.GetFileNameWithoutExtension(exportAssetFiles[0]);
            }
            else
            {
                suggestedName = exportForm.GetSuggestedAssetPath();
            }
        }

        string packageFileName = await GetExportPackageName(exportForm.PackageType, suggestedName);
        if (string.IsNullOrEmpty(packageFileName))
        {
            return;
        }

        var exporter = new PackageExporter();
        switch (exportForm.PackageType)
        {
            case PackageTypes.Package:
                {
                    var option = new PackageExportOptions
                    {
                        PackageFileName = packageFileName,
                        AseetFiles = exportAssetFiles,
                        WorkSpaceFiles = exportWorkspaceFiles,
                        SystemFiles = exportSystemFiles,
                        LibraryFiles = exportLibraryFiles,
                    };
                    await exporter.ExportPackage(option);
                    QueuedAction.Do(() =>
                    {
                        EditorUtility.LocateInPublishView(packageFileName);
                        onComplete?.Invoke();
                    });
                }
                break;

            case PackageTypes.Library:
                await exporter.ExportLibrary(packageFileName, exportAssetFiles);
                QueuedAction.Do(() =>
                {
                    EditorUtility.LocateInPublishView(packageFileName);
                    onComplete?.Invoke();
                });
                break;
        }
    }

    /// <summary>
    /// Headless export (no UI): writes a <c>.suitypackage</c> for the current project using the
    /// selected content kinds. Mirrors the export dialog's default selection (render-target
    /// workspace files and system id files are excluded). Returns the path, or null.
    /// </summary>
    public async Task<string> ExportPackage(string packageFileName, PackageExportSelection selection, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(packageFileName))
        {
            return null;
        }

        var assetFiles = new List<string>();
        var workSpaceFiles = new List<WorkSpaceFile>();
        var systemFiles = new List<string>();

        if ((selection & PackageExportSelection.Asset) != 0)
        {
            var assetDir = new DirectoryInfo(FileAssetManager.Current.DirectoryBasePath);
            if (assetDir.Exists)
            {
                foreach (var file in assetDir.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    token.ThrowIfCancellationRequested();
                    assetFiles.Add(file.FullName);
                }
            }
        }

        if ((selection & PackageExportSelection.WorkSpace) != 0)
        {
            foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
            {
                token.ThrowIfCancellationRequested();
                workSpaceFiles.Add(new WorkSpaceFile
                {
                    FileName = workSpace.ConfigFileName,
                    LocalFileName = WorkSpace.DefaultConfigFileName,
                    WorkSpace = workSpace.Name,
                    InMaster = false,
                });
                CollectWorkSpaceMasterFiles(workSpace, new DirectoryInfo(workSpace.MasterDirectory), workSpaceFiles, token);
            }
        }

        if ((selection & PackageExportSelection.System) != 0)
        {
            var systemDir = new DirectoryInfo(Project.Current.SystemDirectory);
            if (systemDir.Exists)
            {
                foreach (var file in systemDir.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    token.ThrowIfCancellationRequested();
                    var r = file.FullName.MakeRelativePath(Project.Current.SystemDirectory);
                    if (string.Equals(r, "ObjectId.xml", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(r, "SystemId.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    systemFiles.Add(file.FullName);
                }
            }
        }

        if (assetFiles.Count == 0 && workSpaceFiles.Count == 0 && systemFiles.Count == 0)
        {
            return null;
        }

        var option = new PackageExportOptions
        {
            PackageFileName = packageFileName,
            AseetFiles = assetFiles.ToArray(),
            WorkSpaceFiles = workSpaceFiles.ToArray(),
            SystemFiles = systemFiles.ToArray(),
            LibraryFiles = [],
        };

        await new PackageExporter().ExportPackage(option);
        return File.Exists(packageFileName) ? packageFileName : null;
    }

    /// <summary>Recursively collects a workspace's master-directory files (mirrors the export dialog).</summary>
    private static void CollectWorkSpaceMasterFiles(WorkSpace workSpace, DirectoryInfo dir, List<WorkSpaceFile> list, CancellationToken token)
    {
        if (!dir.Exists)
        {
            return;
        }

        if (workSpace.Controller?.CanExport(dir.FullName) == false)
        {
            return;
        }

        foreach (var file in dir.EnumerateFiles())
        {
            token.ThrowIfCancellationRequested();

            if (workSpace.Controller?.CanExport(file.FullName) == false)
            {
                continue;
            }

            var rPath = file.FullName.MakeRelativePath(workSpace.MasterDirectory);
            if (workSpace.GetRenderTargets(rPath).Any())
            {
                // Render-target files are disabled by default in the export dialog.
                continue;
            }

            list.Add(new WorkSpaceFile
            {
                FileName = file.FullName,
                LocalFileName = rPath,
                WorkSpace = workSpace.Name,
                InMaster = true,
            });
        }

        foreach (var subDir in dir.EnumerateDirectories())
        {
            CollectWorkSpaceMasterFiles(workSpace, subDir, list, token);
        }
    }

    /// <summary>
    /// Prompts the user to select a file name and location for the exported package.
    /// </summary>
    /// <param name="packageType">The type of package to export.</param>
    /// <param name="initName">An optional initial name suggestion for the file dialog.</param>
    /// <returns>The full path of the selected package file, or null if cancelled.</returns>
    private static async Task<string> GetExportPackageName(PackageTypes packageType, string initName = null)
    {
        //var project = Project.CurrentProject;

        string ext = string.Empty;
        switch (packageType)
        {
            case PackageTypes.Package:
                ext = ".suitypackage";
                break;

            case PackageTypes.Library:
                ext = ".suitylibrary";
                break;
        }

        string fullPath = await DialogUtility.ShowExportFileNameDialogAsync(initName, ext);

        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        return fullPath;
    }

    /// <summary>
    /// Shows the import package dialog for the specified package file.
    /// </summary>
    /// <param name="fileName">The path to the package file to import.</param>
    /// <param name="packageFullName">An optional full package identifier for metadata tracking.</param>
    /// <param name="onComplete">An optional callback invoked when the import completes.</param>
    public async void ShowImportPackageGui(string fileName, string packageFullName = null, Action onComplete = null)
    {
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            await DialogUtility.ShowMessageBoxAsyncL("File does not exist");
            return;
        }

        var importForm = new ImportImGui();
        importForm.AddPackageFile(fileName, packageFullName);
        string title = $"Import {Path.GetFileNameWithoutExtension(fileName)}";
        await DialogUtility.CreateImGuiDialog(importForm, title, 883, 827);

        if (!importForm.IsSuccess)
        {
            return;
        }

        Project project = Project.Current;

        string[] entries = importForm.GetAssetFiles().Concat(importForm.GetWorkspaceFiles()).Concat(importForm.GetSystemFiles())
            .Select(s => s.MakeRelativePath(project.ProjectBasePath))
            .ToArray();

        if (entries.Length > 0)
        {
            var importer = new PackageImporter();
            await QueuedAction.DoSuspendedAction(() => importer.Import(fileName, entries, packageFullName));

            EditorUtility.RefreshProjectView();

            if (onComplete != null)
            {
                QueuedAction.Do(() => onComplete());
            }
        }
        else
        {
            if (onComplete != null)
            {
                QueuedAction.Do(() => onComplete());
            }
        }
    }

    /// <summary>
    /// Imports a package file without showing a UI dialog, importing all entries by default.
    /// </summary>
    /// <param name="fileName">The path to the package file to import.</param>
    /// <param name="packageFullName">An optional full package identifier for metadata tracking.</param>
    /// <param name="options">The import options.</param>
    public async Task ImportPackage(string fileName, string packageFullName = null, ImportOptions options = ImportOptions.All)
    {
        var importer = new PackageImporter();

        await QueuedAction.DoSuspendedAction(() => importer.Import(fileName, null, packageFullName, options));
        await EditorUtility.WaitForQueuedAction();
    }

    /// <summary>
    /// Removes the asset files that were previously imported from the package.
    /// Workspace and system files are never touched.
    /// </summary>
    /// <param name="fileName">The path to the package file to clean up.</param>
    /// <param name="options">The package content kinds to remove.</param>
    public async Task CleanUpPackage(string fileName, ImportOptions options = ImportOptions.Asset)
    {
        var importer = new PackageImporter();

        await QueuedAction.DoSuspendedAction(() => importer.CleanUp(fileName, options));
        await EditorUtility.WaitForQueuedAction();
    }
}
