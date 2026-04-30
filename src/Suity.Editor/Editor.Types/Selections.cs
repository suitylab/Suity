using Suity.Collections;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Types;

#region Selections

/// <summary>
/// Base class for type design selections.
/// </summary>
public abstract class BaseTypeDesignSelection : ITypeDesignSelection, ISelection,
    IReference,
    INavigable,
    ISyncObject,
    IHasId,
    IHasAsset
{
    private string _prefix = string.Empty;
    private string _originAssetKey = string.Empty;

    private readonly EditorAssetRef _assetRef = new();
    private bool _optional;

    /// <summary>
    /// Gets the selection list.
    /// </summary>
    public abstract ISelectionList GetList();

    /// <summary>
    /// Event raised when the target is updated.
    /// </summary>
    public event EditorObjectEventHandler<EntryEventArgs> TargetUpdated;

    public BaseTypeDesignSelection()
    {
        _assetRef.TargetUpdated += _selection_ObjectUpdated;
    }

    /// <summary>
    /// Gets or sets the selected key (prefix + asset key).
    /// </summary>
    public string SelectedKey
    {
        get
        {
            string assetKey = _assetRef.AssetKey;

            if (!string.IsNullOrEmpty(assetKey))
            {
                return _prefix + assetKey;
            }
            else
            {
                return _prefix + _originAssetKey;
            }
        }
        set
        {
            TypeDefinition.SplitPrefix(value, out _prefix, out _originAssetKey);
            _assetRef.Id = GlobalIdResolver.Resolve(_originAssetKey);
        }
    }

    /// <summary>
    /// Gets or sets the prefix for the selection.
    /// </summary>
    public string Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    /// Gets or sets the ID of the selected asset.
    /// </summary>
    public Guid Id
    {
        get => _assetRef.Id;
        set
        {
            _assetRef.Id = value;
            _originAssetKey = _assetRef.AssetKey;
        }
    }

    /// <summary>
    /// Gets or sets whether the selection is optional.
    /// </summary>
    public bool Optional
    {
        get => _optional;
        set
        {
            if (_optional == value)
            {
                return;
            }

            _optional = value;
        }
    }

    /// <summary>
    /// Gets whether the selection is valid.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (_optional)
            {
                return _assetRef.Id == Guid.Empty || _assetRef.Target != null;
            }
            else
            {
                return _assetRef.Target != null;
            }
        }
    }

    /// <summary>
    /// Gets the DType associated with this selection.
    /// </summary>
public DType GetDType()
    {
        if (_assetRef.Id == Guid.Empty)
        {
            _assetRef.AssetKey = _originAssetKey;
        }

        return _assetRef.Target as DType;
    }

    /// <summary>
    /// Gets the type definition for this selection.
    /// </summary>
public TypeDefinition GetTypeDefinition()
    {
        if (_assetRef.Id == Guid.Empty)
        {
            _assetRef.AssetKey = _originAssetKey;
        }

        return TypeDefinition.Resolve(SelectedKey) ?? TypeDefinition.Empty;
    }

    /// <summary>
    /// Synchronizes the default value for the selected type.
    /// </summary>
    /// <param name="value">The value to sync.</param>
    /// <param name="filter">The asset filter.</param>
    /// The synchronized default value.
public object SyncDefaultValue(object value, IAssetFilter filter)
    {
        if (filter is null)
        {
            throw new ArgumentNullException();
        }

        var type = GetTypeDefinition();
        if (type.IsStruct || type.IsArray)
        {
            return null;
        }
        else
        {
            return type.CreateOrRepairValue(value, false);
        }
    }

    /// <summary>
    /// Returns a string representation of this selection.
    /// </summary>
