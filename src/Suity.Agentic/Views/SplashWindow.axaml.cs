using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Suity.Editor.Services;
using System;
using System.Collections.Generic;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor;

public partial class SplashWindow : Window
{
    readonly List<ProgressItem> _items = [];
    ProgressItem _current;

    bool _started;
    bool _closed;

    public SplashWindow()
    {
        InitializeComponent();
    }

    public ProgressItem? AddProgress(ProgressRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        bool isMainThread = Dispatcher.UIThread.CheckAccess();
        if (!isMainThread)
        {
            throw new InvalidOperationException("Must invoke in main thread.");
        }

        if (_closed)
        {
            return null;
        }

        var item = new ProgressItem(request);
        item.ProgressChanged += Item_ProgressChanged;
        item.ProgressCompleted += Item_ProgressCompleted;

        _items.Add(item);

        if (_current == null)
        {
            _current = item;
            UpdateProgress(_current);
        }

        item.InternalStartItem();

        return item;
    }

    public int ProgressCount => _items.Count;

    private void Item_ProgressChanged(object? sender, EventArgs e)
    {
        ProgressItem item = (ProgressItem)sender;

        bool isMainThread = Dispatcher.UIThread.CheckAccess();
        if (!isMainThread)
        {
            Dispatcher.UIThread.Post(() => UpdateProgress(item));
        }
        else
        {
            UpdateProgress(item);
        }
    }
    private void Item_ProgressCompleted(object? sender, EventArgs e)
    {
        ProgressItem item = (ProgressItem)sender;

        bool isMainThread = Dispatcher.UIThread.CheckAccess();
        if (!isMainThread)
        {
            Dispatcher.UIThread.Post(() => CompleteProgress(item));
        }
        else
        {
            CompleteProgress(item);
        }
    }

    private void UpdateProgress(ProgressItem item)
    {
        if (item != _current)
        {
            return;
        }

        this.StatusText.IsVisible = true;
        this.LoadProgressBar.IsVisible = true;

        this.Title = item.Request.Title ?? string.Empty;
        this.LoadProgressBar.Value = item.Percentage;

        string str = item.MainMessage ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(item.SubMessage))
        {
            str += " " + item.SubMessage;
        }

        if (_items.Count > 1)
        {
            str += " " + L($"{_items.Count} items remaining");
        }

        StatusText.Text = str;

        //this.Update();
    }
    private void CompleteProgress(ProgressItem item)
    {
        item.ProgressChanged -= Item_ProgressChanged;
        item.ProgressCompleted -= Item_ProgressCompleted;

        item.InternalCompleteItem();

        _items.Remove(item);

        if (_items.Count == 0)
        {
            try
            {
                //Close();
            }
            catch (Exception err)
            {
            }

            return;
        }


        if (_current == item)
        {
            _current = _items[0];
        }

        UpdateProgress(_current);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _started = true;
        if (_current != null)
        {
            UpdateProgress(_current);
        }
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_items.Count > 0)
        {
            e.Cancel = true;
        }
        else
        {
            _closed = true;
            DetachCurrentForm(this);
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _closed = true;
        DetachCurrentForm(this);
    }

    #region Static

    public static SplashWindow? CurrentForm { get; private set; }
    private static object _currentFormSync = new();

    public static void ShowWindow(Window? parentForm = null)
    {
        lock (_currentFormSync)
        {
            if (CurrentForm == null)
            {
                CurrentForm = new SplashWindow();
                if (parentForm != null)
                {
                    CurrentForm.ShowDialog(parentForm);
                }
                else if (SuityApp.Instance.Window is null)
                {
                    CurrentForm.Show();
                    SuityApp.Instance.Window = CurrentForm;
                }
                else
                {
                    CurrentForm.Show();
                }
            }
        }
    }
    public static ProgressItem AddCurrentProgress(ProgressRequest request, Window? parentForm = null)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        lock (_currentFormSync)
        {
            ProgressItem item;
            if (CurrentForm != null)
            {
                item = CurrentForm.AddProgress(request);
                if (item != null)
                {
                    return item;
                }
            }

            CurrentForm = new SplashWindow();
            item = CurrentForm.AddProgress(request);

            try
            {
                if (parentForm != null)
                {
                    CurrentForm.ShowDialog(parentForm);
                }
                else if (SuityApp.Instance.Window is null)
                {
                    CurrentForm.Show();
                    SuityApp.Instance.Window = CurrentForm;
                }
                else
                {
                    CurrentForm.Show();
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }

            return item;
        }
    }
    public static ProgressItem[] AddCurrentProgress(ProgressRequest[] requests, Window? parentForm = null)
    {
        if (requests is null)
        {
            throw new ArgumentNullException(nameof(requests));
        }

        lock (_currentFormSync)
        {
            ProgressItem[] items = new ProgressItem[requests.Length];
            if (CurrentForm is null)
            {
                CurrentForm = new SplashWindow();
            }

            for (int i = 0; i < requests.Length; i++)
            {
                items[i] = CurrentForm.AddProgress(requests[i]);
            }

            // Task may have completed immediately
            if (CurrentForm._items.Count > 0)
            {
                if (parentForm != null)
                {
                    CurrentForm.ShowDialog(parentForm);
                }
                else if (SuityApp.Instance.Window is null)
                {
                    CurrentForm.Show();
                    SuityApp.Instance.Window = CurrentForm;
                }
                else
                {
                    CurrentForm.Show();
                }
            }

            return items;
        }
    }

    private static void DetachCurrentForm(SplashWindow progressForm)
    {
        lock (_currentFormSync)
        {
            if (CurrentForm == progressForm)
            {
                CurrentForm = null;
            }
        }
    }
    #endregion
}