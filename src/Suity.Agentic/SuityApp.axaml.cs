using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Newtonsoft.Json;
using Suity.Editor.AIGC;
using Suity.Editor.Analysis;
using Suity.Editor.Documents.TypeEdit;
using Suity.Editor.Flows;
using Suity.Editor.Packaging;
using Suity.Editor.ProjectGui;
using Suity.Editor.Services;
using Suity.Editor.ViewModels;
using Suity.Editor.Views;
using Suity.Editor.Views.Startup;
using Suity.Editor.VirtualTree;
using Suity.Helpers;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor
{
    public partial class SuityApp : Application
    {
        public const string ProjectName = "Suity Agentic";
        public const string VersionCode = "2026.02.21";
        public const string GithubPage = "https://github.com/suitylab/Suity";

        private const string ConfigFileName = "EditorConfig.json";

        public static SuityApp Instance { get; private set; }


        public IDockThemeManager? ThemeManager { get; private set; }
        public DateTime StartTime { get; private set; }
        public string? StartupProjectLocation { get; private set; }
        public Thread? MainThread { get; private set; }

        /// <summary>
        /// Version number
        /// </summary>
        public string? ProductVersion { get; private set; }

        public EditorAppConfig AppConfig { get; private set; } = new();
        public EditorOfficialConfig OfficialConfig { get; private set; } = new();

        /// <summary>
        /// Documents forced to open initially
        /// </summary>
        public string? InitialOpenDocument { get; private set; }

        public override void Initialize()
        {
            Instance = this;

            ThemeManager = new DockFluentThemeManager();
            ThemeManager.SwitchPreset(4);

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{
            //    // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            //    // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            //    DisableAvaloniaDataAnnotationValidation();
            //    desktop.MainWindow = new SimpleWindow
            //    {
            //        DataContext = new SimpleWindowViewModel(),
            //    };
            //}
            
            // DockManager.s_enableSplitToWindow = true;

            switch (ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    CreateDesktopApplication(desktop);
                    break;
            }

            base.OnFrameworkInitializationCompleted();
#if DEBUG
            // this.AttachDevTools();
#endif
        }

        #region Create Window
        public void CreateDesktopApplication(IClassicDesktopStyleApplicationLifetime desktop)
        {
            StartTime = DateTime.UtcNow;
            MainThread = Thread.CurrentThread;

            if (desktop.Args?.Length > 0)
            {
                StartupProjectLocation = desktop.Args[0];
            }

            ProductVersion = typeof(SuityApp).Assembly?.GetName()?.Version?.ToString();

            SetupSystemLog();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AvaEditorDevice.Instance.Initialize();
            LoadAppConfig();

            //CreateTestDockWindow(desktop);

            CreateStartupWindow(desktop);
            //CreateMainWindow(desktop);
            //CreateSplashWindow(desktop);
            //CreateTestAvaEdit(desktop);
        }

        public void CreateMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = new MainWindowViewModel();

            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
#if DEBUG
            //mainWindow.AttachDockDebug(
            //    () => mainWindowViewModel.Layout!,
            //    new KeyGesture(Key.F11));
            //mainWindow.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif
            //mainWindow.Closing += (_, _) =>
            //{
            //    mainWindowViewModel.CloseLayout();
            //};

            //desktop.Exit += (_, _) =>
            //{
            //    mainWindowViewModel.CloseLayout();
            //};

            //// Set as explicit shutdown: this way the program continues to run even without a MainWindow
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            this.Window = mainWindow;
            //mainWindow.Show();
        }

        public void CreateStartupWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            this.Window = new StartupWindow();
        }

        public void CreateSimpleWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            //DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new SimpleWindow
            {
                DataContext = new SimpleWindowViewModel(),
            };
        }

        public void CreateSplashWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            //DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new SplashWindow();
        }

        public void CreateTestAvaEdit(IClassicDesktopStyleApplicationLifetime desktop)
        {
            //DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new TestAvaEdit();
        }


        public void CreateTestDockWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var factory = new Factory();

            // Prepare tools and document
            // Build with one fluent chain
            factory.Tool(out var leftTool, t => t.WithId("Tool1").WithTitle("Tool 1"))
                   .Tool(out var bottomTool, t => t.WithId("Tool2").WithTitle("Output"))
                   .Document(out var doc1, d => d.WithId("Doc1").WithTitle("Document 1"))
                .DocumentDock(out var documentDock, d => d
                    .WithId("Documents")
                    .WithIsCollapsable(false)
                    .WithCanCreateDocument(true))
                .ToolDock(out var leftPane, Alignment.Left, t => t
                    .WithId("LeftPane")
                    .AppendTool(leftTool)
                    .WithActiveDockable(leftTool))
                .ProportionalDockSplitter(out var split1)
                .ProportionalDockSplitter(out var split2)
                .ToolDock(out var bottomPane, Alignment.Bottom, t => t
                    .WithId("BottomPane")
                    .AppendTool(bottomTool)
                    .WithActiveDockable(bottomTool))
                .ProportionalDock(out var mainLayout, Orientation.Horizontal, d => d
                    .Add(leftPane, split1, documentDock, split2, bottomPane))
                .RootDock(out var root, r => r
                    .Add(mainLayout)
                    .WithDefaultDockable(mainLayout)
                    .WithActiveDockable(mainLayout));

            // Proportions
            leftPane.WithProportion(0.25);
            bottomPane.WithProportion(0.25);

            // Document factory and initial document
            if (documentDock is DocumentDock documentDockImpl)
            {
                documentDockImpl.DocumentFactory = () =>
                {
                    var index = documentDock.VisibleDockables?.Count ?? 0;
                    return new Document
                    {
                        Id = $"Doc{index + 1}",
                        Title = $"Document {index + 1}"
                    };
                };
            }

            documentDock.AddDocument(doc1);

            // Initialize and host in DockControl
            factory.InitLayout(root);

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = root,
            };

            desktop.MainWindow = new Window
            {
                Width = 800,
                Height = 600,
                Content = dockControl
            };
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

        #region Task

        public Window? Window
        {
            get
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    return desktop.MainWindow;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    if (ReferenceEquals(desktop.MainWindow, value))
                    {
                        return;
                    }

                    // 1. Get old window reference
                    var oldWindow = desktop.MainWindow;

                    // 2. Set MainWindow to null (unset)
                    // Or directly assign to new window: desktop.MainWindow = new AnotherWindow();
                    desktop.MainWindow = null;

                    // 3. Completely close and destroy the old window
                    oldWindow?.Close();

                    desktop.MainWindow = value;

                    value?.Show();
                }
            }
        }

        public void SetNextWindowAction(Action action)
        {
            QueuedAction.Do(() => 
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // Set to explicit shutdown: program continues running even without MainWindow
                    desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    // 1. Get old window reference
                    var oldWindow = desktop.MainWindow;

                    // 2. Set MainWindow to null (unset)
                    // Or directly assign to new window: desktop.MainWindow = new AnotherWindow();
                    desktop.MainWindow = null;

                    // 3. Completely close and destroy the old window
                    oldWindow?.Close();
                }

                action();
            });
        }

        #endregion

        #region Project

        private static ProjectLoader? _projectLoader;

        public async void OpenProject(string fileName, Guid? projectGuid = null, string? templateFileName = null, string? initialOpenDocument = null)
        {
            if (_projectLoader != null)
            {
                throw new InvalidOperationException();
            }

            this.Window = null;
            EditorServices.ProgressService.ShowProgressWindow();

            await Task.Delay(500);

            InitialOpenDocument = initialOpenDocument;

            EditorServices.SystemLog.AddLog("SuityApp opening project...");
            EditorServices.SystemLog.PushIndent();

            ServiceInternals.InitializeInternalSystems();

            typeof(IInternalEditorInitialize).GetDerivedTypes();

            //EditorObjectManager.Instance._watchingDisabled = true;
            string configFileName = AppContext.BaseDirectory.PathAppend(ProjectLoader.ConfigFileName);

            EditorServices.SystemLog.AddLog($"Load EditorConfig : {configFileName}");

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
                AvaEditorDevice.Instance.AddServiceProvider(e);
            };

            // Load project
            await _projectLoader.OpenProject(fileName, projectGuid);

            EditorServices.SystemLog.PopIndent();
            EditorServices.SystemLog.AddLog("SuityApp project opened.");

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
                EditorObjectManager.Instance.DoUnwatchedAction(() =>
                {
                    EditorRexes.EditorBeforeAwake.Invoke();
                });
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
                EditorObjectManager.Instance.DoUnwatchedAction(() =>
                {
                    EditorRexes.EditorAwake.Invoke();
                });
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
                EditorObjectManager.Instance.DoUnwatchedAction(() =>
                {
                    EditorRexes.EditorStart.Invoke();
                });
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

            SetHeartbeatEnabled(true);

            this.Window = null;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CreateMainWindow(desktop);
            }
        }

        public void CloseProject()
        {
            if (EditorObjectManager.Instance is null)
            {
                return;
            }

            EditorServices.SystemLog.AddLog("SuityApp closing project...");
            EditorServices.SystemLog.PushIndent();

            //EditorObjectManager.Instance._watchingDisabled = true;
            EditorObjectManager.Instance.DoUnwatchedAction(() =>
            {
                _projectLoader?.CloseProject();
            });

            EditorServices.SystemLog.PopIndent();
            EditorServices.SystemLog.AddLog("SuityApp project closed.");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
        }

        #endregion

        #region HeartBeat

        private static System.Threading.Timer? _heartbeatTimer;

        public static readonly TimeSpan HeartBeatDuration = TimeSpan.FromSeconds(10);

        public static void SetHeartbeatEnabled(bool enabled)
        {
            if (enabled)
            {
                if (_heartbeatTimer != null)
                {
                    return;
                }

                _heartbeatTimer = new System.Threading.Timer(OnHeartbeat, null, HeartBeatDuration, HeartBeatDuration);
            }
            else
            {
                if (_heartbeatTimer is null)
                {
                    return;
                }

                _heartbeatTimer.Dispose();
                _heartbeatTimer = null;
            }
        }

        private static void OnHeartbeat(object? state)
        {
            EditorRexes.HeartBeat.Invoke();
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

        //private static void DisableAvaloniaDataAnnotationValidation()
        //{
        //    // Get an array of plugins to remove
        //    var dataValidationPluginsToRemove =
        //        BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        //    // remove each entry found
        //    foreach (var plugin in dataValidationPluginsToRemove)
        //    {
        //        BindingPlugins.DataValidators.Remove(plugin);
        //    }
        //}

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
                typeof(SuityApp).Assembly, // Suity.Editor.WinformGui
                typeof(PropertyTarget).Assembly,  // Suity.Editor.ImGui
                typeof(ProjectViewPlugin).Assembly,
                typeof(VirtualNode).Assembly,
                typeof(TypeDesignDocument).Assembly,
                typeof(CorePlugin).Assembly,
                typeof(PackagerPlugin).Assembly,
                typeof(FlowPlugin).Assembly, // Suity.Editor.Flows
                typeof(BaseLLmCall).Assembly, // Suity.Editor.AIGC
                typeof(LLmModelPlugin).Assembly, // Suity.Editor.AIGC.LLm
                typeof(AigcWorkflowPlugin).Assembly, // Suity.Editor.AIGC.Flows
                typeof(BaseOpenAICall).Assembly, // Suity.Editor.AIGC.API
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
}