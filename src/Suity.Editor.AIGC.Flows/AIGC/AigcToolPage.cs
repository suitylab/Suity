using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class AigcToolPage : AigcTaskPage,
    IAigcToolPage
{
    readonly AssetProperty<ToolAsset> _tool = new("Tool", "Tool");

    private IToolInstance _toolInstance;

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

        sync.Sync("Page", EnsureInstance(), SyncFlag.GetOnly);
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
    protected override TextStatus OnGetTextStatus()
    {
        var done = EnsureInstance()?.GetAllDone();
        return done.ToCheckedStatus();
    }

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Tool;

    #endregion

    #region Virtual (IAigcTaskPage)

    public override IPageAsset GetPageAsset() => _tool.Target;

    public override IPageInstance GetPageInstance() => EnsureInstance();

    public override async Task<bool> RunTask(AIRequest request, TaskEventTypes eventType, string commitName, object parameter)
    {
        var tool = _tool.Target;
        if (tool is null)
        {
            return false;
        }

        var instance = EnsureInstance();
        if (instance is null)
        {
            return false;
        }

        return await tool.RunTask(instance, request.Conversation, request.Cancellation);
    }

    #endregion

    private IToolInstance EnsureInstance()
    {
        if (_toolInstance is null)
        {
            var option = new PageCreateOption 
            {
                 Owner = this,
                 Mode = PageElementMode.Page,
            };

            _toolInstance = _tool.Target?.CreatePageInstance(option) as IToolInstance;
        }

        return _toolInstance;
    }


}

