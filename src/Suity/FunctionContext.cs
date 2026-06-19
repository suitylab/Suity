using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity;

/// <summary>
/// Represents a function call context that carries event key, value, arguments, and a parent context chain.
/// </summary>
public class FunctionContext
{
    internal Dictionary<string, object> _arguments;
    internal FunctionContext _inner;
    internal object _value;

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/>.
    /// </summary>
    public FunctionContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/> with the specified event key.
    /// </summary>
    /// <param name="eventKey">The event key identifying this context.</param>
    public FunctionContext(string eventKey)
    {
        EventKey = eventKey;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/> with the specified event key and value.
    /// </summary>
    /// <param name="eventKey">The event key identifying this context.</param>
    /// <param name="value">The value associated with this context.</param>
    public FunctionContext(string eventKey, object value)
    {
        EventKey = eventKey;
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/> with a parent context.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    public FunctionContext(FunctionContext inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/> with a parent context and event key.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    /// <param name="eventKey">The event key identifying this context.</param>
    public FunctionContext(FunctionContext inner, string eventKey)
        : this(inner)
    {
        EventKey = eventKey;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FunctionContext"/> with a parent context, event key, and value.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    /// <param name="eventKey">The event key identifying this context.</param>
    /// <param name="value">The value associated with this context.</param>
    public FunctionContext(FunctionContext inner, string eventKey, object value)
        : this(inner)
    {
        EventKey = eventKey;
        _value = value;
    }

    /// <summary>
    /// Gets the event key identifying this context.
    /// </summary>
    public string EventKey { get; internal set; }

    /// <summary>
    /// Gets the value associated with this context.
    /// </summary>
    public object Value => _value;

    /// <summary>
    /// Gets the parent context, or <c>null</c> if none.
    /// </summary>
    public FunctionContext Parent => _inner;

    /// <summary>
    /// Sets an argument in this context. If <paramref name="argument"/> is <c>null</c>, the argument is removed.
    /// </summary>
    /// <param name="id">The argument identifier. Must not be <c>null</c> or empty.</param>
    /// <param name="argument">The argument value, or <c>null</c> to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c> or empty.</exception>
    public void SetArgument(string id, object argument)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        _arguments ??= [];

        if (argument != null)
        {
            _arguments[id] = argument;
        }
        else
        {
            _arguments.Remove(id);
        }
    }

    /// <summary>
    /// Gets an argument by identifier from this context, searching up through parent contexts if not found locally.
    /// </summary>
    /// <param name="id">The argument identifier.</param>
    /// <returns>The argument value, or <c>null</c> if not found.</returns>
    public object GetArgument(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return _arguments?.GetValueSafe(id) ?? _inner?.GetArgument(id);
    }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(EventKey))
        {
            return "FunctionContext:" + EventKey;
        }
        else
        {
            return "FunctionContext";
        }
    }
}

/// <summary>
/// Object pool for <see cref="FunctionContext"/> instances to reduce allocations.
/// </summary>
public class FunctionContextPool
{
    private readonly Stack<FunctionContext> _pool = new();

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with default values.
    /// </summary>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get()
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            return ctx;
        }

        return new FunctionContext();
    }

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with the specified event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get(string eventKey)
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            ctx.EventKey = eventKey;
            return ctx;
        }

        return new FunctionContext(eventKey);
    }

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with the specified event key and value.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get(string eventKey, object value)
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            ctx.EventKey = eventKey;
            ctx._value = value;
            return ctx;
        }

        return new FunctionContext(eventKey, value);
    }

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with a parent context.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get(FunctionContext inner)
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            ctx._inner = inner;
            return ctx;
        }

        return new FunctionContext(inner);
    }

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with a parent context and event key.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    /// <param name="eventKey">The event key.</param>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get(FunctionContext inner, string eventKey)
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            ctx._inner = inner;
            ctx.EventKey = eventKey;
            return ctx;
        }

        return new FunctionContext(inner, eventKey);
    }

    /// <summary>
    /// Gets a pooled <see cref="FunctionContext"/> with a parent context, event key, and value.
    /// </summary>
    /// <param name="inner">The parent context. Can be <c>null</c>.</param>
    /// <param name="eventKey">The event key.</param>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="FunctionContext"/> instance.</returns>
    public FunctionContext Get(FunctionContext inner, string eventKey, object value)
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            ctx._inner = inner;
            ctx.EventKey = eventKey;
            ctx._value = value;
            return ctx;
        }

        return new FunctionContext(inner, eventKey, value);
    }

    /// <summary>
    /// Recycles a <see cref="FunctionContext"/> back into the pool, resetting its state.
    /// </summary>
    /// <param name="context">The context to recycle. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public void Recycle(FunctionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context._inner = null;
        context.EventKey = null;
        context._value = null;
        context._arguments?.Clear();

        _pool.Push(context);
    }
}

