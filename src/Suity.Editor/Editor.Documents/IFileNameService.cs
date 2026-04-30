using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Document dialog service
/// </summary>
public interface IFileNameService
{
    /// <summary>
    /// Opens document creation dialog.
    /// </summary>
    /// <param name="basePath">The base folder path.</param>
    /// <param name="defaultName">The default file name.</param>
    /// <param name="ext">The file extension.</param>
    /// <returns>Returns filename without basePath and with extension.</returns>
    Task<string> ShowCreateDocumentDialogAsync(string basePath, string defaultName, string ext);

    /// <summary>
    /// Gets auto-incremented available filename.
    /// </summary>
    /// <param name="basePath">The base folder path.</param>
    /// <param name="defaultName">The default file name.</param>
    /// <param name="ext">The file extension.</param>
    /// <returns>Returns filename without basePath and with extension.</returns>
    string GetIncrementalFileName(string basePath, string defaultName, string ext);

    /// <summary>
    /// Gets auto-incremented available folder name.
    /// </summary>
    /// <param name="basePath">The base folder path.</param>
    /// <param name="defaultName">The default folder name.</param>
    /// <returns>Returns folder name without basePath.</returns>
    string GetIncrementalFolderName(string basePath, string defaultName);
}