using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskScratchPad

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Scratch Pad Items", "*CoreIcon|Task")]
public class GetTaskScratchPad : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes scratch pad items from parent tasks.");
    readonly ValueProperty<int> _hierarchyLimit = new("HierarchyLimit", "Hierarchy Limit", 1, "Maximum number of parent levels to include in the hierarchy.");

    public GetTaskScratchPad()
    {
        var articleType = TypeDefinition.FromNative<ScratchPadItem>().MakeArrayType();
        _scratchPad = AddDataOutputConnector("ScratchPad", articleType, "Scratch Pad");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _inHierarchy.Sync(sync);
        _hierarchyLimit.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _inHierarchy.InspectorField(setup);
        if (_inHierarchy)
        {
            _hierarchyLimit.InspectorField(setup);
        }
    }

    public override void Compute(IFlowComputation compute)
    {
        bool inHierarchy = _inHierarchy.Value;
        int level = inHierarchy ? _hierarchyLimit.Value : 0;

        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var articles = page?.GetScratchPadItems(level) ?? [];
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