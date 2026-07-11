using Suity.Editor.AIGC.Agentic;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("CallSubAgent", CodeBase = "*Suity", Category = "Agent Tools")]
[DisplayText("Call Sub-Agent")]
[ToolTipsText("Call a sub-agent to execute task loops. Each loop item runs independently as a separate loop.")]
public class CallSubAgent : ToolCommand<CallSubAgent.Output>
{
    #region LoopItem
    [NativeType("CallAgent.LoopItem", CodeBase = "*Suity")]
    public class LoopItem : SObjectController
    {
        readonly StringProperty _loopName = new("LoopName", "Loop Name", null, "Loop name (with id prefix)");
        readonly TextBlockProperty _prompt = new("Prompt", "Prompt");

        public string LoopName { get => _loopName.Text; set => _loopName.Text = value; }
        public string Prompt { get => _prompt.Text; set => _prompt.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _loopName.Sync(sync);
            _prompt.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _loopName.InspectorField(setup);
            _prompt.InspectorField(setup);
        }
        public override string ToString() => $"{LoopName}";
    }
    #endregion

    #region LoopResult
    [NativeType("CallAgent.LoopResult", CodeBase = "*Suity")]
    public class LoopResult : SObjectController
    {
        readonly StringProperty _loopName = new("LoopName", "Loop Name");
        readonly TextBlockProperty _result = new("Result", "Result");
        readonly StringProperty _error = new("Error", "Error");

        public string LoopName { get => _loopName.Text; set => _loopName.Text = value; }
        public string Result { get => _result.Text; set => _result.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _loopName.Sync(sync);
            _result.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _loopName.InspectorField(setup);
            _result.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{LoopName}" + (HasError ? $" - Error: {Error}" : "");
    }
    #endregion

    #region Output
    public class Output : SObjectController
    {
        readonly ListProperty<LoopResult> _results = new("Results", "Results");
        readonly ValueProperty<int> _successCount = new("SuccessCount", "Success Count");
        readonly ValueProperty<int> _failCount = new("FailCount", "Fail Count");

        public List<LoopResult> Results => _results.List;
        public int SuccessCount { get => _successCount.Value; set => _successCount.Value = value; }
        public int FailCount { get => _failCount.Value; set => _failCount.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _results.Sync(sync);
            _successCount.Sync(sync);
            _failCount.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _results.InspectorField(setup);
            _successCount.InspectorField(setup);
            _failCount.InspectorField(setup);
        }

        public override string ToString() => $"Results: {SuccessCount} succeeded, {FailCount} failed ({Results.Count} items)";
    } 
    #endregion

    readonly StringProperty _agentName = new("AgentName", "Agent Name");
    readonly ListProperty<LoopItem> _loops = new("Loops", "Loops", "List of loop items to execute.");

    public string AgentName { get => _agentName.Text; set => _agentName.Text = value; }
    public List<LoopItem> Loops => _loops.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _agentName.Sync(sync);
        _loops.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _agentName.InspectorField(setup);
        _loops.InspectorField(setup);
    }

    public override async Task<Output> Run(ToolCallContext context)
    {
        var myAgent = context.FuncContext.GetArgument<IAgent>();
        if (myAgent is null)
        {
            throw new NullReferenceException("Agent is not set");
        }

        var request = context.FuncContext.GetArgument<AIRequest>();
        if (request is null)
        {
            throw new NullReferenceException("AI request is not set");
        }

        var toolPage = context.FuncContext.GetArgument<IAigcToolPage>();
        if (toolPage is null)
        {
            throw new NullReferenceException("Tool page is not set");
        }

        (toolPage as AigcTaskPage)?.CommitStatus = TaskCommitStatus.Delegating;

        myAgent.FlashingConnector(FlowDirections.Input);

        var subAgents = myAgent?.GetSubAgents() ?? [];

        string agentName = AgentName;
        if (string.IsNullOrWhiteSpace(agentName))
        {
            throw new NullReferenceException("Agent name is not set");
        }

        var subAgent = subAgents.FirstOrDefault(o => o.AgentName == agentName);
        if (subAgent is null)
        {
            throw new NullReferenceException($"Agent '{agentName}' not found");
        }

        var runner = context.FuncContext.GetArgument<IAgentGraphRunner>();
        if (runner is null)
        {
            throw new NullReferenceException("Agent graph runner is not set");
        }

        List<IAgentLoop> loops = [];
        var loopRecords = toolPage.GetAttributes<SubAgentLoopIdAttribute>().ToArray();
        if (loopRecords.Length == 0)
        {
            foreach (var loopItem in _loops.List)
            {
                var loop = runner.AddLoop(subAgent, loopItem.LoopName, loopItem.Prompt);
                loops.Add(loop);

                toolPage.AddAttribute<SubAgentLoopIdAttribute>(o => o.Id = loop.Id);
            }
        }
        else
        {
            HashSet<string> ids = [..loopRecords.Select(o => o.Id).Where(id => !string.IsNullOrWhiteSpace(id))];
            foreach (var loop in subAgent.GetLoops())
            {
                if (!ids.Contains(loop.Id))
                {
                    continue;
                }

                loops.Add(loop);
            }

            if (loops.Count != loopRecords.Length)
            {
                var missingIds = loopRecords.Where(o => !loops.Any(l => l.Id == o.Id)).Select(o => o.Id).ToArray();
                throw new NullReferenceException($"Loop records with ids '{string.Join(", ", missingIds)}' not found");
            }
        }

        var output = new Output();
        int successCount = 0;
        int failCount = 0;

        context.ToolInstance.Conversation?.AddRunningMessage($"Call agent '{agentName}' with {Loops.Count} loop(s)", msg =>
        {
            msg.AddCode(string.Join(", ", Loops.Select(l => l.LoopName)));
        });
        context.Conversation?.AddRunningMessage($"Call agent '{agentName}' with {Loops.Count} loop(s)", msg =>
        {
            msg.AddCode(string.Join(", ", Loops.Select(l => l.LoopName)));
        });

        foreach (var loop in loops)
        {
            var loopResult = new LoopResult { LoopName = loop.Description };
            try
            {
                var result = await runner.RunLoop(request, subAgent, loop);
                context.Cancellation.ThrowIfCancellationRequested();

                if (result.Status == AICallStatus.Failed)
                {
                    loopResult.Error = result.Message ?? "Unknown error";
                    failCount++;
                }
                else
                {
                    loopResult.Result = result.Message ?? result.Result?.ToString() ?? "Success";
                    successCount++;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception err)
            {
                loopResult.Error = err.Message;
                failCount++;
            }
            finally
            {
                myAgent.QueueRefreshView();
                subAgent.QueueRefreshView();
            }

            output.Results.Add(loopResult);
        }

        context.Cancellation.ThrowIfCancellationRequested();

        output.SuccessCount = successCount;
        output.FailCount = failCount;

        return output;
    }
}

[NativeType(CodeBase = "*AIGC", Name = "SubAgentLoopId", Description = "Sub-Agent Loop Id", Icon = "*CoreIcon|Loop")]
public class SubAgentLoopIdAttribute : DesignAttribute
{
    readonly StringProperty _id = new("Id");

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _id.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _id.InspectorField(setup);
    }

    public string Id
    {
        get => _id.Text;
        set => _id.Text = value;
    }

    public override string ToString() => _id.Text;
}