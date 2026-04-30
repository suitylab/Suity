using Suity;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Base class for article-related AIGC flow nodes.
/// </summary>
[DisplayText("Article", "*CoreIcon|Article")]
[ToolTipsText("Article related nodes")]
public abstract class AigcArticleNode : FlowNode, IFlowNodeComputeAsync
{
    /// <inheritdoc/>
    public override Image Icon => EditorUtility.ToDisplayIcon(this.GetType()) ?? CoreIconCache.Article;

    /// <inheritdoc/>
    public virtual Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        Compute(compute);

        return Task.FromResult<object>(null);
    }
}

/// <summary>
/// Flow node style for article nodes, applying article-themed colors.
/// </summary>
public class AigcArtcitlesStyle : FlowNodeBaseStyle<AigcArticleNode>
{
    private Brush _nodeFillBrush;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcArtcitlesStyle"/> class.
    /// </summary>
    public AigcArtcitlesStyle()
    {
        Color color = ArticleAsset.ArticleBgColor;
        _nodeFillBrush = new SolidBrush(color);
    }

    /// <inheritdoc/>
    public override Color? BackgroundColor => ArticleAsset.ArticleBgColor;

    /// <inheritdoc/>
    public override Brush NodeFillBrush => _nodeFillBrush;
}

#region GetArticle

