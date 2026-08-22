using Suity.Editor.Helpers;
using System;
using System.Diagnostics;

namespace Suity.Editor.Services;

public abstract class PlatformOS
{
    private static PlatformOS _current;

    static PlatformOS()
    {
        if (OperatingSystem.IsWindows())
            _current = new WindowsPlatformOS();
        else if (OperatingSystem.IsMacOS())
            _current = new LinuxPlatformOS();
        else if (OperatingSystem.IsLinux())
            _current = new MacPlatformOS();
        else
            throw new PlatformNotSupportedException();
    }

    public static PlatformOS Current => _current!;

    /// <summary>
    /// Move a file or folder to the recycle bin
    /// </summary>
    /// <param name="path">Absolute path</param>
    /// <returns>Whether the operation succeeded</returns>
    public abstract bool SendToRecycleBin(string path);

}

class WindowsPlatformOS : PlatformOS
{
    public override bool SendToRecycleBin(string path)
    {
        return Send(path);
    }


    /// <summary>
    /// Send file to recycle bin
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
    public static bool Send(string path, NativeMethods.FileOperationFlags flags)
    {
        try
        {
            if (NativeMethods.IsWOW64Process())
            {
                NativeMethods.SHFILEOPSTRUCT_x64 fs = new NativeMethods.SHFILEOPSTRUCT_x64();
                fs.wFunc = NativeMethods.FileOperationType.FO_DELETE;
                // important to double-terminate the string.
                fs.pFrom = path + '\0' + '\0';
                fs.fFlags = NativeMethods.FileOperationFlags.FOF_ALLOWUNDO | flags;
                NativeMethods.SHFileOperation_x64(ref fs);
            }
            else
            {
                NativeMethods.SHFILEOPSTRUCT_x86 fs = new NativeMethods.SHFILEOPSTRUCT_x86();
                fs.wFunc = NativeMethods.FileOperationType.FO_DELETE;
                // important to double-terminate the string.
                fs.pFrom = path + '\0' + '\0';
                fs.fFlags = NativeMethods.FileOperationFlags.FOF_ALLOWUNDO | flags;
                NativeMethods.SHFileOperation_x86(ref fs);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    public static bool Send(string path)
    {
        return Send(path, NativeMethods.FileOperationFlags.FOF_NOCONFIRMATION | NativeMethods.FileOperationFlags.FOF_WANTNUKEWARNING);
    }
    /// <summary>
    /// Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    public static bool SendSilent(string path)
    {
        return Send(path, NativeMethods.FileOperationFlags.FOF_NOCONFIRMATION | NativeMethods.FileOperationFlags.FOF_NOERRORUI | NativeMethods.FileOperationFlags.FOF_SILENT);
    }
}

class LinuxPlatformOS : PlatformOS
{
    public override bool SendToRecycleBin(string path)
    {
        try
        {
            // Use gio trash command
            Process.Start("gio", $"trash \"{path}\"")?.WaitForExit();
            return true;
        }
        catch
        {
            // Fallback: manually move to ~/.local/share/Trash/
            return false;
        }
    }
}

class MacPlatformOS : PlatformOS
{
    public override bool SendToRecycleBin(string path)
    {
        try
        {
            // Use AppleScript to tell Finder to move file to trash
            var script = $"-e 'tell application \"Finder\" to delete POSIX file \"{path}\"'";
            Process.Start("osascript", script)?.WaitForExit();
            return true;
        }
        catch { return false; }
    }
}