using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents;


#region IArticleContainer
/// <summary>
/// Interface for a container that manages articles.
/// </summary>
public interface IArticleContainer
{
    /// <summary>
    /// Retrieves an existing article with the specified title or adds a new one if not found.
    /// </summary>
    /// <param name="title">The title of the article to retrieve or add.</param>
    /// <returns>The article with the specified title.</returns>
    IArticle GetOrAddArticle(string title);

    /// <summary>
    /// Gets an article by its title or ID.
    /// </summary>
    /// <param name="titleOrId">The title or ID of the article to retrieve.</param>
    /// <returns>The article with the specified title or ID.</returns>
    IArticle GetArticle(string titleOrId);

    /// <summary>
    /// Gets all articles in the container.
    /// </summary>
    IEnumerable<IArticle> Articles { get; }

    /// <summary>
    /// Gets the number of articles in the container.
    /// </summary>
    int ArticleCount { get; }

    /// <summary>
    /// Removes the specified article from the container.
    /// </summary>
    /// <param name="article">The article to remove.</param>
    /// <returns>True if the article was successfully removed; otherwise, false.</returns>
    bool RemoveArticle(IArticle article);

    /// <summary>
    /// Removes all articles from the container.
    /// </summary>
    void ClearArticles();
}
#endregion

#region IArticle
/// <summary>
/// Interface representing an AI article with native type attributes.
/// </summary>
[NativeType(Name = "Article", Description = "Article", CodeBase = "*AIGC", Icon = "*CoreIcon|Article", Color = ArticleAsset.ArticleColorCode)]
public interface IArticle : IArticleContainer, IHasId, IHasAsset
{
    /// <summary>
    /// Gets the parent container of this article.
    /// </summary>
    IArticleContainer Parent { get; }

    /// <summary>
    /// Gets the URL of the article.
    /// </summary>
    string ArticleUrl { get; }

    /// <summary>
    /// Gets the length of the article.
    /// </summary>
    long ArticleLength { get; }

    /// <summary>
    /// Gets or sets the ID of the article.
    /// </summary>
    string ArticleId { get; set; }

    /// <summary>
    /// Gets or sets the title of the article.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets or sets the overview of the article.
    /// </summary>
    string Overview { get; set; }

    /// <summary>
    /// Gets or sets the content of the article.
    /// </summary>
    string Content { get; set; }

    /// <summary>
    /// Gets or sets the type of the article.
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// Gets or sets the guide information for the article.
    /// </summary>
    string Guide { get; set; }

    /// <summary>
    /// Gets or sets the notes for the article.
    /// </summary>
    string Note { get; set; }

    /// <summary>
    /// Gets or sets the reading material flag for the article.
    /// </summary>
    bool IsReadingMaterial { get; set; }

    /// <summary>
    /// Commits the changes made to the article.
    /// </summary>
    void Commit();
}

#endregion

#region IArticleDocument
/// <summary>
/// Interface representing an article document that extends IArticle and includes member container functionality.
/// </summary>
public interface IArticleDocument : IArticle, IMemberContainer
{
    /// <summary>
    /// Gets or sets the description of the article document.
    /// </summary>
    string Description { get; set; }
}
#endregion

#region IArticleAsset

// [DisplayText("Article Asset", "*CoreIcon|Article")]
[NativeType(CodeBase = "Suity", Description = "Article Asset", Icon = "*CoreIcon|Article", Color = "#5C7CFA")]
/// <summary>
/// Interface representing an article asset with attribute design capabilities.
/// </summary>
public interface IArticleAsset : IHasAttributeDesign, IHasId
{
    /// <summary>
    /// Gets the title of the article, optionally including parent titles.
    /// </summary>
    /// <param name="inHierarchy">Whether to include parent titles in hierarchy.</param>
    /// <returns>The article title.</returns>
    string GetTitle(bool inHierarchy);

    /// <summary>
    /// Gets the overview text of the article.
    /// </summary>
    /// <returns>The overview text.</returns>
    string GetOverview();

    /// <summary>
    /// Gets the content text of the article.
    /// </summary>
    /// <returns>The content text.</returns>
    string GetContentText();

