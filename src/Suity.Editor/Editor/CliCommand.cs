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

    public abstract object DoCommand(ICliArguments args);

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

public class CliException : Exception
{
    public CliException(string message) : base(message) { }
    public CliException(string message, Exception innerException) : base(message, innerException) { }
}


[Serializable]
public class RemoteCliException : Exception
{
    public string RemoteTypeName { get; init; }
    public string RemoteStackTrace { get; init; }

    public RemoteCliException() { }
    public RemoteCliException(string message) : base(message) { }
    public RemoteCliException(string message, Exception inner) : base(message, inner) { }
}

public class CliStringArray : IDataWritable
{
    public string[] Strings { get; init; }

    public void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("StringArray");
        var aryWriter = writer.Nodes("Strings", Strings.Length);
        foreach (var str in Strings)
        {
            var strWriter = aryWriter.Item();
            strWriter.WriteString(str);
        }
    }

    public override string ToString()
    {
        return string.Join("\r\n", Strings);
    }
}