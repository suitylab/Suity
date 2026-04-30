using System;

namespace Suity.Rex;

/// <summary>
/// Defines a reactive value that notifies listeners when its value changes.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IRexValue<T>
{
    /// <summary>
    /// Gets the current value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Adds a listener to be invoked when the value changes.
    /// </summary>
    /// <param name="action">The action to execute with the new value.</param>
    void AddListener(Action<T> action);

    /// <summary>
    /// Removes a previously registered listener.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    void RemoveListener(Action<T> action);
}

/// <summary>
/// A mutable reactive value that notifies listeners when its value is set.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class RexValue<T> : IRexValue<T>
{
    private T _value;
    private Action<T> _callBack;

    /// <summary>
    /// Initializes a new instance with the default value of <typeparamref name="T"/>.
    /// </summary>
    public RexValue()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified initial value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public RexValue(T value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets or sets the current value. Setting the value notifies all registered listeners.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            _callBack?.Invoke(_value);
        }
    }

    /// <inheritdoc/>
    public void AddListener(Action<T> action)
    {
        _callBack += action;
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T> action)
    {
        _callBack -= action;
    }
}

/// <summary>
/// A read-only wrapper around an <see cref="IRexValue{T}"/> that prevents direct modification.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class RexReadonlyValue<T> : IRexValue<T>
{
    private IRexValue<T> _inner;

    /// <summary>
    /// Initializes a new instance wrapping the specified inner value.
    /// </summary>
    /// <param name="inner">The inner reactive value to wrap. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
    public RexReadonlyValue(IRexValue<T> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc/>
    public T Value => _inner.Value;

    /// <inheritdoc/>
    public void AddListener(Action<T> action)
    {
        _inner.AddListener(action);
    }

    /// <inheritdoc/>
    public void RemoveListener(Action<T> action)
    {
        _inner.RemoveListener(action);
    }
}