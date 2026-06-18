using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents the result of a task execution, including the end type and associated parameter.
/// </summary>
/// <param name="EndType">The type of page commit that ended the task.</param>
/// <param name="Parameter">The parameter associated with the task result.</param>
internal record TaskRunResult(TaskCommitStatus EndType, object Parameter);

/// <summary>
/// Runner that orchestrates the execution of AIGC task pages, handling task creation, execution, and parent reporting.
/// </summary>
[DisplayText("Task Page Runner")]
internal class AigcLoopRunner : AIAssistant
{
    private readonly AigcLoopDocument _document;
    private readonly DocumentUsageToken _usageToken = new(nameof(AigcLoopRunner));

    private IAigcTaskPage _lastTask;
    private AIRequest _lastRequest;


    /// <summary>
    /// Initializes a new instance of the <see cref="AigcLoopRunner"/> class.
    /// </summary>
    /// <param name="document">The task page document to run tasks for.</param>
    public AigcLoopRunner(AigcLoopDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <inheritdoc/>
    public override async Task<AICallResult> HandleRequest(AIRequest request)
    {
        if (_lastRequest is not null)
        {
            return AICallResult.Empty;
        }

        try
        {
            _lastRequest = request;

            if (request.UserMessage == "/resume")
            {
                return await HandleResume(request);
            }
            else
            {
                return await HandleNew(request);
            }
        }
        finally
        {
            _lastRequest = null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the task runner is currently executing a task.
    /// </summary>
    public bool IsRunning => _lastRequest != null;

    /// <summary>
    /// Requests cancellation of the currently running task.
    /// </summary>
    public void RequestCancel()
    {
        _lastRequest?.RequestCancel?.Invoke();
    }


    private async Task<AICallResult> HandleNew(AIRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return AICallResult.FromFailed("Please enter task content");
        }

        var startupPageAsset = _document.StartupPage;
        if (startupPageAsset is null)
        {
            return AICallResult.FromFailed("Startup page not set");
        }

        var startupTask = AigcWorkflowPage.CreateWorkflowPage(_document, startupPageAsset);
        if (startupTask is null)
        {
            return AICallResult.FromFailed("Failed to create startup page");
        }

        if (startupTask.EnsureInstance() is null)
        {
            return AICallResult.FromFailed("Failed to instantiate startup page");
        }

        string userMessage = request.UserMessage;
        if (userMessage == "/init")
        {
            userMessage = _document.InitialTaskPrompt;
        }

        startupTask.SetPrompt(userMessage);
        startupTask.SetScratchPad(ScratchPadTypes.Clear, null, null, null);

        // Disalbe last un-calculated tasks to avoid unexpected task running when the task tree is being built.
        int count = _document.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (_document.GetTaskAt(i) is { } task && !task.GetAllDone())
            {
                task.CommitStatus = TaskCommitStatus.TaskDisabled;
            }
            else
            {
                break;
            }
        }

        _document.AddTask(startupTask);
        _document.Entry.MarkUsage(_usageToken);

        try
        {
            return await HandleRun(request);
        }
        finally
        {
            _document.Entry.UnmarkUsage(_usageToken);
        }
        
    }

    private Task<AICallResult> HandleResume(AIRequest request)
    {
        return HandleRun(request);
    }

    private async Task<AICallResult> HandleRun(AIRequest request)
    {
        while (true)
        {
            if (request.Cancellation.IsCancellationRequested)
            {
                return AICallResult.FromFailed("Task canceled.");
            }

            if (_document.Count == 0)
            {
                return AICallResult.FromMessage("No tasks have been created.");
            }

            // Get task for running, if the last task is not completed, continue to run it; otherwise get the next task to run.
            var task = _document.GetUnfinishedChildTaskDeep();
            if (task is null || task.GetCommitStatus() == TaskCommitStatus.TaskDisabled)
            {
                return AICallResult.FromMessage("All tasks have been completed.");
            }

            (bool flowControl, AICallResult value) = await RunTask(request, task);
            if (!flowControl)
            {
                return value;
            }
        }
    }

    private async Task<(bool flowControl, AICallResult value)> RunTask(AIRequest request, AigcTaskPage task)
    {
        if ((task.GetPageInstance()?.GetIsDoneInputs()).IsFalse())
        {
            return (flowControl: false, value: AICallResult.FromFailed("Task input is missing, it may be stuck. Task canceled."));
        }

        TaskCommitStatus status = task.GetCommitStatus();

        if (task == _lastTask)
        {
            if (status == TaskCommitStatus.TaskDisabled)
            {
                return (flowControl: false, value: AICallResult.FromFailed("Task is disabled. Task canceled."));
            }
            else if (status != TaskCommitStatus.None)
            {
                return (flowControl: false, value: AICallResult.Success);
            }
            else
            {
                return (flowControl: false, value: AICallResult.FromFailed("Task is not completed, it may be stuck. Task canceled."));
            }
        }

        _lastTask = task;

        bool hasSubTask = task.Count > 0;
        if (!hasSubTask)
        {
            // Task begin.
            var runResult = await RunTaskWithRetry(request, task, TaskEventTypes.TaskBegin, null, null);
            if (request.Cancellation.IsCancellationRequested)
            {
                return (flowControl: false, value: AICallResult.FromFailed("Task canceled."));
            }
        }

        // When a task is completed but no end event is triggered,
        // try to trigger the sub-task completion event to ensure the parent task can correctly perceive the completion status of the sub-task.
        if (status == TaskCommitStatus.None && task.GetAllSubTaskDone() == true)
        {
            if (task.GetTaskAt(task.Count - 1) is { } lastTask)
            {
                CheckCommitScratchPad(task, lastTask);

                // Task commit.
                var eventType = lastTask.GetCommitStatus().ToEventType();

                var commitResult = await RunTaskWithRetry(request, task, eventType, lastTask.CommitName, null);
                if (request.Cancellation.IsCancellationRequested)
                {
                    return (flowControl: false, value: AICallResult.FromFailed("Task canceled."));
                }
            }
        }

        return (flowControl: true, value: null);
    }

    private bool CheckCommitScratchPad(AigcTaskPage task, AigcTaskPage subTask)
    {
        if (task is not AigcWorkflowPage workflow || subTask is not AigcWorkflowPage subWorkflow)
        {
            return false;
        }

        var def = subWorkflow.Workflow?.GetBaseDefinition() as DesignFlowNode;
        var commit = subWorkflow?.GetAttribute<CommitScratchPad>() ?? def?.GetAttribute<CommitScratchPad>();
        if (commit is null)
        {
            return false;
        }


        var scratchPads = subWorkflow.GetScratchPads();
        foreach (var scratchPad in scratchPads)
        {
            workflow.SetScratchPad(scratchPad.Type, scratchPad.Path, scratchPad.Content, scratchPad.Note);
        }

        return true;
    }

    private async Task<TaskRunResult> RunTaskWithRetry(AIRequest request, AigcTaskPage task, TaskEventTypes eventType, string commitName, object parameter)
    {
        if (task.GetCommitStatus() == TaskCommitStatus.TaskDisabled)
        {
            return new(TaskCommitStatus.TaskDisabled, "Task is disabled.");
        }

        var retryConfig = AigcWorkflowPlugin.Instance?.Retry;
        int maxRetry = retryConfig?.RetryCount ?? 0;
        int retry = 0;
        float currentDelay = retryConfig?.Delay ?? 1;
        float multiplier = retryConfig?.DelayMultiplier ?? 1;
        float maxDelay = retryConfig?.MaxDelay ?? 60;

        while (true)
        {
            try
            {
                var result = await RunTask(request, task, eventType, commitName, parameter);
                return result;
            }
            catch (TaskCanceledException)
            {
                return new(TaskCommitStatus.None, "Task is cancelled.");
            }
            catch (Exception err)
            {
                request.Conversation.AddException(err);

                if (retryConfig != null && (maxRetry <= 0 || retry < maxRetry))
                {
                    var delay = new DelayCountDown(request.Conversation);
                    await delay.Run((int)currentDelay, request.Cancellation);
                    delay.Dispose();

                    currentDelay = Math.Min(currentDelay * multiplier, maxDelay);

                    if (maxRetry > 0)
                    {
                        retry++;
                        request.Conversation.AddSystemMessage($"Retry {retry}/{maxRetry} times.");
                    }
                    else
                    {
                        request.Conversation.AddSystemMessage($"Retry {retry} times.");
                    }
                }
                else
                {
                    return new(TaskCommitStatus.TaskFailed, $"{err.GetType().FullName} ({err.Message})");
                }
            }
        }
    }

    /// <summary>
    /// Executes a task event and returns the result of the execution.
    /// </summary>
    /// <param name="request">The AI request containing conversation and cancellation context.</param>
    /// <param name="task">The task page to execute the event on.</param>
    /// <param name="eventType">The type of event to trigger.</param>
    /// <param name="commitName">The name of the commit, if applicable.</param>
    /// <param name="parameter">The parameter to pass to the event handler.</param>
    /// <returns>A <see cref="TaskRunResult"/> containing the end type and result parameter.</returns>
    private async Task<TaskRunResult> RunTask(AIRequest request, AigcTaskPage task, TaskEventTypes eventType, string commitName, object parameter)
    {
        if (task.GetCommitStatus() == TaskCommitStatus.TaskDisabled)
        {
            return new(TaskCommitStatus.TaskDisabled, "Task is disabled.");
        }

        bool autoSelect = task.TaskPageDocument?.AutoFocusRunningTask == true;
        if (autoSelect)
        {
            SelectTask(task);
        }

        string name = task.Name;
        if (!string.IsNullOrWhiteSpace(task.Description))
        {
            name = $"{task.Description} ({name})";
        }

/*        string message = "Run Task: ";
        if (eventType != TaskEventTypes.None)
        {
            message = $"Handle event: {eventType}";
        }

        request.Conversation.AddSystemMessage(message, msg =>
        {
            msg.AddCode(name);
            msg.AddButton("Locate", () => SelectTask(task));
        });*/

        bool handled = await task.RunTask(request, eventType, commitName, parameter);
        if (request.Cancellation.IsCancellationRequested)
        {
            return new(TaskCommitStatus.None, "Task is cancelled.");
        }

        if (!handled)
        {
            return new(TaskCommitStatus.None, "Task is not handled.");
        }

        var end = task.GetPageInstance()?.GetTaskCommitParameter();
        if (end is null)
        {
            return new(TaskCommitStatus.None, "Task it not finished.");
        }

        task.CommitStatus = end.EndType;
        if (end.EndType != TaskCommitStatus.None)
        {
            // Explicit commit.
            return new(end.EndType, end.Value);
        }

        bool? isDone = task.GetPageInstance()?.GetIsDone();
        if (isDone.IsTrueOrEmpty())
        {
            return new(end.EndType, end.Value);
        }

        return new(TaskCommitStatus.None, "Task is not done.");
    }

    private void SelectTask(AigcTaskPage task)
    {
        if (task is null)
        {
            return;
        }

        try
        {
            if (_document.View?.GetService<IViewSelectable>() is { } sel)
            {
                _document.ShowView();
                sel.SetSelection(new ViewSelection(task));
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <summary>
    /// Converts a <see cref="TaskCommitStatus"/> value to the corresponding <see cref="TaskEventTypes"/> value.
    /// </summary>
    /// <param name="endType">The page commit type to convert.</param>
    /// <returns>The corresponding <see cref="TaskEventTypes"/> value.</returns>
    public static TaskEventTypes ToEventType(TaskCommitStatus endType)
    {
        switch (endType)
        {
            case TaskCommitStatus.TaskFinished:
                return TaskEventTypes.SubTaskFinished;

            case TaskCommitStatus.TaskFailed:
                return TaskEventTypes.SubTaskFailed;

            case TaskCommitStatus.None:
            case TaskCommitStatus.TaskDisabled:
            default:
                return TaskEventTypes.None;
        }

    }

}
