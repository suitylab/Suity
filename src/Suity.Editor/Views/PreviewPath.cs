using Suity.Synchonizing.Core;

namespace Suity.Views;

/// <summary>
/// Represents a preview path with optional display information.
/// </summary>
public class PreviewPath
{
    /// <summary>
    /// Gets the synchronization path.
    /// </summary>
    public SyncPath Path { get; }

    /// <summary>
    /// Gets the name of the preview path.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display name of the preview path.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PreviewPath"/> with the specified path and optional names.
    /// </summary>
    /// <param name="path">The synchronization path.</param>
    /// <param name="name">The name of the preview path, or null to use default.</param>
    /// <param name="displayName">The display name of the preview path, or null to use default.</param>
    public PreviewPath(SyncPath path, string name = null, string displayName = null)
    {
        Path = path ?? SyncPath.Empty;
        Name = name;
        DisplayName = displayName;
    }

    /// <summary>
    /// Returns a string representation of the preview path.
    /// </summary>
    /// <returns>The display name, name, or base string representation.</returns>
    public override string ToString() => DisplayName ?? Name ?? base.ToString();
}