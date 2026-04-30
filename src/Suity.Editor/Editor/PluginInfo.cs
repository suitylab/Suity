using System;
using System.Collections.Generic;

namespace Suity.Editor;

/// <summary>
/// Plugin information
/// </summary>
public class PluginInfo
{
    private bool _started;
    private readonly List<PluginFunctionality> _modules = [];

    /// <summary>
    /// The plugin instance associated with this info.
    /// </summary>
    public Plugin Plugin { get; }

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public string Name => Plugin.Name;

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public string Description => Plugin.Description;

    /// <summary>
    /// Gets the display text for the plugin, which is the description, type display text, or name in that priority.
    /// </summary>
    public string DisplayText
    {
        get
        {
            var plugin = Plugin;
            if (plugin is null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(plugin.Description))
            {
                return plugin.Description;
            }

            var typeDisplayText = plugin.GetType().ToDisplayText();
            if (!string.IsNullOrWhiteSpace(typeDisplayText))
            {
                return typeDisplayText;
            }

            return plugin.Name;
        }
    }

    /// <summary>
    /// Gets the full assembly name of the plugin's type.
    /// </summary>
    public string FileName => Plugin.GetType().Assembly.FullName;

    /// <summary>
    /// Gets the load order of the plugin.
    /// </summary>
    public int Order => Plugin.Order;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInfo"/> class.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when plugin is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the plugin already has info assigned.</exception>
    public PluginInfo(Plugin plugin)
    {
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        if (plugin.Info != null)
        {
            throw new InvalidOperationException();
        }

        plugin.Info = this;
    }

    /// <summary>
    /// Gets all functionalities provided by the plugin.
    /// </summary>
    /// <returns>An array of <see cref="PluginFunctionality"/> objects.</returns>
    public PluginFunctionality[] GetFunctionalities()
    {
        PluginFunctionality[] ary = new PluginFunctionality[_modules.Count];
        for (int i = 0; i < _modules.Count; i++)
        {
            ary[i] = _modules[i];
        }

        return ary;
    }

    /// <summary>
    /// Adds a functionality to the plugin.
    /// </summary>
    /// <typeparam name="T">The type of the functionality value.</typeparam>
    /// <param name="type">The type identifier.</param>
    /// <param name="name">The name of the functionality.</param>
    /// <param name="value">The value of the functionality.</param>
    internal void AddFunctionality<T>(string type, string name, T value)
    {
        PluginFunctionality<T> module = new PluginFunctionality<T>(type, name, value);
        _modules.Add(module);
    }

    /// <summary>
    /// Awakes the plugin if not already started.
    /// </summary>
    internal void AwakePlugin()
    {
        if (_started)
        {
            return;
        }

        var context = new PluginContext(this);
        Plugin.Awake(context);
    }

    /// <summary>
    /// Starts the plugin if not already started.
    /// </summary>
    internal void StartPlugin()
    {
        if (_started)
        {
            return;
        }

        var context = new PluginContext(this);
        Plugin.Start(context);

        _started = true;
    }

    /// <summary>
    /// Returns a string representation of the plugin info.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        return Plugin?.ToString() ?? base.ToString();
    }
}
