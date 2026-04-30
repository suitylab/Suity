namespace Suity.Synchonizing;

/// <summary>
/// Synchronization intent
/// </summary>
public enum SyncIntent
{
    /// <summary>
    /// None
    /// </summary>
    None,

    /// <summary>
    /// Data Serialization
    /// </summary>
    Serialize,

    /// <summary>
    /// View interactive access
    /// </summary>
    View,

    /// <summary>
    /// Data export
    /// </summary>
    DataExport,

    /// <summary>
    /// Data clone
    /// </summary>
    Clone,

    /// <summary>
    /// Data visit
    /// </summary>
    Visit,
}