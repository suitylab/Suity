using Suity.Editor.Types;

namespace Suity.Editor.Flows.SubFlows;

#region SubFlowEventTypes

/// <summary>
/// Represents the types of events that can occur during an Sub-flow task lifecycle.
/// </summary>
public enum SubFlowEventTypes
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

/// <summary>
/// Represents a wrapper for chat history text content with implicit conversion support.
/// </summary>
[NativeType(CodeBase = "AIGC")]
public record ChatHistoryText
{
    /// <summary>
    /// Gets an empty <see cref="ChatHistoryText"/> instance.
    /// </summary>
    public static ChatHistoryText Empty { get; } = new ChatHistoryText(string.Empty);

    /// <summary>
    /// Gets the underlying text content.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ChatHistoryText"/> with the specified text.
    /// </summary>
    public ChatHistoryText(string text)
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
    /// Implicitly converts a <see cref="ChatHistoryText"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(ChatHistoryText text)
    {
        return text.Text;
    }

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ChatHistoryText"/>. Returns <see cref="Empty"/> if the text is null or whitespace.
    /// </summary>
    public static implicit operator ChatHistoryText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return new ChatHistoryText(text);
        }
        else
        {
            return Empty;
        }
    }
}

