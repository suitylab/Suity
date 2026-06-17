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

    AgentTaskItem AddTask(string name, string description, string prompt);

    Task<AICallResult> Run(AIRequest request);
}
