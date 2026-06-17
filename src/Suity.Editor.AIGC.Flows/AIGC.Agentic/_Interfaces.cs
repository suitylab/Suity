using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Types;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

[NativeType(CodeBase = "Suity", Description = "Agent Node", Color = "#9900FF")]
public interface IAgentNode
{
    void AddTask(string prompt);

    Task<AICallResult> Run(AIRequest request);
}
