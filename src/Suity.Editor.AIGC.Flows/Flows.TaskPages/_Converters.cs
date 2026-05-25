using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;

namespace Suity.Editor.Flows.TaskPages;

/// <summary>
/// Converts an <see cref="IAigcTaskPage"/> to its associated <see cref="IPageInstance"/>.
/// </summary>
public class TaskPageToPageInstanceConverter : TypeConverter<IAigcTaskPage, IPageInstance>
{
    /// <inheritdoc/>
    public override IPageInstance Convert(IAigcTaskPage objFrom)
    {
        return objFrom.GetPageInstance();
    }
}

/// <summary>
/// Converts an <see cref="IPageInstance"/> to its owning <see cref="IAigcTaskPage"/>.
/// </summary>
public class PageInstanceToTaskPageConverter : TypeConverter<IPageInstance, IAigcTaskPage>
{
    /// <inheritdoc/>
    public override IAigcTaskPage Convert(IPageInstance objFrom)
    {
        return objFrom.Owner as IAigcTaskPage;
    }
}

/// <summary>
/// Converts a <see cref="SubFlowDefinitionAsset"/> to an <see cref="ISubFlow"/> by retrieving the diagram item's node.
/// </summary>
public class PageAssetToAigcPageConverter : TypeConverter<SubFlowDefinitionAsset, ISubFlow>
{
    /// <inheritdoc/>
    public override ISubFlow Convert(SubFlowDefinitionAsset objFrom)
    {
        return objFrom.GetDiagramItem()?.Node;
    }
}

/// <summary>
/// Converts an <see cref="ISubFlow"/> to a <see cref="SubFlowDefinitionAsset"/> by retrieving the page definition node's asset.
/// </summary>
public class AigcPageToPageAssetConverter : TypeConverter<ISubFlow, SubFlowDefinitionAsset>
{
    /// <inheritdoc/>
    public override SubFlowDefinitionAsset Convert(ISubFlow objFrom)
    {
        return (objFrom as SubflowDefinitionNode)?.GetAsset() as SubFlowDefinitionAsset;
    }
}