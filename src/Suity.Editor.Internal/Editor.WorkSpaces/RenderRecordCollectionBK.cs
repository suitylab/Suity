using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Internal implementation of <see cref="RenderRecordCollection"/> that tracks rendered files,
/// render status, and file modifications for a workspace.
/// </summary>
internal class RenderRecordCollectionBK : RenderRecordCollection, IViewObject, ITextDisplay
{
    private readonly WorkSpaceBK _workSpace;

    private readonly Dictionary<string, RenderStatus> _renderStatus = [];

    // Rendered files <rFileLowId, rFileName>
    private readonly Dictionary<string, RenderFileRecordBK> _renderedFiles = [];

    // Externally modified files <rFileLowId, rFileName>
    private readonly Dictionary<string, string> _modifiedFiles = [];

    private readonly RenderModifyList _modifyPreviewList;

    /// <summary>
    /// Initializes a new instance of <see cref="RenderRecordCollectionBK"/> for the specified workspace.
    /// </summary>
    /// <param name="workSpace">The owning workspace.</param>
    public RenderRecordCollectionBK(WorkSpaceBK workSpace)
    {
        _workSpace = workSpace ?? throw new ArgumentNullException(nameof(workSpace));

        _modifyPreviewList = new RenderModifyList(this);
    }

    /// <summary>
    /// Gets the owning workspace.
    /// </summary>
    public WorkSpace WorkSpace => _workSpace;

    /// <summary>
    /// Gets the render targets associated with the specified relative file path.
    /// </summary>
    /// <param name="rPath">The relative file path.</param>
    /// <returns>A collection of render targets for the file.</returns>
    public IEnumerable<RenderTarget> GetRenderTargets(RenderTargetPage page, string rPath)
    {
        return page.RenderTargetsByFileId[rPath.GetPathLowId()];
    }

    #region Rendered

    /// <summary>
    /// Sets the collection of rendered files from a previous configuration.
    /// </summary>
    /// <param name="records">The file records to load.</param>
    /// <param name="now">The current timestamp to assign as last update time.</param>
    public void SetRenderedFiles(IEnumerable<RenderFileRecordBK> records, DateTime now)
    {
        _renderedFiles.Clear();
        _renderedFiles.AddRange(records, o => o.RelativeFileName.GetPathLowId());
        foreach (var item in _renderedFiles.Values)
        {
            item._lastUpdateTime = now;
        }
    }

    /// <summary>
    /// Adds a render target as a rendered file record.
    /// </summary>
    /// <param name="target">The render target to add.</param>
    /// <returns>The created file record.</returns>
    public RenderFileRecordBK AddRenderedFile(RenderTarget target)
    {
        var record = target.ToRecord();
        _renderedFiles[target.FileName.PhysicRelativePath.GetPathLowId()] = record;
        return record;
    }

    /// <summary>
    /// Gets an existing rendered file record or creates a new one.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="now">The timestamp to assign if creating a new record.</param>
    /// <returns>The existing or newly created file record.</returns>
    public RenderFileRecordBK GetOrAddRenderedFile(RenderTarget target, DateTime now)
    {
        string id = target.FileName.PhysicRelativePath.GetPathLowId();
        var record = _renderedFiles.GetOrAdd(id, _ => new RenderFileRecordBK(target.FileName.PhysicRelativePath, now));
        record._lastUpdateTime = now;
        return record;
    }

    /// <summary>
    /// Records the result of a file render operation.
    /// </summary>
    /// <param name="result">The render result.</param>
    /// <param name="now">The timestamp to record.</param>
    public void AddFileRenderResult(TargetFileRenderResult result, DateTime now)
    {
        string rPath = result.Target.FileName.PhysicRelativePath;
        string fileId = rPath.GetPathLowId();
        _renderStatus[fileId] = result.Status;
        _renderedFiles[fileId] = new RenderFileRecordBK(rPath, result.OldFileName, now);
    }

