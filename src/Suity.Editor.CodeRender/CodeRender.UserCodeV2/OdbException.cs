using System;

namespace Suity.Editor.CodeRender.UserCodeV2;

/// <summary>
/// Represents an exception that occurs during database operations in the Open Database (ODB) system.
/// </summary>
[Serializable]
public class OdbException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OdbException"/> class.
    /// </summary>
    public OdbException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OdbException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public OdbException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OdbException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public OdbException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OdbException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected OdbException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}
