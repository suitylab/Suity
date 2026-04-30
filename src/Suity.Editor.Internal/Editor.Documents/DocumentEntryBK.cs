using Suity.Collections;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Documents;

/// <summary>
/// Internal backend implementation of a document entry, managing document lifecycle, loading, saving, view display, and dirty tracking.
/// </summary>
internal sealed class DocumentEntryBK : DocumentEntry, IViewListener
{
    private readonly DateTime _createTime;
    private DateTime _lastVisitTime;
    private StorageLocation _fileName;
    private DocumentFormat _format;
    private DocumentState _state;

    private Document _content;

    private HashSet<DocumentUsageToken> _usageTokens;

    private readonly object _sync = new();

    private readonly QueueOnceAction _updateViewAction;

    #region Construction and Disposal

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentEntryBK"/> class with default values.
    /// </summary>
    internal DocumentEntryBK()
    {
        _lastVisitTime = _createTime = DateTime.Now;
        _updateViewAction = new QueueOnceAction(() => View?.RefreshView());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentEntryBK"/> class with the specified format and file name.
    /// </summary>
    /// <param name="format">The document format.</param>
    /// <param name="fileName">The storage location of the document file.</param>
    internal DocumentEntryBK(DocumentFormat format, StorageLocation fileName)
        : this()
    {
        _format = format ?? throw new ArgumentNullException(nameof(format));
        _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }

    ~DocumentEntryBK()
    {
        Release();
        _content = null;
    }

    /// <inheritdoc/>
    public override bool IsReleased => _state == DocumentState.Released;

    /// <summary>
    /// Release document resources
    /// </summary>
    internal void Release()
    {
        try
        {
            CloseView();
        }
        catch (Exception err)
        {
            err.LogError("Release document view failed.");
        }

        lock (_sync)
        {
            _content?.OnDestroy();
            _content = null;
            _state = DocumentState.Released;
        }
    }

    /// <summary>
    /// Checks whether this document is currently in use.
    /// </summary>
    /// <returns>True if the document has active usage tokens; otherwise, false.</returns>
    internal bool CheckIsInUsage()
    {
        if (_usageTokens is null)
        {
            return false;
        }

        _usageTokens.RemoveAll(o => o.IsDisposed);

        if (_usageTokens.Count == 0)
        {
            _usageTokens = null;

            return false;
        }

        return true;
    }

    #endregion

    #region Main

    /// <summary>
    /// Create time
    /// </summary>
    /// <inheritdoc/>
    public override DateTime CreateTime => _createTime;

    /// <inheritdoc/>
    public override DateTime LastVisitTime => _lastVisitTime;

    /// <inheritdoc/>
    public override Document Content => _content;

    /// <inheritdoc/>
    public override StorageLocation FileName => _fileName;

    /// <summary>
    /// Sets the file name for this document entry.
    /// </summary>
    /// <param name="fileName">The new storage location.</param>
    internal void SetFileName(StorageLocation fileName)
    {
        _fileName = fileName;
    }

    /// <inheritdoc/>
    public override DocumentState State => _state;

    /// <inheritdoc/>
    public override DocumentFormat Format => _format;

    /// <summary>
    /// Sets the document format for this entry.
    /// </summary>
    /// <param name="format">The new document format.</param>
    internal void SetFormat(DocumentFormat format)
    {
        _format = format;
    }

    /// <inheritdoc/>
    public override Image Icon => _content?.Icon ?? _format?.Icon;

    /// <summary>
    /// Gets the underlying storage item for this document.
    /// </summary>
    /// <returns>The storage item.</returns>
    internal IStorageItem GetStream() => FileName.GetStorageItem();

    #endregion

    #region Load and Save

    /// <summary>
    /// Save document
    /// </summary>
    /// <returns>Returns whether save succeeded</returns>
    public override bool Save()
    {
        if (_state != DocumentState.Loaded)
        {
            return false;
        }

        if (!IsDirty)
        {
            return true;
        }

        bool saveSuccess = false;
        Exception err = null;

        // Push view data to document
        View?.SetDataToDocument();

        var document = EnsureDocumentContent();
        if (document is null)
        {
            return false;
        }

        RaiseSaving();

        FileUnwatchedAction.Do(() =>
        {
            lock (_sync)
            {
                try
                {
                    using var op = FileName.GetStorageItem();
                    saveSuccess = document.SaveDocument(op);
                }
                catch (Exception e)
                {
                    err = e;
                    saveSuccess = false;
                }
            }
        });

        if (saveSuccess)
        {
            ClearDirty();
            _content?.OnSaved();
            RaiseSaved();
            DocumentManager.Instance.RaiseDocumentSaved(this);

            return true;
        }
        else
        {
            if (err != null)
            {
                err.LogError($"Failed to save file:{FileName}");
                //EditorUtility.ShowError("Failed to save file:" + FileName, err);
            }
            else
            {
                Logs.LogError($"Failed to save file:{FileName}");
            }

            return false;
        }
    }

    /// <inheritdoc/>
    public override bool ForceSave()
    {
        if (_state != DocumentState.Loaded)
        {
            return false;
        }

        bool saveSuccess = false;
        Exception err = null;

        // Push view data to document
        View?.SetDataToDocument();

        var document = EnsureDocumentContent();
        if (document is null)
        {
            return false;
        }

        FileUnwatchedAction.Do(() =>
        {
            lock (_sync)
            {
                try
                {
                    RaiseSaving();
                    lock (_sync)
                    {
                        using var op = FileName.GetStorageItem();
                        saveSuccess = document.SaveDocument(op);
                    }
                }
                catch (Exception e)
                {
                    err = e;
                }
            }
        });

        if (saveSuccess)
        {
            ClearDirty();
            _content?.OnSaved();
            RaiseSaved();
            DocumentManager.Instance.RaiseDocumentSaved(this);

            return true;
        }
        else
        {
            err?.LogError("Failed to save file:" + FileName);

            return false;
        }
    }

    /// <inheritdoc/>
    public override void SaveDelayed()
    {
        EditorUtility.AddDelayedAction(new DelaySaveDocumentAction(this));
    }

    /// <inheritdoc/>
    public override bool Export(string fileName)
    {
        using var op = new FileStorageItem(fileName);
        return InternalExport(op);
    }


    /// <summary>
    /// Initializes the document content by calling its creation callback.
    /// </summary>
    internal void InternalCreate()
    {
        EnsureDocumentContent()?.OnCreated();
    }

    /// <summary>
    /// Creates a new document from scratch.
    /// </summary>
    /// <returns>True if the document was successfully created; otherwise, false.</returns>
    internal bool InternalNew()
    {
        lock (_sync)
        {
            if (_state != DocumentState.None)
            {
                return false;
            }

            var document = EnsureDocumentContent();
            if (document is null)
            {
                return false;
            }

            bool newOk = document.NewDocument();
            if (newOk)
            {
                _state = DocumentState.Loaded;
                document.OnLoaded(DocumentLoadingIntent.Normal);
                return true;
            }
            else
            {
                _state = DocumentState.None;
                return false;
            }
        }
    }

    /// <summary>
    /// Load document
    /// </summary>
    /// <returns>Returns whether load succeeded</returns>
    internal bool InternalLoad(object loaderObject = null, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        using var op = FileName.GetStorageItem();
        return InternalLoad(op, loaderObject, intent);
    }

    internal bool InternalLoad(IStorageItem storageItem, object loaderObject = null, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        bool loadSuccess = false;
        Exception err = null;

        lock (_sync)
        {
            switch (_state)
            {
                case DocumentState.Loaded:
                    return true;

                case DocumentState.Loading:
                case DocumentState.Released:
                    return false;

                case DocumentState.None:
                case DocumentState.Failed:
                default:
                    break;
            }

            var content = EnsureDocumentContent();
            if (content is null)
            {
                return false;
            }

            try
            {
                _state = DocumentState.Loading;
                content.OnReset();

                loadSuccess = content.LoadDocument(storageItem, loaderObject, intent);
            }
            catch (Exception e)
            {
                err = e;
            }
            if (loadSuccess)
            {
                _state = DocumentState.Loaded;
            }
            else
            {
                _state = DocumentState.Failed;
            }
            ClearDirty();
        }

        if (loadSuccess)
        {
            _content?.OnLoaded(intent);

            QueuedAction.Do(() =>
            {
                View?.GetDataFromDocument();
                if (EditorUtility.Inspector.IsObjectSelected(this))
                {
                    EditorUtility.Inspector.UpdateInspector();
                }
            });
            return true;
        }
        else
        {
            CloseView();

            if (err != null)
            {
            //if (DebugLog)
            //{
            //    AppService.Log.LogError("Failed to read file:" + FullPath, err);
                //}
                //else
                //{
                //}
                err.LogError("Failed to read file:" + FileName);
                return false;
            }
            return false;
        }
    }

    /// <summary>
    /// Reloads the document from its storage location.
    /// </summary>
    /// <returns>True if the document was successfully reloaded; otherwise, false.</returns>
    internal bool InternalReload()
    {
        lock (_sync)
        {
            if (_state == DocumentState.Released)
            {
                return false;
            }

            _state = DocumentState.None;
            _content?.OnReset();

            return InternalLoad(intent: DocumentLoadingIntent.Reload);
        }
    }

    /// <summary>
    /// Reloads the document from the specified storage item.
    /// </summary>
    /// <param name="storageItem">The storage item to load from.</param>
    /// <returns>True if the document was successfully reloaded; otherwise, false.</returns>
    internal bool InternalReload(IStorageItem storageItem)
    {
        lock (_sync)
        {
            if (_state == DocumentState.Released)
            {
                return false;
            }

            _state = DocumentState.None;
            _content?.OnReset();

            return InternalLoad(storageItem);
        }
    }

    /// <summary>
    /// Exports the document to the specified storage item.
    /// </summary>
    /// <param name="storageItem">The target storage item.</param>
    /// <returns>True if the export was successful; otherwise, false.</returns>
    internal bool InternalExport(IStorageItem storageItem)
    {
        if (storageItem is null)
        {
            throw new ArgumentNullException(nameof(storageItem));
        }

        if (_state != DocumentState.Loaded)
        {
            return false;
        }

        if (!Save())
        {
            return false;
        }

        bool exportSuccess = false;
        Exception err = null;

        // Push view data to document
        // Do not push because Export does not execute on main thread, execute all saves before exporting
        //View?.SetDataToDocument();

        var document = EnsureDocumentContent();
        if (document is null)
        {
            return false;
        }

        FileUnwatchedAction.Do(() =>
        {
            lock (_sync)
            {
                try
                {
                    lock (_sync)
                    {
                        exportSuccess = document.ExportDocument(storageItem);
                    }
                }
                catch (Exception e)
                {
                    err = e;
                }
            }
        });

        if (exportSuccess)
        {
            return true;
        }
        else
        {
            err?.LogError("Failed to export file:" + storageItem.FileName);
            return false;
        }
    }



    #endregion

    #region View

    /// <summary>
    /// Document view
    /// </summary>
    public override IDocumentView View
    {
        get
        {
            if (FileName?.PhysicFileName is null)
            {
                return null;
            }

            return DocumentViewManager.Current?.GetDocumentView(this);
        }
    }

    /// <summary>
    /// Show as document view
    /// </summary>
    /// <returns>Returns whether show succeeded</returns>v
    public override IDocumentView ShowView()
    {
        lock (_sync)
        {
            if (FileName?.PhysicFileName is null)
            {
                return null;
            }

            var view = DocumentViewManager.Current?.ShowDocumentView(this);
            if (view != null)
            {
                _content?.OnShowView();
            }

            return view;
        }
    }

    /// <summary>
    /// Refresh view
    /// </summary>
    public override void RefreshView(bool focus)
    {
        if (FileName?.PhysicFileName is null)
        {
            return;
        }

        DocumentViewManager.Current?.GetDocumentView(this)?.ActivateView(focus);
    }

    /// <inheritdoc/>
    public override void QueueUpdateView()
    {
        _updateViewAction.DoQueuedAction();
    }

    /// <summary>
    /// Closes the document view associated with this entry.
    /// </summary>
    internal void CloseView()
    {
        if (FileName?.PhysicFileName is null)
        {
            return;
        }

        _content?.OnCloseView();
        DocumentViewManager.Current?.CloseDocument(this);
    }

    /// <summary>
    /// Show as property edit
    /// </summary>
    /// <returns>Returns whether show succeeded</returns>
    public override bool ShowProperty()
    {
        lock (_sync)
        {
            if (_state != DocumentState.Loaded)
            {
                return false;
            }

            MarkVisit();

            if (Format.CanShowAsProperty)
            {
                // Avoid repeatedly refreshing view, only set if not self.
                if (!EditorUtility.Inspector.IsObjectSelected(this))
                {
                    EditorUtility.Inspector.InspectObject(this);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion

    #region Dirty Flag / Undo

    private bool _isDirty;

    /// <summary>
    /// Is dirty
    /// </summary>
    public override bool IsDirty => _isDirty;

    /// <summary>
    /// Mark as dirty
    /// </summary>
    public override void MarkDirty(object marker)
    {
        if (_state != DocumentState.Loaded)
        {
            return;
        }

        MarkVisit();
        _content?.OnDirty();
        RaiseDirtyMarked(marker);

        if (!IsDirty)
        {
            _isDirty = true;
            RaiseDirtyChanged();
        }
    }

    /// <summary>
    /// Clear dirty flag
    /// </summary>
    internal void ClearDirty()
    {
        MarkVisit();
        if (IsDirty)
        {
            _isDirty = false;
            RaiseDirtyChanged();
        }
    }

    /// <summary>
    /// Mark visit
    /// </summary>
    public override void MarkVisit()
    {
        if (_state != DocumentState.Loaded)
        {
            return;
        }

        _lastVisitTime = DateTime.Now;
    }

    /// <inheritdoc/>
    public override void MarkUsage(DocumentUsageToken token)
    {
        if (token is null || token.IsDisposed)
        {
            return;
        }

        (_usageTokens ??= []).Add(token);
    }

    /// <inheritdoc/>
    public override void UnmarkUsage(DocumentUsageToken token)
    {
        if (token is null)
        {
            return;
        }

        _usageTokens?.Remove(token);
    }

    /// <inheritdoc/>
    public override void MarkDelete()
    {
        _usageTokens?.Clear();
        _usageTokens = null;
    }

    #endregion

    #region IViewListener

    /// <inheritdoc/>
    void IViewListener.NotifyViewEnter(int viewId)
    {
        MarkVisit();
    }

    /// <inheritdoc/>
    void IViewListener.NotifyViewExit(int viewId)
    {
    }

    /// <inheritdoc/>
    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        MarkDirty(null);
    }

    #endregion

    /// <summary>
    /// Raises the renamed event for this document entry.
    /// </summary>
    internal void InternalRaiseRenamed()
    {
        RaiseRenamed();
    }

    /// <summary>
    /// Clears all usage tokens from this document.
    /// </summary>
    internal void UnmarkAllUsage()
    {
        _usageTokens?.Clear();
    }

    private Document EnsureDocumentContent()
    {
        lock (_sync)
        {
            if (_content != null)
            {
                return _content;
            }

            _content = _format?.DocumentType?.CreateInstanceOf() as Document;
            _content?._entry = this;

            return _content;
        }
    }

    public override string ToString()
    {
        return _content?.ToString() ?? base.ToString();
    }

    private class DelaySaveDocumentAction(DocumentEntryBK value) : DelayedAction<DocumentEntryBK>(value)
    {
        public override void DoAction()
        {
            //Logs.LogDebug($"Document delayed save:{Value.FileName}");
            Value.Save();
        }
    }
}