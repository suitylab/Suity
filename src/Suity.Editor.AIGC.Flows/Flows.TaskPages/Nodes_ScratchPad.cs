using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Task Scratch Pads", "*CoreIcon|Scratch")]
[NativeAlias("Suity.Editor.Flows.TaskPages.GetTaskScratchPad")]
public class GetTaskScratchPads : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes scratch pad items from parent tasks.");
    readonly ValueProperty<int> _hierarchyLimit = new("HierarchyLimit", "Hierarchy Limit", 1, "Maximum number of parent levels to include in the hierarchy.");

    public GetTaskScratchPads()
    {
        var type = TypeDefinition.FromNative<ScratchPad>().MakeArrayType();
        _scratchPad = AddDataOutputConnector("ScratchPad", type, "Scratch Pad");
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
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();

        bool inHierarchy = _inHierarchy.Value;
        int level = inHierarchy ? _hierarchyLimit.Value : 0;

        var scratchPads = page?.GetScratchPads(level) ?? [];

        compute.SetValue(_scratchPad, scratchPads);
    }
}

#endregion

#region SetTaskScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true)]
[DisplayText("Set Task Scratch Pads", "*CoreIcon|Scratch")]
public class SetTaskScratchPads : TaskPageNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;
    private readonly FlowNodeConnector _scratchPad;

    public SetTaskScratchPads()
    {
        var type = TypeDefinition.FromNative<ScratchPad>();

        _in = AddActionInputConnector("In", " ");
        _scratchPad = this.AddConnector("ScratchPad", type, FlowDirections.Input, FlowConnectorTypes.Data, true, "Scratch Pad");
        _out = AddActionOutputConnector("Out", " ");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.Context.GetArgument<IAigcWorkflowPage>()
            ?? throw new NullReferenceException("Current task not found.");

        var items = compute.GetValues<ScratchPad>(_scratchPad, true) ?? [];

        foreach (var item in items)
        {
            page.SetScratchPad(item.Type, item.Path, item.Content, item.Note);
        }

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetScratchPadText

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false)]
[DisplayText("Get Scratch Pad Message", "*CoreIcon|Task")]
public class GetScratchPadMessage : TaskPageNode
{
    readonly FlowNodeConnector _items;
    readonly FlowNodeConnector _text;

    public GetScratchPadMessage()
    {
        var padType = TypeDefinition.FromNative<ScratchPad>();
        var msgType = TypeDefinition.FromNative<LLmMessage>().MakeArrayType();

        _items = AddConnector("ScratchPad", padType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Scratch Pad");
        _text = AddDataOutputConnector("Message", msgType, "Message");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var workSpace = page?.GetWorkSpace();
        string workSpaceDir = workSpace?.MasterDirectory;

        var items = compute.GetValues<ScratchPad>(_items, true);
        var msgs = items
            .Select(i => i.ToXmlTag(workSpaceDir))
            .Select(s => new LLmMessage { Role = LLmMessageRole.User, Message = s})
            .ToArray();
        
        compute.SetValue(_text, msgs);
    }
}

#endregion