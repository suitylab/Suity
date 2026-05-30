using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
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
        var articleType = TypeDefinition.FromNative<ScratchPad>().MakeArrayType();
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
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();

        bool inHierarchy = _inHierarchy.Value;
        int level = inHierarchy ? _hierarchyLimit.Value : 0;

        var scratchPads = page?.GetScratchPads(level) ?? [];

        compute.SetValue(_scratchPad, scratchPads);
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