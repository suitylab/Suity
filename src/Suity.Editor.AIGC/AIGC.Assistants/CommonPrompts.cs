namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides access to commonly used prompt templates.
/// </summary>
public static class CommonPrompts
{
    /// <summary>
    /// Gets the prompt template for data model formatting.
    /// </summary>
    public static string PromptDataModelFormat => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.Model.Format");

    /// <summary>
    /// Gets the prompt template for game action formatting.
    /// </summary>
    public static string PromptGameActionFormat => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.GameAction.Format");

    /// <summary>
    /// Gets the prompt template with field type tips.
    /// </summary>
    public static string PromptFieldTypeTips => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.Model.FieldType");

    /// <summary>
    /// Gets the minimal thinking prompt template.
    /// </summary>
    public static string PromptThink => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.MinimalThink");

    /// <summary>
    /// Gets the prompt template for full section updates.
    /// </summary>
    public static string PromptUpdateSectionFull => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.UpdateSection.Full");

    /// <summary>
    /// Gets the prompt template for partial section updates.
    /// </summary>
    public static string PromptUpdateSectionPartial => AIAssistantService.Instance.GetPromptTemplateOrThrow("Common.UpdateSection.Partial");
}