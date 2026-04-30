using Suity.Properties;
using System.Drawing;
using System.IO;

namespace Suity.Views.Im;

/// <summary>
/// Provides built-in bitmap icons used throughout the ImGui rendering system.
/// All icons are loaded from embedded resources.
/// </summary>
public static class ImGuiIcons
{
    /// <summary>Icon representing an add action.</summary>
    public static Bitmap Add { get; } = new Bitmap(new MemoryStream(Resources.Add));
    /// <summary>Icon representing an attribute.</summary>
    public static Bitmap Attribute { get; } = new Bitmap(new MemoryStream(Resources.Attribute));
    /// <summary>Icon representing a cancel action.</summary>
    public static Bitmap Cancel { get; } = new Bitmap(new MemoryStream(Resources.Cancel));
    /// <summary>Icon representing a checked state.</summary>
    public static Bitmap Checked { get; } = new Bitmap(new MemoryStream(Resources.Checked));
    /// <summary>Icon representing a clone action.</summary>
    public static Bitmap Clone { get; } = new Bitmap(new MemoryStream(Resources.Clone));
    /// <summary>Icon representing a collapsed state.</summary>
    public static Bitmap Collapse { get; } = new Bitmap(new MemoryStream(Resources.Collapse));
    /// <summary>Icon representing a column.</summary>
    public static Bitmap Column { get; } = new Bitmap(new MemoryStream(Resources.Column));
    /// <summary>Icon representing a condition.</summary>
    public static Bitmap Condition { get; } = new Bitmap(new MemoryStream(Resources.Condition));
    /// <summary>Icon representing a delete action.</summary>
    public static Bitmap Delete { get; } = new Bitmap(new MemoryStream(Resources.Delete));
    /// <summary>Empty icon used as a placeholder.</summary>
    public static Bitmap Empty { get; } = new Bitmap(new MemoryStream(Resources.Empty));
    /// <summary>Icon representing an expanded state.</summary>
    public static Bitmap Expand { get; } = new Bitmap(new MemoryStream(Resources.Expand));
    /// <summary>Icon representing a label.</summary>
    public static Bitmap Label { get; } = new Bitmap(new MemoryStream(Resources.Label));
    /// <summary>Icon representing more options.</summary>
    public static Bitmap More { get; } = new Bitmap(new MemoryStream(Resources.More));
    /// <summary>Icon representing an open action.</summary>
    public static Bitmap Open { get; } = new Bitmap(new MemoryStream(Resources.Open));
    /// <summary>Icon representing a pending state.</summary>
    public static Bitmap Pending { get; } = new Bitmap(new MemoryStream(Resources.Pending));
    /// <summary>Icon representing a play action.</summary>
    public static Bitmap Play { get; } = new Bitmap(new MemoryStream(Resources.Play));
    /// <summary>Icon representing a row.</summary>
    public static Bitmap Row { get; } = new Bitmap(new MemoryStream(Resources.Row));
    /// <summary>Icon representing sort descending.</summary>
    public static Bitmap SortDown { get; } = new Bitmap(new MemoryStream(Resources.SortDown));
    /// <summary>Icon representing sort ascending.</summary>
    public static Bitmap SortUp { get; } = new Bitmap(new MemoryStream(Resources.SortUp));
    /// <summary>Icon representing a switch action.</summary>
    public static Bitmap Switch { get; } = new Bitmap(new MemoryStream(Resources.Switch));
    /// <summary>Icon representing a warning.</summary>
    public static Bitmap Warning { get; } = new Bitmap(new MemoryStream(Resources.Warning));
}
