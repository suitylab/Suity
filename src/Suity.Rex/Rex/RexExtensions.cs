using Suity.Rex.Operators;
using System;
using System.Collections.Generic;

namespace Suity.Rex;

/// <summary>
/// Extension methods for creating and composing Rex listeners and events.
/// </summary>
public static class RexExtensions
{
    #region Event

    /// <summary>
    /// Converts an <see cref="IRexEvent"/> to an <see cref="IRexListener{T}"/>.
    /// </summary>
    /// <param name="rexEvent">The event to convert.</param>
    /// <returns>A listener that observes the event.</returns>
    public static IRexListener<object> AsRexListener(this IRexEvent rexEvent)
    {
        return new EventListener(rexEvent);
    }

    /// <summary>
    /// Converts an <see cref="IRexEvent{T}"/> to an <see cref="IRexListener{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the event argument.</typeparam>
    /// <param name="rexEvent">The event to convert.</param>
    /// <returns>A listener that observes the event.</returns>
    public static IRexListener<T> AsRexListener<T>(this IRexEvent<T> rexEvent)
    {
        return new EventListener<T>(rexEvent);
    }

    /// <summary>
    /// Converts an <see cref="IRexEvent{T1, T2}"/> to an <see cref="IRexListener{T}"/> wrapping arguments in <see cref="ActionArgument{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="rexEvent">The event to convert.</param>
    /// <returns>A listener that observes the event.</returns>
    public static IRexListener<ActionArgument<T1, T2>> AsRexListener<T1, T2>(this IRexEvent<T1, T2> rexEvent)
    {
        return new EventListener<T1, T2>(rexEvent);
    }

    /// <summary>
    /// Converts an <see cref="IRexEvent{T1, T2, T3}"/> to an <see cref="IRexListener{T}"/> wrapping arguments in <see cref="ActionArgument{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="rexEvent">The event to convert.</param>
    /// <returns>A listener that observes the event.</returns>
    public static IRexListener<ActionArgument<T1, T2, T3>> AsRexListener<T1, T2, T3>(this IRexEvent<T1, T2, T3> rexEvent)
    {
        return new EventListener<T1, T2, T3>(rexEvent);
    }

    /// <summary>
    /// Converts an <see cref="IRexEvent{T1, T2, T3, T4}"/> to an <see cref="IRexListener{T}"/> wrapping arguments in <see cref="ActionArgument{T1, T2, T3, T4}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="rexEvent">The event to convert.</param>
    /// <returns>A listener that observes the event.</returns>
    public static IRexListener<ActionArgument<T1, T2, T3, T4>> AsRexListener<T1, T2, T3, T4>(this IRexEvent<T1, T2, T3, T4> rexEvent)
    {
        return new EventListener<T1, T2, T3, T4>(rexEvent);
    }

    /// <summary>
    /// Subscribes a listener to invoke a parameterless event handle.
    /// </summary>
    /// <typeparam name="T">The type of the listener value.</typeparam>
    /// <param name="listener">The listener to subscribe.</param>
    /// <param name="rexEventHandle">The event handle to invoke.</param>
    /// <returns>A disposable that represents the subscription.</returns>
    public static IDisposable MapTo<T>(this IRexListener<T> listener, RexEventHandle rexEventHandle)
    {
        listener.Subscribe(o => rexEventHandle.Invoke());

        return listener;
    }

    /// <summary>
    /// Subscribes a listener to invoke an event handle with the received value.
    /// </summary>
    /// <typeparam name="T">The type of the listener value.</typeparam>
    /// <param name="listener">The listener to subscribe.</param>
    /// <param name="rexEventHandle">The event handle to invoke.</param>
    /// <returns>A disposable that represents the subscription.</returns>
    public static IDisposable MapTo<T>(this IRexListener<T> listener, RexEventHandle<T> rexEventHandle)
    {
        listener.Subscribe(o => rexEventHandle.Invoke(o));

        return listener;
    }

