using Suity.Editor.AIGC.Agentic;
using Suity.Editor.Services;

namespace Suity.Editor.Flows.Agentic;

public class IAgentNodeToTextConverter : TypeToTextConverter<IAgent>
{
    public override string Convert(IAgent objFrom)
    {
        return objFrom.Name;
    }
}
