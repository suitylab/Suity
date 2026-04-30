using Suity.Editor.Design;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Base class for items in an enum field list, providing description support.
/// </summary>
public class EnumItemBase : DesignField
{
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the description of this enum item.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            value ??= string.Empty;

            if (_description != value)
            {
                _description = value;
                OnDescriptionChanged();
                NotifyFieldUpdated();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Description = sync.Sync("Description", Description, SyncFlag.NotNull);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(_description, new ViewProperty("Description", "Description"));
        }
    }

    /// <summary>
    /// Called when the <see cref="Description"/> property changes.
    /// </summary>
    protected virtual void OnDescriptionChanged()
    {
    }
}
