using System;
using System.Collections.Generic;

namespace Suity.Rex.Mapping;

/// <summary>
/// Base class for objects that interact with a RexMapper, providing convenience methods for registration and resolution.
/// </summary>
public class RexMapperObject : IDisposable
{
    /// <summary>
    /// Gets or sets the dispose collector for tracking disposable registrations.
    /// </summary>
    protected RexDisposeCollector Listeners { get; set; }

    internal RexMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperObject"/> class using the global mapper.
    /// </summary>
    public RexMapperObject()
    {
        _mapper = RexMapper.Global;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperObject"/> class with a specific mapper.
    /// </summary>
    /// <param name="mapper">The RexMapper to use.</param>
    public RexMapperObject(RexMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets the RexMapper instance used by this object.
    /// </summary>
    public RexMapper Mapper => _mapper;

    /// <summary>
    /// Gets a value indicating whether this object has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        Listeners?.Dispose();
        Listeners = null;

        OnDispose();
    }

    /// <summary>
    /// Called when the object is disposed. Override to perform cleanup.
    /// </summary>
    protected virtual void OnDispose()
    {
    }

    /// <summary>
    /// Provides an object instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type of the value to provide.</typeparam>
    /// <param name="value">The object instance to provide.</param>
    protected void Provide<T>(T value) where T : class
    {
        Listeners += _mapper.Provide<T>(value);
    }

    /// <summary>
    /// Provides a type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideType<T>(bool singleton = true) where T : class
    {
        Listeners += _mapper.ProvideType<T>(singleton);
    }

    /// <summary>
    /// Provides a type registration with a specific implementation and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <typeparam name="TImplement">The implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideType<T, TImplement>(bool singleton = true) where T : class where TImplement : T
    {
        Listeners += _mapper.ProvideType<T, TImplement>(singleton);
    }

    /// <summary>
    /// Provides a handler instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <param name="handler">The handler instance to provide.</param>
    protected void ProvideHandler<T>(IRexHandler<T> handler)
    {
        Listeners += _mapper.ProvideHandler<T>(handler);
    }

    /// <summary>
    /// Provides a handler delegate and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <param name="handle">The handler delegate to provide.</param>
    protected void ProvideHandler<T>(RexHandleDelegate<T> handle)
    {
        Listeners += _mapper.ProvideHandler<T>(handle);
    }

    /// <summary>
    /// Provides an action handler and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <param name="handler">The action to provide.</param>
    protected void ProvideHandler<T>(Action<T> handler)
    {
        Listeners += RexMapper.Global.ProvideHandler<T>(v =>
        {
            handler(v);
            return true;
        });
    }

    /// <summary>
    /// Provides a handler type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the handler handles.</typeparam>
    /// <typeparam name="THandler">The handler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideHandlerType<T, THandler>(bool singleton = true) where THandler : IRexHandler<T>
    {
        Listeners += _mapper.ProvideHandlerType<T, THandler>(singleton);
    }

    /// <summary>
    /// Provides a producer instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <param name="producer">The producer instance to provide.</param>
    protected void ProvideProducer<T>(IRexProducer<T> producer)
    {
        Listeners += _mapper.ProvideProducer<T>(producer);
    }

    /// <summary>
    /// Provides a producer delegate and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <param name="produce">The produce delegate to provide.</param>
    protected void ProvideProducer<T>(RexProduceDelegate<T> produce)
    {
        Listeners += _mapper.ProvideProducer<T>(produce);
    }

    /// <summary>
    /// Provides a producer type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the producer produces.</typeparam>
    /// <typeparam name="TProducer">The producer implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideProducerType<T, TProducer>(bool singleton = true) where TProducer : IRexProducer<T>
    {
        Listeners += _mapper.ProvideProducerType<T, TProducer>(singleton);
    }

    /// <summary>
    /// Provides a recycler instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <param name="recycler">The recycler instance to provide.</param>
    protected void ProvideRecycler<T>(IRexRecycler<T> recycler)
    {
        Listeners += _mapper.ProvideRecycler<T>(recycler);
    }

    /// <summary>
    /// Provides a recycler delegate and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <param name="recycle">The recycle delegate to provide.</param>
    protected void ProvideRecycler<T>(RexRecycleDelegate<T> recycle)
    {
        Listeners += _mapper.ProvideRecycler<T>(recycle);
    }

    /// <summary>
    /// Provides a recycler type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the recycler recycles.</typeparam>
    /// <typeparam name="TRecycler">The recycler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideRecyclerType<T, TRecycler>(bool singleton = true) where TRecycler : IRexRecycler<T>
    {
        Listeners += _mapper.ProvideRecyclerType<T, TRecycler>(singleton);
    }

    /// <summary>
    /// Provides an assembler instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <param name="assembler">The assembler instance to provide.</param>
    protected void ProvideAssembler<T>(IRexAssembler<T> assembler)
    {
        Listeners += _mapper.ProvideAssembler<T>(assembler);
    }

    /// <summary>
    /// Provides an assembler delegate and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <param name="assemble">The assemble delegate to provide.</param>
    protected void ProvideAssembler<T>(RexAssembleDelegate<T> assemble)
    {
        Listeners += _mapper.ProvideAssembler<T>(assemble);
    }

    /// <summary>
    /// Provides an assembler type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the assembler assembles.</typeparam>
    /// <typeparam name="TAssembler">The assembler implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideAssemblerType<T, TAssembler>(bool singleton = true) where TAssembler : IRexAssembler<T>
    {
        Listeners += _mapper.ProvideAssemblerType<T, TAssembler>(singleton);
    }

    /// <summary>
    /// Provides a reducer instance and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <param name="reducer">The reducer instance to provide.</param>
    protected void ProvideReducer<T>(IRexReducer<T> reducer)
    {
        Listeners += _mapper.ProvideReducer<T>(reducer);
    }

    /// <summary>
    /// Provides a reducer delegate and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <param name="reduce">The reduce delegate to provide.</param>
    protected void ProvideReducer<T>(RexReduceDelegate<T> reduce)
    {
        Listeners += _mapper.ProvideReducer<T>(reduce);
    }

    /// <summary>
    /// Provides a reducer type registration and tracks it for disposal.
    /// </summary>
    /// <typeparam name="T">The type the reducer reduces.</typeparam>
    /// <typeparam name="TReducer">The reducer implementation type.</typeparam>
    /// <param name="singleton">Whether to register as a singleton.</param>
    protected void ProvideReducerType<T, TReducer>(bool singleton = true) where TReducer : IRexReducer<T>
    {
        Listeners += _mapper.ProvideReducerType<T, TReducer>(singleton);
    }

    /// <summary>
    /// Resolves a single instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>The resolved instance, or null if not found.</returns>
    protected T Get<T>() where T : class
    {
        return _mapper.Get<T>();
    }

    /// <summary>
    /// Resolves all instances of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>A collection of resolved instances.</returns>
    protected IEnumerable<T> GetMany<T>() where T : class
    {
        return _mapper.GetMany<T>();
    }

    /// <summary>
    /// Executes an action on a resolved instance if available.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="action">The action to execute.</param>
    protected void Do<T>(Action<T> action) where T : class
    {
        T obj = _mapper.Get<T>();
        if (obj != null)
        {
            action(obj);
        }
    }

    /// <summary>
    /// Executes an action on all resolved instances.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="action">The action to execute.</param>
    protected void DoMany<T>(Action<T> action) where T : class
    {
        IEnumerable<T> objs = _mapper.GetMany<T>();
        objs.Foreach(o => action(o));
    }

    /// <summary>
    /// Provides an object and immediately handles it through registered handlers.
    /// </summary>
    /// <typeparam name="T">The type of the value to supply.</typeparam>
    /// <param name="value">The value to supply and handle.</param>
    protected void Supply<T>(T value) where T : class
    {
        Provide<T>(value);
        HandleMany<T>(value);
    }

    /// <summary>
    /// Provides a handler and requests an object to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The handler to process the requested object.</param>
    protected void Request<T>(IRexHandler<T> handler) where T : class
    {
        ProvideHandler<T>(handler);
        var value = Get<T>();
        if (value != null)
        {
            handler.Handle(value);
        }
    }

    /// <summary>
    /// Provides a handler delegate and requests an object to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The handler delegate to process the requested object.</param>
    protected void Request<T>(RexHandleDelegate<T> handler) where T : class
    {
        ProvideHandler<T>(handler);
        var value = Get<T>();
        if (value != null)
        {
            handler(value);
        }
    }

    /// <summary>
    /// Provides an action handler and requests an object to handle.
    /// </summary>
    /// <typeparam name="T">The type of object to request.</typeparam>
    /// <param name="handler">The action to process the requested object.</param>
    protected void Request<T>(Action<T> handler) where T : class
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

    /// <summary>
    /// Handles a value through the first registered handler that accepts it.
    /// </summary>
    /// <typeparam name="T">The type of the value to handle.</typeparam>
    /// <param name="value">The value to handle.</param>
    protected void Handle<T>(T value)
    {
        _mapper.Handle(value);
    }

    /// <summary>
    /// Handles a value through all registered handlers.
    /// </summary>
    /// <typeparam name="T">The type of the value to handle.</typeparam>
    /// <param name="value">The value to handle.</param>
    protected void HandleMany<T>(T value)
    {
        _mapper.HandleMany(value);
    }

    /// <summary>
    /// Produces an instance using a registered producer.
    /// </summary>
    /// <typeparam name="T">The type to produce.</typeparam>
    /// <returns>The produced instance, or default if not found.</returns>
    protected T Produce<T>() where T : class
    {
        return _mapper.Produce<T>(null);
    }

    /// <summary>
    /// Produces an instance using a registered producer.
    /// </summary>
    /// <typeparam name="T">The type to produce.</typeparam>
    /// <param name="name">The name identifier for the production.</param>
    /// <returns>The produced instance, or default if not found.</returns>
    protected T Produce<T>(string name) where T : class
    {
        return _mapper.Produce<T>(name);
    }

    /// <summary>
    /// Recycles a produced instance.
    /// </summary>
    /// <typeparam name="T">The type of the product to recycle.</typeparam>
    /// <param name="product">The product instance to recycle.</param>
    /// <returns>True if the recycling was successful, false otherwise.</returns>
    protected bool Recycle<T>(T product) where T : class
    {
        return _mapper.Recycle<T>(null, product);
    }

    /// <summary>
    /// Recycles a produced instance.
    /// </summary>
    /// <typeparam name="T">The type of the product to recycle.</typeparam>
    /// <param name="name">The name identifier for the recycling.</param>
    /// <param name="product">The product instance to recycle.</param>
    /// <returns>True if the recycling was successful, false otherwise.</returns>
    protected bool Recycle<T>(string name, T product) where T : class
    {
        return _mapper.Recycle<T>(name, product);
    }

    /// <summary>
    /// Assembles a result using a registered assembler.
    /// </summary>
    /// <typeparam name="T">The type of the assembly result.</typeparam>
    /// <returns>The assembled result, or default if not found.</returns>
    protected T Assemble<T>() where T : class
    {
        return _mapper.Assemble<T>(this, null);
    }

    /// <summary>
    /// Assembles a result from a target object.
    /// </summary>
    /// <typeparam name="T">The type of the assembly result.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <returns>The assembled result, or default if not found.</returns>
    protected T Assemble<T>(object target) where T : class
    {
        return _mapper.Assemble<T>(target, null);
    }

    /// <summary>
    /// Assembles a result using a registered assembler.
    /// </summary>
    /// <typeparam name="T">The type of the assembly result.</typeparam>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>The assembled result, or default if not found.</returns>
    protected T Assemble<T>(string name) where T : class
    {
        return _mapper.Assemble<T>(this, name);
    }

    /// <summary>
    /// Assembles a result from a target object.
    /// </summary>
    /// <typeparam name="T">The type of the assembly result.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>The assembled result, or default if not found.</returns>
    protected T Assemble<T>(object target, string name) where T : class
    {
        return _mapper.Assemble<T>(target, name);
    }

    /// <summary>
    /// Assembles results using all registered assemblers.
    /// </summary>
    /// <typeparam name="T">The type of the assembly results.</typeparam>
    /// <returns>A collection of assembled results.</returns>
    protected IEnumerable<T> AssembleMany<T>() where T : class
    {
        return _mapper.AssembleMany<T>(this, null);
    }

    /// <summary>
    /// Assembles results from a target object using all registered assemblers.
    /// </summary>
    /// <typeparam name="T">The type of the assembly results.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <returns>A collection of assembled results.</returns>
    protected IEnumerable<T> AssembleMany<T>(object target) where T : class
    {
        return _mapper.AssembleMany<T>(target, null);
    }

    /// <summary>
    /// Assembles results using all registered assemblers.
    /// </summary>
    /// <typeparam name="T">The type of the assembly results.</typeparam>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>A collection of assembled results.</returns>
    protected IEnumerable<T> AssembleMany<T>(string name) where T : class
    {
        return _mapper.AssembleMany<T>(this, name);
    }

    /// <summary>
    /// Assembles results from a target object using all registered assemblers.
    /// </summary>
    /// <typeparam name="T">The type of the assembly results.</typeparam>
    /// <param name="target">The target object to assemble from.</param>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>A collection of assembled results.</returns>
    protected IEnumerable<T> AssembleMany<T>(object target, string name) where T : class
    {
        return _mapper.AssembleMany<T>(target, name);
    }

    /// <summary>
    /// Reduces a state using a registered reducer.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="state">The current state to reduce.</param>
    /// <param name="name">The name identifier for the reduction.</param>
    /// <param name="payload">The payload data for the reduction.</param>
    /// <returns>The new reduced state, or default if not found.</returns>
    protected T Reduce<T>(T state, string name, object payload)
    {
        return _mapper.Reduce<T>(state, name, payload);
    }

    /// <summary>
    /// Gets a mediator for the specified target.
    /// </summary>
    /// <typeparam name="T">The type of the target.</typeparam>
    /// <param name="target">The target object to mediate.</param>
    /// <returns>The resolved mediator, or null if not found.</returns>
    protected IRexMediator<T> GetMediator<T>(T target)
    {
        return _mapper.GetMediator<T>(target);
    }

    /// <summary>
    /// Gets all mediators for the specified target.
    /// </summary>
    /// <typeparam name="T">The type of the target.</typeparam>
    /// <param name="target">The target object to mediate.</param>
    /// <returns>A collection of resolved mediators.</returns>
    protected IEnumerable<IRexMediator<T>> GetMediators<T>(T target)
    {
        return _mapper.GetMediators<T>(target);
    }
}