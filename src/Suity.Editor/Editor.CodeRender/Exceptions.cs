using System;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Exception for compilation errors.
/// </summary>
[Serializable]
public class CompileException : Exception
{
    /// <summary>
    /// Creates a new compile exception.
    /// </summary>
    public CompileException()
    { }

    /// <summary>
    /// Creates a new compile exception with a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public CompileException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new compile exception with a message and inner exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">Inner exception.</param>
    public CompileException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Creates a new compile exception from serialization info.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected CompileException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception for rendering errors.
/// </summary>
[Serializable]
public class RenderException : Exception
{
    /// /// <summary>
    /// Creates a new render exception.
    /// </summary>
    public RenderException()
    { }

    /// <summary>
    /// Creates a new render exception with a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public RenderException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new render exception with a message and inner exception.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">Inner exception.</param>
    public RenderException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Creates a new render exception from serialization info.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected RenderException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}