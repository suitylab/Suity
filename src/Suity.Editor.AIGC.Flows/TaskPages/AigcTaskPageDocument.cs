using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.TaskPages;

/// <summary>
/// Document that manages AIGC task pages, including task creation, configuration, and execution orchestration.
/// </summary>
[DocumentFormat(FormatName = "AigcTaskPage", Extension = "aigctask", DisplayText = "AIGC Task Page", Icon = "*CoreIcon|Task", Categoty = "AIGC", Order = 100, Iteration = LoadingIterations.Iteration2)]
[EditorFeature(EditorFeatures.AigcWorkflow)]
[NativeAlias("Suity.Editor.AIGC.PageTasks.AigcTaskPageDocument")]
public class AigcTaskPageDocument : SNamedDocument<AigcTaskPageAssetBuilder>, IAigcTaskHost
{
    readonly TextBlockProperty _initialTaskPrompt = new("InitialTaskPrompt", "Initial Task Prompt", string.Empty);
    readonly AssetProperty<IAigcToolAsset> _startupPage = new("StartupPage", "Startup Page") { Filter = StartupPageFilter.Instance };
    readonly AssetProperty<WorkSpaceAsset> _workSpace = new("WorkSpace", "WorkSpace");
    readonly AssetListProperty<IAigcToolAsset> _tools = new("Tools", "Tools List");
    readonly AssetListProperty<IArticleAsset> _knowledgeArticles
        = new("KnowledgeArticles", "Knowledge Articles", "Reading materials used as knowledge reference.") { Filter = ReadingMaterialFilter.Instance };
    readonly ValueProperty<int> _maxChatHistory = new("MaxChatHistory", "Max Chat History", 30, "Maximum number of chat history entries to keep, <=0 means unlimited");
    readonly ValueProperty<int> _maxTaskCount = new("MaxTaskCount", "Max Task Count", 0, "Maximum number of tasks supported, <=0 means unlimited");

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcTaskPageDocument"/> class.
    /// </summary>
    public AigcTaskPageDocument()
    {
        ItemCollection.FieldName = "Tasks";
        ItemCollection.FieldDescription = "Tasks";

        ItemCollection.AddItemType<AigcTaskPage>("Task Node");
    }

    #region Startup

    /// <summary>
    /// Gets or sets the selection of available startup pages.
    /// </summary>
    public AssetSelection<IAigcToolAsset> StartupPageSelection
    {
        get => _startupPage.Selection;
        internal set => _startupPage.Selection = value;
    }

    /// <summary>
    /// Gets or sets the startup page used to initiate task execution.
    /// </summary>
    public IAigcToolAsset StartupPage
    {
        get => _startupPage.Target;
        internal set => _startupPage.Target = value;
    }

    /// <summary>
    /// Gets a value indicating whether a startup page has been configured.
    /// </summary>
    public bool IsStartupConfigured => StartupPage != null;

    /// <summary>
    /// Gets the collection of tool pages available for task execution.
    /// </summary>
    public IEnumerable<IAigcToolAsset> ToolPages => _tools.Targets;

