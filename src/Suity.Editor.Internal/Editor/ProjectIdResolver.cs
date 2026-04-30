using LiteDB;
using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.NodeQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Represents a record of an object ID mapping stored in the database.
/// </summary>
public class ObjectIdRecord
{
    /// <summary>
    /// Gets or sets the unique database identifier for this record.
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// Gets or sets the full name/path key used to resolve the object.
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// Gets or sets the resolved <see cref="Guid"/> associated with the path.
    /// </summary>
    public Guid Record { get; set; }
}

/// <summary>
/// Internal class implementing <see cref="IObjectIdResolver"/> for resolving and recording object IDs.
/// Uses LiteDB for persistence, XML files for storage, file watchers for external changes, and caching for performance.
/// Handles ID generation, resolution, renaming, and database checkpoints.
/// </summary>
internal class ProjectIdResolver : IObjectIdResolver
{
    /// <summary>
    /// The duration after which dirty ID configurations are automatically saved.
    /// </summary>
    public static readonly TimeSpan SaveIdConfigDuration = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The file name for the object ID XML storage file.
    /// </summary>
    public const string ObjectIdXmlFileName = "ObjectId.xml";
    /// <summary>
    /// The file name for the system ID XML storage file.
    /// </summary>
    public const string SystemIdXmlFileName = "SystemId.xml";
    /// <summary>
    /// The file name for the LiteDB database cache file.
    /// </summary>
    public const string DbFileName = "ObjectIdCache.db";

    private HashSet<Guid> _ids = [];
    private Dictionary<string, Guid> _dic = [];
    private Dictionary<Guid, string> _dicRevert = [];

    // Static fix dictionary
    private readonly Dictionary<string, string> _staticFix = [];
    private readonly Dictionary<string, string> _staticFixRev = [];

    // Guid fast parse cache
    private readonly Dictionary<string, Guid> _guidParseCache = [];

    private readonly Project _project;

    private LiteDatabase _db;
    private ILiteCollection<ObjectIdRecord> _record;
    private bool _dirty = false;
    private bool _needReload = false;

    private DisposeCollector _listener;

    private EditorFileSystemWatcher _watcher;

    private DateTime _lastSaveTime;

    /// <summary>
    /// Gets a value indicating whether the ID resolver has unsaved changes.
    /// </summary>
    public bool IsDirty => _dirty;

