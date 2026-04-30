using Suity.Editor.Flows;

namespace Suity.Editor.Documents;

/// <summary>
/// Extension methods for Document types.
/// </summary>
public static class DocumentExtensions
{
    /// <summary>
    /// Logs an info message with a clickable link to the document.
    /// </summary>
    /// <param name="doc">The document to log.</param>
    /// <param name="message">The info message.</param>
    public static void LogInfo(this FlowDocument doc, string message)
    {
        var location = doc.FileName;

        Logs.LogInfo(new ActionLogItem(message, () =>
        {
            var doc = DocumentManager.Instance.OpenDocument(location);
            doc?.ShowView();
        }));
    }

    /// <summary>
    /// Logs a warning message with a clickable link to the document.
    /// </summary>
    /// <param name="doc">The document to log.</param>
    /// <param name="message">The warning message.</param>
    public static void LogWarning(this FlowDocument doc, string message)
    {
        var location = doc.FileName;

        Logs.LogWarning(new ActionLogItem(message, () =>
        {
            var doc = DocumentManager.Instance.OpenDocument(location);
            doc?.ShowView();
        }));
    }

    /// <summary>
    /// Logs an error message with a clickable link to the document.
    /// </summary>
    /// <param name="doc">The document to log.</param>
    /// <param name="message">The error message.</param>
    public static void LogError(this FlowDocument doc, string message)
    {
        var location = doc.FileName;

        Logs.LogError(new ActionLogItem(message, () =>
        {
            var doc = DocumentManager.Instance.OpenDocument(location);
            doc?.ShowView();
        }));
    }

}
