using Suity.Views;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Document content
/// </summary>
public abstract class Document : IViewListener, ICommit
{
    internal DocumentEntry _entry;

    // Lock used to protect data integrity
    protected readonly object _sync = new();

    /// <summary>
    /// Gets the icon for this document.
    /// </summary>
    public virtual Image Icon { get; }

    /// <summary>
    /// Always keep opened state
    /// </summary>
    public virtual bool KeepOpened { get; }

    /// <summary>
    /// Gets the document entry that owns this document.
    /// </summary>
    public DocumentEntry Entry => _entry;

    /// <summary>
    /// Gets the storage location (file path) of this document.
    /// </summary>
    public StorageLocation FileName => _entry?.FileName;

    /// <summary>
    /// Gets the format of this document.
    /// </summary>
    public DocumentFormat Format => _entry?.Format;

    /// <summary>
    /// Gets the default icon based on the document type.
    /// </summary>
    public virtual Image DefaultIcon => this.GetType().ToDisplayIcon();

    /// <summary>
    /// Forces the document to be saved immediately.
    /// </summary>
    /// <returns>True if save was successful; otherwise, false.</returns>
    public bool ForceSave() => _entry?.ForceSave() == true;

    /// <summary>
    /// Saves the document.
    /// </summary>
    /// <returns>True if save was successful; otherwise, false.</returns>
    public bool Save() => _entry?.Save() == true;

    /// <summary>
    /// Marks the document as dirty and saves it after a delay.
    /// </summary>
    public void SaveDelayed() => _entry?.SaveDelayed();

    /// <summary>
    /// Exports the document to a specified file path.
    /// </summary>
    /// <param name="fileName">The target file path.</param>
    /// <returns>True if export was successful; otherwise, false.</returns>
    public bool Export(string fileName) => _entry?.Export(fileName) == true;

    /// <summary>
    /// Marks the document as dirty with a specific marker.
    /// </summary>
    /// <param name="marker">The object that caused the dirty state.</param>
    public void MarkDirty(object marker) => _entry?.MarkDirty(marker);

    /// <summary>
    /// Marks the document as dirty and schedules a delayed save.
    /// </summary>
    /// <param name="marker">The object that caused the dirty state.</param>
    public void MarkDirtyAndSaveDelayed(object marker)
    {
        _entry?.MarkDirty(marker);
        _entry?.SaveDelayed();
    }

    /// <summary>
    /// Gets a value indicating whether the document has unsaved changes.
    /// </summary>
    public bool IsDirty => _entry?.IsDirty == true;

    /// <summary>
    /// Gets the current document view.
    /// </summary>
    public IDocumentView View => _entry?.View;

    /// <summary>
    /// Shows the document view, creating one if needed.
    /// </summary>
    /// <returns>The document view.</returns>
    public IDocumentView ShowView() => _entry?.ShowView();

    /// <summary>
    /// Shows the document in the property editor.
    /// </summary>
    /// <returns>True if shown successfully; otherwise, false.</returns>
    public bool ShowProperty() => _entry?.ShowProperty() ?? false;

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <returns>True if creation was successful.</returns>
    protected internal virtual bool NewDocument() => true;

    /// <summary>
    /// Loads the document from storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <param name="loaderObject">The loader object.</param>
    /// <returns>True if load was successful.</returns>
    protected internal virtual bool LoadDocument(IStorageItem op, object loaderObject, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal) => false;

    /// <summary>
    /// Saves the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if save was successful.</returns>
    protected internal virtual bool SaveDocument(IStorageItem op) => false;

    /// <summary>
    /// Exports the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if export was successful.</returns>
    protected internal virtual bool ExportDocument(IStorageItem op)
    {
        var fileName = FileName;

        if (string.IsNullOrEmpty(fileName?.PhysicFileName))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(op.FileName))
        {
            File.Copy(fileName.PhysicFileName, op.FileName, true);
        }
        else
        {
            using var stream = File.OpenRead(fileName.PhysicFileName);
            stream.CopyTo(op.GetOutputStream(), 1024 * 16);
        }

        return true;
    }

    /// <summary>
    /// Called when the document is created.
    /// </summary>
    protected internal virtual void OnCreated()
    { }

    /// <summary>
    /// Called when the document content is reset.
    /// </summary>
    protected internal virtual void OnReset()
    { }

    /// <summary>
    /// Called when the document is loaded.
    /// </summary>
    protected internal virtual void OnLoaded(DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    { }

    /// <summary>
    /// Called when the document is saved.
    /// </summary>
    protected internal virtual void OnSaved()
    { }

    /// <summary>
    /// Called when the document is destroyed.
    /// </summary>
    protected internal virtual void OnDestroy()
    { }

    /// <summary>
    /// Called when the document becomes dirty.
    /// </summary>
    protected internal virtual void OnDirty()
    { }

    /// <summary>
    /// Called when the view is shown.
    /// </summary>
    protected internal virtual void OnShowView()
    { }

    /// <summary>
    /// Called when the view is closed.
    /// </summary>
    protected internal virtual void OnCloseView()
    { }

    #region IViewListener

    void IViewListener.NotifyViewEnter(int viewId)
    {
        _entry?.MarkVisit();
        OnViewEnter(viewId);
    }

    void IViewListener.NotifyViewExit(int viewId)
    {
        OnViewExit(viewId);
    }

    void IViewEditNotify.NotifyViewEdited(object obj, string propertyName)
    {
        _entry?.MarkDirty(this);
        OnViewEdited(obj, propertyName);
    }


    /// <summary>
    /// Called when the view enters the document.
    /// </summary>
    /// <param name="viewId">The view ID.</param>
    protected virtual void OnViewEnter(int viewId) { }

    /// <summary>
    /// Called when the view exits the document.
    /// </summary>
    /// <param name="viewId">The view ID.</param>
    protected virtual void OnViewExit(int viewId) { }

    /// <summary>
    /// Called when the view edits are applied to the document.
    /// </summary>
    /// <param name="obj">The edited object.</param>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnViewEdited(object obj, string propertyName) { }

    #endregion

    #region ICommit

    Task ICommit.Commit(object marker)
    {
        _entry?.MarkDirty(marker);
        _entry?.SaveDelayed();

        return Task.CompletedTask;
    }

    #endregion

    protected internal void RaiseIconChanged() => _entry?.RaiseIconChanged();

    public override string ToString()
    {
        var fileName = FileName;

        if (fileName != null && !string.IsNullOrEmpty(fileName.FullPath))
        {
            return Path.GetFileNameWithoutExtension(fileName.FullPath);
        }
        else
        {
            return base.ToString();
        }
    }
}