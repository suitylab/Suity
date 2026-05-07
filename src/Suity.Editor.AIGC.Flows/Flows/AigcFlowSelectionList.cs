using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.AIGC.Flows.WorkSpaces;
using Suity.Editor.AIGC.FLows.External;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubGraphs;

namespace Suity.Editor.AIGC.Flows;

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
        AddDerived<AigcPageDefNode>();
        AddDerived<AigcPageNode>();
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

        AddDerived<AigcExternalNode>();

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
