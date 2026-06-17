using Suity.Editor.Flows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;

namespace Suity.Editor.AIGC.Agentic;


[NativeAlias("Suity.Editor.AIGC.AgentCanvasNode")]
public class AgentCanvasNode : ExpandedCanvasAssetNode<SubFlowPresetAsset>, IAgentNode
{
    internal FlowNodeConnector _out;
    internal FlowNodeConnector _in;

    readonly StringProperty _agentName = new("AgentName", "Agent Name");
    readonly SyncListProperty<AgentTaskItem> _tasks = new("Tasks", () => new());

    public AgentCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("Out", TypeDefinition.FromNative<IAgentNode>(), description: "Out");
        var input = FixedNodeConnector.CreateControlOutput("In", TypeDefinition.FromNative<IAgentNode>(), description: "In");

        _out = AddConnector(output);
        _in = AddConnector(input);
    }

    public override object GetTargetObject() => this;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _agentName.Sync(sync);
        _tasks.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(AssetRef, new ViewProperty(nameof(AssetRef), "Asset"));
        _agentName.InspectorField(setup);
        _tasks.InspectorField(setup);
    }

    public override string DisplayText => !string.IsNullOrWhiteSpace(_agentName.Text) ? _agentName.Text : base.DisplayText;

    #region IDrawExpandedImGui

    public override ImGuiNode OnExpandedGui(ImGui gui)
    {
        var node = gui.VerticalLayout("task-list")
        .InitTheme(AgentTaskTheme.Instance)
        .InitPadding(25)
        .InitWidth(400)
        .InitFitVertical()
        .InitChildSpacing(10)
        .OnContent(() => 
        {
            gui.Text("TASKS")
            .InitClass("textLight");

            for (int i = 0; i < _tasks.List.Count; i++)
            {
                var item = _tasks.List[i];
                gui.Frame("task-" + i)
                .InitClass("taskFrame")
                .InitPadding(10)
                .InitFullWidth()
                .InitFitVertical()
                .OnContent(() =>
                {
                    gui.Text(item.ToString()?.ToShortcutBeginEnd())
                    .InitClass("textBold");
                });
            }
        });

        return node;
    }

    #endregion

    #region IDataTransport

    void IDataTransport.SendData(object data, int channel)
    {
        
    } 

    #endregion

}

public class AgentTaskItem : IViewObject
{
    readonly AssetProperty<AigcTaskPageAsset> _taskPage = new("TaskPage", "Task Page");
    readonly StringProperty _description = new("Description");

    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _taskPage.Sync(sync);
        _description.Sync(sync);
    }

    public void SetupView(IViewObjectSetup setup)
    {
        _taskPage.InspectorField(setup);
        _description.InspectorField(setup);
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(_description.Text))
        {
            return _description.Text;
        }

        return _taskPage.Target?.ToDisplayTextL() ?? string.Empty;
    }
}