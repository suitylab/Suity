using Suity.Drawing;
using System;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides an LLM chat interface backed by a specific AI assistant type.
/// </summary>
/// <typeparam name="T">The type of AI assistant to use.</typeparam>
public abstract class AssistantChatProvider<T> : StandaloneAsset<ILLmChatProvider>, IAssistantChatProvider
    where T : AIAssistant, new()
{
    /// <summary>
    /// Gets the default icon for this chat provider.
    /// </summary>
    public override ImageDef DefaultIcon => typeof(T).ToDisplayIcon() ?? CoreIconCache.Assistant;

    /// <summary>
    /// Gets the display text for this chat provider.
    /// </summary>
    public override string DisplayText => typeof(T).ToDisplayText() ?? typeof(T).Name;

    /// <summary>
    /// Gets the type of AI assistant used by this provider.
    /// </summary>
    public Type AssistantType => typeof(T);

    /// <summary>
    /// Creates a new LLM chat session using the configured assistant.
    /// </summary>
    /// <param name="context">Optional function context for the chat session.</param>
    /// <returns>A new <see cref="ILLmChat"/> instance.</returns>
    public ILLmChat CreateChat(FunctionContext context = null)
    {
        var assistant = new T();

        return AIAssistantService.Instance.CreateAssistantChat(assistant, context);
    }
}