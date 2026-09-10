using System;
using System.Collections.Generic;
using System.IO;
using Suity.Helpers;

namespace Suity.Editor.Services;

public interface IPlatformFileSystem
{
    object Owner { get; }

    event EventHandler<PlatformFileUpdateEventArgs> FileUpdated;

    void WriteAllText(string relativePath, string content);

    void WriteAllLines(string relativePath, IEnumerable<string> lines);

    void WriteAllBytes(string relativePath, byte[] bytes);

    void WriteStreamWriter(string relativePath, Action<Stream> writer);

    void MoveFile(string relativePath, string newName);

    void DeleteFile(string relativePath);

    void MoveDirectory(string relativePath, string newName);

    void DeleteDirectory(string relativePath);
}

public class PlatformFileUpdateEventArgs : EventArgs
{
    public string RelativePath { get; }
    public string ScopedPath { get; }
    public PlatformFileUpdateEventArgs(string relativePath, string scopedPath)
    {
        RelativePath = relativePath;
        ScopedPath = scopedPath;
    }
}


public class PlatformFileSystem : IPlatformFileSystem
{
    readonly string _basePath;
    private readonly Func<string> _basePathGetter;

    public object Owner { get; }

    public event EventHandler<PlatformFileUpdateEventArgs> FileUpdated;

    public PlatformFileSystem(string basePath, object owner = null)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        Owner = owner;
    }

    public PlatformFileSystem(Func<string> basePathGetter, object owner = null)
    {
        _basePathGetter = basePathGetter ?? throw new ArgumentNullException(nameof(basePathGetter));
        Owner = owner;
    }

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllBytes(fullPath, bytes);
        OnFileUpdted(relativePath, fullPath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllLines(fullPath, lines);
        OnFileUpdted(relativePath, fullPath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllText(fullPath, content);
        OnFileUpdted(relativePath, fullPath);
    }

    public virtual void WriteStreamWriter(string relativePath, Action<Stream> writer)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            writer(stream);
        }

        OnFileUpdted(relativePath, fullPath);
    }

    public virtual void MoveFile(string relativePath, string newName)
    {
        string fullPath = GetFullPath(relativePath);
        string newFullPath = GetFullPath(newName);
        EnsureDirectory(newFullPath);


        File.Move(fullPath, newFullPath);

        OnFileUpdted(relativePath, newFullPath);
        OnFileUpdted(newName, newFullPath);
    }

    public virtual void DeleteFile(string relativePath)
    {
        string fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            return;
        }

        File.Delete(fullPath);
        OnFileUpdted(relativePath, fullPath);
    }

    public virtual void MoveDirectory(string relativePath, string newName)
    {
        string fullPath = GetFullPath(relativePath);
        string newFullPath = GetFullPath(newName);
        EnsureDirectory(newFullPath);

        string basePath = GetBasePath();

        // Collect the events for every file in the directory before the move.
        var beforeEvents = new List<PlatformFileUpdateEventArgs>();
        if (Directory.Exists(fullPath))
        {
            foreach (var file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
            {
                beforeEvents.Add(new PlatformFileUpdateEventArgs(file.MakeRelativePath(basePath), file));
            }
        }

        Directory.Move(fullPath, newFullPath);

        // Collect the events for every file in the moved directory.
        var afterEvents = new List<PlatformFileUpdateEventArgs>();
        if (Directory.Exists(newFullPath))
        {
            foreach (var file in Directory.GetFiles(newFullPath, "*", SearchOption.AllDirectories))
            {
                afterEvents.Add(new PlatformFileUpdateEventArgs(file.MakeRelativePath(basePath), file));
            }
        }

        // Send all the events after the move is completed, so the old paths read as deleted.
        foreach (var e in beforeEvents)
        {
            OnFileUpdted(e.RelativePath, e.ScopedPath);
        }

        foreach (var e in afterEvents)
        {
            OnFileUpdted(e.RelativePath, e.ScopedPath);
        }
    }

    public virtual void DeleteDirectory(string relativePath)
    {
        string fullPath = GetFullPath(relativePath);
        if (!Directory.Exists(fullPath))
        {
            return;
        }

        string basePath = GetBasePath();

        // Collect the events for every file in the directory before deleting it.
        var events = new List<PlatformFileUpdateEventArgs>();
        foreach (var file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            events.Add(new PlatformFileUpdateEventArgs(file.MakeRelativePath(basePath), file));
        }

        Directory.Delete(fullPath, true);

        // Send all the collected events after the directory and its contents are removed.
        foreach (var e in events)
        {
            OnFileUpdted(e.RelativePath, e.ScopedPath);
        }
    }

    protected string GetBasePath() => _basePathGetter?.Invoke() ?? _basePath;

    protected string GetFullPath(string relativePath)
    {
        string basePath = _basePathGetter?.Invoke() ?? _basePath;
        return Path.Combine(basePath, relativePath);
    }

    protected virtual void OnFileUpdted(string relativePath, string fullPath)
    {
        FileUpdated?.Invoke(this, new PlatformFileUpdateEventArgs(relativePath, fullPath));
    }

    public static void EnsureDirectory(string fullPath)
    {
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

}

public class ProjectFileSystem : PlatformFileSystem
{
    private readonly Project _project;

    public Project Project => _project;

    public ProjectFileSystem(Project project)
        : base(() => project.ProjectBasePath, project)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
    }
}

