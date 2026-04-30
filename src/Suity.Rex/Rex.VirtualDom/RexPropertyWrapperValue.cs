using System;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Wraps an <see cref="IRexProperty{T}"/> to provide value access and listener management.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public class RexPropertyWrapperValue<T> : IRexValue<T>
{
    private IRexProperty<T> _property;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexPropertyWrapperValue{T}"/> class.
    /// </summary>
    /// <param name="property">The property to wrap. Must not be null.</param>
    public RexPropertyWrapperValue(IRexProperty<T> property)
    {
        _property = property ?? throw new ArgumentNullException(nameof(property));
    }

    /// <summary>
    /// Gets the current value of the wrapped property.
    /// </summary>
    public T Value => _property.Value;

    /// <summary>
    /// Adds a listener that will be invoked when the property value changes.
    /// </summary>
    /// <param name="action">The callback action to invoke with the new value.</param>
    public void AddListener(Action<T> action)
    {
        _property.Tree.AddDataListener<T>(_property.Path, action);
    }

    /// <summary>
    /// Removes a previously added listener.
    /// </summary>
    /// <param name="action">The callback action to remove.</param>
    public void RemoveListener(Action<T> action)
    {
        _property.Tree.RemoveListener<T>(_property.Path, action);
    }
}
