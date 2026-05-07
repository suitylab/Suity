namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides a factory for creating AI assistant instances.
/// </summary>
public interface IAssistantProvider
{
    /// <summary>
    /// Creates a new AI assistant instance.
    /// </summary>
    /// <returns>A new <see cref="AIAssistant"/> instance.</returns>
    AIAssistant CreateAssistant();
}