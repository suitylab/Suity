using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents a struct type in the type design document.
/// </summary>
[NativeAlias]
[DisplayText("Struct", "*CoreIcon|Box")]
[DisplayOrder(1000)]
public class StructType : StructTypeBase<DStructBuilder>,
    IVariableContainer
{
    private readonly StructFieldList _fieldList;

    private VariableOwnerTextBlock _brief = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StructType"/> class.
    /// </summary>
    public StructType()
    {
        _fieldList = new StructFieldList("Parameters", this);
        AddPrimaryFieldList(_fieldList);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StructType"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the struct.</param>
    public StructType(string name)
        : this()
    {
        Name = name;
    }

    /// <summary>
    /// Gets the struct builder for this type.
    /// </summary>
    internal new DStructBuilder AssetBuilder => base.AssetBuilder;

    /// <inheritdoc/>
    public override Color? TypeColor => DStruct.StructTypeColor;

    /// <inheritdoc/>
    public override Image TypeIcon => CoreIconCache.Box;

    /// <inheritdoc/>
    protected override void OnIsValueTypeChanged()
    {
        base.OnIsValueTypeChanged();

        AssetBuilder.SetIsValueStruct(IsValueType);
    }

    /// <inheritdoc/>
    protected override void OnBaseTypeChanged()
    {
        base.OnBaseTypeChanged();

        AssetBuilder.UpdateBaseType(BaseType?.Id ?? Guid.Empty);
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync(_fieldList.FieldName, _fieldList, SyncFlag.GetOnly);

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

        setup.InspectorField(_brief, new ViewProperty("Brief", "Summary").WithToolTips("Can use {} to include field names"));
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

    /// <inheritdoc/>
    protected override Image OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Box;

    /// <summary>
    /// Gets the field list for this struct type.
    /// </summary>
    protected internal new SNamedFieldList FieldList => _fieldList;
}
