using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC.Flows.Pages;

#region AppendTaskPage
/// <summary>
/// A flow node that appends a task page to the current task workflow.
/// Accepts either a page instance or a page definition as input.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG)]
[DisplayText("Append Task Page", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AppendTaskPageNode")]
public class AppendTaskPage : AigcPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _inPageInstance;
    readonly FlowNodeConnector _inPage;
    readonly ConnectorStringProperty _pageTitle = new("PageTitle", "Page Title");
    readonly ConnectorTextBlockProperty _taskPrompt = new("TaskPrompt", "Task Prompt");
    readonly ConnectorStringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "Name used when submitting to parent task.");

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppendTaskPage"/> class.
    /// </summary>
    public AppendTaskPage()
    {
        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();
        var type = TypeDefinition.FromNative<IAigcToolAsset>();

        _in = this.AddActionInputConnector("In", "Input");
        _inPageInstance = this.AddDataInputConnector("PageInstance", instanceType, "Page Instance");
        _inPage = this.AddDataInputConnector("Page", type, "Page");
        _pageTitle.AddConnector(this);
        _taskPrompt.AddConnector(this);
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
        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _pageTitle.InspectorField(setup, this);
        _taskPrompt.InspectorField(setup, this);
        _commitName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IAigcPageInstance>(_inPageInstance);
        var page = compute.GetValue<IAigcToolAsset>(_inPage);
        if (pageInstance is null && page is null)
        {
            throw new NullReferenceException("At least one of the required inputs (PageInstance or Page) is null.");
        }

        var service = compute.Context.GetArgument<IAigcTaskPage>()
            ?? throw new NullReferenceException("IAigcTaskService is null.");

        string title = _pageTitle.GetValue(compute, this);
        string taskPrompt = _taskPrompt.GetText(compute, this);
        string commitName = _commitName.GetValue(compute, this);

        if (pageInstance != null)
        {
            service.AppendTask(pageInstance, title, taskPrompt, commitName);
        }
        else if (page != null)
        {
            service.AppendTask(page, title, taskPrompt, commitName);
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
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG)]
[DisplayText("Add Sub Task Page", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AddSubTaskPageNode")]
public class AddSubTaskPage : AigcPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _inPageInstance;
    readonly FlowNodeConnector _inPage;
    readonly ConnectorStringProperty _pageTitle = new("PageTitle", "Page Title");
    readonly ConnectorTextBlockProperty _taskPrompt = new("TaskPrompt", "Task Prompt");
    readonly ConnectorStringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "Name used when submitting to parent task.");

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddSubTaskPage"/> class.
    /// </summary>
    public AddSubTaskPage()
    {
        var instanceType = TypeDefinition.FromNative<IAigcPageInstance>();
        var type = TypeDefinition.FromNative<IAigcToolAsset>();

        _in = this.AddActionInputConnector("In", "Input");
        _inPageInstance = this.AddDataInputConnector("PageInstance", instanceType, "Page Instance");
        _inPage = this.AddDataInputConnector("Page", type, "Page");
        _pageTitle.AddConnector(this);
        _taskPrompt.AddConnector(this);
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
        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _pageTitle.InspectorField(setup, this);
        _taskPrompt.InspectorField(setup, this);
        _commitName.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var pageInstance = compute.GetValue<IAigcPageInstance>(_inPageInstance);
        var page = compute.GetValue<IAigcToolAsset>(_inPage);
        if (pageInstance is null && page is null)
        {
            throw new NullReferenceException("At least one of the required inputs (PageInstance or Page) is null.");
        }

        var service = compute.Context.GetArgument<IAigcTaskPage>()
            ?? throw new NullReferenceException("IAigcTaskService is null.");

        string title = _pageTitle.GetValue(compute, this);
        string taskPrompt = _taskPrompt.GetText(compute, this);
        string commitName = _commitName.GetValue(compute, this);

        if (pageInstance != null)
        {
            service.AddSubTask(pageInstance, title, taskPrompt, commitName);
        }
        else if (page != null)
        {
            service.AddSubTask(page, title, taskPrompt, commitName);
        }

        compute.SetResult(this, _out);
    }
}
#endregion

#region GetTaskChatHistory

/// <summary>
/// A flow node that retrieves the chat history from the current task.
/// Optionally includes chat history from parent tasks in the hierarchy.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Chat History", "*CoreIcon|Task")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskChatHistoryNode")]
public class GetTaskChatHistory : AigcPageNode
{
    readonly FlowNodeConnector _chatHistory;

    readonly ConnectorValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes chat history from parent tasks.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskChatHistory"/> class.
    /// </summary>
    public GetTaskChatHistory()
    {
        var msgType = TypeDefinition.FromNative<LLmMessage>().MakeArrayType();
        _inHierarchy.AddConnector(this);
        _chatHistory = AddDataOutputConnector("ChatHistory", msgType, "Chat History");
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

        var taskService = compute.Context.GetArgument<IAigcTaskPage>();
        var history = taskService?.GetChatHistory(inHierarchy) ?? [];

        compute.SetValue(_chatHistory, history);
    }
}

#endregion

#region GetInitialTaskPrompt

/// <summary>
/// A flow node that retrieves the initial task prompt from the current task page.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Initial Task Prompt", "*CoreIcon|Task")]
public class GetInitialTaskPrompt : AigcPageNode
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
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Prompt", "*CoreIcon|Task")]
public class GetCurrentTaskPrompt : AigcPageNode
{
    readonly FlowNodeConnector _prompt;
    readonly ConnectorValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes task prompts from parent tasks.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTaskPrompt"/> class.
    /// </summary>
    public GetCurrentTaskPrompt()
    {
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

        var task = compute.Context.GetArgument<IAigcTaskPage>();
        string prompt = task?.GetPrompt(inHierarchy) ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region GetTaskPrompt

/// <summary>
/// A flow node that retrieves the task prompt from a specified task, optionally including prompts from parent tasks.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Prompt", "*CoreIcon|Task")]
public class GetTaskPrompt : AigcPageNode
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

        var task = compute.GetValue<IAigcTaskPage>(_task);
        string prompt = task?.GetPrompt(inHierarchy) ?? string.Empty;

        compute.SetValue(_prompt, prompt);
    }
}

#endregion

#region GetTaskCommit

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Commit Context", "*CoreIcon|Task")]
public class GetTaskCommit : AigcPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _commit;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskCommit"/> class.
    /// </summary>
    public GetTaskCommit()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _commit = AddDataOutputConnector("Commit", "string", "Commit Info");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task);
        if (task is null)
        {
            throw new FlowComputaionException(this, "Task is null.");
        }

        string commit = task?.GetPageInstance()?.GetTaskCommit() ?? string.Empty;

        compute.SetValue(_commit, commit);
    }
}