    #endregion

    #region Value

    /// <summary>
    /// Converts an <see cref="IRexValue{T}"/> to an <see cref="IRexListener{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="rexValue">The reactive value to convert.</param>
    /// <returns>A listener that observes value changes.</returns>
    public static IRexListener<T> AsRexListener<T>(this IRexValue<T> rexValue)
    {
        return new RexValueListener<T>(rexValue);
    }

    #endregion

    #region Operator

    /// <summary>
    /// Returns a listener that selects one of two values based on a boolean source.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source">The boolean source listener.</param>
    /// <param name="truePart">The value to emit when the source is true.</param>
    /// <param name="falsePart">The value to emit when the source is false.</param>
    /// <returns>A listener that emits the selected value.</returns>
    public static IRexListener<TResult> SelectIf<TResult>(this IRexListener<bool> source, TResult truePart, TResult falsePart)
    {
        return new SelectIf<TResult>(source, truePart, falsePart);
    }

    /// <summary>
    /// Returns a listener that selects one of two values based on whether the source has a non-null value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="truePart">The value to emit when the source has a value.</param>
    /// <param name="falsePart">The value to emit when the source is null.</param>
    /// <returns>A listener that emits the selected value.</returns>
    public static IRexListener<TResult> IfHasValue<TResult>(this IRexListener<object> source, TResult truePart, TResult falsePart)
    {
        return new IfHasValue<TResult>(source, truePart, falsePart);
    }

    /// <summary>
    /// Filters the source listener values using a predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="predicate">The predicate to test each value.</param>
    /// <returns>A listener that emits only values that satisfy the predicate.</returns>
    public static IRexListener<T> Where<T>(this IRexListener<T> source, Predicate<T> predicate)
    {
        return new Where<T>(source, predicate);
    }

    /// <summary>
    /// Projects each value from the source listener into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="selector">A transform function to apply to each value.</param>
    /// <returns>A listener that emits the transformed values.</returns>
    public static IRexListener<TResult> Select<TSource, TResult>(this IRexListener<TSource> source, Func<TSource, TResult> selector)
    {
        return new Select<TSource, TResult>(source, selector);
    }

