using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Services;

public interface IPlatformFileSystem
{
    void WriteAllText(string relativePath, string content);

    void WriteAllLines(string relativePath, IEnumerable<string> lines);

    void WriteAllBytes(string relativePath, byte[] bytes);

    IPlatformFileSystem CreateScoped(string subDirectory);
}

public class PlatformFileSystem : IPlatformFileSystem
{
    readonly string _basePath;

    public PlatformFileSystem(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
    }

    public IPlatformFileSystem CreateScoped(string subDirectory) =>
        new ScopedFileSystem(this, subDirectory);

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        string fullPath = Path.Combine(_basePath, relativePath);
        File.WriteAllBytes(fullPath, bytes);
        OnFileWrite(relativePath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string fullPath = Path.Combine(_basePath, relativePath);
        File.WriteAllLines(fullPath, lines);
        OnFileWrite(relativePath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string fullPath = Path.Combine(_basePath, relativePath);
        File.WriteAllText(fullPath, content);
        OnFileWrite(relativePath);
    }

    protected virtual void OnFileWrite(string relativePath)
    {
    }
}

public class ProjectFileSystem : IPlatformFileSystem
{
    public static ProjectFileSystem Current { get; } = new();

    public ProjectFileSystem()
    {
    }

    public IPlatformFileSystem CreateScoped(string subDirectory) =>
        new ScopedFileSystem(this, subDirectory);

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        if (Project.Current is { } project)
        {
            string fullPath = Path.Combine(project.ProjectBasePath, relativePath);
            File.WriteAllBytes(fullPath, bytes);
            OnFileWrite(relativePath);
        }
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        if (Project.Current is { } project)
        {
            string fullPath = Path.Combine(project.ProjectBasePath, relativePath);
            File.WriteAllLines(fullPath, lines);
            OnFileWrite(relativePath);
        }
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        if (Project.Current is { } project)
        {
            string fullPath = Path.Combine(project.ProjectBasePath, relativePath);
            File.WriteAllText(fullPath, content);
            OnFileWrite(relativePath);
        }
    }

    protected virtual void OnFileWrite(string relativePath)
    {
    }
}

public class ScopedFileSystem : IPlatformFileSystem
{
    private readonly IPlatformFileSystem _parent;
    private readonly string _subDirectory;

    public ScopedFileSystem(IPlatformFileSystem parent, string subDirectory)
    {
        _parent = parent;
        _subDirectory = subDirectory;
    }

    public virtual void WriteAllBytes(string relativePath, byte[] bytes)
    {
        string scopedPath = Path.Combine(_subDirectory, relativePath);
        _parent.WriteAllBytes(scopedPath, bytes);
        OnFileWrite(relativePath);
    }

    public virtual void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        string scopedPath = Path.Combine(_subDirectory, relativePath);
        _parent.WriteAllLines(scopedPath, lines);
        OnFileWrite(relativePath);
    }

    public virtual void WriteAllText(string relativePath, string content)
    {
        string scopedPath = Path.Combine(_subDirectory, relativePath);
        _parent.WriteAllText(scopedPath, content);
        OnFileWrite(relativePath);
    }

    public IPlatformFileSystem CreateScoped(string subDirectory) =>
        new ScopedFileSystem(this, subDirectory);

    protected virtual void OnFileWrite(string relativePath)
    {
    }
}