using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

[NativeType(CodeBase = "Suity", Description = "Agent Node", Color = "#9900FF")]
[NativeAlias("Suity.Editor.AIGC.Agentic.IAgentNode")]
public interface IAgent
{
    string AgentName { get; }

    IAgent ParentAgent { get; }

    IAgent[] GetSubAgents();

    void SetParentAgent(IAgent parent);


    ISubFlowAsset StarterWorkflow { get; }

    IAgentLoop[] GetLoops();

    IAgentLoop AddLoop(IAigcLoopAsset loopAsset, string description);

    Task<AICallResult> Run(AIRequest request, IAgentGraphRunner runner);

    void QueueRefreshView();

    void FlashingConnector(FlowDirections direction);
}

public interface IAgentLoop
{
    IAigcLoopAsset LoopAsset { get; }
}


public interface IAgentGraphRunner
{
    IAgentState GetAgentState(IAgent agent);

    IAgentLoop AddLoop(IAgent agent, string description, string prompt, string loopFileName = null);

    Task<AICallResult> RunLoop(AIRequest request, IAgent agent, IAgentLoop loop);
}

public interface IAgentState
{
    IAgent Agent { get; }

    IAgentLoopState GetLoopState(IAgentLoop loop);

    bool IsRunning { get; }
}

public interface IAgentLoopState
{
    IAgentLoop Loop { get; }

    bool IsRunning { get; }
}