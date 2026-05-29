using Suity.Editor.AIGC;
using Suity.Editor.Documents;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using System;
using System.IO;

namespace Suity.Editor.Flows.SubFlows.Running;

/// <summary>
/// Provides utility extension methods for <see cref="IAigcWorkflowPage"/> operations
/// such as file access, workspace retrieval, and scratch pad management.
/// </summary>
public static class SubFlowUtility
{
    public static IAigcTaskPage GetParentTask(this IPageInstance instance)
    {
        var page = instance?.Owner as IAigcTaskPage;

        return page?.ParentTask;
    }

    /// <summary>
    /// Determines whether the specified file exists at the given path within the page's workspace.
    /// </summary>
    /// <param name="page">The AIGC workflow page.</param>
    /// <param name="path">The file path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public static bool GetFileExist(this IAigcWorkflowPage page, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var workSpace = GetWorkSpace(page);
        if (workSpace is null)
        {
            return false;
        }

        string fullPath = PathUtility.MakeFullPath(path, workSpace.MasterDirectory);

        return File.Exists(fullPath);
    }

    /// <summary>
    /// Reads all text content from the specified file within the page's workspace.
    /// </summary>
    /// <param name="page">The AIGC workflow page.</param>
    /// <param name="path">The file path to read.</param>
    /// <returns>The file content as a string, or <c>null</c> if the file does not exist or cannot be read.</returns>
    public static string ReadAllText(this IAigcWorkflowPage page, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var workSpace = GetWorkSpace(page);
        if (workSpace is null)
        {
            return null;
        }

        string fullPath = PathUtility.MakeFullPath(path, workSpace.MasterDirectory);

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the <see cref="WorkSpace"/> associated with the specified AIGC workflow page.
    /// </summary>
    /// <param name="page">The AIGC workflow page.</param>
    /// <returns>The workspace, or <c>null</c> if not available.</returns>
    public static WorkSpace GetWorkSpace(this IAigcWorkflowPage page)
    {
        var workSpace = page?.TaskHost?.WorkSpace;

        return workSpace;
    }

    /// <summary>
    /// Gets or creates a scratch pad container on the specified page.
    /// </summary>
    /// <param name="page">The AIGC workflow page.</param>
    /// <param name="name">The name of the scratch pad container. Defaults to "ScratchPad".</param>
    /// <param name="autoCreate">Whether to automatically create the container if it does not exist. Defaults to <c>true</c>.</param>
    /// <returns>The scratch pad container.</returns>
    public static IArticle GetScratchPadContainer(this IAigcWorkflowPage page, string name = "ScratchPad", bool autoCreate = true)
    {
        return page?.ResolveArticleBase(autoCreate)?.GetOrAddArticle(name);
    }

    public static void WriteFileToScratchPad(this IAigcWorkflowPage page, string path)
    {
        var article = GetScratchPadContainer(page);
        if (article is null)
        {
            return;
        }

        WriteFileToScratchPad(page, article, path);
    }

    /// <summary>
    /// Writes a file from the page's workspace to an article within the scratch pad.
    /// If the file does not exist, any existing article with that path is removed.
    /// </summary>
    /// <param name="page">The AIGC workflow page.</param>
    /// <param name="article">The article to write the file into.</param>
    /// <param name="path">The file path within the workspace to write.</param>
    public static void WriteFileToScratchPad(this IAigcWorkflowPage page, IArticle article, string path)
    {
        if (article is null)
        {
            return;
        }

        path = path?.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        bool fileExist = GetFileExist(page, path);
        if (!fileExist)
        {
            var remove = article.GetArticle(path);
            if (remove != null)
            {
                article.RemoveArticle(remove);
            }
            return;
        }

        string content = ReadAllText(page, path);
        SetScratchPad(article, "file", path, content, "full content");
    }

    public static void SetScratchPad(this IAigcWorkflowPage page, string type, string path, string content, string note)
    {
        var article = GetScratchPadContainer(page);
        if (article is null)
        {
            return;
        }

        SetScratchPad(article, type, path, content, note);
    }

    public static void SetScratchPad(this IArticle article, string type, string path, string content, string note)
    {
        if (article is null)
        {
            return;
        }

        path = path?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var fileArticle = article.GetOrAddArticle(path);
        fileArticle.Content = content ?? string.Empty;
        fileArticle.Title = path;
        fileArticle.Type = type;
        fileArticle.Note = note;

        article.Commit();
    }

    public static bool RemoveScratchPad(this IAigcWorkflowPage page, string path)
    {
        var article = page.GetScratchPadContainer(autoCreate: false);
        return RemoveScratchPad(article, path);
    }

    public static bool RemoveScratchPad(this IArticle article, string path)
    {
        if (article is null)
        {
            return false;
        }

        path = path?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var remove = article.GetOrAddArticle(path);
        remove.Content = string.Empty;
        remove.Note = "removed";

        return false;
    }
}
