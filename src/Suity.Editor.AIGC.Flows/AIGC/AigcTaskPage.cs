using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public abstract class AigcTaskPage : DesignNode,
    IAigcTaskPage,
    IViewDoubleClickAction,
    INavigable
{
    readonly StringProperty _commitName = new("CommitName", "Commit Name", string.Empty, "Name used when committing to parent task.");

    protected AigcTaskPage()
    {
        
    }

    #region Core Prop

    /// <summary>
    /// Gets the document associated with this task page as an <see cref="AigcTaskPageDocument"/>.
    /// </summary>
    public AigcTaskPageDocument TaskPageDocument => this.GetDocument() as AigcTaskPageDocument;

    /// <summary>
    /// Gets the parent task page, if this task is a sub-task of another task.
    /// </summary>
    public AigcTaskPage ParentTask => ParentNode as AigcTaskPage;

    /// <summary>
    /// Gets or sets the commit name used when committing results to the parent task.
    /// </summary>
    public string CommitName
    {
        get => _commitName.Text ?? string.Empty;
        set => _commitName.Text = value ?? string.Empty;
    }

    #endregion

    #region Virtual / Override

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "#Task-";

    /// <inheritdoc/>
    protected override bool OnCanEditText() => false;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _commitName.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _commitName.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    #endregion

    #region Virtual (IAigcTaskPage)

    /// <inheritdoc/>
    public IAigcTaskHost TaskHost => this.GetDocument() as AigcTaskPageDocument;

    /// <inheritdoc/>
    public abstract IPageAsset GetPageAsset();

    /// <inheritdoc/>
    public abstract IPageInstance GetPageInstance();

    /// <inheritdoc/>
    public abstract Task<bool> RunTask(AIRequest request, TaskEventTypes eventType, string commitName, object parameter);

    #endregion

    #region Virtual (Other)


    /// <summary>
    /// Handle <see cref="IViewDoubleClickAction"/> interface
    /// </summary>
    protected virtual void OnDoubleClick()
    {
    }

    /// <summary>
    /// Handle <see cref="INavigable"/> interface
    /// </summary>
    /// <returns></returns>
    protected virtual object OnGetNavigationTarget()
    {
        return null;
    }

    #endregion

    #region IViewDoubleClickAction

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick() => OnDoubleClick();

    #endregion

    #region INavigable

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget() => OnGetNavigationTarget();

    #endregion

    #region Task

    /// <summary>
    /// Gets a value indicating whether this task has no sub-tasks.
    /// </summary>
    public bool IsSubTaskEmpty => Count == 0;

    /// <summary>
    /// Gets the task at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the task to retrieve.</param>
    /// <returns>The task at the specified index, or null if the index is out of range.</returns>
    public AigcTaskPage GetTaskAt(int index)
    {
        if (index >= 0 && index < Count)
        {
            return GetItemAt(index) as AigcTaskPage;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets an enumerable collection of all tasks.
    /// </summary>
    public IEnumerable<AigcTaskPage> Tasks => Items.OfType<AigcTaskPage>();

    /// <summary>
    /// Gets the unfinished child task, searching from the last task backward.
    /// </summary>
    /// <returns>The first unfinished child <see cref="AigcTaskPage"/>, or null if all tasks are done.</returns>
    public AigcTaskPage GetUnfinishedChildTask()
    {
        int c = Count;
        if (c == 0)
        {
            return null;
        }

        AigcTaskPage unfinished = null;

        for (int i = c - 1; i >= 0; i--)
        {
            var task = GetTaskAt(i);
            if (task is null)
            {
                continue;
            }

            var allDone = task.GetPageInstance()?.GetAllDone();
            if (allDone.IsFalse())
            {
                unfinished = task;
                continue;
            }
            else
            {
                break;
            }
        }

        return unfinished;
    }

    /// <summary>
    /// Gets the unfinished child task, recursively checking sub-tasks.
    /// </summary>
    /// <returns>The last unfinished child task, or null if no tasks exist.</returns>
    public AigcTaskPage GetUnfinishedChildTaskDeep()
    {
        if (Count == 0)
        {
            return null;
        }

        var task = GetUnfinishedChildTask();
        if (task != null)
        {
            return task.GetUnfinishedChildTaskDeep() ?? task;
        }

        // This is the last completed task.
        //task = GetTaskAt(Count - 1);
        //if (task != null)
        //{
        //    return task.GetUnfinishedChildTaskDeep() ?? task;
        //}

        return null;
    }

    /// <summary>
    /// Gets the previous task in the parent list.
    /// </summary>
    /// <returns>The previous task, or null if this is the first task.</returns>
    public AigcTaskPage GetPreviousTask()
    {
        int index = this.GetIndex();
        if (index < 0 || index == 0)
        {
            return null;
        }

        return GetTaskAt(index - 1);
    }

    /// <summary>
    /// Gets the next task in the parent list.
    /// </summary>
    /// <returns>The next task, or null if this is the last task.</returns>
    public AigcTaskPage GetNextTask()
    {
        int index = this.GetIndex();
        if (index < 0 || index == ParentList.Count - 1)
        {
            return null;
        }

        return GetTaskAt(index + 1);
    }

    /// <summary>
    /// Adds a task to this task's collection.
    /// </summary>
    /// <param name="task">The task to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    public void AddTask(AigcTaskPage task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        AddItem(task);
    }



    /// <summary>
    /// Gets a value indicating whether the task is done, and with all sub-tasks also done.
    /// </summary>
    /// <returns>True if this task and all sub-tasks are done, false if any is not done.</returns>
    public bool GetAllDoneWithSubTasks()
    {
        var allDone = GetPageInstance()?.GetAllDone();
        if (allDone.IsFalse())
        {
            return false;
        }

        if (Count == 0)
        {
            return true;
        }

        return Items.OfType<AigcTaskPage>().All(o => o.GetAllDoneWithSubTasks());
    }

    /// <summary>
    /// Gets a value indicating whether all sub-tasks are done.
    /// </summary>
    /// <returns>True if all sub-tasks are done, false if any is not done, or null if no sub-tasks exist.</returns>
    public bool? GetAllSubTaskDone()
    {
        if (Count == 0)
        {
            return null;
        }

        return Items.OfType<AigcTaskPage>().All(o => (o.GetPageInstance()?.GetAllDone()).IsTrueOrEmpty());
    }
    

    #endregion



    /// <summary>
    /// Creates a new task page from a tool asset.
    /// </summary>
    /// <param name="asset">The tool asset to create the task from.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the AigcTaskPageDocument is not found.</exception>
    public IAigcTaskPage CreateTaskPage(IPageAsset asset, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            throw new InvalidOperationException("AigcTaskPageDocument not found.");
        }

        return CreateTaskPage(doc, asset, title, taskPrompt, commitName);
    }

    /// <summary>
    /// Creates a new task page from an existing page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to create the task from.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the AigcTaskPageDocument is not found.</exception>
    public IAigcTaskPage CreateTaskPage(IPageInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            throw new InvalidOperationException("AigcTaskPageDocument not found.");
        }

        return CreateTaskPage(doc, pageInstance, title, taskPrompt, commitName);
    }

    /// <summary>
    /// Creates a new task page from a page asset within the specified document.
    /// </summary>
    /// <param name="doc">The document to create the task page in.</param>
    /// <param name="pageAsset">The page asset to create the task from.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="doc"/> or <paramref name="pageAsset"/> is null.</exception>
    public static IAigcTaskPage CreateTaskPage(AigcTaskPageDocument doc, IPageAsset pageAsset, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (pageAsset is null)
        {
            throw new ArgumentNullException(nameof(pageAsset));
        }

        switch (pageAsset)
        {
            case ISubFlowAsset subFlowAsset:
                return AigcWorkflowPage.CreateWorkflowPage(doc, subFlowAsset, title, taskPrompt, commitName);

            default:
                throw new NotSupportedException($"{pageAsset.GetType().FullName} is not supported.");
        }
    }

    /// <summary>
    /// Creates a new task page from a page instance within the specified document.
    /// </summary>
    /// <param name="doc">The document to create the task page in.</param>
    /// <param name="pageInstance">The page instance to create the task from.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="doc"/> or <paramref name="pageInstance"/> is null.</exception>
    public static IAigcTaskPage CreateTaskPage(AigcTaskPageDocument doc, IPageInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (pageInstance is null)
        {
            throw new ArgumentNullException(nameof(pageInstance));
        }

        switch (pageInstance)
        {
            case ISubFlowInstance subFlowInstance:
                return AigcWorkflowPage.CreateWorkflowPage(doc, subFlowInstance, title, taskPrompt, commitName);

            default:
                throw new NotSupportedException($"{pageInstance.GetType().FullName} is not supported.");
        }
    }

}
