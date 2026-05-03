using Suity.Drawing;
using System;
using System.Threading.Tasks;

namespace Suity.Editor;

/// <summary>
/// Plugin base class.
/// </summary>
public abstract class Plugin : IServiceProvider
{
    public PluginInfo Info { get; internal set; }

    /// <summary>
    /// Plugin name
    /// </summary>
    public virtual string Name => this.GetType().Name;

    public virtual string Description => null;

    public virtual ImageDef Icon => null;

    public virtual int Order => 0;

    internal Plugin(object clue)
    {
    }

    #region Start Stop

    /// <summary>
    /// Awake plugin
    /// </summary>
    /// <param name="context"></param>
    internal protected virtual void Awake(PluginContext context)
    {
    }

    /// <summary>
    /// Start plugin
    /// </summary>
    /// <param name="context">Plugin context</param>
    internal protected virtual void Start(PluginContext context)
    {
    }

    internal protected virtual void AwakeProject()
    {
    }

    internal protected virtual Task StartProject()
    {
        return Task.CompletedTask;
    }

    internal protected virtual void StopProject()
    {
    }

    internal protected virtual void SaveProject()
    {
    } 

    #endregion

    #region State
    protected object GetProjectState()
    {
        return Project.Current?.GetPluginState(this);
    }

    protected void SetProjectState(object state)
    {
        Project.Current?.SetPluginState(this, state);
    }

    protected object GetAssetState(Asset asset)
    {
        var project = Project.Current;
        if (project != null)
        {
            return project.GetAssetState(this, asset);
        }
        else
        {
            return null;
        }
    }

    protected void SetAssetState(Asset asset, object state)
    {
        var project = Project.Current;
        project?.SetAssetState(this, asset, state);
    } 
    #endregion

    public virtual object GetService(Type serviceType)
    {
        return null;
    }
}

/// <summary>
/// Third party plugin
/// </summary>
public abstract class ApiPlugin : Plugin
{
    protected ApiPlugin()
        : base(typeof(ApiPlugin))
    {
    }
}

/// <summary>
/// Editor plugin
/// </summary>
public abstract class BackendPlugin : Plugin
{
    protected BackendPlugin()
        : base(typeof(BackendPlugin))
    {
    }
}

/// <summary>
/// Editor plugin for UI and editing functionality.
/// </summary>
public abstract class EditorPlugin : Plugin
{
    public static bool RuntimeLogging = false;

    protected EditorPlugin() : base(typeof(EditorPlugin))
    {
    }
}

/// <summary>
/// Miscellaneous plugin for additional functionality.
/// </summary>
public abstract class MiscPlugin : Plugin
{
    protected MiscPlugin()
        : base(typeof(MiscPlugin))
    {
    }
}