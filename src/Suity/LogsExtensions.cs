using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using System;

namespace Suity;

/// <summary>
/// Provides extension methods for logging exceptions.
/// </summary>
public static class LogsExtensions
{
    public static void LogError(this Exception exception)
    {
        Logs.LogError(exception);
    }

    public static void LogError(this Exception exception, string message)
    {
        Logs.LogError(new ExceptionLogItem(message, exception));
    }

    public static void LogErrorL(this Exception exception, string message)
    {
        Logs.LogError(new ExceptionLogItem(L(message), exception));
    }

    public static void LogError(this Exception exception, string message, object obj)
    {
        Logs.LogError(new ExceptionLogItem(message, exception, obj));
    }

    public static void LogErrorL(this Exception exception, string message, object obj)
    {
        Logs.LogError(new ExceptionLogItem(L(message), exception, obj));
    }

    public static void LogWarning(this Exception exception)
    {
        Logs.LogWarning(exception);
    }

    public static void LogWarning(this Exception exception, string message)
    {
        Logs.LogWarning(new ExceptionLogItem(message, exception));
    }

    public static void LogWarningL(this Exception exception, string message)
    {
        Logs.LogWarning(new ExceptionLogItem(L(message), exception));
    }

    public static void LogWarning(this Exception exception, string message, object obj)
    {
        Logs.LogWarning(new ExceptionLogItem(message, exception, obj));
    }

    public static void LogWarningL(this Exception exception, string message, object obj)
    {
        Logs.LogWarning(new ExceptionLogItem(L(message), exception, obj));
    }

/*    public static void LogWarning(this ErrorResult err)
    {
        Logs.LogWarning(err.ToString());
    }*/
}

/// <summary>
/// Represents a log item containing an action.
/// </summary>
public class ActionLogItem
{
    public string Message { get; }
    public Action Action { get; }

    public ActionLogItem()
    {
    }

    public ActionLogItem(string message, Action action)
    {
        Message = message;
        Action = action;
    }

    public override string ToString()
    {
        return Message;
    }
}

/// <summary>
/// Represents a log item containing an object.
/// </summary>
public class ObjectLogItem : MarshalByRefObject, INavigable
{
    public string Message { get; }
    public object Object { get; }

    public ObjectLogItem()
    {
    }

    public ObjectLogItem(string message, object obj)
    {
        Message = message;
        Object = obj;
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public object GetNavigationTarget()
    {
        return Object;
    }

    public override string ToString()
    {
        return Message;
    }
}

/// <summary>
/// Represents a log item containing an exception.
/// </summary>
public class ExceptionLogItem : ObjectLogItem
{
    public Exception Exception { get; }

    public ExceptionLogItem()
    {
    }

    public ExceptionLogItem(string message, Exception exception)
        : base(message, null)
    {
        Exception = exception;
    }

    public ExceptionLogItem(string message, Exception exception, object obj)
        : base(message, obj)
    {
        Exception = exception;
    }
}