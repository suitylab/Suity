using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor;

/// <summary>
/// Internal backend implementation of <see cref="Project"/>, managing project lifecycle (open/close),
/// directories, settings, GUIDs, plugin states, asset configs, and assembly file copying.
/// </summary>
internal class ProjectBK : Project
{
    /// <summary>
    /// The file extension used for Suity project files.
    /// </summary>
    public const string ProjectFileExtension = ".suity";

    private readonly string _projectBasePath;
    private readonly string _projectName;

    private ProjectSetting _setting;
    private ProjectIdResolver _idResolver;
    private FileAssetManagerBK _fileLibrary;
    private WorkSpaceManagerBK _workSpaceMngr;

    private ProjectStatus _status;

    internal readonly Dictionary<string, object> _pluginStates = [];
    internal readonly Dictionary<Guid, Dictionary<string, object>> _assetConfigs = [];
    private readonly Dictionary<string, string> _configs = [];

    /// <summary>
    /// Gets the internal ID resolver for this project.
    /// </summary>
    internal ProjectIdResolver IdResolver => _idResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBK"/> class.
    /// </summary>
    /// <param name="projectBasePath">The base directory path for the project.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="projectGuid">Optional GUID for the project. If not provided, one will be generated.</param>
    internal ProjectBK(string projectBasePath, string projectName, Guid? projectGuid = null)
    {
        if (string.IsNullOrEmpty(projectBasePath))
        {
            throw new ArgumentNullException(nameof(projectBasePath));
        }

        EditorServices.SystemLog.AddLog($"Project creating : {projectName}...");
        EditorServices.SystemLog.PushIndent();

        _setting = new ProjectSetting();
        _projectBasePath = projectBasePath;
        _projectName = projectName;
        if (projectGuid.HasValue)
        {
            _setting.ProjectGuid = projectGuid.Value;
        }

        if (!Directory.Exists(projectBasePath))
        {
            Directory.CreateDirectory(projectBasePath);
        }

        EnsureAllSystemDirectories();

        _idResolver = new ProjectIdResolver(this, _projectBasePath.PathAppend(_setting.AssetDirectory));
        _idResolver.Start();
        GlobalIdResolver.Current = _idResolver;

        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            _fileLibrary = new FileAssetManagerBK(this, AssetDirectory);
            _workSpaceMngr = new WorkSpaceManagerBK(this, _projectBasePath.PathAppend(_setting.WorkSpaceDirectory));
        });

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Project created.");
    }


    

    /// <inheritdoc/>
    public override string ProjectBasePath => _projectBasePath;
    /// <inheritdoc/>
    public override string ProjectName => _projectName;
    /// <inheritdoc/>
    public override ProjectStatus Status => _status;

    /// <inheritdoc/>
    public override FileAssetManager FileAssetManager => _fileLibrary;
    /// <inheritdoc/>
    public override WorkSpaceManager WorkSpaceManager => _workSpaceMngr;

    #region Directory Settings

    /// <inheritdoc/>
    public override string ProjectSettingFile => Path.Combine(_projectBasePath, _projectName) + ProjectFileExtension;
    /// <inheritdoc/>
    public override string SolutionFile => Path.Combine(_projectBasePath, _projectName) + ".sln";
    /// <inheritdoc/>
    public override string AssetDirectory => GetSubDirectory(_setting.AssetDirectory);
    /// <inheritdoc/>
    public override string UserDirectory => GetSubDirectory(_setting.UserDirectory);
    /// <inheritdoc/>
    public override string WorkSpaceDirectory => GetSubDirectory(_setting.WorkSpaceDirectory);
    /// <inheritdoc/>
    public override string SystemDirectory => GetSubDirectory(_setting.SystemDirectory);
    /// <inheritdoc/>
    public override string PublishDirectory => GetSubDirectory(_setting.PublishDirectory);
    /// <inheritdoc/>
    public override string AssembliesDirectory => GetSubDirectory(_setting.AssembliesDirectory);

    #endregion

    #region Guid Settings

    /// <inheritdoc/>
    public override Guid ProjectGuid => _setting.ProjectGuid;
    /// <inheritdoc/>
    public override Guid PlanetFolderGuid => _setting.PlanetFolderGuid;
    /// <inheritdoc/>
    public override Guid SateliteFolderGuid => _setting.SateliteFolderGuid;
    /// <inheritdoc/>
    public override Guid SpaceshipFolderGuid => _setting.SpaceshipFolderGuid;
    /// <inheritdoc/>
    public override Guid AstronautFolderGuid => _setting.AstronautFolderGuid;
    /// <inheritdoc/>
    public override Guid PluginFolderGuid => _setting.PluginFolderGuid;

    #endregion

    /// <inheritdoc/>
    public override void HandleExternalRename(RenameAction renameAction)
    {
        _fileLibrary.HandleExternalRename(renameAction);
    }

    /// <inheritdoc/>
    public override string GetConfig(string key)
    {
        return _configs.GetValueSafe(key);
    }

    #region State

    /// <summary>
    /// Gets the state object associated with a specific plugin.
    /// </summary>
    /// <param name="plugin">The plugin to get state for.</param>
    /// <returns>The plugin state object, or <c>null</c> if not set.</returns>
    internal override object GetPluginState(Plugin plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException();
        }

        if (_status == ProjectStatus.Closed) // || _library is null)
        {
            throw new InvalidOperationException();
        }

        if (_pluginStates.TryGetValue(plugin.Name, out object value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the state object associated with a specific asset for a plugin.
    /// </summary>
    /// <param name="plugin">The plugin to get state for.</param>
    /// <param name="asset">The asset to get state for.</param>
    /// <returns>The asset state object, or <c>null</c> if not set.</returns>
    internal override object GetAssetState(Plugin plugin, Asset asset)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException();
        }

        if (asset is null)
        {
            throw new ArgumentNullException();
        }

        if (_status == ProjectStatus.Closed || _idResolver is null)
        {
            throw new InvalidOperationException();
        }

        if (_assetConfigs.TryGetValue(asset.Id, out Dictionary<string, object> assetConfig))
        {
            return assetConfig.GetValueSafe(plugin.Name);
        }

        return null;
    }

    /// <summary>
    /// Sets the state object for a specific plugin.
    /// </summary>
    /// <param name="plugin">The plugin to set state for.</param>
    /// <param name="value">The state object to store.</param>
    internal override void SetPluginState(Plugin plugin, object value)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException();
        }

        if (_status == ProjectStatus.Closed) // || _library is null)
        {
            throw new InvalidOperationException();
        }

        _pluginStates[plugin.Name] = value;
    }

    /// <summary>
    /// Sets the state object for a specific asset within a plugin.
    /// </summary>
    /// <param name="plugin">The plugin that owns the asset state.</param>
    /// <param name="asset">The asset to set state for.</param>
    /// <param name="value">The state object to store.</param>
    internal override void SetAssetState(Plugin plugin, Asset asset, object value)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException();
        }

        if (asset is null)
        {
            throw new ArgumentNullException();
        }

        if (_status == ProjectStatus.Closed) // || _library is null)
        {
            throw new InvalidOperationException();
        }

        //if (asset.Library != _library)
        //{
        //    return;
        //}

        var assetConfig = _assetConfigs.GetOrAdd(asset.Id, _ => []);

        assetConfig[plugin.Name] = value;
    }

    #endregion

    #region Setting

    /// <summary>
    /// Loads plugin settings from the project settings XML file.
    /// </summary>
    internal override void LoadSetting()
    {
        string fileName = SystemDirectory.PathAppend("ProjectSetting.xml");
        if (!File.Exists(fileName))
        {
            return;
        }

        var pluginManager = PluginManager.Instance;

        // Load plugin configuration
        try
        {
            var reader = XmlNodeReader.FromFile(fileName, false);
            if (reader is null)
            {
                return;
            }

            foreach (var pluginReader in reader.Nodes("Plugin"))
            {
                string name = pluginReader.GetAttribute("name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var plugin = pluginManager.GetPluginInfo(name);
                if (plugin is null)
                {
                    continue;
                }

                var viewObj = plugin.Plugin as IViewObject;
                if (viewObj is null)
                {
                    continue;
                }

                try
                {
                    Serializer.Deserialize(viewObj, pluginReader);
                }
                catch (Exception err2)
                {
                    err2.LogError($"Loading plugin setting failed : {name}");
                }
            }
        }
        catch (Exception err)
        {
            err.LogError("Load project setting failed.");
        }
    }

    /// <summary>
    /// Saves plugin settings to the project settings XML file.
    /// </summary>
    internal override void SaveSetting()
    {
        try
        {
            var pluginManager = PluginManager.Instance;
            var writer = new XmlNodeWriter("ProjectSetting");

            foreach (var plugin in pluginManager.Plugins)
            {
                var viewObj = plugin.Plugin as IViewObject;
                if (viewObj is null)
                {
                    continue;
                }

                writer.SetElement("Plugin", pluginWriter => 
                {
                    pluginWriter.SetAttribute("name", plugin.Name);

                    try
                    {
                        Serializer.Serialize(viewObj, pluginWriter);
                    }
                    catch (Exception err2)
                    {
                        err2.LogError($"Save plugin setting failed : {plugin.Name}");
                    }
                });
            }

            string fileName = SystemDirectory.PathAppend("ProjectSetting.xml");

            writer.SaveToFile(fileName);
        }
        catch (Exception err)
        {
            err.LogError("Save project setting failed.");
        }
    }

    #endregion

    #region Directory

    /// <inheritdoc/>
    public override string GetSubDirectory(string subDirectory)
    {
        return Path.Combine(_projectBasePath, subDirectory);
    }

    /// <inheritdoc/>
    public override void EnsureAllSystemDirectories()
    {
        EnsureSubDirectory(_setting.AssetDirectory);
        EnsureSubDirectory(_setting.UserDirectory);
        EnsureSubDirectory(_setting.SystemDirectory);
        EnsureSubDirectory(_setting.WorkSpaceDirectory);
        EnsureSubDirectory(_setting.PublishDirectory);
        EnsureSubDirectory(_setting.AssembliesDirectory);

        if (Device.Current.GetService<IAssemblyNameService>() is { } asmNameService)
        {
            foreach (var asmName in asmNameService.GetAssemblyNames(AssemblyRefLevel.Editor))
            {
                TryCopyAssemblyFile($"{asmName}.dll");
                TryCopyAssemblyFile($"{asmName}.pdb");
                TryCopyAssemblyFile($"{asmName}.xml");
            }
        }
    }

    /// <inheritdoc/>
    public override string EnsureSubDirectory(string subDirectory)
    {
        string dir = GetSubDirectory(subDirectory);
        EnsureDirectory(dir);

        return dir;
    }

    /// <inheritdoc/>
    public override string EnsureAndCleanUpSubDirectory(string subDirectory)
    {
        string dir = GetSubDirectory(subDirectory);
        EnsureAndCleanUpDirectory(dir);

        return dir;
    }

    /// <inheritdoc/>
    public override void DeleteSubDirectory(string subDirectory, bool save = true)
    {
        string path = GetSubDirectory(subDirectory);

        if (save)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        else
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    /// <summary>
    /// Ensures a directory exists at the specified path, deleting any file at that path first.
    /// </summary>
    /// <param name="path">The directory path to ensure.</param>
    private void EnsureDirectory(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Ensures a directory exists and removes all existing files and subdirectories within it.
    /// </summary>
    /// <param name="path">The directory path to ensure and clean up.</param>
    private void EnsureAndCleanUpDirectory(string path)
    {
        EnsureDirectory(path);
        var dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists)
        {
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var subDir in dirInfo.GetDirectories())
            {
                subDir.Delete(true);
            }
        }
    }

    /// <summary>
    /// Copies an assembly file from the application domain base directory to the project assemblies directory if it has changed.
    /// </summary>
    /// <param name="name">The assembly file name (e.g., "Assembly.dll").</param>
    /// <returns><c>true</c> if the file was copied; <c>false</c> if it was already up to date or not found.</returns>
    private bool TryCopyAssemblyFile(string name)
    {
        string officialDllFileName = AppDomain.CurrentDomain.BaseDirectory.PathAppend(name);
        var officialDllInfo = new FileInfo(officialDllFileName);

        if (!officialDllInfo.Exists)
        {
            return false;
        }

        string projectDllFileName = AssembliesDirectory.PathAppend(name);
        var projectDllInfo = new FileInfo(projectDllFileName);

        if (!projectDllInfo.Exists || projectDllInfo.Length != officialDllInfo.Length || projectDllInfo.LastWriteTimeUtc != officialDllInfo.LastWriteTimeUtc)
        {
            try
            {
                EditorServices.SystemLog.AddLog($"Copying assembly file : {projectDllInfo.FullName}");
                officialDllInfo.CopyTo(projectDllInfo.FullName, true);
                return true;
            }
            catch (Exception err)
            {
                err.LogError($"Copy assembly failed : {name}");
                return false;
            }
        }

        return false;
    }

    #endregion

    #region Project creation and management

    /// <summary>
    /// Performs pre-open operations: loads project settings, ensures directories, and starts subsystems.
    /// </summary>
    public void PreOpenProject()
    {
        if (_status >= ProjectStatus.Starting)
        {
            return;
        }

        _status = ProjectStatus.Starting;

        EditorServices.SystemLog.AddLog($"Project openening : {ProjectName}...");
        EditorServices.SystemLog.PushIndent();

        if (File.Exists(_projectBasePath))
        {
            File.Delete(_projectBasePath);
        }

        if (!Directory.Exists(_projectBasePath))
        {
            Directory.CreateDirectory(_projectBasePath);
        }

        string settingFileName = ProjectSettingFile;
        EditorServices.SystemLog.AddLog($"Reading project setting file : {settingFileName}...");

        if (File.Exists(settingFileName))
        {
            try
            {
                string str = TextFileHelper.ReadFile(settingFileName);
                _setting = JsonHelper.Deserialize<ProjectSetting>(str);
            }
            catch (Exception)
            {
                _setting = new ProjectSetting();
                XmlSerializer.DeserializeFromFile(_setting, settingFileName);
            }

            if (string.IsNullOrEmpty(_setting.AssetDirectory))
            {
                throw new FileLoadException();
            }

            if (string.IsNullOrEmpty(_setting.UserDirectory))
            {
                throw new FileLoadException();
            }
        }
        else
        {
            // No need to create default, because there is already one by default
            //_setting = ProjectSetting.CreateDefault();

            _setting.Version = ServiceInternals._license.ProductVersion;
            string str = JsonHelper.Serialize(_setting);
            TextFileHelper.WriteFile(settingFileName, str);
        }

        _setting.Configs ??= [];

        _setting.Configs.RemoveAll(o => string.IsNullOrWhiteSpace(o.Key));
        if (_setting.Configs != null)
        {
            foreach (var item in _setting.Configs.SkipNull())
            {
                if (_configs.ContainsKey(item.Key))
                {
                    Logs.LogWarning($"Duplicate key in project file Configs:{item.Key}");
                    continue;
                }

                _configs[item.Key] = item.Value;
            }
        }

        // Initialize

        EditorServices.SystemLog.AddLog("Ensure all system directories and system assemblies...");
        EnsureAllSystemDirectories();

        _workSpaceMngr.Start(_setting);
        _fileLibrary.Start();

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Project opened.");
    }

    /// <summary>
    /// Marks the project as fully opened after all pre-open operations complete.
    /// </summary>
    public void PostOpenProject()
    {
        if (_status >= ProjectStatus.Opened)
        {
            return;
        }

        _status = ProjectStatus.Opened;
    }

    /// <summary>
    /// Scans the project directory asynchronously to discover assets.
    /// </summary>
    /// <returns>A task representing the scan operation.</returns>
    public Task ScanProjectDirectory()
    {
        return _fileLibrary.ScanProjectDirectoryWithTask();
    }

    /// <summary>
    /// Closes the project, saving settings and releasing resources.
    /// </summary>
    public void CloseProject()
    {
        if (_status == ProjectStatus.Closed)
        {
            return;
        }

        EditorServices.SystemLog.AddLog($"Project closing : {ProjectName}...");
        EditorServices.SystemLog.PushIndent();

        SaveSetting();

        _workSpaceMngr.SaveSetting(_setting);
        _setting.Version = ServiceInternals._license.ProductVersion;

        string settingFileName = ProjectSettingFile;
        //XmlSerializer.SerializeToFile(_setting, settingFileName);

        string str = JsonHelper.Serialize(_setting);
        TextFileHelper.CompareWrite(settingFileName, str);

        _status = ProjectStatus.Closed;

        _idResolver?.Release();
        _workSpaceMngr?.Release();

        _idResolver = null;
        _workSpaceMngr = null;

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Project closed.");
    }

    /// <summary>
    /// Copies all plugin states into the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to populate with plugin states.</param>
    public void GetAllConfigs(Dictionary<string, object> dictionary)
    {
        foreach (var pair in _pluginStates)
        {
            dictionary[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Replaces all plugin states with the values from the provided dictionary.
    /// </summary>
    /// <param name="dictrionary">The dictionary containing new plugin states.</param>
    public void SetAllConfigs(Dictionary<string, object> dictrionary)
    {
        _pluginStates.Clear();
        foreach (var pair in dictrionary)
        {
            _pluginStates[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Populates the specified collection with all plugin and asset configuration items.
    /// </summary>
    /// <param name="items">The collection to populate with configuration items.</param>
    public void GetConfigItems(ICollection<ProjectConfigItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException();
        }

        foreach (var pair in _pluginStates)
        {
            items.Add(new ProjectConfigItem { Plugin = pair.Key, Value = pair.Value });
        }

        foreach (var assetPair in _assetConfigs)
        {
            if (assetPair.Key == Guid.Empty)
            {
                continue;
            }

            foreach (var pair in assetPair.Value)
            {
                items.Add(new ProjectConfigItem { Plugin = pair.Key, Asset = assetPair.Key, Value = pair.Value });
            }
        }
    }

    /// <summary>
    /// Replaces all plugin and asset configuration items with the values from the provided collection.
    /// </summary>
    /// <param name="items">The collection of configuration items to apply.</param>
    public void SetConfigItems(ICollection<ProjectConfigItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException();
        }

        foreach (var item in items)
        {
            if (item is null || string.IsNullOrEmpty(item.Plugin))
            {
                continue;
            }

            if (item.Asset == Guid.Empty)
            {
                _pluginStates[item.Plugin] = item.Value;
            }
            else
            {
                var assetConfig = _assetConfigs.GetOrAdd(item.Asset, _ => []);
                assetConfig[item.Plugin] = item.Value;
            }
        }
    }

    #endregion
}