using System;

namespace Suity.Helpers;

/// <summary>
/// Represents a value that can be lazily updated and cached.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class QueuedValue<T>
{
    private readonly Func<T> _getter;
    private T _value;
    private bool _dirty = true;

    public QueuedValue()
    {
        _getter = () => default;
    }

    public QueuedValue(Func<T> getter)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
    }

    public void MarkDirty()
    {
        _dirty = true;
        _value = default;
    }

    public void Update()
    {
        if (_dirty)
        {
            _value = _getter();
            _dirty = false;
        }
    }

    public T GetValue()
    {
        if (_dirty)
        {
            _value = _getter();
            _dirty = false;
        }

        return _value;
    }

    public void SetValue(T value)
    {
        _value = value;
        _dirty = false;
    }
}