#endregion

#region GetSubTaskCommit

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get SubTask Commit Context", "*CoreIcon|Task")]
public class GetSubTaskCommit : AigcPageNode
{
    readonly FlowNodeConnector _parentTask;
    readonly ConnectorValueProperty<bool> _allSubTasks = new("AllSubTasks", "All Sub Tasks", false,
        "If enabled, retrieves commit information from all sub tasks and concatenates them, otherwise retrieves commit information from the last sub task only.");
    readonly FlowNodeConnector _commit;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskCommit"/> class.
    /// </summary>
    public GetSubTaskCommit()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

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
        var parentTask = compute.GetValue<IAigcTaskPage>(_parentTask);
        if (parentTask is null)
        {
            throw new FlowComputaionException(this, "Parent task is null.");
        }

        bool allSubTasks = _allSubTasks.GetValue(compute, this);

        string commit = string.Empty;

        if (allSubTasks)
        {
            var commits = parentTask.GetAllSubTasks()
                .Select(t => t.GetPageInstance()?.GetTaskCommit())
                .ToArray();

            commit = string.Join(Environment.NewLine, commits.Where(c => !string.IsNullOrEmpty(c)));
        }
        else
        {
            commit = parentTask.GetLastSubTask()?.GetPageInstance().GetTaskCommit() ?? string.Empty;
        }

        compute.SetValue(_commit, commit);
    }
}

#endregion

#region GetTaskContextText

