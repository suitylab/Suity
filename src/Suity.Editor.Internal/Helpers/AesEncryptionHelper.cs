using System;
using System.Security.Cryptography;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for AES encryption and decryption of strings.
/// Uses AES with a randomly generated IV prepended to the ciphertext.
/// </summary>
public class AesEncryptionHelper
{
    /// <summary>
    /// Encrypts a plaintext string using AES with the specified key.
    /// A random IV is generated for each encryption and prepended to the ciphertext.
    /// </summary>
    /// <param name="plainText">The plaintext string to encrypt.</param>
    /// <param name="key">The encryption key as a UTF-8 string.</param>
    /// <returns>The encrypted data as a Base64-encoded string (IV + ciphertext).</returns>
    public static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] encryptedBytes;

        using var aes = Aes.Create();

        aes.Key = keyBytes;
        aes.GenerateIV();

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var ms = new System.IO.MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(plainBytes, 0, plainBytes.Length);
            cs.FlushFinalBlock();
        }

        encryptedBytes = ms.ToArray();

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts a Base64-encoded AES ciphertext using the specified key.
    /// Expects the first 16 bytes of the decoded data to be the IV.
    /// </summary>
    /// <param name="cipherText">The Base64-encoded encrypted data (IV + ciphertext).</param>
    /// <param name="key">The decryption key as a UTF-8 string.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public static string Decrypt(string cipherText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        byte[] decryptedBytes;

        using var aes = Aes.Create();

        aes.Key = keyBytes;

        byte[] iv = new byte[16];
        Array.Copy(cipherBytes, iv, 16);
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using (var ms = new System.IO.MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 16, cipherBytes.Length - 16);
                cs.FlushFinalBlock();
            }

            decryptedBytes = ms.ToArray();
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
