using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Contains information about a synchronized value
/// </summary>
public class SyncValueInfo(Type baseType, object value, SyncFlag flag)
{
    public Type BaseType { get; set; } = baseType;
    public object Value { get; set; } = value;
    public SyncFlag Flag { get; set; } = flag;

    public override string ToString()
    {
        return $"<{BaseType}, {Value}>";
    }
}