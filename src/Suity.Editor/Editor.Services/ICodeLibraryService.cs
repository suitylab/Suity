using Suity.Editor.CodeRender;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for managing code libraries.
/// </summary>
public interface ICodeLibraryService
{
    /// <summary>
    /// Stores user code for a file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="renderTargets">The render targets.</param>
    void StoreUserCode(string fileName, IEnumerable<RenderTarget> renderTargets);

    /// <summary>
    /// Stores user code in a code library.
    /// </summary>
    /// <param name="codeLibrary">The code library.</param>
    /// <param name="renderTargets">The render targets.</param>
    void StoreUserCode(ICodeLibrary codeLibrary, IEnumerable<RenderTarget> renderTargets);
}