    /// <summary>
    /// Gets or sets the maximum number of chat history entries to keep. A value of 0 or less means unlimited.
    /// </summary>
    public int MaxChatHistory
    {
        get => _maxChatHistory.Value;
        set => _maxChatHistory.Value = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of tasks supported. A value of 0 or less means unlimited.
    /// </summary>
    public int MaxTaskCount
    {
        get => _maxTaskCount.Value;
        set => _maxTaskCount.Value = value;
    }

    /// <summary>
    /// Gets the list of configured tool pages, excluding null entries.
    /// </summary>
    public IAigcToolAsset[] GetToolList() => ToolPages.SkipNull().ToArray() ?? [];

    #endregion

    #region Task

    /// <summary>
    /// Gets a value indicating whether the task collection is empty.
    /// </summary>
    public bool IsTaskEmpty => ItemCollection.Count == 0;

    /// <summary>
    /// Gets the number of items in the task collection.
    /// </summary>
    public int Count => ItemCollection.Count;

    /// <summary>
    /// Gets the total number of <see cref="AigcTaskPage"/> instances in the document.
    /// </summary>
    public int GetTotalTaskCount() => ItemCollection.AllItems.OfType<AigcTaskPage>().Count();

    /// <summary>
    /// Gets the task with the specified ID.
    /// </summary>
    /// <param name="taskId">The ID of the task to retrieve.</param>
    /// <returns>The <see cref="AigcTaskPage"/> with the specified ID, or null if not found.</returns>
    public AigcTaskPage GetTask(string taskId) => ItemCollection.GetItem(taskId) as AigcTaskPage;

    /// <summary>
    /// Gets the task at the specified index in the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the task to retrieve.</param>
    /// <returns>The <see cref="AigcTaskPage"/> at the specified index, or null if the index is out of range.</returns>
    public AigcTaskPage GetTaskAt(int index)
    {
        if (index >= 0 && index < ItemCollection.Count)
        {
            return ItemCollection.GetItemAt(index) as AigcTaskPage;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all tasks in the document as an enumerable sequence.
    /// </summary>
    public IEnumerable<AigcTaskPage> Tasks => ItemCollection.Items.OfType<AigcTaskPage>();

    /// <summary>
    /// Gets the first top level task that has not been fully completed, searching from the last task backward.
    /// </summary>
    /// <returns>The first top level running <see cref="AigcTaskPage"/>, or null if all tasks are done.</returns>
    public AigcTaskPage GetUnfinishedChildTask()
    {
        int c = Count;
        if (c == 0)
        {
            return null;
        }

        AigcTaskPage working = null;

        for (int i = c - 1; i >= 0; i--)
        {
            var task = GetTaskAt(i);
            if (task is null)
            {
                continue;
            }

            var allDone = task.GetAllDoneWithSubTasks();
            if (!allDone)
            {
                working = task;
                continue;
            }
            else
            {
                break;
            }
        }

        return working;
    }

    /// <summary>
    /// Gets the last task that is currently unfinished or the most recent task if all are completed.
    /// </summary>
    /// <returns>The last unfinished <see cref="AigcTaskPage"/>, or null if no tasks exist.</returns>
    public AigcTaskPage GetUnfinishedChildTaskDeep()
    {
        if (Count == 0)
        {
            return null;
        }

        var task = GetUnfinishedChildTask();
        if (task != null)
        {
            return task.GetUnfinishedChildTaskDeep() ?? task;
        }

        // This is the last completed task.
        //task = GetTaskAt(Count - 1);
        //if (task != null && task.GetAllDone().IsFalse())
        //{
        //    return task.GetUnfinishedChildTaskDeep() ?? task;
        //}

        return null;
    }

    /// <summary>
    /// Adds a task to the document's task collection.
    /// </summary>
    /// <param name="task">The <see cref="AigcTaskPage"/> to add.</param>
    public void AddTask(AigcTaskPage task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        ItemCollection.AddItem(task);

        MarkDirtyAndSaveDelayed(this);

        View?.RefreshView();
    }

    #endregion

    #region Virtual & Override

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Task;

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Task;


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _initialTaskPrompt.Sync(sync);
        _startupPage.Sync(sync);
        _workSpace.Sync(sync);
        _tools.Sync(sync);
        _knowledgeArticles.Sync(sync);
        _maxChatHistory.Sync(sync);
        _maxTaskCount.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.LabelWithIcon("#PageSetting", "Page", CoreIconCache.Page);
        _startupPage.InspectorField(setup);
        _initialTaskPrompt.InspectorField(setup);
        _workSpace.InspectorField(setup);
        _tools.InspectorField(setup);
        _knowledgeArticles.InspectorField(setup);
        _maxChatHistory.InspectorField(setup);
        _maxTaskCount.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected internal override async Task<bool> OnGuiConfigNewItem(SNamedRootCollection items, INamedNode parentNode, NamedItem item)
    {
        if (item is AigcTaskPage page)
        {
            var selection = new AssetSelection<PageDefinitionAsset>();
            if (!await selection.ShowSelectionGUIAsync("Select Task Page"))
            {
                return false;
            }

            page.Name = AllocateTaskId();
            page.PageDefinition = selection.Target;
            return true;
        }

        return await base.OnGuiConfigNewItem(items, parentNode, item);
    }

    /// <inheritdoc/>
    protected internal override string OnGetSuggestedName(SNamedRootCollection items, string prefix, int digiLen = 2)
    {
        return AllocateTaskId();
    }

    /// <inheritdoc/>
    protected internal override string OnResolveConflictName(SNamedRootCollection items, string name)
    {
        return AllocateTaskId();
    }

    /// <inheritdoc/>
    protected internal override bool OnDropInCheck(SNamedRootCollection items, object value)
    {
        if (value is PageDefinitionAsset)
        {
            return true;
        }

        return base.OnDropInCheck(items, value);
    }

    /// <inheritdoc/>
    protected internal override object OnDropInConvert(SNamedRootCollection items, object value)
    {
        if (value is PageDefinitionAsset pageDef)
        {
            return new AigcTaskPage(pageDef)
            {
                Name = AllocateTaskId(),
            };
        }

        return base.OnDropInConvert(items, value);
    }
    #endregion

    #region IAigcTaskHost

    /// <summary>
    /// Gets or sets the initial task prompt text used when starting a new task.
    /// </summary>
    public string InitialTaskPrompt
    {
        get => _initialTaskPrompt.Text ?? string.Empty;
        set => _initialTaskPrompt.Text = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the workspace associated with this task page document.
    /// </summary>
    public WorkSpace WorkSpace
    {
        get => _workSpace.Target?.WorkSpace;
        set => _workSpace.Target = value?.GetAsset();
    }

    /// <summary>
    /// Creates a new workspace with the specified name and assigns it to this document.
    /// </summary>
    /// <param name="workSpaceName">The name of the workspace to create.</param>
    /// <returns>The newly created <see cref="WorkSpace"/>.</returns>
    public WorkSpace CreateWorkSpace(string workSpaceName)
    {
        if (!NamingVerifier.VerifyIdentifier(workSpaceName))
        {
            throw new ArgumentException("Invalid work space name.", nameof(workSpaceName));
        }

        if (WorkSpaceManager.Current.ContainsWorkSpace(workSpaceName))
        {
            workSpaceName = KeyIncrementHelper.MakeKey(workSpaceName, 2, s => !WorkSpaceManager.Current.ContainsWorkSpace(s));
        }

        var workSpace = WorkSpaceManager.Current.AddWorkSpace(workSpaceName);

        WorkSpace = workSpace;

        return workSpace;
    }


    /// <summary>
    /// Gets the collection of knowledge articles used as reference material.
    /// </summary>
    public IEnumerable<IArticleAsset> KnowledgeArticles => _knowledgeArticles.List.Select(o => o.Target).SkipNull();

    /// <summary>
    /// Adds a knowledge article to the document's knowledge collection if not already present.
    /// </summary>
    /// <param name="articleAsset">The article asset to add.</param>
    public void AddKnowledgeArticle(IArticleAsset articleAsset)
    {
        if (articleAsset is null)
        {
            return;
        }

        // Check if the article is already in the knowledge collection to avoid duplicates.
        if (_knowledgeArticles.List.Any(o => o.Target == articleAsset))
        {
            return;
        }

        // Set the article as reading material if it's not already marked as such.
        if (articleAsset.GetArticle() is { } article && !article.IsReadingMaterial)
        {
            article.IsReadingMaterial = true;
            article.Commit();
        }

        _knowledgeArticles.List.Add(new AssetSelection<IArticleAsset>(articleAsset));

        this.MarkDirtyAndSaveDelayed(this);
    }

    #endregion

    /// <summary>
    /// Generates a unique task ID that does not conflict with existing task names.
    /// </summary>
    /// <returns>A unique task ID string, or null if unable to generate one after 1000 attempts.</returns>
    public string AllocateTaskId()
    {
        for (int i = 0; i < 1000; i++)
        {
            string name = $"#Task-{IdGenerator.GenerateId(12)}";
            if (!ItemCollection.ContainsItem(name, true))
            {
                return name;
            }
        }

        return null;
    }

}


/// <summary>
/// Asset class representing an AIGC task page.
/// </summary>
public class AigcTaskPageAsset : Asset
{
    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Task;
}

/// <summary>
/// Asset builder for creating <see cref="AigcTaskPageAsset"/> instances.
/// </summary>
public class AigcTaskPageAssetBuilder : AssetBuilder<AigcTaskPageAsset>
{
}

/// <summary>
/// Filter that selects assets suitable for use as startup pages.
/// </summary>
public class StartupPageFilter : IAssetFilter
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StartupPageFilter"/>.
    /// </summary>
    public static StartupPageFilter Instance { get; } = new();

    /// <inheritdoc/>
    public bool FilterAsset(Asset asset)
    {
        if (asset is not IAigcToolAsset skillAsset)
        {
            return false;
        }

        return skillAsset.IsStartupPage;
    }
}
