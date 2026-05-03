using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Types;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// An asset that stores AI prompt templates.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Prompt Asset", Icon = "*CoreIcon|Prompt")]
public class PromptAsset : TextDocumentAsset, IRenderable
{
    /// <summary>
    /// Gets the icon for this prompt asset.
    /// </summary>
    public override ImageDef GetIcon() => CoreIconCache.Prompt;

    /// <summary>
    /// Gets the default icon for this prompt asset.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.Prompt;

    /// <summary>
    /// Creates a <see cref="PromptBuilder"/> from the stored text content.
    /// </summary>
    /// <returns>A new <see cref="PromptBuilder"/> instance, or null if the document is not available.</returns>
    public PromptBuilder CreatePromptBuilder()
    {
        if (GetStorageObject() is BaseTextDocument doc)
        {
            return new PromptBuilder(doc.TextContent ?? string.Empty);
        }

        return null;
    }
}

/// <summary>
/// Builder class for creating <see cref="PromptAsset"/> instances.
/// </summary>
public class PromptAssetBuilder : AssetBuilder<PromptAsset>
{
}

/// <summary>
/// Converts a <see cref="PromptAsset"/> to its text representation.
/// </summary>
public class PromptAssetToTextConverter : TypeToTextConverter<PromptAsset>
{
    public override string Convert(PromptAsset objFrom)
    {
        return objFrom.GetText();
    }
}

/// <summary>
/// Converts a <see cref="PromptAsset"/> link to its text representation.
/// </summary>
public class PromptAssetLinkToTextConverter : AssetLinkToTextConverter<PromptAsset>
{
    public override string Convert(PromptAsset objFrom)
    {
        return objFrom.GetText();
    }
}