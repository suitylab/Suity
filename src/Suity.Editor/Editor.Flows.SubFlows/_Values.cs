using Suity.Editor.Types;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Represents the types of events that can occur during an Sub-flow task lifecycle.
/// </summary>
public enum TaskEventTypes
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

/// <summary>
/// Defines the types of page commit events that can occur in the Sub-flow.
/// </summary>
public enum TaskCommitTypes
{
    /// <summary>
    /// No commit type specified.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates the task has finished successfully.
    /// </summary>
    [DisplayText("Task Finished")]
    TaskFinished,

    /// <summary>
    /// Indicates the task has failed.
    /// </summary>
    [DisplayText("Task Failed")]
    TaskFailed,
}

public record TaskCommitInfo(TaskCommitTypes EndType, object Value);

/// <summary>
/// Represents a wrapper for history text content with implicit conversion support.
/// </summary>
[NativeType(CodeBase = "SubFlow")]
[NativeAlias("*AIGC|ChatHistoryText")]
public record HistoryText
{
    /// <summary>
    /// Gets an empty <see cref="HistoryText"/> instance.
    /// </summary>
    public static HistoryText Empty { get; } = new(string.Empty);

    /// <summary>
    /// Gets the underlying text content.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="HistoryText"/> with the specified text.
    /// </summary>
    public HistoryText(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>
    /// Returns the text content as a string representation.
    /// </summary>
    public override string ToString()
    {
        return Text;
    }

    /// <summary>
    /// Implicitly converts a <see cref="HistoryText"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(HistoryText text)
    {
        return text.Text;
    }

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="HistoryText"/>. Returns <see cref="Empty"/> if the text is null or whitespace.
    /// </summary>
    public static implicit operator HistoryText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return new HistoryText(text);
        }
        else
        {
            return Empty;
        }
    }
}

