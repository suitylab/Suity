using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Flows.TaskPages;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im.Flows;
using System.IO;

namespace Suity.Editor.AIGC.Agentic;

[DisplayText("Agent Start", "*CoreIcon|Agent")]
[SimpleFlowNodeStyle(Color = FlowColors.AgentBg)]
[NativeAlias("Suity.Editor.AIGC.AgentStartCanvasNode")]
public class AgentStartCanvasNode : CanvasDesignNode
{
    internal FlowNodeConnector _in;

    private IAgent _agentNode;

    readonly StringProperty _entryTaskName = new(nameof(EntryTaskName), "Entry task name", "Entry");
    readonly AssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");

    private readonly ValueProperty<bool> _isTemplate
        = new("IsTemplate", "Is Template", false, "When enabled, this node will be used as a template for creating new agent.");

    public AgentStartCanvasNode()
    {
        var output = FixedNodeConnector.CreateControlInput("In", TypeDefinition.FromNative<IAgent>(), false, description: "Out");
        _in = AddConnector(output);
    }

    public IAgent AgentNode => _agentNode;

    public string EntryTaskName => _entryTaskName.Text;

    public bool IsTemplate => _isTemplate.Value;

    /// <summary>
    /// Gets or sets the workspace associated with this task page document.
    /// </summary>
    public WorkSpace WorkSpace
    {
        get => _workSpace.Target?.WorkSpace;
        set
        {
            var workSpace = value?.GetAsset();

            if (_workSpace.Target == workSpace)
            {
                return;
            }

            _workSpace.Target = workSpace;
            (this.Canvas as Document)?.MarkDirtyAndSaveDelayed(this);
        }
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _entryTaskName.Sync(sync);
        _workSpace.Sync(sync);
        _isTemplate.Sync(sync);

        if (sync.IsSetterOf(_isTemplate.Property.Name))
        {
            UpdateAsset();
        }
    }

    protected override void OnDiagramItemUpdated()
    {
        base.OnDiagramItemUpdated();

        UpdateAsset();
    }

    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _entryTaskName.InspectorField(setup);
        _workSpace.InspectorField(setup);
        _isTemplate.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        _agentNode?.SetParentAgent(null);
        _agentNode = compute.GetValue<IAgent>(_in);
    }

    private void UpdateAsset()
    {
        (this.DiagramItem as AgentStartDiagramItem)?.AssetBuilder.SetIsStartupPage(_isTemplate.Value);
    }

    internal void FlashingConnector()
    {
        _in.FlashingOnce();
    }
}

public class AgentStartDiagramItem : FlowDiagramItem<AgentStartCanvasNode, AgentStartAssetBuilder>
{
    protected internal override string OnGetSuggestedPrefix() => "Agent";

    protected internal override bool OnVerifyName(string name) => NamingVerifier.VerifyIdentifier(name);
}

public class AgentStartAssetBuilder : AssetBuilder<AgentStartAsset>
{
    bool _startupPage;

    public AgentStartAssetBuilder()
    {
        AddAutoUpdate(nameof(SubFlowPresetAsset.IsStartup), v => v.IsStartup = _startupPage);
    }

    public void SetIsStartupPage(bool isStartupPage)
    {
        _startupPage = isStartupPage;
        this.UpdateAuto(nameof(SubFlowPresetAsset.IsStartup));
    }
}

public class AgentStartAsset : Asset, ILLmChatProvider, IAigcStartup
{
    public AgentStartAsset()
    {
        UpdateAssetTypes(this.GetType(), typeof(ILLmChatProvider), typeof(IAigcStartup));
    }

    public override ImageDef DefaultIcon => CoreIconCache.Agent;

    #region ILLmChatProvider

    public ILLmChat CreateChat(FunctionContext context)
    {
        var node = (this.GetStorageObject(true) as AgentStartDiagramItem)?.Node;
        if (node != null)
        {
            return new AgentGraphRunner(node, context);
        }

        return null;
    }

    #endregion

    #region IAigcStartup

    public bool IsStartup { get; internal set; }

    public void HandleStartup(string prompt, string workspaceName)
    {
        workspaceName = workspaceName?.Trim();
        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return;
        }

        var doc = this.GetDocument();
        if (doc is null)
        {
            return;
        }

        string assetBaseDir = Project.Current.AssetDirectory;

        string finalName = KeyIncrementHelper.MakeKey(workspaceName, 2, s => 
        {
            string assetDir = assetBaseDir.PathAppend(s);
            if (Directory.Exists(assetDir))
            {
                return false;
            }

            if (WorkSpaceManager.Current.ContainsWorkSpace(s))
            {
                return false;
            }

            return true;
        });

        if (string.IsNullOrWhiteSpace(finalName))
        {
            return;
        }

        string assetDir = assetBaseDir.PathAppend(finalName);
        Directory.CreateDirectory(assetDir);

        var workSpace = WorkSpaceManager.Current.AddWorkSpace(finalName);

        var newDocEntry = DocumentManager.Instance.CloneDocument(doc.FileName.PhysicFileName, assetDir.PathAppend("AgentCanvas.sasset"));
        newDocEntry.ShowView();
    }

    #endregion
}

