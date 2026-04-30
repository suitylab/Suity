using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.Articles;

[NativeAlias("Suity.Editor.AIGC.Planning.AigcArticle")]
[NativeAlias("Suity.Editor.AIGC.AigcArticle")]
[DisplayText("Article", "*CoreIcon|Article")]
/// <summary>
/// Represents an article node that can contain nested sub-articles and metadata.
/// </summary>
public class Article : DesignNode<ArticleAssetBuilder>, IArticle,
    ISelectionItem, ISelectionList, ISelectionNode, IDrawEditorImGui
{
    private readonly ValueProperty<bool> _readingMaterial
        = new(nameof(IsReadingMaterial), "Reading Material", false, "Can be referenced as reading material.");
    private readonly StringProperty _title = new(nameof(Title), "Title", "Article");
    private readonly TextBlockProperty _overview = new(nameof(Overview), "Overview");
    private readonly TextBlockProperty _content = new(nameof(Content), "Content");
    private readonly StringProperty _type = new(nameof(Type), "Type");
    private readonly TextBlockProperty _guide = new(nameof(Guide), "Writing Guide");
    private readonly TextBlockProperty _note = new(nameof(Note), "Note");

    /// <summary>
    /// Initializes a new instance of the <see cref="Article"/> class.
    /// </summary>
    public Article()
    {
        _title.ValueChanged += (s, e) => 
        {
            AssetBuilder?.SetDescription(Title);
        };

        _readingMaterial.ValueChanged += (s, e) =>
        {
            AssetBuilder?.SetReadingMaterial(_readingMaterial.Value);
        };
    }


    #region IArticle

    /// <summary>
    /// Gets the parent article container or document.
    /// </summary>
    public IArticleContainer Parent
    {
        get
        {
            if (ParentNode is IArticleContainer container)
            {
                return container;
            }
            else if (this.GetDocument() is ArticleDocument doc)
            {
                return doc;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this article can be referenced as reading material.
    /// </summary>
    public bool IsReadingMaterial 
    {
        get => _readingMaterial.Value;
        set => _readingMaterial.Value = value;
    }

    /// <summary>
    /// Gets the URL of this article in the article:// protocol format.
    /// </summary>
    public string ArticleUrl
    {
        get
        {
            string pathItem = Title;
            if (string.IsNullOrWhiteSpace(pathItem))
            {
                pathItem = ArticleId;
            }

            var parent = ParentNode as Article;
            if (parent != null)
            {
                return $"{parent.ArticleUrl}/{pathItem}";
            }
            else
            {
                var docAsset = GetDocument()?.GetAsset();
                if (docAsset != null)
                {
                    return $"article://{docAsset.AssetKey}?{pathItem}";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }

    /// <summary>
    /// Gets the asset key of the target article asset.
    /// </summary>
    public string ArticleAssetKey => TargetAsset?.AssetKey ?? string.Empty;

    /// <summary>
    /// Gets the total length of this article and all nested articles.
    /// </summary>
    public long ArticleLength 
        => (_content.Value?.Text?.Length ?? 0) 
        + Items.OfType<Article>().Sum(o => o.ArticleLength);

    /// <summary>
    /// Gets or sets the unique identifier of this article.
    /// </summary>
    public string ArticleId
    {
        get => Name;
        set => Name = value;
    }

    /// <summary>
    /// Gets or sets the title of this article.
    /// </summary>
    public string Title
    {
        get => _title.Text ?? string.Empty;
        set => _title.Text = value;
    }

    /// <summary>
    /// Gets or sets the overview/summary of this article.
    /// </summary>
    public string Overview
    {
        get => _overview.Text ?? string.Empty;
        set => _overview.Text = value;
    }

    /// <summary>
    /// Gets or sets the main content of this article.
    /// </summary>
    public string Content
    {
        get => _content.Text ?? string.Empty;
        set => _content.Text = value;
    }

    /// <summary>
    /// Gets or sets the type/category of this article.
    /// </summary>
    public string Type
    {
        get => _type.Text ?? string.Empty;
        set => _type.Text = value;
    }

    /// <summary>
    /// Gets or sets the writing guide for this article.
    /// </summary>
    public string Guide
    {
        get => _guide.Text ?? string.Empty;
        set => _guide.Text = value;
    }

    /// <summary>
    /// Gets or sets notes for this article.
    /// </summary>
    public string Note
    {
        get => _note.Text ?? string.Empty;
        set => _note.Text = value;
    }

    /// <summary>
    /// Commits changes to this article by marking the document as dirty and scheduling a save.
    /// </summary>
    public void Commit()
    {
        var doc = GetDocument();
        if (doc != null)
        {
            doc.MarkDirty(this);
            doc.Entry.SaveDelayed();
        }
    }

    #endregion

    #region IArticleContainer

    /// <summary>
    /// Gets or adds a sub-article by name. Creates a new article if it doesn't exist.
    /// </summary>
    /// <param name="name">The name or ID of the article.</param>
    /// <returns>The existing or newly created article.</returns>
    public IArticle GetOrAddArticle(string name)
    {
        Article node = null;

        if (!string.IsNullOrWhiteSpace(name))
        {
            node = Items.OfType<Article>().FirstOrDefault(o => o.Title == name || o.ArticleId == name);
            if (node != null)
            {
                return node;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = AutoGenerateTitle();
        }

        node = new Article { Title = name };
        AddItem(node);

        return node;
    }

    /// <summary>
    /// Gets a sub-article by name or ID.
    /// </summary>
    /// <param name="name">The name or ID of the article.</param>
    /// <returns>The article if found, otherwise null.</returns>
    public IArticle GetArticle(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Items.OfType<Article>().FirstOrDefault(o => o.Title == name || o.ArticleId == name);
    }

    /// <summary>
    /// Gets all sub-articles in this article.
    /// </summary>
    public IEnumerable<IArticle> Articles => Items.OfType<Article>();

    /// <summary>
    /// Gets the total number of sub-articles.
    /// </summary>
    public int ArticleCount => this.Count;

    /// <summary>
    /// Removes a sub-article from this article.
    /// </summary>
    /// <param name="article">The article to remove.</param>
    /// <returns>True if the article was removed successfully.</returns>
    public bool RemoveArticle(IArticle article) => RemoveItem(article as Article);

    /// <summary>
    /// Clears all sub-articles from this article.
    /// </summary>
    public void ClearArticles()
    {
        base.Clear();
    }


    private string AutoGenerateTitle()
    {
        string prefix = "Article";
        ulong num = 1;

        while (true)
        {
            string name = KeyIncrementHelper.MakeKey(prefix, 2, num);
            if (GetArticle(name) is null)
            {
                return name;
            }

            num++;
        }
    }

    #endregion

    #region ISelectionList

    /// <summary>
    /// Gets all selectable items (sub-articles) in this article.
    /// </summary>
    IEnumerable<ISelectionItem> ISelectionList.GetItems()
    {
        return Items.OfType<Article>();
    }

    /// <summary>
    /// Gets a selectable item by key.
    /// </summary>
    /// <param name="key">The article URL key.</param>
    /// <returns>The article if found, otherwise null.</returns>
    ISelectionItem ISelectionList.GetItem(string key)
    {
        return Items.OfType<Article>().FirstOrDefault(o => o.ArticleUrl == key);
    }

    #endregion

    #region ISelectionNode

    /// <summary>
    /// Gets a value indicating whether this article is selectable.
    /// </summary>
    bool ISelectionNode.Selectable => true;


    #endregion

    #region IDrawEditorImGui

    /// <inheritdoc/>
    public override bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        if (pipeline == EditorImGuiPipeline.Name)
        {
            if (_readingMaterial.Value)
            {
                gui.Image($"#icon_reading_material", CoreIconCache.Read)
                .InitClass("icon")
                .SetToolTipsL("Can be referenced as reading material.");
            }
        }

        if (pipeline == EditorImGuiPipeline.Preview)
        {
            long len = ArticleLength;

            if (len > 0)
            {
                gui.HorizontalFrame("step")
                .InitClass("refBox")
                .InitFit()
                .OverrideColor(ArticleAsset.ArticleBgColor)
                .InitOverridePadding(0, 0, 5, 5)
                .SetToolTips(L("Total Article Length") + ": " + len)
                .OnContent(() =>
                {
                    gui.Text(NumberAbbreviation(len))
                    .InitClass("numBoxText")
                    .SetFontColor(Color.Black);
                });
            }

            if (!string.IsNullOrWhiteSpace(Overview))
            {
                gui.Image($"#icon_overview", CoreIconCache.Brief)
                .InitClass("icon")
                .SetToolTipsL("Overview");
            }

            if (!string.IsNullOrWhiteSpace(Content))
            {
                gui.Image($"#icon_content", CoreIconCache.Article)
                .InitClass("icon")
                .SetToolTipsL("Content");
            }

            if (!string.IsNullOrWhiteSpace(Type))
            {
                gui.Image($"#icon_type", CoreIconCache.Classify)
                .InitClass("icon")
                .SetToolTipsL("Type");
            }

            if (!string.IsNullOrWhiteSpace(Guide))
            {
                gui.Image($"#icon_guide", CoreIconCache.Guiding)
                .InitClass("icon")
                .SetToolTipsL("Guide");
            }

            if (!string.IsNullOrWhiteSpace(Note))
            {
                gui.Image($"#icon_note", CoreIconCache.Script)
                .InitClass("icon")
                .SetToolTipsL("Note");
            }
        }

        return base.OnEditorGui(gui, pipeline, context);
    }

    #endregion

    #region Virtual

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "#Article-";

    /// <inheritdoc/>
    protected override bool OnVerifyName(string name) => true;

    /// <inheritdoc/>
    protected override string OnGetDisplayText() => Title;

    /// <inheritdoc/>
    protected override void OnSetText(string text, ISyncContext setup, bool showNotice)
    {
        if (Title == text)
        {
            return;
        }

        setup.DoServiceAction<IViewSetValue>(v => v.SetValue(nameof(Title), text));
    }

    /// <inheritdoc/>
    public override void Find(ValidationContext context, string findStr, Synchonizing.Core.SearchOption findOption)
    {
        base.Find(context, findStr, findOption);

        if (Validator.Compare(_title.Text, findStr, findOption))
        {
            context.Report(_title.Text, this);
        }

        if (Validator.Compare(_overview.Text, findStr, findOption))
        {
            context.Report(_overview.Text, this);
        }

        if (Validator.Compare(_content.Text, findStr, findOption))
        {
            context.Report(_content.Text, this);
        }

        if (Validator.Compare(_guide.Text, findStr, findOption))
        {
            context.Report(_guide.Text, this);
        }

        if (Validator.Compare(_note.Text, findStr, findOption))
        {
            context.Report(_note.Text, this);
        }
    }

    #endregion

    #region Data Sync

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _readingMaterial.Sync(sync);
        _title.Sync(sync);
        _overview.Sync(sync);
        _content.Sync(sync);
        _type.Sync(sync);
        _guide.Sync(sync);
        _note.Sync(sync);

        //string s = sync.Sync(nameof(Content), string.Empty);
        //if (!string.IsNullOrEmpty(s))
        //{
        //    (_content ??= new()).Text = s;
        //}

        if (sync.Intent == SyncIntent.View)
        {
            sync.Sync(nameof(ArticleAssetKey), ArticleAssetKey);
            sync.Sync(nameof(ArticleUrl), ArticleUrl);
            sync.Sync(nameof(ArticleLength), ArticleLength);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(Name, new ViewProperty(nameof(Name), "Article ID"));
        //setup.InspectorField(Description, new ViewProperty(nameof(Name), "Article ID"));
        _readingMaterial.InspectorField(setup);

        setup.InspectorField(ArticleAssetKey, new ViewProperty(nameof(ArticleAssetKey), "Article Asset Key").WithReadOnly());
        setup.InspectorField(ArticleUrl, new ViewProperty(nameof(ArticleUrl), "Article URL").WithReadOnly());
        setup.InspectorField(ArticleLength, new ViewProperty(nameof(ArticleLength), "Length").WithReadOnly());
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.InspectorField(LabelValue.Empty, new ViewProperty("Article", "Article"));
        _title.InspectorField(setup);
        _overview.InspectorField(setup);
        _content.InspectorField(setup);

        setup.InspectorField(LabelValue.Empty, new ViewProperty("WritingAssistance", "Writing Assistance"));
        _type.InspectorField(setup);
        _guide.InspectorField(setup);
        _note.InspectorField(setup);
    }

    #endregion

    #region Drop In
    /// <inheritdoc/>
    protected override bool OnDropInCheck(object value)
    {
        if (value is CommonFileInfo fileInfo)
        {
            string ext = Path.GetExtension(fileInfo.FilePath);
            if (ext.ToLowerInvariant() == ".md")
            {
                return true;
            }
        }

        return base.OnDropInCheck(value);
    }

    /// <inheritdoc/>
    protected override object OnDropInConvert(object value)
    {
        if (value is CommonFileInfo fileInfo)
        {
            string ext = Path.GetExtension(fileInfo.FilePath);
            if (ext.ToLowerInvariant() == ".md")
            {
                string text = TextFileHelper.ReadFile(fileInfo.FilePath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                var rootNode = MarkdownParser.ParseMarkdownToTree(text);
                if (rootNode is null)
                {
                    return null;
                }

                var article = new Article();
                rootNode.ApplyToArticle(article);

                return article;
            }
        }

        return base.OnDropInConvert(value);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        return Title;
    }

    /// <summary>
    /// Converts a number to a human-readable abbreviated string.
    /// </summary>
    /// <param name="number">The number to abbreviate.</param>
    /// <returns>The abbreviated string (e.g., "1.5K", "2.3M", "1B").</returns>
    public static string NumberAbbreviation(long number)
    {
        if (number >= 1000000000)
        {
            return (number / 1000000000D).ToString("0.#") + "B";
        }
        if (number >= 1000000)
        {
            return (number / 1000000D).ToString("0.#") + "M";
        }
        if (number >= 1000)
        {
            return (number / 1000D).ToString("0.#") + "K";
        }
        return number.ToString();
    }

}