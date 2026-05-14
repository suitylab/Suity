using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC;

public class AigcToolPage : DesignItem,
    IAigcToolPage
{
    DImplementationSelection _tool;

    public AigcToolPage()
    {
        _tool = new(NativeTypes.ToolDefinitionType);
    }

    public DStruct Tool
    {
        get => _tool.Target;
        set => _tool.Target = value;
    }

    #region ITextDisplay (Virtual)

    /// <inheritdoc/>
    protected override TextStatus OnGetTextStatus() => TextStatus.Unchecked;

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Tool;


    #endregion

    #region IAigcToolPage

    public IAigcTaskHost TaskHost => null;

    public bool? GetAllDone() => null;

    public IPageAsset GetPageAsset() => null;

    public IPageInstance GetPageInstance() => null;

    public HistoryText GetTaskCommit() => null;

    #endregion

    #region Virtual / Override

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "#Tool-";

    /// <inheritdoc/>
    protected override bool OnCanEditText() => false;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tool = sync.Sync("ToolDef", _tool, SyncFlag.NotNull);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_tool, new ViewProperty("ToolDef", "Tool Definition"));
    }

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    #endregion

    
}

[NativeType("TestTool", CodeBase = "*Suity")]
public class TestTool : ToolDefinition
{

}