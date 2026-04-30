using Suity.Synchonizing.Core;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Manages clipboard operations for copying and moving data within the editor.
/// </summary>
public class ClipboardService : IClipboardService
{
    /// <summary>
    /// Singleton instance of the clipboard service.
    /// </summary>
    public static readonly ClipboardService Instance = new();

    /// <summary>
    /// Gets whether the current clipboard data was copied (true) or cut (false).
    /// </summary>
    public bool IsCopy { get; private set; }

    /// <summary>
    /// Gets additional metadata associated with the clipboard data.
    /// </summary>
    public object ExtraInfo { get; private set; }

    private readonly List<ClipboardItem> _items = [];

    /// <inheritdoc/>
    public void SetData(object data, bool isCopy, object extraInfo = null)
    {
        _items.Clear();

        if (data is ClipboardItem clipboardItem)
        {
            _items.Add(clipboardItem);
        }
        else if (data is IEnumerable<ClipboardItem> clipboardItems)
        {
            _items.AddRange(clipboardItems);
        }
        else if (data is IEnumerable<object> e)
        {
            _items.AddRange(e.Select(o => new ClipboardItem
            {
                Data = o,
            }));
        }
        else
        {
            _items.Add(new ClipboardItem
            {
                Data = data,
            });
        }

        IsCopy = isCopy;
        ExtraInfo = extraInfo;

        EditorServices.SystemLog.AddLog($"Clipboard SetData : [{string.Join(", ", _items.Select(o => o.ToString()))}], IsCopy:{isCopy}");
    }

    /// <inheritdoc/>
    public IEnumerable<ClipboardItem> GetDatas()
    {
        EditorServices.SystemLog.AddLog($"Clipboard GetData : [{string.Join(", ", _items.Select(o => o.ToString()))}], IsCopy:{IsCopy}");

        if (IsCopy)
        {
            try
            {
                foreach (var item in _items)
                {
                    item.Data = Cloner.Clone(item.Data);
                }
            }
            catch (CloneFailedException f)
            {
                f.LogError($"Failed to clone clipboard object");

                return null;
            }
        }
        else
        {
            IsCopy = true;
        }

        return _items;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _items.Clear();
        IsCopy = false;
        ExtraInfo = null;
    }
}
