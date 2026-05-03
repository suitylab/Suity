using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Workspace
/// </summary>
public abstract class WorkSpace : IRenderHost, IHasId
{
    /// <summary>
    /// Workspace asset key prefix
    /// </summary>
    public const string WorkspaceAssetKeyPrefix = "~WorkSpace";

    /// <summary>
    /// Default workspace configuration file name
    /// </summary>
    public const string DefaultWorkSpaceConfigFileName = "WorkSpace.config";
    /// <summary>
    /// Default workspace database file name
    /// </summary>
    public const string DefaultWorkSpaceDbFileName = "WorkSpace.db";
    /// <summary>
    /// Default master directory name
    /// </summary>
    public const string DefaultMasterDirectory = "Master";
    /// <summary>
    /// Temporary directory name
    /// </summary>
    public const string Temp = "Temp";

    protected bool _silent;
    private WorkSpaceControllerInfo _controllerInfo;
    private WorkSpaceController _controller;

    #region Property

    /// <summary>
    /// Manager
    /// </summary>
    public abstract WorkSpaceManager Manager { get; }

    /// <summary>
    /// Gets the workspace name
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// Gets the workspace ID
    /// </summary>
    public abstract Guid Id { get; }
    /// <summary>
    /// Gets the asset key
    /// </summary>
    public abstract string AssetKey { get; }
    /// <summary>
    /// Gets the base directory path
    /// </summary>
    public abstract string BaseDirectory { get; }
    /// <summary>
    /// Gets or sets the base namespace
    /// </summary>
    public abstract string BaseNameSpace { get; set; }
    /// <summary>
    /// Gets or sets whether debug mode is enabled
    /// </summary>
    public abstract bool Debug { get; set; }

    /// <summary>
    /// Gets the master directory path
    /// </summary>
    public abstract string MasterDirectory { get; }
    /// <summary>
    /// Gets whether the master directory is external
    /// </summary>
    public abstract bool IsExternalMasterDirectory { get; }
    /// <summary>
    /// Gets the temporary directory path
    /// </summary>
    public abstract string TempDirectory { get; }
    /// <summary>
    /// Gets the configuration file name
    /// </summary>
    public abstract string ConfigFileName { get; }
    /// <summary>
    /// Gets the database file name
    /// </summary>
    public abstract string DbFileName { get; }
    /// <summary>
    /// Gets the user code library
    /// </summary>
    public abstract ICodeLibrary UserCode { get; }
    /// <summary>
    /// Gets the workspace GUID
    /// </summary>
    public abstract Guid WorkSpaceGuid { get; }
    /// <summary>
    /// Gets the render record collection
    /// </summary>
    public abstract RenderRecordCollection Records { get; }
    /// <summary>
    /// Gets the controller info
    /// </summary>
    public WorkSpaceControllerInfo ControllerInfo => _controllerInfo;
    /// <summary>
    /// Gets the controller
    /// </summary>
    public WorkSpaceController Controller => _controller;
    /// <summary>
    /// Gets the workspace icon
    /// </summary>
    public abstract ImageDef Icon { get; }

    /// <summary>
    /// Gets whether the workspace is in failure state
    /// </summary>
    public bool IsFailure { get; protected set; }

    /// <summary>
    /// Gets the count of dirty render targets
    /// </summary>
    public abstract int DirtyRenderTargetCount { get; }

    /// <summary>
    /// Gets whether the workspace requires rendering
    /// </summary>
    public bool RequireRender => DirtyRenderTargetCount > 0 || Controller?.IsProjectDirty == true;

    /// <summary>
    /// Gets the order for sorting
    /// </summary>
    public int Order => _controller?.Order ?? 0;

/// <summary>
    /// Disables Id, not currently used
    /// </summary>
    public bool DisableId { get; set; }

