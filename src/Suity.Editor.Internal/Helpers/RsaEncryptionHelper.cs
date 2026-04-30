using Crypto.Client;
using Crypto.Client.Impl;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for RSA encryption, decryption, signing, and signature verification.
/// </summary>
public static class RsaEncryptionHelper
{
    private static readonly IRsaCryptoUtil _rsaCryptoUtil;
    private static readonly IBytesUtil _bytesUtil;

    static RsaEncryptionHelper()
    {
        _rsaCryptoUtil = new RsaCryptoUtil();
        _bytesUtil = new BytesUtil();
    }

    /// <summary>
    /// Encrypts plaintext using the specified RSA public key.
    /// </summary>
    /// <param name="pubKey">The RSA public key in PEM or compatible format.</param>
    /// <param name="plainText">The plaintext to encrypt.</param>
    /// <returns>The encrypted text as a Base64-encoded string.</returns>
    public static string Encrypt(string pubKey, string plainText)
    {
        var plainBytes = _bytesUtil.FromString(plainText);
        var encryptedBytes = _rsaCryptoUtil.Encrypt(plainBytes, pubKey);

        return _bytesUtil.ToBase64(encryptedBytes);
    }

    /// <summary>
    /// Decrypts ciphertext using the specified RSA private key.
    /// </summary>
    /// <param name="privKey">The RSA private key in PEM or compatible format.</param>
    /// <param name="encryptedText">The Base64-encoded encrypted text to decrypt.</param>
    /// <returns>The decrypted plaintext.</returns>
    public static string Decrypt(string privKey, string encryptedText)
    {
        var encryptedBytes = _bytesUtil.FromBase64(encryptedText);
        var plainBytes = _rsaCryptoUtil.Decrypt(encryptedBytes, privKey);

        return _bytesUtil.ToString(plainBytes);
    }

    /// <summary>
    /// Signs the specified text using the RSA private key.
    /// </summary>
    /// <param name="privKey">The RSA private key in PEM or compatible format.</param>
    /// <param name="text">The text to sign.</param>
    /// <returns>The digital signature as a Base64-encoded string.</returns>
    public static string Sign(string privKey, string text)
    {
        var bytes = _bytesUtil.FromString(text);
        var signatureBytes = _rsaCryptoUtil.Sign(bytes, privKey);

        return _bytesUtil.ToBase64(signatureBytes);
    }

    /// <summary>
    /// Verifies a digital signature against the specified text using the RSA public key.
    /// </summary>
    /// <param name="pubKey">The RSA public key in PEM or compatible format.</param>
    /// <param name="text">The original text that was signed.</param>
    /// <param name="signature">The Base64-encoded signature to verify.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    public static bool Verify(string pubKey, string text, string signature)
    {
        var bytes = _bytesUtil.FromString(text);
        var signatureBytes = _bytesUtil.FromBase64(signature);

        return _rsaCryptoUtil.Verify(bytes, signatureBytes, pubKey);
    }
}
