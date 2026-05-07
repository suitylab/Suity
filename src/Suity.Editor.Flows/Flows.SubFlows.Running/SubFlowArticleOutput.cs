using Suity;
using Suity.Editor.AIGC.Flows.Pages;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.Flows.SubFlows.Running;

#region SubFlowArticleRefItem

/// <summary>
/// Represents a reference item for an article within a page output element.
/// </summary>
public class SubFlowArticleRefItem : ArticleViewItem
{
    readonly SubFlowArticleOutput _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefItem"/> class.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="location">The article location.</param>
    public SubFlowArticleRefItem(SubFlowArticleOutput parent, string propertyName, ArticleLocation location)
        : this(parent, propertyName, null, location)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefItem"/> class with a location getter.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    public SubFlowArticleRefItem(SubFlowArticleOutput parent, string propertyName, Func<ArticleLocation> locationGetter)
        : this(parent, propertyName, null, locationGetter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefItem"/> class with a description.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="location">The article location.</param>
    public SubFlowArticleRefItem(SubFlowArticleOutput parent, string propertyName, string description, ArticleLocation location)
        : base(parent, propertyName, description, location)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefItem"/> class with a description and location getter.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    public SubFlowArticleRefItem(SubFlowArticleOutput parent, string propertyName, string description, Func<ArticleLocation> locationGetter)
        : base(parent, propertyName, description, locationGetter)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }


    /// <summary>
    /// Gets the parent page article output element.
    /// </summary>
    public SubFlowArticleOutput Parent => _parent;

    /// <inheritdoc/>
    protected override void DoAction(UndoRedoAction action)
    {
        if (_parent.Root is { } root)
        {
            root.DoAction(action);
        }
        else
        {
            action.Do();
        }
    }
}
#endregion

#region SubFlowArticleRefCollection

/// <summary>
/// Represents a collection of article reference items for a sub-flow output element.
/// </summary>
public class SubFlowArticleRefCollection : ArticleViewCollection<SubFlowArticleRefItem>
{
    readonly SubFlowArticleOutput _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefCollection"/> class.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="articleUsage">The article usage identifier.</param>
    /// <param name="path">The path segments for the article location.</param>
    public SubFlowArticleRefCollection(SubFlowArticleOutput parent, string articleUsage, params string[] path)
        : base(parent, articleUsage, path)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefCollection"/> class with a specific location.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="location">The article location.</param>
    public SubFlowArticleRefCollection(SubFlowArticleOutput parent, ArticleLocation location)
        : base(parent, location)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleRefCollection"/> class with a location getter.
    /// </summary>
    /// <param name="parent">The parent page article output element.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    public SubFlowArticleRefCollection(SubFlowArticleOutput parent, Func<ArticleLocation> locationGetter)
    : base(parent, locationGetter)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    /// <inheritdoc/>
    public override SubFlowArticleRefItem CreateRefItem(string propertyName, string description, Func<ArticleLocation> locationGetter)
    {
        return new SubFlowArticleRefItem(_parent, propertyName, description, locationGetter);
    }
}

#endregion

/// <summary>
/// Represents a page element that outputs article content in an AIGC flow.
/// Implements article resolution and page parameter output functionality.
/// </summary>
public class SubFlowArticleOutput : SubFlowElement, IArticleResolver, IPageParameterOutput
{
    private readonly PageArticleOutputItem _outputItem;
    private FlowNodeConnector _outerConnector;

    private bool _multipleSection;
    private string[] _articlePath;
    private bool _isStaticArticlePath;
    private SubFlowArticleRefItem _articleRef;
    private SubFlowArticleRefCollection _articleRefCollection;


    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowArticleOutput"/> class.
    /// </summary>
    /// <param name="outputItem">The output item that provides configuration data.</param>
    public SubFlowArticleOutput(PageArticleOutputItem outputItem)
        : base(outputItem)
    {
        _outputItem = outputItem ?? throw new ArgumentNullException(nameof(outputItem));
    }

    /// <inheritdoc/>
    public override FlowNodeConnector OuterConnector => _outerConnector;


    /// <summary>
    /// Displaying multiple section
    /// </summary>
    public bool MultipleSection => _multipleSection;

