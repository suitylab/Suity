using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

public class AgentGraphRunner : BaseLLmChat, IAgentGraphRunner
{
    readonly Dictionary<IAgent, AgentState> _agentStates = [];


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
        try
        {
            QueuedAction.Do(QueueRefreshView);
            StartNode.FlashingConnector();

            if (string.IsNullOrWhiteSpace(msg) || msg?.Trim() == "/resume")
            {
                return await HandleResume(option, cancelSource);
            }
            else
            {
                return await HandleNew(msg, option, cancelSource);
            }
        }
        finally
        {
            QueueRefreshView();
        }
    }

    private async Task<object> HandleNew(string msg, object option, CancellationTokenSource cancelSource)
    {
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

    public IAgentLoop AddLoop(IAgent agentNode, string name, string description, string prompt)
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
    public IAgentState GetAgentState(IAgent agent)
    {
        return _agentStates.GetValueSafe(agent);
    }

    public async Task<AICallResult> RunLoop(AIRequest request, IAgent agent, IAgentLoop loop)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (agent is null)
        {
            throw new ArgumentNullException(nameof(agent));
        }

        if (loop is null)
        {
            throw new ArgumentNullException(nameof(loop));
        }

        var agentState = EnsureAgentState(agent);
        var loopState = agentState.EnsureLoop(loop);
        if (loopState.IsRunning)
        {
            return AICallResult.FromFailed("Loop is running.");
        }

        var loopDoc = loop?.LoopAsset?.GetLoop() as AigcLoopDocument;
        if (loopDoc is null)
        {
            return AICallResult.Empty;
        }

        var workSpace = loopDoc.WorkSpace;
        if (workSpace != null)
        {
            StartNode.WorkSpace = workSpace;
        }

        var resume = new AIRequest(request, "/resume");
        resume.FuncContext.SetArgument(this);
        resume.FuncContext.SetArgument<IAgentGraphRunner>(this);
        resume.FuncContext.SetArgument<IAigcLoop>(loopDoc);

        resume.FuncContext.SetArgument(agent);
        resume.FuncContext.SetArgument(loop);

        resume.FuncContext.SetArgument(agentState);
        resume.FuncContext.SetArgument(loopState);

        resume.FuncContext.SetArgument(workSpace);

        var runner = new AigcLoopRunner(loopDoc);
        loopState.Runner = runner;

        return await runner.HandleRequest(resume);
    } 
    #endregion

    public AgentState EnsureAgentState(IAgent agent)
    {
        return _agentStates.GetOrAdd(agent, _ => new AgentState(this, agent));
    }

    public void QueueRefreshView()
    {
        foreach (var agent in _agentStates.Keys)
        {
            agent.QueueRefreshView();
        }
    }
}

public class AgentState : IAgentState
{
    readonly Dictionary<IAgentLoop, AgentLoopState> _loops = [];

    public AgentState(AgentGraphRunner parent, IAgent agent)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        Agent = agent ?? throw new ArgumentNullException(nameof(agent));
    }

    public AgentGraphRunner Parent { get; }

    public IAgent Agent { get; }

    public IAgentLoopState GetLoopState(IAgentLoop loop)
    {
        return _loops.GetValueSafe(loop);
    }

    public bool IsRunning => _loops.Values.Any(o => o.IsRunning);


    public AgentLoopState EnsureLoop(IAgentLoop loop)
    {
        return _loops.GetOrAdd(loop, _ => new AgentLoopState(this, loop));
    }
}

public class AgentLoopState : IAgentLoopState
{
    private AigcLoopRunner _runner;

    public AgentLoopState(AgentState parent, IAgentLoop loop)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        Loop = loop ?? throw new ArgumentNullException(nameof(loop));
    }

    public AgentState Parent { get; }

    internal AigcLoopRunner Runner
    {
        get => _runner;
        set
        {
            if (ReferenceEquals(_runner, value))
            {
                return;
            }

            var runner = _runner;
            runner?.TaskChanged -= OnTaskChanged;

            _runner = value;
            value?.TaskChanged += OnTaskChanged;
        }
    }

    private void OnTaskChanged(object sender, IAigcTaskPage e)
    {
        QueuedAction.Do(Parent.Agent.QueueRefreshView);
    }

    public IAgentLoop Loop { get; }

    public bool IsRunning => Runner?.IsRunning == true;
}