/// <summary>
/// A flow node that retrieves the commit information from a specified task.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task Context Text", "*CoreIcon|Task")]
public class GetTaskContextText : AigcPageNode
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
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();

        _task = this.AddDataInputConnector("Task", taskType, "Task");
        _inputChat = this.AddDataOutputConnector("InputChat", "string", "Input Chat History");
        _outputChat = this.AddDataOutputConnector("OutputChat", "string", "Output Chat History");
        _commit = this.AddDataOutputConnector("Commit", "string", "Commit Info");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task);
        if (task is null)
        {
            throw new FlowComputaionException(this, "Task is null.");
        }

        var pageInstance = task.GetPageInstance();
        if (pageInstance is null)
        {
            throw new FlowComputaionException(this, "Task page instance is null.");
        }

        string inputChat = pageInstance.GetInputChatHistory() ?? string.Empty;
        string outputChat = pageInstance.GetOutputChatHistory() ?? string.Empty;
        string commit = pageInstance.GetTaskCommit() ?? string.Empty;
        
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
[SimpleFlowNodeStyle(Color = AigcColors.Page, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Self Task", "*CoreIcon|Task")]
[DisplayOrder(4960)]
public class GetSelfTask : AigcPageNode
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
[SimpleFlowNodeStyle(Color = AigcColors.Page, HasHeader = false, Width = 100, Height = 20)]
[DisplayText("Get Last Sub Task", "*CoreIcon|Task")]
[DisplayOrder(4950)]
public class GetLastSubTask : AigcPageNode
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
        var caller = compute.Context.GetArgument<IAigcTaskPage>();

        var subTask = caller?.GetLastSubTask();
        bool needDone = _needDone.GetValue(compute, this);

        if (needDone && subTask != null)
        {
            bool isDone = subTask.GetPageInstance()?.GetIsDone() == true;
            if (!isDone)
            {
                subTask = null;
            }
        }

        compute.SetValue(_subTask, subTask);
    }
}

#endregion

#region GetTaskInfomation

/// <summary>
/// A flow node that retrieves various information about a specified task,
/// including ID, title, index, parent task, child tasks, and status flags.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Information", "*CoreIcon|Task")]
public class GetTaskInfomation : AigcPageNode
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
    /// Initializes a new instance of the <see cref="GetTaskInfomation"/> class, setting up output connectors for task information properties.
    /// </summary>
    public GetTaskInfomation()
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

        var task = compute.GetValue<IAigcTaskPage>(_task) as AigcTaskPage
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
            compute.SetValue(_parentTask, task.ParentNode as AigcTaskPage);
        }

        if (diagram.GetIsLinked(_childTaskCount))
        {
            compute.SetValue(_childTaskCount, task.Count);
        }

        if (diagram.GetIsLinked(_childTasks))
        {
            compute.SetValue(_childTasks, task.Items.OfType<AigcTaskPage>().ToArray());
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
            compute.SetValue(_isDone, task.GetIsDone() == true);
        }
    }
}

#endregion

#region GetCurrentTask

/// <summary>
/// A flow node that retrieves the current task from the execution context.
/// </summary>
[SimpleFlowNodeStyle(Color = AigcColors.TaskBG, HasHeader = false)]
[DisplayText("Get Current Task", "*CoreIcon|Task")]
public class GetCurrentTask : AigcPageNode
{
    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentTask"/> class.
    /// </summary>
    public GetCurrentTask()
    {
        var type = TypeDefinition.FromNative<IAigcTaskPage>();
        _out = AddDataOutputConnector("Out", type, "Current Task");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var taskPage = compute.Context.GetArgument<IAigcTaskPage>();
        compute.SetValue(_out, taskPage);
    }
}

#endregion

#region GetTaskArticle

/// <summary>
/// A flow node that retrieves the article associated with a task.
/// Optionally creates a new article if one does not exist.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false)]
[DisplayText("Get Task Article", "*CoreIcon|Article")]
public class GetTaskArticle : AigcPageNode
{
    readonly FlowNodeConnector _task;
    readonly ConnectorValueProperty<bool> _autoCreate = new("AutoCreate", "Auto Create");
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskArticle"/> class.
    /// </summary>
    public GetTaskArticle()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>();

