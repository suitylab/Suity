using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a controller type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Controller)]
public class DController : DCompond, IRenderable
{
    /// <summary>
    /// Initializes a new instance of the DController class.
    /// </summary>
    public DController()
    {
        UpdateAssetTypes(typeof(IRenderable));
    }

    /// <summary>
    /// Initializes a new instance of the DController class with a name.
    /// </summary>
    public DController(string name)
        : this()
    {
        LocalName = name;
    }

    /// <inheritdoc />
    public override Image DefaultIcon => CoreIconCache.Controller;

    /// <inheritdoc />
    public override RenderType RenderType => RenderType.TriggerController;

    /// <inheritdoc />
    public override bool CanExportToLibrary => true;

    #region IRenderable Members

    /// <inheritdoc />
    public virtual bool RenderEnabled => true;

    /// <inheritdoc />
    public virtual IEnumerable<RenderItem> GetRenderItems()
    {
        string typeName = ShortTypeName;

        yield return new RenderItem(Id, this, RenderType.TriggerController, typeName, this, this.LastUpdateTime);
    }

    public virtual IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath)
    {
        var path = basePath.WithNameSpace(NameSpace);

        return GetRenderItems().SelectMany(o => material.GetRenderTargets(o, path));
    }

    /// <inheritdoc />
    public virtual ICodeLibrary GetCodeLibrary() => this.GetAttachedUserLibrary();

    /// <inheritdoc />
    public IMaterial DefaultMaterial => null;

    #endregion

    /// <summary>
    /// Gets the controller container.
    /// </summary>
    /// <returns>The controller container.</returns>
    public virtual IControllerContainer GetControllerContainer()
        => this.GetDocument<IControllerContainer>(true);
}

/// <summary>
/// Builder for creating DController instances.
/// </summary>
public class DControllerBuilder : DBaseStructBuilder<DController>
{
    /// <summary>
    /// Initializes a new instance of the DControllerBuilder class.
    /// </summary>
    public DControllerBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the DControllerBuilder class with a name and icon.
    /// </summary>
    public DControllerBuilder(string name, string iconKey)
        : this()
    {
        SetLocalName(name);
        SetIconKey(iconKey);
    }
}