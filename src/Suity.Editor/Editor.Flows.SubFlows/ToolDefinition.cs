using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;

namespace Suity.Editor.Flows.SubFlows;

[NativeAbstract("*Suity|ToolDefinition")]
public class ToolDefinition : SObjectController
{
    public override string ToString()
    {
        var attr = GetType().GetAttributeCached<NativeTypeAttribute>();

        return attr?.Description ?? attr?.Name ?? GetType().Name;
    }
}