public override string ToString()
    {
        Guid id = _assetRef.Id;
        if (id != Guid.Empty)
        {
            TypeDefinition type = TypeDefinition.Resolve(id);
            string str = type.ToDisplayString();
            return $"{_prefix}{str}";

            //switch (type.Relationship)
            //{
            //    case TypeRelationships.KeyLink:
            //        return "@" + str;
            //    case TypeRelationships.Enum:
            //        return "%" + str;
            //    case TypeRelationships.AbstractSide:
            //        return "?" + str;
            //    case TypeRelationships.Delegate:
            //        return "~" + str;
            //    default:
            //        return str;
            //}
        }

        //if (_assetRef.Id != Guid.Empty)
        //{
        //    string missingKey = GlobalIdResolver.RevertResolve(_assetRef.Id);
        //    if (!string.IsNullOrEmpty(missingKey))
        //    {
        //        return missingKey;
        //    }
        //    else
        //    {
        //        return _assetRef.Id.ToString();
        //    }
        //}

        return string.Empty;
    }

    object INavigable.GetNavigationTarget()
    {
        return _assetRef.Id;
    }

    #region ISyncObject

    /// <inheritdoc />
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        sync.SyncAssetRef(_assetRef, ref _prefix, ref _originAssetKey, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            Optional = sync.Sync("Optional", Optional);
        }
    }

    #endregion

    #region IAssetReferencer

    /// <inheritdoc />
    void IReference.ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        if (_assetRef.Id == Guid.Empty)
        {
            _assetRef.Id = GlobalIdResolver.Resolve(_originAssetKey);
        }

        _assetRef.Id = sync.SyncId(path, _assetRef.Id, null);

        if (sync.Mode == ReferenceSyncMode.Redirect)
        {
            _originAssetKey = _assetRef.AssetKey ?? string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets whether listening is enabled.
    /// </summary>
    public bool ListenEnabled
    {
        get => _assetRef.ListenEnabled;
        set => _assetRef.ListenEnabled = value;
    }

    /// <summary>
    /// Gets the target asset.
    /// </summary>
    public Asset TargetAsset => _assetRef.Target;

    private void _selection_ObjectUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        TargetUpdated?.Invoke(sender, e, ref handled);
    }
}

public class TypeDesignSelection : BaseTypeDesignSelection, IEquatable<TypeDesignSelection>
{
    public override ISelectionList GetList() => TypeDesignSelectionList.Instance;

    public bool Equals(TypeDesignSelection other)
    {
        if (other is null)
        {
            return false;
        }

        return Id == other.Id && Prefix == other.Prefix;
    }
}

public class DataLinkTypeDesignSelection : BaseTypeDesignSelection, IEquatable<DataLinkTypeDesignSelection>
{
    public override ISelectionList GetList() => TypeDesignDataLinkSelectionList.Instance;

    public bool Equals(DataLinkTypeDesignSelection other)
    {
        if (other is null)
        {
            return false;
        }

        return Id == other.Id && Prefix == other.Prefix;
    }
}

#endregion

#region SelectionList

internal sealed class TypeDesignSelectionList : BaseSelectionNode
{
    public static TypeDesignSelectionList Instance { get; } = new TypeDesignSelectionList();

    readonly Dictionary<string, ISelectionItem> _items = [];

    private TypeDesignSelectionList()
    {
        Add(TypeDesignPrimitiveSelectionList.Instance);
        Add(TypeDesignNativeSelectionList.Instance);
        Add(TypeDesignStructSelectionList.Instance);
        Add(TypeDesignAbstractSelectionList.Instance);
        Add(TypeDesignEnumSelectionList.Instance);
        Add(TypeDesignDataLinkSelectionList.Instance);
        Add(TypeDesignAbstractLinkSelectionList.Instance);
        Add(TypeDesignAssetLinkSelectionList.Instance);
        Add(TypeDesignControllerSelectionList.Instance);
        Add(TypeDesignDelegateSelectionList.Instance);
    }

    private void Add(ISelectionItem item)
    {
        _items.Add(item.SelectionKey, item);
    }

    public override string DisplayText => "Type Selection";

    public override string SelectionKey => "Type";

    public override object DisplayIcon => CoreIconCache.Structure;

    #region ISelectionList

    public override IEnumerable<ISelectionItem> GetItems()
    {
        return _items.Values;
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return _items.GetValueSafe(key);

        //TypeDefinition.SplitPrefix(key, out string prefix, out string originKey);

        //if (!string.IsNullOrEmpty(prefix))
        //{
        //    Asset asset = AssetManager.Instance.GetAsset(originKey, AssetFilters.Default);
        //    if (asset != null)
        //    {
        //        return new SelectionItem(key, asset.DisplayText, asset.Icon);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        //else
        //{
        //    return AssetManager.Instance.GetAsset(key, AssetFilters.Default);
        //}
    }

