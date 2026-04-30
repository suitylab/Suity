using System;

namespace Suity;

/// <summary>
/// Event args with a generic value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class EventArgs<T>(T value) : EventArgs
{
    public T Value { get; private set; } = value;
}

/// <summary>
/// Event args with a value and an index.
/// </summary>
/// <typeparam name="T1">The type of the value.</typeparam>
/// <typeparam name="TIndex">The type of the index.</typeparam>
public class IndexEventArgs<T1, TIndex>(T1 value, TIndex index) : EventArgs
{
    public T1 Value { get; private set; } = value;
    public TIndex Index { get; private set; } = index;
}



/// <summary>
/// Event handler delegate for generic event args.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public delegate void EventArgsHandler<T>(object sender, EventArgs<T> args);

/// <summary>
/// Event args for runtime log messages.
/// </summary>
public class RuntimeLogEventArgs(LogMessageType type, object message) : EventArgs
{
    public LogMessageType Type { get; } = type;
    public object Message { get; } = message;
}