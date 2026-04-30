using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines a mediator that manages the lifecycle and behavior of a target object.
/// </summary>
/// <typeparam name="T">The type of target to mediate.</typeparam>
public interface IRexMediator<T> : IDisposable
{
    /// <summary>
    /// Initializes the mediator with the mapper and target object.
    /// </summary>
    /// <param name="mapper">The RexMapper instance.</param>
    /// <param name="target">The target object to mediate.</param>
    void InitializeTarget(RexMapper mapper, T target);
}

/// <summary>
/// Base class for implementing a mediator with automatic mapper injection.
/// </summary>
/// <typeparam name="T">The type of target to mediate.</typeparam>
public abstract class RexMediator<T> : RexMapperObject, IRexMediator<T>
{
    /// <summary>
    /// Gets the target object being mediated.
    /// </summary>
    public T Target { get; private set; }

    /// <inheritdoc/>
    public void InitializeTarget(RexMapper mapper, T target)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        Target = target;

        OnInitialize();
    }

    /// <summary>
    /// Called when the mediator is initialized with the target.
    /// Override this method to perform initialization logic.
    /// </summary>
    protected virtual void OnInitialize()
    {
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        base.OnDispose();

        Target = default(T);
    }
}