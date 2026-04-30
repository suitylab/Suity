using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Rex;
using Suity.Rex.VirtualDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Internal implementation of <see cref="WorkSpaceManager"/> that manages the lifecycle of workspaces,
/// including creation, deletion, renaming, file system watching, and solution generation.
/// </summary>
public class WorkSpaceManagerBK : WorkSpaceManager
{
    private readonly Project _ownerProject;
    private readonly string _basePath;
    private EditorFileSystemWatcher _watcher;
    private readonly Dictionary<string, WorkSpaceBK> _workSpaces = [];
    private readonly WorkSpaceManagerAsset _asset;
    private bool _isReleased;
    private DisposeCollector _listeners;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceManagerBK"/> for the specified project.
    /// </summary>
    /// <param name="project">The owning project.</param>
    /// <param name="basePath">The base directory path for all workspaces.</param>
    internal WorkSpaceManagerBK(Project project, string basePath)
    {
        EditorServices.SystemLog.AddLog($"WorkSpaceManager creating : {basePath}...");
        EditorServices.SystemLog.PushIndent();

        Debug.Assert(project != null);
        Debug.Assert(!string.IsNullOrEmpty(basePath));

        _ownerProject = project;
        _basePath = basePath;

        _asset = new WorkSpaceManagerAsset(this);

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"WorkSpaceManager created.");
    }

    /// <summary>
    /// Starts the workspace manager, initializes file system watching, and loads the project setting.
    /// </summary>
    /// <param name="setting">The project setting to load workspace configurations from.</param>
    internal void Start(ProjectSetting setting)
    {
        if (_watcher != null)
        {
            return;
        }

        EditorServices.SystemLog.AddLog($"WorkSpaceManager staring ...");
        EditorServices.SystemLog.PushIndent();

        _watcher = new EditorFileSystemWatcher(_basePath, this)
        {
            IncludeSubdirectories = false
        };

        _watcher.Created += _watcher_Created;
        _watcher.Deleted += _watcher_Deleted;
        _watcher.Changed += _watcher_Changed;
        _watcher.Renamed += _watcher_Renamed;

        _watcher.EnableRaisingEvents = true;

        DocumentManager.Instance.DocumentLoaded += DocumentManager_DocumentLoad;
        DocumentManager.Instance.DocumentChangedExternal += DocumentManager_DocumentChangedExternal;

        LoadSetting(setting);

        _listeners += EditorRexes.Mapper.Provide<WorkSpaceManager>(this);
        _listeners += EditorRexes.FormReady.AsRexListener().Where(v => v).Subscribe(v =>
        {
            // Before clearing, push all delayed actions
            EditorUtility.FlushDelayedActions();
            foreach (var workSpace in _workSpaces.Values)
            {
                workSpace.ClearDirty();
            }
        });
        _listeners += EditorCommands.SaveAllDocuments.AddActionListener(() =>
        {
            foreach (var workSpace in _workSpaces.Values)
            {
                workSpace.SaveConfig();
            }
        });

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"WorkSpaceManager started.");
    }

    /// <summary>
    /// Releases all resources, stops file system watching, and disposes all managed workspaces.
    /// </summary>
    internal void Release()
    {
        EditorServices.SystemLog.AddLog($"WorkSpaceManager releasing...");
        EditorServices.SystemLog.PushIndent();

        _isReleased = true;

        DocumentManager.Instance.DocumentLoaded -= DocumentManager_DocumentLoad;
        DocumentManager.Instance.DocumentChangedExternal -= DocumentManager_DocumentChangedExternal;

        if (_watcher != null)
        {
            _watcher.Created -= _watcher_Created;
            _watcher.Deleted -= _watcher_Deleted;
            _watcher.Changed -= _watcher_Changed;
            _watcher.Renamed -= _watcher_Renamed;

            _watcher.Dispose();
            _watcher = null;
        }

        foreach (var workSpace in _workSpaces.Values)
        {
            workSpace.Release();
        }

        _workSpaces.Clear();
        _listeners?.Dispose();

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"WorkSpaceManager released.");
    }

    #region Property

    /// <inheritdoc/>
    public override Project OwnerProject => _ownerProject;

    /// <inheritdoc/>
    public override string BasePath => _basePath;

    /// <inheritdoc/>
    public override Guid SolutionGuid => OwnerProject.ProjectGuid;

    /// <inheritdoc/>
    public override bool IsReleased => _isReleased;

    /// <inheritdoc/>
    public override WorkSpaceManagerAsset Asset => _asset;

    #endregion

    #region Setting

    /// <summary>
    /// Loads workspace configurations from the specified project setting and initializes all workspaces.
    /// </summary>
    /// <param name="setting">The project setting containing workspace configurations.</param>
    private void LoadSetting(ProjectSetting setting)
    {
        if (setting is null)
        {
            throw new ArgumentNullException(nameof(setting));
        }

        _workSpaces.Clear();

        var dirInfo = new DirectoryInfo(BasePath);
        foreach (var workSpaceDir in dirInfo.GetDirectories())
        {
            EnsureWorkSpace(workSpaceDir.Name, true, out bool added);
        }

        // Unified starting process to make project reference valid.
        foreach (var workspace in _workSpaces.Values)
        {
            workspace.InternalStartController();
        }

        // Do not write Solution file during loading
        // WriteSolution();
    }

    /// <summary>
    /// Saves the current workspace configurations to the specified project setting.
    /// </summary>
    /// <param name="setting">The project setting to save configurations to.</param>
    internal void SaveSetting(ProjectSetting setting)
    {
        foreach (var workSpace in _workSpaces.Values)
        {
            workSpace.SaveConfig(true);
        }
    }

    #endregion

    #region WorkSpace

    /// <inheritdoc/>
    public override WorkSpace AddWorkSpace(string name, WorkSpaceControllerInfo ctrlInfo = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (GetWorkSpace(name) != null)
        {
            return null;
        }

        WorkSpaceBK workSpace = EnsureWorkSpace(name, false, out bool added);

        // Changed to create FreeController by default
        ctrlInfo ??= WorkSpacesExternalBK.Instance.GetControllerInfo(typeof(CommonController));

        if (ctrlInfo != null)
        {
            try
            {
                workSpace.NewController(ctrlInfo);
                workSpace.SaveConfig(true);
                workSpace.UpdateController();

                workSpace.Controller?.TryWriteProjectFile();

                WriteSolution();

                return workSpace;
            }
            catch (Exception err)
            {
                err.LogError("Add work space failed.");

                return null;
            }
        }
        else if (added)
        {
            workSpace.Controller?.TryWriteProjectFile();
            WriteSolution();
        }

        return workSpace;
    }

    /// <inheritdoc/>
    public override bool DeleteWorkSpace(string name)
    {
        bool success = false;
        FileUnwatchedAction.Do(() =>
        {
            success = InternalDeleteWorkSpcace(name);
        });

        return success;
    }

    /// <inheritdoc/>
    public override bool CanRenameWorkSpace(string oldName, string newName)
    {
        if (oldName == newName)
        {
            return false;
        }

        string oldId = oldName.GetPathLowId();
        string newId = newName.GetPathLowId();

        if (!_workSpaces.ContainsKey(oldId))
        {
            return false;
        }

        if (_workSpaces.ContainsKey(newId))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool RenameWorkSpace(string oldName, string newName)
    {
        bool success = false;
        FileUnwatchedAction.Do(() =>
        {
            success = InternalRenameWorkSpace(oldName, newName, true);
        });

        return success;
    }

    /// <inheritdoc/>
    public override WorkSpace GetWorkSpace(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _workSpaces.GetValueSafe(name.GetPathLowId());
    }

    /// <inheritdoc/>
    public override bool ContainsWorkSpace(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return GetWorkSpace(name) != null;
    }

    /// <inheritdoc/>
    public override IEnumerable<T> GetControllers<T>()
    {
        return _workSpaces.Values.Where(o => o.Controller is T).Select(o => (T)o.Controller);
    }

    /// <inheritdoc/>
    public override IEnumerable<WorkSpace> WorkSpaces => _workSpaces.Values.Select(o => o);

    /// <inheritdoc/>
    public override int WorkSpaceCount => _workSpaces.Count;

    private WorkSpaceBK EnsureWorkSpace(string name, bool initializing, out bool added)
    {
        string id = name.GetPathLowId();
        string workSpacePath = BasePath.PathAppend(name);

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

        if (!_workSpaces.TryGetValue(id, out WorkSpaceBK workSpace))
        {
            workSpace = new WorkSpaceBK(this, name);
            workSpace.LoadConfig(initializing);
            _workSpaces.Add(id, workSpace);
            added = true;

            OnWorkSpaceAdded(new WorkSpaceEventArgs(workSpace));
        }
        else
        {
            if (!workSpace.ConfigFileExists())
            {
                workSpace.NewConfig();
            }

            added = false;
        }

        return workSpace;
    }

    private bool InternalRenameWorkSpace(string oldName, string newName, bool renameDirectory)
    {
        if (oldName == newName)
        {
            return false;
        }

        string oldId = oldName.GetPathLowId();
        string newId = newName.GetPathLowId();

        if (_workSpaces.ContainsKey(newId))
        {
            return false;
        }

        WorkSpaceBK workSpace = _workSpaces.RemoveAndGet(oldId);
        if (workSpace != null)
        {
            var ctrlInfo = workSpace.ControllerInfo;
            string oldDirName = workSpace.BaseDirectory;
            string newDirName = BasePath.PathAppend(newName);

            workSpace.SaveConfig(true);
            workSpace.Release();
            OnWorkSpaceRemoved(new WorkSpaceEventArgs(workSpace));

            if (renameDirectory)
            {
                try
                {
                    Directory.Move(oldDirName, newDirName);
                    FileUnwatchedAction.NotifyRenamed(oldDirName, newDirName);
                }
                catch (Exception e)
                {
                    e.LogError("Failed to rename workspace (folder occupied externally?)");

                    workSpace = new WorkSpaceBK(this, oldName);
                    if (workSpace.ConfigFileExists())
                    {
                        workSpace.LoadConfig(false);
                    }
                    else
                    {
                        workSpace.SaveConfig(true);
                    }
                    _workSpaces.Add(oldId, workSpace);

                    OnWorkSpaceAdded(new WorkSpaceEventArgs(workSpace));

                    return false;
                }
            }

            workSpace = new WorkSpaceBK(this, newName);
            if (workSpace.ConfigFileExists())
            {
                workSpace.LoadConfig(false);
            }
            else
            {
                workSpace.SaveConfig(true);
            }

            _workSpaces.Add(newId, workSpace);
            workSpace.InternalNotifyRenamed(oldName);
            WriteSolution();

            OnWorkSpaceAdded(new WorkSpaceEventArgs(workSpace));
            OnWorkSpaceRenamed(new WorkSpaceRenameEventArgs(workSpace, oldName));

            return true;
        }

        return false;
    }

    private bool InternalDeleteWorkSpcace(string name)
    {
        WorkSpaceBK workSpace = _workSpaces.RemoveAndGet(name.GetPathLowId());
        if (workSpace != null)
        {
            OnWorkSpaceRemoved(new WorkSpaceEventArgs(workSpace));
            workSpace.RemoveController();
            workSpace.Release();
            WriteSolution();
        }

        string dirName = BasePath.PathAppend(name);

        if (Directory.Exists(dirName))
        {
            try
            {
                EditorUtility.SendToRecycleBin(dirName);
            }
            catch (Exception)
            {
            }
        }

        if (!Directory.Exists(dirName))
        {
            return true;
        }

        Logs.LogError("Failed to delete workspace (folder occupied externally?)");

        workSpace = new WorkSpaceBK(this, name);
        if (workSpace.ConfigFileExists())
        {
            workSpace.LoadConfig(false);
        }
        else
        {
            workSpace.SaveConfig(true);
        }
        _workSpaces.Add(name, workSpace);

        OnWorkSpaceAdded(new WorkSpaceEventArgs(workSpace));

        return false;
    }

    #endregion

    #region Path

    /// <inheritdoc/>
    public override string MakeFullPath(string relativePath)
    {
        return relativePath.MakeFullPath(BasePath);
    }

    /// <inheritdoc/>
    public override string MakeRelativePath(string fullPath)
    {
        return fullPath.MakeRalativePath(BasePath);
    }

    #endregion

    #region Event handling

    private void _watcher_Created(string fullPath)
    {
        //FileInfo file = new FileInfo(fullPath);
        //if (string.Compare(file.Name, RenderConfigFileName, true) == 0)
        //{
        //    EnsureWorkSpace(file.FullName);
        //}
        QueuedAction.Do(() =>
        {
            string rPath = fullPath.MakeRalativePath(BasePath);
            if (rPath.GetPathTerminal() == rPath && Directory.Exists(fullPath))
            {
                EnsureWorkSpace(rPath, false, out bool added);
                if (added)
                {
                    WriteSolution();
                }
            }
        });
    }

    private void _watcher_Deleted(string fullPath)
    {
        QueuedAction.Do(() =>
        {
            string rPath = fullPath.MakeRalativePath(BasePath);
            if (rPath.GetPathTerminal() == rPath)
            {
                DeleteWorkSpace(rPath);
            }
        });
    }

    private void _watcher_Changed(string fullPath)
    {
    }

    private void _watcher_Renamed(string fullPath, string oldFullPath)
    {
        QueuedAction.Do(() =>
        {
            string rPathOld = oldFullPath.MakeRalativePath(BasePath);
            string rPathNew = fullPath.MakeRalativePath(BasePath);

            InternalRenameWorkSpace(rPathOld, rPathNew, false);
        });
    }

    private void DocumentManager_DocumentLoad(DocumentEntry document)
    {
    }

    private void DocumentManager_DocumentChangedExternal(DocumentEntry document)
    {
        EditorUtility.AddDelayedAction(new DocumentChangedExternalEvent(this, document.FileName.FullPath));
    }

    private void HandleDocumentChangedExternal(string name)
    {
    }

    #endregion

    #region Solution

    /// <inheritdoc/>
    public override void WriteSolution()
    {
        FileUnwatchedAction.Do(() =>
        {
            foreach (var space in _workSpaces.Values)
            {
                space.UpdateController();
            }
            EditorRexes.WriteSolution.Invoke(this);
        });
    }

    #endregion

    #region Plugin Update

    /// <inheritdoc/>
    public override void UpdatePluginDelayed()
    {
        EditorUtility.AddDelayedAction(new PluginUpdateAction(this));
    }

    /// <summary>
    /// Handles the plugin update event by prompting the user to restart the project.
    /// </summary>
    internal void HandlePluginUpdate()
    {
        EditorCommands.Mapper.Handle(new ShowNotifyVReq
        {
            Title = "Plugin updated, restart project?",
            ButtonText = "Restart",
            Action = EditorRexes.Restart.Invoke,
        });
    }

    #endregion

    /// <inheritdoc/>
    internal override void RaiseRenderTargetUpdated(WorkSpace workSpace)
    {
        OnWorkSpaceRenderTargetUpdated(new WorkSpaceEventArgs(workSpace));
    }

    /// <summary>
    /// Delayed action that handles external document change events.
    /// </summary>
    private class DocumentChangedExternalEvent : DelayedNamedAction<WorkSpaceManagerBK>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DocumentChangedExternalEvent"/>.
        /// </summary>
        /// <param name="manager">The owning workspace manager.</param>
        /// <param name="fullPath">The full path of the changed document.</param>
        public DocumentChangedExternalEvent(WorkSpaceManagerBK manager, string fullPath)
            : base(manager, fullPath, null)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            if (!Value.IsReleased)
            {
                Value.HandleDocumentChangedExternal(Name);
            }
        }
    }

    /// <summary>
    /// Delayed action that handles plugin update notifications.
    /// </summary>
    private class PluginUpdateAction : DelayedAction<WorkSpaceManagerBK>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PluginUpdateAction"/>.
        /// </summary>
        /// <param name="manager">The owning workspace manager.</param>
        public PluginUpdateAction(WorkSpaceManagerBK manager)
            : base(manager, 2)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            base.Value.HandlePluginUpdate();
        }
    }
}