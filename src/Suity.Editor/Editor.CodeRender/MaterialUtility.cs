namespace Suity.Editor.CodeRender;

/// <summary>
/// Provides default materials for code rendering.
/// </summary>
public static class MaterialUtility
{
    /// <summary>
    /// Default text material.
    /// </summary>
    public static IMaterial DefaultTextMaterial { get; internal set; }

    /// <summary>
    /// Default binary material.
    /// </summary>
    public static IMaterial DefaultBinaryMaterial { get; internal set; }

    /// <summary>
    /// C# material.
    /// </summary>
    public static IMaterial CSharpMaterial { get; internal set; }

    /// <summary>
    /// C# single file material.
    /// </summary>
    public static IMaterial CSharpSingleMaterial { get; internal set; }

    /// <summary>
    /// C# formatter material.
    /// </summary>
    public static IMaterial CSharpFormatterMaterial { get; internal set; }

    /// <summary>
    /// JavaScript material.
    /// </summary>
    public static IMaterial JavaScriptMaterial { get; internal set; }

    /// <summary>
    /// JSON material.
    /// </summary>
    public static IMaterial JsonMaterial { get; internal set; }
}