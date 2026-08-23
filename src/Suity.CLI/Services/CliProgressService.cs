namespace Suity.Editor.Services;

internal class CliProgressService : IProgressService
{
    public static CliProgressService Instance { get; } = new();

    private readonly List<CliProgressItem> _items = [];
    private CliProgressItem? _current;

    public int ProgressCount
    {
        get { lock (_items) { return _items.Count; } }
    }

    #region IProgressService

    public bool ProgressRunning => ProgressCount > 0;

    public void ShowProgressWindow()
    {
    }

    public Task DoProgress(ProgressRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var completion = new TaskCompletionSource<bool>();
        var item = AddProgress(request);
        item.ProgressTask.ContinueWith(_ => completion.SetResult(true));
        return completion.Task;
    }

    public Task[] DoProgress(ProgressRequest[] requests)
    {
        if (requests is null)
            throw new ArgumentNullException(nameof(requests));

        var completions = new TaskCompletionSource<bool>[requests.Length];
        for (int i = 0; i < requests.Length; i++)
        {
            completions[i] = new TaskCompletionSource<bool>();
            var item = AddProgress(requests[i]);
            int index = i;
            item.ProgressTask.ContinueWith(_ => completions[index].SetResult(true));
        }
        return completions.Select(c => c.Task).ToArray();
    }

    #endregion

    private CliProgressItem AddProgress(ProgressRequest request)
    {
        var item = new CliProgressItem(request);
        item.ProgressChanged += Item_ProgressChanged;
        item.ProgressCompleted += Item_ProgressCompleted;

        lock (_items)
        {
            _items.Add(item);
            if (_current is null)
            {
                _current = item;
                PrintProgress(_current);
            }
        }

        item.InternalStartItem();
        return item;
    }

    private void Item_ProgressChanged(object? sender, EventArgs e)
    {
        if (sender is CliProgressItem item && item == _current)
        {
            PrintProgress(item);
        }
    }

    private void Item_ProgressCompleted(object? sender, EventArgs e)
    {
        if (sender is not CliProgressItem item) return;

        item.ProgressChanged -= Item_ProgressChanged;
        item.ProgressCompleted -= Item_ProgressCompleted;
        item.InternalCompleteItem();

        bool currentChanged = false;
        lock (_items)
        {
            _items.Remove(item);
            if (_current == item)
            {
                _current = _items.Count > 0 ? _items[0] : null;
                currentChanged = true;
            }
        }

        if (currentChanged && _current != null)
        {
            PrintProgress(_current);
        }
    }

    private void PrintProgress(CliProgressItem item)
    {
        int count = ProgressCount;
        string countText = count > 1 ? $"[{count} remaining] " : "";
        string text = $"{countText}{item.Request.Title} {item.MainMessage} {item.SubMessage} {item.Percentage}%";
        Console.WriteLine(text);
    }
}

#region CliProgressItem

internal class CliProgressItem : IProgress
{
    public ProgressRequest Request { get; }
    public int Percentage { get; private set; }
    public string MainMessage { get; private set; } = "";
    public string SubMessage { get; private set; } = "";
    public bool IsCompleted { get; private set; }

    public event EventHandler? ProgressChanged;
    public event EventHandler? ProgressCompleted;

    public Task ProgressTask { get; private set; } = Task.CompletedTask;

    public CliProgressItem(ProgressRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    internal void InternalStartItem()
    {
        ProgressTask = Task.Run(() => Request.ProgressAction?.Invoke(this));
        ProgressTask.ContinueWith(_ =>
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
        if (IsCompleted) return;

        Percentage = Math.Clamp(percentage, 0, 100);
        MainMessage = mainMessage ?? "";
        SubMessage = subMessage ?? "";
        ProgressChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateProgess(float rate, string mainMessage, string subMessage)
    {
        UpdateProgess((int)(rate * 100), mainMessage, subMessage);
    }

    public void UpdateProgess(int index, int count, string mainMessage, string subMessage)
    {
        if (count <= 0)
            UpdateProgess(0, mainMessage, subMessage);
        else
            UpdateProgess((int)((float)index / count * 100), mainMessage, subMessage);
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
