using Suity.Drawing;
using Suity.Editor.Flows;
using Suity.Editor.Services;
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
    internal FlowNodeConnector _out;

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("Out", TypeDefinition.FromNative<IDataTransport>(), description: "Out");
        _out = AddConnector(output);
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
        return null;
    }

    protected override void HandleConversation(IConversationHandler handler)
    {
    }
}