        _task = AddDataInputConnector("Task", taskType, "Task");
        _autoCreate.AddConnector(this);
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _autoCreate.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _autoCreate.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task) as AigcTaskPage;
        task ??= compute.Context.GetArgument<IAigcTaskPage>() as AigcTaskPage;
        if (task is null)
        {
            throw new NullReferenceException(nameof(task));
        }

        bool autoCreate = _autoCreate.GetValue(compute, this);

        var article = task.ResolveArticleBase(autoCreate);
        var articleAsset = article?.TargetAsset as IArticleAsset;

        compute.SetValue(_articles, articleAsset);
    }
}

#endregion

#region GetTaskKnowledgeArticles

/// <summary>
/// A flow node that retrieves all knowledge articles associated with a task's document.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false)]
[DisplayText("Get Task Knowledge Articles", "*CoreIcon|Article")]
public class GetTaskKnowledgeArticles : AigcPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskKnowledgeArticles"/> class.
    /// </summary>
    public GetTaskKnowledgeArticles()
    {
        var taskType = TypeDefinition.FromNative<IAigcTaskPage>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();

        _task = AddDataInputConnector("Task", taskType, "Task");
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Article");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcTaskPage>(_task) as AigcTaskPage;
        task ??= compute.Context.GetArgument<IAigcTaskPage>() as AigcTaskPage;
        if (task is null)
        {
            throw new NullReferenceException(nameof(task));
        }
        if (task.TaskPageDocument is not { } doc)
        {
            throw new NullReferenceException(nameof(task.TaskPageDocument));
        }

        var knowledge = doc.KnowledgeArticles.ToArray();

        compute.SetValue(_articles, knowledge);
    }
}

#endregion

#region ApplyKnowledgeArticle

/// <summary>
/// A flow node that adds an article to the knowledge base of the current task.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode)]
[DisplayText("Add Knowledge Article", "*CoreIcon|Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ApplyArticleToKnowledgeBase")]
public class AddKnowledgeArticle : AigcPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _article;

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddKnowledgeArticle"/> class.
    /// </summary>
    public AddKnowledgeArticle()
    {
        _in = AddActionInputConnector("In", "Input");

        var articleType = TypeDefinition.FromNative<IArticle>();
        _article = AddDataInputConnector("Article", articleType, "Article");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcTaskPage>();
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }
        if (task.TaskHost is not { } host)
        {
            throw new NullReferenceException(nameof(task.TaskHost));
        }

        var article = compute.GetValue<IArticle>(_article)
            ?? throw new ArgumentNullException("Article");

        var articleAsset = article.TargetAsset as IArticleAsset
            ?? throw new NullReferenceException("Can not convert article to asset.");

        host.AddKnowledgeArticle(articleAsset);

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetKnowledgeArticleList

/// <summary>
/// A flow node that converts an array of article assets into a knowledge article list.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false)]
[DisplayText("Get Knowledge Article List", "*CoreIcon|Knowledge")]
public class GetKnowledgeArticleList : AigcPageNode
{
    readonly FlowNodeConnector _articles;
    readonly FlowNodeConnector _knowledgeList;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetKnowledgeArticleList"/> class.
    /// </summary>
    public GetKnowledgeArticleList()
    {
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();
        _articles = AddDataInputConnector("Articles", articleAssetType, "Articles");

        var knowledgeListType = TypeDefinition.FromNative<KnowledgeArticleList>();
        _knowledgeList = AddDataOutputConnector("KnowledgeList", knowledgeListType, "Knowledge List");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var assets = compute.GetValues<IArticleAsset>(_articles, true).SkipNull().ToArray() ?? [];
        if (assets.Length == 0)
        {
            compute.SetValue(_knowledgeList, string.Empty);
            return;
        }

        var list = new KnowledgeArticleList(assets);
        compute.SetValue(_knowledgeList, list);
    }
}

#endregion

#region GetArticlesFromKnowledge

