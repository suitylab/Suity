using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents;

/// <summary>
/// Represents a view item for an article field, providing access to article content and GUI interactions.
/// </summary>
public abstract class ArticleViewItem : IViewObject, IDrawEditorImGui
{
    readonly IArticleResolver _owner;
    readonly ViewProperty _property;
    readonly Func<ArticleLocation> _locationGetter;


    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewItem"/> class.
    /// </summary>
    /// <param name="owner">The article resolver that owns this item.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="location">The article location.</param>
    public ArticleViewItem(IArticleResolver owner, string propertyName, ArticleLocation location)
        : this(owner, propertyName, null, location)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewItem"/> class.
    /// </summary>
    /// <param name="owner">The article resolver that owns this item.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="description">An optional description for the property.</param>
    /// <param name="location">The article location.</param>
    public ArticleViewItem(IArticleResolver owner, string propertyName, string? description, ArticleLocation location)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(L("Property name cannot be null or empty."), nameof(propertyName));
        }

        if (location is null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            int lastIndex = location.Path.Length - 1;
            description = location.Path[lastIndex];
        }

        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _property = new ViewProperty(propertyName, description);

        _locationGetter = () => location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleViewItem"/> class with a dynamic location getter.
    /// </summary>
    /// <param name="owner">The article resolver that owns this item.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="description">An optional description for the property.</param>
    /// <param name="locationGetter">A function that retrieves the article location.</param>
    protected ArticleViewItem(IArticleResolver owner, string propertyName, string? description, Func<ArticleLocation> locationGetter)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(L("Property name cannot be null or empty."), nameof(propertyName));
        }

        if (locationGetter is null)
        {
            throw new ArgumentNullException(nameof(locationGetter));
        }

        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _property = new ViewProperty(propertyName, description);

        _locationGetter = locationGetter;
    }

    /// <summary>
    /// Gets the article resolver that owns this item.
    /// </summary>
    public IArticleResolver Owner => _owner;

    /// <summary>
    /// Gets the view property associated with this item.
    /// </summary>
    public ViewProperty Property => _property;

    /// <summary>
    /// Gets the article location for this item.
    /// </summary>
    public ArticleLocation Location => _locationGetter();

    /// <summary>
    /// Gets the terminal segment of the article path.
    /// </summary>
    public string Terminal => _locationGetter()?.Terminal ?? string.Empty;

    /// <summary>
    /// Gets the icon associated with this article item.
    /// </summary>
    public virtual Image Icon => CoreIconCache.Article;

    #region Data Sync

    /// <inheritdoc/>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
        string content = GetFieldText();

        var textBlock = sync.Sync("Content", new TextBlock(content));

        if (sync.IsSetterOf("Content"))
        {
            SetFieldText(textBlock?.Text ?? string.Empty);
        }
    }

    /// <inheritdoc/>
    public virtual void SetupView(IViewObjectSetup setup)
    {
        setup.InspectorFieldOf<TextBlock>(new ViewProperty("Content", "Content"));
    }

    #endregion

    #region Article & Content

    /// <summary>
    /// Gets the article at this item's location.
    /// </summary>
    /// <param name="autoCreate">Whether to automatically create the article if it doesn't exist.</param>
    /// <param name="docTitle">An optional document title.</param>
    /// <returns>The article at this location, or null if not found.</returns>
    public IArticle GetArticle(bool autoCreate = false, string? docTitle = null)
        => _locationGetter()?.ResolveArticle(_owner, autoCreate, docTitle);

    /// <summary>
    /// Gets a value indicating whether this item has an associated article.
    /// </summary>
    public bool HasArticle => GetArticle() != null;

    /// <summary>
    /// Resolves the unique identifier of the associated article.
    /// </summary>
    /// <returns>The article ID if available, otherwise null.</returns>
    public Guid? ResolveId()
    {
        var id = (GetArticle() as IHasId)?.Id;

        return id;
    }

    /// <summary>
    /// Gets the text content of the field specified by the article location.
    /// </summary>
    /// <returns>The field text, or an empty string if no article is found.</returns>
    public string GetFieldText()
    {
        if (GetArticle() is not { } article)
        {
            return string.Empty;
        }

        var location = _locationGetter();

        switch (location?.Field)
        {
            case ArticleFields.Content:
                return article.Content;

            case ArticleFields.Overview:
                return article.Overview;

            case ArticleFields.Guide:
                return article.Guide;

            case ArticleFields.Note:
                return article.Note;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Sets the text content of the field specified by the article location.
    /// </summary>
    /// <param name="text">The text to set.</param>
    /// <param name="commit">Whether to commit the changes after setting.</param>
    /// <param name="docTitle">An optional document title.</param>
    public void SetFieldText(string text, bool commit = true, string? docTitle = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            text = string.Empty;
        }

        if (GetArticle(true, docTitle) is not { } article)
        {
            return;
        }

        bool fieldSet = false;

        switch (_locationGetter()?.Field)
        {
            case ArticleFields.Content:
                article.Content = text;
                fieldSet = true;
                break;

            case ArticleFields.Overview:
                article.Overview = text;
                fieldSet = true;
                break;

            case ArticleFields.Guide:
                article.Guide = text;
                fieldSet = true;
                break;

            case ArticleFields.Note:
                article.Note = text;
                fieldSet = true;
                break;
        }

        if (fieldSet && commit)
        {
            article.Commit();
        }
    }


    /// <summary>
    /// Gets the full content text of the article with a depth of 2.
    /// </summary>
    /// <returns>The full text content, or an empty string if no article is found.</returns>
    public string GetFullContent() => GetArticle()?.GetFullText(2) ?? string.Empty;

    /// <summary>
    /// Gets the field text wrapped in an XML tag.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <returns>An XML string containing the field text.</returns>
    public string GetTag(string tagName = "section", string titleAttr = "title")
    {
        return $"<{tagName} {titleAttr}='{Terminal}'>\n{GetFieldText()}\n</{tagName}>";
    }

    /// <summary>
    /// Gets the field text wrapped in an XML tag, or null if the content is empty.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <returns>An XML string containing the field text, or null if empty.</returns>
    public string? GetTagNullable(string tagName = "section", string titleAttr = "title")
    {
        var content = GetFieldText();
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return GetTag(tagName, titleAttr);
    }

    /// <summary>
    /// Gets the field text wrapped in an extended XML tag with additional attributes.
    /// If the field is Content, the guide text is prepended.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <param name="overviewAttr">The name of the overview attribute.</param>
    /// <returns>An extended XML string containing the field text.</returns>
    public string GetTagEx(string tagName = "section", string titleAttr = "title", string overviewAttr = "overview")
    {
        string content = GetFieldText();

        if (_locationGetter()?.Field == ArticleFields.Content)
        {
            string? guide = GetArticle()?.Guide;
            if (!string.IsNullOrWhiteSpace(guide))
            {
                content = $"{guide}\n\n{content}";
            }
        }

        return $"<{tagName} {titleAttr}='{Terminal}' {overviewAttr}='{GetArticle()?.Overview}'>\n{content}\n</{tagName}>";
    }

    /// <summary>
    /// Gets the extended XML tag, or null if the content is empty.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="titleAttr">The name of the title attribute.</param>
    /// <param name="overviewAttr">The name of the overview attribute.</param>
    /// <returns>An extended XML string containing the field text, or null if empty.</returns>
    public string? GetTagExNullable(string tagName = "section", string titleAttr = "title", string overviewAttr = "overview")
    {
        var content = GetFieldText();
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return GetTagEx(tagName, titleAttr, overviewAttr);
    }

    /// <summary>
    /// Gets the field text as plain text with a markdown-style prefix.
    /// </summary>
    /// <param name="prefix">The prefix to prepend (defaults to "#").</param>
    /// <returns>A plain text string with the prefix and terminal.</returns>
    public string GetPlainText(string prefix = "#")
    {
        return $"{prefix} {Terminal}\n{GetFieldText()}";
    }

    /// <summary>
    /// Gets the field text as plain text with a prefix, or null if the content is empty.
    /// </summary>
    /// <param name="prefix">The prefix to prepend (defaults to "#").</param>
    /// <returns>A plain text string with the prefix and terminal, or null if empty.</returns>
    public string? GetPlainTextNullable(string prefix = "#")
    {
        var content = GetFieldText();
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return $"{prefix} {Terminal}\n{content}";
    }

    /// <summary>
    /// Commits the specified content directly to the article's Content field.
    /// </summary>
    /// <param name="content">The content to commit.</param>
    /// <returns>True if the content was committed successfully; otherwise, false.</returns>
    public bool CommitContent(string content)
    {
        var article = GetArticle(true);

        if (article != null)
        {
            article.Content = content;
            article.Commit();

            return true;
        }

        return false;
    }



    #endregion

    #region Gui

    /// <inheritdoc/>
    public virtual bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        // Parent check is required here, otherwise it is impossible to accurately determine if it is selected.
        if (pipeline == EditorImGuiPipeline.Option && gui.CurrentNode?.Parent?.GetIsPropertyFieldSelected() == true)
        {
            OnButtonGui(gui);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Renders the action buttons for this item in the GUI.
    /// </summary>
    /// <param name="gui">The ImGui instance to render with.</param>
    protected virtual void OnButtonGui(ImGui gui)
    {
        gui.Button("#locate", CoreIconCache.GotoDefination)
        .InitClass("configBtn")
        .SetToolTipsL("Locate the article in the editor.")
        .OnClick(() =>
        {
            if (ResolveId() is Guid id)
            {
                EditorUtility.NavigateTo(id);
            }
        });

/*        gui.Button("#copy", CoreIconCache.Copy)
        .InitClass("configBtn")
        .SetToolTipsL("Copy title to clipboard.")
        .OnClick(() =>
        {
            try
            {
                //EditorUtility.SetSystemClipboardText(Property.Name);
                EditorUtility.SetSystemClipboardText(Terminal).ContinueWith(t => 
                {
                    if (t.Result)
                    {
                        DialogUtility.ShowMessageBoxAsync(L("Copied to clipboard: ") + Terminal);
                    }
                });
            }
            catch (Exception)
            {
                DialogUtility.ShowMessageBoxAsyncL("Failed to copy to clipboard.");
            }
        });

        OnRemoveButtonGui(gui);*/
    }

    /// <summary>
    /// Renders the remove button for this item in the GUI.
    /// </summary>
    /// <param name="gui">The ImGui instance to render with.</param>
    protected virtual void OnRemoveButtonGui(ImGui gui)
    {
        gui.Button("#remove", CoreIconCache.Delete)
        .InitClass("configBtn")
        .SetToolTipsL("Remove this item")
        .OnClick(async () =>
        {
            if (GetArticle() is not { } article)
            {
                return;
            }

            if (article.Parent is not IArticle parentArticle)
            {
                return;
            }

            bool confirm = await DialogUtility.ShowYesNoDialogAsyncL("Remove this item?");
            if (!confirm)
            {
                return;
            }

            var action = new ArticleSnapshotAction(parentArticle);

            parentArticle.RemoveArticle(article);

            DoAction(action);
        });
    }

    #endregion


    /// <summary>
    /// Executes an undo/redo action. Must be overridden in derived classes.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    protected abstract void DoAction(UndoRedoAction action);

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetTruncatedText(GetFieldText());
    }

    /// <summary>
    /// Truncates the specified text to the given length, appending "..." if truncated.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="length">The maximum length before truncation (defaults to 30).</param>
    /// <returns>The truncated text, or an empty string if the input is null or whitespace.</returns>
    public static string GetTruncatedText(string text, int length = 30)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (text.Length > length)
        {
            return text.Substring(0, length) + "...";
        }

        return text;
    }
}