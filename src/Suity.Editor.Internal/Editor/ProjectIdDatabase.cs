using LiteDB;
using Suity.Editor.Services;
using System;

namespace Suity.Editor;

/// <summary>
/// Manages a LiteDB database for persistent storage of object ID mappings.
/// Uses lazy initialization and delayed disposal for efficient resource management.
/// </summary>
internal class ProjectIdDatabase : IDisposable
{
    private readonly string _dbFileName;
    private readonly DisposeDelayedAction _disposeAction;

    private LiteDatabase _db;
    private ILiteCollection<ObjectIdRecord> _record;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectIdDatabase"/> class.
    /// </summary>
    /// <param name="fileName">The path to the LiteDB database file.</param>
    public ProjectIdDatabase(string fileName)
    {
        _dbFileName = fileName;
        _disposeAction = new DisposeDelayedAction(this);
    }

    /// <summary>
    /// Lazily initializes and returns the LiteDB collection for object ID records.
    /// Disposes any existing database connection before creating a new one.
    /// </summary>
    /// <returns>The LiteDB collection for <see cref="ObjectIdRecord"/>, or null if initialization fails.</returns>
    private ILiteCollection<ObjectIdRecord> GetRecord()
    {
        if (_record != null)
        {
            return _record;
        }

        lock (this)
        {
            try
            {
                _db?.Dispose();

                _db = new LiteDatabase(_dbFileName);
                _record = _db.GetCollection<ObjectIdRecord>("ObjectIdRecord");
                _record.EnsureIndex(o => o.Path, true);

                EditorUtility.AddDelayedAction(_disposeAction);

                return _record;
            }
            catch (Exception err)
            {
                err.LogError();
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (this)
        {
            try
            {
                _db?.Dispose();
                _record = null;
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Delayed action that disposes the database connection after use.
    /// </summary>
    private class DisposeDelayedAction(ProjectIdDatabase value) : DelayedAction<ProjectIdDatabase>(value)
    {
        /// <inheritdoc/>
        public override void DoAction()
        {
            Value.Dispose();
        }
    }
}