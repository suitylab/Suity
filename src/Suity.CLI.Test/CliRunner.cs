using System.Diagnostics;
using Suity.Json;

namespace Suity.Editor;

public class CliRunner : IDisposable
{
    private const string MagicPrefix = "[SUITY_CMD]->";

    private readonly Dictionary<string, Action<JsonDataReader>> _callbacks = new();
    private readonly Dictionary<string, TaskCompletionSource<JsonDataReader>> _asyncCallbacks = new();
    private readonly object _lock = new();
    private Process? _process;
    private CancellationTokenSource? _cts;
    private Task? _readTask;

    public event Action<object>? NotificationMessage;

    public string CliPath { get; }
    public bool IsRunning => _process != null && !_process.HasExited;

    public CliRunner(string cliPath)
    {
        CliPath = cliPath ?? throw new ArgumentNullException(nameof(cliPath));
    }

    public void Start(string args, Action<JsonDataReader>? response = null)
    {
        if (_process != null)
        {
            throw new InvalidOperationException("Process is already started.");
        }

        var cts = new CancellationTokenSource();
        _cts = cts;

        string? sid = null;
        if (response != null)
        {
            sid = IdGenerator.GenerateId(10);
            lock (_lock)
            {
                _callbacks[sid] = response;
            }
            args = $"{args} --sid {sid}";
        }

        var process = new Process();
        process.StartInfo.FileName = CliPath;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        _process = process;

        _readTask = Task.Run(() => Running(cts.Token));
    }

    public async Task<object> StartAsync(string args)
    {
        if (_process != null)
        {
            throw new InvalidOperationException("Process is already started.");
        }

        var cts = new CancellationTokenSource();
        _cts = cts;

        string sid = IdGenerator.GenerateId(10);
        var tcs = new TaskCompletionSource<JsonDataReader>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            _asyncCallbacks[sid] = tcs;
        }

        var process = new Process();
        process.StartInfo.FileName = CliPath;
        process.StartInfo.Arguments = $"{args} --sid {sid}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        _process = process;

        _readTask = Task.Run(() => Running(cts.Token));

        var reader = await tcs.Task;

        return CliMagicLine.ParseReader(reader);
    }

    public void SendCommand(string args, Action<JsonDataReader> response)
    {
        string sid = IdGenerator.GenerateId(10);

        lock (_lock)
        {
            _callbacks[sid] = response;
        }

        SendCommandInternal(args, sid);
    }

    public async Task<object> SendCommandAsync(string args)
    {
        string sid = IdGenerator.GenerateId(10);
        var tcs = new TaskCompletionSource<JsonDataReader>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            _asyncCallbacks[sid] = tcs;
        }

        SendCommandInternal(args, sid);

        var reader = await tcs.Task;

        return CliMagicLine.ParseReader(reader);
    }

    private void SendCommandInternal(string args, string sid)
    {
        var process = _process ?? throw new InvalidOperationException("Process is not started.");

        string line = $"{args} --sid {sid}";
        process.StandardInput.WriteLine(line);
        process.StandardInput.Flush();
    }

    private async Task Running(CancellationToken cancellationToken)
    {
        var process = _process ?? throw new InvalidOperationException("Process is not started.");

        try
        {
            while (!cancellationToken.IsCancellationRequested && !process.HasExited)
            {
                string? line = await process.StandardOutput.ReadLineAsync(cancellationToken);

                if (line == null)
                    break;

                if (line.StartsWith(MagicPrefix))
                {
                    HandleMagicLine(line);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void HandleMagicLine(string line)
    {
        try
        {
            var reader = CliMagicLine.ParseMagicLine(line);
            string? sid = reader.GetSessionId();

            if (string.IsNullOrEmpty(sid))
            {
                NotificationMessage?.Invoke(CliMagicLine.ParseReader(reader));
                return;
            }

            lock (_lock)
            {
                if (_asyncCallbacks.Remove(sid, out var tcs))
                {
                    tcs.TrySetResult(reader);
                    return;
                }

                if (_callbacks.Remove(sid, out var callback))
                {
                    callback.Invoke(reader);
                }
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        try
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }
        catch
        {
        }

        _process?.Dispose();
        _process = null;
    }
}
