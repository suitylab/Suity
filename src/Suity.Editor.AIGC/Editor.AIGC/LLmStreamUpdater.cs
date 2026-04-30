using Suity.Views;
using System;
using System.Text;

namespace Suity.Editor.AIGC;

/// <summary>
/// Provides an abstract base class for appending streamed text to a conversation and accumulating the full result.
/// </summary>
/// <remarks>LLmStreamAppender is intended for use with conversational AI scenarios where text is received
/// incrementally and needs to be appended to a conversation handler. Derived classes can override behavior to customize
/// how conversation updates are handled. This class is not thread-safe.</remarks>
public abstract class LLmStreamUpdater : IDisposable
{
    /// <summary>
    /// Gets or sets the conversation handler used to manage conversational interactions.
    /// </summary>
    public IConversationHandler Conversation { get; set; }

    /// <summary>
    /// Gets the complete text content as a mutable string builder.
    /// </summary>
    public StringBuilder FullText { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the full result is currently being displayed.
    /// </summary>
    public virtual bool DisplayingFullResult => false;

    /// <summary>
    /// Appends the specified text to the end of the current content.
    /// </summary>
    /// <param name="text">The text to append. Can be null or empty, in which case no changes are made.</param>
    public virtual void Append(string text)
    {
        var fullText = FullText;

        fullText.Append(text);

        if (Conversation is { } conversation)
        {
            UpdateConversation(conversation, text, fullText);
        }
    }

    protected virtual void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {

    }

    public virtual void Dispose()
    {
    }

    public override string ToString()
    {
        return FullText.ToString();
    }
}