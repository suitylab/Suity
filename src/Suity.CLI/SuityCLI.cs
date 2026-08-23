using Newtonsoft.Json;
using Suity.Editor.AIGC;
using Suity.Editor.Analysis;
using Suity.Editor.DataModel;
using Suity.Editor.Documents.TypeEdit;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.VirtualTree;
using Suity.Helpers;
using Suity.Views.Im.PropertyEditing;
using System.Reflection;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor;

public class SuityCLI
{
    public const string ProductName = "Suity CLI";
    public const string VersionCode = "2026.05.10";
    public const string GithubPage = "https://github.com/suitylab/Suity";
    public const string ConfigFileName = "EditorConfig.json";

    public static SuityCLI Instance { get; } = new();


    public DateTime StartTime { get; private set; }
    public Thread? MainThread { get; private set; }
    public string? ProductVersion { get; private set; }
    public EditorAppConfig AppConfig { get; private set; } = new();


    private bool _init;
    private ProjectLoader? _projectLoader;



    public void Initialize()
    {
        if (_init)
        {
            return;   
        }
        _init = true;

        StartTime = DateTime.UtcNow;
        MainThread = Thread.CurrentThread;

        ProductVersion = typeof(SuityCLI).Assembly?.GetName()?.Version?.ToString();

        SetupSystemLog();

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Device.InitializeDevice(CliDevice.Instance);
        LoadAppConfig();
    }

    #region Project
    public async Task OpenProject(string fileName, Guid? projectGuid = null, string? templateFileName = null)
    {
        if (_projectLoader != null)
        {
            throw new InvalidOperationException("Project is already being loaded.");
        }

        Initialize();

        EditorServices.SystemLog.AddLog("Suity.CLI opening project...");
        EditorServices.SystemLog.PushIndent();

        ServiceInternals.InitializeInternalSystems();
        typeof(IInternalEditorInitialize).GetDerivedTypes();

        var asms = CollectCoreAssemblies();
        _projectLoader = new ProjectLoader
        {
            PluginAssemblies = asms,
            TemplateFileName = templateFileName,
        };

        _projectLoader.EditorStart += (s, e) => HandleEditorStart();
        _projectLoader.ProjectStart += (s, e) => HandleProjectStart(fileName);
        _projectLoader.ServiceProviderAdded += (s, e) =>
        {
            CliDevice.Instance.AddServiceProvider(e);
        };

        await QueuedAction.DoSuspendedAction(() => _projectLoader.OpenProject(fileName, projectGuid));

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Suity.CLI project opened.");

        // Save project open record
        AppConfig.AddProjectRecord(_projectLoader.ActiveProject.ProjectSettingFile);
        SaveAppConfig();
    }

    private void HandleEditorStart()
    {
        EditorServices.SystemLog.AddLog("EditorBeforeAwake event begin.");
        EditorServices.SystemLog.PushIndent();
        try
        {
            EditorObjectManager.Instance.DoUnwatchedAction(EditorRexes.EditorBeforeAwake.Invoke);
        }
        catch (Exception err)
        {
            err.LogError();
        }
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("EditorBeforeAwake event end.");

        EditorServices.SystemLog.AddLog("EditorAwake event begin.");
        EditorServices.SystemLog.PushIndent();
        try
        {
            EditorObjectManager.Instance.DoUnwatchedAction(EditorRexes.EditorAwake.Invoke);
        }
        catch (Exception err)
        {
            err.LogError();
        }
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("EditorAwake event end.");

        EditorServices.SystemLog.AddLog("EditorStart event begin.");
        EditorServices.SystemLog.PushIndent();
        try
        {
            EditorObjectManager.Instance.DoUnwatchedAction(EditorRexes.EditorStart.Invoke);
        }
        catch (Exception err)
        {
            err.LogError();
        }
        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("EditorStart event end.");
    }

    private void HandleProjectStart(string fileName)
    {
        //EditorObjectManager.Instance._watchingDisabled = false;

        // SetHeartbeatEnabled(true);
    }

