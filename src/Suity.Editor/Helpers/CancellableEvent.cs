namespace Suity.Helpers;

/// <summary>
/// Represents an event that can be cancelled by setting the <see cref="Cancel"/> property to true.
/// Typically used as an argument in event handlers to allow subscribers to prevent the default action.
/// </summary>
public class CancellableEvent
{
    /// <summary>
    /// Gets or sets a value indicating whether the event has been cancelled.
    /// Set to true to prevent the default action associated with the event.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CancellableEvent"/> class.
    /// </summary>
    public CancellableEvent()
    { }
}