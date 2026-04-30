namespace Suity.Editor.Flows;

#region FlowNodeRunException

/// <summary>
/// Exception thrown when a flow node fails to run.
/// </summary>
[System.Serializable]
public class FlowNodeRunException : System.Exception
{
    /// <summary>
    /// Initializes a new instance of the FlowNodeRunException.
    /// </summary>
    public FlowNodeRunException()
    { }

    /// <summary>
    /// Initializes a new instance of the FlowNodeRunException with a message.
    /// </summary>
    public FlowNodeRunException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the FlowNodeRunException with a message and inner exception.
    /// </summary>
    public FlowNodeRunException(string message, System.Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// Serialization constructor.
    /// </summary>
    protected FlowNodeRunException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

#endregion