    #endregion
}

internal class TypeDesignPrimitiveSelectionList : BaseSelectionNode
{
    public static TypeDesignPrimitiveSelectionList Instance { get; } = new();

    public override string SelectionKey => "Primitive";

    public override string DisplayText => "Primitive";

    public override object DisplayIcon => CoreIconCache.Native;


    #region ISelectionList

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DPrimative>();
        if (collection != null)
        {
            foreach (var item in collection.Assets.Where(AssetFilters.Default.FilterAsset))
            {
                yield return item;
            }
        }

        yield return NativeTypes.DelegateType.Target;

        //yield return NativeTypes.TextBlockType.Target;
        //yield return NativeTypes.ObjectType.Target;
        //yield return NativeTypes.SItemType.Target;
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var item = AssetManager.Instance.GetAsset(key, AssetFilters.Default);
        return item switch
        {
            DPrimative dPrimativeType => dPrimativeType,
            DDelegate dDelegate => dDelegate,
            _ => null,
        };
    }

    #endregion
}

internal class TypeDesignNativeSelectionList : BaseSelectionNode
{
    public static TypeDesignNativeSelectionList Instance { get; } = new TypeDesignNativeSelectionList();

    public override string SelectionKey => "Native";

    public override string DisplayText => "Native";

    public override object DisplayIcon => CoreIconCache.Native;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DNativeType>();
        if (collection != null)
        {
            foreach (var item in collection.Assets.Where(AssetFilters.Default.FilterAsset))
            {
                yield return item;
            }
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var item = AssetManager.Instance.GetAsset(key, AssetFilters.Default);
        return item switch
        {
            DNativeType dNativeObject => dNativeObject,
            _ => null,
        };
    }

    #endregion
}


internal class TypeDesignStructSelectionList : BaseSelectionNode
{
    public static TypeDesignStructSelectionList Instance { get; } = new TypeDesignStructSelectionList();

    public override string SelectionKey => "Struct";

    public override string DisplayText => "Struct";

    public override object DisplayIcon => CoreIconCache.Structure;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DStruct>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset);
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return AssetManager.Instance.GetAsset<DStruct>(key, AssetFilters.Default);
    }

    #endregion
}

internal class TypeDesignAbstractSelectionList : BaseSelectionNode
{
    public static TypeDesignAbstractSelectionList Instance { get; } = new TypeDesignAbstractSelectionList();

    public override string SelectionKey => "Abstract";

    public override string DisplayText => "Abstract Struct";