    /// <summary>
    /// Projects each value from the source listener into an enumerable and flattens the results.
    /// </summary>
    /// <typeparam name="TSource">The type of the source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="selector">A transform function that returns an enumerable for each value.</param>
    /// <returns>A listener that emits each element from the flattened enumerables.</returns>
    public static IRexListener<TResult> SelectMany<TSource, TResult>(this IRexListener<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
    {
        return new SelectMany<TSource, TResult>(source, selector);
    }

    /// <summary>
    /// Filters the source listener values to only those of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source value.</typeparam>
    /// <typeparam name="TResult">The type to filter by.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <returns>A listener that emits only values of type <typeparamref name="TResult"/>.</returns>
    public static IRexListener<TResult> OfType<TSource, TResult>(this IRexListener<TSource> source) where TResult : class
    {
        return new OfType<TSource, TResult>(source);
    }

    /// <summary>
    /// Filters the source listener to only emit non-null values.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <returns>A listener that emits only non-null values.</returns>
    public static IRexListener<T> NotNull<T>(this IRexListener<T> source) where T : class
    {
        return new NotNull<T>(source);
    }

    /// <summary>
    /// Casts each value from the source listener to type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source value.</typeparam>
    /// <typeparam name="TResult">The type to cast to.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <returns>A listener that emits the cast values.</returns>
    public static IRexListener<TResult> Cast<TSource, TResult>(this IRexListener<TSource> source)
    {
        return new Cast<TSource, TResult>(source);
    }

    /// <summary>
    /// Emits each element from an enumerable value individually.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source listener that emits enumerables.</param>
    /// <returns>A listener that emits each element individually.</returns>
    public static IRexListener<T> Each<T>(this IRexListener<IEnumerable<T>> source)
    {
        return new Each<T>(source);
    }

    /// <inheritdoc cref="Each{T}(IRexListener{IEnumerable{T}})"/>
    public static IRexListener<T> Each<T>(this IRexListener<List<T>> source)
    {
        return new Each<T>(source.Select(o => (IEnumerable<T>)o));
    }

    /// <inheritdoc cref="Each{T}(IRexListener{IEnumerable{T}})"/>
    public static IRexListener<T> Each<T>(this IRexListener<IList<T>> source)
    {
        return new Each<T>(source.Select(o => (IEnumerable<T>)o));
    }

    /// <inheritdoc cref="Each{T}(IRexListener{IEnumerable{T}})"/>
    public static IRexListener<T> Each<T>(this IRexListener<T[]> source)
    {
        return new Each<T>(source.Select(o => (IEnumerable<T>)o));
    }

    /// <summary>
    /// Emits only the first element from each enumerable value.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source listener that emits enumerables.</param>
    /// <returns>A listener that emits the first element of each enumerable.</returns>
    public static IRexListener<T> First<T>(this IRexListener<IEnumerable<T>> source)
    {
        return new First<T>(source);
    }

    /// <summary>
    /// Emits the first element from each enumerable value, or the default value if empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source listener that emits enumerables.</param>
    /// <returns>A listener that emits the first element or default.</returns>
    public static IRexListener<T> FirstOrDefault<T>(this IRexListener<IEnumerable<T>> source)
    {
        return new FirstOrDefault<T>(source);
    }

    /// <summary>
    /// Takes a specified number of elements from the beginning of each enumerable value.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source listener that emits enumerables.</param>
    /// <param name="count">The number of elements to take.</param>
    /// <returns>A listener that emits the taken elements as an enumerable.</returns>
    public static IRexListener<IEnumerable<T>> Take<T>(this IRexListener<IEnumerable<T>> source, int count)
    {
        return new Take<T>(source, count);
    }

    /// <summary>
    /// Skips a specified number of elements from the beginning of each enumerable value.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source listener that emits enumerables.</param>
    /// <param name="count">The number of elements to skip.</param>
    /// <returns>A listener that emits the remaining elements as an enumerable.</returns>
    public static IRexListener<IEnumerable<T>> Skip<T>(this IRexListener<IEnumerable<T>> source, int count)
    {
        return new Skip<T>(source, count);
    }

    /// <summary>
    /// Parses a string value into a data object of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the data object.</typeparam>
    /// <param name="source">The source listener that emits string identifiers.</param>
    /// <returns>A listener that emits the resolved data objects.</returns>
    public static IRexListener<TResult> ToDataObject<TResult>(this IRexListener<string> source) where TResult : class
    {
        return new ToDataObject<TResult>(source);
    }

    /// <summary>
    /// Extracts the data identifier from an object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source listener that emits objects.</param>
    /// <returns>A listener that emits the data identifiers.</returns>
    public static IRexListener<string> ToDataId<TSource>(this IRexListener<TSource> source) where TSource : class
    {
        return new ToDataId<TSource>(source);
    }

    /// <summary>
    /// Queues emissions from the source listener for deferred processing.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <returns>A listener that queues and processes emissions.</returns>
    public static IRexListener<T> Queued<T>(this IRexListener<T> source)
    {
        return new Queued<T>(source);
    }

    /// <summary>
    /// Subscribes a parameterless action to the source listener.
    /// </summary>
    /// <typeparam name="T">The type of the listener value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="action">The action to execute on each emission.</param>
    /// <returns>A handle for managing the subscription.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static IRexHandle Subscribe<T>(this IRexListener<T> source, Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return source.Subscribe(_ => action());
    }

