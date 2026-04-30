using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// A workspace reference item that represents a file bunch, producing render targets from grouped files.
/// </summary>
public class FileBunchRefItem : WorkSpaceRefItem
{
    private AssetSelection<IFileBunch> _fileBunch = new();
    private bool _uploadMode;

    /// <summary>
    /// Initializes a new empty instance of <see cref="FileBunchRefItem"/>.
    /// </summary>
    public FileBunchRefItem()
    {
    }

    /// <summary>
    /// Initializes a new instance referencing the file bunch with the specified ID.
    /// </summary>
    /// <param name="id">The file bunch asset ID.</param>
    public FileBunchRefItem(Guid id)
    {
        _fileBunch.Id = id;
    }

    /// <inheritdoc/>
    public override int Order => 3;

    /// <inheritdoc/>
    public override Guid Id
    {
        get => _fileBunch.Id;
        set => base.Id = _fileBunch.Id = value;
    }

    /// <inheritdoc/>
    public override bool UploadMode => _uploadMode;

    /// <summary>
    /// Gets or sets the selected key within the file bunch.
    /// </summary>
    public string Key
    {
        get => _fileBunch.SelectedKey;
        set => _fileBunch.SelectedKey = value;
    }

    /// <summary>
    /// Gets the resolved file bunch, or null if not available.
    /// </summary>
    public IFileBunch FileBunch => _fileBunch.Target;

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets()
    {
        if (!Enabled)
        {
            return [];
        }

        IFileBunch bunch = FileBunch;
        if (bunch is null)
        {
            return [];
        }

        RenderFileName basePath = WorkSpace.GetBasePath();

        var targets = bunch.GetRenderTargets(basePath, UploadMode).WithAction(o => o.Suspended = Suspended);
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
        _fileBunch = sync.Sync("FileBunch", _fileBunch, SyncFlag.NotNull);
        _uploadMode = sync.Sync("UploadMode", _uploadMode);

        if (sync.IsSetterOf("FileBunch"))
        {
            base.Id = _fileBunch.Id;
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(UploadMode, new ViewProperty("UploadMode", "Upload Mode"));
    }

    #endregion
}