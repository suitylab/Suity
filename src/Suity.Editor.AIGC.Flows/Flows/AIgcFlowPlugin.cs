using Suity.Drawing;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Plugin for configuring AIGC workflow settings and behavior.
/// </summary>
[NotAvailable]
public class AIgcFlowPlugin : EditorPlugin, IViewObject
{
    /// <summary>
    /// Gets the singleton instance of the AIGC flow plugin.
    /// </summary>
    public static AIgcFlowPlugin Instance { get; private set; }

    private readonly ValueProperty<bool> _prioritizeTaskSubdivide
        = new("PrioritizeTaskSubdivide", "Prioritize Task Subdivide", toolTips: "When a task can both be submitted and have task subdivision, execute task subdivision.");

    private readonly LabelProperty _funcCallLabel
        = new("FuncCallLabel", "Function Call", toolTips: "When the language model does not support function calling, use function call prompt words to indicate the model output format.", CoreIconCache.Function);

/*    private readonly ListProperty<AigcToolFeatures> _defaultToolFeatures
        = new("DefaultToolFeatures", "Default Expected Features", "When multiple tools implement the same functionality, prioritize tools with this feature.");*/

    private readonly LabelProperty _debugLabel
        = new("DebugLabel", "Debug", icon: CoreIconCache.Debug);


    private readonly ValueProperty<bool> _pauseOnAICall
        = new("PauseOnAICall", "Pause on AI Call");

    private readonly ValueProperty<bool> _pauseOnAILog
        = new("PauseOnAILog", "Pause on AI Log");

    /// <summary>
    /// Initializes a new instance of the <see cref="AIgcFlowPlugin"/> class.
    /// </summary>
    public AIgcFlowPlugin()
    {
        Instance ??= this;
    }

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public override string Description => "AI Workflow";

    /// <summary>
    /// Gets the icon for the plugin.
    /// </summary>
    public override ImageDef Icon => CoreIconCache.Workflow;

    /// <summary>
    /// Gets a value indicating whether task subdivision is prioritized over task submission.
    /// </summary>
    public bool PrioritizeTaskSubdivide => _prioritizeTaskSubdivide.Value;

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI calls.
    /// </summary>
    public bool PauseOnAICall => _pauseOnAICall.Value;

    /// <summary>
    /// Gets a value indicating whether execution should pause on AI log output.
    /// </summary>
    public bool PauseOnAILog => _pauseOnAILog.Value;

    /*public AigcToolFeatures[] DefaultToolFeatures => _defaultToolFeatures.List.ToArray();*/

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _prioritizeTaskSubdivide.Sync(sync);

        /*_defaultToolFeatures.Sync(sync);*/

        _pauseOnAICall.Sync(sync);
        _pauseOnAILog.Sync(sync);
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.LabelWithIcon("Plan", CoreIconCache.Plan);
        _prioritizeTaskSubdivide.InspectorField(setup);

        _funcCallLabel.InspectorField(setup);
        /*_defaultToolFeatures.InspectorField(setup);*/

        _debugLabel.InspectorField(setup);
        _pauseOnAICall.InspectorField(setup);
        _pauseOnAILog.InspectorField(setup);
    }
}