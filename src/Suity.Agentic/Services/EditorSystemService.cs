using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

public class EditorSystemService : IEditorSystemService
{
    public static readonly EditorSystemService Instance = new();


    public EditorSystemService()
    {
    }

    public IDataInputList CreateDataInputList(ISyncPathObject parent, string propertyName)
    {
        return new DataInputList(parent, propertyName);
    }

    public IDataInputItem CreateDataInputItem(IDataInput dataInput)
    {
        return new DataInput(dataInput);
    }


    public IEditorFileSystemWatcher CreateFileSystemWatcher(string path, object owner = null, bool enableUnwatch = true)
    {
        return new EditorFileSystemWatcher(path, owner, enableUnwatch);
    }

    public IInitialize[] ActivateIInitialize()
    {
        // Exclude editor Assembly
        var myAsm = this.GetType().Assembly;

        var types = typeof(IInitialize).GetDerivedTypes()
            .Where(o => o.Assembly != myAsm)
            .ToArray();

        List<IInitialize> list = [];

        foreach (Type initType in types)
        {
            try
            {
                var init = Activator.CreateInstance(initType) as IInitialize;
                if (init != null)
                {
                    list.Add(init);
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return list.ToArray();
    }


    public Type ResolveType(string typeString, MethodInfo declaringMethod = null)
    {
        return InternalTypeResolve.ResolveType(typeString, declaringMethod);
    }


    public string GenerateRandomId(int length) => IdGenerator.GenerateId(length);


    public string RsaEncrypt(string pubKey, string plainText) => RsaEncryptionHelper.Encrypt(pubKey, plainText);

    public string RsaDecrypt(string privKey, string encryptedText) => RsaEncryptionHelper.Decrypt(privKey, encryptedText);

    public string RsaSign(string privKey, string text) => RsaEncryptionHelper.Sign(privKey, text);

    public bool RsaVerify(string pubKey, string text, string signature) => RsaEncryptionHelper.Verify(pubKey, text, signature);


    public uint ComputeCrc32(byte[] input) => Crc32Algorithm.Compute(input);

    public async Task<string> ExecuteCommandAsync(string command, string? workingDirectory, Action<string>? onOutput, CancellationToken token)
    {
        bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        string shell = isWindows ? "cmd.exe" : "/bin/bash";
        string arguments = isWindows ? $"/C {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

        Encoding outputEncoding;
        if (isWindows)
        {
            int consoleCodePage = GetConsoleCodePage();
            outputEncoding = Encoding.GetEncoding(consoleCodePage);
        }
        else
        {
            outputEncoding = Encoding.UTF8;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = outputEncoding,
            StandardErrorEncoding = outputEncoding,
        };

        startInfo.EnvironmentVariables["FORCE_COLOR"] = "1";
        startInfo.EnvironmentVariables["NPM_CONFIG_COLOR"] = "always";

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                string cleanLine = StripAnsiCodes(e.Data);
                outputBuilder.AppendLine(cleanLine);
                onOutput?.Invoke(cleanLine + Environment.NewLine);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                string cleanLine = StripAnsiCodes(e.Data);
                errorBuilder.AppendLine(cleanLine);
                onOutput?.Invoke("[STDERR] " + cleanLine + Environment.NewLine);
            }
        };

        if (!process.Start())
        {
            return "Failed to start process.";
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            // Asynchronously wait for process to exit, binding cancellation token (including timeout)
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(true); // Recursively kill the process tree
            }
            throw; // Continue throwing to be caught by upper layer
        }

        StringBuilder result = new StringBuilder(outputBuilder.ToString());
        if (errorBuilder.Length > 0)
        {
            if (result.Length > 0) result.AppendLine();
            result.Append("[STDERR]").AppendLine().Append(errorBuilder);
        }

        return result.ToString();
    }

    private static readonly Regex AnsiRegex = new(@"\x1b\[[0-9;]*[a-zA-Z]|\x1b[()][a-zA-Z0-9]|\x1b\][^\x07]*\x07|\x1b\[[0-9;]*[A-Z]", RegexOptions.Compiled);

    private static string StripAnsiCodes(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return AnsiRegex.Replace(text, "");
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetConsoleOutputCP();
    private static int GetConsoleCodePage() => GetConsoleOutputCP();
}
