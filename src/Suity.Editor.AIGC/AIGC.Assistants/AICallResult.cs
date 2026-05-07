using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC.Assistants;

#region AICallResultType
/// <summary>
/// Represents the status of an AI call operation.
/// </summary>
public enum AICallStatus
{
    /// <summary>
    /// The call result is empty (no operation performed).
    /// </summary>
    Empty,

    /// <summary>
    /// The call completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The call failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    /// The call returned a message without a specific result.
    /// </summary>
    Message,

    /// <summary>
    /// The call returned a single result object.
    /// </summary>
    Result,

    /// <summary>
    /// The call returned multiple result objects.
    /// </summary>
    MultipleResult,
}
#endregion

#region AICallResult
/// <summary>
/// Represents the base class for all AI call results.
/// </summary>
public abstract class AICallResult
{
    /// <summary>
    /// Gets the status of the AI call.
    /// </summary>
    public abstract AICallStatus Status { get; }

    /// <summary>
    /// Gets the message associated with the call result.
    /// </summary>
    public abstract string Message { get; }

    /// <summary>
    /// Gets the single result object from the call, if any.
    /// </summary>
    public abstract object Result { get; }

    /// <summary>
    /// Gets all result objects from the call.
    /// </summary>
    public abstract IEnumerable<object> Results { get; }

    /// <summary>
    /// Gets the type of the result object.
    /// </summary>
    public abstract Type ResultType { get; }

    /// <summary>
    /// Gets an empty call result instance.
    /// </summary>
    public static AICallResult Empty => EmptyAICallResult.Instance;

    /// <summary>
    /// Gets a success call result instance.
    /// </summary>
    public static AICallResult Success => SuccessAICallResult.Instance;

    /// <summary>
    /// Gets a failed call result instance.
    /// </summary>
    public static AICallResult Failed => SuccessAICallResult.Failed;

    /// <summary>
    /// Creates a call result from a message string.
    /// </summary>
    /// <param name="message">The message to include in the result.</param>
    /// <returns>A new <see cref="MessageAICallResult"/> instance.</returns>
    public static AICallResult FromMessage(string message) => new MessageAICallResult(message);

    /// <summary>
    /// Creates a failed call result with an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="FailedAICallResult"/> instance.</returns>
    public static AICallResult FromFailed(string message) => new FailedAICallResult(message);

    /// <summary>
    /// Creates a call result from a single result object.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="result">The result object.</param>
    /// <returns>A new <see cref="OneAICallResult{T}"/> instance.</returns>
    public static AICallResult FromResult<T>(T result) => new OneAICallResult<T>(result);

    /// <summary>
    /// Creates a call result from a single result object with a message.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="message">An optional message.</param>
    /// <returns>A new <see cref="OneAICallResult{T}"/> instance.</returns>
    public static AICallResult FromResult<T>(T result, string message) => new OneAICallResult<T>(result, message);

    /// <summary>
    /// Creates a call result from multiple result objects.
    /// </summary>
    /// <typeparam name="T">The type of the results.</typeparam>
    /// <param name="results">The collection of result objects.</param>
    /// <returns>A new <see cref="MultiAICallResult{T}"/> instance.</returns>
    public static AICallResult FromResults<T>(IEnumerable<T> results) => new MultiAICallResult<T>(results);

    /// <summary>
    /// Creates a call result from multiple result objects with a message.
    /// </summary>
    /// <typeparam name="T">The type of the results.</typeparam>
    /// <param name="results">The collection of result objects.</param>
    /// <param name="message">An optional message.</param>
    /// <returns>A new <see cref="MultiAICallResult{T}"/> instance.</returns>
    public static AICallResult FromResults<T>(IEnumerable<T> results, string message) => new MultiAICallResult<T>(results, message);
}
#endregion

#region EmptyAICallResult
/// <summary>
/// Represents an empty AI call result indicating no operation was performed.
/// </summary>
public class EmptyAICallResult : AICallResult
{
    /// <summary>
    /// Gets the singleton instance of the empty call result.
    /// </summary>
    public static AICallResult Instance { get; } = new EmptyAICallResult();

    public override AICallStatus Status => AICallStatus.Empty;

    public override string Message => null;

    public override object Result => null;

    public override IEnumerable<object> Results => [];

    public override Type ResultType => null;

    private EmptyAICallResult()
    {
    }
}
#endregion

