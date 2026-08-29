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

    public async Task<JsonDataReader> StartAsync(string args)
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

        ThrowExceptionFromReader(reader);

        return reader;
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

    public async Task<JsonDataReader> SendCommandAsync(string args)
    {
        string sid = IdGenerator.GenerateId(10);
        var tcs = new TaskCompletionSource<JsonDataReader>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            _asyncCallbacks[sid] = tcs;
        }

        SendCommandInternal(args, sid);

        var reader = await tcs.Task;

        ThrowExceptionFromReader(reader);

        return reader;
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
            string base64 = line[MagicPrefix.Length..];
            byte[] bytes = Convert.FromBase64String(base64);
            string json = System.Text.Encoding.UTF8.GetString(bytes);

            var reader = new JsonDataReader(json);
            string? sid = reader.Node("sid").ReadString();

            if (string.IsNullOrEmpty(sid))
                return;

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

    private void ThrowExceptionFromReader(JsonDataReader reader)
    {
        if (reader is null)
        {
            throw new InvalidOperationException("Failed to receive response from the CLI process. (reader is null)");
        }

        string type = reader.Node("@type").ReadString();
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new InvalidOperationException("Failed to receive response from the CLI process. (no type)");
        }

        if (type == "Exception")
        {
            string typeName = reader.Node("exception").ReadString();
            string message = reader.Node("message").ReadString();
            string stackTrace = reader.Node("stackTrace").ReadString();

            var remoteException = new RemoteCliException(message) 
            {
                RemoteTypeName = typeName,
                RemoteStackTrace = stackTrace,
            };

            throw remoteException;
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
