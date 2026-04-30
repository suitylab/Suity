using System.Collections.Generic;
using System.Text;

namespace Suity.Editor.AIGC.Assistants;

#region LLmPresetType

/// <summary>
/// Preset LLM
/// </summary>
public enum LLmModelPreset
{
    [DisplayText("Default")]
    Default,

    /// <summary>
    /// Classify
    /// </summary>
    [DisplayText("Classify")]
    Classify,

    /// <summary>
    /// Chat
    /// </summary>
    [DisplayText("Chat")]
    Chat,

    /// <summary>
    /// Brief
    /// </summary>
    [DisplayText("Brief")]
    Brief,

    /// <summary>
    /// Summary
    /// </summary>
    [DisplayText("Summary")]
    Summary,

    /// <summary>
    /// Identifier Generation
    /// </summary>
    [DisplayText("Identifier Generation")]
    Identifier,

    /// <summary>
    /// Query Keyword Generation
    /// </summary>
    [DisplayText("Query Keyword Generation")]
    QueryKeyword,

    /// <summary>
    /// Tool Calling
    /// </summary>
    [DisplayText("Tool Calling")]
    ToolCalling,

    /// <summary>
    /// Creative Tool Calling
    /// </summary>
    [DisplayText("Creative Tool Calling")]
    CreativeToolCalling,

    /// <summary>
    /// Exact Tool Calling
    /// </summary>
    [DisplayText("Exact Tool Calling")]
    ExactToolCalling,

    /// <summary>
    /// Data Generate Tool Calling
    /// </summary>
    [DisplayText("Data Generate Tool Calling")]
    DataGenerateToolCalling,

    /// <summary>
    /// Creative Writing
    /// </summary>
    [DisplayText("Creative Writing")]
    CreativeWriting,

    /// <summary>
    /// Design Writing
    /// </summary>
    [DisplayText("Design Writing")]
    DesignWriting,

    /// <summary>
    /// Technical Writing
    /// </summary>
    [DisplayText("Technical Writing")]
    TechnicalWriting,

    /// <summary>
    /// Answer Question
    /// </summary>
    [DisplayText("Answer Question")]
    AnswerQuestion,

    /// <summary>
    /// Selection
    /// </summary>
    [DisplayText("Selection")]
    Selection,

    /// <summary>
    /// Code Writing
    /// </summary>
    [DisplayText("Code Writing")]
    Coding,

    /// <summary>
    /// Code Repair
    /// </summary>
    [DisplayText("Code Repair")]
    CodeRepair,
}

#endregion

#region Classify

/// <summary>
/// Represents the main input types provided by the user.
/// </summary>
public enum UserMainInputTypes
{
    /// <summary>
    /// Unknown input type.
    /// </summary>
    [DisplayText("Unknown")]
    Unknown,

    /// <summary>
    /// Operation-related input.
    /// </summary>
    [DisplayText("Operation")]
    Operation,

    /// <summary>
    /// Knowledge base-related input.
    /// </summary>
    [DisplayText("Knowledge Base")]
    Knowledge,

    /// <summary>
    /// Database-related input.
    /// </summary>
    [DisplayText("Database")]
    Database,

    /// <summary>
    /// Get/retrieve information input.
    /// </summary>
    [DisplayText("Get")]
    Get,

    /// <summary>
    /// Ask a question input.
    /// </summary>
    [DisplayText("Ask Question")]
    Ask,
}

/// <summary>
/// Represents the types of operations the user can perform.
/// </summary>
public enum UserOperationTypes
{
    /// <summary>
    /// Unknown operation type.
    /// </summary>
    [DisplayText("Unknown")]
    Unknown,

    /// <summary>
    /// Create a new item.
    /// </summary>
    [DisplayText("Create")]
    Create,

    /// <summary>
    /// Update an existing item.
    /// </summary>
    [DisplayText("Update")]
    Update,

    /// <summary>
    /// Delete an item.
    /// </summary>
    [DisplayText("Delete")]
    Delete,

