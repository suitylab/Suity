using Suity.Collections;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents;

/// <summary>
/// Provides a mechanism for selecting features by name and checking their enabled state.
/// </summary>
public interface IFeatureSelector
{
    /// <summary>
    /// Gets the collection of feature names.
    /// </summary>
    /// <returns>An enumerable of feature names.</returns>
    IEnumerable<string> GetFeatures();

    /// <summary>
    /// Gets whether a specific feature is enabled.
    /// </summary>
    /// <param name="name">The name of the feature.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    bool GetFeatureEnabled(string name);
}

/// <summary>
/// Represents a delegate that resolves an icon for a given article.
/// </summary>
/// <param name="article">The article to get an icon for.</param>
/// <returns>The icon image for the article.</returns>
public delegate Image ArticleIconGetter(IArticle article);


/// <summary>
/// Represents a collection of article view items, providing management and content retrieval capabilities.
/// </summary>
/// <typeparam name="T">The type of article view item, must derive from <see cref="ArticleViewItem"/>.</typeparam>
public abstract class ArticleViewCollection<T> : IEnumerable<T>, IViewObject
    where T : ArticleViewItem
{
    readonly IArticleResolver _owner;
    readonly Func<ArticleLocation> _locationGetter;
    readonly List<T> _items = [];
    readonly Dictionary<string, T> _terminalDict = [];
    readonly Dictionary<string, T> _propDict = [];


    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewCollection{T}"/> class.
    /// </summary>
    /// <param name="owner">The article resolver that owns this collection.</param>
    /// <param name="articleUsage">The usage type of the article.</param>
    /// <param name="path">The path segments for the article location.</param>
    public ArticleViewCollection(IArticleResolver owner, string articleUsage, params string[] path)
        : this(owner, new ArticleLocation(articleUsage, path))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewCollection{T}"/> class.
    /// </summary>
    /// <param name="owner">The article resolver that owns this collection.</param>
    /// <param name="location">The article location.</param>
    public ArticleViewCollection(IArticleResolver owner, ArticleLocation location)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        if (location is null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        _locationGetter = () => location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewCollection{T}"/> class with a dynamic location getter.
    /// </summary>
    /// <param name="owner">The article resolver that owns this collection.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    public ArticleViewCollection(IArticleResolver owner, Func<ArticleLocation> locationGetter)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _locationGetter = locationGetter ?? throw new ArgumentNullException(nameof(locationGetter));
    }

    /// <summary>
    /// Gets or sets the prefix used for property names when adding article references.
    /// </summary>
    public string PropertyPrefix { get; set; } = "ArticleRef_";

    /// <summary>
    /// Gets the article resolver that owns this collection.
    /// </summary>
    public IArticleResolver Owner => _owner;

    /// <summary>
    /// Gets the article location for this collection.
    /// </summary>
    public ArticleLocation Location => _locationGetter();

    /// <summary>
    /// Gets the article usage type.
    /// </summary>
    public string ArticleUsage => _locationGetter()?.ArticleUsage ?? string.Empty;

    /// <summary>
    /// Gets or sets the feature selector used to filter articles.
    /// </summary>
    public IFeatureSelector? Selector { get; set; }

    /// <summary>
    /// Gets or sets the default document title used when resolving articles.
    /// </summary>
    public string? DefaultDocumentTitle { get; set; }

    /// <summary>
    /// Gets or sets the icon resolver for articles in this collection.
    /// </summary>
    public ArticleIconGetter? IconResolve { get; set; }

    #region Collection

    /// <summary>
    /// Creates a new article reference item. Must be implemented in derived classes.
    /// </summary>
    /// <param name="propertyName">The property name for the item.</param>
    /// <param name="description">An optional description for the item.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    /// <returns>A new instance of T.</returns>
    public abstract T CreateRefItem(string propertyName, string? description, Func<ArticleLocation> locationGetter);

    /// <summary>
    /// Adds a new item with the property prefix prepended to the name.
    /// </summary>
    /// <param name="name">The base name for the property.</param>
    /// <param name="terminal">The terminal segment of the article path.</param>
    /// <returns>The newly added item.</returns>
    public T AddPrefix(string name, string terminal) => Add(PropertyPrefix + name, null, terminal);

    /// <summary>
    /// Adds a new item to the collection.
    /// </summary>
    /// <param name="propertyName">The property name for the item.</param>
    /// <param name="terminal">The terminal segment of the article path.</param>
    /// <returns>The newly added item.</returns>
    public T Add(string propertyName, string terminal) => Add(propertyName, null, terminal);

    /// <summary>
    /// Adds a new item to the collection with an optional description.
    /// </summary>
    /// <param name="propertyName">The property name for the item.</param>
    /// <param name="description">An optional description for the item.</param>
    /// <param name="terminal">The terminal segment of the article path.</param>
    /// <returns>The newly added item.</returns>
    public T Add(string propertyName, string? description, string terminal)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(L("Property name cannot be null or empty."), nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(terminal))
        {
            throw new ArgumentException(L("Terminal cannot be null or empty."), nameof(terminal));
        }

        var locationGetter = () => _locationGetter()?.Append(terminal);

        var item = CreateRefItem(propertyName, description, locationGetter)
            ?? throw new NullReferenceException();

        if (item.Owner != _owner)
        {
            throw new InvalidOperationException();
        }

        Add(item);

        return item;
    }

    /// <summary>
    /// Adds an existing item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="terminalCheck">If true, throws if the terminal already exists.</param>
    public void Add(T item, bool terminalCheck = false)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item.Owner != _owner)
        {
            throw new ArgumentException(L("Parent is not the same."), nameof(item));
        }

        string propertyName = item.Property.Name;
        string terminal = item.Terminal;
        if (string.IsNullOrWhiteSpace(terminal))
        {
            throw new ArgumentException(L("Terminal cannot be null or empty."), nameof(propertyName));
        }

        if (terminalCheck)
        {
            if (_terminalDict.ContainsKey(terminal))
            {
                throw new ArgumentException(L("Terminal already exists."), nameof(propertyName));
            }

            _items.Add(item);
            _terminalDict.Add(terminal, item);

            _propDict[propertyName] = item;
        }
        else
        {
            if (!_terminalDict.ContainsKey(terminal))
            {
                _items.Add(item);
                _terminalDict.Add(terminal, item);
            }

            _propDict[propertyName] = item;
        }
    }

    /// <summary>
    /// Gets an existing item for the article, or creates and adds a new one if not found.
    /// </summary>
    /// <param name="article">The article to get or add.</param>
    /// <returns>The existing or newly created item.</returns>
    public T GetOrAdd(IArticle article)
    {
        string title = article.Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = article.ArticleId;
        }

        return GetOrAdd(PropertyPrefix + article.ArticleId, title, title);
    }

    /// <summary>
    /// Gets an existing item by property name, or creates and adds a new one if not found.
    /// </summary>
    /// <param name="propertyName">The property name to look up.</param>
    /// <param name="description">The description for a new item.</param>
    /// <param name="terminal">The terminal for a new item.</param>
    /// <returns>The existing or newly created item.</returns>
    private T GetOrAdd(string propertyName, string description, string terminal)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(L("Property name cannot be null or empty."), nameof(propertyName));
        }

        if (terminal is null)
        {
            throw new ArgumentNullException(nameof(terminal));
        }

        var item = GetItemByProperty(propertyName);
        item ??= Add(propertyName, description, terminal);

        return item;
    }

    /// <summary>
    /// Ensures that items exist in the collection for all child articles.
    /// </summary>
    public void EnsureAllItems()
    {
        var article = GetArticle();
        if (article is null)
        {
            return;
        }

        foreach (var childArticle in article.Articles)
        {
            GetOrAdd(childArticle);
        }
    }


    /// <summary>
    /// Gets all valid items, ensuring that items exist for all child articles.
    /// </summary>
    public IEnumerable<T> ValidItems
    {
        get
        {
            var article = GetArticle();
            if (article is null)
            {
                return [];
            }

            foreach (var childArticle in article.Articles)
            {
                GetOrAdd(childArticle);
            }

            return article.Articles.Select(GetOrAdd);
        }
    }

    /// <summary>
    /// Gets valid items filtered by an optional predicate.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter articles.</param>
    /// <returns>An enumerable of valid items matching the filter.</returns>
    public IEnumerable<T> GetValidItems(Predicate<IArticle>? predicate = null)
    {
        var article = GetArticle();
        if (article is null)
        {
            return [];
        }

        foreach (var childArticle in article.Articles)
        {
            GetOrAdd(childArticle);
        }

        if (predicate != null)
        {
            return article.Articles.Where(o => predicate(o)).Select(GetOrAdd);
        }
        else
        {
            return article.Articles.Select(GetOrAdd);
        }
    }


    /// <summary>
    /// Gets an item by its terminal segment.
    /// </summary>
    /// <param name="terminal">The terminal segment to look up.</param>
    /// <returns>The item if found; otherwise, the default value for T.</returns>
    public T GetItemByTerminal(string terminal) => _terminalDict.GetValueSafe(terminal);

    /// <summary>
    /// Gets an item by its property name.
    /// </summary>
    /// <param name="propertyName">The property name to look up.</param>
    /// <returns>The item if found; otherwise, the default value for T.</returns>
    public T GetItemByProperty(string propertyName) => _propDict.GetValueSafe(propertyName);

    /// <summary>
    /// Removes an item from the collection by its terminal segment.
    /// </summary>
    /// <param name="terminal">The terminal segment of the item to remove.</param>
    /// <returns>True if the item was found and removed; otherwise, false.</returns>
    public bool RemoveItem(string terminal)
    {
        if (_terminalDict.TryGetValue(terminal, out var item))
        {
            _items.Remove(item);
            _terminalDict.Remove(terminal);
            _propDict.Remove(item.Property.Name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clears all items from the collection.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _terminalDict.Clear();
        _propDict.Clear();
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => _items.Count;

    #endregion

    #region Article
    /// <summary>
    /// Resolves the root article for this collection's usage type.
    /// </summary>
    /// <param name="autoCreate">Whether to automatically create the article if it doesn't exist.</param>
    /// <returns>The resolved article.</returns>
    public IArticle ResolveArticle(bool autoCreate = false)
        => _owner.ResolveArticle(ArticleUsage, autoCreate);

    /// <summary>
    /// Gets the article at this collection's location.
    /// </summary>
    /// <param name="autoCreate">Whether to automatically create the article if it doesn't exist.</param>
    /// <param name="docTitle">An optional document title.</param>
    /// <returns>The article if found; otherwise, null.</returns>
    public IArticle? GetArticle(bool autoCreate = false, string? docTitle = null) 
        => _locationGetter()?.ResolveArticle(_owner, autoCreate, docTitle ?? DefaultDocumentTitle);

    /// <summary>
    /// Gets or creates the article at this collection's location.
    /// </summary>
    /// <param name="docTitle">An optional document title.</param>
    /// <returns>The article if resolved or created; otherwise, null.</returns>
    public IArticle? GetOrCreateArticle(string? docTitle = null) 
        => _locationGetter()?.ResolveArticle(_owner, true, docTitle ?? DefaultDocumentTitle);

    /// <summary>
    /// Gets a value indicating whether the article has any child articles.
    /// </summary>
    public bool HasArticles => GetArticle()?.ArticleCount > 0;

    #endregion

    #region Get Content
    /// <summary>
    /// Gets a value indicating whether any article in the collection has empty content.
    /// </summary>
    /// <returns>True if any article has empty or whitespace-only content; otherwise, false.</returns>
    public bool HasEmptyArticle()
    {
        if (Selector is { } selector)
        {
            return selector.GetFeatures().Any(n => string.IsNullOrWhiteSpace(GetItemByTerminal(n)?.GetFieldText()));
        }
        else
        {
            return _items.Any(o => string.IsNullOrWhiteSpace(o.GetFieldText()));
        }
    }

    /// <summary>
    /// Gets a value indicating whether any article in the collection has non-empty content.
    /// </summary>
    /// <returns>True if any article has non-empty content; otherwise, false.</returns>
    public bool HasFilledArticle()
    {
        if (Selector is { } selector)
        {
            return selector.GetFeatures().Any(n => !string.IsNullOrWhiteSpace(GetItemByTerminal(n)?.GetFieldText()));
        }
        else
        {
            return _items.Any(o => !string.IsNullOrWhiteSpace(o.GetFieldText()));
        }
    }

    /// <summary>
    /// Gets the combined text content of all items formatted as XML tags, suitable for editing.
    /// </summary>
    /// <returns>A string containing all item content separated by double newlines.</returns>
    public string GetTextForEditing()
    {
        IEnumerable<string?> contents;

        if (Selector is { } selector)
        {
            contents = selector.GetFeatures().Select(n => GetItemByTerminal(n)?.GetTag());
        }
        else
        {
            contents = _items.Select(o => o.GetTag());
        }

        return string.Join("\n\n", contents);
    }

    /// <summary>
    /// Gets the combined text content of all items formatted for guiding purposes.
    /// </summary>
    /// <param name="additionalText">Optional additional text lines to append.</param>
    /// <returns>A string containing all item content and additional text.</returns>
    public string GetTextForGuiding(string[]? additionalText = null)
    {
        EnsureAllItems();

        StringBuilder builder = new();

        foreach (var article in _items)
        {
            AppendArticle(builder, article);
            builder.AppendLine();
        }

        if (additionalText?.Length > 0)
        {
            foreach (var text in additionalText)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                builder.AppendLine(text);
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the text content for a specific article by terminal name, formatted for guiding.
    /// </summary>
    /// <param name="name">The terminal name of the article to retrieve.</param>
    /// <param name="additionalText">Optional additional text lines to append.</param>
    /// <returns>A string containing the specified article's content and additional text, or the full guiding text if not found.</returns>
    public string GetTextForGuiding(string name, string[]? additionalText = null)
    {
        EnsureAllItems();

        StringBuilder builder = new();

        if (_items.Where(o => o.Terminal == name).FirstOrDefault() is not { } article)
        {
            // If the specified article is not found, return the whole text for guiding.
            return GetTextForGuiding(additionalText);
        }

        AppendArticle(builder, article);
        builder.AppendLine();

        if (additionalText?.Length > 0)
        {
            foreach (var text in additionalText)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                builder.AppendLine(text);
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the combined XML tags of all valid items.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all item XML tags separated by double newlines.</returns>
    public string GetTag(string tagName = "section", string titleAttr = "title", Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetTag(tagName, titleAttr)));
    }

    /// <summary>
    /// Gets the combined nullable XML tags of all valid items, excluding empty content.
    /// </summary>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all non-empty item XML tags separated by double newlines.</returns>
    public string GetTagNullable(Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetTagNullable()));
    }

    /// <summary>
    /// Gets the combined extended XML tags of all valid items with additional attributes.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <param name="overviewAttr">The name of the overview attribute.</param>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all item extended XML tags separated by double newlines.</returns>
    public string GetTagEx(string tagName = "section", string titleAttr = "title", string overviewAttr = "overview", Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetTagEx(tagName, titleAttr, overviewAttr)));
    }

    /// <summary>
    /// Gets the combined extended nullable XML tags of all valid items, excluding empty content.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <param name="overviewAttr">The name of the overview attribute.</param>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all non-empty item extended XML tags separated by double newlines.</returns>
    public string GetTagExNullable(string tagName = "section", string titleAttr = "title", string overviewAttr = "overview", Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetTagExNullable(tagName, titleAttr, overviewAttr)));
    }

    /// <summary>
    /// Gets the combined XML tags of items filtered by a comma or space-separated filter string.
    /// </summary>
    /// <param name="filter">A filter string containing property names separated by commas or spaces.</param>
    /// <returns>A string containing filtered item XML tags separated by double newlines.</returns>
    public string GetFilteredTag(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return GetTag();
        }

        HashSet<string> filters = [];
        filters.AddRange(filter.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries));

        var sections = ValidItems.Where(o => filters.Contains(o.Property.Name)).Select(o => o.GetTag());

        return string.Join("\n\n", sections);
    }

    /// <summary>
    /// Gets the combined plain text of all valid items with a markdown-style prefix.
    /// </summary>
    /// <param name="prefix">The prefix to prepend to each item (defaults to "#").</param>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all item plain text separated by double newlines.</returns>
    public string GetPlainText(string prefix = "#", Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetPlainText(prefix)));
    }

    /// <summary>
    /// Gets the combined nullable plain text of all valid items, excluding empty content.
    /// </summary>
    /// <param name="prefix">The prefix to prepend to each item (defaults to "#").</param>
    /// <param name="filter">An optional filter predicate for articles.</param>
    /// <returns>A string containing all non-empty item plain text separated by double newlines.</returns>
    public string GetPlainTextNullable(string prefix = "#", Predicate<IArticle>? filter = null)
    {
        return string.Join("\n\n", GetValidItems(filter).Select(o => o.GetPlainTextNullable(prefix)));
    }

    /// <summary>
    /// Appends an article's content to a string builder, handling feature enablement.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="article">The article view item to append.</param>
    private void AppendArticle(StringBuilder builder, T article)
    {
        string name = article.Terminal;
        string title = article.Property.DisplayName;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = name;
        }

        bool support = Selector?.GetFeatureEnabled(name) ?? true;
        if (support && article?.GetTag() is { } content && !string.IsNullOrWhiteSpace(content))
        {
            builder.AppendLine(content);
        }
        else
        {
            builder.AppendLine($"<section title='{name}'>\n{title} feature is disabled, please remove it from the template.\n</section>");
        }
    }
    #endregion

    /// <summary>
    /// Updates the collection by synchronizing item properties with their underlying articles.
    /// </summary>
    public void Update()
    {
        if (GetArticle() is { } articles)
        {
            var iconResolve = IconResolve;

            foreach (var article in articles.Articles)
            {
                var refItem = GetOrAdd(article);
                if (article.Overview is { } overview && !string.IsNullOrWhiteSpace(overview))
                {
                    refItem.Property.Description = article.Overview;
                }
                else
                {
                    refItem.Property.Description = article.Title;
                }

                refItem.Property.Icon = iconResolve?.Invoke(article) ?? refItem.Icon;
            }
        }
    }

    #region Data Sync

    /// <inheritdoc/>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.Intent == SyncIntent.View)
        {
            if (sync.Name?.StartsWith(PropertyPrefix) == true && GetItemByProperty(sync.Name) is { } item)
            {
                sync.Sync(item.Property.Name, item, SyncFlag.GetOnly);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void SetupView(IViewObjectSetup setup)
    {
        if (GetArticle() is { } articles)
        {
            var iconResolve = IconResolve;

            foreach (var article in articles.Articles)
            {
                var refItem = GetOrAdd(article);
                //if (article.Overview is { } overview && !string.IsNullOrWhiteSpace(overview))
                //{
                //    refItem.Property.Description = article.Overview;
                //}
                //else
                //{
                    refItem.Property.Description = article.Title;
                //}

                refItem.Property.Icon = iconResolve?.Invoke(article) ?? refItem.Icon;
                setup.InspectorField(refItem, refItem.Property);
            }
        }
        else
        {
            setup.Warning("Article is not connected.", 0, this.PropertyPrefix);
        }
    }

    #endregion

    #region Enumerator
    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_items).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }
    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        var article = GetArticle();
        if (article != null)
        {
            return L($"{article.ArticleCount} Items");
        }
        else
        {
            return L("Article not connected.");
        }
    }
}