using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents an abstract struct type that can be implemented by other struct types.
/// </summary>
[NativeAlias("AbstractType", UseForSaving = true)]
[NativeAlias("Suity.Editor.Documents.TypeEdit.SideType", UseForSaving = false)]
[DisplayText("Abstract", "*CoreIcon|Abstract")]
[DisplayOrder(980)]
public class AbstractType : StructTypeBase<DAbstractBuilder>
{
    private readonly StructFieldList _fieldList;

    private readonly FieldTypeDesign _type;

    private VariableOwnerTextBlock _brief = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractType"/> class.
    /// </summary>
    public AbstractType()
    {
        _fieldList = new StructFieldList("Parameters", this);
        AddPrimaryFieldList(_fieldList);

        _type = new FieldTypeDesign(this, true);
        ShowRenderTargets = true;
        ShowUsings = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractType"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the abstract type.</param>
    public AbstractType(string name)
        : this()
    {
        _type = new FieldTypeDesign(this, true);
        Name = name;
    }

    /// <summary>
    /// Gets the abstract builder for this type.
    /// </summary>
    internal new DAbstractBuilder AssetBuilder => base.AssetBuilder;

    /// <inheritdoc/>
    protected override bool AbstractEnabled => false;

    /// <summary>
    /// Gets the type design for this abstract type.
    /// </summary>
    public FieldTypeDesign Type => _type;

    
    /// <inheritdoc/>
    public override string PreviewText => "Abstract Struct";

    /// <inheritdoc/>
    public override Color? TypeColor => DAbstract.AbstractTypeColor;

    /// <inheritdoc/>
    public override Image TypeIcon => CoreIconCache.Abstract;

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix()
    {
        return "Abstract";
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync(_fieldList.FieldName, _fieldList, SyncFlag.GetOnly);

        sync.Sync("Type", Type, SyncFlag.GetOnly);

        _brief = sync.Sync("Brief", _brief, SyncFlag.NotNull);
        _brief.VariableOwner = this;
        if (sync.IsSetterOf("Brief"))
        {
            AssetBuilder.SetBrief(_brief.Text);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.DetailTreeViewField(_fieldList, new ViewProperty(_fieldList.FieldName, "Fields"));

        base.OnSetupView(setup);

        if (setup.IsTypeSupported(this.GetType()))
        {
            setup.InspectorField(Type, new ViewProperty("Type", "Type") { Expand = true });
        }

        setup.InspectorField(_brief, new ViewProperty("Brief", "Summary").WithToolTips("Can use {} to include field names"));
    }

    /// <inheritdoc/>
    protected override Image OnGetIcon()
    {
        return base.OnGetIcon() ?? CoreIconCache.Abstract;
    }

    /// <inheritdoc/>
    protected override void OnBaseTypeChanged()
    {
        base.OnBaseTypeChanged();

        AssetBuilder.UpdateBaseType(BaseType?.Id ?? Guid.Empty);
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRenamed(NamedFieldList list, NamedField item, string oldName)
    {
        base.OnFieldListItemRenamed(list, item, oldName);

        if (item is StructField p)
        {
            if (_brief.Rename(oldName, p.Name))
            {
                AssetBuilder.SetBrief(_brief.Text);
            }
        }
    }
}
