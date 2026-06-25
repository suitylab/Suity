using Suity.Drawing;
using Suity.Editor.AIGC.Agentic;
using Suity.Editor.Types;

namespace Suity.Editor.Flows.Agentic;

[SimpleFlowNodeStyle(Color = FlowColors.AgentBg, Icon = "*CoreIcon|Agent", HasHeader = false, Width = 100, Height = 20)]
public class GetSubAgentList : AgentNode
{
    readonly FlowNodeConnector _agents;


    public GetSubAgentList()
    {
        var type = TypeDefinition.FromNative<IAgent>().MakeArrayType();
        _agents = this.AddDataOutputConnector("Agents", type, "Agents");
    }

    public override ImageDef Icon => CoreIconCache.Agent;

    public override void Compute(IFlowComputation compute)
    {
        var agent = compute.Context.GetArgument<IAgent>();

        var subAgents = agent?.GetSubAgents() ?? [];
        compute.SetValue(_agents, subAgents);
    }
}

[SimpleFlowNodeStyle(Color = FlowColors.AgentBg, Icon = "*CoreIcon|Agent", HasHeader = false, Width = 100, Height = 20)]
public class GetParentAgent : AgentNode
{
    readonly FlowNodeConnector _parent;


    public GetParentAgent()
    {
        _parent = this.AddDataOutputConnector("Parent", TypeDefinition.FromNative<IAgent>(), "Parent");
    }

    public override ImageDef Icon => CoreIconCache.Agent;

    public override void Compute(IFlowComputation compute)
    {
        var agent = compute.Context.GetArgument<IAgent>();

        var parent = agent.ParentAgent;
        compute.SetValue(_parent, parent);
    }
}