using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Views.Named;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

[NativeType(CodeBase = "Suity", Description = "Agent Node", Color = "#9900FF")]
[NativeAlias("Suity.Editor.AIGC.Agentic.IAgentNode")]
public interface IAgent : INamed
{
    IAgent ParentAgent { get; }

    IAgent[] GetSubAgents();

    void SetParentAgent(IAgent parent);


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
    IAgentState GetAgentState(IAgent agent);

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