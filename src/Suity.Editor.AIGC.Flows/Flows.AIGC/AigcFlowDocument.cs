using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Types;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// AIGC Flow document type for creating and editing AI workflow diagrams.
/// </summary>
[DocumentFormat(FormatName = "AigcFlow", Extension = "aigcflow", DisplayText = "AIGC Flow", Icon = "*CoreIcon|Workflow", Categoty = "AIGC", Order = 100, Iteration = LoadingIterations.Iteration2)]
[EditorFeature(EditorFeatures.AigcWorkflow)]
[NativeAlias("Suity.Editor.AIGC.Flows.AigcFlowDocument")]
public class AigcFlowDocument : BaseAigcFlowDocument<AigcDiagramAssetBuilder>
{
    /// <summary>
    /// Gets the icon for this document.
    /// </summary>
    public override ImageDef Icon => CoreIconCache.Workflow;

    /// <summary>
    /// Gets the default icon for this document.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.Workflow;
}

/// <summary>
/// Represents an AIGC diagram asset that can be stored in the library.
/// </summary>
[DisplayText("AIGC Flow", "*CoreIcon|AI")]
public class AigcDiagramAsset : GroupAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AigcDiagramAsset"/> class.
    /// </summary>
    public AigcDiagramAsset()
    {
    }

    /// <summary>
    /// Gets the default icon for this asset.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.Workflow;

    /// <summary>
    /// Gets a value indicating whether this asset can be exported to the library.
    /// </summary>
    public override bool CanExportToLibrary => true;
}

/// <summary>
/// Builder class for creating <see cref="AigcDiagramAsset"/> instances.
/// </summary>
public class AigcDiagramAssetBuilder : GroupAssetBuilder<AigcDiagramAsset>
{
}