    /// <summary>
    /// Binds a relative file path as a rendered file.
    /// </summary>
    /// <param name="rPath">The relative file path.</param>
    /// <returns>True if the file was newly bound; false if it already existed.</returns>
    public bool BindRenderedFile(string rPath)
    {
        string lowId = rPath.GetPathLowId();
        if (!_renderedFiles.ContainsKey(lowId))
        {
            _renderedFiles.Add(lowId, new RenderFileRecordBK(rPath, DateTime.MinValue));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Unbinds a rendered file by its relative path.
    /// </summary>
    /// <param name="rPath">The relative file path.</param>
    /// <returns>True if the file was removed.</returns>
    public bool UnbindRenderedFile(string rPath)
    {
        return _renderedFiles.Remove(rPath.GetPathLowId());
    }

    /// <summary>
    /// Removes a rendered file by its internal ID.
    /// </summary>
    /// <param name="id">The file ID.</param>
    /// <returns>True if the file was removed.</returns>
    public bool RemoveRenderedFile(string id)
    {
        return _renderedFiles.Remove(id);
    }

    /// <summary>
    /// Removes a rendered file by its relative path.
    /// </summary>
    /// <param name="rPath">The relative file path.</param>
    /// <returns>True if the file was removed.</returns>
    public bool RemoveRenderedFileByRelativePath(string rPath)
    {
        return _renderedFiles.Remove(rPath.GetPathLowId());
    }

    /// <inheritdoc/>
    public override IEnumerable<RenderFileRecord> RenderedFiles => _renderedFiles.Values;

    /// <inheritdoc/>
    public override IEnumerable<string> RenderedFileIds => _renderedFiles.Keys;

    /// <inheritdoc/>
    public override RenderFileRecord GetRenderedFile(string id) => _renderedFiles.GetValueSafe(id);

    /// <inheritdoc/>
    public override bool ContainsRenderedFile(string id) => _renderedFiles.ContainsKey(id);

    #endregion

    #region RenderStatus

    /// <inheritdoc/>
    public override RenderStatus GetRenderStatus(string id)
    {
        return _renderStatus.GetValueSafe(id);
    }

    /// <inheritdoc/>
    public override RenderStatus GetRenderStatusByRelativePath(string rPath)
    {
        string id = rPath.GetPathLowId();
        return _renderStatus.GetValueSafe(id);
    }

    /// <summary>
    /// Removes all render status entries whose keys match the specified predicate.
    /// </summary>
    /// <param name="predicateId">The predicate to test file IDs.</param>
    public void RemoveAllRenderStatus(Predicate<string> predicateId)
    {
        _renderStatus.RemoveAllByKey(predicateId);
    }

    #endregion

    #region Modify

    /// <inheritdoc/>
    public override bool ContainsModifiedFile(string id) => _modifiedFiles.ContainsKey(id);

    /// <inheritdoc/>
    public override void AddModifiedFileByRelativePath(string rPath)
    {
        _modifiedFiles[rPath.GetPathLowId()] = rPath;
    }

    /// <summary>
    /// Checks whether there are any files scheduled to be added.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    /// <returns>True if there are adding files.</returns>
    public bool ContainsAddingFiles(RenderTargetPage page)
    {
        return page.RenderTargetsByFileId.Keys.Any(id => !ContainsRenderedFile(id));
    }

    /// <summary>
    /// Checks whether there are any files scheduled to be removed.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    /// <returns>True if there are removing files.</returns>
    public bool ContainsRemovingFiles(RenderTargetPage page)
    {
        return RenderedFileIds.Any(id => !page.RenderTargetsByFileId.ContainsKey(id));
    }

    /// <inheritdoc/>
    public override bool ContainsModifyingFiles() => _modifiedFiles.Count > 0;

    /// <summary>
    /// Gets the list of files undergoing the specified modification type.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    /// <param name="type">The type of modification.</param>
    /// <returns>A collection of relative file paths.</returns>
    public IEnumerable<string> GetModifyingFiles(RenderTargetPage page, RenderModifyType type)
    {
        switch (type)
        {
            case RenderModifyType.Add:
                return from id in page.RenderTargetsByFileId.Keys
                       where !ContainsRenderedFile(id)
                       select page.RenderTargetsByFileId[id].First().FileName.PhysicRelativePath;

            case RenderModifyType.Remove:
                return from id in RenderedFileIds
                       where !page.RenderTargetsByFileId.ContainsKey(id)
                       select GetRenderedFile(id).RelativeFileName;

            case RenderModifyType.Modify:
                return from id in page.RenderTargetsByFileId.Keys
                       where ContainsRenderedFile(id) && page.RenderTargetsByFileId[id].Any(o => GetIsDirty(o))
                       select page.RenderTargetsByFileId[id].First().FileName.PhysicRelativePath;

            case RenderModifyType.None:
            default:
                return [];
        }
    }

    #endregion

    #region Dirty

    /// <summary>
    /// Updates the render time for each rendered file based on the latest target update times.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    public void UpdateRenderTime(RenderTargetPage page)
    {
        foreach (var item in _renderedFiles)
        {
            foreach (var target in page.RenderTargetsByFileId[item.Key])
            {
                if (target.LastUpdateTime > item.Value.LastUpdateTime)
                {
                    item.Value._lastUpdateTime = target.LastUpdateTime;
                }
            }
        }
    }

    /// <summary>
    /// Updates the render time for all rendered files to the specified timestamp.
    /// </summary>
    /// <param name="now">The new timestamp.</param>
    public void UpdateRenderTime(DateTime now)
    {
        foreach (var item in _renderedFiles)
        {
            item.Value._lastUpdateTime = now;
        }
    }

    /// <summary>
    /// Determines whether a render target is dirty and needs re-rendering.
    /// </summary>
    /// <param name="target">The render target to check.</param>
    /// <returns>True if the target is dirty.</returns>
    public bool GetIsDirty(RenderTarget target)
    {
        string relativePath = target.FileName.PhysicRelativePath;
        string fileId = relativePath.GetPathLowId();

        var record = GetRenderedFile(fileId);
        if (record is null)
        {
            return true;
        }

        if (record.Dirty)
        {
            return true;
        }

        if (target.LastUpdateTime > record.LastUpdateTime)
        {
            return true;
        }

        if (_modifiedFiles.ContainsKey(fileId))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a file at the specified relative path is dirty.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    /// <param name="relativePath">The relative file path.</param>
    /// <returns>True if the file is dirty.</returns>
    public bool GetIsDirtyByRelativePath(RenderTargetPage page, string relativePath)
    {
        string fileId = relativePath.GetPathLowId();
        var target = page.RenderTargetsByFileId[fileId].FirstOrDefault();
        if (target is null)
        {
            return true;
        }

        var record = GetRenderedFile(fileId);
        if (record is null)
        {
            return true;
        }

        if (record.Dirty)
        {
            return true;
        }

        if (target.LastUpdateTime > record.LastUpdateTime)
        {
            return true;
        }

        if (_modifiedFiles.ContainsKey(fileId))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all dirty render targets, optionally filtered by incremental mode.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    /// <param name="incremental">If true, returns only dirty targets; otherwise returns all.</param>
    /// <returns>A collection of dirty render targets.</returns>
    public IEnumerable<RenderTarget> GetDirtyRenderTargets(RenderTargetPage page, bool incremental)
    {
        var targets = page.RenderTargetsByFileId.Values;

        if (incremental)
        {
            return targets.Where(o => GetIsDirty(o));
        }
        else
        {
            return targets;
        }
    }

    /// <summary>
    /// Clears the dirty state by removing all modified file entries.
    /// </summary>
    public void ClearDirty()
    {
        _modifiedFiles.Clear();
    }

    /// <summary>
    /// Clears render state and optionally preserves rendered files in incremental mode.
    /// </summary>
    /// <param name="removedRPaths">The relative paths of removed files.</param>
    /// <param name="incremental">If true, preserves non-removed rendered files.</param>
    public void ClearRender(IEnumerable<string> removedRPaths, bool incremental)
    {
        if (incremental)
        {
            _renderStatus.Clear();
            // Cannot delete all rendered files in incremental mode
            foreach (var remove in removedRPaths)
            {
                _renderedFiles.Remove(remove.GetPathLowId());
            }
        }
        else
        {
            _renderStatus.Clear();
            _renderedFiles.Clear();
        }

        _modifiedFiles.Clear();
    }

    /// <summary>
    /// Clears all render status, rendered files, and modified files.
    /// </summary>
    public void ClearAll()
    {
        _renderStatus.Clear();
        _renderedFiles.Clear();
        _modifiedFiles.Clear();
    }

    #endregion

    #region IViewObject

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        _modifyPreviewList.Refresh(_workSpace.RenderPage);
        setup.DetailTreeViewField(_modifyPreviewList, new ViewProperty("ModifyList", $"Modified ({_modifyPreviewList.Count})"));
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.Intent == SyncIntent.View)
        {
            sync.Sync("ModifyList", _modifyPreviewList, SyncFlag.GetOnly);
        }
    }

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText => "Render Preview";

    /// <inheritdoc/>
    public object DisplayIcon => CoreIconCache.Render;

    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Normal;

    #endregion
}

/// <summary>
/// A list that displays render file modifications (add, modify, remove) for preview purposes.
/// </summary>
internal class RenderModifyList : ISyncList, IViewList, ITextDisplay
{
    private readonly RenderRecordCollectionBK _collection;

    private readonly List<RenderModifyListItem> _items = [];

    /// <summary>
    /// Initializes a new instance of <see cref="RenderModifyList"/>.
    /// </summary>
    /// <param name="collection">The owning render record collection.</param>
    public RenderModifyList(RenderRecordCollectionBK collection)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    /// <summary>
    /// Gets the number of modification items in the list.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Refreshes the list by scanning the current page for added, modified, and removed files.
    /// </summary>
    /// <param name="page">The current render target page.</param>
    public void Refresh(RenderTargetPage page)
    {
        _items.Clear();

        foreach (var file in _collection.GetModifyingFiles(page, RenderModifyType.Add))
        {
            _items.Add(new RenderModifyListItem(_collection, file, RenderModifyType.Add));
        }

        foreach (var file in _collection.GetModifyingFiles(page, RenderModifyType.Modify))
        {
            _items.Add(new RenderModifyListItem(_collection, file, RenderModifyType.Modify));
        }

        foreach (var file in _collection.GetModifyingFiles(page, RenderModifyType.Remove))
        {
            _items.Add(new RenderModifyListItem(_collection, file, RenderModifyType.Remove));
        }
    }

    #region IViewList

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;

    /// <inheritdoc/>
    int ISyncList.Count => _items.Count;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        switch (sync.Mode)
        {
            case SyncMode.RequestElementType:
                sync.Sync(0, typeof(RenderModifyListItem));
                break;

            case SyncMode.Get:
                if (sync.Index >= 0 && sync.Index < _items.Count)
                {
                    sync.Sync(sync.Index, _items[sync.Index]);
                }
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < _items.Count; i++)
                {
                    sync.Sync(i, _items[i]);
                }
                break;
        }
    }

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value) => false;

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value) => null;

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText => "Render";

    /// <inheritdoc/>
    public object DisplayIcon => CoreIconCache.Tag;

    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Reference;

    #endregion
}

