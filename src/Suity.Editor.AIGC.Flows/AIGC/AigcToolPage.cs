using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Selecting;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
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
        _tool.TargetUpdated += _tool_TargetUpdated;
        _tool.SelectionChanged += _tool_SelectionChanged;
        _tool.ListenEnabled = true;
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
    protected override void OnSetupViewTask(IViewObjectSetup setup)
    {
        base.OnSetupViewTask(setup);

        _tool.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected internal override bool OnVerifyName(string name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }


    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Tool;

    #endregion

    #region Virtual (IAigcTaskPage)

    public override IPageAsset GetPageAsset() => _tool.Target;

    public override IPageInstance GetPageInstance() => EnsureInstance();

    public override async Task<bool> RunTask(AIRequest request, TaskEventTypes eventType, string commitName, object parameter)
    {
        if (GetCommitStatus() == TaskCommitStatus.TaskDisabled)
        {
            return false;
        }

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

        var context = new ToolCallContext
        {
            ToolInstance = instance,
            WorkSpaceDirectory = this.TaskPageDocument?.WorkSpace?.MasterDirectory,
            Conversation = request.Conversation,
            Cancellation = request.Cancellation,
        };

        return await tool.Run(context);
    }

    public override LLmMessage[] GetChatMessages(bool input, bool output)
    {
        LLmMessage inputMsg = null;
        LLmMessage outputMsg = null;

        var instance = EnsureInstance();
        if (instance is null)
        {
            return [];
        }

        if (input)
        {
            var writer = new XmlNodeWriter("Input", false);

            if (instance.InputObject is { } inputObj)
            {
                Serializer.Serialize(inputObj, writer);
            }

            inputMsg = new()
            {
                Role = LLmMessageRole.Assistant,
                Message = writer.ToString(),
            };
        }

        if (output) 
        {
            var writer = new XmlNodeWriter("Output", false);

            string errorMsg = instance.GetErrorMessage();
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                writer.SetElement("Error", w => w.SetValue(errorMsg));
            }

            if (instance.OutputObject is { } outputObj)
            {
                Serializer.Serialize(outputObj, writer);
            }

            outputMsg = new()
            {
                Role = LLmMessageRole.Assistant,
                Message = writer.ToString(),
            };
        }

        if (input && output)
        {
            return [inputMsg, outputMsg];
        }
        else if (input)
        {
            return [inputMsg];
        }
        else if (output)
        {
            return [outputMsg];
        }
        else
        {
            return [];
        }
    }

    #endregion

    private IToolInstance EnsureInstance()
    {
        if (_toolInstance is null)
        {
            BuildInstance();
        }

        return _toolInstance;
    }

    private IToolInstance BuildInstance()
    {
        var option = new PageCreateOption
        {
            Owner = this,
            Mode = PageElementMode.Page,
        };

        var old = _toolInstance;
        _toolInstance = _tool.Target?.CreatePageInstance(option) as IToolInstance;
        if (_toolInstance is null)
        {
            return null;
        }

        if (_toolInstance is ToolInstance toolInstance && toolInstance.GetType() == old?.GetType())
        {
            toolInstance.UpdateFromOther(old);
        }

        this.TaskPageDocument?.View?.DoServiceAction<IViewRefresh>(o => o.QueueRefreshView());

        return _toolInstance;
    }

    private void _tool_SelectionChanged(object sender, EventArgs e)
    {
        BuildInstance();
    }

    private void _tool_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        BuildInstance();
    }

    public static AigcToolPage CreatToolPage(AigcLoopDocument doc, IToolAsset toolAsset, string title = null, string taskPrompt = null, string commitName = null)
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
            Description = title ?? toolAsset?.ToDisplayTextL() ?? string.Empty,
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

    public static AigcToolPage CreatToolPage(AigcLoopDocument doc, IToolInstance toolInstance, string title = null, string taskPrompt = null, string commitName = null)
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
            Description = title ?? toolAsset?.ToDisplayTextL() ?? string.Empty,
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

