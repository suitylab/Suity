namespace Suity.Rex.VirtualDom;

/// <summary>
/// Represents a reactive property in the Rex tree that supports getting, setting, and queuing values.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public interface IRexProperty<T> : IRexTreeInstance<T>
{
    /// <summary>
    /// Gets or sets the current value of the property.
    /// </summary>
    T Value { get; set; }

    /// <summary>
    /// Sets the property value in a queued manner for deferred execution.
    /// </summary>
    /// <param name="value">The value to set.</param>
    void SetValueQueued(T value);

    /// <summary>
    /// Sets the property value deeply, replacing all nested child data.
    /// </summary>
    /// <param name="value">The value to set deeply.</param>
    void SetValueDeep(T value);
}
