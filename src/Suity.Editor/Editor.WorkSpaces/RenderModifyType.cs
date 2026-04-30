namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Represents the type of modification for rendered files
/// </summary>
public enum RenderModifyType
{
    /// <summary>
    /// No modification
    /// </summary>
    None,
    /// <summary>
    /// File added
    /// </summary>
    Add,
    /// <summary>
    /// File removed
    /// </summary>
    Remove,
    /// <summary>
    /// File modified
    /// </summary>
    Modify,
}