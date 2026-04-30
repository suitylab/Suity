using System;

namespace Suity.Synchonizing.Core;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class SyncProxyUsageAttribute(Type objectType) : Attribute
{
    public Type ObjectType { get; } = objectType;
}