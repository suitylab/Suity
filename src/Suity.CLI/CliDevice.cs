using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Networking;
using Suity.Rex;
using Suity.Synchonizing.Core;
using Suity.Views.Graphics;
using Suity.Views.Named;
using System.Collections.Concurrent;
using System.Drawing;

namespace Suity.Editor;

sealed class CliDevice : Device, IRexResolver, ISystemLog
{
    public static readonly bool StandaloneMode = true;

    private static Lazy<CliDevice> _instance { get; } = new(() => new());
    public static CliDevice Instance => _instance.Value;

    bool _init = false;
    readonly DateTime _startTime = DateTime.Now;
    readonly List<IServiceProvider> _providers = [];
    readonly Dictionary<Type, object> _services = [];
    readonly Dictionary<Type, object> _fallBackServices = [];

    readonly ConcurrentQueue<Action> _actionQueue = [];
    int _actionQueueSuspended = 0;

    int _indent = 0;

    public CliDevice()
    {   
    }

    public override void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        EditorServices.SystemLog.AddLog("Suity Agentic - CLI Device initializing...");
        EditorServices.SystemLog.PushIndent();

        ServiceInternals._license = CliLicenseService.Instance;

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
        

        AddService<ISystemLog>(this);
        AddService<IEditorSystemService>(EditorSystemService.Instance);
        AddService<IPlatformService>(CliPlatformService.Instance);
        AddService<IToolWindowService>(ToolWindowService.Instance);
        AddService<IMenuService>(MenuService.Instance);
        AddService<IImGuiService>(CliImGuiService.Instance);
        AddService<IColorConfig>(ColorConfigBK.Instance);
        AddService<IEditorColorConfig>(ColorConfigBK.Instance);
        AddService<IIconService>(IconService.Instance);
        AddService<FileUpdateService>(FileUpdateServiceBK.Instance);
        AddService<NavigationService>(NavigationServiceBK.Instance);
        AddService<StorageManager>(StorageManagerBK.Instance);
        AddService<IProgressService>(CliProgressService.Instance);
        AddService<IJsonResourceService>(JsonResourceService.Instance);
        AddService<DocumentViewManager>(CliDocumentViewManager.Instance);
        AddService<IDrawingService>(CliDrawingService.Instance);

        AddService<IFileNameService>(FileNameServiceBK.Instance);

        AddService<DocumentViewResolver>(DocumentViewResolver.Instance);
        AddService<DrawExpandedImGuiResolver>(DrawExpandedImGuiResolver.Instance);
        //AddService<IAppConfig>(AppConfiguration.Instance);

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
        ImGuiServices.Initialize();


        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("Device initialized.");
    }


    #region Override
    public override string Location => "Suity.CLI";
    public override float Time => (float)(DateTime.Now - _startTime).TotalSeconds;
    public override void AddLog(LogMessageType type, object message)
    {
        if (message is ExceptionLogItem ex)
        {
            Console.WriteLine($"[{type}] {ex.Message}");
            if (ex.Exception != null)
            {
                Console.WriteLine($"[{type}] {ex.Exception}");
            }
            if (ex.Object != null)
            {
                Console.WriteLine($"[{type}] {ex.Object}");
            }
        }
        else if (message is ObjectLogItem logItem)
        {
            Console.WriteLine($"[{type}] {logItem.Message}");
            if (logItem.Object != null)
            {
                Console.WriteLine($"[{type}] {logItem.Object}");
            }
        }
        else
        {
            Console.WriteLine($"[{type}] {message}");
        }
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
        //if (_actionQueueSuspended == 0)
        //{
        //    Dispatcher.UIThread.Post(action);
        //}
        //else
        //{
            _actionQueue.Enqueue(action);
        //}
    }

    public override void DoSuspendedAction(Action action)
    {
        try
        {
            int n = Interlocked.Increment(ref _actionQueueSuspended);

            action();
        }
        finally
        {
            int n = Interlocked.Decrement(ref _actionQueueSuspended);
            if (n == 0)
            {
                FlushQueuedActions();
            }
        }
    }

    public override async Task DoSuspendedAction(Func<Task> action)
    {
        try
        {
            int n = Interlocked.Increment(ref _actionQueueSuspended);

            await action();
        }
        finally
        {
            int n = Interlocked.Decrement(ref _actionQueueSuspended);
            if (n == 0)
            {
                FlushQueuedActions();
            }
        }
    }

    public override void FlushQueuedActions()
    {
        while (_actionQueue.TryDequeue(out var queuedAction))
        {
            try
            {
                //Dispatcher.UIThread.Post(queuedAction);
                queuedAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    public override bool IsQueueSuspended => _actionQueueSuspended > 0;

    #endregion

    #region ISystemLog

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
        _indent++;
    }

    public void PopIndent(EditorLogCategory category)
    {
        if (_indent > 0)
        {
            _indent--;
        }
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

    public void DoQueuedAction(Action action) => QueueAction(action);

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
    }
    internal void PushAsyncQueue()
    {
    }
}
