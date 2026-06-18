using Suity.Editor.AIGC.Assistants;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

public class AgentGraphRunner : BaseLLmChat
{
    public AgentStartCanvasNode StartNode { get; }

    public AgentGraphRunner(AgentStartCanvasNode node, FunctionContext ctx)
        : base(node.Name, node.ToDisplayTextL(), ctx)
    {
        StartNode = node ?? throw new ArgumentNullException(nameof(node));
    }

    protected override async Task<object> HandleStart(string msg, object option, CancellationTokenSource cancelSource)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return null;
        }

        var node = StartNode.AgentNode;
        if (node is null)
        {
            return null;
        }

        string name = node.PageAsset?.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        node.AddTask(name, StartNode.EntryTaskName, msg);

        var request = new AIRequest
        {
            UserMessage = msg ?? string.Empty,
            Conversation = _conversation,
            FuncContext = this._context,
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = cancelSource.Cancel
        };

        var result = await node.Run(request);

        return result?.Result;
    }

    protected override void HandleConversation(IConversationHandler handler)
    {
    }
}