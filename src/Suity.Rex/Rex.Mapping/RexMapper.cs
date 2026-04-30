using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Rex.Mapping;

/// <summary>
/// Main mapper class for managing type registrations, resolutions, and dependency injection.
/// Provides mechanisms for registering objects, handlers, producers, recyclers, assemblers, reducers, and mediators.
/// </summary>
public class RexMapper
{
    /// <summary>
    /// Global singleton instance of RexMapper with environment service enabled.
    /// </summary>
    public static readonly RexMapper Global = new(true, true);

    internal readonly bool _isGlobal;
    private readonly HashSet<Type> _doNotProvide = [];

    private readonly Dictionary<Type, RexMappingCollection> _typeToObject = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToHandler = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToProducer = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToRecycler = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToAssembler = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToReducer = [];
    private readonly Dictionary<Type, RexMappingCollection> _typeToMediator = [];

    private readonly List<IServiceProvider> _externalResolvers = [];

    private bool _useEnvService = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use the environment service for resolution.
    /// </summary>
    public bool UseEnvironmentService
    {
        get => _useEnvService;
        // set { _useGlobalService = value; }
    }

    /// <summary>
    /// Event raised when an object is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperEventArgs> ObjectResolved;

    /// <summary>
    /// Event raised when a handler is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperHandlerEventArgs> HandlerResolved;

    /// <summary>
    /// Event raised when a producer is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperProducerEventArgs> ProducerResolved;

    /// <summary>
    /// Event raised when a recycler is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperProducerEventArgs> RecyclerResolved;

    /// <summary>
    /// Event raised when an assembler is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperAssemblerEventArgs> AssemblerResolved;

    /// <summary>
    /// Event raised when a reducer is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperReducerEventArgs> ReducerResolved;

    /// <summary>
    /// Event raised when a mediator is successfully resolved.
    /// </summary>
    public event EventHandler<RexMapperEventArgs> MediatorResolved;

    /// <summary>
    /// Event raised when an object resolution fails.
    /// </summary>
    public event EventHandler<RexMapperEventArgs> ObjectUnsolved;

    /// <summary>
    /// Event raised when a handler resolution fails.
    /// </summary>
    public event EventHandler<RexMapperHandlerEventArgs> HandlerUnsolved;

    /// <summary>
    /// Event raised when a producer resolution fails.
    /// </summary>
    public event EventHandler<RexMapperProducerEventArgs> ProducerUnsolved;

    /// <summary>
    /// Event raised when a recycler resolution fails.
    /// </summary>
    public event EventHandler<RexMapperProducerEventArgs> RecyclerUnsolved;

    /// <summary>
    /// Event raised when an assembler resolution fails.
    /// </summary>
    public event EventHandler<RexMapperAssemblerEventArgs> AssemblerUnsolved;

    /// <summary>
    /// Event raised when a reducer resolution fails.
    /// </summary>
    public event EventHandler<RexMapperReducerEventArgs> ReducerUnsolved;

    /// <summary>
    /// Event raised when a mediator resolution fails.
    /// </summary>
    public event EventHandler<RexMapperEventArgs> MediatorUnsolved;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapper"/> class with default settings.
    /// </summary>
    public RexMapper()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapper"/> class.
    /// </summary>
    /// <param name="useGlobalService">Whether to use the global environment service for resolution.</param>
    public RexMapper(bool useGlobalService)
    {
        _useEnvService = useGlobalService;
    }

    private RexMapper(bool useGlobalService, bool isGlobal)
    {
        _isGlobal = isGlobal;
    }

    //protected override string GetName()
    //{
    //    if (_isGlobal)
    //    {
    //        return "Global RexMapper";
    //    }
    //    else
    //    {
    //        return base.GetName();
    //    }
    //}

    /// <summary>
    /// Adds an external service provider to use for resolution.
    /// </summary>
    /// <param name="resolver">The service provider to add.</param>
    public void AddResolver(IServiceProvider resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        _externalResolvers.Add(resolver);
    }

    /// <summary>
    /// Removes an external service provider from the resolution chain.
    /// </summary>
    /// <param name="resolver">The service provider to remove.</param>
    public void RemoveResolver(IServiceProvider resolver)
    {
        if (resolver != null)
        {
            _externalResolvers.Remove(resolver);
        }
    }

    #region Provide

    /// <summary>
    /// Provides an object instance for a specific type.
    /// </summary>
    /// <param name="type">The type to register the value for.</param>
    /// <param name="value">The object instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable Provide(Type type, object value)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!type.IsAssignableFrom(value.GetType()))
        {
            throw new ArgumentException("Value is not a " + type.Name, nameof(value));
        }

