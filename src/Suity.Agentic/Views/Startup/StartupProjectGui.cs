using Newtonsoft.Json;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Analysis;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Views.Startup;

internal class StartupProjectGui : IDrawImGuiNode, IDisposable
{
    public static BitmapDef BmpUsageTemplate { get; } = Suity.Editor.Properties.Resources.UsageTemplate.ToBitmap();
    public static BitmapDef BmpUsageExample { get; } = Suity.Editor.Properties.Resources.UsageExample.ToBitmap();
    public static BitmapDef BmpUsageLearning { get; } = Suity.Editor.Properties.Resources.UsageLearning.ToBitmap();
    public static BitmapDef BmpUsagePreset { get; } = Suity.Editor.Properties.Resources.UsagePreset.ToBitmap();
    public static BitmapDef BmpCloudDownload { get; } = Suity.Editor.Properties.Resources.CloudDownload.ToBitmap();

    private enum Pages
    {
        None,
        DownloadExtensions,
        OpenProject,
        CreateProject
    }

    private readonly ImGuiNodeRef _guiRef = new();
    private Pages _page = Pages.OpenProject;
    private StartupHelper.DownloadFileInfo? _downloadFileInfo;

    private bool _guiEnabled = true;

    private string? _newFolderName;
    private string? _newProjectName;

    private bool _hasNewVersion;
    private bool _legacyVersion;

    private readonly Dictionary<string, BitmapDef> _templateImgs = [];

    private readonly PropertyTarget _languageTarget;

    private DrawImGui? _drawNotice;


    public event EventHandler? ProjectOpened;
    public event EventHandler? Logout;

    public StartupProjectGui()
    {
        string? lastProject = SuityApp.Instance.AppConfig.LastProjects.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(lastProject))
        {
            try
            {
                string lastProjectDir = Path.GetDirectoryName(lastProject);
                _newFolderName = Directory.GetParent(lastProjectDir)?.FullName;
            }
            catch (Exception)
            {
                // Do nothing.
            }
        }

        if (string.IsNullOrWhiteSpace(_newFolderName))
        {
            _newFolderName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }

        _newProjectName = "MySuityProject";

        var lang = LocalizeManager.Instance.ParseCurrentLanguage();
        _languageTarget = PropertyTargetUtility.CreatePropertyTarget(lang);

