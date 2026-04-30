namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents the state of a virtual node operation result.
/// </summary>
public enum VOpState
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    OK,

    /// <summary>
    /// Multiple nodes are selected, operation not applicable.
    /// </summary>
    MultipleNodeSelected,

    /// <summary>
    /// The operation is not supported for the current context.
    /// </summary>
    NotSupported,

    /// <summary>
    /// A null reference was encountered.
    /// </summary>
    NullReference,

    /// <summary>
    /// Failed to create a new item.
    /// </summary>
    CreateNewItemFailed,
}