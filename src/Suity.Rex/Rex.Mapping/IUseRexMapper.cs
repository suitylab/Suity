namespace Suity.Rex.Mapping;

/// <summary>
/// Interface for objects that require access to a RexMapper instance.
/// </summary>
public interface IUseRexMapper
{
    /// <summary>
    /// Gets or sets the RexMapper instance.
    /// </summary>
    RexMapper Mapper { get; set; }
}