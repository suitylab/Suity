using System.Diagnostics;
using Suity.Json;

namespace Suity.Editor;

public class CliRunner : IDisposable
{
    private const string MagicPrefix = "[SUITY_CMD]->";

    private readonly Dictionary<string, Action<JsonDataReader>> _callbacks = new();
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

    public void SendCommand(string args, Action<JsonDataReader> response)
    {
        string sid = IdGenerator.GenerateId(10);

        lock (_lock)
        {
            _callbacks[sid] = response;
        }

        SendCommandInternal(args, sid);
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

            Action<JsonDataReader>? callback = null;

            lock (_lock)
            {
                if (_callbacks.Remove(sid, out var found))
                {
                    callback = found;
                }
            }

            callback?.Invoke(reader);
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
