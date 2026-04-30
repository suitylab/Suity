using LiteDB;
using Polenter.Serialization;
using Suity.Editor.Types;
using Suity.Editor.Documents;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Suity.Editor.WorkSpaces;
using Suity.Editor.Services;
using Suity.Editor.Analyzing;
using Suity;

namespace Suity.Editor.Analysis;

/// <summary>
/// Manages the lifecycle of a project including opening, loading plugins, starting the editor,
/// and closing. Coordinates initialization of native types, assets, plugins, and documents.
/// </summary>
public class ProjectLoader
{
    /// <summary>
    /// The name of the editor configuration file.
    /// </summary>
    public const string ConfigFileName = "EditorConfig.xml";

    /// <summary>
    /// The name of the user option file stored in binary XML format.
    /// </summary>
    public const string ProjectUserOptionFileName = "UserOption.bxml";

    /// <summary>
    /// Gets a value indicating whether documents should be pre-validated during project startup.
    /// </summary>
    public const bool PrevalidateDocument = false;

    private ProjectAnalysis _projectAnalysis;
    private ProjectBK _project;


    /// <summary>
    /// Gets the active project analysis for the currently loaded project.
    /// </summary>
    public ProjectAnalysis ActiveProjectAnalysis => _projectAnalysis;

    /// <summary>
    /// Gets the currently active project.
    /// </summary>
    public Project ActiveProject => _project;

    /// <summary>
    /// Gets or sets the collection of plugin assemblies to load.
    /// </summary>
    public ICollection<Assembly> PluginAssemblies { get; set; }

    /// <summary>
    /// The full path to the template file (.suitypackage).
    /// </summary>
    public string TemplateFileName { get; set; }

    /// <summary>
    /// Occurs when the editor UI is starting.
    /// </summary>
    public event EventHandler EditorStart;

    /// <summary>
    /// Occurs after the project has started.
    /// </summary>
    public event EventHandler ProjectStart;

    /// <summary>
    /// Occurs when a project is closed.
    /// </summary>
    public event EventHandler ProjectClosed;

    /// <summary>
    /// Occurs when a service provider is being configured.
    /// </summary>
    public event EventHandler<IServiceProvider> ServiceProviderAdded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectLoader"/> class.
    /// </summary>
    public ProjectLoader()
    {
    }

    /// <summary>
    /// Creates and configures a LiteDatabase instance for storing editor settings.
    /// Uses encrypted connection with custom BSON mapping.
    /// </summary>
    /// <returns>A configured <see cref="LiteDatabase"/> instance for editor settings.</returns>
    private LiteDatabase GetSettingDb()
    {
        var mapper = new BsonMapper()
        {
            EmptyStringToNull = false,
            SerializeNullValues = true,
            TrimWhitespace = false,
        };

        ConnectionString conn = new ConnectionString
        {
            Filename = "EditorSettings.db",
            Upgrade = true,
            Password = "sty.ed.db.common.50186-19482-63900-45674",
        };

        return new LiteDatabase(conn, mapper);
    }

    /// <summary>
    /// Opens a project by loading configuration, initializing native types, loading plugins,
    /// starting the editor UI, and finally starting the project itself.
    /// </summary>
    /// <param name="fileName">The full path to the project file (.suity).</param>
    /// <param name="projectGuid">Optional GUID for the project.</param>
    /// <exception cref="InvalidOperationException">Thrown when a project is already open or the file extension is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the file name is null or empty.</exception>
    public async Task OpenProject(string fileName, Guid? projectGuid)
    {
        if (_project != null)
        {
            throw new InvalidOperationException("Project is already open.");
        }

        PluginAssemblies ??= new HashSet<Assembly>();

        EditorServices.SystemLog.AddLog($"ProjectBuilder open project : {fileName}");

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException();
        }

