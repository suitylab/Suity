using Suity.Editor.Documents;
using Suity.Editor.WorkSpaces;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Editor services collection
/// </summary>
public static class EditorServices
{
    public static Project CurrentProject => Project.Current;

    /// <summary>
    /// Editor object manager
    /// </summary>
    public static EditorObjectManager EditorObjectManager => EditorObjectManager.Instance;

    /// <summary>
    /// Asset manager
    /// </summary>
    public static AssetManager AssetManager => AssetManager.Instance;

    /// <summary>
    /// Value manager
    /// </summary>
    public static ValueManager ValueManager => ValueManager.Instance;

    /// <summary>
    /// Document manager
    /// </summary>
    public static DocumentManager DocumentManager => DocumentManager.Instance;

    /// <summary>
    /// Document view manager
    /// </summary>
    public static DocumentViewManager DocumentViewManager => DocumentViewManager.Current;

    /// <summary>
    /// Storage manager
    /// </summary>
    public static StorageManager StorageManager => StorageManager.Current;

    /// <summary>
    /// File asset manager
    /// </summary>
    public static FileAssetManager FileAssetManager => FileAssetManager.Current;

    /// <summary>
    /// Reference manager
    /// </summary>
    public static ReferenceManager ReferenceManager => ReferenceManager.Current;

    /// <summary>
    /// Workspace manager
    /// </summary>
    public static WorkSpaceManager WorkSpaceManager => WorkSpaceManager.Current;

    /// <summary>
    /// Analysis service
    /// </summary>
    public static AnalysisService AnalysisService => AnalysisService.Current;

    /// <summary>
    /// License service
    /// </summary>
    public static LicenseService LicenseService => LicenseService.Current;

    /// <summary>
    /// File update service
    /// </summary>
    public static FileUpdateService FileUpdateService => FileUpdateService.Current;

    /// <summary>
    /// Navigation service
    /// </summary>
    public static NavigationService NavigationService => NavigationService.Current;


    #region IAppConfig

    private static readonly ServiceStore<IAppConfig> _appConfig = new();
    public static IAppConfig AppConfig => _appConfig.Get();

    #endregion

    #region ISystemLog

    private static ISystemLog _systemLog = ConsleSystemLog.Instance;

    /// <summary>
    /// Property to get or set the system log instance
    /// </summary>
    /// <value>The current system log implementation</value>
    public static ISystemLog SystemLog
    {
        get => _systemLog;
        set
        {
            _systemLog = value ?? ConsleSystemLog.Instance;
        }
    }

    #endregion

    #region IToolWindowService

    private static readonly ServiceStore<IToolWindowService> _toolWindowService = new();

    public static IToolWindowService ToolWindow => _toolWindowService.Get();

    #endregion

    #region IIconService

    private static readonly ServiceStore<IIconService> _icon = new();
    public static IIconService IconService => _icon.Get();

    #endregion

    #region Inspector

    private static readonly ServiceStore<IInspector> _inspector = new();

    /// <summary>
    /// Retrieves the inspector service.
    /// </summary>
    public static IInspector Inspector => _inspector.Get(EmptyInspector.Empty);

    #endregion

    #region IDialogService

    private static readonly ServiceStore<IDialogService> _dialog = new();
    public static IDialogService DialogService => _dialog.Get();

    #endregion

    #region IDialogServiceAsync

    private static readonly ServiceStore<IDialogServiceAsync> _dialogAsync = new();
    public static IDialogServiceAsync DialogServiceAsync => _dialogAsync.Get();

    #endregion

    #region IDialogExService

    private static readonly ServiceStore<IDialogExService> _dialogEx = new();
    public static IDialogExService DialogExService => _dialogEx.Get();

    #endregion

    #region ISelectionService

    private static readonly ServiceStore<ISelectionService> _selectionSerice = new();
    public static ISelectionService SelectionService => _selectionSerice.Get();

    #endregion

    #region IRunDelayed

    private static readonly ServiceStore<IRunDelayed> _runDelayed = new();
    public static IRunDelayed RunDelayed => _runDelayed.Get();

    #endregion

    #region IProgressService

    private static readonly ServiceStore<IProgressService> _progressService = new();
    public static IProgressService ProgressService => _progressService.Get();

