using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents an enum type in the type design document.
/// </summary>
[NativeAlias]
[DisplayText("Enum", "*CoreIcon|Enum")]
[DisplayOrder(990)]
public class EnumType : TypeDesignItem<DEnumBuilder>, IMemberContainer, IDataItem
{
    private readonly EnumItemList _fieldList;
    private IdAutomations _idAutomaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumType"/> class.
    /// </summary>
    public EnumType()
    {
        _fieldList = new EnumItemList("Items", this)
        {
            FieldDescription = "Enum Items",
            FieldIcon = CoreIconCache.EnumField
        };
        AddPrimaryFieldList(_fieldList);

        ShowRenderTargets = true;
        ShowUsings = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumType"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the enum.</param>
    public EnumType(string name)
        : this()
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the ID automation mode for enum values.
    /// </summary>
    public IdAutomations IdAutomation
    {
        get => _idAutomaction;
        set
        {
            if (_idAutomaction == value)
            {
                return;
            }

            _idAutomaction = value;
            AssetBuilder.SetIdAutomation(value);
        }
    }

    /// <summary>
    /// Gets the enum builder for this type.
    /// </summary>
    internal new DEnumBuilder AssetBuilder => base.AssetBuilder;

    #region Virtual

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "Enum";

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.DetailTreeViewField(_fieldList, new ViewProperty(_fieldList.FieldName, "Enum Items"));

        base.OnSetupView(setup);

        if (setup.IsTypeSupported(this.GetType()))
        {
            setup.InspectorField(IdAutomation, new ViewProperty("IdAutomation", "Id Automation"));
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        IdAutomation = sync.Sync("IdAutomation", IdAutomation);
        sync.Sync(_fieldList.FieldName, _fieldList, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon()
    {
        return base.OnGetIcon() ?? Suity.Editor.CoreIconCache.Enum;
    }

    /// <inheritdoc/>
    public override string PreviewText => "Enum";

    /// <inheritdoc/>
    public override ImageDef TypeIcon => CoreIconCache.Enum;

    /// <inheritdoc/>
    public override Color? TypeColor => DEnum.EnumTypeColor;

    /// <inheritdoc/>
    protected override void OnFieldListItemAdded(NamedFieldList list, NamedField item, bool isNew)
    {
        base.OnFieldListItemAdded(list, item, isNew);

        if (item is EnumItem field)
        {
            AssetBuilder.AddOrUpdateField(
                field.Name,
                field.Value,
                field.Description,
                null,
                field.Attributes,
                forceUpdate: false,
                isNew: isNew,
                recorededId: field.RecorededFieldId
                );
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRemoved(NamedFieldList list, NamedField item)
    {
        base.OnFieldListItemRemoved(list, item);

        if (item is EnumItem eItem)
        {
            AssetBuilder.RemoveField(eItem.Name);
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemUpdated(NamedFieldList list, NamedField item, bool forceUpdate)
    {
        base.OnFieldListItemUpdated(list, item, forceUpdate);

        if (item is EnumItem field)
        {
            AssetBuilder.AddOrUpdateField(
                field.Name,
                field.Value,
                field.Description,
                null,
                field.Attributes,
                forceUpdate: forceUpdate,
                isNew: false,
                recorededId: field.RecorededFieldId
                );
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRenamed(NamedFieldList list, NamedField item, string oldName)
    {
        base.OnFieldListItemRenamed(list, item, oldName);

        if (item is EnumItem eItem)
        {
            AssetBuilder.RenameField(oldName, eItem.Name);
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListArrageItem(NamedFieldList list)
    {
        base.OnFieldListArrageItem(list);

        int index = 0;
        var items = _fieldList.OfType<EnumItem>().ToArray();
        foreach (var n in items)
        {
            AssetBuilder.UpdateField(n.Name, index);
            index++;
        }
        AssetBuilder.Sort();
    }

    #endregion

    #region Fields

    /// <summary>
    /// Gets all enum fields in the field list.
    /// </summary>
    public IEnumerable<EnumItem> Fields => FieldList?.OfType<EnumItem>() ?? [];

    /// <summary>
    /// Gets an enum field by name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The enum field, or null if not found.</returns>
    public EnumItem GetField(string name) => FieldList?.GetItem(name) as EnumItem;

    #endregion

    #region IMemberContainer

    /// <inheritdoc/>
    IMember IMemberContainer.GetMember(string name) => _fieldList.GetItem(name) as IMember;

    /// <inheritdoc/>
    IEnumerable<IMember> IMemberContainer.Members => _fieldList.OfType<IMember>();

    /// <inheritdoc/>
    int IMemberContainer.MemberCount => _fieldList.Count;

    #endregion

    #region IDataRow

    /// <summary>
    /// Gets the data container that owns this item.
    /// </summary>
    public IDataContainer DataContainer => GetDocument() as IDataContainer;

    /// <inheritdoc/>
    bool IDataItem.IsLinked => false;

    /// <inheritdoc/>
    Guid IDataItem.DataGuid => this.Id;

    /// <inheritdoc/>
    string IDataItem.DataLocalId => this.Name;

    /// <inheritdoc/>
    int IDataItem.Index => this.GetIndex();

    /// <inheritdoc/>
    IEnumerable<SObject> IDataItem.Components
    {
        get
        {
            var sObj = new SObject(TypeDefinition.Resolve("TypeInfoDescriptor"));
            sObj["Kind"] = "Enum";
            if (!string.IsNullOrEmpty(Description))
            {
                sObj["Description"] = Description;
            }
            if (IconId != Guid.Empty)
            {
                sObj["Icon"] = IconId;
            }
            string category = Category;
            if (!string.IsNullOrEmpty(category))
            {
                sObj["Category"] = category;
            }

            var fields = new SArray(TypeDefinition.Resolve("FieldDescriptor[]"));
            foreach (var p in _fieldList.OfType<EnumItem>())
            {
                var pObj = new SObject(TypeDefinition.Resolve("FieldDescriptor"));
                pObj["Name"] = p.Name;
                if (!string.IsNullOrEmpty(p.Description))
                {
                    pObj["Description"] = p.Description;
                }

                fields.Add(pObj);
            }
            sObj["Fields"] = fields;

            if (Attributes?.Count > 0)
            {
                return [sObj, .. Attributes.GetAttributes().OfType<SObject>()];
            }
            else
            {
                return [sObj];
            }
        }
    }

    #endregion
}
