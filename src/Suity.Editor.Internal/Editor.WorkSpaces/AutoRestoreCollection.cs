using Suity.Collections;
using Suity.Editor.CodeRender;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Represents a group of render targets that share the same user code library for auto-restore operations.
/// </summary>
internal class AutoRestoreItem
{
    /// <summary>
    /// A short display name for this item, typically derived from the owning asset.
    /// </summary>
    public string ShortName;

    /// <summary>
    /// The user code library associated with this restore group.
    /// </summary>
    public ICodeLibrary UserCode;

    /// <summary>
    /// The set of render targets belonging to this restore group.
    /// </summary>
    public readonly HashSet<RenderTarget> Targets = [];
}

/// <summary>
/// Collects render targets grouped by their user code library for batch restore operations during rendering.
/// Targets that do not specify a custom restore library or have auto-restore disabled fall back to the workspace's default user code.
/// </summary>
internal class AutoRestoreCollection
{
    private readonly string _defaultFullPath;
    private readonly AutoRestoreItem _defaultItem;

    private readonly Dictionary<ICodeLibrary, AutoRestoreItem> _dicByUserCode = [];

    /// <summary>
    /// Initializes a new instance with the workspace's default user code as the fallback.
    /// </summary>
    /// <param name="workSpace">The owning workspace.</param>
    public AutoRestoreCollection(WorkSpace workSpace)
    {
        _defaultFullPath = workSpace.UserCode.FileName.FullPath;
        _defaultItem = new AutoRestoreItem { UserCode = workSpace.UserCode, ShortName = workSpace.Name };
    }

    /// <summary>
    /// Adds multiple render targets to the collection.
    /// </summary>
    /// <param name="targets">The render targets to add.</param>
    public void AddRange(IEnumerable<RenderTarget> targets)
    {
        foreach (var target in targets)
        {
            Add(target);
        }
    }

    /// <summary>
    /// Adds a single render target, grouping it by its user code library if applicable.
    /// </summary>
    /// <param name="target">The render target to add.</param>
    /// <returns>True if the target was added to a custom user code group; false if it fell back to the default group.</returns>
    public bool Add(RenderTarget target)
    {
        if (target is null)
        {
            return false;
        }

        do
        {
            if (target.Tag is not WorkSpaceRefItem refItem)
            {
                break;
            }

            ICodeLibrary userCode;
            if (target.RestoreCodeLibrary != null)
            {
                userCode = target.RestoreCodeLibrary;
            }
            else
            {
                if (!refItem.AutoRestoreUserCode)
                {
                    break;
                }

                userCode = refItem.UserCode;
                if (userCode is null)
                {
                    break;
                }
            }

            //if (!StorageManager.Instance.FileExists(userCode.FileName))
            //{
            //    break;
            //}
            if (userCode.FileName?.FullPath == _defaultFullPath)
            {
                break;
            }

            AutoRestoreItem item = _dicByUserCode.GetValueSafe(userCode);
            if (item != null)
            {
                return item.Targets.Add(target);
            }

            Asset asset = AssetManager.Instance.GetAsset(target.OwnerId);
            if (asset is null)
            {
                break;
            }

            item = new AutoRestoreItem
            {
                UserCode = userCode,
                ShortName = asset.ShortTypeName,
            };
            item.Targets.Add(target);

            _dicByUserCode.Add(userCode, item);

            return true;
        } while (false);

        _defaultItem.Targets.Add(target);
        return false;
    }

    /// <summary>
    /// Enumerates all auto-restore items, starting with the default item if it has targets.
    /// </summary>
    public IEnumerable<AutoRestoreItem> Items
    {
        get
        {
            if (_defaultItem.Targets.Count > 0)
            {
                yield return _defaultItem;
            }

            foreach (var item in _dicByUserCode.Values)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Gets the total number of restore groups (including the default group if it has targets).
    /// </summary>
    public int Count
    {
        get
        {
            if (_defaultItem.Targets.Count > 0)
            {
                return _dicByUserCode.Count + 1;
            }
            else
            {
                return _dicByUserCode.Count;
            }
        }
    }
}