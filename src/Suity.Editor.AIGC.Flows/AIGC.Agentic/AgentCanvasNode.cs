using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using System.Linq;
using System.Threading.Tasks;

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
        var input = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgentNode>(), description: "Out");
        var output = FixedNodeConnector.CreateControlOutput("Out", TypeDefinition.FromNative<IAgentNode>(), description: "In");

        _in = AddConnector(input);
        _out = AddConnector(output);
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

    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, this);
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
                TaskItemGui(gui, i, item);
            }
        });

        return node;
    }

    private static void TaskItemGui(ImGui gui, int i, AgentTaskItem item)
    {
        gui.Frame("task-" + i)
        .InitClass("taskFrame")
        .InitPadding(10)
        .InitFullWidth()
        .InitFitVertical()
        .OnContent(() =>
        {
            if (item.LoopAsset is not { } asset)
            {
                gui.Text("title-missing", item.ToString()?.ToShortcutBeginEnd())
                .InitFullWidth()
                .InitClass("textBoldRed");

                return;
            }

            gui.HorizontalLayout("task-view")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() => 
            {
                var status = item.GetCommitStatus();

                gui.Text(item.ToString()?.ToShortcutBeginEnd())
                .InitWidthRest(48)
                .InitClass("textBold");

                gui.Image("btnChecked", status.ToCheckedStatus().ToStatusIcon())
                .InitClass("icon");

                gui.Button("btnNavigate", ImGuiIcons.Open)
                .InitClass("configBtn")
                .OnClick(() =>
                {
                    EditorUtility.LocateInProject(asset);
                });
            });

            if (item.GetLastTask() is { } lastTask)
            {
                gui.Text("sub-title", lastTask.DisplayText)
                .InitClass("textSub");
            }
        });
    }

    #endregion

    #region IAgentNode

    public IPageAsset PageAsset => this.Target;

    public IAgentTask AddTask(IAigcLoopAsset loopAsset, string description)
    {
        var item = new AgentTaskItem(loopAsset, description);
        _tasks.List.Add(item);

        return item;
    }

    public async Task<AICallResult> Run(AIRequest request)
    {
        var tasks = _tasks.List.SkipNull().ToArray();
        if (tasks.Length == 0)
        {
            return AICallResult.Empty;
        }

        var result = AICallResult.Empty;

        var resume = new AIRequest(request, "/resume");
        foreach (var item in tasks)
        {
            var taskDoc = item.LoopAsset?.GetTaskHost() as AigcLoopDocument;
            if (taskDoc is null)
            {
                continue;
            }

            var runner = new AigcLoopRunner(taskDoc);
            await runner.HandleRequest(resume);
        }

        return result;
    }

    #endregion
}

public class AgentTaskItem : IAgentTask, IViewObject
{
    readonly AssetProperty<IAigcLoopAsset> _loop = new("Loop", "Loop");
    readonly StringProperty _description = new("Description");

    public AgentTaskItem()
    {
    }

    public AgentTaskItem(IAigcLoopAsset loopAsset, string description)
    {
        _loop.Target = loopAsset;
        _description.Text = description;
    }

    public string Description => _description.Text;

    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _loop.Sync(sync);
        _description.Sync(sync);
    }

    public void SetupView(IViewObjectSetup setup)
    {
        _loop.InspectorField(setup);
        _description.InspectorField(setup);
    }

    public TaskCommitStatus GetCommitStatus()
    {
        if (_loop.Target?.GetTaskHost() is { } doc)
        {
            return doc.GetCommitStatus();
        }

        return TaskCommitStatus.None;
    }

    public IAigcTaskPage GetLastTask()
    {
        if (_loop.Target?.GetTaskHost() is { } doc)
        {
            return doc.GetLastTask();
        }

        return null;
    }

    #region IAgentTask

    public IAigcLoopAsset LoopAsset => _loop.Target;

    #endregion

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(_description.Text))
        {
            return _description.Text;
        }

        return _loop.Target?.ToDisplayTextL() ?? string.Empty;
    }
}