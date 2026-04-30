using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides methods for generating random string identifiers.
/// </summary>
public static class AvaIdGenerator
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private static readonly char[] _chars = CHARS.ToCharArray();

    [ThreadStatic]
    private static Random _rnd;

    /// <summary>
    /// Generates a random string identifier of the specified length.
    /// </summary>
    /// <param name="length">The length of the identifier to generate.</param>
    /// <returns>A random alphanumeric string.</returns>
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
