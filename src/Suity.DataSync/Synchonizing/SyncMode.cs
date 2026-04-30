namespace Suity.Synchonizing;

/// <summary>
/// Synchronization mode
/// </summary>
public enum SyncMode
{
    /// <summary>
    /// Initialization
    /// </summary>
    Initialize,

    /// <summary>
    /// Get element type
    /// </summary>
    RequestElementType,

    /// <summary>
    /// Get
    /// </summary>
    Get,

    /// <summary>
    /// Set
    /// </summary>
    Set,

    /// <summary>
    /// Get all
    /// </summary>
    GetAll,

    /// <summary>
    /// Set all
    /// </summary>
    SetAll,

    /// <summary>
    /// Insert to list
    /// </summary>
    Insert,

    /// <summary>
    /// Remove from list
    /// </summary>
    RemoveAt,

    /// <summary>
    /// Create new list item
    /// </summary>
    CreateNew,
}