    /// <summary>
    /// Returns the full text content, optionally including a Markdown-formatted title at the specified heading level.
    /// </summary>
    /// <param name="markdownTitle">The heading level to use for the Markdown title. Must be a positive integer. The default is 1, which corresponds
    /// to a top-level heading.</param>
    /// <returns>A string containing the full text, formatted with a Markdown title if specified.</returns>
    string GetFullText(int markdownTitle = 1);

    /// <summary>
    /// Gets a value indicating whether the item is considered reading material.
    /// </summary>
    bool ReadingMaterial { get; }

    /// <summary>
    /// Gets the article associated with this asset.
    /// </summary>
    /// <param name="tryLoadStorage">Whether to try loading from storage if not loaded.</param>
    /// <returns>The article, or null if not found.</returns>
    IArticle GetArticle(bool tryLoadStorage = true);
}
#endregion


#region ArticleAssetBuilder
/// <summary>
/// Builder for creating ArticleAsset instances.
/// </summary>
public class ArticleAssetBuilder : AssetBuilder<ArticleAsset>, IDesignBuilder
{
    private bool _readingMaterial = false;
    private IAttributeDesign _attribute = EmptyAttributeDesign.Empty;

    public ArticleAssetBuilder()
    {
        AddAutoUpdate(nameof(ArticleAsset.ReadingMaterial), v => v.ReadingMaterial = _readingMaterial);
        AddAutoUpdate(nameof(DType.Attributes), o => o.UpdateAttributes(_attribute, false));
    }

    public void SetBindingInfo(object bindingInfo)
    {
    }

    public void SetReadingMaterial(bool readingMaterial)
    {
        _readingMaterial = readingMaterial;
        TryUpdateNow(d => d.ReadingMaterial = _readingMaterial);
    }

    public void UpdateAttributes(IAttributeDesign attributes)
    {
        _attribute = attributes ?? EmptyAttributeDesign.Empty;
        TryUpdateNow(d => d.UpdateAttributes(_attribute, true));
    }
}
#endregion

#region ArticleAsset
/// <summary>
/// Asset type representing an article.
/// </summary>
public class ArticleAsset : Asset, IArticleAsset
{
    /// <summary>
    /// The background color code for articles.
    /// </summary>
    public const string ArticleBgColorCode = "#4B6AE5"; //"#5C7CFA"; //"#486788";

    /// <summary>
    /// The color code for articles.
    /// </summary>
    public const string ArticleColorCode = "#3854C6"; //"#4E82B5";
    
    /// <summary>
    /// Gets the background color for articles.
    /// </summary>
    public static Color ArticleBgColor { get; } = ColorTranslator.FromHtml(ArticleBgColorCode);

    /// <summary>
    /// Gets the article type definition.
    /// </summary>
    public static TypeDefinition ArticleType => TypeDefinition.FromNative<IArticle>();


    internal IAttributeDesign _attributes = EmptyAttributeDesign.Empty;

    public ArticleAsset()
    {
        UpdateAssetTypes(typeof(IArticleAsset));
    }

    public IArticle GetArticle(bool tryLoadStorage = true)
        => GetStorageObject(tryLoadStorage) as IArticle;

    public bool ReadingMaterial { get; internal set; }

    // Display full title name in project view.
    public override string NameInTreeView => this.Description;

    /// <summary>
    /// Gets or sets the attribute design for the article.
    /// </summary>
    public IAttributeDesign Attributes
    {
        get => _attributes;
        internal protected set
        {
            if (ReferenceEquals(_attributes, value))
            {
                return;
            }

            UpdateAttributes(value, true);
        }
    }

    internal void UpdateAttributes(IAttributeDesign value, bool notify)
    {
        _attributes = value ?? EmptyAttributeDesign.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }


    public string GetTitle(bool inHierarchy)
    {
        if (inHierarchy)
        {
            return GetArticle(true)?.GetFullTitle() ?? string.Empty;
        }
        else 
        {
            return GetArticle(true)?.Title ?? string.Empty;
        }
    }

    public string GetOverview() => GetArticle(true)?.Overview ?? string.Empty;

    public string GetContentText() => GetArticle(true)?.Content ?? string.Empty;

    public string GetFullText(int depth = 1)
    {
        return GetArticle(true)?.GetFullText(depth) ?? string.Empty;
    }
}
#endregion

