using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Suity.Editor.Analysis;

/// <summary>
/// Analyzes a workspace within a project, providing access to workspace configuration,
/// directory paths, and plugin loading capabilities.
/// </summary>
public class WorkSpaceAnalysis : ISyncObject
{
    /// <summary>
    /// Gets the project analysis this workspace belongs to.
    /// </summary>
    public ProjectAnalysis Project { get; }

    /// <summary>
    /// Gets the name of this workspace.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the base directory path for this workspace by combining the project's workspace directory with the workspace name.
    /// </summary>
    public string BaseDirectory => Project.WorkSpaceDirectory.PathAppend(Name);

    /// <summary>
    /// Gets the full path to the workspace configuration file.
    /// </summary>
    public string ConfigFileName => BaseDirectory.PathAppend(WorkSpace.DefaultWorkSpaceConfigFileName);

    /// <summary>
    /// Gets a value indicating whether the workspace configuration has been successfully loaded and is valid.
    /// </summary>
    public bool IsWorkSpaceValid { get; private set; }

    /// <summary>
    /// Gets the name of the controller associated with this workspace.
    /// </summary>
    public string ControllerName { get; private set; }

    private string _baseNameSpace = string.Empty;
    private string _externalRPath = null;

    /// <summary>
    /// Gets the master directory path for this workspace.
    /// Returns the external path if configured and exists, otherwise returns the default master directory.
    /// </summary>
    public string MasterDirectory
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

            return configBasePath.PathAppend(WorkSpace.DefaultMasterDirectory);
        }
    }

    /// <summary>
    /// Gets the full path to the compiled plugin output assembly file.
    /// </summary>
    /// <returns>The full path to the plugin DLL file, or null if not applicable.</returns>
    public string GetPluginOutputFileName()
    {
        //OutputInfo info = GetOutputInfo();
        //if (info != null && !string.IsNullOrEmpty(info.OutputPath))
        //{
        //    string path = MasterDirectory.PathAppend(info.OutputPath).PathAppend(info.AssemblyName);
        //    if (info.OutputType == "Library")
        //    {
        //        path += ".dll";
        //    }
        //    else if (info.OutputType == "WinExe")
        //    {
        //        path += ".exe";
        //    }
        //    return path;
        //}
        //else
        //{
        //    return null;
        //}

        // Changed from net461 to netstandard2.0
        // return MasterDirectory.PathAppend($"bin/Debug/net461/{Name}.dll");
        return MasterDirectory.PathAppend($"bin/Debug/{TargetFrameworkConfig.Name_NetStandardDefault}/{Name}.dll");
    }

    /// <summary>
    /// Gets a value indicating whether this workspace is a plugin workspace.
    /// </summary>
    public bool IsPlugin => ControllerName == "Plugin";

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkSpaceAnalysis"/> class.
    /// </summary>
    /// <param name="project">The project analysis this workspace belongs to.</param>
    /// <param name="name">The name of this workspace.</param>
    public WorkSpaceAnalysis(ProjectAnalysis project, string name)
    {
        Project = project;
        Name = name;
    }

    /// <summary>
    /// Loads the plugin assembly for this workspace and copies build outputs to the plugins directory.
    /// Only works for workspaces where <see cref="IsPlugin"/> is true.
    /// </summary>
    /// <param name="collector">The collection to add the loaded plugin assembly to.</param>
    /// <returns>True if the plugin was successfully loaded; otherwise, false.</returns>
    public bool LoadPlugin(ICollection<Assembly> collector)
    {
        if (!IsPlugin)
        {
            return false;
        }

        //if (!ServiceInternals._license.GetCapability(EditorCapabilities.CustomPlugin))
        //{
        //    Logs.LogError(ServiceInternals._license.GetFailedMessage(EditorCapabilities.CustomPlugin));
        //    return false;
        //}

        string fileName = GetPluginOutputFileName();
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        try
        {
            string binPath = Path.GetDirectoryName(fileName);
            string pluginBinPath = Project.PluginsDirectory.PathAppend(Name);
            string pluginFileName = pluginBinPath.PathAppend(Path.GetFileName(fileName));

            if (!Directory.Exists(pluginBinPath))
            {
                Directory.CreateDirectory(pluginBinPath);
            }

            var binDir = new DirectoryInfo(binPath);
            if (!binDir.Exists)
            {
                return false;
            }

            foreach (var file in binDir.GetFiles())
            {
                string newFileName = pluginBinPath.PathAppend(file.Name);
                try
                {
                    file.CopyTo(newFileName, true);
                }
                catch (Exception fileErr)
                {
                    fileErr.LogError($"Copy file failed : {newFileName}");
                }
            }

            new PluginLoader(Name, pluginFileName).LoadAssembly(collector);
        }
        catch (Exception err)
        {
            err.LogError($"Failed to load plugin : {Name}");

            return false;
        }

        return true;
    }

    /// <summary>
    /// Constructs the full path to the project file (.csproj) for this workspace.
    /// </summary>
    /// <returns>The full path to the workspace's .csproj file.</returns>
    private string MakeProjectFileName()
    {
        return MasterDirectory.PathAppend(Name + ".csproj");
    }

    /// <summary>
    /// Loads and deserializes the workspace configuration from the config file.
    /// Sets <see cref="IsWorkSpaceValid"/> based on whether the configuration was loaded successfully.
    /// </summary>
    internal void LoadConfig()
    {
        IsWorkSpaceValid = false;

        if (!File.Exists(ConfigFileName))
        {
            return;
        }

        try
        {
            XmlSerializer.DeserializeFromFile(this, ConfigFileName);
            IsWorkSpaceValid = true;
        }
        catch (Exception err)
        {
            err.LogError($"Failed to analyze workspace config file:{Name}");
        }
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _baseNameSpace = sync.Sync("BaseNameSpace", _baseNameSpace);
        _externalRPath = sync.Sync("ExternalPath", _externalRPath);

        ControllerName = sync.Sync("ControllerName", ControllerName);
    }
}
