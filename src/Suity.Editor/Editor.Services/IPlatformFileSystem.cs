using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Services;

public interface IPlatformFileSystem
{
    void WriteAllText(string relativePath, string content);

    void WriteAllLines(string relativePath, IEnumerable<string> lines);

    void WriteAllBytes(string relativePath, byte[] bytes);
}

public class PlatformFileSystem : IPlatformFileSystem
{
    readonly string _basePath;
    private readonly Func<string> _basePathGetter;

    public PlatformFileSystem(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
    }

    public PlatformFileSystem(Func<string> basePathGetter)
    {
        _basePathGetter = basePathGetter ?? throw new ArgumentNullException(nameof(basePathGetter));
    }

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        string fullPath = GetFullPath(relativePath);
        File.WriteAllBytes(fullPath, bytes);
        OnFileWrite(relativePath, fullPath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string fullPath = GetFullPath(relativePath);
        File.WriteAllLines(fullPath, lines);
        OnFileWrite(relativePath, fullPath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string fullPath = GetFullPath(relativePath);
        File.WriteAllText(fullPath, content);
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
    }
}

public class ProjectFileSystem : PlatformFileSystem
{
    private readonly Project _project;

    public Project Project => _project;

    public ProjectFileSystem(Project project)
        : base(() => project.ProjectBasePath)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
    }
}

public class ScopedFileSystem : IPlatformFileSystem
{
    private readonly IPlatformFileSystem _parent;
    private readonly string _subDirectory;
    private readonly Func<string> _subDirectoryGetter;

    public ScopedFileSystem(IPlatformFileSystem parent, string subDirectory)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _subDirectory = subDirectory ?? throw new ArgumentNullException(nameof(subDirectory));
    }

    public ScopedFileSystem(IPlatformFileSystem parent, Func<string> subDirectoryGetter)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _subDirectoryGetter = subDirectoryGetter ?? throw new ArgumentNullException(nameof(subDirectoryGetter));
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


    protected string GetSubDirectory() => _subDirectoryGetter?.Invoke() ?? _subDirectory;

    protected string MakeScopedPath(string relativePath)
    {
        string subDir = _subDirectoryGetter?.Invoke() ?? _subDirectory;
        return Path.Combine(subDir, relativePath);
    }

    protected virtual void OnFileWrite(string relativePath, string scopedPath)
    {
    }
}