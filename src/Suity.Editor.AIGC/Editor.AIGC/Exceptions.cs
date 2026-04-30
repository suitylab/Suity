using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC;


/// <summary>
/// Represents the base exception for all AIGC-related errors.
/// </summary>
[Serializable]
public class AigcException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AigcException"/> class.
    /// </summary>
    public AigcException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AigcException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public AigcException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected AigcException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Represents an exception that occurs during AIGC repair operations, containing details about the problems found and the repair context.
/// </summary>
[Serializable]
public class AigcRepairException : AigcException
{
    /// <summary>
    /// Gets or sets the list of problems identified during the repair process.
    /// </summary>
    public List<string> Problems { get; set; }

    /// <summary>
    /// Gets or sets the prompt used for the repair operation.
    /// </summary>
    public string RepairPrompt { get; set; }

    /// <summary>
    /// Gets or sets the merged result from the repair operation.
    /// </summary>
    public string MergedResult { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcRepairException"/> class.
    /// </summary>
    public AigcRepairException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcRepairException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AigcRepairException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcRepairException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public AigcRepairException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcRepairException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected AigcRepairException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}