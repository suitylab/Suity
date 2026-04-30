using System.Reflection;

namespace Suity.Reflecting;

/// <summary>
/// Provides common BindingFlags combinations for reflection operations.
/// </summary>
public static class BindingFlagsPreset
{
    /// <summary>
    /// Equals <c>BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic</c>.
    /// </summary>
    public const BindingFlags BindInstanceAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Equals <c>BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic</c>.
    /// </summary>
    public const BindingFlags BindStaticAll = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Equals <c>BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic</c>.
    /// </summary>
    public const BindingFlags BindAll = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
}