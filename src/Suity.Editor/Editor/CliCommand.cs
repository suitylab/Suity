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
    public CliExceptionObject RemoteException { get; init; }

    public RemoteCliException() { }
    public RemoteCliException(string message) : base(message) { }
    public RemoteCliException(string message, Exception inner) : base(message, inner) { }
}

public class CliExceptionObject : IDataWritable
{
    public Exception Origin { get; }

    public string TypeName { get; private set; }
    public string Message { get; private set; }
    public string StackTrace { get; private set; }

    public CliExceptionObject InnerException { get; private set; }

    public CliExceptionObject()
    {
    }

    public CliExceptionObject(Exception origin)
    {
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        TypeName = origin.GetType().FullName;
        Message = origin.Message;
        StackTrace = origin.StackTrace;

        if (origin.InnerException is { } innerEx)
        {
            InnerException = new CliExceptionObject(innerEx);
        }
    }

    public void WriteData(IDataWriter writer)
    {
        var ex = Origin;

        writer.Node("@type").WriteString("Exception");
        if (ex is null)
        {
            return;
        }

        writer.Node("exception").WriteString(TypeName);
        writer.Node("message").WriteString(Message);
        writer.Node("stackTrace").WriteString(StackTrace);

        if (InnerException is { } innerEx)
        {
            var innerNode = writer.Node("inner");
            innerEx.WriteData(innerNode);
        }
    }

    public void ReadData(IDataReader reader)
    {
        TypeName = reader.Node("exception").ReadString();
        Message = reader.Node("message").ReadString();
        StackTrace = reader.Node("stackTrace").ReadString();

        if (reader.Node("inner") is { } innerNode && 
            innerNode.Node("@type").ReadString() == "Exception")
        {
            InnerException = CliExceptionObject.Create(innerNode);
        }
    }

    public static CliExceptionObject Create(IDataReader reader)
    {
        var ex = new CliExceptionObject();
        ex.ReadData(reader);
        return ex;
    }
}