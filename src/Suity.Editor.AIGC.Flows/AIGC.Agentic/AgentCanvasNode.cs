using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;


[NativeAlias("Suity.Editor.AIGC.AgentCanvasNode")]
public class AgentCanvasNode : ExpandedCanvasAssetNode<SubFlowPresetAsset>, IAgentNode, IHasFlowComputionState
{
    internal FlowNodeConnector _out;
    internal FlowNodeConnector _in;

    readonly StringProperty _agentName = new("AgentName", "Agent Name");
    readonly SyncListProperty<AgentLoopItem> _loops = new("Loops", () => new());

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
        _loops.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(AssetRef, new ViewProperty(nameof(AssetRef), "Asset"));
        _agentName.InspectorField(setup);
        _loops.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, this);
    }

    public override string DisplayText => !string.IsNullOrWhiteSpace(_agentName.Text) ? _agentName.Text : base.DisplayText;

    #region IDrawExpandedImGui

    public override ImGuiNode OnExpandedGui(ImGui gui)
    {
        var runner = LLmService.Instance.CurrentChat as IAgentGraphRunner;
        var state = runner?.GetAgentState(this);

        var node = gui.VerticalLayout("loop-list")
        .InitTheme(AgentTaskTheme.Instance)
        .InitPadding(25)
        .InitWidth(400)
        .InitFitVertical()
        .InitChildSpacing(10)
        .OnContent(() => 
        {
            gui.Text("LOOPS")
            .InitClass("textLight");

            for (int i = 0; i < _loops.List.Count; i++)
            {
                var loop = _loops.List[i];
                var loopState = state?.GetLoopState(loop);
                LoopItemGui(gui, i, loop, loopState);
            }
        });

        return node;
    }

    private static void LoopItemGui(ImGui gui, int i, AgentLoopItem loop, IAgentLoopState loopState)
    {
        bool running = loopState?.IsRunning == true;

        var node = gui.Frame("loop-" + i)
        .OnInitialize(n =>
        {
            n.InitClass("loopFrame");
            n.InitPadding(10);
            n.InitFullWidth();
            n.InitFitVertical();
        })
        .SetPseudo(running ? "running" : null)
        .OnContent(() =>
        {
            if (loop.LoopAsset is not { } asset)
            {
                gui.Text("title-missing", loop.ToString()?.ToShortcutBeginEnd())
                .InitFullWidth()
                .InitClass("textBoldRed");

                return;
            }

            gui.HorizontalLayout("loop-view")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() => 
            {
                var status = loop.GetCommitStatus();

                gui.Text(loop.ToString()?.ToShortcutBeginEnd())
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

            if (loop.GetLastTask() is { } lastTask)
            {
                gui.Text("sub-title", lastTask.DisplayText)
                .InitClass("textSub");
            }
        });
    }

    #endregion

    #region IAgentNode

    public IPageAsset PageAsset => this.Target;

    public IAgentLoop AddLoop(IAigcLoopAsset loopAsset, string description)
    {
        var item = new AgentLoopItem(loopAsset, description);
        _loops.List.Add(item);

        (this.Canvas as Document)?.MarkDirtyAndSaveDelayed(this);
        QueueRefreshView();

        return item;
    }

    public async Task<AICallResult> Run(AIRequest request, IAgentGraphRunner runner)
    {
        _out.FlashingOnce();

        var tasks = _loops.List.SkipNull().ToArray();
        if (tasks.Length == 0)
        {
            return AICallResult.Empty;
        }

        var result = AICallResult.Empty;

        foreach (var item in tasks)
        {
            result = await runner.RunLoop(request, this, item);
        }

        return result;
    }

    #endregion

    #region IHasFlowComputionState

    public FlowComputationStates ComputationState
    {
        get
        {
            var runner = LLmService.Instance.CurrentChat as IAgentGraphRunner;
            if (runner?.GetAgentState(this) is { } state)
            {
                return state.IsRunning ? FlowComputationStates.Running : FlowComputationStates.None;
            }

            return FlowComputationStates.None;
        }
    }


    #endregion
}

public class AgentLoopItem : IAgentLoop, IViewObject
{
    readonly AssetProperty<IAigcLoopAsset> _loop = new("Loop", "Loop");
    readonly StringProperty _description = new("Description");

    public AgentLoopItem()
    {
    }

    public AgentLoopItem(IAigcLoopAsset loopAsset, string description)
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
        if (_loop.Target?.GetLoop() is { } doc)
        {
            return doc.GetCommitStatus();
        }

        return TaskCommitStatus.None;
    }

    public IAigcTaskPage GetLastTask()
    {
        if (_loop.Target?.GetLoop() is { } doc)
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