    /// <summary>
    /// Combines two source listeners with a predicate and selector.
    /// </summary>
    /// <typeparam name="TSource1">The type of the first source value.</typeparam>
    /// <typeparam name="TSource2">The type of the second source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="predicate">A function to test if the combined values should emit.</param>
    /// <param name="selector">A function to transform the combined values.</param>
    /// <returns>A listener that emits combined results.</returns>
    public static IRexListener<TResult> Combine<TSource1, TSource2, TResult>(
        this IRexListener<TSource1> source1,
        IRexListener<TSource2> source2,
        Func<TSource1, TSource2, bool> predicate,
        Func<TSource1, TSource2, TResult> selector
        )
    {
        return new Combine<TSource1, TSource2, TResult>(source1, source2, predicate, selector);
    }

    /// <summary>
    /// Combines three source listeners with a predicate and selector.
    /// </summary>
    /// <typeparam name="TSource1">The type of the first source value.</typeparam>
    /// <typeparam name="TSource2">The type of the second source value.</typeparam>
    /// <typeparam name="TSource3">The type of the third source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="source3">The third source listener.</param>
    /// <param name="predicate">A function to test if the combined values should emit.</param>
    /// <param name="selector">A function to transform the combined values.</param>
    /// <returns>A listener that emits combined results.</returns>
    public static IRexListener<TResult> Combine<TSource1, TSource2, TSource3, TResult>(
        this IRexListener<TSource1> source1,
        IRexListener<TSource2> source2,
        IRexListener<TSource3> source3,
        Func<TSource1, TSource2, TSource3, bool> predicate,
        Func<TSource1, TSource2, TSource3, TResult> selector
        )
    {
        return new Combine<TSource1, TSource2, TSource3, TResult>(source1, source2, source3, predicate, selector);
    }

    /// <summary>
    /// Combines two source listeners with a selector (no predicate).
    /// </summary>
    /// <typeparam name="TSource1">The type of the first source value.</typeparam>
    /// <typeparam name="TSource2">The type of the second source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="selector">A function to transform the combined values.</param>
    /// <returns>A listener that emits combined results.</returns>
    public static IRexListener<TResult> Combine<TSource1, TSource2, TResult>(
        this IRexListener<TSource1> source1,
        IRexListener<TSource2> source2,
        Func<TSource1, TSource2, TResult> selector
        )
    {
        return new Combine<TSource1, TSource2, TResult>(source1, source2, null, selector);
    }

    /// <summary>
    /// Combines three source listeners with a selector (no predicate).
    /// </summary>
    /// <typeparam name="TSource1">The type of the first source value.</typeparam>
    /// <typeparam name="TSource2">The type of the second source value.</typeparam>
    /// <typeparam name="TSource3">The type of the third source value.</typeparam>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="source3">The third source listener.</param>
    /// <param name="selector">A function to transform the combined values.</param>
    /// <returns>A listener that emits combined results.</returns>
    public static IRexListener<TResult> Combine<TSource1, TSource2, TSource3, TResult>(
        this IRexListener<TSource1> source1,
        IRexListener<TSource2> source2,
        IRexListener<TSource3> source3,
        Func<TSource1, TSource2, TSource3, TResult> selector
        )
    {
        return new Combine<TSource1, TSource2, TSource3, TResult>(source1, source2, source3, null, selector);
    }

    /// <summary>
    /// Returns a listener that emits true only when both boolean sources are true.
    /// </summary>
    /// <param name="source1">The first boolean source listener.</param>
    /// <param name="source2">The second boolean source listener.</param>
    /// <returns>A listener that emits the logical AND result.</returns>
    public static IRexListener<bool> And(this IRexListener<bool> source1, IRexListener<bool> source2)
    {
        return new And(source1, source2);
    }

    /// <summary>
    /// Returns a listener that emits true when either boolean source is true.
    /// </summary>
    /// <param name="source1">The first boolean source listener.</param>
    /// <param name="source2">The second boolean source listener.</param>
    /// <returns>A listener that emits the logical OR result.</returns>
    public static IRexListener<bool> Or(this IRexListener<bool> source1, IRexListener<bool> source2)
    {
        return new Or(source1, source2);
    }

