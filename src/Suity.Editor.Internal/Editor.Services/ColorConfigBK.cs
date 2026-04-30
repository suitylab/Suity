namespace Suity.Editor.Services;

/// <summary>
/// Backend implementation of the editor color configuration.
/// </summary>
internal class ColorConfigBK : DefaultEditorColorConfig
{
    /// <summary>
    /// Singleton instance of the color configuration.
    /// </summary>
    public static readonly ColorConfigBK Instance = new();
}
