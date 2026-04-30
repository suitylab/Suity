using System.Collections.Generic;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Render target library.
/// </summary>
public interface IRenderTargetLibrary : IHasId
{
    /// <summary>
    /// Gets render targets.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="caller">Caller object.</param>
    /// <param name="resolveContext">Condition context.</param>
    /// <returns>Render targets.</returns>
    IEnumerable<RenderTarget> GetRenderTargets(RenderFileName basePath, object caller, ICondition resolveContext);
}