        if (_doNotProvide.Contains(value.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        RexMappingCollection collection = _typeToObject.GetOrAdd(type, _ => new RexMappingCollection(type, InfoFilter));
        if (collection.Contains(value))
        {
            return new RexDisposableAction(() => collection.Remove(value));
        }

        if (value is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(value);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(value));
    }

    /// <summary>
    /// Provides an object instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type to register the value for.</typeparam>
    /// <param name="value">The object instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable Provide<T>(T value) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (_doNotProvide.Contains(value.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToObject.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(value))
        {
            return new RexDisposableAction(() => collection.Remove(value));
        }

        if (value is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(value);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(value));
    }

    /// <summary>
    /// Provides a type registration with a specific implementation type.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <typeparam name="TImplement">The implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideType<T, TImplement>(bool singleton = true) where T : class where TImplement : T
    {
        if (_doNotProvide.Contains(typeof(TImplement)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToObject.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TImplement)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TImplement)));
        }

        var info = new RexMappingInfo(typeof(TImplement), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TImplement)));
    }

    /// <summary>
    /// Provides a type registration where the service type is also the implementation type.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideType<T>(bool singleton = true) where T : class
    {
        if (_doNotProvide.Contains(typeof(T)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToObject.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(T)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(T)));
        }

        var info = new RexMappingInfo(typeof(T), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(T)));
    }

    /// <summary>
    /// Provides a handler instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <param name="handler">The handler instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideHandler<T>(IRexHandler<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (_doNotProvide.Contains(handler.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToHandler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(handler))
        {
            return new RexDisposableAction(() => collection.Remove(handler));
        }

        if (handler is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(handler);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(handler));
    }

    /// <summary>
    /// Provides a handler delegate for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <param name="handler">The handler delegate to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideHandler<T>(RexHandleDelegate<T> handler)
    {
        return ProvideHandler<T>(new RexHandler<T>(handler));
    }

    /// <summary>
    /// Provides a handler type registration.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <typeparam name="THandler">The handler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideHandlerType<T, THandler>(bool singleton = true) where THandler : IRexHandler<T>
    {
        if (_doNotProvide.Contains(typeof(THandler)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToHandler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(THandler)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(THandler)));
        }

        var info = new RexMappingInfo(typeof(THandler), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(THandler)));
    }

    /// <summary>
    /// Provides a producer instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <param name="producer">The producer instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideProducer<T>(IRexProducer<T> producer)
    {
        if (producer is null)
        {
            throw new ArgumentNullException(nameof(producer));
        }

        if (_doNotProvide.Contains(producer.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToProducer.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(producer))
        {
            return new RexDisposableAction(() => collection.Remove(producer));
        }

        if (producer is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(producer);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(producer));
    }

    /// <summary>
    /// Provides a producer delegate for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <param name="produce">The produce delegate to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideProducer<T>(RexProduceDelegate<T> produce)
    {
        return ProvideProducer<T>(new RexProducer<T>(produce));
    }

    /// <summary>
    /// Provides a producer type registration.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <typeparam name="TProducer">The producer implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideProducerType<T, TProducer>(bool singleton = true) where TProducer : IRexProducer<T>
    {
        if (_doNotProvide.Contains(typeof(TProducer)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToProducer.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TProducer)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TProducer)));
        }

        var info = new RexMappingInfo(typeof(TProducer), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TProducer)));
    }

    /// <summary>
    /// Provides a recycler instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <param name="recycler">The recycler instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideRecycler<T>(IRexRecycler<T> recycler)
    {
        if (recycler is null)
        {
            throw new ArgumentNullException(nameof(recycler));
        }
        if (_doNotProvide.Contains(recycler.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToRecycler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(recycler))
        {
            return new RexDisposableAction(() => collection.Remove(recycler));
        }

        if (recycler is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(recycler);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(recycler));
    }

    /// <summary>
    /// Provides a recycler delegate for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <param name="recycle">The recycle delegate to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideRecycler<T>(RexRecycleDelegate<T> recycle)
    {
        return ProvideRecycler<T>(new RexRecycler<T>(recycle));
    }

    /// <summary>
    /// Provides a recycler type registration.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <typeparam name="TRecycler">The recycler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideRecyclerType<T, TRecycler>(bool singleton = true) where TRecycler : IRexRecycler<T>
    {
        if (_doNotProvide.Contains(typeof(TRecycler)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToRecycler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TRecycler)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TRecycler)));
        }

        var info = new RexMappingInfo(typeof(TRecycler), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TRecycler)));
    }

    /// <summary>
    /// Provides an assembler instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <param name="assembler">The assembler instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideAssembler<T>(IRexAssembler<T> assembler)
    {
        if (assembler is null)
        {
            throw new ArgumentNullException(nameof(assembler));
        }

        if (_doNotProvide.Contains(assembler.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToAssembler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(assembler))
        {
            return new RexDisposableAction(() => collection.Remove(assembler));
        }

        if (assembler is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(assembler);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(assembler));
    }

    /// <summary>
    /// Provides an assembler delegate for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <param name="assemble">The assemble delegate to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideAssembler<T>(RexAssembleDelegate<T> assemble)
    {
        return ProvideAssembler(new RexAssembler<T>(assemble));
    }

    /// <summary>
    /// Provides an assembler type registration.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <typeparam name="TAssembler">The assembler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideAssemblerType<T, TAssembler>(bool singleton = true) where TAssembler : IRexAssembler<T>
    {
        if (_doNotProvide.Contains(typeof(TAssembler)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToAssembler.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TAssembler)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TAssembler)));
        }

        var info = new RexMappingInfo(typeof(TAssembler), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TAssembler)));
    }

    /// <summary>
    /// Provides a reducer instance for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <param name="reducer">The reducer instance to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideReducer<T>(IRexReducer<T> reducer)
    {
        if (reducer is null)
        {
            throw new ArgumentNullException(nameof(reducer));
        }

        if (_doNotProvide.Contains(reducer.GetType()))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToReducer.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(reducer))
        {
            return new RexDisposableAction(() => collection.Remove(reducer));
        }

        if (reducer is IUseRexMapper useRexMapper)
        {
            useRexMapper.Mapper = this;
        }

        var info = new RexMappingInfo(reducer);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(reducer));
    }

    /// <summary>
    /// Provides a reducer delegate for a specific type.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <param name="reduce">The reduce delegate to provide.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideReducer<T>(RexReduceDelegate<T> reduce)
    {
        return ProvideReducer(new RexReducer<T>(reduce));
    }

    /// <summary>
    /// Provides a reducer type registration.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <typeparam name="TReducer">The reducer implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideReducerType<T, TReducer>(bool singleton = true) where TReducer : IRexReducer<T>
    {
        if (_doNotProvide.Contains(typeof(TReducer)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToReducer.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TReducer)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TReducer)));
        }

        var info = new RexMappingInfo(typeof(TReducer), singleton);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TReducer)));
    }

    /// <summary>
    /// Provides a mediator type registration.
    /// </summary>
    /// <typeparam name="T">The type the mediator mediates.</typeparam>
    /// <typeparam name="TMediator">The mediator implementation type.</typeparam>
    /// <returns>A disposable that removes the registration when disposed.</returns>
    public IDisposable ProvideMediator<T, TMediator>() where TMediator : IRexMediator<T>
    {
        if (_doNotProvide.Contains(typeof(TMediator)))
        {
            return EmptyDisposable.Empty;
        }

        var collection = _typeToMediator.GetOrAdd(typeof(T), _ => new(typeof(T), InfoFilter));
        if (collection.Contains(typeof(TMediator)))
        {
            return new RexDisposableAction(() => collection.Remove(typeof(TMediator)));
        }

        var info = new RexMappingInfo(typeof(TMediator), false);
        collection.Add(info);

        return new RexDisposableAction(() => collection.Remove(typeof(TMediator)));
    }

    /// <summary>
    /// Marks a type as not providable through this mapper.
    /// </summary>
    /// <typeparam name="T">The type to disable.</typeparam>
    public void DoNotProvide<T>()
    {
        _doNotProvide.Add(typeof(T));
    }

    /// <summary>
    /// Marks a type as not providable through this mapper.
    /// </summary>
    /// <param name="type">The type to disable.</param>
    public void DoNotProvide(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        _doNotProvide.Add(type);
    }

    #endregion

    #region Supply & Request

    /// <summary>
    /// Provides an object and immediately handles it through registered handlers.
    /// </summary>
    /// <typeparam name="T">The type of the value to supply.</typeparam>
    /// <param name="value">The value to supply and handle.</param>
    public void Supply<T>(T value) where T : class
    {
        Provide(value);
        HandleMany(value);
    }

    /// <summary>
    /// Provides a handler and requests an object of the specified type to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The handler to process the requested object.</param>
    public void Request<T>(IRexHandler<T> handler) where T : class
    {
        ProvideHandler(handler);
        var value = Get<T>();
        if (value != null)
        {
            handler.Handle(value);
        }
    }
    /// <summary>
    /// Provides a handler delegate and requests an object of the specified type to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The handler delegate to process the requested object.</param>
    public void Request<T>(RexHandleDelegate<T> handler) where T : class
    {
        ProvideHandler(handler);
        var value = Get<T>();
        if (value != null)
        {
            handler(value);
        }
    }
    /// <summary>
    /// Provides an action handler and requests an object of the specified type to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The action to process the requested object.</param>
    public void Request<T>(Action<T> handler) where T : class
    {
        ProvideHandler<T>(v =>
        {
            handler(v);
            return true;
        });

        var value = Get<T>();
        if (value != null)
        {
            handler(value);
        }
    }

    #endregion

    #region Resolve

    /// <summary>
    /// Resolves a single instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>The resolved instance, or null if not found.</returns>
    public T Get<T>() where T : class
    {
        var result = Resolve<T, T>(_typeToObject, true, true, out RexMappingInfo info);
        if (result != null)
        {
            ObjectResolved?.Invoke(this, new RexMapperEventArgs(typeof(T), result.GetType()));

            return result;
        }
        else
        {
            ObjectUnsolved?.Invoke(this, new RexMapperEventArgs(typeof(T), null));

            return null;
        }
    }

    /// <summary>
    /// Resolves all instances of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>A collection of resolved instances, or null if none found.</returns>
    public IEnumerable<T> GetMany<T>() where T : class
    {
        var results = ResolveMany<T, T>(_typeToObject);
        if (results != null)
        {
            ObjectResolved?.Invoke(this, new RexMapperEventArgs(typeof(T), results.GetType()));

            return results;
        }
        else
        {
            ObjectUnsolved?.Invoke(this, new RexMapperEventArgs(typeof(T), null));

            return null;
        }
    }

    /// <summary>
    /// Resolves a single instance of the specified type (non-generic).
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The resolved instance, or null if not found.</returns>
    public object Get(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var getterType = typeof(RexMappingGenericObjectGetter<>).MakeGenericType([type]);
        var getter = (IRexMappingGenericObjectGetter)Activator.CreateInstance(getterType);

        return getter.GetObject(this);
    }

    /// <summary>
    /// Resolves all instances of the specified type (non-generic).
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>A collection of resolved instances, or null if none found.</returns>
    public IEnumerable<object> GetMany(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var getterType = typeof(RexMappingGenericObjectGetter<>).MakeGenericType([type]);
        var getter = (IRexMappingGenericObjectGetter)Activator.CreateInstance(getterType);

        return getter.GetObjects(this);
    }

    /// <summary>
    /// Handles a value through the first registered handler that accepts it.
    /// </summary>
    /// <typeparam name="T">The type of the value to handle.</typeparam>
    /// <param name="value">The value to handle.</param>
    /// <returns>True if a handler processed the value successfully, false otherwise.</returns>
    public bool Handle<T>(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        IEnumerable<IRexHandler<T>> results = ResolveMany<T, IRexHandler<T>>(_typeToHandler);
        if (results?.Any() == true)
        {
            HandlerResolved?.Invoke(this, new RexMapperHandlerEventArgs(typeof(T), results.GetType(), value));

            foreach (var result in results)
            {
                if (result.Handle(value))
                {
                    return true;
                }
            }
        }
        else
        {
            HandlerUnsolved?.Invoke(this, new RexMapperHandlerEventArgs(typeof(T), null, value));
        }

        return false;
    }

    /// <summary>
    /// Handles a value through all registered handlers.
    /// </summary>
    /// <typeparam name="T">The type of the value to handle.</typeparam>
    /// <param name="value">The value to handle.</param>
    public void HandleMany<T>(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        IEnumerable<IRexHandler<T>> results = ResolveMany<T, IRexHandler<T>>(_typeToHandler);
        if (results?.Any() == true)
        {
            HandlerResolved?.Invoke(this, new RexMapperHandlerEventArgs(typeof(T), results.GetType(), value));

            foreach (var result in results)
            {
                result.Handle(value);
            }
        }
        else
        {
            HandlerUnsolved?.Invoke(this, new RexMapperHandlerEventArgs(typeof(T), null, value));
        }
    }

    /// <summary>
    /// Produces an instance of the specified type using a registered producer.
    /// </summary>
    /// <typeparam name="T">The type to produce.</typeparam>
    /// <param name="name">The name identifier for the production.</param>
    /// <returns>The produced instance, or default if not found.</returns>
    public T Produce<T>(string name) where T : class
    {
        IRexProducer<T> result = Resolve<T, IRexProducer<T>>(_typeToProducer, true, true, out RexMappingInfo info);
        if (result != null)
        {
            var product = result.Produce(name);
            if (product != null)
            {
                info?.AddName(name, true);
                ProducerResolved?.Invoke(this, new RexMapperProducerEventArgs(typeof(T), result.GetType(), name, product));

                return product;
            }
        }

        info?.AddName(name, false);
        ProducerUnsolved?.Invoke(this, new RexMapperProducerEventArgs(typeof(T), null, name, null));

        return default;
    }

    /// <summary>
    /// Recycles a produced instance using a registered recycler.
    /// </summary>
    /// <typeparam name="T">The type of the product to recycle.</typeparam>
    /// <param name="name">The name identifier for the recycling.</param>
    /// <param name="product">The product instance to recycle.</param>
    /// <returns>True if the recycling was successful, false otherwise.</returns>
    public bool Recycle<T>(string name, T product)
    {
        IRexRecycler<T> result = Resolve<T, IRexRecycler<T>>(_typeToRecycler, true, true, out RexMappingInfo info);
        if (result != null)
        {
            bool success = result.Recycle(name, product);
            if (success)
            {
                info?.AddName(name, true);
                RecyclerResolved?.Invoke(this, new RexMapperProducerEventArgs(typeof(T), result.GetType(), name, product));

                return success;
            }
        }

        info?.AddName(name, false);
        RecyclerUnsolved?.Invoke(this, new RexMapperProducerEventArgs(typeof(T), null, name, product));
        return false;
    }

    /// <summary>
    /// Assembles a result from a target object using a registered assembler.
    /// </summary>
    /// <typeparam name="T">The type of the assembly result.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>The assembled result, or default if not found.</returns>
    public T Assemble<T>(object target, string name)
    {
        IRexAssembler<T> result = Resolve<T, IRexAssembler<T>>(_typeToAssembler, true, true, out RexMappingInfo info);
        if (result != null)
        {
            var aResult = result.Assemble(target, name);
            if (aResult != null)
            {
                AssemblerResolved?.Invoke(this, new RexMapperAssemblerEventArgs(typeof(T), result.GetType(), target, name, aResult));

                return aResult;
            }
        }

        AssemblerUnsolved?.Invoke(this, new RexMapperAssemblerEventArgs(typeof(T), null, target, name, null));

        return default;
    }

    /// <summary>
    /// Assembles results from a target object using all registered assemblers.
    /// </summary>
    /// <typeparam name="T">The type of the assembly results.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>A collection of assembled results.</returns>
    public IEnumerable<T> AssembleMany<T>(object target, string name)
    {
        IEnumerable<IRexAssembler<T>> results = ResolveMany<T, IRexAssembler<T>>(_typeToAssembler);
        if (results?.Any() == true)
        {
            List<T> assembles = [];
            foreach (var result in results)
            {
                var aResult = result.Assemble(target, name);
                if (aResult != null)
                {
                    AssemblerResolved?.Invoke(this, new RexMapperAssemblerEventArgs(typeof(T), results.GetType(), target, name, aResult));
                }

                assembles.Add(aResult);
            }

            return assembles;
        }

        AssemblerUnsolved?.Invoke(this, new RexMapperAssemblerEventArgs(typeof(T), null, target, name, null));
        return [];
    }

    /// <summary>
    /// Reduces a state using a registered reducer.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="state">The current state to reduce.</param>
    /// <param name="name">The name identifier for the reduction.</param>
    /// <param name="payload">The payload data for the reduction.</param>
    /// <returns>The new reduced state, or default if not found.</returns>
    public T Reduce<T>(T state, string name, object payload)
    {
        IRexReducer<T> result = Resolve<T, IRexReducer<T>>(_typeToReducer, true, true, out RexMappingInfo info);
        if (result != null)
        {
            var newState = result.Reduce(state, name, payload);
            if (newState != null)
            {
                ReducerResolved?.Invoke(this, new RexMapperReducerEventArgs(typeof(T), result.GetType(), state, name, payload, newState));

                return newState;
            }
        }

        ReducerUnsolved?.Invoke(this, new RexMapperReducerEventArgs(typeof(T), null, state, name, payload, null));

        return default;
    }

    /// <summary>
    /// Gets a mediator for the specified target.
    /// </summary>
    /// <typeparam name="T">The type of the target.</typeparam>
    /// <param name="target">The target object to mediate.</param>
    /// <returns>The resolved mediator, or null if not found.</returns>
    public IRexMediator<T> GetMediator<T>(T target)
    {
        IRexMediator<T> result = Resolve<T, IRexMediator<T>>(_typeToMediator, false, false, out RexMappingInfo info);
        if (result != null)
        {
            MediatorResolved?.Invoke(this, new RexMapperEventArgs(typeof(T), result.GetType()));

            result.InitializeTarget(this, target);
            return result;
        }
        else
        {
            MediatorUnsolved?.Invoke(this, new RexMapperEventArgs(typeof(T), null));

            return null;
        }
    }

    /// <summary>
    /// Gets all mediators for the specified target.
    /// </summary>
    /// <typeparam name="T">The type of the target.</typeparam>
    /// <param name="target">The target object to mediate.</param>
    /// <returns>A collection of resolved mediators.</returns>
    public IEnumerable<IRexMediator<T>> GetMediators<T>(T target)
    {
        return ResolveMany<T, IRexMediator<T>>(_typeToMediator).Select(o =>
        {
            MediatorResolved?.Invoke(this, new RexMapperEventArgs(typeof(T), o.GetType()));
            o.InitializeTarget(this, target);

            return o;
        });
    }

    private TImplement Resolve<T, TImplement>(Dictionary<Type, RexMappingCollection> dic, bool tryExternal, bool tryEnvService, out RexMappingInfo info) where TImplement : class
    {
        TImplement result;

        RexMappingCollection collection = dic.GetValueSafe(typeof(T));
        info = collection?.First();
        if (info != null)
        {
            result = info.Resolve<TImplement>();
            if (result != null)
            {
                return result;
            }
        }

        if (tryExternal)
        {
            foreach (var resolver in _externalResolvers)
            {
                result = resolver.GetService(typeof(TImplement)) as TImplement;
                if (result != null && !_doNotProvide.Contains(result.GetType()))
                {
                    collection ??= new RexMappingCollection(typeof(T));
                    info = collection.IncreaseExternalResolved();

                    return result;
                }
            }
        }

        if (tryEnvService && _useEnvService)
        {
            result = RexGlobalResolve.Current?.GetService(typeof(TImplement)) as TImplement;
            if (result != null && !_doNotProvide.Contains(result.GetType()))
            {
                collection ??= new RexMappingCollection(typeof(T));
                info = collection.IncreaseExternalResolved();

                return result;
            }
        }

        return null;
    }

    private IEnumerable<TImplement> ResolveMany<T, TImplement>(Dictionary<Type, RexMappingCollection> dic) where TImplement : class
    {
        RexMappingCollection collection = dic.GetValueSafe(typeof(T));
        if (collection != null)
        {
            return collection.Infos.Select(o => o.Resolve<TImplement>()).OfType<TImplement>();
        }
        else
        {
            return [];
        }
    }

    private bool InfoFilter(RexMappingInfo info)
    {
        return !_doNotProvide.Contains(info.ImplementType);
    }

    #endregion

    /// <summary>
    /// Gets the types that are disabled from being provided.
    /// </summary>
    public IEnumerable<Type> DisabledTypes => _doNotProvide.Select(o => o);
    /// <summary>
    /// Gets the object type mapping collections.
    /// </summary>
    public IEnumerable<RexMappingCollection> ObjectTypes => _typeToObject.Values.Select(o => o);
    /// <summary>
    /// Gets the handler type mapping collections.
    /// </summary>
    public IEnumerable<RexMappingCollection> HandlerTypes => _typeToHandler.Values.Select(o => o);
    /// <summary>
    /// Gets the producer type mapping collections.
    /// </summary>
    public IEnumerable<RexMappingCollection> ProducerTypes => _typeToProducer.Values.Select(o => o);
    /// <summary>
    /// Gets the assembler type mapping collections.
    /// </summary>
    public IEnumerable<RexMappingCollection> AssemblerTypes => _typeToAssembler.Values.Select(o => o);
    /// <summary>
    /// Gets the mediator type mapping collections.
    /// </summary>
    public IEnumerable<RexMappingCollection> MediatorTypes => _typeToMediator.Values.Select(o => o);
}