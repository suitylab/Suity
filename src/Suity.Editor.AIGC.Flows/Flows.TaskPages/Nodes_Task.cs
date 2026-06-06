using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region AppendTaskPage
/// <summary>
/// A flow node that appends a task page to the current task workflow.
/// Accepts either a page instance or a page definition as input.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG)]
[DisplayText("Append Task Page", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AppendTaskPageNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AppendTaskPage")]
public class AppendTaskPage : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _inPageInstance;
    readonly FlowNodeConnector _inPage;
    readonly ConnectorStringProperty _pageTitle = new("PageTitle", "Page Title");
    readonly ConnectorTextBlockProperty _taskPrompt = new("TaskPrompt", "Task Prompt");
    readonly ConnectorAssetProperty<PromptAsset> _taskRule = new("TaskRule", "Task Rule", "Optional rule to apply to the task.");
    readonly ConnectorStringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "Name used when submitting to parent task.");

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppendTaskPage"/> class.
    /// </summary>
    public AppendTaskPage()
    {
        var instanceType = TypeDefinition.FromNative<IPageInstance>();
        var type = TypeDefinition.FromAssetLink<IPageAsset>();

        _in = this.AddActionInputConnector("In", "Input");
        _inPageInstance = this.AddDataInputConnector("PageInstance", instanceType, "Page Instance");
        _inPage = this.AddDataInputConnector("Page", type, "Page");
        _pageTitle.AddConnector(this);
        _taskPrompt.AddConnector(this);
        _taskRule.AddConnector(this);
        _commitName.AddConnector(this);

        _out = this.AddActionOutputConnector("Out", "Output");
    }

    /// <summary>
    /// Gets the name used when submitting to the parent task.
    /// </summary>
    public string CommitName => _commitName.Text;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _pageTitle.Sync(sync);
        _taskPrompt.Sync(sync);
        _taskRule.Sync(sync);
        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _pageTitle.InspectorField(setup, this);
        _taskPrompt.InspectorField(setup, this);
        _taskRule.InspectorField(setup, this);
        _commitName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IPageInstance>(_inPageInstance);
        var page = compute.GetValue<IPageAsset>(_inPage);
        if (pageInstance is null && page is null)
        {
            throw new NullReferenceException("At least one of the required inputs (PageInstance or Page) is null.");
        }

        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>()
            ?? throw new NullReferenceException("IAigcTaskService is null.");

        string title = _pageTitle.GetValue(compute, this);
        string taskPrompt = _taskPrompt.GetText(compute, this);
        var taskRule = _taskRule.GetTarget(compute, this);
        string commitName = _commitName.GetValue(compute, this);

        if (pageInstance != null)
        {
            workflow.AppendTask(pageInstance, title, taskPrompt, taskRule, commitName);
        }
        else if (page != null)
        {
            workflow.AppendTask(page, title, taskPrompt, taskRule, commitName);
        }
        

        compute.SetResult(this, _out);
    }
}
#endregion

#region AddSubTaskPage