#region SuccessAICallResult
/// <summary>
/// Represents a successful AI call result with no specific return value.
/// </summary>
public class SuccessAICallResult : AICallResult
{
    /// <summary>
    /// Gets the singleton instance of the success call result.
    /// </summary>
    public static AICallResult Instance { get; } = new SuccessAICallResult();

    public override AICallStatus Status => AICallStatus.Success;

    public override string Message => "Success";

    public override object Result => null;

    public override IEnumerable<object> Results => [];

    public override Type ResultType => null;

    private SuccessAICallResult()
    {
    }
}
#endregion

#region MessageAICallResult
/// <summary>
/// Represents an AI call result that contains only a message.
/// </summary>
public class MessageAICallResult : AICallResult
{
    private string _message;

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">The message content.</param>
    public MessageAICallResult(string message)
    {
        _message = message ?? "";
    }

    public override AICallStatus Status => AICallStatus.Message;

    public override string Message => _message;

    public override object Result => null;

    public override IEnumerable<object> Results => [];

    public override Type ResultType => null;

    public override string ToString()
    {
        return _message;
    }
}
#endregion

#region FailedAICallResult
/// <summary>
/// Represents a failed AI call result with an error message.
/// </summary>
public class FailedAICallResult : AICallResult
{
    /// <summary>
    /// Gets the singleton instance of a generic failed call result.
    /// </summary>
    public static AICallResult Instance { get; } = new FailedAICallResult();

    private readonly string _message;

    /// <summary>
    /// Initializes a new instance without a specific error message.
    /// </summary>
    public FailedAICallResult()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing the failure.</param>
    public FailedAICallResult(string message)
    {
        _message = message;
    }

    public override AICallStatus Status => AICallStatus.Failed;

    public override string Message => _message ?? "Failed";

    public override object Result => null;

    public override IEnumerable<object> Results => [];

    public override Type ResultType => null;
}
#endregion

#region OneAICallResult
/// <summary>
/// Represents an AI call result containing a single typed result.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public class OneAICallResult<T> : AICallResult
{
    private readonly string _message;
    private readonly T _result;

    /// <summary>
    /// Initializes a new instance with the specified result.
    /// </summary>
    /// <param name="result">The result object.</param>
    public OneAICallResult(T result)
    {
        _result = result;
    }

    /// <summary>
    /// Initializes a new instance with the specified result and message.
    /// </summary>
    /// <param name="result">The result object.</param>
    /// <param name="message">An optional message.</param>
    public OneAICallResult(T result, string message)
    {
        _result = result;
        _message = message;
    }

    public override AICallStatus Status => AICallStatus.Result;

    public override string Message => _message ?? "";

    public override object Result => _result;

    public override IEnumerable<object> Results => [_result];

    public override Type ResultType => typeof(T);

    public override string ToString()
    {
        return _result?.ToString() ?? "";
    }
}
#endregion

#region MultiAICallResult
/// <summary>
/// Represents an AI call result containing multiple typed results.
/// </summary>
/// <typeparam name="T">The type of the results.</typeparam>
public class MultiAICallResult<T> : AICallResult
{
    private readonly string _message;
    private readonly List<T> _results = [];

    /// <summary>
    /// Initializes a new empty instance.
    /// </summary>
    public MultiAICallResult()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">The message content.</param>
    public MultiAICallResult(string message)
    {
        _message = message;
    }

    /// <summary>
    /// Initializes a new instance with the specified results.
    /// </summary>
    /// <param name="results">The collection of result objects.</param>
    public MultiAICallResult(IEnumerable<T> results)
    {
        _results.AddRange(results);
    }

    /// <summary>
    /// Initializes a new instance with the specified results and message.
    /// </summary>
    /// <param name="results">The collection of result objects.</param>
    /// <param name="message">An optional message.</param>
    public MultiAICallResult(IEnumerable<T> results, string message)
    {
        _results.AddRange(results);
        _message = message;
    }

    /// <summary>
    /// Adds a result to the collection.
    /// </summary>
    /// <param name="result">The result to add.</param>
    public void Add(T result)
    {
        _results.Add(result);
    }

    public override string Message => _message ?? "";

    public override AICallStatus Status => AICallStatus.MultipleResult;

    public override object Result => _results;

    public override IEnumerable<object> Results => _results.OfType<object>();

    public override Type ResultType => typeof(T);

    public override string ToString()
    {
        return string.Join("\n\n", _results);
    }
} 
#endregion