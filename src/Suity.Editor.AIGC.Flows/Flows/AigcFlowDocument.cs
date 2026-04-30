using Suity.Editor.Documents;
using Suity.Editor.Services;
using System.Drawing;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// AIGC Flow document type for creating and editing AI workflow diagrams.
/// </summary>
[DocumentFormat(FormatName = "AigcFlow", Extension = "aigcflow", DisplayText = "AIGC Flow", Icon = "*CoreIcon|Workflow", Categoty = "AIGC", Order = 100, Iteration = LoadingIterations.Iteration2)]
[EditorFeature(EditorFeatures.AigcWorkflow)]
public class AigcFlowDocument : BaseAigcFlowDocument<AigcDiagramAssetBuilder>
{
    /// <summary>
    /// Gets the icon for this document.
    /// </summary>
    public override Image Icon => CoreIconCache.Workflow;

    /// <summary>
    /// Gets the default icon for this document.
    /// </summary>
    public override Image DefaultIcon => CoreIconCache.Workflow;
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
    public override Image DefaultIcon => CoreIconCache.Workflow;

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