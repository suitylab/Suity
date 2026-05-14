using Suity.Drawing;
using Suity.Editor.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.SubFlows;

#region IToolAsset

[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Tool Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Tool")]
public interface IToolAsset : IPageAsset
{
    Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation);
}


#endregion

#region IToolInstance

public interface IToolInstance : IPageInstance
{
}

#endregion

#region ToolAsset

public abstract class ToolAsset : StandaloneAsset<IToolAsset>, IToolAsset
{
    protected ToolAsset()
    {
    }

    public override ImageDef DefaultIcon => CoreIconCache.Tool;

    public override ImageDef GetIcon() => CoreIconCache.Tool;

    public abstract IPageInstance CreatePageInstance(PageCreateOption option);
    public abstract Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation);
}

#endregion