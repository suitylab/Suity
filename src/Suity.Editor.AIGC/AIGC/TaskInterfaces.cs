using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

#region TaskDirectoryTargets

/// <summary>
/// Defines the target locations for task directories, such as workspaces or assets.
/// </summary>
public enum TaskDirectoryTargets
{
    None,
    WorkSpace,
    Assets,
}

#endregion

#region IAigcLoop

/// <summary>
/// Represents a loop of tasks that can be executed in sequence.
/// </summary>
public interface IAigcLoop
{
    /// <summary>
    /// Gets the initial task prompt.
    /// </summary>
    string InitialTaskPrompt { get; }

    /// <summary>
    /// Gets the current workspace.
    /// </summary>
    WorkSpace WorkSpace { get; }

    /// <summary>
    /// Creates a new workspace with the specified name and assigns it to this document.
    /// </summary>
    /// <param name="workSpaceName">The name of the workspace to create.</param>
    /// <returns>The newly created <see cref="WorkSpace"/>.</returns>
    WorkSpace CreateWorkSpace(string workSpaceName);

    /// <summary>
    /// Gets the collection of knowledge articles used as reference material.
    /// </summary>
    IEnumerable<IArticleAsset> KnowledgeArticles { get; }

    /// <summary>
    /// Adds a knowledge article to the document's knowledge collection if not already present.
    /// </summary>
    /// <param name="articleAsset">The article asset to add.</param>
    void AddKnowledgeArticle(IArticleAsset articleAsset);

    /// <summary>
    /// Gets the task page associated with the specified task ID.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>The task page associated with the specified task ID, or null if not found.</returns>
    IAigcTaskPage GetTask(string taskId);

        /// <summary>
    /// Gets sub-tasks as an array.
    /// </summary>
    /// <returns>An array of sub-tasks.</returns>
    IAigcTaskPage[] GetSubTasks();

    TaskCommitStatus GetCommitStatus();

    IAigcTaskPage GetLastTask();
}

#endregion

#region IAigcTaskHostAsset

public interface IAigcLoopAsset
{
    IAigcLoop GetLoop();
}

#endregion

#region IAigcTaskPage

/// <summary>
/// Represents an AIGC task page that manages task execution, subtasks, prompts, and chat history.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Task Page", Color = FlowColors.Task, Icon = "*CoreIcon|Task")]
public interface IAigcTaskPage : INamed, ITextDisplay, IAttributeGetter
{
    /// <summary>
    /// Gets the parent loop.
    /// </summary>
    IAigcLoop ParentLoop { get; }

    /// <summary>
    /// Gets the parent task page, or null if this is a top-level task.
    /// </summary>
    IAigcTaskPage ParentTask { get; }

    /// <summary>
    /// Gets the task ID, which is used to uniquely identify the task.
    /// </summary>
    string TaskId { get; }

    /// <summary>
    /// Gets the task commit name, which is used to match against event nodes for handling AI request events.
    /// </summary>
    string CommitName { get; }

    /// <summary>
    /// Gets the task status.
    /// </summary>
    TaskCommitStatus GetCommitStatus();

    /// <summary>
    /// Gets the page asset associated with this task.
    /// </summary>
    /// <returns></returns>
    IPageAsset GetPageAsset();

    /// <summary>
    /// Gets the page instance associated with this task.
    /// </summary>
    IPageInstance GetPageInstance();

    /// <summary>
    /// Gets the chat messages for this task, optionally including input and output messages.
    /// </summary>
    /// <param name="input">If true, includes input messages.</param>
    /// <param name="output">If true, includes output messages.</param>
    /// <returns>An array of <see cref="LLmMessage"/> objects representing the chat messages.</returns>
    LLmMessage[] GetChatMessages(bool input, bool output);

    /// <summary>
    /// Handles an AI request event by finding matching begin elements and executing them.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="eventType">The type of event to handle.</param>
    /// <param name="commitName">The commit name to match against event nodes.</param>
    /// <param name="parameter">The parameter to pass to the event handler.</param>
    /// <returns>True if any events were handled; otherwise, false.</returns>
    Task<bool> RunTask(AIRequest request, TaskEventTypes eventType, string commitName, object parameter);

    T AddAttribute<T>(Action<T> setter) where T : DesignAttribute, new();

    void RemoveAttributes<T>() where T : DesignAttribute;
}

#endregion

#region IAigcWorkflowPage

/// <summary>
/// Represents an AIGC workflow page that manages workflow execution, subtasks, prompts, and chat history.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Workflow Page", Color = FlowColors.Task, Icon = "*CoreIcon|Workflow")]
public interface IAigcWorkflowPage : IAigcTaskPage, IScratchPadOwner
{
    /// <summary>
    /// Gets the sub-flow instance associated with this workflow page.
    /// </summary>
    ISubFlowInstance GetSubFlowInstance();

    /// <summary>
    /// Gets the current task prompt.
    /// </summary>
    /// <returns>Return the current task prompt.</returns>
    string GetPrompt();

    /// <summary>
    /// Sets the task prompt and marks the document as dirty for saving.
    /// </summary>
    /// <param name="prompt">The prompt text to set.</param>
    void SetPrompt(string taskPromt);

    /// <summary>
    /// Gets the last available prompt in the workflow sequence, which may be used for the next task or sub-task. This is typically the most recent prompt that was set or used in the workflow.
    /// </summary>
    /// <returns>The last prompt used by the workflow sequence.</returns>
    string GetLastPrompt();

