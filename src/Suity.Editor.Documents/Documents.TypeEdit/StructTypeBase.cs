using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Base class for struct-like type design items, providing field management and inheritance support.
/// </summary>
public abstract class StructTypeBase : TypeDesignItem,
    IDataItem, IVariableContainer
{
    private bool _isValueType;
    private AssetSelection<DAbstract> _baseType = new();

    private readonly QueueOnceAction _arrangeAction;

    private readonly IStructBuilder _structBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructTypeBase"/> class.
    /// </summary>
    /// <param name="builder">The asset builder, must implement <see cref="IStructBuilder"/>.</param>
    protected StructTypeBase(AssetBuilder builder)
        : base(builder)
    {
        _structBuilder = builder as IStructBuilder
            ?? throw new InvalidOperationException($"{nameof(builder)} must implements {nameof(IStructBuilder)}");

        ShowRenderTargets = true;
        ShowUsings = true;

        _arrangeAction = new QueueOnceAction(() =>
        {
            if (HasParent)
            {
                FieldList?.ArrangeItem();
            }
        });
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "Struct";

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon() => base.OnGetIcon() ?? _baseType.Target?.GetIcon();

    /// <inheritdoc/>
    protected override Color? OnGetColor() => base.OnGetColor() ?? _baseType.Target?.ViewColor;

    /// <inheritdoc/>
    public override string PreviewText => "Struct";

    /// <summary>
    /// Gets a value indicating whether base type selection is enabled for this struct type.
    /// </summary>
    protected virtual bool AbstractEnabled => true;

    /// <summary>
    /// Gets or sets a value indicating whether this struct is a value type.
    /// </summary>
    public bool IsValueType
    {
        get => _isValueType;
        set
        {
            if (_isValueType == value)
            {
                return;
            }

            _isValueType = value;
            OnIsValueTypeChanged();
        }
    }

    /// <summary>
    /// Gets or sets the base type target directly.
    /// </summary>
    public DAbstract BaseTypeTarget
    {
        get => BaseType?.Target;
        set
        {
            if (BaseType is { } sel)
            {
                sel.Target = value;
                OnBaseTypeChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the base type selection for inheritance.
    /// </summary>
    public AssetSelection<DAbstract> BaseType
    {
        get => AbstractEnabled ? _baseType : null;
        set
        {
            if (!AbstractEnabled)
            {
                return;
            }

            var v = value ?? new AssetSelection<DAbstract>();
            if (_baseType.Id == v.Id)
            {
                return;
            }

            _baseType = v;
            OnBaseTypeChanged();
        }
    }

    #region Virtual

    /// <summary>
    /// Called when the <see cref="IsValueType"/> property changes.
    /// </summary>
    protected virtual void OnIsValueTypeChanged()
    {
    }

    /// <summary>
    /// Called when the <see cref="BaseType"/> property changes.
    /// </summary>
    protected virtual void OnBaseTypeChanged()
    {
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemAdded(NamedFieldList list, NamedField item, bool isNew)
    {
        base.OnFieldListItemAdded(list, item, isNew);

        if (item is StructField field)
        {
            _structBuilder.AddOrUpdateField(
                field.Name, 
                field.FieldType.FieldType, 
                AssetAccessMode.Public, 
                field.DefaultValue, 
                field.Optional,
                null,
                field.Attributes,
                forceUpdate: false,
                isNew: isNew,
                recorededId: field.RecorededFieldId
                );
        }

        _arrangeAction.DoQueuedAction();
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRemoved(NamedFieldList list, NamedField item)
    {
        base.OnFieldListItemRemoved(list, item);

        if (item is StructField field)
        {
            _structBuilder.RemoveField(field.Name);
        }

        _arrangeAction.DoQueuedAction();
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemUpdated(NamedFieldList list, NamedField item, bool forceUpdate)
    {
        base.OnFieldListItemUpdated(list, item, forceUpdate);

        if (item is StructField field)
        {
            _structBuilder.AddOrUpdateField(
                field.Name, 
                field.FieldType.FieldType, 
                AssetAccessMode.Public, 
                field.DefaultValue, 
                field.Optional, 
                null, 
                field.Attributes,
                forceUpdate: forceUpdate,
                isNew: false,
                recorededId: field.RecorededFieldId
                );
        }

        _arrangeAction.DoQueuedAction();
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRenamed(NamedFieldList list, NamedField item, string oldName)
    {
        base.OnFieldListItemRenamed(list, item, oldName);

        if (item is StructField p)
        {
            _structBuilder.RenameField(oldName, p.Name);
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListArrageItem(NamedFieldList list)
    {
        base.OnFieldListArrageItem(list);

        int index = 0;
        StructFieldItem[] items = (list as StructFieldList)?.OfType<StructFieldItem>().ToArray();

        string labelText = null;

        foreach (var item in items)
        {
            if (item is StructField field)
            {
                _structBuilder.UpdateFieldDisplay(field.Name,
                    index: index,
                    description: field.Description,
                    label: labelText,
                    showInDetail: field.ShowInDetail,
                    unit: field.Unit);

                labelText = null;
            }
            else if (item is StructFieldLabel label)
            {
                labelText = label.Description;
            }

            index++;
        }

        _structBuilder.Sort();
    }

    #endregion

    #region Fields

    /// <summary>
    /// Gets all struct fields in the field list.
    /// </summary>
    public IEnumerable<StructField> Fields => FieldList?.OfType<StructField>() ?? [];

    /// <summary>
    /// Gets a field by name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The struct field, or null if not found.</returns>
    public StructField GetField(string name) => FieldList?.GetItem(name) as StructField;

    #endregion

    #region Sync

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        IsValueType = sync.Sync("IsValueType", IsValueType);
        if (AbstractEnabled)
        {
            BaseType = sync.SyncRename("BaseType", "Side", BaseType, SyncFlag.NotNull);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            if (AbstractEnabled)
            {
                if (IsImported)
                {
                    setup.InspectorField(IsValueType, new ViewProperty("IsValueType", "Value Type"));
                }

                setup.InspectorField(BaseType, new ViewProperty("BaseType", "Base Type").WithToolTips("Set to an implementation struct of an abstract struct"));
            }
        }
    }

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
            var sObj = new SObject(TypeDefinition.Resolve(nameof(TypeInfoDescriptor)));
            sObj["Kind"] = "Class";

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

            if (BaseType?.SelectedKey is { } selKey && !string.IsNullOrWhiteSpace(selKey))
            {
                sObj["BaseType"] = selKey;
            }

            if (IsValueType)
            {
                sObj["IsValueType"] = true;
            }

            var fields = new SArray(TypeDefinition.Resolve($"{nameof(FieldDescriptor)}[]"));
            if (FieldList != null)
            {
                foreach (var p in FieldList.OfType<StructField>())
                {
                    var pObj = new SObject(TypeDefinition.Resolve(nameof(FieldDescriptor)));
                    pObj["Name"] = p.Name;
                    if (!string.IsNullOrEmpty(p.Description))
                    {
                        pObj["Description"] = p.Description;
                    }
                    pObj["Type"] = p.FieldType.FieldType.ToString();

                    fields.Add(pObj);
                }
            }
            sObj["Fields"] = fields;

            if (Attributes?.Count > 0)
            {
                return new SObject[] { sObj }.Concat(Attributes.GetAttributes().OfType<SObject>());
            }
            else
            {
                return [sObj];
            }
        }
    }

    #endregion

    #region IVariableContainer

    /// <inheritdoc/>
    string IVariableContainer.AssetKey
    {
        get
        {
            var document = GetDocument();
            if (document != null)
            {
                return KeyCode.Combine(document.AssetKey, this.Name);
            }
            else
            {
                return null;
            }
        }
    }

    /// <inheritdoc/>
    IEnumerable<IVariable> IVariableContainer.Variables => FieldList?.OfType<IVariable>() ?? [];

    /// <inheritdoc/>
    IVariable IVariableContainer.GetVariable(string name) => FieldList?.GetItem(name) as IVariable;

    #endregion

    #region IMemberContainer

    /// <inheritdoc/>
    IMember IMemberContainer.GetMember(string name) => FieldList?.GetItem(name) as IMember;

    /// <inheritdoc/>
    int IMemberContainer.MemberCount => FieldList?.Count ?? 0;

    /// <inheritdoc/>
    IEnumerable<IMember> IMemberContainer.Members => FieldList?.OfType<IMember>() ?? [];

    #endregion
}

/// <summary>
/// Generic base class for struct types with a specific asset builder type.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of the asset builder.</typeparam>
public abstract class StructTypeBase<TAssetBuilder> : StructTypeBase
    where TAssetBuilder : AssetBuilder, IStructBuilder, new()
{
    private readonly TAssetBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructTypeBase{TAssetBuilder}"/> class.
    /// </summary>
    protected StructTypeBase()
         : base(new TAssetBuilder())
    {
        _builder = (TAssetBuilder)base.AssetBuilder;
    }

    /// <summary>
    /// Gets the typed asset builder.
    /// </summary>
    protected new TAssetBuilder AssetBuilder => _builder;
}
