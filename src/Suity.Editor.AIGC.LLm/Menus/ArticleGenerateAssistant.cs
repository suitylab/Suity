using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Documents;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Menus;

#region BaseArticleAssistant
/// <summary>
/// Base class for article-related AI assistants. Provides common functionality for accessing article content and parent articles.
/// </summary>
internal abstract class BaseArticleAssistant : AIAssistant
{
    /// <summary>
    /// Gets the article associated with this assistant.
    /// </summary>
    public IArticle Article { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseArticleAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to assist with.</param>
    protected BaseArticleAssistant(IArticle article)
    {
        Article = article ?? throw new ArgumentNullException(nameof(article));
    }

    /// <summary>
    /// Checks with the user whether to reference parent articles, and if so, returns the formatted text of all parent articles.
    /// </summary>
    /// <param name="request">The AI request context for user interaction.</param>
    /// <param name="article">The article whose parents to retrieve.</param>
    /// <returns>The formatted parent article text, or an empty string if the user declines or no parents exist.</returns>
    protected async Task<string> CheckGetParentArticleText(AIRequest request, IArticle article)
    {
        if (Article.Parent is not IArticle parent)
        {
            return string.Empty;
        }

        bool useParent = await request.ConversationYesNoButtons(L("Reference parent articles?"));
        if (!useParent)
        {
            return string.Empty;
        }

        var parents = GetAllParents(parent);
        if (parents.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var p in parents)
        {
            sb.AppendLine($"[{p.Title}]");
            sb.AppendLine(p.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets all parent articles of the specified article, ordered from immediate parent to root.
    /// </summary>
    /// <param name="article">The starting article.</param>
    /// <returns>A stack of parent articles, with the root at the bottom.</returns>
    protected Stack<IArticle> GetAllParents(IArticle article)
    {
        var parents = new Stack<IArticle>();

        parents.Push(article);

        while (article.Parent is IArticle parent)
        {
            parents.Push(parent);
            article = parent;
        }

        return parents;
    }

    /// <summary>
    /// Formats a guiding text string, returning a fallback value if the guiding text is empty or whitespace.
    /// </summary>
    /// <param name="guiding">The guiding text to format.</param>
    /// <param name="noGuiding">The fallback text to return when <paramref name="guiding"/> is empty. Defaults to "Not unavailable."</param>
    /// <returns>The guiding text if not empty, otherwise the fallback text.</returns>
    public static string FormatGuidingText(string guiding, string noGuiding = "Not unavailable.")
    {
        if (!string.IsNullOrWhiteSpace(guiding))
        {
            return guiding;
        }

        return noGuiding;
    }
}
#endregion

#region ArticleGenerateAssistant
/// <summary>
/// AI assistant that generates article content based on user prompts and optional parent article context.
/// </summary>
internal class ArticleGenerateAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleGenerateAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to generate content for.</param>
    public ArticleGenerateAssistant(IArticle article)
        : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        string userMsg = await request.ConversationInput(L("Please input the generation prompt."));
        if (string.IsNullOrWhiteSpace(userMsg))
        {
            return AICallResult.Empty;
        }

        var article = Article;

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.Generate");
        builder.Replace(TAG.TITLE, FormatGuidingText(article.Title));
        builder.Replace(TAG.PROMPT, FormatGuidingText(userMsg));
        builder.Replace(TAG.PARENT, FormatGuidingText(parentText));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Generate Article"),
        };

        var result = await call.Call(callReq);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        article.Content = result;
        article.Commit();

        return AICallResult.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Generate Article");
}
#endregion

#region ArticleOptimizeAssistant
/// <summary>
/// AI assistant that optimizes existing article content based on user prompts.
/// </summary>
internal class ArticleOptimizeAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleOptimizeAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to optimize.</param>
    public ArticleOptimizeAssistant(IArticle article)
    : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        string userMsg = await request.ConversationInput(L("Please input the optimazation prompt."));
        if (string.IsNullOrWhiteSpace(userMsg))
        {
            return AICallResult.Empty;
        }

        var article = Article;

        string content = $"[{article.Title}]\n{article.Content}";

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.Optimize");
        builder.Replace(TAG.CONTENT, FormatGuidingText(content));
        builder.Replace(TAG.PROMPT, FormatGuidingText(userMsg));
        builder.Replace(TAG.PARENT, FormatGuidingText(parentText));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Optimize Article"),
        };

        var result = await call.Call(callReq);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        article.Content = result;
        article.Commit();

        return AICallResult.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Optimize Article");
}
#endregion