    /// <summary>
    /// Gets the last available prompt in the workflow sequence, which may be used for the next task or sub-task. This is typically the most recent prompt that was set or used in the workflow.
    /// Optionally including prompts from the parent hierarchy.
    /// </summary>
    /// <param name="inHierarchy">If true, collects prompts from all parent tasks in the hierarchy.</param>
    /// <returns>The task prompt text, potentially combined with parent prompts.</returns>
    string GetLastPrompt(bool inHierarchy);

    /// <summary>
    /// Gets or sets the rule prompt associated with this workflow page.
    /// </summary>
    PromptAsset Rule { get; set; }

    /// <summary>
    /// Gets the rule prompt, optionally including prompts from the parent hierarchy.
    /// </summary>
    /// <param name="inHierarchy">If true, collects prompts from all parent tasks in the hierarchy.</param>
    /// <returns>The rule prompt asset, potentially combined with parent prompts.</returns>
    PromptAsset GetRule(bool inHierarchy);

    /// <summary>
    /// Appends a new task to the parent list using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="rule">The rule prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    bool AppendTask(IPageAsset asset, string title = null, string taskPrompt = null, PromptAsset rule = null, string commitName = null);

    /// <summary>
    /// Appends a new task to the parent list using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="rule">The rule prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    bool AppendTask(IPageInstance pageInstance, string title = null, string taskPrompt = null, PromptAsset rule = null, string commitName = null);

    /// <summary>
    /// Adds a new sub-task using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="rule">The rule prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    bool AddSubTask(IPageAsset asset, string title = null, string taskPrompt = null, PromptAsset rule = null, string commitName = null);

    /// <summary>
    /// Adds a new sub-task using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="rule">The rule prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    bool AddSubTask(IPageInstance pageInstance, string title = null, string taskPrompt = null, PromptAsset rule = null, string commitName = null);

    /// <summary>
    /// Gets the last sub-task in this task's collection.
    /// </summary>
    /// <returns>The last sub-task, or null if no sub-tasks exist.</returns>
    IAigcTaskPage GetLastSubTask();

    /// <summary>
    /// Gets sub-tasks as an array.
    /// </summary>
    /// <returns>An array of sub-tasks.</returns>
    IAigcTaskPage[] GetSubTasks();

    /// <summary>
    /// Gets the number of sub-tasks in this task's collection.
    /// </summary>
    int SubTaskCount { get; }

    /// <summary>
    /// Resolves the article for this task, checking for transferable article parameters first.
    /// </summary>
    /// <param name="autoCreate">If true, automatically creates the article if it doesn't exist.</param>
    /// <returns>The resolved article, or null if not available.</returns>
    IArticle ResolveArticle(bool autoCreate);

    /// <summary>
    /// Resolves the base article for this task, optionally using the parent's article.
    /// </summary>
    /// <param name="autoCreate">If true, automatically creates the article if it doesn't exist.</param>
    /// <returns>The resolved article, or null if not available.</returns>
    IArticle ResolveArticleBase(bool autoCreate);

    /// <summary>
    /// Gets the chat history for this task, optionally including parent hierarchy.
    /// </summary>
    /// <param name="hierarchyLevels">The number of parent levels to include in the chat history. If 0, disables hierarchy inclusion.</param>
    /// <returns>An array of LLM messages representing the chat history.</returns>
    LLmMessage[] GetChatHistory(int hierarchyLevels = 0);

    /// <summary>
    /// Gets the list of available tools for this task page.
    /// </summary>
    /// <param name="includeDocumentTools">If true, includes tools from the document.</param>
    /// <returns>An array of available tool assets.</returns>
    IPageAsset[] GetToolList(bool includeDocumentTools);
}

#endregion

#region IAigcToolPage

public interface IAigcToolPage : IAigcTaskPage
{
}

#endregion

#region IScratchPadOwner

/// <summary>
/// Represents an object that can own scratch pad items.
/// </summary>
public interface IScratchPadOwner
{
    /// <summary>
    /// Clears all scratch pad items.
    /// </summary>
    void ClearScratchPad();

    /// <summary>
    /// Sets a scratch pad item.
    /// </summary>
    /// <param name="type">The type of scratch pad item.</param>
    /// <param name="path">The path for the scratch pad item.</param>
    /// <param name="content">The content for the scratch pad item.</param>
    /// <param name="note">The note for the scratch pad item.</param>
    ScratchPad SetScratchPad(ScratchPadTypes type, string path, string content, string note);

    /// <summary>
    /// Gets the scratch pad items for this task.
    /// </summary>
    /// <returns>The scratch pad items, or null if not available.</returns>
    ScratchPad[] GetScratchPads();

    /// <summary>
    /// Resolves the history scratch pad items for this task, checking for transferable article parameters first.
    /// </summary>
    /// <param name="hierarchyLevels">The number of parent levels to include in the scratch pad items.</param>
    /// <returns>The resolved scratch pad items, or null if not available.</returns>
    ScratchPad[] GetHistoryScratchPads(int hierarchyLevels = 0);

    ScratchPad GetHistoryScratchPad(string path, int hierarchyLevels = 0);
}

#endregion

#region IAigcLoopRunner

/// <summary>
/// Represents a runner for an AI loop.
/// </summary>
public interface IAigcLoopRunner
{
    bool IsRunning { get; }

    IAigcTaskPage LastTask { get; }

    void RequestCancel();
}

#endregion