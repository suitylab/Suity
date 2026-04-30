namespace Suity.Editor.Flows;

/// <summary>
/// Plugin that provides flow-based visual scripting functionality.
/// </summary>
public class FlowPlugin : EditorPlugin
{
    /// <inheritdoc/>
    public override string Description => "Flows";

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowPlugin"/> class.
    /// </summary>
    public FlowPlugin()
    {
    }

    /// <inheritdoc/>
    protected internal override void Awake(PluginContext context)
    {
        base.Awake(context);

        FlowsExternalBK.Instance.Initialize();
    }
}