using Suity.Editor.Documents;
using Suity.Views;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.AIGC.Menus;

/// <summary>
/// Menu command that provides AIGC operations for articles, including generate, optimize, summarize, subdivide, segment, and answer question.
/// </summary>
[InsertInto("#ArticleEdit")]
public class ArticleEditMenuCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleEditMenuCommand"/> class with all AIGC sub-commands.
    /// </summary>
    public ArticleEditMenuCommand()
        : base("AIGC", CoreIconCache.Prompt)
    {
        this.AddCommand("Generate", typeof(IArticle), CoreIconCache.Article, true, _ => HandleGenerate());
        this.AddCommand("Optimize", typeof(IArticle), CoreIconCache.Edit, true, _ => HandleOptimize());
        this.AddCommand("Summarize", typeof(IArticle), CoreIconCache.Concept, true, _ => HandleSummarize());
        this.AddCommand("Subdivide", typeof(IArticle), CoreIconCache.Item, true, _ => HandleSubdivide());
        this.AddCommand("Segment", typeof(IArticle), CoreIconCache.List, true, _ => HandleSegment());
        this.AddCommand("Answer question", typeof(IArticle), CoreIconCache.Item, true, _ => HandleAnswerQuestion());
    }

    /// <summary>
    /// Handles the generate command by opening a chat with the <see cref="ArticleGenerateAssistant"/>.
    /// </summary>
    private void HandleGenerate()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("Generate", new ArticleGenerateAssistant(article));
    }

    /// <summary>
    /// Handles the optimize command by opening a chat with the <see cref="ArticleOptimizeAssistant"/>.
    /// </summary>
    private void HandleOptimize()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("Optimize", new ArticleOptimizeAssistant(article));
    }

    /// <summary>
    /// Handles the summarize command by opening a chat with the <see cref="ArticleSummarizeAssistant"/>.
    /// </summary>
    private void HandleSummarize()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("Summarize", new ArticleSummarizeAssistant(article));
    }

    /// <summary>
    /// Handles the subdivide command by opening a chat with the <see cref="ArticleSubdivideAssistant"/>.
    /// </summary>
    private void HandleSubdivide()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("Subdivide", new ArticleSubdivideAssistant(article));
    }

    /// <summary>
    /// Handles the segment command by opening a chat with the <see cref="ArticleSegmentAssistant"/>.
    /// </summary>
    private void HandleSegment()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("Segment", new ArticleSegmentAssistant(article));
    }

    /// <summary>
    /// Handles the answer question command by opening a chat with the <see cref="ArticleAnswerQuestionAssistant"/>.
    /// </summary>
    private void HandleAnswerQuestion()
    {
        if (Selection?.FirstOrDefault() is not IArticle article)
        {
            return;
        }

        LLmService.Instance.InputMainChat("AnswerQuestion", new ArticleAnswerQuestionAssistant(article));
    }
}
