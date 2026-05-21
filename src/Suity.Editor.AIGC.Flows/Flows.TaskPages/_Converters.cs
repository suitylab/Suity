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