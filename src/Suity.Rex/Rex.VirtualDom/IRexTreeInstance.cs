namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents a Rex tree instance with a specific path and associated tree reference.
/// </summary>
/// <typeparam name="T">The type of data or action associated with this tree instance.</typeparam>
public interface IRexTreeInstance<T>
{
    /// <summary>
    /// Gets the RexTree associated with this instance.
    /// </summary>
    RexTree Tree { get; }

    /// <summary>
    /// Gets the path within the RexTree for this instance.
    /// </summary>
    RexPath Path { get; }
}
