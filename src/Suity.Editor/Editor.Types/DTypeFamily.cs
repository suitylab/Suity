using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a type family in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.TypeFamily, "Type Family")]
[NativeType(CodeBase = "Suity", Color = "#FCC419")]
public class DTypeFamily : GroupAsset,
    IRenderable,
    IAttributeGetter,
    IHasToolTips,
    ICodeRenderElement,
    IDataTableAsset
{

    private readonly string _version;
    private readonly string[] _supportedVersions;

    internal IAttributeDesign _attributes = EmptyAttributeDesign.Empty;
    private Color? _color;

    /// <summary>
    /// Initializes a new instance of the DTypeFamily class.
    /// </summary>
    public DTypeFamily()
    {
        _supportedVersions = [];

        UpdateAssetTypes(typeof(IRenderable), typeof(IDataTableAsset));
    }

    /// <summary>
    /// Gets the attributes associated with this type family.
    /// </summary>
    public IAttributeDesign Attributes
    {
        get => _attributes;
        internal protected set
        {
            if (_attributes == value)
            {
                return;
            }
            _attributes = value ?? EmptyAttributeDesign.Empty;
            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Updates the attributes for this type family.
    /// </summary>
    /// <param name="value">The new attributes.</param>
    /// <param name="notify">Whether to notify of changes.</param>
    internal void UpdateAttributes(IAttributeDesign value, bool notify)
    {
        _attributes = value ?? EmptyAttributeDesign.Empty;

        foreach (var attr in _attributes.GetAttributes<DesignAttribute>())
        {
            attr.AttributeOwner = this;
        }

        _color = _attributes.GetAttribute<IViewColor>()?.ViewColor;
        if (_color == Color.Empty)
        {
            _color = null;
        }

        if (notify)
        {
            NotifyPropertyUpdated(nameof(Attributes));
        }
    }

    /// <inheritdoc />
    public override ImageDef DefaultIcon => CoreIconCache.Class;

    /// <inheritdoc />
    public override bool CanExportToLibrary => true;

    /// <summary>
    /// Gets the version of this type family.
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// Gets the supported versions.
    /// </summary>
    public string[] SupportedVersions => [.. _supportedVersions];

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    #region IHasToolTips

    public string ToolTips => _attributes.GetAttribute<ToolTipsAttribute>()?.ToolTips;

    #endregion

    #region IViewColor

    public override Color? ViewColor
    {
        get => _color ?? base.ViewColor;
        internal protected set => base.ViewColor = value;
    }

    #endregion

    #region IRenderable

    public virtual bool RenderEnabled => true;

    public virtual IEnumerable<RenderItem> GetRenderItems()
    {
        string typeName = ShortTypeName;

        yield return new RenderItem(Id, this, RenderType.TypeFamily, typeName, this, this.LastUpdateTime, _attributes);
        //yield return new RenderItem(Id, this, RenderType.TypeFormatter, typeName + "Formatter", new DTypeFormatter(this), this.LastUpdateTime, _attributes);

        foreach (DType type in GetChildAssets(AssetFilters.Default).OfType<DType>())
        {
            if (type.IsImported)
            {
                continue;
            }

            if (type.RenderType != null)
            {
                yield return new RenderItem(type.Id, this, type.RenderType, type.LocalName, type, type.LastUpdateTime, type.Attributes);
            }
        }
    }

    public virtual IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath)
    {
        var path = basePath.WithNameSpace(NameSpace);

        return GetRenderItems().SelectMany(o => material.GetRenderTargets(o, path));
    }

    public virtual ICodeLibrary GetCodeLibrary() => this.GetAttachedUserLibrary();

    public virtual IMaterial DefaultMaterial => null;

    #endregion

    #region IDataContainer

    public virtual IDataContainer GetDataContainer(bool tryLoadStorage) => this.GetDocument<IDataContainer>(tryLoadStorage);

    #endregion

    /// <summary>
    /// Gets the function container for this type family.
    /// </summary>
    /// <returns>The function container.</returns>
    public virtual IFunctionContainer GetFunctionContainer() => this.GetDocument<IFunctionContainer>(true);

    /// <inheritdoc />
    public override object GetProperty(CodeRenderProperty property, object argument)
    {
        switch (property.PropertyName)
        {
            case CodeRenderProperty.Attributes:
                if (argument is string typeName)
                {
                    return _attributes?.GetAttributes(typeName);
                }
                else if (argument == null)
                {
                    // Do a conversion here, get all attributes when no argument
                    return _attributes?.GetAttributes();
                }
                else
                {
                    return Array.Empty<object>();
                }
            case CodeRenderProperty.Version:
                return _version;

            case CodeRenderProperty.SupportedVersions:
                return _supportedVersions;

            default:
                return base.GetProperty(property, argument);
        }
    }
}

/// <summary>
/// Builder for creating and modifying DTypeFamily instances.
/// </summary>
public class DTypeFamilyBuilder : GroupAssetBuilder<DTypeFamily>, IDesignBuilder
{
    private IAttributeDesign _attribute = EmptyAttributeDesign.Empty;

    /// <summary>
    /// Initializes a new instance of the DTypeFamilyBuilder class.
    /// </summary>
    public DTypeFamilyBuilder()
    {
        AddAutoUpdate(nameof(DTypeFamily.Attributes), o => o.UpdateAttributes(_attribute, false));
    }

    /// <summary>
    /// Updates the attributes for the type family.
    /// </summary>
    /// <param name="attributes">The new attributes.</param>
    public void UpdateAttributes(IAttributeDesign attributes)
    {
        _attribute = attributes ?? EmptyAttributeDesign.Empty;
        TryUpdateNow(d => d.UpdateAttributes(_attribute, true));
    }

    /// <inheritdoc />
    public void SetBindingInfo(object bindingInfo)
    {
    }
}

/// <summary>
/// Provides formatting information for a type family.
/// </summary>
public class DTypeFamilyFormatter : ICodeRenderElement, IHasId
{
    private readonly DTypeFamily _family;

    /// <summary>
    /// Initializes a new instance of the DTypeFamilyFormatter class.
    /// </summary>
    /// <param name="family">The type family.</param>
    public DTypeFamilyFormatter(DTypeFamily family)
    {
        _family = family;
    }

    /// <summary>
    /// Gets the type family.
    /// </summary>
    public DTypeFamily Family => _family;

    /// <inheritdoc />
    public RenderType RenderType => RenderType.TypeFormatter;

    #region ICodeRenderSupport

    /// <inheritdoc />
    public object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.Id => _family.Id,
        CodeRenderProperty.Name => _family.LocalName + "Formatter",
        CodeRenderProperty.PathName => _family.AssetKey,
        _ => null,
    };

    /// <inheritdoc />
    public Guid Id => _family.Id;

    #endregion
}

/// <summary>
/// Provides formatting information for a struct.
/// </summary>
public class DStructFormatter : ICodeRenderElement, IHasId
{
    private readonly DCompond _type;

    /// <summary>
    /// Initializes a new instance of the DStructFormatter class.
    /// </summary>
    /// <param name="type">The struct type.</param>
    public DStructFormatter(DCompond type)
    {
        _type = type;
    }

    /// <summary>
    /// Gets the struct.
    /// </summary>
    public DCompond Struct => _type;

    /// <inheritdoc />
    public RenderType RenderType => RenderType.TypeFormatter;

    #region ICodeRenderSupport

    /// <inheritdoc />
    public object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.Id => _type.Id,
        CodeRenderProperty.Name => _type.LocalName + "Formatter",
        CodeRenderProperty.PathName => _type.AssetKey,
        _ => null,
    };

    /// <inheritdoc />
    public Guid Id => _type.Id;

    #endregion
}