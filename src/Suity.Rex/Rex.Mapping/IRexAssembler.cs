using System;

namespace Suity.Rex.Mapping;

/// <summary>
/// Defines an assembler that creates results from target objects.
/// </summary>
/// <typeparam name="T">The type of result to assemble.</typeparam>
public interface IRexAssembler<T>
{
    /// <summary>
    /// Assembles a result from the target object.
    /// </summary>
    /// <param name="target">The target object to assemble from.</param>
    /// <param name="name">The name identifier for the assembly.</param>
    /// <returns>The assembled result, or null if not available.</returns>
    T Assemble(object target, string name);
}

/// <summary>
/// Delegate for assembling results from target objects.
/// </summary>
/// <typeparam name="T">The type of result to assemble.</typeparam>
/// <param name="target">The target object to assemble from.</param>
/// <param name="name">The name identifier for the assembly.</param>
/// <returns>The assembled result.</returns>
public delegate T RexAssembleDelegate<T>(object target, string name);

/// <summary>
/// An assembler implementation that wraps an assemble delegate.
/// </summary>
/// <typeparam name="T">The type of result to assemble.</typeparam>
public class RexAssembler<T> : IRexAssembler<T>
{
    private readonly RexAssembleDelegate<T> _assemble;

    /// <summary>
    /// Initializes a new instance of the <see cref="RexAssembler{T}"/> class.
    /// </summary>
    /// <param name="assemble">The assemble delegate.</param>
    public RexAssembler(RexAssembleDelegate<T> assemble)
    {
        _assemble = assemble ?? throw new ArgumentNullException(nameof(assemble));
    }

    /// <inheritdoc/>
    public T Assemble(object target, string name)
    {
        return _assemble(target, name);
    }
}