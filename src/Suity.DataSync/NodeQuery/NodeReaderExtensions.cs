using System;

namespace Suity.NodeQuery;

/// <summary>
/// Extension methods for INodeReader
/// </summary>
public static class NodeReaderExtensions
{
    public static void WriteTo(this INodeReader reader, INodeWriter writer, int maxDepth = -1)
    {
        if (maxDepth == 0)
        {
            return;
        }

        if (maxDepth > 0)
        {
            maxDepth--;
        }

        writer.SetValueObj(reader.NodeValueObj);

        foreach (var attr in reader.Attributes)
        {
            writer.SetAttribute(attr.Key, attr.Value);
        }

        foreach (var childReader in reader.Nodes())
        {
            writer.SetElement(childReader.NodeName, childWriter => childReader.WriteTo(childWriter, maxDepth));
        }
    }

    public static string GetStringValue(this INodeReader reader, string defaultValue = null)
    {
        return reader.NodeValueObj?.ToString() ?? defaultValue;
    }

    public static bool GetBooleanValue(this INodeReader reader, bool defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is bool v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (bool.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToBoolean(obj);
    }

    public static byte GetByteValue(this INodeReader reader, byte defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is byte v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (byte.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToByte(obj);
    }

    public static sbyte GetSByteValue(this INodeReader reader, sbyte defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is sbyte v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (sbyte.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToSByte(obj);
    }

    public static short GetInt16Value(this INodeReader reader, short defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is short v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (short.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToInt16(obj);
    }

    public static int GetInt32Value(this INodeReader reader, int defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is int v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (int.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToInt32(obj);
    }

    public static long GetInt64Value(this INodeReader reader, long defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is long v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (long.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToInt64(obj);
    }

    public static ushort GetUInt16Value(this INodeReader reader, ushort defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is ushort v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (ushort.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToUInt16(obj);
    }

    public static uint GetUInt32Value(this INodeReader reader, uint defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is uint v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (uint.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToUInt32(obj);
    }

    public static ulong GetUInt64Value(this INodeReader reader, ulong defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is ulong v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (ulong.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToUInt64(obj);
    }

    public static float GetSingleValue(this INodeReader reader, float defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is float v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (float.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToSingle(obj);
    }

    public static double GetDoubleValue(this INodeReader reader, double defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is double v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (double.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToDouble(obj);
    }

    public static decimal GetDecimalValue(this INodeReader reader, decimal defaultValue = default)
    {
        var obj = reader.NodeValueObj;
        if (obj is null)
        {
            return defaultValue;
        }

        if (obj is decimal v)
        {
            return v;
        }

        if (obj is string s)
        {
            if (decimal.TryParse(s, out v))
            {
                return v;
            }
            else
            {
                return defaultValue;
            }
        }

        return Convert.ToDecimal(obj);
    }

    public static bool GetBooleanAttribute(this INodeReader reader, string name, bool defaultValue = default)
    {
        if (bool.TryParse(reader.GetAttribute(name), out bool value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static byte GetByteAttribute(this INodeReader reader, string name, byte defaultValue = default)
    {
        if (byte.TryParse(reader.GetAttribute(name), out byte value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static sbyte GetSByteAttribute(this INodeReader reader, string name, sbyte defaultValue = default)
    {
        if (sbyte.TryParse(reader.GetAttribute(name), out sbyte value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static short GetInt16Attribute(this INodeReader reader, string name, short defaultValue = default)
    {
        if (short.TryParse(reader.GetAttribute(name), out short value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static int GetInt32Attribute(this INodeReader reader, string name, int defaultValue = default)
    {
        if (int.TryParse(reader.GetAttribute(name), out int value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static long GetInt64Attribute(this INodeReader reader, string name, long defaultValue = default)
    {
        if (long.TryParse(reader.GetAttribute(name), out long value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static ushort GetUInt16Attribute(this INodeReader reader, string name, ushort defaultValue = default)
    {
        if (ushort.TryParse(reader.GetAttribute(name), out ushort value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static uint GetUInt32Attribute(this INodeReader reader, string name, uint defaultValue = default)
    {
        if (uint.TryParse(reader.GetAttribute(name), out uint value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static ulong GetUInt64Attribute(this INodeReader reader, string name, ulong defaultValue = default)
    {
        if (ulong.TryParse(reader.GetAttribute(name), out ulong value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static float GetSingleAttribute(this INodeReader reader, string name, float defaultValue = default)
    {
        if (float.TryParse(reader.GetAttribute(name), out float value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static double GetDoubleAttribute(this INodeReader reader, string name, double defaultValue = default)
    {
        if (double.TryParse(reader.GetAttribute(name), out double value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static decimal GetDecimalAttribute(this INodeReader reader, string name, decimal defaultValue = default)
    {
        if (decimal.TryParse(reader.GetAttribute(name), out decimal value))
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }
}