        UpdateProductConfig();
    }

    public ImGuiNode OnNodeGui(ImGui gui)
    {
        var node = _guiRef.Node = gui.Frame("#Project")
        .InitTheme(StartupStyles.Instance.Theme)
        .InitFullSize()
        .OnContent(() =>
        {
            gui.OverlayLayout()
            .InitFullWidth()
            .InitHeight(70)
            .OnContent(() =>
            {
                gui.Frame()
                .InitClass("formHeader")
                .InitFullWidth()
                .OnContent(() =>
                {
                    gui.VerticalLayout()
                    .InitWidth(180)
                    .InitPadding(10)
                    .OnContent(() =>
                    {
                        gui.Image(StartupStyles.Instance.Icon, true)
                        .InitCenter();
                    });

                    gui.SwitchButton("btnProject", L("Open Project"))
                    .InitClass("navBtn")
                    .InitOptionActive()
                    .OnClick(() => GotoPage(Pages.OpenProject));

                    gui.SwitchButton("btnNews", L("New Project"))
                    .InitClass("navBtn")
                    .OnClick(() => GotoPage(Pages.CreateProject));
                });

                gui.HorizontalReverseLayout()
                .InitPadding(5)
                .InitFullHeight()
                .InitFullWidth()
                .OnContent(() =>
                {
                    gui.HorizontalLayout()
                    .InitWidth(150)
                    .InitHeight(30)
                    .InitCenterVertical()
                    .OnContent(() =>
                    {
/*                        gui.PropertyEditor(_languageTarget, act =>
                        {
                            act.DoAction();

                            string? language = _languageTarget.GetValues().First()?.ToString()?.Replace('_', '-');
                            LocalizeManager.Instance.UpdateLanguage(language, true);
                            _guiRef.QueueRefresh(true);
                        });*/
                    });

                });
            });

            gui.VerticalLayout("page")
            .InitFullWidth()
            .SetHeightRest(30)
            .SetEnabled(_guiEnabled)
            .OnContent(() =>
            {
                switch (_page)
                {
                    case Pages.DownloadExtensions:
                        DownloadExtensionsGui(gui);
                        break;

                    case Pages.OpenProject:
                        OpenProjectGui(gui);
                        break;

                    case Pages.CreateProject:
                        CreateProjectGui(gui);
                        break;
                }
            });

            if (_drawNotice is { } drawNotice)
            {
                drawNotice(gui);
            }
            else
            {
                DrawFooter(gui);
            }
        });

        return node;
    }

    #region DownloadExtensions

    private void DownloadExtensionsGui(ImGui gui)
    {
        gui.OverlayLayout("#download")
        .InitFullSize()
        .OnContent(() => 
        {
            gui.VerticalLayout()
            .InitFit()
            .InitCenter()
            .OnContent(() => 
            {
                if (_downloadFileInfo != null)
                {
                    gui.Text(L($"File updating...({_downloadFileInfo.Index + 1}/{_downloadFileInfo.Total})"));
                }
            });
        });
    }

    #endregion

    #region Open Project

    private void OpenProjectGui(ImGui gui)
    {
        gui.Frame("#OpenProject")
        .InitTheme(StartupStyles.Instance.Theme)
        .InitClass("formBody")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.Frame("bodyHeader")
            .InitClass("bodyHeader")
            .InitFullWidth()
            .OnContent(() =>
            {
                gui.HorizontalLayout()
                .InitFullWidth()
                .OnContent(() =>
                {
                    gui.Text(L("Recent Projects")).InitClass("smallText").InitCenter();
                });

                gui.HorizontalReverseLayout()
                .InitFullWidth()
                .OverrideChildSpacing(20)
                .OnContent(() =>
                {
                    gui.Button(L("Import")).InitClass("toolBtn").InitCenter()
                    .OnClick(async () =>
                    {
                        string fileName = await DialogUtility.ShowOpenFileAsync("*.suity|*.suity", AppContext.BaseDirectory);

                        HandleOpenProject(fileName);
                    });
                });
            });

            gui.HorizontalLine().InitFullWidth().SetHeight(1);
            gui.ScrollableFrame(GuiOrientation.Vertical)
            .InitClass("projectFrame")
            .SetSizeRest()
            .OnContent(() =>
            {
                var config = SuityApp.Instance.AppConfig;

                for (int i = 0; i < config.LastProjects.Count; i++)
                {
                    var projectFileName = config.LastProjects[i];
                    ConfigLastProjectItem(gui, i, projectFileName);
                }
            });
        });
    }

    private void ConfigLastProjectItem(ImGui gui, int i, string projectFileName)
    {
        string title = Path.GetFileNameWithoutExtension(projectFileName);

        gui.OverlayFrame($"item{i}")
        .InitClass("projectItem")
        .OnContent(() =>
        {
            if (!File.Exists(projectFileName))
            {
                gui.HorizontalReverseLayout().InitFullSize().InitPadding(10).OnContent(() =>
                {
                    gui.Image("#not_exist", CoreIconCache.Warning).InitClass("icon").InitCenterVertical();
                });
            }

            var btn = gui.VerticalButton("btnName")
            .InitClass("transButton")
            .InitVerticalLayout(true)
            .InitPseudoAffectsChildren(true)
            .InitFullWidth()
            .OnContent(() =>
            {
                gui.Text(title).InitClass("projectTitle");
                gui.Text(projectFileName).InitClass("projectText2");
                //gui.Text("5.6").InitClass("projectText2");
            })
            .OnClick(() =>
            {
                if (!string.IsNullOrEmpty(projectFileName))
                {
                    HandleOpenProject(projectFileName);
                }
            })
            .InitInputMouseInSync();

            if (btn.IsMouseIn)
            {
                gui.HorizontalReverseLayout()
                .InitFullSize()
                .OnContent(() =>
                {
                    gui.Button(L("Open in Explorer")).InitClass("toolBtn").InitCenterVertical()
                    .OnClick(() =>
                    {
                        var itemNavi = Suity.Collections.CollectionExtensions.GetListItemSafe<string>(SuityApp.Instance.AppConfig.LastProjects, i);
                        if (!string.IsNullOrEmpty(itemNavi))
                        {
                            TextFileHelper.NavigateFolder(Path.GetDirectoryName(itemNavi));
                        }
                    });

                    gui.Button(L("Remove")).InitClass("toolBtn").InitCenterVertical()
                    .OnClick(async () =>
                    {
                        var result = await DialogUtility.ShowYesNoDialogAsyncL($"Confirm remove {title}?");
                        if (result)
                        {
                            SuityApp.Instance.AppConfig.LastProjects.RemoveAt(i);
                            _guiRef.QueueRefresh();
                        }
                    });
                });
            }
        });
    }

    private async void HandleOpenProject(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        if (!File.Exists(fileName))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Project file does not exist");
            return;
        }

        var projectAnalysis = new ProjectAnalysis(fileName);

        try
        {
            projectAnalysis.Analyze(false);
        }
        catch (Exception)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Failed to open project file");
            return;
        }

        if (projectAnalysis.Version != ServiceInternals.License.ProductVersion)
        {
            string msg = L($"The project version ({projectAnalysis.Version}) differs from this editor version ({ServiceInternals.License.ProductVersion}). Continue loading the project?");
            bool confirm = await DialogUtility.ShowYesNoDialogAsync(msg);
            if (!confirm)
            {
                return;
            }
        }

        SuityApp.Instance.SetNextWindowAction(() => SuityApp.Instance.OpenProject(fileName));

        ProjectOpened?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Create Project

    private ProductConfig? _productConfig;
    private List<ProjectTemplateInfo>? _projectTemplates;
    private ProjectTemplateInfo? _selectedTemplate;

    private void CreateProjectGui(ImGui gui)
    {
        gui.HorizontalFrame("#CreateProject")
        .InitClass("formBody")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.VerticalLayout()
            .InitFullHeight()
            .InitWidthRest(350)
            .OnContent(() =>
            {
                gui.VerticalLayout("#templates")
                .InitFullSize()
                .OnContent(() =>
                {
                    if (_projectTemplates != null)
                    {
                        gui.HorizontalLine().InitFullWidth().SetHeight(1);
                        gui.ScrollableFrame(GuiOrientation.Vertical)
                        .SetSizeRest()
                        .OnContent(() =>
                        {
                            int num = 0;
                            foreach (var template in _projectTemplates)
                            {
                                ConfigProjectTemplate(gui, num, template);
                                num++;
                            }
                        });
                    }
                    else
                    {
                        gui.OverlayLayout("#loading")
                        .InitFullSize()
                        .OnContent(() =>
                        {
                            gui.Text(L("Loading..."))
                            .InitCenter();
                        });
                    }
                });
            });

            gui.HorizontalResizer().InitWidth(5).InitFullHeight();

            gui.VerticalLayout()
            .InitWidthRest()
            .InitFullHeight()
            .InitPadding(10)
            .SetEnabled(_guiEnabled)
            .OnContent(() =>
            {
                gui.Text(L("Folder"));
                gui.HorizontalLayout().InitFullWidth().OnContent(() =>
                {
                    _newFolderName = gui.StringInput("#folder", _newFolderName).InitWidthRest(60).Text;
                    gui.Button(L("Browse")).InitClass("toolBtn").InitWidthRest().OnClick(async () =>
                    {
                        var result = await DialogUtility.ShowOpenFolderAsync(_newFolderName);
                        if (result != null)
                        {
                            _newFolderName = result;
                            _guiRef.QueueRefresh();
                        }
                    });
                });

                gui.Text(L("Project Name"));
                _newProjectName = gui.StringInput("#project", null, _newProjectName).InitFullWidth().Text;
                gui.VerticalLayout().InitHeight(30);
                gui.Button(L("Create"))
                .InitClass("toolBtn")
                .InitWidth(100)
                .InitCenter()
                .OnClick(() =>
                {
                    HandleNewProject();
                });
            });
        });
    }


    private void ConfigProjectTemplate(ImGui gui, int i, ProjectTemplateInfo template)
    {
        gui.HorizontalButton($"item{i}")
        .OnInitialize(n =>
        {
            n.SetClass("projectItem");
            n.IsDoubleLayout = true;
        })
        .SetPseudoActive(template.Id == _selectedTemplate?.Id)
        .OnContent(() =>
        {
            switch (template.TemplateUsage)
            {
                case ProjectTemplateUsages.Template:
                    gui.Image(BmpUsageTemplate).InitClass()
                    .InitClass("icon32")
                    .SetToolTipsL("Template");
                    break;

                case ProjectTemplateUsages.Example:
                    gui.Image(BmpUsageExample).InitClass()
                    .InitClass("icon32")
                    .SetToolTipsL("Example");
                    break;

                case ProjectTemplateUsages.Learning:
                    gui.Image(BmpUsageLearning).InitClass()
                    .InitClass("icon32")
                    .SetToolTipsL("Learning");
                    break;

                case ProjectTemplateUsages.Preset:
                default:
                    gui.Image(BmpUsagePreset).InitClass()
                    .InitClass("icon32")
                    .SetToolTipsL("Preset");
                    break;
            }

            gui.VerticalFrame("space").InitWidth(10);

            var btn = gui.VerticalFrame("btnName")
            .InitClass("transButton")
            .InitPseudoAffectsChildren(true)
            .InitWidthRest(40)
            .InitFullHeight()
            .OnContent(() =>
            {
                gui.Text(template.GetTitle()).InitClass("templateText");
                gui.Text(template.Description).InitClass("templateText2");
                //gui.Text(item.Pseudo);
            });

            gui.HorizontalLayout()
            .InitWidthRest()
            .OnContent(() =>
            {
                if (!string.IsNullOrWhiteSpace(template.DownloadUrl))
                {
                    var templateDir = SuityApp.Instance.OfficialConfig.ProjectTemplateDirectory;
                    if (!Directory.Exists(templateDir))
                    {
                        Directory.CreateDirectory(templateDir);
                    }

                    string tFileNameR = Path.GetFileName(template.DownloadUrl);
                    string tFileName = templateDir.PathAppend(tFileNameR);

                    if (!File.Exists(tFileName))
                    {
                        gui.Image("CloudDownload", BmpCloudDownload)
                        .InitClass("icon32")
                        .SetToolTipsL("File not downloaded");
                    }
                }
            });
        })
        .OnClick(() =>
        {
            _selectedTemplate = template;
            _guiRef.QueueRefresh();
        });
    }

    private async void HandleNewProject()
    {
        string baseDir = _newFolderName;
        string projectName = _newProjectName;
        string dir = Path.Combine(baseDir, projectName);
        string fileName = Path.Combine(dir, projectName) + ".suity";

        if (string.IsNullOrEmpty(baseDir))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Please fill in the folder");
            return;
        }

        if (string.IsNullOrEmpty(projectName))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Please fill in the project name");
            return;
        }

        if (!StringCharVarifier.WordVarifier.Varify(projectName))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Invalid project name");
            return;
        }

        if (Directory.Exists(dir))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Project folder already exists");
            return;
        }

        if (File.Exists(fileName))
        {
            await DialogUtility.ShowMessageBoxAsyncL("Project file already exists");
            return;
        }

        SetControlsEnabled(false);

        string templateFileName = null;
        var template = _selectedTemplate;
        if (template != null && !string.IsNullOrWhiteSpace(template.DownloadUrl))
        {
            try
            {
                string templateUrl = SuityApp.Instance.OfficialConfig.ProjectTemplateBaseUrl
                    .UrlAppend(template.DownloadUrl);

                var templateDir = SuityApp.Instance.OfficialConfig.ProjectTemplateDirectory;
                if (!Directory.Exists(templateDir))
                {
                    Directory.CreateDirectory(templateDir);
                }

                string tFileNameR = Path.GetFileName(template.DownloadUrl);
                templateFileName = templateDir.PathAppend(tFileNameR);

                if (!File.Exists(templateFileName))
                {
                    await StartupHelper.DownloadFileAsync(templateUrl, templateFileName);
                }
            }
            catch (Exception)
            {
                await DialogUtility.ShowMessageBoxAsyncL("Failed to download template file.");
                SetControlsEnabled(true);
                return;
            }
        }

        Guid projectGuid = Guid.Empty;

        try
        {
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        catch (Exception)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Failed to create folder");
            SetControlsEnabled(true);
            return;
        }

        ProjectOpened?.Invoke(this, EventArgs.Empty);

        SuityApp.Instance.SetNextWindowAction(
            () => SuityApp.Instance.OpenProject(fileName, projectGuid, templateFileName, template?.InitialOpenDocument));
    }


    #endregion

    private void GotoPage(Pages page)
    {
        if (_page == page)
        {
            return;
        }

        _page = page;

        switch (page)
        {
            case Pages.OpenProject:
                break;

            case Pages.CreateProject:
                UpdateProjectTemplates();
                break;
        }

        _guiRef.QueueRefresh();
    }

