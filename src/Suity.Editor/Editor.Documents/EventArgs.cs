using System;

namespace Suity.Editor.Documents;

/// <summary>
/// Event arguments for dirty state change events.
/// </summary>
public class DirtyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the object that marked the document as dirty.
    /// </summary>
    public object DirtyMarker { get; }

    /// <summary>
    /// Initializes a new instance of the DirtyEventArgs class.
    /// </summary>
    /// <param name="dirtyMarker">The object that caused the dirty state.</param>
    public DirtyEventArgs(object dirtyMarker)
    {
        DirtyMarker = dirtyMarker;
    }
}