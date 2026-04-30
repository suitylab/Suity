namespace Suity.Editor.CodeRender;

/// <summary>
/// Render status enumeration.
/// </summary>
public enum RenderStatus
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// Success.
    /// </summary>
    Success,

    /// <summary>
    /// Content is identical.
    /// </summary>
    Same,

    /// <summary>
    /// Message.
    /// </summary>
    Message,

    /// <summary>
    /// File exists.
    /// </summary>
    FileExist,

    /// <summary>
    /// File does not exist.
    /// </summary>
    FileNotExist,

    /// <summary>
    /// File is stale.
    /// </summary>
    FileLegacy,

    /// <summary>
    /// File is stale in database.
    /// </summary>
    ContainsDbLegacy,

    /// <summary>
    /// File updated in database.
    /// </summary>
    SameAndDbUpdated,

    /// <summary>
    /// File in other protocol.
    /// </summary>
    FileInOtherProtocol,

    /// <summary>
    /// User file.
    /// </summary>
    UserFile,

    /// <summary>
    /// File mismatch.
    /// </summary>
    FileMismatch,

    /// <summary>
    /// Legacy user code.
    /// </summary>
    LagecyUserCode,

    /// <summary>
    /// Error interrupt.
    /// </summary>
    ErrorInterrupt,

    /// <summary>
    /// Error continue.
    /// </summary>
    ErrorContinue,
}