    /// <summary>
    /// Formats a single value using a format string.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="format">The format string (e.g., "{0}").</param>
    /// <returns>A listener that emits the formatted string.</returns>
    public static IRexListener<string> Format<T>(this IRexListener<T> source, string format)
    {
        return new Format<T>(source, format);
    }

    /// <summary>
    /// Formats two values using a format string.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="format">The format string (e.g., "{0} {1}").</param>
    /// <returns>A listener that emits the formatted string.</returns>
    public static IRexListener<string> Format<T1, T2>(this IRexListener<T1> source1, IRexListener<T2> source2, string format)
    {
        return new Format2<T1, T2>(source1, source2, format);
    }

    /// <summary>
    /// Formats three values using a format string.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="source1">The first source listener.</param>
    /// <param name="source2">The second source listener.</param>
    /// <param name="source3">The third source listener.</param>
    /// <param name="format">The format string (e.g., "{0} {1} {2}").</param>
    /// <returns>A listener that emits the formatted string.</returns>
    public static IRexListener<string> Format<T1, T2, T3>(this IRexListener<T1> source1, IRexListener<T2> source2, IRexListener<T3> source3, string format)
    {
        return new Format3<T1, T2, T3>(source1, source2, source3, format);
    }

    #endregion

    #region Snippets

    /// <summary>
    /// Executes actions based on a boolean source value and returns the source.
    /// </summary>
    /// <param name="source">The boolean source listener.</param>
    /// <param name="trueAction">The action to execute when the value is true.</param>
    /// <param name="falseAction">The action to execute when the value is false.</param>
    /// <returns>The original source listener.</returns>
    public static IRexListener<bool> If(this IRexListener<bool> source, Action trueAction = null, Action falseAction = null)
    {
        source.Subscribe(v =>
        {
            if (v)
            {
                trueAction?.Invoke();
            }
            else
            {
                falseAction?.Invoke();
            }
        });
        return source;
    }

    /// <summary>
    /// Executes an action when the boolean source is true and returns the source.
    /// </summary>
    /// <param name="source">The boolean source listener.</param>
    /// <param name="action">The action to execute when the value is true.</param>
    /// <returns>The original source listener.</returns>
    public static IRexListener<bool> IfTrue(this IRexListener<bool> source, Action action)
    {
        source.Subscribe(v =>
        {
            if (v)
            {
                action();
            }
        });
        return source;
    }

    /// <summary>
    /// Executes an action when the boolean source is false and returns the source.
    /// </summary>
    /// <param name="source">The boolean source listener.</param>
    /// <param name="action">The action to execute when the value is false.</param>
    /// <returns>The original source listener.</returns>
    public static IRexListener<bool> IfFalse(this IRexListener<bool> source, Action action)
    {
        source.Subscribe(v =>
        {
            if (!v)
            {
                action();
            }
        });
        return source;
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Wraps a disposable to execute a callback when disposed.
    /// </summary>
    /// <param name="disposable">The disposable to wrap.</param>
    /// <param name="callBack">The callback to execute on disposal.</param>
    /// <returns>A new disposable that executes the callback before disposing the original.</returns>
    public static IDisposable OnDispose(this IDisposable disposable, Action callBack)
    {
        return new DisposableWrapper(disposable, callBack);
    }

    /// <summary>
    /// Wraps a Rex handle to execute a callback when disposed.
    /// </summary>
    /// <param name="handle">The handle to wrap.</param>
    /// <param name="disposeCallBack">The callback to execute on disposal.</param>
    /// <returns>A new handle that executes the callback before disposing the original.</returns>
    public static IRexHandle OnRexDispose(this IRexHandle handle, Action disposeCallBack)
    {
        return new RexHandleWrapper(handle, disposeCallBack);
    }

    #endregion
}