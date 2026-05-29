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
        var padType = TypeDefinition.FromNative<ScratchPadItem>();
        var msgType = TypeDefinition.FromNative<LLmMessage>().MakeArrayType();

        _items = AddConnector("ScratchPad", padType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Scratch Pad");
        _text = AddDataOutputConnector("Text", msgType, "Chat History");
    }

    public override void Compute(IFlowComputation compute)
    {
        var items = compute.GetValues<ScratchPadItem>(_items, true);
        var msgs = items
            .Select(i => i.ToString())
            .Select(s => new LLmMessage { Role = LLmMessageRole.User, Message = s})
            .ToArray();
        
        compute.SetValue(_text, msgs);
    }
}

#endregion