using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Views.Named;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.TaskPages;

#region AigcTaskEventTypes

/// <summary>
/// Represents the types of events that can occur during an AIGC task lifecycle.
/// </summary>
public enum AigcTaskEventTypes
{
    /// <summary>
    /// No event.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates that a task has begun.
    /// </summary>
    [DisplayText("Task Start")]
    TaskBegin,

    /// <summary>
    /// Indicates that a subtask has completed successfully.
    /// </summary>
    [DisplayText("Subtask Completed")]
    SubTaskFinished,

    /// <summary>
    /// Indicates that a subtask has failed.
    /// </summary>
    [DisplayText("Subtask Failed")]
    SubTaskFailed,
}

#endregion

#region IAigcPage

/// <summary>
/// Represents an AIGC page that can provide its definition, result, and associated document item.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Aigc Page", Color = AigcColors.Task, Icon = "*CoreIcon|Page")]
public interface IAigcPage : INamed
{
    /// <summary>
    /// Gets the page definition.
    /// </summary>
    IAigcPage GetPageDefinition();

    /// <summary>
    /// Gets the page result.
    /// </summary>
    IAigcPage GetPageResult();

    /// <summary>
    /// Gets the associated document item for this page.
    /// </summary>
    object GetDocumentItem();
}
#endregion

#region IAigcPageInstance

/// <summary>
/// Represents an element within an AIGC page that can report its completion status and output history capabilities.
/// </summary>
public interface ISubGraphElement : INamed
{
    /// <summary>
    /// Gets whether this element is done. Returns null if the state is unknown.
    /// </summary>
    /// <returns></returns>
    bool? GetIsDone();

    /// <summary>
    /// Gets whether history output is available in the specified direction.
    /// </summary>
    bool GetCanOutputHistory(FlowDirections diraction);
}

/// <summary>
/// Represents an instance of an AIGC page, providing access to its definition, skill, tool asset, elements, and parameters.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Page Instance", Color = AigcColors.Task, Icon = "*CoreIcon|Page")]
public interface IAigcPageInstance : ISubGraphElement
{
    /// <summary>
    /// Gets the owner of this page instance.
    /// </summary>
    object Owner { get; }

    /// <summary>
    /// Gets the base definition of this page.
    /// </summary>
    IAigcPage BaseDefinition { get; }

    /// <summary>
    /// Gets the skill associated with this page.
    /// </summary>
    IAigcSkill GetSkill();

    /// <summary>
    /// Gets the tool asset associated with this page.
    /// </summary>
    IAigcToolAsset GetToolAsset();

    /// <summary>
    /// Gets all elements contained in this page.
    /// </summary>
    IEnumerable<ISubGraphElement> Elements { get; }

    /// <summary>
    /// Gets the input chat history.
    /// </summary>
    ChatHistoryText GetInputChatHistory();

    /// <summary>
    /// Gets the output chat history.
    /// </summary>
    ChatHistoryText GetOutputChatHistory();

    /// <summary>
    /// Gets the task commit information.
    /// </summary>
    ChatHistoryText GetTaskCommit();

    /// <summary>
    /// Converts this page instance to a simple type representation.
    /// </summary>
    SimpleType ToSimpleType();

    /// <summary>
    /// Sets a parameter value by name.
    /// </summary>
    void SetParameter(string name, object value);

    /// <summary>
    /// Gets all input parameters for this page.
    /// </summary>
    IEnumerable<IPageParameterInput> GetInputParameters();

    /// <summary>
    /// Gets whether all child elements are done/completed.
    /// </summary>
    /// <returns>True if all child elements are done, false if not, or null if indeterminate.</returns>
    bool? GetAllDone();
}

/// <summary>
/// Provides options for configuring a page element.
/// </summary>
public class PageElementOption
{
    /// <summary>
    /// Gets the mode of the page element.
    /// </summary>
    public PageElementMode Mode { get; init; }

    /// <summary>
    /// Gets the owner of the page element.
    /// </summary>
    public object Owner { get; init; }
}

/// <summary>
/// Defines the mode of a page element.
/// </summary>
public enum PageElementMode
{
    /// <summary>
    /// A standard page mode.
    /// </summary>
    Page,

    /// <summary>
    /// A task mode.
    /// </summary>
    Task,

    /// <summary>
    /// A function mode.
    /// </summary>
    Function,

    /// <summary>
    /// A skill mode.
    /// </summary>
    Skill,
}

#endregion

#region IFlowCallerContext

/// <summary>
/// Provides a context for calling and managing flow computations, including lifecycle events and parameter management.
/// </summary>
public interface IFlowCallerContext
{
    /// <summary>
    /// Gets or sets the title of the flow context.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Called when a flow computation begins.
    /// </summary>
    void OnBeginFlow(IFlowComputation computation, string name);

    /// <summary>
    /// Gets the data required to compute for a specific flow computation.
    /// </summary>
    string[] GetDatasToCompute(IFlowComputation computation, string name);

    /// <summary>
    /// Called when a flow computation ends.
    /// </summary>
    void OnEndFlow(IFlowComputation computation, string name, object value);

    /// <summary>
    /// Attempts to retrieve a parameter value by name.
    /// </summary>
    bool TryGetParameter(IFlowComputation computation, string name, out object value);

    /// <summary>
    /// Sets a parameter value by name.
    /// </summary>
    void SetParameter(IFlowComputation computation, string name, object value);

    /// <summary>
    /// Calls a function asynchronously with the given value.
    /// </summary>
    Task<object> CallFunction(IFlowComputation computation, string name, object value, CancellationToken cancel);

    /// <summary>
    /// Gets the definition page for the tool asset.
    /// </summary>
    IAigcToolAsset GetDefinitionPage();
}

#endregion

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
[NativeType(CodeBase = "AIGC", Description = "AIGC Task Page", Color = AigcColors.Task, Icon = "*CoreIcon|Task")]
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
    IAigcPageInstance GetPageInstance();

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
    bool AppendTask(IAigcToolAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Appends a task using the specified page instance.
    /// </summary>
    bool AppendTask(IAigcPageInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a subtask using the specified tool asset.
    /// </summary>
    bool AddSubTask(IAigcToolAsset asset, string title = null, string taskPrompt = null, string commitName = null);

    /// <summary>
    /// Adds a subtask using the specified page instance.
    /// </summary>
    bool AddSubTask(IAigcPageInstance pageInstance, string title = null, string taskPrompt = null, string commitName = null);

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
    IAigcToolAsset[] GetToolList(bool includeDocumentTools);
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
    IEnumerable<IAigcToolAsset> Tools { get; }

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

#region Assets

/// <summary>
/// Represents an asset that defines an AIGC page.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Page Definition Asset", Color = AigcColors.Page, Icon = "*CoreIcon|Page")]
public interface IAigcPageDefinitionAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the base definition of the page.
    /// </summary>
    IAigcPage GetBaseDefinition();
}

/// <summary>
/// Represents a tool asset that can create page instances and provides access to page and skill definitions.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Tool Asset", Color = AigcColors.Tool, Icon = "*CoreIcon|Tool")]
public interface IAigcToolAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the description of the tool asset.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets whether this tool asset is a startup page.
    /// </summary>
    bool IsStartupPage { get; }

    /// <summary>
    /// Gets the base page definition.
    /// </summary>
    IAigcPage GetBaseDefinition();

    /// <summary>
    /// Gets the skill definition.
    /// </summary>
    IAigcSkill GetSkillDefinition();

    /// <summary>
    /// Creates a new page instance with the specified options.
    /// </summary>
    IAigcPageInstance CreatePageInstance(PageElementOption option);
}

#endregion