using Suity.Editor.Design;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Base class for items in a struct field list, providing description support.
/// </summary>
public class StructFieldItem : DesignField
{
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the description of this field item.
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

    /// <summary>
    /// Gets the sibling item immediately before this item.
    /// </summary>
    /// <returns>The previous sibling item, or null if none exists.</returns>
    public StructFieldItem GetSiblingPrevious()
    {
        return (List as StructFieldList)?.GetSiblingPrevious(this);
    }

    /// <summary>
    /// Gets the sibling item immediately after this item.
    /// </summary>
    /// <returns>The next sibling item, or null if none exists.</returns>
    public StructFieldItem GetSiblingNext()
    {
        return (List as StructFieldList)?.GetSiblingNext(this);
    }

    /// <inheritdoc/>
    public override void Find(ValidationContext context, string findStr, SearchOption findOption)
    {
        base.Find(context, findStr, findOption);

        if (Validator.Compare(_description, findStr, findOption))
        {
            context.Report(_description, this);
        }
    }
}
