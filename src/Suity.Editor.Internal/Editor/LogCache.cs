using System;
using System.Collections.Generic;

namespace Suity.Editor;


/// <summary>
/// Provides a thread-safe static cache for log entries with support for indentation levels and event-based notification when new entries are added.
/// </summary>
public static class LogCache
{
    // Synchronization object for thread-safe access to shared state.
    private static readonly object _syncRoot = new();

    // Internal buffer holding the cached log entries awaiting pickup.
    private static readonly List<LogEntry> _logEntries = [];

    /// <summary>
    /// Event raised when a new log entry is added to the cache.
    /// </summary>
    public static event Action<LogEntry> LogEntryAdded;

    // Current indentation level for log entries.
    private static int _indent;

    /// <summary>
    /// Retrieves and clears all cached log entries in a thread-safe manner.
    /// </summary>
    /// <returns>An array of all log entries that were in the cache, or an empty array if the cache was empty.</returns>
    public static LogEntry[] PickUp()
    {
        lock (_syncRoot)
        {
            if (_logEntries.Count == 0)
            {
                return [];
            }

            var entries = _logEntries.ToArray();
            _logEntries.Clear();

            return entries;
        }
    }

    /// <summary>
    /// Adds a new log entry to the cache with the specified message level and content.
    /// </summary>
    /// <param name="level">The severity level of the log message.</param>
    /// <param name="msg">The log message content. Null messages are ignored.</param>
    public static void AddLog(LogMessageType level, object msg)
    {
        if (msg is null)
        {
            return;
        }

        var entry = new LogEntry
        {
            LogLevel = level,
            Message = msg,
            Indent = _indent,
        };

        lock (_syncRoot)
        {
            _logEntries.Add(entry);
        };

        LogEntryAdded?.Invoke(entry);
        EditorRexes.LogEntryAdded.Invoke(entry);
    }

    /// <summary>
    /// Increases the current indentation level for subsequently added log entries.
    /// </summary>
    public static void PushIndent()
    {
        lock (_syncRoot)
        {
            _indent++;
        }
    }

    /// <summary>
    /// Decreases the current indentation level for subsequently added log entries, ensuring it does not go below zero.
    /// </summary>
    public static void PopIndent()
    {
        lock (_syncRoot)
        {
            if (_indent > 0)
            {
                _indent--;
            }
        }
    }
}