using Suity.Collections;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Suity.Editor.Services;

/// <summary>
/// Manages editor plugins and assemblies, handling plugin scanning, registration, and lifecycle.
/// </summary>
internal sealed class PluginManager : IAssemblyService, IPluginService
{
    /// <summary>
    /// Singleton instance of the plugin manager.
    /// </summary>
    internal static PluginManager Instance { get; } = new PluginManager();

    private readonly Dictionary<string, PluginInfo> _plugins = [];
    private readonly Dictionary<Type, PluginInfo> _typeLookup = [];

    private readonly HashSet<Assembly> _assemblies = [];
    private bool _init;

    private PluginManager()
    {
    }

    #region IAssemblyService

    //public void ScanPluginDirectory()
    //{
    //    string[] fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
    //    foreach (string fileName in fileNames)
    //    {
    //        string name = Path.GetFileName(fileName);
    //        name = name != null ? name.ToLowerInvariant() : string.Empty;

    //        if (name.StartsWith("dsplug.") && name.EndsWith(".dll"))
    //        {
    //            Assembly asm = null;
    //            try
    //            {
    //                asm = Assembly.LoadFile(fileName);
    //                //EditorGlobals.Messages.LogDebug("############################## Assembly:" + asm.FullName);
    //            }
    //            catch (Exception e)
    //            {
    //                //EditorGlobals.Messages.LogDebug("############################## Failed!!:" + fileName);
    //                AppService.Log.ShowError("Failed to load assembly:" + fileName, e);
    //            }
    //            if (asm != null)
    //            {
    //                RegisterAssembly(asm);
    //            }
    //        }
    //    }
    //}

    //public void ScanForPlugin(string fileName)
    //{
    //    Assembly asm = Assembly.LoadFrom(fileName);
    //    RegisterAssembly(asm);
    //}

    /// <inheritdoc/>
    public void StartPlugins(IEnumerable<Assembly> assemblies, Action<IServiceProvider> serviceAdd)
    {
        if (_init)
        {
            throw new InvalidOperationException("IAssemblyService is already initialized.");
        }
        _init = true;

        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }


        EditorServices.SystemLog.AddLog("PluginManager setup plugins...");
        EditorServices.SystemLog.PushIndent();

        // Reset DerivedType cache
        DerivedTypeHelper.ResetCache();
        TypeResolveHelper.ResetCache();
        InternalTypeResolve.ResetCache();

        _assemblies.Clear();
        // Add
        _assemblies.Add(this.GetType().Assembly);
        foreach (var asm in assemblies)
        {
            _assemblies.Add(asm);
        }

        // Initialize documents and assets
        //AssetActivatorManager.Instance.Initialize();
        //DocumentManager.Instance.Initialize();
        //LinkedAssetHelper.Initialize();
        //DocumentViewManager.Instance.Initialize();

        // Important: After obtaining all assemblies, immediately register all native types,
        // because the following ScanForPlugin step will immediately initialize plugins, and plugins may need to use native types
        NativeTypeReflector.Instance.Initialize();

        // Scan plugins
        foreach (var asm in assemblies)
        {
            ScanForPlugin(asm);
        }

        // Register service providers
        foreach (var pluginInfo in _plugins.Values)
        {
            serviceAdd?.Invoke(pluginInfo.Plugin);
        }

        // Pre-start
        foreach (var pluginInfo in _plugins.Values)
        {
            EditorServices.SystemLog.AddLog($"Awake plugin : {pluginInfo.Name}");
            EditorServices.SystemLog.PushIndent();
            try
            {
                pluginInfo.AwakePlugin();
            }
            catch (Exception err)
            {
                err.LogError($"Plugin awakes failed : {pluginInfo.Name}");
            }
            EditorServices.SystemLog.PopIndent();
        }

