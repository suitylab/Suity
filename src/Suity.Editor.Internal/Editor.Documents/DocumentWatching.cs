using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.IO;

namespace Suity.Editor.WinformGui;

/// <summary>
/// Watches a document file for external changes and notifies the document manager when changes occur.
/// </summary>
public class DocumentWatching : IDisposable
{
    private readonly DocumentEntry _entry;
    private readonly IDocumentView _view;

    /// <summary>
    /// Gets the document being watched.
    /// </summary>
    public DocumentEntry Document => _entry;
    /// <summary>
    /// Gets the view associated with the document.
    /// </summary>
    public IDocumentView View => _view;

    EditorFileSystemWatcher _watcher;

    readonly RaiseChangeAction _changeAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentWatching"/> class.
    /// </summary>
    /// <param name="document">The document to watch.</param>
    /// <param name="view">The view associated with the document.</param>
    public DocumentWatching(DocumentEntry document, IDocumentView view)
    {
        _entry = document ?? throw new ArgumentNullException(nameof(document));
        _view = view ?? throw new ArgumentNullException(nameof(view));

        _changeAction = new RaiseChangeAction(Document);

        CreateWatcher();

        Document.Saving += Document_Saving;
        Document.Saved += Document_Saved;
        Document.Renamed += Document_Renamed;
    }


    ~DocumentWatching()
    {
        Dispose();
    }

    private void Document_Saving(object sender, EventArgs e)
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }
    private void Document_Saved(object sender, EventArgs e)
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = true;
        }
    }
    private void Document_Renamed(object sender, EventArgs e)
    {
        DisposeWatcher();
        CreateWatcher();
    }


    private void CreateWatcher()
    {
        lock (this)
        {
            if (_watcher != null)
            {
                return;
            }
            if (Document.FileName.PhysicFileName == null)
            {
                return;
            }

            try
            {
                string dir = Path.GetDirectoryName(Document.FileName.PhysicFileName);
                string fileName = Path.GetFileName(Document.FileName.PhysicFileName);

                _watcher = new EditorFileSystemWatcher(dir, null, false)
                {
                    Filter = fileName
                };

                _watcher.Created += _watcher_Created;
                _watcher.Deleted += _watcher_Deleted;
                _watcher.Changed += _watcher_Changed;
                _watcher.Renamed += _watcher_Renamed;

                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                    _watcher = null;
                }
            }
        }
    }

    private void DisposeWatcher()
    {
        lock (this)
        {
            var watcher = _watcher;
            _watcher = null;

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                //QueuedAction.Do(() =>
                //{
                    watcher.Dispose();
                //});
            }
        }
    }

    void _watcher_Created(string fullPath)
    {
    }
    void _watcher_Deleted(string fullPath)
    {
        //MarkDirty();
    }
    void _watcher_Changed(string fullPath)
    {
        EditorUtility.AddDelayedAction(_changeAction);
    }
    void _watcher_Renamed(string fullPath, string oldFullPath)
    {
        //MarkDirty();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Document.Saving -= Document_Saving;
        Document.Saved -= Document_Saved;

        DisposeWatcher();
    }

    /// <summary>
    /// Delayed action that raises a document change external event.
    /// </summary>
    class RaiseChangeAction : DelayedAction<DocumentEntry>
    {
        public RaiseChangeAction(DocumentEntry value) : base(value)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            if (!Value.IsReleased)
            {
                DocumentManager.Instance.RaiseDocumentChangeExternal(Value);
            }
        }
    }
}