/// <summary>
/// A flow node that adds a sub-task page to the current task hierarchy.
/// Accepts either a page instance or a page definition as input.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG)]
[DisplayText("Add Sub Task Page", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AddSubTaskPageNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AddSubTaskPage")]
public class AddSubTaskPage : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _inPageInstance;
    readonly FlowNodeConnector _inPage;
    readonly ConnectorStringProperty _pageTitle = new("PageTitle", "Page Title");
    readonly ConnectorTextBlockProperty _taskPrompt = new("TaskPrompt", "Task Prompt");
    readonly ConnectorAssetProperty<PromptAsset> _taskRule = new("TaskRule", "Task Rule", "Optional rule to apply to the task.");
    readonly ConnectorStringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "Name used when submitting to parent task.");

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddSubTaskPage"/> class.
    /// </summary>
    public AddSubTaskPage()
    {
        var instanceType = TypeDefinition.FromNative<IPageInstance>();
        var type = TypeDefinition.FromAssetLink<IPageAsset>();

        _in = this.AddActionInputConnector("In", "Input");
        _inPageInstance = this.AddDataInputConnector("PageInstance", instanceType, "Page Instance");
        _inPage = this.AddDataInputConnector("Page", type, "Page");
        _pageTitle.AddConnector(this);
        _taskPrompt.AddConnector(this);
        _taskRule.AddConnector(this);
        _commitName.AddConnector(this);

        _out = this.AddActionOutputConnector("Out", "Output");
    }

    /// <summary>
    /// Gets the name used when submitting to the parent task.
    /// </summary>
    public string CommitName => _commitName.Text;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _pageTitle.Sync(sync);
        _taskPrompt.Sync(sync);
        _taskRule.Sync(sync);
        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _pageTitle.InspectorField(setup, this);
        _taskPrompt.InspectorField(setup, this);
        _taskRule.InspectorField(setup, this);
        _commitName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IPageInstance>(_inPageInstance);
        var page = compute.GetValue<IPageAsset>(_inPage);
        if (pageInstance is null && page is null)
        {
            throw new NullReferenceException("At least one of the required inputs (PageInstance or Page) is null.");
        }

        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>()
            ?? throw new NullReferenceException("IAigcTaskService is null.");

        string title = _pageTitle.GetValue(compute, this);
        string taskPrompt = _taskPrompt.GetText(compute, this);
        var taskRule = _taskRule.GetTarget(compute, this);
        string commitName = _commitName.GetValue(compute, this);

        if (pageInstance != null)
        {
            workflow.AddSubTask(pageInstance, title, taskPrompt, taskRule, commitName);
        }
        else if (page != null)
        {
            workflow.AddSubTask(page, title, taskPrompt, taskRule, commitName);
        }

        compute.SetResult(this, _out);
    }
}
#endregion

#region GetCurrentChatHistory

