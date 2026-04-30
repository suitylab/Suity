using static Suity.Helpers.GlobalLocalizer;
using ICSharpCode.SharpZipLib.Zip;
using Suity.Editor.CodeRender;
using Suity.Editor.CodeRender.Replacing;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Libraries;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Json;
using Suity.NodeQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Suity.Editor.Packaging.Exporting;

/// <summary>
/// Handles the export of project files and workspace files into package archives or library archives.
/// </summary>
internal class PackageExporter
{
    /// <summary>
    /// The name of the temporary directory used during export operations.
    /// </summary>
    public const string TempDir = "ExportTemp";

    private const string ManifestXmlFileName = "manifest.xml";
    private const string WorkSpaceExportJsonFileName = "export.json";

    private readonly Project _project;

    private readonly List<RenderFile> _renderFiles = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageExporter"/> class using the current project.
    /// </summary>
    public PackageExporter()
    {
        _project = Project.Current;
    }

    /// <summary>
    /// Exports the specified asset and workspace files into a Suity package archive.
    /// </summary>
    /// <param name="fileNames">The asset file paths to export.</param>
    /// <param name="workSpaceFiles">The workspace file entries to export.</param>
    /// <param name="packageFileName">The destination path for the package archive.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    public Task ExportPackage(IEnumerable<string> fileNames, IEnumerable<WorkSpaceFile> workSpaceFiles, string packageFileName)
    {
        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.Export))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.Export));
        //    return Task.FromResult(0);
        //}

        CleanUp();

        var source = new TaskCompletionSource<bool>();

        DocumentManager.Instance.SaveAllDocuments();

        EditorUtility.DoProgress(L("Packaging..."), p =>
        {
            try
            {
                string tempDir = _project.EnsureAndCleanUpSubDirectory(TempDir);
                var tempDirInfo = new DirectoryInfo(tempDir);
                if (!tempDirInfo.Exists)
                {
                    throw new InvalidProgramException();
                }

                string tempAssetDir = tempDir.PathAppend("Assets");
                if (!Directory.Exists(tempAssetDir))
                {
                    Directory.CreateDirectory(tempAssetDir);
                }

                string tempWorkspaceDir = tempDir.PathAppend("WorkSpaces");
                if (!Directory.Exists(tempWorkspaceDir))
                {
                    Directory.CreateDirectory(tempWorkspaceDir);
                }

                foreach (var fileName in fileNames)
                {
                    p.UpdateProgess(0, L($"Exporting {fileName}..."), string.Empty);
                    ExportAssetFile(fileName, tempAssetDir, PackageTypes.SuityPackage);
                }

                foreach (var workspaceFile in workSpaceFiles)
                {
                    p.UpdateProgess(0, L($"Exporting: {workspaceFile.FileName}..."), string.Empty);
                    ExportWorkspaceFile(workspaceFile, tempWorkspaceDir);
                }

                p.UpdateProgess(0, L("Exporting configuration..."), string.Empty);
                string manifestFileName = tempWorkspaceDir.PathAppend(WorkSpaceExportJsonFileName);
                ExportWorkSpaceSetting(manifestFileName);

                p.UpdateProgess(0, L("Compressing..."), string.Empty);
                if (File.Exists(packageFileName))
                {
                    File.Delete(packageFileName);
                }

                var fastZip = new FastZip();
                fastZip.CreateZip(packageFileName, tempDir, true, null);
            }
            catch (Exception err)
            {
                err.LogError();
            }
            finally
            {
                _project.DeleteSubDirectory(TempDir);
                p.CompleteProgess();
            }
        }, () =>
        {
            CleanUp();
            source.SetResult(true);
        });

        return source.Task;
    }

    /// <summary>
    /// Exports the specified asset files into a Suity library archive with a manifest.
    /// </summary>
    /// <param name="fileNames">The asset file paths to export.</param>
    /// <param name="libraryFileName">The destination path for the library archive.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    public Task ExportLibrary(IEnumerable<string> fileNames, string libraryFileName)
    {
        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.Export))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.Export));
        //    return Task.FromResult(0);
        //}

        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.ExportLibrary))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.ExportLibrary));
        //    return Task.FromResult(0);
        //}

        CleanUp();

        var source = new TaskCompletionSource<bool>();

        DocumentManager.Instance.SaveAllDocuments();

        EditorUtility.DoProgress(L("Packaging..."), p =>
        {
            try
            {
                string tempDir = _project.EnsureAndCleanUpSubDirectory(TempDir);
                var tempDirInfo = new DirectoryInfo(tempDir);
                if (!tempDirInfo.Exists)
                {
                    throw new InvalidProgramException();
                }

                string tempAssetDir = tempDir.PathAppend("Assets");
                if (!Directory.Exists(tempAssetDir))
                {
                    Directory.CreateDirectory(tempAssetDir);
                }

                foreach (var fileName in fileNames)
                {
                    p.UpdateProgess(0, L($"Exporting {fileName}..."), string.Empty);
                    ExportAssetFile(fileName, tempAssetDir, PackageTypes.SuityLibrary);
                }

                p.UpdateProgess(0, L($"Exporting: {"manifest"}..."), string.Empty);
                MakeManifest(Path.GetFileNameWithoutExtension(libraryFileName), "0", fileNames, tempDir, tempAssetDir);

                p.UpdateProgess(0, L("Compressing..."), string.Empty);
                if (File.Exists(libraryFileName))
                {
                    File.Delete(libraryFileName);
                }

                var fastZip = new FastZip();
                if (!string.IsNullOrEmpty(LibraryAssetBK._xx))
                {
                    fastZip.Password = LibraryAssetBK._xx;
                }
                fastZip.CreateZip(libraryFileName, tempDir, true, null);
            }
            catch (Exception err)
            {
                err.LogError();
            }
            finally
            {
                _project.DeleteSubDirectory(TempDir);
                p.CompleteProgess();
            }
        }, () =>
        {
            CleanUp();
            source.SetResult(true);
        });

        return source.Task;
    }

    /// <summary>
    /// Clears all internal state collected during a previous export operation.
    /// </summary>
    private void CleanUp()
    {
        _renderFiles.Clear();
    }

    /// <summary>
    /// Exports a single asset file to the temporary export directory.
    /// </summary>
    /// <param name="fileName">The source file path.</param>
    /// <param name="tempAssetDir">The temporary asset directory root.</param>
    /// <param name="packageType">The type of package being created.</param>
    /// <returns>True if the file was exported successfully; otherwise, false.</returns>
    private bool ExportAssetFile(string fileName, string tempAssetDir, PackageTypes packageType)
    {
        var file = new FileInfo(fileName);
        if (!file.Exists)
        {
            Logs.LogWarning(L("File does not exist") + ": " + file.FullName);
            return false;
        }

        string rFileName = FileAssetManager.Current.MakeRelativePath(fileName);
        if (string.IsNullOrEmpty(rFileName))
        {
            Logs.LogWarning(L("File is not in the project") + ": " + file.FullName);
            return false;
        }

        string exportFileName = tempAssetDir.PathAppend(rFileName);
        string exportDir = Path.GetDirectoryName(exportFileName);
        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        DocumentEntry doc = DocumentManager.Instance.OpenDocument(fileName);
        if (doc?.Content != null)
        {
            if (packageType == PackageTypes.SuityLibrary && doc.GetAsset() is IFileBunch fileBunch)
            {
                return ExportFileBunch(fileBunch, exportFileName, tempAssetDir);
            }
            else
            {
                doc.Export(exportFileName);
            }

            if (doc.View == null)
            {
                DocumentManager.Instance.CloseDocument(doc);
            }
        }
        else if (fileName.FileExtensionEquals(Asset.MetaExtension))
        {
            var info = MetaDataInfo.Load(fileName);
            MetaDataInfo.Export(info, exportFileName);
        }
        else
        {
            File.Copy(fileName, exportFileName);
        }
        return true;
    }

    /// <summary>
    /// Exports a file bunch (multi-file asset) by converting the file name to a directory.
    /// </summary>
    /// <param name="fileBunch">The file bunch to export.</param>
    /// <param name="exportFileName">The destination path (which becomes a directory).</param>
    /// <param name="tempAssetDir">The temporary asset directory root.</param>
    /// <returns>True if the bunch was exported successfully; otherwise, false.</returns>
    private bool ExportFileBunch(IFileBunch fileBunch, string exportFileName, string tempAssetDir)
    {
        // Convert file name to directory
        if (!Directory.Exists(exportFileName))
        {
            Directory.CreateDirectory(exportFileName);
        }

        fileBunch.SaveToFiles(fileBunch.Files, exportFileName);

        return true;
    }

    /// <summary>
    /// Exports a workspace file to the temporary export directory, handling render target files specially.
    /// </summary>
    /// <param name="file">The workspace file entry to export.</param>
    /// <param name="tempWorkspaceDir">The temporary workspace directory root.</param>
    /// <returns>True if the file was exported successfully; otherwise, false.</returns>
    private bool ExportWorkspaceFile(WorkSpaceFile file, string tempWorkspaceDir)
    {
        var workSpace = WorkSpaceManager.Current.GetWorkSpace(file.WorkSpace);
        if (workSpace == null)
        {
            Logs.LogWarning(L("Workspace does not exist") + ": " + file);
            return false;
        }

        string rFileName = WorkSpaceManager.Current.MakeRelativePath(file.FileName);
        string localFileName = file.LocalFileName;
        string fileName;
        if (file.InMaster)
        {
            fileName = workSpace.MakeMasterFullPath(localFileName);
            rFileName = workSpace.Name.PathAppend(WorkSpace.DefaultMasterDirectory.PathAppend(localFileName));
        }
        else
        {
            fileName = workSpace.MakeBaseFullPath(localFileName);
        }

        if (!File.Exists(fileName))
        {
            Logs.LogError(L("File does not exist") + ": " + fileName);
        }

        string targetFileName = tempWorkspaceDir.PathAppend(rFileName);
        string dir = Path.GetDirectoryName(targetFileName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (string.Equals(localFileName, WorkSpace.DefaultWorkSpaceConfigFileName, StringComparison.OrdinalIgnoreCase) && !file.InMaster)
        {
            workSpace.ExportConfig(targetFileName);
        }
        else
        {
            var renderTarget = workSpace.GetRenderTargets(localFileName).FirstOrDefault();
            if (renderTarget != null)
            {
                try
                {
                    ExportFileWithGuid(renderTarget, fileName, targetFileName);
                }
                catch (Exception err)
                {
                    err.LogError(L("Failed to export render file") + ": " + file);
                }

                rFileName = rFileName.Replace("\\", "/");
                _renderFiles.Add(new RenderFile { FileName = rFileName, Language = renderTarget.Language });
            }
            else
            {
                File.Copy(fileName, targetFileName);
            }
        }

        return true;
    }

    /// <summary>
    /// Exports a render target file by replacing resolved global IDs with their original keys.
    /// </summary>
    /// <param name="target">The render target containing the segment configuration.</param>
    /// <param name="fileName">The source file path.</param>
    /// <param name="targetFileName">The destination file path.</param>
    private void ExportFileWithGuid(RenderTarget target, string fileName, string targetFileName)
    {
        var config = target.GetSegmentConfig();
        if (config is null)
        {
            Logs.LogError($"{nameof(CodeSegmentConfig)} not found : {fileName}");
            return;
        }

        string source = TextFileHelper.TryReadAllText(fileName, Encoding.UTF8);

        var doc = new SegmentDocument(config);
        doc.Parse(source, target.OwnerId.ToString());
        doc.ReplaceKeys(key =>
        {
            if (Guid.TryParseExact(key, "D", out Guid id))
            {
                return GlobalIdResolver.RevertResolve(id);
            }
            else
            {
                return key;
            }
        });

        string result = doc.GenerateCode();

        TextFileHelper.WriteFile(targetFileName, result);
    }

    /// <summary>
    /// Exports the workspace settings (render file metadata) to a JSON file.
    /// </summary>
    /// <param name="targetFileName">The destination file path for the settings JSON.</param>
    private void ExportWorkSpaceSetting(string targetFileName)
    {
        var writer = new JsonDataWriter();

        //writer.Node("author").WriteString(Internals._license.UserId);
        var renderNodes = writer.Nodes("renderFiles", _renderFiles.Count);
        foreach (var renderFile in _renderFiles)
        {
            var renderNode = renderNodes.Item();
            renderNode.Node("fileName").WriteString(renderFile.FileName);
            renderNode.Node("language").WriteString(renderFile.Language);
        }
        renderNodes.Finish();

        string result = writer.ToString(true);

        TextFileHelper.WriteFile(targetFileName, result);
    }

    /// <summary>
    /// Creates an XML manifest file for a library package, including file sizes and MD5 checksums.
    /// </summary>
    /// <param name="name">The library name.</param>
    /// <param name="version">The library version string.</param>
    /// <param name="fileNames">The asset file paths to include in the manifest.</param>
    /// <param name="baseDir">The base directory for the manifest output.</param>
    /// <param name="baseAssetDir">The base asset directory containing the exported files.</param>
    private void MakeManifest(string name, string version, IEnumerable<string> fileNames, string baseDir, string baseAssetDir)
    {
        MD5 md5 = MD5.Create();

        var writer = new XmlNodeWriter("manifest");
        writer.SetElement("library", gw =>
        {
            gw.SetAttribute("name", name);
            gw.SetAttribute("version", version);
            gw.SetAttribute("packageType", "Library");
            gw.SetAttribute("publishTime", DateTime.UtcNow.ToString());

            foreach (string fileName in fileNames)
            {
                if (!File.Exists(fileName))
                {
                    continue;
                }
                string rFileName = FileAssetManager.Current.MakeRelativePath(fileName);
                if (string.IsNullOrEmpty(rFileName))
                {
                    continue;
                }

                byte[] md5Result = null;

                if (EditorUtility.GetFileAsset(fileName) is IFileBunch fileBunch)
                {
                    gw.SetElement("bunch", bw =>
                    {
                        bw.SetAttribute("name", rFileName);

                        foreach (var bFile in fileBunch.Files)
                        {
                            string fullPath = baseAssetDir.PathAppend(rFileName).PathAppend(bFile.FileId);

                            using (var stream = File.OpenRead(fullPath))
                            {
                                md5Result = md5.ComputeHash(stream);
                            }

                            bw.SetElement("file", sw =>
                            {
                                sw.SetAttribute("name", bFile.FileId);
                                sw.SetAttribute("size", new FileInfo(fullPath).Length);
                                sw.SetAttribute("md5", BitConverter.ToString(md5Result));
                            });
                        }
                    });
                }
                else
                {
                    string fullPath = baseAssetDir.PathAppend(rFileName);

                    using (var stream = File.OpenRead(fullPath))
                    {
                        md5Result = md5.ComputeHash(stream);
                    }

                    gw.SetElement("file", sw =>
                    {
                        sw.SetAttribute("name", rFileName);
                        sw.SetAttribute("size", new FileInfo(fullPath).Length);
                        sw.SetAttribute("md5", BitConverter.ToString(md5Result));
                    });
                }
            }
        });

        string manifestFileName = Path.Combine(baseDir, ManifestXmlFileName);

        writer.SaveToFile(manifestFileName);
    }
}
