namespace Suity.Networking;

/// <summary>
/// Network delivery methods
/// </summary>
public enum NetworkDeliveryMethods
{
    /// <summary>
    /// Default delivery method
    /// </summary>
    Default,

    /// <summary>
    /// Unreliable, out-of-order transmission
    /// </summary>
    Unreliable,

    /// <summary>
    /// Unreliable transmission, but delayed information will be automatically discarded
    /// </summary>
    UnreliableSequenced,

    /// <summary>
    /// Reliable transmission, but no order guarantee
    /// </summary>
    ReliableUnordered,

    /// <summary>
    /// Reliable transmission, delayed information may be lost, but the latest information is guaranteed to be delivered
    /// </summary>
    ReliableSequenced,

    /// <summary>
    /// Reliable and guaranteed order delivery
    /// </summary>
    ReliableOrdered,
}