using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Services;

public interface IPlatformFileSystem
{
    object Owner { get; }

    event EventHandler<PlatformFileUpdateEventArgs> FileWrite;

    void WriteAllText(string relativePath, string content);

    void WriteAllLines(string relativePath, IEnumerable<string> lines);

    void WriteAllBytes(string relativePath, byte[] bytes);

    void WriteStreamWriter(string relativePath, Action<Stream> writer);
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

    public event EventHandler<PlatformFileUpdateEventArgs> FileWrite;

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
        OnFileWrite(relativePath, fullPath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllLines(fullPath, lines);
        OnFileWrite(relativePath, fullPath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllText(fullPath, content);
        OnFileWrite(relativePath, fullPath);
    }

    public virtual void WriteStreamWriter(string relativePath, Action<Stream> writer)
    {
        string fullPath = GetFullPath(relativePath);
        EnsureDirectory(fullPath);

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            writer(stream);
        }

        OnFileWrite(relativePath, fullPath);
    }


    protected string GetBasePath() => _basePathGetter?.Invoke() ?? _basePath;

    protected string GetFullPath(string relativePath)
    {
        string basePath = _basePathGetter?.Invoke() ?? _basePath;
        return Path.Combine(basePath, relativePath);
    }

    protected virtual void OnFileWrite(string relativePath, string fullPath)
    {
        FileWrite?.Invoke(this, new PlatformFileUpdateEventArgs(relativePath, fullPath));
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

    public event EventHandler<PlatformFileUpdateEventArgs> FileWrite;


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
        OnFileWrite(relativePath, scopedPath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteAllLines(scopedPath, lines);
        OnFileWrite(relativePath, scopedPath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteAllText(scopedPath, content);
        OnFileWrite(relativePath, scopedPath);
    }

    public virtual void WriteStreamWriter(string relativePath, Action<Stream> writer)
    {
        string scopedPath = MakeScopedPath(relativePath);
        _parent.WriteStreamWriter(scopedPath, writer);
        OnFileWrite(relativePath, scopedPath);
    }

    protected string GetSubDirectory() => _subDirectoryGetter?.Invoke() ?? _subDirectory;

    protected string MakeScopedPath(string relativePath)
    {
        string subDir = _subDirectoryGetter?.Invoke() ?? _subDirectory;
        return Path.Combine(subDir, relativePath);
    }

    protected virtual void OnFileWrite(string relativePath, string scopedPath)
    {
        FileWrite?.Invoke(this, new PlatformFileUpdateEventArgs(relativePath, scopedPath));
    }
}