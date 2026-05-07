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
    /// Gets the name of the task.
    /// </summary>
    public string TaskName { get; }

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public TextStatus TaskStatus { get; }

    /// <summary>
    /// Gets the task host that manages this task.
    /// </summary>
    IAigcTaskHost TaskHost { get; }

    /// <summary>
    /// Gets the page instance associated with this task.
    /// </summary>
    ISubFlowInstance GetPageInstance();

    /// <summary>
    /// Gets the task prompt, optionally including hierarchical context.
    /// </summary>
    string GetPrompt(bool inHierarchy);

    /// <summary>
    /// Sets the task prompt.
    /// </summary>
    void SetPrompt(string taskPromt);

    /// <summary>
    /// Appends a task using the specified tool asset.
    /// </summary>
    bool AppendTask(ISubFlowAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Appends a task using the specified page instance.
    /// </summary>
    bool AppendTask(ISubFlowInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a subtask using the specified tool asset.
    /// </summary>
    bool AddSubTask(ISubFlowAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a subtask using the specified page instance.
    /// </summary>
    bool AddSubTask(ISubFlowInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Gets the last subtask added to this task page.
    /// </summary>
    IAigcTaskPage GetLastSubTask();

    /// <summary>
    /// Gets all subtasks of this task page.
    /// </summary>
    IAigcTaskPage[] GetAllSubTasks();

    /// <summary>
    /// Resolves the article, taking into account returning transferable article parameters.
    /// </summary>
    /// <param name="autoCreate"></param>
    /// <returns></returns>
    IArticle ResolveArticle(bool autoCreate);

    /// <summary>
    /// Resolves the article without considering returning transferable article parameters.
    /// </summary>
    /// <param name="autoCreate"></param>
    /// <returns></returns>
    IArticle ResolveArticleBase(bool autoCreate);

    /// <summary>
    /// Gets the chat history
    /// </summary>
    /// <param name="inHierarchy">Get parent chat history</param>
    /// <returns></returns>
    LLmMessage[] GetChatHistory(bool inHierarchy);

    /// <summary>
    /// Gets the tool list
    /// </summary>
    /// <param name="includeDocumentTools"></param>
    /// <returns></returns>
    ISubFlowAsset[] GetToolList(bool includeDocumentTools);
}

#endregion

#region IAigcSkill

/// <summary>
/// Represents an AIGC skill that provides tools and parameter access.
/// </summary>
public interface IAigcSkill
{
    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    string SkillName { get; }

    /// <summary>
    /// Gets the tooltips/description of the skill.
    /// </summary>
    string SkillTooltips { get; }

    /// <summary>
    /// Used to provide guidance on how to use the skill effectively.
    /// </summary>
    string PromptHint { get; }

    /// <summary>
    /// Gets all tools provided by this skill.
    /// </summary>
    IEnumerable<ISubFlowAsset> Tools { get; }

    /// <summary>
    /// Gets whether this skill is a startup page.
    /// </summary>
    bool IsStartupPage { get; }

    /// <summary>
    /// Gets whether this skill uses the parent article.
    /// </summary>
    bool UseParentArticle { get; }

    /// <summary>
    /// Attempts to retrieve a parameter value by name.
    /// </summary>
    bool TryGetParameter(string name, out object value);
}


#endregion

#region IHasSkill

public interface IHasSkill
{
    /// <summary>
    /// Gets the skill definition.
    /// </summary>
    IAigcSkill GetSkill();
}

#endregion