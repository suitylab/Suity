using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Project asset context that manages file-based assets, watches for file system changes,
/// and coordinates asset lifecycle with document management.
/// </summary>
internal sealed class FileAssetManagerBK : FileAssetManager
{
    /// <summary>
    /// Whether to log external file changes.
    /// </summary>
    public static bool LogFileChangedExternal = false;

    /// <summary>
    /// Enables debug logging for file watcher events.
    /// </summary>
    internal static bool MessageDebug_Watcher = false;

    /// <summary>
    /// Whether to load documents in parallel during scanning.
    /// </summary>
    internal static bool ParallelDocumentLoad = true;

    private readonly Project _project;
    private EditorFileSystemWatcher _watcher;
    private readonly ConcurrentQueue<FileUpdateItem> _updatingFileQueue = new();

    private bool _isReleased;

    private DisposeCollector _listeners;

    private readonly FileAssetCollection _collection;

    /// <summary>
    /// Creates a new file asset manager for the specified project.
    /// </summary>
    /// <param name="project">The owning project.</param>
    /// <param name="basePath">The base directory path for assets.</param>
    internal FileAssetManagerBK(Project project, string basePath)
        : base(basePath)
    {
        EditorServices.SystemLog.AddLog($"FileAssetManager creating : {basePath}...");
        EditorServices.SystemLog.PushIndent();

        Debug.Assert(project != null);
        Debug.Assert(!string.IsNullOrEmpty(basePath));

        _project = project;

        _watcher = new EditorFileSystemWatcher(basePath, this)
        {
            IncludeSubdirectories = true
        };

        _collection = new FileAssetCollection(basePath);

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("FileAssetManager created.");
    }

    /// <summary>
    /// Gets the project that owns this asset manager.
    /// </summary>
    public Project OwnerProject => _project;

