using Suity.Editor.Services;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Provides helper methods for code rendering.
/// </summary>
public static class RenderHelper
{
    private static readonly ServiceStore<ICodeRenderService> _renderService = new();

    /// <summary>
    /// Gets the segment configuration for a render target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <returns>The segment configuration.</returns>
    public static CodeSegmentConfig GetSegmentConfig(this RenderTarget target)
        => _renderService.Get()?.GetLanguage(target.Language)?.SegmentConfig ?? CodeSegmentConfig.CsDefault;

    /// <summary>
    /// Gets the render language for a render target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <returns>The render language, or null if not found.</returns>
    public static IRenderLanguage GetLanguage(this RenderTarget target)
        => _renderService.Get()?.GetLanguage(target.Language);

    /// <summary>
    /// Gets the segment configuration for a language by name.
    /// </summary>
    /// <param name="languageName">The language name.</param>
    /// <returns>The segment configuration.</returns>
    public static CodeSegmentConfig GetSegmentConfig(string languageName)
        => _renderService.Get()?.GetLanguage(languageName)?.SegmentConfig ?? CodeSegmentConfig.CsDefault;

    /// <summary>
    /// Gets the render language by name.
    /// </summary>
    /// <param name="languageName">The language name.</param>
    /// <returns>The render language, or null if not found.</returns>
    public static IRenderLanguage GetLanguage(string languageName)
        => _renderService.Get()?.GetLanguage(languageName);

    /// <summary>
    /// Creates a text render result.
    /// </summary>
    /// <param name="status">The render status.</param>
    /// <param name="data">The text data.</param>
    /// <returns>The render result.</returns>
    public static RenderResult CreateTextRenderResult(RenderStatus status, string data)
        => _renderService.Get()?.CreateTextRenderResult(status, data);

    /// <summary>
    /// Creates a binary render result.
    /// </summary>
    /// <param name="status">The render status.</param>
    /// <param name="data">The binary data.</param>
    /// <returns>The render result.</returns>
    public static RenderResult CreateBinaryRenderResult(RenderStatus status, byte[] data)
        => _renderService.Get()?.CreateBinaryRenderResult(status, data);
}