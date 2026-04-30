using Suity.Collections;
using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Manages AI prompt prototypes and their associated metadata for the editor.
/// </summary>
public class AIPromptManager
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="AIPromptManager"/>.
    /// </summary>
    public static AIPromptManager Instance { get; } = new();

    private bool _init = false;
    private readonly Dictionary<string, AIPrompt> _prototypes = [];
    private readonly Dictionary<string, AIPromptInfo> _prompts = [];

    private AIPromptManager()
    {
    }

    /// <summary>
    /// Initializes the prompt manager by discovering and registering all available <see cref="AIPrompt"/> types.
    /// </summary>
    internal void Initialize()
    {
        if (_init) { return; }
        _init = true;

        foreach (var type in typeof(AIPrompt).GetAvailableClassTypes())
        {
            AIPrompt prototype = null;

            try
            {
                prototype = (AIPrompt)Activator.CreateInstance(type);
            }
            catch (Exception err)
            {
                EditorServices.SystemLog.AddLog($"Failed to create instance: " + type.Name);
            }

            if (string.IsNullOrWhiteSpace(prototype.PromptId))
            {
                EditorServices.SystemLog.AddLog($"Prompt {type.Name} has no model id.");
                continue;
            }
            if (_prompts.ContainsKey(prototype.PromptId))
            {
                EditorServices.SystemLog.AddLog($"Prompt {type.Name} has same model id with {prototype.PromptId}.");
                continue;
            }

            var prompt = prototype.ToInfo();
            prompt.EnsureTarget();

            _prototypes[prototype.PromptId] = prototype;
            _prompts[prototype.PromptId] = prompt;
        }

        EditorServices.SystemLog.AddLog("Added game making prompts: " + _prompts.Count);
    }

    /// <summary>
    /// Gets a collection of all registered prompt information objects.
    /// </summary>
    public IEnumerable<AIPromptInfo> Prompts => _prompts.Values;

    /// <summary>
    /// Gets the prototype <see cref="AIPrompt"/> for the specified prompt ID.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt prototype if found; otherwise, null.</returns>
    public AIPrompt GetPrototype(string promptId) => _prototypes.GetValueSafe(promptId);

    /// <summary>
    /// Gets the prompt template string for the specified prompt ID.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt template string if found; otherwise, null.</returns>
    public string GetPrompt(string promptId) => _prototypes.GetValueSafe(promptId)?.Prompt;

}
