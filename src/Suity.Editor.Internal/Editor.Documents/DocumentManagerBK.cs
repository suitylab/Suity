using Suity.Collections;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Suity.Editor.Documents;

/// <summary>
/// Internal backend implementation of the document manager, handling document format registration, document lifecycle, and caching.
/// </summary>
internal sealed class DocumentManagerBK : DocumentManager
{
    /// <summary>
    /// Gets or sets whether document open/close messages should be logged.
    /// </summary>
    internal static bool DebugMessageDocument = true;

    /// <summary>
    /// Timer interval in seconds for document cache eviction.
    /// </summary>
    public const int TimerDurationSec = 1;
    /// <summary>
    /// Duration to cache hidden documents before eviction.
    /// </summary>
    public static readonly TimeSpan HiddenDocumentCacheDuration = TimeSpan.FromMinutes(3);
    /// <summary>
    /// Gets the singleton instance of the document manager.
    /// </summary>
    public new static readonly DocumentManagerBK Instance = new();

    private DocumentFormat _textBaseFormat;

    private readonly Dictionary<string, DocumentFormat> _formats = [];
    private readonly Dictionary<string, IDocumentFormatResolver> _resolvers = [];
    private readonly UniqueMultiDictionary<string, DocumentFormat> _documentFormatByExt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DocumentEntryBK> _namedDocuments = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<DocumentEntryBK> _allDocuments = [];

    private readonly object _sync = new();
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable

    private Timer _timer;
    private bool _init;

    private DocumentManagerBK()
    {
    }

    /// <summary>
    /// Initializes the document manager and binds it to the static instance.
    /// </summary>
    public void Initialize()
    {
        DocumentManager.Instance = this;

        EditorRexes.EditorBeforeAwake.AddActionListener(PostInitialize);
        var storage = StorageManager.Current;
    }

    private void PostInitialize()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        EditorServices.SystemLog.AddLog("DocumentManager Initializing...");
        EditorServices.SystemLog.PushIndent();

        _textBaseFormat = UnknownDocumentFormat.Instance;

        _timer = new Timer(OnTimer);
        _timer.Change(TimerDurationSec * 1000, TimerDurationSec * 1000);

        FileUnwatchedAction.Pause += FileUnwatchedAction_Pause;
        FileUnwatchedAction.Resume += FileUnwatchedAction_Resume;

