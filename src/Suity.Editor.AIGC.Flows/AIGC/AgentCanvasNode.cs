using Suity.Drawing;
using Suity.Editor.Flows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;

namespace Suity.Editor.AIGC;

[DisplayText("Agent Start", "*CoreIcon|Agent")]
public class AgentStartCanvasNode : CanvasToolNode
{
    internal FlowNodeConnector _out;

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("Out", TypeDefinition.FromNative<IDataTransport>(), description: "Out");
        _out = AddConnector(output);
    }

    public override ImageDef Icon => CoreIconCache.Agent;
}

public class AgentCanvasNode : ExpandedCanvasAssetNode<SubFlowPresetAsset>
{
    internal FlowNodeConnector _out;
    internal FlowNodeConnector _in;

    readonly StringProperty _agentName = new("AgentName", "Agent Name");

    public AgentCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("Out", TypeDefinition.FromNative<IDataTransport>(), description: "Out");
        var input = FixedNodeConnector.CreateControlOutput("In", TypeDefinition.FromNative<IDataTransport>(), description: "In");

        _out = AddConnector(output);
        _in = AddConnector(input);
    }

    public override object GetTargetObject() => this;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _agentName.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(AssetRef, new ViewProperty(nameof(AssetRef), "Asset"));
        _agentName.InspectorField(setup);
    }

    public override string DisplayText => !string.IsNullOrWhiteSpace(_agentName.Text) ? _agentName.Text : base.DisplayText;

    #region IDrawExpandedImGui

    public override ImGuiNode OnExpandedGui(ImGui gui)
    {
        return gui.Text("OKOKOK");
    }

    #endregion
}
