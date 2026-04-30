using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// A workspace reference item that represents a render target library.
/// Produces render targets from the associated <see cref="IRenderTargetLibrary"/>.
/// </summary>
public class RenderTargetLibraryRefItem : WorkSpaceRefItem, IEditorObjectListener
{
    private AssetSelection<IRenderTargetLibrary> _library = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="RenderTargetLibraryRefItem"/>.
    /// </summary>
    public RenderTargetLibraryRefItem()
    {
    }

    /// <summary>
    /// Initializes a new instance referencing the library with the specified ID.
    /// </summary>
    /// <param name="id">The library asset ID.</param>
    public RenderTargetLibraryRefItem(Guid id)
    {
        _library.Id = id;
    }

    /// <inheritdoc/>
    public override int Order => 2;

    /// <inheritdoc/>
    public override Guid Id
    {
        get => _library.Id;
        set => base.Id = _library.Id = value;
    }

    /// <inheritdoc/>
    public override bool UploadMode => false;

    /// <summary>
    /// Gets or sets the selected key within the render target library.
    /// </summary>
    public string Key
    {
        get => _library.SelectedKey;
        set => _library.SelectedKey = value;
    }

    /// <summary>
    /// Gets the resolved render target library, or null if not available.
    /// </summary>
    public IRenderTargetLibrary Library => _library.Target;

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets()
    {
        if (!Enabled)
        {
            return [];
        }

        var lib = Library;
        if (lib is null)
        {
            return [];
        }

        RenderFileName basePath = WorkSpace.GetBasePath();

        RenderTarget[] targets = lib.GetRenderTargets(basePath, this, null).ToArray();
        foreach (var target in targets)
        {
            target.UpdateTime(LastUpdateTime);
            target.Suspended = Suspended;
            target.Tag = this;
            target.UserCodeEnabled = this.UserCodeEnabled;
        }

        return targets;
    }

    #region ISyncObject, IInspectorObject

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        _library = sync.Sync("Library", _library, SyncFlag.NotNull);

        if (sync.IsSetterOf("Library"))
        {
            base.Id = _library.Id;
        }

        _library.Filter = AssetFilters.All;
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
    }

    #endregion

    #region IEditorObjectListener

    /// <inheritdoc/>
    void IEditorObjectListener.HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool haneld)
    {
        Modified?.Invoke(this);
    }

    #endregion
}