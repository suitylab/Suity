using Suity.Drawing;
using Suity.Properties;

namespace Suity.Views.Im;

/// <summary>
/// Provides built-in bitmap icons used throughout the ImGui rendering system.
/// All icons are loaded from embedded resources.
/// </summary>
public static class ImGuiIcons
{
    /// <summary>Icon representing an add action.</summary>
    public static BitmapDef Add { get; } = new BitmapDef(Resources.Add);
    /// <summary>Icon representing an attribute.</summary>
    public static BitmapDef Attribute { get; } = new BitmapDef(Resources.Attribute);
    /// <summary>Icon representing a cancel action.</summary>
    public static BitmapDef Cancel { get; } = new BitmapDef(Resources.Cancel);
    /// <summary>Icon representing a checked state.</summary>
    public static BitmapDef Checked { get; } = new BitmapDef(Resources.Checked);
    /// <summary>Icon representing a clone action.</summary>
    public static BitmapDef Clone { get; } = new BitmapDef(Resources.Clone);
    /// <summary>Icon representing a collapsed state.</summary>
    public static BitmapDef Collapse { get; } = new BitmapDef(Resources.Collapse);
    /// <summary>Icon representing a column.</summary>
    public static BitmapDef Column { get; } = new BitmapDef(Resources.Column);
    /// <summary>Icon representing a condition.</summary>
    public static BitmapDef Condition { get; } = new BitmapDef(Resources.Condition);
    /// <summary>Icon representing a delete action.</summary>
    public static BitmapDef Delete { get; } = new BitmapDef(Resources.Delete);
    /// <summary>Empty icon used as a placeholder.</summary>
    public static BitmapDef Empty { get; } = new BitmapDef(Resources.Empty);
    /// <summary>Icon representing an expanded state.</summary>
    public static BitmapDef Expand { get; } = new BitmapDef(Resources.Expand);
    /// <summary>Icon representing a label.</summary>
    public static BitmapDef Label { get; } = new BitmapDef(Resources.Label);
    /// <summary>Icon representing more options.</summary>
    public static BitmapDef More { get; } = new BitmapDef(Resources.More);
    /// <summary>Icon representing an open action.</summary>
    public static BitmapDef Open { get; } = new BitmapDef(Resources.Open);
    /// <summary>Icon representing a pending state.</summary>
    public static BitmapDef Pending { get; } = new BitmapDef(Resources.Pending);
    /// <summary>Icon representing a play action.</summary>
    public static BitmapDef Play { get; } = new BitmapDef(Resources.Play);
    /// <summary>Icon representing a row.</summary>
    public static BitmapDef Row { get; } = new BitmapDef(Resources.Row);
    /// <summary>Icon representing sort descending.</summary>
    public static BitmapDef SortDown { get; } = new BitmapDef(Resources.SortDown);
    /// <summary>Icon representing sort ascending.</summary>
    public static BitmapDef SortUp { get; } = new BitmapDef(Resources.SortUp);
    /// <summary>Icon representing a switch action.</summary>
    public static BitmapDef Switch { get; } = new BitmapDef(Resources.Switch);
    /// <summary>Icon representing a warning.</summary>
    public static BitmapDef Warning { get; } = new BitmapDef(Resources.Warning);
}
