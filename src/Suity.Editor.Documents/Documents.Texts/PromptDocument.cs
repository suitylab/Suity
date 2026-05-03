using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;

namespace Suity.Editor.Documents.Texts;

[DocumentFormat(FormatName = "AigcPrompt", Extension = "sprompt", DisplayText = "Prompt Template", Icon = "*CoreIcon|Prompt", Categoty = "AIGC")]
/// <summary>
/// Represents a prompt template document used for AIGC workflows.
/// </summary>
public class PromptDocument : BaseTextDocument<PromptAssetBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PromptDocument"/> class.
    /// </summary>
    public PromptDocument()
    {
    }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Prompt;

    /// <inheritdoc/>
    protected override bool NewDocument()
    {
        TextContent = string.Empty;
        return true;
    }

    /// <inheritdoc/>
    protected override void OnSaved()
    {
        base.OnSaved();
        AssetBuilder.NotifyUpdated();
    }
}