    /// <summary>
    /// Knowledge base operation.
    /// </summary>
    [DisplayText("Knowledge Base")]
    Knowledge,

    /// <summary>
    /// Database operation.
    /// </summary>
    [DisplayText("Database")]
    Database,

    /// <summary>
    /// Multiple operations combined.
    /// </summary>
    [DisplayText("Multiple Operation")]
    Multiple,

    /// <summary>
    /// Get/retrieve information.
    /// </summary>
    [DisplayText("Get")]
    Get,

    /// <summary>
    /// Ask a question.
    /// </summary>
    [DisplayText("Ask Question")]
    Ask,
}

/// <summary>
/// Represents the target types of an operation.
/// </summary>
public enum OperationTargetTypes
{
    /// <summary>
    /// Unknown target type.
    /// </summary>
    [DisplayText("Unknown")]
    Unknown,

    /// <summary>
    /// Structure design target.
    /// </summary>
    [DisplayText("Structure Design")]
    Structure,

    /// <summary>
    /// Data design target.
    /// </summary>
    [DisplayText("Data Design")]
    Data,

    /// <summary>
    /// Article writing target.
    /// </summary>
    [DisplayText("Article Writing")]
    Article,
}


/// <summary>
/// Query Scope
/// </summary>
public enum QueryScopeTypes
{
    [DisplayText("Unknown")]
    Unknown,

    [DisplayText("Get Overview of All Objects in Document")]
    AllOverview,

    [DisplayText("Get Overview of Matched Objects")]
    Overview,

    [DisplayText("Get Full Content of Matched Objects")]
    AppliedContent,
}

/// <summary>
/// Generate Multiple Types
/// </summary>
public enum GenerateMultipleTypes
{
    [DisplayText("Unknown")]
    Unknown,

    [DisplayText("Single")]
    Single,

    [DisplayText("Multiple")]
    Multiple,
}

/// <summary>
/// Generate Source Types
/// </summary>
public enum GenerateSourceTypes
{
    [DisplayText("Unknown")]
    Unknown,

    [DisplayText("Direct Manual Input")]
    Manual,

    [DisplayText("Knowledge Base")]
    Knowledge,
}
#endregion

#region AssistantCall

/// <summary>
/// Represents a chain of assistant calls with an instruction and a list of individual calls.
/// </summary>
/// <typeparam name="T">The type of the assistant identifier.</typeparam>
public class AssistantCallChain<T>
{
    /// <summary>
    /// Gets or sets the instruction for the assistant call chain.
    /// </summary>
    public string Instruction { get; init; }

    /// <summary>
    /// Gets or sets the list of assistant calls in the chain.
    /// </summary>
    public List<AssistantCall<T>> Calls { get; init; } = [];

    /// <summary>
    /// Converts the entire call chain to a full text representation.
    /// </summary>
    /// <returns>A formatted string containing the instruction and all calls.</returns>
    public string ToFullText()
    {
        StringBuilder builder = new();

        builder.AppendLine(Instruction);

        for (int i = 0; i < Calls.Count; i++)
        {
            builder.AppendLine($"{i + 1}. {Calls[i].ToFullText()}");
        }

        return builder.ToString();
    }
}

/// <summary>
/// Represents a single assistant call with an assistant reference and a calling message.
/// </summary>
/// <typeparam name="T">The type of the assistant identifier.</typeparam>
public class AssistantCall<T>
{
    /// <summary>
    /// Gets or sets the assistant for this call.
    /// </summary>
    public T Assistant { get; init; }

    /// <summary>
    /// Gets or sets the message used when calling the assistant.
    /// </summary>
    public string CallingMessage { get; init; }

    /// <summary>
    /// Converts the assistant call to a full text representation.
    /// </summary>
    /// <returns>A formatted string containing the assistant display text and the calling message.</returns>
    public string ToFullText()
    {
        return $"Select Assistant: [{Assistant?.ToDisplayText()}]\nCommand: {CallingMessage}";
    }
}

#endregion
