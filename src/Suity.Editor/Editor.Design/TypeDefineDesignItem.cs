using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for design items that define a type.
/// </summary>
public abstract class TypeDefineDesignItem : DesignItem, INavigable
{
    /// <summary>
    /// Gets the type design for this item.
    /// </summary>
    public ITypeDesign FieldType { get; }

    /// <summary>
    /// Gets or sets the default value for this field.
    /// </summary>
    public object DefaultValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDefineDesignItem"/> class.
    /// </summary>
    protected TypeDefineDesignItem()
    {
        FieldType = DTypeManager.Instance.CreateTypeDesign(this);
        FieldType.BaseType.SelectedKey = "*System|String";
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Type", FieldType, SyncFlag.GetOnly | SyncFlag.AffectsOthers);
        DefaultValue = sync.Sync("DefaultValue", DefaultValue);

        UpdateDefaultValue();
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(FieldType, new ViewProperty("Type", "Type") { Expand = true });

        UpdateDefaultValue();

        setup.InspectorFieldOfType(FieldType.FieldType, new ViewProperty("DefaultValue", "Default Value"));
    }

    private void UpdateDefaultValue()
    {
        DefaultValue = FieldType.SyncDefaultValue(DefaultValue, this.GetAssetFilter());
    }

    protected override TextStatus OnGetTextStatus()
    {
        if (FieldType.BaseType.GetDType() != null)
        {
            return TextStatus.Normal;
        }
        else
        {
            return TextStatus.Error;
        }
    }

    protected override Image OnGetIcon() => base.OnGetIcon() ?? CoreIconCache.Field;

    public override string PreviewText
    {
        get
        {
            if (DefaultValue != null)
            {
                if (DefaultValue is string)
                {
                    if (!string.IsNullOrEmpty(DefaultValue as string))
                    {
                        return $"{FieldType} = {DefaultValue}";
                    }
                    else
                    {
                        return FieldType.ToString();
                    }
                }
                else
                {
                    return $"{FieldType} = {DefaultValue}";
                }
            }
            else
            {
                return FieldType.ToString();
            }
        }
    }

    public override object PreviewIcon => FieldType.Icon;

    #region INavigable

    object INavigable.GetNavigationTarget() => FieldType.FieldType;

    #endregion
}