using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

[DisplayText("Agent Start", "*CoreIcon|Agent")]
[NativeAlias("Suity.Editor.AIGC.AgentStartCanvasNode")]
public class AgentStartCanvasNode : CanvasDesignNode
{
    internal FlowNodeConnector _in;

    private IAgentNode _agentNode;

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgentNode>(), false, description: "Out");
        _in = AddConnector(output);
    }

    public IAgentNode AgentNode => _agentNode;

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
            return new AgentChat(node, context);
        }

        return null;
    } 

    #endregion
}

public class AgentChat : BaseLLmChat
{
    public AgentStartCanvasNode StartNode { get; }

    public AgentChat(AgentStartCanvasNode node, FunctionContext ctx)
        : base(node.Name, node.ToDisplayTextL(), ctx)
    {
        StartNode = node ?? throw new ArgumentNullException(nameof(node));
    }

    protected override async Task<object> HandleStart(string msg, object option, CancellationTokenSource cancelSource)
    {
        var node = StartNode.AgentNode;
        if (node is null)
        {
            return null;
        }

        var request = new AIRequest
        {
            UserMessage = msg ?? string.Empty,
            Conversation = _conversation,
            FuncContext = this._context,
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = () =>
            {
                cancelSource.Cancel();
                // _conversation.AddSystemMessage(L("Request cancelled."));
            }
        };

        var result = await node.Run(request);

        //this._conversation.AddSystemMessage("OK");

        return result?.Result;
    }

    protected override void HandleConversation(IConversationHandler handler)
    {
    }
}