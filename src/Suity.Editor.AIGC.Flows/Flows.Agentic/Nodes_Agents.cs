using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Agentic;
using Suity.Editor.Types;

namespace Suity.Editor.Flows.Agentic;

[SimpleFlowNodeStyle(Color = FlowColors.AgentBg, HasHeader = false, Width = 100, Height = 20)]
public class GetSubAgentList : AgentNode
{
    readonly FlowNodeConnector _agents;


    public GetSubAgentList()
    {
        var type = TypeDefinition.FromNative<IAgentNode>().MakeArrayType();
        _agents = this.AddDataOutputConnector("Agents", type, "Agents");
    }

    public override ImageDef Icon => CoreIconCache.Agent;

    public override void Compute(IFlowComputation compute)
    {
        var loopRunner = compute.Context.GetArgument<IAigcLoopRunner>();
        var agentRunner = compute.Context.GetArgument<IAgentGraphRunner>();

        var workflowPage = compute.Context.GetArgument<IAigcWorkflowPage>();
        var loop = workflowPage?.ParentLoop;

        var agent = compute.Context.GetArgument<IAgentNode>();
    }
}
