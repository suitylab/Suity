using System.Collections.Generic;

namespace Suity.Editor.Libraries;

/// <summary>
/// Represents an entry within a library archive, containing location, index, and optional child entries.
/// </summary>
internal class LibraryEntry
{
    /// <summary>
    /// Gets or sets the location/path of the entry within the library.
    /// </summary>
    public string Location;

    /// <summary>
    /// Gets or sets the index of the entry within the archive file.
    /// </summary>
    public int Index;

    /// <summary>
    /// Gets or sets the asset associated with this entry.
    /// </summary>
    public Asset Asset;

    /// <summary>
    /// Gets or sets the list of child entries for bunch-type entries.
    /// </summary>
    public List<LibraryEntry> ChildEntries;
}