    #endregion

    #region ISObjectService

    private static readonly ServiceStore<ISObjectService> _isobjectService = new();
    public static ISObjectService SObjectService => _isobjectService.Get();

    #endregion

    #region IEditorCommonService

    private static readonly ServiceStore<IEditorCommonService> _editorCommon = new();

    /// <summary>
    /// File name service
    /// </summary>
    public static IEditorCommonService EditorCommon => _editorCommon.Get();

    #endregion

    #region IPlatformService

    private static readonly ServiceStore<IPlatformService> _platformService = new();

    /// <summary>
    /// Platform service
    /// </summary>
    public static IPlatformService PlatformService => _platformService.Get();

    #endregion

    #region IFileNameService

    private static readonly ServiceStore<IFileNameService> _fileNameService = new();

    /// <summary>
    /// File name service
    /// </summary>
    public static IFileNameService FileNameService => _fileNameService.Get();

    #endregion

    #region IAssemblyService

    private static readonly ServiceStore<IAssemblyService> _asmService = new();

    /// <summary>
    /// Assembly service
    /// </summary>
    public static IAssemblyService AssemblyService => _asmService.Get();

    #endregion

    #region IPluginService

    private static readonly ServiceStore<IPluginService> _plugin = new();

    /// <summary>
    /// Plugin service
    /// </summary>
    public static IPluginService PluginService => _plugin.Get();

    #endregion

    #region IMonitorService

    private static readonly ServiceStore<IMonitorService> _monitorService = new();

    /// <summary>
    /// Monitor service
    /// </summary>
    public static IMonitorService MonitorService => _monitorService.Get();

    #endregion

    #region IClipBoardService

    private static readonly ServiceStore<IClipboardService> _clipboardService = new();

    /// <summary>
    /// Clipboard Services
    /// </summary>
    public static IClipboardService ClipboardService => _clipboardService.Get();

    #endregion

    #region IColorConfig

    private static readonly ServiceStore<IEditorColorConfig> _colorConfig = new();

    /// <summary>
    /// Color config
    /// </summary>
    public static IEditorColorConfig ColorConfig => _colorConfig.Get();

    #endregion

    #region IDrawingService

    private static readonly ServiceStore<IDrawingService> _drawingService = new();

    /// <summary>
    /// Drawing service
    /// </summary>
    public static IDrawingService DrawingService => _drawingService.Get();

    #endregion

    #region IImGuiService

    private static readonly ServiceStore<IImGuiService> _imgui = new();

    /// <summary>
    /// ImGui service
    /// </summary>
    public static IImGuiService ImGuiService => _imgui.Get();

    #endregion

    #region IJsonSchemaService

    private static readonly ServiceStore<IJsonSchemaService> _JsonSchema = new();

    /// <summary>
    /// Json schema service
    /// </summary>
    public static IJsonSchemaService JsonSchemaService => _JsonSchema.Get();

    #endregion

    #region ITypeConvertService

    private static readonly ServiceStore<ITypeConvertService> _typeConvert = new();

    /// <summary>
    /// Json schema service
    /// </summary>
    public static ITypeConvertService TypeConvertService => _typeConvert.Get();

    #endregion

    #region IMermaidService

    private static readonly ServiceStore<IMermaidService> _mermaid = new();

    /// <summary>
    /// Gets the current cryptographic service used for encryption and decryption operations.
    /// </summary>
    public static IMermaidService MermaidService => _mermaid.Get();

    #endregion

    #region Building

    private static readonly ServiceStore<ICodeRenderService> _codeRender = new();
    private static readonly ServiceStore<ICodeRenderInfoService> _codeRenderInfo = new(EmptyCodeRenderInfoService.Empty);
    private static readonly ServiceStore<IJsonResourceService> _jsonResource = new();
    private static readonly ServiceStore<IExpressionRenderService> _exprRender = new();

    /// <summary>
    /// Code render service
    /// </summary>
    public static ICodeRenderService CodeRender => _codeRender.Get();

    /// <summary>
    /// Code render information
    /// </summary>
    public static ICodeRenderInfoService CodeRenderInfo => _codeRenderInfo.Get();

