using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for managing plugins.
/// </summary>
public interface IPluginService
{
    /// <summary>
    /// Gets all available plugins.
    /// </summary>
    IEnumerable<PluginInfo> Plugins { get; }

    /// <summary>
    /// Gets a plugin of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of plugin to retrieve.</typeparam>
    /// <returns>The plugin instance, or null if not found.</returns>
    T GetPlugin<T>() where T : Plugin;
}

/// <summary>
/// Empty implementation of the plugin service.
/// </summary>
public sealed class EmptyPluginService : IPluginService
{
    /// <summary>
    /// Gets the singleton instance of EmptyPluginService.
    /// </summary>
    public static readonly EmptyPluginService Empty = new();

    private EmptyPluginService()
    { }

    /// <inheritdoc/>
    public IEnumerable<PluginInfo> Plugins => [];

    /// <inheritdoc/>
    public T GetPlugin<T>() where T : Plugin
    {
        return null;
    }
}