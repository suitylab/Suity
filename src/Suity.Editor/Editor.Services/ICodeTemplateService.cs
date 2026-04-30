using Suity.Editor.CodeRender;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for code template operations.
/// </summary>
public interface ICodeTemplateService
{
    /// <summary>
    /// Creates a code template from text.
    /// </summary>
    /// <param name="text">The template text.</param>
    /// <returns>A code template instance.</returns>
    ICodeTemplate MakeTemplate(string text);
}