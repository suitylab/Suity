using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Views.Named;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.SubFlows;

#region ISubFlowDef

/// <summary>
/// Represents an sub-flow definition that can provide its definition, result, and associated document item.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Sub-flow Definition", Color = FlowColors.Task, Icon = "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.IAigcPage")]
public interface ISubFlowDef : INamed
{
    /// <summary>
    /// Gets the page definition.
    /// </summary>
    ISubFlowDef GetPageDefinition();

    /// <summary>
    /// Gets the page result.
    /// </summary>
    ISubFlowDef GetPageResult();

    /// <summary>
    /// Gets the associated document item for this page.
    /// </summary>
    object GetDocumentItem();
}

#endregion

#region ISubFlowDefAsset

/// <summary>
/// Represents an asset that defines an AIGC page.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Sub-flow Definition Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Page")]
public interface ISubFlowDefAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the base definition of the page.
    /// </summary>
    ISubFlowDef GetBaseDefinition();
}

#endregion

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

#region IAigcPageInstance

/// <summary>
/// Represents an element within an AIGC page that can report its completion status and output history capabilities.
/// </summary>
public interface ISubFlowElement : INamed
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
[NativeType(CodeBase = "AIGC", Description = "Sub-flow Instance", Color = FlowColors.Task, Icon = "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.IAigcPageInstance")]
[NativeAlias("*AIGC|IAigcPageInstance")]
public interface ISubFlowInstance : ISubFlowElement
{
    /// <summary>
    /// Gets the owner of this page instance.
    /// </summary>
    object Owner { get; }

    /// <summary>
    /// Gets the base definition of this page.
    /// </summary>
    ISubFlowDef BaseDefinition { get; }

    /// <summary>
    /// Gets the tool asset associated with this page.
    /// </summary>
    ISubFlowAsset GetToolAsset();

    /// <summary>
    /// Gets all elements contained in this page.
    /// </summary>
    IEnumerable<ISubFlowElement> Elements { get; }

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
    ISubFlowAsset GetDefinitionPage();
}

#endregion

#region ISubFlowAsset

/// <summary>
/// Represents a tool asset that can create page instances and provides access to page and skill definitions.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "AIGC Tool Asset", Color = FlowColors.Tool, Icon = "*CoreIcon|Tool")]
public interface ISubFlowAsset : INamed, IHasId
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
    ISubFlowDef GetBaseDefinition();

    /// <summary>
    /// Creates a new page instance with the specified options.
    /// </summary>
    ISubFlowInstance CreateInstance(PageElementOption option);
}

#endregion