    public void CloseProject()
    {
        if (EditorObjectManager.Instance is null)
        {
            return;
        }

        EditorServices.SystemLog.AddLog("Suity.CLI closing project...");
        EditorServices.SystemLog.PushIndent();

        //EditorObjectManager.Instance._watchingDisabled = true;
        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            _projectLoader?.CloseProject();
        });

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Suity.CLI project closed.");

    }

    #endregion

    #region Config

    public bool LoadAppConfig()
    {
        try
        {
            if (File.Exists(ConfigFileName))
            {
                var json = File.ReadAllText(ConfigFileName);
                var config = JsonConvert.DeserializeObject<EditorAppConfig>(json);
                if (config != null)
                {
                    AppConfig = config;

                    var lang = LocalizeManager.ParseLanguage(config.Language);
                    LocalizeManager.Instance.UpdateLanguage(lang, true);
                }
            }
            else
            {
                AppConfig.Language = "en";
                var lang = LocalizeManager.ParseLanguage(AppConfig.Language);
                LocalizeManager.Instance.UpdateLanguage(lang, true);
            }
        }
        catch (Exception err)
        {
            err.LogError(L("Failed to load application configuration file."));
        }

        return true;
    }

    public void SaveAppConfig()
    {
        if (AppConfig is null)
        {
            throw new NullReferenceException(nameof(AppConfig));
        }

        AppConfig.Language = LocalizeManager.Instance.LanguageCode;

        try
        {
            var json = JsonConvert.SerializeObject(AppConfig, Formatting.Indented);
            File.WriteAllText(ConfigFileName, json);
        }
        catch (Exception err)
        {
            err.LogError(L("Failed to save application configuration file."));
        }
    }

    #endregion

    #region Static

    private static void SetupSystemLog()
    {
        try
        {
            if (File.Exists("SystemLog.log"))
            {
                File.Delete("SystemLog.log");
            }

            EditorServices.SystemLog = new FileSystemLog("SystemLog.log");
        }
        catch (Exception)
        {
        }
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);

        {
            // Search main program folder

            string mainDllPath = AppDomain.CurrentDomain.BaseDirectory.PathAppend($"{name.Name}.dll");
            if (File.Exists(mainDllPath))
            {
                return Assembly.LoadFile(mainDllPath);
            }

            var mainExePath = Path.ChangeExtension(mainDllPath, "exe");
            if (File.Exists(mainExePath))
            {
                return Assembly.LoadFile(mainExePath);
            }
        }

        // Search modules folder
        string modulePath = AppDomain.CurrentDomain.BaseDirectory.PathAppend("Modules");
        var moduleDir = new DirectoryInfo(modulePath);
        if (moduleDir.Exists)
        {
            foreach (var dir in moduleDir.GetDirectories())
            {
                string path = dir.FullName;

                var dllPath = Path.Combine(path, $"{name.Name}.dll");
                if (File.Exists(dllPath))
                {
                    return Assembly.LoadFile(dllPath);
                }

                var exePath = Path.ChangeExtension(dllPath, "exe");
                if (File.Exists(exePath))
                {
                    return Assembly.LoadFile(exePath);
                }
            }
        }

        return null;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        //TODO:
    }


    private static ICollection<Assembly> CollectCoreAssemblies()
    {
        HashSet<Assembly> asms =
        [
            typeof(Asset).Assembly, // Suity.Editor
                typeof(SuityCLI).Assembly, // Suity.CLI
                typeof(PropertyTarget).Assembly,  // Suity.Editor.ImGui
                typeof(VirtualNode).Assembly,
                typeof(TypeDesignDocument).Assembly,
                typeof(CorePlugin).Assembly,
                typeof(FlowPlugin).Assembly, // Suity.Editor.Flows
                typeof(BaseLLmCall).Assembly, // Suity.Editor.AIGC
                typeof(LLmModelPlugin).Assembly, // Suity.Editor.AIGC.LLm
                typeof(AigcWorkflowPlugin).Assembly, // Suity.Editor.AIGC.Flows
                typeof(BaseOpenAICall).Assembly, // Suity.Editor.AIGC.API
                typeof(DataModelPlugin).Assembly, // Suity.Editor.DataModel
            ];

        string extPath = AppContext.BaseDirectory.PathAppend("Extensions");
        if (!Directory.Exists(extPath))
        {
            Directory.CreateDirectory(extPath);
        }

        foreach (string fileName in Directory.GetFiles(extPath))
        {
            string rFileName = Path.GetFileName(fileName);
            if (Path.GetExtension(rFileName).ToLower() == ".dll")
            {
                LoadExtensions(asms, rFileName, fileName);
            }
        }

        EditorServices.SystemLog.AddLog($"Assemlies in AppDomain:");
        EditorServices.SystemLog.PushIndent();

        var allAsms = AppDomain.CurrentDomain.GetAssemblies().ToList();
        allAsms.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

        foreach (var asm in allAsms)
        {
            EditorServices.SystemLog.AddLog(asm.FullName);
        }
        EditorServices.SystemLog.PopIndent();

        //EditorUtility.LogCore.LogDebug($"Types in Main Assembly:");
        //EditorUtility.LogCore.PushIndent();
        //foreach (var type in typeof(SuityApp).Assembly.GetExportedTypes())
        //{
        //    EditorUtility.LogCore.LogDebug(type.FullName);
        //}
        //EditorUtility.LogCore.PopIndent();

        return asms;
    }

    private static void LoadExtensions(HashSet<Assembly> asms, string rFileName, string fileName)
    {
        if (!File.Exists(fileName))
        {
            EditorServices.SystemLog.AddLog($"Extensions file not found : {rFileName}");

            Logs.LogError(L("Extension file not found: ") + $"{rFileName}.");
        }

        bool loaded = false;
        Exception? loadException = null;

        do
        {
            try
            {
                //if (exFileInfo != null)
                //{
                //    // Check file integrity
                //    var fileInfo = new FileInfo(fileName);
                //    if (exFileInfo.FileSize > 0 && fileInfo.Length != exFileInfo.FileSize)
                //    {
                //        break;
                //    }

                //    if (!string.IsNullOrWhiteSpace(exFileInfo.VerifyCode))
                //    {
                //        string s = CheckSumHelper.CalculateFileChecksumSHA256(fileName);
                //        string r2 = AesEncryptionHelper.Decrypt(exFileInfo.VerifyCode, ExtVarifyKey);
                //        if (s != r2)
                //        {
                //            break;
                //        }
                //    }
                //}

                EditorServices.SystemLog.AddLog($"Loading extensions : {rFileName} ...");

                Assembly assembly = Assembly.LoadFrom(fileName);
                asms.Add(assembly);

                EditorServices.SystemLog.AddLog($"Loaded extensions : {rFileName}.");

                loaded = true;
            }
            catch (Exception err)
            {
                EditorServices.SystemLog.AddLog($"Loaded extensions FAILED : {rFileName} : {err.Message}");

                loadException = err;
            }

        } while (false);


        if (!loaded)
        {
            if (loadException != null)
            {
                loadException.LogError(L("Failed to load extension: ") + $"{rFileName}.");
            }
            else
            {
                Logs.LogError(L("Failed to load extension: ") + $"{rFileName}.");
            }

            //if (File.Exists(fileName))
            //{
            //    try
            //    {
            //        File.Delete(fileName);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
        }
    } 
    #endregion
}
