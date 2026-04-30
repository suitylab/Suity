using System;

namespace ComputerBeacon.Json;

internal static class Helper
{
    private readonly static Type[] ValidTypes = [typeof(JsonArray),typeof(JsonObject),
                    typeof(string),typeof(bool),typeof(byte),typeof(sbyte),
                    typeof(short),typeof(ushort),typeof(int),typeof(uint),typeof(long),typeof(ulong),
                    typeof(float),typeof(double),typeof(decimal)];

    internal static void AssertValidType(object value)
    {
        if (value is null) return;

        var type = value.GetType();
        for (int i = 0; i < ValidTypes.Length; i++) if (type == ValidTypes[i]) return;
        throw new FormatException("Invalid value type: " + value.GetType().ToString());
    }
}