using Suity.Helpers;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Workspace controller
/// </summary>
public abstract class WorkSpaceController
{
    /// <summary>
    /// Controller planet identifier
    /// </summary>
    public const string CONTROLLER_PLANET = "Planet";
    /// <summary>
    /// Controller satelite identifier
    /// </summary>
    public const string CONTROLLER_SATELITE = "Satelite";
    /// <summary>
    /// Controller spaceship identifier
    /// </summary>
    public const string CONTROLLER_SPACESHIP = "SpaceShip";
    /// <summary>
    /// Controller astronaut identifier
    /// </summary>
    public const string CONTROLLER_ASTRONAUT = "Astronaut";
    /// <summary>
    /// Controller alien identifier
    /// </summary>
    public const string CONTROLLER_ALIEN = "Alien";
    /// <summary>
    /// Controller plugin identifier
    /// </summary>
    public const string CONTROLLER_PLUGIN = "Plugin";

    /// <summary>
    /// Gets all registered controller infos
    /// </summary>
    public static WorkSpaceControllerInfo[] ControllerInfos
        => WorkSpacesExternal._external.ControllerInfos;

    /// <summary>
    /// Gets controller info by name
    /// </summary>
    /// <param name="name">Controller name</param>
    /// <returns>Controller info</returns>
    public static WorkSpaceControllerInfo GetControllerInfo(string name)
        => WorkSpacesExternal._external.GetControllerInfo(name);

    /// <summary>
    /// Gets controller info by type
    /// </summary>
    /// <param name="type">Controller type</param>
    /// <returns>Controller info</returns>
    public static WorkSpaceControllerInfo GetControllerInfo(Type type)
        => WorkSpacesExternal._external.GetControllerInfo(type);

    /// <summary>
    /// Gets controller info by generic type
    /// </summary>
    /// <typeparam name="T">Controller type</typeparam>
    /// <returns>Controller info</returns>
    public static WorkSpaceControllerInfo GetControllerInfo<T>() where T : WorkSpaceController
        => WorkSpacesExternal._external.GetControllerInfo<T>();

    /// <summary>
    /// Workspace
    /// </summary>
    public WorkSpace WorkSpace { get; internal set; }

    /// <summary>
    /// Whether workspace is started
    /// </summary>
    public virtual bool IsSpaceShip => false;

    /// <summary>
    /// Gets output info
    /// </summary>
    /// <returns>Output info</returns>
    public virtual OutputInfo GetOutputInfo() => null;

    /// <summary>
    /// Gets output info
    /// </summary>
    /// <param name="framwork">Target framework</param>
    /// <returns>Output info</returns>
    public virtual OutputInfo GetOutputInfo(string framwork) => null;

    /// <summary>
    /// Gets whether the specified path needs to be hidden
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if path should be hidden</returns>
    public virtual bool GetIsPathHidden(string path) => false;

    /// <summary>
    /// Gets whether the specified path can be exported
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if can be exported</returns>
    public virtual bool CanExport(string path) => true;

    /// <summary>
    /// Gets whether assembly reference is enabled
    /// </summary>
    public virtual bool AssemblyReferenceEnabled => false;

    /// <summary>
    /// Gets or sets whether the project is dirty
    /// </summary>
    public bool IsProjectDirty { get; internal set; }

    /// <summary>
    /// Gets whether the specified assembly reference can be added
    /// </summary>
    /// <param name="assemblyReference">Assembly reference</param>
    /// <returns>True if can be added</returns>
    public virtual bool CanAddAssemlbyReference(IAssemblyReference assemblyReference) => false;

    /// <summary>
    /// Gets the project file name
    /// </summary>
    /// <returns>Project file name</returns>
    public virtual string GetProjectFileName() => null;

    /// <summary>
    /// Gets the controller order
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Attempts to write the project file
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryWriteProjectFile()
    {
        try
        {
            WriteProjectFile();
            return true;
        }
        catch (Exception err)
        {
            err.LogError($"Failed to write project file:{WorkSpace?.Name}");
            return false;
        }
    }

    /// <summary>
    /// Attempts to migrate the project file
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryMigrateProjectFile()
    {
        try
        {
            MigrateProjectFile();
            return true;
        }
        catch (Exception err)
        {
            err.LogError($"Failed to migrate project file:{WorkSpace?.Name}");
            return false;
        }
    }

    /// <summary>
    /// Writes project file
    /// </summary>
    public virtual void WriteProjectFile()
    { }

    /// <summary>
    /// Migrates the project file
    /// </summary>
    public virtual void MigrateProjectFile() => WriteProjectFile();

    /// <summary>
    /// Gets full output folder path
    /// </summary>
    /// <returns>Full output path</returns>
    public string GetOutputFullPath()
    {
        OutputInfo info = GetOutputInfo();
        if (info != null && !string.IsNullOrEmpty(info.OutputPath))
        {
            return WorkSpace.MasterDirectory.PathAppend(info.OutputPath);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets full output folder path
    /// </summary>
    /// <param name="framework">Target framework</param>
    /// <returns>Full output path</returns>
    public string GetOutputFullPath(string framework)
    {
        OutputInfo info = GetOutputInfo(framework);
        if (info != null && !string.IsNullOrEmpty(info.OutputPath))
        {
            return WorkSpace.MasterDirectory.PathAppend(info.OutputPath);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Workspace started
    /// </summary>
    protected internal virtual void OnStart()
    { }

    /// <summary>
    /// Workspace stopped
    /// </summary>
    protected internal virtual void OnStop()
    { }

    /// <summary>
    /// Workspace updated
    /// </summary>
    protected internal virtual void OnUpdate()
    { }

    /// <summary>
    /// Workspace newly created
    /// </summary>
    protected internal virtual void OnNew()
    { }

    /// <summary>
    /// Workspace removed
    /// </summary>
    protected internal virtual void OnRemove()
    { }

    /// <summary>
    /// Workspace renamed
    /// </summary>
    /// <param name="oldName">Old name</param>
    protected internal virtual void OnRenamed(string oldName)
    { }

    /// <summary>
    /// Workspace master path changed
    /// </summary>
    protected internal virtual void OnMasterPathChanged()
    { }

    /// <summary>
    /// Workspace executes rendering
    /// </summary>
    /// <param name="addedFiles">Added files</param>
    /// <param name="removedFiles">Removed files</param>
    /// <param name="incremental">Whether incremental</param>
    protected internal virtual void OnRender(IEnumerable<string> addedFiles, IEnumerable<string> removedFiles, bool incremental)
    { }

    /// <summary>
    /// Notifies workspace controller configuration has changed
    /// </summary>
    protected void NotifyConfigChanged()
    {
        try
        {
            WorkSpace?.OnControllerConfigUpdated();
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Notifies that the project has changed
    /// </summary>
    protected void NotifyProjectChanged()
    {
        IsProjectDirty = true;

        NotifyConfigChanged();
    }
}