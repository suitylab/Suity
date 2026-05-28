using Suity.Editor.Types;
using System.Collections.Generic;

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
/// Defines the possible statuses for a task commit, indicating the outcome of a task or subtask.
/// </summary>
public enum TaskCommitStatus
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

public record TaskCommitParameter(TaskCommitStatus EndType, object Value);

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
        return text?.Text;
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

public record HistoryTag
{
    public static HistoryTag Empty { get; } = new(string.Empty, null);

    public HistoryTag()
    {
    }

    public HistoryTag(HistoryText text)
    {
        Text = text;
    }

    public HistoryTag(HistoryText text, List<KeyValuePair<string, string>> attributes)
    {
        Text = text;
        Attributes = attributes;
    }


    public HistoryText Text { get; }

    public List<KeyValuePair<string, string>> Attributes { get; }


    public override string ToString() => Text;

    public static implicit operator string(HistoryTag text)
    {
        return text?.Text;
    }

    public static implicit operator HistoryText(HistoryTag text)
    {
        return text?.Text;
    }

    public static implicit operator HistoryTag(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return new HistoryTag(new HistoryText(text));
        }
        else
        {
            return Empty;
        }
    }

    public static implicit operator HistoryTag(HistoryText text)
    {
        if (!string.IsNullOrWhiteSpace(text.Text))
        {
            return new HistoryTag(text);
        }
        else
        {
            return Empty;
        }
    }
}

public enum ResolveChatIntents
{
    Normal,
    Preview,
}