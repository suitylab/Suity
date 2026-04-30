using Suity.Editor.CodeRender;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents an event type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Event, "Event")]
public class DEvent : DType, ICodeRenderElement
{
    /// <summary>
    /// Gets the default color for event types.
    /// </summary>
    public static Color EventTypeColor { get; } = Color.FromArgb(255, 182, 0);

    /// <summary>
    /// The color code for events.
    /// </summary>
    public const string EventColorCode = "#FFB600";

    /// <summary>
    /// The icon key for event types.
    /// </summary>
    public const string DEventIconKey = "*CoreIcon|Event";

    /// <summary>
    /// Initializes a new instance of the DEvent class.
    /// </summary>
    public DEvent()
    { }

    /// <summary>
    /// Initializes a new instance of the DEvent class with a name.
    /// </summary>
    public DEvent(string name)
        : base(name)
    {
    }

    /// <inheritdoc />
    public override Image DefaultIcon => CoreIconCache.Event;
}

/// <summary>
/// Builder for creating DEvent instances.
/// </summary>
public class DEventBuilder : DTypeBuilder<DEvent>
{
}