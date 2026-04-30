using Suity.Collections;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Extension methods for IArticle and IArticleContainer types.
/// </summary>
public static class ArticleExtensions
{
    /// <summary>
    /// Document type identifier for articles.
    /// </summary>
    public const string DOC_TYPE = "ArticleEdit";


    /// <summary>
    /// Gets the document containing this article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <returns>The article document, or null if not found.</returns>
    public static IArticleDocument GetArticlewDocument(this IArticle article)
    {
        while (article != null)
        {
            if (article is IArticleDocument doc)
            {
                return doc;
            }

            article = article.Parent as IArticle;
        }

        return null;
    }

    /// <summary>
    /// Gets an article from the container by path.
    /// </summary>
    /// <param name="container">The article container.</param>
    /// <param name="path">The path to the article.</param>
    /// <returns>The article, or null if not found.</returns>
    public static IArticle GetArticle(this IArticleContainer container, string[] path)
    {
        if (path is null || path.Length == 0)
        {
            return null;
        }

        IArticle article = container.GetArticle(path[0]);
        if (article is null)
        {
            return null;
        }

        for (int i = 1; i < path.Length; i++)
        {
            article = article.GetArticle(path[i]);
            if (article is null)
            {
                return null;
            }
        }

        return article;
    }

    /// <summary>
    /// Gets or adds an article from the container by path.
    /// </summary>
    /// <param name="container">The article container.</param>
    /// <param name="path">The path to the article.</param>
    /// <returns>The article, or null if creation failed.</returns>
    public static IArticle GetOrAddArticle(this IArticleContainer container, string[] path)
    {
        if (path is null || path.Length == 0)
        {
            return null;
        }

        IArticle article = container.GetOrAddArticle(path[0]);
        if (article is null)
        {
            return null;
        }

        for (int i = 1; i < path.Length; i++)
        {
            article = article.GetOrAddArticle(path[i]);
            if (article is null)
            {
                return null;
            }
        }

        return article;
    }

    /// <summary>
    /// Resolves an article from a parent by path.
    /// </summary>
    /// <param name="parent">The parent article.</param>
    /// <param name="path">The path to the article.</param>
    /// <param name="autoCreate">Whether to create articles if they don't exist.</param>
    /// <returns>The resolved article, or null if not found.</returns>
    public static IArticle ResolveArticle(this IArticle parent, string[] path, bool autoCreate = false)
    {
        IArticle article;

        if (autoCreate)
        {
            article = parent?.GetOrAddArticle(path.FirstOrDefault());
        }
        else
        {
            article = parent?.GetArticle(path.FirstOrDefault());
        }

        if (article is null)
        {
            return null;
        }

        foreach (var p in path.Skip(1))
        {
            if (autoCreate)
            {
                article = article?.GetOrAddArticle(p);
            }
            else
            {
                article = article?.GetArticle(p);
            }

            if (article is null)
            {
                return null;
            }
        }

        return article;
    }

    /// <summary>
    /// Gets the full text content of an article including child articles.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="depth">The depth for Markdown heading levels.</param>
    /// <returns>The full text content.</returns>
    public static string GetFullText(this IArticle article, int depth = 1)
    {
        var builder = new StringBuilder();
        article.BuildText(builder, depth);

        return builder.ToString();
    }

    /// <summary>
    /// Builds the text content into a StringBuilder.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="builder">The StringBuilder to append to.</param>
    /// <param name="depth">The depth for Markdown heading levels.</param>
    public static void BuildText(this IArticle article, StringBuilder builder, int depth = 1)
    {
        if (depth > 6)
        {
            depth = 6;
        }

        if (!string.IsNullOrWhiteSpace(article.Title))
        {
            string h = new string('#', depth);
            builder.Append(h);
            builder.Append(' ');
            builder.Append(article.Title);
            builder.AppendLine();
        }

        builder.AppendLine(article.Content);
        builder.AppendLine();

        depth++;
        foreach (var childNode in article.Articles)
        {
            BuildText(childNode, builder, depth);
        }
    }

