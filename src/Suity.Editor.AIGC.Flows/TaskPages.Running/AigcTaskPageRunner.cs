using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.TaskPages.Running;

/// <summary>
/// Represents the result of a task execution, including the end type and associated parameter.
/// </summary>
/// <param name="EndType">The type of page commit that ended the task.</param>
/// <param name="Parameter">The parameter associated with the task result.</param>
internal record TaskRunResult(PageCommitTypes EndType, object Parameter);

/// <summary>
/// Runner that orchestrates the execution of AIGC task pages, handling task creation, execution, and parent reporting.
/// </summary>
[DisplayText("Task Page Runner")]
internal class AigcTaskPageRunner : AIAssistant
{
    private readonly AigcTaskPageDocument _document;
    private readonly DocumentUsageToken _usageToken = new(nameof(AigcTaskPageRunner));

    private AigcTaskPage _lastTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcTaskPageRunner"/> class.
    /// </summary>
    /// <param name="document">The task page document to run tasks for.</param>
    public AigcTaskPageRunner(AigcTaskPageDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <inheritdoc/>
    public override Task<AICallResult> HandleRequest(AIRequest request)
    {
        if (_document.IsTaskEmpty)
        {
            return HandleNew(request);
        }
        else
        {
            return HandleResume(request);
        }
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

        var startupTask = AigcTaskPage.CreateTaskPage(_document, startupPageAsset);
        if (startupTask is null)
        {
            return AICallResult.FromFailed("Failed to create startup page");
        }

        if (startupTask.EnsureInstance() is null)
        {
            return AICallResult.FromFailed("Failed to instantiate startup page");
        }

        startupTask.SetPrompt(request.UserMessage);

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
            if (request.Cancel.IsCancellationRequested)
            {
                return AICallResult.FromFailed("Task canceled.");
            }

            // Get task for running, if the last task is not completed, continue to run it; otherwise get the next task to run.
            var task = _document.GetLastRunningTask();
            if (task is null)
            {
                break;
            }

            if (task.GetIsDoneInputs().IsFalse())
            {
                return AICallResult.FromFailed("Task input is missing, it may be stuck. Task canceled.");
            }

            if (task == _lastTask)
            {
                if (task.GetAllDone().IsTrueOrEmpty())
                {
                    return AICallResult.Success;
                }
                else
                {
                    return AICallResult.FromFailed("Task is not completed, it may be stuck. Task canceled.");
                }
            }

            _lastTask = task;

            if (_document.View?.GetService<IViewSelectable>() is { } sel)
            {
                sel.SetSelection(new ViewSelection(task));
            }

            var runResult = await RunTask(request, task, AigcTaskEventTypes.TaskBegin, null, null);
            if (request.Cancel.IsCancellationRequested)
            {
                return AICallResult.FromFailed("Task canceled.");
            }

            if (runResult.EndType != PageCommitTypes.None)
            {
                try
                {
                    // Commit to its parent task, if the task is committed as finished or failed, to trigger parent task continue running or some other logic.
                    await CommitToParent(request, task, runResult);
                }
                catch (TaskCanceledException)
                {
                    return AICallResult.FromFailed("Task canceled.");
                }
                catch (Exception err)
                {
                    request.Conversation.AddException(err);
                    return AICallResult.FromFailed("Task interrupted.");
                }
            }
        }

        if (request.Cancel.IsCancellationRequested)
        {
            return AICallResult.FromFailed("Task canceled.");
        }
        else
        {
            return AICallResult.FromMessage("Task completed.");
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
    private async Task<TaskRunResult> RunTask(AIRequest request, AigcTaskPage task, AigcTaskEventTypes eventType, string commitName, object parameter)
    {
        try
        {
            string name = task.Name;
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                name = $"{task.Description} ({name})";
            }

            string message = "Run Task: ";
            if (eventType != AigcTaskEventTypes.None)
            {
                message = $"Handle event: {eventType}";
            }

            request.Conversation.AddSystemMessage(message, msg =>
            {
                msg.AddCode(name);
                msg.AddButton("Locate", () => 
                {
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
                });
            });

            bool handled = await task.HandleEvent(request, eventType, commitName, parameter);
            if (request.Cancel.IsCancellationRequested)
            {
                return new(PageCommitTypes.None, "Task is cancelled.");
            }

            if (!handled)
            {
                return new(PageCommitTypes.None, "Task is not handled.");
            }

            bool? isDone = task.GetAllDone();
            if (!isDone.IsTrueOrEmpty())
            {
                return new(PageCommitTypes.None, "Task is not done.");
            }

            var end = task.Instance?.CurrentEndElement;
            if (end is null)
            {
                return new(PageCommitTypes.None, "Task it not finished.");
            }
            else
            {
                return new(end.EndType, end.Value);
            }
        }
        catch (TaskCanceledException)
        {
            return new(PageCommitTypes.None, "Task is cancelled.");
        }
        catch (Exception err)
        {
            request.Conversation.AddException(err);
            return new(PageCommitTypes.TaskFailed, $"{err.GetType().FullName} ({err.Message})");
        }
    }

    /// <summary>
    /// Commits the task result up the parent task hierarchy until no further reporting is needed.
    /// </summary>
    /// <param name="request">The AI request containing conversation and cancellation context.</param>
    /// <param name="task">The child task that completed execution.</param>
    /// <param name="runResult">The result of the child task execution.</param>
    /// <returns>An <see cref="AICallResult"/> indicating the success or failure of the reporting process.</returns>
    private async Task<AICallResult> CommitToParent(AIRequest request, AigcTaskPage task, TaskRunResult runResult)
    {
        while (task != null)
        {
            if (task.ParentNode is not AigcTaskPage parent)
            {
                break;
            }

            if (!task.GetAllDoneSubTasks())
            {
                break;
            }

            if (task.GetNextTask() != null)
            {
                break;
            }

            var eventType = ToEventType(runResult.EndType);
            if (eventType == AigcTaskEventTypes.None)
            {
                return AICallResult.Empty;
            }

            var parameter = runResult.Parameter;
            string commitName = task.CommitName;
            
            runResult = await RunTask(request, parent, eventType, commitName, parameter);
            task = parent;

            if (request.Cancel.IsCancellationRequested)
            {
                return AICallResult.Empty;
            }
        }

        return AICallResult.Success;
    }

    /// <summary>
    /// Converts a <see cref="PageCommitTypes"/> value to the corresponding <see cref="AigcTaskEventTypes"/> value.
    /// </summary>
    /// <param name="endType">The page commit type to convert.</param>
    /// <returns>The corresponding <see cref="AigcTaskEventTypes"/> value.</returns>
    public static AigcTaskEventTypes ToEventType(PageCommitTypes endType)
    {
        switch (endType)
        {
            case PageCommitTypes.TaskFinished:
                return AigcTaskEventTypes.SubTaskFinished;

            case PageCommitTypes.TaskFailed:
                return AigcTaskEventTypes.SubTaskFailed;

            case PageCommitTypes.None:
            default:
                return AigcTaskEventTypes.None;
        }

    }
}
