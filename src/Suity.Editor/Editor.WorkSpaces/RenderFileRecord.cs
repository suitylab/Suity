using System;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Record of a rendered file
/// </summary>
public abstract class RenderFileRecord
{
    /// <summary>
    /// Gets the relative file name
    /// </summary>
    public abstract string RelativeFileName { get; }
    /// <summary>
    /// Gets the old file name (before rename)
    /// </summary>
    public abstract string OldFileName { get; }
    /// <summary>
    /// Gets the last update time
    /// </summary>
    public abstract DateTime LastUpdateTime { get; }
    /// <summary>
    /// Gets whether this record is dirty
    /// </summary>
    public abstract bool Dirty { get; }
}