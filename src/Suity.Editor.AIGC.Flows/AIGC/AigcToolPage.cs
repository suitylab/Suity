using Suity.Drawing;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class AigcToolPage : AigcTaskPage,
    IAigcToolPage
{
    readonly AssetProperty<ToolAsset> _tool = new("Tool", "Tool");

    public AigcToolPage()
    {
    }

    public ToolAsset Tool
    {
        get => _tool.Target;
        set => _tool.Target = value;
    }

    #region Virtual / Override

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "#Tool-";

    /// <inheritdoc/>
    protected override bool OnCanEditText() => false;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tool.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tool.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <inheritdoc/>
    protected override TextStatus OnGetTextStatus() => TextStatus.Unchecked;

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Tool;

    #endregion

    #region Virtual (IAigcTaskPage)

    public override IPageAsset GetPageAsset() => null;

    public override IPageInstance GetPageInstance() => null;

    #endregion


}

[NativeType("TestTool", CodeBase = "*Suity")]
[AssetAutoCreate]
public class TestTool : ToolAsset
{
    public TestTool()
        : base()
    {
    }

    public override IPageInstance CreatePageInstance(PageCreateOption option)
    {
        return null;
    }

    public override async Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation)
    {
        return false;
    }
}