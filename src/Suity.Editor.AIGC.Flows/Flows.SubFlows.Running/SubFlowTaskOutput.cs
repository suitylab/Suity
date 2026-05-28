using Suity;
using Suity.Collections;
using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Linq;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Represents a sub-graph element that handles sub-task output for sub-graph.
/// Implements both parameter output and tool parameter interfaces.
/// </summary>
public class SubFlowTaskOutput : SubFlowElement, IPageParameterOutput, IPageParameterToolCall
{
    private readonly PageTaskOutputItem _outputItem;

    private TextBlockProperty _subTaskCommit;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowTaskOutput"/> class.
    /// </summary>
    /// <param name="item">The sub-task output item to associate with this element.</param>
    public SubFlowTaskOutput(PageTaskOutputItem item)
        : base(item)
    {
        _outputItem = item ?? throw new System.ArgumentNullException(nameof(item));
    }

    #region IPageParameterOutput

    /// <summary>
    /// Gets the type definition of the output parameter. Returns the type for <see cref="ISubFlowInstance"/>.
    /// </summary>
    public TypeDefinition ParameterType => TypeDefinition.FromNative<ISubFlowInstance>();

    /// <summary>
    /// Gets the sub-task output value. Returns either all sub-tasks or the last sub-task based on <see cref="AllSubTasks"/>.
    /// </summary>
    public object Value => AllSubTasks ? GetAllSubTasks() : GetLastSubTask();

    /// <inheritdoc/>
    public bool IsValueSet { get => true; set { } }

    /// <summary>
    /// Gets a value indicating whether to output all sub-tasks instead of just the last one.
    /// </summary>
    public bool AllSubTasks { get; private set; }

    /// <inheritdoc/>
    public void SetValue(object value)
    {
    }
    
    /// <inheritdoc/>
    public object EnsureValue() => AllSubTasks ? GetAllSubTasks() : GetLastSubTask();

    /// <summary>
    /// Gets a value indicating whether this output is related to task completion.
    /// </summary>
    public bool Required { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output is related to task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output includes chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <inheritdoc/>
    public HistoryTag ResolveChatHistory(ResolveChatIntents intent)
    {
        if (AllSubTasks)
        {
            var contents = GetAllSubTasks().Select(o => o?.GetPageInstance()?.GetTaskCommit(intent)).SkipNull();
            return string.Join("\r\n\r\n", contents);
        }
        else
        {
            var subTask = GetLastSubTask();
            string content = subTask?.GetPageInstance()?.GetTaskCommit(intent) ?? HistoryText.Empty;
            return new HistoryTag(content, [new("tool", ToolName)]);
        }
    }

    /// <inheritdoc/>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
    }
    #endregion

    #region IPageParameterTool

    public string TaskId => (Option.Owner as IAigcTaskPage)?.TaskId;

    /// <summary>
    /// Gets the tool name, which corresponds to the last sub-task's page instance name when <see cref="AllSubTasks"/> is <c>false</c>.
    /// Returns <c>null</c> when <see cref="AllSubTasks"/> is <c>true</c>.
    /// </summary>
    public string ToolName
    {
        get
        {
            if (!AllSubTasks)
            {
                string toolName = GetLastSubTask()?.GetPageAsset()?.Name ?? string.Empty;
                return toolName;
            }
            else
            {
                return null;
            }
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        AllSubTasks = _outputItem.Node?.AllSubTasks == true;

        Required = _outputItem.Node?.Required == true;
        TaskCommit = _outputItem.Node?.TaskCommit == true;
        ChatHistory = _outputItem.Node?.ChatHistory == true;

        _subTaskCommit = new(Name);
        _subTaskCommit.Flag |= SyncFlag.GetOnly;
        _subTaskCommit.Property.WithReadOnly().WithOptional();
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (_subTaskCommit != null)
        {
            if (AllSubTasks)
            {
                var tasks = GetAllSubTasks();
                if (tasks.Length == 0)
                {
                    _subTaskCommit.Text = null;
                }
                else
                {
                    int numCompleted = tasks.Count(o => o.GetCommitStatus() != TaskCommitStatus.None);
                    _subTaskCommit.Text = $"{numCompleted}/{tasks.Length} tasks completed";
                }
                
                _subTaskCommit.Sync(sync);
            }
            else
            {
                var task = GetLastSubTask();
                _subTaskCommit.Text = task?.GetPageInstance()?.GetTaskCommit(ResolveChatIntents.Preview);
                _subTaskCommit.Sync(sync);
            }
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (_subTaskCommit != null)
        {
            _subTaskCommit.Property.WithStatus(GetStatus());
            _subTaskCommit.InspectorField(setup);
        }
    }


    /// <summary>
    /// Gets the last sub-task associated with this output element.
    /// </summary>
    /// <returns>The last sub-task, or <c>null</c> if no sub-tasks exist.</returns>
    public IAigcTaskPage GetLastSubTask()
    {
        var task = Option.Owner as IAigcWorkflowPage;
        return task?.GetLastSubTask();
    }

    /// <summary>
    /// Gets all sub-tasks associated with this output element.
    /// </summary>
    /// <returns>An array of all sub-tasks, or an empty array if none exist.</returns>
    public IAigcTaskPage[] GetAllSubTasks()
    {
        var task = Option.Owner as IAigcWorkflowPage;
        return task?.GetSubTasks() ?? [];
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (!Required)
        {
            return null;
        }

        bool isDone;

        if (AllSubTasks)
        {
            var tasks = GetAllSubTasks();
            if (tasks.Length == 0)
            {
                return false;
            }

            isDone = tasks.All(o => o.GetCommitStatus() != TaskCommitStatus.None);
        }
        else
        {
            var task = GetLastSubTask();
            if (task is null)
            {
                return false;
            }

            isDone = task.GetCommitStatus() != TaskCommitStatus.None;
        }

        return isDone;
    }
}
