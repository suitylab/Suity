using static Suity.Helpers.GlobalLocalizer;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.CodeRender.Replacing;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Packaging.Importing;

/// <summary>
/// Handles the import of package files (zip archives) into the project, including assets and workspace files.
/// </summary>
internal class PackageImporter
{
    /// <summary>
    /// The name of the temporary directory used during import operations.
    /// </summary>
    public const string TempDir = "ExportTemp";

    /// <summary>
    /// The relative path to the workspace export settings file within a package.
    /// </summary>
    public const string WorkSpaceExportSettingFileName = "WorkSpaces/export.json";

    private readonly Project _project;
    private readonly Dictionary<string, RenderFile> _renderFiles = [];
    private readonly List<List<string>> _assetFileNames = [];
    private readonly List<string> _workspaceNames = [];

    // 4K is optimum
    private readonly byte[] _buffer = new byte[4096];

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageImporter"/> class using the current project.
    /// </summary>
    public PackageImporter()
    {
        _project = Project.Current;
    }

    /// <summary>
    /// Imports files from a package archive into the project.
    /// </summary>
    /// <param name="packageFileName">The path to the package zip file.</param>
    /// <param name="entryNames">An optional collection of specific entries to import. If null, all entries are imported.</param>
    /// <param name="packageFullName">An optional full package identifier for metadata tracking.</param>
    /// <param name="onComplete">An optional callback invoked when import completes.</param>
    /// <returns>A task representing the asynchronous import operation.</returns>
    public Task Import(string packageFileName, IEnumerable<string> entryNames = null, string packageFullName = null, Action onComplete = null)
    {
        CleanUp();

        var source = new TaskCompletionSource<bool>();

        DocumentManager.Instance.SaveAllDocuments();

        const string password = null;

        HashSet<string> entryIncludes = entryNames != null ? [.. entryNames] : null;

        Project project = Project.Current;

        EditorUtility.DoProgress(L("Importing..."), p =>
        {
            // Do not monitor any disk operations during the entire import process
            FileUnwatchedAction.Do(() =>
            {
                try
                {
                    using (Stream fs = File.OpenRead(packageFileName))
                    using (var zf = new ZipFile(fs))
                    {
                        if (!string.IsNullOrEmpty(password))
                        {
                            // AES encrypted entries are handled automatically
                            zf.Password = password;
                        }

                        var manifestEntry = zf.GetEntry(WorkSpaceExportSettingFileName);
                        if (manifestEntry != null)
                        {
                            p.UpdateProgess(0, L("Importing configuration..."), string.Empty);

                            try
                            {
                                using var stream = zf.GetInputStream(manifestEntry);
                                ReadManifest(stream);
                            }
                            catch (Exception err)
                            {
                                err.LogError(L("Failed to import configuration"));
                            }
                        }

                        var itrList = GroupZipEntryByIteration(zf);
                        foreach (var iteration in itrList)
                        {
                            foreach (ZipEntry zipEntry in iteration)
                            {
                                // Ignore directories
                                if (!zipEntry.IsFile)
                                {
                                    continue;
                                }

                                // Not selected
                                if (entryIncludes != null && !entryIncludes.Contains(zipEntry.Name))
                                {
                                    continue;
                                }

                                p.UpdateProgess(0, L($"{iteration.Key} Importing {zipEntry.Name}..."), string.Empty);

                                try
                                {
                                    ImportFile(zf, zipEntry, iteration.Key);
                                }
                                catch (Exception err)
                                {
                                    err.LogError(L($"Failed to import {zipEntry.Name}."));
                                }
                            }

                            EditorUtility.FlushDelayedActions();
                        }
                    }

                    p.UpdateProgess(0, L("Please wait..."), string.Empty);


                    // Open all documents and create assets in the project.
                    foreach (var assetFileNameItr in _assetFileNames)
                    {
                        foreach (var assetFileName in assetFileNameItr)
                        {
                            DocumentManager.Instance.OpenDocument(assetFileName, DocumentLoadingIntent.Import);
                        }
                    }

                    // Write package metadata to asset meta files
                    if (!string.IsNullOrEmpty(packageFullName))
                    {
                        foreach (var assetFileNameItr in _assetFileNames)
                        {
                            foreach (var assetFileName in assetFileNameItr.Where(w => !w.FileExtensionEquals(Asset.MetaExtension)))
                            {
                                string metaFileName = assetFileName + Asset.MetaExtension;
                                try
                                {
                                    MetaDataInfo info = null;
                                    if (File.Exists(metaFileName))
                                    {
                                        info = MetaDataInfo.Load(metaFileName);
                                    }

                                    info ??= new MetaDataInfo();
                                    info.PackageFullName = packageFullName;
                                    MetaDataInfo.Save(info, metaFileName);

                                    var asset = FileAssetManager.Current.GetAsset(assetFileName);
                                    asset?.LoadMetaFile(metaFileName);
                                }
                                catch (Exception err)
                                {
                                    err.LogError(L("Failed to update meta file") + ": " + metaFileName);
                                }
                            }
                        }
                    }

                    // Update or create workspaces referenced in the package
                    foreach (string workspaceName in _workspaceNames)
                    {
                        WorkSpace workSpace = EditorServices.WorkSpaceManager.GetWorkSpace(workspaceName);
                        if (workSpace != null)
                        {
                            workSpace.UpdateConfig();
                        }
                        else
                        {
                            workSpace = EditorServices.WorkSpaceManager.AddWorkSpace(workspaceName);
                            if (workSpace is null)
                            {
                                Logs.LogError(L("Failed to create workspace") + ": " + workspaceName);
                                continue;
                            }

                            if (workSpace.Controller is null)
                            {
                                workSpace.NewController<CommonController>();
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    err.LogError();
                }
                finally
                {
                    p.CompleteProgess();
                }
            });

            string[] assetFileNames = [];
            assetFileNames = _assetFileNames.SelectMany(o => o).ToArray();

            QueuedAction.Do(() =>
            {
                // Close all documents without saving as references have not been fully established yet
                foreach (var assetFileName in assetFileNames)
                {
                    var doc = DocumentManager.Instance.OpenDocument(assetFileName, DocumentLoadingIntent.Import);
                    if (doc != null)
                    {
                        DocumentManager.Instance.CloseDocument(assetFileName);
                    }
                }

                foreach (var assetFileName in assetFileNames)
                {
                    // Reopen all the documents with normal intent and force save to ensure all references are correctly resolved and saved in Guid format.
                    var doc = DocumentManager.Instance.OpenDocument(assetFileName, DocumentLoadingIntent.Normal);
                    if (doc != null)
                    {
                        // Force save once using Guid format
                        doc.ForceSave();

                        DocumentManager.Instance.CloseDocument(assetFileName);
                    }
                }

                EditorUtility.RefreshProjectView();
            });
        }, () =>
        {
            source.SetResult(true);
        });

        return source.Task;
    }

    /// <summary>
    /// Clears all internal state collected during a previous import operation.
    /// </summary>
    private void CleanUp()
    {
        _renderFiles.Clear();
        _assetFileNames.Clear();
        _workspaceNames.Clear();
    }

    /// <summary>
    /// Groups zip entries by their loading iteration order based on document format resolution.
    /// </summary>
    /// <param name="zf">The zip file to analyze.</param>
    /// <returns>A sorted list of entry groups ordered by iteration priority.</returns>
    private List<IGrouping<LoadingIterations, ZipEntry>> GroupZipEntryByIteration(ZipFile zf)
    {
        var files = zf.OfType<ZipEntry>().Where(o => o.IsFile);

        var act = AssetActivatorManager.Instance;
        var list = files.GroupBy(o => 
        {
            var stream = zf.GetInputStream(o);
            string ext = Path.GetExtension(o.Name);
            var resolve = DocumentManager.Instance.ResolveInFileFormat(ext, stream);

            return resolve?.Format?.Iteration ?? LoadingIterations.Iteration1;
        }).ToList();

        list.Sort((a, b) => a.Key.CompareTo(b.Key));

        return list;
    }

    /// <summary>
    /// Imports a single file entry from the zip archive to the project.
    /// </summary>
    /// <param name="zf">The zip file containing the entry.</param>
    /// <param name="zipEntry">The specific zip entry to import.</param>
    /// <param name="itr">The loading iteration this entry belongs to.</param>
    private void ImportFile(ZipFile zf, ZipEntry zipEntry, LoadingIterations itr)
    {
        string targetFileName = _project.ProjectBasePath.PathAppend(zipEntry.Name);

        var directoryName = Path.GetDirectoryName(targetFileName);
        if (directoryName.Length > 0)
        {
            Directory.CreateDirectory(directoryName);
        }

        if (File.Exists(targetFileName))
        {
            File.Delete(targetFileName);
            QueuedAction.Do(() => 
            {
                DocumentManager.Instance?.GetDocument(targetFileName)?.MarkDelete();
                DocumentManager.Instance.CloseDocument(targetFileName);
            });
        }

        bool isWorkSpaceFile = zipEntry.Name.StartsWith("WorkSpaces/");
        bool isAssetFile = zipEntry.Name.StartsWith("Assets/");

        string workSpaceName = null;
        WorkSpace workSpace = null;

        // Unzip file in buffered chunks. This is just as fast as unpacking
        // to a buffer the full size of the file, but does not waste memory.
        // The "using" will close the stream even if an exception occurs.
        using (var zipStream = zf.GetInputStream(zipEntry))
        {
            if (isAssetFile)
            {
                using Stream fsOutput = File.Create(targetFileName);
                StreamUtils.Copy(zipStream, fsOutput, _buffer);
            }
            else if (isWorkSpaceFile)
            {
                if (zipEntry.Name == WorkSpaceExportSettingFileName)
                {
                    return;
                }

                string rFileName = zipEntry.Name.RemoveFromFirst(11);
                workSpaceName = rFileName.FindAndGetBefore('/', true);
                workSpace = WorkSpaceManager.Current.GetWorkSpace(workSpaceName);

                string localFileName = rFileName.RemoveFromFirst(workSpaceName.Length + 1);
                bool inMaster = localFileName.StartsWith(WorkSpace.DefaultMasterDirectory);
                if (inMaster)
                {
                    localFileName = localFileName.RemoveFromFirst(WorkSpace.DefaultMasterDirectory.Length + 1);
                }

                if (workSpace != null && inMaster)
                {
                    targetFileName = workSpace.MakeMasterFullPath(localFileName);
                }

                if (_renderFiles.GetValueSafe(rFileName) is RenderFile renderFile)
                {
                    ImportRenderFile(zipStream, renderFile, targetFileName);
                }
                else
                {
                    using Stream fsOutput = File.Create(targetFileName);
                    StreamUtils.Copy(zipStream, fsOutput, _buffer);
                }
            }
        }

        // Update Workspace Config file
        if (isWorkSpaceFile)
        {
            workSpaceName = zipEntry.Name.RemoveFromFirst(11).FindAndGetBefore('/', true);
            _workspaceNames.Add(workSpaceName);
        }
        else if (isAssetFile)
        {
            int index = (int)itr;
            _assetFileNames.EnsureListSize(index + 1, () => []);
            _assetFileNames[index].Add(targetFileName);
        }
    }

    /// <summary>
    /// Imports a render file by resolving global IDs from the source content.
    /// </summary>
    /// <param name="stream">The input stream containing the source file content.</param>
    /// <param name="renderFile">The render file metadata specifying the language.</param>
    /// <param name="targetFileName">The destination file path.</param>
    private void ImportRenderFile(Stream stream, RenderFile renderFile, string targetFileName)
    {
        string source = string.Empty;

        using (var streamReader = new StreamReader(stream))
        {
            source = streamReader.ReadToEnd();
        }

        var config = RenderHelper.GetSegmentConfig(renderFile.Language);

        var doc = new SegmentDocument(config);
        doc.Parse(source, null);
        doc.ReplaceKeys(key =>
        {
            Guid id = GlobalIdResolver.Resolve(key);
            if (id != Guid.Empty)
            {
                return id.ToString();
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
    /// Reads the package manifest (export.json) from the zip archive to extract render file metadata.
    /// </summary>
    /// <param name="stream">The input stream containing the manifest JSON content.</param>
    private void ReadManifest(Stream stream)
    {
        string str = string.Empty;

        using (var streamReader = new StreamReader(stream))
        {
            str = streamReader.ReadToEnd();
        }

        var obj = ComputerBeacon.Json.Parser.Parse(str);

        var reader = new JsonDataReader(obj);

        foreach (var rfReader in reader.Nodes("renderFiles"))
        {
            string fileName = rfReader.Node("fileName").ReadString();
            string language = rfReader.Node("language").ReadString();

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                _renderFiles[fileName] = new RenderFile
                {
                    FileName = fileName,
                    Language = language,
                };
            }
        }
    }
}
