using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Represents an AI generation value containing a prompt and its generated result.
/// </summary>
[DisplayText("AI Generation")]
[NativeAlias("Suity.Editor.Values.SPromptValue")]
[NativeAlias("Suity.Editor.Values.SAIGenerated")]
public class SAIGeneration : SDynamic, ITextDisplay
{
    /// <summary>
    /// Gets the prompt text property for AI generation.
    /// </summary>
    public TextBlockProperty Prompt { get; } = new(nameof(Prompt), "Prompt");

    /// <summary>
    /// Initializes a new instance of the <see cref="SAIGeneration"/> class.
    /// </summary>
    public SAIGeneration()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SAIGeneration"/> class with the specified prompt.
    /// </summary>
    /// <param name="prompt">The prompt text for AI generation.</param>
    public SAIGeneration(string prompt)
    {
        Prompt.Text = prompt;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SAIGeneration"/> class with the specified prompt and value.
    /// </summary>
    /// <param name="prompt">The prompt text for AI generation.</param>
    /// <param name="value">The generated value associated with the prompt.</param>
    public SAIGeneration(string prompt, object value)
        : this(prompt)
    {
        base.Value = value;
    }

    /// <summary>
    /// Gets the icon representing this AI generation value.
    /// </summary>
    public override Image Icon => CoreIconCache.Prompt;

    /// <summary>
    /// Synchronizes the prompt property with the sync context.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Prompt.Sync(sync);
    }

    /// <summary>
    /// Sets up the view for the prompt property in the inspector.
    /// </summary>
    /// <param name="setup">The view object setup context.</param>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        Prompt.InspectorField(setup);
    }

    /// <summary>
    /// Gets the value of this generation, optionally filtered by condition.
    /// </summary>
    /// <param name="condition">Optional condition to filter the value.</param>
    /// <returns>The generated value.</returns>
    public override object GetValue(ICondition condition = null)
    {
        return base.GetValue(condition);
    }

    #region ITextDisplay

    /// <summary>
    /// Gets the display text for this AI generation, derived from the prompt.
    /// </summary>
    public string DisplayText => Prompt.Text?.ToShortcutString() ?? string.Empty;

    /// <summary>
    /// Gets the display icon for this AI generation.
    /// </summary>
    object ITextDisplay.DisplayIcon => Icon;

    /// <summary>
    /// Gets the display status indicating this is a reference text.
    /// </summary>
    public TextStatus DisplayStatus => TextStatus.Reference;

    #endregion

    /// <summary>
    /// Returns a localized string representation of this AI generation.
    /// </summary>
    /// <returns>The localized display text.</returns>
    public override string ToString() => L(DisplayText);
}