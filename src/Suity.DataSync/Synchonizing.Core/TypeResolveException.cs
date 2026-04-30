using System;

namespace Suity.Synchonizing.Core;

[Serializable]
public class TypeResolveException : Exception
{
    public TypeResolveException()
    { }

    public TypeResolveException(string message) : base(message)
    {
    }

    public TypeResolveException(string message, Exception inner) : base(message, inner)
    {
    }

    protected TypeResolveException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}