/// <summary>
/// A flow node that retrieves the chat history from the current task.
/// Optionally includes chat history from parent tasks in the hierarchy.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Chat History", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskChatHistoryNode")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskChatHistory")]
[NativeAlias("Suity.Editor.Flows.TaskPages.GetTaskChatHistory")]
public class GetCurrentChatHistory : TaskPageNode
{
    readonly FlowNodeConnector _chatHistory;

    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes chat history from parent tasks.");
    readonly ValueProperty<int> _hierarchyLimit = new("HierarchyLimit", "Hierarchy Limit", 1, "Maximum number of parent levels to include in the hierarchy.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentChatHistory"/> class.
    /// </summary>
    public GetCurrentChatHistory()
    {
        var msgType = TypeDefinition.FromNative<LLmMessage>().MakeArrayType();
        _chatHistory = AddDataOutputConnector("ChatHistory", msgType, "Chat History");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
        _hierarchyLimit.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup);
        if (_inHierarchy)
        {
            _hierarchyLimit.InspectorField(setup);
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool inHierarchy = _inHierarchy.Value;
        int level = inHierarchy ? _hierarchyLimit.Value : 0;

        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>();
        var history = workflow?.GetChatHistory(level) ?? [];

        compute.SetValue(_chatHistory, history);
    }
}

#endregion

#region GetInitialTaskPrompt

/// <summary>
/// A flow node that retrieves the initial task prompt from the current task page.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Initial Task Prompt", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetInitialTaskPrompt")]
public class GetInitialTaskPrompt : TaskPageNode
{
    readonly FlowNodeConnector _prompt;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInitialTaskPrompt"/> class.
    /// </summary>
    public GetInitialTaskPrompt()
    {
        _prompt = AddDataOutputConnector("Prompt", "string", "Prompt");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var taskPage = compute.Context.GetArgument<IAigcTaskPage>();
        string prompt = taskPage?.TaskHost?.InitialTaskPrompt ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region GetCurrentTaskPrompt

/// <summary>
/// A flow node that retrieves the current task prompt, optionally including prompts from parent tasks.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Prompt", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetCurrentTaskPrompt")]
public class GetCurrentTaskPrompt : TaskPageNode
{
    readonly FlowNodeConnector _prompt;
    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes task prompts from parent tasks.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTaskPrompt"/> class.
    /// </summary>
    public GetCurrentTaskPrompt()
    {
        _prompt = AddDataOutputConnector("Prompt", "string", "Prompt");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool inHierarchy = _inHierarchy.Value;

        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>();
        string prompt = workflow?.GetLastPrompt(inHierarchy) ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region GetTaskLastPrompt

/// <summary>
/// A flow node that retrieves the current task prompt, optionally including prompts from parent tasks.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Last Prompt", "*CoreIcon|Task")]
public class GetTaskLastPrompt : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _prompt;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskLastPrompt"/> class.
    /// </summary>
    public GetTaskLastPrompt()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _prompt = AddDataOutputConnector("LastPrompt", "string", "Last Prompt");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workflow = compute.GetValue<IAigcWorkflowPage>(_task);
        string prompt = workflow?.GetLastPrompt() ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region GetCurrentTaskRule

/// <summary>
/// A flow node that retrieves the current task rule.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Rule", "*CoreIcon|Task")]
public class GetCurrentTaskRule : TaskPageNode
{
    readonly FlowNodeConnector _rule;
    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", true, "If enabled, retrieves rule from parent tasks.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTaskRule"/> class.
    /// </summary>
    public GetCurrentTaskRule()
    {
        var ruleType = TypeDefinition.FromAssetLink<PromptAsset>();
        _rule = AddDataOutputConnector("Rule", ruleType, "Rule");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool inHierarchy = _inHierarchy.Value;

        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>();
        var rule = workflow?.GetRule(inHierarchy);
        var ruleKey = new SAssetKey(TypeDefinition.FromAssetLink<PromptAsset>(), rule?.Id ?? Guid.Empty);
        compute.SetValue(_rule, ruleKey);
    }
}

#endregion

#region GetTaskInformation

/// <summary>
/// A flow node that retrieves various information about a specified task,
/// including ID, title, index, parent task, child tasks, and status flags.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Information", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskInfomation")]
[NativeAlias("Suity.Editor.Flows.TaskPages.GetTaskInfomation")]
public class GetTaskInformation : TaskPageNode
{
    readonly FlowNodeConnector _task;

    readonly FlowNodeConnector _taskId;
    readonly FlowNodeConnector _taskTitle;
    readonly FlowNodeConnector _taskIndex;
    readonly FlowNodeConnector _parentTask;
    readonly FlowNodeConnector _childTaskCount;
    readonly FlowNodeConnector _childTasks;

    readonly FlowNodeConnector _isInitialTask;
    readonly FlowNodeConnector _isFirstTask;
    readonly FlowNodeConnector _isDone;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskInformation"/> class, setting up output connectors for task information properties.
    /// </summary>
    public GetTaskInformation()
    {
        var type = TypeDefinition.FromNative<IAigcTaskPage>();
        var aryType = type.MakeArrayType();

        _task = AddDataInputConnector("Task", type, "Task");

        _taskId = AddDataOutputConnector("TaskId", "string", "Task Id");
        _taskTitle = AddDataOutputConnector("TaskTitle", "string", "Task Title");
        _taskIndex = AddDataOutputConnector("TaskIndex", "int", "Task Index");
        _parentTask = AddDataOutputConnector("ParentTask", type, "Parent Task");
        _childTaskCount = AddDataOutputConnector("ChildTaskCount", "int", "Child Task Count");
        _childTasks = AddDataOutputConnector("ChildTasks", aryType, "Sub Tasks");

        _isInitialTask = AddDataOutputConnector("IsInitialTask", "bool", "Is Initial Task");
        _isFirstTask = AddDataOutputConnector("IsFirstTask", "bool", "Is First Task");
        _isDone = AddDataOutputConnector("IsDone", "bool", "Is Done");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        var task = compute.GetValue<IAigcTaskPage>(_task) as AigcWorkflowPage
            ?? throw new NullReferenceException("Task is null.");

        var diagram = this.Diagram
            ?? throw new NullReferenceException("Diagram is null.");

        if (diagram.GetIsLinked(_taskId))
        {
            compute.SetValue(_taskId, task.Name);
        }

        if (diagram.GetIsLinked(_taskTitle))
        {
            compute.SetValue(_taskTitle, task.Description);
        }

        if (diagram.GetIsLinked(_taskIndex))
        {
            compute.SetValue(_taskIndex, task.GetIndex());
        }

        if (diagram.GetIsLinked(_parentTask))
        {
            compute.SetValue(_parentTask, task.ParentNode as AigcWorkflowPage);
        }

        if (diagram.GetIsLinked(_childTaskCount))
        {
            compute.SetValue(_childTaskCount, task.Count);
        }

        if (diagram.GetIsLinked(_childTasks))
        {
            compute.SetValue(_childTasks, task.Items.OfType<AigcWorkflowPage>().ToArray());
        }

        if (diagram.GetIsLinked(_isInitialTask))
        {
            compute.SetValue(_isInitialTask, task.GetIndex() == 0 && task.ParentNode is null);
        }

        if (diagram.GetIsLinked(_isFirstTask))
        {
            compute.SetValue(_isFirstTask, task.GetIndex() == 0);
        }

        if (diagram.GetIsLinked(_isDone))
        {
            compute.SetValue(_isDone, task.GetPageInstance()?.GetIsDone() == true);
        }
    }
}

#endregion

#region GetTaskPrompt

/// <summary>
/// A flow node that retrieves the task prompt from a specified task, optionally including prompts from parent tasks.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Prompt", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskPrompt")]
public class GetTaskPrompt : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _prompt;
    readonly ConnectorValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes task prompts from parent tasks.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskPrompt"/> class.
    /// </summary>
    public GetTaskPrompt()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _prompt = AddDataOutputConnector("Prompt", "string", "Prompt");
        _inHierarchy.AddConnector(this);
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool inHierarchy = _inHierarchy.GetValue(compute, this);

        var workflow = compute.GetValue<IAigcWorkflowPage>(_task);
        string prompt = workflow?.GetPrompt() ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region SetTaskPrompt

/// <summary>
/// A flow node that sets the task prompt for a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true)]
[DisplayText("Set Task Prompt", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.SetTaskPrompt")]
public class SetTaskPrompt : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _task;
    readonly ConnectorTextBlockProperty _prompt;
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetTaskPrompt"/> class.
    /// </summary>
    public SetTaskPrompt()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _in = this.AddActionInputConnector("In", "Input");
        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _prompt = new ConnectorTextBlockProperty("Prompt", "Prompt");
        _prompt.AddConnector(this);
        _out = this.AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _prompt.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _prompt.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workflow = compute.GetValue<IAigcWorkflowPage>(_task);
        string prompt = _prompt.GetText(compute, this);

        if (workflow != null)
        {
            workflow.SetPrompt(prompt);
        }

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetTaskRule

/// <summary>
/// A flow node that retrieves the task rule from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Rule", "*CoreIcon|Task")]
public class GetTaskRule : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly ConnectorValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, retrieves rule from parent tasks if current task has no rule.");
    readonly FlowNodeConnector _rule;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskRule"/> class.
    /// </summary>
    public GetTaskRule()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();
        var ruleType = TypeDefinition.FromAssetLink<PromptAsset>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _inHierarchy.AddConnector(this);
        _rule = AddDataOutputConnector("Rule", ruleType, "Rule");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var ruleType = TypeDefinition.FromAssetLink<PromptAsset>();
        var workflow = compute.GetValue<IAigcWorkflowPage>(_task);
        bool inHierarchy = _inHierarchy.GetValue(compute, this);

        var rule = workflow?.GetRule(inHierarchy);
        var ruleKey = new SAssetKey(ruleType, rule?.Id ?? Guid.Empty);

        compute.SetValue(_rule, ruleKey);
    }
}

#endregion

#region SetTaskRule

/// <summary>
/// A flow node that sets the task rule for a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true)]
[DisplayText("Set Task Rule", "*CoreIcon|Task")]
public class SetTaskRule : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _task;
    readonly ConnectorAssetProperty<PromptAsset> _rule;
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetTaskRule"/> class.
    /// </summary>
    public SetTaskRule()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _in = this.AddActionInputConnector("In", "Input");
        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _rule = new ConnectorAssetProperty<PromptAsset>("Rule", "Rule", "Optional rule to apply to the task.");
        _rule.AddConnector(this);
        _out = this.AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _rule.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _rule.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workflow = compute.GetValue<IAigcWorkflowPage>(_task);
        var rule = _rule.GetTarget(compute, this);

        if (workflow != null)
        {
            workflow.Rule = rule;
        }

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetTaskCommit

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Commit Context", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskCommit")]
public class GetTaskCommit : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _commit;
    readonly FlowNodeConnector _commitName;
    readonly FlowNodeConnector _commitStatus;


    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskCommit"/> class.
    /// </summary>
    public GetTaskCommit()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");

        _commit = AddDataOutputConnector("Commit", "string", "Commit Context");
        _commitName = AddDataOutputConnector("CommitName", "string", "Commit Name");
        _commitStatus = AddDataOutputConnector("CommitStatus", "string", "Commit Status");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task) as AigcWorkflowPage
            ?? throw new NullReferenceException("Task is null.");

