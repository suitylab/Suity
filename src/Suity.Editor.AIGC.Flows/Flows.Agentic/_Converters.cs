using Suity.Editor.AIGC.Agentic;
using Suity.Editor.Services;

namespace Suity.Editor.Flows.Agentic;

public class IAgentNodeToTextConverter : TypeToTextConverter<IAgentNode>
{
    public override string Convert(IAgentNode objFrom)
    {
        return objFrom.Name;
    }
}
