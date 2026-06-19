using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Agentic;

namespace Suity.Editor.Flows.Agentic;

[SimpleFlowNodeStyle(Color = FlowColors.AgentBg, HasHeader = false, Width = 100, Height = 20)]
public class GetSubAgentList : AgentNode
{
    readonly FlowNodeConnector _agents;


    public GetSubAgentList()
    {
        var type = AssetManager.Instance.GetAssetLink<IAgentNode>().Definition.MakeArrayType();
        _agents = this.AddDataOutputConnector("Agents", type, "Agents");
    }

    public override ImageDef Icon => CoreIconCache.Agent;

    public override void Compute(IFlowComputation compute)
    {
        var runner = compute.Context.GetArgument<IAgentGraphRunner>();
        var workflowPage = compute.Context.GetArgument<IAigcWorkflowPage>();
    }
}