        var diagram = this.Diagram
            ?? throw new NullReferenceException("Diagram is null.");

        if (diagram.GetIsLinked(_commit))
        {
            string commit = task?.GetPageInstance()?.GetTaskCommit(ResolveChatIntents.Normal) ?? string.Empty;
            compute.SetValue(_commit, commit);
        }

        if (diagram.GetIsLinked(_commitName))
        {
            compute.SetValue(_commitName, task.CommitName);
        }

        if (diagram.GetIsLinked(_commitStatus))
        {
            compute.SetValue(_commitStatus, task.GetCommitStatus());
        }
    }
}

#endregion

#region GetCurrentTaskCommitName

/// <summary>
/// A flow node that retrieves the commit name from the current task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Commit Name", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetCurrentTaskCommitName")]
public class GetCurrentTaskCommitName : TaskPageNode
{
    readonly FlowNodeConnector _commitName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTaskCommitName"/> class.
    /// </summary>
    public GetCurrentTaskCommitName()
    {
        _commitName = AddDataOutputConnector("CommitName", "string", "Commit Name");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcTaskPage>();
        compute.SetValue(_commitName, task?.CommitName ?? string.Empty);
    }
}

#endregion

#region GetSubTaskCommit

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get SubTask Commit Context", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetSubTaskCommit")]
public class GetSubTaskCommit : TaskPageNode
{
    readonly FlowNodeConnector _parentTask;
    readonly ConnectorValueProperty<bool> _allSubTasks = new("AllSubTasks", "All Sub Tasks", false,
        "If enabled, retrieves commit information from all sub tasks and concatenates them, otherwise retrieves commit information from the last sub task only.");
    readonly FlowNodeConnector _commit;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSubTaskCommit"/> class.
    /// </summary>
    public GetSubTaskCommit()
    {
        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();

        _parentTask = this.AddDataInputConnector("ParentTask", taskType, "Parent Task");
        _allSubTasks.AddConnector(this);
        _commit = AddDataOutputConnector("Commit", "string", "Commit Info");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _allSubTasks.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _allSubTasks.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var parentTask = compute.GetValue<IAigcWorkflowPage>(_parentTask);
        if (parentTask is null)
        {
            throw new FlowComputaionException(this, "Parent task is null.");
        }

        bool allSubTasks = _allSubTasks.GetValue(compute, this);

        string commit = string.Empty;

        if (allSubTasks)
        {
            var commits = parentTask.GetSubTasks()
                .Select(t => t.GetPageInstance()?.GetTaskCommit(ResolveChatIntents.Normal))
                .ToArray();

            commit = string.Join(Environment.NewLine, commits.Where(c => !string.IsNullOrEmpty(c)));
        }
        else
        {
            commit = parentTask.GetLastSubTask()?.GetPageInstance()?.GetTaskCommit(ResolveChatIntents.Normal) ?? string.Empty;
        }

        compute.SetValue(_commit, commit);
    }
}

