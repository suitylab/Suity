using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Solution reference level
/// </summary>
public enum AssemblyRefLevel
{
    None,
    RuntimeMin,
    Runtime,
    ServerMin,
    Server,
    Editor,
    Full,
}

/// <summary>
/// C# project settings
/// </summary>
public class CSharpProjectSetting
{
    public AssemblyRefLevel RefLevel = AssemblyRefLevel.None;
    public string FrameworkVersion = "v4.6.1";
    public string ProjectTemplate;
    public string AssemblyInfoTemplate;

    public bool IncludeNonCsFile = true;
    public List<string> ExceptedFiles = null;
    public List<CSharpLibReference> References = null;
}

/// <summary>
/// C# library reference
/// </summary>
public class CSharpLibReference
{
    public string Name;
    public string HintPath;
}

/// <summary>
/// Managed solution service
/// </summary>
public interface IAssemblyNameService
{
    /// <summary>
    /// Get Suity assembly name
    /// </summary>
    /// <param name="level">Suity assembly reference level</param>
    /// <returns></returns>
    IEnumerable<string> GetAssemblyNames(AssemblyRefLevel level);
}