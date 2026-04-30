namespace Suity.Rex.VirtualDom;

/// <summary>
/// Defines a Rex tree property or action with an associated path.
/// </summary>
/// <typeparam name="T">The type of data or action argument associated with this definition.</typeparam>
public interface IRexTreeDefine<T>
{
    /// <summary>
    /// Gets the path within the RexTree for this definition.
    /// </summary>
    RexPath Path { get; }
}