    public override object DisplayIcon => CoreIconCache.Abstract;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DAbstract>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem(o.AssetKey) 
                {
                    DisplayText = o.DisplayText + " " + L("Abstract Struct"), 
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('?');

        var o = AssetManager.Instance.GetAsset<DAbstract>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem(o.AssetKey)
            { 
                DisplayText = o.DisplayText + " " + L("Abstract Struct"), 
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

internal class TypeDesignEnumSelectionList : BaseSelectionNode
{
    public static TypeDesignEnumSelectionList Instance { get; } = new TypeDesignEnumSelectionList();

    public override string SelectionKey => "Enum";

    public override string DisplayText => "Enum";

    public override object DisplayIcon => CoreIconCache.Enum;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DEnum>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem("%" + o.AssetKey) 
                {
                    DisplayText = o.DisplayText + " " + L("Enum"),
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('%');

        var o = AssetManager.Instance.GetAsset<DEnum>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem("%" + o.AssetKey) 
            {
                DisplayText = o.DisplayText + " " + L("Enum"), 
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

internal class TypeDesignFunctionSelectionList : BaseSelectionNode
{
    public static TypeDesignFunctionSelectionList Instance { get; } = new TypeDesignFunctionSelectionList();

    public override string SelectionKey => "Function";

    public override string DisplayText => "Function";

    public override object DisplayIcon => CoreIconCache.Structure;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DFunction>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset);
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return AssetManager.Instance.GetAsset<DFunction>(key, AssetFilters.Default);
    }

    #endregion
}

internal class TypeDesignDataLinkSelectionList : BaseSelectionNode
{
    public static TypeDesignDataLinkSelectionList Instance { get; } = new TypeDesignDataLinkSelectionList();

    public override string SelectionKey => AssetDefNames.Data;

    public override string DisplayText => "Data Link";

    public override object DisplayIcon => CoreIconCache.Link;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DStruct>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem("@" + o.AssetKey) 
                {
                    DisplayText = o.DisplayText + " " + L("Data Link"), 
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('@');

        var o = AssetManager.Instance.GetAsset<DStruct>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem("@" + o.AssetKey) 
            {
                DisplayText = o.DisplayText + " " + L("Data Link"),
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

internal class TypeDesignAbstractLinkSelectionList : BaseSelectionNode
{
    public static TypeDesignAbstractLinkSelectionList Instance { get; } = new TypeDesignAbstractLinkSelectionList();

    public override string SelectionKey => "AbstractDataLink";

    public override string DisplayText => "Abstract Link";

    public override object DisplayIcon => CoreIconCache.Link;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DAbstract>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem("@" + o.AssetKey) 
                {
DisplayText = o.DisplayText + " " + L("Abstract Link"),
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('@');

        var o = AssetManager.Instance.GetAsset<DStruct>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem("@" + o.AssetKey) 
            {
                DisplayText = o.DisplayText + " " + L("Abstract Link"),
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

internal class TypeDesignAssetLinkSelectionList : BaseSelectionNode
{
    public static TypeDesignAssetLinkSelectionList Instance { get; } = new TypeDesignAssetLinkSelectionList();

    public override string SelectionKey => "AssetLink";

    public override string DisplayText => "Asset Link";

    public override object DisplayIcon => CoreIconCache.Link;


    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DAssetLink>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem("&" + o.AssetKey) 
                {
DisplayText = o.DisplayText + " " + L("Asset Link"),
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('&');

        var o = AssetManager.Instance.GetAsset<DAssetLink>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem("&" + o.AssetKey) 
            {
                DisplayText = o.DisplayText + " " + L("Asset Link"),
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

internal class TypeDesignControllerSelectionList : BaseSelectionNode
{
    public static TypeDesignControllerSelectionList Instance { get; } = new TypeDesignControllerSelectionList();

    public override string SelectionKey => "Controller";

    public override string DisplayText => "Controller";

    public override object DisplayIcon => CoreIconCache.Controller;

    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DController>();
        if (collection != null)
        {
            return collection.Assets.Where(AssetFilters.Default.FilterAsset);
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return AssetManager.Instance.GetAsset<DController>(key, AssetFilters.Default);
    }

    #endregion
}

internal class TypeDesignDelegateSelectionList : BaseSelectionNode
{
    public static TypeDesignDelegateSelectionList Instance { get; } = new TypeDesignDelegateSelectionList();

    public override string SelectionKey => "Delegate";

    public override string DisplayText => "Delegate";

    public override object DisplayIcon => CoreIconCache.Delegate;

    #region ISelectionList Members

    public override IEnumerable<ISelectionItem> GetItems()
    {
        var collection = AssetManager.Instance.GetAssetCollection<DDelegate>();
        if (collection != null)
        {
            return collection.Assets
                .Where(AssetFilters.Default.FilterAsset)
                .Select(o => new SelectionItem("~" + o.AssetKey) 
                {
DisplayText = o.DisplayText + " " + L("Delegate"),
                    DisplayIcon = o.Icon,
                    ViewColor = o.ViewColor,
                });
        }
        else
        {
            return [];
        }
    }

    public override ISelectionItem GetItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        key = key.TrimStart('~');

        var o = AssetManager.Instance.GetAsset<DDelegate>(key, AssetFilters.Default);
        if (o != null)
        {
            return new SelectionItem("~" + o.AssetKey) 
            {
                DisplayText = o.DisplayText + " " + L("Delegate"),
                DisplayIcon = o.Icon,
                ViewColor = o.ViewColor,
            };
        }
        else
        {
            return null;
        }
    }

    #endregion
}

#endregion
