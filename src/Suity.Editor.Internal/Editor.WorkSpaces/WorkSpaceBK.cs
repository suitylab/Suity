using Suity;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Expressions;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Internal implementation of <see cref="WorkSpace"/> that manages rendering, references, configuration,
/// and the lifecycle of a single workspace within a project.
/// </summary>
public class WorkSpaceBK : WorkSpace,
    ISyncObject,
    IViewObject,
    IRenderHost,
    IViewListener,
    IEditorObjectListener,
    IViewRedirect,
    IInspectorSplittedView
{
    /// <summary>
    /// Gets or sets whether debug logging is enabled for workspace operations.
    /// </summary>
    public static bool LogDebug = false;
    private static readonly HashSet<Guid> ValidateIgnoreIds = [];

    private readonly WatchableList<WorkSpaceRefItem> _references = [];
    private readonly List<IAssemblyReferenceItem> _assemblyRefs = [];
    private readonly WatchableList<string> _conditions = [];

    private RenderTargetPage _renderPage;
    private readonly object _pageSync = new();

    private readonly RenderRecordCollectionBK _renderRecord;
    private readonly List<RenderFileRecordBK> _renderedFilesConfig = [];

    private readonly ConcurrentHashSet<Guid> _dirtyRefs = [];

    private readonly WorkSpaceManager _manager;
    private bool _analyzed;
    private string _baseNameSpace = string.Empty;
    private string _externalRPath = null;
    private readonly SyncReferenceHost _refSet;
    private bool _released;
    private Guid _guid;
    private bool _debug;

    private WorkSpaceAsset _asset;

    private int _dirtyRenderTargetCount;
    private bool _configDirty;

    private EditorFileSystemWatcher _configWatcher;
    private bool _needReloadConfig;

    private readonly AnalyzeAction _analyzeAction;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceBK"/> with the specified manager and name.
    /// </summary>
    /// <param name="manager">The owning workspace manager.</param>
    /// <param name="name">The name of the workspace.</param>
    internal WorkSpaceBK(WorkSpaceManager manager, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(L($"\"{nameof(name)}\" cannot be null or whitespace."), nameof(name));
        }

        _renderPage = new RenderTargetPage(this);

        _manager = manager;
        Name = name;
        _baseNameSpace = name;

        _references.Updated += (mode, index, old) =>
        {
            switch (mode)
            {
                case EventListUpdateMode.Added:
                    _references[index].WorkSpace = this;
                    break;

                case EventListUpdateMode.Removed:
                    old.WorkSpace = null;
                    break;

                case EventListUpdateMode.Changed:
                    _references[index].WorkSpace = this;
                    old.WorkSpace = null;
                    break;

                default:
                    break;
            }

            MarkConfigDirty();
        };

        _conditions.Updated += (mode, index, old) =>
        {
            MarkConfigDirty();
            MarkAllDirty();
        };

        _renderRecord = new RenderRecordCollectionBK(this);

        _refSet = new ObjectReferenceHost(this);
        _refSet.MarkDirty();
        _guid = Guid.NewGuid();

        EnsureDirectory();

        _asset = new WorkSpaceAsset(this);

        _configWatcher = new EditorFileSystemWatcher(BaseDirectory, this)
        {
            Filter = DefaultWorkSpaceConfigFileName,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };
        _configWatcher.Changed += p =>
        {
            if (_released)
            {
                return;
            }
            _needReloadConfig = true;
            EditorServices.FileUpdateService.UpdateFileDelayed();
        };

        EditorServices.FileUpdateService.AddFileUpdateListener(LoadingIterations.WorkSpace, DoFileUpdateWorkSpace);

        _analyzeAction = new AnalyzeAction(this);
    }

    private void DoFileUpdateWorkSpace(IProgress obj)
    {
        if (_needReloadConfig)
        {
            _needReloadConfig = false;
            Logs.LogInfo($"Reloading workspace: {Name}");
            LoadConfig(false);
        }
    }

    /// <summary>
    /// Gets the internal render target page for this workspace.
    /// </summary>
    internal RenderTargetPage RenderPage => _renderPage;

    /// <summary>
    /// Releases all resources and stops the workspace controller.
    /// </summary>
    internal void Release()
    {
        _configWatcher?.Dispose();
        _configWatcher = null;

        StopController();

        _refSet.Remove();
        _released = true;

        _asset?.Release();
        _asset = null;
    }

    #region Property

    /// <inheritdoc/>
    public override WorkSpaceManager Manager => _manager;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    public override Guid Id => _asset?.Id ?? Guid.Empty;

    /// <inheritdoc/>
    public override string AssetKey => KeyCode.Combine(WorkSpace.WorkspaceAssetKeyPrefix, Name);

    /// <inheritdoc/>
    public override string BaseDirectory => _manager.BasePath.PathAppend(Name);

    /// <inheritdoc/>
    public override string BaseNameSpace
    {
        get => _baseNameSpace;
        set
        {
            if (_baseNameSpace != value)
            {
                _baseNameSpace = value;
                MarkAllDirty();
            }
        }
    }

    /// <inheritdoc/>
    public override bool Debug
    {
        get => _debug;
        set
        {
            _debug = value;
        }
    }

    /// <inheritdoc/>
    public override string MasterDirectory
    {
        get
        {
            string configBasePath = BaseDirectory;

            if (!string.IsNullOrEmpty(_externalRPath))
            {
                string path = _externalRPath.MakeFullPath(configBasePath);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return configBasePath.PathAppend(DefaultMasterDirectory);
        }
    }

    /// <inheritdoc/>
    public override bool IsExternalMasterDirectory => !string.IsNullOrEmpty(_externalRPath);

    /// <inheritdoc/>
    public override string TempDirectory => _manager.BasePath.PathAppend(Name).PathAppend(Temp);

    /// <inheritdoc/>
    public override string ConfigFileName => BaseDirectory.PathAppend(DefaultWorkSpaceConfigFileName);

    /// <inheritdoc/>
    public override string DbFileName => BaseDirectory.PathAppend(DefaultWorkSpaceDbFileName);

    /// <inheritdoc/>
    public override ICodeLibrary UserCode => _asset;

    /// <inheritdoc/>
    public override Guid WorkSpaceGuid => _guid;

    /// <inheritdoc/>
    public override RenderRecordCollection Records => _renderRecord;

    /// <inheritdoc/>
    public override ImageDef Icon
    {
        get
        {
            if (IsFailure)
            {
                return CoreIconCache.Error;
            }
            else if (ControllerInfo != null)
            {
                return EditorUtility.GetIconByAssetKey(ControllerInfo.IconKey) ?? CoreIconCache.WorkSpace;
            }
            else
            {
                return CoreIconCache.Question;
            }
        }
    }

    /// <summary>
    /// Gets the internal render record collection for tracking file rendering state.
    /// </summary>
    internal RenderRecordCollectionBK RenderRecord => _renderRecord;

    #endregion

    #region Reference

    /// <inheritdoc/>
    public override IWorkSpaceRefItem AddReferenceItem(Guid id)
    {
        Asset asset = AssetManager.Instance.GetAsset(id);

        WorkSpaceRefItem item = null;
        bool added = false;

        switch (asset)
        {
            case IRenderable renderable:
                if (!_references.Any(o => o.Id == asset.Id))
                {
                    item = new RenderableRefItem(asset.Id) { Modified = OnRefItemModified };

                    _references.InsertSorted(item, (v1, v2) => v1.CompareTo(v2));
                    added = true;
                }
                break;

            case IRenderTargetLibrary library:
                if (!_references.Any(o => o.Id == asset.Id))
                {
                    item = new RenderTargetLibraryRefItem(asset.Id) { Modified = OnRefItemModified };

                    _references.InsertSorted(item, (v1, v2) => v1.CompareTo(v2));
                    added = true;
                }
                break;

            case ICodeLibrary userCode:
                if (!_references.Any(o => o.Id == asset.Id))
                {
                    item = new UserFileRefItem() { Id = asset.Id, Modified = OnRefItemModified };

                    _references.InsertSorted(item, (v1, v2) => v1.CompareTo(v2));
                    added = true;
                }
                break;

            case IFileBunch fileBunch:
                if (!_references.Any(o => o.Id == asset.Id))
                {
                    item = new FileBunchRefItem() { Id = asset.Id, Modified = OnRefItemModified };

                    _references.InsertSorted(item, (v1, v2) => v1.CompareTo(v2));
                    added = true;
                }
                break;

            default:
                break;
        }

        // Automatically add user code with same path and same name
        var codeLib = asset.GetAttachedUserLibrary();
        if (codeLib != null)
        {
            item.UserCodeId = codeLib.Id;
        }

        _refSet.MarkDirty();
        _dirtyRefs.Add(id);

        if (added)
        {
            RequestAnalyze();
            return item;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override void RemoveReferenceItem(Guid id)
    {
        if (_references.RemoveAll(o => o.Id == id) > 0)
        {
            RequestAnalyze();
        }
        _refSet.MarkDirty();
        _dirtyRefs.Remove(id);
    }

    /// <inheritdoc/>
    public override IWorkSpaceRefItem GetReferenceItem(Guid id)
    {
        return _references.Find(o => o.Id == id);
    }

    /// <inheritdoc/>
    public override void SetupFileBunch(IFileBunch bunch)
    {
        if (bunch.Id == Guid.Empty)
        {
            return;
        }

        AddReferenceItem(bunch.Id);

        var basePath = new RenderFileName(MasterDirectory, BaseNameSpace);

        foreach (var target in bunch.GetRenderTargets(basePath, false))
        {
            _renderRecord.AddRenderedFile(target);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderFileName> GetAffectedFileNames(Guid id)
    {
        return GetAffactedRenderTargets(id).Select(o => o.FileName);
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetAffactedRenderTargets(Guid id)
    {
        AnalyzeRenderFile();

        WorkSpaceRefItem refItem = _references.Find(o => o.Id == id);
        if (refItem is null)
        {
            return [];
        }

        switch (refItem)
        {
            case RenderableRefItem renderableRefItem:
                IRenderable obj = renderableRefItem.Renderable;

                if (obj is IDataInputOwner owner)
                {
                    // Data set
                    return _renderPage.RenderTargetsByFileId.Values.Where(r =>
                    {
                        var renderable = r.Item?.Renderable;
                        return renderable != null && owner.ContainsDataInput(renderable.Id);
                    });
                }
                else
                {
                    return _renderPage.RenderTargetsByFileId.Values.Where(o => o.Item?.Renderable == obj);
                }
            case RenderTargetLibraryRefItem libItem:
                return _renderPage.RenderTargetsByFileId.Values.Where(o => o.Tag == refItem);

            case FileBunchRefItem fileBunchRefItem:
                IFileBunch bunch = fileBunchRefItem.FileBunch;
                return _renderPage.RenderTargetsByFileId.Values.Where(o => o.FileBunch == bunch);

            default:
                return _renderPage.RenderTargetsByFileId.Values.Where(o => o.Tag == refItem);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetAffactedRenderTargets(string relativePath)
    {
        AnalyzeRenderFile();

        return _renderPage.RenderTargetsByFileId[relativePath.GetPathLowId()];
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetDependency(string relativePath)
    {
        AnalyzeRenderFile();

        foreach (var target in _renderPage.RenderTargetsByFileId[relativePath.GetPathLowId()])
        {
            if (target.Item?.Renderable != null)
            {
                yield return target.Item.Renderable;
            }

            if (target.Material != null)
            {
                yield return target.Material;
            }

            if (target.FileBunch != null)
            {
                yield return target.FileBunch;
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> ReferenceIds => _references.Select(o => o.Id);

    /// <inheritdoc/>
    public override IEnumerable<T> GetReferences<T>()
    {
        return _references.Select(o => AssetManager.Instance.GetAsset(o.Id)).OfType<T>();
    }

    /// <inheritdoc/>
    public override bool ContainsFileBunches()
    {
        return _references.Any(o => o is FileBunchRefItem);
    }

    /// <summary>
    /// Gets the collection of reference items in this workspace.
    /// </summary>
    internal IEnumerable<WorkSpaceRefItem> ReferenceItems => _references.Pass();

    /// <summary>
    /// Handles modification events from reference items.
    /// </summary>
    /// <param name="item">The modified reference item.</param>
    private void OnRefItemModified(WorkSpaceRefItem item)
    {
        _dirtyRefs.Add(item.Id);
        RequestAnalyze();
    }

    /// <inheritdoc/>
    public override bool AddAssemblyReference(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        if (!AssemblyReferenceEnabled)
        {
            return false;
        }

        Asset content = AssetManager.Instance.GetAsset(id);
        if (content is IAssemblyReference asmRef)
        {
            if (Controller is null || !Controller.CanAddAssemlbyReference(asmRef))
            {
                return false;
            }

            if (!_assemblyRefs.OfType<AssetAssemblyReferenceItem>().Any(o => o.Id == content.Id))
            {
                _assemblyRefs.InsertSorted(
                    new AssetAssemblyReferenceItem(id),
                    (v1, v2) => v1.CompareTo(v2)
                    );
                _refSet.MarkDirty();
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool AddSystemAssemblyReference(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (!AssemblyReferenceEnabled)
        {
            return false;
        }

        if (!_assemblyRefs.OfType<SystemAssemblyReferenceItem>().Any(o => o.Key == name))
        {
            _assemblyRefs.InsertSorted(
                new SystemAssemblyReferenceItem(name),
                (v1, v2) => v1.CompareTo(v2)
                );
            _refSet.MarkDirty();
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool AddDisabledAssemblyReference(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!AssemblyReferenceEnabled)
        {
            return false;
        }

        if (!_assemblyRefs.OfType<DisabledAssemblyReferenceItem>().Any(o => o.Key == path))
        {
            _assemblyRefs.InsertSorted(
                new DisabledAssemblyReferenceItem(path),
                (v1, v2) => v1.CompareTo(v2)
                );
            _refSet.MarkDirty();
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool RemoveAssemblyReference(string key)
    {
        if (!AssemblyReferenceEnabled)
        {
            return false;
        }

        int count = _assemblyRefs.RemoveAll(o => o.Key == key);
        if (count > 0)
        {
            _refSet.MarkDirty();
        }

        return count > 0;
    }

    /// <inheritdoc/>
    public override IEnumerable<IAssemblyReferenceItem> AssemblyReferenceItems
    {
        get
        {
            if (AssemblyReferenceEnabled)
            {
                return _assemblyRefs.Select(o => o);
            }
            else
            {
                return [];
            }
        }
    }

    #endregion

    #region Render Status

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> RenderTargets => _renderPage.RenderTargetsByFileId.Values;

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargetsByDirectory(string relativePath)
    {
        return _renderPage.RenderTargetsByDir[relativePath.GetPathLowId()];
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetRenderDirectoryByDirectory(string relativePath)
    {
        //foreach (var subDirId in _renderDirsByDir[relativePath.GetPathLowId()])
        //{
        //    RenderDirectoryInfo info = _renderDirs.GetValueOrDefault(subDirId);

        //}

        return _renderPage.RenderDirsByDir[relativePath.GetPathLowId()].
            Select(id => _renderPage.RenderDirectories.GetValueSafe(id.GetPathLowId())).
            Where(o => o?.Rendering == true).
            Select(o => o.Path);
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets(string relativePath)
    {
        return _renderPage.RenderTargetsByFileId[relativePath.GetPathLowId()];
    }

    /// <inheritdoc/>
    public override RenderTarget GetRenderTargetByFullPath(string fullPath)
    {
        return _renderPage.RenderTargets.GetValueSafe(fullPath);
    }

    /// <inheritdoc/>
    public override bool ContainsRenderDirectory(string relativePath)
    {
        var info = _renderPage.RenderDirectories.GetValueSafe(relativePath.GetPathLowId());
        return info?.Rendering == true;
    }

    /// <inheritdoc/>
    public override FileState GetDirectoryStatus(string relativePath)
    {
        var info = _renderPage.RenderDirectories.GetValueSafe(relativePath.GetPathLowId());
        if (info is null)
        {
            return FileState.None;
        }

        if (info.ContainsErrorFiles)
        {
            return FileState.Warning;
        }
        else if (info.ContainsRemovingFiles)
        {
            return FileState.Remove;
        }
        else if (info.ContainsAddingFiles)
        {
            return FileState.Add;
        }
        else if (info.ContainsUpdatingFiles)
        {
            return FileState.Update;
        }
        else if (info.ContainsModifiedFiles)
        {
            return FileState.Modified;
        }
        else
        {
            return FileState.None;
        }
    }

    /// <inheritdoc/>
    public override RenderStatus GetFileRenderStatus(string relativePath) => _renderRecord.GetRenderStatusByRelativePath(relativePath);

    /// <inheritdoc/>
    public override FileState GetFileStatus(string relativePath)
    {
        if (!_analyzed)
        {
            AnalyzeRenderFile();
        }

        return _renderPage.GetFileStatus(relativePath);
    }

    /// <inheritdoc/>
    public override void BindRenderFile(string relativePath)
    {
        if (_renderRecord.BindRenderedFile(relativePath))
        {
            RequestAnalyze();
        }
    }

    /// <inheritdoc/>
    public override void UnbindRenderFile(string relativePath)
    {
        if (_renderRecord.UnbindRenderedFile(relativePath))
        {
            RequestAnalyze();
        }
    }

    /// <inheritdoc/>
    public override bool UnbindAllRemovingFiles()
    {
        var unbinds = _renderRecord.RenderedFiles.Where(o => GetFileStatus(o.RelativeFileName) == FileState.Remove).ToArray();

        if (unbinds.Length > 0)
        {
            foreach (var unbind in unbinds)
            {
                UnbindRenderFile(unbind.RelativeFileName);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool BindAllUserFiles()
    {
        var binds = _renderPage.RenderTargetsByFileId.Keys.Where(o => GetFileStatus(o) == FileState.UserOccupied).ToArray();

        if (binds.Length > 0)
        {
            foreach (var bind in binds)
            {
                BindRenderFile(bind);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool ContainsAddingFiles()
        => _renderRecord.ContainsAddingFiles(_renderPage);

    /// <inheritdoc/>
    public override bool ContainsRemovingFiles()
        => _renderRecord.ContainsRemovingFiles(_renderPage);

    /// <inheritdoc/>
    public override bool ContainsUserOccupiedFiles()
    {
        //TODO: Keys will cause multi-threading issues
        return _renderPage.RenderTargetsByFileId.Keys.Any(key => GetFileStatus(key) == FileState.UserOccupied);
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetModifyingFiles(RenderModifyType type) => _renderRecord.GetModifyingFiles(_renderPage, type);

    /// <inheritdoc/>
    public override void ClearDirty()
    {
        _analyzed = true;

        DateTime now = DateTime.Now + TimeSpan.FromSeconds(3);
        _renderRecord.UpdateRenderTime(now);

        var page = RefreshPage();

        //foreach (var target in _renderPage.RenderTargetsByFileId.Values)
        //{
        //    var record = _renders.GetOrAddRenderedFile(target, now);
        //    record.LastUpdateTime = now;

        //    foreach (var dirInfo in _renderPage.EnsureDirectoryInfosBubble(record.RelativeFileName))
        //    {
        //        dirInfo.Clear();
        //    }
        //}

        //_renders.ClearDirty();

        _dirtyRefs.Clear();
        //_dirtyRenderTargetCount = 0;
        _dirtyRenderTargetCount = _renderRecord.GetDirtyRenderTargets(page, true).Count();

        RaiseUpdate();
    }

    /// <summary>
    /// Marks all render targets and reference items as dirty, forcing a full re-analysis.
    /// </summary>
    public void MarkAllDirty()
    {
        _analyzed = false;

        DateTime now = DateTime.Now;

        if (_renderPage != null)
        {
            foreach (var target in _renderPage.RenderTargetsByFileId.Values)
            {
                target.UpdateTime(now);
            }
        }

        foreach (var item in _references)
        {
            item.LastUpdateTime = now;
        }

        RequestAnalyze();
    }

    #endregion

    #region Config

    /// <summary>
    /// Loads the workspace configuration from disk, creating a new one if it doesn't exist.
    /// </summary>
    /// <param name="initializing">Whether this is an initial load during project startup.</param>
    public void LoadConfig(bool initializing)
    {
        if (!File.Exists(ConfigFileName))
        {
            NewConfig();
            return;
        }
        LoadConfig(ConfigFileName, initializing);
    }

    /// <inheritdoc/>
    public override void UpdateConfig()
    {
        if (File.Exists(ConfigFileName))
        {
            LoadConfig(ConfigFileName, false);
        }
    }

    private void LoadConfig(string fileName, bool initializing)
    {
        EnsureWorkSpaceDirectory();

        try
        {
            _silent = initializing;
            XmlSerializer.DeserializeFromFile(this, fileName);
            _renderRecord.SetRenderedFiles(_renderedFilesConfig, DateTime.Now + TimeSpan.FromSeconds(3));

            // Since it's a loading process, no need to set dirty
            if (Controller != null)
            {
                Controller.IsProjectDirty = false;
            }

            EnsureDirectory();

            RequestAnalyze();
        }
        catch (Exception err)
        {
            err.LogError($"Failed to read workspace configuration file: {Name}");
        }
        finally
        {
            _silent = false;
        }

        foreach (var r in _references)
        {
            r.Modified += OnRefItemModified;
        }

        _configDirty = false;
    }

    /// <summary>
    /// Saves the workspace configuration to disk if dirty or forced.
    /// </summary>
    /// <param name="force">Whether to force saving regardless of dirty state.</param>
    public void SaveConfig(bool force = false)
    {
        if (_configDirty || force)
        {
            SaveConfig(ConfigFileName, false);
        }
    }

    /// <inheritdoc/>
    public override void ExportConfig(string fileName)
    {
        SaveConfig(fileName, true);
    }

    private void SaveConfig(string fileName, bool export)
    {
        if (Controller is null)
        {
            return;
        }

        EnsureWorkSpaceDirectory();

        SyncIntent intent = export ? SyncIntent.DataExport : SyncIntent.Serialize;

        try
        {
            _renderedFilesConfig.Clear();
            _renderedFilesConfig.AddRange(_renderRecord.RenderedFiles.Select(o => new RenderFileRecordBK(o.RelativeFileName, o.LastUpdateTime)));

            foreach (var item in _renderedFilesConfig)
            {
                if (_renderRecord.GetIsDirtyByRelativePath(_renderPage, item.RelativeFileName))
                {
                    item._dirty = true;
                }
            }

            FileUnwatchedAction.Do(() => XmlSerializer.SerializeToFile(this, fileName, intent));
        }
        catch (Exception err)
        {
            err.LogError();
        }

        _configDirty = false;
    }

    /// <summary>
    /// Creates a new default configuration for this workspace.
    /// </summary>
    internal void NewConfig()
    {
        _guid = Guid.NewGuid();
        _baseNameSpace = Name;

        SaveConfig(true);
    }

    /// <summary>
    /// Checks whether the configuration file exists on disk.
    /// </summary>
    /// <returns>True if the config file exists.</returns>
    public bool ConfigFileExists()
    {
        string workSpacePath = _manager.BasePath.PathAppend(Name);
        string configFile = workSpacePath.PathAppend(WorkSpaceBK.DefaultWorkSpaceConfigFileName);
        return File.Exists(configFile);
    }

    private void EnsureDirectory()
    {
        DirectoryUtility.EnsureDirectory(BaseDirectory);
        DirectoryUtility.EnsureDirectory(MasterDirectory);
        DirectoryUtility.EnsureDirectory(TempDirectory);
    }

    /// <inheritdoc/>
    public override RenderConfig CreateRenderConfig()
    {
        return new RenderConfig
        {
            WorkSpace = AssetKey,
            BasePath = MasterDirectory,
            UserCode = _asset,
            Naming = SystemNamingOption.Instance,
            Disabled = DisableId,
            Condition = new ConditionStore(_conditions),
        };
    }

    #endregion

    #region ISyncObject, IViewObject

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.IsGetterOf("ControllerName"))
        {
            sync.Sync("ControllerName", ControllerInfo?.Name);
        }
        else if (sync.IsSetterOf("ControllerName"))
        {
            string ctrlName = sync.Sync("ControllerName", string.Empty);
            if (ControllerInfo is null || ctrlName != ControllerInfo.Name)
            {
                StopController();
                if (!string.IsNullOrEmpty(ctrlName))
                {
                    StartController(ctrlName);
                }
            }
        }

        BaseNameSpace = sync.Sync("BaseNameSpace", BaseNameSpace);
        _externalRPath = sync.Sync("ExternalPath", _externalRPath);
        DisableId = sync.Sync("DisableId", DisableId);

        sync.Sync("Conditions", _conditions, SyncFlag.GetOnly);
        sync.Sync("References", _references, SyncFlag.GetOnly);
        sync.Sync("AssemblyReferences", _assemblyRefs, SyncFlag.GetOnly);
        sync.Sync("RenderedFiles", _renderedFilesConfig, SyncFlag.GetOnly);

        if (sync.Intent != SyncIntent.DataExport)
        {
            if (sync.IsGetterOf("Guid"))
            {
                sync.Sync("Guid", _guid.ToString());
            }
            else if (sync.IsSetterOf("Guid"))
            {
                string guidStr = sync.Sync("Guid", string.Empty);
                if (Guid.TryParseExact(guidStr, "D", out Guid guid))
                {
                    _guid = guid;
                }
            }
        }

        if (sync.Mode == SyncMode.SetAll)
        {
            // Read all settings
            _references.Sort((a, b) => a.CompareTo(b));
        }

        if (Controller != null)
        {
            sync.Sync("ControllerConfig", Controller, SyncFlag.GetOnly);
        }

        if (sync.IsSetter())
        {
            MarkConfigDirty();
        }
    }

    /// <inheritdoc/>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        if (Controller is not IViewObject ctrl)
        {
            return;
        }

        setup.InspectorField(_baseNameSpace, new ViewProperty("BaseNameSpace", "Default Namespace"));
        setup.InspectorField(_conditions, new ViewProperty("Conditions", "Condition"));

        // Disable Id - temporarily not used
        // setup.InspectorField(DisableId, new ViewProperty("DisableId", "DisableId"));

        setup.InspectorField(ctrl, new ViewProperty("ControllerConfig", ControllerInfo.DisplayName) { Expand = true });
    }

    #endregion

    #region Controller

    /// <summary>
    /// Notifies the workspace that it has been renamed.
    /// </summary>
    /// <param name="oldName">The previous name of the workspace.</param>
    internal void InternalNotifyRenamed(string oldName)
    {
        NotifyRenamed(oldName);
    }

    /// <summary>
    /// Starts the workspace controller internally.
    /// </summary>
    internal void InternalStartController()
    {
        StartController();
    }

    /// <inheritdoc/>
    protected internal override void OnControllerConfigUpdated()
    {
        MarkConfigDirty();
    }

    #endregion

    #region IRenderHost

    /// <inheritdoc/>
    public override bool ExecuteRender(bool incremental)
    {
        //EditorRexes.EnsureInMainThread.Invoke();

        // Must have generated at least once before incremental generation can be used
        // Cancel firstRender mechanism
        //if (!_firstRender)
        //{
        //    incremental = false;
        //}

        //Execute all delayed actions immediately
        for (int i = 0; i < 5; i++)
        {
            EditorUtility.FlushDelayedActions();
        }

        string basePath = MasterDirectory;
        //PreRender
        AnalyzeRenderFile();

        var page = _renderPage;

        PreValidateRenderFile(page.AllTargets);

        bool preRenderError = page.CheckPreRenderError();
        if (preRenderError)
        {
            return false;
        }

        //Get render service
        ICodeRenderService renderService = Device.Current.GetService<ICodeRenderService>();

        //bool structureChanged = ContainsAddingFiles() || ContainsRemovingFiles();

        //CleanUp
        page.PreRenderCleanUp();

        var targets = _renderRecord.GetDirtyRenderTargets(page, incremental).Where(o => !o.Suspended).ToArray();
        List<TargetFileRenderResult> results = [];
        bool success = true;

        // Restore automatically sets target.UserCodeEnabled
        var collection = new AutoRestoreCollection(this);
        collection.AddRange(targets);

        // TODO: Does WorkSpace generation need UnwatchedAction?
        // Render
        // Divided by user code database, settings without auto-restore default to workspace database
        if (collection.Count > 0)
        {
            Logs.LogInfo($"Rendering {this}...");

            RenderConfig config = CreateRenderConfig();
            foreach (var item in collection.Items)
            {
                config.UserCode = item.UserCode;
                bool tempSuccess = renderService.RenderTargets(config, item.Targets, out var tempResults);
                if (tempSuccess)
                {
                    results.AddRange(tempResults);
                }
                else
                {
                    success = false;
                }
            }
        }

        // Cache
        var added = new List<string>(_renderRecord.GetModifyingFiles(page, RenderModifyType.Add));
        var removed = new List<string>(_renderRecord.GetModifyingFiles(page, RenderModifyType.Remove));

        // Optimization acceleration
        if (collection.Count == 0 && added.Count == 0 && removed.Count == 0 && incremental)
        {
            // Only update project files
            RenderController(added, removed, incremental);
            return true;
        }

        if (success)
        {
            _renderRecord.ClearRender(removed, incremental);
        }

        // Loading documents causes delayed update events on resources, leading to false positives
        // Add 3 seconds here to ensure render results are up to date
        DateTime now = DateTime.Now + TimeSpan.FromSeconds(3);
        foreach (var result in results)
        {
            _renderRecord.AddFileRenderResult(result, now);
        }

        RenderController(added, removed, incremental);

        SaveConfig(true);

        // Clear
        _dirtyRefs.Clear();

        RequestAnalyze();

        return true;
    }

    /// <inheritdoc/>
    public override bool ExecuteRender(IEnumerable<RenderTarget> targets)
    {
        //EditorRexes.EnsureInMainThread.Invoke();

        PreValidateRenderFile(targets);

        ICodeRenderService renderService = Device.Current.GetService<ICodeRenderService>();

        RenderConfig config = CreateRenderConfig();

        IEnumerable<TargetFileRenderResult> results = [];
        bool success = renderService.RenderTargets(config, targets, out results);

        return true;
    }

    /// <inheritdoc/>
    public override bool RestoreRender(ICodeLibrary userCode = null)
    {
        //EditorRexes.EnsureInMainThread.Invoke();

        string basePath = MasterDirectory;
        //PreRender
        AnalyzeRenderFile();

        bool preRenderError = _renderPage.CheckPreRenderError();
        if (preRenderError)
        {
            return false;
        }

        //Get render service
        ICodeRenderService renderService = Device.Current.GetService<ICodeRenderService>();

        //CleanUp
        _renderPage.PreRenderCleanUp();

        //Render
        RenderConfig config = CreateRenderConfig();
        if (userCode != null)
        {
            config.UserCode = userCode;
        }

        var targets = _renderPage.RenderTargetsByFileId.Values;
        bool success = renderService.RestoreTargets(config, targets, out var results);

        RequestAnalyze();

        return true;
    }

    /// <inheritdoc/>
    public override bool RestoreRender(IWorkSpaceRefItem refItem, ICodeLibrary userCode = null)
    {
        //EditorRexes.EnsureInMainThread.Invoke();

        //PreRender
        AnalyzeRenderFile();

        bool preRenderError = _renderPage.CheckPreRenderError();
        if (preRenderError)
        {
            return false;
        }

        //Get render service
        var renderService = Device.Current.GetService<ICodeRenderService>();

        //CleanUp
        _renderPage.PreRenderCleanUp();

        //Render
        RenderConfig config = CreateRenderConfig();
        if (userCode != null)
        {
            config.UserCode = userCode;
        }

        //string trimNameSpace = (_baseNameSpace ?? string.Empty).Trim('.', '*');

        var targets = refItem.GetRenderTargets().ToArray();
        bool success = renderService.RestoreTargets(config, targets, out var results);

        RequestAnalyze();

        return success;
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets() => _renderPage.RenderTargetsByFileId.Values;

    /// <inheritdoc/>
    public override int DirtyRenderTargetCount => _dirtyRenderTargetCount;

    #endregion

    #region IInspectorNotify

    /// <inheritdoc/>
    void IViewListener.NotifyViewEnter(int viewId)
    {
    }

    /// <inheritdoc/>
    void IViewListener.NotifyViewExit(int viewId)
    {
    }

    /// <inheritdoc/>
    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName) => _refSet.MarkDirty();

    #endregion

    #region IEntryListener

    /// <inheritdoc/>
    public void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    {
        if (args is RenameAssetEventArgs)
        {
            OnDependencyRenamed();
            RequestAnalyze();
        }
        else
        {
            OnDepedencyUpdated();
            RequestAnalyze();
        }

        if (obj is Asset asset)
        {
            HandleAssetNotifyEvent(asset, args);
        }
    }

    /// <summary>
    /// Handles asset notification events recursively for grouped assets.
    /// </summary>
    /// <param name="asset">The asset being notified.</param>
    /// <param name="args">The event arguments.</param>
    private void HandleAssetNotifyEvent(Asset asset, EntryEventArgs args)
    {
        switch (args)
        {
            case GroupAssetEventArgs groupArgs:
                if (groupArgs.ChildAsset != null && groupArgs.Inner != null)
                {
                    HandleAssetNotifyEvent(groupArgs.ChildAsset, groupArgs.Inner);
                }
                break;

            case RenameAssetEventArgs renameArgs:
                NotifyAssetRenamed(asset, renameArgs);
                break;

            default:
                NotifyAssetUpdated(asset, args);
                break;
        }
    }

    /// <summary>
    /// Marks an asset as dirty when it is updated.
    /// </summary>
    /// <param name="asset">The updated asset.</param>
    /// <param name="args">The event arguments.</param>
    private void NotifyAssetUpdated(Asset asset, EntryEventArgs args) => _dirtyRefs.Add(asset.Id);

    /// <summary>
    /// Handles asset rename events.
    /// </summary>
    /// <param name="asset">The renamed asset.</param>
    /// <param name="args">The rename event arguments.</param>
    private void NotifyAssetRenamed(Asset asset, RenameAssetEventArgs args)
    {
        //string oldKey = args.OldName;
        //string newKey = args.NewName;

        //IRenderService render = Device.Current.GetService<IRenderService>();
        //if (asset is IRenderable)
        //{
        //    render.RenameKeyString(DbFileName, new[] { new UserCodeRename { OldKeyString = oldKey, NewKeyString = newKey } });
        //}
        //if (asset is IMaterial)
        //{
        //    render.RenameMaterial(DbFileName, new[] { new UserCodeRename { OldKeyString = oldKey, NewKeyString = newKey } });
        //}

        //_dirtyRefs.Remove(oldKey);
        //if (asset != null)
        //{
        //    _dirtyRefs.Add(asset.Id);
        //}
    }

    #endregion

    #region IViewRedirect

    object IViewRedirect.GetRedirectedObject(int viewId)
    {
        if (ServiceInternals._license.LicenseType != LicenseTypes.Professional)
        {
            return null;
        }

        if (viewId == ViewIds.DetailTreeView)
        {
            return _renderRecord;
        }

        return this;
    }

    #endregion

    #region Misc

    public override bool SetExternalMasterPath(string masterFullPath)
    {
        string rPath = masterFullPath.MakeRalativePath(BaseDirectory);

        if (rPath == _externalRPath)
        {
            return false;
        }

        if (rPath.StartsWith("../") || WorkSpaceManager.AbsoluteExternalMasterPath)
        {
            _externalRPath = rPath;
            NotifyMasterPathChanged();

            AnalyzeRenderFile(true);

            OnMasterBasePathUpdated();
            return true;
        }

        return false;
    }

    public override bool UnsetExternalMasterPath()
    {
        if (!string.IsNullOrEmpty(_externalRPath))
        {
            _externalRPath = null;
            NotifyMasterPathChanged();

            AnalyzeRenderFile(true);

            OnMasterBasePathUpdated();

            return true;
        }

        return false;
    }

    public override void ShowUserCodeEditor(RenderTarget target)
    {
        if (target?.FileName is null)
        {
            return;
        }

        ICodeRenderService render = Device.Current.GetService<ICodeRenderService>();

        RenderConfig config = CreateRenderConfig();

        var segcfg = target.GetLanguage()?.SegmentConfig ?? CodeSegmentConfig.CsDefault;
        render.ShowUserCodeEditor(config, target.FileName, segcfg);
    }

    public override void MarkFileAsModified(string relativePath)
    {
        string fullName = relativePath.MakeFullPath(MasterDirectory);
        if (File.Exists(fullName))
        {
            _renderRecord.AddModifiedFileByRelativePath(relativePath);
            foreach (var info in _renderPage.EnsureDirectoryInfosBubble(relativePath))
            {
                info.ContainsModifiedFiles = true;
            }
        }
        if (Directory.Exists(fullName))
        {
            _renderPage.EnsureDirectoryInfo(relativePath).ContainsModifiedFiles = true;
            foreach (var info in _renderPage.EnsureDirectoryInfosBubble(relativePath))
            {
                info.ContainsModifiedFiles = true;
            }
        }

        RequestAnalyze();
        //_analyzed = false;
        //EditorUtility.AddDelayedAction(new RaiseUpdateAction(this));
    }

    #endregion

    public override WorkSpaceAsset GetAsset() => _asset;

    private void MarkConfigDirty()
    {
        _configDirty = true;
        EditorRexes.ProjectDirty.Value = true;

        OnRenderTargetUpdated();
    }

    #region Analyze

    private void RequestAnalyze()
    {
        _analyzed = false;
        EditorUtility.AddDelayedAction(_analyzeAction);
    }

    /// <summary>
    /// Analyzes render files
    /// </summary>
    private void AnalyzeRenderFile(bool resetCache = false)
    {
        if (_released)
        {
            return;
        }

        //EditorRexes.EnsureInMainThread.Invoke();

        if (resetCache)
        {
            _renderRecord.ClearAll();
            _analyzed = false;
        }

        if (_analyzed)
        {
            return;
        }

        if (LogDebug)
        {
            Logs.LogDebug("WorkSpace starts analyzing files...");
        }

        _analyzed = true;

        var page = RefreshPage();

        _dirtyRenderTargetCount = _renderRecord.GetDirtyRenderTargets(page, true).Count();

        RaiseUpdate();
    }

    private RenderTargetPage RefreshPage()
    {
        RenderTargetPage page = new(this);
        page.Collect();

        lock (_pageSync)
        {
            _renderPage = page;
        }

        return page;
    }

    private bool PreValidateRenderFile(IEnumerable<RenderTarget> targets)
    {
        var refMngr = Device.Current.GetService<ReferenceManager>();
        if (refMngr is null)
        {
            Logs.LogWarning("Can not validate render files, ReferenceManager is not found.");
            return true;
        }

        HashSet<Asset> validated = [];
        HashSet<Guid> missings = [];

        foreach (var target in targets)
        {
            if (target.Item?.Renderable is Asset asset && !validated.Contains(asset) && asset.GetStorageLocation() is StorageLocation location)
            {
                var refHost = EditorUtility.GetReferenceHost(location);
                if (refHost != null)
                {
                    missings.Clear();

                    foreach (var id in refMngr.GetDependencies(refHost))
                    {
                        if (id == Guid.Empty || ValidateIgnoreIds.Contains(id))
                        {
                            continue;
                        }

                        if (EditorObjectManager.Instance.GetObject(id) != null)
                        {
                            continue;
                        }

                        string name = GlobalIdResolver.RevertResolve(id);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            string assetName = name.FindLastAndGetBefore('.');
                            Guid assetId = GlobalIdResolver.Resolve(assetName);
                            if (assetId != Guid.Empty)
                            {
                                // Is a system type field
                                ValidateIgnoreIds.Add(assetId);
                            }
                        }

                        missings.Add(id);
                    }
                }

                if (missings.Count > 0)
                {
                    foreach (var id in missings)
                    {
                        string name = GlobalIdResolver.RevertResolve(id);
                        string desc;

                        if (!string.IsNullOrEmpty(name))
                        {
                            // Internal types (including SObject.Controller) fields default to no Id, skip
                            continue;

                            //if (name.StartsWith("*Render|"))
                            //{
                            //    ValidateIgnoreIds.Add(id);
                            //    continue;
                            //}

                            //desc = $"{name}({id})";
                        }
                        else
                        {
                            desc = id.ToString();
                        }

                        Logs.LogWarning($"Missing reference to {asset.AssetKey} in workspace: {desc}");
                    }

                    missings.Clear();
                }

                validated.Add(asset);
            }
        }

        return true;
    }

    #endregion

    private void RaiseUpdate()
    {
        OnRenderTargetUpdated();
        _manager.RaiseRenderTargetUpdated(this);
    }

    private string GetFinalPath(string basePath, string nameSpace)
    {
        string finalPath = basePath;
        if (!string.IsNullOrEmpty(nameSpace))
        {
            if (nameSpace.StartsWith("*"))
            {
                nameSpace = nameSpace.TrimStart('*');
            }
            finalPath = Path.Combine(finalPath, nameSpace.Replace('.', '/'));
        }

        return finalPath;
    }

    private void EnsureWorkSpaceDirectory()
    {
        string workSpacePath = _manager.BasePath.PathAppend(Name);
        if (!Directory.Exists(workSpacePath))
        {
            try
            {
                Directory.CreateDirectory(workSpacePath);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    public override string ToString() => Name ?? base.ToString();

    private class AnalyzeAction : DelayedAction<WorkSpaceBK>
    {
        public AnalyzeAction(WorkSpaceBK value) : base(value, 1)
        {
            value._analyzed = false;
        }

        public override void DoAction()
        {
            Value.AnalyzeRenderFile();
        }
    }

    private class RaiseUpdateAction : DelayedAction<WorkSpaceBK>
    {
        public RaiseUpdateAction(WorkSpaceBK value) : base(value)
        {
        }

        public override void DoAction()
        {
            Value.AnalyzeRenderFile();
        }
    }

    private class ConditionStore : ICondition
    {
        private readonly string[] _contidions;

        public ConditionStore(IEnumerable<string> conditions = null)
        {
            _contidions = conditions?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? [];
        }

        public IEnumerable<string> Conditions => _contidions.Pass();

        public bool HasCondition(string condition)
        {
            return _contidions.Any(s => s == condition);
        }
    }
}