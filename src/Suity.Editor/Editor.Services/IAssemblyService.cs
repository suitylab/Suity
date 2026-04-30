using System.Collections.Generic;
using System.Reflection;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for managing assemblies.
/// </summary>
public interface IAssemblyService
{
    /// <summary>
    /// Gets all registered assemblies.
    /// </summary>
    IEnumerable<Assembly> RegisteredAssemblies { get; }

    /// <summary>
    /// Checks if an assembly is registered.
    /// </summary>
    /// <param name="asm">The assembly to check.</param>
    /// <returns>True if registered.</returns>
    bool ContainsAssembly(Assembly asm);
}

/// <summary>
/// Empty implementation of the assembly service.
/// </summary>
public sealed class EmptyAssemblyService : IAssemblyService
{
    /// <summary>
    /// Gets the singleton instance of EmptyAssemblyService.
    /// </summary>
    public static readonly EmptyAssemblyService Empty = new();

    private EmptyAssemblyService()
    { }

    /// <inheritdoc/>
    public IEnumerable<Assembly> RegisteredAssemblies => [];

    /// <inheritdoc/>
    public bool ContainsAssembly(Assembly asm)
    {
        return false;
    }
}