    /// <summary>
    /// Starts the file watcher and registers event listeners.
    /// </summary>
    internal void Start()
    {
        EditorServices.SystemLog.AddLog($"FileAssetManager starting...");
        EditorServices.SystemLog.PushIndent();

        _watcher.Created += _watcher_Created;
        _watcher.Deleted += _watcher_Deleted;
        _watcher.Changed += _watcher_Changed;
        _watcher.Renamed += _watcher_Renamed;

        _watcher.EnableRaisingEvents = true;

        DocumentManager.Instance.DocumentLoaded += documentManager_DocumentCreated;
        DocumentManager.Instance.DocumentChangedExternal += documentManager_DocumentChangedExternal;

        _listeners += EditorRexes.Mapper.Provide<FileAssetManager>(this);

        EditorServices.FileUpdateService.AddFileUpdateListener(LoadingIterations.Iteration1, DoFileUpdateByIteration);
        EditorServices.FileUpdateService.UpdateFinished += (s, e) =>
        {
            if (_updatingFileQueue.Count > 0)
            {
                EditorServices.FileUpdateService.UpdateFileDelayed();
            }
        };

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"FileAssetManager started...");
    }

    /// <summary>
    /// Releases resources and stops the file watcher.
    /// </summary>
    internal void Release()
    {
        EditorServices.SystemLog.AddLog($"FileAssetManager releasing...");

        DocumentManager.Instance.DocumentLoaded -= documentManager_DocumentCreated;
        DocumentManager.Instance.DocumentChangedExternal -= documentManager_DocumentChangedExternal;

        _listeners?.Dispose();
        _listeners = null;
        _isReleased = true;

        if (_watcher != null)
        {
            _watcher.Created -= _watcher_Created;
            _watcher.Deleted -= _watcher_Deleted;
            _watcher.Changed -= _watcher_Changed;
            _watcher.Renamed -= _watcher_Renamed;

            _watcher.Dispose();
            _watcher = null;
        }

        EditorServices.SystemLog.AddLog($"FileAssetManager released...");
    }

    /// <inheritdoc/>
    public override void EnsureStorage(Asset asset)
    {
        asset.GetDocumentEntry(true);
    }

    /// <inheritdoc/>
    public override Asset GetAsset(string fullPath)
    {
        return _collection.Get(fullPath);
    }

    /// <inheritdoc/>
    internal override Asset GetOrUpdateAsset(string fullPath)
    {
        return _collection.GetOrUpdate(fullPath);
    }

    /// <inheritdoc/>
    internal override Asset UpdateAsset(string fullPath)
    {
        return _collection.Update(fullPath);
    }

    /// <inheritdoc/>
    public override ImageDef GetIcon(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        ImageDef image;
        do
        {
            Asset asset = GetAsset(fullPath);
            image = asset?.Icon;
            if (image != null)
            {
                break;
            }

            var document = DocumentManager.Instance.GetDocument(fullPath);
            image = document?.Icon;
            if (image != null)
            {
                break;
            }

            var factory = DocumentManager.Instance.GetDocumentFormatByPath(fullPath);
            image = factory?.Icon;
            if (image != null)
            {
                break;
            }
        } while (false);

        return image;
    }

    /// <inheritdoc/>
    internal override IReferenceHost GetReferenceHost(string fullPath)
    {
        return _collection.GetReferenceHost(fullPath);
    }

    /// <inheritdoc/>
    internal override IReferenceHost EnsureReferenceHost(string fullPath)
    {
        return _collection.EnsureReferenceHost(fullPath);
    }

    /// <inheritdoc/>
    internal override void RemoveReferenceHost(string fullPath)
    {
        _collection.RemoveReferenceHost(fullPath);
    }

    /// <inheritdoc/>
    internal override DocumentEntry GetDocumentEntry(EditorObject obj, bool tryLoadStorage)
    {
        if (obj.Entry is null)
        {
            return null;
        }

        var location = obj.GetStorageLocation();
        if (location != null)
        {
            if (tryLoadStorage)
            {
                //Logs.LogInfo($"ThreadId={Thread.CurrentThread.ManagedThreadId} OpenDocument by EditorObject : {fileName.FullPath}");

                DocumentEntry docEntry = DocumentManager.Instance.OpenDocument(location);

                if (docEntry?.Content is AssetDocument assetDoc)
                {
                    // When document opens naturally, assets will be attached after one frame delay
                    // When manually getting document, assets need to be attached immediately
                    var builder = assetDoc.AssetBuilder;
                    if (builder != null)
                    {
                        LockedWithFileAsset(builder, docEntry.FileName);
                        builder.ResolveId();
                        builder.Owner = assetDoc;
                    }
                }

                return docEntry;
            }
            else
            {
                return DocumentManager.Instance.GetDocument(location.FullPath);
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    internal override TAssetBuilder LockedWithFileAsset<TAssetBuilder>(TAssetBuilder builder, StorageLocation location)
    {
        if (builder is null)
        {
            return null;
        }

        if (builder.TargetAsset != null)
        {
            return builder;
        }

        if (location is null)
        {
            lock (builder)
            {
                return builder.WithAsset();
            }
        }

        var asset = location.GetAsset();

        if (asset != null)
        {
            EditorServices.SystemLog.AddLog($"Asset attaching : {location.FullPath}");

            lock (builder)
            {
                builder.WithLocalName(asset.AssetKey);

                if (builder.AttachAsset(asset))
                {
                    EditorServices.SystemLog.AddLog($"Asset attached : {location.FullPath}");
                }
                else
                {
                    EditorServices.SystemLog.AddLog($"Asset attached failed : {location.FullPath}");
                    builder.WithAsset();

                    if (location.PhysicFileName != null)
                    {
                        UpdateAsset(location.PhysicFileName);
                    }
                }
            }
        }
        else
        {
            EditorServices.SystemLog.AddLog($"Asset created : {location.FullPath}");
            string assetKey = MakeAssetKey(location.FullPath);

            lock (builder)
            {
                builder.WithLocalName(assetKey);
                builder.WithAsset();
            }
        }

        return builder;
    }

    #region Scan

    /// <summary>
    /// Scans the project directory and updates all assets.
    /// </summary>
    public void ScanProjectDirectory()
    {
        // Get file list
        var interations = GroupProjectFilesByIteration();

        FileUnwatchedAction.Do(() =>
        {
            foreach (var iteration in interations)
            {
                // Load assets
                foreach (FileInfo file in iteration.Select(o => o.FilePath))
                {
                    _collection.Update(file.FullName);
                }
            }
        });

        // Build dependency cache
        ReferenceManager.Current.Update();
    }

    /// <summary>
    /// Scans the project directory asynchronously with progress reporting.
    /// </summary>
    /// <returns>A task representing the scan operation.</returns>
    public Task ScanProjectDirectoryWithTask()
    {
        // Get file list
        var iterations = GroupProjectFilesByIteration();

        if (ParallelDocumentLoad)
        {
            //TODO: Distinguish Iteration

            return EditorUtility.DoProgress("Loading documents...", p =>
            {
                EditorObjectManager.Instance.DoUnwatchedAction(() =>
                {
                    int index = 1;

                    foreach (var iteration in iterations)
                    {
                        //TODO: Parallel.ForEach will cause errors
                        Parallel.ForEach(iteration, file =>
                        {
                            p.UpdateProgess(index, iterations.Count, $"Loading {file.FilePath.Name}", $"Iteration {iteration.Key}");

                            //_collection.Create(file.FullName);
                            //DocumentManager.Instance.GetOrOpenDocument(file.FullName);
                            _collection.Update(file.FilePath.FullName);

                            Interlocked.Increment(ref index);
                        });
                    }
                });

                ReferenceManager.Current.Update();

                // Prevent completion too fast
                Thread.Sleep(100);

                p.CompleteProgess();
            });
        }
        else
        {
            return EditorUtility.DoProgress("Loading documents...", p =>
            {
                EditorObjectManager.Instance.DoUnwatchedAction(() =>
                {
                    int index = 0;

                    foreach (var group in iterations)
                    {
                        foreach (var file in group)
                        {
                            p.UpdateProgess(index, iterations.Count, $"Loading {file.FilePath.Name}", $"Iteration {group.Key}");
                            //_collection.Create(file.FullName);
                            _collection.Update(file.FilePath.FullName);

                            index++;
                        }
                    }
                });

                ReferenceManager.Current.Update();

                // Prevent completion too fast
                Thread.Sleep(100);

                p.CompleteProgess();
            });
        }
    }

    /// <summary>
    /// Groups all project files by their loading iteration order.
    /// </summary>
    /// <returns>A list of file groups ordered by iteration.</returns>
    private List<IGrouping<LoadingIterations, FileUpdateItem>> GroupProjectFilesByIteration()
    {
        List<FileUpdateItem> files = DirectoryUtility.GetAllFiles(DirectoryBasePath)
            .Where(o => !o.GetIsMetaFile())
            .Select(o => new FileUpdateItem(o))
            .ToList();

        return GroupFileByIteration(files);
    }

    /// <summary>
    /// Groups files by their loading iteration based on document format.
    /// </summary>
    /// <param name="files">The files to group.</param>
    /// <returns>A sorted list of file groups by iteration.</returns>
    private List<IGrouping<LoadingIterations, FileUpdateItem>> GroupFileByIteration(IEnumerable<FileUpdateItem> files)
    {
        var act = AssetActivatorManager.Instance;
        var list = files.GroupBy(o => 
        {
            var resolve = DocumentManager.Instance.ResolveInFileFormat(o.FilePath.FullName);

            return resolve?.Format?.Iteration ?? LoadingIterations.Iteration1;
        }).ToList();

        list.Sort((a, b) => a.Key.CompareTo(b.Key));

        return list;
    }

    #endregion

    #region Watcher events

    /// <summary>
    /// Handles file creation events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the created file.</param>
    private void _watcher_Created(string fullPath)
    {
        _updatingFileQueue.Enqueue(new FileUpdateItem(FileUpdateType.Created, fullPath));
        
        EditorServices.FileUpdateService.UpdateFileDelayed();
    }

    /// <summary>
    /// Handles file deletion events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the deleted file.</param>
    private void _watcher_Deleted(string fullPath)
    {
        _updatingFileQueue.Enqueue(new FileUpdateItem(FileUpdateType.Deleted, fullPath));
        EditorServices.FileUpdateService.UpdateFileDelayed();
    }

    /// <summary>
    /// Handles file change events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The full path of the changed file.</param>
    private void _watcher_Changed(string fullPath)
    {
        _updatingFileQueue.Enqueue(new FileUpdateItem(FileUpdateType.Changed, fullPath));
        EditorServices.FileUpdateService.UpdateFileDelayed();
    }

    /// <summary>
    /// Handles file rename events from the file system watcher.
    /// </summary>
    /// <param name="fullPath">The new full path of the renamed file.</param>
    /// <param name="oldFullPath">The previous full path of the file.</param>
    private void _watcher_Renamed(string fullPath, string oldFullPath)
    {
        _updatingFileQueue.Enqueue(new FileUpdateItem(FileUpdateType.Renamed, fullPath, oldFullPath));
        EditorServices.FileUpdateService.UpdateFileDelayed();
    }

    #endregion

    #region Document manager events

    /// <summary>
    /// Handles document creation events from the document manager.
    /// </summary>
    /// <param name="document">The created document entry.</param>
    private void documentManager_DocumentCreated(DocumentEntry document)
    {
        if (document.FileName.PhysicFileName != null)
        {
            // Do not use delayed loading, as there may be other operations that need to be performed after binding after document creation
            EditorObjectManager.Instance.DoUnwatchedAction(() =>
            {
                _collection.Update(document.FileName.PhysicFileName);
            });
        }
    }

    /// <summary>
    /// Handles external document change events from the document manager.
    /// </summary>
    /// <param name="documentEntry">The document entry that was changed externally.</param>
    private void documentManager_DocumentChangedExternal(DocumentEntry documentEntry)
    {
        if (documentEntry.FileName.PhysicFileName != null)
        {
            _updatingFileQueue.Enqueue(new FileUpdateItem(FileUpdateType.Changed, documentEntry.FileName.PhysicFileName));
            EditorServices.FileUpdateService.UpdateFileDelayed();
        }
    }

    #endregion

    #region File update

    /// <summary>
    /// Processes pending file updates grouped by iteration.
    /// </summary>
    /// <param name="p">Progress reporter for the update operation.</param>
    private void DoFileUpdateByIteration(IProgress p)
    {
        HashSet<string> changedFileNames = null;
        List<FileUpdateItem> items = [];

        while (_updatingFileQueue.TryDequeue(out FileUpdateItem item))
        {
            items.Add(item);
        }
        if (items.Count == 0)
        {
            lock (this)
            {
                p.CompleteProgess();
                return;
            }
        }

        var list = GroupFileByIteration(items);
        foreach (var iteration in list)
        {
            var ary = iteration.ToArray();

            for (int i = 0; i < ary.Length; i++)
            {
                FileUpdateItem item = ary[i];
                float rate = (float)i / (float)ary.Length;
                int percent = (int)(rate * 100f);
                p.UpdateProgess(percent, $"Updating {item.FilePath}...", $"Iteration {iteration.Key}");

                switch (item.UpdateType)
                {
                    case FileUpdateType.Created:
                        DoFileUpdate_Created(item.FilePath.FullName);
                        break;

                    case FileUpdateType.Deleted:
                        QueuedAction.Do(() => DoFileUpdate_Deleted(item.FilePath.FullName));
                        break;

                    case FileUpdateType.Changed:
                        // Prevent duplicate mechanism
                        if (changedFileNames?.Contains(item.FilePath.FullName) == true)
                        {
                            break;
                        }

                        DoFileUpdate_Changed(item.FilePath.FullName);
                        (changedFileNames ??= []).Add(item.FilePath.FullName);
                        break;

                    case FileUpdateType.Renamed:
                        QueuedAction.Do(() => DoFileUpdate_Renamed(item.FilePath.FullName, item.OldFilePath.FullName));
                        break;

                    default:
                        break;
                }
            }
        }

        // Force send update reference command
        EditorRexes.RaiseUpdateReference.Invoke();

        p.CompleteProgess();
    }

    /// <summary>
    /// Handles file creation updates.
    /// </summary>
    /// <param name="fullPath">The full path of the created file or directory.</param>
    private void DoFileUpdate_Created(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            if (MessageDebug_Watcher)
            {
                Logs.LogDebug($"*** File Created : {fullPath}");
            }

            //_collection.Create(fullPath);
            //DocumentManager.Instance.GetOrOpenDocument(fullPath);
            _collection.Update(fullPath);
        }
        else if (Directory.Exists(fullPath))
        {
            if (MessageDebug_Watcher)
            {
                Logs.LogDebug($"*** Directory Created : {fullPath}");
            }

            List<FileInfo> files = [.. DirectoryUtility.GetAllFiles(fullPath)];

            foreach (FileInfo file in files)
            {
                if (MessageDebug_Watcher)
                {
                    Logs.LogDebug($"*** File Created : {file.FullName}");
                }
                //_collection.Create(file.FullName);
                //DocumentManager.Instance.GetOrOpenDocument(fullPath);
                _collection.Update(file.FullName);
            }
        }
    }

    /// <summary>
    /// Handles file deletion updates, prompting for unsaved documents.
    /// </summary>
    /// <param name="fullPath">The full path of the deleted file or directory.</param>
    private async void DoFileUpdate_Deleted(string fullPath)
    {
        // Modified files should prompt
        DocumentEntry document = DocumentManager.Instance.GetDocument(fullPath);
        string assetName = MakeAssetKey(fullPath);

        if (document != null)
        {
            bool doDelete = true;

            if (document.IsDirty)
            {
                doDelete = !await DialogUtility.ShowYesNoDialogAsync(string.Format("{0} has been deleted, keep document?", assetName));
            }

            if (doDelete)
            {
                // Remove document
                DocumentManager.Instance.CloseDocument(document);
            }
            else
            {
                // Keep document
                document.Save();
                document.ShowProperty();
                document.ShowView();
            }

            // If document removed, remove asset
            if (document.IsReleased)
            {
                if (_collection.Remove(assetName))
                {
                    if (MessageDebug_Watcher)
                    {
                        Logs.LogDebug("*** File Deleted : " + fullPath);
                    }
                }
            }
        }
        else
        {
            if (_collection.Remove(fullPath))
            {
                if (MessageDebug_Watcher)
                {
                    Logs.LogDebug("*** File Deleted : " + fullPath);
                }
            }
        }

        foreach (Asset subAsset in GetAssetsInDirectory(fullPath))
        {
            if (_collection.Remove(subAsset.FileName?.PhysicFileName))
            {
                if (MessageDebug_Watcher)
                {
                    Logs.LogDebug("*** File Deleted : " + subAsset.AssetKey);
                }
            }
        }
    }

    /// <summary>
    /// Handles file change updates, reloading documents if needed.
    /// </summary>
    /// <param name="fullPath">The full path of the changed file or directory.</param>
    private async void DoFileUpdate_Changed(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            if (MessageDebug_Watcher)
            {
                Logs.LogDebug("*** File Changed : " + fullPath);
            }

            if (LogFileChangedExternal)
            {
                Logs.LogDebug(new ObjectLogCoreItem($"File changed externally : {fullPath}", new StorageLocation(fullPath)));
            }

            DocumentEntryBK documentEntry = DocumentManager.Instance.GetDocument(fullPath) as DocumentEntryBK;
            if (documentEntry != null)
            {
                if (!documentEntry.IsDirty)
                {
                    documentEntry.InternalReload();
                }
                else
                {
                    bool reload = await DialogUtility.ShowYesNoDialogAsync($"{Path.GetFileName(fullPath)} has been changed externally, reload?");
                    if (reload)
                    {
                        documentEntry.InternalReload();
                    }
                }
            }

            // Create new asset to overwrite old asset regardless
            Asset asset = _collection.Update(fullPath);

            // Update dependencies while document still exists
            if (asset != null || documentEntry != null)
            {
                ReferenceManager.Current.Update();
            }
        }
        if (Directory.Exists(fullPath))
        {
            // Remove non-existent assets
            foreach (Asset asset in GetAssetsInDirectory(fullPath))
            {
                string physicFileName = asset.FileName?.PhysicFileName;
                string assetFullPath = MakeFullPath(asset.AssetKey);

                // Previous judgment of assetFullPath may be incorrect, PhysicFileName should be the real path of the resource.
                if (!string.IsNullOrWhiteSpace(physicFileName) && !File.Exists(physicFileName))
                {
                    if (_collection.Remove(physicFileName))
                    {
                        if (MessageDebug_Watcher)
                        {
                            Logs.LogDebug("*** File Deleted : " + asset.AssetKey);
                        }
                    }
                }
            }
            // Add new assets
            foreach (FileInfo fileInfo in DirectoryUtility.GetAllFiles(fullPath))
            {
                if (GetAsset(fileInfo.FullName) == null)
                {
                    if (MessageDebug_Watcher)
                    {
                        Logs.LogDebug("*** File Created : " + fileInfo.FullName);
                    }
                    //_collection.Create(fileInfo.FullName);
                    DocumentManager.Instance.OpenDocument(fileInfo.FullName);
                }
            }
        }
    }

    /// <summary>
    /// Handles file rename updates, performing pre and post rename operations.
    /// </summary>
    /// <param name="fullPath">The new full path after rename.</param>
    /// <param name="oldFullPath">The previous full path before rename.</param>
    private void DoFileUpdate_Renamed(string fullPath, string oldFullPath)
    {
        // LiteDB auto backup
        if (fullPath.EndsWith("-bkp.sbunch") || oldFullPath.EndsWith("-temp.sbunch"))
        {
            return;
        }

        // CommandUtility.ClearLog.Invoke();
        // Logs.LogDebug("Start executing internal rename");

        if (File.Exists(fullPath))
        {
            FileUnwatchedAction.Do(() => PreExternalRename(fullPath, oldFullPath));
            PostExternalRename(fullPath, oldFullPath);
        }
        else if (Directory.Exists(fullPath))
        {
            //AppService.Instance.DispatchEvent(this, new CommonNotifyEvent(CommonNotifyEvent.ClearLog));
            if (MessageDebug_Watcher)
            {
                Logs.LogDebug("*** Directory Renamed : " + fullPath);
            }

            List<FileInfo> files = [.. DirectoryUtility.GetAllFiles(fullPath)];

            FileUnwatchedAction.Do(() =>
            {
                foreach (FileInfo file in files)
                {
                    string fileRelativePath = file.FullName.MakeRalativePath(fullPath);
                    string fileOldFullPath = fileRelativePath.MakeFullPath(oldFullPath);
                    PreExternalRename(file.FullName, fileOldFullPath);
                }
            });

            foreach (FileInfo file in files)
            {
                string fileRelativePath = file.FullName.MakeRalativePath(fullPath);
                string fileOldFullPath = fileRelativePath.MakeFullPath(oldFullPath);
                PostExternalRename(file.FullName, fileOldFullPath);
            }
        }
    }

    #endregion

    /// <summary>
    /// Handles an external rename action, performing pre and post rename operations.
    /// </summary>
    /// <param name="renameAction">The rename action to execute.</param>
    public void HandleExternalRename(RenameAction renameAction)
    {
        if (renameAction == null)
        {
            throw new ArgumentNullException();
        }

        RenameItem[] items = null;

        //Logs.LogDebug("Start executing external rename");
        EditorCommands.ClearLog.Invoke();

        FileUnwatchedAction.Do(() =>
        {
            items = renameAction();

            if (items != null)
            {
                foreach (RenameItem item in items)
                {
                    PreExternalRename(item.FileName, item.OldFileName);
                }
            }
        });

        if (items != null)
        {
            QueuedAction.Do(() =>
            {
                foreach (RenameItem item in items)
                {
                    PostExternalRename(item.FileName, item.OldFileName);
                }
            });
        }
    }

    /// <summary>
    /// Performs pre-rename operations on documents and assets.
    /// </summary>
    /// <param name="newFullPath">The new full path after rename.</param>
    /// <param name="oldFullPath">The previous full path before rename.</param>
    private void PreExternalRename(string newFullPath, string oldFullPath)
    {
        if (EditorUtility.IsFileWatching)
        {
            throw new InvalidOperationException("Call this method inside FileUnwatchedAction.Do()");
        }

        if (File.Exists(newFullPath))
        {
            string oldExt = Path.GetExtension(oldFullPath);
            string newExt = Path.GetExtension(newFullPath);

            oldExt = oldExt != null ? oldExt.TrimStart('.') : string.Empty;
            newExt = newExt != null ? newExt.TrimStart('.') : string.Empty;

            // Rename document
            DocumentManagerBK.Instance.RenameDocument(oldFullPath, newFullPath);

            // Rename asset
            Asset asset = _collection.Get(oldFullPath);
            if (asset != null)
            {
                // If format is the same
                if (oldExt == newExt)
                {
                    // Then only rename operation
                    _collection.ChangePath(oldFullPath, newFullPath);
                }
                else
                {
                    // Perform remove and add operation
                    _collection.Remove(oldFullPath);
                    _collection.Update(newFullPath);
                }
            }
            else
            {
                _collection.Update(newFullPath);
            }

            //LinkedAssetHelper.SaveDocuments(refSets);
        }
        else if (Directory.Exists(newFullPath))
        {
            // Directory rename handling
        }
        else
        {
            // File not exist
        }
    }

    /// <summary>
    /// Performs post-rename operations, recording the new asset name.
    /// </summary>
    /// <param name="fullPath">The new full path after rename.</param>
    /// <param name="oldFullPath">The previous full path before rename.</param>
    private void PostExternalRename(string fullPath, string oldFullPath)
    {
        // Record asset new name
        var asset = _collection.Get(fullPath);
        if (asset != null)
        {
            MarkAssetKeyDeep(asset);
        }
    }

    /// <summary>
    /// Recursively marks asset keys for code render elements.
    /// </summary>
    /// <param name="node">The code render element to process.</param>
    private void MarkAssetKeyDeep(ICodeRenderElement node)
    {
        return;

        object idObj = node.GetProperty(CodeRenderProperty.IdProperty);
        string pathName = node.GetProperty(CodeRenderProperty.PathNameProperty) as string;

        if (idObj is Guid && !string.IsNullOrEmpty(pathName))
        {
            GlobalIdResolver.Record(pathName, (Guid)idObj);
        }

        if (node.GetProperty(CodeRenderProperty.ChildNodesProperty) is IEnumerable<object> childObjs)
        {
            foreach (var childObj in childObjs.OfType<ICodeRenderElement>())
            {
                MarkAssetKeyDeep(childObj);
            }
        }
    }

    /// <summary>
    /// Gets all assets contained within a directory.
    /// </summary>
    /// <param name="fullPath">The full path of the directory.</param>
    /// <returns>Array of assets found in the directory.</returns>
    private Asset[] GetAssetsInDirectory(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return [];
        }

        string prefix = this.MakeAssetKey(fullPath);

        return [.. AssetManager.Instance.GetAssetsByPrefix(prefix)];
    }

    /// <summary>
    /// Represents the type of file update operation.
    /// </summary>
    private enum FileUpdateType
    {
        None,
        Created,
        Deleted,
        Changed,
        Renamed,
    }

    /// <summary>
    /// Represents a file update item with type and path information.
    /// </summary>
    private class FileUpdateItem
    {
        /// <summary>
        /// The type of update operation.
        /// </summary>
        public FileUpdateType UpdateType { get; }

        /// <summary>
        /// The file path being updated.
        /// </summary>
        public FileInfo FilePath { get; }

        /// <summary>
        /// The previous file path (for rename operations).
        /// </summary>
        public FileInfo OldFilePath { get; }

        /// <summary>
        /// Creates a file update item for a simple file.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        public FileUpdateItem(FileInfo fileInfo)
        {
            UpdateType = FileUpdateType.None;
            FilePath = fileInfo;
        }

        /// <summary>
        /// Creates a file update item with a specific update type.
        /// </summary>
        /// <param name="updateType">The type of update.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="oldFilePath">The old file path for renames.</param>
        public FileUpdateItem(FileUpdateType updateType, string filePath, string oldFilePath = null)
        {
            UpdateType = updateType;
            FilePath = new FileInfo(filePath);

            if (!string.IsNullOrEmpty(oldFilePath))
            {
                OldFilePath = new FileInfo(oldFilePath);
            }
        }
    }
}
