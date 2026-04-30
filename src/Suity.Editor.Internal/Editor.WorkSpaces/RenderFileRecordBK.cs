using Suity.Synchonizing;
using System;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Internal implementation of a render file record that tracks file rendering state and supports synchronization.
/// </summary>
public class RenderFileRecordBK : RenderFileRecord, ISyncObject
{
    internal string _rFileName;
    internal string _oldFileName;
    internal DateTime _lastUpdateTime;
    internal bool _dirty;

    /// <inheritdoc/>
    public override string RelativeFileName => _rFileName;

    /// <inheritdoc/>
    public override string OldFileName => _oldFileName;

    /// <inheritdoc/>
    public override DateTime LastUpdateTime => _lastUpdateTime;

    /// <inheritdoc/>
    public override bool Dirty => _dirty;

    /// <summary>
    /// Initializes a new empty instance of <see cref="RenderFileRecordBK"/>.
    /// </summary>
    public RenderFileRecordBK()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified file name and update time.
    /// </summary>
    /// <param name="rFileName">The relative file name.</param>
    /// <param name="lastUpdateTime">The last update timestamp.</param>
    public RenderFileRecordBK(string rFileName, DateTime lastUpdateTime)
    {
        _rFileName = rFileName;
        _lastUpdateTime = lastUpdateTime;
    }

    /// <summary>
    /// Initializes a new instance with the specified file name, old file name, and update time.
    /// </summary>
    /// <param name="rFileName">The relative file name.</param>
    /// <param name="oldFileName">The previous file name before renaming, or null.</param>
    /// <param name="lastUpdateTime">The last update timestamp.</param>
    public RenderFileRecordBK(string rFileName, string oldFileName, DateTime lastUpdateTime)
    {
        _rFileName = rFileName;
        _oldFileName = oldFileName;
        _lastUpdateTime = lastUpdateTime;
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _rFileName = sync.Sync(nameof(RelativeFileName), _rFileName);
        _oldFileName = sync.Sync(nameof(OldFileName), _oldFileName);
        if (sync.Intent == SyncIntent.Clone)
        {
            _lastUpdateTime = sync.Sync(nameof(LastUpdateTime), _lastUpdateTime);
        }

        _dirty = sync.Sync(nameof(Dirty), _dirty, SyncFlag.None, false);
    }
}