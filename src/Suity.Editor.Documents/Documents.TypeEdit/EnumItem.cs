using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents an individual enum item with a name, value, and description.
/// </summary>
[NativeAlias]
[DisplayText("Enum item", "*CoreIcon|EnumField")]
public class EnumItem : EnumItemBase, IMember, IDescriptionDisplay, IPreviewDisplay
{
    private int _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumItem"/> class.
    /// </summary>
    public EnumItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumItem"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the enum item.</param>
    public EnumItem(string name)
        : this()
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets the parent enum type that contains this item.
    /// </summary>
    public EnumType ParentEnum => ParentSItem as EnumType;

    /// <inheritdoc/>
    public override EditorObject AssetField => ParentEnum?.AssetBuilder?.Asset?.GetField(Name);

    /// <summary>
    /// Gets or sets the integer value of this enum item.
    /// </summary>
    public int Value
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;
            }

            _value = value;
            NotifyFieldUpdated();
        }
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "EnumItem";

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        Value = sync.SyncRename("Value", "Id", Value);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(Value, new ViewProperty("Value", "Enum Value"));
        }
    }

    /// <inheritdoc/>
    protected override Image OnGetIcon() => CoreIconCache.EnumField;

    /// <inheritdoc/>
    protected override void OnDrawPreviewImGui(ImGui gui)
    {
        Color color = _value != 0 ?
            EditorServices.ColorConfig.GetStatusColor(TextStatus.EnumReference) :
            EditorServices.ColorConfig.GetStatusColor(TextStatus.Preview);

        var node = gui.HorizontalFrame("enumPreview")
        .InitClass("refBox")
        .InitFit()
        .OverrideColor(color)
        .InitOverridePadding(0, 0, 5, 5)
        .OnContent(() =>
        {
            //if (icon != null)
            //{
            //    gui.Image(icon).InitClass("icon");
            //}
            gui.Text(_value.ToString()).InitClass("numBoxText").SetFontColor(Color.Black); //.InitInputToolTip(toolTip);
        });
    }

    #region IPreviewDisplay

    /// <inheritdoc/>
    public string PreviewText => Description ?? string.Empty;

    /// <inheritdoc/>
    public object PreviewIcon => null;

    #endregion

    #region IMember

    /// <inheritdoc/>
    IMemberContainer IMember.Container => ParentEnum;

    /// <inheritdoc/>
    string INamed.Name => this.Name;

    /// <inheritdoc/>
    Guid IHasId.Id => ParentEnum?.AssetBuilder?.Asset?.GetField(Name)?.Id ?? Guid.Empty;

    #endregion

    #region INavigationItem

    /// <inheritdoc/>
    string ISelectionItem.SelectionKey => ParentSItem != null ? $"{ParentSItem.Name}.{Name}" : Name;

    #endregion
}
