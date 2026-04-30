using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Documents.Articles;

[NativeAlias("ArticleEdit", UseForSaving = true)]
[NativeAlias("Suity.Editor.AIGC.AigcArticleDocument")]
[DocumentFormat(FormatName = "ArticleEdit", Extensions = ["sarticle"], DisplayText = "Article", Icon = "*CoreIcon|Book", Order = 200)]
[EditorFeature(EditorFeatures.AigcWorkflow)]
/// <summary>
/// Represents an article document that manages a collection of articles for editing.
/// </summary>
public class ArticleDocument : DesignDocument<AigcArticleGroupAssetBuilder>, IArticleDocument
{
    private readonly ValueProperty<bool> _readingMaterial
       = new(nameof(IsReadingMaterial), "Reading Material", false, "Can be referenced as reading material.");

    private readonly TextBlockProperty _overview = new(nameof(IArticle.Overview), "Overview");

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleDocument"/> class.
    /// </summary>
    public ArticleDocument()
    {
        ItemCollection.FieldName = "Articles";
        ItemCollection.FieldDescription = "Article";

        ItemCollection.AddItemType<Article>("Article Node");


        _readingMaterial.ValueChanged += (s, e) =>
        {
            AssetBuilder?.SetReadingMaterial(_readingMaterial.Value);
        };
    }

    /// <summary>
    /// Gets or sets a value indicating whether this document can be referenced as reading material.
    /// </summary>
    public bool IsReadingMaterial
    {
        get => _readingMaterial.Value;
        set => _readingMaterial.Value = value;
    }

    #region IArticleContainer

    /// <summary>
    /// Gets or adds an article by name. Creates a new article if it doesn't exist.
    /// </summary>
    /// <param name="name">The name or ID of the article.</param>
    /// <returns>The existing or newly created article.</returns>
    public IArticle GetOrAddArticle(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var node = ItemCollection.Items.OfType<Article>().FirstOrDefault(o => o.Title == name || o.ArticleId == name);
        if (node is null)
        {
            node = new Article { Title = name };
            ItemCollection.AddItem(node);
        }

        return node;
    }

    /// <summary>
    /// Gets an article by name or ID.
    /// </summary>
    /// <param name="name">The name or ID of the article.</param>
    /// <returns>The article if found, otherwise null.</returns>
    public IArticle GetArticle(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return ItemCollection.Items.OfType<Article>().FirstOrDefault(o => o.Title == name || o.ArticleId == name);
    }

    /// <summary>
    /// Gets all articles in this document.
    /// </summary>
    public IEnumerable<IArticle> Articles => ItemCollection.Items.OfType<Article>();

    /// <summary>
    /// Gets the total number of articles.
    /// </summary>
    public int ArticleCount => ItemCollection.Count;

    /// <summary>
    /// Removes an article from the document.
    /// </summary>
    /// <param name="article">The article to remove.</param>
    /// <returns>True if the article was removed successfully.</returns>
    public bool RemoveArticle(IArticle article) => ItemCollection.RemoveItem(article as Article);


    /// <summary>
    /// Clears all articles from the document.
    /// </summary>
    public void ClearArticles()
    {
        base.ItemCollection.Clear();
    }

    #endregion

    #region IArticle

    IArticleContainer IArticle.Parent => null;

    string IArticle.ArticleUrl
    {
        get
        {
            var asset = this.GetAsset();
            if (asset is null)
            {
                return string.Empty;
            }

            return $"article://{asset.AssetKey}";
        }
    }

    long IArticle.ArticleLength => 0;

    string IArticle.ArticleId { get => null; set { } }
    string IArticle.Title { get => Description ?? string.Empty; set => Description = value ?? string.Empty; }
    string IArticle.Overview { get => _overview.Text ?? string.Empty; set => _overview.Text = value ?? string.Empty; }
    string IArticle.Content { get => null; set { } }
    string IArticle.Type { get => null; set { } }
    string IArticle.Guide { get => null; set { } }
    string IArticle.Note { get => null; set { } }