public class ScopedFileSystem : IPlatformFileSystem
{
    private readonly IPlatformFileSystem _parent;
    private readonly string _subDirectory;
    private readonly Func<string> _subDirectoryGetter;

    public object Owner { get; }

    public event EventHandler<PlatformFileUpdateEventArgs> FileUpdated;


    public ScopedFileSystem(IPlatformFileSystem parent, string subDirectory, object owner = null)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _subDirectory = subDirectory ?? throw new ArgumentNullException(nameof(subDirectory));
        Owner = owner;
    }

    public ScopedFileSystem(IPlatformFileSystem parent, Func<string> subDirectoryGetter, object owner = null)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _subDirectoryGetter = subDirectoryGetter ?? throw new ArgumentNullException(nameof(subDirectoryGetter));
        Owner = owner;
    }

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteAllBytes(scopedPath, bytes);
        OnFileUpdated(relativePath, scopedPath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteAllLines(scopedPath, lines);
        OnFileUpdated(relativePath, scopedPath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteAllText(scopedPath, content);
        OnFileUpdated(relativePath, scopedPath);
    }

    public virtual void WriteStreamWriter(string relativePath, Action<Stream> writer)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteStreamWriter(scopedPath, writer);
        OnFileUpdated(relativePath, scopedPath);
    }

    public virtual void MoveFile(string relativePath, string newName)
    {
        string scopedPath = MakeScopedPath(relativePath);
        string newScopedPath = MakeScopedPath(newName);
        _parent.MoveFile(scopedPath, newScopedPath);
        OnFileUpdated(relativePath, scopedPath);
        OnFileUpdated(newName, newScopedPath);
    }

    public virtual void DeleteFile(string relativePath)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.DeleteFile(scopedPath);
        OnFileUpdated(relativePath, scopedPath);
    }

    public virtual void MoveDirectory(string relativePath, string newName)
    {
        string scopedPath = MakeScopedPath(relativePath);
        string newScopedPath = MakeScopedPath(newName);

        // Collect the parent events for every affected file, split into the
        // before-move (old paths) and after-move (new paths) lists.
        var beforeEvents = new List<PlatformFileUpdateEventArgs>();
        var afterEvents = new List<PlatformFileUpdateEventArgs>();

        EventHandler<PlatformFileUpdateEventArgs> collector = (s, e) =>
        {
            if (IsUnderPath(e.RelativePath, newScopedPath))
            {
                afterEvents.Add(e);
            }
            else
            {
                beforeEvents.Add(e);
            }
        };

        _parent.FileUpdated += collector;
        try
        {
            _parent.MoveDirectory(scopedPath, newScopedPath);
        }
        finally
        {
            _parent.FileUpdated -= collector;
        }

        string subDir = GetSubDirectory();

        foreach (var e in beforeEvents)
        {
            RaiseScopedEvent(subDir, e);
        }

        foreach (var e in afterEvents)
        {
            RaiseScopedEvent(subDir, e);
        }
    }

    public virtual void DeleteDirectory(string relativePath)
    {
        string scopedPath = MakeScopedPath(relativePath);

        // Collect the parent events for every file in the directory.
        var events = new List<PlatformFileUpdateEventArgs>();
        EventHandler<PlatformFileUpdateEventArgs> collector = (s, e) => events.Add(e);

        _parent.FileUpdated += collector;
        try
        {
            _parent.DeleteDirectory(scopedPath);
        }
        finally
        {
            _parent.FileUpdated -= collector;
        }

        string subDir = GetSubDirectory();
        foreach (var e in events)
        {
            RaiseScopedEvent(subDir, e);
        }
    }

    private void RaiseScopedEvent(string subDir, PlatformFileUpdateEventArgs e)
    {
        string scopedRelative = e.RelativePath.MakeRelativePath(subDir);
        OnFileUpdated(scopedRelative, e.RelativePath);
    }

    private static bool IsUnderPath(string path, string prefix)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(prefix))
        {
            return false;
        }

        string normalizedPath = path.Replace('\\', '/').TrimEnd('/');
        string normalizedPrefix = prefix.Replace('\\', '/').TrimEnd('/');

        return normalizedPath.StartsWith(normalizedPrefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    protected string GetSubDirectory() => _subDirectoryGetter?.Invoke() ?? _subDirectory;

    protected string MakeScopedPath(string relativePath)
    {
        string subDir = _subDirectoryGetter?.Invoke() ?? _subDirectory;
        return Path.Combine(subDir, relativePath);
    }

    protected virtual void OnFileUpdated(string relativePath, string scopedPath)
    {
        FileUpdated?.Invoke(this, new PlatformFileUpdateEventArgs(relativePath, scopedPath));
    }


}