    /// <summary>
    /// Json resource service
    /// </summary>
    public static IJsonResourceService JsonResource => _jsonResource.Get();

    /// <summary>
    /// Expression render service
    /// </summary>
    public static IExpressionRenderService ExpressionRender => _exprRender.Get();

    #endregion

    #region IFileBunchService

    private static readonly ServiceStore<IFileBunchService> _fileBunchService = new();
    public static IFileBunchService FileBunchService => _fileBunchService.Get();

    #endregion

    #region IClipboardService

    private static readonly ServiceStore<IClipboardService> _clipboard = new();

    /// <summary>
    /// Gets the clipboard service instance for clipboard operations.
    /// </summary>
    /// <value>An instance of IClipboardService that provides clipboard functionality.</value>
    public static IClipboardService Clipboard => _clipboard.Get();

    #endregion

    #region ISystemClipboard

    private static readonly ServiceStore<ISystemClipboard> _systemClipboard = new();

    public static ISystemClipboard SystemClipboard => _systemClipboard.Get();

    #endregion

    #region Localize

    private static readonly ServiceStore<ILocalizationService> _localization = new();

    /// <summary>
    /// Localization service
    /// </summary>
    public static ILocalizationService LocalizationService => _localization.Get();

    #endregion


    public static void LogStatus()
    {
        Logs.LogInfo("--- Platform Status ---");
        Logs.LogInfo($"Platform: {Environment.OSVersion.Platform.ToString()}");
        Logs.LogInfo($"ApplicationMode: {PlatformService?.ApplicationMode.ToString() ?? "Unknown"}");
        Logs.LogInfo($"IsLocalDbSupported: {PlatformService?.IsLocalDbSupported ?? false}");
        Logs.LogInfo($"IsFileSystemWatcherSupported: {PlatformService?.IsFileSystemWatcherSupported ?? false}");
        Logs.LogInfo($"IsConsoleColorSupported: {PlatformService?.IsConsoleColorSupported ?? false}");

        Logs.LogInfo("--- License Service Status ---");
        Logs.LogInfo($"Product Name: {LicenseService?.ProductName ?? "Unknown"}");
        Logs.LogInfo($"Product Version: {LicenseService?.ProductVersion ?? "Unknown"}");
        Logs.LogInfo($"User Id: {LicenseService?.UserId ?? "Unknown"}");
        Logs.LogInfo($"License Type: {LicenseService?.LicenseType.ToString() ?? "Unknown"}");

        //Logs.LogInfo("--- Editor Services Status ---");
        //Logs.LogInfo($"Editor Object Manager: {(EditorObjectManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Asset Manager: {(AssetManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Value Manager: {(ValueManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Document Manager: {(DocumentManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Document View Manager: {(DocumentViewManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Storage Manager: {(StorageManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"File Asset Manager: {(FileAssetManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Reference Manager: {(ReferenceManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Workspace Manager: {(WorkSpaceManager != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Analysis Service: {(AnalysisService != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"License Service: {(LicenseService != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"File Update Service: {(FileUpdateService != null ? "Available" : "Not Available")}");
        //Logs.LogInfo($"Navigation Service: {(NavigationService != null ? "Available" : "Not Available")}");

        Logs.LogInfo("--- Suity Status ---");
        Logs.LogInfo($"Location: {Device.Current?.Location ?? "Unknown"}");
        Logs.LogInfo($"IsQueueSuspended: {Device.Current?.IsQueueSuspended ?? false}");
        Logs.LogInfo($"QueuedActionCount: {Device.Current?.QueuedActionCount ?? 0}");
        Logs.LogInfo($"DelayedActionCount: {RunDelayed?.DelayedActionCount ?? 0}");

        Logs.LogInfo("--- Project Status ---");
        Logs.LogInfo($"Project Name: {CurrentProject?.ProjectName ?? "None"}");
        Logs.LogInfo($"Project Status: {CurrentProject?.Status.ToString() ?? "None"}");
        Logs.LogInfo($"Project GUID: {CurrentProject?.ProjectGuid.ToString() ?? "None"}");
        Logs.LogInfo($"Project Base Path: {CurrentProject?.ProjectBasePath ?? "None"}");
    }
}