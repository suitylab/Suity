using System;

namespace Suity.Rex;

/// <summary>
/// Abstract base class for computed data that provides getter and setter access through object types.
/// </summary>
internal abstract class ComputedData
{
    /// <summary>
    /// Gets the current data value.
    /// </summary>
    /// <returns>The data value as an object.</returns>
    public abstract object GetData();

    /// <summary>
    /// Sets the data value.
    /// </summary>
    /// <param name="data">The value to set.</param>
    public abstract void SetData(object data);
}

/// <summary>
/// Generic implementation of <see cref="ComputedData"/> that wraps a getter and setter function.
/// </summary>
/// <typeparam name="T">The type of the data value.</typeparam>
internal class ComputedData<T> : ComputedData
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;

    /// <summary>
    /// Initializes a new instance with the specified getter and setter functions.
    /// </summary>
    /// <param name="getter">The function to get the value.</param>
    /// <param name="setter">The function to set the value.</param>
    public ComputedData(Func<T> getter, Action<T> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    /// <inheritdoc/>
    public override object GetData()
    {
        return _getter != null ? _getter() : null;
    }

    /// <inheritdoc/>
    public override void SetData(object data)
    {
        if (_setter is null)
        {
            return;
        }

        if (data is T t)
        {
            _setter(t);
        }
        else if (data is null)
        {
            if (typeof(T).IsClass)
            {
                _setter(default);
            }
        }
    }
}