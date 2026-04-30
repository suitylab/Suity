using Suity.Editor.CodeRender.Replacing;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Service interface for handling user code conversion operations.
/// </summary>
public interface IUserCodeService
{
    /// <summary>
    /// Shows the user code conversion interface for a segment document.
    /// </summary>
    /// <param name="dbFileName">The database file name.</param>
    /// <param name="basePath">The base path for the code library.</param>
    /// <param name="fileId">The file identifier.</param>
    /// <param name="collection">The segment document collection.</param>
    void ShowUserCodeConvert(string dbFileName, string basePath, string fileId, SegmentDocument collection);
}