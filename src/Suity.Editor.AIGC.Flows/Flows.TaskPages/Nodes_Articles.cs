using Suity.Collections;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Editor.Flows.TaskPages;

#region GetTaskArticle

/// <summary>
/// A flow node that retrieves the article associated with a task.
/// Optionally creates a new article if one does not exist.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Category = "Article")]
[DisplayText("Get Task Article", "*CoreIcon|Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskArticle")]
public class GetTaskArticle : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly ConnectorValueProperty<bool> _autoCreate = new("AutoCreate", "Auto Create");
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskArticle"/> class.
    /// </summary>
    public GetTaskArticle()
    {
        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>();

        _task = AddDataInputConnector("Task", taskType, "Task");
        _autoCreate.AddConnector(this);
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _autoCreate.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _autoCreate.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcWorkflowPage>(_task) as AigcWorkflowPage;
        task ??= compute.Context.GetArgument<IAigcWorkflowPage>() as AigcWorkflowPage;
        if (task is null)
        {
            throw new NullReferenceException(nameof(task));
        }

        bool autoCreate = _autoCreate.GetValue(compute, this);

        var article = task.ResolveArticleBase(autoCreate);
        var articleAsset = article?.TargetAsset as IArticleAsset;

        compute.SetValue(_articles, articleAsset);
    }
}

#endregion

#region GetTaskKnowledgeArticles

/// <summary>
/// A flow node that retrieves all knowledge articles associated with a task's document.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Category = "Article")]
[DisplayText("Get Task Knowledge Articles", "*CoreIcon|Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetTaskKnowledgeArticles")]
public class GetTaskKnowledgeArticles : TaskPageNode
{
    readonly FlowNodeConnector _task;
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskKnowledgeArticles"/> class.
    /// </summary>
    public GetTaskKnowledgeArticles()
    {
        var taskType = TypeDefinition.FromNative<IAigcWorkflowPage>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();

        _task = AddDataInputConnector("Task", taskType, "Task");
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Article");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.GetValue<IAigcWorkflowPage>(_task) as AigcWorkflowPage;
        task ??= compute.Context.GetArgument<IAigcWorkflowPage>() as AigcWorkflowPage;
        if (task is null)
        {
            throw new NullReferenceException(nameof(task));
        }
        if (task.TaskPageDocument is not { } doc)
        {
            throw new NullReferenceException(nameof(task.TaskPageDocument));
        }

        var knowledge = doc.KnowledgeArticles.ToArray();

        compute.SetValue(_articles, knowledge);
    }
}

#endregion

#region ApplyKnowledgeArticle

/// <summary>
/// A flow node that adds an article to the knowledge base of the current task.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, Category = "Article")]
[DisplayText("Add Knowledge Article", "*CoreIcon|Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.ApplyArticleToKnowledgeBase")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AddKnowledgeArticle")]
public class AddKnowledgeArticle : TaskPageNode
{
    readonly FlowNodeConnector _in;
    readonly FlowNodeConnector _article;

    readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddKnowledgeArticle"/> class.
    /// </summary>
    public AddKnowledgeArticle()
    {
        _in = AddActionInputConnector("In", "Input");

        var articleType = TypeDefinition.FromNative<IArticle>();
        _article = AddDataInputConnector("Article", articleType, "Article");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcWorkflowPage>();
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }
        if (task.TaskHost is not { } host)
        {
            throw new NullReferenceException(nameof(task.TaskHost));
        }

        var article = compute.GetValue<IArticle>(_article)
            ?? throw new ArgumentNullException("Article");

        var articleAsset = article.TargetAsset as IArticleAsset
            ?? throw new NullReferenceException("Can not convert article to asset.");

        host.AddKnowledgeArticle(articleAsset);

        compute.SetResult(this, _out);
    }
}

#endregion

#region GetKnowledgeArticleList