#region ArticleSummarizeAssistant
/// <summary>
/// AI assistant that summarizes article content.
/// </summary>
internal class ArticleSummarizeAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleSummarizeAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to summarize.</param>
    public ArticleSummarizeAssistant(IArticle article)
        : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        var article = Article;

        string content = article.GetFullText();

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.Summarize");
        builder.Replace(TAG.CONTENT, FormatGuidingText(content));
        builder.Replace(TAG.PARENT, FormatGuidingText(parentText));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Summarize Article"),
        };

        var result = await call.Call(callReq);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        article.Content = result;
        article.Commit();

        return AICallResult.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Summarize Article");
}
#endregion

#region ArticleSubdivideAssistant
/// <summary>
/// AI assistant that subdivides an article into multiple sub-topics based on user prompts.
/// </summary>
internal class ArticleSubdivideAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleSubdivideAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to subdivide.</param>
    public ArticleSubdivideAssistant(IArticle article)
        : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        string userMsg = await request.ConversationInput(L("Please input the subdivision prompt."));
        if (string.IsNullOrWhiteSpace(userMsg))
        {
            return AICallResult.Empty;
        }

        var article = Article;

        string content = article.GetFullText();

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.Subdivide");
        builder.Replace(TAG.CONTENT, FormatGuidingText(content));
        builder.Replace(TAG.PROMPT, FormatGuidingText(userMsg));
        builder.Replace(TAG.PARENT, FormatGuidingText(parentText));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Subdivide Article"),
        };

        var result = await call.Call(callReq);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        var nodes = LooseXml.ExtractNodes(result, "topic");
        if (nodes == null || nodes.Length == 0)
        {
            return null;
        }

        article.ClearArticles();
        foreach (var node in nodes)
        {
            string topicTitle = node.GetAttribute("title") ?? string.Empty;
            string topicContent = node.InnerText ?? string.Empty;

            article.GetOrAddArticle(topicTitle).Content = topicContent;
        }

        article.Commit();

        return AICallResult.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Subdivide Article");
}
#endregion

#region ArticleSegmentAssistant
/// <summary>
/// AI assistant that segments an article into multiple sections with a summary.
/// </summary>
internal class ArticleSegmentAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleSegmentAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to segment.</param>
    public ArticleSegmentAssistant(IArticle article)
        : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        var article = Article;

        string content = article.GetFullText();

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.Segment");
        builder.Replace(TAG.CONTENT, FormatGuidingText(content));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Subdivide Article"),
        };

        var result = await call.Call(callReq);
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        var segs = LooseXml.ExtractNodes(result, "segment");
        if (segs == null || segs.Length == 0)
        {
            return null;
        }

        var summary = LooseXml.ExtractNodes(result, "summary")?.FirstOrDefault()?.InnerText ?? string.Empty;

        article.ClearArticles();
        foreach (var seg in segs)
        {
            string segTitle = seg.GetAttribute("title") ?? string.Empty;
            string segContent = seg.InnerText ?? string.Empty;

            article.GetOrAddArticle(segTitle).Content = segContent;
        }

        article.Content = summary;

        article.Commit();

        return AICallResult.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => L("Segment Article");
}
#endregion

#region ArticleAnswerQuestionAssistant
/// <summary>
/// AI assistant that answers questions based on article content.
/// </summary>
internal class ArticleAnswerQuestionAssistant : BaseArticleAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleAnswerQuestionAssistant"/> class.
    /// </summary>
    /// <param name="article">The article to answer questions about.</param>
    public ArticleAnswerQuestionAssistant(IArticle article)
        : base(article)
    {
    }

    /// <inheritdoc/>
    public async override Task<AICallResult> HandleRequest(AIRequest request)
    {
        string userMsg = await request.ConversationInput(L("INPUT_QUESTION"));
        if (string.IsNullOrWhiteSpace(userMsg))
        {
            return AICallResult.Empty;
        }

        var article = Article;

        string content = article.GetFullText();

        string parentText = await CheckGetParentArticleText(request, article);

        var builder = PromptBuilder.FromTemplate("Common.Article.AnswerQuestion");
        builder.Replace(TAG.CONTENT, FormatGuidingText(content));
        builder.Replace(TAG.PROMPT, FormatGuidingText(userMsg));
        builder.Replace(TAG.PARENT, FormatGuidingText(parentText));
        builder.Replace(TAG.SPEECH_LANGUAGE, request.GetSpeechLanguage());
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Answer question"),
        };

        string result = await call.Call(callReq);

        if (!string.IsNullOrWhiteSpace(result))
        {
            request.Conversation.AddSystemMessage(result);
        }

        return AICallResult.FromMessage(result);
    }

    /// <inheritdoc/>
    public override string ToString() => L("Subdivide Article");
}
#endregion
