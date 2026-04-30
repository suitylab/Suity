namespace Suity.Editor.Documents;

#region IArticleUsageResolver

/// <summary>
/// Interface for resolving articles from their usage identifier.
/// </summary>
public interface IArticleResolver
{
    /// <summary>
    /// Resolves an article by its usage identifier.
    /// </summary>
    /// <param name="usage">The usage identifier of the article.</param>
    /// <param name="autoCreate">Whether to create the article if it doesn't exist.</param>
    /// <param name="docTitle">Optional title for a new document.</param>
    /// <returns>The resolved article, or null if not found and autoCreate is false.</returns>
    IArticle ResolveArticle(string usage, bool autoCreate, string docTitle = null);
}

#endregion

#region ArticleFields

/// <summary>
/// Represents different fields of an article.
/// </summary>
public enum ArticleFields
{
    /// <summary>
    /// The main content field.
    /// </summary>
    [DisplayText("Content")]
    Content,

    /// <summary>
    /// The overview field.
    /// </summary>
    [DisplayText("Overview")]
    Overview,

    /// <summary>
    /// The writing guide field.
    /// </summary>
    [DisplayText("Writing Guide")]
    Guide,

    /// <summary>
    /// The note field.
    /// </summary>
    [DisplayText("Note")]
    Note,
}

#endregion

#region ArticleLocation

/// <summary>
/// Represents the location of an article within a container, including usage, path, and field.
/// </summary>
public class ArticleLocation
{
    /// <summary>
    /// Gets the usage identifier of the article container.
    /// </summary>
    public string ArticleUsage { get; }

    /// <summary>
    /// Gets the field of the article.
    /// </summary>
    public ArticleFields Field { get; }

    /// <summary>
    /// Gets the path to the article within the container.
    /// </summary>
    public string[] Path { get; }

    /// <summary>
    /// Gets the terminal (last element) of the path.
    /// </summary>
    public string Terminal { get; }


    /// <summary>
    /// Initializes a new instance of ArticleLocation with just a usage identifier.
    /// </summary>
    /// <param name="articleUsage">The usage identifier.</param>
    public ArticleLocation(string articleUsage)
        : this(articleUsage, [], ArticleFields.Content)
    {
    }


    /// <summary>
    /// Initializes a new instance of ArticleLocation with usage and path.
    /// </summary>
    /// <param name="articleUsage">The usage identifier.</param>
    /// <param name="path">The path to the article.</param>
    public ArticleLocation(string articleUsage, string[] path)
        : this(articleUsage, path, ArticleFields.Content)
    {
    }

    /// <summary>
    /// Initializes a new instance of ArticleLocation with usage and field.
    /// </summary>
    /// <param name="articleUsage">The usage identifier.</param>
    /// <param name="field">The article field.</param>
    public ArticleLocation(string articleUsage, ArticleFields field)
        : this(articleUsage, [], field)
    {
    }

    /// <summary>
    /// Initializes a new instance of ArticleLocation with full parameters.
    /// </summary>
    /// <param name="articleUsage">The usage identifier.</param>
    /// <param name="path">The path to the article.</param>
    /// <param name="field">The article field.</param>
    public ArticleLocation(string articleUsage, string[] path, ArticleFields field)
    {
        if (string.IsNullOrWhiteSpace(articleUsage))
        {
            articleUsage = string.Empty;
        }

        //if (path.Any(string.IsNullOrWhiteSpace))
        //{
        //    throw new ArgumentException("Path contains empty string.", nameof(path));
        //}

        ArticleUsage = articleUsage;
        Field = field;

        path ??= [];
        Path = [.. path];

        if (path.Length > 0)
        {
            Terminal = path[^1];
        }
    }


    /// <summary>
    /// Appends additional path elements to this location.
    /// </summary>
    /// <param name="path">The path elements to append.</param>
    /// <returns>A new ArticleLocation with the appended path.</returns>
    public ArticleLocation Append(params string[] path)
    {
        if (path is null || path.Length == 0)
        {
            return this;
        }

        string[] newPath = [.. Path, .. path];

        return new ArticleLocation(ArticleUsage, newPath, Field);
    }

    /// <summary>
    /// Creates a new location with a different field.
    /// </summary>
    /// <param name="field">The new field.</param>
    /// <returns>A new ArticleLocation with the changed field.</returns>
    public ArticleLocation ChangeField(ArticleFields field)
    {
        return new ArticleLocation(ArticleUsage, [.. Path], field);
    }

    /// <summary>
    /// Resolves the article using a resolver.
    /// </summary>
    /// <param name="resolver">The article resolver.</param>
    /// <param name="autoCreate">Whether to create the article if it doesn't exist.</param>
    /// <param name="docTitle">Optional title for a new document.</param>
    /// <returns>The resolved article, or null if resolution failed.</returns>
    public IArticle ResolveArticle(IArticleResolver resolver, bool autoCreate = false, string docTitle = null)
    {
        if (resolver is null)
        {
            return null;
        }

        var doc = resolver.ResolveArticle(ArticleUsage, autoCreate, docTitle);

        return ResolveArticle(doc, autoCreate);
    }

    /// <summary>
    /// Resolves the article from a parent article.
    /// </summary>
    /// <param name="article">The parent article to resolve from.</param>
    /// <param name="autoCreate">Whether to create the article if it doesn't exist.</param>
    /// <returns>The resolved article, or null if not found.</returns>
    public IArticle ResolveArticle(IArticle article, bool autoCreate = false)
    {
        if (Path.Length > 0)
        {
            return article.ResolveArticle(Path, autoCreate);
        }
        else
        {
            return article;
        }
    }

}

#endregion
