using System;
using System.Runtime.Serialization;

namespace Suity.Views.Im;

/// <summary>
/// Exception thrown when an ImGui node is in an invalid state.
/// </summary>
[Serializable]
public class ImGuiInvalidStateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiInvalidStateException"/> class.
    /// </summary>
    public ImGuiInvalidStateException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiInvalidStateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ImGuiInvalidStateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiInvalidStateException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ImGuiInvalidStateException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiInvalidStateException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    protected ImGuiInvalidStateException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}
