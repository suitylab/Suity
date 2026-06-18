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

    IAgentTask AddTask(IAigcLoopAsset loopAsset, string description);

    Task<AICallResult> Run(AIRequest request);

    void QueueRefreshView();
}

public interface IAgentTask
{
    IAigcLoopAsset LoopAsset { get; }
}


public interface IAgentGraphRunner
{
    IAgentState GetAgentState(IAgentNode agent);
}

public interface IAgentState
{
    IAgentTaskState GetTaskState(IAgentTask task);
}

public interface IAgentTaskState
{

}