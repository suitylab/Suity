using System;

namespace Suity;

/// <summary>
/// Defines a getter interface for retrieving values.
/// </summary>
/// <typeparam name="T">The type of value to get.</typeparam>
public interface IGetter<T> where T : class
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <returns>The value.</returns>
    T Get();
}

/// <summary>
/// Provides a store for holding and managing a single value.
/// </summary>
/// <typeparam name="T">The type of value to store.</typeparam>
public class ValueStore<T> : IGetter<T> where T : class
{
    private T _value;

    /// <summary>
    /// Gets the stored value.
    /// </summary>
    /// <returns>The stored value.</returns>
    public T Get() => _value;

    /// <summary>
    /// Picks up and returns the stored value, then clears the store.
    /// </summary>
    /// <returns>The stored value.</returns>
    public T PickUp()
    {
        var value = _value;
        _value = null;

        return value;
    }

    /// <summary>
    /// Exchanges the current value with a new value and returns the old value.
    /// </summary>
    /// <param name="value">The new value to store.</param>
    /// <returns>The previous value.</returns>
    public T Exchange(T value)
    {
        var current = _value;
        _value = value;

        return current;
    }

    /// <summary>
    /// Sets the stored value.
    /// </summary>
    /// <param name="value">The value to store.</param>
    public void Set(T value) => _value = value;

    /// <summary>
    /// Clears the stored value.
    /// </summary>
    public void Clear() => _value = null;
}

/// <summary>
/// Defines a getter interface for retrieving global service.
/// </summary>
/// <typeparam name="T">The type of service to get.</typeparam>
public class ServiceStore<T> : IGetter<T> where T : class
{
    private T _service;
    private T _fallBack;

    public T Get()
    {
        var service = _service ??= Device.Current.GetService<T>();
        service ??= _fallBack;

        return service;
    }

    public T Get(T defaultService)
    {
        T service = Get();
        if (service != null)
        {
            return service;
        }

        _service = defaultService;

        return defaultService ?? _fallBack;
    }

    public ServiceStore()
    {
    }

    public ServiceStore(T fallBack)
    {
        _fallBack = fallBack;
    }

    public void Clear() => _service = null;
}


/// <summary>
/// Represents a getter that selects a transformed value from another getter.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="T">The result type.</typeparam>
public class IGetterSelect<TSource, T>(IGetter<TSource> getter, Func<TSource, T> selector) : IGetter<T>
    where TSource : class
    where T : class
{
    private readonly IGetter<TSource> _getter = getter ?? throw new ArgumentNullException(nameof(getter));
    private readonly Func<TSource, T> _selector = selector ?? throw new ArgumentNullException(nameof(selector));

    private T _value;

    public T Get() => _value ??= _selector(_getter.Get());
}

/// <summary>
/// Provides extension methods for IGetter.
/// </summary>
public static class IGetterExtensions
{
    /// <summary>
    /// Selects a transformed value from the getter.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TSelect">The result type.</typeparam>
    /// <param name="getter">The source getter.</param>
    /// <param name="selector">The transformation selector.</param>
    /// <returns>A getter that returns the transformed value.</returns>
    public static IGetter<TSelect> Select<T, TSelect>(this IGetter<T> getter, Func<T, TSelect> selector) where T : class where TSelect : class
    {
        return new IGetterSelect<T, TSelect>(getter, selector);
    }
}