#endregion

#region GetTaskContextText

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Context Text", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskContextText")]
public class GetTaskContextText : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _inputChat;
    readonly FlowNodeConnector _outputChat;
    readonly FlowNodeConnector _commit;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskContextText"/> class.
    /// </summary>
    public GetTaskContextText()
    {
        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _inputChat = this.AddDataOutputConnector("InputChat", "string", "Input Chat History");
        _outputChat = this.AddDataOutputConnector("OutputChat", "string", "Output Chat History");
        _commit = this.AddDataOutputConnector("Commit", "string", "Commit Info");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcWorkflowPage>(_task);
        if (task is null)
        {
            throw new FlowComputaionException(this, "Task is null.");
        }

        var subFlowInstance = task.GetSubFlowInstance();
        if (subFlowInstance is null)
        {
            throw new FlowComputaionException(this, "Task page instance is null.");
        }

        string inputChat = subFlowInstance.GetInputChatHistory(ResolveChatIntents.Normal) ?? string.Empty;
        string outputChat = subFlowInstance.GetOutputChatHistory(ResolveChatIntents.Normal) ?? string.Empty;
        string commit = subFlowInstance.GetTaskCommit(ResolveChatIntents.Normal) ?? string.Empty;
        
        compute.SetValue(_inputChat, inputChat);
        compute.SetValue(_outputChat, outputChat);
        compute.SetValue(_commit, commit);
    }
}

