namespace Suity.Editor;

/// <summary>
/// Plugin constructor context.
/// </summary>
public class PluginContext
{
    private readonly PluginInfo _pluginInfo;

    public PluginContext(PluginInfo pluginInfo)
    {
        this._pluginInfo = pluginInfo;
    }
}