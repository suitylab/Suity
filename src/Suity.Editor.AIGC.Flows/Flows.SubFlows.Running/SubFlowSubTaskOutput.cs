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
public class SubFlowSubTaskOutput : SubFlowElement, IPageParameterOutput, IPageParameterTool
{
    readonly PageSubTaskOutputItem _outputItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowSubTaskOutput"/> class.
    /// </summary>
    /// <param name="item">The sub-task output item to associate with this element.</param>
    public SubFlowSubTaskOutput(PageSubTaskOutputItem item)
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
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output is related to task commit.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this output includes chat history.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <inheritdoc/>
    public ChatHistoryText ResolveChatHistory()
    {
        if (AllSubTasks)
        {
            var contents = GetAllSubTasks().Select(o => o?.GetTaskCommit()).SkipNull();
            return string.Join("\r\n\r\n", contents);
        }
        else
        {
            var subTask = GetLastSubTask();
            return subTask?.GetTaskCommit() ?? ChatHistoryText.Empty;
        }
    }

    /// <inheritdoc/>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
    }
    #endregion

    #region IPageParameterTool

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
                string toolName = GetLastSubTask()?.PageName;
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

        TaskCompletion = _outputItem.Node?.TaskCompletion == true;
        TaskCommit = _outputItem.Node?.TaskCommit == true;
        ChatHistory = _outputItem.Node?.ChatHistory == true;
    }

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (AllSubTasks)
        {
            var tasks = GetAllSubTasks();
            sync.Sync<bool>(Name, tasks.Length > 0, SyncFlag.GetOnly);
        }
        else
        {
            var task = GetLastSubTask();
            sync.Sync<bool>(Name, task != null, SyncFlag.GetOnly);
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        var property = new ViewProperty(Name, DisplayText, Icon)
            .WithReadOnly()
            .WithStatus(GetStatus());

        setup.InspectorFieldOf<bool>(property);
    }


    /// <summary>
    /// Gets the last sub-task associated with this output element.
    /// </summary>
    /// <returns>The last sub-task, or <c>null</c> if no sub-tasks exist.</returns>
    public IAigcTaskPage GetLastSubTask()
    {
        var taskService = Option.Owner as IAigcWorkflowPage;
        return taskService?.GetLastSubTask();
    }

    /// <summary>
    /// Gets all sub-tasks associated with this output element.
    /// </summary>
    /// <returns>An array of all sub-tasks, or an empty array if none exist.</returns>
    public IAigcTaskPage[] GetAllSubTasks()
    {
        var taskService = Option.Owner as IAigcWorkflowPage;
        return taskService?.GetAllSubTasks() ?? [];
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (!TaskCompletion)
        {
            return null;
        }

        bool done;

        if (AllSubTasks)
        {
            var tasks = GetAllSubTasks();
            if (tasks.Length == 0)
            {
                return false;
            }

            done = tasks.All(o => (o?.GetAllDone()).IsTrueOrEmpty());
        }
        else
        {
            var task = GetLastSubTask();
            if (task is null)
            {
                return false;
            }

            var taskIsDone = task?.GetAllDone();
            done = taskIsDone.IsTrueOrEmpty();
        }

        return done;
    }
}
