using Suity.Editor.AIGC.Agentic;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
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
[ToolTipsText("Call a sub-agent to execute tasks. Each loop item runs independently as a separate loop.")]
public class CallSubAgent : ToolCommand<CallSubAgent.Output>
{
    [NativeType("CallAgent.LoopItem", CodeBase = "*Suity")]
    public class LoopItem : SObjectController
    {
        readonly StringProperty _taskName = new("TaskName", "Task Name");
        readonly TextBlockProperty _prompt = new("Prompt", "Prompt");

        public string TaskName { get => _taskName.Text; set => _taskName.Text = value; }
        public string Prompt { get => _prompt.Text; set => _prompt.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _taskName.Sync(sync);
            _prompt.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _taskName.InspectorField(setup);
            _prompt.InspectorField(setup);
        }
        public override string ToString() => $"{TaskName}";
    }

    [NativeType("CallAgent.LoopResult", CodeBase = "*Suity")]
    public class LoopResult : SObjectController
    {
        readonly StringProperty _taskName = new("TaskName", "Task Name");
        readonly TextBlockProperty _result = new("Result", "Result");
        readonly StringProperty _error = new("Error", "Error");

        public string TaskName { get => _taskName.Text; set => _taskName.Text = value; }
        public string Result { get => _result.Text; set => _result.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _taskName.Sync(sync);
            _result.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _taskName.InspectorField(setup);
            _result.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{TaskName}" + (HasError ? $" - Error: {Error}" : "");
    }

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

    public override Task<Output> Run(ToolCallContext context)
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

        myAgent.FlashingConnector(FlowDirections.Input);


        var subAgents = myAgent?.GetSubAgents() ?? [];

        string agentName = AgentName;
        if (string.IsNullOrWhiteSpace(agentName))
        {
            throw new NullReferenceException("Agent name is not set");
        }

        var agent = subAgents.FirstOrDefault(o => o.AgentName == agentName);
        if (agent is null)
        {
            throw new NullReferenceException($"Agent '{agentName}' not found");
        }

        var runner = context.FuncContext.GetArgument<IAgentGraphRunner>();
        if (runner is null)
        {
            throw new NullReferenceException("Agent graph runner is not set");
        }

        foreach (var loopItem in _loops.List)
        {
            runner.AddLoop(agent, loopItem.TaskName, loopItem.Prompt);
        }

        var output = new Output();
        int successCount = 0;
        int failCount = 0;

        context.ToolInstance.Conversation?.AddRunningMessage($"Call agent '{agentName}' with {Loops.Count} loop(s)", msg =>
        {
            msg.AddCode(string.Join(", ", Loops.Select(l => l.TaskName)));
        });
        context.Conversation?.AddRunningMessage($"Call agent '{agentName}' with {Loops.Count} loop(s)", msg =>
        {
            msg.AddCode(string.Join(", ", Loops.Select(l => l.TaskName)));
        });

        // TODO: Implement agent invocation logic here
        // For each LoopItem:
        //   1. Create and run a loop for the specified agent
        //   2. Capture result or error
        //   3. Add LoopResult to output.Results

        output.SuccessCount = successCount;
        output.FailCount = failCount;

        return Task.FromResult(output);
    }
}
