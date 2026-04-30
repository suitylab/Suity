using System;
using System.Linq;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for byte array manipulation and encoding.
/// </summary>
public static class ByteHelpers
{
    public static int ToUTF8Bytes(this string s, byte[] bytes, int index)
    {
        return Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, index);
    }

    public static int GetUTF8Length(this string s)
    {
        return Encoding.UTF8.GetByteCount(s);
    }

    public static byte[] FromUTF8(this string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    public static byte[] FromBase64(this string base64Text)
    {
        return Convert.FromBase64String(base64Text);
    }

    public static string ToUTF8(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static string GetUTF8(this byte[] bytes, int index, int count)
    {
        return Encoding.UTF8.GetString(bytes, index, count);
    }

    public static string ToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static string ToHex(this byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.AppendFormat("{0:X2}", b);
        }
        return builder.ToString();
    }

    public static byte[] Combine(params byte[][] arrays)
    {
        var result = new byte[arrays.Sum(a => a.Length)];

        var offset = 0;

        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }
}