using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

#region CreateScratchPad

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Create Scratch Pad", "*CoreIcon|Scratch")]
public class CreateScratchPad : TaskPageNode
{
    readonly ConnectorStringProperty _path;
    readonly ConnectorValueProperty<ScratchPadTypes> _type;
    readonly ConnectorStringProperty _note;
    readonly ConnectorTextBlockProperty _content;
    readonly FlowNodeConnector _scratchPad;

    public CreateScratchPad()
    {
        _path = new ConnectorStringProperty("Path", "Path");
        _type = new ConnectorValueProperty<ScratchPadTypes>("Type", "Type", ScratchPadTypes.Memory, "Type of the scratch pad item");
        _note = new ConnectorStringProperty("Note", "Note", null, "Note of the scratch pad item");
        _content = new ConnectorTextBlockProperty("Content", "Content", null, "Content of the scratch pad item");

        _path.AddConnector(this);
        _type.AddConnector(this);
        _note.AddConnector(this);
        _content.AddConnector(this);

        var scratchPadType = TypeDefinition.FromNative<ScratchPad>();
        _scratchPad = this.AddDataOutputConnector("ScratchPad", scratchPadType, "Scratch Pad");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _path.Sync(sync);
        _type.Sync(sync);
        _note.Sync(sync);
        _content.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _path.InspectorField(setup, this);
        _type.InspectorField(setup, this);
        _note.InspectorField(setup, this);
        _content.InspectorField(setup, this);
    }

    public override void Compute(IFlowComputation compute)
    {
        var scratchPad = new ScratchPad
        {
            Path = _path.GetValue(compute, this),
            Type = _type.GetValue(compute, this),
            Note = _note.GetValue(compute, this),
            Content = _content.GetValue(compute, this),
        };

        compute.SetValue(_scratchPad, scratchPad);
    }
}

#endregion

#region GetTaskScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Get Task Scratch Pads", "*CoreIcon|Scratch")]
public class GetTaskScratchPads : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    readonly FlowNodeConnector _task;
    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes scratch pad items from parent tasks.");
    readonly ValueProperty<int> _hierarchyLimit = new("HierarchyLimit", "Hierarchy Limit", 1, "Maximum number of parent levels to include in the hierarchy.");

    public GetTaskScratchPads()
    {
        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();
        _task = this.AddDataInputConnector("Task", taskType, "Task");

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
        var page = compute.GetValue<IAigcWorkflowPage>(_task)
            ?? throw new NullReferenceException("Task is null.");

        bool inHierarchy = _inHierarchy.Value;
        int level = inHierarchy ? _hierarchyLimit.Value : 0;

        var scratchPads = page?.GetHistoryScratchPads(level) ?? [];

        compute.SetValue(_scratchPad, scratchPads);
    }
}

#endregion

#region SetTaskScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true, Category = "Scratch Pad")]
[DisplayText("Set Task Scratch Pads", "*CoreIcon|Scratch")]
public class SetTaskScratchPads : TaskPageNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;
    private readonly FlowNodeConnector _task;
    private readonly FlowNodeConnector _scratchPad;

    public SetTaskScratchPads()
    {
        _in = AddActionInputConnector("In", " ");

        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();
        _task = this.AddDataInputConnector("Task", taskType, "Task");

        var type = TypeDefinition.FromNative<ScratchPad>();
        _scratchPad = this.AddConnector("ScratchPad", type, FlowDirections.Input, FlowConnectorTypes.Data, true, "Scratch Pad");

        _out = AddActionOutputConnector("Out", " ");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.GetValue<IAigcWorkflowPage>(_task)
            ?? throw new NullReferenceException("Task is null.");

        var items = compute.GetValues<ScratchPad>(_scratchPad, true) ?? [];

        foreach (var item in items)
        {
            page.SetScratchPad(item.Type, item.Path, item.Content, item.Note);
        }

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetCurrentScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Icon = "*CoreIcon|Scratch", Category = "Scratch Pad")]
[DisplayText("Get Current Scratch Pads", "*CoreIcon|Scratch")]
public class GetCurrentScratchPads : TaskPageNode
{
    readonly FlowNodeConnector _scratchPad;

    readonly ValueProperty<bool> _inHierarchy = new("InHierarchy", "In Hierarchy", false, "If enabled, includes scratch pad items from parent tasks.");
    readonly ValueProperty<int> _hierarchyLimit = new("HierarchyLimit", "Hierarchy Limit", 1, "Maximum number of parent levels to include in the hierarchy.");

    public GetCurrentScratchPads()
    {
        var type = TypeDefinition.FromNative<ScratchPad>().MakeArrayType();
        _scratchPad = AddDataOutputConnector("ScratchPad", type, "Scratch Pad");
    }

    public override ImageDef Icon => CoreIconCache.Scratch;

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

        var scratchPads = page?.GetHistoryScratchPads(level) ?? [];

        compute.SetValue(_scratchPad, scratchPads);
    }
}

#endregion

