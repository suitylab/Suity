using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("GetTaskHistory", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Get Task History")]
[ToolTipsText("Get task history records by task ID and query conditions.")]
public class GetTaskHistory : ToolCommand<GetTaskHistory.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _result = new("Result");

        public string Result { get => _result.Text; set => _result.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _result.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _result.InspectorField(setup);
        }

        public override string ToString() => Result ?? string.Empty;
    }

    readonly StringProperty _taskId = new("TaskId", "Task ID", string.Empty, "The unique identifier of the task to query.");

    public string TaskId { get => _taskId.Text; set => _taskId.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _taskId.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _taskId.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var myTask = context.ToolInstance?.Owner as IAigcTaskPage
            ?? throw new NullReferenceException($"The tool instance's owner is not a {nameof(IAigcTaskPage)}.");

        string result = GetTaskChatHistoryText(myTask, TaskId);

        return Task.FromResult(new Output { Result = result });
    }

    public static string GetTaskChatHistoryText(IAigcTaskPage myTask, string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new ArgumentException("Task ID cannot be null or whitespace.", nameof(taskId));
        }

        var host = myTask.TaskHost
            ?? throw new NullReferenceException("The task host is null.");

        var task = host.GetTask(taskId) as IAigcTaskPage
            ?? throw new NullReferenceException($"No workflow task found with ID '{taskId}'.");

        return task.GetTaskChatHistoryText();
    }
}