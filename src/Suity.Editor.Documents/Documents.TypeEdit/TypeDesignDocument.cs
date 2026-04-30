using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Document for designing data types including structs, enums, abstract types, and logic modules.
/// </summary>
[DocumentFormat(FormatName = "TypeDesign", FormatNames = ["TypeEdit"], Extension = "stype", DisplayText = "Type Design", Icon = "*CoreIcon|Class", Categoty = "Data", Order = 1000)]
[EditorFeature(EditorFeatures.DataDesign)]
[NativeAlias("TypeDesign", UseForSaving = true)]
[NativeAlias("TypeEdit")]
[NativeAlias("Suity.Editor.Documents.TypeEdit.TypeEditDocument")]
public class TypeDesignDocument : DesignDocument<DTypeFamilyBuilder>,
    ITypeDesignDocument,
    IDataContainer,
    IFunctionContainer
{
    private string _version = "0";
    private readonly List<string> _supportedVersions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDesignDocument"/> class.
    /// </summary>
    public TypeDesignDocument()
    {
        AnalyzeNameSpace = true;

        ItemCollection.FieldName = "Types";
        ItemCollection.FieldDescription = "Types";

        var plugin = EditorServices.PluginService.GetPlugin<TypeDesignPlugin>();
        foreach (var type in plugin.DesignItemTypes)
        {
            ItemCollection.AddItemType(type, type.ToDisplayText());
        }

        ItemCollection.AddItemType<TypeDesignGroup>("Group");
        ItemCollection.AddItemType<NamedComment>("Comment");

        PreviewFieldName = "Types";
    }

    /// <summary>
    /// Gets the current version of the document format.
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// Gets the list of supported compatible versions.
    /// </summary>
    public IEnumerable<string> SupportedVersions => _supportedVersions.Select(o => o);

    /// <summary>
    /// Gets the root collection of type edit items.
    /// </summary>
    public SNamedRootCollection TypeEditItems => base.ItemCollection;

    #region Virtual

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _version = sync.Sync("Version", _version, SyncFlag.NotNull, "0");
        sync.Sync("SupportedVersions", _supportedVersions, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(_version, new ViewProperty("Version", "Version"));
            setup.InspectorField(_supportedVersions, new ViewProperty("SupportedVersions", "Compatible Versions") { WriteBack = true });
        }
    }

    /// <inheritdoc/>
    protected override void OnItemAdded(SNamedRootCollection bag, NamedItem item, bool isNew)
    {
        base.OnItemAdded(bag, item, isNew);

        if (item is ClassFunction classFunction)
        {
            classFunction.AccessMode = AssetAccessMode.Public;
        }
    }

    /// <inheritdoc/>
    public override SNamedGroup CreateGroup() => new TypeDesignGroup();

    #endregion

    #region IFunctionContainer

    /// <inheritdoc/>
    string IFunctionContainer.Name => this.Entry.GetShortTypeName();

    /// <inheritdoc/>
    System.Collections.Generic.IEnumerable<IFunction> IFunctionContainer.Functions => ItemCollection.AllItemsSorted.OfType<IFunction>();

    /// <inheritdoc/>
    IFunction IFunctionContainer.GetFunction(string name) => ItemCollection.GetItemAll(name) as IFunction;

    #endregion

    #region ITypeDesignDocument

    /// <summary>
    /// Gets all type design items in the document.
    /// </summary>
    public IEnumerable<TypeDesignItem> TypeItems => ItemCollection.AllItems.OfType<TypeDesignItem>();

    /// <summary>
    /// Adds a new type item to the document.
    /// </summary>
    /// <param name="type1">The type of data structure to create.</param>
    /// <param name="name">The name of the type item.</param>
    /// <param name="description">The description of the type item.</param>
    /// <param name="groupPath">The group path where the item should be added.</param>
    /// <returns>The created type design item, or null if creation failed.</returns>
    public TypeDesignItem AddTypeItem(DataStructureType type1, string name, string description, string groupPath)
    {
        if (!NamingVerifier.VerifyIdentifier(name))
        {
            return null;
        }

        var current = ItemCollection.GetItemAll(name) as TypeDesignItem;
        TypeDesignItem item = null;

        switch (type1)
        {
            case DataStructureType.Enum:
                if (current is EnumType) return current;
                if (current != null) return null;
                item = new EnumType(name);
                break;

            case DataStructureType.Struct:
                if (current is StructType) return current;
                if (current != null) return null;
                item = new StructType(name);
                break;

            case DataStructureType.Abstract:
                if (current is AbstractType) return current;
                if (current != null) return null;
                item = new AbstractType(name);
                break;
        }

        if (item is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(groupPath))
        {
            this.EnsureGroupByPath(groupPath).AddItem(item);
        }
        else
        {
            ItemCollection.AddItem(item);
        }

        MarkDirtyAndSaveDelayed(this);

        return item;
    }

    /// <summary>
    /// Removes a type item from the document.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed successfully, false otherwise.</returns>
    public bool RemoveTypeItem(TypeDesignItem item)
    {
        if (item is null)
        {
            return false;
        }

        if (ItemCollection.RemoveItem(item))
        {
            MarkDirtyAndSaveDelayed(this);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Adds a new field to the specified type item.
    /// </summary>
    /// <param name="item">The parent type item.</param>
    /// <param name="name">The name of the field.</param>
    /// <param name="description">The description of the field.</param>
    /// <returns>The created design field, or null if creation failed.</returns>
    public DesignField AddField(TypeDesignItem item, string name, string description = null)
    {
        if (!NamingVerifier.VerifyIdentifier(name))
        {
            return null;
        }

        var field = item.FieldList.GetItem(name) as DesignField;
        if (field != null)
        {
            return field;
        }

        switch (item)
        {
            case EnumType:
                field = new EnumItem(name) { Description = description };
                break;

            case StructType:
            case AbstractType:
                field = new StructField(name) { Description = description };
                break;
        }

        if (field != null)
        {
            item.FieldList.Add(field);
            MarkDirtyAndSaveDelayed(this);
            return field;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the type of a design field.
    /// </summary>
    /// <param name="field">The field to modify.</param>
    /// <param name="type">The type definition to set.</param>
    /// <returns>True if the type was set successfully, false otherwise.</returns>
    public bool SetFieldType(DesignField field, TypeDefinition type)
    {
        if (field is null)
        {
            return false;
        }

        if (type is null)
        {
            return false;
        }

        if (field is not StructField structField)
        {
            return false;
        }

        bool isArray = type.IsArray;
        if (isArray)
        {
            type = type.ElementType;
        }

        structField.FieldType.FieldType = type;
        structField.FieldType.IsArray = isArray;

        MarkDirtyAndSaveDelayed(this);

        return true;
    }

    #endregion

    #region IDataContainer

    /// <inheritdoc/>
    Guid IHasId.Id => this.Id;

    /// <inheritdoc/>
    string IDataContainer.TableId
    {
        get => TargetAsset?.FullTypeName;
        set { }
    }

    /// <inheritdoc/>
    string IDataContainer.Name => TargetAsset?.FullTypeName;

    /// <inheritdoc/>
    IEnumerable<IDataItem> IDataContainer.Datas => ItemCollection.AllItemsSorted.OfType<IDataItem>();

    /// <inheritdoc/>
    IDataItem IDataContainer.GetData(string name) => ItemCollection.GetItemAll(name) as IDataItem;

    /// <inheritdoc/>
    void IDataContainer.CleanUp()
    {
    }

    /// <inheritdoc/>
    Asset IHasAsset.TargetAsset => this.TargetAsset;

    #endregion
}
