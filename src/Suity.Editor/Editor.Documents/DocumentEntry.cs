using Suity.Views.Named;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Represents the current state of a document.
/// </summary>
public enum DocumentState
{
    /// <summary>
    /// Document state has not been set.
    /// </summary>
    None,
    /// <summary>
    /// Document is currently loading.
    /// </summary>
    Loading,
    /// <summary>
    /// Document has been loaded successfully.
    /// </summary>
    Loaded,
    /// <summary>
    /// Document failed to load.
    /// </summary>
    Failed,
    /// <summary>
    /// Document resources have been released.
    /// </summary>
    Released,
}

/// <summary>
/// Token representing a usage reference to a document, implementing INamed and IDisposable.
/// </summary>
public sealed class DocumentUsageToken : INamed, IDisposable
{
    /// <summary>
    /// Gets the name of this usage token.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this token has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the DocumentUsageToken class.
    /// </summary>
    public DocumentUsageToken()
    {
    }

    /// <summary>
    /// Initializes a new instance of the DocumentUsageToken class with a name.
    /// </summary>
    /// <param name="name">The name of the token.</param>
    public DocumentUsageToken(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Disposes this token and marks it as disposed.
    /// </summary>
    public void Dispose()
    {
        IsDisposed = true;
    }

    /// <summary>
    /// Returns the name of this token or the type name if no name was set.
    /// </summary>
    /// <returns>The token name.</returns>
    public override string ToString()
    {
        return Name ?? nameof(DocumentUsageToken);
    }
}

/// <summary>
/// Document entry representing a document in the document manager.
/// </summary>
public abstract class DocumentEntry : ICommit
{
    /// <summary>
    /// Event raised when the document is about to be saved.
    /// </summary>
    public event EventHandler Saving;

    /// <summary>
    /// Event raised after the document has been saved.
    /// </summary>
    public event EventHandler Saved;

    /// <summary>
    /// Event raised when the document is renamed.
    /// </summary>
    public event EventHandler Renamed;

    /// <summary>
    /// Event raised when the document icon changes.
    /// </summary>
    public event EventHandler IconChanged;

    /// <summary>
    /// Event raised when the document is marked as dirty.
    /// </summary>
    public event EventHandler<DirtyEventArgs> DirtyMarked;

    /// <summary>
    /// Event raised when the dirty flag is updated.
    /// </summary>
    public event EventHandler DirtyChanged;

    /// <summary>
    /// Gets a value indicating whether the document has been released.
    /// </summary>
    public abstract bool IsReleased { get; }

    /// <summary>
    /// Gets the creation time of the document.
    /// </summary>
    public abstract DateTime CreateTime { get; }

    /// <summary>
    /// Gets the last visit time of the document.
    /// </summary>
    public abstract DateTime LastVisitTime { get; }

    /// <summary>
    /// Gets the content of the document.
    /// </summary>
    public abstract Document Content { get; }

    /// <summary>
    /// Gets the storage location (file path) of the document.
    /// </summary>
    public abstract StorageLocation FileName { get; }

    /// <summary>
    /// Gets the current state of the document.
    /// </summary>
    public abstract DocumentState State { get; }

    /// <summary>
    /// Gets the format of the document.
    /// </summary>
    public abstract DocumentFormat Format { get; }

    /// <summary>
    /// Gets the icon for the document.
    /// </summary>
    public abstract Image Icon { get; }

    /// <summary>
    /// Saves the document.
    /// </summary>
    /// <returns>True if saved successfully.</returns>
    public abstract bool Save();

    /// <summary>
    /// Forces the document to be saved immediately.
    /// </summary>
    /// <returns>True if saved successfully.</returns>
    public abstract bool ForceSave();

    /// <summary>
    /// Saves the document after a delay.
    /// </summary>
    public abstract void SaveDelayed();

    /// <summary>
    /// Exports the document to a specified file name.
    /// </summary>
    /// <param name="fileName">The target file name.</param>
    /// <returns>True if exported successfully.</returns>
    public abstract bool Export(string fileName);

    /// <summary>
    /// Gets the view associated with the document.
    /// </summary>
    public abstract IDocumentView View { get; }

    /// <summary>
    /// Shows the document view, creating one if needed.
    /// </summary>
    /// <returns>The document view.</returns>
    public abstract IDocumentView ShowView();

    /// <summary>
    /// Refreshes the view with optional focus.
    /// </summary>
    /// <param name="focus">Whether to focus the view.</param>
    public abstract void RefreshView(bool focus);

    /// <summary>
    /// Queues an update for the view.
    /// </summary>
    public abstract void QueueUpdateView();

    /// <summary>
    /// Shows the document in the property editor.
    /// </summary>
    /// <returns>True if shown successfully.</returns>
    public abstract bool ShowProperty();

    /// <summary>
    /// Gets a value indicating whether the document has unsaved changes.
    /// </summary>
    public abstract bool IsDirty { get; }

    /// <summary>
    /// Marks the document as dirty with an optional marker.
    /// </summary>
    /// <param name="marker">The object that caused the dirty state.</param>
    public abstract void MarkDirty(object marker = null);

    /// <summary>
    /// Marks the document as visited.
    /// </summary>
    public abstract void MarkVisit();

    /// <summary>
    /// Marks the document for deletion.
    /// </summary>
    public abstract void MarkDelete();

    /// <summary>
    /// Marks the document as used with a usage token.
    /// </summary>
    /// <param name="token">The usage token.</param>
    public abstract void MarkUsage(DocumentUsageToken token);

    /// <summary>
    /// Unmarks the document usage.
    /// </summary>
    /// <param name="token">The usage token.</param>
    public abstract void UnmarkUsage(DocumentUsageToken token);


    internal void RaiseIconChanged() => IconChanged?.Invoke(this, EventArgs.Empty);

    internal void RaiseSaving() => Saving?.Invoke(this, EventArgs.Empty);

    internal void RaiseSaved() => Saved?.Invoke(this, EventArgs.Empty);

    internal void RaiseRenamed() => Renamed?.Invoke(this, EventArgs.Empty);

    internal void RaiseDirtyMarked(object marker = null) => DirtyMarked?.Invoke(this, new DirtyEventArgs(marker));

    internal void RaiseDirtyChanged() => DirtyChanged?.Invoke(this, EventArgs.Empty);

    #region ICommit

    Task ICommit.Commit(object marker)
    {
        MarkDirty(marker);
        SaveDelayed();

        return Task.CompletedTask;
    }

    #endregion
}