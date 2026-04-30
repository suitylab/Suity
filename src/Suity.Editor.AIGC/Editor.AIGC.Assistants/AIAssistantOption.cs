using System;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Used to start a custom assistant workflow.
/// </summary>
public class AIAssistantOption
{
    /// <summary>
    /// Gets the AI assistant to use in the workflow.
    /// </summary>
    public AIAssistant Assistant { get; init; }

    /// <summary>
    /// Gets the custom option data associated with the assistant.
    /// </summary>
    public object Option { get; init; }
}