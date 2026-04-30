using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides utility methods for calculating SHA256 and MD5 checksums and hashes for files and strings.
/// </summary>
public static class CheckSumHelper
{
    /// <summary>
    /// Calculates the SHA256 checksum of a file and returns it as a Base64-encoded string.
    /// </summary>
    /// <param name="filePath">The path to the file to calculate the checksum for.</param>
    /// <returns>A Base64-encoded string representing the SHA256 checksum of the file.</returns>
    public static string CalculateFileChecksumSHA256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);

        byte[] checksumBytes = sha256.ComputeHash(fileStream);
        string base64Checksum = Convert.ToBase64String(checksumBytes);

        return base64Checksum;
    }

    /// <summary>
    /// Calculates the MD5 checksum of a file and returns it as a Base64-encoded string.
    /// </summary>
    /// <param name="filePath">The path to the file to calculate the checksum for.</param>
    /// <returns>A Base64-encoded string representing the MD5 checksum of the file.</returns>
    public static string CalculateFileChecksumMD5(string filePath)
    {
        using var md5 = MD5.Create();
        using var fileStream = File.OpenRead(filePath);

        byte[] checksumBytes = md5.ComputeHash(fileStream);
        string base64Checksum = Convert.ToBase64String(checksumBytes);

        return base64Checksum;
    }

    /// <summary>
    /// Computes the SHA256 hash of a string and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A lowercase hexadecimal string representing the SHA256 hash of the input.</returns>
    public static string GetSHA256Hash(string input)
    {
        using SHA256 sha256 = SHA256.Create();

        // Convert input string to byte array and compute hash
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert byte array to hex string
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Computes the MD5 hash of a string and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A lowercase hexadecimal string representing the MD5 hash of the input.</returns>
    public static string GetMD5Hash(string input)
    {
        using MD5 md5 = MD5.Create();

        // Convert input string to byte array and compute hash
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert byte array to hex string
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

}
