using static Suity.Helpers.GlobalLocalizer;
using ICSharpCode.SharpZipLib.Zip;
using Suity.Helpers;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Packaging.Importing;

/// <summary>
/// ImGui-based UI dialog for selecting which files to import from a package archive.
/// </summary>
internal class ImportImGui : IDrawImGui
{
    private readonly PackagePreviewImGui _previewGui;

    /// <summary>
    /// Gets the path to the package file being imported.
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Gets the full package identifier for metadata tracking.
    /// </summary>
    public string PackageFullName { get; private set; }

    private bool _init;
    private bool _closing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportImGui"/> class with an import-direction preview.
    /// </summary>
    public ImportImGui()
    {
        _previewGui = new PackagePreviewImGui();

        _previewGui.SetupNode(PackageDirection.Import);
    }

    /// <summary>
    /// Gets a value indicating whether the user confirmed the import operation.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Registers a package file for import and extracts its contents into the preview tree.
    /// </summary>
    /// <param name="fileName">The path to the package file.</param>
    /// <param name="packageFullName">An optional full package identifier.</param>
    public void AddPackageFile(string fileName, string packageFullName = null)
    {
        FileName = fileName;
        PackageFullName = packageFullName;

        //this.Text = $"Import {Path.GetFileNameWithoutExtension(fileName)}";

        try
        {
            ExtractInfo(fileName);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Gets the paths of all enabled asset files selected for import.
    /// </summary>
    /// <returns>A collection of enabled asset file paths.</returns>
    public IEnumerable<string> GetFiles()
    {
        return _previewGui.GetFiles();
    }

    /// <summary>
    /// Gets the paths of all enabled workspace files selected for import.
    /// </summary>
    /// <returns>A collection of enabled workspace file paths.</returns>
    public IEnumerable<string> GetWorkspaceFiles()
    {
        return _previewGui.GetWorkspaceFileNames();
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
            gui.VerticalLayout()
            .InitFullWidth()
            .InitHeight(35, GuiLengthMode.RestExcept)
            .OnContent(() =>
            {
                _previewGui.OnGui(gui);
            });

            gui.HorizontalReverseLayout()
            .InitClass("headerBar")
            .InitFullWidth()
            .OnContent(() =>
            {
                gui.Button("importBtn", L("Import"))
                .InitClass("mainBtn")
                .OnClick(() =>
                {
                    OkClose();
                });
            });
        });

        if (_closing)
        {
            gui.IsClosing = true;
        }
    }

    /// <summary>
    /// Extracts file entries from the package archive and populates the preview tree.
    /// </summary>
    /// <param name="fileName">The path to the package zip file.</param>
    /// <param name="password">An optional password for encrypted packages.</param>
    private void ExtractInfo(string fileName, string password = null)
    {
        Project project = Project.Current;

        using Stream fs = File.OpenRead(fileName);
        using var zf = new ZipFile(fs);

        if (!string.IsNullOrEmpty(password))
        {
            // AES encrypted entries are handled automatically
            zf.Password = password;
        }

        foreach (ZipEntry zipEntry in zf)
        {
            if (!zipEntry.IsFile)
            {
                // Ignore directories
                continue;
            }

            if (zipEntry.Name == PackageImporter.WorkSpaceExportSettingFileName)
            {
                continue;
            }

            string entryFileName = project.ProjectBasePath.PathAppend(zipEntry.Name);

            if (zipEntry.Name.StartsWith("Assets"))
            {
                _previewGui.AddAssetFile(entryFileName, true);
            }
            else if (zipEntry.Name.StartsWith("WorkSpaces"))
            {
                string rFileName = zipEntry.Name.RemoveFromFirst(11);

                _previewGui.AddWorkSpaceFile(rFileName, true);
            }
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
