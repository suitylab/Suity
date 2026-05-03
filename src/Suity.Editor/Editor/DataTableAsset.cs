using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Data table aset。
/// Must inherit from this type to support rendering in languages ​​other than JSON.
/// Only json rendering can be implemented <see cref="IDataTableAsset"/>.
/// </summary>
[AssetTypeBinding(AssetDefNames.DataFamily, "Data table asset")]
public abstract class DataTableAsset : GroupAsset, 
    IRenderable,
    IDataTableAsset
{
    //private string _tableId;

    public DataTableAsset()
    {
        UpdateAssetTypes(typeof(IRenderable), typeof(IDataTableAsset));
    }

    public override ImageDef DefaultIcon => CoreIconCache.DataGrid;

    public override bool CanExportToLibrary => true;

    #region IRenderable

    public virtual bool RenderEnabled => true;

    public virtual IMaterial DefaultMaterial => null;

    public virtual IEnumerable<RenderItem> GetRenderItems()
    {
        string typeName = this.ShortTypeName;

        yield return new RenderItem(Id, this, RenderType.DataFamily, typeName, this, this.LastUpdateTime);
    }

    public virtual IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath)
    {
        var path = basePath.WithNameSpace(NameSpace);

        return GetRenderItems().SelectMany(o => material.GetRenderTargets(o, path));
    }

    public virtual ICodeLibrary GetCodeLibrary()
    {
        return this.GetAttachedUserLibrary();
    }

    #endregion

    #region IDataAsset

    public abstract IDataContainer GetDataContainer(bool tryLoadStorage);

    #endregion
}


public sealed class FixedDataTableAsset : DataTableAsset, IDataContainer
{
    string _tableId = string.Empty;
    readonly Dictionary<string, FixedDataAsset> _rows = [];

    public FixedDataTableAsset(string localName, string description, IDictionary<string, SObject> rows, Func<string, string> descriptionGetter = null)
        : base()
    {
        base.LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
        Description = description ?? string.Empty;

        int index = 0;
        foreach (var pair in rows)
        {
            var row = new FixedDataAsset(this, pair.Key, index, [pair.Value]);
            var desc = descriptionGetter?.Invoke(pair.Key);
            if (desc != null)
            {
                row.Description = desc;
            }

            pair.Value.Context = row;

            _rows.Add(pair.Key, row);
            AddOrUpdateChildAsset(row);
        }

        base.ResolveId();
    }

    public FixedDataTableAsset(string localName, string description, UniqueMultiDictionary<string, SObject> rows, Func<string, string> descriptionGetter = null)
        : base()
    {
        base.LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
        Description = description ?? string.Empty;

        int index = 0;
        foreach (var key in rows.Keys)
        {
            var objs = rows[key];
            var row = new FixedDataAsset(this, key, index, objs);
            var desc = descriptionGetter?.Invoke(key);
            if (desc != null)
            {
                row.Description = desc;
            }

            _rows.Add(key, row);
            AddOrUpdateChildAsset(row);
        }

        base.ResolveId();
    }


    #region DataTableAsset
    public override IDataContainer GetDataContainer(bool tryLoadStorage) => this;
    #endregion

    #region IDataContainer
    string IDataContainer.TableId { get => _tableId; set => _tableId = value; }

    IEnumerable<IDataItem> IDataContainer.Datas => _rows.Values;

    IEnumerable<IMember> IMemberContainer.Members => _rows.Values;

    int IMemberContainer.MemberCount => _rows.Count;

    Asset IHasAsset.TargetAsset => this;

    void IDataContainer.CleanUp()
    {
    }

    IDataItem IDataContainer.GetData(string name) => _rows.GetValueSafe(name);

    IMember IMemberContainer.GetMember(string name) => _rows.GetValueSafe(name);
    #endregion

    class FixedDataAsset : Asset, IDataAsset, IDataItem
    {
        readonly FixedDataTableAsset _table;
        readonly int _index;
        readonly List<SObject> _objs = [];

        public FixedDataAsset(FixedDataTableAsset table, string localName, int index, IEnumerable<SObject> objs)
            : base()
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            LocalName = localName ?? throw new ArgumentNullException(nameof(localName));
            _index = index;
            _objs.AddRange(objs?.SkipNull() ?? []);

            base.UpdateAssetTypes(typeof(IDataAsset));

            UpdateAssetTypes([.. _objs.Select(o => o.ObjectType.TypeCode), typeof(IDataAsset).ResolveAssetTypeName()]);
        }

        #region IDataRowAsset
        IDataItem IDataAsset.GetData(bool tryLoadStorage) => this;

        TypeDefinition[] IDataAsset.GetDataTypes() => [.. _objs.Select(o => o.ObjectType)];

        bool IDataAsset.SupportType(TypeDefinition type) => _objs.Any(o => o.ObjectType == type);
        #endregion

        #region IDataRow

        IDataContainer IDataItem.DataContainer => _table;

        bool IDataItem.IsLinked => false;

        Guid IDataItem.DataGuid => this.Id;

        string IDataItem.DataLocalId => this.LocalName;

        int IDataItem.Index => _index;

        IEnumerable<SObject> IDataItem.Components => _objs.Pass();

        IMemberContainer IMember.Container => _table;

        Asset IHasAsset.TargetAsset => this;

        #endregion
    }
}