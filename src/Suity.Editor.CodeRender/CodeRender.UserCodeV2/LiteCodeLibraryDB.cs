using LiteDB;
using Suity.Editor.CodeRender.Replacing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender.UserCodeV2;

/// <summary>
/// Provides a LiteDB-backed implementation of <see cref="ICodeLibraryDatabase"/> for storing and querying code tags.
/// This class manages a local database file with encrypted access for persisting user code metadata.
/// </summary>
public class LiteCodeLibraryDB : ICodeLibraryDatabase, IDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether database operation logging is enabled.
    /// When enabled, debug logs are written for query and store operations.
    /// </summary>
    public static bool Logging;

    private static readonly BsonMapper _mapper;

    private readonly string _dbFile;
    private readonly LiteDatabase _db;

    /// <summary>
    /// Initializes the static <see cref="LiteCodeLibraryDB"/> class and configures the BSON mapper.
    /// </summary>
    static LiteCodeLibraryDB()
    {
        _mapper = new BsonMapper
        {
            EmptyStringToNull = false,
            SerializeNullValues = true,
            TrimWhitespace = false
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteCodeLibraryDB"/> class with the specified database file path.
    /// Opens a shared connection to the LiteDB database and ensures indexes are created for efficient querying.
    /// </summary>
    /// <param name="dbFile">The path to the LiteDB database file. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbFile"/> is null.</exception>
    public LiteCodeLibraryDB(string dbFile)
    {
        _dbFile = dbFile ?? throw new ArgumentNullException(nameof(dbFile));

        var conn = new ConnectionString
        {
            Connection = ConnectionType.Shared,
            Filename = dbFile,
            Upgrade = true,
            Password = "sty.ed.db.common.50186-19482-63900-45674",
        };
        _db = new LiteDatabase(conn, _mapper);

        var codeTags = _db.GetCollection<CodeTag>("CodeTag");
        codeTags.EnsureIndex(o => o.FileName);
        codeTags.EnsureIndex(o => o.Location);
        codeTags.EnsureIndex(o => o.Material);
        codeTags.EnsureIndex(o => o.RenderType);
        codeTags.EnsureIndex(o => o.KeyString);
        codeTags.EnsureIndex(o => o.Extension);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            _db.Commit();
            _db.Checkpoint();
        }
        finally
        {
            _db.Dispose();
        }
    }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    public void BeginTrans()
    {
        _db.BeginTrans();
    }

    /// <inheritdoc/>
    public void CheckPoint()
    {
        _db.Commit();
        _db.Checkpoint();
    }

    /// <inheritdoc/>
    public bool RestoreMode => false;

    /// <summary>
    /// Queries and returns all code tags stored in the database.
    /// </summary>
    /// <returns>An enumerable of all <see cref="CodeTag"/> instances in the database.</returns>
    public IEnumerable<CodeTag> QueryAll()
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");
            var now = DateTime.Now;
            var results = codeTags.FindAll();

            if (Logging)
            {
                Logs.LogDebug("QueryAll : " + (DateTime.Now - now).Ticks);
            }

            return results;
        }
    }

    /// <summary>
    /// Queries and returns all code tags associated with a specific file ID (file name).
    /// </summary>
    /// <param name="fileId">The file name to search for.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified file ID.</returns>
    public CodeTagCollection QueryByFileId(string fileId)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            return QueryByFileId(codeTags, fileId);
        }
    }

    /// <summary>
    /// Queries and returns all code tags that exactly match the specified key components.
    /// </summary>
    /// <param name="location">The location to match.</param>
    /// <param name="material">The material to match.</param>
    /// <param name="renderType">The render type to match.</param>
    /// <param name="keyString">The key string to match.</param>
    /// <param name="extension">The extension to match.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified criteria.</returns>
    public CodeTagCollection QueryExact(string location, string material, string renderType, string keyString, string extension)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            return QueryExact(codeTags, location, material, renderType, keyString, extension);
        }
    }

    /// <inheritdoc/>
    public CodeTag QueryTag(string location, string material, string renderType, string keyString, string extension)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            return QueryTag(codeTags, location, material, renderType, keyString, extension);
        }
    }

    /// <summary>
    /// Queries and returns all code tags that match the specified key string.
    /// </summary>
    /// <param name="keyString">The key string to search for.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified key string.</returns>
    [Obsolete]
    public CodeTagCollection QueryTags(string keyString)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            return QueryByKeyString(codeTags, keyString);
        }
    }

    /// <summary>
    /// Replaces all code tags in the database for a specific file name with the tags from the provided collection.
    /// Existing tags for the file are deleted before inserting the new ones.
    /// </summary>
    /// <param name="entry">The <see cref="CodeTagCollection"/> containing the replacement tags. Must not be null and must have a non-empty <see cref="CodeTagCollection.FileName"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entry"/> has an empty <see cref="CodeTagCollection.FileName"/>.</exception>
    public void ReplaceEntriesByFileName(CodeTagCollection entry)
    {
        if (entry is null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(entry.FileName)) throw new ArgumentException();

        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            var exist = QueryByFileId(codeTags, entry.FileName);
            foreach (var item in exist.Tags)
            {
                codeTags.Delete(item.Id);
            }

            foreach (var item in entry.Tags)
            {
                codeTags.Insert(item);
            }
        }
    }

    /// <inheritdoc/>
    public CodeTagCollection IncrementalStore(string fileName, string location, SegmentDocument collection, bool skipEmpty)
    {
        if (collection is null)
        {
            throw new ArgumentNullException();
        }

        var entry = CodeTagCollection.CreateBySegments(fileName, location, collection, skipEmpty);

        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            foreach (var item in entry.Tags)
            {
                var now = DateTime.Now;
                var current = QueryTag(codeTags, item.Location, item.Material, item.RenderType, item.KeyString, item.Extension);

                if (current != null)
                {
                    if (current.FileName != fileName || current.Code != item.Code)
                    {
                        current.FileName = fileName;
                        current.Code = item.Code;
                        codeTags.Update(current);
                    }
                }
                else
                {
                    codeTags.Insert(item);
                }
                if (Logging) Logs.LogDebug("IncrementalStore : " + (DateTime.Now - now).Ticks);
            }

            return entry;
        }
    }

    /// <summary>
    /// Performs an incremental store operation, updating existing tags or inserting new ones from the provided collection.
    /// Tags are matched by their key components (location, material, render type, key string, extension).
    /// </summary>
    /// <param name="entry">The <see cref="CodeTagCollection"/> containing tags to store. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
    public void IncrementalStore(CodeTagCollection entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException();
        }

        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            var now = DateTime.Now;

            foreach (var item in entry.Tags)
            {
                var current = QueryTag(codeTags, item.Location, item.Material, item.RenderType, item.KeyString, item.Extension);
                if (current != null)
                {
                    if (current.FileName != item.FileName || current.Code != item.Code)
                    {
                        current.FileName = item.FileName;
                        current.Code = item.Code;
                        codeTags.Update(current);
                    }
                }
                else
                {
                    codeTags.Insert(item);
                }
            }
            if (Logging)
            {
                Logs.LogDebug("IncrementalStore : " + (DateTime.Now - now).Ticks);
            }
        }
    }

    /// <summary>
    /// Determines whether the database contains any code tags for the specified file name.
    /// </summary>
    /// <param name="fileName">The file name to search for.</param>
    /// <returns><c>true</c> if any tags exist for the specified file name; otherwise, <c>false</c>.</returns>
    public bool Contains(string fileName)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");
            var result = codeTags.FindOne(o => o.FileName == fileName);

            return result != null;
        }
    }

    /// <summary>
    /// Determines whether the user code in the specified segment document has changed compared to the stored tags matching the given criteria.
    /// </summary>
    /// <param name="location">The location to match.</param>
    /// <param name="material">The material to match.</param>
    /// <param name="renderType">The render type to match.</param>
    /// <param name="keyString">The key string to match.</param>
    /// <param name="extension">The extension to match.</param>
    /// <param name="collection">The <see cref="SegmentDocument"/> containing the current user code to compare against.</param>
    /// <returns><c>true</c> if no matching tags exist or if the user code has changed; otherwise, <c>false</c>.</returns>
    public bool IsUserCodeChanged(string location, string material, string renderType, string keyString, string extension, SegmentDocument collection)
    {
        var entry = QueryExact(location, material, renderType, keyString, extension);
        if (entry.Tags.Count == 0)
        {
            return true;
        }
        else
        {
            return entry.IsUserCodeChanged(collection);
        }
    }

    /// <summary>
    /// Renames all code tags that match the specified old key string to use a new key string.
    /// Any existing tags with the new key string are deleted to avoid conflicts.
    /// </summary>
    /// <param name="oldKey">The current key string to search for and replace.</param>
    /// <param name="newKey">The new key string to assign.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing the renamed tags.</returns>
    public CodeTagCollection RenameKeyString(string oldKey, string newKey)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            var entry = QueryByKeyString(codeTags, oldKey);

            foreach (CodeTag item in entry.Tags)
            {
                // Delete all current ones to avoid conflicts
                var exists = QueryExact(codeTags, item.Location, item.Material, item.RenderType, newKey, item.Extension);
                foreach (var exist in exists.Tags)
                {
                    codeTags.Delete(exist.Id);
                }

                item.KeyString = newKey;
                codeTags.Update(item);
            }
            return entry;
        }
    }

    /// <summary>
    /// Renames all code tags that match the specified old material to use a new material.
    /// Any existing tags with the new material (and matching other key components) are deleted to avoid conflicts.
    /// </summary>
    /// <param name="oldKey">The current material to search for and replace.</param>
    /// <param name="newKey">The new material to assign.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing the renamed tags.</returns>
    public CodeTagCollection RenameMaterial(string oldKey, string newKey)
    {
        lock (this)
        {
            var codeTags = _db.GetCollection<CodeTag>("CodeTag");

            var entry = QueryByMaterial(codeTags, oldKey);

            foreach (CodeTag item in entry.Tags)
            {
                // Delete all current ones to avoid conflicts
                var exists = QueryExact(codeTags, item.Location, newKey, item.RenderType, item.KeyString, item.Extension);
                foreach (var exist in exists.Tags)
                {
                    codeTags.Delete(exist.Id);
                }

                item.Material = newKey;
                codeTags.Update(item);
            }
            return entry;
        }
    }

    /// <summary>
    /// Defragments the database file by shrinking it to reclaim unused space.
    /// Currently disabled (commented out).
    /// </summary>
    public void Defragment()
    {
        //lock (this)
        //{
        //    _db.Shrink();
        //}
    }

    /// <summary>
    /// Queries code tags by file name using the specified LiteDB collection.
    /// </summary>
    /// <param name="codeTags">The LiteDB collection to query. Must not be null.</param>
    /// <param name="fileId">The file name to search for. Must not be null or empty.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified file name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeTags"/> is null or <paramref name="fileId"/> is null or empty.</exception>
    /// <exception cref="OdbException">Thrown when the database query fails.</exception>
    private CodeTagCollection QueryByFileId(ILiteCollection<CodeTag> codeTags, string fileId)
    {
        if (codeTags is null)
        {
            throw new ArgumentNullException();
        }

        if (string.IsNullOrEmpty(fileId))
        {
            throw new ArgumentNullException();
        }

        lock (this)
        {
            try
            {
                var now = DateTime.Now;
                var results = codeTags.Find(tag => tag.FileName == fileId);
                if (Logging) Logs.LogDebug("QueryByFileName : " + (DateTime.Now - now).Ticks);
                var entry = new CodeTagCollection(fileId, null, results);
                entry.CheckTagsNull();

                return entry;
            }
            catch (Exception err)
            {
                throw new OdbException("Query database failed.", err);
            }
        }
    }

    /// <summary>
    /// Queries code tags that exactly match all specified key components using the specified LiteDB collection.
    /// </summary>
    /// <param name="codeTags">The LiteDB collection to query. Must not be null.</param>
    /// <param name="location">The location to match.</param>
    /// <param name="material">The material to match.</param>
    /// <param name="renderType">The render type to match.</param>
    /// <param name="keyString">The key string to match.</param>
    /// <param name="extension">The extension to match.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeTags"/> is null.</exception>
    /// <exception cref="OdbException">Thrown when the database query fails.</exception>
    private CodeTagCollection QueryExact(ILiteCollection<CodeTag> codeTags, string location, string material, string renderType, string keyString, string extension)
    {
        if (codeTags is null)
        {
            throw new ArgumentNullException();
        }

        lock (this)
        {
            try
            {
                var now = DateTime.Now;
                var results = codeTags.Find(tag =>
                    tag.Location == location &&
                    tag.Material == material &&
                    tag.RenderType == renderType &&
                    tag.KeyString == keyString &&
                    tag.Extension == extension
                );

                if (Logging) Logs.LogDebug("QueryByLocation : " + (DateTime.Now - now).TotalMilliseconds);
                var entry = new CodeTagCollection(null, location, results);
                entry.CheckTagsNull();

                return entry;
            }
            catch (Exception err)
            {
                throw new OdbException("Query database failed.", err);
            }
        }
    }

    /// <summary>
    /// Queries code tags by key string using the specified LiteDB collection.
    /// </summary>
    /// <param name="codeTags">The LiteDB collection to query. Must not be null.</param>
    /// <param name="keyString">The key string to search for. Must not be null or empty.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified key string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeTags"/> is null or <paramref name="keyString"/> is null or empty.</exception>
    /// <exception cref="OdbException">Thrown when the database query fails.</exception>
    private CodeTagCollection QueryByKeyString(ILiteCollection<CodeTag> codeTags, string keyString)
    {
        if (codeTags is null)
        {
            throw new ArgumentNullException();
        }
        if (string.IsNullOrEmpty(keyString))
        {
            throw new ArgumentNullException();
        }

        lock (this)
        {
            try
            {
                var now = DateTime.Now;
                var results = codeTags.Find(tag => tag.KeyString == keyString);

                if (Logging) Logs.LogDebug("QueryByKeyString : " + (DateTime.Now - now).Ticks);
                var entry = new CodeTagCollection(results);
                entry.CheckTagsNull();
                return entry;
            }
            catch (Exception err)
            {
                throw new OdbException("Query database failed.", err);
            }
        }
    }

    /// <summary>
    /// Queries code tags by material using the specified LiteDB collection.
    /// </summary>
    /// <param name="codeTags">The LiteDB collection to query. Must not be null.</param>
    /// <param name="material">The material to search for. Must not be null or empty.</param>
    /// <returns>A <see cref="CodeTagCollection"/> containing all tags matching the specified material.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeTags"/> is null or <paramref name="material"/> is null or empty.</exception>
    /// <exception cref="OdbException">Thrown when the database query fails.</exception>
    private CodeTagCollection QueryByMaterial(ILiteCollection<CodeTag> codeTags, string material)
    {
        if (codeTags is null)
        {
            throw new ArgumentNullException();
        }

        if (string.IsNullOrEmpty(material))
        {
            throw new ArgumentNullException();
        }

        lock (this)
        {
            try
            {
                var now = DateTime.Now;
                var results = codeTags.Find(tag => tag.Material == material);

                if (Logging) Logs.LogDebug("QueryByMaterial : " + (DateTime.Now - now).Ticks);
                var entry = new CodeTagCollection(results);
                entry.CheckTagsNull();
                return entry;
            }
            catch (Exception err)
            {
                throw new OdbException("Query database failed.", err);
            }
        }
    }

    /// <summary>
    /// Queries a single code tag that exactly matches all specified key components using the specified LiteDB collection.
    /// </summary>
    /// <param name="codeTags">The LiteDB collection to query.</param>
    /// <param name="location">The location to match.</param>
    /// <param name="material">The material to match.</param>
    /// <param name="renderType">The render type to match.</param>
    /// <param name="keyString">The key string to match.</param>
    /// <param name="extension">The extension to match.</param>
    /// <returns>The first matching <see cref="CodeTag"/>, or null if no match is found.</returns>
    /// <exception cref="OdbException">Thrown when the database query fails.</exception>
    private CodeTag QueryTag(ILiteCollection<CodeTag> codeTags, string location, string material, string renderType, string keyString, string extension)
    {
        lock (this)
        {
            try
            {
                var tags = codeTags.Find(tag =>
                        tag.Location == location &&
                        tag.Material == material &&
                        tag.RenderType == renderType &&
                        tag.KeyString == keyString &&
                        tag.Extension == extension
                        );

                return tags.FirstOrDefault();
            }
            catch (Exception err)
            {
                throw new OdbException("Query database failed.", err);
            }
        }
    }
}
