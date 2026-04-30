using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity;

/// <summary>
/// Function call context
/// </summary>
public class FunctionContext
{
    internal Dictionary<string, object> _arguments;
    internal FunctionContext _inner;
    internal object _value;

    public FunctionContext()
    {
    }

    public FunctionContext(string eventKey)
    {
        EventKey = eventKey;
    }

    public FunctionContext(string eventKey, object value)
    {
        EventKey = eventKey;
        _value = value;
    }

    public FunctionContext(FunctionContext inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public FunctionContext(FunctionContext inner, string eventKey)
        : this(inner)
    {
        EventKey = eventKey;
    }

    public FunctionContext(FunctionContext inner, string eventKey, object value)
        : this(inner)
    {
        EventKey = eventKey;
        _value = value;
    }

    public string EventKey { get; internal set; }
    public object Value => _value;
    public FunctionContext Parent => _inner;

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
/// Function call context object pool
/// </summary>
public class FunctionContextPool
{
    private readonly Stack<FunctionContext> _pool = new();

    public FunctionContext Get()
    {
        if (_pool.Count > 0)
        {
            var ctx = _pool.Pop();
            return ctx;
        }

        return new FunctionContext();
    }

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

public static class FunctionContextExtensions
{
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