    /// <summary>
    /// Gets the base render file name path
    /// </summary>
    /// <param name="customNameSpace">Optional custom namespace</param>
    /// <returns>Render file name</returns>
    public RenderFileName GetBasePath(string customNameSpace = null)
    {
        if (!string.IsNullOrEmpty(customNameSpace))
        {
            return new RenderFileName(MasterDirectory, BaseNameSpace, customNameSpace);
        }
        else
        {
            return new RenderFileName(MasterDirectory, BaseNameSpace);
        }
    }

    #endregion

    #region Reference

    /// <summary>
    /// Adds reference
    /// </summary>
    /// <param name="id"></param>
    public virtual IWorkSpaceRefItem AddReferenceItem(Guid id) => null;

    /// <summary>
    /// Removes reference
    /// </summary>
    /// <param name="id"></param>
    public virtual void RemoveReferenceItem(Guid id)
    { }

    /// <summary>
    /// Gets reference settings
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual IWorkSpaceRefItem GetReferenceItem(Guid id) => null;

    /// <summary>
    /// Sets file bunch
    /// </summary>
    /// <param name="bunch"></param>
    public virtual void SetupFileBunch(IFileBunch bunch)
    { }

    /// <summary>
    /// Gets affected render targets
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual IEnumerable<RenderTarget> GetAffactedRenderTargets(Guid id) => [];

    /// <summary>
    /// Gets affected render targets
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public virtual IEnumerable<RenderTarget> GetAffactedRenderTargets(string relativePath) => [];

    /// <summary>
    /// Gets affected file names
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual IEnumerable<RenderFileName> GetAffectedFileNames(Guid id) => [];

    /// <summary>
    /// Gets dependency objects
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public virtual IEnumerable<object> GetDependency(string relativePath) => [];

    /// <summary>
    /// Gets all reference Ids
    /// </summary>
    public virtual IEnumerable<Guid> ReferenceIds => [];

    /// <summary>
    /// Gets references of specified type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual IEnumerable<T> GetReferences<T>() where T : class => [];

    /// <summary>
    /// Gets whether this workspace contains file bunches
    /// </summary>
    /// <returns></returns>
    public virtual bool ContainsFileBunches() => false;

    /// <summary>
    /// Gets whether assembly reference is enabled
    /// </summary>
    public bool AssemblyReferenceEnabled => Controller?.AssemblyReferenceEnabled == true;

    /// <summary>
    /// Adds an assembly reference by ID
    /// </summary>
    /// <param name="id">Assembly ID</param>
    /// <returns>True if successful</returns>
    public virtual bool AddAssemblyReference(Guid id) => false;

    /// <summary>
    /// Adds a system assembly reference by name
    /// </summary>
    /// <param name="name">Assembly name</param>
    /// <returns>True if successful</returns>
    public virtual bool AddSystemAssemblyReference(string name) => false;

    /// <summary>
    /// Adds a disabled assembly reference by path
    /// </summary>
    /// <param name="path">Assembly path</param>
    /// <returns>True if successful</returns>
    public virtual bool AddDisabledAssemblyReference(string path) => false;

    /// <summary>
    /// Removes an assembly reference by key
    /// </summary>
    /// <param name="key">Reference key</param>
    /// <returns>True if successful</returns>
    public virtual bool RemoveAssemblyReference(string key) => false;

    /// <summary>
    /// Gets all assembly reference items
    /// </summary>
    public virtual IEnumerable<IAssemblyReferenceItem> AssemblyReferenceItems => [];

    #endregion

    #region Render Status

    /// <summary>
    /// Gets all render targets
    /// </summary>
    public abstract IEnumerable<RenderTarget> RenderTargets { get; }

    /// <summary>
    /// Gets render targets by directory
    /// </summary>
    /// <param name="relativePath">Relative directory path</param>
    /// <returns>Enumerable of render targets</returns>
    public abstract IEnumerable<RenderTarget> GetRenderTargetsByDirectory(string relativePath);

    /// <summary>
    /// Gets render directories by directory
    /// </summary>
    /// <param name="relativePath">Relative directory path</param>
    /// <returns>Enumerable of directory paths</returns>
    public abstract IEnumerable<string> GetRenderDirectoryByDirectory(string relativePath);

