using Suity.Editor.CodeRender;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Extension methods for workspace-related operations on render targets.
/// </summary>
public static class WorkSpaceExtensions
{
    /// <summary>
    /// Converts a render target to a render file record.
    /// </summary>
    /// <param name="target">The render target to convert.</param>
    /// <returns>A new <see cref="RenderFileRecordBK"/> instance.</returns>
    public static RenderFileRecordBK ToRecord(this RenderTarget target)
    {
        return new RenderFileRecordBK(target.FileName.ProjectRelativePath, target.LastUpdateTime);
    }

    /// <summary>
    /// Gets the workspace associated with a render target via its tag.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <returns>The workspace if the tag is an <see cref="IWorkSpaceRefItem"/>; otherwise, null.</returns>
    public static WorkSpace GetWorkSpace(this RenderTarget target)
    {
        return (target.Tag as IWorkSpaceRefItem)?.WorkSpace;
    }
}