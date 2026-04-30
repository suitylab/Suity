using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Menu;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Menus;

/// <summary>
/// Menu command to import article content from a markdown or text file into an <see cref="IArticle"/>.
/// </summary>
[InsertInto("#ArticleEdit")]
public class ImportArticleMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportArticleMenu"/> class.
    /// </summary>
    public ImportArticleMenu()
        : base("Import", CoreIconCache.Import)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticle);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        string initFolder = EditorServices.CurrentProject.AssetDirectory;
        string fileName = await DialogUtility.ShowOpenFileAsync("md|*.md|txt|*.txt|*|*.*", initFolder);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        string text = TextFileHelper.ReadFile(fileName);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var rootNode = MarkdownParser.ParseMarkdownToTree(text);
        if (rootNode is null)
        {
            return;
        }

        rootNode.ApplyToArticle(article);
    }
}

/// <summary>
/// Menu command to import article content from a markdown or text file into an <see cref="IArticleDocument"/>.
/// </summary>
[InsertInto("#ArticleEdit")]
public class ImportArticleDocMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportArticleDocMenu"/> class.
    /// </summary>
    public ImportArticleDocMenu()
        : base("Import", CoreIconCache.Import)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticleDocument);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticleDocument article)
        {
            return;
        }

        string initFolder = EditorServices.CurrentProject.AssetDirectory;
        string fileName = await DialogUtility.ShowOpenFileAsync("md|*.md|txt|*.txt|*|*.*", initFolder);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        string text = TextFileHelper.ReadFile(fileName);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var rootNode = MarkdownParser.ParseMarkdownToTree(text);
        if (rootNode is null)
        {
            return;
        }

        rootNode.ApplyToArticle(article);
    }
}

/// <summary>
/// Menu command to export an <see cref="IArticle"/> to a markdown file.
/// </summary>
[InsertInto("#ArticleEdit")]
public class ExportArticleMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportArticleMenu"/> class.
    /// </summary>
    public ExportArticleMenu()
        : base("Export", CoreIconCache.Export)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticle);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        var fullPath = await DialogUtility.ShowExportFileNameDialogAsync(article.ArticleId, ".md");
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        string text = article.GetFullText();

        TextFileHelper.WriteFile(fullPath, text);

        QueuedAction.Do(() =>
        {
            EditorUtility.LocateInPublishView(fullPath);
        });
    }
}

/// <summary>
/// Menu command to export an <see cref="IArticleDocument"/> to a markdown file.
/// </summary>
[InsertInto("#ArticleEdit")]
public class ExportArticleDocMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportArticleDocMenu"/> class.
    /// </summary>
    public ExportArticleDocMenu()
        : base("Export", CoreIconCache.Export)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticleDocument);
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticleDocument doc)
        {
            return;
        }

        if (doc is not Document doc2)
        {
            return;
        }

        string name = Path.GetFileNameWithoutExtension(doc2.FileName.FullPath);
        var fullPath = await DialogUtility.ShowExportFileNameDialogAsync(name, ".md");
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        string text = doc.GetFullText();

        TextFileHelper.WriteFile(fullPath, text);

        QueuedAction.Do(() =>
        {
            EditorUtility.LocateInPublishView(fullPath);
        });
    }
}


/// <summary>
/// Menu command to preview the full text content of an <see cref="IArticle"/>.
/// </summary>
[InsertInto("#ArticleEdit")]
public class PreviewArticleMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewArticleMenu"/> class.
    /// </summary>
    public PreviewArticleMenu()
        : base("Preview", CoreIconCache.Preview)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticle);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        string text = article.GetFullText() ?? string.Empty;
        EditorUtility.ShowText(article.Title ?? L("Article"), text);
    }
}

/// <summary>
/// Menu command to preview the full text content of an <see cref="IArticleDocument"/>.
/// </summary>
[InsertInto("#ArticleEdit")]
public class PreviewArticleDocMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewArticleDocMenu"/> class.
    /// </summary>
    public PreviewArticleDocMenu()
        : base("Preview", CoreIconCache.Preview)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(IArticleDocument);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IArticleDocument doc)
        {
            return;
        }

        if (doc is not Document doc2)
        {
            return;
        }

        string text = doc.GetFullText();
        string title = Path.GetFileNameWithoutExtension(doc2.FileName.PhysicFileName);
        EditorUtility.ShowText(title ?? L("Article"), text);
    }
}
