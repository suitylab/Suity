using Suity.Editor.Types;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Represents the completion state of a page.
/// </summary>
public enum PageCompleteState
{
    /// <summary>
    /// No state assigned.
    /// </summary>
    None,

    /// <summary>
    /// The page is currently being processed.
    /// </summary>
    Working,

    /// <summary>
    /// The page has completed processing.
    /// </summary>
    Complete,
}

/// <summary>
/// Represents a page value element that holds a typed parameter value.
/// </summary>
public interface IPageValueElement : ISubFlowElement
{
    /// <summary>
    /// Gets the type definition of the parameter.
    /// </summary>
    TypeDefinition ParameterType { get; }

    /// <summary>
    /// Gets the current value of the element.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets or sets whether a value has been explicitly set.
    /// </summary>
    bool IsValueSet { get; set; }

    /// <summary>
    /// Sets the value of this element.
    /// </summary>
    void SetValue(object value);

    /// <summary>
    /// Ensures and returns the value, initializing it if necessary.
    /// </summary>
    object EnsureValue();
}

/// <summary>
/// Represents a page parameter that can indicate task completion, commit, and chat history.
/// </summary>
public interface IPageParameter : IPageValueElement
{
    /// <summary>
    /// Gets whether this parameter indicates task completion.
    /// </summary>
    bool TaskCompletion { get; }

    /// <summary>
    /// Gets whether this parameter represents a task commit.
    /// </summary>
    bool TaskCommit { get; }

    /// <summary>
    /// Gets whether this parameter contains chat history.
    /// </summary>
    bool ChatHistory { get; }

    /// <summary>
    /// Resolves and returns the chat history content.
    /// </summary>
    HistoryText ResolveChatHistory();
}

/// <summary>
/// Represents a page parameter that accepts input from an outer flow computation.
/// </summary>
public interface IPageParameterInput : IPageParameter
{
    /// <summary>
    /// Gets whether this parameter is a preset input.
    /// </summary>
    bool IsPresetInput { get; }

    /// <summary>
    /// Gets the value from the outer flow computation.
    /// </summary>
    object GetOuterValue(IFlowComputation outerCompute);
}

/// <summary>
/// Represents a page message parameter.
/// </summary>
public interface IPageMessage : IPageParameter
{

}

/// <summary>
/// Represents a page parameter that outputs values to an outer flow computation.
/// </summary>
public interface IPageParameterOutput : IPageParameter
{
    /// <summary>
    /// Sets the value in the outer flow computation.
    /// </summary>
    void SetOuterValue(IFlowComputation outerCompute, object value);
}

/// <summary>
/// Represents a page parameter associated with a tool.
/// </summary>
public interface IPageParameterTool : IPageParameter
{
    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    string ToolName { get; }
}
