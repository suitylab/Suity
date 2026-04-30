using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

internal class AvaProgressService : IProgressService
{
    public const bool AlwaysSplashForm = false;

    public static readonly AvaProgressService Instance = new();

    public bool ProgressRunning => SplashWindow.CurrentForm?.ProgressCount > 0 || StatusBarProgress.ProgressCount > 0;

    public void ShowProgressWindow()
    {
        SplashWindow.ShowWindow(SuityApp.Instance.Window);
    }

    public Task DoProgress(ProgressRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var completion = new TaskCompletionSource<bool>();

        QueuedAction.Do(() =>
        {
            ProgressItem item;
            if (SuityApp.Instance.Window is SplashWindow || SuityApp.Instance.Window is null || AlwaysSplashForm)
            {
                item = SplashWindow.AddCurrentProgress(request, SuityApp.Instance.Window);
            }
            else
            {
                item = StatusBarProgress.AddCurrentProgress(request);
            }

            if (item is null)
            {
                return;
            }

            MakeCompletionResult(request, completion, item);
        });

        return completion.Task;
    }
        

    public Task[] DoProgress(ProgressRequest[] requests)
    {
        var completions = new TaskCompletionSource<bool>[requests.Length];

        for (int i = 0; i < requests.Length; i++)
        {
            completions[i] = new TaskCompletionSource<bool>();
        }

        QueuedAction.Do(() =>
        {
            ProgressItem[] items;

            if (SuityApp.Instance.Window is SplashWindow || SuityApp.Instance.Window is null || AlwaysSplashForm)
            {
                items = SplashWindow.AddCurrentProgress(requests, SuityApp.Instance.Window);
            }
            else
            {
                items = StatusBarProgress.AddCurrentProgress(requests);
            }

            for (int i = 0; i < requests.Length; i++)
            {
                MakeCompletionResult(requests[i], completions[i], items[i]);
            }
        });

        return completions.Select(o => o.Task).ToArray();
    }

    private void MakeCompletionResult(ProgressRequest request, TaskCompletionSource<bool> completion, ProgressItem item)
    {
        item.ProgressTask.ContinueWith(t => completion.SetResult(true));
    }
}

#region StatusBarProgress

internal static class StatusBarProgress
{
    private static readonly List<ProgressItem> _items = [];
    private static ProgressItem _current;

    public static int ProgressCount => _items.Count;

    public static ProgressItem AddCurrentProgress(ProgressRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return AddProgress(request);
    }

    public static ProgressItem[] AddCurrentProgress(ProgressRequest[] requests)
    {
        if (requests is null)
        {
            throw new ArgumentNullException(nameof(requests));
        }

        ProgressItem[] items = new ProgressItem[requests.Length];
        for (int i = 0; i < requests.Length; i++)
        {
            items[i] = AddProgress(requests[i]);
        }

        return items;
    }

    private static ProgressItem AddProgress(ProgressRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var item = new ProgressItem(request);
        item.ProgressChanged += Item_ProgressChanged;
        item.ProgressCompleted += Item_ProgressCompleted;

        lock (_items)
        {
            _items.Add(item);

            if (_current is null)
            {
                _current = item;
                UpdateProgress(_current);
            }
        }

        item.InternalStartItem();

        return item;
    }

    private static void Item_ProgressChanged(object sender, EventArgs e)
    {
        ProgressItem item = (ProgressItem)sender;
        UpdateProgress(item);
    }

    private static void Item_ProgressCompleted(object sender, EventArgs e)
    {
        ProgressItem item = (ProgressItem)sender;
        CompleteProgress(item);
    }

    private static void UpdateProgress(ProgressItem item)
    {
        if (item != _current)
        {
            return;
        }

        string countText;

        if (_items.Count > 1)
        {
            countText = $"{_items.Count} items remaining ";
        }
        else
        {
            countText = string.Empty;
        }

        string text = $"{countText}{item.Request.Title} {item.MainMessage} {item.SubMessage} {item.Percentage}%";

        EditorRexes.ShowStatusText.Invoke(text);
    }

    private static void CompleteProgress(ProgressItem item)
    {
        item.ProgressChanged -= Item_ProgressChanged;
        item.ProgressCompleted -= Item_ProgressCompleted;
        item.InternalCompleteItem();

        int itemCount;
        bool currentChanged = false;

        lock (_items)
        {
            _items.Remove(item);

            itemCount = _items.Count;

            if (_current == item)
            {
                _current = _items.Count > 0 ? _items[0] : null;
                currentChanged = true;
            }
        }

        if (currentChanged && _current != null)
        {
            UpdateProgress(_current);
        }

        if (itemCount == 0)
        {
            EditorRexes.ShowStatusText.Invoke(string.Empty);
        }
    }
}

#endregion

#region IProgress

public class ProgressItem : IProgress
{
    public ProgressRequest Request { get; }

    public int Percentage { get; private set; }
    public string MainMessage { get; private set; }
    public string SubMessage { get; private set; }
    public bool IsCompleted { get; private set; }

    public event EventHandler ProgressChanged;

    public event EventHandler ProgressCompleted;

    public Task ProgressTask { get; private set; }

    public ProgressItem(ProgressRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    internal void InternalStartItem()
    {
        if (ProgressTask != null)
        {
            return;
        }

        ProgressTask = Task.Run(() =>
        {
            Request.ProgressAction?.Invoke(this);
        });
        ProgressTask.ContinueWith(t =>
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                ProgressCompleted?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    internal void InternalCompleteItem()
    {
        Request.FinishedAction?.Invoke();
    }

    #region IProgress

    public void UpdateProgess(int percentage, string mainMessage, string subMessage)
    {
        if (IsCompleted)
        {
            return;
        }

        if (percentage < 0)
        {
            percentage = 0;
        }

        if (percentage > 100)
        {
            percentage = 100;
        }

        Percentage = percentage;
        MainMessage = mainMessage;
        SubMessage = subMessage;

        ProgressChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateProgess(float rate, string mainMessage, string subMessage)
    {
        UpdateProgess((int)(rate * 100), mainMessage, subMessage);
    }

    public void UpdateProgess(int index, int count, string mainMessage, string subMessage)
    {
        if (count <= 0)
        {
            UpdateProgess(0, mainMessage, subMessage);
        }
        else
        {
            float rate = (float)index / (float)count;
            UpdateProgess((int)(rate * 100), mainMessage, subMessage);
        }
    }

    public void CompleteProgess()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            ProgressCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion
}

#endregion