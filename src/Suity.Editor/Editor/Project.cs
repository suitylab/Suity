using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;

namespace Suity.Editor;

/// <summary>
/// Specifies the project status.
/// </summary>
public enum ProjectStatus
{
    None,
    Starting,
    Opened,
    Closed,
}

/// <summary>
/// Editor project
/// </summary>
public abstract class Project
{
    /// <summary>
    /// Current project
    /// </summary>
    public static Project Current { get; internal set; }

    /// <summary>
    /// Gets the base path of the project.
    /// </summary>
    public abstract string ProjectBasePath { get; }

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public abstract string ProjectName { get; }

    /// <summary>
    /// Gets the current status of the project.
    /// </summary>
    public abstract ProjectStatus Status { get; }

    /// <summary>
    /// Gets the file asset manager for the project.
    /// </summary>
    public abstract FileAssetManager FileAssetManager { get; }

    /// <summary>
    /// Gets the workspace manager for the project.
    /// </summary>
    public abstract WorkSpaceManager WorkSpaceManager { get; }

    #region File

    /// <summary>
    /// Gets the path to the project settings file.
    /// </summary>
    public abstract string ProjectSettingFile { get; }

    /// <summary>
    /// Gets the path to the solution file.
    /// </summary>
    public abstract string SolutionFile { get; }

    /// <summary>
    /// Gets the path to the assets directory.
    /// </summary>
    public abstract string AssetDirectory { get; }

    /// <summary>
    /// Gets the path to the user directory.
    /// </summary>
    public abstract string UserDirectory { get; }

    /// <summary>
    /// Gets the path to the workspace directory.
    /// </summary>
    public abstract string WorkSpaceDirectory { get; }

    /// <summary>
    /// Gets the path to the system directory.
    /// </summary>
    public abstract string SystemDirectory { get; }

    /// <summary>
    /// Gets the path to the publish directory.
    /// </summary>
    public abstract string PublishDirectory { get; }

    /// <summary>
    /// Gets the path to the assemblies directory.
    /// </summary>
    public abstract string AssembliesDirectory { get; }

    #endregion

    #region State

    /// <summary>
    /// Gets the state for the specified plugin.
    /// </summary>
    /// <param name="plugin">The plugin to get state for.</param>
    /// <returns>The plugin state.</returns>
    internal abstract object GetPluginState(Plugin plugin);

    /// <summary>
    /// Gets the state for the specified asset and plugin.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <param name="asset">The asset.</param>
    /// <returns>The asset state.</returns>
    internal abstract object GetAssetState(Plugin plugin, Asset asset);

    /// <summary>
    /// Sets the state for the specified plugin.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <param name="value">The state value.</param>
    internal abstract void SetPluginState(Plugin plugin, object value);

    /// <summary>
    /// Sets the state for the specified asset and plugin.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <param name="asset">The asset.</param>
    /// <param name="value">The state value.</param>
    internal abstract void SetAssetState(Plugin plugin, Asset asset, object value);

    #endregion

    #region Setting


    /// <summary>
    /// Loads the project settings.
    /// </summary>
    internal abstract void LoadSetting();

    /// <summary>
    /// Saves the project settings.
    /// </summary>
    internal abstract void SaveSetting();


    /// <summary>
    /// Gets the project GUID.
    /// </summary>
    public abstract Guid ProjectGuid { get; }

    /// <summary>
    /// Gets the planet folder GUID.
    /// </summary>
    public abstract Guid PlanetFolderGuid { get; }

    /// <summary>
    /// Gets the satellite folder GUID.
    /// </summary>
    public abstract Guid SateliteFolderGuid { get; }

    /// <summary>
    /// Gets the spaceship folder GUID.
    /// </summary>
    public abstract Guid SpaceshipFolderGuid { get; }

    /// <summary>
    /// Gets the astronaut folder GUID.
    /// </summary>
    public abstract Guid AstronautFolderGuid { get; }

    /// <summary>
    /// Gets the plugin folder GUID.
    /// </summary>
    public abstract Guid PluginFolderGuid { get; }

    #endregion

    /// <summary>
    /// Gets the full path to a subdirectory within the project.
    /// </summary>
    /// <param name="subDirectory">The subdirectory name.</param>
    /// <returns>The full path to the subdirectory.</returns>
    public abstract string GetSubDirectory(string subDirectory);

    /// <summary>
    /// Ensures all system directories exist.
    /// </summary>
    public abstract void EnsureAllSystemDirectories();

    /// <summary>
    /// Ensures a subdirectory exists, creating it if necessary.
    /// </summary>
    /// <param name="subDirectory">The subdirectory name.</param>
    /// <returns>The full path to the subdirectory.</returns>
    public abstract string EnsureSubDirectory(string subDirectory);

    /// <summary>
    /// Ensures a subdirectory exists and clears its contents.
    /// </summary>
    /// <param name="subDirectory">The subdirectory name.</param>
    /// <returns>The full path to the subdirectory.</returns>
    public abstract string EnsureAndCleanUpSubDirectory(string subDirectory);

    /// <summary>
    /// Deletes a subdirectory.
    /// </summary>
    /// <param name="subDirectory">The subdirectory name.</param>
    /// <param name="save">Whether to save settings after deletion.</param>
    public abstract void DeleteSubDirectory(string subDirectory, bool save = true);

    /// <summary>
    /// Handles external rename operations.
    /// </summary>
    /// <param name="doRenameAction">The rename action to execute.</param>
    public abstract void HandleExternalRename(RenameAction doRenameAction);

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value, or null if not found.</returns>
    public abstract string GetConfig(string key);
}