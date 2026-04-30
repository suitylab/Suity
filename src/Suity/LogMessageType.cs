using System;

namespace Suity;

/// <summary>
/// Specifies the type of log message.
/// </summary>
[Flags]
public enum LogMessageType
{
    Debug = 1,
    Info = 2,
    Warning = 4,
    Error = 8,
    All = Debug | Info | Warning | Error,
}