/// <summary>
/// A flow node that converts an array of article assets into a knowledge article list.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Category = "Article")]
[DisplayText("Get Knowledge Article List", "*CoreIcon|Knowledge")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetKnowledgeArticleList")]
public class GetKnowledgeArticleList : TaskPageNode
{
    readonly FlowNodeConnector _articles;
    readonly FlowNodeConnector _knowledgeList;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetKnowledgeArticleList"/> class.
    /// </summary>
    public GetKnowledgeArticleList()
    {
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();
        _articles = AddDataInputConnector("Articles", articleAssetType, "Articles");

        var knowledgeListType = TypeDefinition.FromNative<KnowledgeArticleList>();
        _knowledgeList = AddDataOutputConnector("KnowledgeList", knowledgeListType, "Knowledge List");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var assets = compute.GetValues<IArticleAsset>(_articles, true).SkipNull().ToArray() ?? [];
        if (assets.Length == 0)
        {
            compute.SetValue(_knowledgeList, string.Empty);
            return;
        }

        var list = new KnowledgeArticleList(assets);
        compute.SetValue(_knowledgeList, list);
    }
}

#endregion

#region GetArticlesFromKnowledge

/// <summary>
/// A flow node that retrieves specific articles from a knowledge list by their IDs.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Category = "Article")]
[DisplayText("Get Articles From Knowledge", "*CoreIcon|Knowledge")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetArticlesFromKnowledge")]
public class GetArticlesFromKnowledge : TaskPageNode
{
    readonly FlowNodeConnector _knowledgeList;
    readonly FlowNodeConnector _ids;
    readonly FlowNodeConnector _articles;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticlesFromKnowledge"/> class.
    /// </summary>
    public GetArticlesFromKnowledge()
    {
        var knowledgeListType = TypeDefinition.FromNative<KnowledgeArticleList>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();

        _knowledgeList = AddDataInputConnector("KnowledgeList", knowledgeListType, "Knowledge List");
        _ids = AddDataInputConnector("Ids", "string[]", "Ids");
        _articles = AddDataOutputConnector("Articles", articleAssetType, "Articles");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var knowledge = compute.GetValue<KnowledgeArticleList>(_knowledgeList);
        string[] ids = compute.GetValues<string>(_ids, true)
            ?.Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToArray() ?? [];

        if (knowledge is null || ids.Length == 0)
        {
            compute.SetValue(_articles, Array.Empty<IArticleAsset>());
            return;
        }

        var articles = ids
            .Select(id => knowledge.GetItem(id)?.Article)
            .SkipNull()
            .ToArray();

        compute.SetValue(_articles, articles);
    }
}

#endregion

#region GetArticleTaggedContents

/// <summary>
/// A flow node that extracts tagged XML contents from articles, converting them into LooseXmlTag objects
/// with configurable tag name and title attribute settings.
/// </summary>
[SimpleFlowNodeStyle(Color = ArticleAsset.ArticleBgColorCode, HasHeader = false, Category = "Article")]
[DisplayText("Get Article Tagged Contents", "*CoreIcon|Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.GetArticleTaggedContents")]
public class GetArticleTaggedContents : TaskPageNode
{
    readonly FlowNodeConnector _articles;
    readonly ConnectorStringProperty _tagName = new("TagName", "Tag Name", "section");
    readonly ConnectorStringProperty _titleAttr = new("TitleAttribute", "Title Attribute", "title");
    readonly ConnectorValueProperty<bool> _titleInHierarchy = new("TitleInHierarchy", "Hierarchy Title", false, "If selected, the title attribute will be the full path title in the hierarchy.");

    readonly FlowNodeConnector _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticleTaggedContents"/> class.
    /// </summary>
    public GetArticleTaggedContents()
    {
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>().MakeArrayType();
        var tagType = TypeDefinition.FromNative<LooseXmlTag>().MakeArrayType();

        _articles = AddDataInputConnector("Articles", articleAssetType, "Articles");
        _tagName.AddConnector(this);
        _titleAttr.AddConnector(this);
        _titleInHierarchy.AddConnector(this);
        _tags = AddDataOutputConnector("Tags", tagType, "Tags");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _tagName.Sync(sync);
        _titleAttr.Sync(sync);
        _titleInHierarchy.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _tagName.InspectorField(setup, this);
        _titleAttr.InspectorField(setup, this);
        _titleInHierarchy.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var articles = compute.GetValues<IArticleAsset>(_articles, true).SkipNull().ToArray() ?? [];
        if (articles.Length == 0)
        {
            compute.SetValue(_tags, Array.Empty<LooseXmlTag>());
            return;
        }

        string tagName = _tagName.GetValue(compute, this)?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tagName))
        {
            tagName = "section";
        }

        string titleAttr = _titleAttr.GetValue(compute, this)?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(titleAttr))
        {
            titleAttr = "title";
        }

        bool titleInHierarchy = _titleInHierarchy.GetValue(compute, this);

        List<LooseXmlTag> tags = [];

        foreach (var assetItem in articles)
        {
            string title = assetItem.GetTitle(titleInHierarchy) ?? string.Empty;
            string content = assetItem.GetContentText() ?? string.Empty;

            var tag = new LooseXmlTag()
            {
                TagName = tagName,
                InnerText = content,
            };

            tag.SetAttribute(titleAttr, title);
            tags.Add(tag);
        }

        compute.SetValue(_tags, tags.ToArray());
    }
}


#endregion