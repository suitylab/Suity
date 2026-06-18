using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

public class AgentGraphRunner : BaseLLmChat, IAgentGraphRunner
{
    public AgentStartCanvasNode StartNode { get; }
    public ICanvasDocument CanvasDocument { get; }

    public AgentGraphRunner(AgentStartCanvasNode node, FunctionContext ctx)
        : base(node.Name, node.ToDisplayTextL(), ctx)
    {
        StartNode = node ?? throw new ArgumentNullException(nameof(node));
        CanvasDocument = StartNode.Canvas ?? throw new ArgumentNullException(nameof(CanvasDocument));
    }

    public WorkSpace WorkSpace => StartNode.WorkSpace;


    protected override async Task<object> HandleStart(string msg, object option, CancellationTokenSource cancelSource)
    {
        if (string.IsNullOrWhiteSpace(msg) || msg?.Trim() == "/resume")
        {
            return await HandleResume(option, cancelSource);
        }

        var starter = StartNode.AgentNode;
        if (starter is null)
        {
            return null;
        }

        string name = starter.PageAsset?.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        AddLoop(starter, name, StartNode.EntryTaskName, msg);

        var request = new AIRequest
        {
            UserMessage = msg ?? string.Empty,
            Conversation = _conversation,
            FuncContext = this._context,
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = cancelSource.Cancel
        };

        var result = await starter.Run(request, this);

        return result?.Result;
    }

    private async Task<object> HandleResume(object option, CancellationTokenSource cancelSource) 
    {
        var starter = StartNode.AgentNode;
        if (starter is null)
        {
            return null;
        }

        var request = new AIRequest
        {
            UserMessage = "/resume",
            Conversation = _conversation,
            FuncContext = this._context,
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = cancelSource.Cancel
        };

        var result = await starter.Run(request, this);

        return result?.Result;
    }

    public IAgentLoop AddLoop(IAgentNode agentNode, string name, string description, string prompt)
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

        var startupWorkflow = AigcWorkflowPage.CreateWorkflowPage(loopDoc, startupPage);
        if (startupWorkflow is null)
        {
            return null;
        }

        if (startupWorkflow.EnsureInstance() is null)
        {
            return null;
        }

        startupWorkflow.SetPrompt(prompt);
        startupWorkflow.SetScratchPad(ScratchPadTypes.Clear, null, null, null);

        loopDoc.AddTask(startupWorkflow);
        loopDoc.MarkDirtyAndSaveDelayed(this);
        loopDoc.WorkSpace = this.WorkSpace;

        var loopAsset = loopDoc.TargetAsset as IAigcLoopAsset;
        var item = agentNode.AddLoop(loopAsset, description);

        (CanvasDocument as Document)?.MarkDirtyAndSaveDelayed(this);
        agentNode.QueueRefreshView();

        return item;
    }


    protected override void HandleConversation(IConversationHandler handler)
    {
    }

    #region IAgentGraphRunner
    public IAgentState GetAgentState(IAgentNode agent)
    {
        return null;
    }

    public async Task<AICallResult> RunLoop(AIRequest request, IAgentNode agent, IAgentLoop loop)
    {
        var loopDoc = loop?.LoopAsset?.GetLoop() as AigcLoopDocument;
        if (loopDoc is null)
        {
            return AICallResult.Empty;
        }

        if (loopDoc.WorkSpace is { } workSpace)
        {
            StartNode.WorkSpace = workSpace;
        }

        var resume = new AIRequest(request, "/resume");

        var runner = new AigcLoopRunner(loopDoc);
        return await runner.HandleRequest(resume);
    } 
    #endregion
}