    /// <summary>
    /// Gets the full hierarchical title of an article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <returns>The full title with parent titles.</returns>
    public static string GetFullTitle(this IArticle article)
    {
        var titles = new LinkedList<string>();

        while (article != null)
        {
            string title = article.Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = article.ArticleId?.Trim() ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                titles.AddFirst(title);
            }

            article = article.Parent as IArticle;
        }

        return string.Join("/", titles);
    }


    /// <summary>
    /// Gets the root node of the article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <returns>The root article.</returns>
    public static IArticle GetRoot(this IArticle article)
    {
        while (article?.Parent is IArticle parent)
        {
            article = parent;
        }

        return article;
    }

    /// <summary>
    /// Gets all parent hierarchy nodes of the article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <returns>List of articles from root to this article.</returns>
    public static List<IArticle> GetParentHierarchy(this IArticle article)
    {
        List<IArticle> nodes = [article];

        while (article?.Parent is IArticle parent)
        {
            article = parent;
            nodes.Add(article);
        }

        nodes.Reverse();

        return nodes;
    }

    /// <summary>
    /// Clone article properties from another article.
    /// </summary>
    /// <param name="article">The target article.</param>
    /// <param name="other">The source article.</param>
    /// <param name="title">Whether to clone the title.</param>
    /// <param name="overview">Whether to clone the overview.</param>
    /// <param name="content">Whether to clone the content.</param>
    public static void ClonePropertyFrom(this IArticle article, IArticle other, bool title, bool overview, bool content)
    {
        if (title)
        {
            article.Title = other.Title;
        }

        if (overview)
        {
            article.Overview = other.Overview;
        }

        if (content)
        {
            article.Content = other.Content;
        }
    }

    /// <summary>
    /// Gets the overview of the article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="notBlank">Requires non-blank or non-blank</param>
    /// <returns>The overview text.</returns>
    public static string GetOverview(this IArticle article, bool? notBlank = null)
    {
        StringBuilder builder = new StringBuilder();
        article.BuildOverview(builder, notBlank: notBlank);

        return builder.ToString();
    }

    /// <summary>
    /// Gets the deep overview of all articles in a container.
    /// </summary>
    /// <param name="container">The article container.</param>
    /// <param name="notBlank">Requires non-blank or non-blank.</param>
    /// <returns>The overview text.</returns>
    public static string GetOverviewDeep(this IArticleContainer container, bool? notBlank = null)
    {
        StringBuilder builder = new StringBuilder();
        container.BuildOverviewDeep(builder, notBlank: notBlank);

        return builder.ToString();
    }

    /// <summary>
    /// Gets the overview hierarchy of an article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="self">Whether to include the article itself.</param>
    /// <param name="fullContent">Whether to include full content.</param>
    /// <returns>The overview hierarchy.</returns>
    public static string GetOverviewHierarchy(this IArticle article, bool self, bool fullContent)
    {
        StringBuilder builder = new StringBuilder();
        article.BuildParentHierarchy(builder, self, fullContent);

        return builder.ToString();
    }

    /// <summary>
    /// Creates a new article silently without showing UI.
    /// </summary>
    /// <param name="newArticleName">The name of the new article.</param>
    /// <returns>The created article, or null if creation failed.</returns>
    public static IArticle SilentNewArticle(string newArticleName = "NewArticle")
    {
        var format = DocumentManager.Instance.GetDocumentFormat(DOC_TYPE);
        if (format is null)
        {
            return null;
        }

        string basePath = Project.Current.AssetDirectory;
        var docEntry = format.SilentCreate(basePath);
        if (docEntry is null)
        {
            return null;
        }

        var doc = docEntry.Content as IArticleDocument;
        if (doc is null)
        {
            return null;
        }

        var article = doc.GetOrAddArticle(newArticleName);

        docEntry.MarkDirty(typeof(ArticleExtensions));
        docEntry.Save();

        return article;
    }

    /// <summary>
    /// Creates a new article using a dialog.
    /// </summary>
    /// <param name="newArticleName">The name of the new article.</param>
    /// <returns>The created article, or null if creation failed.</returns>
    public static async Task<IArticle> DialogNewArticle(string newArticleName = "NewArticle")
    {
        var format = DocumentManager.Instance.GetDocumentFormat(DOC_TYPE);
        if (format is null)
        {
            return null;
        }

        string basePath = Project.Current.AssetDirectory;

        string fileName = await format.OpenCreationUI(basePath);
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        string fullPath = Path.Combine(basePath, fileName);

        if (File.Exists(fullPath))
        {
            await DialogUtility.ShowMessageBoxAsync("File already exists");
            return null;
        }

        DocumentEntry docEntry = DocumentManager.Instance.NewDocument(fullPath, format);
        if (docEntry is null)
        {
            return null;
        }

        var doc = docEntry.Content as IArticleDocument;
        if (doc is null)
        {
            return null;
        }

        var article = doc.GetOrAddArticle(newArticleName);

        docEntry.MarkDirty(typeof(ArticleExtensions));
        docEntry.Save();

        return article;
    }

    /// <summary>
    /// Creates a new article container silently.
    /// </summary>
    /// <returns>The new article container, or null if creation failed.</returns>
    public static IArticleContainer SilentNewArticleContainer()
    {
        var format = DocumentManager.Instance.GetDocumentFormat(DOC_TYPE);
        if (format is null)
        {
            return null;
        }

        string basePath = Project.Current.AssetDirectory;
        var docEntry = format.SilentCreate(basePath);
        if (docEntry is null)
        {
            return null;
        }

        return docEntry.Content as IArticleDocument;
    }

    /// <summary>
    /// Creates a new article container asset silently.
    /// </summary>
    /// <returns>The new article container asset, or null if creation failed.</returns>
    public static ArticleContainerAsset SilentNewArticleContainerAsset()
    {
        var format = DocumentManager.Instance.GetDocumentFormat(DOC_TYPE);
        if (format is null)
        {
            return null;
        }

        string basePath = Project.Current.AssetDirectory;
        var docEntry = format.SilentCreate(basePath);
        if (docEntry is null)
        {
            return null;
        }

        var doc = docEntry.Content as IArticleDocument;
        if (doc is null)
        {
            return null;
        }

        return doc.TargetAsset as ArticleContainerAsset;
    }

    /// <summary>
    /// Gets the root container for an asset by its ID.
    /// </summary>
    /// <param name="assetDataId">The asset data ID.</param>
    /// <param name="autoNewContainer">Whether to create a new container if not found.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <returns>The article container, or null.</returns>
    public static IArticleContainer GetRootContainer(string assetDataId, bool autoNewContainer, bool throwErr)
    {
        var asset = AssetManager.Instance.GetAssetByResourceName(assetDataId);
        if (asset is null && autoNewContainer)
        {
            asset = SilentNewArticleContainerAsset();
        }

        if (asset is null)
        {
            if (throwErr)
            {
                throw new NullReferenceException($"Asset not found : {assetDataId}");
            }
            else
            {
                return null;
            }
        }

        var container = asset.GetDocument<IArticleContainer>(true);
        if (container is null)
        {
            if (throwErr)
            {
                throw new NullReferenceException($"{nameof(IArticleContainer)} not found.");
            }
            else
            {
                return null;
            }
        }

        return container;
    }

    /// <summary>
    /// Gets an article by path within a container.
    /// </summary>
    /// <param name="container">The article container.</param>
    /// <param name="paths">The path array.</param>
    /// <param name="startIndex">The starting index.</param>
    /// <returns>The article, or null if not found.</returns>
    public static IArticle GetArticleByPath(this IArticleContainer container, string[] paths, int startIndex = 1)
    {
        var c = container;
        IArticle article = null;

        for (int i = startIndex; i < paths.Length; i++)
        {
            string path = Uri.UnescapeDataString(paths[i]);
            article = c.GetArticle(path);

            if (article is null)
            {
                return null;
            }

            c = article;
        }

        return article;
    }

    /// <summary>
    /// Gets an article by URL.
    /// </summary>
    /// <param name="url">The article URL.</param>
    /// <param name="autoNewContainer">Whether to create container if not found.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <returns>The article, or null.</returns>
    public static IArticle GetArticleByUrl(string url, bool autoNewContainer, bool throwErr)
    {
        var paths = ParseArticleUrl(url, throwErr);
        if (paths is null)
        {
            return null;
        }

        return GetArticleByUrl(paths, autoNewContainer, throwErr);
    }

    /// <summary>
    /// Gets an article by URL path array.
    /// </summary>
    /// <param name="paths">The path array.</param>
    /// <param name="autoNewContainer">Whether to create container if not found.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <returns>The article, or null.</returns>
    public static IArticle GetArticleByUrl(string[] paths, bool autoNewContainer, bool throwErr)
    {
        if (paths.Length == 0)
        {
            if (throwErr)
            {
                throw new ArgumentException(nameof(paths));
            }
            else
            {
                return null;
            }
        }

        var container = GetRootContainer(paths[0], autoNewContainer, throwErr);
        var article = container?.GetArticleByPath(paths, 1);

        return article;
    }

    /// <summary>
    /// Gets or creates an article by URL.
    /// </summary>
    /// <param name="url">The article URL.</param>
    /// <param name="autoNewConatainer">Whether to create if not found.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <param name="created">Output indicating if a new article was created.</param>
    /// <returns>The article, or null.</returns>
    public static IArticle GetOrCreateArticleByUrl(string url, bool autoNewConatainer, bool throwErr, out bool created)
    {
        var paths = ParseArticleUrl(url, throwErr) ?? [];
        if (paths.Length == 0 && throwErr)
        {
            throw new NullReferenceException($"Url is empty");
        }

        return GetOrCreateArticleByUrl(paths, autoNewConatainer, throwErr, out created);
    }

    /// <summary>
    /// Gets or creates an article by URL path array.
    /// </summary>
    /// <param name="paths">The path array.</param>
    /// <param name="autoNewConatainer">Whether to create if not found.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <param name="created">Output indicating if a new article was created.</param>
    /// <returns>The article, or null.</returns>
    public static IArticle GetOrCreateArticleByUrl(string[] paths, bool autoNewConatainer, bool throwErr, out bool created)
    {
        created = false;

        var dataId = paths.Length > 0 ? paths[0] : null;

        var container = GetRootContainer(dataId, autoNewConatainer, throwErr);
        var article = container?.GetArticleByPath(paths, 1);
        if (article != null)
        {
            return article;
        }

        if (container is null)
        {
            return null;
        }

        if (paths.Length == 1)
        {
            if (container is IArticle rootArticle)
            {
                return rootArticle;
            }
            else if (throwErr)
            {
                throw new InvalidOperationException($"Article title is not defined.");
            }
            else
            {
                return null;
            }
        }

        if (paths.Length > 1)
        {
            var c = container;
            foreach (var path in paths.Skip(1))
            {
                string title = Uri.UnescapeDataString(path);
                article = c.GetOrAddArticle(title);

                if (article is null)
                {
                    if (throwErr)
                    {
                        throw new NullReferenceException("Article create failed.");
                    }
                    else
                    {
                        return null;
                    }
                }

                created = true;
                c = article;
            }

            return article;
        }

        // Length == 0

        //article = container.GetOrAddArticle("Article01");
        //if (article is null)
        //{
        //    if (throwErr)
        //    {
        //        throw new NullReferenceException("Article create failed.");
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //created = true;

        return container as IArticle;
    }

    /// <summary>
    /// Iterates through an article and all its children recursively.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="action">The action to perform on each article.</param>
    public static void ForeachArticleDeep(this IArticle article, Action<IArticle> action)
    {
        action(article);

        foreach (var subArticle in article.Articles)
        {
            subArticle.ForeachArticleDeep(action);
        }
    }

    /// <summary>
    /// Parses an article URL into path segments.
    /// </summary>
    /// <param name="url">The URL to parse.</param>
    /// <param name="throwErr">Whether to throw an exception on error.</param>
    /// <returns>The path segments, or null.</returns>
    public static string[] ParseArticleUrl(string url, bool throwErr)
    {
        var parse = ResourceHelper.ParseUrl(url);
        if (parse.scheme != "article")
        {
            if (throwErr)
            {
                throw new InvalidOperationException("Url is not an article url.");
            }
            else
            {
                return null;
            }
        }

        var paths = parse.address.Split('/');
        if (paths.Length == 0)
        {
            if (throwErr)
            {
                throw new InvalidOperationException("Url address is empty.");
            }
            else
            {
                return null;
            }
        }

        return paths;
    }

    /// <summary>
    /// Builds the parent hierarchy as text.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="builder">The StringBuilder to append to.</param>
    /// <param name="self">Whether to include the article itself.</param>
    /// <param name="fullContent">Whether to include full content.</param>
    public static void BuildParentHierarchy(this IArticle article, StringBuilder builder, bool self, bool fullContent)
    {
        var list = article.GetParentHierarchy();
        if (!self)
        {
            list.RemoveAtSafe(list.Count - 1);
        }

        foreach (var item in list)
        {
            builder.AppendLine(item.ArticleUrl);
            builder.AppendLine(item.Title);

            if (!string.IsNullOrWhiteSpace(item.Overview))
            {
                builder.AppendLine($"  - {item.Overview}");
            }
            if (fullContent)
            {
                builder.AppendLine(item.Content);
            }

            builder.AppendLine();
        }
    }

    /// <summary>
    /// Builds the overview of an article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="builder">The StringBuilder to append to.</param>
    /// <param name="notBlank">Requires non-blank or non-blank.</param>
    public static void BuildOverview(this IArticle article, StringBuilder builder, bool? notBlank = null)
    {
        if (notBlank is { } nb)
        {
            if (nb)
            {
                if (string.IsNullOrWhiteSpace(article.Content))
                {
                    return;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(article.Content))
                {
                    return;
                }
            }
        }

        //builder.Append(new string(' ', indent * 2));
        builder.AppendLine($"[URL] {article.ArticleUrl}");
        builder.AppendLine($"[Title] {article.Title}");

        if (!string.IsNullOrWhiteSpace(article.Overview))
        {
            builder.AppendLine($"  - {article.Overview}");
        }
        builder.AppendLine();
    }

    /// <summary>
    /// Builds the deep overview of all articles in a container.
    /// </summary>
    /// <param name="container">The article container.</param>
    /// <param name="builder">The StringBuilder to append to.</param>
    /// <param name="indent">The indentation level.</param>
    /// <param name="notBlank">Requires non-blank or non-blank.</param>
    public static void BuildOverviewDeep(this IArticleContainer container, StringBuilder builder, int indent = 0, bool? notBlank = null)
    {
        int nextIndent = indent;

        if (container is IArticle node)
        {
            do
            {
                if (notBlank is { } nb)
                {
                    if (nb)
                    {
                        if (string.IsNullOrWhiteSpace(node.Content))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(node.Content))
                        {
                            break;
                        }
                    }
                }

                //builder.Append(new string(' ', indent * 2));
                builder.AppendLine(node.ArticleUrl);
                builder.AppendLine(node.Title);

                if (!string.IsNullOrWhiteSpace(node.Overview))
                {
                    builder.AppendLine($"  - {node.Overview}");
                }
                builder.AppendLine();

                nextIndent++;
            } while (false);
        }

        foreach (var childNode in container.Articles)
        {
            childNode.BuildOverviewDeep(builder, nextIndent, notBlank);
        }
    }
}