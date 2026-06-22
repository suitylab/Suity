using Suity.Editor.AIGC.Agentic;
using Suity.Editor.Services;
using System;

namespace Suity.Editor.Flows.Agentic;

public class IAgentNodeToTextConverter : TypeToTextConverter<IAgent>
{
    public override string Convert(IAgent objFrom)
    {
        string newline = Environment.NewLine;

        return $"<Agent name='{objFrom.AgentName}'>{newline}{objFrom.Overview}{newline}</Agent>";
    }
}
