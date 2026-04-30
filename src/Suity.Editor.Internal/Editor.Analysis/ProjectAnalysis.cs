using Suity.Collections;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Analysis;

/// <summary>
/// Analyzes a project's structure, settings, and workspaces.
/// Provides access to project directories, configuration, and workspace analysis instances.
/// </summary>
public class ProjectAnalysis
{
    /// <summary>
    /// The file extension used for project files.
    /// </summary>
    public const string ProjectFileExtension = ".suity";

    private readonly string _projectBasePath;
    private readonly string _projectName;

    private ProjectSetting _setting;
    private readonly Dictionary<string, WorkSpaceAnalysis> _workSpaces = new(IgnoreCaseStringComparer.Instance);

    /// <summary>
    /// Gets the unique GUID identifier for this project.
    /// </summary>
    public Guid ProjectGuid => _setting.ProjectGuid;

    /// <summary>
    /// Gets the name of this project.
    /// </summary>
    public string ProjectName => _projectName;

    /// <summary>
    /// Gets the version string of this project.
    /// </summary>
    public string Version => _setting.Version;

    /// <summary>
    /// Gets the base directory path of this project.
    /// </summary>
    public string ProjectBasePath => _projectBasePath;

    /// <summary>
    /// Gets the full path to the project setting file.
    /// </summary>
    public string ProjectSettingFileName => Path.Combine(_projectBasePath, _projectName) + ProjectFileExtension;

    /// <summary>
    /// Gets the directory path where project assets are stored.
    /// </summary>
    public string AssetDirectory => GetSubDirectory(_setting.AssetDirectory);

    /// <summary>
    /// Gets the directory path where user-specific data is stored.
    /// </summary>
    public string UserDirectory => GetSubDirectory(_setting.UserDirectory);

    /// <summary>
    /// Gets the directory path where workspaces are stored.
    /// </summary>
    public string WorkSpaceDirectory => GetSubDirectory(_setting.WorkSpaceDirectory);

    /// <summary>
    /// Gets the directory path where system files are stored.
    /// </summary>
    public string SystemDirectory => GetSubDirectory(_setting.SystemDirectory);

    /// <summary>
    /// Gets the directory path where project plugins are stored.
    /// </summary>
    public string PluginsDirectory => GetSubDirectory(_setting.UserDirectory.PathAppend("Plugins"));

    /// <summary>
    /// Gets the directory path where published output is stored.
    /// </summary>
    public string PublishDirectory => GetSubDirectory(_setting.PublishDirectory);

    /// <summary>
    /// Gets the directory path where external assemblies are stored.
    /// </summary>
    public string AssembliesDirectory => GetSubDirectory(_setting.AssembliesDirectory);

    /// <summary>
    /// Gets a value indicating whether the project configuration is valid.
    /// </summary>
    public bool IsProjectValid { get; }



    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectAnalysis"/> class from a project file path.
    /// </summary>
    /// <param name="fileName">The full path to the project file.</param>
    public ProjectAnalysis(string fileName)
    {
        _setting = new ProjectSetting();

        _projectBasePath = Path.GetDirectoryName(fileName);
        _projectName = Path.GetFileNameWithoutExtension(fileName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectAnalysis"/> class with explicit base path and project name.
    /// </summary>
    /// <param name="projectBasePath">The base directory of the project.</param>
    /// <param name="projectName">The name of the project.</param>
    public ProjectAnalysis(string projectBasePath, string projectName)
    {
        if (string.IsNullOrEmpty(projectBasePath))
        {
            throw new ArgumentNullException(nameof(projectBasePath));
        }

        _setting = new ProjectSetting();
        _projectBasePath = projectBasePath;
        _projectName = projectName;
    }

    /// <summary>
    /// Cleans up the plugins directory by ensuring it exists and removing all existing contents.
    /// </summary>
    public void CleanUpPluginsDirectory()
    {
        var pluginDir = PluginsDirectory;
        DirectoryUtility.EnsureDirectory(pluginDir);
        DirectoryUtility.CleanUpDirectory(pluginDir);
    }

    /// <summary>
    /// Gets an enumerable collection of all workspace analysis instances in this project.
    /// </summary>
    public IEnumerable<WorkSpaceAnalysis> WorkSpaces => _workSpaces.Values;

    /// <summary>
    /// Gets a workspace analysis by name.
    /// </summary>
    /// <param name="name">The name of the workspace to retrieve.</param>
    /// <returns>The <see cref="WorkSpaceAnalysis"/> for the specified name, or null if not found.</returns>
    public WorkSpaceAnalysis GetWorkSpace(string name) => _workSpaces.GetValueSafe(name);

    /// <summary>
    /// Analyzes the project by loading settings and optionally discovering workspaces.
    /// </summary>
    /// <param name="workSpace">If true, discovers and analyzes all workspace directories under the workspace directory.</param>
    public void Analyze(bool workSpace)
    {
        string settingFileName = ProjectSettingFileName;
        if (!File.Exists(settingFileName))
        {
            return;
        }

        string str = TextFileHelper.ReadFile(settingFileName);
        _setting = JsonHelper.Deserialize<ProjectSetting>(str);

        if (string.IsNullOrEmpty(_setting.AssetDirectory))
        {
            return;
        }

        if (string.IsNullOrEmpty(_setting.UserDirectory))
        {
            return;
        }

        if (!Directory.Exists(WorkSpaceDirectory))
        {
            return;
        }

        if (workSpace)
        {
            var dirInfo = new DirectoryInfo(WorkSpaceDirectory);
            foreach (var workSpaceDir in dirInfo.GetDirectories())
            {
                EnsureWorkSpace(workSpaceDir.Name);
            }
        }
    }

    /// <summary>
    /// Ensures that a workspace with the specified name exists and is analyzed.
    /// Creates a new <see cref="WorkSpaceAnalysis"/> instance if one does not already exist.
    /// </summary>
    /// <param name="name">The name of the workspace to ensure.</param>
    /// <returns>The <see cref="WorkSpaceAnalysis"/> for the workspace, or null if the directory does not exist.</returns>
    private WorkSpaceAnalysis EnsureWorkSpace(string name)
    {
        string workSpacePath = WorkSpaceDirectory.PathAppend(name);

        if (!Directory.Exists(workSpacePath))
        {
            return null;
        }

        if (!_workSpaces.TryGetValue(name, out WorkSpaceAnalysis workSpace))
        {
            workSpace = new WorkSpaceAnalysis(this, name);
            workSpace.LoadConfig();
            _workSpaces.Add(name, workSpace);
        }

        return workSpace;
    }

    /// <summary>
    /// Combines the project base path with a subdirectory name to produce a full path.
    /// </summary>
    /// <param name="subDirectory">The subdirectory name to append to the project base path.</param>
    /// <returns>The full path to the subdirectory.</returns>
    private string GetSubDirectory(string subDirectory)
    {
        return Path.Combine(_projectBasePath, subDirectory);
    }
}
