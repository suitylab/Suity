using Suity.Editor.Services;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Extension methods for code template operations.
/// </summary>
public static class CodeTemplateExtensions
{
    /// <summary>
    /// Creates a code template from the specified text.
    /// </summary>
    /// <param name="text">The template text.</param>
    /// <returns>The created code template, or null if the service is not available.</returns>
    public static ICodeTemplate MakeCodeTemplate(string text)
    {
        ICodeTemplateService service = Device.Current.GetService<ICodeTemplateService>();
        if (service != null)
        {
            return service.MakeTemplate(text);
        }
        else
        {
            return null;
        }
    }
}