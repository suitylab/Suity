using Avalonia.Threading;
using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.MenuCommands.AppMenus;
using Suity.Editor.Services;
using Suity.Networking;
using Suity.Rex;
using Suity.Rex.Mapping;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor;

sealed class AvaEditorDevice : Device, IRexResolver, ISystemLog, IRexHandler<NavigateVReq>, IRexHandler<LocateInCanvasVReq>
{
    public static readonly bool StandaloneMode = true;


    public static readonly AvaEditorDevice Instance = new();

    readonly DateTime _startTime = DateTime.Now;
    readonly List<IServiceProvider> _providers = [];
    readonly Dictionary<Type, object> _services = [];
    readonly Dictionary<Type, object> _fallBackServices = [];

    private AvaEditorDevice()
    {
        
    }
    internal void Initialize()
    {
        EditorServices.SystemLog.AddLog("AvaEditorDevice initializing...");
        EditorServices.SystemLog.PushIndent();

        ServiceInternals._license = AvaLicenseService.Instance;

        // ObjectType.RegisterTypeInfoResolver(SValueTypeInfoResolver.Instance);
        RexGlobalResolve.Current = this;
        SyncTypes.InitializeGlobalResolver(EditorSyncTypeResolver.Instance);
        SyncTypes.SetValueResolver(typeof(Color), 
            s => ColorTranslators.FromHtmlSafe(s), 
            o => o is Color c ? ColorTranslators.ToHtml(c) : string.Empty);

        NamedExternalBK.Instance._globalResolver = EditorSyncTypeResolver.Instance;

        EditorRexes.EnsureInMainThread.AddActionListener(() => EnsureInMainThread());
        EditorRexes.PushQueuedActions.AddActionListener(() => PushAsyncQueue());
        EditorRexes.SendToRecycleBin.AddActionListener(fileName => PlatformOS.Current.SendToRecycleBin(fileName));
        EditorRexes.GotoDefinition.AddActionListener(o => Navigator.GuiGotoDefinition(o));
        EditorRexes.FindReference.AddActionListener(o => Navigator.FindReference(o));
        EditorRexes.FindImplement.AddActionListener(o => Navigator.FindImplement(o));
        EditorRexes.GlobalSearch.AddActionListener((str, option) => Navigator.GlobalSearch(str, option));
        EditorRexes.ShowProjectSetting.AddActionListener(ProjectSettingMenuCommand.OpenProjectSetting);
        EditorCommands.Mapper.ProvideHandler<NavigateVReq>(this);
        EditorCommands.Mapper.ProvideHandler<LocateInCanvasVReq>(this);

        AddService<ISystemLog>(this);
        AddService<IEditorSystemService>(EditorSystemService.Instance);
        AddService<IToolWindowService>(AvaToolWindowService.Instance);
        AddService<IMenuService>(MenuService.Instance);
        AddService<IImGuiService>(AvaImGuiService.Instance);
        AddService<IColorConfig>(ColorConfigBK.Instance);
        AddService<IEditorColorConfig>(ColorConfigBK.Instance);
        AddService<IIconService>(AvaIconService.Instance);
        AddService<FileUpdateService>(FileUpdateServiceBK.Instance);
        AddService<NavigationService>(NavigationServiceBK.Instance);
        AddService<StorageManager>(StorageManagerBK.Instance);
        AddService<IProgressService>(AvaProgressService.Instance);
        AddService<IJsonResourceService>(JsonResourceService.Instance);
        AddService<DocumentViewManager>(AvaDocumentViewManager.Instance);
        AddService<IDrawingService>(AvaDrawingService.Instance);

        AddService<IDialogService>(AvaDialogService.Instance);
        AddService<IDialogExService>(AvaDialogService.Instance);
        AddService<IFileNameService>(FileNameServiceBK.Instance);
        AddService<IDialogServiceAsync>(AvaDialogServiceAsync.Instance);

        AddService<DocumentViewResolver>(DocumentViewResolver.Instance);
        AddService<DrawExpandedImGuiResolver>(DrawExpandedImGuiResolver.Instance);
        AddService<ISelectionService>(AvaSelectionService.Instance);
        AddService<ISystemClipboard>(AvaClipboardService.Instance);
        AddService<IAppConfig>(AppConfiguration.Instance);

        // Must be registered first, needed for startup process
        AddService<IPluginService>(PluginManager.Instance);
        AddService<IAssemblyService>(PluginManager.Instance);
        AddService<IRunDelayed>(RunDelayed.Default);
        AddService<ILocalizationService>(LocalizeManager.Instance);
        AddService<IAssemblyNameService>(AssemblyNameService.Instance);

        AddService<LicenseService>(EmptyLicenseService.Empty);

        AddFallBackService<ICodeRenderInfoService>(EmptyCodeRenderInfoService.Empty);
        AddFallBackService<IMonitorService>(EmptyMonitorService.Empty);

        // ================================
        EditorServices.SystemLog.AddLog("Initialize environment device...");
        Device.InitializeDevice(AvaEditorDevice.Instance);
        ImGuiServices.Initialize();


        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("EditorDevice initialized.");
    }