        ScanDocumentFormat();

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("DocumentManager Initialized.");
    }

    #region Scan

    private void ScanDocumentFormat()
    {
        foreach (Type type in typeof(IDocumentFormatResolver).GetDerivedTypes())
        {
            try
            {
                IDocumentFormatResolver resolver = (IDocumentFormatResolver)type.CreateInstanceOf();

                string ext = resolver.Extension;

                if (string.IsNullOrEmpty(ext))
                {
                    Logs.LogError($"IDocumentFormatResolver.Extension is empty : {type.Name}");
                    continue;
                }

                if (_resolvers.ContainsKey(ext))
                {
                    Logs.LogError($"IDocumentFormatResolver extension {ext} already exists : {type.Name}");
                    continue;
                }

                _resolvers.Add(ext, resolver);

                // Due to DocumentFormat introducing Alias mechanism, formats may have duplicates
                // Here we need to deduplicate
                foreach (var format in resolver.Formats.Distinct())
                {
                    RegisterDocumentFormat(format);
                }
            }
            catch (Exception err)
            {
                err.LogError($"Create IDocumentFormatResolver failed : {type.Name}");
            }
        }

        foreach (Type docType in typeof(Document).GetDerivedTypes())
        {
            var docFormatAttr = docType.GetAttributeCached<DocumentFormatAttribute>();
            if (docFormatAttr is null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(docFormatAttr.FormatName))
            {
                continue;
            }

            if (_formats.ContainsKey(docFormatAttr.FormatName))
            {
                continue;
            }

            var docFormat = new AttributedDocumentFormat(docType, docFormatAttr);

            RegisterDocumentFormat(docFormat, docType.HasAttributeCached<BaseTextDocumentFormatAttribute>());
        }

        foreach (Type formatType in typeof(DocumentFormat).GetDerivedTypes())
        {
            DocumentFormat format = (DocumentFormat)formatType.CreateInstanceOf();
            RegisterDocumentFormat(format, formatType.HasAttributeCached<BaseTextDocumentFormatAttribute>());
        }
    }

    #endregion

    #region Factory

    /// <summary>
    /// Get document view format provider
    /// </summary>
    /// <returns>Returns document view format provider</returns>
    public override DocumentFormat GetDocumentFormat(string formatName)
    {
        formatName ??= string.Empty;

        lock (_sync)
        {
            return _formats.GetValueSafe(formatName) ?? 
                _textBaseFormat ??
                UnknownDocumentFormat.Instance;
        }
    }

    /// <inheritdoc/>
    public override DocumentFormat GetDocumentFormatByExtension(string ext)
    {
        if (string.IsNullOrEmpty(ext))
        {
            return null;
        }

        ext = ext.ToLowerInvariant().TrimStart('.');

        lock (_sync)
        {
            return _documentFormatByExt[ext].FirstOrDefault() ??
                _textBaseFormat ??
                UnknownDocumentFormat.Instance;
        }
    }

    /// <inheritdoc/>
    public override DocumentFormat GetDocumentFormatByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        // Do not start Resolver here, it will cause heavy consumption

        string ext = Path.GetExtension(path).ToLowerInvariant().TrimStart('.');

        //TODO: Need to support parsing sasset
        return GetDocumentFormatByExtension(ext);
    }

    /// <inheritdoc/>
    public override IDocumentResolveResult ResolveInFileFormat(string ext, Stream stream)
    {
        if (string.IsNullOrWhiteSpace(ext))
        {
            return null;
        }

        ext = ext.ToLowerInvariant().TrimStart('.');

        if (stream is null)
        {
            return null;
        }

        var resolver = _resolvers.GetValueSafe(ext);
        if (resolver is null)
        {
            var format = GetDocumentFormatByExtension(ext);
            if (format != null)
            {
                return new DocumentResolveResult { Format = format };
            }
            else
            {
                return null;
            }
        }

        return resolver.ResolveDocumentFormat(stream);
    }

    /// <inheritdoc/>
    public override IDocumentResolveResult ResolveInFileFormat(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string ext = Path.GetExtension(path).TrimStart('.');

        var resolver = _resolvers.GetValueSafe(ext);
        if (resolver is null)
        {
            var format = GetDocumentFormatByExtension(ext);
            if (format != null)
            {
                return new DocumentResolveResult { Format = format };
            }
            else
            {
                return null;
            }
        }

        var location = StorageLocation.Create(path);
        var op = location.GetStorageItem();
        Stream stream;

        try
        {
            stream = op.GetInputStream();
        }
        catch (Exception)
        {
            return null;
        }

        if (stream is null)
        {
            return null;
        }

        return resolver.ResolveDocumentFormat(stream);
    }

    /// <inheritdoc/>
    public override IEnumerable<DocumentFormat> GetDocumentFormats(string ext)
    {
        return _documentFormatByExt[ext];
    }

    /// <inheritdoc/>
    public override IEnumerable<DocumentFormat> GetDocumentFormats()
    {
        return _formats.Values.Distinct();
    }

    /// <summary>
    /// Registers a document format with the manager.
    /// </summary>
    /// <param name="format">The document format to register.</param>
    /// <param name="isTextBase">Whether this format is text-based.</param>
    private void RegisterDocumentFormat(DocumentFormat format, bool isTextBase = false)
    {
        if (format is null)
        {
            throw new ArgumentNullException();
        }

        string name = format.FormatName;
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException();
        }

        lock (_sync)
        {
            if (_formats.ContainsKey(name))
            {
                throw new InvalidOperationException("Document format exist : " + name);
            }

            _formats[name] = format;

            foreach (var alias in format.FormatNames)
            {
                if (string.IsNullOrEmpty(alias))
                {
                    continue;
                }

                if (_formats.ContainsKey(alias))
                {
                    Logs.LogError("Document format alias exist : " + alias);
                    continue;
                }

                _formats[alias] = format;
            }

            string[] exts = format.GetAdditionalExtensions();
            if (exts != null)
            {
                EditorServices.SystemLog.AddLog($"Register document format : {format.FormatName} to {format.GetType().Name} ext : {string.Join(",", exts)}");

                foreach (string ext in exts)
                {
                    if (string.IsNullOrEmpty(ext))
                    {
                        continue;
                    }

                    _documentFormatByExt.Add(ext, format);
                }
            }

            if (isTextBase && _textBaseFormat is null)
            {
                _textBaseFormat = format;
            }
        }
    }

    #endregion

    #region Get

    /// <inheritdoc/>
    public override IEnumerable<DocumentEntry> AllOpenedDocuments => _allDocuments.Select(o => o);

    /// <inheritdoc/>
    public override DocumentEntry OpenDocument(StorageLocation path, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        return OpenDocument(path.FullPath);
    }

    /// <inheritdoc/>
    public override DocumentEntry OpenDocument(string path, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        lock (_sync)
        {
            string pathCode = path.GetPathId();

            var documentEntry = _namedDocuments.GetValueSafe(pathCode);
            if (documentEntry != null)
            {
                documentEntry.MarkVisit();

                return documentEntry;
            }
        }

        var result = ResolveInFileFormat(path);
        if (result != null && result.Format?.DocumentType != null)
        {
            var entry = OpenDocument(path, result.Format, result.LoaderObject, intent);
            result.Dispose();

            return entry;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Opens a document with the specified path and format.
    /// </summary>
    /// <param name="path">The document file path.</param>
    /// <param name="format">The document format.</param>
    /// <param name="loaderObject">Optional loader object for custom loading.</param>
    /// <returns>The opened document entry, or null if opening failed.</returns>
    private DocumentEntry OpenDocument(string path, DocumentFormat format, object loaderObject = null, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        if (format is null)
        {
            return null;
        }

        ValidateDocumentFormat(format);

        DocumentEntryBK documentEntry = null;

        lock (_sync)
        {
            do
            {
                string pathCode = path.GetPathId();

                documentEntry = _namedDocuments.GetValueSafe(pathCode);
                if (documentEntry != null)
                {
                    documentEntry.MarkVisit();

                    return documentEntry;
                }

                StorageLocation fileName = StorageLocation.Create(path);

                if (fileName.PhysicFileName != null)
                {
                    // Exclude folders
                    if (Directory.Exists(path))
                    {
                        return null;
                    }

                    if (!File.Exists(path))
                    {
                        return null;
                    }
                }

                documentEntry = new DocumentEntryBK(format, fileName);
                documentEntry.SetFormat(format);
                documentEntry.SetFileName(fileName);

                _allDocuments.Add(documentEntry);
                _namedDocuments.Add(pathCode, documentEntry);
            }
            while (false);
        }

        // Prevent lock wait, put outside lock to start loading process
        if (documentEntry != null)
        {
            // Order sensitive, need to add to collection first then load document
            // Produce double lock
            documentEntry.InternalCreate();
            documentEntry.InternalLoad(loaderObject, intent);

            if (DebugMessageDocument)
            {
                EditorServices.SystemLog.AddLog("DocumentManager : Open document : " + documentEntry.FileName);
            }

            RaiseDocumentLoaded(documentEntry);
        }

        return documentEntry;
    }

    /// <inheritdoc/>
    public override DocumentEntry ReloadDocument(string path)
    {
        if (GetDocument(path) is DocumentEntryBK documentEntry)
        {
            documentEntry.InternalReload();

            if (DebugMessageDocument)
            {
                EditorServices.SystemLog.AddLog("DocumentManager : Reload document : " + documentEntry.FileName);
            }

            RaiseDocumentLoaded(documentEntry);

            return documentEntry;
        }
        else
        {
            return OpenDocument(path);
        }
    }

    /// <inheritdoc/>
    public override DocumentEntry NewDocument(string path, DocumentFormat format)
    {
        if (format is null)
        {
            throw new ArgumentNullException(nameof(format));
        }

        ValidateDocumentFormat(format);

        DocumentEntryBK documentEntry = null;

        lock (_sync)
        {
            string pathCode = path.GetPathId();

            documentEntry = _namedDocuments.GetValueSafe(pathCode);
            if (documentEntry != null)
            {
                return null;
            }

            var fileName = StorageLocation.Create(path);

            if (fileName.PhysicFileName != null)
            {
                // Exclude folders
                if (Directory.Exists(path))
                {
                    return null;
                }

                if (File.Exists(path))
                {
                    return null;
                }
            }

            documentEntry = new DocumentEntryBK(format, fileName);

            _allDocuments.Add(documentEntry);
            _namedDocuments.Add(pathCode, documentEntry);
        }

        if (documentEntry != null)
        {
            // Order sensitive, need to add to collection first then load document
            // Produce double lock
            documentEntry.InternalCreate();
            documentEntry.InternalNew();

            if (DebugMessageDocument)
            {
                EditorServices.SystemLog.AddLog("DocumentManager : New document : " + documentEntry.FileName);
            }

            // After testing, new documents should be saved immediately.
            documentEntry.MarkDirty(this);
            documentEntry.Save();

            RaiseDocumentNew(documentEntry);
            RaiseDocumentLoaded(documentEntry);
        }

        return documentEntry;
    }

    /// <inheritdoc/>
    public override DocumentEntry GetDocument(StorageLocation path)
        => GetDocument(path.FullPath);

    /// <inheritdoc/>
    public override DocumentEntry GetDocument(string path)
    {
        string pathCode = path.GetPathId();

        lock (_sync)
        {
            DocumentEntryBK document = _namedDocuments.GetValueSafe(pathCode);
            if (document != null)
            {
                document.MarkVisit();

                return document;
            }
            else
            {
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public override DocumentEntry CloneDocument(string path, string pathClone)
    {
        if (path == pathClone)
        {
            throw new ArgumentException();
        }

        if (OpenDocument(path) is not DocumentEntryBK doc)
        {
            return null;
        }

        using var memory = new MemoryStorageItem();
        doc.InternalExport(memory);

        var format = doc.Format;

        if (NewDocument(pathClone, format) is not DocumentEntryBK docClone)
        {
            return null;
        }

        memory.Stream.Position = 0;

        docClone.InternalReload(memory);

        RaiseDocumentLoaded(docClone);

        docClone.ForceSave();

        return docClone;
    }

    /// <inheritdoc/>
    public override bool CloseDocument(string path)
    {
        if (GetDocument(path) is DocumentEntryBK document)
        {
            return CloseDocument(document);
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool CloseDocument(DocumentEntry entry)
    {
        if (entry is not DocumentEntryBK documentEntry)
        {
            return false;
        }

        // Check file occupation
        bool fileExist = File.Exists(entry.FileName.FullPath);
        if (fileExist && documentEntry.CheckIsInUsage())
        {
            return false;
        }

        // Prevent pending tasks
        EditorUtility.FlushDelayedActions();

        // Keep opened documents && when file exists, ignore closing document
        if (documentEntry.Content?.KeepOpened == true && documentEntry.FileName.Exists())
        {
            return false;
        }

        lock (_sync)
        {
            if (!_allDocuments.Contains(documentEntry))
            {
                return false;
            }

            if (documentEntry.IsReleased)
            {
                return false;
            }

            string pathCode = documentEntry.FileName.FullPath.GetPathId();
            _namedDocuments.Remove(pathCode);
            _allDocuments.Remove(documentEntry);
            // Produce double lock
        }

        documentEntry.Release();
        if (DebugMessageDocument)
        {
            EditorServices.SystemLog.AddLog("DocumentManager : Close document : " + documentEntry.FileName);
        }

        RaiseDocumentClosed(documentEntry);

        return true;
    }

    /// <inheritdoc/>
    public override void CloseAllDocuments()
    {
        foreach (var document in _allDocuments.ToArray())
        {
            // Force release occupation
            document.UnmarkAllUsage();

            CloseDocument(document);
        }
    }

    /// <inheritdoc/>
    public override IDocumentView ShowDocument(string path)
    {
        var result = ResolveInFileFormat(path);
        if (result != null)
        {
            var view = ShowDocument(path, result.Format, result.LoaderObject);
            result.Dispose();

            return view;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override IDocumentView ShowDocument(string path, DocumentFormat format)
    {
        return ShowDocument(path, format, null);
    }

    /// <summary>
    /// Shows a document view for the specified path and format.
    /// </summary>
    /// <param name="path">The document file path.</param>
    /// <param name="format">The document format.</param>
    /// <param name="loaderObject">Optional loader object for custom loading.</param>
    /// <returns>The document view, or null if showing failed.</returns>
    private IDocumentView ShowDocument(string path, DocumentFormat format, object loaderObject)
    {
        if (format is null)
        {
            return null;
        }

        if (!format.CanShowView)
        {
            return null;
        }

        ValidateDocumentFormat(format);

        if (OpenDocument(path, format, loaderObject) is DocumentEntryBK document)
        {
            return document.ShowView();
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override bool ShowProperty(string path)
    {
        var result = ResolveInFileFormat(path);
        if (result != null)
        {
            bool show = ShowProperty(path, result.Format, result.LoaderObject);
            result.Dispose();

            return show;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Shows the document property inspector for the specified path.
    /// </summary>
    /// <param name="path">The document file path.</param>
    /// <param name="format">The document format.</param>
    /// <param name="loaderObject">Optional loader object for custom loading.</param>
    /// <returns>True if the property inspector was shown; otherwise, false.</returns>
    private bool ShowProperty(string path, DocumentFormat format, object loaderObject = null)
    {
        if (format is null)
        {
            return false;
        }

        if (!format.CanShowAsProperty)
        {
            return false;
        }

        ValidateDocumentFormat(format);

        if (OpenDocument(path, format, loaderObject) is DocumentEntryBK document)
        {
            return document.ShowProperty();
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Save/Rename

    /// <inheritdoc/>
    public override void SaveAllDocuments()
    {
        DocumentEntryBK[] docs = null;
        lock (_sync)
        {
            docs = [.. _namedDocuments.Values];
        }

        foreach (DocumentEntryBK document in docs)
        {
            document.Save();
        }

        RaiseAllDocumentsSaved();
    }

    /// <inheritdoc/>
    public override void SaveUnopenedDocuments()
    {
        DocumentEntryBK[] docs = null;
        lock (_sync)
        {
            docs = [.. _namedDocuments.Values];
        }

        foreach (DocumentEntryBK document in docs)
        {
            if (document.View != null)
            {
                document.Save();
            }
        }
    }

    /// <inheritdoc/>
    public override void CleanUp()
    {
        DocumentEntryBK[] docs = null;
        lock (_sync)
        {
            docs = [.. _namedDocuments.Values.Where(o => o.View is null && !o.CheckIsInUsage())];
        }

        foreach (var doc in docs)
        {
            CloseDocument(doc);
        }
    }

    /// <summary>
    /// Renames a document entry from the old path to the new path.
    /// </summary>
    /// <param name="path">The current document path.</param>
    /// <param name="newPath">The new document path.</param>
    /// <returns>True if the document was successfully renamed; otherwise, false.</returns>
    internal bool RenameDocument(string path, string newPath)
    {
        string pathCode = path.GetPathId();
        string newPathCode = newPath.GetPathId();

        lock (_sync)
        {
            if (_namedDocuments.ContainsKey(newPathCode))
            {
                return false;
            }

            DocumentEntryBK document = _namedDocuments.GetValueSafe(pathCode);
            if (document != null)
            {
                if (document.FileName.PhysicFileName is null)
                {
                    return false;
                }

                document.MarkVisit();
                document.SetFileName(StorageLocation.Create(newPath));
                _namedDocuments.Remove(pathCode);
                _namedDocuments.Add(newPathCode, document);
                document.InternalRaiseRenamed();

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public override DocumentEntry[] ViewingDocuments => _allDocuments.Where(doc => doc.View != null).ToArray();

    #endregion

    /// <summary>
    /// Pauses file watching actions.
    /// </summary>
    private void FileUnwatchedAction_Pause()
    {
    }

    /// <summary>
    /// Resumes file watching actions.
    /// </summary>
    private void FileUnwatchedAction_Resume()
    {
    }

    /// <summary>
    /// Timer callback that evicts hidden documents that have not been visited recently and are not in use.
    /// </summary>
    /// <param name="state">The timer state (unused).</param>
    private void OnTimer(object state)
    {
        List<DocumentEntryBK> removes = null;

        lock (_sync)
        {
            DateTime now = DateTime.Now;

            foreach (var entry in _namedDocuments.Values)
            {
                if (entry.View != null)
                {
                    continue;
                }

                if (EditorUtility.Inspector.IsObjectSelected(entry))
                {
                    continue;
                }

                if (now - entry.LastVisitTime < HiddenDocumentCacheDuration)
                {
                    continue;
                }

                if (entry.CheckIsInUsage())
                {
                    continue;
                }

                (removes ??= []).Add(entry);
            }

            if (removes != null)
            {
                foreach (DocumentEntryBK document in removes)
                {
                    string pathCode = document.FileName.FullPath.GetPathId();
                    _namedDocuments.Remove(pathCode);
                    _allDocuments.Remove(document);
                }
            }
        }

        if (removes != null)
        {
            foreach (DocumentEntryBK documentEntry in removes)
            {
                documentEntry.Save();
                documentEntry.Release();
                if (DebugMessageDocument)
                {
                    EditorServices.SystemLog.AddLog("DocumentManager : Evict document : " + documentEntry.FileName);
                }

                RaiseDocumentClosed(documentEntry);
            }
        }
    }

    /// <summary>
    /// Validates that a document format is available for use.
    /// </summary>
    /// <param name="format">The document format to validate.</param>
    /// <exception cref="NotAvailableException">Thrown when the document type is not available.</exception>
    private void ValidateDocumentFormat(DocumentFormat format)
    {
        if (format is null)
        {
            throw new ArgumentNullException(nameof(format));
        }

        if (format.DocumentType.HasAttributeCached<NotAvailableAttribute>())
        {
            throw new NotAvailableException("Document type is not available.");
        }
    }

    /// <inheritdoc cref="RaiseDocumentSaved"/>
    internal void InternalRaiseDocumentSaved(DocumentEntryBK documentEntry)
    {
        base.RaiseDocumentSaved(documentEntry);
    }
}