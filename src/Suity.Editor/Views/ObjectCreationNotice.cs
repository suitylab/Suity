using System;

namespace Suity.Views;

/// <summary>
/// A singleton marker class used to signal object creation events.
/// </summary>
[Serializable]
public sealed class ObjectCreationNotice
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ObjectCreationNotice"/>.
    /// </summary>
    public static ObjectCreationNotice Instance { get; } = new();

    private ObjectCreationNotice()
    {
    }
}
