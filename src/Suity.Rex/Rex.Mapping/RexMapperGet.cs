namespace Suity.Rex.Mapping;

/// <summary>
/// A lazy caching wrapper for resolving services from the global RexMapper.
/// </summary>
/// <typeparam name="T">The type of service to get.</typeparam>
public class RexMapperGet<T> /*: IGetter<T>*/ where T : class
{
    private T _value;

    /// <summary>
    /// Gets the cached value, resolving it from the global mapper if not already cached.
    /// </summary>
    /// <returns>The resolved service instance.</returns>
    public T Get()
    {
        return _value ?? (_value = RexMapper.Global.Get<T>());
    }
}