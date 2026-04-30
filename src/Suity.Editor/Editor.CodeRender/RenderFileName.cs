using Suity.Helpers;
using Suity.Selecting;
using System.IO;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Render file name.
/// </summary>
public class RenderFileName : ISelectionItem
{
    /// <summary>
    /// Empty instance.
    /// </summary>
    public static readonly RenderFileName Empty = new(null);

    /// <summary>
    /// Base path.
    /// </summary>
    public string BasePath { get; }

    /// <summary>
    /// Default namespace.
    /// </summary>
    public string DefaultNameSpace { get; }

    /// <summary>
    /// Project-relative path.
    /// </summary>
    public string ProjectRelativePath { get; }

    /// <summary>
    /// Physical-relative path.
    /// </summary>
    public string PhysicRelativePath { get; }

    /// <summary>
    /// Project full path.
    /// </summary>
    public string ProjectFullPath { get; }

    /// <summary>
    /// Physical full path.
    /// </summary>
    public string PhysicFullPath { get; }

    private RenderFileName()
    {
        BasePath = string.Empty;
        DefaultNameSpace = string.Empty;
        ProjectRelativePath = string.Empty;
        ProjectFullPath = string.Empty;
        PhysicFullPath = string.Empty;
    }

    /// <summary>
    /// Creates a render file name with the specified base path.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    public RenderFileName(string basePath)
        : this(basePath, null, null)
    {
    }

    /// <summary>
    /// Creates a render file name with the specified base path and default namespace.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="defaultNameSpace">Default namespace.</param>
    public RenderFileName(string basePath, string defaultNameSpace)
        : this(basePath, defaultNameSpace, null)
    {
    }

    /// <summary>
    /// Creates a render file name.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="defaultNameSpace">Default namespace.</param>
    /// <param name="relativePath">Relative path.</param>
    public RenderFileName(string basePath, string defaultNameSpace, string relativePath)
    {
        basePath = (basePath ?? string.Empty).Trim('*', ':', '.', '/', '\\').Replace('\\', '/');
        defaultNameSpace = (defaultNameSpace ?? string.Empty).Trim('*', ':', '.', '/', '\\').Replace('.', '/').Replace('\\', '/');
        relativePath = (relativePath ?? string.Empty).Trim('*', ':', '.', '/', '\\').Replace('\\', '/');

        BasePath = basePath;
        DefaultNameSpace = defaultNameSpace;
        ProjectRelativePath = relativePath;

        if (!string.IsNullOrEmpty(basePath))
        {
            ProjectFullPath = $"{basePath}/{relativePath}";
        }
        else
        {
            ProjectFullPath = relativePath;
        }

        if (!string.IsNullOrEmpty(defaultNameSpace) && relativePath.StartsWith(defaultNameSpace + "/"))
        {
            PhysicRelativePath = relativePath.RemoveFromFirst(defaultNameSpace.Length + 1);
            PhysicFullPath = $"{basePath}/{PhysicRelativePath}";
        }
        else
        {
            PhysicRelativePath = ProjectRelativePath;
            PhysicFullPath = ProjectFullPath;
        }
    }

    /// <summary>
    /// Creates a render file name from a parent and relative path.
    /// </summary>
    /// <param name="parent">Parent render file name.</param>
    /// <param name="relativePath">Relative path.</param>
    public RenderFileName(RenderFileName parent, string relativePath)
        : this(parent.BasePath, parent.DefaultNameSpace, Path.Combine(parent.ProjectRelativePath, relativePath))
    {
    }

    /// <summary>
    /// Appends a relative path.
    /// </summary>
    /// <param name="relativePath">Relative path to append.</param>
    /// <returns>New render file name.</returns>
    public RenderFileName Append(string relativePath)
    {
        return new RenderFileName(this, relativePath);
    }

    /// <summary>
    /// Creates a new render file name with the specified namespace.
    /// </summary>
    /// <param name="nameSpace">Namespace.</param>
    /// <returns>New render file name.</returns>
    public RenderFileName WithNameSpace(string nameSpace)
    {
        if (string.IsNullOrWhiteSpace(nameSpace))
        {
            return this;
        }

        string path = EditorUtility.NameSpaceToPath(nameSpace);
        if (string.IsNullOrWhiteSpace(path))
        {
            return this;
        }

        return new RenderFileName(this, path);
    }

    /// <inheritdoc/>
    public override string ToString() => PhysicRelativePath;

    /// <inheritdoc/>
    public string SelectionKey => PhysicFullPath;

    /// <inheritdoc/>
    public string DisplayText => Path.GetFileName(PhysicFullPath);

    /// <inheritdoc/>
    public object Icon => EditorUtility.GetIconForFileExact(PhysicFullPath);
}