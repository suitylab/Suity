using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskScratchPad

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Scratch Pad Items", "*CoreIcon|Task")]
public class GetTaskScratchPad : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    public GetTaskScratchPad()
    {
        var articleType = TypeDefinition.FromNative<ScratchPadItem>().MakeArrayType();
        _scratchPad = AddDataOutputConnector("ScratchPad", articleType, "Scratch Pad");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var articles = page?.GetScratchPadItems() ?? [];
        var items = articles.Select(ScratchPadItem.FromArticle).ToArray();

        compute.SetValue(_scratchPad, items);
    }
}

#endregion

#region GetScratchPadText

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Scratch Pad Text", "*CoreIcon|Task")]
public class GetScratchPadText : TaskPageNode
{
    readonly FlowNodeConnector _items;
    readonly FlowNodeConnector _text;

    public GetScratchPadText()
    {
        _items = AddDataInputConnector("Items", TypeDefinition.FromNative<ScratchPadItem>().MakeArrayType());
        _text = AddDataOutputConnector("Text", "string");
    }

    public override void Compute(IFlowComputation compute)
    {
        var items = compute.GetValue(_items) as ScratchPadItem[] ?? [];
        var textAry = items.Select(i => i.ToString()).ToArray();
        var text = string.Join("\r\n\r\n", textAry);
        compute.SetValue(_text, text);
    }
}

#endregion