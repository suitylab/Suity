using System;

namespace Suity.Rex;

/// <summary>
/// Base class for implementing a reactive listener with a single source.
/// </summary>
/// <typeparam name="TSource">The type of the source value.</typeparam>
/// <typeparam name="TResult">The type of the result value emitted to subscribers.</typeparam>
public class RexListenerBase<TSource, TResult> : IRexListener<TResult>
{
    internal readonly IRexListener<TSource> _source;
    internal Action<TResult> _callBack;

    /// <summary>
    /// Initializes a new instance with the specified source listener.
    /// </summary>
    /// <param name="source">The source listener to subscribe to. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public RexListenerBase(IRexListener<TSource> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <inheritdoc/>
    public IRexHandle Subscribe(Action<TResult> callBack)
    {
        _callBack += callBack;

        return this;
    }

    /// <summary>
    /// Invokes all registered callbacks with the specified result.
    /// </summary>
    /// <param name="result">The result value to emit.</param>
    internal void HandleCallBack(TResult result)
    {
        _callBack?.Invoke(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _callBack = null;
        _source.Dispose();
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        _source.Push();

        return this;
    }
}

/// <summary>
/// Base class for implementing a reactive listener with two sources.
/// </summary>
/// <typeparam name="TSource1">The type of the first source value.</typeparam>
/// <typeparam name="TSource2">The type of the second source value.</typeparam>
/// <typeparam name="TResult">The type of the result value emitted to subscribers.</typeparam>
internal class RexListenerBase<TSource1, TSource2, TResult> : IRexListener<TResult>
{
    internal readonly IRexListener<TSource1> _source1;
    internal readonly IRexListener<TSource2> _source2;
    internal Action<TResult> _callBack;

    /// <summary>
    /// Initializes a new instance with the specified source listeners.
    /// </summary>
    /// <param name="source1">The first source listener. Must not be null.</param>
    /// <param name="source2">The second source listener. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when either source is null.</exception>
    public RexListenerBase(IRexListener<TSource1> source1, IRexListener<TSource2> source2)
    {
        _source1 = source1 ?? throw new ArgumentNullException(nameof(source1));
        _source2 = source2 ?? throw new ArgumentNullException(nameof(source2));
    }

    /// <inheritdoc/>
    public IRexHandle Subscribe(Action<TResult> callBack)
    {
        _callBack += callBack;

        return this;
    }

    /// <summary>
    /// Invokes all registered callbacks with the specified result.
    /// </summary>
    /// <param name="result">The result value to emit.</param>
    internal void HandleCallBack(TResult result)
    {
        _callBack?.Invoke(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _callBack = null;
        _source1.Dispose();
        _source2.Dispose();
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        _source1.Push();
        _source2.Push();

        return this;
    }
}

/// <summary>
/// Base class for implementing a reactive listener with three sources.
/// </summary>
/// <typeparam name="TSource1">The type of the first source value.</typeparam>
/// <typeparam name="TSource2">The type of the second source value.</typeparam>
/// <typeparam name="TSource3">The type of the third source value.</typeparam>
/// <typeparam name="TResult">The type of the result value emitted to subscribers.</typeparam>
internal class RexListenerBase<TSource1, TSource2, TSource3, TResult> : IRexListener<TResult>
{
    internal readonly IRexListener<TSource1> _source1;
    internal readonly IRexListener<TSource2> _source2;
    internal readonly IRexListener<TSource3> _source3;
    internal Action<TResult> _callBack;

    /// <summary>
    /// Initializes a new instance with the specified source listeners.
    /// </summary>
    /// <param name="source1">The first source listener. Must not be null.</param>
    /// <param name="source2">The second source listener. Must not be null.</param>
    /// <param name="source3">The third source listener. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when any source is null.</exception>
    public RexListenerBase(IRexListener<TSource1> source1, IRexListener<TSource2> source2, IRexListener<TSource3> source3)
    {
        _source1 = source1 ?? throw new ArgumentNullException(nameof(source1));
        _source2 = source2 ?? throw new ArgumentNullException(nameof(source2));
        _source3 = source3 ?? throw new ArgumentNullException(nameof(source3));
    }

    /// <inheritdoc/>
    public IRexHandle Subscribe(Action<TResult> callBack)
    {
        _callBack += callBack;

        return this;
    }

    /// <summary>
    /// Invokes all registered callbacks with the specified result.
    /// </summary>
    /// <param name="result">The result value to emit.</param>
    internal void HandleCallBack(TResult result)
    {
        _callBack?.Invoke(result);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _callBack = null;
        _source1.Dispose();
        _source2.Dispose();
        _source3.Dispose();
    }

    /// <inheritdoc/>
    public IRexHandle Push()
    {
        _source1.Push();
        _source2.Push();
        _source3.Push();

        return this;
    }
}