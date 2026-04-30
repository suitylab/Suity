using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Event arguments for basic mapper resolution events.
/// </summary>
public class RexMapperEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type that was requested.
    /// </summary>
    public Type RequestType { get; }
    /// <summary>
    /// Gets the type that was resolved (null if unsolved).
    /// </summary>
    public Type ResolvedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested.</param>
    /// <param name="resolvedType">The type that was resolved.</param>
    public RexMapperEventArgs(Type requestType, Type resolvedType)
    {
        RequestType = requestType;
        ResolvedType = resolvedType;
    }
}

/// <summary>
/// Event arguments for handler resolution events.
/// </summary>
public class RexMapperHandlerEventArgs : RexMapperEventArgs
{
    /// <summary>
    /// Gets the object being handled.
    /// </summary>
    public object RequestObject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperHandlerEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested.</param>
    /// <param name="resolvedType">The type that was resolved.</param>
    /// <param name="requestObject">The object being handled.</param>
    public RexMapperHandlerEventArgs(Type requestType, Type resolvedType, object requestObject)
        : base(requestType, resolvedType)
    {
        RequestObject = requestObject;
    }
}

/// <summary>
/// Event arguments for producer and recycler resolution events.
/// </summary>
public class RexMapperProducerEventArgs : RexMapperEventArgs
{
    /// <summary>
    /// Gets the name associated with the production/recycling.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the product object.
    /// </summary>
    public object ProductObject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperProducerEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested.</param>
    /// <param name="resolvedType">The type that was resolved.</param>
    /// <param name="name">The name associated with the production/recycling.</param>
    /// <param name="productObject">The product object.</param>
    public RexMapperProducerEventArgs(Type requestType, Type resolvedType, string name, object productObject) : base(requestType, resolvedType)
    {
        Name = name;
        ProductObject = productObject;
    }
}

/// <summary>
/// Event arguments for assembler resolution events.
/// </summary>
public class RexMapperAssemblerEventArgs : RexMapperEventArgs
{
    /// <summary>
    /// Gets the target object being assembled.
    /// </summary>
    public object TargetObject { get; }
    /// <summary>
    /// Gets the name associated with the assembly.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the result object from assembly.
    /// </summary>
    public object ResultObject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperAssemblerEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested.</param>
    /// <param name="resolvedType">The type that was resolved.</param>
    /// <param name="targetObject">The target object being assembled.</param>
    /// <param name="name">The name associated with the assembly.</param>
    /// <param name="resultObject">The result object from assembly.</param>
    public RexMapperAssemblerEventArgs(Type requestType, Type resolvedType, object targetObject, string name, object resultObject) : base(requestType, resolvedType)
    {
        TargetObject = targetObject;
        Name = name;
        ResultObject = resultObject;
    }
}

/// <summary>
/// Event arguments for reducer resolution events.
/// </summary>
public class RexMapperReducerEventArgs : RexMapperEventArgs
{
    /// <summary>
    /// Gets the old state before reduction.
    /// </summary>
    public object OldState { get; }
    /// <summary>
    /// Gets the name associated with the reduction.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the payload data for the reduction.
    /// </summary>
    public object Payload { get; }
    /// <summary>
    /// Gets the new state after reduction.
    /// </summary>
    public object NewState { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperReducerEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested.</param>
    /// <param name="resolvedType">The type that was resolved.</param>
    /// <param name="oldState">The old state before reduction.</param>
    /// <param name="name">The name associated with the reduction.</param>
    /// <param name="payload">The payload data for the reduction.</param>
    /// <param name="newState">The new state after reduction.</param>
    public RexMapperReducerEventArgs(Type requestType, Type resolvedType, object oldState, string name, object payload, object newState) : base(requestType, resolvedType)
    {
        OldState = oldState;
        Name = name;
        Payload = payload;
        NewState = newState;
    }
}

/// <summary>
/// Event arguments for unsolved resolution events.
/// </summary>
public class RexMapperUnsolvedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type that was requested but not resolved.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexMapperUnsolvedEventArgs"/> class.
    /// </summary>
    /// <param name="requestType">The type that was requested but not resolved.</param>
    public RexMapperUnsolvedEventArgs(Type requestType)
    {
        RequestType = requestType;
    }
}