    /// <summary>
    /// Gets render targets for the specified path
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Enumerable of render targets</returns>
    public abstract IEnumerable<RenderTarget> GetRenderTargets(string relativePath);

    /// <summary>
    /// Gets the render target by full path
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>The render target, or null if not found</returns>
    public abstract RenderTarget GetRenderTargetByFullPath(string fullPath);

    /// <summary>
    /// Checks whether the specified render directory exists
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>True if exists</returns>
    public abstract bool ContainsRenderDirectory(string relativePath);

    /// <summary>
    /// Gets the directory status
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>File state</returns>
    public abstract FileState GetDirectoryStatus(string relativePath);

    /// <summary>
    /// Gets the file render status
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Render status</returns>
    public abstract RenderStatus GetFileRenderStatus(string relativePath);

    /// <summary>
    /// Gets the file status
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>File state</returns>
    public abstract FileState GetFileStatus(string relativePath);

    /// <summary>
    /// Binds a render file
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    public abstract void BindRenderFile(string relativePath);

    /// <summary>
    /// Unbinds a render file
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    public abstract void UnbindRenderFile(string relativePath);

    /// <summary>
    /// Unbinds all removing files
    /// </summary>
    /// <returns>True if successful</returns>
    public abstract bool UnbindAllRemovingFiles();

    /// <summary>
    /// Binds all user files
    /// </summary>
    /// <returns>True if successful</returns>
    public abstract bool BindAllUserFiles();

    /// <summary>
    /// Checks whether there are adding files
    /// </summary>
    /// <returns>True if contains adding files</returns>
    public abstract bool ContainsAddingFiles();

    /// <summary>
    /// Checks whether there are removing files
    /// </summary>
    /// <returns>True if contains removing files</returns>
    public abstract bool ContainsRemovingFiles();

    /// <summary>
    /// Checks whether there are user occupied files
    /// </summary>
    /// <returns>True if contains user occupied files</returns>
    public abstract bool ContainsUserOccupiedFiles();

    /// <summary>
    /// Gets files modified by the specified type
    /// </summary>
    /// <param name="type">Modify type</param>
    /// <returns>Enumerable of file paths</returns>
    public abstract IEnumerable<string> GetModifyingFiles(RenderModifyType type);

    /// <summary>
    /// Clears the dirty state
    /// </summary>
    public abstract void ClearDirty();

    #endregion

    #region Config

    /// <summary>
    /// Creates the render configuration
    /// </summary>
    /// <returns>Render configuration</returns>
    public abstract RenderConfig CreateRenderConfig();

    /// <summary>
    /// Exports the configuration to a file
    /// </summary>
    /// <param name="fileName">File name</param>
    public virtual void ExportConfig(string fileName)
    {
    }

    /// <summary>
    /// Updates the configuration
    /// </summary>
    public virtual void UpdateConfig()
    {
    }

    #endregion

    #region Controller