/// <summary>
/// Provides extension methods for <see cref="FunctionContext"/>.
/// </summary>
public static class FunctionContextExtensions
{
    /// <summary>
    /// Gets an argument of the specified type from the context using the type's full name as the key.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="context">The function context.</param>
    /// <returns>The argument value, or <c>null</c> if not found.</returns>
    public static T GetArgument<T>(this FunctionContext context) where T : class
    {
        return context.GetArgument(typeof(T).FullName) as T;
    }

    /// <summary>
    /// Sets an argument of the specified type in the context.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="value">The argument value.</param>
    public static void SetArgument<T>(this FunctionContext context, T value) where T : class
    {
        context.SetArgument(typeof(T).FullName, value);
    }

    /// <summary>
    /// Gets an argument of the specified type by name from the context.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value.</returns>
    public static T GetArgument<T>(this FunctionContext context, string name) where T : class
    {
        return context.GetArgument(name) as T;
    }

    /// <summary>
    /// Sets an argument of the specified type by name in the context.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The argument value.</param>
    public static void SetArgument<T>(this FunctionContext context, string name, T value) where T : class
    {
        context.SetArgument(name, value);
    }



    /// <summary>
    /// Gets a value type argument from the context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="defaultValue">The default value if not found.</param>
    /// <returns>The value.</returns>
    public static T GetValue<T>(this FunctionContext context, T defaultValue = default) where T : struct
    {
        if (context.GetArgument(typeof(T).FullName) is T t)
        {
            return t;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets a value type argument in the context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="value">The value.</param>
    public static void SetValue<T>(this FunctionContext context, T value) where T : struct
    {
        context.SetArgument(typeof(T).FullName, value);
    }

    /// <summary>
    /// Gets a value type argument by name from the context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValue">The default value if not found.</param>
    /// <returns>The value.</returns>
    public static T GetValue<T>(this FunctionContext context, string name, T defaultValue = default) where T : struct
    {
        if (context.GetArgument(name) is T t)
        {
            return t;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets a value type argument by name in the context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The value.</param>
    public static void SetValue<T>(this FunctionContext context, string name, T value) where T : struct
    {
        context.SetArgument(name, value);
    }


    /// <summary>
    /// Gets a nullable value type argument from the context.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <returns>The nullable value.</returns>
    public static T? GetNullable<T>(this FunctionContext context) where T : struct
    {
        if (context.GetArgument(typeof(T).FullName) is T t)
        {
            return t;
        }

        return null;
    }

    /// <summary>
    /// Sets a nullable value type argument in the context.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="value">The nullable value.</param>
    public static void SetNullable<T>(this FunctionContext context, T? value) where T : struct
    {
        if (value.HasValue)
        {
            context.SetArgument(typeof(T).FullName, value.Value);
        }
        else
        {
            context.SetArgument(typeof(T).FullName, null);
        }
    }

    /// <summary>
    /// Gets a nullable value type argument by name from the context.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <returns>The nullable value.</returns>
    public static T? GetNullable<T>(this FunctionContext context, string name) where T : struct
    {
        if (context.GetArgument(name) is T t)
        {
            return t;
        }

        return null;
    }

    /// <summary>
    /// Sets a nullable value type argument by name in the context.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable value.</typeparam>
    /// <param name="context">The function context.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The nullable value.</param>
    public static void SetNullable<T>(this FunctionContext context, string name, T? value) where T : struct
    {
        if (value.HasValue)
        {
            context.SetArgument(name, value.Value);
        }
        else
        {
            context.SetArgument(name, null);
        }
    }
}