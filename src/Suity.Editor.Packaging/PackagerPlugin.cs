using Suity.Editor.Packaging.Exporting;
using Suity.Editor.Packaging.Importing;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        exportForm.AddFiles(files, true);

        foreach (var workSpaceName in workSpaces)
        {
            WorkSpace workSpace = WorkSpaceManager.Current.GetWorkSpace(workSpaceName);
            if (workSpace != null)
            {
                exportForm.AddWorkspace(workSpace, true);
            }
        }

        await EditorUtility.CreateImGuiDialog(exportForm, "Export", 883, 827);

        if (!exportForm.IsSuccess)
        {
            return;
        }

        string[] exportFiles = [.. exportForm.GetFiles()];
        WorkSpaceFile[] exportWorkspaceFiles = [.. exportForm.GetWorkspaceFiles()];

        if (exportFiles.Length == 0 && exportWorkspaceFiles.Length == 0)
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
            if (exportFiles.Length == 1)
            {
                suggestedName = Path.GetFileNameWithoutExtension(exportFiles[0]);
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
            case PackageTypes.SuityPackage:
                await exporter.ExportPackage(exportFiles, exportWorkspaceFiles, packageFileName);
                QueuedAction.Do(() =>
                {
                    EditorUtility.LocateInPublishView(packageFileName);
                    onComplete?.Invoke();
                });
                break;

            case PackageTypes.SuityLibrary:
                await exporter.ExportLibrary(exportFiles, packageFileName);
                QueuedAction.Do(() =>
                {
                    EditorUtility.LocateInPublishView(packageFileName);
                    onComplete?.Invoke();
                });
                break;
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
            case PackageTypes.SuityPackage:
                ext = ".suitypackage";
                break;

            case PackageTypes.SuityLibrary:
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
        await EditorUtility.CreateImGuiDialog(importForm, title, 883, 827);

        if (!importForm.IsSuccess)
        {
            return;
        }

        Project project = Project.Current;

        string[] entries = importForm.GetFiles().Concat(importForm.GetWorkspaceFiles())
            .Select(s => s.MakeRalativePath(project.ProjectBasePath)).ToArray();

        if (entries.Length > 0)
        {
            var importer = new PackageImporter();
            await importer.Import(fileName, entries, packageFullName);

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
    public async Task ImportPackage(string fileName, string packageFullName = null)
    {
        var importer = new PackageImporter();
        await importer.Import(fileName, null, packageFullName);

        await EditorUtility.WaitForNextQueuedAction();
    }
}