#region AigcArticleGroupAssetBuilder
/// <summary>
/// Builder for creating ArticleContainerAsset instances.
/// </summary>
public class AigcArticleGroupAssetBuilder : GroupAssetBuilder<ArticleContainerAsset>, IDesignBuilder
{
    private bool _readingMaterial = false;
    private IAttributeDesign _attribute = EmptyAttributeDesign.Empty;

    public AigcArticleGroupAssetBuilder()
    {
        AddAutoUpdate(nameof(ArticleAsset.ReadingMaterial), v => v.ReadingMaterial = _readingMaterial);
        AddAutoUpdate(nameof(DType.Attributes), o => o.UpdateAttributes(_attribute, false));
    }

    public void SetBindingInfo(object bindingInfo)
    {
    }

    public void SetReadingMaterial(bool readingMaterial)
    {
        _readingMaterial = readingMaterial;
        TryUpdateNow(d => d.ReadingMaterial = _readingMaterial);
    }

    public void UpdateAttributes(IAttributeDesign attributes)
    {
        _attribute = attributes ?? EmptyAttributeDesign.Empty;
        TryUpdateNow(d => d.UpdateAttributes(_attribute, true));
    }
}
#endregion

#region ArticleContainerAsset
/// <summary>
/// Asset type representing a group of articles (container).
/// </summary>
[DisplayText("Article Group", "*CoreIcon|Book")]
[NativeType(CodeBase = "Suity", Description = "Article Group", Color = "#5C7CFA")]
public class ArticleContainerAsset : GroupAsset, IArticleAsset, ITextAsset
{
    internal IAttributeDesign _attributes = EmptyAttributeDesign.Empty;

    public ArticleContainerAsset()
    {
        UpdateAssetTypes(typeof(IArticleAsset), typeof(ITextAsset));
    }

    public IArticle GetArticle(bool tryLoadStorage = true) => this.GetDocument<IArticleDocument>(tryLoadStorage);

    public IArticleDocument GetDocument(bool tryLoadStorage = true) => this.GetDocument<IArticleDocument>(tryLoadStorage);

    public bool ReadingMaterial { get; internal set; }

    public override bool CanExportToLibrary => true;

    public IAttributeDesign Attributes
    {
        get => _attributes;
        internal protected set
        {
            if (ReferenceEquals(_attributes, value))
            {
                return;
            }

            UpdateAttributes(value, true);
        }
    }

    internal void UpdateAttributes(IAttributeDesign value, bool notify)
    {
        _attributes = value ?? EmptyAttributeDesign.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }

    public string GetText() => GetFullText(1);

    public string GetTitle(bool inHierarchy)
    {
        if (GetDocument()?.Description is { } desc && !string.IsNullOrWhiteSpace(desc))
        {
            return desc;
        }
        else
        {
            return this.LocalName;
        }
    }

    public string GetOverview() => string.Empty;

    public string GetContentText() => string.Empty;

    public string GetFullText(int depth = 1)
    {
        var doc = GetDocument();
        if (doc is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var article in doc.Articles)
        {
            article.BuildText(builder, depth);
            builder.AppendLine();
        }

        return builder.ToString();
    }

}
#endregion


#region ArticleSnapshotAction
/// <summary>
/// Undo/redo action for article snapshot operations.
/// </summary>
public class ArticleSnapshotAction : RawSnapshotUndoAction
{
    private readonly IArticle _article;

    public ArticleSnapshotAction(IArticle article, Action refresh = null)
        : base(article, postViewAction: refresh)
    {
        _article = article ?? throw new ArgumentNullException(nameof(article));
    }

    public override string Name => L("Edit: ") + _article.Title;

    public override void Do()
    {
        base.Do();

        _article.Commit();
    }

    public override void Undo()
    {
        base.Undo();

        _article.Commit();
    }
}

#endregion

#region ReadingMaterialFilter
/// <summary>
/// Filter for identifying reading material assets.
/// </summary>
public class ReadingMaterialFilter : IAssetFilter
{
    public static ReadingMaterialFilter Instance { get; } = new();

    public bool FilterAsset(Asset asset)
    {
        //return asset is IArticleAsset articleAsset && articleAsset.Attributes.GetAttribute<ReadingMaterialAttribute>() != null;
        return asset is IArticleAsset { ReadingMaterial: true };
    }
}

#endregion
