using Suity.Drawing;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC.Agentic;

[DisplayText("Agent Start", "*CoreIcon|Agent")]
[NativeAlias("Suity.Editor.AIGC.AgentStartCanvasNode")]
public class AgentStartCanvasNode : CanvasDesignNode
{
    internal FlowNodeConnector _in;

    private IAgentNode _agentNode;

    readonly StringProperty _entryTaskName = new(nameof(EntryTaskName), "Entry task name", "Entry");

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgentNode>(), false, description: "Out");
        _in = AddConnector(output);
    }

    public IAgentNode AgentNode => _agentNode;

    public string EntryTaskName => _entryTaskName.Text;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _entryTaskName.Sync(sync);
    }

    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _entryTaskName.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        _agentNode = compute.GetValue<IAgentNode>(_in);
    }
}

public class AgentStartDiagramItem : FlowDiagramItem<AgentStartCanvasNode, AgentStartAssetBuilder>
{
    protected internal override string OnGetSuggestedPrefix() => "Agent";

    protected internal override bool OnVerifyName(string name) => NamingVerifier.VerifyIdentifier(name);
}

public class AgentStartAssetBuilder : AssetBuilder<AgentStartAsset>
{
}

public class AgentStartAsset : Asset, ILLmChatProvider
{
    public AgentStartAsset()
    {
        UpdateAssetTypes(this.GetType(), typeof(ILLmChatProvider));
    }

    public override ImageDef DefaultIcon => CoreIconCache.Agent;

    #region ILLmChatProvider

    public ILLmChat CreateChat(FunctionContext context)
    {
        var node = (this.GetStorageObject(true) as AgentStartDiagramItem)?.Node;
        if (node != null)
        {
            return new AgentGraphRunner(node, context);
        }

        return null;
    } 

    #endregion
}

