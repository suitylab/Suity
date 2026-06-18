using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

public class AgentGraphRunner : BaseLLmChat
{
    public AgentStartCanvasNode StartNode { get; }
    public ICanvasDocument CanvasDocument { get; }

    public AgentGraphRunner(AgentStartCanvasNode node, FunctionContext ctx)
        : base(node.Name, node.ToDisplayTextL(), ctx)
    {
        StartNode = node ?? throw new ArgumentNullException(nameof(node));
        CanvasDocument = StartNode.Canvas ?? throw new ArgumentNullException(nameof(CanvasDocument));
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

        AddTask(node, name, StartNode.EntryTaskName, msg);

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

    public IAgentTask AddTask(IAgentNode agentNode, string name, string description, string prompt)
    {
        var startupPage = agentNode.PageAsset as ISubFlowAsset;
        if (startupPage is null)
        {
            return null;
        }

        var loopFormat = DocumentManager.Instance.GetDocumentFormat("AigcLoop");
        if (loopFormat is null)
        {
            return null;
        }

        var fileName = (CanvasDocument as Document)?.FileName;

        name = name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "TaskPage";
        }

        description = description?.Trim() ?? string.Empty;

        string currentPath = Path.GetDirectoryName(fileName?.PhysicFileName);
        currentPath = PathUtility.MakeRalativePath(currentPath, EditorServices.CurrentProject.AssetDirectory);

        var loopDocEntry = loopFormat.AutoNewDocument(name, currentPath);
        if (loopDocEntry is null)
        {
            return null;
        }

        var loopDoc = loopDocEntry.Content as AigcLoopDocument;
        if (loopDoc is null)
        {
            return null;
        }

        loopDoc.StartupPage = startupPage;
        loopDoc.InitialTaskPrompt = prompt;

        var startupTask = AigcWorkflowPage.CreateWorkflowPage(loopDoc, startupPage);
        if (startupTask is null)
        {
            return null;
        }

        if (startupTask.EnsureInstance() is null)
        {
            return null;
        }

        startupTask.SetPrompt(prompt);
        startupTask.SetScratchPad(ScratchPadTypes.Clear, null, null, null);
        loopDoc.AddTask(startupTask);
        loopDoc.MarkDirtyAndSaveDelayed(this);

        var loopAsset = loopDoc.TargetAsset as IAigcLoopAsset;
        var item = agentNode.AddTask(loopAsset, description);

        (CanvasDocument as Document)?.MarkDirtyAndSaveDelayed(this);
        agentNode.QueueRefreshView();

        return item;
    }


    protected override void HandleConversation(IConversationHandler handler)
    {
    }
}