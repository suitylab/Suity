using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// System clipboard interface for text operations.
/// </summary>
public interface ISystemClipboard
{
    /// <summary>
    /// Sets text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to set.</param>
    /// <returns>True if successful.</returns>
    Task<bool> SetText(string text);

    /// <summary>
    /// Gets text from the system clipboard.
    /// </summary>
    /// <returns>The clipboard text.</returns>
    Task<string> GetText();
}

/// <summary>
/// Editor clipboard service interface.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Sets data to the clipboard.
    /// </summary>
    /// <param name="data">The data to set.</param>
    /// <param name="isCopy">Whether this is a copy (vs cut).</param>
    /// <param name="extraInfo">Optional extra information.</param>
    void SetData(object data, bool isCopy, object extraInfo = null);

    /// <summary>
    /// Gets whether the clipboard operation was a copy.
    /// </summary>
    bool IsCopy { get; }

    /// <summary>
    /// Gets clipboard items.
    /// </summary>
    /// <returns>An enumerable of clipboard items.</returns>
    IEnumerable<ClipboardItem> GetDatas();

    /// <summary>
    /// Gets extra information.
    /// </summary>
    object ExtraInfo { get; }

    /// <summary>
    /// Clears the clipboard.
    /// </summary>
    void Clear();
}

/// <summary>
/// Represents a clipboard item.
/// </summary>
public class ClipboardItem
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Data?.ToString() ?? "null";
    }
}