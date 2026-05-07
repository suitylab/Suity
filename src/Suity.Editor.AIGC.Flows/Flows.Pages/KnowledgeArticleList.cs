using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Represents a single knowledge article item within a knowledge article list,
/// containing its identifier, title, overview, and underlying article asset.
/// </summary>
public class KnowledgeArticleItem
{
    /// <summary>
    /// Gets the unique identifier for this article item.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the title of the article.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the overview or summary text of the article.
    /// </summary>
    public string Overview { get; init; }

    /// <summary>
    /// Gets the underlying article asset associated with this item.
    /// </summary>
    public IArticleAsset Article { get; init; }
}

/// <summary>
/// Represents a collection of knowledge articles, providing indexed and named access
/// to individual article items. Automatically collects nested child articles
/// and filters out articles without content.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Knowledge Article List", Icon = "*CoreIcon|Knowledge", Color = "#5C7CFA")]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.KnowledgeArticleList")]
[NativeAlias("Suity.Editor.AIGC.Flows.KnowledgeArticleList")]
public class KnowledgeArticleList
{
    /// <summary>
    /// Gets an empty instance of <see cref="KnowledgeArticleList"/>.
    /// </summary>
    public static KnowledgeArticleList Empty { get; } = new KnowledgeArticleList();

    private readonly IArticleAsset[] _assets;

    private readonly List<KnowledgeArticleItem> _list = [];

    /// <summary>
    /// Initializes a new empty instance of the <see cref="KnowledgeArticleList"/> class.
    /// This constructor is internal and used primarily for creating the <see cref="Empty"/> instance.
    /// </summary>
    internal KnowledgeArticleList()
    {
        _assets = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeArticleList"/> class
    /// with the specified collection of article assets. Recursively collects all nested
    /// child articles that contain content.
    /// </summary>
    /// <param name="assets">The collection of article assets to include.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="assets"/> is null.</exception>
    public KnowledgeArticleList(IEnumerable<IArticleAsset> assets)
    {
        _assets = assets?.ToArray()
            ?? throw new ArgumentNullException(nameof(assets));

        List<IArticleAsset> list = [];
        HashSet<IArticleAsset> visited = [];

        foreach (var asset in assets)
        {
            CollectArticle(asset, list, visited);
        }

        for (int i = 0; i < list.Count; i++)
        {
            var assetItem = list[i];
            string title = assetItem.GetTitle(true) ?? string.Empty;
            string overview = assetItem.GetOverview() ?? string.Empty;

            var knowledgeItem = new KnowledgeArticleItem
            {
                Id = $"K{i}",
                Title = title,
                Overview = overview,
                Article = assetItem,
            };
            _list.Add(knowledgeItem);
        }
    }

    /// <summary>
    /// Gets the total number of article items in this list.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the article item at the specified zero-based index.
    /// Returns null if the index is out of range.
    /// </summary>
    /// <param name="index">The zero-based index of the article item to retrieve.</param>
    /// <returns>The <see cref="KnowledgeArticleItem"/> at the specified index, or null if out of range.</returns>
    public KnowledgeArticleItem GetItemAt(int index) => _list.GetListItemSafe(index);

    /// <summary>
    /// Gets the article item with the specified identifier.
    /// Identifiers are expected to be in the format "K" followed by an integer index.
    /// </summary>
    /// <param name="id">The identifier of the article item to retrieve.</param>
    /// <returns>The <see cref="KnowledgeArticleItem"/> with the specified identifier, or null if not found.</returns>
    public KnowledgeArticleItem GetItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (!id.StartsWith("K"))
        {
            return null;
        }

        id = id[1..];
        if (!int.TryParse(id, out int index))
        {
            return null;
        }

        return GetItemAt(index);
    }

    /// <summary>
    /// Gets an enumerable collection of all article items in this list.
    /// </summary>
    public IEnumerable<KnowledgeArticleItem> Items => _list.Pass();


    private static void CollectArticle(IArticleAsset asset, List<IArticleAsset> list, HashSet<IArticleAsset> visited)
    {
        if (asset is null)
        {
            return;
        }

        if (!visited.Add(asset))
        {
            return;
        }

        // Only articles with content are added to the list.
        if (!string.IsNullOrWhiteSpace(asset.GetContentText()))
        {
            list.Add(asset);
        }

        if (asset.GetArticle() is { } article)
        {
            foreach (var childArticle in article.Articles)
            {
                if (childArticle?.TargetAsset is ArticleAsset childArticleAsset)
                {
                    CollectArticle(childArticleAsset, list, visited);
                }
            }
        }
    }


}

/// <summary>
/// Converts an array of <see cref="IArticleAsset"/> instances into a <see cref="KnowledgeArticleList"/>.
/// </summary>
public class ArticlesToKnowledgeListConverter : ITypeDefinitionConverter
{
    /// <inheritdoc/>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromNative<IArticleAsset>().MakeArrayType()];

    /// <inheritdoc/>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromNative<KnowledgeArticleList>()];

    /// <inheritdoc/>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        if (objFrom is not string && objFrom is System.Collections.IEnumerable collection)
        {
            List<IArticleAsset> list = [];
            foreach (var item in collection)
            {
                var asset = SItem.ResolveValue(item) as IArticleAsset;
                if (asset != null)
                {
                    list.Add(asset);
                }
            }

            return new KnowledgeArticleList(list);
        }
        else
        {
            return KnowledgeArticleList.Empty;
        }
    }
}

/// <summary>
/// Converts a <see cref="KnowledgeArticleList"/> into a formatted text representation
/// with XML-like article tags containing title and overview information.
/// </summary>
public class KnowledgeListToTextConverter : TypeToTextConverter<KnowledgeArticleList>
{
    /// <inheritdoc/>
    public override string Convert(KnowledgeArticleList objFrom)
    {
        var builder = new StringBuilder();

        foreach (var item in objFrom.Items)
        {
            builder.AppendLine($"<article id='{item.Id}'>");
            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                builder.Append("- Title: ");
                builder.AppendLine(item.Title);
            }
            if (!string.IsNullOrWhiteSpace(item.Overview))
            {
                builder.AppendLine("- Overview:");
                builder.AppendLine(item.Overview);
            }
            builder.AppendLine("</article>");
        }

        return builder.ToString();
    }
}
