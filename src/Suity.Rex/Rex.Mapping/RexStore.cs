namespace Suity.Rex.Mapping;

/// <summary>
/// A lazy caching wrapper for resolving services from a RexMapper.
/// </summary>
/// <typeparam name="T">The type of service to store.</typeparam>
public class RexStore<T> /*: IGetter<T>*/ where T : class
{
    private readonly RexMapper _mapper;
    private T _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexStore{T}"/> class using the global mapper.
    /// </summary>
    public RexStore()
    {
        _mapper = RexMapper.Global;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexStore{T}"/> class with a specific mapper.
    /// </summary>
    /// <param name="mapper">The RexMapper to use for resolution.</param>
    public RexStore(RexMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Gets the cached service, resolving it from the mapper if not already cached.
    /// </summary>
    /// <returns>The resolved service instance.</returns>
    public T Get() => _service ??= _mapper.Get<T>();

    /// <summary>
    /// Gets the cached service, or uses the default if not available.
    /// </summary>
    /// <param name="defaultService">The default service to use if none is resolved.</param>
    /// <returns>The resolved service or the default service.</returns>
    public T Get(T defaultService)
    {
        T service = Get();
        if (service != null)
        {
            return service;
        }

        _service = defaultService;

        return defaultService; ;
    }

    /// <summary>
    /// Clears the cached service.
    /// </summary>
    public void Clear() => _service = null;
}