/// <summary>
/// Node that retrieves an article from an asset reference.
/// </summary>
[DisplayText("Get Article")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetArticleByAssetKey", UseForSaving = true)]
public class GetArticle : AigcArticleNode
{
    private readonly ConnectorAssetProperty<ArticleAsset> _asset
        = new("Asset", "Asset");

    private readonly ConnectorValueProperty<bool> _autoCreate = new("AutoCreate", "Auto Create");

    private readonly FlowNodeConnector _articleOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticle"/> class.
    /// </summary>
    public GetArticle()
    {
        _asset.AddConnector(this);

        _articleOut = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Article");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticle"/> class with a specific asset.
    /// </summary>
    /// <param name="asset">The article asset to retrieve.</param>
    public GetArticle(ArticleAsset asset)
        : this()
    {
        _asset.TargetAsset = asset;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _asset.Sync(sync);
        _autoCreate.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _asset.InspectorField(setup, this);
        _autoCreate.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var asset = _asset.GetTarget(compute, this);

        //var asset = AssetManager.Instance.GetAsset<AigcArticleAsset>(assetKey);
        IArticle article = asset?.GetArticle(true);

        if (article is null && _autoCreate.GetValue(compute, this))
        {
            article = ArticleExtensions.SilentNewArticle();
        }

        compute.SetValue(_articleOut, article);
    }
}

#endregion

#region GetArticleByUrl

/// <summary>
/// Node that retrieves an article by its URL.
/// </summary>
[DisplayText("Get Article By Url")]
[ToolTipsText("Get article by URL starting with 'article://...', supports getting child article nodes.")]
public class GetArticleByUrl : AigcArticleNode
{
    private readonly ConnectorStringProperty _url
        = new("Url");

    private readonly ConnectorValueProperty<bool> _autoCreate
        = new("AutoCreate", "Auto Create");

    private readonly FlowNodeConnector _articleOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticleByUrl"/> class.
    /// </summary>
    public GetArticleByUrl()
    {
        _url.AddConnector(this);
        _autoCreate.AddConnector(this);

        _articleOut = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _url.Sync(sync);
        _autoCreate.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _url.InspectorField(setup, this);
        _autoCreate.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string url = _url.GetValue(compute, this);
        bool autoCreate = _autoCreate.GetValue(compute, this);

        bool created = false;

        var article = autoCreate ?
            ArticleExtensions.GetOrCreateArticleByUrl(url, true, false, out created) :
            ArticleExtensions.GetArticleByUrl(url, false, false);

        if (created)
        {
            article.Commit();
        }

        compute.SetValue(_articleOut, article);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_autoCreate.GetIsLinked(this) && _autoCreate.BaseValue)
        {
            return $"{DisplayText}(Auto Create)";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region GetArticleFromParent

/// <summary>
/// Node that retrieves a child article from a parent article by title.
/// </summary>
[FlowConnectorAlias("DocNodeOut", "ArticleOut")]
[DisplayText("Get Article From Parent")]
public class GetArticleFromParent : AigcArticleNode
{
    private readonly FlowNodeConnector _parentArticleIn;
    private readonly FlowNodeConnector _titleIn;

    private readonly FlowNodeConnector _articleOut;

    private readonly ConnectorValueProperty<bool> _autoCreate
        = new ConnectorValueProperty<bool>("AutoCreate", "Auto Create");

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticleFromParent"/> class.
    /// </summary>
    public GetArticleFromParent()
    {
        _parentArticleIn = AddDataInputConnector("ParentArticle", ArticleAsset.ArticleType, "Parent Article");
        _titleIn = AddDataInputConnector("TitleIn", "string", "Article Title");
        _autoCreate.AddConnector(this);

        _articleOut = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Article");
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
        var parent = compute.GetValue<IArticle>(_parentArticleIn);
        string title = compute.GetValueConvert<string>(_titleIn);
        bool autoCreate = _autoCreate.GetValue(compute, this);

        if (parent is null || string.IsNullOrWhiteSpace(title))
        {
            compute.SetValue(_articleOut, null);
            return;
        }

        IArticle article = parent.GetArticle(title);
        if (article is null && autoCreate)
        {
            article = parent.GetOrAddArticle(title);
            if (article != null)
            {
                article.Title = title;
            }
        }

        compute.SetValue(_articleOut, article);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_autoCreate.GetIsLinked(this) && _autoCreate.BaseValue)
        {
            return $"{DisplayText}(Auto Create)";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region CreateArticle

/// <summary>
/// Node that gets or creates a child article within a parent article, setting its properties.
/// </summary>
[DisplayText("Get Or Create Article")]
[ToolTipsText("Get or create a child article within the parent article.")]
[NativeAlias("Suity.Editor.AIGC.Flows.CreateArticle")]
public class GetOrCreateArticle : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private readonly FlowNodeConnector _articleIn;

    private readonly ConnectorStringProperty _id = new("Id");
    private readonly ConnectorStringProperty _title = new("Title", "Title");
    private readonly ConnectorStringProperty _overview = new("Overview", "Overview");
    private readonly ConnectorTextBlockProperty _content = new("Content", "Content");
    private readonly ConnectorTextBlockProperty _guide = new("Guide", "Writing Guide");

    private readonly FlowNodeConnector _articleOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrCreateArticle"/> class.
    /// </summary>
    public GetOrCreateArticle()
    {
        _in = AddActionInputConnector("In", "Input");
        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Parent Article");

        _id.AddConnector(this);
        _title.AddConnector(this);
        _overview.AddConnector(this);
        _guide.AddConnector(this);
        _content.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
        _articleOut = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _id.Sync(sync);
        _title.Sync(sync);
        _overview.Sync(sync);
        _content.Sync(sync);
        _guide.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _id.InspectorField(setup, this);
        _title.InspectorField(setup, this);
        _overview.InspectorField(setup, this);
        _content.InspectorField(setup, this);
        _guide.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var diagram = this.Diagram
            ?? throw new NullReferenceException($"Diagram is null.");

        var parent = compute.GetValue<IArticle>(_articleIn) 
            ?? throw new NullReferenceException($"Parent article is null.");

        string id = _id.GetValue(compute, this);
        string title = _title.GetValue(compute, this);


        if (!string.IsNullOrWhiteSpace(id))
        {
            id = id.Trim().Replace(' ', '_');
            id = Uri.EscapeUriString(id);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = id;
        }

        var article = parent.GetOrAddArticle(title);
        if (article is null)
        {
            compute.SetValue(_articleOut, null);
            return Task.FromResult<object>(_out);
        }

        article.ArticleId = id;
        article.Title = title;

        if (diagram.GetIsLinked(_overview.Connector))
        {
            article.Overview = _overview.GetValue(compute, this) ?? string.Empty;
        }
        if (diagram.GetIsLinked(_content.Connector))
        {
            article.Content = _content.GetText(compute, this) ?? string.Empty;
        }
        if (diagram.GetIsLinked(_guide.Connector))
        {
            article.Guide = _guide.GetText(compute, this) ?? string.Empty;
        }

        compute.SetValue(_articleOut, article);

        article.Commit();

        return Task.FromResult<object>(_out);
    }
}

#endregion

#region SetArticle

/// <summary>
/// Node that sets article properties such as title, content, overview, guide, and note.
/// </summary>
[DisplayText("Set Article")]
[ToolTipsText("Set article title, content, etc.")]
public class SetArticle : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private readonly FlowNodeConnector _articleIn;

    private readonly FlowNodeConnector _title;
    private readonly FlowNodeConnector _overview;
    private readonly FlowNodeConnector _guide;
    private readonly FlowNodeConnector _content;
    private readonly FlowNodeConnector _note;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetArticle"/> class.
    /// </summary>
    public SetArticle()
    {
        _in = AddActionInputConnector("In", "Input");
        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Article");

        _title = AddDataInputConnector("Title", "string", "Title");
        _overview = AddDataInputConnector("Overview", "string", "Overview");
        _content = AddDataInputConnector("Content", "string", "Content");
        _guide = AddDataInputConnector("Guide", "string", "Writing Guide");
        _note = AddDataInputConnector("Note", "string", "Note");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var diagram = Diagram;

        IArticle article = compute.GetValue<IArticle>(_articleIn);
        if (article is null)
        {
            throw new NullReferenceException($"{nameof(IArticle)} not found.");
        }

        if (diagram.GetIsLinked(_title))
        {
            article.Title = compute.GetValueConvert<string>(_title);
        }

        if (diagram.GetIsLinked(_overview))
        {
            article.Overview = compute.GetValueConvert<string>(_overview);
        }

        if (diagram.GetIsLinked(_content))
        {
            article.Content = compute.GetValueConvert<string>(_content);
        }

        if (diagram.GetIsLinked(_guide))
        {
            article.Guide = compute.GetValueConvert<string>(_guide);
        }

        if (diagram.GetIsLinked(_note))
        {
            article.Note = compute.GetValueConvert<string>(_note);
        }

        article.Commit();

        return Task.FromResult<object>(_out);
    }
}

#endregion

#region GetArticleInformation

/// <summary>
/// Node that retrieves various information from an article, including basic properties, content, and hierarchy.
/// </summary>
[DisplayText("Get Article Information")]
public class GetArticleInformation : AigcArticleNode
{
    private FlowNodeConnector _articleIn;

    private FlowNodeConnector _url;
    private FlowNodeConnector _title;
    private FlowNodeConnector _overview;
    private FlowNodeConnector _content;
    private FlowNodeConnector _guide;
    private FlowNodeConnector _note;

    private FlowNodeConnector _parentArticle;
    private FlowNodeConnector _childArticles;
    private FlowNodeConnector _hasChildArticles;
    private FlowNodeConnector _childDocOverview;
    private FlowNodeConnector _hierarchyDocOverview;
    private FlowNodeConnector _hierarchyDocContent;

    private readonly ValueProperty<bool> _basicProps = new("BasicProps", "Basic Properties") { Value = true, DefaultValue = true };
    private readonly ValueProperty<bool> _contentProps = new("ContentProps", "Content Properties") { Value = true, DefaultValue = true };
    private readonly ValueProperty<bool> _hierarchyProps = new("HierarchyProps", "Hierarchy Properties") { Value = true, DefaultValue = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArticleInformation"/> class.
    /// </summary>
    public GetArticleInformation()
    {
        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _basicProps.Sync(sync);
        _contentProps.Sync(sync);
        _hierarchyProps.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _basicProps.InspectorField(setup);
        _contentProps.InspectorField(setup);
        _hierarchyProps.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Article");

        if (_basicProps.Value)
        {
            _url = AddDataOutputConnector("Url", "string", "Url");
            _title = AddDataOutputConnector("Title", "string", "Title");
            _overview = AddDataOutputConnector("Overview", "string", "Overview");
        }

        if (_contentProps.Value)
        {
            _content = AddDataOutputConnector("Content", "string", "Content");
            _guide = AddDataOutputConnector("Guide", "string", "Writing Guide");
            _note = AddDataOutputConnector("Note", "string", "Note");
        }

        if (_hierarchyProps.Value)
        {
            _parentArticle = AddDataOutputConnector("ParentArticle", ArticleAsset.ArticleType, "Parent Article");
            _childArticles = AddDataOutputConnector("ChildArticles", ArticleAsset.ArticleType.MakeArrayType(), "Child Articles");
            _hasChildArticles = AddDataOutputConnector("HasChildArticles", "bool", "Has Child Articles");
            _childDocOverview = AddDataOutputConnector("ChildArticleOverview", "string", "Child Article Overview");
            _hierarchyDocOverview = AddDataOutputConnector("HierarchyArticleOverview", "string", "Parent Article Overview");
            _hierarchyDocContent = AddDataOutputConnector("HierarchyArticleContent", "string", "Parent Article Content");
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var article = compute.GetValue<IArticle>(_articleIn);

        var diagram = DiagramItem.Diagram;

        if (_basicProps.Value)
        {
            if (diagram.GetIsLinked(_url))
            {
                compute.SetValue(_url, article?.ArticleUrl);
            }

            if (diagram.GetIsLinked(_title))
            {
                compute.SetValue(_title, article?.Title);
            }

            if (diagram.GetIsLinked(_overview))
            {
                compute.SetValue(_overview, article?.Overview);
            }
        }

        if (_contentProps.Value)
        {
            if (diagram.GetIsLinked(_content))
            {
                compute.SetValue(_content, article?.Content);
            }

            if (diagram.GetIsLinked(_guide))
            {
                compute.SetValue(_guide, article?.Guide);
            }

            if (diagram.GetIsLinked(_note))
            {
                compute.SetValue(_note, article?.Note);
            }
        }

        if (_hierarchyProps.Value)
        {
            if (diagram.GetIsLinked(_parentArticle))
            {
                compute.SetValue(_parentArticle, article?.Parent);
            }

            if (diagram.GetIsLinked(_childArticles))
            {
                var childAricles = article?.Articles.ToArray() ?? [];
                compute.SetValue(_childArticles, childAricles);
            }

            if (diagram.GetIsLinked(_hasChildArticles))
            {
                compute.SetValue(_hasChildArticles, article?.Articles.Any() == true);
            }

            if (diagram.GetIsLinked(_childDocOverview))
            {
                string overview = article?.GetOverviewDeep() ?? string.Empty;
                compute.SetValue(_childDocOverview, overview);
            }

            if (diagram.GetIsLinked(_hierarchyDocOverview))
            {
                string overview = article?.GetOverviewHierarchy(false, false) ?? string.Empty;
                compute.SetValue(_hierarchyDocOverview, overview);
            }

            if (diagram.GetIsLinked(_hierarchyDocContent))
            {
                string overview = article?.GetOverviewHierarchy(false, true) ?? string.Empty;
                compute.SetValue(_hierarchyDocContent, overview);
            }
        }
    }
}

#endregion

#region ForeachArticles

/// <summary>
/// Enum defining filters for article enumeration operations.
/// </summary>
[NativeType("ArticleFilters", CodeBase = "*AIGC")]
public enum ArticleFilters
{
    /// <summary>
    /// No filtering, includes all articles.
    /// </summary>
    [DisplayText("All")]
    All,

    /// <summary>
    /// Filters articles with blank content.
    /// </summary>
    [DisplayText("Blank Content")]
    BlankContent,

    /// <summary>
    /// Filters articles with non-blank content.
    /// </summary>
    [DisplayText("Not Blank Content")]
    NotBlankContent,

    /// <summary>
    /// Filters articles with blank guide.
    /// </summary>
    [DisplayText("Blank Guide")]
    BlankGuide,

    /// <summary>
    /// Filters articles with non-blank guide.
    /// </summary>
    [DisplayText("Not Blank Guide")]
    NotBlankGuide,

    /// <summary>
    /// Filters articles with blank note.
    /// </summary>
    [DisplayText("Blank Note")]
    BlankNote,

    /// <summary>
    /// Filters articles with non-blank note.
    /// </summary>
    [DisplayText("Not Blank Note")]
    NotBlankNote,
}

/// <summary>
/// Node that enumerates child articles within a parent article, optionally with deep traversal and filtering.
/// </summary>
[DisplayText("Enumerate Child Articles")]
[ToolTipsText("Enumerate child articles within an article, parent article node must be provided.")]
public class ForeachArticles : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _articleIn;

    private readonly FlowNodeConnector _subNodeAction;
    private readonly FlowNodeConnector _childArticle;
    private readonly FlowNodeConnector _out;

    private readonly ConnectorValueProperty<bool> _includeSelf
        = new("IncludeSelf", "Include Self");

    private readonly ConnectorValueProperty<bool> _deep
        = new("Deep", "Deep Enumeration");

    private readonly ConnectorValueProperty<ArticleFilters> _filter
        = new("Filter", "Filter");

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeachArticles"/> class.
    /// </summary>
    public ForeachArticles()
    {
        _in = AddActionInputConnector("In", "Input");
        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Article");
        _includeSelf.AddConnector(this);
        _deep.AddConnector(this);
        _filter.AddConnector(this);

        _subNodeAction = AddActionOutputConnector("ChildArticleAction", "Child Article Action");
        _childArticle = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Child Article");
        _out = AddActionOutputConnector("Out", "Done");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _includeSelf.Sync(sync);
        _deep.Sync(sync);
        _filter.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _includeSelf.InspectorField(setup, this);
        _deep.InspectorField(setup, this);
        _filter.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var nodeContainer = compute.GetValue<IArticle>(_articleIn);
        if (nodeContainer is null)
        {
            throw new NullReferenceException("Article not found.");
        }

        bool includeSelf = _includeSelf.GetValue(compute, this);
        bool deep = _deep.GetValue(compute, this);
        ArticleFilters filter = _filter.GetValue(compute, this);

        if (includeSelf && nodeContainer is IArticle node)
        {
            compute.InvalidateOutputs(this);

            if (FilterArticle(filter, node))
            {
                compute.SetValue(_childArticle, node);
                await compute.RunAction(_subNodeAction, cancel);
            }

            cancel.ThrowIfCancellationRequested();
        }

        await PopulateAction(nodeContainer, compute, cancel, filter, deep);

        return _out;
    }

    /// <summary>
    /// Recursively iterates through child articles and executes the child article action.
    /// </summary>
    /// <param name="container">The article container to iterate.</param>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <param name="deep">Whether to recursively enumerate child articles.</param>
    private async Task PopulateAction(IArticleContainer container, IFlowComputationAsync compute, CancellationToken cancel, ArticleFilters filter, bool deep)
    {
        foreach (var childNode in container.Articles)
        {
            compute.InvalidateOutputs(this);

            if (FilterArticle(filter, childNode))
            {
                compute.SetValue(_childArticle, childNode);
                await compute.RunAction(_subNodeAction, cancel);
            }

            cancel.ThrowIfCancellationRequested();

            if (deep)
            {
                await PopulateAction(childNode, compute, cancel, filter, deep);
            }
        }
    }

    /// <summary>
    /// Determines whether an article passes the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <param name="node">The article to check.</param>
    /// <returns>True if the article passes the filter; otherwise, false.</returns>
    private bool FilterArticle(ArticleFilters filter, IArticle node)
    {
        switch (filter)
        {
            case ArticleFilters.All:
                return true;

            case ArticleFilters.BlankContent:
                return string.IsNullOrWhiteSpace(node.Content);

            case ArticleFilters.NotBlankContent:
                return !string.IsNullOrWhiteSpace(node.Content);

            case ArticleFilters.BlankGuide:
                return string.IsNullOrWhiteSpace(node.Guide);

            case ArticleFilters.NotBlankGuide:
                return !string.IsNullOrWhiteSpace(node.Guide);

            case ArticleFilters.BlankNote:
                return string.IsNullOrWhiteSpace(node.Note);

            case ArticleFilters.NotBlankNote:
                return !string.IsNullOrWhiteSpace(node.Note);

            default:
                return true;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_filter.GetIsLinked(this) && _filter.BaseValue != ArticleFilters.All)
        {
            return $"{DisplayText}({_filter.BaseValue.ToDisplayText()})";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region CloneArticleOutline

/// <summary>
/// Node that clones an article outline (titles and overviews only) to a new article node.
/// </summary>
[DisplayText("Clone Article Outline")]
[ToolTipsText("Clone an article outline (containing only title and overview) to a new article node.")]
public class CloneArticleOutline : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _outlineIn;
    private readonly FlowNodeConnector _articleIn;

    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloneArticleOutline"/> class.
    /// </summary>
    public CloneArticleOutline()
    {
        _in = AddActionInputConnector("In", "Input");
        _outlineIn = AddDataInputConnector("OutlineIn", ArticleAsset.ArticleType, "Outline Node");
        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Article");

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var outline = compute.GetValue<IArticle>(_outlineIn);
        var doc = compute.GetValue<IArticle>(_articleIn);

        //if (outline is null)
        //{
        //    compute.AddLog(TextStatus.Warning, "Failed to clone article outline, outline is empty");
        //    return Task.FromResult<object>(_out);
        //}

        if (doc is null)
        {
            compute.AddLog(TextStatus.Warning, "Failed to clone article outline, article is empty");
            return Task.FromResult<object>(_out);
        }

        if (outline is null)
        {
            return Task.FromResult<object>(_out);
        }

        CloneOutlineDeep(outline, doc);

        return Task.FromResult<object>(_out);
    }

    /// <summary>
    /// Recursively clones the article outline hierarchy.
    /// </summary>
    /// <param name="outline">The source outline article.</param>
    /// <param name="doc">The target article to clone into.</param>
    private void CloneOutlineDeep(IArticle outline, IArticle doc)
    {
        foreach (var childOutline in outline.Articles)
        {
            var childNode = doc.GetOrAddArticle(childOutline.Title);
            if (childNode != null)
            {
                childNode.Overview = childOutline.Overview;
                childNode.Commit();
            }

            CloneOutlineDeep(childOutline, childNode);
        }
    }
}

#endregion

#region CloneArticle

/// <summary>
/// Node that clones a single article to a target article, optionally keeping hierarchy and content.
/// </summary>
[DisplayText("Clone Article")]
[ToolTipsText("Clone a single article")]
public class CloneArticle : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _docSource;
    private readonly FlowNodeConnector _docTarget;

    private readonly FlowNodeConnector _out;
    private readonly FlowNodeConnector _docResult;

    private readonly ConnectorValueProperty<bool> _keepHierarchy
        = new ConnectorValueProperty<bool>("KeepHierarchy", "Keep Hierarchy", false, "Also create all parent article nodes of this article node.");

    private readonly ConnectorValueProperty<bool> _cloneContent
        = new ConnectorValueProperty<bool>("CloneContent", "Clone Content");

    /// <summary>
    /// Initializes a new instance of the <see cref="CloneArticle"/> class.
    /// </summary>
    public CloneArticle()
    {
        _in = AddActionInputConnector("In", "Input");
        _docSource = AddDataInputConnector("ArticleSource", ArticleAsset.ArticleType, "Article Source");
        _docTarget = AddDataInputConnector("ArticleTarget", ArticleAsset.ArticleType, "Article Target");
        _keepHierarchy.AddConnector(this);
        _cloneContent.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
        _docResult = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Result Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _keepHierarchy.Sync(sync);
        _cloneContent.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _keepHierarchy.InspectorField(setup, this);
        _cloneContent.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var docSource = compute.GetValue<IArticle>(_docSource);
        if (docSource is null)
        {
            throw new NullReferenceException($"Source doc not found.");
        }

        var docTarget = compute.GetValue<IArticle>(_docTarget);

        bool keepHierarchy = _keepHierarchy.GetValue(compute, this);
        bool cloneContent = _cloneContent.GetValue(compute, this);

        if (keepHierarchy)
        {
            var list = docSource.GetParentHierarchy();
            var target = docTarget;
            target.ClonePropertyFrom(list[0], false, true, cloneContent);

            foreach (var item in list.Skip(1))
            {
                target = target.GetOrAddArticle(item.Title);
                target.ClonePropertyFrom(item, false, true, cloneContent);
            }

            compute.SetValue(_docResult, target);
            target.Commit();
        }
        else
        {
            var target = docTarget;
            target.ClonePropertyFrom(docSource, false, true, cloneContent);

            compute.SetValue(_docResult, target);
            target.Commit();
        }

        return Task.FromResult<object>(_out);
    }
}

#endregion

#region ManualSelectArticle

/// <summary>
/// Node that allows manual selection of an article through the UI interface.
/// </summary>
[DisplayText("Manual Select Article")]
[ToolTipsText("Manually select an article through the UI interface")]
//TODO: UI dialog now uses async mode, implementing with Coroutine will cause issues.
[NotAvailable] 
public class ManualSelectArticle : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private ConnectorTextBlockProperty _message = new("Message", "Message");

    private readonly FlowNodeConnector _created;
    private readonly FlowNodeConnector _opened;
    private readonly FlowNodeConnector _failed;

    private readonly FlowNodeConnector _articleOut;

    private readonly ValueProperty<bool> _openEnabled
        = new("OpenEnabled", "Allow Open Document", true);

    private readonly ValueProperty<bool> _createEnabled
        = new("CreateEnabled", "Allow Create Document", true);

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualSelectArticle"/> class.
    /// </summary>
    public ManualSelectArticle()
    {
        _in = AddActionInputConnector("In", "Input");
        _message.AddConnector(this);

        _created = AddActionOutputConnector("Created", "Created");
        _opened = AddActionOutputConnector("Opened", "Opened");
        _failed = AddActionOutputConnector("Failed", "Failed");
        _articleOut = AddDataOutputConnector("ArticleOut", ArticleAsset.ArticleType, "Article");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
        _createEnabled.Sync(sync);
        _openEnabled.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
        _createEnabled.InspectorField(setup);
        _openEnabled.InspectorField(setup);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>();
        if (conversation is null)
        {
            throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");
        }

        if (!_openEnabled.Value && !_createEnabled.Value)
        {
            compute.SetValue(_articleOut, null);

            return _failed;
        }

        string msg = _message.GetText(compute, this) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = "Please select an article";
        }

        cancel.ThrowIfCancellationRequested();

        // Create passive task
        var source = new TaskCompletionSource<Tuple<IArticle, bool>>();
        // Register for cancellation
        var cancelReg = cancel.Register(() => source.TrySetCanceled());

        conversation.StartCoroutine(DialogCoroutine(conversation, msg, (p, n) => source.SetResult(new(p, n))));

        var result = await source.Task;

        cancelReg.Dispose();
        compute.SetValue(_articleOut, result.Item1);
        if (result.Item1 != null)
        {
            if (result.Item2)
            {
                return _created;
            }
            else
            {
                return _opened;
            }
        }
        else
        {
            return _failed;
        }
    }

    /// <summary>
    /// Coroutine that displays a dialog for selecting or creating an article.
    /// </summary>
    /// <param name="conversation">The conversation handler.</param>
    /// <param name="msg">The message to display.</param>
    /// <param name="setter">Action to set the result.</param>
    private IEnumerator DialogCoroutine(IConversationHandler conversation, string msg, Action<IArticle, bool> setter)
    {
        var dialogItem = conversation.AddDebugMessage(msg, o =>
        {
            List<ConversationButton> btns = new List<ConversationButton>();

            if (_createEnabled.Value)
            {
                btns.Add(new ConversationButton { Key = "Create", Text = "Create" });
            }

            if (_openEnabled.Value)
            {
                btns.Add(new ConversationButton { Key = "Select", Text = "Select" });
            }

            o.AddButtons(string.Empty, btns.ToArray());
        });

    label_dialog_01:
        {
            yield return null;

            switch (conversation.InputButton)
            {
                case "Select":
                    goto label_select_item;

                case "Create":
                    goto label_create_doc;

                default:
                    yield break;
            }
        }

    label_select_item:
        {
            var list = new ArticleDocumentSelectionNode();
            var result = list.ShowSelectionGUI("Please select an article", new SelectionOption { AllowSelectList = true });
            if (!result.IsSuccess)
            {
                goto label_dialog_01;
            }

            // Remove this dialog, user cannot continue clicking to select.
            if (dialogItem != null)
            {
                dialogItem?.Dispose();
                conversation.AddDebugMessage(msg);
                conversation.AddUserMessage("Selected: " + result.Item?.ToDisplayText() ?? string.Empty);
                dialogItem = null;
            }

            setter(result.Item as IArticle, false);

            yield break;
        }

    label_create_doc:
        {
            var articleCreate = ArticleExtensions.DialogNewArticle().GetAwaiter().GetResult(); //TODO: Will cause error.
            if (articleCreate is null)
            {
                goto label_dialog_01;
            }

            // Remove this dialog, user cannot continue clicking to select.
            if (dialogItem != null)
            {
                dialogItem?.Dispose();
                conversation.AddDebugMessage(msg);
                conversation.AddUserMessage("Created: " + articleCreate?.ToDisplayText() ?? string.Empty);
                dialogItem = null;
            }

            setter(articleCreate, true);
        }
    }

    #region SelectionNode

    /// <summary>
    /// Selection node that lists all available article documents.
    /// </summary>
    private class ArticleDocumentSelectionNode : SelectionNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleDocumentSelectionNode"/> class.
        /// </summary>
        public ArticleDocumentSelectionNode()
        {
            var articles = AssetManager.Instance.GetAssets<ArticleContainerAsset>().OrderBy(o => o.AssetKey);

            foreach (var articleGroupAsset in articles)
            {
                Add(new ArticleSelectionNode(articleGroupAsset));
            }
        }
    }

    /// <summary>
    /// Selection node representing an article container asset with its child articles.
    /// </summary>
    private class ArticleSelectionNode : SelectionNode
    {
        private readonly ArticleContainerAsset _asset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleSelectionNode"/> class.
        /// </summary>
        /// <param name="asset">The article container asset to represent.</param>
        public ArticleSelectionNode(ArticleContainerAsset asset)
        {
            _asset = asset ?? throw new ArgumentNullException(nameof(asset));

            var doc = _asset.GetDocument<IArticleDocument>(true);
            if (doc is null)
            {
                return;
            }

            AddRange(doc.Articles.OfType<ISelectionItem>());
        }

        /// <inheritdoc/>
        public override string SelectionKey => ((ISelectionItem)_asset).SelectionKey;

        /// <inheritdoc/>
        public override string DisplayText => _asset.DisplayText;

        /// <inheritdoc/>
        public override object DisplayIcon => _asset.Icon;

        /// <inheritdoc/>
        public override bool Selectable => true;
    }

    #endregion
}

#endregion

#region ArticleContentToGuide

/// <summary>
/// Node that transcribes the writing guide from an article to its content, then clears the guide.
/// </summary>
[DisplayText("Guide To Content")]
[ToolTipsText("Transcribe the writing guide from the article to the content, and delete the writing guide.")]
public class ArticleGuideToContent : AigcArticleNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _article;
    private readonly ConnectorValueProperty<bool> _deep = new("Deep", "Set Child Articles", false, "Transcribe all child articles.");

    private readonly FlowNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleGuideToContent"/> class.
    /// </summary>
    public ArticleGuideToContent()
    {
        _in = AddActionInputConnector("In", "Input");
        _article = AddDataInputConnector("Article", ArticleAsset.ArticleType, "Article");
        _deep.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _deep.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _deep.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var article = compute.GetValue<IArticle>(_article);
        if (article is null)
        {
            throw new NullReferenceException($"{nameof(IArticle)} Not found.");
        }

        if (_deep.GetValue(compute, this))
        {
            article.ForeachArticleDeep(a =>
            {
                a.Content = a.Guide;
                a.Guide = string.Empty;
            });
        }
        else
        {
            article.Content = article.Guide;
            article.Guide = string.Empty;
        }

        article.Commit();

        return Task.FromResult<object>(_out);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_deep.GetIsLinked(this) && _deep.BaseValue)
        {
            return $"{DisplayText}(Deep)";
        }
        else
        {
            return DisplayText;
        }
    }
}

#endregion

#region ArticleFullContent

/// <summary>
/// Node that retrieves the full content text of an article, including child articles, in Markdown format.
/// </summary>
[SimpleFlowNodeStyle(HasHeader = false)]
[DisplayText("Article Full Content")]
[ToolTipsText("Get the full content text of the article, including child article content, in Markdown format.")]
public class ArticleFullContent : AigcArticleNode
{
    private readonly FlowNodeConnector _articleIn;

    private readonly FlowNodeConnector _content;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleFullContent"/> class.
    /// </summary>
    public ArticleFullContent()
    {
        _articleIn = AddDataInputConnector("ArticleIn", ArticleAsset.ArticleType, "Article");
        _content = AddDataOutputConnector("Content", "string", "Full Content");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var article = compute.GetValue<IArticle>(_articleIn);

        string text = article?.GetFullText() ?? string.Empty;

        compute.SetValue(_content, text);
    }

    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();
    }
}

#endregion
