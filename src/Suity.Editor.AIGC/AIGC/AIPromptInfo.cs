using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;

namespace Suity.Editor.AIGC;

#region AIPromptTemplateInfo

/// <summary>
/// Represents an AI prompt template with configurable properties for prompt ID, prompt text, model level, and model preset.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = nameof(AIPromptInfo), Description = "AI Prompt Template", Icon = "*CoreIcon|Template")]
public class AIPromptInfo : SObjectController, INamed, ITextDisplay
{
    private readonly string _name;
    private readonly string _title;

    /// <summary>
    /// Gets the property representing the unique identifier of the prompt.
    /// </summary>
    public StringProperty PromptId { get; } = new(nameof(PromptId), "Prompt ID");

    /// <summary>
    /// Gets the property representing the prompt text content.
    /// </summary>
    public TextBlockProperty Prompt { get; } = new(nameof(Prompt), "Prompt");

    /// <summary>
    /// Gets the property representing the AI model level setting.
    /// </summary>
    public ValueProperty<AigcModelLevel> ModelLevel { get; } = new(nameof(ModelLevel), "Model Level", AigcModelLevel.Default);

    /// <summary>
    /// Gets the property representing the LLM model preset configuration.
    /// </summary>
    public ValueProperty<LLmModelPreset> ModelPreset { get; } = new(nameof(ModelPreset), "Model Preset", LLmModelPreset.Default);

    /// <summary>
    /// Initializes a new instance of the <see cref="AIPromptInfo"/> class.
    /// </summary>
    public AIPromptInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIPromptInfo"/> class with the specified name and optional title.
    /// </summary>
    /// <param name="name">The name of the prompt template.</param>
    /// <param name="title">An optional display title for the prompt template.</param>
    public AIPromptInfo(string name, string title = null)
        : this()
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _title = title ?? string.Empty;
    }

    /// <summary>
    /// Gets the name of the prompt template, resolved from the data context or fallback to the stored name.
    /// </summary>
    public string Name => GetRootContext<IDataItem>()?.Name ?? _name ?? string.Empty;

    /// <summary>
    /// Gets the display title of the prompt template, falling back to the name if no title is set.
    /// </summary>
    public string Title => _title ?? _name;

    #region Sync

    /// <summary>
    /// Synchronizes the prompt properties with the specified sync context.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptId.Sync(sync);
        Prompt.Sync(sync);
        ModelLevel.Sync(sync);
        ModelPreset.Sync(sync);
    }

    /// <summary>
    /// Sets up the view inspector fields for the prompt properties.
    /// </summary>
    /// <param name="setup">The view object setup context.</param>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptId.InspectorField(setup);
        Prompt.InspectorField(setup);
        ModelLevel.InspectorField(setup);
        ModelPreset.InspectorField(setup);
    }

    #endregion

    #region ITextDisplay

    /// <summary>
    /// Gets the display text for this prompt, resolved from the data context or fallback to the stored name.
    /// </summary>
    string ITextDisplay.DisplayText
        => GetRootContext<IDataItem>()?.ToDisplayText() ?? _name ?? string.Empty;

    /// <summary>
    /// Gets the display icon for this prompt.
    /// </summary>
    object ITextDisplay.DisplayIcon => CoreIconCache.Template;

    /// <summary>
    /// Gets the display status for this prompt, indicating its current state.
    /// </summary>
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    /// <summary>
    /// Returns a string representation of this prompt template, using the title or name.
    /// </summary>
    /// <returns>A string representing this prompt template.</returns>
    public override string ToString() => _title ?? _name ?? base.ToString();
}

#endregion

#region AIPromptPrototype

/// <summary>
/// Represents an abstract base class for AI prompts with configurable properties.
/// </summary>
public abstract class AIPrompt
{
    /// <summary>
    /// Gets the unique identifier of the prompt.
    /// </summary>
    public abstract string PromptId { get; }

    /// <summary>
    /// Gets the description of the prompt.
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// Gets the prompt text content.
    /// </summary>
    public virtual string Prompt => string.Empty;

    /// <summary>
    /// Gets the AI model level setting for this prompt.
    /// </summary>
    public virtual AigcModelLevel ModelLevel => AigcModelLevel.Default;

    /// <summary>
    /// Gets the LLM model preset configuration for this prompt.
    /// </summary>
    public virtual LLmModelPreset ModelPreset => LLmModelPreset.Default;

    /// <summary>
    /// Converts this prompt to an <see cref="AIPromptInfo"/> instance with all properties populated.
    /// </summary>
    /// <returns>An <see cref="AIPromptInfo"/> containing the prompt data.</returns>
    public AIPromptInfo ToInfo()
    {
        string name = PromptId
            .Replace('.', '_')
            .Replace('-', '_')
            .Replace('/', '_')
            .Replace('\\', '_');

        var info = new AIPromptInfo(name, Description)
        {
            PromptId = { Text = PromptId },
            Prompt = { Text = Prompt?.Trim() ?? string.Empty },
            ModelLevel = { Value = ModelLevel },
            ModelPreset = { Value = ModelPreset },
        };

        return info;
    }
}

/// <summary>
/// Represents an abstract base class for core AI prompts.
/// </summary>
public abstract class CoreAIPrompt : AIPrompt
{
}


#endregion

#region AIPromptRecord

/// <summary>
/// Represents a record containing prompt text, model preset, and model level information.
/// </summary>
/// <param name="Prompt">The prompt text content.</param>
/// <param name="ModelPreset">The LLM model preset configuration.</param>
/// <param name="ModelLevel">The AI model level setting.</param>
public record AIPromptRecord(string Prompt, LLmModelPreset ModelPreset, AigcModelLevel ModelLevel);
#endregion