using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Suity.Editor.Services;

/// <summary>
/// Enumeration defining different categories for editor logs
/// </summary>
public enum EditorLogCategory
{
    Core,      // Core system related logs
    Editor,    // Editor specific functionality logs
    Runtime    // Runtime execution logs
}


/// <summary>
/// Interface for system logging functionality
/// </summary>
public interface ISystemLog
{
    /// <summary>
    /// Adds a log message to the system log
    /// </summary>
    /// <param name="message">The message to be logged</param>
    void AddLog(object message);

    /// <summary>
    /// Increases the indentation level for subsequent log messages
    /// </summary>
    void PushIndent();

    /// <summary>
    /// Decreases the indentation level for subsequent log messages
    /// </summary>
    void PopIndent();
}


/// <summary>
/// Empty implementation of the system log that discards all log messages.
/// </summary>
public class EmptySystemLog : ISystemLog
{
    /// <summary>
    /// Gets the singleton instance of the EmptySystemLog.
    /// </summary>
    public static EmptySystemLog Empty { get; } = new();

    private EmptySystemLog()
    {
    }

    /// <inheritdoc/>
    public void AddLog(object message)
    {
    }

    /// <inheritdoc/>
    public void PushIndent()
    {
    }

    /// <inheritdoc/>
    public void PopIndent()
    {
    }
}

/// <summary>
/// System log implementation that writes to the debug output.
/// </summary>
public class TraceSystemLog : ISystemLog
{
    /// <summary>
    /// Gets the singleton instance of the TraceSystemLog.
    /// </summary>
    public static TraceSystemLog Instance { get; } = new();

    private int _indent;

    private TraceSystemLog()
    {
        
    }

    /// <inheritdoc/>
    public void AddLog(object message)
    {
        if (_indent > 0)
        {
            message = $"{new string(' ', _indent * 4)}{message}";
        }

        // Debug.WriteLine(message);
    }

    /// <inheritdoc/>
    public void PushIndent()
    {
        _indent++;
    }

    /// <inheritdoc/>
    public void PopIndent()
    {
        _indent--;
        if (_indent < 0)
        {
            _indent = 0;
        }
    }
}

/// <summary>
/// System log implementation that writes to a file.
/// </summary>
public class FileSystemLog : ISystemLog
{
    readonly string _filePath;

    private int _indent;

    private object _lock = new();

    /// <summary>
    /// Initializes a new instance of the FileSystemLog class.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    public FileSystemLog(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(L($"'{nameof(filePath)}' cannot be null or white space."), nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <inheritdoc/>
    public void AddLog(object message)
    {
        if (_indent > 0)
        {
            message = $"{new string(' ', _indent * 4)}{message}";
        }

        lock (_lock)
        {
            try
            {
                using (var fileStream = new FileStream(
                    _filePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite)) //Allow other processes to read or write
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Add log failed: " + ex.Message);
            }
        }
    }

    /// <inheritdoc/>
    public void PushIndent()
    {
        _indent++;
    }

    /// <inheritdoc/>
    public void PopIndent()
    {
        _indent--;
        if (_indent < 0)
        {
            _indent = 0;
        }
    }
}

/// <summary>
/// Represents a log item that can be navigated to its target object.
/// </summary>
public class ObjectLogCoreItem : INavigable
{
    /// <summary>
    /// Gets the log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the target object for navigation.
    /// </summary>
    public object Target { get; }

    /// <summary>
    /// Initializes a new instance of the ObjectLogCoreItem class.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="target">The target object.</param>
    public ObjectLogCoreItem(string message, object target)
    {
        Message = message;
        Target = target;
    }

    /// <inheritdoc/>
    public object GetNavigationTarget()
    {
        return Target;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Message ?? string.Empty;
    }
}