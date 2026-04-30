using System;

namespace Suity.Rex;

/// <summary>
/// A singleton <see cref="IDisposable"/> that performs no action when disposed.
/// Used as a no-op return value when a disposable is required but no cleanup is needed.
/// </summary>
public sealed class EmptyDisposable : IDisposable
{
    /// <summary>
    /// Gets the singleton empty disposable instance.
    /// </summary>
    public static readonly EmptyDisposable Empty = new();

    private EmptyDisposable()
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}