#region SetCurrentScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = true, Category = "Scratch Pad")]
[DisplayText("Set Current Scratch Pads", "*CoreIcon|Scratch")]
public class SetCurrentScratchPads : TaskPageNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;
    private readonly FlowNodeConnector _scratchPad;

    public SetCurrentScratchPads()
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

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Get Scratch Pad Message", "*CoreIcon|Scratch")]
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
            .Select(i => i.ToXmlTag(ResolveChatIntents.Normal, workSpaceDir))
            .Select(s => new LLmMessage { Role = LLmMessageRole.User, Message = s})
            .ToArray();
        
        compute.SetValue(_text, msgs);
    }
}

#endregion

#region ReadFilesToScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Read Files To Scratch Pads", "*CoreIcon|Scratch")]
public class ReadFilesToScratchPads : TaskPageNode
{
    readonly FlowNodeConnector _fileNames;
    readonly FlowNodeConnector _scratchPads;

    public ReadFilesToScratchPads()
    {
        _fileNames = AddDataInputConnector("FileNames", "string[]", "File Names");
        _scratchPads = AddDataOutputConnector("ScratchPads", TypeDefinition.FromNative<ScratchPad>().MakeArrayType(), "Scratch Pads");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var workSpace = page?.GetWorkSpace();
        string workSpaceDir = workSpace?.MasterDirectory;

        var fileNames = compute.GetValues<string>(_fileNames, true)
            ?.Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim())
            .ToArray() ?? [];

        var result = fileNames.Select(fileName =>
        {
            if (!string.IsNullOrWhiteSpace(workSpaceDir))
            {
                var fullPath = workSpaceDir.PathAppend(fileName);
                if (File.Exists(fullPath))
                {
                    return new ScratchPad
                    {
                        Path = fileName,
                        Type = ScratchPadTypes.FileFullContent,
                    };
                }
            }

            return new ScratchPad
            {
                Path = fileName,
                Type = ScratchPadTypes.NotFound,
            };
        }).ToArray();

        compute.SetValue(_scratchPads, result);
    }
}

#endregion

#region FullContentScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Full Content Scratch Pads", "*CoreIcon|Scratch")]
public class FullContentScratchPads : TaskPageNode
{
    private readonly FlowNodeConnector _scratchPadIn;
    private readonly FlowNodeConnector _scratchPadOut;

    public FullContentScratchPads()
    {
        var type = TypeDefinition.FromNative<ScratchPad>();

        _scratchPadIn = this.AddConnector("In", type, FlowDirections.Input, FlowConnectorTypes.Data, true, "In");
        _scratchPadOut = AddDataOutputConnector("FullContent", type.MakeArrayType(), "Full Content");
    }

    public override void Compute(IFlowComputation compute)
    {
        var page = compute.Context.GetArgument<IAigcWorkflowPage>();
        var workSpace = page?.GetWorkSpace();
        string workSpaceDir = workSpace?.MasterDirectory;

        var items = compute.GetValues<ScratchPad>(_scratchPadIn, true) ?? [];

        var result = items.Select(item =>
        {
            if (item.Type == ScratchPadTypes.FileSegment || item.Type == ScratchPadTypes.FileEdit)
            {
                string path = item.Path?.Trim();
                if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(workSpaceDir))
                {
                    var fullPath = workSpaceDir.PathAppend(path);
                    if (File.Exists(fullPath))
                    {
                        return new ScratchPad
                        {
                            Path = path,
                            Type = ScratchPadTypes.FileFullContent,
                        };
                    }
                    else
                    {
                        return new ScratchPad
                        {
                            Path = path,
                            Type = ScratchPadTypes.NotFound,
                        };
                    }
                }
                else
                {
                    return new ScratchPad
                    {
                        Path = path,
                        Type = ScratchPadTypes.FileFullContent,
                    };
                }
            }
            else
            {
                return item;
            }
            
        }).ToArray();

        compute.SetValue(_scratchPadOut, result);
    }
}

#endregion

#region AppendScratchPads

[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Scratch Pad")]
[DisplayText("Append Scratch Pads", "*CoreIcon|Scratch")]
public class AppendScratchPads : TaskPageNode
{
    readonly FlowNodeConnector _scratchPads;
    readonly FlowNodeConnector _append;
    readonly FlowNodeConnector _combined;

    public AppendScratchPads()
    {
        var type = TypeDefinition.FromNative<ScratchPad>();
        var aryType = type.MakeArrayType();

        _scratchPads = AddDataInputConnector("ScratchPads", aryType, "Scratch Pads");
        _append = AddConnector("Append", type, FlowDirections.Input, FlowConnectorTypes.Data, true, "Append");
        _combined = AddDataOutputConnector("Combined", aryType, "Combined");
    }

    public override void Compute(IFlowComputation compute)
    {
        var existing = compute.GetValues<ScratchPad>(_scratchPads, true)?.ToList() ?? [];
        var appending = compute.GetValues<ScratchPad>(_append, true) ?? [];

        foreach (var item in appending)
        {
            string path = item.Path?.Trim();
            int index = existing.FindIndex(e => string.Equals(e.Path?.Trim(), path, StringComparison.Ordinal));
            if (index >= 0)
            {
                existing[index] = item;
            }
            else
            {
                existing.Add(item);
            }
        }

        compute.SetValue(_combined, existing.ToArray());
    }
}

#endregion