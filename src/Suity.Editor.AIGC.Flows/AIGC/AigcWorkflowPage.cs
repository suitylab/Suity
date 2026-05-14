using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents an AIGC task page that manages AI-generated content tasks within a document flow.
/// Implements design node functionality, task page interface, view double-click actions, and navigation.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.AigcTaskPage")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.AigcTaskPage")]
[NativeAlias("Suity.Editor.AIGC.AigcWorkflowPage")]
public class AigcWorkflowPage : AigcTaskPage,
    IAigcWorkflowPage, 
    IViewDoubleClickAction,
    INavigable,
    IDrawEditorImGui
{
    readonly AssetProperty<ISubFlowAsset> _workflow = new("Workflow", "Workflow");
    readonly AssetProperty<ArticleContainerAsset> _article = new("Article", "Article");
    readonly TextBlockProperty _taskPrompt = new("TaskPrompt", "Task Prompt", string.Empty);

    private SubFlowInstance _instance;
    private readonly QueueOnceAction _buildAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcWorkflowPage"/> class.
    /// </summary>
    public AigcWorkflowPage()
    {
        _workflow.TargetUpdated += _pageDef_TargetUpdated;
        _workflow.SelectionChanged += _pageDef_SelectionChanged;
        _workflow.ListenEnabled = true;

        _buildAction = new(() => BuildInstance());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcWorkflowPage"/> class with the specified page definition.
    /// </summary>
    /// <param name="pageDef">The page definition asset to use for this task page.</param>
    public AigcWorkflowPage(SubFlowDefinitionAsset pageDef)
        : this()
    {
        _workflow.Target = pageDef;
    }

    #region Core Prop

    /// <summary>
    /// Gets or sets the page definition asset that defines the structure and behavior of this task page.
    /// </summary>
    public ISubFlowAsset Workflow
    {
        get => _workflow.Target;
        set => _workflow.Target = value;
    }

    /// <summary>
    /// Gets or sets the article container asset associated with this task page.
    /// </summary>
    public ArticleContainerAsset Article
    {
        get => _article.Target;
        set => _article.Target = value;
    }

    /// <summary>
    /// Gets the diagram item representation of the page definition.
    /// </summary>
    /// <returns>The <see cref="SubFlowDefinitionDiagramItem"/> for this page, or null if not available.</returns>
    public SubFlowDefinitionDiagramItem GetDefinitionItem()
        => (Workflow?.GetBaseDefinition() as SubflowDefinitionNode)?.DiagramItem as SubFlowDefinitionDiagramItem;

    /// <summary>
    /// Gets or sets the page instance associated with this task page.
    /// The setter manages event subscription and unsubscription for the instance.
    /// </summary>
    public SubFlowInstance Instance
    {
        get => _instance;
        private set
        {
            if (ReferenceEquals(_instance, value))
            {
                return;
            }

            if (_instance != null)
            {
                _instance.TitleUpdated -= _instance_TitleUpdated;
                _instance.ResultOutput -= _instance_ResultOutput;
                _instance.RefreshRequesting -= _instance_RefreshRequesting;
                _instance.ConfigComputation -= _instance_ConfigComputation;
                _instance.DoActionRequesting -= _instance_DoActionRequesting;
                _instance.ParameterSet -= _instance_ParameterSet;
            }

            _instance = value;

            if (_instance != null)
            {
                _instance.TitleUpdated += _instance_TitleUpdated;
                _instance.ResultOutput += _instance_ResultOutput;
                _instance.RefreshRequesting += _instance_RefreshRequesting;
                _instance.ConfigComputation += _instance_ConfigComputation;
                _instance.DoActionRequesting += _instance_DoActionRequesting;
                _instance.ParameterSet += _instance_ParameterSet;
            }
        }
    }

    /// <summary>
    /// Gets or sets the task prompt text that describes the current task.
    /// </summary>
    public string TaskPrompt
    {
        get => _taskPrompt.Text ?? string.Empty;
        set => _taskPrompt.Text = value ?? string.Empty;
    }

    #endregion

    #region Virtual / Override 

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedPrefix() => "#Workflow-";

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _workflow.Sync(sync);
        _article.Sync(sync);
        _taskPrompt.Sync(sync);

        sync.Sync("Page", EnsureInstance(), SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _workflow.InspectorField(setup);
        _article.InspectorField(setup);
        _taskPrompt.InspectorField(setup);

        CheckRebuild();
    }

    /// <inheritdoc/>
    protected override void OnAdded()
    {
        base.OnAdded();

        // Purpose is to refresh IInspectorContext
        _buildAction.DoQueuedAction();
    }

    #endregion

    #region Virtual (IAigcTaskPage)

    /// <inheritdoc/>
    public override IPageAsset GetPageAsset() => _workflow.Target;

    /// <inheritdoc/>
    public override IPageInstance GetPageInstance() => EnsureInstance();

    /// <inheritdoc/>
    public override HistoryText GetTaskCommit() => EnsureInstance()?.GetTaskCommit();

    /// <inheritdoc/>
    public override bool? GetAllDone() => EnsureInstance()?.GetAllDone();

    #endregion

    #region Virtual (Task)


    /// <inheritdoc/>
    public override bool? GetIsDone() => EnsureInstance()?.GetIsDone();

    /// <inheritdoc/>
    public override bool? GetIsDoneInputs() => EnsureInstance()?.GetIsDoneInputs();

    /// <inheritdoc/>
    public override bool? GetIsDoneOutputs() => EnsureInstance()?.GetIsDoneOutputs();

    /// <inheritdoc/>
    public override bool SetParameter(string name, object value)
    {
        if (_instance?.GetElement(name) is not IPageParameter p)
        {
            return false;
        }

        if (value != null)
        {
            var valueType = TypeDefinition.FromNative(value.GetType());
            if (!TypeDefinition.IsNullOrEmpty(valueType))
            {
                var c = EditorServices.TypeConvertService.TryConvert(valueType, p.ParameterType, false, value, out var result);
                if (c == TypeConvertState.Unconvertible)
                {
                    return false;
                }

                value = result;
            }
        }

        p.SetValue(value);

        return true;
    }


    /// <summary>
    /// Run task with specific event by finding matching begin elements and executing them.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="eventType">The type of event to handle.</param>
    /// <param name="commitName">The commit name to match against event nodes.</param>
    /// <param name="parameter">The parameter to pass to the event handler.</param>
    /// <returns>True if any events were handled; otherwise, false.</returns>
    public override async Task<bool> RunTask(AIRequest request, SubFlowEventTypes eventType, string commitName, object parameter)
    {
        if (EnsureInstance() is not { } instance)
        {
            return false;
        }

        var begins = instance.GetAllChildElements(true)
            .OfType<SubFlowBeginElement>()
            .Where(o => MatchBeginElement(o, eventType, commitName))
            .ToArray();

        if (begins.Length == 0)
        {
            return false;
        }

        foreach (var begin in begins)
        {
            var parentDefPage = begin.FindParentDefPage();
            if (parentDefPage is null)
            {
                continue;
            }

            if (parentDefPage.GetIsDone().IsTrueOrEmpty())
            {
                request.Conversation.AddDisabledMessage("Skip completed event: " + eventType, msg =>
                {
                    msg.AddCode(begin.Name);
                });
                continue;
            }

            // request.Conversation.AddRunningMessage("Execute event: " + element.Name);

            begin.SetValue(parameter);
            await instance.HandleBeginTask(request, begin);
        }

        return true;
    }

    private bool MatchBeginElement(SubFlowBeginElement begin, SubFlowEventTypes eventType, string commitName)
    {
        if (eventType == SubFlowEventTypes.TaskBegin && begin.Node is SubFlowBeginNode)
        {
            // PageBeginNode can be used for TaskBegin event without commitName, for better compatibility with old version page definitions.
            return true;
        }

        // Exact match with PageEventNode and eventType, commitName.
        return begin.Node is PageEventNode node && node.MathEvent(eventType, commitName);
    }


    public override TaskCommitInfo GetTaskCommitInfo()
    {
        if (Instance?.CurrentEndElement is { } end)
        {
            return new TaskCommitInfo(end.EndType, end.Value);
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Virtual (ITextDisplay)

    /// <inheritdoc/>
    protected override TextStatus OnGetTextStatus() => EnsureInstance()?.GetAllStatus() ?? TextStatus.Normal;

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? EnsureInstance()?.Icon ?? CoreIconCache.Task;

    #endregion

    #region Vitual (Other)

    protected override void OnDoubleClick() => ShowWorkflow();

    protected override object OnGetNavigationTarget() => Instance?.DiagramItem?.TargetAsset;


    #endregion

    #region Event Handling

    private void _instance_TitleUpdated(object sender, EventArgs e)
    {
        this.Description = Instance?.Title ?? string.Empty;
        if (this.GetDocument() is { } doc)
        {
            doc.MarkDirtyAndSaveDelayed(this);
        }

        QueueRefreshView();
    }

    private void _instance_ResultOutput(object sender, EventArgs e)
    {
        if (this.GetDocument() is { } doc)
        {
            doc.MarkDirtyAndSaveDelayed(this);
        }

        QueueRefreshView();
    }

    private void _instance_RefreshRequesting(object sender, EventArgs e)
    {
        QueueRefreshView();
    }

    private void _instance_ConfigComputation(object sender, IFlowComputation compute)
    {
        compute.Context.SetArgument<IAigcTaskPage>(this);
        compute.Context.SetArgument<IAigcWorkflowPage>(this);

        var doc = this.GetDocument() as AigcTaskPageDocument;
        compute.Context.SetArgument<WorkSpace>(doc?.WorkSpace);
    }

    private void _instance_DoActionRequesting(object sender, UndoRedoAction e)
    {
        if (this.GetDocument()?.View?.GetService<UndoRedoManager>() is { } manager)
        {
            manager.Do(e);
        }
        else
        {
            e.Do();
        }
    }

    private void _instance_ParameterSet(object sender, IPageParameter e)
    {
        if (this.GetDocument() is { } doc)
        {
            doc.MarkDirtyAndSaveDelayed(this);
        }

        QueueRefreshView();
    }

    private void _pageDef_SelectionChanged(object sender, EventArgs e)
    {
        // Cannot use Queue, loading data requires immediate update
        _buildAction.DoAction();
    }

    private void _pageDef_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        _buildAction.DoQueuedAction();
    }
    #endregion

    #region IAigcWorkflowPage

    /// <summary>
    /// Gets the sub-flow instance associated with this workflow page.
    /// </summary>
    public ISubFlowInstance GetSubFlowInstance() => EnsureInstance();

    /// <summary>
    /// Gets the task prompt, optionally including prompts from the parent hierarchy.
    /// </summary>
    /// <param name="inHierarchy">If true, collects prompts from all parent tasks in the hierarchy.</param>
    /// <returns>The task prompt text, potentially combined with parent prompts.</returns>
    public string GetPrompt(bool inHierarchy)
    {
        if (inHierarchy)
        {
            LinkedList<string> prompts = [];

            AigcWorkflowPage task = this;
            while (task != null)
            {
                string s = task.GetLatestPrompt();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    prompts.AddFirst(s);
                }

                task = task.ParentNode as AigcWorkflowPage;
            }

            return string.Join("\r\n\r\n", prompts);
        }
        else
        {
            return GetLatestPrompt();
        }
    }

    /// <summary>
    /// Sets the task prompt and marks the document as dirty for saving.
    /// </summary>
    /// <param name="prompt">The prompt text to set.</param>
    public void SetPrompt(string prompt)
    {
        this.TaskPrompt = prompt;

        if (this.GetDocument() is { } doc)
        {
            doc.MarkDirtyAndSaveDelayed(this);
        }
    }

    private string GetLatestPrompt()
    {
        if (this.ParentList is not { } list)
        {
            return string.Empty;
        }

        int index = this.GetIndex();
        if (index < 0)
        {
            return string.Empty;
        }

        int count = list.Count;

        string prompt = string.Empty;

        // Start from the most recent task, get the latest prompt, return when encountering itself
        for (int i = index; i < count; i++)
        {
            if (list.GetItemAt(i) is not AigcWorkflowPage task)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(task.TaskPrompt))
            {
                prompt = task.TaskPrompt;
                break;
            }
        }

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        return string.Empty;
    }

    /// <summary>
    /// Appends a new task to the parent list using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    public bool AppendTask(IPageAsset asset, string title, string taskPrompt, string commitName)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            return false;
        }

        int maxTask = doc.MaxTaskCount;
        if (maxTask > 0 && doc.GetTotalTaskCount() > maxTask)
        {
            return false;
        }

        // If a task does not have a task prompt set, it will automatically transfer the prompt from the previous task to ensure continuity of the task chain.
        // Clear the previous task's prompt to prevent repeated use and save storage space.
        // Without transferring the prompt, there would be additional overhead of searching upward for task prompts.
