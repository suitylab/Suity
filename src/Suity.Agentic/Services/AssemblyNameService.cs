using Suity.Editor.Services;
using System.Collections.Generic;

namespace Suity.Editor;

public sealed class AssemblyNameService : IAssemblyNameService
{
    public static AssemblyNameService Instance { get; } = new();

    private AssemblyNameService()
    {
    }

    public IEnumerable<string> GetAssemblyNames(AssemblyRefLevel level) => [];
}