using System;

namespace Suity.Rex;

/// <summary>
/// Exception thrown when a Rex operation is cancelled.
/// </summary>
[Serializable]
public class RexCancelException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RexCancelException"/> class.
    /// </summary>
    public RexCancelException()
    { }

    /// <summary>
    /// Initializes a new instance with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RexCancelException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public RexCancelException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <inheritdoc/>
    protected RexCancelException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}