    /// <summary>
    /// Creates a new controller with the specified info
    /// </summary>
    /// <param name="ctrlInfo">Controller info</param>
    /// <returns>True if successful</returns>
    public bool NewController(WorkSpaceControllerInfo ctrlInfo)
    {
        if (StartController(ctrlInfo))
        {
            _controller.OnNew();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new controller of the specified type
    /// </summary>
    /// <typeparam name="T">Controller type</typeparam>
    /// <returns>True if successful</returns>
    public bool NewController<T>() where T : WorkSpaceController
    {
        var info = WorkSpaceController.GetControllerInfo<T>();
        if (info == null)
        {
            return false;
        }

        return NewController(info);
    }

    /// <summary>
    /// Removes the current controller
    /// </summary>
    /// <returns>True if successful</returns>
    public bool RemoveController()
    {
        _controller?.OnRemove();
        return StopController();
    }

    /// <summary>
    /// Updates the current controller
    /// </summary>
    public void UpdateController()
    {
        _controller?.OnUpdate();
    }

    /// <summary>
    /// Starts a controller by name
    /// </summary>
    /// <param name="name">Controller name</param>
    /// <returns>True if successful</returns>
    protected bool StartController(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        if (_controller != null)
        {
            return false;
        }
        WorkSpaceControllerInfo ctrlInfo = WorkSpaceController.GetControllerInfo(name);
        if (ctrlInfo == null)
        {
            return false;
        }
        return StartController(ctrlInfo);
    }

    /// <summary>
    /// Starts a controller with the specified info
    /// </summary>
    /// <param name="ctrlInfo">Controller info</param>
    /// <returns>True if successful</returns>
    protected bool StartController(WorkSpaceControllerInfo ctrlInfo)
    {
        if (ctrlInfo == null)
        {
            throw new ArgumentNullException(nameof(ctrlInfo));
        }

        if (_controller != null)
        {
            return false;
        }

        WorkSpaceController ctrl = (WorkSpaceController)ctrlInfo.ControllerType.CreateInstanceOf();
        _controllerInfo = ctrlInfo;
        _controller = ctrl ?? throw new InvalidOperationException("Cannot create solution : " + ctrlInfo.Name);
        _controller.WorkSpace = this;

        if (!_silent)
        {
            try
            {
                _controller.OnStart();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        return true;
    }

    /// <summary>
    /// Stops the current controller
    /// </summary>
    /// <returns>True if successful</returns>
    protected bool StopController()
    {
        if (_controllerInfo == null)
        {
            return false;
        }
        if (_controller != null)
        {
            if (!_silent)
            {
                try
                {
                    _controller.OnStop();
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }
            _controller.WorkSpace = null;
        }

        _controller = null;
        _controllerInfo = null;
        return true;
    }

    /// <summary>
    /// Renders using the controller
    /// </summary>
    /// <param name="added">Added files</param>
    /// <param name="removed">Removed files</param>
    /// <param name="incremental">Whether incremental render</param>
    protected void RenderController(List<string> added, List<string> removed, bool incremental)
    {
        try
        {
            _controller?.OnRender(added, removed, incremental);
        }
        catch (Exception err)
        {
            err.LogError($"Workspace controller generation failed: {Name}");
        }
    }

    /// <summary>
    /// Starts the controller
    /// </summary>
    protected void StartController()
    {
        try
        {
            _controller?.OnStart();
        }
        catch (Exception err)
        {
            err.LogError($"Start workspace {this.Name} failed.");
            IsFailure = true;
        }
    }

    /// <summary>
    /// Notifies the controller of a rename
    /// </summary>
    /// <param name="oldName">Old name</param>
    protected void NotifyRenamed(string oldName)
    {
        try
        {
            _controller?.OnRenamed(oldName);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Notifies the controller that the master path changed
    /// </summary>
    protected void NotifyMasterPathChanged()
    {
        try
        {
            _controller?.OnMasterPathChanged();
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Called when the controller configuration is updated
    /// </summary>
    protected internal virtual void OnControllerConfigUpdated()
    {
    }

    #endregion

    #region IRenderHost

    /// <summary>
    /// Executes rendering
    /// </summary>
    /// <param name="incremental">Whether incremental render</param>
    /// <returns>True if successful</returns>
    public abstract bool ExecuteRender(bool incremental);

    /// <summary>
    /// Executes rendering for specified targets
    /// </summary>
    /// <param name="targets">Render targets</param>
    /// <returns>True if successful</returns>
    public abstract bool ExecuteRender(IEnumerable<RenderTarget> targets);

    /// <summary>
    /// Restores rendering
    /// </summary>
    /// <param name="userCode">Optional user code library</param>
    /// <returns>True if successful</returns>
    public abstract bool RestoreRender(ICodeLibrary userCode = null);

    /// <summary>
    /// Restores rendering with a specific reference item
    /// </summary>
    /// <param name="refItem">Reference item</param>
    /// <param name="userCode">Optional user code library</param>
    /// <returns>True if successful</returns>
    public abstract bool RestoreRender(IWorkSpaceRefItem refItem, ICodeLibrary userCode = null);

    /// <summary>
    /// Gets all render targets
    /// </summary>
    /// <returns>Enumerable of render targets</returns>
    public abstract IEnumerable<RenderTarget> GetRenderTargets();

    #endregion

    #region Event

    /// <summary>
    /// Event raised when dependency is updated
    /// </summary>
    public event EventHandler DependencyUpdated;

    /// <summary>
    /// Event raised when dependency is renamed
    /// </summary>
    public event EventHandler DependencyRenamed;

    /// <summary>
    /// Event raised when render target is updated
    /// </summary>
    public event EventHandler RenderTargetUpdated;

    /// <summary>
    /// Event raised when master base path is updated
    /// </summary>
    public event EventHandler MasterBasePathUpdated;

    /// <summary>
    /// Raises the DependencyUpdated event
    /// </summary>
    protected virtual void OnDepedencyUpdated()
    {
        try
        {
            DependencyUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Raises the DependencyRenamed event
    /// </summary>
    protected virtual void OnDependencyRenamed()
    {
        try
        {
            DependencyRenamed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Raises the RenderTargetUpdated event
    /// </summary>
    protected virtual void OnRenderTargetUpdated()
    {
        try
        {
            RenderTargetUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Raises the MasterBasePathUpdated event
    /// </summary>
    protected virtual void OnMasterBasePathUpdated()
    {
        try
        {
            MasterBasePathUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    #endregion

    #region Misc

    /// <summary>
    /// Gets the workspace asset
    /// </summary>
    /// <returns>Workspace asset</returns>
    public abstract WorkSpaceAsset GetAsset();

    /// <summary>
    /// Gets the render file name for the specified relative path
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Render file name</returns>
    public RenderFileName GetRenderFileName(string relativePath)
    {
        return new RenderFileName(MasterDirectory, BaseNameSpace, relativePath);
    }

    /// <summary>
    /// Sets an external master path
    /// </summary>
    /// <param name="masterFullPath">Full path to the master directory</param>
    /// <returns>True if successful</returns>
    public abstract bool SetExternalMasterPath(string masterFullPath);

    /// <summary>
    /// Unsets the external master path
    /// </summary>
    /// <returns>True if successful</returns>
    public abstract bool UnsetExternalMasterPath();

    /// <summary>
    /// Shows the user code editor for the specified render target
    /// </summary>
    /// <param name="tareget">Render target</param>
    public abstract void ShowUserCodeEditor(RenderTarget tareget);

    /// <summary>
    /// Marks a file as modified
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    public abstract void MarkFileAsModified(string relativePath);

    /// <summary>
    /// Gets the relative path from the file name
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Relative path</returns>
    public string GetRalativePath(string fileName)
    {
        return fileName.MakeRalativePath(fileName);
    }

    #endregion

    #region Path

    /// <summary>
    /// Makes a relative path from the base directory
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>Relative path</returns>
    public string MakeBaseRelativePath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return string.Empty;
        }

        return fullPath.MakeRalativePath(BaseDirectory);
    }

    /// <summary>
    /// Makes a full path from the base directory
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Full path</returns>
    public string MakeBaseFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return BaseDirectory;
        }

        return relativePath.MakeFullPath(BaseDirectory);
    }

    /// <summary>
    /// Makes a relative path from the master directory
    /// </summary>
    /// <param name="fullPath">Full path</param>
    /// <returns>Relative path</returns>
    public string MakeMasterRelativePath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return string.Empty;
        }

        return fullPath.MakeRalativePath(MasterDirectory);
    }

    /// <summary>
    /// Makes a full path from the master directory
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <returns>Full path</returns>
    public string MakeMasterFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return MasterDirectory;
        }

        return relativePath.MakeFullPath(MasterDirectory);
    }

    #endregion
}