    /// <summary>
    /// Gets a value indicating whether the output should be passed to sub-tasks.
    /// </summary>
    public bool PassToSubTasks { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the linked mode is enabled.
    /// </summary>
    public bool LinkedMode { get; private set; }

    /// <summary>
    /// Gets the target writing field for the article content.
    /// </summary>
    public ArticleFields WritingTarget { get; private set; }

    #region IPageParameterOutput

    /// <inheritdoc/>
    public TypeDefinition ParameterType => TypeDefinition.FromNative<IArticle>();

    /// <inheritdoc/>
    public object Value => ResolveArticle(false);

    /// <inheritdoc/>
    public bool IsValueSet { get => true; set { } }

    /// <inheritdoc/>
    public void SetValue(object value)
    {
    }

    /// <inheritdoc/>
    public object EnsureValue() => ResolveArticle(true);

    /// <summary>
    /// Gets a value indicating whether task completion tracking is enabled.
    /// </summary>
    public bool TaskCompletion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether task commit tracking is enabled.
    /// </summary>
    public bool TaskCommit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether chat history is enabled.
    /// </summary>
    public bool ChatHistory { get; private set; }

    /// <summary>
    /// Resolves the chat history text content for this output element.
    /// </summary>
    /// <returns>The resolved chat history text, or an empty string if not available.</returns>
    public ChatHistoryText ResolveChatHistory()
    {
        var article = ResolveArticle(false);

        if (LinkedMode)
        {
            return article?.ArticleUrl ?? string.Empty;
        }
        else
        {
            if (_articleRefCollection != null)
            {
                return _articleRefCollection.GetPlainTextNullable();
            }
            else if (_articleRef != null)
            {
                return _articleRef.GetPlainTextNullable();
            }
            else
            {
                return string.Empty;
            }

/*            switch (WritingTarget)
            {
                case ArticleFields.Content:
                    return article?.GetFullText() ?? string.Empty;

                case ArticleFields.Overview:
                    return article?.Overview ?? string.Empty;

                case ArticleFields.Guide:
                    return article?.Guide ?? string.Empty;

                case ArticleFields.Note:
                    return article.Note ?? string.Empty;

                default:
                    return string.Empty;
            }*/
        }
    }

    #endregion

    #region IArticleResolver

    /// <inheritdoc/>
    public IArticle ResolveArticle(string usage, bool autoCreate, string docTitle = null)
    {
        // Here we need to get Base, combine with variable ArticleLocation, and pass to _articleRef
        return ResolveArticleBase(autoCreate);
    }

    /// <inheritdoc/>
    public IArticle ResolveArticle(bool autoCreate)
    {
        IArticle article = ResolveArticleBase(autoCreate);
        if (article is null)
        {
            return null;
        }

        var paths = ResolvePath();
        foreach (var p in paths)
        {
            article = article?.GetOrAddArticle(p);
        }

        article?.Commit();

        return article;
    }

    /// <summary>
    /// Resolves the base article from the task service.
    /// </summary>
    /// <param name="autoCreate">Whether to create the article if it does not exist.</param>
    /// <returns>The resolved base article, or null if not available.</returns>
    public IArticle ResolveArticleBase(bool autoCreate)
    {
        if (Option.Owner is not IAigcTaskPage taskService)
        {
            return null;
        }

        try
        {
            return taskService.ResolveArticleBase(autoCreate);
        }
        catch (Exception err)
        {
            err.LogError("Failed to get article document.");
            return null;
        }
    }

    #endregion

    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (sync.Intent == SyncIntent.View)
        {
            if (_multipleSection)
            {
                if (_articleRefCollection is { } articles)
                {
                    sync.Sync(Name, articles, SyncFlag.GetOnly);
                }
            }
            else
            {
                if (_articleRef is { } articleRef)
                {
                    sync.Sync(articleRef.Property.Name, articleRef, SyncFlag.GetOnly);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SetupView(IViewObjectSetup setup)
    {
        if (!Option.Mode.IsTaskOrPage())
        {
            return;
        }

        if (_multipleSection)
        {
            if (_articleRefCollection is { } articles)
            {
                var property = new ViewProperty(Name, DisplayText, Icon)
                    .WithExpand()
                    .WithStatus(GetStatus());

                setup.InspectorField(articles, property);
            }
        }
        else
        {
            if (_articleRef is { } articleRef)
            {
                var property = articleRef.Property
                    .WithExpand()
                    .WithStatus(GetStatus());

                setup.InspectorField(articleRef, articleRef.Property);
            }
        }
    }

    /// <inheritdoc/>
    public override void UpdateConnector(PageFunctionNode node)
    {
        var articleType = TypeDefinition.FromNative<IArticle>();
        _outerConnector = node.AddDataOutputConnector(Name, articleType, DisplayText);
    }




    /// <inheritdoc/>
    protected override void OnBuild()
    {
        base.OnBuild();

        var node = _outputItem.Node;

        PassToSubTasks = node?.PassToSubTasks == true;
        LinkedMode = node?.LinkedMode == true;
        WritingTarget = node?.WritingTarget ?? ArticleFields.Content;

        TaskCompletion = node?.TaskCompletion == true;
        TaskCommit = node?.TaskCommit == true;
        ChatHistory = node?.ChatHistory == true;


        string pathStr = node?.ArticlePath ?? string.Empty;
        _articlePath ??= ResolvePath(pathStr, out _isStaticArticlePath);
        
        Func<ArticleLocation> locationGetter;
        //if (_isStaticArticlePath)
        //{
        //    var location = new ArticleLocation(string.Empty, _articlePath);
        //    locationGetter = () => location;
        //}
        //else
        //{
            locationGetter = ResolveLocation;
        //}


        _multipleSection = node?.MultipleSection ?? false;
        if (_multipleSection)
        {
            _articleRef = null;

            _articleRefCollection = new(this, locationGetter);
        }
        else
        {
            _articleRefCollection = null;

            _articleRef = new(this, Name, DisplayText, locationGetter);
            _articleRef.Property.Description = DisplayText;
            _articleRef.Property.Icon = Icon;
        }
    }

    /// <summary>
    /// Resolves the article location based on the current writing target and path.
    /// </summary>
    /// <returns>The resolved article location.</returns>
    public ArticleLocation ResolveLocation()
    {
        var node = _outputItem.Node;
        var field = WritingTarget;
        var paths = ResolvePath();

        return new ArticleLocation(string.Empty, paths, field);
    }

    private string[] ResolvePath()
    {
        var node = _outputItem.Node;

        string pathStr = node?.ArticlePath ?? string.Empty;
        _articlePath = ResolvePath(pathStr, out _isStaticArticlePath);

        if (_isStaticArticlePath)
        {
            return _articlePath;
        }
        else
        {
            string[] paths = new string[_articlePath.Length];
            Array.Copy(_articlePath, paths, _articlePath.Length);

            for (int i = 0; i < paths.Length; i++)
            {
                string p = paths[i];
                if (p.StartsWith("{") && p.EndsWith("}"))
                {
                    var parameterName = p.Substring(1, p.Length - 2);
                    if (Root?.TryGetParameter(null, parameterName, out var parameter) == true)
                    {
                        paths[i] = parameter?.ToString() ?? string.Empty;
                    }
                }
            }

            return paths;
        }
    }


    /// <inheritdoc/>
    public override void UpdateFromOther(ISubFlowElement other)
    {
        if (other is SubFlowArticleOutput otherOutput)
        {
            UpdateFromOther(otherOutput);
        }
    }

    /// <summary>
    /// Updates this element from another page article output element.
    /// </summary>
    /// <param name="otherParameter">The other element to update from.</param>
    public void UpdateFromOther(SubFlowArticleOutput otherParameter)
    {
    }

    /// <summary>
    /// Sets the output value on the outer flow computation connector.
    /// </summary>
    /// <param name="outerCompute">The outer flow computation instance.</param>
    /// <param name="value">The value to set.</param>
    public void SetOuterValue(IFlowComputation outerCompute, object value)
    {
        if (_outerConnector != null)
        {
            outerCompute.SetValue(_outerConnector, value);
        }
    }

    /// <inheritdoc/>
    public override bool? GetIsDone()
    {
        if (TaskCompletion)
        {
            if (_multipleSection)
            {
                var v = _articleRefCollection != null && _articleRefCollection.HasArticles;
                return v;
            }
            else
            {
                var v = _articleRef != null && _articleRef.GetArticle() is { } article && !string.IsNullOrWhiteSpace(article.Content);
                return v;
            }
        }
        else
        {
            return null;
        }
    }




    /// <summary>
    /// Resolves a path string into an array of path segments.
    /// </summary>
    /// <param name="pathStr">The path string to resolve, with segments separated by '/'.</param>
    /// <param name="isStatic">When this method returns, contains a value indicating whether the path is static (no variable placeholders).</param>
    /// <returns>An array of path segment strings.</returns>
    public static string[] ResolvePath(string pathStr, out bool isStatic)
    {
        isStatic = true;

        if (string.IsNullOrWhiteSpace(pathStr))
        {
            return [];
        }

        var paths = pathStr.Split(['/'], StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .ToArray();

        isStatic = !paths.Any(p => p.StartsWith("{") && p.EndsWith("}"));

        return paths;
    }
}
