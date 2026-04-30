namespace Suity.Rex;

/// <summary>
/// Abstract base class for representing a collection of action arguments.
/// </summary>
public abstract class ActionArguments
{
    internal ActionArguments()
    {
    }

    /// <summary>
    /// Gets the number of arguments in this collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the argument at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the argument to get.</param>
    /// <returns>The argument at the specified index, or null if out of range.</returns>
    public abstract object GetArgument(int index);
}

/// <summary>
/// Represents an empty action argument collection with zero arguments.
/// </summary>
public class ActionArgument : ActionArguments
{
    /// <summary>
    /// Gets the singleton empty action argument instance.
    /// </summary>
    public static readonly ActionArgument Empty = new();

    /// <inheritdoc/>
    public override int Count => 0;

    /// <inheritdoc/>
    public override object GetArgument(int index)
    {
        return null;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "()";
    }
}

/// <summary>
/// Represents an action argument collection with a single argument.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
public class ActionArgument<T1> : ActionArguments
{
    /// <summary>
    /// The first argument value.
    /// </summary>
    public readonly T1 Arg1;

    /// <summary>
    /// Initializes a new instance with the specified argument.
    /// </summary>
    /// <param name="arg1">The first argument value.</param>
    public ActionArgument(T1 arg1)
    {
        Arg1 = arg1;
    }

    /// <inheritdoc/>
    public override int Count => 1;

    /// <inheritdoc/>
    public override object GetArgument(int index)
    {
        switch (index)
        {
            case 0:
                return Arg1;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Arg1})";
    }
}

/// <summary>
/// Represents an action argument collection with two arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
public class ActionArgument<T1, T2> : ActionArguments
{
    /// <summary>
    /// The first argument value.
    /// </summary>
    public readonly T1 Arg1;

    /// <summary>
    /// The second argument value.
    /// </summary>
    public readonly T2 Arg2;

    /// <summary>
    /// Initializes a new instance with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument value.</param>
    /// <param name="arg2">The second argument value.</param>
    public ActionArgument(T1 arg1, T2 arg2)
    {
        Arg1 = arg1;
        Arg2 = arg2;
    }

    /// <inheritdoc/>
    public override int Count => 2;

    /// <inheritdoc/>
    public override object GetArgument(int index)
    {
        switch (index)
        {
            case 0:
                return Arg1;

            case 1:
                return Arg2;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Arg1}, {Arg2})";
    }
}

/// <summary>
/// Represents an action argument collection with three arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
public class ActionArgument<T1, T2, T3> : ActionArguments
{
    /// <summary>
    /// The first argument value.
    /// </summary>
    public readonly T1 Arg1;

    /// <summary>
    /// The second argument value.
    /// </summary>
    public readonly T2 Arg2;

    /// <summary>
    /// The third argument value.
    /// </summary>
    public readonly T3 Arg3;

    /// <summary>
    /// Initializes a new instance with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument value.</param>
    /// <param name="arg2">The second argument value.</param>
    /// <param name="arg3">The third argument value.</param>
    public ActionArgument(T1 arg1, T2 arg2, T3 arg3)
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
    }

    /// <inheritdoc/>
    public override int Count => 3;

    /// <inheritdoc/>
    public override object GetArgument(int index)
    {
        switch (index)
        {
            case 0:
                return Arg1;

            case 1:
                return Arg2;

            case 2:
                return Arg3;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Arg1}, {Arg2}, ({Arg3}))";
    }
}

/// <summary>
/// Represents an action argument collection with four arguments.
/// </summary>
/// <typeparam name="T1">The type of the first argument.</typeparam>
/// <typeparam name="T2">The type of the second argument.</typeparam>
/// <typeparam name="T3">The type of the third argument.</typeparam>
/// <typeparam name="T4">The type of the fourth argument.</typeparam>
public class ActionArgument<T1, T2, T3, T4> : ActionArguments
{
    /// <summary>
    /// The first argument value.
    /// </summary>
    public readonly T1 Arg1;

    /// <summary>
    /// The second argument value.
    /// </summary>
    public readonly T2 Arg2;

    /// <summary>
    /// The third argument value.
    /// </summary>
    public readonly T3 Arg3;

    /// <summary>
    /// The fourth argument value.
    /// </summary>
    public readonly T4 Arg4;

    /// <summary>
    /// Initializes a new instance with the specified arguments.
    /// </summary>
    /// <param name="arg1">The first argument value.</param>
    /// <param name="arg2">The second argument value.</param>
    /// <param name="arg3">The third argument value.</param>
    /// <param name="arg4">The fourth argument value.</param>
    public ActionArgument(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
        Arg4 = arg4;
    }

    /// <inheritdoc/>
    public override int Count => 4;

    /// <inheritdoc/>
    public override object GetArgument(int index)
    {
        switch (index)
        {
            case 0:
                return Arg1;

            case 1:
                return Arg2;

            case 2:
                return Arg3;

            case 3:
                return Arg4;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Arg1}, {Arg2}, ({Arg3}), ({Arg4}))";
    }
}

/// <summary>
/// Extension methods for working with <see cref="ActionArguments"/>.
/// </summary>
public static class ActionArgumentExtensions
{
    /// <summary>
    /// Gets an argument at the specified index, returning a default value if the argument is null or not of the expected type.
    /// </summary>
    /// <typeparam name="T">The expected type of the argument.</typeparam>
    /// <param name="arguments">The arguments collection.</param>
    /// <param name="index">The zero-based index of the argument to get.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found or not of type <typeparamref name="T"/>.</param>
    /// <returns>The argument value cast to <typeparamref name="T"/>, or <paramref name="defaultValue"/> if not available.</returns>
    public static T GetArgumentDefault<T>(this ActionArguments arguments, int index, T defaultValue = default(T))
    {
        object obj = arguments.GetArgument(index);
        if (obj is T t)
        {
            return t;
        }
        else
        {
            return defaultValue;
        }
    }
}