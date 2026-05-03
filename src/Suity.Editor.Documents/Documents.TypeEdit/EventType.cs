using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Types;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents an event type in the type design document.
/// </summary>
[NativeAlias]
[DisplayText("Event", "*CoreIcon|Event")]
[DisplayOrder(960)]
public class EventType : TypeDesignItem<DEventBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventType"/> class.
    /// </summary>
    public EventType()
    {
        ShowRenderTargets = true;
        ShowUsings = true;
    }

    /// <inheritdoc/>
    public override ImageDef TypeIcon => CoreIconCache.Event;

    /// <inheritdoc/>
    public override Color? TypeColor => DEvent.EventTypeColor;

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "Event";

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon()
    {
        return base.OnGetIcon() ?? CoreIconCache.Event;
    }

    /// <inheritdoc/>
    public override string PreviewText => "Event";
}