/// <summary>
/// A flow node that retrieves specific articles from a knowledge list by their IDs.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false)]
[DisplayText("Get Articles From Knowledge", "*CoreIcon|Knowledge")]
public class GetArticlesFromKnowledge : AigcPageNode
{
    readonly FlowNodeConnector _knowledgeList;
    readonly FlowNodeConnector _ids;
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticlesFromKnowledge"/> class.
    /// </summary>
    public GetArticlesFromKnowledge()
    {
        var knowledgeListType = TypeDefinition.FromNative<KnowledgeArticleList>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();

        _knowledgeList = AddDataInputConnector("KnowledgeList", knowledgeListType, "Knowledge List");
        _ids = AddDataInputConnector("Ids", "string[]", "Ids");
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Articles");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var knowledge = compute.GetValue<KnowledgeArticleList>(_knowledgeList);
        string[] ids = compute.GetValues<string>(_ids, true)
            ?.Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToArray() ?? [];

        if (knowledge is null || ids.Length == 0)
        {
            compute.SetValue(_articles, Array.Empty<IArticleAsset>());
            return;
        }

        var articles = ids
            .Select(id => knowledge.GetItem(id)?.Article)
            .SkipNull()
            .ToArray();

        compute.SetValue(_articles, articles);
    }
}

#endregion

#region GetArticleTaggedContents

/// <summary>
/// A flow node that extracts tagged XML contents from articles, converting them into LooseXmlTag objects
/// with configurable tag name and title attribute settings.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false)]
[DisplayText("Get Article Tagged Contents", "*CoreIcon|Article")]
public class GetArticleTaggedContents : AigcPageNode
{
    readonly FlowNodeConnector _articles;
    readonly ConnectorStringProperty _tagName = new("TagName", "Tag Name", "section");
    readonly ConnectorStringProperty _titleAttr = new("TitleAttribute", "Title Attribute", "title");
    readonly ConnectorValueProperty<bool> _titleInHierarchy = new("TitleInHierarchy", "Hierarchy Title", false, "If selected, the title attribute will be the full path title in the hierarchy.");

    readonly FlowNodeConnector _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticleTaggedContents"/> class.
    /// </summary>
    public GetArticleTaggedContents()
    {
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();
        var tagType = TypeDefinition.FromNative<LooseXmlTag>().MakeArrayType();

        _articles = AddDataInputConnector("Articles", articleAssetType, "Articles");
        _tagName.AddConnector(this);
        _titleAttr.AddConnector(this);
        _titleInHierarchy.AddConnector(this);
        _tags = AddDataOutputConnector("Tags", tagType, "Tags");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tagName.Sync(sync);
        _titleAttr.Sync(sync);
        _titleInHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tagName.InspectorField(setup, this);
        _titleAttr.InspectorField(setup, this);
        _titleInHierarchy.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var articles = compute.GetValues<IArticleAsset>(_articles, true).SkipNull().ToArray() ?? [];
        if (articles.Length == 0)
        {
            compute.SetValue(_tags, Array.Empty<LooseXmlTag>());
            return;
        }

        string tagName = _tagName.GetValue(compute, this)?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tagName))
        {
            tagName = "section";
        }

        string titleAttr = _titleAttr.GetValue(compute, this)?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(titleAttr))
        {
            titleAttr = "title";
        }

        bool titleInHierarchy = _titleInHierarchy.GetValue(compute, this);

        List<LooseXmlTag> tags = [];

        foreach (var assetItem in articles)
        {
            string title = assetItem.GetTitle(titleInHierarchy) ?? string.Empty;
            string content = assetItem.GetContentText() ?? string.Empty;

            var tag = new LooseXmlTag()
            {
                TagName = tagName,
                InnerText = content,
            };

            tag.SetAttribute(titleAttr, title);
            tags.Add(tag);
        }

        compute.SetValue(_tags, tags.ToArray());
    }
}


#endregion

#region Converters
/// <summary>
/// Converts a <see cref="SubFlowDefinitionAsset"/> to an <see cref="IAigcPage"/> by retrieving the diagram item's node.
/// </summary>
public class PageAssetToAigcPageConverter : TypeConverter<SubFlowDefinitionAsset, IAigcPage>
{
    /// <inheritdoc/>
    public override IAigcPage Convert(SubFlowDefinitionAsset objFrom)
    {
        return objFrom.GetDiagramItem()?.Node;
    }
}

/// <summary>
/// Converts an <see cref="IAigcPage"/> to a <see cref="SubFlowDefinitionAsset"/> by retrieving the page definition node's asset.
/// </summary>
public class AigcPageToPageAssetConverter : TypeConverter<IAigcPage, SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override SubFlowDefinitionAsset Convert(IAigcPage objFrom)
    {
        return (objFrom as SubflowDefinitionNode)?.GetAsset() as SubFlowDefinitionAsset;
    }
}

#endregion