/// <summary>
/// Represents a single file modification entry in the render modify list.
/// </summary>
internal class RenderModifyListItem : IViewObject, ITextDisplay, IViewDoubleClickAction, INavigable
{
    private RenderRecordCollectionBK _collection;

    /// <summary>
    /// Initializes a new instance of <see cref="RenderModifyListItem"/>.
    /// </summary>
    /// <param name="collection">The owning render record collection.</param>
    /// <param name="rPath">The relative file path.</param>
    /// <param name="type">The type of modification.</param>
    public RenderModifyListItem(RenderRecordCollectionBK collection, string rPath, RenderModifyType type)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        RelativePath = rPath ?? throw new ArgumentNullException(nameof(rPath));
        Type = type;
    }

    /// <summary>
    /// Gets the relative file path of the modified file.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Gets the type of modification.
    /// </summary>
    public RenderModifyType Type { get; }

    /// <summary>
    /// Gets the type of modification (alias for <see cref="Type"/>).
    /// </summary>
    public RenderModifyType ModifyType { get; }

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText => RelativePath;

    /// <inheritdoc/>
    public object DisplayIcon => EditorUtility.GetIconForFileExact(RelativePath);

    /// <inheritdoc/>
    public TextStatus DisplayStatus => Type switch
    {
        RenderModifyType.Add => TextStatus.Add,
        RenderModifyType.Remove => TextStatus.Remove,
        RenderModifyType.Modify => TextStatus.Modify,
        _ => TextStatus.Normal,
    };

    #endregion

    #region IViewObject

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    #endregion

    #region IViewDoubleClickAction

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        EditorUtility.LocateWorkSpace(_collection.WorkSpace, RelativePath);
    }

    #endregion

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return new LocateWorkSpaceVReq { WorkSpace = _collection.WorkSpace, RelativeFileName = RelativePath };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return RelativePath;
    }
}