    private readonly DbCheckPointDelayedAction _checkPointAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectIdResolver"/> class.
    /// </summary>
    /// <param name="project">The project this resolver belongs to.</param>
    /// <param name="assetPath">The asset directory path.</param>
    public ProjectIdResolver(Project project, string assetPath)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));

        _checkPointAction = new DbCheckPointDelayedAction(this);

        AddFix("*RenderType|Class", "*RenderType|Struct");
        AddFix("*RenderType|Side", "*RenderType|Abstract");
        AddFix("*Engine|Attribute", "*Suity|Attribute");

        EditorServices.SystemLog.AddLog($"Creating Project entry library...");
    }

    /// <summary>
    /// Adds a static fix mapping from an old name to a new name.
    /// </summary>
    /// <param name="oldName">The old name to fix.</param>
    /// <param name="newName">The new name to map to.</param>
    private void AddFix(string oldName, string newName)
    {
        _staticFix.Add(oldName, newName);
        _staticFixRev[newName] = oldName;
    }

    /// <summary>
    /// Starts the ID resolver, loading existing data and setting up file watchers and save listeners.
    /// </summary>
    public void Start()
    {
        Load();

        _listener += EditorCommands.SaveAllDocuments.AddActionListener(() =>
        {
            if (_dirty)
            {
                Save();
            }
        });

        _lastSaveTime = DateTime.UtcNow;
        _listener += EditorRexes.HeartBeat.AddActionListener(() =>
        {
            DateTime now = DateTime.UtcNow;
            if (_dirty && now - _lastSaveTime > SaveIdConfigDuration)
            {
                _lastSaveTime = now;
                Save();
            }
        });

        EditorServices.FileUpdateService.AddFileUpdateListener(LoadingIterations.System, DoFileUpdateSystem);

        CreateFileWatcher();
    }

    /// <summary>
    /// Handles file update events for system ID files, triggering a reload if needed.
    /// </summary>
    /// <param name="obj">The progress object.</param>
    private void DoFileUpdateSystem(IProgress obj)
    {
        if (_needReload)
        {
            _needReload = false;
            Logs.LogInfo("Project Id file changed externally");
            Reload();
        }
    }

    /// <summary>
    /// Releases all resources used by the ID resolver, saving pending changes and disposing the database.
    /// </summary>
    public void Release()
    {
        Save();

        if (_db != null)
        {
            try
            {
                //_db.Rebuild();
                _db.Dispose();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        DisposeFileWatcher();

        EditorServices.SystemLog.AddLog($"Object id file saved.");

        _listener?.Dispose();
    }

    /// <summary>
    /// Generates a new unique <see cref="Guid"/> that does not conflict with existing IDs or editor objects.
    /// </summary>
    /// <returns>A new unique <see cref="Guid"/>.</returns>
    public Guid NewGuid()
    {
        Guid id;

        while (true)
        {
            id = Guid.NewGuid();
            if (!_ids.Contains(id) && EditorObjectManager.Instance.GetEntry(id) is null)
            {
                break;
            }
        }

        _ids.Add(id);

        return id;
    }

    /// <summary>
    /// Resolves a key string to a <see cref="Guid"/>, generating a new one if not found.
    /// Checks asset manager, custom names, key codes, cache, and database before generating.
    /// </summary>
    /// <param name="key">The key string to resolve.</param>
    /// <param name="resolveCustomName">Whether to also check by resource name.</param>
    /// <returns>The resolved or newly generated <see cref="Guid"/>.</returns>
    public Guid Resolve(string key, bool resolveCustomName)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Guid.Empty;
        }

        if (_staticFix.ContainsKey(key))
        {
            key = _staticFix[key];
        }

        Guid id = AssetManager.Instance.GetAsset(key)?.Id ?? Guid.Empty;
        if (id != Guid.Empty)
        {
            return id;
        }

        if (resolveCustomName)
        {
            id = AssetManager.Instance.GetAssetByResourceName(key)?.Id ?? Guid.Empty;
            if (id != Guid.Empty)
            {
                return id;
            }
        }

        if (key.Contains('|'))
        {
            var keyCode = new KeyCode(key);
            var asset = AssetManager.Instance.GetAsset(keyCode.MainKey) as GroupAsset;

            if (keyCode.ElementKey.Contains('.'))
            {
                var fieldCode = new FieldCode(keyCode.ElementKey);
                var childAsset = asset?.GetChildAsset(fieldCode.MainName) as IFieldGroup<DField>;
                id = childAsset?.GetField(fieldCode.FieldName)?.Id ?? Guid.Empty;
            } else
            {
                id = asset?.GetChildAsset(keyCode.ElementKey)?.Id ?? Guid.Empty;
            }

            if (id != Guid.Empty)
            {
                return id;
            }
        }

        lock (this)
        {
            id = ResolveInCache(key);
            if (id != Guid.Empty)
            {
                return id;
            }

            // Parse Guid
            if (Guid.TryParseExact(key, "D", out id))
            {
                _guidParseCache[key] = id;
                return id;
            }

            // Generate one
            while (true)
            {
                id = Guid.NewGuid();
                if (!_ids.Contains(id) && EditorObjectManager.Instance.GetEntry(id) is null)
                {
                    break;
                }
            }

            Record(key, id);

            return id;
        }
    }

    /// <summary>
    /// Resolves a key to an existing entry ID without generating a new one.
    /// Checks the reverse fix dictionary first, then the cache, then falls back to <see cref="Resolve"/>.
    /// </summary>
    /// <param name="key">The key string to resolve.</param>
    /// <param name="resolveCustomName">Whether to also check by resource name.</param>
    /// <returns>The resolved <see cref="Guid"/>, or <see cref="Guid.Empty"/> if not found.</returns>
    public Guid ResolveEntry(string key, bool resolveCustomName)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Guid.Empty;
        }

        string key2 = key;

        if (_staticFixRev.ContainsKey(key2))
        {
            key2 = _staticFixRev[key2];
        }

        Guid id = ResolveInCache(key2);
        if (id != Guid.Empty)
        {
            return id;
        }

        return Resolve(key, resolveCustomName);
    }

    /// <summary>
    /// Resolves a key in the in-memory cache and database without generating a new ID.
    /// </summary>
    /// <param name="key">The key string to resolve.</param>
    /// <returns>The resolved <see cref="Guid"/>, or <see cref="Guid.Empty"/> if not found.</returns>
    private Guid ResolveInCache(string key)
    {
        Guid id = _dic.GetValueSafe(key);
        if (id != Guid.Empty)
        {
            return id;
        }

        if (_record != null)
        {
            // Search library
            try
            {
                id = _record.FindOne(x => x.Path == key)?.Record ?? Guid.Empty;
                // Id found in library cannot conflict with existing Id
                if (id != Guid.Empty)
                {
                    _ids.Add(id);
                    _dic[key] = id;
                    RecordRevert(id, key);

                    EditorServices.SystemLog.AddLog($"Recover resolved id : {key}");
                    return id;
                }
            }
            catch (Exception err)
            {
                err.LogError("Failed to query id from database");
            }
        }
        else
        {
            Logs.LogError("Project Id database is not initialized.");
        }

        // Guid parse cache
        if (_guidParseCache.TryGetValue(key, out id))
        {
            return id;
        }

        return id;
    }

    /// <summary>
    /// Attempts to resolve a key to a <see cref="Guid"/> without generating a new one.
    /// </summary>
    /// <param name="key">The key string to resolve.</param>
    /// <param name="id">The resolved <see cref="Guid"/>, or <see cref="Guid.Empty"/> if not found.</param>
    /// <returns><c>true</c> if the key was successfully resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolve(string key, out Guid id)
    {
        if (string.IsNullOrEmpty(key))
        {
            id = Guid.Empty;
            return false;
        }

        if (_staticFix.ContainsKey(key))
        {
            key = _staticFix[key];
        }

        // Search current usage
        id = AssetManager.Instance.GetAsset(key)?.Id ?? Guid.Empty;
        if (id != Guid.Empty)
        {
            return true;
        }

        id = AssetManager.Instance.GetAssetByResourceName(key)?.Id ?? Guid.Empty;
        if (id != Guid.Empty)
        {
            return true;
        }

        lock (this)
        {
            // Search cache
            id = _dic.GetValueSafe(key);
            if (id != Guid.Empty)
            {
                return true;
            }

            // Search library
            try
            {
                id = _record.FindOne(x => x.Path == key)?.Record ?? Guid.Empty;
                // Id found in library cannot conflict with existing Id
                if (id != Guid.Empty)
                {
                    _ids.Add(id);
                    _dic[key] = id;
                    RecordRevert(id, key);

                    EditorServices.SystemLog.AddLog($"Recover resolved id : {key}");
                    return true;
                }
            }
            catch (Exception err)
            {
                err.LogError("Failed to query id from database");
            }
        }

        // Parse Guid
        if (Guid.TryParseExact(key, "D", out id))
        {
            return true;
        }

        id = Guid.Empty;
        return false;
    }

    /// <summary>
    /// Reverse-resolves a <see cref="Guid"/> back to its original key string.
    /// </summary>
    /// <param name="id">The <see cref="Guid"/> to reverse-resolve.</param>
    /// <returns>The original key string, or <c>null</c> if not found.</returns>
    public string RevertResolve(Guid id)
    {
        lock (this)
        {
            string result = _dicRevert.GetValueSafe(id);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            try
            {
                result = _record.FindOne(x => x.Record == id)?.Path;
                if (!string.IsNullOrEmpty(result))
                {
                    _dicRevert[id] = result;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }
    }

    /// <summary>
    /// Records a key-to-ID mapping in the cache and database.
    /// </summary>
    /// <param name="key">The key string to record.</param>
    /// <param name="id">The <see cref="Guid"/> to associate with the key.</param>
    public void Record(string key, Guid id)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        bool dbUpdated = false;

        lock (this)
        {
            try
            {
                Guid current = _dic.GetValueSafe(key);
                if (current != id)
                {
                    _dic[key] = id;
                    RecordRevert(id, key);
                    _ids.Add(id);

                    var record = _record.FindOne(x => x.Path == key) ?? new ObjectIdRecord { Path = key };
                    if (record.Record != id)
                    {
                        record.Record = id;
                        _record.Upsert(record);
                        dbUpdated = true;
                    }

                    if (EditorPlugin.RuntimeLogging)
                    {
                        EditorServices.SystemLog.AddLog($"Record id : {key}");
                    }
                }

                if (dbUpdated)
                {
                    _dirty = true;
                }
            }
            catch (Exception err)
            {
                err.LogError($"Failed to record id : {key}");
            }
        }

        if (dbUpdated)
        {
            EditorUtility.AddDelayedAction(_checkPointAction);
            EditorRexes.ProjectDirty.Value = true;
        }
    }

    /// <summary>
    /// Renames a recorded key to a new key, updating both cache and database.
    /// </summary>
    /// <param name="key">The old key name.</param>
    /// <param name="newKey">The new key name.</param>
    public void Rename(string key, string newKey)
    {
        if (key is null || newKey is null)
        {
            return;
        }

        bool dbUpdated = false;

        lock (this)
        {
            // Need to remove, otherwise it easily causes conflicts
            if (_dic.TryRemoveAndGet(key, out Guid id))
            {
                _dic[newKey] = id;

                RecordRevert(id, newKey);

                try
                {
                    _record.DeleteMany(x => x.Path == key);

                    var record = _record.FindOne(x => x.Path == newKey) ?? new ObjectIdRecord { Path = newKey };
                    record.Record = id;
                    _record.Upsert(record);
                    dbUpdated = true;

                    EditorServices.SystemLog.AddLog($"Record rename id : {newKey}");
                }
                catch (Exception err)
                {
                    err.LogError($"Failed to rename id : {key} to {newKey}");
                }
            }

            if (dbUpdated)
            {
                _dirty = true;
            }
        }

        if (dbUpdated)
        {
            EditorUtility.AddDelayedAction(_checkPointAction);
            EditorRexes.ProjectDirty.Value = true;
        }
    }

    /// <summary>
    /// Loads ID mappings from XML files and initializes the LiteDB database.
    /// Transfers existing in-memory records to the database if needed.
    /// </summary>
    private void Load()
    {
        _ids.Clear();
        _dic.Clear();
        _dicRevert.Clear();

        string sysFileName = _project.SystemDirectory.PathAppend(SystemIdXmlFileName);
        string objFileName = _project.SystemDirectory.PathAppend(ObjectIdXmlFileName);

        bool allExist = File.Exists(sysFileName) && File.Exists(objFileName);

        if (File.Exists(sysFileName))
        {
            FileUnwatchedAction.Do(() =>
            {
                try
                {
                    LoadIdXml(sysFileName);
                }
                catch (Exception err)
                {
                    err.LogError("Failed to load system Id file.");
                }
            });
        }

        if (File.Exists(objFileName))
        {
            FileUnwatchedAction.Do(() =>
            {
                try
                {
                    LoadIdXml(objFileName);
                }
                catch (Exception err)
                {
                    err.LogError("Failed to load object Id file.");
                }
            });
        }

        if (!allExist)
        {
            Save(true);
        }

        try
        {
            string dbFileName = _project.UserDirectory.PathAppend(DbFileName);
            bool fileExist = File.Exists(dbFileName);

            if (_db is null)
            {
                var conn = new ConnectionString
                {
                    Filename = dbFileName,
                    Connection = ConnectionType.Shared,
                    //Password = "{83E7D916-C2B8-4CE1-B335-7B96B61873DC}"
                };
                _db = new LiteDatabase(dbFileName);
                _record = _db.GetCollection<ObjectIdRecord>("ObjectIdRecord");
                _record.EnsureIndex(o => o.Path, true);
                _record.EnsureIndex(o => o.Record, false);
            }

            _db.BeginTrans();
            foreach (var pair in _dic)
            {
                var record = _record.FindOne(x => x.Path == pair.Key) ?? new ObjectIdRecord { Path = pair.Key };
                if (record.Record != pair.Value)
                {
                    record.Record = pair.Value;

                    EditorServices.SystemLog.AddLog($"Record transfer id : {pair.Key}");
                    try
                    {
                        _record.Upsert(record);
                    }
                    catch (Exception err)
                    {
                        err.LogWarning($"Ignore record Id:{pair.Key}");
                    }
                }
            }
            _db.Commit();

            _db.Checkpoint();
        }
        catch (Exception err)
        {
            err.LogError("Failed to initialize id database.");
        }
    }

    /// <summary>
    /// Loads ID mappings from a single XML file into the in-memory dictionaries.
    /// </summary>
    /// <param name="fileName">The path to the XML file to load.</param>
    private void LoadIdXml(string fileName)
    {
        EditorServices.SystemLog.AddLog($"Start loading id file : {fileName}");
        int count = 0;

        INodeReader reader = XmlNodeReader.FromFile(fileName, false);
        if (reader.NodeName != "ObjectId")
        {
            throw new InvalidOperationException("File format is invalid.");
        }

        foreach (var childReader in reader.Nodes("Object"))
        {
            string idStr = childReader.GetAttribute("id");
            if (string.IsNullOrEmpty(idStr))
            {
                continue;
            }

            string name = childReader.GetAttribute("name");
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (_staticFix.ContainsKey(name))
            {
                name = _staticFix[name];
            }

            // Fixed rule fix
            if (name.StartsWith("*Render|"))
            {
                string nameFix = "*Design" + name[7..];
                name = nameFix;
                _dirty = true;
            }

            if (!Guid.TryParseExact(idStr, "D", out Guid id))
            {
                Logs.LogWarning($"Id read error:{name}");
                continue;
            }

            _dic[name] = id;
            RecordRevert(id, name);

            _ids.Add(id);
            count++;
        }

        EditorServices.SystemLog.AddLog($"{count} object ids loaded.");
    }

    /// <summary>
    /// Saves ID mappings to XML files and triggers plugin save operations.
    /// </summary>
    /// <param name="forceSave">If <c>true</c>, saves even when no changes are pending.</param>
    private void Save(bool forceSave = false)
    {
        foreach (var plugin in EditorServices.PluginService.Plugins.Select(o => o.Plugin))
        {
            try
            {
                plugin.SaveProject();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        if (!_dirty && !forceSave)
        {
            return;
        }

        string sysFileName = _project.SystemDirectory.PathAppend(SystemIdXmlFileName);
        SaveIdXml(sysFileName, o => o.FullName.StartsWith("*"));

        string objFileName = _project.SystemDirectory.PathAppend(ObjectIdXmlFileName);
        SaveIdXml(objFileName, o => !o.FullName.StartsWith("*"));
    }

    /// <summary>
    /// Saves ID mappings to a specific XML file, filtering objects by the given predicate.
    /// </summary>
    /// <param name="fileName">The path to the XML file to save.</param>
    /// <param name="predicate">A predicate to determine which editor objects to include.</param>
    private void SaveIdXml(string fileName, Predicate<EditorObject> predicate)
    {
        EditorServices.SystemLog.AddLog($"Start saving id file : {fileName}");

        var writer = new XmlNodeWriter("ObjectId");

        var allObjs = EditorObjectManager.Instance.Entries
            .SelectMany(o => o.Targets)
            .SkipNull()
            .OrderBy(o => o.FullName ?? string.Empty)
            .ToList();

        // HasRecorededId means this object has already been recorded in the file, no need to record again
        foreach (var obj in allObjs.Where(o => !o.IsIdDocumented))
        {
            if (!predicate(obj))
            {
                continue;
            }

            writer.BeginElement("Object");
            writer.SetAttribute("id", obj.Id);
            writer.SetAttribute("name", obj.FullName);
            writer.EndElement();
        }

        FileUnwatchedAction.Do(() =>
        {
            try
            {
                writer.SaveToFile(fileName);
                _dirty = false;
                EditorServices.SystemLog.AddLog($"{fileName} saved.");
            }
            catch (Exception err)
            {
                err.LogError();
            }
        });
    }

    /// <summary>
    /// Records a reverse mapping from an ID back to its key, logging warnings on conflicts.
    /// </summary>
    /// <param name="id">The <see cref="Guid"/> to map from.</param>
    /// <param name="key">The key string to map to.</param>
    private void RecordRevert(Guid id, string key)
    {
        if (_dicRevert.TryGetValue(id, out string oldPath) && oldPath != key)
        {
            if (AssetManager.Instance.GetAsset(oldPath) != null)
            {
                Logs.LogWarning($"Reverse cache Id conflict:{id} ({oldPath} and {key})");
            }
        }
        _dicRevert[id] = key;
    }

    #region Watcher

    /// <summary>
    /// Creates and configures a file system watcher to monitor ID file changes.
    /// </summary>
    private void CreateFileWatcher()
    {
        if (_watcher != null)
        {
            return;
        }

        _watcher = new EditorFileSystemWatcher(_project.SystemDirectory, this);

        _watcher.Changed += watcher_Changed;
        _watcher.Created += watcher_Created;
        _watcher.Deleted += watcher_Deleted;
        _watcher.Renamed += watcher_Renamed;

        _watcher.EnableRaisingEvents = true;

        FileUnwatchedAction.Pause += FileUnwatchedAction_Pause;
        FileUnwatchedAction.Resume += FileUnwatchedAction_Resume;
    }

    /// <summary>
    /// Disposes the file system watcher and unsubscribes from file unwatched actions.
    /// </summary>
    private void DisposeFileWatcher()
    {
        if (_watcher is null)
        {
            return;
        }

        FileUnwatchedAction.Pause -= FileUnwatchedAction_Pause;
        FileUnwatchedAction.Resume -= FileUnwatchedAction_Resume;

        _watcher.Changed -= watcher_Changed;
        _watcher.Created -= watcher_Created;
        _watcher.Deleted -= watcher_Deleted;
        _watcher.Renamed -= watcher_Renamed;

        _watcher.Dispose();
        _watcher = null;
    }

    /// <summary>
    /// Handles the file changed event from the watcher.
    /// </summary>
    /// <param name="path">The path of the changed file.</param>
    private void watcher_Changed(string path)
    {
        CheckReload(path);
    }

    /// <summary>
    /// Handles the file created event from the watcher.
    /// </summary>
    /// <param name="path">The path of the created file.</param>
    private void watcher_Created(string path)
    {
        CheckReload(path);
    }

    /// <summary>
    /// Handles the file deleted event from the watcher.
    /// </summary>
    /// <param name="path">The path of the deleted file.</param>
    private void watcher_Deleted(string path)
    {
        CheckReload(path);
    }

    /// <summary>
    /// Handles the file renamed event from the watcher.
    /// </summary>
    /// <param name="path">The new path of the renamed file.</param>
    /// <param name="oldPath">The old path of the renamed file.</param>
    private void watcher_Renamed(string path, string oldPath)
    {
        CheckReload(path);
    }

    /// <summary>
    /// Pauses the file watcher when file unwatched actions begin.
    /// </summary>
    private void FileUnwatchedAction_Pause()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    /// <summary>
    /// Resumes the file watcher when file unwatched actions complete.
    /// </summary>
    private void FileUnwatchedAction_Resume()
    {
        if (_watcher != null)
        {
            try
            {
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    #endregion

    #region Reload

    /// <summary>
    /// Checks if the given file path matches the object ID XML file name and marks for reload if so.
    /// </summary>
    /// <param name="fullPath">The full path to check.</param>
    private void CheckReload(string fullPath)
    {
        if (Path.GetFileName(fullPath).IgnoreCaseEquals(ObjectIdXmlFileName))
        {
            _needReload = true;
            EditorServices.FileUpdateService.UpdateFileDelayed();
        }
    }

    /// <summary>
    /// Reloads ID mappings from disk and updates any editor objects whose IDs have changed.
    /// </summary>
    private void Reload()
    {
        //var ids = _ids;
        var dic = _dic;
        //var dicRevert = _dicRevert;

        _ids = [];
        _dic = [];
        _dicRevert = [];

        Load();

        // During document reload, because AssetBuilder was not deleted and all Ids were preserved
        // Updated documents did not re-resolve Ids, so need to manually update Ids

        foreach (var item in dic)
        {
            Guid idOld = item.Value;

            // Find same Key, different Id
            if (_dic.TryGetValue(item.Key, out Guid idNew) && idNew != idOld)
            {
                EditorObject obj = EditorObjectManager.Instance.GetObject(item.Value);
                if (obj != null)
                {
                    // Change Id
                    obj.Entry = EditorObjectManager.Instance.EnsureEntry(idNew);
                    Logs.LogWarning($"{item.Key}'s Id changed from {idOld} to {idNew}.");
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Performs a database checkpoint to flush pending writes. Called as a delayed action.
    /// </summary>
    internal void DbCheckPoint()
    {
        lock (this)
        {
            _db?.Checkpoint();
        }
    }

    /// <summary>
    /// Delayed action that triggers a database checkpoint after ID changes.
    /// </summary>
    private class DbCheckPointDelayedAction(ProjectIdResolver value) : DelayedAction<ProjectIdResolver>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            EditorServices.SystemLog.AddLog($"Run {nameof(DbCheckPointDelayedAction)}");

            Value.DbCheckPoint();
        }
    }

    /// <summary>
    /// Delayed action that reloads object IDs from disk after external file changes.
    /// </summary>
    private class ReloadObjectIdDelayedAction(ProjectIdResolver value) : DelayedAction<ProjectIdResolver>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Logs.LogInfo("Reload ObjectId...");
            FileUnwatchedAction.Do(Value.Load);
        }
    }
}