/*    private void CheckUpdate()
    {
        _hasNewVersion = false;
        _legacyVersion = false;

        if (_productConfig is null)
        {
            return;
        }

        _hasNewVersion = VersionHelper.CompareVersions(_productConfig.LatestVersion, SuityApp.Instance.ProductVersion) > 0;
        _legacyVersion = VersionHelper.CompareVersions(_productConfig.MinVersion, SuityApp.Instance.ProductVersion) > 0;

        _guiRef.QueueRefresh();
    }*/

    private async void UpdateProjectTemplates()
    {
        if (_projectTemplates != null)
        {
            return;
        }

        var config = await DownloadStartupConfig();
        if (config is null)
        {
            _projectTemplates = null;

            _drawNotice = gui => 
            {
                string msg = "Project template download failed.";
                ShowNotice(gui, msg, "Retry", () => 
                {
                    _drawNotice = null;
                    UpdateProjectTemplates();
                    _guiRef.QueueRefresh();
                });
            };

            _guiRef.QueueRefresh();
            return;
        }

        var filtered = config.ProjectTemplates.SkipNull().Where(o => !string.IsNullOrWhiteSpace(o.Id));
        _projectTemplates = [.. filtered];
        if (_projectTemplates.Count > 0)
        {
            _selectedTemplate = _projectTemplates[0];
        }

        _guiRef.QueueRefresh();
    }

    private async Task<ProjectStartupConfig> DownloadStartupConfig()
    {
        string url = "https://storage.suitylab.com/SuityAgentic/StartupConfig.json";

        try
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync(url);
            var config = JsonConvert.DeserializeObject<ProjectStartupConfig>(json);
            return config;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to download startup config: {ex.Message}");
            return null;
        }
    }


    private async void UpdateProductConfig()
    {
        if (_productConfig != null)
        {
            return;
        }

        _productConfig = await DownloadProductConfig();

        if (_productConfig != null && !string.IsNullOrWhiteSpace(_productConfig.Version) && SuityApp.VersionCode != _productConfig.Version)
        {
            _drawNotice = gui => 
            {
                var msg = L($"New version {_productConfig.Version} is available, click 'Update' to download the latest version.");
                ShowNotice(gui, msg, L("Update"), () => NavigateToDownloadPage());
            };
        }

        _guiRef.QueueRefresh();
    }

    private async Task<ProductConfig?> DownloadProductConfig()
    {
        string url = "https://storage.suitylab.com/SuityAgentic/ProductConfig.json";

        try
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync(url);
            var config = JsonConvert.DeserializeObject<ProductConfig>(json);
            return config;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to download product config: {ex.Message}");
            return null;
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        _guiEnabled = enabled;
        _guiRef.QueueRefresh();
    }

    private void ShowNotice(ImGui gui, Action content)
    {
        gui.HorizontalFrame("formFooterNotice")
        .InitClass("formFooterNotice")
        .InitFullWidth()
        .SetHeightRest()
        .OnContent(content);
    }

    private void ShowNotice(ImGui gui, string message, string buttonText = null, Action buttonAction = null)
    {
        ShowNotice(gui, () =>
        {
            gui.Text("#msg", message)
            .InitClass("noticeText")
            .InitCenterVertical();

            if (!string.IsNullOrWhiteSpace(buttonText))
            {
                gui.Button("#btn", buttonText)
                .InitClass("toolBtn")
                .InitCenterVertical()
                .OnClick(() => buttonAction?.Invoke());
            }
        });
    }

    private void DrawFooter(ImGui gui)
    {
        gui.HorizontalFrame("formFooter")
        .InitClass("formFooter")
        .InitFullWidth()
        .SetHeightRest();
    }

    private void NavigateToDownloadPage()
    {
        if (_productConfig?.DownloadUrl is { } url && !string.IsNullOrWhiteSpace(url))
        {
            EditorUtility.OpenBrowser(url);
        }
        else
        {
            EditorUtility.OpenBrowser(SuityApp.GithubPage);
        }
    }

    public void Dispose()
    {
        foreach (var img in _templateImgs.Values)
        {
            img?.Dispose();
        }

        _templateImgs.Clear();

        _page = Pages.None;
        _guiRef.Node = null;
    }
}