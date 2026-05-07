using Suity.Editor.Flows.Nodes;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Flows.WorkSpaces;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// Selection list containing all available AIGC flow nodes for the node factory palette.
/// </summary>
internal class AigcFlowSelectionList : FlowNodeSelectionNode
{
    /// <summary>
    /// Gets the singleton instance of the AIGC flow selection list.
    /// </summary>
    public static AigcFlowSelectionList Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcFlowSelectionList"/> class.
    /// </summary>
    private AigcFlowSelectionList()
    {
        Add<WorkflowNode>();
        //Add<AgentFlowNode>();
        //Add<ToolFlowNode>();

        AddDerived<AigcFlowNode>();
        AddDerived<SubFlowNode>();
        AddDerived<TaskPageNode>();
        AddDerived<AigcXmlNode>();
        //AddDerived<AigcAgentNode>();
        AddDerived<AigcArticleNode>();
        //AddDerived<KnowledgeBaseFlowNode>();
        //AddDerived<AigcCodeDesignNode>();
        AddDerived<AigcWorkSpaceNode>();
        /*        AddDerived<AigcPlanNode>(
                    //condition: _ => ServiceInternals._license.GetCapability(EditorCapabilities.AigcPlanning)
                    );
        */

        AddDerived<ExternalNode>();

        AddDerived<ActionFlowNode>();
        AddDerived<DialogFlowNode>();
        AddDerived<TextFlowNode>();
        AddDerived<LinqFlowNode>();
        AddDerived<JsonFlowNode>();
        AddDerived<ValueFlowNode>();
        AddDerived<SValueFlowNode>();
        AddDerived<VariableFlowNode>();
    }

}
