using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using System.IO;
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

    #region IAgentNode

    public IPageAsset PageAsset => this.Target;

    public AgentTaskItem AddTask(string name, string description, string prompt)
    {
        var currentDoc = this.Canvas as Document;
        if (currentDoc is null)
        {
            return null;
        }

        var startupPage = this.Target;
        if (startupPage is null)
        {
            return null;
        }

        var format = DocumentManager.Instance.GetDocumentFormat("AigcTaskPage");
        if (format is null)
        {
            return null;
        }

        name = name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "TaskPage";
        }

        description = description?.Trim() ?? string.Empty;

        string currentPath = Path.GetDirectoryName(currentDoc.FileName.PhysicFileName);
        currentPath = PathUtility.MakeRalativePath(currentPath, EditorServices.CurrentProject.AssetDirectory);

        var docEntry = format.AutoNewDocument(name, currentPath);
        if (docEntry is null)
        {
            return null;
        }

        var doc = docEntry.Content as AigcTaskPageDocument;
        if (doc is null)
        {
            return null;
        }

        doc.StartupPage = startupPage;
        doc.InitialTaskPrompt = prompt;
        doc.MarkDirtyAndSaveDelayed(this);

        var taskPageAsset = doc.TargetAsset as AigcTaskPageAsset;
        var item = new AgentTaskItem(taskPageAsset, description);
        _tasks.List.Add(item);

        this.GetTargetDocument()?.MarkDirtyAndSaveDelayed(this);
        this.QueueRefreshView();

        return item;
    }

    public async Task<AICallResult> Run(AIRequest request)
    {
        return AICallResult.Empty;
    }

    #endregion
}

public class AgentTaskItem : IViewObject
{
    readonly AssetProperty<AigcTaskPageAsset> _taskPage = new("TaskPage", "Task Page");
    readonly StringProperty _description = new("Description");


    public AgentTaskItem()
    {
    }

    public AgentTaskItem(AigcTaskPageAsset asset, string description)
    {
        _taskPage.Target = asset;
        _description.Text = description;
    }

    public AigcTaskPageAsset PageAsset => _taskPage.Target;

    public string Description => _description.Text;

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