namespace Suity.Editor.CodeRender;

/// <summary>
/// File state enumeration.
/// </summary>
public enum FileState
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// User file.
    /// </summary>
    User,

    /// <summary>
    /// Add.
    /// </summary>
    Add,

    /// <summary>
    /// Update.
    /// </summary>
    Update,

    /// <summary>
    /// Exists.
    /// </summary>
    Exist,

    /// <summary>
    /// Remove.
    /// </summary>
    Remove,

    /// <summary>
    /// Duplicated.
    /// </summary>
    Duplicated,

    /// <summary>
    /// User occupied.
    /// </summary>
    UserOccupied,

    /// <summary>
    /// Modified.
    /// </summary>
    Modified,

    /// <summary>
    /// Warning.
    /// </summary>
    Warning,
}