using System;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Generates random alphanumeric IDs using a thread-safe random number generator.
/// </summary>
public static class IdGenerator
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private static readonly char[] _chars = CHARS.ToCharArray();

    [ThreadStatic]
    private static Random _rnd;

    /// <summary>
    /// Generates a random alphanumeric ID of the specified length.
    /// </summary>
    /// <param name="length">The length of the ID to generate.</param>
    /// <returns>A random string containing uppercase letters, lowercase letters, and digits.</returns>
    public static string GenerateId(int length)
    {
        var rnd = _rnd ??= new Random();

        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            result.Append(_chars[rnd.Next(_chars.Length)]);
        }

        return result.ToString();
    }
}
