using System;
using System.Collections.Generic;

namespace Suity.Editor;

public interface ICliArguments
{
    string CommandKey { get; }
    string[] RawArgs { get; }
    IReadOnlyDictionary<string, string> Options { get; }
    int Count { get; }

    string? GetOption(string key, string? defaultValue = null);
    bool HasFlag(string flag);
    string? this[int index] { get; }
}


public abstract class CliCommand
{
    public abstract string Description { get; }

    public virtual string Usage => string.Empty;

    public virtual string DetailedHelp => string.Empty;

    public abstract void DoCommand(ICliArguments args);

    public virtual void ShowHelp()
    {
        Console.WriteLine(Description);
        Console.WriteLine();
        if (!string.IsNullOrEmpty(Usage))
        {
            Console.WriteLine($"Usage: {Usage}");
            Console.WriteLine();
        }
        if (!string.IsNullOrEmpty(DetailedHelp))
        {
            Console.WriteLine(DetailedHelp);
        }
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CliCommandKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
