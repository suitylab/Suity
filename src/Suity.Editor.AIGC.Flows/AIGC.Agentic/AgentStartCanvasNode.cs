using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im.Flows;

namespace Suity.Editor.AIGC.Agentic;

[DisplayText("Agent Start", "*CoreIcon|Agent")]
[SimpleFlowNodeStyle(Color = FlowColors.AgentBg)]
[NativeAlias("Suity.Editor.AIGC.AgentStartCanvasNode")]
public class AgentStartCanvasNode : CanvasDesignNode
{
    internal FlowNodeConnector _in;

    private IAgentNode _agentNode;

    readonly StringProperty _entryTaskName = new(nameof(EntryTaskName), "Entry task name", "Entry");
    readonly AssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgentNode>(), false, description: "Out");
        _in = AddConnector(output);
    }

    public IAgentNode AgentNode => _agentNode;

    public string EntryTaskName => _entryTaskName.Text;

    /// <summary>
    /// Gets or sets the workspace associated with this task page document.
    /// </summary>
    public WorkSpace WorkSpace
    {
        get => _workSpace.Target?.WorkSpace;
        set
        {
            var workSpace = value?.GetAsset();

            if (_workSpace.Target == workSpace)
            {
                return;
            }

            _workSpace.Target = workSpace;
            (this.Canvas as Document)?.MarkDirtyAndSaveDelayed(this);
        }
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _entryTaskName.Sync(sync);
        _workSpace.Sync(sync);
    }

    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _entryTaskName.InspectorField(setup);
        _workSpace.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        _agentNode = compute.GetValue<IAgentNode>(_in);
    }

    internal void FlashingConnector()
    {
        _in.FlashingOnce();
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

