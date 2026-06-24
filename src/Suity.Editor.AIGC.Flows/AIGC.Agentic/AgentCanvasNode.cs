using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;


[NativeAlias("Suity.Editor.AIGC.AgentCanvasNode")]
public class AgentCanvasNode : ExpandedCanvasAssetNode<SubFlowPresetAsset>, IAgent, IHasFlowComputionState
{
    internal FlowNodeConnector _out;
    internal FlowNodeConnector _in;

    readonly StringProperty _agentName = new("AgentName", "Agent Name");
    readonly TextBlockProperty _overview = new("Overview", "Overview");
    readonly SyncListProperty<AgentLoopItem> _loops = new("Loops", () => new());

    private IAgent _parent;
    private IAgent[] _subAgents = [];

    public AgentCanvasNode()
    {
        base.Filter = StartupPageFilter.Instance;

        var input = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgent>(), true, description: "Out");
        var output = FixedNodeConnector.CreateControlOutput("Out", TypeDefinition.FromNative<IAgent>(), false, description: "In");

        _in = AddConnector(input);
        _out = AddConnector(output);

        _overview.Property.WithOptional();

        _loops.SyncList.Added += SyncList_Added;
    }

    private void SyncList_Added(object sender, IndexEventArgs<AgentLoopItem, int> e)
    {
        if (!string.IsNullOrWhiteSpace(e.Value.Id))
        {
            return;
        }

        string id = null;
        while (true)
        {
            id = IdGenerator.GenerateId(10);
            if (_loops.List.All(x => x.Id != id))
            {
                break;
            }
        }

        e.Value.Id = id;
    }

    public override object GetTargetObject() => this;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _agentName.Sync(sync);
        _overview.Sync(sync);
        _loops.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _agentName.Property.WithHintText(AssetRef?.TargetAsset?.Name ?? string.Empty);
        _agentName.InspectorField(setup);

        _overview.Property.WithHintText(AssetRef?.Target?.GetPresetDocument()?.Overview ?? string.Empty);
        _overview.InspectorField(setup);

        setup.InspectorField(AssetRef, new ViewProperty(nameof(AssetRef), "Starter Workflow"));

        setup.LabelWithIcon("Execution", CoreIconCache.Play);
        _loops.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, this);

        var lastSubAgents = _subAgents ?? [];
        _subAgents = compute.GetValues<IAgent>(_in) ?? [];

        foreach (var agent in lastSubAgents)
        {
            agent.SetParentAgent(null);
        }

        foreach (var agent in _subAgents)
        {
            agent.SetParentAgent(this);
        }
    }

    public override string DisplayText => AgentName;

    protected override void OnAssetTargetUpdated()
    {
        base.OnAssetTargetUpdated();

        if (string.IsNullOrWhiteSpace(_agentName.Text))
        {
            _agentName.Text = Target?.Name ?? string.Empty;
        }
    }

    #region IDrawExpandedImGui

    public override ImGuiNode OnExpandedGui(ImGui gui)
    {
        var runner = LLmService.Instance.CurrentChat as IAgentGraphRunner;
        var state = runner?.GetAgentState(this);

        var node = gui.VerticalLayout("loop-layout")
        .OnInitialize(n =>
        {
            n.InitTheme(AgentTaskTheme.Instance);
            n.SetClass("debug_draw");
            n.InitWidth(400);
            n.InitFitVertical();
        })
        .OnContent(() =>
        {
            gui.VerticalLayout("loop-list")
            .OnInitialize(n => 
            {
                n.InitPadding(25);
                n.InitFullWidth();
                n.InitFitVertical();
                n.InitChildSpacing(10);
            })
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
        });

        return node;
    }

    private static void LoopItemGui(ImGui gui, int i, AgentLoopItem loop, IAgentLoopState loopState)
    {
        bool running = loopState?.IsRunning == true;
        var commitStatus = loop.GetCommitStatus();
        var textStatus = commitStatus.ToCheckedStatus();

        string style = "loopFrame";
        if (running)
        {
            if (commitStatus == TaskCommitStatus.Delegating)
            {
                style = "loopFrame-delegating";
            }
            else
            {
                style = "loopFrame-running";
            }
        }
        else
        {
            if (loop.LoopAsset is null)
            {
                style = "loopFrame-missing";
                textStatus = TextStatus.Error;
            }
        }

        var node = gui.Frame("loop-" + i)
        .OnInitialize(n =>
        {
            n.InitPadding(10);
            n.InitFullWidth();
            n.InitFitVertical();
            //n.SetInputFunction(ImGuiInputSystem.MouseInRender);
            n.InitInputFunctionChain(nameof(GuiButtonExtensions.Button));
        })
        .SetClass([style, "debug_draw"])
        .OnContent(() =>
        {
            gui.HorizontalLayout("loop-view")
            .SetClass("debug_draw")
            .InitFullWidth()
            .InitFitVertical()
            .OnContent(() =>
            {
                if (loop.LoopAsset is { } loopAsset)
                {
                    gui.Text("title", loop.ToString()?.ToShortcutBeginEnd())
                    .InitWidthRest(48)
                    .InitClass("textBold");

                    gui.Image("btnChecked", textStatus.ToStatusIcon())
                    .InitClass("icon");

                    gui.Button("btnNavigate", ImGuiIcons.Open)
                    .InitClass("configBtn")
                    .OnClick(() =>
                    {
                        EditorUtility.LocateInProject(loopAsset);
                    })
                    .OnDoubleClick(() =>
                    {
                        if (loopAsset is Asset asset)
                        {
                            asset.ShowDocumentView();
                        }
                    });
                }
                else
                {
                    gui.Text("title-missing", loop.ToString()?.ToShortcutBeginEnd())
                    .InitWidthRest(24)
                    .InitClass("textBoldRed");

                    gui.Image("btnChecked", TextStatus.Error.ToStatusIcon())
                    .InitClass("icon");
                }
            });

            if (loop.GetLastTask() is { } lastTask)
            {
                gui.Text("sub-title", lastTask.DisplayText)
                .InitClass("textSub");
            }
            else if (loop.LoopAsset is null)
            {
                gui.Text("sub-title", "Loop missing.")
                .InitClass("textSub");
            }
        })
        .OnDoubleClick(() => 
        {
            if (loop.LoopAsset is Asset asset)
            {
                asset.ShowDocumentView();
            }
        });
    }

    #endregion

    #region IAgentNode

    public string AgentName
    {
        get
        {
            string agentName = _agentName.Text;
            if (!string.IsNullOrWhiteSpace(agentName))
            {
                return agentName;
            }

            string assetName = AssetRef?.TargetAsset?.Name;
            if (!string.IsNullOrWhiteSpace(assetName))
            {
                return assetName;
            }

            return base.Name;
        }
    }

    public string Overview
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_overview.Text))
            {
                return _overview.Text;
            }

            if (this.Target is { } target)
            {
                return target.GetPresetDocument()?.Overview ?? string.Empty;
            }

            return string.Empty;
        }
    }

    public IAgent ParentAgent => _parent;

    public IAgent[] GetSubAgents() => _subAgents;

    public void SetParentAgent(IAgent parent) => _parent = parent;


    public ISubFlowAsset StarterWorkflow => this.Target;

    public IAgentLoop[] GetLoops()
    {
        return _loops.List.SkipNull().ToArray();
    }

    public IAgentLoop GetLoop(string id)
    {
        return _loops.List.FirstOrDefault(x => x.Id == id);
    }

    public IAgentLoop AddLoop(IAigcLoopAsset loopAsset, string description)
    {
        _out.FlashingOnce();

        string id = null;
        while (true)
        {
            id = IdGenerator.GenerateId(10);
            if (_loops.List.All(x => x.Id != id))
            {
                break;
            }
        }

        var item = new AgentLoopItem(id, description, loopAsset);
        _loops.List.Add(item);

        (this.Canvas as Document)?.MarkDirtyAndSaveDelayed(this);
        QueueRefreshView();

        return item;
    }

    public async Task<AICallResult> Run(AIRequest request, IAgentGraphRunner runner)
    {
        _out.FlashingOnce();

        var loos = _loops.List.SkipNull().ToArray();
        if (loos.Length == 0)
        {
            request.Conversation.AddWarningMessage($"No loops defined for agent '{this.ToDisplayTextL()}'");
            return AICallResult.Empty;
        }

        var result = AICallResult.Empty;

        foreach (var item in loos)
        {
            result = await runner.RunLoop(request, this, item);
            request.Cancellation.ThrowIfCancellationRequested();
        }

        request.Cancellation.ThrowIfCancellationRequested();

        return result;
    }

    public void FlashingConnector(FlowDirections direction)
    {
        if (direction == FlowDirections.Input)
        {
            _in.FlashingOnce();
        }
        else
        {
            _out.FlashingOnce();
        }
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
                if (state.IsRunning)
                {
                    if (state.GetLoopStates().FirstOrDefault() is { } loopState)
                    {
                        if (loopState.Loop?.GetCommitStatus() == TaskCommitStatus.Delegating)
                        {
                            return FlowComputationStates.Delegating;
                        }
                    }

                    return FlowComputationStates.Running;
                }
            }

            return FlowComputationStates.None;
        }
    }


    #endregion

    public override string ToString() => _agentName.Text;
}

public class AgentLoopItem : IAgentLoop, IViewObject
{
    readonly StringProperty _id = new("Id");
    readonly StringProperty _description = new("Description");
    readonly AssetProperty<IAigcLoopAsset> _loop = new("Loop", "Loop");
    

    public AgentLoopItem()
    {
    }

    public AgentLoopItem(string id, string description, IAigcLoopAsset loopAsset)
    {
        _id.Text = id;
        _description.Text = description;
        _loop.Target = loopAsset;
    }


    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _id.Sync(sync);
        _description.Sync(sync);
        _loop.Sync(sync);
    }

    public void SetupView(IViewObjectSetup setup)
    {
        _id.InspectorField(setup);
        _description.InspectorField(setup);
        _loop.InspectorField(setup);
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
            return doc.GetTaskToRun();
        }

        return null;
    }

    #region IAgentLoop

    public string Id
    {
        get => _id.Text;
        set => _id.Text = value;
    }

    public string Description => _description.Text;

    public IAigcLoopAsset LoopAsset => _loop.Target;

    #endregion

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(_description.Text))
        {
            return _description.Text;
        }

        return _id.Text ?? string.Empty;
    }
}