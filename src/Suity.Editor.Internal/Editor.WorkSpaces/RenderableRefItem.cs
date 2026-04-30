using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// A workspace reference item that represents a renderable asset, producing render targets
/// for each associated material.
/// </summary>
public class RenderableRefItem : WorkSpaceRefItem, IRenderableRefItem, IEditorObjectListener
{
    private AssetSelection<IRenderable> _renderable = new();
    private readonly AutoNewSyncList<AssetSelection<IMaterial>> _includes = new();
    private string _customNameSpace = string.Empty;

    /// <summary>
    /// Initializes a new empty instance of <see cref="RenderableRefItem"/>.
    /// </summary>
    public RenderableRefItem()
    {
    }

    /// <summary>
    /// Initializes a new instance referencing the renderable asset with the specified ID.
    /// </summary>
    /// <param name="id">The renderable asset ID.</param>
    public RenderableRefItem(Guid id)
    {
        _renderable.Id = id;

        var asset = _renderable.TargetAsset;
        if (asset != null)
        {
            var codeLib = asset.GetAttachedUserLibrary();
            if (codeLib != null)
            {
                UserCodeId = codeLib.Id;
            }
        }
    }

    /// <inheritdoc/>
    public override int Order => 1;

    /// <inheritdoc/>
    public override Guid Id
    {
        get => _renderable.Id;
        set => base.Id = _renderable.Id = value;
    }

    /// <inheritdoc/>
    public override bool UploadMode => false;

    /// <summary>
    /// Gets the resolved renderable asset, or null if not available.
    /// </summary>
    public IRenderable Renderable => _renderable.Target;

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets()
    {
        return GetRenderTargets([]);
    }

    /// <summary>
    /// Gets the render targets produced by this renderable for the specified materials.
    /// </summary>
    /// <param name="materials">Additional materials to include alongside configured ones.</param>
    /// <returns>A collection of render targets.</returns>
    public IEnumerable<RenderTarget> GetRenderTargets(IEnumerable<IMaterial> materials)
    {
        if (!Enabled)
        {
            yield break;
        }

        HashSet<IMaterial> myMats = [.. materials.OfType<IMaterial>()];
        myMats.UnionWith(_includes.List.Select(o => o?.Target).OfType<IMaterial>());

        IRenderable renderable = _renderable.Target;
        if (renderable == null)
        {
            yield break;
        }

        RenderFileName basePath = WorkSpace.GetBasePath().WithNameSpace(_customNameSpace);

        foreach (var material in myMats)
        {
            RenderTarget[] targets = null;
            try
            {
                targets = renderable.GetRenderTargets(material, basePath).ToArray();
            }
            catch (Exception err)
            {
                err.LogError($"Failed to get RenderTarget:{renderable}");
            }

            if (targets != null)
            {
                foreach (RenderTarget target in targets)
                {
                    target.UpdateTime(LastUpdateTime);
                    target.Suspended = Suspended;
                    target.Tag = this;
                    target.UserCodeEnabled = this.UserCodeEnabled;

                    yield return target;
                }
            }
        }
    }

    #region ISyncObject

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        _renderable = sync.Sync("Renderable", _renderable, SyncFlag.NotNull);
        _customNameSpace = sync.Sync("CustomNameSpace", _customNameSpace, SyncFlag.NotNull);
        sync.Sync("IncludedMaterials", _includes, SyncFlag.GetOnly);

        if (sync.IsSetterOf("Renderable"))
        {
            base.Id = _renderable.Id;
        }

        _renderable.Filter = AssetFilters.All;
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_includes, new ViewProperty("IncludedMaterials", "Materials").WithWriteBack());
        //setup.Field(_customNameSpace, new ViewProperty("CustomNameSpace", "Custom Namespace") { ForceWriteBack = true });
    }

    #endregion

    #region IEditorObjectListener

    void IEditorObjectListener.HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool haneld)
    {
        Modified?.Invoke(this);
    }

    #endregion

    #region IRenderableRefItem

    public bool AddMaterial(IMaterial material)
    {
        if (material is null || material.Id == Guid.Empty)
        {
            return false;
        }

        if (_includes.List.Any(o => o.Id == material.Id))
        {
            return false;
        }

        var sel = new AssetSelection<IMaterial>(material);
        _includes.List.Add(sel);
        Modified?.Invoke(this);

        return true;
    }

    public bool RemoveMaterial(IMaterial material)
    {
        if (material is null)
        {
            return false;
        }

        var sel = _includes.List.FirstOrDefault(o => o.Target == material);
        if (sel is null)
        {
            return false;
        }

        _includes.List.Remove(sel);
        Modified?.Invoke(this);

        return true;
    }

    #endregion
}