    void IArticle.Commit()
    {
        MarkDirty(this);
        Entry.SaveDelayed();
    }

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

        return base.OnEditorGui(gui, pipeline, context);
    }

    #endregion

    #region Data Sync

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _readingMaterial.Sync(sync);
        _overview.Sync(sync);

        if (sync.Intent == SyncIntent.View)
        {
            sync.Sync("Title", Description, SyncFlag.GetOnly);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _readingMaterial.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.InspectorField(LabelValue.Empty, new ViewProperty("Article", "Article"));
        setup.InspectorFieldOf<string>(new ViewProperty("Title", "Title").WithReadOnly());
        _overview.InspectorField(setup);
    }

    #endregion

    #region Drop
    /// <inheritdoc/>
    protected override bool OnDropInCheck(SNamedRootCollection items, object value)
    {
        if (value is CommonFileInfo fileInfo)
        {
            string ext = Path.GetExtension(fileInfo.FilePath);
            if (ext.ToLowerInvariant() == ".md")
            {
                return true;
            }
        }

        return base.OnDropInCheck(items, value);
    }

    /// <inheritdoc/>
    protected override object OnDropInConvert(SNamedRootCollection items, object value)
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

                var article = new Article()
                {
                    Name = AllocateTaskId(),
                };

                rootNode.ApplyToArticle(article);

                return article;
            }
        }

        return base.OnDropInConvert(items, value);
    }
    #endregion

    /// <inheritdoc/>
    protected override Task<bool> OnGuiConfigNewItem(SNamedRootCollection items, INamedNode parentNode, NamedItem item)
    {
        if (item is Article article)
        {
            article.Name = AllocateTaskId();
            return Task.FromResult<bool>(true);
        }

        return base.OnGuiConfigNewItem(items, parentNode, item);
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedName(SNamedRootCollection items, string prefix, int digiLen = 2)
    {
        return AllocateTaskId();
    }

    /// <inheritdoc/>
    protected override string OnResolveConflictName(SNamedRootCollection items, string name)
    {
        return AllocateTaskId();
    }

    /// <summary>
    /// Generates a unique task ID for a new article.
    /// </summary>
    /// <returns>A unique article ID, or null if generation fails.</returns>
    public string AllocateTaskId()
    {
        for (int i = 0; i < 1000; i++)
        {
            string name = $"#Article-{IdGenerator.GenerateId(12)}";
            if (!ItemCollection.ContainsItem(name, true))
            {
                return name;
            }
        }

        return null;
    }
}


/// <summary>
/// Resolves article assets from article:// URIs.
/// </summary>
public class ArticleAssetRessolver : TypedAssetResolver<IArticleAsset>
{
    /// <inheritdoc/>
    protected override IArticleAsset OnResolveAsset(string anyKey)
    {
        anyKey = anyKey?.Trim();

        if (string.IsNullOrWhiteSpace(anyKey))
        {
            return null;
        }

        if (!anyKey.StartsWith("article://"))
        {
            return null;
        }

        anyKey = anyKey["article://".Length..];
        string assetKey = string.Empty;
        string path = string.Empty;

        int index = anyKey.IndexOf('?');
        if (index > 0)
        {
            assetKey = anyKey[..index];
            path = anyKey[(index + 1)..];
        }
        else
        {
            assetKey = anyKey;
        }

        if (string.IsNullOrWhiteSpace(assetKey))
        {
            return null;
        }

        var asset = AssetManager.Instance.GetAsset<IArticleAsset>(assetKey);
        if (asset is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return asset;
        }

        var paths = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        if (paths.Length == 0)
        {
            return asset;
        }

        var article = asset.GetArticle();
        if (article is null)
        {
            return null;
        }

        for (int i = 0; i < paths.Length && article != null; i++)
        {
            article = article.GetArticle(paths[i]);
        }

        return article?.TargetAsset as IArticleAsset;
    }
}