using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Types
{
    /// <summary>
    /// Represents an abstract struct type in the editor.
    /// </summary>
[AssetTypeBinding(AssetDefNames.Abstract, "Abstract Structure")]
    public class DAbstract : DCompond, IDataAsset
{
    /// <summary>
    /// Gets the default color for abstract types.
    /// </summary>
    public static Color AbstractTypeColor { get; } = Color.FromArgb(222, 147, 244);

    private readonly EditorAssetRef<DAbstract> _baseType = new();
    private IRegistryHandle<DAbstract> _baseTypeEntry;

    /// <summary>
    /// Initializes a new instance of the DAbstract class.
    /// </summary>
    public DAbstract()
    {
        UpdateAssetTypes(typeof(IDataAsset));
        AddUpdateRelationship(_baseType);
    }

    /// <summary>
    /// Initializes a new instance of the DAbstract class with a name and icon.
    /// </summary>
    public DAbstract(string localName, string icon)
        : this()
    {
        LocalName = localName;
        IconKey = icon;
    }

    /// <inheritdoc />
    public override ImageDef DefaultIcon => CoreIconCache.Abstract;

    /// <inheritdoc />
    public override Color? TypeColor => AbstractTypeColor;

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.AbstractStruct;

    /// <inheritdoc />
    public override TypeDefinition PrimaryTypeDefinition => DTypeManager.Instance.GetStructsByBaseType(this)?.PrimaryAsset?.Definition;

    /// <inheritdoc />
    public override RenderType RenderType => RenderType.Abstract;

    /// <summary>
    /// Gets or sets the base type ID.
    /// </summary>
    public Guid BaseTypeId
        {
            get => _baseType.Id;
            internal protected set
            {
                if (_baseType.Id == value)
                {
                    return;
                }
                _baseType.Id = value;
                _baseTypeEntry?.Dispose();
                _baseTypeEntry = DTypeManager.Instance.AddToBaseType(this);
                NotifyPropertyUpdated(nameof(BaseType));
            }
        }

        public override bool IsAssignableFrom(TypeDefinition implementType)
        {
            return implementType.BaseAbstractType == Definition;
        }

        #region IDataRowContext

        public TypeDefinition[] GetDataTypes() => [];

        public bool SupportType(TypeDefinition type) => false;

        public IDataItem GetData(bool tryLoadStorage) => null;

        #endregion

/// <summary>
    /// Gets the derived structs.
    /// </summary>
    /// <returns>Collection of derived structs.</returns>
    public IEnumerable<DStruct> GetDerivedStructs()
    {
        var collection = DTypeManager.Instance.GetStructsByBaseType(this);
        if (collection != null)
        {
            return collection.Assets;
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc />
    internal override void InternalOnAssetActivate(string assetKey)
        {
            _baseTypeEntry?.Dispose();
            _baseTypeEntry = DTypeManager.Instance.AddToBaseType(this);

            base.InternalOnAssetActivate(assetKey);
        }

        internal override void InternalOnAssetDeactivate(string assetKey)
        {
            var entry = _baseTypeEntry;
            _baseTypeEntry = null;
            entry?.Dispose();

            base.InternalOnAssetDeactivate(assetKey);
        }

        protected override void OnIsPrimaryUpdated()
        {
            base.OnIsPrimaryUpdated();

            _baseTypeEntry?.Update();
        }
    }

    public class DAbstractBuilder : DBaseStructBuilder<DAbstract>
    {
        private readonly EditorAssetRef<DAbstract> _baseType = new();

        public DAbstractBuilder()
        {
            AddAutoUpdate(nameof(DStruct.BaseType), o => o.BaseTypeId = _baseType.Id);
        }

        public DAbstractBuilder(string name, string iconKey)
            : this()
        {
            SetLocalName(name);
            SetIconKey(iconKey);
        }

        public void UpdateBaseType(DAbstract baseType)
        {
            _baseType.Target = baseType;
            UpdateAuto(nameof(DStruct.BaseType));
        }

        public void UpdateBaseType(Guid id)
        {
            _baseType.Id = id;
            UpdateAuto(nameof(DStruct.BaseType));
        }

        public void UpdateBaseType(string assetKey)
        {
            _baseType.AssetKey = assetKey;
            UpdateAuto(nameof(DStruct.BaseType));
        }
    }
}