        // Start
        foreach (var pluginInfo in _plugins.Values)
        {
            EditorServices.SystemLog.AddLog($"Start plugin : {pluginInfo.Name}");
            EditorServices.SystemLog.PushIndent();
            try
            {
                pluginInfo.StartPlugin();
            }
            catch (Exception err)
            {
                err.LogError($"Plugin starts failed : {pluginInfo.Name}");
            }
            EditorServices.SystemLog.PopIndent();
        }

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("PluginManager setup plugins finished.");
    }

    /// <inheritdoc/>
    public IEnumerable<Assembly> RegisteredAssemblies
    {
        get
        {
            return _assemblies;
        }
    }

    /// <inheritdoc/>
    public bool ContainsAssembly(Assembly asm)
    {
        return _assemblies.Contains(asm);
    }

    #endregion

    #region IPluginService

    /// <inheritdoc/>
    public IEnumerable<PluginInfo> Plugins => _plugins.Values;

    /// <inheritdoc/>
    public T GetPlugin<T>() where T : Plugin
    {
        PluginInfo info = _typeLookup.GetValueSafe(typeof(T));
        if (info != null)
        {
            return (T)info.Plugin;
        }
        else
        {
            return null;
        }
    }

    #endregion

    /// <summary>
    /// Scans an assembly for plugin types and registers them.
    /// </summary>
    /// <param name="assembly">The assembly to scan for plugins.</param>
    private void ScanForPlugin(Assembly assembly)
    {
        // Loading plugins is prohibited in other Domains
        if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
        {
            return;
        }

        EditorServices.SystemLog.AddLog($"Scanning assembly : {assembly.FullName}");

        // Get types
        Type[] types;
        try
        {
            types = assembly.GetExportedTypes();
        }
        catch (TypeLoadException typeLoadErr)
        {
            EditorUtility.ShowError("Load failed:" + assembly.FullName, typeLoadErr);

            return;
        }
        catch (ReflectionTypeLoadException typeLoadErr)
        {
            foreach (Exception innerError in typeLoadErr.LoaderExceptions)
            {
                EditorUtility.ShowError("Load failed:" + assembly.FullName, innerError);
            }

            return;
        }
        //catch (Exception otherErr)
        //{
        //    AppService.Log.ShowError("Load failed:" + assembly.FullName, otherErr);
        //    return;
        //}

        bool thirdParty = true;
        bool custom = true;

        // Scan plugins
        foreach (Type type in types)
        {
            if (_typeLookup.ContainsKey(type))
            {
                continue;
            }

            if (type.HasAttributeCached<NotAvailableAttribute>())
            {
                continue;
            }

            //EditorGlobals.Messages.LogDebug(type.FullName);
            if (!typeof(Plugin).IsAssignableFrom(type))
            {
                continue;
            }

            if (!type.IsPublic || !type.IsClass || type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            bool enabled = false;

            if (typeof(EditorPlugin).IsAssignableFrom(type))
            {
                enabled = true;
            }
            else if (typeof(ApiPlugin).IsAssignableFrom(type))
            {
                enabled = thirdParty;
            }
            else if (typeof(BackendPlugin).IsAssignableFrom(type))
            {
                enabled = custom;
            }
            else if (typeof(MiscPlugin).IsAssignableFrom(type))
            {
                enabled = true;
            }

            if (!enabled)
            {
                continue;
            }

            try
            {
                Plugin plugin = (Plugin)Activator.CreateInstance(type);
                RegisterPlugin(plugin);
            }
            catch (Exception innerEx)
            {
                innerEx.LogError($"Plugin initialize failed : {type.Name}");
            }
        }
    }

    /// <summary>
    /// Registers a plugin instance with the manager.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <returns>The registered plugin info.</returns>
    private PluginInfo RegisterPlugin(Plugin plugin)
    {
        if (_plugins.ContainsKey(plugin.Name))
        {
            throw new ArgumentException("Plugin name exist : " + plugin.Name);
        }

        if (_typeLookup.ContainsKey(plugin.GetType()))
        {
            throw new ArgumentException("Plugin type exist : " + plugin.GetType().Name);
        }

        EditorServices.SystemLog.AddLog($"Register plugin : {plugin.Name}");

        PluginInfo pluginInfo = new PluginInfo(plugin);
        _plugins[pluginInfo.Name] = pluginInfo;
        _typeLookup[plugin.GetType()] = pluginInfo;

        return pluginInfo;
    }

    /// <summary>
    /// Gets plugin information by name.
    /// </summary>
    /// <param name="name">The plugin name.</param>
    /// <returns>The plugin info, or null if not found.</returns>
    public PluginInfo GetPluginInfo(string name)
    {
        if (_plugins.TryGetValue(name, out PluginInfo pluginInfo))
        {
            return pluginInfo;
        }
        else
        {
            return null;
        }
    }
}