#endregion

#region GetSelfTask

/// <summary>
/// A flow node that retrieves the self task reference from the current execution context.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.PageGroup, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Self Task", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetSelfTask")]
[DisplayOrder(4960)]
public class GetSelfTask : TaskPageNode
{
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSelfTask"/> class.
    /// </summary>
    public GetSelfTask()
    {
        var type = TypeDefinition.FromNative<IAigcTaskPage>();

        _out = this.AddDataOutputConnector("SelfTask", type, "Self Task");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Task;

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcTaskPage>();

        compute.SetValue(_out, task);
    }
}

#endregion

#region GetLastSubTask

/// <summary>
/// A flow node that retrieves the last sub-task from a specified task.
/// Optionally requires the sub-task to be completed before returning it.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.PageGroup, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Last Sub Task", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetLastSubTask")]
[DisplayOrder(4950)]
public class GetLastSubTask : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly ConnectorValueProperty<bool> _needDone = new("NeedDone", "Need Done", false, "If enabled, returns null unless the last sub task is completed.");

    readonly FlowNodeConnector _subTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLastSubTask"/> class.
    /// </summary>
    public GetLastSubTask()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _needDone.AddConnector(this);

        _subTask = this.AddDataOutputConnector("SubTask", taskType, "Sub Task");
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Task;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _needDone.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _needDone.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var workflow = compute.Context.GetArgument<IAigcWorkflowPage>();

        var subTask = workflow?.GetLastSubTask();
        bool needDone = _needDone.GetValue(compute, this);

        if (needDone && subTask != null)
        {
            bool? isDone = subTask.GetPageInstance()?.GetIsDone();

            bool doneBool = isDone == true;
            if (!doneBool)
            {
                subTask = null;
            }
        }

        compute.SetValue(_subTask, subTask);
    }
}

#endregion

#region GetCurrentTask

/// <summary>
/// A flow node that retrieves the current task from the execution context.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetCurrentTask")]
public class GetCurrentTask : TaskPageNode
{
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTask"/> class.
    /// </summary>
    public GetCurrentTask()
    {
        var type = TypeDefinition.FromNative<IAigcWorkflowPage>();
        _out = AddDataOutputConnector("Out", type, "Current Task");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcWorkflowPage>();
        compute.SetValue(_out, task);
    }
}

#endregion

#region GetParentTask

/// <summary>
/// A flow node that retrieves the parent task of the current task.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Parent Task", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetParentTask")]
public class GetParentTask : TaskPageNode
{
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetParentTask"/> class.
    /// </summary>
    public GetParentTask()
    {
        var type = TypeDefinition.FromNative<IAigcTaskPage>();
        _out = AddDataOutputConnector("Out", type, "Parent Task");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var taskPage = compute.Context.GetArgument<IAigcTaskPage>();
        compute.SetValue(_out, taskPage?.ParentTask);
    }
}

#endregion

#region GetTaskById

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task By Id", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskById")]
public class GetTaskById : TaskPageNode
{
    readonly ConnectorStringProperty _taskId = new("TaskId", "Task ID", string.Empty, "The unique identifier of the task to query.");
    readonly FlowNodeConnector _task;

    public GetTaskById()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _taskId.AddConnector(this);
        _task = AddDataOutputConnector("Task", taskType, "Task");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _taskId.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _taskId.InspectorField(setup, this);
    }

    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcTaskPage>();
        string taskId = _taskId.GetValue(compute, this);

        var taskHost = task?.TaskHost;
        var targetTask = taskHost?.GetTask(taskId);

        compute.SetValue(_task, targetTask);
    }
}

#endregion

#region GetTaskHistory

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task History", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskHistory")]
public class GetTaskHistory : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _history;

    public GetTaskHistory()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _history = AddDataOutputConnector("History", "string", "History");
    }

    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task)
            ?? throw new NullReferenceException("Task is null.");

        string result = task.GetTaskChatHistoryText();

        compute.SetValue(_history, result);
    }
}

#endregion