using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents the type of an event argument.
/// </summary>
public class EventArgumentType : DesignItem<DEventArgumentBuilder>, INavigable
{
    private readonly FieldTypeDesign _argumentType;

    /// <summary>
    /// Gets the field type design for the event argument.
    /// </summary>
    public FieldTypeDesign ArgumentType => _argumentType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventArgumentType"/> class.
    /// </summary>
    public EventArgumentType()
    {
        _argumentType = new FieldTypeDesign(this, true);
        ShowRenderTargets = true;
        ShowUsings = true;
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix()
    {
        return "EventArgument";
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        sync.Sync("Type", ArgumentType, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
        setup.InspectorField(ArgumentType, new ViewProperty("Type", "Type") { Expand = true });
    }

    /// <inheritdoc/>
    protected override Image OnGetIcon()
    {
        return base.OnGetIcon() ?? Suity.Editor.CoreIconCache.EventArgument;
    }

    /// <inheritdoc/>
    public override string PreviewText => ArgumentType.ToString();

    #region INavigable

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return ArgumentType.FieldType;
    }

    #endregion
}
