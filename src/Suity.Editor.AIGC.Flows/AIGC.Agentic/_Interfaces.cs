using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Views.Named;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

[NativeType(CodeBase = "Suity", Description = "Agent Node", Color = "#9900FF")]
public interface IAgentNode : INamed
{
    IPageAsset PageAsset { get; }

    IAgentLoop AddLoop(IAigcLoopAsset loopAsset, string description);

    Task<AICallResult> Run(AIRequest request, IAgentGraphRunner runner);

    void QueueRefreshView();
}

public interface IAgentLoop
{
    IAigcLoopAsset LoopAsset { get; }
}


public interface IAgentGraphRunner
{
    IAgentState GetAgentState(IAgentNode agent);

    Task<AICallResult> RunLoop(AIRequest request, IAgentNode agent, IAgentLoop loop);
}

public interface IAgentState
{
    IAgentLoopState GetLoopState(IAgentLoop loop);
}

public interface IAgentLoopState
{
    IAgentLoop AgentLoop { get; }

    bool IsRunning { get; }

    Task<AICallResult> Run(AIRequest request);
}