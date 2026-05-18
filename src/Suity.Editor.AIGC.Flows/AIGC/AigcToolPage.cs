using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class AigcToolPage : AigcTaskPage,
    IAigcToolPage
{
    readonly AssetProperty<IToolAsset> _tool = new("Tool", "Tool");

    private IToolInstance _toolInstance;

    public AigcToolPage()
    {
    }

    public IToolAsset Tool
    {
        get => _tool.Target;
        set => _tool.Target = value;
    }

    public IToolInstance Instance
    {
        get => _toolInstance;
        set => _toolInstance = value;
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

        var context = new ToolCallContext(instance, request.Conversation, request.Cancellation);

        return await tool.RunTask(context);
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


    public static AigcToolPage CreatToolPage(AigcTaskPageDocument doc, IToolAsset toolAsset, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (toolAsset is null)
        {
            throw new ArgumentNullException(nameof(toolAsset));
        }

        var name = doc.AllocateTaskId();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = toolAsset.Description;
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            title = toolAsset.Name;
        }

        var taskPage = new AigcToolPage()
        {
            Name = name,
            Description = title ?? string.Empty,
            Tool = toolAsset,
        };

        var option = new PageCreateOption
        {
            Mode = PageElementMode.Task,
            Owner = taskPage,
        };

        var pageInstance = toolAsset.CreatePageInstance(option) as ToolInstance
            ?? throw new NullReferenceException($"Failed to create sub-flow instance for {toolAsset.Name}.");

        taskPage.Instance = pageInstance;

        //if (!string.IsNullOrWhiteSpace(taskPrompt))
        //{
        //    taskPage.SetPrompt(taskPrompt);
        //}

        if (!string.IsNullOrWhiteSpace(commitName))
        {
            taskPage.CommitName = commitName;
        }

        return taskPage;
    }

    public static AigcToolPage CreatToolPage(AigcTaskPageDocument doc, IToolInstance toolInstance, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (toolInstance is null)
        {
            throw new ArgumentNullException(nameof(toolInstance));
        }

        if (toolInstance.GetToolAsset() is not { } toolAsset)
        {
            throw new NullReferenceException("Tool instance does not have a tool asset.");
        }

        //var item = pageDefAsset.GetBaseDefinition()?.GetDocumentItem() as SubFlowDefinitionDiagramItem
        //    ?? throw new NullReferenceException("Task dose not contain PageDefinitionDiagramItem.");

        var name = doc.AllocateTaskId();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = toolAsset.Description;
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            title = toolAsset.Name;
        }

        var taskPage = new AigcToolPage()
        {
            Name = name,
            Description = title ?? string.Empty,
            Tool = toolAsset,
        };

        var option = new PageCreateOption
        {
            Mode = PageElementMode.Task,
            Owner = taskPage,
        };

        var newInstance = toolAsset.CreatePageInstance(option) as ToolInstance
            ?? throw new NullReferenceException($"Failed to create sub-flow instance for {toolAsset.Name}.");

        newInstance.UpdateFromOther(toolInstance);
        taskPage.Instance = newInstance;

        //if (!string.IsNullOrWhiteSpace(taskPrompt))
        //{
        //    taskPage.SetPrompt(taskPrompt);
        //}

        if (!string.IsNullOrWhiteSpace(commitName))
        {
            taskPage.CommitName = commitName;
        }

        return taskPage;
    }
}