/*        if (string.IsNullOrWhiteSpace(taskPrompt))
        {
            var lastTask = GetParentLastSubTask() as AigcTaskPage;
            taskPrompt = lastTask?.TaskPrompt ?? string.Empty;
            lastTask?.TaskPrompt = string.Empty;
        }*/

        var task = CreateTaskPage(asset, title, taskPrompt, commitName);
        if (task is not NamedItem item)
        {
            return false;
        }

        this.ParentList?.Add(item);

        doc.MarkDirtyAndSaveDelayed(this);

        doc.View?.RefreshView();

        return true;
    }

    /// <summary>
    /// Appends a new task to the parent list using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new task.</param>
    /// <param name="title">The title for the new task.</param>
    /// <param name="taskPrompt">The prompt for the new task.</param>
    /// <param name="commitName">The commit name for the new task.</param>
    /// <returns>True if the task was successfully appended; otherwise, false.</returns>
    public bool AppendTask(IPageInstance pageInstance, string title, string taskPrompt, string commitName)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            return false;
        }

        int maxTask = doc.MaxTaskCount;
        if (maxTask > 0 && doc.GetTotalTaskCount() > maxTask)
        {
            return false;
        }

/*        if (string.IsNullOrWhiteSpace(taskPrompt))
        {
            var lastTask = GetParentLastSubTask() as AigcTaskPage;
            taskPrompt = lastTask?.TaskPrompt ?? string.Empty;
            lastTask?.TaskPrompt = string.Empty;
        }*/

        var task = CreateTaskPage(pageInstance, title, taskPrompt, commitName);
        if (task is not NamedItem item)
        {
            return false;
        }

        this.ParentList?.Add(item);

        doc.MarkDirtyAndSaveDelayed(this);

        doc.View?.RefreshView();

        return true;
    }

    /// <summary>
    /// Adds a new sub-task using the specified tool asset.
    /// </summary>
    /// <param name="asset">The AIGC tool asset to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    public bool AddSubTask(IPageAsset asset, string title, string taskPrompt, string commitName)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            return false;
        }

        int maxTask = doc.MaxTaskCount;
        if (maxTask > 0 && doc.GetTotalTaskCount() > maxTask)
        {
            return false;
        }

        var task = CreateTaskPage(asset, title, taskPrompt, commitName);
        if (task is not NamedItem { } item)
        {
            return false;
        }

        this.AddItem(item);

        doc.MarkDirtyAndSaveDelayed(this);

        doc.View?.RefreshView();

        return true;
    }

    /// <summary>
    /// Adds a new sub-task using the specified page instance.
    /// </summary>
    /// <param name="pageInstance">The page instance to use for the new sub-task.</param>
    /// <param name="title">The title for the new sub-task.</param>
    /// <param name="taskPrompt">The prompt for the new sub-task.</param>
    /// <param name="commitName">The commit name for the new sub-task.</param>
    /// <returns>True if the sub-task was successfully added; otherwise, false.</returns>
    public bool AddSubTask(IPageInstance pageInstance, string title, string taskPrompt, string commitName)
    {
        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            return false;
        }

        int maxTask = doc.MaxTaskCount;
        if (maxTask > 0 && doc.GetTotalTaskCount() > maxTask)
        {
            return false;
        }

        var task = CreateTaskPage(pageInstance, title, taskPrompt, commitName);
        if (task is not NamedItem item)
        {
            return false;
        }

        this.AddItem(item);

        doc.MarkDirtyAndSaveDelayed(this);

        doc.View?.RefreshView();

        return true;
    }

    /// <summary>
    /// Resolves the article for this task, checking for transferable article parameters first.
    /// </summary>
    /// <param name="autoCreate">If true, automatically creates the article if it doesn't exist.</param>
    /// <returns>The resolved article, or null if not available.</returns>
    public IArticle ResolveArticle(bool autoCreate)
    {
        var pass = EnsureInstance()
            ?.GetAllChildElements(true)
            .OfType<SubFlowArticleOutput>()
            .FirstOrDefault(o => o.PassToSubTasks);

        if (pass != null)
        {
            // Return transferable article parameter
            return pass.ResolveArticle(autoCreate);
        }
        else
        {
            // Return base article
            return ResolveArticleBase(autoCreate);
        }
    }

    /// <summary>
    /// Resolves the base article for this task, optionally using the parent's article.
    /// </summary>
    /// <param name="autoCreate">If true, automatically creates the article if it doesn't exist.</param>
    /// <returns>The resolved article, or null if not available.</returns>
    public IArticle ResolveArticleBase(bool autoCreate)
    {
        if (_instance?.UseParentArticle == true && this.ParentNode is AigcWorkflowPage parent)
        {
            return parent.ResolveArticle(autoCreate);
        }

        if (_article.Target is { } articleAsset)
        {
            return articleAsset.GetArticle();
        }

        string id = this.Name;
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string filePath = this.GetDocument()?.FileName?.PhysicFileName;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        string dirPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(dirPath))
        {
            return null;
        }

        string articleDirPath = dirPath.PathAppend(fileName + "Assets");
        if (!Directory.Exists(articleDirPath))
        {
            Directory.CreateDirectory(articleDirPath);
        }

        var articleFormat = DocumentManager.Instance.GetDocumentFormat("ArticleEdit");
        if (articleFormat is null)
        {
            return null;
        }

        string articleFilePath = articleDirPath.PathAppend(id + "." + articleFormat.Extension);

        var doc = DocumentManager.Instance.OpenDocument(articleFilePath);
        if (doc?.Content is IArticleDocument articleDoc)
        {
            return articleDoc;
        }

        if (doc?.Content is not null)
        {
            return null;
        }

        if (!autoCreate)
        {
            return null;
        }

        doc = DocumentManager.Instance.NewDocument(articleFilePath, articleFormat);

        var docArticle = doc?.Content as IArticleDocument;
        if (docArticle?.TargetAsset is ArticleContainerAsset newDocAsset)
        {
            this.Article = newDocAsset;
            this.GetDocument()?.MarkDirtyAndSaveDelayed(this);
        }

        return docArticle;
    }

    /// <summary>
    /// Gets the last sub-task in this task's collection.
    /// </summary>
    /// <returns>The last sub-task, or null if no sub-tasks exist.</returns>
    public IAigcTaskPage GetLastSubTask()
    {
        if (Count > 0)
        {
            return GetItemAt(Count - 1) as IAigcTaskPage;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all sub-tasks as an array.
    /// </summary>
    /// <returns>An array of all sub-tasks.</returns>
    public IAigcTaskPage[] GetAllSubTasks()
    {
        return Items.OfType<IAigcTaskPage>()
            .SkipNull()
            .ToArray();
    }

    /// <summary>
    /// Gets the chat history for this task, optionally including parent hierarchy.
    /// </summary>
    /// <param name="inHierarchy">If true, includes chat history from parent tasks.</param>
    /// <returns>An array of LLM messages representing the chat history.</returns>
    public LLmMessage[] GetChatHistory(bool inHierarchy)
    {
        if (GetDocument() is not AigcTaskPageDocument doc)
        {
            return [];
        }

        LinkedList<LLmMessage> list = [];

        CollectChatHistory(list, inHierarchy, doc.MaxChatHistory, false);

        return [.. list];
    }

    private void CollectChatHistory(LinkedList<LLmMessage> list, bool inHierarchy, int maxHistory, bool includeSelf)
    {
        int index = this.GetIndex();
        if (index < 0 || index >= ParentList.Count)
        {
            return;
        }

        if (includeSelf)
        {
            // As a parent task, since it is currently executing, its current output information should be invalid
            var myMsgs = CreateChatHistory(true, true);
            for (int j = myMsgs.Length - 1; j >= 0; j--)
            {
                var myMsg = myMsgs[j];
                if (string.IsNullOrWhiteSpace(myMsg.Message))
                {
                    continue;
                }

                list.AddFirst(myMsgs[j]);
                if (maxHistory > 0 && list.Count > maxHistory)
                {
                    return;
                }
            }
        }

        if (maxHistory > 0 && list.Count > maxHistory)
        {
            return;
        }

        index--;
        if (index >= 0)
        {
            for (int i = index; i >= 0; i--)
            {
                if (ParentList.GetItemAt(i) is AigcWorkflowPage task)
                {
                    var msgs = task.CreateChatHistory(true, true);
                    for (int j = msgs.Length - 1; j >= 0; j--)
                    {
                        var msg = msgs[j];
                        if (string.IsNullOrWhiteSpace(msg.Message))
                        {
                            continue;
                        }

                        list.AddFirst(msgs[j]);
                        if (maxHistory > 0 && list.Count > maxHistory)
                        {
                            return;
                        }
                    }
                }
            }
        }


        if (maxHistory > 0 && list.Count > maxHistory)
        {
            return;
        }

        if (inHierarchy && ParentNode is AigcWorkflowPage parent)
        {
            parent.CollectChatHistory(list, true, maxHistory, true);
        }
    }

    private LLmMessage[] CreateChatHistory(bool input, bool output)
    {
        LLmMessage inputMsg;
        LLmMessage outputMsg;

        string prompt = this.TaskPrompt;

        var instance = EnsureInstance();
        if (instance is null)
        {
            return [];
        }

        if (input && output)
        {
            inputMsg = new()
            {
                Role = LLmMessageRole.User,
                Message = instance?.GetInputChatHistory()?.Text,
            };

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                inputMsg.Message = prompt + "\r\n\r\n" + inputMsg.Message;
            }

            outputMsg = new()
            {
                Role = LLmMessageRole.Assistant,
                Message = instance?.GetOutputChatHistory()?.Text,
            };

            return [inputMsg, outputMsg];
        }
        else if (input)
        {
            inputMsg = new()
            {
                Role = LLmMessageRole.User,
                Message = instance?.GetInputChatHistory()?.Text,
            };

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                inputMsg.Message = prompt + "\r\n\r\n" + inputMsg.Message;
            }

            return [inputMsg];
        }
        else if (output)
        {
            outputMsg = new()
            {
                Role = LLmMessageRole.Assistant,
                Message = instance?.GetOutputChatHistory()?.Text,
            };

            return [outputMsg];
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the list of available tools for this task page.
    /// </summary>
    /// <param name="includeDocumentTools">If true, includes tools from the document.</param>
    /// <returns>An array of available tool assets.</returns>
    public IPageAsset[] GetToolList(bool includeDocumentTools)
    {
        if (EnsureInstance() is not { } instance)
        {
            return [];
        }

        IEnumerable<IPageAsset> tools = instance.GetToolList();
        if (includeDocumentTools && this.GetDocument() is AigcTaskPageDocument doc)
        {
            tools = tools.Concat(doc.GetToolList());
        }

        return tools.Distinct().ToArray();
    }

    #endregion

    #region IDrawEditorImGui

    /// <inheritdoc/>
    public override bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Preview)
        {
            var selInfo = this.GetDocument()?.View?.GetService<IViewSelectionInfo>();
            if (selInfo?.SelectedObjects is { } sels && sels.CountOne())
            {
                bool selected = sels.Contains(this);
                if (selected)
                {
                    gui.VerticalLayout("#spacingW")
                    .InitWidth(20);

                    gui.Button("Workflow", CoreIconCache.Workflow)
                    .InitClass("smallBtn")
                    .InitCenter()
                    .InitToolTips("Open workflow")
                    .OnClick(ShowWorkflow);
                }
            }
        }

        return base.OnEditorGui(gui, pipeline, context);
    }

    #endregion

    

    /// <summary>
    /// Navigates to the workflow associated with this task page.
    /// </summary>
    public void ShowWorkflow()
    {
        (this.GetDocument()?.View as AigcTaskPageDocumentView)?.HandleGotoWorkflow(this);
    }

    /// <summary>
    /// Queues a view refresh on the associated document view.
    /// </summary>
    public void QueueRefreshView() => this.GetDocument()?.View?.RefreshView();

    /// <summary>
    /// Ensures the page instance is built and returns it.
    /// Builds the instance if it doesn't exist or is not in the diagram.
    /// </summary>
    /// <returns>The built page instance, or null if no definition is available.</returns>
    public SubFlowInstance EnsureInstance()
    {
        if (_instance != null && _instance.IsInDiagram)
        {
            return _instance;
        }
        else
        {
            return BuildInstance();
        }
    }

    /// <summary>
    /// Checks if the instance needs to be rebuilt and queues the build action if necessary.
    /// </summary>
    private void CheckRebuild()
    {
        if (_instance is null || !_instance.IsInDiagram)
        {
            _buildAction.DoQueuedAction();
        }
    }

    private IAigcTaskPage GetParentLastSubTask()
    {
        // Parent is maybe a task page, or root collection.
        var node = ParentNode;
        if (node is null)
        {
            return null;
        }

        if (node.Count > 0)
        {
            return  node.GetItemAt(Count - 1) as AigcWorkflowPage;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Builds the page instance from the definition item.
    /// </summary>
    /// <returns>The built page instance, or null if no definition is available.</returns>
    private SubFlowInstance BuildInstance()
    {
        if (GetDefinitionItem() is { } page)
        {
            var instance = Instance;

            if (instance != null && instance.IsInDiagram && instance.DiagramItem == page)
            {
                instance.Build();
            }
            else
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Task,
                    Owner = this,
                };

                Instance = new SubFlowInstance(page, option);
                if (instance != null)
                {
                    Instance.UpdateFromOther(instance);
                }
            }

            return instance;
        }
        else
        {
            Instance = null;
            return null;
        }
    }




    /// <summary>
    /// Creates a new task page from an <see cref="ISubFlow"/> definition.
    /// </summary>
    /// <param name="page">The page to create the task from.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="NullReferenceException">Thrown when the page is not a PageDefinitionNode or PageDefinitionAsset.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the AigcTaskPageDocument is not found.</exception>
    public AigcWorkflowPage CreateWorkflowPage(ISubFlow page, string title = null, string taskPrompt = null, string commitName = null)
    {
        var asset = (page as SubflowDefinitionNode)?.GetAsset() as SubFlowDefinitionAsset
            ?? throw new NullReferenceException("page is not a PageDefinitionNode or PageDefinitionAsset.");

        if (this.GetDocument() is not AigcTaskPageDocument doc)
        {
            throw new InvalidOperationException("AigcTaskPageDocument not found.");
        }

        return CreateWorkflowPage(doc, asset, title, taskPrompt, commitName);
    }

    /// <summary>
    /// Creates a new task page from a tool asset within the specified document.
    /// </summary>
    /// <param name="doc">The document to create the task page in.</param>
    /// <param name="subFlowAsset">The page definition asset to use.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subFlowAsset"/> is null.</exception>
    public static AigcWorkflowPage CreateWorkflowPage(AigcTaskPageDocument doc, ISubFlowAsset subFlowAsset, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (subFlowAsset is null)
        {
            throw new ArgumentNullException(nameof(subFlowAsset));
        }

        var name = doc.AllocateTaskId();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = subFlowAsset.Description;
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            title = subFlowAsset.Name;
        }

        var taskPage = new AigcWorkflowPage()
        {
            Name = name,
            Description = title ?? string.Empty,
            Workflow = subFlowAsset,
        };

        var option = new PageElementOption
        {
            Mode = PageElementMode.Task,
            Owner = taskPage,
        };

        var pageInstance = subFlowAsset.CreateSubFlowInstance(option) as SubFlowInstance
            ?? throw new NullReferenceException($"Failed to create sub-flow instance for {subFlowAsset.Name}.");

        taskPage.Instance = pageInstance;

        if (!string.IsNullOrWhiteSpace(taskPrompt))
        {
            taskPage.SetPrompt(taskPrompt);
        }

        if (!string.IsNullOrWhiteSpace(commitName))
        {
            taskPage.CommitName = commitName;
        }

        return taskPage;
    }

    /// <summary>
    /// Creates a new task page from an existing page instance within the specified document.
    /// </summary>
    /// <param name="doc">The document to create the task page in.</param>
    /// <param name="subFlowInstance">The page instance to use.</param>
    /// <param name="title">Optional title for the task page.</param>
    /// <param name="taskPrompt">Optional task prompt for the task page.</param>
    /// <param name="commitName">Optional commit name for the task page.</param>
    /// <returns>The newly created task page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subFlowInstance"/> is null.</exception>
    public static AigcWorkflowPage CreateWorkflowPage(AigcTaskPageDocument doc, ISubFlowInstance subFlowInstance, string title = null, string taskPrompt = null, string commitName = null)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (subFlowInstance is null)
        {
            throw new ArgumentNullException(nameof(subFlowInstance));
        }

        if (subFlowInstance.GetSubFlowAsset() is not { } subFlowAsset)
        {
            throw new NullReferenceException("SubFlowInstance does not have a SubFlowAsset.");
        }

        //var item = pageDefAsset.GetBaseDefinition()?.GetDocumentItem() as SubFlowDefinitionDiagramItem
        //    ?? throw new NullReferenceException("Task dose not contain PageDefinitionDiagramItem.");

        var name = doc.AllocateTaskId();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = subFlowAsset.Description;
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            title = subFlowAsset.Name;
        }

        var taskPage = new AigcWorkflowPage()
        {
            Name = name,
            Description = title ?? string.Empty,
            Workflow = subFlowAsset,
        };

        var option = new PageElementOption
        {
            Mode = PageElementMode.Task,
            Owner = taskPage,
        };

        var newInstance = subFlowAsset.CreateSubFlowInstance(option) as SubFlowInstance
            ?? throw new NullReferenceException($"Failed to create sub-flow instance for {subFlowAsset.Name}.");

        newInstance.UpdateFromOther(subFlowInstance);
        taskPage.Instance = newInstance;

        if (!string.IsNullOrWhiteSpace(taskPrompt))
        {
            taskPage.SetPrompt(taskPrompt);
        }
        
        if (!string.IsNullOrWhiteSpace(commitName))
        {
            taskPage.CommitName = commitName;
        }

        return taskPage;
    }
}
