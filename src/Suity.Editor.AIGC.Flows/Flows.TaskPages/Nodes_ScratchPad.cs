using Suity.Editor.AIGC;
using Suity.Editor.Documents;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskScratchPad

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Scratch Pad", "*CoreIcon|Task")]
public class GetTaskScratchPad : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    public GetTaskScratchPad()
    {
        var articleType = ArticleAsset.ArticleType.MakeArrayType();
        _scratchPad = AddDataOutputConnector("ScratchPad", articleType, "Scratch Pad");
    }

    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var scratchPad = page?.GetScratchPadItems() ?? [];

        compute.SetValue(_scratchPad, scratchPad);
    }
}

#endregion