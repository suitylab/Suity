using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
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
using static Suity.Helpers.GlobalLocalizer;

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
        if (StartNode.IsTemplate)
        {
            string errorMsg = L("This is a template agent graph, can't run.");
            _conversation.AddErrorMessage(errorMsg);
            return AICallResult.FromFailed(errorMsg);
        }

        try
        {
            CanvasDocument.ComputeConnections();

            QueuedAction.Do(QueueRefreshView);
            StartNode.FlashingConnector();

            AICallResult result = null;

            if (string.IsNullOrWhiteSpace(msg) || msg?.Trim() == "/resume")
            {
                result = await HandleResume(option, cancelSource);
            }
            else
            {
                result = await HandleNew(msg, option, cancelSource);
            }

            string resultMsg = result?.Message;
            if (string.IsNullOrWhiteSpace(msg))
            {
                resultMsg = null;
            }

            if (result?.Status == AICallStatus.Failed)
            {
                _conversation.AddErrorMessage(resultMsg ?? "Unkown Error.");
            }
            else
            {
                _conversation.AddSystemMessage(resultMsg ?? "Agent Graph Finished.");
            }

            return result?.Result;
        }
        finally
        {
            QueueRefreshView();
        }
    }

    private async Task<AICallResult> HandleNew(string msg, object option, CancellationTokenSource cancelSource)
    {
        var starter = StartNode.AgentNode;
        if (starter is null)
        {
            throw new NullReferenceException("Starter agent node is not connected.");
        }

        if (starter.StarterWorkflow is null)
        {
            throw new NullReferenceException("Agent workflow not set: " + starter.AgentName);
        }

        GetOrAddEntryLoop(starter, StartNode.EntryTaskName, msg);

        var request = new AIRequest
        {
            UserMessage = msg ?? string.Empty,
            Conversation = _conversation,
            FuncContext = new FunctionContext(this._context),
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = cancelSource.Cancel
        };

        request.FuncContext.SetArgument(request);

        var result = await starter.Run(request, this);

        return result;
    }

    private async Task<AICallResult> HandleResume(object option, CancellationTokenSource cancelSource)
    {
        var starter = StartNode.AgentNode;
        if (starter is null)
        {
            return null;
        }

        _conversation.AddRunningMessage(L("Resuming agent execution..."));

        var request = new AIRequest
        {
            UserMessage = "/resume",
            Conversation = _conversation,
            FuncContext = new FunctionContext(this._context),
            Option = option,
            Cancellation = cancelSource.Token,
            RequestCancel = cancelSource.Cancel
        };

        request.FuncContext.SetArgument(request);

        var result = await starter.Run(request, this);

        return result;
    }

    private IAgentLoop GetOrAddEntryLoop(IAgent agent, string description, string prompt)
    {
        var loops = agent.GetLoops().Where(o => o.Description == description).ToArray();
        if (loops.Length == 0)
        {
            return AddLoop(agent, description, prompt);
        }

        var loop = loops[loops.Length - 1];
        var loopDoc = loop.LoopAsset.GetLoop() as AigcLoopDocument;
        if (loopDoc is null)
        {
            throw new NullReferenceException("Failed to create loop document.");
        }

        var startupWorkflow = loopDoc.NewTaskPrompt(prompt);

        loopDoc.MarkDirtyAndSaveDelayed(this);

        return loop;
    }


    protected override void HandleConversation(IConversationHandler handler)
    {
    }

    #region IAgentGraphRunner

    public IAgentState GetAgentState(IAgent agent)
    {
        return _agentStates.GetValueSafe(agent);
    }

    public IAgentLoop AddLoop(IAgent agent, string description, string prompt, string loopFileName = null)
    {
        var startupPage = agent.StarterWorkflow;
        if (startupPage is null)
        {
            throw new NullReferenceException("Agent workflow not set: " + agent.AgentName);
        }

        var loopFormat = DocumentManager.Instance.GetDocumentFormat("AigcLoop");
        if (loopFormat is null)
        {
            throw new NullReferenceException("AigcLoop document format not found.");
        }

        var fileName = (CanvasDocument as Document)?.FileName;

        loopFileName = loopFileName?.Trim();
        if (string.IsNullOrWhiteSpace(loopFileName))
        {
            loopFileName = agent.StarterWorkflow?.Name;
        }
        if (string.IsNullOrWhiteSpace(loopFileName))
        {
            loopFileName = "TaskPage";
        }

        description = description?.Trim() ?? string.Empty;

        string currentFullPath = Path.GetDirectoryName(fileName?.PhysicFileName);
        string currentPath = PathUtility.MakeRalativePath(currentFullPath, EditorServices.CurrentProject.AssetDirectory);

        string subDir = ResolveDirectoryName(agent.AgentName);
        currentPath = currentPath.PathAppend(subDir);

        var loopDocEntry = loopFormat.AutoNewDocument(loopFileName, currentPath);
        if (loopDocEntry is null)
        {
            throw new NullReferenceException("Failed to create loop document.");
        }

        var loopDoc = loopDocEntry.Content as AigcLoopDocument;
        if (loopDoc is null)
        {
            throw new NullReferenceException("Failed to create loop document.");
        }

        loopDoc.StartupPage = startupPage;
        loopDoc.InitialTaskPrompt = prompt;
        loopDoc.Attributes.AddAttribute<UsageAttribute>(o => o.Usage = "AgentGraph");

        var startupWorkflow = loopDoc.NewTaskPrompt(prompt);
        
        loopDoc.WorkSpace = this.WorkSpace;
        loopDoc.MarkDirtyAndSaveDelayed(this);

        var loopAsset = loopDoc.TargetAsset as IAigcLoopAsset;
        var item = agent.AddLoop(loopAsset, description);

        (CanvasDocument as Document)?.MarkDirtyAndSaveDelayed(this);
        agent.QueueRefreshView();

        return item;
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

        if (loop?.LoopAsset?.GetLoop() is not AigcLoopDocument loopDoc)
        {
            return AICallResult.Empty;
        }

        var workSpace = loopDoc.WorkSpace;
        if (workSpace != null)
        {
            StartNode.WorkSpace = workSpace;
        }

        // workflow is created by loopDoc.NewTaskPrompt(prompt)
        //var resume = new AIRequest(request);
        var resume = new AIRequest(request, "/resume");

        resume.FuncContext.SetArgument(this);
        resume.FuncContext.SetArgument(resume);

        resume.FuncContext.SetArgument<IAgentGraphRunner>(this);
        resume.FuncContext.SetArgument<IAigcLoop>(loopDoc);

        resume.FuncContext.SetArgument(agent);
        resume.FuncContext.SetArgument(loop);

        resume.FuncContext.SetArgument(agentState);
        resume.FuncContext.SetArgument(loopState);

        resume.FuncContext.SetArgument(workSpace);

        var runner = new AigcLoopRunner(loopDoc);
        loopState.Runner = runner;

        QueueRefreshView();

        try
        {
            return await runner.HandleRequest(resume);
        }
        finally
        {
            QueueRefreshView();
        }
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

    public static string ResolveDirectoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Unnamed";
        }

        name = name.Replace(' ', '_');
        name = name.Replace('/', '_');
        name = name.Replace('\\', '_');
        name = name.Replace(':', '_');
        name = name.Replace('*', '_');
        name = name.Replace('?', '_');
        name = name.Replace('"', '_');
        name = name.Replace('<', '_');
        name = name.Replace('>', '_');
        name = name.Replace('|', '_');

        return name;
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

    public IAgentLoopState GetLoopState(IAgentLoop loop) => _loops.GetValueSafe(loop);

    public IEnumerable<IAgentLoopState> GetLoopStates() => _loops.Values;

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