        string basePath = Path.GetDirectoryName(fileName);
        string name = Path.GetFileNameWithoutExtension(fileName);

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
            //throw new DirectoryNotFoundException();
        }

        if (Path.GetExtension(fileName).ToLowerInvariant() != ProjectBK.ProjectFileExtension)
        {
            throw new InvalidOperationException();
        }

        EditorServices.SystemLog.PushIndent();
        _projectAnalysis = new ProjectAnalysis(basePath, name);
        _projectAnalysis.Analyze(true);

        _project = new ProjectBK(basePath, name, projectGuid);
        EditorServices.SystemLog.PopIndent();

        // 1) Initialize internal resources ** BackendProject must be created first, which includes creating ProjectIdResolver.
        // Only with ProjectIdResolver can objects with Id be generated.
        EditorServices.SystemLog.AddLog($"ProjectBuilder (1) Initialize native types...");
        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            // New adjustment: type reflection is moved forward because native type construction may use other native types.
            // NativeTypeReflector.Instance.Initialize(pluginAssemblies);
            NativeTypeExternalBK.Instance.Initialize();
        });

        EditorServices.SystemLog.AddLog($"ProjectBuilder (1) Register core icon...");
        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            new ResourceImageGroupAsset(CoreIcon.ResourceManager, "*CoreIcon");
        });

        // Before ProjectOpen, the project folder needs to be analyzed and plugins loaded,
        // because AssetLibrary construction automatically scans Assemblies and GetDerivedTypes.
        // 2) Internal plugins + project plugins load together
        EditorServices.SystemLog.AddLog($"ProjectBuilder (2) Load plugins...");
        EditorServices.SystemLog.PushIndent();
        LoadPlugins(_projectAnalysis);
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"ProjectBuilder (2) Finish load plugins.");

        EditorServices.SystemLog.AddLog($"ProjectBuilder (2) Initialize asset type bindings...");
        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            //AssetTypeBindings.Initialize();
        });

        EditorServices.SystemLog.AddLog($"ProjectBuilder (3) Instantiating IInitialize...");
        EditorServices.SystemLog.PushIndent();

        foreach (Type init in typeof(IInitialize).GetDerivedTypes().Where(o => o.IsClass && !o.IsAbstract))
        {
            try
            {
                Activator.CreateInstance(init);
                EditorServices.SystemLog.AddLog($"Initialize {init.FullName} ({init.Assembly.FullName}).");
            }
            catch (Exception)
            {
                Logs.LogError($"IInitialize FAILED : {init.FullName} ({init.Assembly.FullName}).");
            }
        }

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"ProjectBuilder (3) Finish start plugins.");

        EditorServices.SystemLog.AddLog($"ProjectBuilder (4) Start plugins...");
        EditorServices.SystemLog.PushIndent();
        StartPlugins(_projectAnalysis);
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"ProjectBuilder (4) Finish start plugins.");

        EditorServices.SystemLog.AddLog($"ProjectBuilder (4) Loading settings...");
        EditorServices.SystemLog.PushIndent();
        _project.LoadSetting();
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"ProjectBuilder (4) Finish loading settings.");

        EditorServices.SystemLog.AddLog($"ProjectBuilder (5) Start editor...");
        EditorServices.SystemLog.PushIndent();

        // 3) Start UI before project startup
        EditorStart?.Invoke(this, EventArgs.Empty);
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"ProjectBuilder (5) Finish start editor.");

        EditorServices.SystemLog.AddLog($"ProjectBuilder (6) Setup project...");
        EditorServices.SystemLog.PushIndent();

        // 4) Finally open the project
        await StartProject(_project);

        await EditorUtility.WaitForNextQueuedAction();

        PostStartProject(_project);

        ProjectStart?.Invoke(this, EventArgs.Empty);

        EditorServices.SystemLog.PopIndent();

        EditorServices.SystemLog.AddLog($"ProjectBuilder (6) project opened");
    }

    /// <summary>
    /// Loads all plugin assemblies from the project's workspace analyses.
    /// Cleans up the plugins directory before loading.
    /// </summary>
    /// <param name="projectAnalysis">The project analysis containing workspace definitions.</param>
    private void LoadPlugins(ProjectAnalysis projectAnalysis)
    {
        projectAnalysis.CleanUpPluginsDirectory();
        foreach (var pluginAnalysis in projectAnalysis.WorkSpaces.Where(o => o.IsPlugin))
        {
            pluginAnalysis.LoadPlugin(PluginAssemblies);
        }
    }

    /// <summary>
    /// Starts all loaded plugins using the PluginManager.
    /// </summary>
    /// <param name="projectAnalysis">The project analysis (unused but kept for consistency).</param>
    private void StartPlugins(ProjectAnalysis projectAnalysis)
    {
        EditorServices.SystemLog.PushIndent();

        if (PluginAssemblies is not { } plugins)
        {
            EditorServices.SystemLog.AddLog("No plugins to start.");
            return;
        }

        PluginManager.Instance.StartPlugins(plugins, p => ServiceProviderAdded?.Invoke(this, p));
        EditorServices.SystemLog.PopIndent();
    }

    /// <summary>
    /// Asynchronously starts the project by pre-opening, loading configuration,
    /// scanning documents, and invoking plugin awake/start methods.
    /// </summary>
    /// <param name="project">The project to start.</param>
    private async Task StartProject(ProjectBK project)
    {
        EditorServices.SystemLog.AddLog("Starting project...");

        EditorObjectManager.Instance.DoUnwatchedAction(project.PreOpenProject);

        var serializer = CreateBinarySerializer();
        string configFileName = GetUserOptionFileName(project);

        try
        {
            if (File.Exists(configFileName))
            {
                if (serializer.Deserialize(configFileName) is ProjectConfig config)
                {
                    project.SetConfigItems(config.PluginConfigs);
                }
            }
        }
        catch (Exception err)
        {
            err.LogError("Failed to read user file");
        }

        //EditorRexes.PushQueuedActions.Invoke();
        await EditorUtility.WaitForNextQueuedAction();

        Project.Current = project;
        EditorRexes.ProjectOpened?.Invoke(project);

        var plugins = EditorServices.PluginService.Plugins.Select(o => o.Plugin).ToArray();

        foreach (var plugin in plugins)
        {
            EditorServices.SystemLog.AddLog($"Plugin awake project : {plugin.Name}");
            EditorServices.SystemLog.PushIndent();
            try
            {
                EditorObjectManager.Instance.DoUnwatchedAction(plugin.AwakeProject);
            }
            catch (Exception err)
            {
                err.LogError($"Start plugin failed : {plugin.Name}");
            }
            EditorServices.SystemLog.PopIndent();
        }

        // Disable reference management to prevent cross-updates during loading
        EditorRexes.ReferenceManagerDisabled.Value = true;

        var templateFileName = TemplateFileName;
        if (!string.IsNullOrWhiteSpace(templateFileName) && File.Exists(templateFileName))
        {
            EditorServices.SystemLog.AddLog($"Importing template file...");
            EditorServices.SystemLog.PushIndent();
            await EditorUtility.ImportPackage(templateFileName);
            EditorServices.SystemLog.PopIndent();
            EditorServices.SystemLog.AddLog($"Finish importing template file.");
        }

        // Open all documents
        await project.ScanProjectDirectory();

        //await Task.Delay(100);
        await EditorUtility.WaitForNextQueuedAction();

        for (int i = 0; i < 10; i++)
        {
            EditorUtility.FlushDelayedActions();
            //EditorRexes.PushQueuedActions.Invoke();
        }

        foreach (var workSpace in WorkSpaceManager.Current.WorkSpaces)
        {
            workSpace.Controller?.TryWriteProjectFile();
        }

        List<Task> postTasks = null;

        foreach (var plugin in plugins)
        {
            EditorServices.SystemLog.AddLog($"Plugin start project : {plugin.Name}");
            EditorServices.SystemLog.PushIndent();
            try
            {
                var postTask = plugin.StartProject();
                if (postTask != null)
                {
                    (postTasks ??= []).Add(postTask);
                }
            }
            catch (Exception err)
            {
                err.LogError($"Post start plugin failed : {plugin.Name}");
            }
            EditorServices.SystemLog.PopIndent();
        }

        if (postTasks != null)
        {
            foreach (var postTask in postTasks)
            {
                Task.WaitAll(postTask);
            }
        }
    }

    /// <summary>
    /// Performs post-startup operations including document validation, reference updates,
    /// asset activation, and final project initialization.
    /// </summary>
    /// <param name="project">The project that was started.</param>
    private void PostStartProject(ProjectBK project)
    {
        for (int i = 0; i < 3; i++)
        {
            EditorUtility.FlushDelayedActions();
        }

        EditorRexes.ReferenceManagerDisabled.Value = false;
        EditorObjectManager.Instance.DoUnwatchedAction(ReferenceManager.Current.Update);

        if (PrevalidateDocument)
        {
            // Pre-validate documents
            Task analysisTask = EditorUtility.DoProgress("Validating documents...", p =>
            {
                AnalysisService analysis = Device.Current.GetService<AnalysisService>();
                var docs = DocumentManager.Instance.AllOpenedDocuments.Select(o => o.Content).OfType<ISupportAnalysis>().ToArray();

                int index = 0;

                Parallel.ForEach(docs, doc =>
                {
                    p.UpdateProgess(index + 1, docs.Length, string.Empty, string.Empty);

                    var option = new AnalysisOption { Intent = AnalysisIntents.Startup };

                    try
                    {
                        analysis.Analyze(docs[index], option);
                    }
                    catch (Exception err)
                    {
                        err.LogError();
                    }
                    Interlocked.Increment(ref index);
                });

                p.CompleteProgess();
            });

            Task.WaitAll(analysisTask);
        }

        for (int i = 0; i < 3; i++)
        {
            EditorUtility.FlushDelayedActions();
        }

        // Documents loaded initially may have incomplete references, which can cause default value retrieval to fail, so they need to be closed first.
        DocumentManager.Instance.CloseAllDocuments();

        // Since resource update notifications are blocked during loading, resources cannot reference each other.
        // So after all documents are loaded, reactivate all resources.
        AssetManagerBK.Instance.StartUp();


        // Push delayed actions
        for (int i = 0; i < 10; i++)
        {
            EditorUtility.FlushDelayedActions();
        }

        project.PostOpenProject();

        EditorServices.SystemLog.AddLog("Project started.");
    }

    /// <summary>
    /// Closes the currently open project, stopping all plugins, closing documents,
    /// saving user configuration, and cleaning up resources.
    /// </summary>
    public void CloseProject()
    {
        if (_project is null)
        {
            DocumentManager.Instance.CloseAllDocuments();
            return;
        }

        foreach (var editorPlugin in EditorServices.PluginService.Plugins.Select(o => o.Plugin))
        {
            try
            {
                editorPlugin.StopProject();
            }
            catch (Exception err)
            {
                err.LogError($"Stop plugin failed : {editorPlugin.Name}");
                continue;
            }
        }

        DocumentManager.Instance.CloseAllDocuments();

        if (_project != null)
        {
            ProjectClosed?.Invoke(this, EventArgs.Empty);
            EditorRexes.ProjectClosing.Invoke(_project);
        }

        // Pass back user data from InspectorContext
        EditorUtility.Inspector.InspectObject(null);

        var config = new ProjectConfig();
        _project.GetConfigItems(config.PluginConfigs);
        var serializer = CreateBinarySerializer();
        string configFileName = GetUserOptionFileName(_project);

        try
        {
            serializer.Serialize(config, configFileName);
        }
        catch (Exception err)
        {
            err.LogError("Failed to write user file");
        }

        _project.CloseProject();
        _project = null;
        _projectAnalysis = null;
    }

    /// <summary>
    /// Gets the full path to the user option file for the specified project.
    /// </summary>
    /// <param name="project">The project to get the user option file path for.</param>
    /// <returns>The full path to the user option file.</returns>
    private string GetUserOptionFileName(Project project)
    {
        return Path.Combine(project.UserDirectory, ProjectUserOptionFileName);
    }

    /// <summary>
    /// Creates a SharpSerializer instance configured for size-optimized binary serialization.
    /// </summary>
    /// <returns>A configured <see cref="SharpSerializer"/> instance.</returns>
    private SharpSerializer CreateBinarySerializer()
    {
        SharpSerializerBinarySettings settings = new(BinarySerializationMode.SizeOptimized);

        return new SharpSerializer(settings);
    }
}