    #region Override
    public override string Location => "SuityEditor5";
    public override float Time => (float)(DateTime.Now - _startTime).TotalSeconds;
    public override void AddLog(LogMessageType type, object message)
    {
        LogCache.AddLog(type, message);

    }
    public override void AddNetworkLog(LogMessageType type, NetworkDirection direction, string sessionId, string channelId, object message)
    {
    }
    public override void AddOperationLog(int level, string category, string userId, string ip, object data, bool successful)
    {
    }
    public override void AddResourceLog(string key, string path)
    {
    }
    public override void AddEntityLog(long roomId, long entityId, string entityName, EntityActionTypes actionType, LogMessageType messageType, object component)
    {
    }
    public override string? GetEnvironmentConfig(string key)
    {
        return null;
    }
    public override object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        object? result;

        result = _services.GetValueSafe(serviceType);
        if (result != null)
        {
            return result;
        }

        foreach (var provider in _providers)
        {
            result = provider.GetService(serviceType);
            if (result != null)
            {
                _services.Add(serviceType, result);
                return result;
            }
        }

        result = EditorRexes.Mapper.Get(serviceType);
        if (result != null)
        {
            return result;
        }

        result = EditorCommands.Mapper.Get(serviceType);
        if (result != null)
        {
            return result;
        }

        result = _fallBackServices.GetValueSafe(serviceType);
        if (result != null)
        {
            return result;
        }

        Logs.LogWarning($"Editor service not found : {serviceType.Name}");

        return null;

    }
    public override void ObjectCreate(Object obj)
    {
    }
    public override void QueueAction(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    #endregion

    #region IEditorLog

    void ISystemLog.AddLog(object message) => AddLog(EditorLogCategory.Core, LogMessageType.Debug, message);

    void ISystemLog.PushIndent() => PushIndent(EditorLogCategory.Core);

    void ISystemLog.PopIndent() => PopIndent(EditorLogCategory.Core);

    public void AddLog(EditorLogCategory category, LogMessageType type, object message)
    {
        switch (category)
        {
            case EditorLogCategory.Core:
                LogCache.AddLog(type, message);
                break;
            case EditorLogCategory.Editor:
                LogCache.AddLog(type, message);
                break;
            case EditorLogCategory.Runtime:
                LogCache.AddLog(type, message);
                break;
            default:
                break;
        }
    }

    public void PushIndent(EditorLogCategory category)
    {
        LogCache.PushIndent();
    }

    public void PopIndent(EditorLogCategory category)
    {
        LogCache.PopIndent();
    }
    #endregion

    #region IRexHandler<NavigateVo>
    bool IRexHandler<NavigateVReq>.Handle(NavigateVReq value)
    {
        if (value != null)
        {
            value.Successful = Navigator.Navigate(value.Target);
            return true;
        }

        return false;
    }

    bool IRexHandler<LocateInCanvasVReq>.Handle(LocateInCanvasVReq value)
    {
        if (value != null && EditorObjectManager.Instance.GetObject(value.Id) is { } obj)
        {
            value.Successful = obj.LocateInCanvas();
            if (!value.Successful)
            {
                value.Successful = obj.ShowDocumentView() != null;
            }

            return true;
        }

        return false;
    }

    #endregion

    #region IRexResolver

    public string[] GetPropertyNames(object obj) => [];

    public object? GetProperty(object obj, string propertyName) => null;

    public string? GetDataId(object o) => null;

    public T? GetObject<T>(string key) where T : class => null;

    public void LogException(Exception exception)
    {
        exception.LogError();
    }

    public void DoQueuedAction(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    #endregion

    internal void AddService(Type type, object service)
    {
        EditorServices.SystemLog.AddLog($"Add service : {type.Name}");

        _services[type] = service;
    }

    internal void AddService<T>(T service)
    {
        ArgumentNullException.ThrowIfNull(service);

        EditorServices.SystemLog.AddLog($"Add service : {typeof(T).Name}");

        _services[typeof(T)] = service;
    }

    internal void AddFallBackService<T>(T service)
    {
        ArgumentNullException.ThrowIfNull(service);

        EditorServices.SystemLog.AddLog($"Add fall back service : {typeof(T).Name}");

        _fallBackServices[typeof(T)] = service;
    }

    internal void AddServiceProvider(IServiceProvider provider)
    {
        if (provider == null)
        {
            return;
        }

        EditorServices.SystemLog.AddLog($"Add service provider : {provider.GetType().Name}");

        _providers.Add(provider);
    }
    internal void EnsureInMainThread()
    {
        //if (MainThread != Thread.CurrentThread)
        //{
        //    throw new InvalidOperationException("Invalid thread access.");
        //}
    }

    internal void PushAsyncQueue()
    {
        //if (Thread.CurrentThread != MainThread)
        //{
        //    return;
        //}

        //while (_actionQueue.TryDequeue(out Action action))
        //{
        //    try
        //    {
        //        action.Invoke();
        //    }
        //    catch (Exception err)
        //    {
        //        err.LogError();
        //    }
        //}
    }

}
