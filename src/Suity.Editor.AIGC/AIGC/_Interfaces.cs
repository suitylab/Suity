using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using System.Collections.Generic;

namespace Suity.Editor.AIGC;

#region IAigcTaskHost

/// <summary>
/// Represents a host for AIGC tasks, providing workspace management and knowledge article access.
/// </summary>
public interface IAigcTaskHost
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
    /// Creates a new workspace with the specified name.
    /// </summary>
    WorkSpace CreateWorkSpace(string workSpaceName);

    /// <summary>
    /// Gets all knowledge articles available to the task.
    /// </summary>
    IEnumerable<IArticleAsset> KnowledgeArticles { get; }

    /// <summary>
    /// Adds a knowledge article to the task.
    /// </summary>
    void AddKnowledgeArticle(IArticleAsset articleAsset);
}

#endregion

#region IAigcTaskPage

/// <summary>
/// Represents an AIGC task page that manages task execution, subtasks, prompts, and chat history.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Task Page", Color = FlowColors.Task, Icon = "*CoreIcon|Task")]
public interface IAigcTaskPage
{
    /// <summary>
    /// Gets the task name, preferring the description over the name if available.
    /// </summary>
    public string TaskName { get; }

    /// <summary>
    /// Gets the current text status of the task.
    /// </summary>
    public TextStatus TaskStatus { get; }

    /// <summary>
    /// Gets the task host document for this task page.
    /// </summary>
    IAigcTaskHost TaskHost { get; }
}

#endregion

#region IAigcWorkflowPage

/// <summary>
/// Represents an AIGC workflow page that manages workflow execution, subtasks, prompts, and chat history.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Workflow Page", Color = FlowColors.Task, Icon = "*CoreIcon|Workflow")]
public interface IAigcWorkflowPage : IAigcTaskPage
{
    /// <summary>
    /// Gets the page instance associated with this task.
    /// </summary>
    ISubFlowInstance GetPageInstance();

    /// <summary>
    /// Gets the task prompt, optionally including prompts from the parent hierarchy.
    /// </summary>
    /// <param name="inHierarchy">If true, collects prompts from all parent tasks in the hierarchy.</param>
    /// <returns>The task prompt text, potentially combined with parent prompts.</returns>
    string GetPrompt(bool inHierarchy);

    /// <summary>
    /// Sets the task prompt and marks the document as dirty for saving.
    /// </summary>
    /// <param name="prompt">The prompt text to set.</param>
    void SetPrompt(string taskPromt);

    /// <summary>
    /// Appends a new task to the parent list using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    bool AppendTask(ISubFlowAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Appends a new task to the parent list using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    bool AppendTask(ISubFlowInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a new sub-task using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    bool AddSubTask(ISubFlowAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a new sub-task using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    bool AddSubTask(ISubFlowInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Gets the last sub-task in this task's collection.
    /// </summary>
    /// <returns>The last sub-task, or null if no sub-tasks exist.</returns>
    IAigcWorkflowPage GetLastSubTask();

    /// <summary>
    /// Gets all sub-tasks as an array.
    /// </summary>
    /// <returns>An array of all sub-tasks.</returns>
    IAigcWorkflowPage[] GetAllSubTasks();

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
    /// <param name="inHierarchy">If true, includes chat history from parent tasks.</param>
    /// <returns>An array of LLM messages representing the chat history.</returns>
    LLmMessage[] GetChatHistory(bool inHierarchy);

    /// <summary>
    /// Gets the list of available tools for this task page.
    /// </summary>
    /// <param name="includeDocumentTools">If true, includes tools from the document.</param>
    /// <returns>An array of available tool assets.</returns>
    ISubFlowAsset[] GetToolList(bool includeDocumentTools);
}

#endregion